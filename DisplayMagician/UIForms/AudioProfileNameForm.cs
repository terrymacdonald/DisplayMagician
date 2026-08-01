using System;
using System.Drawing;
using System.Windows.Forms;
using DisplayMagicianShared;

namespace DisplayMagician.UIForms
{
    public enum AudioProfileNameFormMode
    {
        Create,
        Rename
    }

    public partial class AudioProfileNameForm : Form
    {
        private string _originalName;
        private AudioProfileNameFormMode _mode;

        public string ProfileName { get; private set; }

        public AudioProfileNameForm(AudioProfileNameFormMode mode, string currentName = "")
        {
            _mode = mode;
            _originalName = currentName ?? string.Empty;
            InitializeComponent();
        }

        private void AudioProfileNameForm_Load(object sender, EventArgs e)
        {
            if (_mode == AudioProfileNameFormMode.Create)
            {
                Text = "Create Audio Profile";
                lbl_instruction.Text = "Enter a name for the new Audio Profile:";
                txt_profile_name.Text = "New Audio Profile";
                txt_profile_name.SelectAll();
                btn_ok.Text = "&Create";
            }
            else
            {
                Text = "Rename Audio Profile";
                lbl_instruction.Text = "Enter a new name for the Audio Profile:";
                txt_profile_name.Text = _originalName;
                txt_profile_name.SelectAll();
                btn_ok.Text = "&Rename";
            }

            ValidateName();
        }

        private void txt_profile_name_TextChanged(object sender, EventArgs e)
        {
            ValidateName();
        }

        private void ValidateName()
        {
            string name = txt_profile_name.Text?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(name))
            {
                lbl_validation.Text = "Please enter a name.";
                lbl_validation.Visible = true;
                btn_ok.Enabled = false;
                return;
            }

            if (_mode == AudioProfileNameFormMode.Rename &&
                name.Equals(_originalName, StringComparison.OrdinalIgnoreCase))
            {
                lbl_validation.Text = "Please enter a different name.";
                lbl_validation.Visible = true;
                btn_ok.Enabled = false;
                return;
            }

            if (!AudioProfileItem.IsValidName(name))
            {
                lbl_validation.Text = "An Audio Profile with that name already exists. Please choose another name.";
                lbl_validation.Visible = true;
                btn_ok.Enabled = false;
                return;
            }

            lbl_validation.Visible = false;
            btn_ok.Enabled = true;
        }

        private void btn_ok_Click(object sender, EventArgs e)
        {
            ProfileName = txt_profile_name.Text?.Trim();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btn_cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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
