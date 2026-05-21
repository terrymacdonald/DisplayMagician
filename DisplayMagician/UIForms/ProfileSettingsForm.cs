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
            // Wallpaper mode: select matching item, defaulting to Apply
            cmb_wallpaper_mode.SelectedIndex = Profile.WallpaperConfiguration.WallpaperMode switch
            {
                Wallpaper.Mode.Apply => 0,
                Wallpaper.Mode.Clear => 1,
                _ => 2
            };

            // Show the captured background type (read-only informational label)
            lbl_wallpaper_bg_type.Text = Profile.WallpaperConfiguration.BackgroundType switch
            {
                Wallpaper.BackgroundType.SolidColour => "Solid Colour (same on all displays)",
                Wallpaper.BackgroundType.Slideshow   => "Slideshow (same on all displays)",
                Wallpaper.BackgroundType.Spotlight   => "Windows Spotlight (same on all displays)",
                _                                    => "Saved Picture (unique per display)",
            };

            logger.Info($"ProfileSettingsForm/ProfileSettingsForm_Load: Profile {Profile.Name} loaded. Wallpaper mode: {Profile.WallpaperConfiguration.WallpaperMode}.");

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
            Profile.WallpaperConfiguration.WallpaperMode = cmb_wallpaper_mode.SelectedIndex switch
            {
                0 => Wallpaper.Mode.Apply,
                1 => Wallpaper.Mode.Clear,
                _ => Wallpaper.Mode.DoNothing
            };

            Profile.ApplyProfileCount = (int)nud_apply_profile_count.Value;

            Profile.ApplyProfileDelay = (int)nud_apply_profile_delay.Value;

            Profile.ForceExplorerRestart = (bool)cb_force_restart_explorer.Checked;

        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cmb_wallpaper_mode_SelectedIndexChanged(object sender, EventArgs e)
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
