using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using DisplayMagicianShared;

namespace DisplayMagician.UIForms
{
    public partial class AudioProfilesForm : Form
    {
        private AudioProfileItem _selectedAudioProfile;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public AudioProfilesForm()
        {
            InitializeComponent();
        }

        private void AudioProfilesForm_Load(object sender, EventArgs e)
        {
            RefreshAudioProfilesList();
            if (lb_audio_profiles.Items.Count > 0)
            {
                lb_audio_profiles.SelectedIndex = 0;
            }
            UpdateSelectionState();
        }

        private void RefreshAudioProfilesList()
        {
            lb_audio_profiles.Items.Clear();
            foreach (AudioProfileItem audioProfile in AudioProfileRepository.AllAudioProfiles.OrderBy(p => p.Name))
            {
                lb_audio_profiles.Items.Add(audioProfile);
            }
        }

        private void UpdateSelectionState()
        {
            bool hasSelection = _selectedAudioProfile != null;
            btn_update_audio_profile.Visible = hasSelection;
            btn_delete_audio_profile.Visible = hasSelection;
            btn_rename_audio_profile.Visible = hasSelection;
            btn_apply_audio_profile.Visible = hasSelection;
            //gb_selected_audio_settings.Visible = hasSelection;

            if (hasSelection && _selectedAudioProfile.WindowsAudioConfig != null)
            {
                txt_audio_profile_settings.Text = _selectedAudioProfile.GenerateSettingsText();
            }
            else
            {
                txt_audio_profile_settings.Clear();
            }
        }

        private void lb_audio_profiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedAudioProfile = lb_audio_profiles.SelectedItem as AudioProfileItem;
            UpdateSelectionState();
        }

        private void btn_create_audio_profile_Click(object sender, EventArgs e)
        {
            using (AudioProfileNameForm nameForm = new AudioProfileNameForm(AudioProfileNameFormMode.Create))
            {
                nameForm.StartPosition = FormStartPosition.CenterParent;
                if (nameForm.ShowDialog(this) != DialogResult.OK)
                    return;

                AudioProfileItem newAudioProfile = new AudioProfileItem
                {
                    Name = nameForm.ProfileName
                };

                if (AudioProfileRepository.AddAudioProfile(newAudioProfile))
                {
                    RefreshAudioProfilesList();
                    lb_audio_profiles.SelectedItem = newAudioProfile;
                    _selectedAudioProfile = newAudioProfile;
                }
                else
                {
                    MessageBox.Show(this, "Unable to create the Audio Profile.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btn_update_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileItem selected = lb_audio_profiles.SelectedItem as AudioProfileItem;
            if (selected == null)
                return;

            if (MessageBox.Show(this,
                $"Do you really want to overwrite the audio settings in the '{selected.Name}' Audio Profile with the audio settings currently in use? This cannot be undone.",
                "Update Audio Profile settings?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning) != DialogResult.Yes)
            {
                return;
            }

            if (selected.CreateProfileFromCurrentAudioSettings())
            {
                AudioProfileRepository.SaveAudioProfiles();
                RefreshAudioProfilesList();
                lb_audio_profiles.SelectedItem = selected;
                _selectedAudioProfile = selected;
            }
            else
            {
                MessageBox.Show(this, "Unable to update the Audio Profile from current settings.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_rename_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileItem selected = lb_audio_profiles.SelectedItem as AudioProfileItem;
            if (selected == null)
                return;

            using (AudioProfileNameForm nameForm = new AudioProfileNameForm(AudioProfileNameFormMode.Rename, selected.Name))
            {
                nameForm.StartPosition = FormStartPosition.CenterParent;
                if (nameForm.ShowDialog(this) != DialogResult.OK)
                    return;

                if (AudioProfileRepository.RenameAudioProfile(selected, nameForm.ProfileName))
                {
                    RefreshAudioProfilesList();
                    lb_audio_profiles.SelectedItem = selected;
                    _selectedAudioProfile = selected;
                }
                else
                {
                    MessageBox.Show(this, "Unable to rename the Audio Profile.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btn_delete_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileItem selected = lb_audio_profiles.SelectedItem as AudioProfileItem;
            if (selected == null)
                return;

            DialogResult result = MessageBox.Show(this,
                $"Delete the Audio Profile '{selected.Name}'? This cannot be undone.",
                "Delete Audio Profile",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            if (AudioProfileRepository.RemoveAudioProfile(selected))
            {
                RefreshAudioProfilesList();
                if (lb_audio_profiles.Items.Count > 0)
                {
                    lb_audio_profiles.SelectedIndex = 0;
                    _selectedAudioProfile = lb_audio_profiles.SelectedItem as AudioProfileItem;
                }
                else
                {
                    _selectedAudioProfile = null;
                }
                UpdateSelectionState();
            }
            else
            {
                MessageBox.Show(this, "Unable to delete the selected Audio Profile.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_apply_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileItem selected = lb_audio_profiles.SelectedItem as AudioProfileItem;
            if (selected == null)
                return;

            if (!selected.SetActive())
            {
                MessageBox.Show(this, $"Unable to apply the Audio Profile '{selected.Name}'.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void groupbox_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);

            GroupBox groupbox = sender as GroupBox;

            if (!groupbox.Enabled)
            {
                int x = groupbox.ClientRectangle.X + 3;
                int y = groupbox.ClientRectangle.Y;

                TextRenderer.DrawText(e.Graphics, groupbox.Text,
                    groupbox.Font, new Point(x, y), Color.Gray,
                    TextFormatFlags.LeftAndRightPadding);
            }
        }

        private void checkbox_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);

            CheckBox checkbox = sender as CheckBox;

            if (!checkbox.Enabled)
            {
                int x = checkbox.ClientRectangle.X + CheckBoxRenderer.GetGlyphSize(
                    e.Graphics, CheckBoxState.UncheckedNormal).Width;
                int y = checkbox.ClientRectangle.Y + 1;

                TextRenderer.DrawText(e.Graphics, checkbox.Text,
                    checkbox.Font, new Point(x, y), Color.Gray,
                    TextFormatFlags.LeftAndRightPadding);
            }
        }

        private void label_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);

            Label label = sender as Label;

            if (!label.Enabled)
            {
                int x = label.ClientRectangle.X - 3;
                int y = label.ClientRectangle.Y;

                TextRenderer.DrawText(e.Graphics, label.Text,
                    label.Font, new Point(x, y), Color.Gray,
                    TextFormatFlags.LeftAndRightPadding);
            }
        }
    }
}
