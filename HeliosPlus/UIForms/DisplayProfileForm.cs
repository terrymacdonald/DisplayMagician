using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeliosPlus.Resources;
using HeliosPlus.Shared;
using Manina.Windows.Forms;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace HeliosPlus.UIForms
{
    internal partial class DisplayProfileForm : Form
    {
        private ProfileItem _selectedProfile;
        //private List<ProfileItem> _savedProfiles = new List<ProfileItem>();
        private string _saveOrRenameMode = "save";
        //private static bool _inDialog = false;
        private static ProfileItem _profileToLoad = null;
        private ProfileAdaptor _profileAdaptor = new ProfileAdaptor();

        public DisplayProfileForm()
        {
            InitializeComponent();
            this.AcceptButton = this.btn_save_or_rename;
        }

        public DisplayProfileForm(ProfileItem profileToLoad) : this()
        {
            _profileToLoad = profileToLoad;
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            if (!(_selectedProfile is ProfileItem))
                return;

            if (!_selectedProfile.IsPossible)
            {
                MessageBox.Show(this, Language.This_profile_is_currently_impossible_to_apply,
                    Language.Apply_Profile,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            // Apply the Profile
            //ProfileRepository.ApplyProfile(_selectedProfile);
            Program.ApplyProfile(_selectedProfile);

        }


        private void Exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private void Delete_Click(object sender, EventArgs e)
        {
            if (!(_selectedProfile is ProfileItem))
                return;

            if (MessageBox.Show($"Are you sure you want to delete the '{_selectedProfile.Name}' Display Profile?", $"Delete '{_selectedProfile.Name}' Display Profile?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                return;

            // remove the profile from the imagelistview
            int currentIlvIndex = ilv_saved_profiles.SelectedItems[0].Index;
            ilv_saved_profiles.Items.RemoveAt(currentIlvIndex);

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
                    if (newSelectedProfile.Name.Equals(ilv_saved_profiles.Items[ilvItemToSelect].Text))
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

        }

        private void RefreshDisplayProfileUI()
        {

            ImageListViewItem newItem = null;

            // Temporarily stop updating the saved_profiles listview
            // To stop the display showing all sorts of changes happening
            ilv_saved_profiles.SuspendLayout();

            // Empty the imageListView
            ilv_saved_profiles.Items.Clear();

            // Fill it back up with the Profiles we have
            foreach (ProfileItem profile in ProfileRepository.AllProfiles.OrderBy(p=>p.Name))
            {
                // Create a new ImageListViewItem from the profile
                newItem = new ImageListViewItem(profile, profile.Name);

                // Select it if its the selectedProfile
                if (_selectedProfile is ProfileItem && _selectedProfile.Equals(profile))
                    newItem.Selected = true;

                // Add it to the list!
                ilv_saved_profiles.Items.Add(newItem, _profileAdaptor);

            }

            // Restart updating the saved_profiles listview
            ilv_saved_profiles.ResumeLayout();
        }


        private void DisplayProfileForm_Activated(object sender, EventArgs e)
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
        }

        private void DisplayProfileForm_Load(object sender, EventArgs e)
        {
            
            // Update the Current Profile
            ProfileRepository.UpdateActiveProfile();

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

            // And update the save/rename textbox
            txt_profile_save_name.Text = _selectedProfile.Name;

            if (ProfileRepository.ContainsProfile(profile))
            {
                // we already have the profile stored
                _saveOrRenameMode = "rename";
                btn_save_or_rename.Text = "Rename To";
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
                        lbl_profile_shown_subtitle.Text = "This is the Display Profile currently in use.";
                    }
                    else
                    {
                        btn_apply.Visible = true;
                        lbl_profile_shown_subtitle.Text = "";
                    }
                }
            }
            else
            {
                // we don't have the profile stored yet
                _saveOrRenameMode = "save";
                btn_save_or_rename.Text = "Save As";
                lbl_profile_shown_subtitle.Text = "The current Display configuration hasn't been saved as a Display Profile yet.";
                btn_apply.Visible = false;
            }

            // Refresh the image list view
            //RefreshImageListView(profile);

            // And finally show the profile in the display view
            dv_profile.Profile = profile;
            dv_profile.Refresh();

        }

        private void btn_save_as_Click(object sender, EventArgs e)
        {

            // Check the name is valid
            if (!Program.IsValidFilename(txt_profile_save_name.Text))
            {
                MessageBox.Show("The profile name cannot contain the following characters:" + Path.GetInvalidFileNameChars(), "Invalid characters in profile name", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            // Check we're not already using the name
            foreach (ProfileItem savedProfile in ProfileRepository.AllProfiles)
            {
                //if (String.Equals(txt_profile_save_name.Text, savedProfile.Name, StringComparison.InvariantCultureIgnoreCase))
                if (savedProfile.Name.Equals(txt_profile_save_name.Text))
                {
                    MessageBox.Show("Sorry, each saved display profile needs a unique name.", "Profile name already exists", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // If we're saving the current profile as a new item
            // then we'll be in "save" mode
            if (_saveOrRenameMode == "save")
            {
                // We're in 'save' mode!

                // Check we're not already saving this profile
                foreach (ProfileItem savedProfile in ProfileRepository.AllProfiles)
                {
                    //if (String.Equals(txt_profile_save_name.Text, savedProfile.Name, StringComparison.InvariantCultureIgnoreCase))
                    if (savedProfile.Equals(_selectedProfile))
                    {
                        MessageBox.Show($"Sorry, this display profile was already saved as '{savedProfile.Name}'.", "Profile already saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }

                // So we've already passed the check that says this profile is unique

                // Update the name just to make sure we record it if the user changed it
                _selectedProfile.Name = txt_profile_save_name.Text;

                // Add the current profile to the list of profiles so it gets saved
                ProfileRepository.AddProfile(_selectedProfile);

                // Also update the imagelistview so that we can see the new profile we just saved

                // Load the currentProfile image into the imagelistview
                //ImageListViewItem newItem = new ImageListViewItem(_selectedProfile.SavedProfileCacheFilename, _selectedProfile.Name);
                ImageListViewItem newItem = new ImageListViewItem(_selectedProfile, _selectedProfile.Name);
                newItem.Selected = true;
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

                // Lets rename the entry in the imagelistview to the new name
                foreach (ImageListViewItem myItem in ilv_saved_profiles.Items)
                {
                    if (myItem.Text == oldProfileName)
                    {
                        myItem.Text = txt_profile_save_name.Text;
                    }
                }
                // Lets rename the selectedProfile to the new name
                ProfileRepository.RenameProfile(_selectedProfile, txt_profile_save_name.Text);
                
                // Lets update the rest of the profile screen too
                lbl_profile_shown.Text = txt_profile_save_name.Text;

                // And we also need to go through the Shortcuts in the library and rename them!
                ShortcutRepository.RenameShortcutProfile(_selectedProfile);
                

            }

            ChangeSelectedProfile(_selectedProfile);

            // now update the profiles image listview
            //ilv_saved_profiles.Refresh();

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
            
        }

        private void btn_view_current_Click(object sender, EventArgs e)
        {
            // Reload the profiles in case we swapped to another program to change it
            //ProfileRepository.UpdateCurrentProfile();
            ProfileRepository.GetActiveProfile();
            // Change to the current selected Profile
            ChangeSelectedProfile(ProfileRepository.GetActiveProfile());
            // Refresh the Profile UI
            RefreshDisplayProfileUI();
        }

        private void txt_profile_save_name_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.Equals(Keys.Enter))
            {
                MessageBox.Show("Click works!", "Click works", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DISPLAYCHANGE = 0x007E;

            const int x_bitshift = 0;
            const int y_bitshift = 16;
            const int xy_mask = 0xFFFF;

            bool displayChange = false;

            switch (m.Msg)
            {
                case WM_DISPLAYCHANGE:
                    ProfileRepository.UpdateActiveProfile();
                    break;
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

        /*private static void MainWindow_Closed(object sender, EventArgs e)
        {
            DeviceNotification.UnRegisterUsbDeviceNotification();
            DeviceNotification.UnRegisterMonitorDeviceNotification();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            if (!(PresentationSource.FromVisual(this) is HwndSource source))
                return;

            source.AddHook(this.WndProc);

            DeviceNotification.RegisterUsbDeviceNotification(source.Handle);
            DeviceNotification.RegisterMonitorDeviceNotification(source.Handle);
        }*/

        // Notification registeration for display detection from here http://codetips.nl/detectmonitor.html

        /*protected override void WndProc(ref Message m)
        {
            const int WM_SETTINGCHANGE = 0x001A;
            const int SPI_SETWORKAREA = 0x02F;
            const int WM_DISPLAYCHANGE = 0x007E;
            const int WM_DEVICECHANGE = 0x0219; // WM Device change message ID
            const int DBT_DEVICEARRIVAL = 0x8000; // WM Device Change Event: System detected a new device        
            const int DBT_DEVICEREMOVECOMPLETE = 0x8004; // WM Device Change Event: Device is gone      


            const int x_bitshift = 0;
            const int y_bitshift = 16;
            const int xy_mask = 0xFFFF;

            bool displayChange = false;

            switch (m.Msg)
            {
                case WM_DISPLAYCHANGE:
                case DeviceNotification.DbtDeviceRemoveComplete:
                case DeviceNotification.DbtDeviceArrival:
                    {
                        if (DeviceNotification.IsMonitor(lParam))
                            Debug.WriteLine($"Monitor {((int)wParam == DeviceNotification.DbtDeviceArrival ? "arrived" : "removed")}");

                        if (DeviceNotification.IsUsbDevice(lParam))
                            Debug.WriteLine($"Usb device {((int)wParam == DeviceNotification.DbtDeviceArrival ? "arrived" : "removed")}");
                    }
                    break;
            }

            base.WndProc(ref m);
        }*/

    }
}