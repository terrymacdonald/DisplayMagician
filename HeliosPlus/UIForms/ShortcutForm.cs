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

        private ProfileAdaptor _profileAdaptor;
        //private List<ProfileItem> _loadedProfiles = new List<ProfileItem>();
        private ProfileItem _profileToUse= null;
        private GameStruct _gameToUse;
        private Executable _executableToUse;
        private ShortcutPermanence _permanence = ShortcutPermanence.Temporary;
        List<StartProgram> _startPrograms = new List<StartProgram>();
        private ShortcutItem _shortcutToEdit = null;
        private bool _isNewShortcut = true;
        private bool _isUnsaved = true;
        private bool _loadedShortcut = false;
        private bool _autoName = true;
        private uint _gameId = 0;
        private string  _uuid = "";

        public ShortcutForm()
        {
            InitializeComponent();

            // Set the profileAdaptor we need to load images from Profiles
            // into the Profiles ImageListView
            _profileAdaptor = new ProfileAdaptor();

            // Create a new SHortcut if we are creating a new one
            // And set up the page (otherwise this is all set when we load an
            // existing Shortcut)
            /*if (_shortcutToEdit == null)
            {
                shortcutToEdit = new ShortcutItem();
                isNewShortcut = true;
            }*/
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



            // Check the permanence requirements
            if (rb_switch_temp.Checked)
                _permanence = ShortcutPermanence.Temporary;

            if (rb_switch_permanent.Checked)
                _permanence = ShortcutPermanence.Permanent;

            // Save the start program 1
            StartProgram myStartProgram = new StartProgram();
            myStartProgram.Priority = 1;
            myStartProgram.Enabled = cb_start_program1.Checked;
            myStartProgram.Executable = txt_start_program1.Text;
            myStartProgram.ExecutableArgumentsRequired = cb_start_program_pass_args1.Checked;
            myStartProgram.Arguments = txt_start_program_args1.Text;
            myStartProgram.CloseOnFinish = cb_start_program_close1.Checked;
            _startPrograms.Add(myStartProgram);

            myStartProgram = new StartProgram();
            myStartProgram.Priority = 2;
            myStartProgram.Executable = txt_start_program2.Text;
            myStartProgram.Enabled = cb_start_program2.Checked;
            myStartProgram.ExecutableArgumentsRequired = cb_start_program_pass_args2.Checked;
            myStartProgram.Arguments = txt_start_program_args2.Text;
            myStartProgram.CloseOnFinish = cb_start_program_close2.Checked;
            _startPrograms.Add(myStartProgram);

            myStartProgram = new StartProgram();
            myStartProgram.Priority = 3;
            myStartProgram.Executable = txt_start_program3.Text;
            myStartProgram.Enabled = cb_start_program3.Checked;
            myStartProgram.ExecutableArgumentsRequired = cb_start_program_pass_args3.Checked;
            myStartProgram.Arguments = txt_start_program_args3.Text;
            myStartProgram.CloseOnFinish = cb_start_program_close3.Checked;
            _startPrograms.Add(myStartProgram);

            myStartProgram = new StartProgram();
            myStartProgram.Priority = 4;
            myStartProgram.Executable = txt_start_program4.Text;
            myStartProgram.Enabled = cb_start_program4.Checked;
            myStartProgram.ExecutableArgumentsRequired = cb_start_program_pass_args4.Checked;
            myStartProgram.Arguments = txt_start_program_args4.Text;
            myStartProgram.CloseOnFinish = cb_start_program_close4.Checked;
            _startPrograms.Add(myStartProgram);

            // Now we create the Shortcut Object ready to save
            // If we're launching a game
            if (rb_launcher.Checked)
            {
                // If the game is a SteamGame
                if(txt_game_launcher.Text == SupportedGameLibrary.Steam.ToString())
                {
                    // Find the SteamGame
                    _gameToUse = new GameStruct();
                    _gameToUse.GameToPlay = (from steamGame in SteamLibrary.AllInstalledGames where steamGame.Id == _gameId select steamGame).First();
                    _gameToUse.StartTimeout = Convert.ToUInt32(nud_timeout_game.Value);
                    _gameToUse.GameArguments = txt_args_game.Text;
                    _gameToUse.GameArgumentsRequired = cb_args_game.Checked;

                    _shortcutToEdit = new ShortcutItem(
                        txt_shortcut_save_name.Text,
                        _profileToUse,
                        _gameToUse,
                        _permanence,
                        _gameToUse.GameToPlay.IconPath,
                        _startPrograms,
                        _autoName,
                        _uuid
                    );
                    /*_shortcutToEdit.UpdateGameShortcut(
                        Name,
                        _profileToUse,
                        _gameToUse,
                        _permanence,
                        _gameToUse.GameToPlay.IconPath,
                        _startPrograms,
                        _autoName,
                        _uuid
                    );*/
                    
                }
                // If the game is a SteamGame
                /*else if (txt_game_launcher.Text == SupportedGameLibrary.Uplay.ToString())
                {
                    // Find the UplayGame
                    _steamGameToUse = (from UplayGame in SteamLibrary.AllInstalledGames where UplayGame.GameId == _shortcutToEdit.GameAppId).First();

                    _shortcutToEdit.UpdateGameShortcut(
                        Name,
                        _profileToUse,
                        _steamGameToUse,
                        _permanence,
                        _steamGameToUse.GameIconPath,
                        _startPrograms,
                        _autoName
                    );

                }*/
            }
            else if (rb_standalone.Checked)
            {
                _executableToUse = new Executable();
                _executableToUse.ExecutableArguments = txt_args_executable.Text;
                _executableToUse.ExecutableArgumentsRequired = cb_args_executable.Checked;
                _executableToUse.ExecutableNameAndPath = txt_executable.Text;
                _executableToUse.ExecutableTimeout = Convert.ToUInt32(nud_timeout_executable.Value);

                if (rb_wait_alternative_executable.Checked && !String.IsNullOrWhiteSpace(txt_alternative_executable.Text))
                {
                    _executableToUse.ProcessNameToMonitorUsesExecutable = true;
                    _executableToUse.DifferentExecutableToMonitor = txt_alternative_executable.Text;
                }
                else
                {
                    _executableToUse.ProcessNameToMonitorUsesExecutable = false;
                }

                _shortcutToEdit = new ShortcutItem(
                    txt_shortcut_save_name.Text,
                    _profileToUse,
                    _executableToUse,
                    _permanence,
                    _executableToUse.ExecutableNameAndPath,
                    _startPrograms,
                    _autoName
                );
/*                _shortcutToEdit.UpdateExecutableShortcut(
                    Name,
                    _profileToUse,
                    _executableToUse,
                    _permanence,
                    _executableToUse.ExecutableNameAndPath,
                    _startPrograms,
                    _autoName
                );
*/
            }
            else
            {

                _shortcutToEdit = new ShortcutItem(
                    txt_shortcut_save_name.Text,
                    _profileToUse,
                    _permanence,
                    _executableToUse.ExecutableNameAndPath,
                    _startPrograms,
                    _autoName
                );
/*                _shortcutToEdit.UpdateNoGameShortcut(
                    Name,
                    _profileToUse,
                    _permanence,
                    _executableToUse.ExecutableNameAndPath,
                    _startPrograms,
                    _autoName
                );
*/
            }

            // Generate the Shortcut Icon ready to be used
            _shortcutToEdit.SaveShortcutIconToCache();


            // Add the Shortcut to the list of saved Shortcuts so it gets saved for later
            // but only if it's new... if it is an edit then it will already be in the list.
                
            // We've saved, so mark it as so
            _isUnsaved = false;

            // Save everything is golden and close the form.
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void txt_different_executable_TextChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        private void txt_executable_TextChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
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
            if (_autoName && _profileToUse is ProfileItem)
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
                if (_loadedShortcut)
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
                if (_loadedShortcut)
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
                if (_loadedShortcut)
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
            if (_loadedShortcut)
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
                matchingImageListViewItems.First().Enabled = true;
            }
        }

        private async void ShortcutForm_Load(object sender, EventArgs e)
        {

            // Load all the profiles to prepare things
            bool foundChosenProfileInLoadedProfiles = false;
            ProfileItem chosenProfile = null;


            // Load the Games ListView
            foreach (var game in SteamLibrary.AllInstalledGames.OrderBy(game => game.Name))
            {
                // Get the bitmap out of the IconPath 
                // IconPath can be an ICO, or an EXE
                Bitmap bm = null;
                try
                {
                    bm = ShortcutItem.ToSmallBitmap(game.IconPath);
                }
                catch (Exception innerEx)
                {
                    Console.WriteLine($"ShortcutForm exception: {innerEx.Message}: {innerEx.StackTrace} - {innerEx.InnerException}");
                    bm = Properties.Resources.Steam.ToBitmap();
                }

                // Add the images to the images array
                il_games.Images.Add(bm);

                if (!Visible)
                {
                    return;
                }

                // ADd the game to the game array
                lv_games.Items.Add(new ListViewItem
                {
                    Text = game.Name,
                    Tag = game,
                    ImageIndex = il_games.Images.Count - 1
                });
            }

            // If it is a new Shortcut then we don't have to load anything!
            if (_isNewShortcut)
            {
                RefreshShortcutUI();
                ChangeSelectedProfile(ProfileRepository.CurrentProfile);
                _isUnsaved = true;
                return;
            }

            // We only get down here if the form has loaded a shortcut to edit
            if (_shortcutToEdit is ShortcutItem && _shortcutToEdit.ProfileToUse is ProfileItem)
            {
                foreach (ProfileItem loadedProfile in ProfileRepository.AllProfiles)
                {
                    if (_shortcutToEdit.ProfileToUse.Equals(loadedProfile))
                    {
                        // We have loaded the profile used last time
                        // so we need to show the selected profile in the UI
                        chosenProfile = loadedProfile;
                        foundChosenProfileInLoadedProfiles = true;

                        // If the profile is the same, but the user has renamed the profile
                        // since the shortcut was last created, then we need to tell the user
                        if (!loadedProfile.Name.Equals(_shortcutToEdit.ProfileToUse.Name))
                        {

                            MessageBox.Show(
                            @"The Display Profile used by this Shortcut still exists, but it's changed it's name. We've updated the shortcut's name to reflect this change.",
                            @"Display Profile name changed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                            break;
                        }
                        
                    }

                }
            }

            // If we get to the end of the loaded profiles and haven't
            // found a matching profile, then we need to show the current profile
            // that we're running now
            if (!foundChosenProfileInLoadedProfiles && ProfileRepository.ProfileCount > 0)
            {
                foreach (ProfileItem loadedProfile in ProfileRepository.AllProfiles)
                {
                    if (ProfileRepository.CurrentProfile.Equals(loadedProfile))
                    {
                        // We have loaded the profile used last time
                        // so we need to show the selected profile in the UI
                        chosenProfile = loadedProfile;
                        foundChosenProfileInLoadedProfiles = true;

                        // If the profile is the same, but the user has renamed the profile
                        // since the shortcut was last created, then we need to tell the user
                        if (!loadedProfile.Name.Equals(ProfileRepository.CurrentProfile.Name))
                        {

                            MessageBox.Show(
                            @"The Display Profile used by this Shortcut still exists, but it's changed it's name. We've updated the shortcut's name to reflect this change.",
                            @"Display Profile name changed",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        }

                    }

                }
            }


            // Now start populating the other fields
            _uuid = _shortcutToEdit.UUID;



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
            nud_timeout_game.Value = _shortcutToEdit.StartTimeout;
            txt_args_game.Text = _shortcutToEdit.GameArguments;
            if (_shortcutToEdit.GameArgumentsRequired)
            {
                cb_args_game.Checked = true;
            }

            cb_autosuggest.Checked = _shortcutToEdit.AutoName;

            //select the loaded Game item if it is there
            foreach (ListViewItem gameItem in lv_games.Items)
            {
                if (gameItem.Text.Equals(_shortcutToEdit.GameName))
                {
                    gameItem.Selected = true;
                    break;
                }
            }

            // Set the executable items if we have them
            txt_executable.Text = _shortcutToEdit.ExecutableNameAndPath;
            nud_timeout_executable.Value = _shortcutToEdit.StartTimeout;
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

            // Set up the start programs

            if (_shortcutToEdit.StartPrograms is List<StartProgram> && _shortcutToEdit.StartPrograms.Count > 0)
            {
                foreach (StartProgram myStartProgram in _shortcutToEdit.StartPrograms)
                {

                    // Update the 4 programs to start
                    switch (myStartProgram.Priority)
                    {
                        case 1:
                            txt_start_program1.Text = myStartProgram.Executable;
                            cb_start_program1.Checked = myStartProgram.Enabled;
                            cb_start_program_pass_args1.Checked = myStartProgram.ExecutableArgumentsRequired;
                            txt_start_program_args1.Text = myStartProgram.Arguments;
                            cb_start_program_close1.Checked = myStartProgram.CloseOnFinish;
                            break;
                        case 2:
                            txt_start_program2.Text = myStartProgram.Executable;
                            cb_start_program2.Checked = myStartProgram.Enabled;
                            cb_start_program_pass_args2.Checked = myStartProgram.ExecutableArgumentsRequired;
                            txt_start_program_args2.Text = myStartProgram.Arguments;
                            cb_start_program_close2.Checked = myStartProgram.CloseOnFinish;
                            break;
                        case 3:
                            txt_start_program3.Text = myStartProgram.Executable;
                            cb_start_program3.Checked = myStartProgram.Enabled;
                            cb_start_program_pass_args3.Checked = myStartProgram.ExecutableArgumentsRequired;
                            txt_start_program_args3.Text = myStartProgram.Arguments;
                            cb_start_program_close3.Checked = myStartProgram.CloseOnFinish;
                            break;
                        case 4:
                            txt_start_program4.Text = myStartProgram.Executable;
                            cb_start_program4.Checked = myStartProgram.Enabled;
                            cb_start_program_pass_args4.Checked = myStartProgram.ExecutableArgumentsRequired;
                            txt_start_program_args4.Text = myStartProgram.Arguments;
                            cb_start_program_close4.Checked = myStartProgram.CloseOnFinish;
                            break;

                    }
                }
            }

            // Refresh the Shortcut UI
            RefreshShortcutUI();
            ChangeSelectedProfile(chosenProfile);
            RefreshImageListView(chosenProfile);

            _loadedShortcut = true;
            _isUnsaved = false;

            // Finally enable the save button if it's still valid
            enableSaveButtonIfValid();

        }

        private void rb_wait_process_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_wait_alternative_executable.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_wait_executable.Checked = false;
                txt_alternative_executable.Enabled = true;
            }
        }

        private void rb_wait_executable_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_wait_executable.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_wait_alternative_executable.Checked = false;
                txt_alternative_executable.Enabled = false;
            }
        }


        private void btn_app_different_executable_Click(object sender, EventArgs e)
        {
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (_loadedShortcut)
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
            if (_loadedShortcut)
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
                foreach (SteamGame game in SteamLibrary.AllInstalledGames)
                {
                    if (game.Name == txt_game_name.Text)
                    {
                        if (_loadedShortcut)
                            _isUnsaved = true;
                        txt_game_launcher.Text = game.GameLibrary.ToString();
                        _gameId = game.Id;
                    }
                }
            }

            suggestShortcutName();
            enableSaveButtonIfValid();

        }

        private void ilv_saved_profiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            foreach (ProfileItem savedProfile in ProfileRepository.AllProfiles)
            {
                if (savedProfile.Name == e.Item.Text)
                {
                    ChangeSelectedProfile(savedProfile);
                    break;
                }
            }

            suggestShortcutName();
            enableSaveButtonIfValid();

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
                lbl_profile_shown_subtitle.Text = "This is the Display Profile currently in use.";
            else
                lbl_profile_shown_subtitle.Text = "";

            // Refresh the image list view
            RefreshImageListView(profile);

            // And finally show the profile in the display view
            dv_profile.Profile = profile;
            dv_profile.Refresh();

        }

        private void RefreshShortcutUI()
        {

               
            if (ProfileRepository.ProfileCount > 0)
            {

                // Temporarily stop updating the saved_profiles listview
                ilv_saved_profiles.SuspendLayout();

                ImageListViewItem newItem = null;
                foreach (ProfileItem loadedProfile in ProfileRepository.AllProfiles)
                {
                    bool thisLoadedProfileIsAlreadyHere = (from item in ilv_saved_profiles.Items where item.Text == loadedProfile.Name orderby item.Text select item.Text).Any();
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
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_switch_permanent.Checked = false;

                suggestShortcutName();
            }
        }

        private void rb_switch_permanent_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_permanent.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_switch_temp.Checked = false;
                suggestShortcutName();
            }
        }

        private void txt_shortcut_save_name_TextChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            enableSaveButtonIfValid();
        }

        private void ShortcutForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (_isUnsaved && _loadedShortcut)
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
            txt_executable.Text = get_exe_file();
        }

        private void txt_shortcut_save_name_Click(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            _autoName = false;
            cb_autosuggest.Checked = false;
        }

        private void cb_autosuggest_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            if (cb_autosuggest.Checked)
            {
                _autoName = true;
                suggestShortcutName();
            }
            else
                _autoName = false;
        }

        private string get_exe_file()
        {
            string textToReturn = "";
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(dialog_open.FileName) && Path.GetExtension(dialog_open.FileName) == @".exe")
                {
                    textToReturn = dialog_open.FileName;
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
            return textToReturn;
        }
        private void btn_start_program1_Click(object sender, EventArgs e)
        {
            txt_start_program1.Text = get_exe_file();
        }

        private void btn_start_program2_Click(object sender, EventArgs e)
        {
            txt_start_program2.Text = get_exe_file();
        }

        private void btn_start_program3_Click(object sender, EventArgs e)
        {
            txt_start_program3.Text = get_exe_file();
        }

        private void btn_start_program4_Click(object sender, EventArgs e)
        {
            txt_start_program4.Text = get_exe_file();
        }

        private void cb_start_program1_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            // Disable the start program 1 fields
            if (cb_start_program1.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program1.Visible = true;
                btn_start_program1.Visible = true;
                cb_start_program_pass_args1.Visible = true;
                cb_start_program_close1.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program1.Visible = false;
                btn_start_program1.Visible = false;
                cb_start_program_pass_args1.Visible = false;
                cb_start_program_close1.Visible = false;
            }
        }

        private void cb_start_program2_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            // Disable the start program 2 fields
            if (cb_start_program2.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program2.Visible = true;
                btn_start_program2.Visible = true;
                cb_start_program_pass_args2.Visible = true;
                cb_start_program_close2.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program2.Visible = false;
                btn_start_program2.Visible = false;
                cb_start_program_pass_args2.Visible = false;
                cb_start_program_close2.Visible = false;
            }
        }

        private void cb_start_program3_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            // Disable the start program 3 fields
            if (cb_start_program3.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program3.Visible = true;
                btn_start_program3.Visible = true;
                cb_start_program_pass_args3.Visible = true;
                cb_start_program_close3.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program3.Visible = false;
                btn_start_program3.Visible = false;
                cb_start_program_pass_args3.Visible = false;
                cb_start_program_close3.Visible = false;
            }
        }

        private void cb_start_program4_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            // Disable the start program 4 fields
            if (cb_start_program4.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program4.Visible = true;
                btn_start_program4.Visible = true;
                cb_start_program_pass_args4.Visible = true;
                cb_start_program_close4.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program4.Visible = false;
                btn_start_program4.Visible = false;
                cb_start_program_pass_args4.Visible = false;
                cb_start_program_close4.Visible = false;
            }
        }

        private void cb_start_program_pass_args1_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            // Disable the start program 1 fields
            if (cb_start_program_pass_args1.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program_args1.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program_args1.Visible = false;
            }
        }

        private void cb_start_program_pass_args2_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            // Disable the start program 2 fields
            if (cb_start_program_pass_args2.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program_args2.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program_args2.Visible = false;
            }
        }

        private void cb_start_program_pass_args3_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            // Disable the start program 3 fields
            if (cb_start_program_pass_args3.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program_args3.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program_args3.Visible = false;
            }
        }

        private void cb_start_program_pass_args4_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            // Disable the start program 4 fields
            if (cb_start_program_pass_args4.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program_args4.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program_args4.Visible = false;
            }
        }
    }
}