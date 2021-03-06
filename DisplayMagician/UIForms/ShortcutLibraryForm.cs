using DisplayMagician.GameLibraries;
using DisplayMagician.Resources;
using DisplayMagicianShared;
using Manina.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class ShortcutLibraryForm : Form
    {

        private ShortcutAdaptor _shortcutAdaptor = new ShortcutAdaptor();
        private ShortcutItem _selectedShortcut = null;
        //public static Dictionary<string, bool> shortcutValidity = new Dictionary<string, bool>();

        public ShortcutLibraryForm()
        {
            InitializeComponent();
            ilv_saved_shortcuts.MultiSelect = false;
            ilv_saved_shortcuts.ThumbnailSize = new Size(100,100);
            ilv_saved_shortcuts.AllowDrag = false;
            ilv_saved_shortcuts.AllowDrop = false;
            ilv_saved_shortcuts.SetRenderer(new ShortcutILVRenderer());
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShortcutLibraryForm_Load(object sender, EventArgs e)
        {
            // Refresh the Shortcut Library UI
            RefreshShortcutLibraryUI();

            RemoveWarningIfShortcuts();
        }



        private void RefreshShortcutLibraryUI()
        {

            if (ShortcutRepository.ShortcutCount == 0)
                return;

            ProfileRepository.IsPossibleRefresh();


            // Temporarily stop updating the saved_profiles listview
            ilv_saved_shortcuts.SuspendLayout();            

            ImageListViewItem newItem = null;
            ilv_saved_shortcuts.Items.Clear();
            ShortcutRepository.IsValidRefresh();

            foreach (ShortcutItem loadedShortcut in ShortcutRepository.AllShortcuts.OrderBy(s => s.Name))
            {
                newItem = new ImageListViewItem(loadedShortcut, loadedShortcut.Name);

                // Select it if its the selectedProfile
                if (_selectedShortcut is ShortcutItem && _selectedShortcut.Equals(loadedShortcut))
                    newItem.Selected = true;

                //ilv_saved_profiles.Items.Add(newItem);
                ilv_saved_shortcuts.Items.Add(newItem, _shortcutAdaptor);
            }

    
            // Restart updating the saved_profiles listview
            ilv_saved_shortcuts.ResumeLayout();

        }
    
        private ShortcutItem GetShortcutFromName(string shortcutName)
        {
            return (from item in ShortcutRepository.AllShortcuts where item.Name == shortcutName select item).First();
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.None;

            // Only do something if there is a shortcut selected
            if (_selectedShortcut != null)
            {

                // if shortcut is not valid then ask if the user
                // really wants to save it to desktop
                (ShortcutValidity valid, string reason) = _selectedShortcut.IsValid();
                if (valid == ShortcutValidity.Error || valid == ShortcutValidity.Warning)
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
                    if (_selectedShortcut.DisplayPermanence == ShortcutPermanence.Permanent)
                    {

                        dialog_save.FileName = _selectedShortcut.Name;
                    }
                    else
                    {
                        if (_selectedShortcut.Category == ShortcutCategory.Application)
                        {
                            dialog_save.FileName = String.Concat(Path.GetFileNameWithoutExtension(_selectedShortcut.ExecutableNameAndPath), @" (", _selectedShortcut.Name.ToLower(CultureInfo.InvariantCulture), @")");
                        }
                        else
                        {
                            dialog_save.FileName = _selectedShortcut.Name;
                        }
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
                        DialogResult = DialogResult.OK;
                    }
                }
                catch (Exception ex)
                {
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
            _selectedShortcut = GetShortcutFromName(e.Item.Text);

            SetRunOption();

            if (e.Buttons == MouseButtons.Right)
            {
                cms_shortcuts.Show(ilv_saved_shortcuts,e.Location);
            }
        }

        private void ilv_saved_shortcuts_ItemDoubleClick(object sender, ItemClickEventArgs e)
        {
            _selectedShortcut = GetShortcutFromName(e.Item.Text);

            SetRunOption();

            if (_selectedShortcut == null)
                return;

            if (!ShortcutRepository.ShortcutWarningLookup[_selectedShortcut.Name])
                return;

            // Run the selected shortcut
            btn_run.PerformClick();
        }

        private void SetRunOption()
        {
            if (ShortcutRepository.ShortcutWarningLookup[_selectedShortcut.Name] || ShortcutRepository.ShortcutErrorLookup[_selectedShortcut.Name])
            {
                btn_run.Visible = false;
                cms_shortcuts.Items[1].Enabled = false;
            }

            else
            {
                btn_run.Visible = true;
                cms_shortcuts.Items[1].Enabled = true;
            }
                
        }

        private void btn_new_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            var shortcutForm = new ShortcutForm(new ShortcutItem());
            shortcutForm.ShowDialog(this);
            if (shortcutForm.DialogResult == DialogResult.OK)
            {
                ShortcutRepository.AddShortcut(shortcutForm.Shortcut);
                _selectedShortcut = shortcutForm.Shortcut;
                RefreshShortcutLibraryUI();
            }
            this.Cursor = Cursors.Default;
            RemoveWarningIfShortcuts();

        }

        private void btn_edit_Click(object sender, EventArgs e)
        {
            int currentIlvIndex = ilv_saved_shortcuts.SelectedItems[0].Index;
            string shortcutName = ilv_saved_shortcuts.Items[currentIlvIndex].Text;
            _selectedShortcut = GetShortcutFromName(shortcutName);

            if (_selectedShortcut == null)
                return;

            this.Cursor = Cursors.WaitCursor;

            var shortcutForm = new ShortcutForm(_selectedShortcut);
            shortcutForm.ShowDialog(this);
            if (shortcutForm.DialogResult == DialogResult.OK)
            {
                RefreshShortcutLibraryUI();
                // As this is an edit, we need to manually force saving the shortcut library
                ShortcutRepository.SaveShortcuts();
            }

            this.Cursor = Cursors.Default;
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut == null)
                return;

            if (MessageBox.Show($"Are you sure you want to delete the '{_selectedShortcut.Name}' Shortcut?", $"Delete '{_selectedShortcut.Name}' Shortcut?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                return;

            // remove the profile from the imagelistview
            int currentIlvIndex = ilv_saved_shortcuts.SelectedItems[0].Index;
            ilv_saved_shortcuts.Items.RemoveAt(currentIlvIndex);

            // Remove the shortcut
            ShortcutRepository.RemoveShortcut(_selectedShortcut);
            _selectedShortcut = null;

            RefreshShortcutLibraryUI();
            RemoveWarningIfShortcuts();
        }

        private void btn_run_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut == null)
                return;

            // Only run the if shortcut is valid
            (ShortcutValidity valid, string reason) = _selectedShortcut.IsValid();
            if (valid == ShortcutValidity.Error || valid == ShortcutValidity.Warning)
            {
                // We tell the user the reason that we couldnt run the shortcut
                if (MessageBox.Show($"The shortcut '{_selectedShortcut.Name}' isn't valid for some reason so we cannot run the application or game. Has your hardware or screen layout changed from when the shortcut was made? We recommend that you edit the shortcut to make it valid again, or reverse the hardware changes you made. Do you want to do that now?", $"Edit the '{_selectedShortcut.Name}' Shortcut?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                    return;
                else
                    btn_edit.PerformClick();
            }

            // Figure out the string we're going to use as the MaskedForm message
            string message = "";
            if (_selectedShortcut.Category.Equals(ShortcutCategory.Application))
                message = $"Running the {_selectedShortcut.ExecutableNameAndPath} application and waiting until you close it.";
            else if (_selectedShortcut.Category.Equals(ShortcutCategory.Game))
                message = $"Running the {_selectedShortcut.GameName} game and waiting until you close it.";

            // Create a MaskForm that will cover the ShortcutLibrary Window to lock
            // the controls and inform the user that the game is running....
            MaskedForm maskedForm = MaskedForm.Show(this, message);

            // Get the MainForm so we can access the NotifyIcon on it.
            MainForm mainForm = (MainForm)this.Owner;


            // Run the shortcut
            ShortcutRepository.RunShortcut(_selectedShortcut, mainForm.notifyIcon);

            maskedForm.Close();
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

        private void ShortcutLibraryForm_Activated(object sender, EventArgs e)
        {
            // Refresh the Shortcut Library UI
            RefreshShortcutLibraryUI();

            RemoveWarningIfShortcuts();
        }

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
    }
}
