using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    internal enum AudioApplyFailureAction
    {
        Retry,
        ContinueWithoutChangingAudio,
        Cancel
    }

    internal enum AudioApplyFailureContext
    {
        Shortcut,
        AudioProfile
    }

    internal partial class AudioApplyFailureForm : Form
    {
        public AudioApplyFailureAction SelectedAction { get; private set; } = AudioApplyFailureAction.Cancel;

        public AudioApplyFailureForm(string profileName, List<string> missingAudioDeviceNames)
            : this(profileName, missingAudioDeviceNames, AudioApplyFailureContext.Shortcut)
        {
        }

        public AudioApplyFailureForm(string profileName, List<string> missingAudioDeviceNames, AudioApplyFailureContext context)
        {
            InitializeComponent();

            string missingDevicesText = missingAudioDeviceNames != null && missingAudioDeviceNames.Count > 0
                ? $"{Environment.NewLine}{Environment.NewLine}Missing audio devices: {String.Join(", ", missingAudioDeviceNames)}."
                : String.Empty;
            if (context == AudioApplyFailureContext.AudioProfile)
            {
                btn_continue_without_audio_change.Visible = false;
                btn_retry.Location = new System.Drawing.Point(btn_cancel.Left - btn_retry.Width - 6, btn_retry.Top);
                lbl_message.Text = $"DisplayMagician could not apply the '{profileName}' audio profile in time.{missingDevicesText}{Environment.NewLine}{Environment.NewLine}" +
                    "Check or reconnect your audio device, then retry or cancel.";
            }
            else
            {
                lbl_message.Text = $"DisplayMagician could not apply the '{profileName}' audio profile in time.{missingDevicesText}{Environment.NewLine}{Environment.NewLine}" +
                    "You can retry after enabling or reconnecting the audio device; run the shortcut using your current audio setup; or cancel.";
            }
        }

        private void btn_retry_Click(object sender, EventArgs e)
        {
            SelectedAction = AudioApplyFailureAction.Retry;
            DialogResult = DialogResult.Retry;
            Close();
        }

        private void btn_continue_without_audio_change_Click(object sender, EventArgs e)
        {
            SelectedAction = AudioApplyFailureAction.ContinueWithoutChangingAudio;
            DialogResult = DialogResult.Ignore;
            Close();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            SelectedAction = AudioApplyFailureAction.Cancel;
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
