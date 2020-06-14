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
        private List<ProfileItem> _loadedProfiles = new List<ProfileItem>();
        private ProfileItem _profileToUse= null;
        private ShortcutItem _shortcutToEdit = null;
        private bool _isNewShortcut = false;
        private bool _isUnsaved = false;
        private bool _saveNameAutomatic = true;
        private uint _gameId = 0;
        private uint _id = 0;

        public ShortcutForm()
        {
            InitializeComponent();

            // Set the profileAdaptor we need to load images from Profiles
            // into the Profiles ImageListView
            _profileAdaptor = new ProfileAdaptor();

            // Create a new SHortcut if we are creating a new one
            // And set up the page (otherwise this is all set when we load an
            // existing Shortcut)
            if (_shortcutToEdit == null)
            {
                _shortcutToEdit = new ShortcutItem();
                _isNewShortcut = true;
            }


        }

        public ShortcutForm(ShortcutItem shortcutToEdit) : this()
        {
            _shortcutToEdit = shortcutToEdit;
            _isNewShortcut = false;
        }

        public string ProcessNameToMonitor
        {
            get
            {
                if (rb_switch_temp.Checked && rb_standalone.Checked) {
                    if (rb_wait_executable.Checked)
                    {
                        return txt_alternative_executable.Text;
                    } 
                }
                return string.Empty;
            }
            set
            {
                // We we're setting this entry, then we want to set it to a particular entry
                txt_alternative_executable.Text = value;
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
            get => rb_switch_temp.Checked && rb_launcher.Checked ? _gameId : 0;
            set
            {
                rb_switch_temp.Checked = true;
                rb_launcher.Checked = true;
                _gameId = value;
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

        public ShortcutItem Shortcut
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


            // Check the name is valid to save
            if (String.IsNullOrWhiteSpace(txt_shortcut_save_name.Text))
            {
                MessageBox.Show(
                    @"You need to specify a name for this Shortcut before it can be saved.",
                    @"Please name this Shortcut.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            // Please use a plain name that can be
            if (_isNewShortcut && ShortcutRepository.ContainsShortcut(txt_shortcut_save_name.Text))
            {
                MessageBox.Show(
                    @"A shortcut has already been created with this name. Please enter a different name for this shortcut.",
                    @"Please rename this Shortcut.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

            // Check the profile is set and that it's still valid
            if (!(_profileToUse is ProfileItem))
            {
                MessageBox.Show(
                    @"You need to select a Display Profile to use with this shortcut. Please select one from the list of Display Profiles on the left of the screen.",
                    @"Please choose a Display Profile.",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                return;
            }

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
                        @"The executable you have chosen does not exist! Please reselect the executable, or check you have permissions to view it.",
                        @"Executable doesn't exist",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                if (rb_wait_alternative_executable.Checked && String.IsNullOrWhiteSpace(txt_alternative_executable.Text))
                {
                    MessageBox.Show(
                        $"If you want to wait for an alternative executable then you need to choose it! Click the 'Choose' button next to the different executable field.",
                        @"Need to choose the different executable",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                if (rb_wait_alternative_executable.Checked && !File.Exists(txt_alternative_executable.Text))
                {
                    MessageBox.Show(
                        @"The alternative executable you have chosen does not exist! Please reselect the alternative executable, or check you have permissions to view it.",
                        @"Alternative executable doesn't exist",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

            }
            else if (rb_launcher.Checked)
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

                if (_gameId == 0)
                {
                    MessageBox.Show(
                        @"Please choose a Game by scrolling through the list, selecting the Game that you want, and then clicking the '>>' button to fill the Game fields.",
                        @"Please choose a Game.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                bool gameStillInstalled = false;
                foreach (ListViewItem gameItem in lv_games.Items)
                {
                    if (gameItem.Text.Equals(txt_game_name.Text))
                        gameStillInstalled = true;
                }
                if (!gameStillInstalled)
                {
                    DialogResult result = MessageBox.Show(
                        $"This shortcut refers to the '{txt_game_name.Text}' game that was installed in your {txt_game_launcher.Text} library. This game is no longer installed, so the shortcut won't work. Do you still want to save the shortcut?",
                        @"Game no longer exists",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation);
                    if (result == DialogResult.No)
                        return;
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
            _shortcutToEdit.GameAppId = _gameId;

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

            if (rb_switch_permanent.Checked)
                _shortcutToEdit.Permanence = ShortcutPermanence.Permanent;

            // Update the process name to monitor
            if (!String.IsNullOrWhiteSpace(txt_alternative_executable.Text)) {
                _shortcutToEdit.DifferentExecutableToMonitor = txt_alternative_executable.Text;
            }

            if (rb_wait_alternative_executable.Checked && !String.IsNullOrWhiteSpace(txt_alternative_executable.Text))
            {
                _shortcutToEdit.ProcessNameToMonitorUsesExecutable = true;
                _shortcutToEdit.DifferentExecutableToMonitor = txt_alternative_executable.Text;
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
            {
                _shortcutToEdit.OriginalIconPath = (from steamGame in SteamGame.AllGames where steamGame.GameId == _shortcutToEdit.GameAppId select steamGame.GameIconPath).First();
                _shortcutToEdit.GameLibrary = SupportedGameLibrary.Steam;
            }
            else if (txt_game_launcher.Text == SupportedGameLibrary.Uplay.ToString())
            {
                _shortcutToEdit.OriginalIconPath = (from uplayGame in UplayGame.AllGames where uplayGame.GameId == _shortcutToEdit.GameAppId select uplayGame.GameIconPath).First();
                _shortcutToEdit.GameLibrary = SupportedGameLibrary.Uplay;
            }
            else if (rb_standalone.Checked)
                _shortcutToEdit.Category = ShortcutCategory.Application;

            // Add the Shortcut to the list of saved Shortcuts so it gets saved for later
            // but only if it's new... if it is an edit then it will already be in the list.
            ShortcutRepository.AddShortcut(_shortcutToEdit);

            // We've saved, so mark it as so
            _isUnsaved = false;

            // Save everything is golden and close the form.
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txt_different_executable_TextChanged(object sender, EventArgs e)
        {
            _isUnsaved = true;
        }

        private void txt_executable_TextChanged(object sender, EventArgs e)
        {
            _isUnsaved = true;
            suggestShortcutName();
            enableSaveButtonIfValid();
        }

        private bool canEnableSaveButton()
        {
            if ((txt_shortcut_save_name.Text.Length > 0) &&
                _profileToUse is ProfileItem &&
                (rb_no_game.Checked ||
                rb_launcher.Checked && _gameId > 0 ||
                rb_standalone.Checked && txt_executable.Text.Length > 0))
                return true;
            else
                return false;
        }

        private void enableSaveButtonIfValid()
        {
            if (canEnableSaveButton())
                btn_save.Enabled = true;
            else
                btn_save.Enabled = false;

        }

        private void suggestShortcutName()
        {
            if (_saveNameAutomatic && _profileToUse is ProfileItem)
            {
                if (rb_no_game.Checked)
                {
                    if (rb_switch_permanent.Checked)
                        txt_shortcut_save_name.Text = $"{_profileToUse.Name}";
                    else if (rb_switch_temp.Checked)
                        txt_shortcut_save_name.Text = $"{_profileToUse.Name} (Temporary)";
                }
                else if (rb_launcher.Checked && txt_game_name.Text.Length > 0)
                {
                    txt_shortcut_save_name.Text = $"{txt_game_name.Text} ({_profileToUse.Name})";
                }
                else if (rb_standalone.Checked && txt_executable.Text.Length > 0)
                {
                    string baseName = Path.GetFileNameWithoutExtension(txt_executable.Text);
                    txt_shortcut_save_name.Text = $"{baseName} ({_profileToUse.Name})";
                }
            }
        }

        private void rb_standalone_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_standalone.Checked)
            {
                _isUnsaved = true;
                rb_no_game.Checked = false;
                rb_launcher.Checked = false;

                // Enable the Standalone Panel
                p_standalone.Enabled = true;
                // Disable the Game Panel
                p_game.Enabled = false;

                suggestShortcutName();
                enableSaveButtonIfValid();
            }
           
        }

        private void rb_launcher_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_launcher.Checked)
            {
                _isUnsaved = true;
                rb_no_game.Checked = false;
                rb_standalone.Checked = false;

                // Enable the Game Panel
                p_game.Enabled = true;
                // Disable the Standalone Panel
                p_standalone.Enabled = false;

                suggestShortcutName();
                enableSaveButtonIfValid();

            }
        }

        private void rb_no_game_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_no_game.Checked)
            {
                _isUnsaved = true;
                rb_launcher.Checked = false;
                rb_standalone.Checked = false;

                // Disable the Standalone Panel
                p_standalone.Enabled = false;
                // Disable the Game Panel
                p_game.Enabled = false;

                suggestShortcutName();
                enableSaveButtonIfValid();

            }
        }



        private void cb_args_executable_CheckedChanged(object sender, EventArgs e)
        {
            _isUnsaved = true;
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

        private void RefreshImageListView(ProfileItem profile)
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

            // Load all the profiles to prepare things
            _loadedProfiles = ProfileRepository.AllProfiles;

            bool foundCurrentProfileInLoadedProfiles = false;
            foreach (ProfileItem loadedProfile in _loadedProfiles)
            {
                if (ProfileRepository.CurrentProfile.Equals(loadedProfile))
                {
                    // We have already saved the selected profile!
                    // so we need to show the selected profile 
                    ChangeSelectedProfile(loadedProfile);
                    foundCurrentProfileInLoadedProfiles = true;
                }

            }

            // If we get to the end of the loaded profiles and haven't
            // found a matching profile, then we need to show the first
            // Profile
            if (!foundCurrentProfileInLoadedProfiles && _loadedProfiles.Count > 0)
                ChangeSelectedProfile(_loadedProfiles[0]);


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
                            Bitmap extractedBitmap = ShortcutItem.ExtractVistaIcon(icoAppIcon);
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

            // Now start populating the other fields

            _id = _shortcutToEdit.Id;
            // Set if we launch App/Game/NoGame
            switch (_shortcutToEdit.Category)
            {
                case ShortcutCategory.NoGame:
                    rb_no_game.Checked = true;
                    break;
                case ShortcutCategory.Game:
                    rb_launcher.Checked = true;
                    break;
                case ShortcutCategory.Application:
                    rb_standalone.Checked = true;
                    break;
            }

            // Set the launcher items if we have them
            txt_game_launcher.Text = _shortcutToEdit.GameLibrary.ToString();
            txt_game_name.Text = _shortcutToEdit.GameName;
            _gameId = _shortcutToEdit.GameAppId;
            nud_timeout_game.Value = _shortcutToEdit.GameTimeout;
            txt_args_game.Text = _shortcutToEdit.GameArguments;
            if (_shortcutToEdit.GameArgumentsRequired)
            {
                cb_args_game.Checked = true;
            }

            //select the loaded Game item if it is there
            foreach (ListViewItem gameItem in lv_games.Items)
            {
                if (gameItem.Text.Equals(_shortcutToEdit.GameName))
                {
                    gameItem.Selected = true;
                }
            }


            // Set the executable items if we have them
            txt_executable.Text = _shortcutToEdit.ExecutableNameAndPath;
            nud_timeout_executable.Value = _shortcutToEdit.ExecutableTimeout;
            txt_args_executable.Text = _shortcutToEdit.ExecutableArguments;
            if (_shortcutToEdit.ExecutableArgumentsRequired)
            {
                cb_args_executable.Checked = true;
            }
            if (_shortcutToEdit.ProcessNameToMonitorUsesExecutable)
            {
                rb_wait_executable.Checked = true;
                rb_wait_alternative_executable.Checked = false;
            }
            else
            {
                rb_wait_executable.Checked = false;
                rb_wait_alternative_executable.Checked = true;
            }
            txt_alternative_executable.Text = _shortcutToEdit.DifferentExecutableToMonitor;


            // Set the shortcut name
            txt_shortcut_save_name.Text = _shortcutToEdit.Name;

            // Refresh the Shortcut UI
            RefreshShortcutUI();


        }

        private void rb_wait_process_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_wait_alternative_executable.Checked)
            {
                _isUnsaved = true;
                rb_wait_executable.Checked = false;
                txt_alternative_executable.Enabled = true;
            }
        }

        private void rb_wait_executable_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_wait_executable.Checked)
            {
                _isUnsaved = true;
                rb_wait_alternative_executable.Checked = false;
                txt_alternative_executable.Enabled = false;
            }
        }


        private void btn_app_different_executable_Click(object sender, EventArgs e)
        {
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                _isUnsaved = true;
                if (File.Exists(dialog_open.FileName) && Path.GetExtension(dialog_open.FileName) == @".exe")
                {
                    txt_alternative_executable.Text = dialog_open.FileName;
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
            _isUnsaved = true;
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
                        _isUnsaved = true;
                        txt_game_launcher.Text = SteamGame.GameLibrary.ToString();
                        _gameId = game.GameId;
                    }
                }
            }

            suggestShortcutName();
            enableSaveButtonIfValid();

        }

        private void ilv_saved_profiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            foreach (ProfileItem loadedProfile in _loadedProfiles)
            {
                if (loadedProfile.Name == e.Item.Text)
                {
                    ChangeSelectedProfile(loadedProfile);
                }
            }

        }

        private void ChangeSelectedProfile(ProfileItem profile)
        {
            // If the profile is null then return
            // (this happens when a new blank shortcut is created
            if (profile == null)
                return;

            // And we need to update the actual selected profile too!
            _profileToUse = profile;

            // We also need to load the saved profile name to show the user
            lbl_profile_shown.Text = _profileToUse.Name;

            if (_profileToUse.Equals(ProfileRepository.CurrentProfile))
            {
                lbl_profile_shown_subtitle.Text = "(Current Display Profile in use)";
            }
            else
            {
                if (!_profileToUse.IsPossible)
                {
                    lbl_profile_shown_subtitle.Text = "(Display Profile is not valid so cannot be used)";
                }
                else
                {
                    lbl_profile_shown_subtitle.Text = "";
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

               
            if (_loadedProfiles.Count > 0)
            {

                // Temporarily stop updating the saved_profiles listview
                ilv_saved_profiles.SuspendLayout();

                ImageListViewItem newItem = null;
                bool foundCurrentProfileInLoadedProfiles = false;
                foreach (ProfileItem loadedProfile in _loadedProfiles)
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
                    ChangeSelectedProfile(ProfileRepository.CurrentProfile);

                // Check if we were loading a profile to edit
                // If so, select that instead of all that other stuff above!
                if (_shortcutToEdit != null)
                    ChangeSelectedProfile(_shortcutToEdit.ProfileToUse);

                // Restart updating the saved_profiles listview
                ilv_saved_profiles.ResumeLayout();
            }
                
            enableSaveButtonIfValid();
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
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

        private void rb_switch_temp_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_temp.Checked)
            {
                _isUnsaved = true;
                rb_switch_permanent.Checked = false;

                suggestShortcutName();
            }
        }

        private void rb_switch_permanent_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_permanent.Checked)
            {
                _isUnsaved = true;
                rb_switch_temp.Checked = false;
                suggestShortcutName();
            }
        }

        private void txt_shortcut_save_name_TextChanged(object sender, EventArgs e)
        {
            _isUnsaved = true;
            enableSaveButtonIfValid();
        }

        private void lv_games_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            _isUnsaved = true;
        }

        private void ShortcutForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (_isUnsaved)
            {
                // If the user doesn't want to close this window without saving, then don't close the window.
                DialogResult result = MessageBox.Show(
                    @"You have unsaved changes! Do you want to close this window without saving your changes?",
                    @"You have unsaved changes.",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation);
                e.Cancel = (result == DialogResult.No); 
            }
           
        }

        private void btn_exe_to_start_Click(object sender, EventArgs e)
        {
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                _isUnsaved = true;
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

        private void txt_shortcut_save_name_Click(object sender, EventArgs e)
        {
            _saveNameAutomatic = false;
            cb_autosuggest.Checked = false;
        }

        private void cb_autosuggest_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_autosuggest.Checked)
                _saveNameAutomatic = true;
            else
                _saveNameAutomatic = false;
        }
    }
}