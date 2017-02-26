using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeliosDisplayManagement.Resources;
using HeliosDisplayManagement.Shared;

namespace HeliosDisplayManagement.UIForms
{
    internal partial class MainForm : Form
    {
        private const string GroupCurrent = "current";
        private const string GroupSaved = "saved";
        private const string GroupActive = "active";
        public MainForm()
        {
            InitializeComponent();
            lv_profiles.Groups.Add(GroupCurrent, "Current");
            lv_profiles.Groups.Add(GroupActive, "Active Profiles");
            lv_profiles.Groups.Add(GroupSaved, "Saved Profiles");
        }

        private void Clone_Click(object sender, EventArgs e)
        {
            if (dv_profile.Profile != null)
            {
                var clone = dv_profile.Profile.Clone();
                var i = 0;
                string name;
                while (true)
                {
                    i++;
                    name = $"{clone.Name} ({i})";
                    if (lv_profiles.Items.OfType<Profile>().Any(profile => profile.Name == name))
                        continue;
                    break;
                }
                clone.Name = name;
                AddProfile(clone).Selected = true;
                SaveProfiles();
                btn_edit.PerformClick();
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ReloadProfiles();
        }

        private ListViewItem AddProfile(Profile profile = null)
        {
            il_profiles.Images.Add(
                new ProfileIcon(profile ?? Profile.GetCurrent()).ToBitmap(
                    il_profiles.ImageSize.Width,
                    il_profiles.ImageSize.Height));
            return lv_profiles.Items.Add(new ListViewItem()
            {
                Text = profile?.ToString() ?? Language.Current,
                ImageIndex = il_profiles.Images.Count - 1,
                Tag = profile,
                Group = lv_profiles.Groups[profile == null ? GroupCurrent : (profile.IsActive ? GroupActive : GroupSaved)]
            });
        }

        private void ReloadProfiles()
        {
            Profile.RefreshActiveStatus();
            var profiles = Profile.GetAllProfiles().ToArray();
            lv_profiles.Items.Clear();
            il_profiles.Images.Clear();
            if (!profiles.Any(profile => profile.IsActive))
            {
                AddProfile().Selected = true;
            }
            foreach (var profile in profiles)
            {
                AddProfile(profile);
            }
            lv_profiles.SelectedIndices.Clear();
            lv_profiles.Invalidate();
        }

        private void RefreshProfilesStatus()
        {
            Profile.RefreshActiveStatus();
            lv_profiles.Invalidate();
        }

        private void SaveProfiles()
        {
            Profile.SetAllProfiles(lv_profiles.Items.Cast<ListViewItem>().Select(item => item.Tag as Profile).Where(profile => profile != null));
            ReloadProfiles();
        }

        private void lv_profiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_profiles.SelectedItems.Count > 0)
            {
                dv_profile.Profile = lv_profiles.SelectedItems[0].Tag as Profile ?? Profile.GetCurrent(Language.Current);
            }
            else
            {
                dv_profile.Profile = null;
            }
            lbl_profile.Text = dv_profile.Profile?.Name ?? Language.None;
            applyToolStripMenuItem.Enabled =
                btn_apply.Enabled = (dv_profile.Profile != null) && (lv_profiles.SelectedIndices[0] != 0) &&
                                    !dv_profile.Profile.IsActive;
            editToolStripMenuItem.Enabled =
                btn_edit.Enabled = (dv_profile.Profile != null) && (lv_profiles.SelectedIndices[0] != 0);
            deleteToolStripMenuItem.Enabled =
                btn_delete.Enabled = (dv_profile.Profile != null) && (lv_profiles.SelectedIndices[0] != 0);
            cloneToolStripMenuItem.Enabled = btn_clone.Enabled = dv_profile.Profile != null;
            createShortcutToolStripMenuItem.Enabled =
                btn_shortcut.Enabled = (dv_profile.Profile != null) && (lv_profiles.SelectedIndices[0] != 0);
            RefreshProfilesStatus();
        }


        private void Apply_Click(object sender, EventArgs e)
        {
            if (dv_profile.Profile != null && lv_profiles.SelectedIndices.Count > 0 && lv_profiles.SelectedIndices[0] > 0)
            {
                if (!dv_profile.Profile.IsPossible)
                {
                    MessageBox.Show(this, Language.This_profile_is_currently_impossible_to_apply, Language.Apply_Profile,
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
                        }, 3, 30).ShowDialog(this) != DialogResult.Cancel)
                {
                    ReloadProfiles();
                }
                Visible = true;
                Enabled = true;
                Activate();
            }
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void Edit_Click(object sender, EventArgs e)
        {
            if (dv_profile.Profile != null && lv_profiles.SelectedIndices.Count > 0 && lv_profiles.SelectedIndices[0] > 0)
            {
                var selectedIndex = lv_profiles.SelectedIndices[0];
                var editForm = new EditForm(dv_profile.Profile);
                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    lv_profiles.Items[selectedIndex].Tag = editForm.Profile;
                    SaveProfiles();
                }
            }
            ReloadProfiles();
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            if (dv_profile.Profile != null && lv_profiles.SelectedIndices.Count > 0 && lv_profiles.SelectedIndices[0] > 0)
            {
                var selectedIndex = lv_profiles.SelectedIndices[0];
                if (
                    MessageBox.Show(this, Language.Are_you_sure, Language.Deletion, MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    il_profiles.Images.RemoveAt(lv_profiles.Items[selectedIndex].ImageIndex);
                    lv_profiles.Items.RemoveAt(selectedIndex);
                    SaveProfiles();
                }
            }
            ReloadProfiles();
        }

        private void lv_profiles_DoubleClick(object sender, EventArgs e)
        {
            btn_edit.PerformClick();
        }

        private void CreateShortcut_Click(object sender, EventArgs e)
        {
            if (dv_profile.Profile != null && lv_profiles.SelectedIndices.Count > 0 && lv_profiles.SelectedIndices[0] > 0)
            {
                var shortcutForm = new ShortcutForm(dv_profile.Profile);
                shortcutForm.ShowDialog(this);
                SaveProfiles();
            }
        }

        private void lv_profiles_MouseUp(object sender, MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Right) && lv_profiles.SelectedItems.Count > 0)
            {
                var itemRect = lv_profiles.GetItemRect(lv_profiles.SelectedIndices[0]);
                if ((e.Location.X > itemRect.X) && (e.Location.X <= itemRect.Right) && (e.Location.Y > itemRect.Y) &&
                    (e.Location.Y <= itemRect.Bottom))
                    menu_profiles.Show(lv_profiles, e.Location);
            }
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            ReloadProfiles();
        }
    }
}