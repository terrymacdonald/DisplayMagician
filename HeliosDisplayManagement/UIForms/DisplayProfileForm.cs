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
        private Profile _selectedProfile;
        private List<Profile> _savedProfiles = new List<Profile>();
        private string _saveOrRenameMode = "save";
        private static bool _inDialog = false;
        private static Profile _profileToLoad = null;

        public DisplayProfileForm()
        {
            InitializeComponent();
            this.AcceptButton = this.btn_save_or_rename;
        }

        public DisplayProfileForm(Profile profileToLoad)
        {
            InitializeComponent();
            this.AcceptButton = this.btn_save_or_rename;
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
            this.Close();
        }


        private void Delete_Click(object sender, EventArgs e)
        {
            _inDialog = true;
            if (MessageBox.Show($"Are you sure you want to delete the '{_selectedProfile.Name}' Display Profile?", $"Delete '{_selectedProfile.Name}' Display Profile?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                return;

            Profile profileToDelete = _selectedProfile;
            string profileToDeleteFilename = _selectedProfile.SavedProfileCacheFilename;

            // remove the profile from the imagelistview
            int currentIlvIndex = ilv_saved_profiles.SelectedItems[0].Index;
            ilv_saved_profiles.Items.RemoveAt(currentIlvIndex);

            // remove the profile from the saved profiles list
            _savedProfiles.Remove(profileToDelete);

            // delete the profile image used in the image listview
            // we'll delete the old PNG that we no longer need
            try
            {
                File.Delete(profileToDeleteFilename);
            }
            catch (Exception ex)
            {
                // TODO write error to console
                // TODO specify the correct the exceptions 
            }

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
                foreach (Profile newSelectedProfile in _savedProfiles)
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
                ChangeSelectedProfile(Profile.CurrentProfile);

            }

            // Then save the profiles so we always have it updated
            // Generating the imagelistview images automatically as we save.
            Profile.SaveAllProfiles(_savedProfiles);
        }

        private void RefreshDisplayProfileUI()
        {

            if (!_inDialog)
            {
                // Temporarily stop updating the saved_profiles listview
                ilv_saved_profiles.SuspendLayout();

                if (_savedProfiles.Count > 0)
                {
                    ImageListViewItem newItem = null; 
                    bool foundCurrentProfileInLoadedProfiles = false;
                    foreach (Profile loadedProfile in _savedProfiles)
                    {
                        bool thisLoadedProfileIsAlreadyHere = (from item in ilv_saved_profiles.Items where item.Text == loadedProfile.Name select item.Text).Any();
                        if (!thisLoadedProfileIsAlreadyHere)
                        {
                            loadedProfile.SaveProfileImageToCache();
                            newItem = new ImageListViewItem(loadedProfile.SavedProfileCacheFilename, loadedProfile.Name);
                            ilv_saved_profiles.Items.Add(newItem);
                        }

                        if (Profile.CurrentProfile.Equals(loadedProfile))
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
                        ChangeSelectedProfile(Profile.CurrentProfile);

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
                    ChangeSelectedProfile(Profile.CurrentProfile);
                }

                // Restart updating the saved_profiles listview
                ilv_saved_profiles.ResumeLayout();
            }
            else
                // Otherwise turn off the dialog mode we were just in
                _inDialog = false;
        }


        private void DisplayProfileForm_Activated(object sender, EventArgs e)
        {
            // We handle the UI updating in DisplayProfileForm_Activated so that
            // the app will check for changes to the current profile when the
            // user clicks back to this app. This is designed to allow people to
            // alter their Windows Display settings then come back to our app
            // and the app will automatically recognise that things have changed.

            // Reload the profiles in case we swapped to another program to change it
            Profile.UpdateCurrentProfile();
            // Refresh the Profile UI
            RefreshDisplayProfileUI();
        }

        private void DisplayProfileForm_Load(object sender, EventArgs e)
        {
            // Load all the profiles to prepare things
            _savedProfiles = (List<Profile>)Profile.LoadAllProfiles();
            // Update the Current Profile
            Profile.UpdateCurrentProfile();
            // Refresh the Profile UI
            RefreshDisplayProfileUI();
        }


        private void ChangeSelectedProfile(Profile profile)
        {

            // And we need to update the actual selected profile too!
            _selectedProfile = profile;

            // We also need to load the saved profile name to show the user
            lbl_profile_shown.Text = _selectedProfile.Name;

            // And update the save/rename textbox
            txt_profile_save_name.Text = _selectedProfile.Name;

            if (_selectedProfile.Equals(Profile.CurrentProfile))
            {
                if (_savedProfiles.Contains(_selectedProfile))
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
            }
            // Refresh the image list view
            RefreshImageListView(profile);

            // And finally show the profile in the display view
            dv_profile.Profile = profile;
            dv_profile.Refresh();

        }

        private void RefreshImageListView(Profile profile)
        {
            ilv_saved_profiles.ClearSelection();
            IEnumerable<ImageListViewItem> matchingImageListViewItems = (from item in ilv_saved_profiles.Items where item.Text == profile.Name select item);
            if (matchingImageListViewItems.Any())
            {
                matchingImageListViewItems.First().Selected = true;
                matchingImageListViewItems.First().Focused = true;
            }

        }

        private bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }

        private void btn_save_as_Click(object sender, EventArgs e)
        {

            // Check the name is valid
            if (!IsValidFilename(txt_profile_save_name.Text))
            {
                MessageBox.Show("The profile name cannot contain the following characters:" + Path.GetInvalidFileNameChars(), "Invalid characters in profile name", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            // Check we're not already using the name
            foreach (Profile savedProfile in Profile.AllSavedProfiles)
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
                foreach (Profile savedProfile in Profile.AllSavedProfiles)
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

                // Add the current profile to the list of profiles so it gets saved later
                _savedProfiles.Add(_selectedProfile);

                // Also update the imagelistview so that we can see the new profile we just saved
                _selectedProfile.SaveProfileImageToCache();

                // Load the currentProfile image into the imagelistview
                ImageListViewItem newItem = new ImageListViewItem(_selectedProfile.SavedProfileCacheFilename, _selectedProfile.Name);
                newItem.Selected = true;
                ilv_saved_profiles.Items.Add(newItem);
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
                string oldProfileCacheFilename = _selectedProfile.SavedProfileCacheFilename;

                // Lets rename the entry in the imagelistview to the new name
                foreach (ImageListViewItem myItem in ilv_saved_profiles.Items)
                {
                    if (myItem.Text == oldProfileName)
                    {
                        myItem.Text = txt_profile_save_name.Text;
                    }
                }
                // Lets rename the selectedProfile to the new name
                _selectedProfile.Name = txt_profile_save_name.Text;

                // Lets update the rest of the profile screen too
                lbl_profile_shown.Text = txt_profile_save_name.Text;

                // Then we'll delete the old PNG that we renamed from so we don't get a buildup of them!
                // as a new one will be created when we save later
                try
                {
                    File.Delete(oldProfileCacheFilename);
                }
                catch(Exception ex)
                {
                    // TODO write error to console
                    // TODO specify the correct the exceptions 
                }
            }

            ChangeSelectedProfile(_selectedProfile);

            // Then save the profiles so we always have it updated
            // Generating the imagelistview images automatically as we save.
            Profile.SaveAllProfiles(_savedProfiles);

            // now update the profiles image listview
            //ilv_saved_profiles.Refresh();

        }

        private void ilv_saved_profiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            foreach (Profile savedProfile in _savedProfiles)
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
            Profile.UpdateCurrentProfile();
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