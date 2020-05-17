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
using Manina.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace HeliosPlus.UIForms
{
    public partial class ShortcutForm : Form
    {

        List<SteamGame> _allSteamGames;
        private ProfileAdaptor _profileAdaptor;
        private static bool _inDialog = false;
        private List<Profile> _loadedProfiles = new List<Profile>();
        private Profile _profileToUse= null;
        private Shortcut _shortcutToEdit = null;
        private string _saveOrRenameMode = "save";
        private bool _isNewShortcut = true;

        public ShortcutForm()
        {
            InitializeComponent();

            // Set the profileAdaptor we need to load images from Profiles
            // into the Profiles ImageListView
            _profileAdaptor = new ProfileAdaptor();

        }

        public ShortcutForm(Shortcut shortcutToEdit) : this()
        {
            _shortcutToEdit = shortcutToEdit;
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

        public Shortcut Shortcut
        {
            get => _shortcutToEdit;
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
            // Store all of the information in the Shortcut object based on what's been selected in this form

            // Validate the fields are filled as they should be!
            // Check the name is valid
            if (String.IsNullOrWhiteSpace(txt_shortcut_save_name.Text) && Program.IsValidFilename(txt_shortcut_save_name.Text))
            {
                MessageBox.Show(
                    @"You need to specify a name for this Shortcut before it can be saved.",
                    @"Please name this Shortcut.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }


            // Check the profile is set and that it's still valid
            if (!(_profileToUse is Profile))
            {
                MessageBox.Show(
                    @"You need to select a Display Profile to use with this shortcut. Please select one from the list of Display Profiles on the left of the screen.",
                    @"Please choose a Display Profile.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            // Check the permanence requirements
            if (rb_switch_perm.Checked)
            { 
                // Check the Shortcut Category to see if it's application
                if (rb_standalone.Checked)
                {
                    if (cb_args_executable.Checked && String.IsNullOrWhiteSpace(txt_args_executable.Text))
                    {
                        MessageBox.Show(
                            @"If you have chosen to pass extra arguments to the executable when it is run, then you need to enter them in the 'Pass arguments to Executable' field. If you didn't want to pass extra arguments then please uncheck the 'Pass arguments to Executable' checkbox.",
                            @"Please add Executable arguments.",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        return;

                    }

                    if (!File.Exists(txt_executable.Text))
                    {
                        MessageBox.Show(
                            @"The executable you have chosen does not exist! Please reselect the executable, or check you have persmissions to view it.",
                            @"Executable doesn't exist",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        return;
                    }

                    if (String.IsNullOrWhiteSpace(txt_process_name.Text))
                    {
                        string message = "";

                        // figure out the message we want to give the user
                        if (_shortcutToEdit.ProcessNameToMonitorUsesExecutable)
                            message = @"Cannot work out the process to monitor from the executable. Please reselect the executable (and we'll try again), and if that doesn't work then manually enter the process name into the 'Process to monitor' field.";
                        else
                            message = @"Please manually enter the process name into the 'Process to monitor' field.";

                        // show the error message
                        MessageBox.Show(
                            message,
                            @"Empty process monitor",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        return;
                    }
                }
                else if (rb_switch_temp.Checked)
                {

                    if (cb_args_game.Checked && String.IsNullOrWhiteSpace(txt_args_game.Text))
                    {
                        MessageBox.Show(
                            @"If you have chosen to pass extra arguments to the Game when it is run, then you need to enter them in the 'Pass arguments to Game' field. If you didn't want to pass extra arguments then please uncheck the 'Pass arguments to Game' checkbox.",
                            @"Please add Game arguments.",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        return;
                    }

                    if (Convert.ToUInt32(txt_game_id.Text) == 0)
                    {
                        MessageBox.Show(
                            @"Please choose a Game by scrolling through the list, selecting the Game that you want, and then clicking the '>>' button to fill the Game fields.",
                            @"Please choose a Game.",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        return;
                    }
                }
            }

            // Fill the Shortcut object with the bits we care about saving

            // Update the Executable args
            _shortcutToEdit.ExecutableArguments = txt_args_executable.Text;

            // Update if the executable args are needed
            _shortcutToEdit.ExecutableArgumentsRequired = cb_args_executable.Checked;

            // Update the Executable name and path
            _shortcutToEdit.ExecutableNameAndPath = txt_executable.Text;

            // Update the executable timeout
            _shortcutToEdit.ExecutableTimeout = Convert.ToUInt32(nud_timeout_executable.Value);

            // Update the game app id
            _shortcutToEdit.GameAppId = Convert.ToUInt32(txt_game_id.Text);

            // Update the game args
            _shortcutToEdit.GameArguments = txt_args_game.Text;

            // Update if the game args are needed
            _shortcutToEdit.GameArgumentsRequired = cb_args_game.Checked;

            // Update what game library it's from
            //_shortcutToEdit.GameLibrary = SupportedGameLibrary.Steam;

            // Update the Game Name
            _shortcutToEdit.GameName = txt_game_name.Text;

            // Update the Game Timeout
            _shortcutToEdit.GameTimeout = Convert.ToUInt32(nud_timeout_game.Value);

            // Update the Shortcut name
            _shortcutToEdit.Name = txt_shortcut_save_name.Text;

            // Check the permanence requirements
            if (rb_switch_temp.Checked)
                _shortcutToEdit.Permanence = ShortcutPermanence.Temporary;

            if (rb_switch_perm.Checked)
                _shortcutToEdit.Permanence = ShortcutPermanence.Permanent;

            // Update the process name to monitor
            if (!String.IsNullOrWhiteSpace(txt_process_name.Text)) {
                _shortcutToEdit.ProcessNameToMonitor = txt_process_name.Text;
            }

            if (rb_wait_process.Checked && !String.IsNullOrWhiteSpace(txt_process_name.Text))
            {
                _shortcutToEdit.ProcessNameToMonitorUsesExecutable = true;
                _shortcutToEdit.ProcessNameToMonitor = txt_process_name.Text;
            }
            else
            {
                _shortcutToEdit.ProcessNameToMonitorUsesExecutable = false;
            }

            // Update the profile to use
            _shortcutToEdit.ProfileToUse = _profileToUse;

            // Update the Category as well as the OriginalIconPath
            // (as we need the OriginalIconPath to run the SaveShortcutIconToCache method)
            if (rb_launcher.Checked)
                _shortcutToEdit.Category = ShortcutCategory.Game;
            if (txt_game_launcher.Text == SupportedGameLibrary.Steam.ToString())
                _shortcutToEdit.OriginalIconPath = (from steamGame in SteamGame.AllGames where steamGame.GameId == _shortcutToEdit.GameAppId select steamGame.GameIconPath).First();
            else if (txt_game_launcher.Text == SupportedGameLibrary.Uplay.ToString())
                _shortcutToEdit.OriginalIconPath = (from uplayGame in UplayGame.AllGames where uplayGame.GameId == _shortcutToEdit.GameAppId select uplayGame.GameIconPath).First();
            else if (rb_standalone.Checked)
                _shortcutToEdit.Category = ShortcutCategory.Application;

            // Save the shortcut icon
            _shortcutToEdit.SaveShortcutIconToCache();

            // Add the Shortcut to the list of saved Shortcuts so it gets saved for later
            // but only if it's new... if it is an edit then it will already be in the list.
            if (_isNewShortcut)
                Shortcut.AllSavedShortcuts.Add(_shortcutToEdit);

            // Save everything is golden and close the form.
            DialogResult = DialogResult.OK;
            this.Close();
        }

        

        private void txt_executable_TextChanged(object sender, EventArgs e)
        {
            if (File.Exists(txt_executable.Text))
            {

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

                if (txt_shortcut_save_name.Text.Length > 0)
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = true;
                    btn_save.Visible = true;
                }
                else
                {
                    btn_save.Enabled = false;
                    btn_save.Visible = false;
                }

            }
            else
            {
                // Turn off the CreateShortcut Button
                btn_save.Enabled = false;
                btn_save.Visible = false;
            }
        }

        private void rb_switch_perm_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_perm.Checked)
            {
                // Disable the Temporary Group
                g_temporary.Enabled = false;
            }
            if (txt_shortcut_save_name.Text.Length > 0)
            {
                // Turn on the CreateShortcut Button
                btn_save.Enabled = true;
                btn_save.Visible = true;
            }
            else
            {
                btn_save.Enabled = false;
                btn_save.Visible = false;
            }

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
                    btn_save.Visible = true;
                }
                // else if it's a valid executable
                else if (txt_executable.Text.Length > 0 && File.Exists(txt_executable.Text) && txt_shortcut_save_name.Text.Length > 0)
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = true;
                    btn_save.Visible = true;
                }
                else
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = false;
                    btn_save.Visible = false;
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

                if (txt_executable.Text.Length > 0 && File.Exists(txt_executable.Text) && txt_shortcut_save_name.Text.Length > 0)
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = true;
                    btn_save.Visible = true;
                }
                else
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = false;
                    btn_save.Visible = false;
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
                if (txt_game_launcher.Text.Length > 0 && txt_game_id.Text.Length > 0 && txt_game_name.Text.Length > 0 && txt_shortcut_save_name.Text.Length > 0)
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = true;
                    btn_save.Visible = true;
                }
                else
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = false;
                    btn_save.Visible = false;
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

        private void RefreshImageListView(Profile profile)
        {
            ilv_saved_profiles.ClearSelection();
            IEnumerable<ImageListViewItem> matchingImageListViewItems = (from item in ilv_saved_profiles.Items where item.Text == profile.Name select item);
            if (matchingImageListViewItems.Any())
            {
                matchingImageListViewItems.First().Selected = true;
                matchingImageListViewItems.First().Focused = true;
            }

        }

        private async void ShortcutForm_Load(object sender, EventArgs e)
        {

            // Create a new SHortcut if we are creating a new one
            // And set up the page.
            if (_shortcutToEdit == null)
            {
                _shortcutToEdit = new Shortcut();
                _isNewShortcut = true;
                _saveOrRenameMode = "save";
                btn_save_or_rename.Text = "Save As";
            }
            else
            {
                _isNewShortcut = false;
                _saveOrRenameMode = "rename";
                btn_save_or_rename.Text = "Rename To";
            }

            // Load all the profiles to prepare things
            _loadedProfiles = (List<Profile>)Profile.LoadAllProfiles();

            bool foundCurrentProfileInLoadedProfiles = false;
            foreach (Profile loadedProfile in _loadedProfiles)
            {
                if (Profile.CurrentProfile.Equals(loadedProfile))
                {
                    // We have already saved the selected profile!
                    // so we need to show the selected profile 
                    ChangeSelectedProfile(loadedProfile);
                    foundCurrentProfileInLoadedProfiles = true;
                }

            }

            // If we get to the end of the loaded profiles and haven't
            // found a matching profile, then we need to show the current
            // Profile
            if (!foundCurrentProfileInLoadedProfiles)
                ChangeSelectedProfile(Profile.CurrentProfile);

            // Refresh the Shortcut UI
            RefreshShortcutUI();

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
                            Bitmap extractedBitmap = Shortcut.ExtractVistaIcon(icoAppIcon);
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

                if (txt_shortcut_save_name.Text.Length > 0)
                {
                    // Turn on the CreateShortcut Button
                    btn_save.Enabled = true;
                    btn_save.Visible = true;
                }
            }
            
        }

        private void ilv_saved_profiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            foreach (Profile loadedProfile in _loadedProfiles)
            {
                if (loadedProfile.Name == e.Item.Text)
                {
                    ChangeSelectedProfile(loadedProfile);
                }
            }

        }

        private void ChangeSelectedProfile(Profile profile)
        {
            // If the profile is null then return
            // (this happens when a new blank shortcut is created
            if (profile == null)
                return;

            // And we need to update the actual selected profile too!
            _profileToUse = profile;

            // We also need to load the saved profile name to show the user
            lbl_profile_shown.Text = _profileToUse.Name;

            if (_profileToUse.Equals(Profile.CurrentProfile))
            {
                lbl_profile_shown_subtitle.Text = "(Current Display Profile in use)";
                btn_save.Visible = false;
            }
            else
            {
                if (!_profileToUse.IsPossible)
                {
                    lbl_profile_shown_subtitle.Text = "(Display Profile is not valid so cannot be used)";
                    btn_save.Visible = false;
                }
                else
                {
                    lbl_profile_shown_subtitle.Text = "";
                    btn_save.Visible = true;
                }
            }
            // Refresh the image list view
            RefreshImageListView(profile);

            // And finally show the profile in the display view
            dv_profile.Profile = profile;
            dv_profile.Refresh();

        }

        private void RefreshShortcutUI()
        {

            if (!_inDialog)
            {
                
                if (_loadedProfiles.Count > 0)
                {

                    // Temporarily stop updating the saved_profiles listview
                    ilv_saved_profiles.SuspendLayout();

                    ImageListViewItem newItem = null;
                    bool foundCurrentProfileInLoadedProfiles = false;
                    foreach (Profile loadedProfile in _loadedProfiles)
                    {
                        bool thisLoadedProfileIsAlreadyHere = (from item in ilv_saved_profiles.Items where item.Text == loadedProfile.Name select item.Text).Any();
                        if (!thisLoadedProfileIsAlreadyHere)
                        {
                            //loadedProfile.SaveProfileImageToCache();
                            //newItem = new ImageListViewItem(loadedProfile.SavedProfileCacheFilename, loadedProfile.Name);
                            //newItem = new ImageListViewItem(loadedProfile, loadedProfile.Name);
                            newItem = new ImageListViewItem(loadedProfile, loadedProfile.Name);
                            //ilv_saved_profiles.Items.Add(newItem);
                            ilv_saved_profiles.Items.Add(newItem, _profileAdaptor);
                        }

                    }

                    // If we get to the end of the loaded profiles and haven't
                    // found a matching profile, then we need to show the current
                    // Profile
                    if (!foundCurrentProfileInLoadedProfiles)
                        ChangeSelectedProfile(Profile.CurrentProfile);

                    // Check if we were loading a profile to edit
                    // If so, select that instead of all that other stuff above!
                    if (_shortcutToEdit != null)
                        ChangeSelectedProfile(_shortcutToEdit.ProfileToUse);

                    // Restart updating the saved_profiles listview
                    ilv_saved_profiles.ResumeLayout();
                }
                
            }
            else
                // Otherwise turn off the dialog mode we were just in
                _inDialog = false;
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void radiobutton_Paint(object sender, PaintEventArgs e)
        {
            
            base.OnPaint(e);

            RadioButton radiobutton = sender as RadioButton;

            if (!radiobutton.Enabled)
            {
                int x = ClientRectangle.X + CheckBoxRenderer.GetGlyphSize(
                    e.Graphics, CheckBoxState.UncheckedNormal).Width + 1;
                int y = ClientRectangle.Y + 2;

                TextRenderer.DrawText(e.Graphics, radiobutton.Text,
                    radiobutton.Font, new Point(x, y), Color.Gray,
                    TextFormatFlags.LeftAndRightPadding);
            }
            
        }

        private void checkbox_Paint(object sender, PaintEventArgs e)
        {

            base.OnPaint(e);

            CheckBox checkbox = sender as CheckBox;

            if (!checkbox.Enabled)
            {
                int x = ClientRectangle.X + CheckBoxRenderer.GetGlyphSize(
                    e.Graphics, CheckBoxState.UncheckedNormal).Width + 1;
                int y = ClientRectangle.Y + 1;

                TextRenderer.DrawText(e.Graphics, checkbox.Text,
                    checkbox.Font, new Point(x, y), Color.Gray,
                    TextFormatFlags.LeftAndRightPadding);
            }

        }

        private void label_Paint(object sender, PaintEventArgs e)
        {

            base.OnPaint(e);

            Label label = sender as Label;

            if (!label.Enabled)
            {
                int x = ClientRectangle.X - 2;
                int y = ClientRectangle.Y;

                TextRenderer.DrawText(e.Graphics, label.Text,
                    label.Font, new Point(x, y), Color.Gray,
                    TextFormatFlags.LeftAndRightPadding);
            }

        }

    }
}