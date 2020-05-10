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

        /*void lv_profiles_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            if ((e.State & ListViewItemStates.Selected) != 0)
            {
                // Draw the background and focus rectangle for a selected item.
                e.Graphics.FillRectangle(Brushes.Maroon, e.Bounds);
                e.DrawFocusRectangle();
            }
            *//*else
            {
                // Draw the background for an unselected item.
                using (LinearGradientBrush brush =
                    new LinearGradientBrush(e.Bounds, Color.Orange,
                    Color.Maroon, LinearGradientMode.Horizontal))
                {
                    e.Graphics.FillRectangle(brush, e.Bounds);
                }
            }*//*

            Point profileImagePoint = new Point(50, 50);
            e.Graphics.DrawImage(il_profiles.Images[e.Item.ImageIndex], profileImagePoint);

            // Draw the item text for views other than the Details view.
            if (lv_profiles_old.View != View.Details)
            {
                e.DrawText();
            }
        }

        // Draws subitem text and applies content-based formatting.
        private void lv_profiles_DrawSubItem(object sender,
            DrawListViewSubItemEventArgs e)
        {
            TextFormatFlags flags = TextFormatFlags.Left;

            using (StringFormat sf = new StringFormat())
            {
                // Store the column text alignment, letting it default
                // to Left if it has not been set to Center or Right.
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        flags = TextFormatFlags.HorizontalCenter;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        flags = TextFormatFlags.Right;
                        break;
                }

                // Draw the text and background for a subitem with a 
                // negative value. 
                double subItemValue;
                if (e.ColumnIndex > 0 && Double.TryParse(
                    e.SubItem.Text, NumberStyles.Currency,
                    NumberFormatInfo.CurrentInfo, out subItemValue) &&
                    subItemValue < 0)
                {
                    // Unless the item is selected, draw the standard 
                    // background to make it stand out from the gradient.
                    if ((e.ItemState & ListViewItemStates.Selected) == 0)
                    {
                        e.DrawBackground();
                    }

                    // Draw the subitem text in red to highlight it. 
                    e.Graphics.DrawString(e.SubItem.Text,
                        lv_profiles_old.Font, Brushes.Red, e.Bounds, sf);

                    return;
                }

                // Draw normal text for a subitem with a nonnegative 
                // or nonnumerical value.
                e.DrawText(flags);
            }
        }

        // Draws column headers.
        private void lv_profiles_DrawColumnHeader(object sender,
            DrawListViewColumnHeaderEventArgs e)
        {
            using (StringFormat sf = new StringFormat())
            {
                // Store the column text alignment, letting it default
                // to Left if it has not been set to Center or Right.
                switch (e.Header.TextAlign)
                {
                    case HorizontalAlignment.Center:
                        sf.Alignment = StringAlignment.Center;
                        break;
                    case HorizontalAlignment.Right:
                        sf.Alignment = StringAlignment.Far;
                        break;
                }

                // Draw the standard header background.
                e.DrawBackground();

                // Draw the header text.
                using (Font headerFont =
                            new Font("Helvetica", 10, FontStyle.Bold))
                {
                    e.Graphics.DrawString(e.Header.Text, headerFont,
                        Brushes.Black, e.Bounds, sf);
                }
            }
            return;
        }
*/


        public DisplayProfileForm()
        {
            InitializeComponent();

            //txt_profile_save_name.Validating += new System.ComponentModel.CancelEventHandler(txt_profile_save_name_Validating);
            //lv_profiles.Groups.Add(GroupCurrent, Language.Current);
            //lv_profiles.Groups.Add(GroupActive, Language.Active_Profiles);
            //lv_profiles.Groups.Add(GroupSaved, Language.Saved_Profiles);

            //lv_profiles.Groups.Add(GroupCurrent, Language.Current);

            /* lv_profiles_old.View = View.Tile;
             lv_profiles_old.Alignment = ListViewAlignment.Left;
             lv_profiles_old.OwnerDraw = true;
             lv_profiles_old.DrawItem += lv_profiles_DrawItem;
             lv_profiles_old.DrawItem += new DrawListViewItemEventHandler(lv_profiles_DrawItem);
             lv_profiles_old.DrawSubItem += new DrawListViewSubItemEventHandler(lv_profiles_DrawSubItem);
             lv_profiles_old.DrawColumnHeader += new DrawListViewColumnHeaderEventHandler(lv_profiles_DrawColumnHeader);
             lv_profiles_old.TileSize = new Size((lv_profiles_old.ClientSize.Height/9)*16, lv_profiles_old.ClientSize.Height - (SystemInformation.HorizontalScrollBarHeight + 4));*/

            //Profile.LoadAllProfiles();

        }

        /*private ListViewItem AddProfile(Profile profile = null)
        {
            il_profiles.Images.Add(
                new ProfileIcon(profile ?? Profile.GetCurrent()).ToBitmap(
                    il_profiles.ImageSize.Width,
                    il_profiles.ImageSize.Height));

            *//*return lv_profiles_old.Items.Add(new ListViewItem
            {
                Text = profile.Name,
                ImageIndex = il_profiles.Images.Count - 1,
                Tag = profile
                //Group = lv_profiles.Groups[profile == null ? GroupCurrent : (profile.IsActive ? GroupActive : GroupSaved)]
            });*//*
        }*/


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
            /*if (dv_profile.Profile != null &&
                lv_profiles_old.SelectedIndices.Count > 0 &&
                lv_profiles_old.SelectedItems[0].Tag != null)
            {
                var selectedIndex = lv_profiles_old.SelectedIndices[0];

                if (
                    MessageBox.Show(this, Language.Are_you_sure, Language.Deletion, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) ==
                    DialogResult.Yes)
                {
                    il_profiles.Images.RemoveAt(lv_profiles_old.Items[selectedIndex].ImageIndex);
                    lv_profiles_old.Items.RemoveAt(selectedIndex);
                    SaveProfiles();
                }
            }*/

            ReloadProfiles();
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

            ReloadProfiles();
        }

        /*private void lv_profiles_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            var selectedProfile = (Profile) lv_profiles_old.Items[e.Item].Tag;

            if (selectedProfile == null || e.Label == null || selectedProfile.Name == e.Label)
            {
                e.CancelEdit = true;

                return;
            }

            if (string.IsNullOrWhiteSpace(e.Label) ||
                lv_profiles_old.Items.Cast<ListViewItem>()
                    .Select(item => item.Tag as Profile)
                    .Where(profile => profile != null)
                    .Any(
                        profile =>
                            !profile.Equals(selectedProfile) &&
                            profile.Name.Equals(e.Label, StringComparison.InvariantCultureIgnoreCase)))
            {
                MessageBox.Show(this, Language.Invalid_or_duplicate_profile_name, Language.Validation,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                e.CancelEdit = true;

                return;
            }

            lv_profiles_old.Items[e.Item].Text = selectedProfile.Name = e.Label;
            SaveProfiles();
        }

        private void lv_profiles_BeforeLabelEdit(object sender, LabelEditEventArgs e)
        {
            e.CancelEdit = !(lv_profiles_old.Items[e.Item].Tag is Profile);
        }

        private void lv_profiles_DoubleClick(object sender, EventArgs e)
        {
            btn_edit.PerformClick();
        }

        // Need to check why we did it this way rather than just using the 
        // list items themselves for clicking? That way we'd avoid selecting nothing...
        private void lv_profiles_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && lv_profiles_old.SelectedItems.Count > 0)
            {
                var itemRect = lv_profiles_old.GetItemRect(lv_profiles_old.SelectedIndices[0]);

                if (e.Location.X > itemRect.X &&
                    e.Location.X <= itemRect.Right &&
                    e.Location.Y > itemRect.Y &&
                    e.Location.Y <= itemRect.Bottom)
                {
                    menu_profiles.Show(lv_profiles_old, e.Location);
                }
            }
        }

        private void lv_profiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_profiles_old.SelectedItems.Count > 0)
            {
                dv_profile.Profile = lv_profiles_old.SelectedItems[0].Tag as Profile ?? Profile.CurrentProfile;
            }
            else
            {
                dv_profile.Profile = null;
            }

            // Set the Profile name
            lbl_profile.Text = $"Selected Profile: {dv_profile.Profile?.Name ?? Language.None}";

            // Turn on the buttons if the 
            if (dv_profile.Profile != null) {
                if (lv_profiles_old.SelectedItems[0].Tag != null)
                {
                    editToolStripMenuItem.Enabled = true;
                    btn_edit.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                    btn_delete.Enabled = true;
                    createShortcutToolStripMenuItem.Enabled = true;

                    if (!dv_profile.Profile.IsActive)
                    {
                        applyToolStripMenuItem.Enabled = true;
                        btn_apply.Enabled = true;
                    }
                }
                cloneToolStripMenuItem.Enabled = true;
                btn_copy.Enabled = true;

            }
            
            // Refresh the profiles again in case anything changed
            RefreshProfilesStatus();
        }*/

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
            //ReloadProfiles();
            // Select the first item in the profiles list so pressing the buttons makes sense!
            //lv_profiles_old.Items[0].Selected = true;
            //lv_profiles.Items[0].Focused = true;
            //lv_profiles.Items[0].Checked = true;
            ImageListViewItem newItem;
            _savedProfiles = (List<Profile>)Profile.LoadAllProfiles();
            

            dv_profile.Profile = Profile.CurrentProfile;
            _selectedProfile = Profile.CurrentProfile;

            // Temporarily stop updating the saved_profiles listview
            ilv_saved_profiles.SuspendLayout();

            if (_savedProfiles.Count > 0)
            {
                foreach (Profile loadedProfile in _savedProfiles)
                {
                    Bitmap profileIconImage = loadedProfile.ProfileIcon.ToBitmap(
                        il_profiles.ImageSize.Width,
                        il_profiles.ImageSize.Height
                    );

                    newItem = new ImageListViewItem(loadedProfile.SavedProfileCacheFilename, loadedProfile.Name);
                    ilv_saved_profiles.Items.Add(newItem);

                    if (Profile.CurrentProfile.Equals(loadedProfile))
                    {
                        // We have already saved the current profile!
                        // so we need to show the current profile 
                        // and make sure the current profile name has
                        // been updated with the name saved last time
                        // So as the current profile was already saved, we need to change
                        // the save button to a rename one
                        _saveOrRenameMode = "rename";
                        btn_save_or_rename.Text = "Rename to";
                        // And finally we need to select the currentProfile, as it's the one we're using now
                        newItem.Selected = true;

                    }
                    else
                    {
                        // We haven't saved the current profile yet, but we've saved others!
                        // This is a new one
                        _saveOrRenameMode = "save";
                        btn_save_or_rename.Text = "Save As";
                        if (!loadedProfile.IsPossible)
                        {
                            // TODO mark the imagelist view netItem in some way
                            // to identify that it's not avalid profile, but
                            // still allow users to select it to rename it and
                            // see what the profile looks like.
                        }
                    }

                }

                ChangeSelectedProfile(Profile.CurrentProfile);
            }
            else
            {
                // If there are no profiles at all then we are starting from scratch!
                // Show the profile in the DV window
                // Use the current profile name in the label and the save name
                _saveOrRenameMode = "save";
                btn_save_or_rename.Text = "Save As";
                ChangeSelectedProfile(Profile.CurrentProfile);
            }

            // Restart updating the saved_profiles listview
            ilv_saved_profiles.ResumeLayout();

            //olv_profiles.LargeImageList = il_profiles;

            // Start the ObjectListView list view
            //ilv_saved_profiles.Items = il_profiles;
        }

        /*   public object ProfileImageGetter(object rowObject)
           {
               Profile p = (Profile)rowObject;
               return 
           };*/

        private void ChangeSelectedProfile(Profile profile)
        {

            // We also need to load the saved profile name to show the user
            ChangeSelectedProfileDetails(profile.Name);
            // And we need to update the actual selected profile too!
            _selectedProfile = profile;
            // And finally show the profile in the display view
            dv_profile.Profile = profile;
            dv_profile.Refresh();
        }

        private void ChangeSelectedProfileDetails(string profileName)
        {
            // We also need to load the saved profile name to show the user
            lbl_profile_shown.Text = profileName;
            txt_profile_save_name.Text = profileName;
        }


        private bool IsValidFilename(string testName)
        {
            string strTheseAreInvalidFileNameChars = new string(Path.GetInvalidFileNameChars());
            Regex regInvalidFileName = new Regex("[" + Regex.Escape(strTheseAreInvalidFileNameChars) + "]");

            if (regInvalidFileName.IsMatch(testName)) { return false; };

            return true;
        }


        private void RefreshProfilesStatus()
        {
            Profile.RefreshActiveStatus();
            //lv_profiles_old.Invalidate();
        }

        private void ReloadProfiles()
        {
            //Profile.RefreshActiveStatus();
            var profiles = Profile.LoadAllProfiles();
            //lv_profiles_old.Items.Clear();
            //il_profiles.Images.Clear();
            //olv_profiles
            

            if (!profiles.Any(profile => profile.IsActive))
            {
                //AddProfile().Selected = true;
            }

            foreach (var profile in profiles)
            {
                //AddProfile(profile);
            }

            //lv_profiles_old.SelectedIndices.Clear();
            //lv_profiles_old.Invalidate();
        }



        private void btn_save_as_Click(object sender, EventArgs e)
        {

            // Check the name is valid
            if (!IsValidFilename(txt_profile_save_name.Text))
            {
                MessageBox.Show("The profile name cannot contain the following characters:" + Path.GetInvalidFileNameChars(), "Invalid characters in profile name", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }
            

            // If we're saving the current profile as a new item
            // then we'll be in "save" mode
            if (_saveOrRenameMode == "save")
            {

                // Check we're not already using the name
                foreach (Profile savedProfile in Profile.AllSavedProfiles)
                {
                    //if (String.Equals(txt_profile_save_name.Text, savedProfile.Name, StringComparison.InvariantCultureIgnoreCase))
                    if (_selectedProfile.Equals(Profile.CurrentProfile))
                    {
                        MessageBox.Show("Sorry, you can only have one saved display profile for each display configuration.", "Display profile already exists", MessageBoxButtons.OK, MessageBoxIcon.Error);

                        return;
                    }
                }


                // We're in 'save' mode!
                // Add the current profile to the list of profiles
                _savedProfiles.Add(_selectedProfile);
                // Then save the profiles so we always have it updated
                Profile.SaveAllProfiles(_savedProfiles);
                // Load the currentProfile image into the imagelistview
                ImageListViewItem newItem = new ImageListViewItem(_selectedProfile.SavedProfileCacheFilename, _selectedProfile.Name);
                newItem.Selected = true;
                ilv_saved_profiles.Items.Add(newItem);

                //ilv_saved_profiles.Refresh();
                /*foreach (ImageListViewItem myItem in ilv_saved_profiles.Items)
                {
                    if (_selectedProfile.Name == myItem.Text) 
                    {
                        myItem.Selected = true;
                        myItem.Focused = true;
                    }
                    
                }*/
            }
            else
            {
                // We're in 'rename' mode!
                // THen rename the imagelistview item
                string oldProfileName = _selectedProfile.Name;
                foreach (ImageListViewItem myItem in ilv_saved_profiles.Items)
                {
                    if (myItem.Text == oldProfileName)
                    {
                        myItem.Text = txt_profile_save_name.Text;
                    }
                }
                // So lets rename the profile itself in the saved profiles
                // and the selectprofile name too
                foreach (Profile savedProfile in _savedProfiles)
                {
                    if (savedProfile.Equals(_selectedProfile))
                    {
                        _selectedProfile.Name = txt_profile_save_name.Text;
                        savedProfile.Name = txt_profile_save_name.Text;
                        ChangeSelectedProfileDetails(_selectedProfile.Name);
                    }
                }

                // TODO - delete the old PNG that we renamed from so we don't get a buildup of them!
                // Then save all the profiles to lock it in
                Profile.SaveAllProfiles(_savedProfiles);
            }

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

        /*protected void txt_profile_save_name_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                (txt_profile_save_name.Text);
                errorProvider1.SetError(txt_profile_save_name, "");
            }
            catch (Exception ex)
            {
                errorProvider1.SetError(txt_profile_save_name, "Save name already used in another saved display profile.");
            }
        }*/

    }
}