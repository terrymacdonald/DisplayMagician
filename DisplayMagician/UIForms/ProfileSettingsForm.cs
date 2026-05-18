using DisplayMagicianShared;
using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{

    public partial class ProfileSettingsForm : Form
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _profileSettingChanged = false;

        public ProfileSettingsForm()
        {
            logger.Info($"ProfileSettingsForm/ProfileSettingsForm: Creating a ProfileSettingsForm UI Form");
            InitializeComponent();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ProfileItem Profile
        {
            get;
            set;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ProfileSettingChanged
        {
            get
            {
                return _profileSettingChanged;
            }
            set
            {
                _profileSettingChanged = value;
            }
        }


        private void ProfileSettingsForm_Load(object sender, EventArgs e)
        {
            // Wallpaper mode: checked = Apply, unchecked = DoNothing
            cb_apply_wallpaper.Checked = Profile.WallpaperConfiguration.WallpaperMode == Wallpaper.Mode.Apply;
            logger.Info($"ProfileSettingsForm/ProfileSettingsForm_Load: Profile {Profile.Name} loaded. Apply wallpapers: {cb_apply_wallpaper.Checked}.");

            if (Profile.ApplyProfileCount >= 0 && Profile.ApplyProfileCount <= 10)
            {
                nud_apply_profile_count.Value = Profile.ApplyProfileCount;
            }
            else
            {
                nud_apply_profile_count.Value = 1;
            }

            if (Profile.ApplyProfileDelay >= 0 && Profile.ApplyProfileDelay <= 1000)
            {
                nud_apply_profile_delay.Value = Profile.ApplyProfileDelay;
            }
            else
            {
                nud_apply_profile_delay.Value = 0;
            }

            if (nud_apply_profile_count.Value > 1)
            {
                lbl_apply_profile_delay.Visible = true;
                nud_apply_profile_delay.Visible = true;
                lbl_seconds.Visible = true;
            }
            else
            {
                lbl_apply_profile_delay.Visible = false;
                nud_apply_profile_delay.Visible = false;
                lbl_seconds.Visible = false;
            }

            if (Profile.ForceExplorerRestart)
            {
                cb_force_restart_explorer.Checked = true;
            }
            else
            {
                cb_force_restart_explorer.Checked = false;
            }
        }

        private void ProfileSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Profile.WallpaperConfiguration.WallpaperMode = cb_apply_wallpaper.Checked
                ? Wallpaper.Mode.Apply
                : Wallpaper.Mode.DoNothing;

            Profile.ApplyProfileCount = (int)nud_apply_profile_count.Value;

            Profile.ApplyProfileDelay = (int)nud_apply_profile_delay.Value;

            Profile.ForceExplorerRestart = (bool)cb_force_restart_explorer.Checked;

        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cb_apply_wallpaper_CheckedChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
        }

        private void nud_apply_profile_count_ValueChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
            if (nud_apply_profile_count.Value > 1)
            {
                lbl_apply_profile_delay.Visible = true;
                nud_apply_profile_delay.Visible = true;
                lbl_seconds.Visible = true;
            }
            else
            {
                lbl_apply_profile_delay.Visible = false;
                nud_apply_profile_delay.Visible = false;
                lbl_seconds.Visible = false;
            }
        }

        private void nud_apply_profile_delay_ValueChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
        }

        private void cb_force_restart_explorer_CheckedChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
        }
    }
}
