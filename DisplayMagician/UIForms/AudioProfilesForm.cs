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
        private bool _isUpdatingUI = false;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public AudioProfilesForm()
        {
            InitializeComponent();
        }

        private void AudioProfilesForm_Load(object sender, EventArgs e)
        {
            RefreshAudioProfilesList();
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
            btn_update_audio_profile.Enabled = hasSelection;
            btn_delete_audio_profile.Enabled = hasSelection;
            btn_rename_audio_profile.Enabled = hasSelection;
            btn_apply_audio_profile.Enabled = hasSelection;
            gb_selected_audio_settings.Enabled = hasSelection;

            _isUpdatingUI = true;
            try
            {
                if (hasSelection && _selectedAudioProfile.WindowsAudioConfig != null)
                {
                    txt_audio_profile_settings.Text = _selectedAudioProfile.GenerateSettingsText();

                    nud_speaker_volume.Value = _selectedAudioProfile.WindowsAudioConfig.Playback.VolumePercent;
                    cb_speaker_mute.Checked = _selectedAudioProfile.WindowsAudioConfig.Playback.IsMuted;

                    nud_microphone_volume.Value = _selectedAudioProfile.WindowsAudioConfig.Recording.VolumePercent;
                    cb_microphone_mute.Checked = _selectedAudioProfile.WindowsAudioConfig.Recording.IsMuted;

                    cb_mono_audio.Checked = _selectedAudioProfile.WindowsAudioConfig.System.IsMonoAudioEnabled;
                    cb_system_audio_enabled.Checked = _selectedAudioProfile.WindowsAudioConfig.System.IsSystemAudioEnabled;
                }
                else
                {
                    txt_audio_profile_settings.Clear();
                    nud_speaker_volume.Value = 50;
                    cb_speaker_mute.Checked = false;
                    nud_microphone_volume.Value = 50;
                    cb_microphone_mute.Checked = false;
                    cb_mono_audio.Checked = false;
                    cb_system_audio_enabled.Checked = true;
                }
            }
            finally
            {
                _isUpdatingUI = false;
            }
        }

        private void lb_audio_profiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedAudioProfile = lb_audio_profiles.SelectedItem as AudioProfileItem;
            UpdateSelectionState();
        }

        private void btn_create_audio_profile_Click(object sender, EventArgs e)
        {
            string profileName = PromptForAudioProfileName("New Audio Profile");
            if (string.IsNullOrWhiteSpace(profileName))
                return;

            if (!AudioProfileItem.IsValidName(profileName))
            {
                MessageBox.Show(this, "That Audio Profile name already exists. Please choose a unique name.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AudioProfileItem newAudioProfile = new AudioProfileItem
            {
                Name = profileName
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

        private void btn_update_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileItem selected = lb_audio_profiles.SelectedItem as AudioProfileItem;
            if (selected == null)
                return;

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

            string newName = PromptForAudioProfileName("Rename Audio Profile", selected.Name);
            if (string.IsNullOrWhiteSpace(newName) || newName == selected.Name)
                return;

            if (!AudioProfileItem.IsValidName(newName))
            {
                MessageBox.Show(this, "That Audio Profile name already exists. Please choose a unique name.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (AudioProfileRepository.RenameAudioProfile(selected, newName))
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

        private void btn_delete_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileItem selected = lb_audio_profiles.SelectedItem as AudioProfileItem;
            if (selected == null)
                return;

            DialogResult result = MessageBox.Show(this,
                $"Delete the Audio Profile '{selected.Name}'?",
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

        private string PromptForAudioProfileName(string title, string currentValue = "")
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 520;
                prompt.Height = 180;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = title;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;

                Label textLabel = new Label() { Left = 12, Top = 12, Width = 480, Text = "Audio Profile name:" };
                TextBox inputBox = new TextBox() { Left = 12, Top = 40, Width = 480, Text = currentValue ?? string.Empty };
                Button confirmation = new Button() { Text = "OK", Left = 326, Width = 80, Top = 80, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Cancel", Left = 412, Width = 80, Top = 80, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog(this) == DialogResult.OK ? inputBox.Text?.Trim() : string.Empty;
            }
        }

        private void groupbox_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);

            GroupBox groupbox = sender as GroupBox;

            if (!groupbox.Enabled)
            {
                int x = ClientRectangle.X + 3;
                int y = ClientRectangle.Y;

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
                int x = ClientRectangle.X + CheckBoxRenderer.GetGlyphSize(
                    e.Graphics, CheckBoxState.UncheckedNormal).Width;
                int y = ClientRectangle.Y + 1;

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
                int x = ClientRectangle.X - 3;
                int y = ClientRectangle.Y;

                TextRenderer.DrawText(e.Graphics, label.Text,
                    label.Font, new Point(x, y), Color.Gray,
                    TextFormatFlags.LeftAndRightPadding);
            }
        }
    }
}
