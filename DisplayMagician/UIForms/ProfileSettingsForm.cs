using DisplayMagicianShared;
using NHotkey;
using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WK.Libraries.BootMeUpNS;

namespace DisplayMagician.UIForms
{

    public partial class ProfileSettingsForm : Form
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<Wallpaper.Style, string> wallpaperStyleText = new Dictionary<Wallpaper.Style, string>();
        private bool _profileSettingChanged = false;

        public ProfileSettingsForm()
        {
            logger.Info($"ProfileSettingsForm/ProfileSettingsForm: Creating a ProfileSettingsForm UI Form");

            InitializeComponent();

            // Populate the Style dictionary
            wallpaperStyleText.Add(Wallpaper.Style.Centered, "Center the Wallpaper");
            wallpaperStyleText.Add(Wallpaper.Style.Stretched, "Stretch the Wallpaper");
            wallpaperStyleText.Add(Wallpaper.Style.Tiled, "Tile the Wallpaper");

            cmb_wallpaper_display_mode.DisplayMember = "Value";
            cmb_wallpaper_display_mode.ValueMember = "Text";
            cmb_wallpaper_display_mode.DataSource = new BindingSource(wallpaperStyleText, null);
        }

        public ProfileItem Profile
        {
            get;
            set;
        }

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
            if (Profile.SetWallpaper)
            {
                logger.Info($"ProfileSettingsForm/ProfileSettingsForm_Load: Profile {Profile.Name} has loaded with Set Wallpaper enabled and Wallpaper Style {Profile.WallpaperStyle.ToString("G")} and Wallpaper Filename of {Profile.WallpaperBitmapFilename}.");
                cb_set_wallpaper.Checked = true;
                cmb_wallpaper_display_mode.SelectedIndex = cmb_wallpaper_display_mode.FindStringExact(wallpaperStyleText[Profile.WallpaperStyle]);
                /*if (Profile.WallpaperBitmapFilename != "")
                {
                    txt_wallpaper_filename.Text = Profile.WallpaperBitmapFilename;
                }*/
            }
            else
            {
                cb_set_wallpaper.Checked = false;
            }
            
        }

        private void ProfileSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Profile.SetWallpaper = cb_set_wallpaper.Checked;
            Profile.WallpaperStyle = ((KeyValuePair<Wallpaper.Style, string>)cmb_wallpaper_display_mode.SelectedItem).Key;
            //Profile.WallpaperBitmapFilename = txt_wallpaper_filename.Text;
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void cb_set_wallpaper_CheckedChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
        }

        private void cmb_wallpaper_display_mode_SelectedIndexChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
        }

        private void btn_select_wallpaper_Click(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
        }
    }
}
