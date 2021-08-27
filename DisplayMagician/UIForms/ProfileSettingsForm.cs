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

        public ProfileSettingsForm()
        {
            logger.Info($"ProfileSettingsForm/ProfileSettingsForm: Creating a ProfileSettingsForm UI Form");

            InitializeComponent();

            // Populate the Style dictionary
            wallpaperStyleText.Add(Wallpaper.Style.Centered, "Center the Wallpaper");
            wallpaperStyleText.Add(Wallpaper.Style.Stretched, "Stretch the Wallpaper");
            wallpaperStyleText.Add(Wallpaper.Style.Tiled, "Tile the Wallpaper");
            
            // Now use it to populate the Style Dropdown
            cmb_wallpaper_display_mode.Items.Clear();
            cmb_wallpaper_display_mode.Items.AddRange(wallpaperStyleText.Values.ToArray());
            cmb_wallpaper_display_mode.SelectedIndex = 0;            
        }

        public ProfileItem Profile
        {
            get;
            set;
        }


        private void ProfileSettingsForm_Load(object sender, EventArgs e)
        {
            if (Profile.SetWallpaper)
            {
                logger.Info($"ProfileSettingsForm/ProfileSettingsForm_Load: Profile {Profile.Name} has loaded with Set Wallpaper enabled and Wallpaper Style {Profile.WallpaperStyle.ToString("G")} and Wallpaper Filename of {Profile.WallpaperBitmapFilename}.");
                cmb_wallpaper_display_mode.SelectedIndex = cmb_wallpaper_display_mode.FindStringExact(wallpaperStyleText[Profile.WallpaperStyle]);
                if (Profile.WallpaperBitmapFilename != "")
                {
                    txt_wallpaper_filename.Text = Profile.WallpaperBitmapFilename;
                }
            }
            
        }

        private void ProfileSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {       
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            Profile.SetWallpaper = cb_set_wallpaper.Checked;
            Profile.WallpaperStyle = (Wallpaper.Style)cmb_wallpaper_display_mode.SelectedValue;
            Profile.WallpaperBitmapFilename = txt_wallpaper_filename.Text;
            this.Close();
        }

        private void cmb_wallpaper_display_mode_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
