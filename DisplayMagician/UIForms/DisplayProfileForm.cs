using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
//using DisplayMagician.Resources;
using DisplayMagicianShared;
using DisplayMagicianShared.UserControls;
using DisplayMagicianShared.Windows;
using Manina.Windows.Forms;
using System.Drawing;
//using NHotkey.WindowsForms;
//using NHotkey;
using System.Threading.Tasks;
using System.Collections.Generic;
using DisplayMagician.Processes;
using static DisplayMagician.Program;

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


        private List<HotkeyKeyboard> _shownKeyboardHotkeys = new();
        private List<HotkeyJoystick> _shownJoystickHotkeys = new();

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public DisplayProfileForm()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
            this.AcceptButton = this.btn_save_or_rename;
            ilv_saved_profiles.MultiSelect = false;
            //ilv_saved_profiles.ThumbnailSize = new Size(ilv_saved_profiles.Height, ilv_saved_profiles.Height);
            ilv_saved_profiles.AllowDrag = false;
            ilv_saved_profiles.AllowDrop = false;
            ilv_saved_profiles.SetRenderer(new ProfileILVRenderer());
            // Center the form on the primary screen
            //Utils.CenterOnPrimaryScreen(this);
        }

        public DisplayProfileForm(ProfileItem profileToLoad) : this()
        {
            _profileToLoad = profileToLoad;
        }

        protected override void OnLoad(EventArgs e)
        {
            Utils.LoadFormState(this);
            base.OnLoad(e);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            Utils.SaveFormState(this);
            base.OnFormClosing(e);
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            if (_selectedProfile == null)
                return;

            if (!_selectedProfile.IsValid())
            {
                MessageBox.Show(this, "This display profile contains errors and cannot be applied. Please update or recreate the profile.",
                    "Apply Profile",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            // Stop the user from applying this profile if one is already being applied
            if (ProfileRepository.UserChangingProfiles)
            {
                logger.Error($"DisplayProfileForm/Apply_Click: The User is currently changing to another Display Profile. We can't change to another Display Profile right now. Please wait.");
                MessageBox.Show("The User is currently changing to another Display Profile. We can't change to another Display Profile right now. Please wait.", "User changing profiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

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
            //RecenterWindow();

        }

        /* private void RecenterWindow()
         {
             if (Program.AppMainForm is Form)
             {
                 // Center the MainAppForm
                 Utils.CenterOnPrimaryScreen(Program.AppMainForm);
                 // Also refresh the right-click menu (if we have a main form loaded)
                 Program.AppMainForm.RefreshNotifyIconMenus();
                 // We update the Game Shortcut context menu is always updated and correct.
                 if (Program.AppProgramSettings.InstallDesktopContextMenu)
                 {
                     ContextMenu.UpdateShortcutContextMenu();
                 }

             }

             // Bring the window back to the front
             Utils.ActivateCenteredOnPrimaryScreen(this);

         }*/


        private void Exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private void Delete_Click(object sender, EventArgs e)
        {
            if (_selectedProfile == null)
                return;

            if (MessageBox.Show($"Are you sure you want to delete the '{_selectedProfile.Name}' Display Profile? This cannot be undone.", $"Delete '{_selectedProfile.Name}' Display Profile?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                return;

            // remove the profile from the imagelistview
            int currentIlvIndex = ilv_saved_profiles.SelectedItems[0].Index;
            ilv_saved_profiles.Items.RemoveAt(currentIlvIndex);

            // Remove the hotkey if it is enabled for this profile
            /*if (_selectedProfile.Hotkey != Keys.None)
            {
                // Remove the Hotkey if it needs to be removed
                HotkeyManager.Current.Remove(_selectedProfile.UUID);
            }*/

            // Remove the hotkey if there is one
            Program.AppDirectInputManager.RemoveHotkeysByUUID(_selectedProfile.UUID);

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
            // We update the Game Shortcut context menu is always updated and correct.
            if (Program.AppProgramSettings.InstallDesktopContextMenu)
            {
                DisplayMagician.ContextMenu.UpdateShortcutContextMenu();
            }

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
                                String.Format("Shortcut successfully saved to '{0}'", dialog_save.FileName),
                                "Shortcut",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to create the shortcut. Unexpected exception occurred.",
                                "Shortcut",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                        }

                        dialog_save.FileName = string.Empty;
                        //DialogResult = DialogResult.OK;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

            // If the user is changing profiles right now, then we need to wait until the profile change has finished
            // We need a 30 second timeout in there too, just in case the user is changing profiles and it's taking a long time
            if (ProfileRepository.UserChangingProfiles)
            {
                logger.Error($"DisplayProfileForm/DisplayProfileForm_Load: Waiting for the User to finish changing profiles before we can load the Display Profile window.");
                int timeout = 30;
                while (ProfileRepository.UserChangingProfiles && timeout > 0)
                {
                    System.Threading.Thread.Sleep(1000);
                    timeout--;
                }
                if (timeout == 0)
                {
                    logger.Error($"DisplayProfileForm/DisplayProfileForm_Load: The User is still changing profiles after 30 seconds. We can't load the Display Profile window until they're finished.");
                    MessageBox.Show("The User is still changing profiles after 30 seconds. We can't load the Display Profile window until they're finished.", "Display Profile Window Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

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

            // Start the donation animation if it's time to do so
            if (Utils.TimeToRunDonationAnimation())
            {
                Utils.AddAnimation(btn_donate);
            }

            UpdateHotkeyText();
        }


        private void ChangeSelectedProfile(ProfileItem profile)
        {
            // And we need to update the actual selected profile too!
            _selectedProfile = profile;

            // We also need to load the saved profile name to show the user
            lbl_profile_shown.Text = _selectedProfile.Name;

            /*// And show the logo for the driver
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
            }*/

            // And update the save/rename textbox
            txt_profile_save_name.Text = _selectedProfile.Name;

            bool profileHasErrors = !_selectedProfile.IsValid();
            List<string> undetectedDisplays = profileHasErrors
                ? new List<string>()
                : _selectedProfile.UndetectedDisplayIdentifiers;

            p_profile_advisory.Visible = ProfileRepository.ContainsProfile(profile) && (profileHasErrors || undetectedDisplays.Count > 0);
            if (profileHasErrors)
            {
                p_profile_advisory.BackColor = Color.Firebrick;
                lbl_profile_advisory.ForeColor = Color.White;
                lbl_profile_advisory.Text = "This display profile contains errors and cannot be applied. Please update or recreate the profile.";
            }
            else if (undetectedDisplays.Count > 0)
            {
                p_profile_advisory.BackColor = Color.FromArgb(255, 193, 7);
                lbl_profile_advisory.ForeColor = Color.Black;
                lbl_profile_advisory.Text = $"⚠ Your display profile may not apply as expected.{Environment.NewLine}DisplayMagician could not detect the following: {String.Join(", ", undetectedDisplays)}. This may be normal when using switchers, alternate inputs, or screens that are powered off. You may still apply the profile but it may not apply as expected.";
            }

            if (ProfileRepository.ContainsProfile(profile))
            {
                // we already have the profile stored
                _saveOrRenameMode = "rename";
                btn_save_or_rename.Text = "Rename To";
                lbl_save_profile.Visible = false;
                btn_update.Visible = true;
                if (profileHasErrors)
                {
                    lbl_profile_shown_subtitle.Text = "This Display Profile contains errors.";
                    lbl_profile_shown_subtitle.Visible = true;
                    btn_apply.Visible = false;
                    cms_profiles.Items[0].Enabled = false;
                }
                else
                {
                    if (ProfileRepository.IsActiveProfile(_selectedProfile))
                    {
                        btn_apply.Visible = false;
                        lbl_profile_shown_subtitle.Text = "This is the Display Profile currently in use.";
                        lbl_profile_shown_subtitle.Visible = true;
                        cms_profiles.Items[0].Enabled = false;
                    }
                    else
                    {
                        btn_apply.Visible = true;
                        lbl_profile_shown_subtitle.Text = "";
                        lbl_profile_shown_subtitle.Visible = false;
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
                lbl_profile_shown_subtitle.Visible = true;
                btn_apply.Visible = false;
                btn_update.Visible = false;
                lbl_save_profile.Visible = true;
            }

            // Update the Hotkey Label text
            UpdateHotkeyText();

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
            // Stop the user from saving this profile if one is already being applied
            if (ProfileRepository.UserChangingProfiles)
            {
                logger.Error($"DisplayProfileForm/btn_save_as_Click: The User is currently changing profiles. We can't save this profile until they're finished.");
                MessageBox.Show("The User is currently changing profiles. We can't save this profile until they're finished.", "User changing profiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


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
                if (savedProfile.Name.Equals(txt_profile_save_name.Text, StringComparison.OrdinalIgnoreCase))
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
                    logger.Warn(ex, $"DisplayProfileForm/btn_save_as_Click: Exception whilst trying to save the display layout. We won't be able to save this profile. Please log a new issue at https://github.com/terrymacdonald/DisplayMagician/issues/new/choose");
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
            // We update the Game Shortcut context menu is always updated and correct.
            if (Program.AppProgramSettings.InstallDesktopContextMenu)
            {
                DisplayMagician.ContextMenu.UpdateShortcutContextMenu();
            }


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
            if (ProfileRepository.UserChangingProfiles)
            {
                logger.Error($"DisplayProfileForm/btn_view_current_Click: The User is currently changing profiles. We can't view the current display layout until they're finished.");
                MessageBox.Show("The User is currently changing profiles. We can't view the current display layout until they're finished.", "User changing profiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Refresh the profiles to see whats valid
            ProfileRepository.IsPossibleRefresh();
            // Reload the profiles in case we swapped to another program to change it
            ProfileRepository.UpdateActiveProfile(false);
            // Change to the current selected Profile
            ChangeSelectedProfile(ProfileRepository.GetActiveProfile());
            // Refresh the Profile UI
            RefreshDisplayProfileUI();
            // Recenter the Window
            //RecenterWindow();
        }

        public void RefreshCurrentView()
        {
            btn_view_current.PerformClick();
        }

        private void txt_profile_save_name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                //MessageBox.Show("Click works!", "Click works", MessageBoxButtons.OK, MessageBoxIcon.Information);
                btn_save_or_rename.PerformClick();
            }
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
            // Find the matching hotkeys so that we can load them in
            // and then show the hotkey form
            /*List<HotkeyKeyboard> _keyboardHotkeys = new List<HotkeyKeyboard>();
            _keyboardHotkeys.AddRange(Program.AppDirectInputManager.GetKeyboardHotkeysByUUID(_selectedProfile.UUID));
            List<HotkeyJoystick> _joystickHotkeys = new List<HotkeyJoystick>();
            _joystickHotkeys.AddRange(Program.AppDirectInputManager.GetJoystickHotkeysByUUID(_selectedProfile.UUID));*/

            string hotkeyHeading = $"Manage your '{_selectedProfile.Name}' Display Profile Hotkeys";
            string hotkeyDescription = $"Choose one or more Hotkeys so that you can apply this Display Profile using your keyboard, joystick or button box. " +
                "This must be a Hotkey that is unique across all your applications otherwise DisplayMagician might not see it. " +
                "Click Add to add it to the list or click the trashcan to remove it from the list. To see all your hotkeys " +
                "go to the Main Window and click the Settings button. ";
            HotkeyForm displayHotkeyForm = new HotkeyForm(HotkeyTask.ChangeDisplayProfile, _selectedProfile.UUID, hotkeyHeading, hotkeyDescription);
            //ilv_saved_shortcuts.SuspendLayout();
            //Program.HotkeyListener.SuspendOn(displayHotkeyForm);
            displayHotkeyForm.ShowDialog(this);
            if (displayHotkeyForm.Changed)
            {
                UpdateHotkeyText();

            }
        }
        private void lbl_hotkey_assigned_Click(object sender, EventArgs e)
        {
            btn_hotkey.PerformClick();
        }

        private void UpdateHotkeyText()
        {

            try
            {
                _shownKeyboardHotkeys = Program.AppProgramSettings.KeyboardHotkeys.Where(k => k.Task == HotkeyTask.ChangeDisplayProfile && k.UUID == _selectedProfile.UUID).ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"DisplayProfileForm/UpdateHotkeyText: Exception attempting to get the keyboard hotkeys from the settings file that match this taskmode ChangeDisplayProfile and UUID {_selectedProfile.UUID}.");
            }
            try
            {
                _shownJoystickHotkeys = Program.AppProgramSettings.JoystickHotkeys.Where(k => k.Task == HotkeyTask.ChangeDisplayProfile && k.UUID == _selectedProfile.UUID).ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"DisplayProfileForm/UpdateHotkeyText: Exception attempting to get the joystick hotkeys from the settings file that match this taskmode ChangeDisplayProfile and UUID {_selectedProfile.UUID}.");
            }

            // We want the keyboard hotkeys to win if both are provided. Joystick and keyboard hotkeys do not mix and cannot be used together.
            List<string> hotkeyList = new List<string>();
            if (_shownKeyboardHotkeys.Count > 0)
            {
                foreach (HotkeyKeyboard kb in _shownKeyboardHotkeys)
                {
                    hotkeyList.Add(Program.AppDirectInputManager.GetNameOfKeyboardHotkey(kb));
                }
            }
            else if (_shownJoystickHotkeys.Count > 0)
            {
                foreach (HotkeyJoystick kb in _shownJoystickHotkeys)
                {
                    hotkeyList.Add(Program.AppDirectInputManager.GetNameOfJoystickHotkey(kb));
                }
            }
            string hotkeyText = string.Join(", ", hotkeyList);
            if (hotkeyList.Count > 0)
            {
                if (lbl_hotkey_assigned.InvokeRequired)
                {
                    lbl_hotkey_assigned.Invoke(new Action(() =>
                    {
                        lbl_hotkey_assigned.Text = $"Hotkeys: {hotkeyText}";
                        lbl_hotkey_assigned.Visible = true;
                    }));
                }
                else
                {
                    lbl_hotkey_assigned.Text = $"Hotkeys: {hotkeyText}";
                    lbl_hotkey_assigned.Visible = true;
                }
            }
            else
            {
                if (lbl_hotkey_assigned.InvokeRequired)
                {
                    lbl_hotkey_assigned.Invoke(new Action(() =>
                    {
                        lbl_hotkey_assigned.Text = "Hotkeys: None";
                        lbl_hotkey_assigned.Visible = false;
                    }));
                }
                else
                {
                    lbl_hotkey_assigned.Text = "Hotkeys: None";
                    lbl_hotkey_assigned.Visible = false;
                }

            }

        }

        private void btn_profile_settings_Click(object sender, EventArgs e)
        {
            ProfileSettingsForm profileSettingsForm = new ProfileSettingsForm();
            profileSettingsForm.Profile = _selectedProfile;
            profileSettingsForm.ShowDialog(this);
            // Refresh the DisplayView so it reflects the updated wallpaper mode immediately
            dv_profile.Profile = _selectedProfile;
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
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
        }

        private void saveProfileToDesktopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save_Click(sender, e);
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
            string targetURL = "https://github.com/sponsors/terrymacdonald?frequency=one-time";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
            // Update the settings to say that user has donated.
            Utils.UserHasDonated();
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            if (ProfileRepository.UserChangingProfiles)
            {
                logger.Error($"DisplayProfileForm/btn_update_Click: The User is currently changing profiles. We can't update the Display Profile settings until they're finished.");
                MessageBox.Show("The User is currently changing profiles. We can't update the Display Profile settings until they're finished.", "User changing profiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // check if the user really wants to update
            if (MessageBox.Show($"Do you really want to overwrite the display settings in the '{_selectedProfile.Name}' Display Profile with the display settings currently in use? This cannot be undone.", "Update Display Profile settings?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                // Check there is a name
                if (String.IsNullOrWhiteSpace(txt_profile_save_name.Text))
                {
                    logger.Warn($"DisplayProfileForm/btn_update_Click: You need to provide a name for this profile before it can be updated.");
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

                    // Replace the profile data with the current active profile data
                    ProfileRepository.CopyCurrentLayoutToProfile(_selectedProfile);

                    // Save the Profiles JSON as it's different now
                    ProfileRepository.SaveProfiles();

                    // Refresh the profiles to see whats valid
                    ProfileRepository.IsPossibleRefresh();

                    // Update the active profile so the UI knows which profile is currently in use
                    ProfileRepository.UpdateActiveProfile(false);

                    // Refresh the Profile UI
                    RefreshDisplayProfileUI();
                    // Recenter the Window
                    //RecenterWindow();

                    logger.Trace($"DisplayProfileForm/btn_update_Click: Changing the selected profile in the imagelistview to Profile {_selectedProfile.Name}.");
                    ChangeSelectedProfile(_selectedProfile);

                    SharedLogger.logger.Debug($"DisplayProfileForm/btn_update_Click: The profile {_selectedProfile.Name} was successfully updated with the latest display settings");
                    MessageBox.Show($"The profile {_selectedProfile.Name} was successfully updated with the latest display settings.", "Profile updated", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // And finally refresh the profile in the display view
                    dv_profile.Profile = _selectedProfile;
                    dv_profile.Refresh();

                    // Disable the Apply button as the curretn settings should be the same as now
                    btn_apply.Visible = false;
                }
            }
        }

    }
}
