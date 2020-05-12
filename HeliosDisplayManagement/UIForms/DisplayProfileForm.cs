using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeliosPlus.Resources;
using HeliosPlus.Shared;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using Manina.Windows.Forms;
using System.Text.RegularExpressions;

namespace HeliosPlus.UIForms
{
    internal partial class DisplayProfileForm : Form
    {
        private const string GroupActive = "active";
        private const string GroupCurrent = "current";
        private const string GroupSaved = "saved";
        private Profile _selectedProfile;
        private List<Profile> _savedProfiles = new List<Profile>();
        private string _saveOrRenameMode = "save";
                
        public DisplayProfileForm()
        {
            InitializeComponent();
            this.AcceptButton = this.btn_save_or_rename;
        }

        private void Apply_Click(object sender, EventArgs e)
        {
           /* if (dv_profile.Profile != null &&
                lv_profiles_old.SelectedIndices.Count > 0 &&
                lv_profiles_old.SelectedItems[0].Tag != null)
            {
                if (!dv_profile.Profile.IsPossible)
                {
                    MessageBox.Show(this, Language.This_profile_is_currently_impossible_to_apply,
                        Language.Apply_Profile,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                Enabled = false;
                Visible = false;

                if (
                    new SplashForm(
                        () =>
                        {
                            Task.Factory.StartNew(() => dv_profile.Profile.Apply(), TaskCreationOptions.LongRunning);
                        }, 3, 30).ShowDialog(this) !=
                    DialogResult.Cancel)
                {
                    ReloadProfiles();
                }

                Visible = true;
                Enabled = true;
                Activate();
            }*/
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void Delete_Click(object sender, EventArgs e)
        {
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

        private void Edit_Click(object sender, EventArgs e)
        {
           /* if (dv_profile.Profile != null &&
                lv_profiles_old.SelectedIndices.Count > 0 &&
                lv_profiles_old.SelectedItems[0].Tag != null)
            {
                var selectedIndex = lv_profiles_old.SelectedIndices[0];
                var editForm = new EditForm(dv_profile.Profile);

                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    lv_profiles_old.Items[selectedIndex].Tag = editForm.Profile;
                    SaveProfiles();
                }
            }*/

        }
               

        private void MainForm_Activated(object sender, EventArgs e)
        {
            // Reload the profiles in case we swapped to another program to change it
            //ReloadProfiles();
            // If nothing is selected then select the currently used profile
            /*if (lv_profiles_old.SelectedItems.Count == 0)
            {
                lv_profiles_old.Items[0].Selected = true;
            }*/
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ImageListViewItem newItem;
            _savedProfiles = (List<Profile>)Profile.LoadAllProfiles();
            
            // Temporarily stop updating the saved_profiles listview
            ilv_saved_profiles.SuspendLayout();

            if (_savedProfiles.Count > 0)
            {
                foreach (Profile loadedProfile in _savedProfiles)
                {
                    /*loadedProfile.ProfileIcon.ToBitmap(
                        il_profiles.ImageSize.Width,
                        il_profiles.ImageSize.Height
                    );
                    */
                    loadedProfile.SaveProfileImageToCache();
                    newItem = new ImageListViewItem(loadedProfile.SavedProfileCacheFilename, loadedProfile.Name);
                    ilv_saved_profiles.Items.Add(newItem);

                    if (Profile.CurrentProfile.Equals(loadedProfile))
                    {
                        // We have already saved the current profile!
                        // so we need to show the current profile 
                        // And finally we need to select the currentProfile, as it's the one we're using now
                        newItem.Selected = true;
                    }
                }
                ChangeSelectedProfile(Profile.CurrentProfile);
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


        private void ChangeSelectedProfile(Profile profile)
        {

            // And we need to update the actual selected profile too!
            _selectedProfile = profile;

            // We also need to load the saved profile name to show the user
            lbl_profile_shown.Text = _selectedProfile.Name;

            // And update the save/rename textbox
            txt_profile_save_name.Text = _selectedProfile.Name;


            if (_selectedProfile.Name == Profile.CurrentProfile.Name)
            {
                lbl_profile_shown_subtitle.Text = "(Current Display Profile in use)";
                if (_savedProfiles.Contains(_selectedProfile))
                {
                    _saveOrRenameMode = "rename";
                    btn_save_or_rename.Text = "Rename To";
                }
                else
                {
                    _saveOrRenameMode = "save";
                    btn_save_or_rename.Text = "Save As";
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
            // And finally show the profile in the display view
            dv_profile.Profile = profile;
            dv_profile.Refresh();
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
            ChangeSelectedProfile(Profile.CurrentProfile);
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