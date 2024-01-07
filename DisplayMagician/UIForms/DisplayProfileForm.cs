using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DisplayMagician.Resources;
using DisplayMagicianShared;
using DisplayMagicianShared.Windows;
using Manina.Windows.Forms;
using System.Drawing;
using NHotkey.WindowsForms;
using NHotkey;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DisplayMagician.UIForms
{
    internal partial class DisplayProfileForm : Form
    {
        private ProfileItem _selectedProfile;
        //private List<ProfileItem> _savedProfiles = new List<ProfileItem>();
        private string _saveOrRenameMode = "save";
        //private static bool _inDialog = false;
        private static ProfileItem _profileToLoad = null;
        private ProfileAdaptor _profileAdaptor = new ProfileAdaptor();
        //public static Dictionary<string, bool> profileValidity = new Dictionary<string, bool>();
        public Task _monitorTaskBarRegKeysForChangesTask = null;
        //public bool  _monitorTaskBarRegKeysForChanges = false;
        //private readonly object _monitorTaskBarRegKeysForChangesLock = new object();

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public DisplayProfileForm()
        {
            InitializeComponent();
            this.AcceptButton = this.btn_save_or_rename;
            ilv_saved_profiles.MultiSelect = false;
            ilv_saved_profiles.ThumbnailSize = new Size(100, 100);
            ilv_saved_profiles.AllowDrag = false;
            ilv_saved_profiles.AllowDrop = false;
            ilv_saved_profiles.SetRenderer(new ProfileILVRenderer());
            // Center the form on the primary screen
            Utils.CenterOnPrimaryScreen(this);
        }

        public DisplayProfileForm(ProfileItem profileToLoad) : this()
        {
            _profileToLoad = profileToLoad;
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            if (_selectedProfile == null)
                return;

            if (!_selectedProfile.IsPossible)
            {
                MessageBox.Show(this, Language.This_profile_is_currently_impossible_to_apply,
                    Language.Apply_Profile,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            ProfileRepository.UserChangingProfiles = true;

            // Apply the Profile
            //if (ProfileRepository.ApplyProfile(_selectedProfile) == ApplyProfileResult.Successful)
            ApplyProfileResult result = Program.ApplyProfileTask(_selectedProfile);
            if (result == ApplyProfileResult.Successful)
            {
                logger.Trace($"DisplayProfileForm/Apply_Click: The Profile {_selectedProfile.Name} was successfully applied. Waiting 0.5 sec for the display to settle after the change.");
                System.Threading.Thread.Sleep(500);
                logger.Trace($"DisplayProfileForm/Apply_Click: Changing the selected profile in the imagelistview to Profile {_selectedProfile.Name}.");
                ChangeSelectedProfile(_selectedProfile);
                MainForm myMainForm = Program.AppMainForm;
                myMainForm.UpdateNotifyIconText($"DisplayMagician ({ProfileRepository.CurrentProfile.Name})");
            }
            else if (result == ApplyProfileResult.Cancelled)
            {
                logger.Warn($"DisplayProfileForm/Apply_Click: The user cancelled changing to Profile {_selectedProfile.Name}.");
            }
            else
            {
                logger.Error($"DisplayProfileForm/Apply_Click: Error applying the Profile {_selectedProfile.Name}. Unable to change the display layout.");
            }

            // Recenter the Window
            RecenterWindow();

            ProfileRepository.UserChangingProfiles = false;
        }

        private void RecenterWindow()
        {
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


        private void Exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private void Delete_Click(object sender, EventArgs e)
        {
            if (_selectedProfile == null)
                return;

            if (MessageBox.Show($"Are you sure you want to delete the '{_selectedProfile.Name}' Display Profile?", $"Delete '{_selectedProfile.Name}' Display Profile?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                return;

            // remove the profile from the imagelistview
            int currentIlvIndex = ilv_saved_profiles.SelectedItems[0].Index;
            ilv_saved_profiles.Items.RemoveAt(currentIlvIndex);

            // Remove the hotkey if it is enabled for this profile
            if (_selectedProfile.Hotkey != Keys.None)
            {
                // Remove the Hotkey if it needs to be removed
                HotkeyManager.Current.Remove(_selectedProfile.UUID);
            }


            // Remove the Profile
            ProfileRepository.RemoveProfile(_selectedProfile);
            _selectedProfile = null;

            // If the imageview isn't empty
            if (ilv_saved_profiles.Items.Count > 0)
            {
                // set the new selected profile as the next one in the imagelistview
                // or the new end one if we deleted the last one before
                int ilvItemToSelect = currentIlvIndex;
                if (ilv_saved_profiles.Items.Count < currentIlvIndex + 1)
                    ilvItemToSelect = ilv_saved_profiles.Items.Count - 1;

                // Set the nearest profile image as selected
                ilv_saved_profiles.Items[ilvItemToSelect].Selected = true;

                // select the 
                foreach (ProfileItem newSelectedProfile in ProfileRepository.AllProfiles)
                {
                    if (newSelectedProfile.UUID.Equals(ilv_saved_profiles.Items[ilvItemToSelect].EquipmentModel))
                    {
                        ChangeSelectedProfile(newSelectedProfile);
                    }
                }
            }
            else
            {
                // We now only have an unsaved current profile, and no saved ones
                // So we need to change the mode
                ChangeSelectedProfile(ProfileRepository.CurrentProfile);

            }

            // As this may impact which game shortcuts are now usable, also force a refresh of the game shortcuts validity
            ShortcutRepository.IsValidRefresh();

            // Also refresh the right-click menu (if we have a main form loaded)
            if (Program.AppMainForm is Form)
            {
                Program.AppMainForm.RefreshNotifyIconMenus();
            }

        }

        private void Save_Click(object sender, EventArgs e)
        {
            //DialogResult = DialogResult.None;

            // Only do something if there is a shortcut selected
            if (_selectedProfile != null)
            {

                // if shortcut is not valid then ask if the user
                // really wants to save it to desktop
                if (!_selectedProfile.IsPossible)
                {
                    // We ask the user of they still want to save the desktop shortcut
                    if (MessageBox.Show($"The '{_selectedProfile.Name}' Desktop Profile isn't possible to use right now so a desktop shortcut wouldn't work until your Displays are changed to match the profile. Has your hardware or screen layout changed from when the Display Profile was made? Do you still want to save the desktop shortcut?", $"Still save the '{_selectedProfile.Name}' Display Profile?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                        return;
                }

                try
                {
                    // Set the profile save folder to the Desktop as that's where people will want it most likely
                    dialog_save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    // Try to set up some sensible suggestions for the profile name
                    dialog_save.FileName = _selectedProfile.Name;

                    // Show the Save Profile window
                    if (dialog_save.ShowDialog(this) == DialogResult.OK)
                    {
                        if (_selectedProfile.CreateShortcut(dialog_save.FileName))
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
                    MessageBox.Show(ex.Message, Language.Shortcut, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

            }
        }


        private void RefreshDisplayProfileUI()
        {

            ImageListViewItem newItem = null;

            // Temporarily stop updating the saved_profiles listview
            // To stop the display showing all sorts of changes happening
            ilv_saved_profiles.SuspendLayout();

            // Figure out if anything is selected at the moment
            // and if it is save it to reselect it after the refresh
            // We only take the first as there is only one thing selected at a time
            /*string lastSelectedItemName = "";
            if (ilv_saved_profiles.SelectedItems.Count > 0)
                lastSelectedItemName = ilv_saved_profiles.SelectedItems[0].Text;
*/
            // Empty the imageListView
            ilv_saved_profiles.Items.Clear();

            //IOrderedEnumerable<ProfileItem> orderedProfiles = ProfileRepository.AllProfiles.OrderBy(p => p.Name);

            // Check if the last selected profile is still in the list of profiles
            //bool lastSelectedItemStillThere = (from profile in orderedProfiles select profile.Name).Contains(lastSelectedItemName);

            // Fill it back up with the Profiles we have
            foreach (ProfileItem profile in ProfileRepository.AllProfiles.OrderBy(p => p.Name))
            {
                // Create a new ImageListViewItem from the profile
                newItem = new ImageListViewItem(profile, profile.Name);

                // if the item was removed from the list during this 
                // list refresh, then we select this profile only if it 
                // is the currently used Profile
                if (profile.Equals(_selectedProfile))
                    newItem.Selected = true;

                // Add it to the list!
                ilv_saved_profiles.Items.Add(newItem, _profileAdaptor);

            }

            // Restart updating the saved_profiles listview
            ilv_saved_profiles.ResumeLayout();
        }


        /*private void DisplayProfileForm_Activated(object sender, EventArgs e)
        {
            // We handle the UI updating in DisplayProfileForm_Activated so that
            // the app will check for changes to the current profile when the
            // user clicks back to this app. This is designed to allow people to
            // alter their Windows Display settings then come back to our app
            // and the app will automatically recognise that things have changed.

            // Reload the profiles in case we swapped to another program to change it
            ChangeSelectedProfile(ProfileRepository.CurrentProfile);
            // Refresh the Profile UI
            RefreshDisplayProfileUI();
        }*/

        private void DisplayProfileForm_Load(object sender, EventArgs e)
        {

            // Refresh the profiles to see whats valid
            ProfileRepository.IsPossibleRefresh();

            // Update the Current Profile, but if another task is running then just wait.
            if (Program.AppBackgroundTaskSemaphoreSlim.CurrentCount == 0)
            {
                logger.Error($"DisplayProfileForm/DisplayProfileForm_Load: Waiting to run the UpdateActiveProfile as there is another Task running!");
            }
            Program.AppBackgroundTaskSemaphoreSlim.Wait();
            logger.Trace($"DisplayProfileForm/DisplayProfileForm_Load: Running the UpdateActiveProfile as there are no other Tasks running!");
            ProfileRepository.UpdateActiveProfile();
            Program.AppBackgroundTaskSemaphoreSlim.Release();

            ChangeSelectedProfile(ProfileRepository.CurrentProfile);

            // Refresh the Profile UI
            RefreshDisplayProfileUI();
                
        }


        private void ChangeSelectedProfile(ProfileItem profile)
        {
            // And we need to update the actual selected profile too!
            _selectedProfile = profile;

            // We also need to load the saved profile name to show the user
            lbl_profile_shown.Text = _selectedProfile.Name;

            // And show the logo for the driver
            if (_selectedProfile.VideoMode == VIDEO_MODE.NVIDIA)
            {
                pbLogo.Image = PickBitmapBasedOnBgColour(BackColor, Properties.Resources.nvidiablack, Properties.Resources.nvidiawhite);
            }
            else if (_selectedProfile.VideoMode == VIDEO_MODE.AMD)
            {
                pbLogo.Image = PickBitmapBasedOnBgColour(BackColor, Properties.Resources.amdblack, Properties.Resources.amdwhite);
            }
            else
            {
                pbLogo.Image = PickBitmapBasedOnBgColour(BackColor, Properties.Resources.winblack, Properties.Resources.winwhite);
            }

            // And update the save/rename textbox
            txt_profile_save_name.Text = _selectedProfile.Name;

            if (ProfileRepository.ContainsProfile(profile))
            {
                // we already have the profile stored
                _saveOrRenameMode = "rename";
                btn_save_or_rename.Text = "Rename To";
                lbl_save_profile.Visible = false;
                if (!_selectedProfile.IsPossible)
                {
                    lbl_profile_shown_subtitle.Text = "This Display Profile can't be used as not all Displays are connected.";
                    btn_apply.Visible = false;
                }
                else
                {
                    if (ProfileRepository.IsActiveProfile(_selectedProfile))
                    {
                        btn_apply.Visible = false;
                        btn_update.Visible = false;
                        lbl_profile_shown_subtitle.Text = "This is the Display Profile currently in use.";
                        cms_profiles.Items[0].Enabled = false;
                    }
                    else
                    {
                        btn_apply.Visible = true;
                        btn_update.Visible = true;
                        lbl_profile_shown_subtitle.Text = "";
                        cms_profiles.Items[0].Enabled = true;
                    }
                }
            }
            else
            {
                // we don't have the profile stored yet
                _saveOrRenameMode = "save";
                btn_save_or_rename.Text = "Save";
                lbl_profile_shown_subtitle.Text = "The current Display configuration hasn't been saved as a Display Profile yet.";
                btn_apply.Visible = false;
                btn_update.Visible = false;
                lbl_save_profile.Visible = true;
            }

            // Update the Hotkey Label text
            UpdateHotkeyLabel(_selectedProfile.Hotkey);

            // Refresh the image list view
            //RefreshImageListView(profile);

            // Also refresh the right-click menu (if we have a main form loaded)
            if (Program.AppMainForm is Form)
            {
                Program.AppMainForm.RefreshNotifyIconMenus();
            }

            // And finally refresh the profile in the display view
            dv_profile.Profile = profile;
            dv_profile.Refresh();


        }

        

        private void btn_save_as_Click(object sender, EventArgs e)
        {
            // Check there is a name
            if (String.IsNullOrWhiteSpace(txt_profile_save_name.Text))
            {
                logger.Warn($"DisplayProfileForm/btn_save_as_Click: You need to provide a name for this profile before it can be saved.");
                MessageBox.Show("You need to provide a name for this profile before it can be saved.", "Your profile needs a name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check the name is valid
            if (!Program.IsValidFilename(txt_profile_save_name.Text))
            {
                logger.Warn($"DisplayProfileForm/btn_save_as_Click: The profile name cannot contain the following characters: {Path.GetInvalidFileNameChars()}. Unable to save this profile.");
                MessageBox.Show($"The profile name cannot contain the following characters: [{Path.GetInvalidFileNameChars()}]. Please change the profile name.", "Invalid characters in profile name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check we're not already using the name
            foreach (ProfileItem savedProfile in ProfileRepository.AllProfiles)
            {
                //if (String.Equals(txt_profile_save_name.Text, savedProfile.Name, StringComparison.InvariantCultureIgnoreCase))
                if (savedProfile.Name.Equals(txt_profile_save_name.Text))
                {
                    logger.Warn($"DisplayProfileForm/btn_save_as_Click: The profile name {txt_profile_save_name.Text} already exists. Each profile name must be unique. Unable to save this profile.");
                    MessageBox.Show("Sorry, each saved display profile needs a unique name. Please change the profile name.", "Profile name already exists", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // If we're saving the current profile as a new item
            // then we'll be in "save" mode
            if (_saveOrRenameMode == "save")
            {
                // We're in 'save' mode!

                // Check we're not already saving this profile
                string previouslySavedProfileName;
                if (ProfileRepository.ContainsCurrentProfile(out previouslySavedProfileName))
                {
                    MessageBox.Show($"Sorry, this display profile was already saved as '{previouslySavedProfileName}'.", "Profile already saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    // Check the config actual results in an image (might be a logic error that we missed)
                    if (_selectedProfile.ProfileBitmap.Width == 0 || _selectedProfile.ProfileBitmap.Height == 0)
                    {
                        logger.Warn($"DisplayProfileForm/btn_save_as_Click: Display Layout image rendering error (ProfileBitmap)! We won't be able to save this profile. Please log a new issue at https://github.com/terrymacdonald/DisplayMagician/issues/new/choose");
                        MessageBox.Show("Display Layout image rendering error (ProfileBitmap)! We won't be able to save this profile. Please log a new issue at https://github.com/terrymacdonald/DisplayMagician/issues/new/choose", "Display rendering error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Check the config actual results in an image (might be a logic error that we missed)
                    if (_selectedProfile.ProfileTightestBitmap.Width == 0 || _selectedProfile.ProfileTightestBitmap.Height == 0)
                    {
                        logger.Warn($"DisplayProfileForm/btn_save_as_Click: Display Layout image rendering error (ProfileTightestBitmap)! We won't be able to save this profile. Please log a new issue at https://github.com/terrymacdonald/DisplayMagician/issues/new/choose");
                        MessageBox.Show("Display Layout image rendering error (ProfileTightestBitmap)! We won't be able to save this profile. Please log a new issue at https://github.com/terrymacdonald/DisplayMagician/issues/new/choose", "Display rendering error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }               
                catch (Exception ex)
                {
                    logger.Warn($"DisplayProfileForm/btn_save_as_Click: Exception whilst trying to save the display layout. We won't be able to save this profile. Please log a new issue at https://github.com/terrymacdonald/DisplayMagician/issues/new/choose");
                    MessageBox.Show("Exception whilst trying to save the display layout. We won't be able to save this profile. Please log a new issue at https://github.com/terrymacdonald/DisplayMagician/issues/new/choose", "Display rendering error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                }
                // So we've already passed the check that says this profile is unique

                // Update the name just to make sure we record it if the user changed it
                _selectedProfile.Name = txt_profile_save_name.Text;

                // Add the current profile to the list of profiles so it gets saved
                ProfileRepository.AddProfile(_selectedProfile);

                // Also update the imagelistview so that we can see the new profile we just saved

                // Load the currentProfile image into the imagelistview
                //ImageListViewItem newItem = new ImageListViewItem(_selectedProfile.SavedProfileCacheFilename, _selectedProfile.Name);
                ImageListViewItem newItem = new ImageListViewItem(_selectedProfile, _selectedProfile.Name)
                {
                    Selected = true
                };
                //ilv_saved_profiles.Items.Add(newItem);
                ilv_saved_profiles.Items.Add(newItem, _profileAdaptor);
            }
            else
            {
                // We're in 'rename' mode!
                // Check the name is the same, and if so do nothing
                if (_selectedProfile.Name.Equals(txt_profile_save_name.Text))
                {
                    return;
                }

                // Lets save the old names for usage next
                string oldProfileName = _selectedProfile.Name;

                // Lets rename the selectedProfile to the new name
                ProfileRepository.RenameProfile(_selectedProfile, txt_profile_save_name.Text);

                // Lets rename the entry in the imagelistview to the new name
                foreach (ImageListViewItem myItem in ilv_saved_profiles.Items)
                {
                    if (myItem.Text == oldProfileName)
                    {
                        myItem.Text = txt_profile_save_name.Text;
                    }
                }

                // Lets update the rest of the profile screen too
                lbl_profile_shown.Text = txt_profile_save_name.Text;

                // And we also need to go through the any Shortcuts that use the profile and rename them too!
                ShortcutRepository.RenameShortcutProfile(_selectedProfile);
                

            }

            ChangeSelectedProfile(_selectedProfile);

            // now update the profiles image listview
            RefreshDisplayProfileUI();

            // Also refresh the right-click menu (if we have a main form loaded)
            if (Program.AppMainForm is Form)
            {
                Program.AppMainForm.RefreshNotifyIconMenus();
            }

        }

        private void ilv_saved_profiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            foreach (ProfileItem savedProfile in ProfileRepository.AllProfiles)
            {
                if (savedProfile.Name == e.Item.Text)
                {
                    ChangeSelectedProfile(savedProfile);
                }
            }

            if (e.Buttons == MouseButtons.Right)
            {
                cms_profiles.Show(ilv_saved_profiles, e.Location);
            }

        }

        private void btn_view_current_Click(object sender, EventArgs e)
        {
            ProfileRepository.UserChangingProfiles = true;
            // Refresh the profiles to see whats valid
            ProfileRepository.IsPossibleRefresh();
            // Reload the profiles in case we swapped to another program to change it
            ProfileRepository.UpdateActiveProfile(false);
            // Change to the current selected Profile
            ChangeSelectedProfile(ProfileRepository.GetActiveProfile());
            // Refresh the Profile UI
            RefreshDisplayProfileUI();
            // Recenter the Window
            RecenterWindow();
            ProfileRepository.UserChangingProfiles = false;
        }

        private void txt_profile_save_name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                //MessageBox.Show("Click works!", "Click works", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btn_save.PerformClick();                
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DISPLAYCHANGE = 0x007E;
            const int WM_SETTINGCHANGE = 0x001A;
            const int WM_DEVICECHANGE = 0x0219;

            const int DBT_DEVICEARRIVAL = 0x8000;
            const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

            switch (m.Msg)
            {

                case WM_DEVICECHANGE:
                    switch ((int)m.WParam)
                    {
                        case DBT_DEVICEARRIVAL:
                            logger.Trace($"DisplayProfileForm/WndProc: Windows just sent a msg telling us a device has been added. We need to check if this was a USB display. Updating the current view by running btn_view_current.");
                            btn_view_current.PerformClick();
                            break;

                        case DBT_DEVICEREMOVECOMPLETE:
                            logger.Trace($"DisplayProfileForm/WndProc: Windows just sent a msg telling us a device has been removed. We need to check if this was a USB display. Updating the current view by running btn_view_current.");
                            btn_view_current.PerformClick();
                            break;
                    }
                    break;

                case WM_DISPLAYCHANGE:
                    logger.Trace($"DisplayProfileForm/WndProc: Windows just sent a msg telling us the display has changed. Updating the current view by running btn_view_current.");
                    if (!ProfileRepository.UserChangingProfiles)
                    {
                        btn_view_current.PerformClick();
                    }                    
                    break;

                    // This auto taskbar detection logic just doesn't work at the moment
                    // It tries to set a 5 second timer when it detects a settings change, and tries every 1 second to see if the taskbar position has changed
                    // If taskbar position changed, then it attempts to get the new display layout.
                    // In reality, it appears that multiple tasks are firing for each message, and the multople tasks are confusing each other. So I'm going to leave this be for now.
                /*case WM_SETTINGCHANGE:
                    switch ((int)m.WParam)
                    {
                        case 0x2f:
                            // This occurs when the taskbar is moved! We use it to set a timer to monitor the relevant registry keys for changes within the next 10 seconds
                            logger.Trace($"DisplayProfileForm/WndProc: Windows just sent a msg telling us a taskbar has been moved. We need to set a timer to check for registry key changes within the next 10 seconds, and update the current view if that happens.");
                            if (_monitorTaskBarRegKeysForChangesTask == null)
                            {
                                logger.Trace($"DisplayProfileForm/WndProc: We are starting to monitor the taskbar for changes for the next 10 seconds.");
                                //_monitorTaskBarRegKeysForChanges = true;                                
                                List<TaskBarStuckRectangle> original = TaskBarStuckRectangle.GetAllTaskBarStuckRectangles();
                                _monitorTaskBarRegKeysForChangesTask = new Task((Action)delegate
                                {
                                    bool _itChanged = false;                                    
                                    for (int d = 0; d < 10; d++)
                                    {
                                        Task.Delay(1000);
                                        List<TaskBarStuckRectangle> subsequent = TaskBarStuckRectangle.GetAllTaskBarStuckRectangles();
                                        bool matched = true;
                                        for (int x = 0; x < subsequent.Count; x++)
                                        {
                                            if (!original[x].Equals(subsequent[x]))
                                            {
                                                matched = false;
                                                break;
                                            }
                                        }
                                        if (!matched)
                                        {
                                            logger.Trace($"DisplayProfileForm/WndProc: The taskbar registry key has been updated within the 10 seconds of a taskbar move message, so updating the config again window.");
                                            if (btn_view_current.InvokeRequired)
                                            {
                                                this.Invoke(new Action(() => btn_view_current.PerformClick()));
                                            }
                                            else
                                            {
                                                btn_view_current.PerformClick();
                                            }
                                            _itChanged = true;
                                            break;
                                        }
                                    }                                    
                                    if (!_itChanged)
                                    {
                                        logger.Trace($"DisplayProfileForm/WndProc: The taskbar registry key did not update within 5 seconds of a taskbar move message. Returning without doing anything.");
                                    }
                                    _monitorTaskBarRegKeysForChangesTask = null;
                                });
                                _monitorTaskBarRegKeysForChangesTask.Start();
                            }
                            break;
                    }
                    break;*/
            }

            base.WndProc(ref m);
        }

        private void ilv_saved_profiles_ItemHover(object sender, ItemHoverEventArgs e)
        {
            if (e.Item != null)
            {
                tt_selected.SetToolTip(ilv_saved_profiles, e.Item.Text);
            }
            else
            {
                tt_selected.RemoveAll();
            }
        }

        private Bitmap PickBitmapBasedOnBgColour(Color bgColour, Bitmap lightBitmap, Bitmap darkBitmap)
        {
            if ((bgColour.R * 0.299 + bgColour.G * 0.587 + bgColour.B * 0.114) > 186)
            {
                return darkBitmap;
            }
            else
            {
                return lightBitmap;
            }
        }

        private void btn_hotkey_Click(object sender, EventArgs e)
        {
            Keys testHotkey;
            if (_selectedProfile.Hotkey != Keys.None)
                testHotkey = _selectedProfile.Hotkey;
            else
                testHotkey = Keys.None;
            string hotkeyHeading = $"Choose a '{_selectedProfile.Name}' Display Profile Hotkey";
            string hotkeyDescription = $"Choose a Hotkey (a keyboard shortcut) so that you can apply this" + Environment.NewLine +
                "Display Profile using your keyboard. This must be a Hotkey that" + Environment.NewLine +
                "is unique across all your applications otherwise DisplayMagician" + Environment.NewLine +
                "might not see it.";
            HotkeyForm displayHotkeyForm = new HotkeyForm(testHotkey,hotkeyHeading, hotkeyDescription);
            //ilv_saved_shortcuts.SuspendLayout();
            //Program.HotkeyListener.SuspendOn(displayHotkeyForm);
            displayHotkeyForm.ShowDialog(this);
            if (displayHotkeyForm.DialogResult == DialogResult.OK)
            {
                // now we save the Hotkey
                _selectedProfile.Hotkey = displayHotkeyForm.Hotkey;
                // And cause this has changed within a Profile we need to save all the profiles
                ProfileRepository.SaveProfiles();
                // And if we get back and this is a Hotkey with a value, we need to show that in the UI
                UpdateHotkeyLabel(_selectedProfile.Hotkey);
                if (displayHotkeyForm.Hotkey == Keys.None)
                    // Remove the Hotkey if it needs to be removed
                    HotkeyManager.Current.Remove(_selectedProfile.UUID);
                else
                    // And then apply the Hotkey now
                    HotkeyManager.Current.AddOrReplace(_selectedProfile.UUID, _selectedProfile.Hotkey, OnWindowHotkeyPressed);
            }
        }
        private void lbl_hotkey_assigned_Click(object sender, EventArgs e)
        {
            btn_hotkey.PerformClick();
        }

        private void UpdateHotkeyLabel (Keys myHotkey)
        {
            // And if we get back and this is a Hotkey with a value, we need to show that in the UI
            if (myHotkey != Keys.None)
            {
                KeysConverter kc = new KeysConverter();

                lbl_hotkey_assigned.Text = "Hotkey: " + kc.ConvertToString(myHotkey);
                lbl_hotkey_assigned.Visible = true;
            }
            else 
            {
                lbl_hotkey_assigned.Text = "Hotkey: None";
                lbl_hotkey_assigned.Visible = false;
            }
            
        }

        public void OnWindowHotkeyPressed(object sender, HotkeyEventArgs e)
        {
            if (ProfileRepository.ContainsProfile(e.Name))
            {
                string displayProfileUUID = e.Name;
                ProfileItem chosenProfile = ProfileRepository.GetProfile(displayProfileUUID);
                if (chosenProfile is ProfileItem)
                    //ProfileRepository.ApplyProfile(chosenProfile);
                    Program.ApplyProfileTask(chosenProfile);
            }
            
        }

        private void btn_profile_settings_Click(object sender, EventArgs e)
        {
            ProfileSettingsForm profileSettingsForm = new ProfileSettingsForm();
            profileSettingsForm.Profile = _selectedProfile;
            profileSettingsForm.ShowDialog(this);
            // If the profile was previously saved and is now changed then save all the profiles
            // otherwise we'll save it only when the user wants to save this profile.
            if (_saveOrRenameMode == "rename" && profileSettingsForm.ProfileSettingChanged)
            {
                //_selectedProfile = profileSettingsForm.Profile;
                ProfileRepository.SaveProfiles();
            }                
        }

        private void btn_help_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/wiki/Initial-DisplayMagician-Setup";
            System.Diagnostics.Process.Start(targetURL);
        }

        private void saveProfileToDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btn_save.PerformClick();
        }

        private void applyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btn_apply.PerformClick();
        }

        private void deleteProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btn_delete.PerformClick();
        }

        private void sendToClipboardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string commandline = _selectedProfile.CreateCommand();
            Clipboard.SetText(commandline);
        }

        private void ilv_saved_profiles_ItemDoubleClick(object sender, ItemClickEventArgs e)
        {
            // This is the double click to apply
            _selectedProfile = ProfileRepository.GetProfile(e.Item.Text);            

            // Apply the selected profile
            btn_apply.PerformClick();
        }

        private void btn_donate_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/sponsors/terrymacdonald";
            System.Diagnostics.Process.Start(targetURL);
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            // Check there is a name
            if (String.IsNullOrWhiteSpace(txt_profile_save_name.Text))
            {
                logger.Warn($"DisplayProfileForm/btn_save_as_Click: You need to provide a name for this profile before it can be updated.");
                MessageBox.Show("You need to provide a name for this profile before it can be updated.", "Your profile needs a name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Check the name is valid
            if (!Program.IsValidFilename(txt_profile_save_name.Text))
            {
                logger.Warn($"DisplayProfileForm/btn_update_Click: The profile name cannot contain the following characters: {Path.GetInvalidFileNameChars()}. Unable to save this profile.");
                MessageBox.Show($"The profile name cannot contain the following characters: [{Path.GetInvalidFileNameChars()}]. Please change the profile name.", "Invalid characters in profile name", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            // If we're saving the current profile as a new item
            // then we'll be in "save" mode
            if (_saveOrRenameMode == "rename")
            { 
                // We're in 'rename' mode!
                // This also means we are going to have to get the latest current Profile and then overwrtite this data

                ProfileRepository.UserChangingProfiles = true;
                // Refresh the profiles to see whats valid
                ProfileRepository.IsPossibleRefresh();                
                // Replace the profile data with the current active profile data
                ProfileRepository.CopyCurrentLayoutToProfile(_selectedProfile); 
                // Refresh the Profile UI
                RefreshDisplayProfileUI();
                // Recenter the Window
                RecenterWindow();
                ProfileRepository.UserChangingProfiles = false;


            }
        }
    }
}