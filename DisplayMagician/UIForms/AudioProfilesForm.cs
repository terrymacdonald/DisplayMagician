using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using DisplayMagician.Contracts;

namespace DisplayMagician.UIForms
{
    public partial class AudioProfilesForm : DisplayMagicianForm
    {
        private AudioProfileView _selectedAudioProfile;
        private int _audioProfileAdvisoryRefreshVersion;
        private bool _canAccessAudioSettings;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public AudioProfilesForm()
        {
            InitializeComponent();
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            UpdateSelectionState();
        }

        private async void AudioProfilesForm_Load(object sender, EventArgs e)
        {
            try
            {
                await RefreshAudioProfilesAsync();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "AudioProfilesForm/AudioProfilesForm_Load: Could not load service-authoritative audio profiles.");
                MessageBox.Show(this, "DisplayMagician could not load your Audio Profiles through the User Agent.", "Audio Profiles", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (lb_audio_profiles.SelectedIndex < 0 && lb_audio_profiles.Items.Count > 0)
            {
                lb_audio_profiles.SelectedIndex = 0;
            }
            UpdateSelectionState();
        }

        private async Task RefreshAudioProfilesAsync(string selectedProfileId = null)
        {
            AudioProfileListResult profileList = await new ControlServicePipeClient().ListAudioProfilesAsync(System.Threading.CancellationToken.None);
            _canAccessAudioSettings = profileList.CanAccessAudioSettings;
            lb_audio_profiles.Items.Clear();
            lb_audio_profiles.DisplayMember = nameof(AudioProfileView.Name);
            foreach (AudioProfileView audioProfile in profileList.SavedProfiles.OrderBy(profile => profile.Name))
            {
                lb_audio_profiles.Items.Add(audioProfile);
            }

            if (!string.IsNullOrWhiteSpace(selectedProfileId))
            {
                _selectedAudioProfile = profileList.SavedProfiles.FirstOrDefault(profile => string.Equals(profile.Id, selectedProfileId, StringComparison.OrdinalIgnoreCase));
                lb_audio_profiles.SelectedItem = _selectedAudioProfile;
            }
            else
            {
                _selectedAudioProfile = profileList.SavedProfiles.FirstOrDefault(profile => profile.IsActive);
                lb_audio_profiles.SelectedItem = _selectedAudioProfile;
            }
        }

        private void UpdateSelectionState()
        {
            bool hasSelection = _selectedAudioProfile != null;
            btn_update_audio_profile.Visible = hasSelection;
            btn_delete_audio_profile.Visible = hasSelection;
            btn_rename_audio_profile.Visible = hasSelection;
            btn_apply_audio_profile.Visible = hasSelection;
            btn_create_audio_profile.Enabled = _canAccessAudioSettings;
            btn_update_audio_profile.Enabled = hasSelection && _canAccessAudioSettings;
            btn_apply_audio_profile.Enabled = hasSelection && _canAccessAudioSettings;
            //gb_selected_audio_settings.Visible = hasSelection;

            if (hasSelection)
            {
                txt_audio_profile_settings.Text = _selectedAudioProfile.SettingsText;
            }
            else
            {
                txt_audio_profile_settings.Clear();
            }

            RefreshAudioProfileAdvisory();
        }

        private async void RefreshAudioProfileAdvisory()
        {
            int refreshVersion = ++_audioProfileAdvisoryRefreshVersion;
            AudioProfileView selectedAudioProfile = _selectedAudioProfile;
            p_audio_profile_advisory.Visible = false;
            btn_open_microphone_settings.Visible = false;

            if (!_canAccessAudioSettings)
            {
                p_audio_profile_advisory.BackColor = System.Drawing.Color.FromArgb(194, 31, 31);
                lbl_audio_profile_advisory.ForeColor = System.Drawing.Color.White;
                lbl_audio_profile_advisory.Text = $"✖ Windows has denied microphone access, so DisplayMagician cannot read or apply audio profiles.{Environment.NewLine}Enable microphone access for DisplayMagician in Windows Settings, then return to DisplayMagician.";
                btn_open_microphone_settings.Visible = true;
                p_audio_profile_advisory.Visible = true;
                return;
            }

            if (selectedAudioProfile == null)
                return;

            string[] unavailableAudioDeviceNames = selectedAudioProfile.UnavailableDeviceNames;
            if (IsDisposed || refreshVersion != _audioProfileAdvisoryRefreshVersion || selectedAudioProfile != _selectedAudioProfile)
                return;

            if (unavailableAudioDeviceNames.Length > 0)
            {
                p_audio_profile_advisory.BackColor = System.Drawing.Color.FromArgb(255, 193, 7);
                lbl_audio_profile_advisory.ForeColor = System.Drawing.Color.Black;
                lbl_audio_profile_advisory.Text = $"⚠ Your audio profile may not apply as expected.{Environment.NewLine}DisplayMagician could not detect the following: {String.Join(", ", unavailableAudioDeviceNames)}. This may be normal if an associated display or device is disconnected or powered off. You may still apply the profile but it may not apply as expected.";
                p_audio_profile_advisory.Visible = true;
            }
        }

        private void lb_audio_profiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            _selectedAudioProfile = lb_audio_profiles.SelectedItem as AudioProfileView;
            UpdateSelectionState();
        }

        private async void btn_create_audio_profile_Click(object sender, EventArgs e)
        {
            using (AudioProfileNameForm nameForm = new AudioProfileNameForm(AudioProfileNameFormMode.Create, isNameAvailable: name => !lb_audio_profiles.Items.Cast<AudioProfileView>().Any(profile => string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase))))
            {
                nameForm.StartPosition = FormStartPosition.CenterParent;
                if (nameForm.ShowDialog(this) != DialogResult.OK)
                    return;

                ControlResponse response = await new ControlServicePipeClient().CreateAudioProfileFromCurrentAsync(nameForm.ProfileName, System.Threading.CancellationToken.None);
                if (response.IsSuccessful)
                {
                    await RefreshAudioProfilesAsync();
                    _selectedAudioProfile = lb_audio_profiles.Items.Cast<AudioProfileView>().FirstOrDefault(profile => string.Equals(profile.Name, nameForm.ProfileName, StringComparison.OrdinalIgnoreCase));
                    lb_audio_profiles.SelectedItem = _selectedAudioProfile;
                }
                else
                {
                    MessageBox.Show(this, response.Message, "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btn_update_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileView selected = lb_audio_profiles.SelectedItem as AudioProfileView;
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

            ControlResponse response = await new ControlServicePipeClient().UpdateAudioProfileFromCurrentAsync(selected.Id, System.Threading.CancellationToken.None);
            if (response.IsSuccessful)
            {
                await RefreshAudioProfilesAsync(selected.Id);
            }
            else
            {
                MessageBox.Show(this, response.Message, "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btn_rename_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileView selected = lb_audio_profiles.SelectedItem as AudioProfileView;
            if (selected == null)
                return;

            using (AudioProfileNameForm nameForm = new AudioProfileNameForm(AudioProfileNameFormMode.Rename, selected.Name, name => !lb_audio_profiles.Items.Cast<AudioProfileView>().Any(profile => !string.Equals(profile.Id, selected.Id, StringComparison.OrdinalIgnoreCase) && string.Equals(profile.Name, name, StringComparison.OrdinalIgnoreCase))))
            {
                nameForm.StartPosition = FormStartPosition.CenterParent;
                if (nameForm.ShowDialog(this) != DialogResult.OK)
                    return;

                ControlResponse response = await new ControlServicePipeClient().RenameAudioProfileAsync(selected.Id, nameForm.ProfileName, System.Threading.CancellationToken.None);
                if (response.IsSuccessful)
                {
                    await RefreshAudioProfilesAsync(selected.Id);
                }
                else
                {
                    MessageBox.Show(this, response.Message, "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private async void btn_delete_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileView selected = lb_audio_profiles.SelectedItem as AudioProfileView;
            if (selected == null)
                return;

            DialogResult result = MessageBox.Show(this,
                $"Delete the Audio Profile '{selected.Name}'? This cannot be undone.",
                "Delete Audio Profile",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            ControlResponse response = await new ControlServicePipeClient().DeleteAudioProfileAsync(selected.Id, System.Threading.CancellationToken.None);
            if (response.IsSuccessful)
            {
                await RefreshAudioProfilesAsync();
                if (lb_audio_profiles.SelectedIndex < 0 && lb_audio_profiles.Items.Count > 0)
                {
                    lb_audio_profiles.SelectedIndex = 0;
                    _selectedAudioProfile = lb_audio_profiles.SelectedItem as AudioProfileView;
                }
                else
                {
                    _selectedAudioProfile = null;
                }
                UpdateSelectionState();
            }
            else
            {
                MessageBox.Show(this, response.Message, "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btn_apply_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileView selected = lb_audio_profiles.SelectedItem as AudioProfileView;
            if (selected == null)
                return;

            btn_apply_audio_profile.Enabled = false;
            try
            {
                int audioDeviceWaitMilliseconds = Program.AppProgramSettings.AudioDeviceWaitSecs * 1000;
                while (true)
                {
                    ControlResponse response = await new ControlServicePipeClient().ApplyAudioProfileAsync(selected.Id, audioDeviceWaitMilliseconds, System.Threading.CancellationToken.None);
                    if (response.IsSuccessful)
                    {
                        logger.Trace($"AudioProfilesForm/btn_apply_audio_profile_Click: Applied '{selected.Name}' audio profile successfully.");
                        return;
                    }

                    logger.Warn($"AudioProfilesForm/btn_apply_audio_profile_Click: {response.Message}");
                    using (AudioApplyFailureForm failureForm = new AudioApplyFailureForm(selected.Name, new List<string> { response.Message }, AudioApplyFailureContext.AudioProfile))
                    {
                        if (failureForm.ShowDialog(this) != DialogResult.Retry || failureForm.SelectedAction != AudioApplyFailureAction.Retry)
                            return;
                    }
                }
            }
            finally
            {
                btn_apply_audio_profile.Enabled = true;
            }
        }

        private void btn_open_microphone_settings_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo("ms-settings:privacy-microphone") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "AudioProfilesForm/btn_open_microphone_settings_Click: Could not open Windows microphone privacy settings.");
                MessageBox.Show(this, "DisplayMagician could not open Windows microphone privacy settings. Open Settings > Privacy & security > Microphone and enable access for DisplayMagician.", "Audio Access", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
