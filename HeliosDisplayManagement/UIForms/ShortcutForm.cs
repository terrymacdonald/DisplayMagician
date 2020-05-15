using System;
using System.Collections.Generic;
using System.Drawing.IconLib;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HeliosPlus.Resources;
using HeliosPlus.Shared;
using HeliosPlus.GameLibraries;
using System.Globalization;

namespace HeliosPlus.UIForms
{
    public partial class ShortcutForm : Form
    {

        List<SteamGame> _allSteamGames;
        internal Profile[] _allProfiles; 

        public ShortcutForm()
        {
            InitializeComponent();
            
        }

        public ShortcutForm(Profile profile) : this()
        {
            SelectedProfile = profile;
        }

        public string ProcessNameToMonitor
        {
            get
            {
                if (rb_switch_temp.Checked && rb_standalone.Checked) {
                    if (rb_wait_executable.Checked)
                    {
                        return txt_process_name.Text;
                    } 
                }
                return string.Empty;
            }
            set
            {
                // We we're setting this entry, then we want to set it to a particular entry
                txt_process_name.Text = value;
                rb_wait_executable.Checked = true;
            }
        }

        public Profile SelectedProfile
        {
            get => dv_profile.Profile;
            set
            {
                // Check the profile is valid
                // Create an array of display profiles we have
                var profiles = Profile.LoadAllProfiles().ToArray();
                _allProfiles = profiles;
                // Check if the user supplied a --profile option using the profiles' ID
                var profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Id.Equals(value.Id, StringComparison.InvariantCultureIgnoreCase)) : -1;
                // If the profileID wasn't there, maybe they used the profile name?
                if (profileIndex == -1)
                {
                    // Try and lookup the profile in the profiles' Name fields
                    profileIndex = profiles.Length > 0 ? Array.FindIndex(profiles, p => p.Name.StartsWith(value.Name, StringComparison.InvariantCultureIgnoreCase)) : -1;
                }
                // If the profileID still isn't there, then raise the alarm
                if (profileIndex == -1)
                {
                    MessageBox.Show(
                        $"ShortcutForm: Setting SelectProfile: Couldn't find either Profile Name '{SelectedProfile.Name}'or ID '{SelectedProfile.Id}' supplied to Profile property.",
                        Language.Executable,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
                dv_profile.Profile = value;
            }
        }

        public string ExecutableNameAndPath
        {
            get => rb_switch_temp.Checked && rb_launcher.Checked ? txt_executable.Text : string.Empty;
            set
            {
                if (File.Exists(txt_executable.Text))
                {
                    rb_switch_temp.Checked = true;
                    rb_launcher.Checked = true;
                    txt_executable.Text = value;
                }
            }
        }

        public uint ExecutableTimeout
        {
            get
            {

                if (rb_wait_executable.Checked)
                {
                    return (uint)nud_timeout_executable.Value;
                }

                return 0;
            }
            set
            {
                nud_timeout_executable.Value = value;
            }
        }

        public string ExecutableArguments
        {
            get => cb_args_executable.Checked ? txt_args_executable.Text : string.Empty;
            set
            {
                txt_args_executable.Text = value;
                cb_args_executable.Checked = true;
            }
        }


        public uint GameAppId
        {
            get => rb_switch_temp.Checked && rb_launcher.Checked ? (uint) Convert.ToInt32(txt_game_id.Text) : 0;
            set
            {
                rb_switch_temp.Checked = true;
                rb_launcher.Checked = true;
                txt_game_id.Text = value.ToString();
            }
        }

        public string GameName
        {
            get => rb_switch_temp.Checked && rb_launcher.Checked ? txt_game_name.Text : string.Empty;
            set
            {
                rb_switch_temp.Checked = true;
                rb_launcher.Checked = true;
                txt_game_name.Text = value;
            }
        }

        public SupportedGameLibrary GameLibrary
        {
            get
            {
                if (rb_switch_temp.Checked && rb_launcher.Checked)
                {
                    if (txt_game_launcher.Text.Contains("Steam"))
                    {
                        return SupportedGameLibrary.Steam;
                    }
                    else if (txt_game_launcher.Text.Contains("Uplay"))
                    {
                        return SupportedGameLibrary.Uplay;
                    }

                }
                return SupportedGameLibrary.Unknown;
            }
            set
            {
                rb_switch_temp.Checked = true;
                rb_launcher.Checked = true;
                switch (value)
                {
                    case SupportedGameLibrary.Steam:
                        txt_game_launcher.Text = Enum.GetName(typeof(SupportedGameLibrary), SupportedGameLibrary.Steam);
                        break;

                    case SupportedGameLibrary.Uplay:
                        txt_game_launcher.Text = Enum.GetName(typeof(SupportedGameLibrary), SupportedGameLibrary.Uplay);
                        break;

                }
                // TODO - If SupportedGameLibrary.Unknown; then we need to show an error message.

            }
        }


        public uint GameTimeout
        {
            get
            {
                if (rb_switch_temp.Checked && rb_launcher.Checked)
                {
                    return (uint)nud_timeout_game.Value;
                }
                return 0;
            }
            set
            {
                nud_timeout_game.Value = value;
            }
        }

        public string GameArguments
        {
            get => cb_args_game.Checked ? txt_args_game.Text : string.Empty;
            set
            {
                txt_args_game.Text = value;
                cb_args_game.Checked = true;
            }
        }

        private static bool IsLowQuality(IconImage iconImage)
        {
            return iconImage.PixelFormat == System.Drawing.Imaging.PixelFormat.Format1bppIndexed ||
                    iconImage.PixelFormat == System.Drawing.Imaging.PixelFormat.Format4bppIndexed ||
                    iconImage.PixelFormat == System.Drawing.Imaging.PixelFormat.Format8bppIndexed;
        }

        private static Bitmap ExtractVistaIcon(Icon icoIcon)
        {
            Bitmap bmpPngExtracted = null;
            try
            {
                byte[] srcBuf = null;
                using (System.IO.MemoryStream stream = new System.IO.MemoryStream())
                { icoIcon.Save(stream); srcBuf = stream.ToArray(); }
                const int SizeICONDIR = 6;
                const int SizeICONDIRENTRY = 16;
                int iCount = BitConverter.ToInt16(srcBuf, 4);
                for (int iIndex = 0; iIndex < iCount; iIndex++)
                {
                    int iWidth = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex];
                    int iHeight = srcBuf[SizeICONDIR + SizeICONDIRENTRY * iIndex + 1];
                    int iBitCount = BitConverter.ToInt16(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 6);
                    if (iWidth == 0 && iHeight == 0 && iBitCount == 32)
                    {
                        int iImageSize = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 8);
                        int iImageOffset = BitConverter.ToInt32(srcBuf, SizeICONDIR + SizeICONDIRENTRY * iIndex + 12);
                        System.IO.MemoryStream destStream = new System.IO.MemoryStream();
                        System.IO.BinaryWriter writer = new System.IO.BinaryWriter(destStream);
                        writer.Write(srcBuf, iImageOffset, iImageSize);
                        destStream.Seek(0, System.IO.SeekOrigin.Begin);
                        bmpPngExtracted = new Bitmap(destStream); // This is PNG! :)
                        break;
                    }
                }
            }
            catch { return null; }
            return bmpPngExtracted;
        }

        private static string GetValidFilename(string uncheckedFilename)
        {
            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                uncheckedFilename = uncheckedFilename.Replace(c.ToString(), "");
            }
            return uncheckedFilename;
        }



        private void btn_app_executable_Click(object sender, EventArgs e)
        {
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(dialog_open.FileName) && Path.GetExtension(dialog_open.FileName) == @".exe")
                {
                    txt_executable.Text = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        Language.Selected_file_is_not_a_valid_executable_file,
                        Language.Executable,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.None;

            try
            {
                // Set the Shortcut save folder to the Desktop as that's where people will want it most likely
                dialog_save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                // Try to set up some sensible suggestions for the Shortcut name
                if (rb_switch_perm.Checked)
                {
                    
                    dialog_save.FileName = SelectedProfile.Name;
                } 
                else
                {
                    if (rb_standalone.Checked)
                    {
                        dialog_save.FileName = String.Concat(Path.GetFileNameWithoutExtension(ExecutableNameAndPath),@" (", SelectedProfile.Name.ToLower(), @")");
                    }
                    else
                    {
                        dialog_save.FileName = String.Concat(GameName, @" (", SelectedProfile.Name, @")");
                    }
                }

                // Show the Save Shortcut window
                if (dialog_save.ShowDialog(this) == DialogResult.OK)
                {
                    if (CreateShortcut(dialog_save.FileName))
                    {
                        MessageBox.Show(
                            Language.Shortcut_placed_successfully,
                            Language.Shortcut,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show(
                            Language.Failed_to_create_the_shortcut_Unexpected_exception_occurred,
                            Language.Shortcut,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                    }

                    dialog_save.FileName = string.Empty;
                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Language.Shortcut, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ReSharper disable once FunctionComplexityOverflow
        // ReSharper disable once CyclomaticComplexity
        private bool CreateShortcut(string fileName)
        {
            string programName = Path.GetFileNameWithoutExtension(txt_executable.Text);
            string shortcutDescription = string.Empty;
            MultiIcon shortcutIcon;
            string shortcutIconFileName = string.Empty;

            var args = new List<string>
            {
                // Add the SwitchProfile command as the first argument to start to switch to another profile
                $"{HeliosStartupAction.SwitchProfile}"
            };

            // Only add the rest of the options if the temporary switch radio button is set
            if (rb_switch_temp.Checked)
            {
                // Only add this set of options if the standalone programme radio button is set
                if (rb_standalone.Checked)
                {
                    // Doublecheck the Executable text field is filled in
                    if (string.IsNullOrWhiteSpace(ExecutableNameAndPath))
                    {
                        throw new Exception(Language.Executable_address_can_not_be_empty);
                    }

                    // Doublecheck the Executable text field is a path to a real file
                    if (!File.Exists(ExecutableNameAndPath))
                    {
                        throw new Exception(Language.Executable_file_not_found);
                    }

                    // Add the executable command and the executable name to the shortcut arguments
                    args.Add($"execute \"{ExecutableNameAndPath}\"");

                    // Add the Profile Name as the first option (use that rather than ID - though ID still will work!)
                    args.Add($"--profile \"{SelectedProfile.Name}\"");

                    // Check that the wait for executable radiobutton is on
                    if (rb_wait_executable.Checked)
                    {
                        // Doublecheck the process name has text in it
                        if (!string.IsNullOrWhiteSpace(ProcessNameToMonitor))
                        {
                            // Add the waitfor argument and the process name to the shortcut arguments
                            args.Add($"--waitfor \"{ProcessNameToMonitor}\"");
                        }
                    }

                    // Add the timeout argument and the timeout duration in seconds to the shortcut arguments
                    args.Add($"--timeout {ExecutableTimeout}");

                    if (cb_args_executable.Checked)
                    {
                        args.Add($"--arguments \"{ExecutableArguments}\"");
                    }

                    // Prepare text for the shortcut description field
                    shortcutDescription = string.Format(Language.Executing_application_with_profile, programName, SelectedProfile.Name);

                    // Work out the name of the shortcut we'll save.
                    shortcutIconFileName = Path.Combine(Program.ShortcutIconCachePath, String.Concat(@"executable-", GetValidFilename(SelectedProfile.Name).ToLower(CultureInfo.InvariantCulture), "-", Path.GetFileNameWithoutExtension(ExecutableNameAndPath), @".ico"));

                    // Grab an icon for the selected executable
                    try
                    {
                        // We'll first try to extract the Icon from the executable the user provided
                        //shortcutIcon.Load(ExecutableNameAndPath);
                    }
                    catch (Exception)
                    {
                        // but if that doesn't work, then we use our own one.
                        //shortcutIcon.Load(Assembly.GetExecutingAssembly().Location);
                    }
                }
                // Only add the rest of the options if the temporary switch radio button is set
                // and if the game launching radio button is set
                else if (rb_launcher.Checked)
                {
                    // TODO need to make this work so at least one game library is installed
                    // i.e. if (!SteamGame.SteamInstalled && !UplayGame.UplayInstalled )
                    if (GameLibrary == SupportedGameLibrary.Steam)
                    {
                        if (!SteamGame.SteamInstalled)
                        {
                            throw new Exception(Language.Steam_is_not_installed);
                        }

                        List<SteamGame> allSteamGames = SteamGame.GetAllInstalledGames();

                        SteamGame steamGameToRun = null;
                        foreach (SteamGame steamGameToCheck in allSteamGames)
                        {
                            if (steamGameToCheck.GameId == GameAppId)
                            {
                                steamGameToRun = steamGameToCheck;
                                break;
                            }

                        }


                        // Work out the name of the shortcut we'll save.
                        shortcutIconFileName = Path.Combine(Program.ShortcutIconCachePath, String.Concat(@"steam-", GetValidFilename(SelectedProfile.Name).ToLower(CultureInfo.InvariantCulture), "-", GameAppId.ToString(), @".ico"));

                        shortcutIcon = new ProfileIcon(SelectedProfile).ToIconOverly(steamGameToRun.GameIconPath);
                        shortcutIcon.Save(shortcutIconFileName, MultiIconFormat.ICO);

                        args.Add($"steam {GameAppId}");

                    }
                    else if (GameLibrary == SupportedGameLibrary.Uplay)
                    {

                        if (!UplayGame.UplayInstalled)
                        {
                            throw new Exception(Language.Steam_is_not_installed);
                        }

                        List<UplayGame> allUplayGames = UplayGame.GetAllInstalledGames();

                        UplayGame uplayGameToRun = null;
                        foreach (UplayGame uplayGameToCheck in allUplayGames)
                        {
                            if (uplayGameToCheck.GameId == GameAppId)
                            {
                                uplayGameToRun = uplayGameToCheck;
                                break;
                            }

                        }

                        //shortcutIcon = uplayGameToRun.GameIcon;

                        // Work out the name of the shortcut we'll save.
                        shortcutIconFileName = Path.Combine(Program.ShortcutIconCachePath, String.Concat(@"uplay-", GetValidFilename(SelectedProfile.Name).ToLower(CultureInfo.InvariantCulture), "-" , GameAppId.ToString(), @".ico"));

                        shortcutIcon = new ProfileIcon(SelectedProfile).ToIconOverly(uplayGameToRun.GameIconPath);
                        shortcutIcon.Save(shortcutIconFileName, MultiIconFormat.ICO);

                        args.Add($"uplay {GameAppId}");

                    }

                    // Add the Profile Name as the first option (use that rather than ID - though ID still will work!)
                    args.Add($"--profile \"{SelectedProfile.Name}\"");

                    // Add the game timeout argument and the timeout duration in seconds to the shortcut arguments
                    args.Add($"--timeout {GameTimeout}");

                    if (cb_args_game.Checked)
                    {
                        args.Add($"--arguments \"{GameArguments}\"");
                    }

                    // Prepare text for the shortcut description field
                    shortcutDescription = string.Format(Language.Executing_application_with_profile, GameName, SelectedProfile.Name);

                }

            }
            // Only add the rest of the options if the permanent switch radio button is set
            else
            {
                // Add the action switch to make the permanent switch to a different profile
                args.Add($"permanent");
                
                // Add the Profile Name as the first option (use that rather than ID - though ID still will work!)
                args.Add($"--profile \"{SelectedProfile.Name}\"");

                // Prepare text for the shortcut description field
                shortcutDescription = string.Format(Language.Switching_display_profile_to_profile, SelectedProfile.Name);

                // Work out the name of the shortcut we'll save.
                shortcutIconFileName = Path.Combine(Program.ShortcutIconCachePath, String.Concat(@"permanent-", GetValidFilename(SelectedProfile.Name).ToLower(CultureInfo.InvariantCulture), @".ico"));

                // Grab an icon for the selected profile
                try
                {
                    shortcutIcon = new ProfileIcon(SelectedProfile).ToIcon();
                    shortcutIcon.Save(shortcutIconFileName, MultiIconFormat.ICO);
                    
                }
                catch
                {
                    // but if that doesn't work, then we use our own one.
                    shortcutIcon = new ProfileIcon(SelectedProfile).ToIconOverly(Assembly.GetExecutingAssembly().Location);
                    shortcutIcon.Save(shortcutIconFileName, MultiIconFormat.ICO);
                }
            }

            // Now we are ready to create a shortcut based on the filename the user gave us
            fileName = Path.ChangeExtension(fileName, @"lnk");

            // If the user supplied a file
            if (fileName != null)
            {
                try
                {
                    // Remove the old file to replace it
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }

                    // Actually create the shortcut!
                    var wshShellType = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
                    dynamic wshShell = Activator.CreateInstance(wshShellType);

                    try
                    {
                        var shortcut = wshShell.CreateShortcut(fileName);

                        try
                        {
                            shortcut.TargetPath = Application.ExecutablePath;
                            shortcut.Arguments = string.Join(" ", args);
                            shortcut.Description = shortcutDescription;
                            shortcut.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) ??
                                                        string.Empty;

                            shortcut.IconLocation = shortcutIconFileName;

                            shortcut.Save();
                        }
                        finally
                        {
                            Marshal.FinalReleaseComObject(shortcut);
                        }
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(wshShell);
                    }
                }
                catch
                {
                    // Clean up a failed attempt
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
            }

            // Return a status on how it went
            // true if it was a success or false if it was not
            return fileName != null && File.Exists(fileName);
        }

        private void txt_executable_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(txt_executable.Text))
            {

                // Turn on the CreateShortcut Button
                btn_save.Enabled = true;

                // Try and discern the process name for this
                // if the user hasn't entered anything already
                if (txt_process_name.Text == String.Empty)
                {
                    try
                    {
                        txt_process_name.Text = Path.GetFileNameWithoutExtension(txt_executable.Text)?.ToLower() ?? txt_process_name.Text;

                    }
                    catch
                    {
                        // ignored
                    }
                }

            } else
            {
                // Turn off the CreateShortcut Button
                btn_save.Enabled = false;
            }
        }

        private void rb_switch_perm_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_perm.Checked)
            {
                // Disable the Temporary Group
                g_temporary.Enabled = false;
            }
            // Turn on the CreateShortcut Button
            btn_save.Enabled = true;

        }
        private void rb_switch_temp_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_temp.Checked)
            {
                // Enable the Temporary Group
                g_temporary.Enabled = true;

                // If it's been set already to a valid gamelauncher, then enable the button
                if (txt_game_launcher.Text.Length > 0 && txt_game_id.Text.Length > 0 && txt_game_name.Text.Length > 0)
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = true;
                }
                // else if it's a valid executable
                else if (txt_executable.Text.Length > 0 && File.Exists(txt_executable.Text))
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = true;
                }
                else
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = false;
                }

            }
            
        }

        private void rb_standalone_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_standalone.Checked)
            {
                // Enable the Standalone Panel
                p_standalone.Enabled = true;
                // Disable the Game Panel
                p_game.Enabled = false;

                if (txt_executable.Text.Length > 0 && File.Exists(txt_executable.Text))
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = true;
                }
                else
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = false;
                }
            }
           
        }

        private void rb_launcher_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_launcher.Checked)
            {
                // Enable the Game Panel
                p_game.Enabled = true;
                // Disable the Standalone Panel
                p_standalone.Enabled = false;

                // If it's been set already to a valid gamelauncher, then enable the button
                if (txt_game_launcher.Text.Length > 0 && txt_game_id.Text.Length > 0 && txt_game_name.Text.Length > 0)
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = true;
                }
                else
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = false;
                }

            }
        }


        private void cb_args_executable_CheckedChanged(object sender, EventArgs e)
        {
            // Disable the Process Name Text field
            if (cb_args_executable.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_args_executable.Enabled = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_args_executable.Enabled = false;
            }

        }

        private async void ShortcutForm_Load(object sender, EventArgs e)
        {

            // Set the Profile name
            lbl_profile.Text = $"Selected Profile: {dv_profile.Profile?.Name ?? Language.None}";

            /*// Reload the profiles in case we swapped to another program to change it
            ReloadProfiles();
            // If nothing is selected then select the currently used profile
            if (lv_profiles.SelectedItems.Count == 0)
            {
                lv_profiles.Items[0].Selected = true;
            }
*/
            // Start finding the games and loading the Games ListView
            List<SteamGame> allSteamGames = SteamGame.GetAllInstalledGames();
            _allSteamGames = allSteamGames;
            foreach (var game in allSteamGames.OrderBy(game => game.GameName))
            {
                if (File.Exists(game.GameIconPath))
                {
                    try
                    {
                        if (game.GameIconPath.EndsWith(".ico"))
                        {
                            // if it's an icon try to load it as a bitmap
                            il_games.Images.Add(Image.FromFile(game.GameIconPath));
                        }
                        else if (game.GameIconPath.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase) || game.GameIconPath.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
                        {
                            // otherwise use IconExtractor
                            /*IconExtractor IconEx = new IconExtractor(game.GameIconPath);
                            Icon icoAppIcon = IconEx.GetIcon(0); // Because standard System.Drawing.Icon.ExtractAssociatedIcon() returns ONLY 32x32.*/

                            Icon icoAppIcon = Icon.ExtractAssociatedIcon(game.GameIconPath);
                            // We first try high quality icons
                            Bitmap extractedBitmap = ExtractVistaIcon(icoAppIcon);
                            if (extractedBitmap == null)
                                extractedBitmap = icoAppIcon.ToBitmap();
                            il_games.Images.Add(extractedBitmap);
                        }
                    } 
                    catch (Exception)
                    {
                        il_games.Images.Add(Image.FromFile("Resources/Steam.ico"));
                    }
                } else
                {
                    //(Icon)global::Calculate.Properties.Resources.ResourceManager.GetObject("Steam.ico");
                    il_games.Images.Add(Image.FromFile("Resources/Steam.ico"));
                }

                if (!Visible)
                {
                    return;
                }

                lv_games.Items.Add(new ListViewItem
                {
                    Text = game.GameName,
                    Tag = game,
                    ImageIndex = il_games.Images.Count - 1
                });
            }
        }

        private void rb_wait_process_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_wait_process.Checked)
            {
                // Enable the Process Name Text field
                txt_process_name.Enabled = true;
            } else
            {
                txt_process_name.Enabled = false;
            }
        }

        private void rb_wait_executable_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_wait_executable.Checked)
            {
                // Disable the Process Name Text field
                txt_process_name.Enabled = false;
            } else
            {
                txt_process_name.Enabled = true;
            }

        }


        private void btn_app_process_Click(object sender, EventArgs e)
        {
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(dialog_open.FileName) && Path.GetExtension(dialog_open.FileName) == @".exe")
                {
                    txt_process_name.Text = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        Language.Selected_file_is_not_a_valid_executable_file,
                        Language.Executable,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }

        private void cb_args_game_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_args_game.Checked)
            {
                txt_args_game.Enabled = true;
            } else
            {
                txt_args_game.Enabled = false;
            }
        }

        private void btn_choose_game_Click(object sender, EventArgs e)
        {
            if (lv_games.SelectedItems.Count > 0)
            {
                txt_game_name.Text = lv_games.SelectedItems[0].Text;
                foreach (SteamGame game in _allSteamGames)
                {
                    if (game.GameName == txt_game_name.Text)
                    {
                        txt_game_launcher.Text = SteamGame.GameLibrary.ToString();
                        txt_game_id.Text = game.GameId.ToString();
                    }
                }

                // Turn on the CreateShortcut Button
                btn_save.Enabled = true;
            }
            
        }

        /*private void RefreshProfilesStatus()
        {
            Profile.RefreshActiveStatus();
            lv_profiles.Invalidate();
        }

        private void ReloadProfiles()
        {
            Profile.RefreshActiveStatus();
            var profiles = Profile.GetAllProfiles().ToArray();
            lv_profiles.Items.Clear();
            il_profiles.Images.Clear();

            if (!profiles.Any(profile => profile.IsActive))
            {
                AddProfile().Selected = true;
            }

            foreach (var profile in profiles)
            {
                AddProfile(profile);
            }

            lv_profiles.SelectedIndices.Clear();
            lv_profiles.Invalidate();
        }
*/
        private void cb_selected_profile_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (Profile profile in _allProfiles)
            {
                if (SelectedProfile.Name == cb_selected_profile.SelectedItem.ToString())
                {
                    SelectedProfile = profile;
                }
            }
            
        }
    }
}