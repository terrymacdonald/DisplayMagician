using DisplayMagicianShared;
using DisplayMagicianShared.Windows;
//using Microsoft.Win32;
using NHotkey;
using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{

    public partial class ProfileSettingsForm : Form
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<Wallpaper.Style, string> wallpaperStyleText = new Dictionary<Wallpaper.Style, string>();
        Bitmap wallpaperImage = null;
        private Dictionary<TaskBarForcedEdge, string> forcedTaskBarEdgeText = new Dictionary<TaskBarForcedEdge, string>();
        private bool _profileSettingChanged = false;

        public ProfileSettingsForm()
        {
            logger.Info($"ProfileSettingsForm/ProfileSettingsForm: Creating a ProfileSettingsForm UI Form");

            InitializeComponent();

            // Populate the Style dictionary
            wallpaperStyleText.Add(Wallpaper.Style.Fill, "Fill the Wallpaper");
            wallpaperStyleText.Add(Wallpaper.Style.Fit, "Fit the Wallpaper");
            wallpaperStyleText.Add(Wallpaper.Style.Stretch, "Stretch the Wallpaper");
            wallpaperStyleText.Add(Wallpaper.Style.Tile, "Tile the Wallpaper");
            wallpaperStyleText.Add(Wallpaper.Style.Center, "Center the Wallpaper");
            wallpaperStyleText.Add(Wallpaper.Style.Span, "Span the Wallpaper");

            cmb_wallpaper_display_mode.DisplayMember = "Value";
            cmb_wallpaper_display_mode.ValueMember = "Text";
            cmb_wallpaper_display_mode.DataSource = new BindingSource(wallpaperStyleText, null);

            // Populate the Forced Taskbar Location dictionary
            if (Utils.IsWindows11())
            {
                // Is Windows 11
                forcedTaskBarEdgeText.Add(TaskBarForcedEdge.Left, "Left");
                forcedTaskBarEdgeText.Add(TaskBarForcedEdge.Top, "Top");
                forcedTaskBarEdgeText.Add(TaskBarForcedEdge.Right, "Right");
                forcedTaskBarEdgeText.Add(TaskBarForcedEdge.Bottom, "Bottom");
            }
            else
            {
                // Is Windows 10
                forcedTaskBarEdgeText.Add(TaskBarForcedEdge.Left, "Left");
                forcedTaskBarEdgeText.Add(TaskBarForcedEdge.Top, "Top");
                forcedTaskBarEdgeText.Add(TaskBarForcedEdge.Right, "Right");
                forcedTaskBarEdgeText.Add(TaskBarForcedEdge.Bottom, "Bottom");
            }

            cmb_forced_taskbar_location.DisplayMember = "Value";
            cmb_forced_taskbar_location.ValueMember = "Text";
            cmb_forced_taskbar_location.DataSource = new BindingSource(forcedTaskBarEdgeText, null);
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
            if (Profile.WallpaperMode.Equals(Wallpaper.Mode.Apply))
            {
                logger.Info($"ProfileSettingsForm/ProfileSettingsForm_Load: Profile {Profile.Name} has loaded with Apply Wallpaper enabled and Wallpaper Style {Profile.WallpaperStyle.ToString("G")} and Wallpaper Filename of {Profile.WallpaperBitmapFilename}.");
                rb_apply_wallpaper.Checked = true;
                cmb_wallpaper_display_mode.SelectedIndex = cmb_wallpaper_display_mode.FindStringExact(wallpaperStyleText[Profile.WallpaperStyle]);
                if (Profile.WallpaperBitmapFilename != "" && File.Exists(Profile.WallpaperBitmapFilename))
                {
                    // Load the existing Wallpaper into the PictureBox
                    //Read the contents of the file into a stream
                    FileStream fileStream = new FileStream(Profile.WallpaperBitmapFilename,FileMode.Open);

                    wallpaperImage = new Bitmap(fileStream);
                    fileStream.Close();
                    pb_wallpaper.Image = wallpaperImage;
                }
            }
            else if (Profile.WallpaperMode.Equals(Wallpaper.Mode.Clear))
            {
                logger.Info($"ProfileSettingsForm/ProfileSettingsForm_Load: Profile {Profile.Name} has loaded with Clear Wallpaper enabled.");
                rb_clear_wallpaper.Checked = true;                
            }
            else
            {
                logger.Info($"ProfileSettingsForm/ProfileSettingsForm_Load: Profile {Profile.Name} is set to do nothing.");
                rb_leave_wallpaper.Checked = true;
                cmb_wallpaper_display_mode.SelectedIndex = 0;
            }

            WINDOWS_DISPLAY_CONFIG winConfig = Profile.WindowsDisplayConfig;
            if (winConfig.TaskBarForcedEdge.Equals(TaskBarForcedEdge.None))
            {
                rb_default_taskbar.Checked = true;
                cmb_forced_taskbar_location.SelectedIndex = 3;
            }
            else
            {
                rb_forced_taskbar.Checked = true;
                int forcedTaskBarSelectedIndex = cmb_forced_taskbar_location.FindStringExact(forcedTaskBarEdgeText[winConfig.TaskBarForcedEdge]);
                if (forcedTaskBarSelectedIndex >= 0)
                {
                    cmb_forced_taskbar_location.SelectedIndex = forcedTaskBarSelectedIndex;
                }                    
            }

        }

        private void ProfileSettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (rb_apply_wallpaper.Checked)
            {
                Profile.WallpaperMode = Wallpaper.Mode.Apply;
            }
            else if (rb_clear_wallpaper.Checked)
            {
                Profile.WallpaperMode = Wallpaper.Mode.Clear;
            }
            else
            {
                Profile.WallpaperMode = Wallpaper.Mode.DoNothing;
            }

            Profile.WallpaperStyle = ((KeyValuePair<Wallpaper.Style, string>)cmb_wallpaper_display_mode.SelectedItem).Key;

            WINDOWS_DISPLAY_CONFIG winConfig = Profile.WindowsDisplayConfig;
            // Reset the taskbar layout binary to the original one we stored when the profile was made
            //winConfig.TaskBarLayout = new List<TaskBarStuckRectangle>(winConfig.OriginalTaskBarLayout);
            if (rb_default_taskbar.Checked)
            {
                winConfig.TaskBarForcedEdge = TaskBarForcedEdge.None;
                
            }
            else
            {
                TaskBarForcedEdge forcedTaskBarSelected = cmb_forced_taskbar_location.FindStringExact(forcedTaskBarEdgeText[winConfig.TaskBarForcedEdge]).Key;
                winConfig.TaskBarForcedEdge = forcedTaskBarSelected;
            }
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            // Check that if there isn't an image, and yet apply this profile checkbox is selected, then we need to unselect it to stop an error state
            if (rb_apply_wallpaper.Checked == true && (Profile.WallpaperBitmapFilename == "" || Profile.WallpaperBitmapFilename == null))
            {
                // We need to force turn off the application of the desktop wallpaper as it won't work
                Profile.WallpaperMode = Wallpaper.Mode.DoNothing;
                Profile.WallpaperBitmapFilename = "";
                rb_apply_wallpaper.Checked = false;
            }
            this.Close();
        }

        private void rb_apply_wallpaper_CheckedChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
            if (rb_apply_wallpaper.Checked)
            {
                // Enable all the things
                pb_wallpaper.Enabled = true;
                btn_select.Enabled = true;
                btn_current.Enabled = true;
                btn_clear.Enabled = true;
                lbl_style.Enabled = true;
                cmb_wallpaper_display_mode.Enabled = true;
            }
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
                    openFileDialog.InitialDirectory = Environment.SpecialFolder.MyPictures.ToString();
                    openFileDialog.Filter = "Image Files(*.bmp; *.jpg; *.gif; *.png; *.tiff)| *.bmp; *.jpg; *.gif; *.png; *.tiff | All files(*.*) | *.*";
                    openFileDialog.FilterIndex = 2;
                    openFileDialog.RestoreDirectory = true;

                    if (openFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Get the path of specified file
                        filePath = openFileDialog.FileName;
                        wallpaperPath = Path.Combine(Program.AppWallpaperPath, $"wallpaper-{Profile.UUID}.jpg");
                        SharedLogger.logger.Trace($"ProfileSettingsForm/btn_select_wallpaper_Click: Storing desktop wallpaper {filePath} as {wallpaperPath} for use in profile {Profile.Name}");

                        //Read the contents of the file into a stream
                        Stream fileStream = openFileDialog.OpenFile();

                        // Create a bitmap
                        wallpaperImage = new Bitmap(fileStream);

                        // Save a copy of the bitmap as PNG
                        wallpaperImage.Save(wallpaperPath, ImageFormat.Png);

                        // Close the original file to free it up
                        fileStream.Close();

                        // Save the path of the saved wallpaper
                        Profile.WallpaperBitmapFilename = wallpaperPath;

                        // Show the wallpaper image so that the user can decide to use it
                        pb_wallpaper.Image = wallpaperImage;
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                SharedLogger.logger.Warn(ex, $"ProfileSettingsForm/btn_select_wallpaper_Click: Argument Null Exception while while storing desktop wallpaper in {wallpaperPath}");
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                SharedLogger.logger.Warn(ex, $"ProfileSettingsForm/btn_select_wallpaper_Click: External InteropServices Exception while while storing desktop wallpaper in {wallpaperPath}");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"ProfileSettingsForm/btn_select_wallpaper_Click: Exception while while storing desktop wallpaper in {wallpaperPath}");
            }
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            // clear the wallpaper sample picturebox
            pb_wallpaper.Image = null;

            // delete the saved wallpaper item
            try
            {
                if (File.Exists(Profile.WallpaperBitmapFilename))
                {
                    File.Delete(Profile.WallpaperBitmapFilename);
                }
            }
            catch (Exception ex)
            {

            }

            // Empty the file name in the Profile
            Profile.WallpaperBitmapFilename = "";
        }

        private void btn_current_Click(object sender, EventArgs e)
        {
            SharedLogger.logger.Trace($"ProfileSettingsForm/btn_current_Click: User requested we get the current windows desktop wallpaper and add them to this display profile");
            // Check if there is a current desktop wallpaper
            Microsoft.Win32.RegistryKey wallpaperKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);
            string wallpaperLocation = (string)wallpaperKey.GetValue("WallPaper");
            if (wallpaperLocation == null || wallpaperLocation == String.Empty)
            {
                // There is no current desktop wallpaper to use
                SharedLogger.logger.Trace($"ProfileSettingsForm/btn_current_Click: There is no existing desktop wallpaper for us to use!");
                MessageBox.Show("There isn't a desktop wallpaper currently being used in Windows, so we have nothing to use!","Cannot find Wallpaper",MessageBoxButtons.OK,MessageBoxIcon.Exclamation);
            }
            else
            {
                // Grab the current desktop wallpaper and save that with this profile
                SharedLogger.logger.Trace($"ProfileSettingsForm/btn_current_Click: There IS an existing desktop wallpaper for us to use!");
                // Figure out the stored filename we want
                string savedWallpaperPath = Path.Combine(Program.AppWallpaperPath, $"wallpaper-{Profile.UUID}.jpg");
                // Try and grab the file from the location listed in the wallpaper key
                if (File.Exists(wallpaperLocation))
                {
                    // If the file is there then great!, we can grab it and save a copy of it
                    SharedLogger.logger.Trace($"ProfileSettingsForm/btn_current_Click: The existing desktop wallpaper file {wallpaperLocation} exists");
                    File.Copy(wallpaperLocation, savedWallpaperPath);
                }
                else
                {
                    // If the file doesn't exist, then we need to try another way to get it.
                    // We try the themes folder first
                    //% appdata %\Microsoft\Windows\Themes and look for TrancodedWallpaper, and just shove a .jpg on it
                    wallpaperLocation = Path.Combine(Environment.SpecialFolder.ApplicationData.ToString(), @"Microsoft\Windows\Themes\TranscodedWallpaper");
                    SharedLogger.logger.Trace($"ProfileSettingsForm/btn_current_Click: Now looking for the TranscodedWallpaper file in {wallpaperLocation}");
                    if (!File.Exists(wallpaperLocation))
                    {
                        SharedLogger.logger.Trace($"ProfileSettingsForm/btn_current_Click: The TranscodedWallpaper file does NOT exist in {wallpaperLocation}");
                        wallpaperLocation = Path.Combine(Environment.SpecialFolder.ApplicationData.ToString(), @"Microsoft\Windows\Themes\CachedFiles\TranscodedWallpaper");
                        if (!File.Exists(wallpaperLocation))
                        {
                            SharedLogger.logger.Trace($"ProfileSettingsForm/btn_current_Click: The TranscodedWallpaper file does NOT exist in {wallpaperLocation} either!");
                            return;
                        }
                        SharedLogger.logger.Trace($"ProfileSettingsForm/btn_current_Click: The TranscodedWallpaper file exists in {wallpaperLocation} (2nd try)");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"ProfileSettingsForm/btn_current_Click: The TranscodedWallpaper file exists in {wallpaperLocation}");
                    }

                }

                // Create an image from the wallpaper
                wallpaperImage = new Bitmap(wallpaperLocation);

                // Save a copy of the bitmap as PNG
                wallpaperImage.Save(savedWallpaperPath, ImageFormat.Png);

                // Save the path of the saved wallpaper
                Profile.WallpaperBitmapFilename = savedWallpaperPath;

                // Show the wallpaper image so that the user can decide to use it
                pb_wallpaper.Image = wallpaperImage;
            }
        }
        

        private void rb_clear_wallpaper_CheckedChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
            if (rb_clear_wallpaper.Checked)
            {
                // Disable all the things
                pb_wallpaper.Enabled = false;
                btn_select.Enabled = false;
                btn_current.Enabled = false;
                btn_clear.Enabled = false;
                lbl_style.Enabled = false;
                cmb_wallpaper_display_mode.Enabled = false;
            }
        }

        private void rb_leave_wallpaper_CheckedChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
            if (rb_leave_wallpaper.Checked)
            {
                // Disable all the things
                pb_wallpaper.Enabled = false;
                btn_select.Enabled = false;
                btn_current.Enabled = false;
                btn_clear.Enabled = false;
                lbl_style.Enabled = false;
                cmb_wallpaper_display_mode.Enabled = false;
            }

        }

        private void cmb_forced_taskbar_location_SelectedIndexChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
        }

        private void rb_default_taskbar_CheckedChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
            if (rb_default_taskbar.Checked)
            {
                // Disable all the things
                cmb_forced_taskbar_location.Enabled = false;
            }
        }

        private void rb_forced_taskbar_CheckedChanged(object sender, EventArgs e)
        {
            _profileSettingChanged = true;
            if (rb_forced_taskbar.Checked)
            {
                // Disable all the things
                cmb_forced_taskbar_location.Enabled = true;
            }
        }
    }
}
