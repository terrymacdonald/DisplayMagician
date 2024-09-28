using DisplayMagician.GameLibraries;
using DisplayMagician.Processes;
using DisplayMagician.Resources;
using DisplayMagicianShared;
using Manina.Windows.Forms;
using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class ShortcutLibraryForm : Form
    {

        private ShortcutAdaptor _shortcutAdaptor = new ShortcutAdaptor();
        private ShortcutItem _selectedShortcut = null;
        private ShortcutForm _shortcutForm = null;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ShortcutLibraryForm()
        {
            InitializeComponent();
            ilv_saved_shortcuts.MultiSelect = false;
            ilv_saved_shortcuts.ThumbnailSize = new Size(100,100);
            ilv_saved_shortcuts.AllowDrag = false;
            ilv_saved_shortcuts.AllowDrop = false;
            ilv_saved_shortcuts.SetRenderer(new ShortcutILVRenderer());
            // Center the form on the primary screen
            Utils.CenterOnPrimaryScreen(this);
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShortcutLibraryForm_Load(object sender, EventArgs e)
        {
            // Refresh the profiles and the shortcut validity to start
            // The rest of the refreshing happens as the shortcuts are added
            // and deleted.
            logger.Trace($"ShortcutLibraryForm/ShortcutLibraryForm_Load: Refreshing Possibilty.");
            ProfileRepository.IsPossibleRefresh();
            logger.Trace($"ShortcutLibraryForm/ShortcutLibraryForm_Load: Refreshing Validity.");
            ShortcutRepository.IsValidRefresh();
            logger.Trace($"ShortcutLibraryForm/ShortcutLibraryForm_Load: Refreshing SHortutLibraryUI.");
            // Refresh the Shortcut Library UI
            RefreshShortcutLibraryUI();
            logger.Trace($"ShortcutLibraryForm/ShortcutLibraryForm_Load: Remove the UI warning if we do have some shortcuts to show the user.");
            RemoveWarningIfShortcuts();

            // Start the donation animation if it's time to do so
            if (Utils.TimeToRunDonationAnimation())
            {
                Utils.AddAnimation(btn_donate);
            }
        }


        private void RefreshShortcutLibraryUI()
        {

            if (ShortcutRepository.ShortcutCount == 0)
                return;

            // Temporarily stop updating the saved_profiles listview
            logger.Trace($"ShortcutLibraryForm/RefreshShortcutLibraryUI: Suspending the imagelistview layout");
            ilv_saved_shortcuts.SuspendLayout();            

            ImageListViewItem newItem = null;
            logger.Trace($"ShortcutLibraryForm/RefreshShortcutLibraryUI: Emptying shortcut list");
            ilv_saved_shortcuts.Items.Clear();

            
            foreach (ShortcutItem loadedShortcut in ShortcutRepository.AllShortcuts.OrderBy(s => s.Name))
            {
                logger.Trace($"ShortcutLibraryForm/RefreshShortcutLibraryUI: Adding shortcut {loadedShortcut.Name} into the list of shortcuts shown to the user ");

                // Ignore any shortcuts with incompatible game libraries
                if (loadedShortcut.Category == ShortcutCategory.Game && (!Enum.IsDefined(typeof(SupportedGameLibraryType), loadedShortcut.GameLibrary) || loadedShortcut.GameLibrary == SupportedGameLibraryType.Unknown))
                {
                    // Skip showing unknown game library items as we have no way to deal with them
                    logger.Warn( $"ShortcutLibraryForm/RefreshShortcutLibraryUI: Ignoring game shortcut {loadedShortcut.Name} as it's from a Game library this version doesn't support.");
                    continue;
                }

                newItem = new ImageListViewItem(loadedShortcut, loadedShortcut.Name);

                // Select it if its the selectedProfile
                if (_selectedShortcut is ShortcutItem && _selectedShortcut.Equals(loadedShortcut))
                {
                    logger.Trace($"ShortcutLibraryForm/RefreshShortcutLibraryUI: This shortcut {loadedShortcut.Name} is the selected one so selecting it in the UI");
                    newItem.Selected = true;
                    // Hide the run button if the shortcut isn't valid
                    if (_selectedShortcut.IsValid == ShortcutValidity.Warning || _selectedShortcut.IsValid == ShortcutValidity.Error)
                    {
                        logger.Trace($"ShortcutLibraryForm/RefreshShortcutLibraryUI: This shortcut {loadedShortcut.Name} is the selected one and is invalid ({_selectedShortcut.IsValid.ToString("G")}), so highlighting that in the UI");
                        btn_run.Visible = false;
                        cms_shortcuts.Items[1].Enabled = false;
                    }

                    else
                    {
                        logger.Trace($"ShortcutLibraryForm/RefreshShortcutLibraryUI: This shortcut {loadedShortcut.Name} is the selected one and is valid, so highlighting that in the UI");
                        btn_run.Visible = true;
                        cms_shortcuts.Items[1].Enabled = true;
                    }
                }

                //ilv_saved_profiles.Items.Add(newItem);
                logger.Trace($"ShortcutLibraryForm/RefreshShortcutLibraryUI: Adding this shortcut {loadedShortcut.Name} to the imagelistview");
                ilv_saved_shortcuts.Items.Add(newItem, _shortcutAdaptor);
            }

            logger.Trace($"ShortcutLibraryForm/RefreshShortcutLibraryUI: Resuming the imagelistview layout");

            // Restart updating the saved_profiles listview
            ilv_saved_shortcuts.ResumeLayout();

        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            // Only do something if there is a shortcut selected
            if (_selectedShortcut == null)
            {
                if (ShortcutRepository.ShortcutCount > 0)
                {
                    MessageBox.Show(
                        @"You need to select a Game Shortcut in order to save a desktop shortcut to it. Please select a Game Shortcut then try again, or right-click on the Game Shortcut and select 'Save Shortcut to Desktop'.",
                        @"Select Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    MessageBox.Show(
                        @"You need to create a Game Shortcut before you can save a desktop shortcut to it. Please create a Game Shortcut by clicking the New button.",
                        @"Create Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            else
            {

                // if shortcut is not valid then ask if the user
                // really wants to save it to desktop
                if (_selectedShortcut.IsValid == ShortcutValidity.Error || _selectedShortcut.IsValid == ShortcutValidity.Warning)
                {
                    // We ask the user of they still want to save the desktop shortcut
                    if (MessageBox.Show($"The shortcut '{_selectedShortcut.Name}' isn't valid for some reason so a desktop shortcut wouldn't work until the shortcut is fixed. Has your hardware or screen layout changed from when the shortcut was made? We recommend that you edit the shortcut to make it valid again, or reverse the hardware changes you made. Do you still want to save the desktop shortcut?", $"Still save the '{_selectedShortcut.Name}' Desktop Shortcut?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                        return;
                }

                try
                {
                    // Set the Shortcut save folder to the Desktop as that's where people will want it most likely
                    dialog_save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    // Try to set up some sensible suggestions for the Shortcut name
                    if (_selectedShortcut.AutoName)
                    {
                        if (_selectedShortcut.DisplayPermanence == ShortcutPermanence.Permanent)
                        {

                            dialog_save.FileName = _selectedShortcut.Name;
                        }
                        else
                        {
                            if (_selectedShortcut.Category == ShortcutCategory.Executable)
                            {
                                dialog_save.FileName = String.Concat(Path.GetFileNameWithoutExtension(_selectedShortcut.ExecutableNameAndPath), @" (", _selectedShortcut.Name.ToLower(CultureInfo.InvariantCulture), @")");
                            }
                            else
                            {
                                dialog_save.FileName = _selectedShortcut.Name;
                            }
                        }
                    }
                    else
                    {
                        dialog_save.FileName = _selectedShortcut.Name;
                    }

                    // Show the Save Shortcut window
                    if (dialog_save.ShowDialog(this) == DialogResult.OK)
                    {
                        if (_selectedShortcut.CreateShortcut(dialog_save.FileName))
                        {
                            MessageBox.Show(
                                String.Format(Language.Shortcut_placed_successfully, dialog_save.FileName),
                                Language.Shortcut,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                Language.Failed_to_create_the_shortcut_Unexpected_exception_occurred,
                                Language.Shortcut,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                        }

                        dialog_save.FileName = string.Empty;
                        //DialogResult = DialogResult.OK;
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutLibraryForm/btn_save_Click: Exception saving shortcut to {dialog_save.FileName}.");
                    MessageBox.Show(ex.Message, Language.Shortcut, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void RemoveWarningIfShortcuts()
        {
            if (ShortcutRepository.AllShortcuts.Count > 0)
                lbl_create_shortcut.Visible = false;
            else
                lbl_create_shortcut.Visible = true;
        }

        private void ilv_saved_shortcuts_ItemClick(object sender, ItemClickEventArgs e)
        {
            // This is the single click to select
            //_selectedShortcut = GetShortcutFromName(e.Item.Text);
            _selectedShortcut = ShortcutRepository.GetShortcut(e.Item.Text);

            // Hide the run button if the shortcut isn't valid
            if (_selectedShortcut.IsValid == ShortcutValidity.Warning || _selectedShortcut.IsValid == ShortcutValidity.Error)
            {
                btn_run.Visible = false;
                cms_shortcuts.Items[1].Enabled = false;
            }

            else
            {
                btn_run.Visible = true;
                cms_shortcuts.Items[1].Enabled = true;
            }

            if (e.Buttons == MouseButtons.Right)
            {
                cms_shortcuts.Show(ilv_saved_shortcuts,e.Location);
            }
        }

        private void ilv_saved_shortcuts_ItemDoubleClick(object sender, ItemClickEventArgs e)
        {
            // This is the double click to run
            _selectedShortcut = ShortcutRepository.GetShortcut(e.Item.Text);
            
            // Hide the run button if the shortcut isn't valid
            if (_selectedShortcut.IsValid == ShortcutValidity.Warning || _selectedShortcut.IsValid == ShortcutValidity.Error)
            {
                btn_run.Visible = false;
                cms_shortcuts.Items[1].Enabled = false;
            }

            else
            {
                btn_run.Visible = true;
                cms_shortcuts.Items[1].Enabled = true;
            }

            // Run the selected shortcut
            btn_run.PerformClick();
        }

        private void btn_new_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            ShortcutItem si = new ShortcutItem();
            ShowShortcutLoadingWindow();
            if (_shortcutForm == null)
            {
                _shortcutForm = new ShortcutForm();
            } 
            _shortcutForm.Owner = this;

            //ShortcutRepository.IsValidRefresh()
            // Set the Shortcut to as a new shortcut
            _shortcutForm.Shortcut = si;
            _shortcutForm.EditingExistingShortcut = false;            
            _shortcutForm.ShowDialog(this);
            if (_shortcutForm.DialogResult == DialogResult.OK)
            {
                ShortcutRepository.AddShortcut(_shortcutForm.Shortcut);
                _selectedShortcut = _shortcutForm.Shortcut;
                //ShortcutRepository.IsValidRefresh();
                RefreshShortcutLibraryUI();
                // As this is an edit, we need to manually force saving the shortcut library
                ShortcutRepository.SaveShortcuts();
                // We update the Game Shortcut context menu is always updated and correct.
                if (Program.AppProgramSettings.InstallDesktopContextMenu)
                {
                    ContextMenu.UpdateShortcutContextMenu();
                }

            }
            this.Cursor = Cursors.Default;
            RemoveWarningIfShortcuts();

            // Also refresh the right-click menu (if we have a main form loaded)
            if (Program.AppMainForm is Form)
            {
                Program.AppMainForm.RefreshNotifyIconMenus();
            }
        }

        private void ShowShortcutLoadingWindow()
        {
            Program.AppShortcutLoadingSplashScreen = new ShortcutLoadingForm();
            Program.AppShortcutLoadingSplashScreen.Title = "Scanning Game and App Data...";
            Program.AppShortcutLoadingSplashScreen.Description = "Getting the Shortcut information ready for you to edit. Scanning your computer for all Games and Apps so you can choose them from a list.";
            int resultX = this.DesktopLocation.X + ((this.Width - Program.AppShortcutLoadingSplashScreen.Width) / 2);
            int resultY = this.DesktopLocation.Y + ((this.Height - Program.AppShortcutLoadingSplashScreen.Height) / 2);
            Program.AppShortcutLoadingSplashScreen.WantedLocation = new Point(resultX, resultY);
            //_loadingScreen.CenterParent(ownerRect);
            //_loadingScreen.StartPosition = FormStartPosition.Manual;
            var splashThread = new Thread(new ThreadStart(
                () => Application.Run(Program.AppShortcutLoadingSplashScreen)));
            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.Start();
        }

        private void btn_edit_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut == null)
            {
                if (ShortcutRepository.ShortcutCount > 0)
                {
                    MessageBox.Show(
                        @"You need to select a Game Shortcut in order to edit it. Please select a Game Shortcut then try again, or right-click on the Game Shortcut and select 'Edit Shortcut'.",
                        @"Select Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    MessageBox.Show(
                        @"You need to create a Game Shortcut before you can edit it. Please create a Game Shortcut by clicking the New button.",
                        @"Create Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

            }

            if (ilv_saved_shortcuts.SelectedItems.Count > 0)
            {
                int currentIlvIndex = ilv_saved_shortcuts.SelectedItems[0].Index;
                string shortcutUUID = ilv_saved_shortcuts.Items[currentIlvIndex].EquipmentModel;
                _selectedShortcut = ShortcutRepository.GetShortcut(shortcutUUID);

                this.Cursor = Cursors.WaitCursor;
                ShowShortcutLoadingWindow();
                if (_shortcutForm == null)
                {
                    _shortcutForm = new ShortcutForm();
                }
                _shortcutForm.Owner = this; 

                _shortcutForm.Shortcut = _selectedShortcut;
                _shortcutForm.EditingExistingShortcut = true;
                //ilv_saved_shortcuts.SuspendLayout();
                _shortcutForm.ShowDialog(this);
                if (_shortcutForm.DialogResult == DialogResult.OK)
                {
                    RefreshShortcutLibraryUI();
                    // As this is an edit, we need to manually force saving the shortcut library
                    ShortcutRepository.SaveShortcuts();
                    // We update the Game Shortcut context menu is always updated and correct.
                    if (Program.AppProgramSettings.InstallDesktopContextMenu)
                    {
                        ContextMenu.UpdateShortcutContextMenu();
                    }

                }

                this.Cursor = Cursors.Default;

                // Also refresh the right-click menu (if we have a main form loaded)
                if (Program.AppMainForm is Form)
                {
                    Program.AppMainForm.RefreshNotifyIconMenus();
                }
            }
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut == null)
            {
                if (ShortcutRepository.ShortcutCount > 0)
                {
                    MessageBox.Show(
                        @"You need to select a Game Shortcut in order to delete it. Please select a Game Shortcut then try again, or right-click on the Game Shortcut and select 'Delete Shortcut'.",
                        @"Select Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    MessageBox.Show(
                        @"You need to create a Game Shortcut before you can delete it. Please create a Game Shortcut by clicking the New button.",
                        @"Create Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            if (MessageBox.Show($"Are you sure you want to delete the '{_selectedShortcut.Name}' Shortcut?", $"Delete '{_selectedShortcut.Name}' Shortcut?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                return;

            // remove the profile from the imagelistview
            int currentIlvIndex = ilv_saved_shortcuts.SelectedItems[0].Index;
            ilv_saved_shortcuts.Items.RemoveAt(currentIlvIndex);

            // Remove the hotkey if it is enabled for this shortcut
            if (_selectedShortcut.Hotkey != Keys.None)
            {
                // Remove the Hotkey if it needs to be removed
                HotkeyManager.Current.Remove(_selectedShortcut.UUID);
            }

            // Remove the shortcut
            ShortcutRepository.RemoveShortcut(_selectedShortcut);
            _selectedShortcut = null;

            ShortcutRepository.IsValidRefresh();
            RefreshShortcutLibraryUI();
            RemoveWarningIfShortcuts();
            // We update the Game Shortcut context menu is always updated and correct.
            if (Program.AppProgramSettings.InstallDesktopContextMenu)
            {
                ContextMenu.UpdateShortcutContextMenu();
            }


            // Also refresh the right-click menu (if we have a main form loaded)
            if (Program.AppMainForm is Form)
            {
                Program.AppMainForm.RefreshNotifyIconMenus();
            }
        }

        private void btn_run_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut == null)
            {
                if (ShortcutRepository.ShortcutCount > 0)
                {
                    MessageBox.Show(
                        @"You need to select a Game Shortcut in order to run it. Please select a Game Shortcut then try again, or right-click on the Game Shortcut and select 'Run Shortcut'. Please note you cannot run an invalid Game Shortcut.",
                        @"Select Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    MessageBox.Show(
                        @"You need to create a Game Shortcut in order to run it. Please create a Game Shortcut by clicking the New button.",
                        @"Create Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }

            // Only run the if shortcut is valid
            if (_selectedShortcut.IsValid == ShortcutValidity.Warning || _selectedShortcut.IsValid == ShortcutValidity.Error)
            {
                // We tell the user the reason that we couldnt run the shortcut
                if (MessageBox.Show($"The shortcut '{_selectedShortcut.Name}' isn't valid for some reason so we cannot run the application or game. Has your hardware or screen layout changed from when the shortcut was made? We recommend that you edit the shortcut to make it valid again, or reverse the hardware changes you made. Do you want to do that now?", $"Edit the '{_selectedShortcut.Name}' Shortcut?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                    return;
                else
                    btn_edit.PerformClick();
            }

            // Figure out the string we're going to use as the MaskedForm message
            string message = "";
            if (_selectedShortcut.Category.Equals(ShortcutCategory.Game))
                message = $"Running the {_selectedShortcut.GameName} game and waiting until you close it.";
            else if (_selectedShortcut.Category.Equals(ShortcutCategory.Executable))
                message = $"Running the {_selectedShortcut.ExecutableNameAndPath} executable and waiting until you close it.";
            else if(_selectedShortcut.Category.Equals(ShortcutCategory.Application))
                message = $"Running the {_selectedShortcut.ApplicationName} application and waiting until you close it.";

            // Create a Mask Control that will cover the ShortcutLibrary Window to lock
            lbl_mask.Text = message;
            lbl_mask.Location = new Point(0, 0);
            lbl_mask.Size = this.Size;
            lbl_mask.BackColor = Color.FromArgb(100, Color.Black);
            lbl_mask.BringToFront();
            lbl_mask.Visible = true;

            // Show the cancel button
            btn_cancel.Visible = true;
            btn_cancel.Enabled = true;
            btn_cancel.BringToFront();

            ilv_saved_shortcuts.SuspendLayout();
            ilv_saved_shortcuts.Refresh();

            RunShortcutResult result = RunShortcutResult.Error;
            try
            {
                result = Program.RunShortcutTask(_selectedShortcut);
            }
            catch (OperationCanceledException ex)
            {
                if (ex.CancellationToken == Program.AppCancellationTokenSource.Token)
                    logger.Trace($"ShortcutLibraryForm/btn_run_Click: Cancellation token provided while running shortcut {_selectedShortcut.Name}. User asked to cancel.");
            }            
            catch(Exception ex)
            {
                logger.Error(ex, $"ShortcutLibraryForm/btn_run_Click: An exception occurred while trying to run the shortcut {_selectedShortcut.Name}.");
            }

            ilv_saved_shortcuts.ResumeLayout();

            // Remove the Masked Control to allow the user to start using DisplayMagician again.
            lbl_mask.Visible = false;
            lbl_mask.SendToBack();

            // Hide the cancel button
            btn_cancel.Visible = false;
            btn_cancel.Enabled = false;

            if (Program.AppMainForm is Form)
            {
                // Center the MainAppForm
                Utils.CenterOnPrimaryScreen(Program.AppMainForm);
                // Also refresh the right-click menu (if we have a main form loaded)
                Program.AppMainForm.RefreshNotifyIconMenus();

            }
            // Bring the window back to the front            
            Utils.ActivateCenteredOnPrimaryScreen(this);


        }

        private void ilv_saved_shortcuts_ItemHover(object sender, ItemHoverEventArgs e)
        {
            if (e.Item != null)
            {
                tt_selected.SetToolTip(ilv_saved_shortcuts, e.Item.Text);
            }
            else
            {
                tt_selected.RemoveAll();
            }
        }

        /*private void ShortcutLibraryForm_Activated(object sender, EventArgs e)
        {
            // Refresh the Shortcut Library UI
            RefreshShortcutLibraryUI();

            RemoveWarningIfShortcuts();
        }*/

        private void tsmi_save_to_desktop_Click(object sender, EventArgs e)
        {
            btn_save.PerformClick();
        }

        private void tsmi_run_Click(object sender, EventArgs e)
        {
            btn_run.PerformClick();
        }

        private void tsmi_edit_Click(object sender, EventArgs e)
        {
            btn_edit.PerformClick();
        }

        private void tsmi_delete_Click(object sender, EventArgs e)
        {
            btn_delete.PerformClick();
        }

        private void tsmi_copy_Click(object sender, EventArgs e)
        {
            btn_copy.PerformClick();
        }

        private void ShortcutLibraryForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (lbl_mask.Visible == true)
            {
                if (e.KeyChar == 27)
                {
                    // We set the CancellationTokenSource to true and it will be picked up by the shortcut check.
                    Program.AppCancellationTokenSource.Cancel();
                }
            }
        }

        private void btn_help_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/wiki/Troubleshooting-DisplayMagician";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
        }

        private void btn_donate_Click(object sender, EventArgs e)
        {
            string targetURL = "https://github.com/sponsors/terrymacdonald?frequency=one-time";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
            // Update the settings to say that user has donated.
            Utils.UserHasDonated();

        }

        private void btn_copy_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut == null)
            {
                if (ShortcutRepository.ShortcutCount > 0)
                {
                    MessageBox.Show(
                        @"You need to select a Game Shortcut in order to copy it. Please select a Game Shortcut then try again, or right-click on the Game Shortcut and select 'Copy Shortcut'.",
                        @"Select Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                else
                {
                    MessageBox.Show(
                        @"You need to create a Game Shortcut in order to copy it. Please create a Game Shortcut by clicking the New button.",
                        @"Create Game Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
            }
            else
            {
                ShortcutItem copiedShortcut;
                // Copy the shortcut
                ShortcutRepository.CopyShortcut(_selectedShortcut, out copiedShortcut);
                // Select the new copied shortcut
                _selectedShortcut = copiedShortcut;
                // Invalidate the list of shortcuts so it gets redrawn again with the copy included!
                ilv_saved_shortcuts.Invalidate();
                // Refresh the UI
                RefreshShortcutLibraryUI();
            }
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            // Inform the ShortcutRepository that it needs to cancel the running shortcut.
            Program.AppCancellationTokenSource.Cancel();
        }

        private void sendToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut != null)
            {
                string commandline = _selectedShortcut.CreateCommand();
                Clipboard.SetText(commandline);
            }
                
        }
    }
}
