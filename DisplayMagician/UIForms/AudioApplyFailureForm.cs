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

    internal partial class AudioApplyFailureForm : Form
    {
        public AudioApplyFailureAction SelectedAction { get; private set; } = AudioApplyFailureAction.Cancel;

        public AudioApplyFailureForm(string profileName, List<string> missingAudioDeviceNames)
        {
            InitializeComponent();

            string missingDevicesText = missingAudioDeviceNames != null && missingAudioDeviceNames.Count > 0
                ? $"{Environment.NewLine}{Environment.NewLine}Missing audio devices: {String.Join(", ", missingAudioDeviceNames)}."
                : String.Empty;
            lbl_message.Text = $"DisplayMagician could not apply the '{profileName}' audio profile in time.{missingDevicesText}{Environment.NewLine}{Environment.NewLine}" +
                "You can retry after enabling or reconnecting the audio device; run the shortcut using your current audio setup; or cancel.";
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
