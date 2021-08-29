using DisplayMagicianShared;
using NHotkey;
using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WK.Libraries.BootMeUpNS;

namespace DisplayMagician.UIForms
{

    public partial class ProfileSettingsForm : Form
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<Wallpaper.Style, string> wallpaperStyleText = new Dictionary<Wallpaper.Style, string>();
        Bitmap wallpaperImage = null;
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
                if (Profile.WallpaperBitmapFilename != "" && File.Exists(Profile.WallpaperBitmapFilename))
                {
                    // Load the existing Wallpaper into the PictureBox
                    //Read the contents of the file into a stream
                    //StreamReader streamReader = new StreamReader(Profile.WallpaperBitmapFilename);
                    FileStream fileStream = new FileStream(Profile.WallpaperBitmapFilename,FileMode.Open);

                    wallpaperImage = new Bitmap(fileStream);                    
                    pb_wallpaper.Image = wallpaperImage;
                }
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
            wallpaperImage.Dispose();
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
            string filePath = string.Empty;
            string wallpaperPath = string.Empty;

            try
            {
                using (OpenFileDialog openFileDialog = new OpenFileDialog())
                {
                    openFileDialog.InitialDirectory = "c:\\";
                    openFileDialog.Filter = "Image Files(*.bmp; *.jpg; *.gif; *.png; *.tiff)| *.bmp; *.jpg; *.gif; *.png; *.tiff | All files(*.*) | *.*";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Get the path of specified file
                        filePath = openFileDialog.FileName;

                        // If The user selected a photo then we need to set the set wallpaper to yes
                        cb_set_wallpaper.Checked = true;
                        
                        //Read the contents of the file into a stream
                        Stream fileStream = openFileDialog.OpenFile();

                        wallpaperImage = new Bitmap(fileStream);
                        wallpaperPath = Path.Combine(Program.AppWallpaperPath, $"wallpaper-{Profile.UUID}.jpg");

                        wallpaperImage.Save(wallpaperPath, ImageFormat.Png);

                        Profile.WallpaperBitmapFilename = wallpaperPath;

                        pb_wallpaper.Image = wallpaperImage;
                    }
                }
            }
            catch (Exception ex)
            {
                // TODO: handle the exceptions
            }
        }
    }
}
