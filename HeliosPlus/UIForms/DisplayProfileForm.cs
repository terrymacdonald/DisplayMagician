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

namespace HeliosPlus.UIForms
{
    internal partial class DisplayProfileForm : Form
    {
        private ProfileItem _selectedProfile;
        //private List<ProfileItem> _savedProfiles = new List<ProfileItem>();
        private string _saveOrRenameMode = "save";
        //private static bool _inDialog = false;
        private static ProfileItem _profileToLoad = null;
        private ProfileAdaptor _profileAdaptor;
        private ProfileRepository _profileRepository;

        public DisplayProfileForm()
        {
            InitializeComponent();
            this.AcceptButton = this.btn_save_or_rename;
            _profileAdaptor = new ProfileAdaptor();
            _profileRepository = new ProfileRepository();
        }

        public DisplayProfileForm(ProfileItem profileToLoad) : this()
        {
            _profileToLoad = profileToLoad;
        }

        private void Apply_Click(object sender, EventArgs e)
        {
            if (!_selectedProfile.IsPossible)
            {
                MessageBox.Show(this, Language.This_profile_is_currently_impossible_to_apply,
                    Language.Apply_Profile,
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            IDictionary<string, Action> actions = dv_profile.Profile.applyProfileActions();
            IDictionary<string, string> messages = dv_profile.Profile.applyProfileMsgs();
            List<string> sequence = dv_profile.Profile.applyProfileSequence();

            if (new ApplyingChangesForm(
                () =>   {
                            Task.Factory.StartNew(() =>
                                {
                                    System.Threading.Thread.Sleep(2000);
                                    actions[sequence[0]]();
                                }, TaskCreationOptions.LongRunning);
                        }, 3, 30, 5, messages[sequence[0]]
                ).ShowDialog(this) != DialogResult.Cancel)
            {
                for (int i = 1; i < sequence.Count; i++)
                {
                    new ApplyingChangesForm(
                    () =>
                    {
                        Task.Factory.StartNew(() => actions[sequence[i]](), TaskCreationOptions.LongRunning);
                    }, 0, 30, 5, messages[sequence[i]]).ShowDialog(this);
                }
                // nothing to do
                Console.WriteLine("Applying profile " + _selectedProfile.Name);
            }

            Activate();
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private void Delete_Click(object sender, EventArgs e)
        {
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

            // Temporarily stop updating the saved_profiles listview
            ilv_saved_profiles.SuspendLayout();

            if (ProfileRepository.ProfileCount > 0)
            {
                ImageListViewItem newItem = null; 
                bool foundCurrentProfileInLoadedProfiles = false;
                foreach (ProfileItem loadedProfile in ProfileRepository.AllProfiles)
                {
                    bool thisLoadedProfileIsAlreadyHere = (from item in ilv_saved_profiles.Items where item.Text == loadedProfile.Name select item.Text).Any();
                    if (!thisLoadedProfileIsAlreadyHere)
                    {
                        //loadedProfile.SaveProfileImageToCache();
                        //newItem = new ImageListViewItem(loadedProfile.SavedProfileCacheFilename, loadedProfile.Name);
                        //newItem = new ImageListViewItem(loadedProfile, loadedProfile.Name);
                        newItem = new ImageListViewItem(loadedProfile, loadedProfile.Name);
                        //ilv_saved_profiles.Items.Add(newItem);
                        ilv_saved_profiles.Items.Add(newItem, _profileAdaptor);
                    }

                    if (ProfileRepository.CurrentProfile.Equals(loadedProfile))
                    {
                        // We have already saved the selected profile!
                        // so we need to show the selected profile 
                        ChangeSelectedProfile(loadedProfile);
                        foundCurrentProfileInLoadedProfiles = true;
                    }
                }

                // If we get to the end of the loaded profiles and haven't
                // found a matching profile, then we need to show the current
                // Profile
                if (!foundCurrentProfileInLoadedProfiles)
                    ChangeSelectedProfile(ProfileRepository.CurrentProfile);

                // Check if we were loading a profile to edit
                // If so, select that instead of all that other stuff above!
                if (_profileToLoad != null)
                    ChangeSelectedProfile(_profileToLoad);

            }
            else
            {
                // If there are no profiles at all then we are starting from scratch!
                // Show the profile in the DV window
                // Use the current profile name in the label and the save name
                ChangeSelectedProfile(ProfileRepository.CurrentProfile);
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
            ProfileRepository.UpdateCurrentProfile();
            // Refresh the Profile UI
            RefreshDisplayProfileUI();
        }

        private void DisplayProfileForm_Load(object sender, EventArgs e)
        {
            // Load all the profiles to prepare things
            //_savedProfiles = ProfileRepository.AllProfiles;
            // Update the Current Profile
            ProfileRepository.UpdateCurrentProfile();
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

            if (ProfileRepository.AllProfiles.Contains(_selectedProfile))
            {
                // we already have the profile stored
                _saveOrRenameMode = "rename";
                btn_save_or_rename.Text = "Rename To";
                if (!_selectedProfile.IsPossible)
                {
                    lbl_profile_shown_subtitle.Text = "(Display Profile is not valid so cannot be used)";
                    btn_apply.Visible = false;
                }
                else
                {
                    lbl_profile_shown_subtitle.Text = "";
                    btn_apply.Visible = true;
                }
            }
            else
            {
                // we don't have the profile stored yet
                _saveOrRenameMode = "save";
                btn_save_or_rename.Text = "Save As";
                lbl_profile_shown_subtitle.Text = "(Current Display Profile in use - UNSAVED)";
                btn_apply.Visible = false;
            }


            /*if (ProfileRepository.CurrentProfile.Equals(_selectedProfile))
            {
                if (ProfileRepository.AllProfiles.Contains(_selectedProfile))
                {
                    _saveOrRenameMode = "rename";
                    btn_save_or_rename.Text = "Rename To";
                    lbl_profile_shown_subtitle.Text = "(Current Display Profile in use)";
                }
                else
                {
                    _saveOrRenameMode = "save";
                    btn_save_or_rename.Text = "Save As";
                    lbl_profile_shown_subtitle.Text = "(Current Display Profile in use - UNSAVED)";
                }
                btn_apply.Visible = false;
            }
            else
            {
                _saveOrRenameMode = "rename";
                btn_save_or_rename.Text = "Rename To";
                if (!_selectedProfile.IsPossible)
                {
                    lbl_profile_shown_subtitle.Text = "(Display Profile is not valid so cannot be used)";
                    btn_apply.Visible = false;
                }
                else
                {
                    lbl_profile_shown_subtitle.Text = "";
                    btn_apply.Visible = true;
                }
            }*/
            // Refresh the image list view
            RefreshImageListView(profile);

            // And finally show the profile in the display view
            dv_profile.Profile = profile;
            dv_profile.Refresh();

        }

        private void RefreshImageListView(ProfileItem profile)
        {
            ilv_saved_profiles.ClearSelection();
            IEnumerable<ImageListViewItem> matchingImageListViewItems = (from item in ilv_saved_profiles.Items where item.Text == profile.Name select item);
            if (matchingImageListViewItems.Any())
            {
                matchingImageListViewItems.First().Selected = true;
                matchingImageListViewItems.First().Focused = true;
            }

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
            ProfileRepository.UpdateCurrentProfile();
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
    }
}