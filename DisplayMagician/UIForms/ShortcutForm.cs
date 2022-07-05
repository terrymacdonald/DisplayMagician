using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using DisplayMagician.Resources;
using DisplayMagicianShared;
using DisplayMagician.GameLibraries;
using Manina.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi;
using NHotkey.WindowsForms;
using NHotkey;
using System.Threading;
using DisplayMagician.AppLibraries;

namespace DisplayMagician.UIForms
{

    public partial class ShortcutForm : Form
    {

        private ProfileAdaptor _profileAdaptor;
        private GameAdaptor _gameAdaptor;
        private bool _editingExistingShortcut = false;
        //private List<ProfileItem> _loadedProfiles = new List<ProfileItem>();
        private ProfileItem _profileToUse = null;
        private string _gameLauncher = "";
        private GameStruct _gameToUse;
        private Executable _executableToUse;
        private ShortcutPermanence _displayPermanence = ShortcutPermanence.Temporary;
        private ShortcutPermanence _audioPermanence = ShortcutPermanence.Temporary;
        private ShortcutPermanence _capturePermanence = ShortcutPermanence.Temporary;
        List<StartProgram> _startPrograms = new List<StartProgram>();
        List<StopProgram> _stopPrograms = new List<StopProgram>();
        private string _audioDevice = "";
        private bool _changeAudioDevice = false;
        private bool _setAudioVolume = false;
        private decimal _audioVolume = -1;
        private string _captureDevice = "";
        private bool _changeCaptureDevice = false;
        private bool _setCaptureVolume = false;
        private decimal _captureVolume = -1;
        private ShortcutItem _shortcutToEdit = null;
        Game _selectedGame = null;
        App _selectedApp = null;
        private bool _isUnsaved = true;
        private bool _loadedShortcut = false;
        private bool _autoName = true;
        private string _gameId = "0";
        private string _uuid = "";
        private CoreAudioController audioController = null;
        private List<CoreAudioDevice> audioDevices = null;
        private CoreAudioDevice selectedAudioDevice = null;
        private List<CoreAudioDevice> captureDevices = null;
        private CoreAudioDevice selectedCaptureDevice = null;
        private Keys _hotkey = Keys.None;
        //private bool _userChoseOwnGameIcon = false;
        //private string _userGameIconPath = "";
        //private bool _userChoseOwnExeIcon = false;
        //private string _userExeIconPath = "";
        private List<ShortcutBitmap> _availableImages = new List<ShortcutBitmap>();
        private ShortcutBitmap _selectedImage = new ShortcutBitmap();
        private bool _firstShow = true;
        private ShortcutLoadingForm _loadingScreen;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ShortcutForm()
        {
            InitializeComponent();
            // Set the profileAdaptor we need to load images from Profiles
            // into the Profiles ImageListView
            try
            {
                _profileAdaptor = new ProfileAdaptor();
                _gameAdaptor = new GameAdaptor();

                // Style the Saved Profiles list
                ilv_saved_profiles.MultiSelect = false;
                ilv_saved_profiles.ThumbnailSize = new Size(100, 100);
                ilv_saved_profiles.AllowDrag = false;
                ilv_saved_profiles.AllowDrop = false;
                ilv_saved_profiles.SetRenderer(new ProfileILVRenderer());


                ilv_games.MultiSelect = false;
                ilv_games.ThumbnailSize = new Size(100, 100);
                ilv_games.AllowDrag = false;
                ilv_games.AllowDrop = false;
                ilv_games.SetRenderer(new GameILVRenderer());               

            }
            catch (Exception ex)
            {
                logger.Error(ex, $"ShortcutForm/ShortcutForm: Exception while trying to setup the game ImageListView and set the render.");
            }

            lbl_profile_shown.Text = "No Display Profiles available";
            lbl_profile_shown_subtitle.Text = "Please go back to the main window, click on 'Display Profiles', and save a new Display Profile. Then come back here.";

            try
            {
                if (audioController == null)
                {
                    audioController = new CoreAudioController();
                }
                    
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutForm/ShortcutForm: Exception while trying to initialise CoreAudioController in ShortcutForm. Audio Chipset on your computer is not supported. You will be unable to set audio settings.");
            }

            // Center the form on the primary screen
            Utils.CenterOnPrimaryScreen(this);

        }    

        public ShortcutItem Shortcut
        {
            get => _shortcutToEdit;
            set => _shortcutToEdit = value;
        }

        public bool EditingExistingShortcut
        {
            get => _editingExistingShortcut;
            set => _editingExistingShortcut = value;
        }

        public SupportedGameLibraryType GameLibrary
        {
            get
            {
                if (_gameLauncher.Contains("Steam"))
                {
                    return SupportedGameLibraryType.Steam;
                }
                else if (_gameLauncher.Contains("Uplay"))
                {
                    return SupportedGameLibraryType.Uplay;
                }
                else if (_gameLauncher.Contains("Origin"))
                {
                    return SupportedGameLibraryType.Origin;
                }
                else if (_gameLauncher.Contains("Epic"))
                {
                    return SupportedGameLibraryType.Epic;
                }
                else if (_gameLauncher.Contains("GOG"))
                {
                    return SupportedGameLibraryType.GOG;
                }

                return SupportedGameLibraryType.Unknown;
            }
            set
            {
                switch (value)
                {
                    case SupportedGameLibraryType.Steam:
                        _gameLauncher = Enum.GetName(typeof(SupportedGameLibraryType), SupportedGameLibraryType.Steam);
                        break;

                    case SupportedGameLibraryType.Uplay:
                        _gameLauncher = Enum.GetName(typeof(SupportedGameLibraryType), SupportedGameLibraryType.Uplay);
                        break;

                    case SupportedGameLibraryType.Origin:
                        _gameLauncher = Enum.GetName(typeof(SupportedGameLibraryType), SupportedGameLibraryType.Origin);
                        break;

                    case SupportedGameLibraryType.Epic:
                        _gameLauncher = Enum.GetName(typeof(SupportedGameLibraryType), SupportedGameLibraryType.Epic);
                        break;

                    case SupportedGameLibraryType.GOG:
                        _gameLauncher = Enum.GetName(typeof(SupportedGameLibraryType), SupportedGameLibraryType.GOG);
                        break;

                    case SupportedGameLibraryType.Unknown:
                        _gameLauncher = "No supported Game Libraries found";
                        break;
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

                if (_gameId.Equals("0"))
                {
                    MessageBox.Show(
                        @"Please choose a Game by scrolling through the list, selecting the Game that you want, and then clicking the '>>' button to fill the Game fields.",
                        @"Please choose a Game.",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                bool gameStillInstalled = false;
                foreach (ImageListViewItem gameItem in ilv_games.Items)
                {
                    if (gameItem.Text.Equals(txt_game_name.Text))
                    {
                        gameStillInstalled = true;
                        break;
                    }
                        
                }
                if (!gameStillInstalled)
                {
                    DialogResult result = MessageBox.Show(
                        $"This shortcut refers to the '{txt_game_name.Text}' game that was installed in your {_gameLauncher} library. This game is no longer installed, so the shortcut won't work. Do you still want to save the shortcut?",
                        @"Game no longer exists",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Exclamation);
                    if (result == DialogResult.No)
                        return;
                }

                if (cb_wait_alternative_game.Checked && String.IsNullOrWhiteSpace(txt_alternative_game.Text))
                {
                    MessageBox.Show(
                        $"If you want to wait for an alternative game executable then you need to choose it! Click the 'Choose' button next to the different game executable field.",
                        @"Need to choose the different game executable",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

                if (cb_wait_alternative_game.Checked && !File.Exists(txt_alternative_game.Text))
                {
                    MessageBox.Show(
                        @"The alternative game executable you have chosen does not exist! Please reselect the alternative game executable, or check you have permissions to view it.",
                        @"Alternative game executable doesn't exist",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    return;
                }

            }


            // Check the display permanence requirements
            if (rb_switch_display_temp.Checked)
                _displayPermanence = ShortcutPermanence.Temporary;

            if (rb_switch_display_permanent.Checked)
                _displayPermanence = ShortcutPermanence.Permanent;

            // If we can get access to the audio chipset then
            // we try to get the settings
            if (audioController != null)
            {
                if (audioDevices != null && audioDevices.Count > 0)
                {
                    // Save the Audio features
                    if (rb_change_audio.Checked)
                    {
                        _changeAudioDevice = true;
                        _audioDevice = cb_audio_device.Text;
                    }
                    else
                    {
                        _changeAudioDevice = false;
                        _audioDevice = "";
                    }


                    if (rb_set_audio_volume.Checked)
                    {
                        _setAudioVolume = true;
                        _audioVolume = nud_audio_volume.Value;
                    }
                    else
                    {
                        _setAudioVolume = false;
                        _audioVolume = -1;
                    }

                    // Check the audio permanence requirements
                    if (rb_switch_audio_temp.Checked)
                        _audioPermanence = ShortcutPermanence.Temporary;

                    if (rb_switch_audio_permanent.Checked)
                        _audioPermanence = ShortcutPermanence.Permanent;

                }
                else
                {
                    // No active audio devices found, so we force the save to disable changing the audio device
                    logger.Warn($"ShortcutForm/btn_save_Click: No active audio devices found, so forcing the save to disable changing the audio device for this shortcut.");
                    _changeAudioDevice = false;
                    _audioDevice = "";
                    _setAudioVolume = false;
                    _audioVolume = -1;
                    _audioPermanence = ShortcutPermanence.Temporary;
                }

                if (captureDevices != null && captureDevices.Count > 0) 
                {
                    // Save the Capture features
                    if (rb_change_capture.Checked)
                    {
                        _changeCaptureDevice = true;
                        _captureDevice = cb_capture_device.Text;
                    }
                    else
                    {
                        _changeCaptureDevice = false;
                        _captureDevice = "";
                    }


                    if (rb_set_capture_volume.Checked)
                    {
                        _setCaptureVolume = true;
                        _captureVolume = nud_capture_volume.Value;
                    }
                    else
                    {
                        _setCaptureVolume = false;
                        _captureVolume = -1;
                    }

                    // Check the microphone permanence requirements
                    if (rb_switch_capture_temp.Checked)
                        _capturePermanence = ShortcutPermanence.Temporary;

                    if (rb_switch_capture_permanent.Checked)
                        _capturePermanence = ShortcutPermanence.Permanent;

                }
                else
                {
                    // No active capture devices found, so we force the save to disable changing the capture device
                    logger.Warn($"ShortcutForm/btn_save_Click: No active capture devices found, so forcing the save to disable changing the capture device for this shortcut.");
                    _changeCaptureDevice = false;
                    _captureDevice = "";
                    _setCaptureVolume = false;
                    _captureVolume = -1;
                    _capturePermanence = ShortcutPermanence.Temporary;
                }

            }
            // Otherwise we force set the audio settings to no change
            // just to be sure
            else
            {
                _changeAudioDevice = false;
                _audioDevice = "";
                _setAudioVolume = false;
                _audioVolume = -1;
                _changeCaptureDevice = false;
                _captureDevice = "";
                _setCaptureVolume = false;
                _captureVolume = -1;
                _audioPermanence = ShortcutPermanence.Temporary;
                _capturePermanence = ShortcutPermanence.Temporary;
            }

            // Add the startprograms to the list
            List<StartProgram> newStartPrograms = new List<StartProgram>() { };
            foreach (StartProgramControl myStartProgramControl in flp_start_programs.Controls)
            {
                newStartPrograms.Add(myStartProgramControl.StartProgram);
            }

            // Replace the old start programs with the ones we've created now
            _startPrograms = newStartPrograms;

            // Store the single stop program if it's set (but wth lots of defaults)
            if (!String.IsNullOrWhiteSpace(txt_run_cmd_afterwards.Text) && File.Exists(txt_run_cmd_afterwards.Text))
            {
                _stopPrograms = new List<StopProgram>();
                StopProgram stopProgram = new StopProgram();
                stopProgram.Executable = txt_run_cmd_afterwards.Text;
                stopProgram.Priority = 0;
                stopProgram.DontStartIfAlreadyRunning = false;                
                if (cb_run_cmd_afterwards.Checked)
                {
                    stopProgram.Disabled = false;
                }
                else
                {
                    stopProgram.Disabled = true;
                }
                if (cb_run_cmd_afterwards_args.Checked)
                {
                    stopProgram.ExecutableArgumentsRequired = true;
                    stopProgram.Arguments = txt_run_cmd_afterwards_args.Text;
                }
                else
                {                    
                    stopProgram.ExecutableArgumentsRequired = false;
                    stopProgram.Arguments = "";
                }
                stopProgram.ProcessPriority = ProcessPriority.Normal;
                if (cb_run_cmd_afterwards_dont_start.Checked)
                {
                    stopProgram.DontStartIfAlreadyRunning = true;
                }
                else
                {
                    stopProgram.DontStartIfAlreadyRunning = false;
                }
                if (cb_run_cmd_afterwards_run_as_administrator.Checked)
                {
                    stopProgram.RunAsAdministrator = true;
                }
                else
                {
                    stopProgram.RunAsAdministrator = false;
                }                

                _stopPrograms.Add(stopProgram);
            }

            // Now we create the Shortcut Object ready to save
            // If we're launching a game
            if (rb_launcher.Checked)
            {
                logger.Trace($"ShortcutForm/btn_save_Click: We're saving a game!");
                
                _gameToUse = new GameStruct
                {
                    StartTimeout = Convert.ToInt32(nud_timeout_game.Value),
                    GameArguments = txt_args_game.Text,
                    GameArgumentsRequired = cb_args_game.Checked,
                    DifferentGameExeToMonitor = txt_alternative_game.Text,
                    MonitorDifferentGameExe = cb_wait_alternative_game.Checked,
                    ProcessPriority = (ProcessPriority)cbx_game_priority.SelectedValue,
                };

                // If the game is a SteamGame
                if (_gameLauncher == SupportedGameLibraryType.Steam.ToString())
                {
                    logger.Trace($"ShortcutForm/btn_save_Click: We're saving a Steam game!");
                    // Find the SteamGame

                    _gameToUse.GameToPlay = (from steamGame in SteamLibrary.GetLibrary().AllInstalledGames where steamGame.Id == _gameId select steamGame).First();
                }
                // If the game is a UplayGame
                else if (_gameLauncher == SupportedGameLibraryType.Uplay.ToString())
                {
                    logger.Trace($"ShortcutForm/btn_save_Click: We're saving a Uplay game!");
                    // Find the UplayGame
                    _gameToUse.GameToPlay = (from uplayGame in UplayLibrary.GetLibrary().AllInstalledGames where uplayGame.Id == _gameId select uplayGame).First();
                }
                // If the game is an Origin Game
                else if (_gameLauncher == SupportedGameLibraryType.Origin.ToString())
                {
                    logger.Trace($"ShortcutForm/btn_save_Click: We're saving an Origin game!");
                    _gameToUse.GameToPlay = (from originGame in OriginLibrary.GetLibrary().AllInstalledGames where originGame.Id == _gameId select originGame).First();
                }
                // If the game is an Epic Game
                else if (_gameLauncher == SupportedGameLibraryType.Epic.ToString())
                {
                    logger.Trace($"ShortcutForm/btn_save_Click: We're saving an Epic game!");
                    _gameToUse.GameToPlay = (from epicGame in EpicLibrary.GetLibrary().AllInstalledGames where epicGame.Id == _gameId select epicGame).First();
                }
                // If the game is an GOG Game
                else if (_gameLauncher == SupportedGameLibraryType.GOG.ToString())
                {
                    logger.Trace($"ShortcutForm/btn_save_Click: We're saving an GOG game!");
                    _gameToUse.GameToPlay = (from gogGame in GogLibrary.GetLibrary().AllInstalledGames where gogGame.Id == _gameId select gogGame).First();
                }

                try
                {
                    _shortcutToEdit.UpdateGameShortcut(
                        txt_shortcut_save_name.Text,
                        _profileToUse,
                        _gameToUse,
                        _displayPermanence,
                        _audioPermanence,
                        _capturePermanence,
                        _gameToUse.GameToPlay.IconPath,
                        _selectedImage,
                        _availableImages,
                        _changeAudioDevice,
                        _audioDevice,
                        _setAudioVolume,
                        _audioVolume,
                        _changeCaptureDevice,
                        _captureDevice,
                        _setCaptureVolume,
                        _captureVolume,
                        _startPrograms,
                        _stopPrograms,
                        _autoName,
                        _uuid,
                        _hotkey
                    );
                }
                catch(Exception ex) 
                {
                    logger.Error(ex,$"ShortcutForm/btn_save_Click: Exception while trying to update a game shortcut! :  ",
                        txt_shortcut_save_name.Text, 
                        _profileToUse,
                        _gameToUse,
                        _displayPermanence,
                        _audioPermanence,
                        _capturePermanence,
                        _gameToUse.GameToPlay.IconPath,
                        _selectedImage, 
                        _availableImages, 
                        _changeAudioDevice,
                        _audioDevice,
                        _setAudioVolume,
                        _audioVolume,
                        _changeCaptureDevice,
                        _captureDevice,
                        _setCaptureVolume,
                        _captureVolume,
                        _startPrograms,
                        _stopPrograms,
                        _autoName,
                        _uuid,
                        _hotkey
                    );
                }
                
            }
            else if (rb_standalone.Checked)
            {
                logger.Trace($"ShortcutForm/btn_save_Click: We're saving a standalone executable!");
                _executableToUse = new Executable
                {
                    ExecutableArguments = txt_args_executable.Text,
                    ExecutableArgumentsRequired = cb_args_executable.Checked,
                    ExecutableNameAndPath = txt_executable.Text,
                    RunAsAdministrator = cb_run_exe_as_administrator.Checked,
                    ExecutableTimeout = Convert.ToInt32(nud_timeout_executable.Value),
                    ProcessPriority = (ProcessPriority)cbx_exe_priority.SelectedValue,
                };

                if (rb_wait_alternative_executable.Checked && !String.IsNullOrWhiteSpace(txt_alternative_executable.Text))
                {
                    _executableToUse.ProcessNameToMonitorUsesExecutable = false;
                    _executableToUse.DifferentExecutableToMonitor = txt_alternative_executable.Text;
                }
                else
                {
                    _executableToUse.ProcessNameToMonitorUsesExecutable = true;
                }


                try
                {
                    _shortcutToEdit.UpdateExecutableShortcut(
                        txt_shortcut_save_name.Text,
                        _profileToUse,
                        _executableToUse,
                        _displayPermanence,
                        _audioPermanence,
                        _capturePermanence,
                        _executableToUse.ExecutableNameAndPath,
                        _selectedImage, 
                        _availableImages,
                        _changeAudioDevice,
                        _audioDevice,
                        _setAudioVolume,
                        _audioVolume,
                        _changeCaptureDevice,
                        _captureDevice,
                        _setCaptureVolume,
                        _captureVolume,
                        _startPrograms,
                        _stopPrograms,
                        _autoName,
                        _hotkey
                    );
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutForm/btn_save_Click: Exception while trying to update an application shortcut! :  ",
                        txt_shortcut_save_name.Text,
                        _profileToUse,
                        _executableToUse,
                        _displayPermanence,
                        _audioPermanence,
                        _capturePermanence,
                        _executableToUse.ExecutableNameAndPath,
                        _selectedImage, 
                        _availableImages,
                        _changeAudioDevice,
                        _audioDevice,
                        _setAudioVolume,
                        _audioVolume,
                        _changeCaptureDevice,
                        _captureDevice,
                        _setCaptureVolume,
                        _captureVolume,
                        _startPrograms,
                        _stopPrograms,
                        _autoName,
                        _hotkey
                    );
                }

            }
            else
            {
                logger.Trace($"ShortcutForm/btn_save_Click: We're not saving any game or executable to start!");
                try
                {
                    _shortcutToEdit.UpdateNoGameShortcut(
                        txt_shortcut_save_name.Text,
                        _profileToUse,
                        _displayPermanence,
                        _audioPermanence,
                        _capturePermanence,
                        _executableToUse.ExecutableNameAndPath,
                        _changeAudioDevice,
                        _audioDevice,
                        _setAudioVolume,
                        _audioVolume,
                        _changeCaptureDevice,
                        _captureDevice,
                        _setCaptureVolume,
                        _captureVolume,
                        _startPrograms,
                        _stopPrograms,
                        _autoName,
                        _hotkey
                    );
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutForm/btn_save_Click: Exception while trying to update a shortcut that doesn't run anything! :  ",
                        txt_shortcut_save_name.Text,
                        _profileToUse,
                        _displayPermanence,
                        _audioPermanence,
                        _capturePermanence,
                        _executableToUse.ExecutableNameAndPath,
                        _changeAudioDevice,
                        _audioDevice,
                        _setAudioVolume,
                        _audioVolume,
                        _changeCaptureDevice,
                        _captureDevice,
                        _setCaptureVolume,
                        _captureVolume,
                        _startPrograms,
                        _stopPrograms,
                        _autoName,
                        _hotkey
                    );
                }


            }

            if (_hotkey == Keys.None)
                // Remove the Hotkey if it needs to be removed
                HotkeyManager.Current.Remove(_shortcutToEdit.UUID);
            else
                // Set the hokey if there is one
                HotkeyManager.Current.AddOrReplace(_shortcutToEdit.UUID, _shortcutToEdit.Hotkey, OnWindowHotkeyPressed);

            // Refresh validity after these changes
            _shortcutToEdit.RefreshValidity();

            // We've saved, so mark it as so
            _isUnsaved = false;

            // Save everything is golden and close the form.
            DialogResult = DialogResult.OK;
            this.Hide();
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
                        Language.Selected_file_is_not_a_valid_file,
                        Language.Executable,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }

        private void txt_alternative_executable_TextChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        private void txt_executable_TextChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            SuggestShortcutName();
            EnableSaveButtonIfValid();
        }

        private bool CanEnableSaveButton()
        {
            // Check the name is valid to save
            if (String.IsNullOrWhiteSpace(txt_shortcut_save_name.Text))
            {logger.Error($"ShortcutForm/CanEnableSaveButton: The application doesn't have an executable listed the shortcut");
                return false;
            }

            // Check the profile is set and that it's still valid
            if (!(_profileToUse is ProfileItem))
            {
                logger.Error($"ShortcutForm/CanEnableSaveButton: The shortcut doesn't have a profile assigned to it!");
                return false;
            }

            // Check the Shortcut Category to see if it's application
            if (rb_standalone.Checked)
            {
                if (cb_args_executable.Checked && String.IsNullOrWhiteSpace(txt_args_executable.Text))
                {
                    logger.Error($"ShortcutForm/CanEnableSaveButton: The application doesn't have an executable listed");
                    return false;

                }

                if (!File.Exists(txt_executable.Text))
                {
                    logger.Error($"ShortcutForm/CanEnableSaveButton: The application executable {txt_executable.Text} doesn't exist. Please check the file '{txt_executable.Text}' is still there, and that the file has the correct permissions.");
                    return false;
                }

                if (rb_wait_alternative_executable.Checked && String.IsNullOrWhiteSpace(txt_alternative_executable.Text))
                {
                    logger.Error($"ShortcutForm/CanEnableSaveButton: The user asked to wait for an alternative application executable, but failed to provide one!");
                    return false;
                }

                if (rb_wait_alternative_executable.Checked && !File.Exists(txt_alternative_executable.Text))
                {
                    logger.Error($"ShortcutForm/CanEnableSaveButton: The alternative application executable the user wants to monitor doesn't exist. Please check the file '{txt_alternative_executable.Text}' is still there, and that the file has the correct permissions.");
                    return false;
                }

            }
            else if (rb_launcher.Checked)
            {

                if (cb_args_game.Checked && String.IsNullOrWhiteSpace(txt_args_game.Text))
                {
                    logger.Error($"ShortcutForm/CanEnableSaveButton: The user wanted to pass arguments to the game executable, but failed to provide any!");
                    return false;
                }

                if (_gameId.Equals("0"))
                {
                    logger.Error($"ShortcutForm/CanEnableSaveButton: The game ID provided is 0, and this is invalid. We cannot run the game.");
                    return false;
                }

                bool gameStillInstalled = false;
                foreach (ImageListViewItem gameItem in ilv_games.Items)
                {
                    if (gameItem.Text.Equals(txt_game_name.Text))
                    {
                        gameStillInstalled = true;
                        break;
                    }

                }
                if (!gameStillInstalled)
                {
                    logger.Error($"ShortcutForm/CanEnableSaveButton: The {_gameLauncher} game with ID {_gameId} isn't installed at present, so can't be used!");
                    return false;
                }

                if (cb_wait_alternative_game.Checked && String.IsNullOrWhiteSpace(txt_alternative_game.Text))
                {
                    logger.Error($"ShortcutForm/CanEnableSaveButton: The user asked to wait for an alternative game executable, but failed to provide one!");                    
                    return false;
                }

                if (cb_wait_alternative_game.Checked && !File.Exists(txt_alternative_game.Text))
                {
                    logger.Error($"ShortcutForm/CanEnableSaveButton: The alternative game executable the user wants to monitor doesn't exist. Please check the file '{txt_alternative_game.Text}' is still there, and that the file has the correct permissions.");
                    return false;
                }

            }

            // Look for any start programs without an exe
            if (_shortcutToEdit.StartPrograms.Count > 0)
            {
                List<StartProgram> startProgramsToStartWithoutExe = _shortcutToEdit.StartPrograms.Where(program => program.Executable == null).ToList();
                if (startProgramsToStartWithoutExe.Count > 0)
                {
                    foreach (StartProgram myStartProgram in startProgramsToStartWithoutExe)
                    {
                        logger.Error($"ShortcutForm/CanEnableSaveButton: The start program at position #{myStartProgram.Priority} doesn't have an executable listed");
                    }
                    return false;
                }
            }            

            // Check the stop program has an exe in there
            if (cb_run_cmd_afterwards.Checked && String.IsNullOrWhiteSpace(txt_run_cmd_afterwards.Text))
            {
                logger.Error($"ShortcutForm/CanEnableSaveButton: The run command afterwards command is selected, yet doesn't have an executable listed");
                return false;
            }

            return true;
        }

        private void EnableSaveButtonIfValid()
        {
            if (CanEnableSaveButton())
                btn_save.Enabled = true;
            else
                btn_save.Enabled = false;

        }

        private void SuggestShortcutName()
        {
            if (_autoName && _profileToUse is ProfileItem)
            {
                if (rb_no_game.Checked)
                {
                    if (rb_switch_display_permanent.Checked)
                        txt_shortcut_save_name.Text = $"{_profileToUse.Name}";
                    else if (rb_switch_display_temp.Checked)
                        txt_shortcut_save_name.Text = $"{_profileToUse.Name} (Temporary)";
                }
                else if (rb_launcher.Checked && ilv_games.SelectedItems.Count > 0 && _selectedGame != null)
                {
                    txt_shortcut_save_name.Text = $"{_selectedGame.Name} ({_profileToUse.Name})";                    
                }
                else if (rb_standalone.Checked && txt_executable.Text.Length > 0)
                {
                    if (_selectedApp is App)
                    {                        
                        txt_shortcut_save_name.Text = $"{_selectedApp.Name} ({_profileToUse.Name})";
                    }
                    else
                    {
                        string baseName = Path.GetFileNameWithoutExtension(txt_executable.Text);
                        txt_shortcut_save_name.Text = $"{baseName} ({_profileToUse.Name})";
                    }                    
                }
                
            }
        }


        private void RefreshProfileImageListView(ProfileItem profile)
        {
            ilv_saved_profiles.ClearSelection();
            IEnumerable<ImageListViewItem> matchingImageListViewItems = (from item in ilv_saved_profiles.Items where item.Text == profile.Name select item);
            if (matchingImageListViewItems.Any())
            {
                ImageListViewItem itemToSelect = matchingImageListViewItems.First();
                itemToSelect.Selected = true;
                itemToSelect.Focused = true;
                itemToSelect.Enabled = true;
                ilv_saved_profiles.EnsureVisible(itemToSelect.Index);
            }
        }

        private void ClearForm()
        {
            // Reset all the tracking variables back to default
            //_loadedProfiles = new List<ProfileItem>();
            _profileToUse = null;
            _gameLauncher = "";
            
            //_gameToUse;
            // _executableToUse;
            _displayPermanence = ShortcutPermanence.Temporary;
            _audioPermanence = ShortcutPermanence.Temporary;
            _capturePermanence = ShortcutPermanence.Temporary;
            _startPrograms = new List<StartProgram>();
            _stopPrograms = new List<StopProgram>();
            _audioDevice = "";
            _changeAudioDevice = false;
            _setAudioVolume = false;
            _audioVolume = -1;
            _captureDevice = "";
            _changeCaptureDevice = false;
            _setCaptureVolume = false;
            _captureVolume = -1;
            _selectedGame = null;
            _isUnsaved = true;
            _loadedShortcut = false;
            _autoName = true;
            _gameId = "0";
            _uuid = "";
            audioDevices = null;
            selectedAudioDevice = null;
            captureDevices = null;
            selectedCaptureDevice = null;
            _hotkey = Keys.None;


            // Prepare the Game process priority combo box
            cbx_game_priority.DataSource = new ComboItem[] {
                    new ComboItem{ Value = ProcessPriority.High, Text = "High" },
                    new ComboItem{ Value = ProcessPriority.AboveNormal, Text = "Above Normal" },
                    new ComboItem{ Value = ProcessPriority.Normal, Text = "Normal" },
                    new ComboItem{ Value = ProcessPriority.BelowNormal, Text = "Below Normal" },
                    new ComboItem{ Value = ProcessPriority.Idle, Text = "Idle" },
                };
            cbx_game_priority.ValueMember = "Value";
            cbx_game_priority.DisplayMember = "Text";
            cbx_game_priority.SelectedIndex = 2; //Normal
            cbx_game_priority.Enabled = true;

            // Prepare the exe process priority combo box
            cbx_exe_priority.DataSource = new ComboItem[] {
                    new ComboItem{ Value = ProcessPriority.High, Text = "High" },
                    new ComboItem{ Value = ProcessPriority.AboveNormal, Text = "Above Normal" },
                    new ComboItem{ Value = ProcessPriority.Normal, Text = "Normal" },
                    new ComboItem{ Value = ProcessPriority.BelowNormal, Text = "Below Normal" },
                    new ComboItem{ Value = ProcessPriority.Idle, Text = "Idle" },
                };
            cbx_exe_priority.ValueMember = "Value";
            cbx_exe_priority.DisplayMember = "Text";
            cbx_exe_priority.SelectedIndex = 2; //Normal
            cbx_exe_priority.Enabled = true;

            // Empty the selected game in case this is a reload
            txt_alternative_executable.Text = "";


            // Populate all the Profiles in the profile listview
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
                        newItem = new ImageListViewItem(loadedProfile, loadedProfile.Name);
                        ilv_saved_profiles.Items.Add(newItem, _profileAdaptor);
                    }

                }

                // Restart updating the saved_profiles listview
                ilv_saved_profiles.ResumeLayout();
            }
            

            // Populate all the audio devices in the audio devices select box
            if (audioController != null)
            {
                cb_audio_device.Items.Clear();
                try
                {
                    audioDevices = audioController.GetPlaybackDevices(DeviceState.Active).ToList();
                    if (audioDevices != null && audioDevices.Count > 0)
                    {
                        
                        // we populate the audio devce list 
                        foreach (CoreAudioDevice audioDevice in audioDevices)
                        {
                            int index = cb_audio_device.Items.Add(audioDevice.FullName);
                            // Set the audio device to the default device by default
                            if (audioDevice.IsDefaultDevice)
                            {
                                selectedAudioDevice = audioDevice;
                                cb_audio_device.SelectedIndex = index;
                                nud_audio_volume.Value = Convert.ToDecimal(audioDevice.Volume);
                            }
                        }
                        rb_keep_audio_volume.Checked = true;                       
                    }
                    else
                    {
                        // There are no active audio devices found
                        // so we hide all audio changing controls
                        gb_audio_settings.Visible = false;
                        lbl_no_active_audio_devices.Visible = true;
                        logger.Warn($"ShortcutForm/ShortcutForm_Load: No active playback devices so hiding the audio output controls.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutForm/ShortcutForm_Load: Exception while trying to get active playback devices.");
                }                


                // Populate all the Capture devices in the capture devices list.
                cb_capture_device.Items.Clear();
                try
                {
                    captureDevices = audioController.GetCaptureDevices(DeviceState.Active).ToList();
                    if (captureDevices != null && captureDevices.Count > 0)
                    {
                        
                        // Then we need to populate the list 
                        foreach (CoreAudioDevice captureDevice in captureDevices)
                        {
                            int index = cb_capture_device.Items.Add(captureDevice.FullName);
                            // Set the capture device to the default device by default
                            if (captureDevice.IsDefaultDevice)
                            {
                                selectedCaptureDevice = captureDevice;
                                cb_capture_device.SelectedIndex = index;
                                nud_capture_volume.Value = Convert.ToDecimal(captureDevice.Volume);
                            }
                        }
                        rb_keep_capture_volume.Checked = true;                      
                    }
                    else
                    {
                        // There are no active audio devices found
                        // so we hide all audio changing controls
                        gb_capture_settings.Visible = false;
                        lbl_no_active_capture_devices.Visible = true;
                        logger.Warn($"ShortcutForm/ShortcutForm_Load: No active capture devices so hiding the microphone input controls.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutForm/ShortcutForm_Load: Exception while trying to get active capture devices.");
                }
            }
            else
            {
                // Audio Controller == null, so the audio device isn't supported by AudioSwitcher.CoreAudio!
                // We just have to disable the switching functionality at present :(
                // Hopefully I find another library that works with everything including RealTek Audio
                gb_audio_settings.Visible = false;
                gb_capture_settings.Visible = false;
                lbl_disabled_shortcut_audio_chipset.Visible = true;

                // We also force the audio settings to off, just in case
                rb_change_audio.Checked = false;
                rb_set_audio_volume.Checked = false;
                rb_change_capture.Checked = false;
                rb_set_capture_volume.Checked = false;
            }          


            // Clear the textboxes
            txt_alternative_executable.Text = "";
            txt_alternative_game.Text = "";
            txt_args_executable.Text = "";
            txt_args_game.Text = "";
            txt_executable.Text = "";
            txt_game_name.Text = "Please select a game from the list below...";
            txt_run_cmd_afterwards.Text = "";
            txt_run_cmd_afterwards_args.Text = "";
            txt_shortcut_save_name.Text = "";

            // Set the radio buttons to default
            rb_no_change_audio.Checked = true;
            rb_change_audio.Checked = false;
            rb_keep_audio_volume.Checked = true;
            rb_set_audio_volume.Checked = false;
            rb_change_capture.Checked = false;
            rb_keep_capture_volume.Checked = true;
            rb_no_change_capture.Checked = true;
            rb_set_capture_volume.Checked = false;            

            // Set the game mode on load
            rb_launcher.Checked = true;
            rb_no_game.Checked = false;
            rb_standalone.Checked = false;

            // Set the checkboxes
            cb_args_executable.Checked = false;
            cb_args_game.Checked = false;
            cb_autosuggest.Checked = true;
            cb_run_cmd_afterwards.Checked = false;
            cb_run_cmd_afterwards_args.Checked = false;
            cb_wait_alternative_game.Checked = false;
            cb_run_exe_as_administrator.Checked = false;

            // Wipe the start programs flp
            flp_start_programs.Controls.Clear();
            
            // Wipe the pictureboxes if they're in use
            if (pb_exe_icon.Image != null)
            {
                pb_exe_icon.Image = null;
            }
            if (pb_game_icon.Image != null)
            {
                pb_game_icon.Image = null;
            }

            // Select the DisplayProfile tab
            tabc_shortcut.SelectedTab = tabp_display;
        }

        private void LoadShortcut()
        {
            // Load all the profiles to prepare things
            bool foundChosenProfileInLoadedProfiles = false;
            ProfileItem chosenProfile = null;

            // Close the splash screen
            CloseTheSplashScreen();

            // =============================================
            // CLEAR THE FORM
            // =============================================
            ClearForm();

            // =============================================
            // SETTING COMMON VARIABLES
            // =============================================

            // *** Hidden Shortcut variables ***
            // Track the shortcut UUID
            _uuid = _shortcutToEdit.UUID;

            // Populate all the Games into the Games ListView            
            ilv_games.Enabled = true;
            ilv_games.Visible = true;
            ilv_games.SuspendLayout();
            ilv_games.Items.Clear();
            foreach (var game in DisplayMagician.GameLibraries.GameLibrary.AllInstalledGamesInAllLibraries.OrderBy(game => game.Name))
            {
                // Add the game to the game array
                ImageListViewItem newItem = new ImageListViewItem(game, game.Name);
                ilv_games.Items.Add(newItem, _gameAdaptor);
                //newItem.Update();
            }
            // Make sure that if the item is selected that it's visible
            if (ilv_games.SelectedItems.Count > 0)
            {
                int selectedIndex = ilv_games.SelectedItems[0].Index;
                ilv_games.EnsureVisible(selectedIndex);
            }
            ilv_games.Update();
            ilv_games.ResumeLayout();
            Application.DoEvents();

            // =============================================
            // IF THE SHORTCUT IS AN EXISTING SHORTCUT
            // =============================================
            if (_editingExistingShortcut)
            {
                // *** Main Shortcut controls ***
                // Set the shortcut name
                txt_shortcut_save_name.Text = _shortcutToEdit.Name;
                // Set the autoname checkbox
                cb_autosuggest.Checked = _shortcutToEdit.AutoName;
                // Set the Hotkey text
                UpdateHotkeyLabel(_shortcutToEdit.Hotkey);

                // *** 1. Choose Display Profile Tab ***
                // Find the profile
                if (ProfileRepository.ContainsProfile(_shortcutToEdit.ProfileUUID))
                {
                    // We have loaded the profile used last time
                    // so we need to show the selected profile in the UI
                    chosenProfile = ProfileRepository.GetProfile(_shortcutToEdit.ProfileUUID);
                    foundChosenProfileInLoadedProfiles = true;

                    // If the profile is the same, but the user has renamed the profile
                    // since the shortcut was last created, then we need to tell the user
                    if (!chosenProfile.IsPossible)
                    {
                        MessageBox.Show(
                        $"The '{chosenProfile.Name}' Display Profile used by this Shortcut still exists, but it isn't possible to use it right now. You can either change the Display Profile this Shortcut uses, or you can change your Displays to make the Display Profile valid again.",
                        @"Display Profile isn't possible now",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    }

                }

                if (!foundChosenProfileInLoadedProfiles && !String.IsNullOrWhiteSpace(_shortcutToEdit.ProfileUUID))
                {
                    MessageBox.Show(
                        @"The Display Profile used by this Shortcut no longer exists and cannot be used. You need to choose a new Display Profile for this Shortcut.",
                        @"Display Profile no longer exists",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
                // If we get to the end of the loaded profiles and haven't
                // found a matching profile, then we need to show the current profile
                // that we're running now (only if that's been saved)
                else if (!foundChosenProfileInLoadedProfiles && ProfileRepository.ProfileCount > 0)
                {
                    ProfileItem currentProfile = ProfileRepository.GetActiveProfile();
                    bool foundCurrentProfile = false;
                    foreach (ProfileItem profileToCheck in ProfileRepository.AllProfiles)
                    {
                        if (profileToCheck.Equals(currentProfile))
                        {
                            chosenProfile = currentProfile;
                            foundCurrentProfile = true;
                        }
                    }

                    // If we get here, and we still haven't matched the profile, then just pick the first one
                    if (!foundCurrentProfile)
                    {
                        if (ProfileRepository.ProfileCount > 0)
                        {
                            chosenProfile = ProfileRepository.AllProfiles[0];
                        }

                    }

                }

                _profileToUse = chosenProfile;

                // *** 2. Choose Audio Tab ***
                // Populate all the Audio devices in the audio devices list.
                // Set the Audio device to the shortcut audio device only if 
                // the Change Audio radiobutton is set
                if (audioController != null)
                {
                    rb_change_audio.Checked = _shortcutToEdit.ChangeAudioDevice;
                    if (audioDevices != null && audioDevices.Count > 0)
                    {
                        // If the shortcut is to change the audio device
                        if (_shortcutToEdit.ChangeAudioDevice)
                        {
                            // Then we need to select the item in the list if it exists
                            bool foundAudioDevice = false;
                            bool foundAudioDeviceList = false;
                            int cbIndex = cb_audio_device.FindStringExact(_shortcutToEdit.AudioDevice);
                            if (cbIndex >= 0)
                            {
                                foundAudioDeviceList = true;
                                cb_audio_device.SelectedIndex = cbIndex;
                                foreach (CoreAudioDevice audioDevice in audioDevices)
                                {
                                    if (audioDevice.FullName.Equals(_shortcutToEdit.AudioDevice))
                                    {
                                        selectedAudioDevice = audioDevice;
                                        foundAudioDevice = true;
                                    }
                                }
                                if (_shortcutToEdit.SetAudioVolume && _shortcutToEdit.AudioVolume >= 0 && _shortcutToEdit.AudioVolume <= 100)
                                {
                                    nud_audio_volume.Value = _shortcutToEdit.AudioVolume;
                                    rb_set_audio_volume.Checked = true;
                                }
                                else
                                {
                                    nud_audio_volume.Value = Convert.ToDecimal(selectedAudioDevice.Volume);
                                    rb_set_audio_volume.Checked = false;
                                }
                            }

                            // We need to handle the edgecase where the selected audio device
                            // isn't currently plugged in. We don't want to break the shortcut
                            // as it could be plugged in when it comes time to actually run
                            // the shortcut, so we need to just add it to the list to not break
                            // the UI.

                            if (!foundAudioDevice || !foundAudioDeviceList)
                            {
                                int dIndex = cb_audio_device.Items.Add(_shortcutToEdit.AudioDevice);
                                cb_audio_device.SelectedIndex = dIndex;
                                selectedAudioDevice = null;
                                if (_shortcutToEdit.SetAudioVolume && _shortcutToEdit.AudioVolume >= 0 && _shortcutToEdit.AudioVolume <= 100)
                                {
                                    rb_set_audio_volume.Checked = true;
                                    nud_audio_volume.Value = _shortcutToEdit.AudioVolume;
                                }
                                else
                                {
                                    rb_keep_audio_volume.Checked = true;
                                    nud_audio_volume.Value = 50;
                                }
                            }
                        }
                    }
                    else
                    {
                        // There are no active audio devices found
                        // so we hide all audio changing controls
                        gb_audio_settings.Visible = false;
                        lbl_no_active_audio_devices.Visible = true;
                        logger.Warn($"ShortcutForm/ShortcutForm_Load: No active playback devices so hiding the audio output controls.");
                    }


                    // Populate all the Capture devices in the capture devices list.
                    // Set the Capture device to the shortcut capture device only if 
                    // the Change Capture radiobutton is set
                    rb_change_capture.Checked = _shortcutToEdit.ChangeCaptureDevice;
                    if (captureDevices != null && captureDevices.Count > 0)
                    {
                        // If the shortcut is to change the capture device
                        if (_shortcutToEdit.ChangeCaptureDevice)
                        {
                            // Then we need to select the item in the list if it exists
                            bool foundCaptureDevice = false;
                            bool foundCaptureDeviceList = false;
                            int cbIndex = cb_capture_device.FindStringExact(_shortcutToEdit.CaptureDevice);
                            if (cbIndex >= 0)
                            {
                                foundCaptureDeviceList = true;
                                cb_capture_device.SelectedIndex = cbIndex;
                                foreach (CoreAudioDevice captureDevice in captureDevices)
                                {
                                    if (captureDevice.FullName.Equals(_shortcutToEdit.CaptureDevice))
                                    {
                                        selectedCaptureDevice = captureDevice;
                                        foundCaptureDevice = true;
                                    }
                                }
                                if (_shortcutToEdit.SetCaptureVolume && _shortcutToEdit.CaptureVolume >= 0 && _shortcutToEdit.CaptureVolume <= 100)
                                {
                                    nud_capture_volume.Value = _shortcutToEdit.CaptureVolume;
                                    rb_set_capture_volume.Checked = true;
                                }
                                else
                                {
                                    nud_capture_volume.Value = Convert.ToDecimal(selectedCaptureDevice.Volume);
                                    rb_set_capture_volume.Checked = false;
                                }
                            }

                            // We need to handle the edgecase where the selected capture device
                            // isn't currently plugged in. We don't want to break the shortcut
                            // as it could be plugged in when it comes time to actually run
                            // the shortcut, so we need to just add it to the list to not break
                            // the UI.

                            if (!foundCaptureDevice || !foundCaptureDeviceList)
                            {
                                int dIndex = cb_capture_device.Items.Add(_shortcutToEdit.CaptureDevice);
                                cb_capture_device.SelectedIndex = dIndex;
                                selectedCaptureDevice = null;
                                if (_shortcutToEdit.SetCaptureVolume && _shortcutToEdit.CaptureVolume >= 0 && _shortcutToEdit.CaptureVolume <= 100)
                                {
                                    rb_set_capture_volume.Checked = true;
                                    nud_capture_volume.Value = _shortcutToEdit.CaptureVolume;
                                }
                                else
                                {
                                    rb_keep_capture_volume.Checked = true;
                                    nud_capture_volume.Value = 50;
                                }
                            }
                        }
                    }
                    else
                    {
                        // There are no active audio devices found
                        // so we hide all audio changing controls
                        gb_capture_settings.Visible = false;
                        lbl_no_active_capture_devices.Visible = true;
                        logger.Warn($"ShortcutForm/ShortcutForm_Load: No active capture devices so hiding the microphone input controls.");
                    }

                }

                // *** 3. Choose what happens before Tab ***
                // Set up the start programs
                if (_shortcutToEdit.StartPrograms is List<StartProgram> && _shortcutToEdit.StartPrograms.Count > 0)
                {
                    flp_start_programs.Controls.Clear();

                    Padding firstStartProgramMargin = new Padding(10) { };
                    Padding otherStartProgramMargin = new Padding(10, 0, 10, 10) { };

                    // Order the inital list in order of priority
                    int spOrder = 1;
                    foreach (StartProgram myStartProgram in _shortcutToEdit.StartPrograms.OrderBy(sp => sp.Priority))
                    {
                        if (String.IsNullOrWhiteSpace(myStartProgram.Executable))
                        {
                            logger.Warn($"ShortcutForm/ShortcutForm_Load: Start program #{myStartProgram.Priority} is empty, so skipping.");
                            continue;
                        }                        

                        StartProgramControl startProgramControl = new StartProgramControl(myStartProgram, spOrder);
                        startProgramControl.Dock = DockStyle.None;
                        if (spOrder == 1)
                        {
                            startProgramControl.Margin = firstStartProgramMargin;
                        }
                        else
                        {
                            startProgramControl.Margin = otherStartProgramMargin;
                        }
                        startProgramControl.Width = flp_start_programs.Width - 40;
                        startProgramControl.MouseDown += new MouseEventHandler(StartProgramControl_MouseDown);
                        startProgramControl.DragOver += new DragEventHandler(StartProgramControl_DragOver);
                        startProgramControl.DragDrop += new DragEventHandler(StartProgramControl_DragDrop);
                        startProgramControl.AllowDrop = true;
                        flp_start_programs.Controls.Add(startProgramControl);
                        spOrder++;
                    }
                }
                else
                {
                    flp_start_programs.Controls.Clear();
                }

                // *** 4. Choose Game to Start ***
                // Set the shortcut category
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


                // =============================================
                // IF THE EXISTING SHORTCUT IS AN APPLICATION
                // =============================================
                if (_shortcutToEdit.Category == ShortcutCategory.Application)
                {

                    // Check that the executable to run still exists
                    if (!String.IsNullOrWhiteSpace(_shortcutToEdit.ExecutableNameAndPath) && !File.Exists(_shortcutToEdit.ExecutableNameAndPath))
                    {
                        MessageBox.Show(
                        $"The '{_shortcutToEdit.ExecutableNameAndPath}' application used by this Shortcut no longer exists. Your shortcut won't work unless you reinstall the missing application or choose a different one.",
                        @"Application doesn't exist",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    }

                    // If we don't have any available images, then we need to get some
                    if (_shortcutToEdit.AvailableImages.Count > 0)
                    {
                        _availableImages = _shortcutToEdit.AvailableImages;
                    }
                    else
                    {                        
                        _availableImages = new List<ShortcutBitmap>();
                        // If the exe is selected, then grab images from the exe
                        _availableImages.AddRange(ImageUtils.GetMeAllBitmapsFromFile(_shortcutToEdit.ExecutableNameAndPath));
                        // If the different exe to monitor is set, then grab the icons from there too!
                        if (!String.IsNullOrWhiteSpace(_shortcutToEdit.DifferentExecutableToMonitor) && File.Exists(_shortcutToEdit.DifferentExecutableToMonitor))
                        {
                            _availableImages.AddRange(ImageUtils.GetMeAllBitmapsFromFile(_shortcutToEdit.DifferentExecutableToMonitor));
                        }

                        if (_availableImages.Count == 0)
                        {
                            logger.Trace($"ShortcutForm/ShortcutForm_Load: Unknown Game Library, so using the DisplayMagician icon as the icon instead.");
                            ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.DisplayMagician.ToBitmap(), "DisplayMagician Icon", "", 0);
                            _availableImages.Add(bm);

                        }

                        bool matchedImage = false;
                        if (_shortcutToEdit.OriginalLargeBitmap != null)
                        {
                            // go through available images and match the one we had
                            foreach (ShortcutBitmap sc in _availableImages)
                            {
                                if (ImageUtils.ImagesAreEqual(sc.Image, _shortcutToEdit.OriginalLargeBitmap))
                                {
                                    // We've found the original image!
                                    _selectedImage = sc;
                                    pb_exe_icon.Image = _selectedImage.Image;
                                    matchedImage = true;
                                }
                            }
                        }

                        if (!matchedImage)
                        {
                            _selectedImage = ImageUtils.GetMeLargestAvailableBitmap(_availableImages);
                           pb_exe_icon.Image = _selectedImage.Image;
                        }

                        if (_shortcutToEdit.OriginalLargeBitmap != null)
                        {
                            btn_choose_exe_icon.Enabled = true;
                        }
                    }

                    // If we have a selected image, then we need to set it
                    if (_shortcutToEdit.SelectedImage.Image != null)
                    {
                        _selectedImage = _shortcutToEdit.SelectedImage;
                        pb_exe_icon.Image = _shortcutToEdit.SelectedImage.Image;
                        btn_choose_exe_icon.Enabled = true;
                    }


                    // Set the executable items if we have them
                    txt_executable.Text = _shortcutToEdit.ExecutableNameAndPath;
                    nud_timeout_executable.Value = _shortcutToEdit.StartTimeout;
                    txt_args_executable.Text = _shortcutToEdit.ExecutableArguments;
                    cbx_exe_priority.SelectedValue = _shortcutToEdit.ProcessPriority;
                    if (_shortcutToEdit.RunExeAsAdministrator)
                    {
                        cb_run_exe_as_administrator.Checked = true;
                    }
                    else
                    {
                        cb_run_exe_as_administrator.Checked = false;
                    }
                    if (_shortcutToEdit.ExecutableArgumentsRequired)
                    {
                        cb_args_executable.Checked = true;
                    }
                    else
                    {
                        cb_args_executable.Checked = false;
                    }
                    if (_shortcutToEdit.ProcessNameToMonitorUsesExecutable)
                    {
                        rb_wait_executable.Checked = true;
                    }
                    else
                    {
                        rb_wait_alternative_executable.Checked = true;
                    }
                    txt_alternative_executable.Text = _shortcutToEdit.DifferentExecutableToMonitor;

                }
                // =============================================
                // IF THE EXISTING SHORTCUT IS A GAME
                // =============================================
                else if (_shortcutToEdit.Category == ShortcutCategory.Game)
                {
                    // Set up the game launcher radio button
                    rb_launcher.Checked = true;

                    // Show an error message if there isn't a game launcher selected
                    if (_shortcutToEdit.GameLibrary.Equals(SupportedGameLibraryType.Unknown))
                    {
                        if (GameLibraries.GameLibrary.AllInstalledGamesInAllLibraries.Count <= 0)
                        {
                            // Fill in the game library information to highlight there isn't one detected.
                            _gameLauncher = "None detected";
                            txt_game_name.Text = "No supported game libraries detected";
                            txt_args_game.Text = "";

                            // Disable the Game library option, and select the Executable option instead.
                            p_game.Enabled = false;
                            p_game.Visible = false;
                            rb_wait_executable.Checked = true;
                            rb_launcher.Enabled = false;
                            rb_launcher.Visible = false;
                            lbl_no_game_libraries.Visible = true;

                        }
                    }
                    else
                    {
                        // Set the launcher items if we have them
                        _gameLauncher = _shortcutToEdit.GameLibrary.ToString("G");
                        txt_game_name.Text = _shortcutToEdit.GameName;
                        _gameId = _shortcutToEdit.GameAppId;
                        nud_timeout_game.Value = _shortcutToEdit.StartTimeout;
                        txt_args_game.Text = _shortcutToEdit.GameArguments;
                        cbx_game_priority.SelectedValue = _shortcutToEdit.ProcessPriority;
                        lbl_game_library.Text = $"Game Library: {_gameLauncher}";
                        if (_shortcutToEdit.GameArgumentsRequired)
                        {
                            cb_args_game.Checked = true;
                        }
                    }

                    if (_shortcutToEdit.GameAppId != null)
                    {
                        bool gameStillInstalled = false;
                        foreach (ImageListViewItem gameItem in ilv_games.Items)
                        {
                            if (gameItem.Text.Equals(_shortcutToEdit.GameName))
                            {
                                gameStillInstalled = true;
                                gameItem.Selected = true;
                                ilv_games.EnsureVisible(gameItem.Index);
                                break;
                            }

                        }
                        if (!gameStillInstalled)
                        {                            
                            DialogResult result = MessageBox.Show(
                                $"This shortcut refers to the '{_shortcutToEdit.GameName}' game that was installed in your {_shortcutToEdit.GameLibrary.ToString("G")} library. This game is no longer installed, so the shortcut won't work. You either need to change the game used in the Shortcut to another installed game, or you need to install the game files on your computer again.",
                                @"Game no longer exists",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                        }

                        foreach (Game game in GameLibraries.GameLibrary.AllInstalledGamesInAllLibraries)
                        {
                            if (game.Name == _shortcutToEdit.GameName)
                            {
                                _selectedGame = game;
                                break;
                            }
                        }
                    }

                    
                    // Monitor the alternative game exe if we have it
                    if (_shortcutToEdit.MonitorDifferentGameExe)
                    {
                        cb_wait_alternative_game.Checked = true;
                        if (!String.IsNullOrWhiteSpace(_shortcutToEdit.DifferentGameExeToMonitor))
                        {
                            txt_alternative_game.Text = _shortcutToEdit.DifferentGameExeToMonitor;
                        }
                    }
                    else
                    {
                        cb_wait_alternative_game.Checked = false;
                    }

                    // If we don't have any available images, then we need to get some
                    if (_shortcutToEdit.AvailableImages.Count > 0)
                    {
                        _availableImages = _shortcutToEdit.AvailableImages;
                    }
                    else
                    {
                        // If this is a shortcut we're editing
                        _availableImages = new List<ShortcutBitmap>();
                        // If the game is selected, then grab images from the game
                        if (_selectedGame != null)
                        {
                            _availableImages.AddRange(ImageUtils.GetMeAllBitmapsFromFile(_selectedGame.IconPath));
                            if (_selectedGame.ExePath != _selectedGame.IconPath)
                            {
                                _availableImages.AddRange(ImageUtils.GetMeAllBitmapsFromFile(_selectedGame.ExePath));
                            }

                        }
                        // If the different exe to monitor is set, then grab the icons from there too!
                        if (!String.IsNullOrWhiteSpace(_shortcutToEdit.DifferentGameExeToMonitor) && File.Exists(_shortcutToEdit.DifferentGameExeToMonitor))
                        {
                            _availableImages.AddRange(ImageUtils.GetMeAllBitmapsFromFile(_shortcutToEdit.DifferentGameExeToMonitor));
                        }

                        // If we still don't have any availableImages, then we need to add some emergency replacements!
                        if (_availableImages.Count == 0)
                        {
                            if (_shortcutToEdit.GameLibrary == SupportedGameLibraryType.Steam)
                            {
                                logger.Trace($"ShortcutForm/ShortcutForm_Load: Using the Steam icon as the icon instead.");
                                ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Steam, "Steam Icon", "", 0);
                                _availableImages.Add(bm);
                            }
                            else if (_shortcutToEdit.GameLibrary == SupportedGameLibraryType.Uplay)
                            {
                                logger.Trace($"ShortcutForm/ShortcutForm_Load: Using the Uplay icon as the icon instead.");
                                ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Uplay, "Uplay Icon", "", 0);
                                _availableImages.Add(bm);
                            }
                            else if (_shortcutToEdit.GameLibrary == SupportedGameLibraryType.Origin)
                            {
                                logger.Trace($"ShortcutForm/ShortcutForm_Load: Using the Origin icon as the icon instead.");
                                ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Origin, "Origin Icon", "", 0);
                                _availableImages.Add(bm);
                            }
                            else if (_shortcutToEdit.GameLibrary == SupportedGameLibraryType.Epic)
                            {
                                logger.Trace($"ShortcutForm/ShortcutForm_Load: Using the Epic icon as the icon instead.");
                                ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.Epic, "Epic Icon", "", 0);
                                _availableImages.Add(bm);
                            }
                            else if (_shortcutToEdit.GameLibrary == SupportedGameLibraryType.GOG)
                            {
                                logger.Trace($"ShortcutForm/ShortcutForm_Load: Using the GOG icon as the icon instead.");
                                ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.GOG, "GOG Icon", "", 0);
                                _availableImages.Add(bm);
                            }
                            else
                            {
                                logger.Trace($"ShortcutForm/ShortcutForm_Load: Unknown Game Library, so using the DisplayMagician icon as the icon instead.");
                                ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.DisplayMagician.ToBitmap(), "DisplayMagician Icon", "", 0);
                                _availableImages.Add(bm);
                            }

                        }

                        bool matchedImage = false;
                        if (_shortcutToEdit.OriginalLargeBitmap != null)
                        {
                            // go through available images and match the one we had
                            foreach (ShortcutBitmap sc in _availableImages)
                            {
                                if (ImageUtils.ImagesAreEqual(sc.Image, _shortcutToEdit.OriginalLargeBitmap))
                                {
                                    // We've found the original image!
                                    _selectedImage = sc;
                                    pb_game_icon.Image = _selectedImage.Image;
                                    matchedImage = true;
                                    break;
                                }
                            }
                        }

                        if (!matchedImage)
                        {
                            _selectedImage = ImageUtils.GetMeLargestAvailableBitmap(_availableImages);
                            pb_game_icon.Image = _selectedImage.Image;
                        }

                        if (_shortcutToEdit.OriginalLargeBitmap != null)
                        {
                            btn_choose_game_icon.Enabled = true;
                        }
                    }

                    // If we have a selected image, then we need to set it
                    if (_shortcutToEdit.SelectedImage.Image != null)
                    {
                        // Set up the selected Game Image
                        _selectedImage = _shortcutToEdit.SelectedImage;
                        pb_game_icon.Image = _shortcutToEdit.SelectedImage.Image;
                        btn_choose_game_icon.Enabled = true;
                    }

                }
                // ==================================================
                // IF THE EXISTING SHORTCUT IS NO GAME OR APPLICATION
                // ==================================================
                else
                {
                    // Set up the selected images if we have some available images
                    if (_shortcutToEdit.AvailableImages.Count > 0)
                    {
                        _selectedImage = _shortcutToEdit.SelectedImage;
                        _availableImages = _shortcutToEdit.AvailableImages;
                    }
                }

                // *** 5. Choose what happens afterwards tab ***
                switch (_shortcutToEdit.DisplayPermanence)
                {
                    case ShortcutPermanence.Permanent:
                        rb_switch_display_permanent.Checked = true;
                        break;
                    case ShortcutPermanence.Temporary:
                        rb_switch_display_temp.Checked = true;
                        break;
                }

                switch (_shortcutToEdit.AudioPermanence)
                {
                    case ShortcutPermanence.Permanent:
                        rb_switch_audio_permanent.Checked = true;
                        break;
                    case ShortcutPermanence.Temporary:
                        rb_switch_audio_temp.Checked = true;
                        break;
                }

                switch (_shortcutToEdit.CapturePermanence)
                {
                    case ShortcutPermanence.Permanent:
                        rb_switch_capture_permanent.Checked = true;
                        break;
                    case ShortcutPermanence.Temporary:
                        rb_switch_capture_temp.Checked = true;
                        break;
                }

                // Setup the single stop program we're beginning with
                if (_shortcutToEdit.StopPrograms is List<StopProgram> && _shortcutToEdit.StopPrograms.Count > 0)
                {
                    if (_shortcutToEdit.StopPrograms[0].Disabled == false)
                    {
                        txt_run_cmd_afterwards.Enabled = true;
                        btn_run_cmd_afterwards.Enabled = true;
                        cb_run_cmd_afterwards_args.Enabled = true;
                        cb_run_cmd_afterwards_dont_start.Enabled = true;
                        cb_run_cmd_afterwards_run_as_administrator.Enabled = true;
                        cb_run_cmd_afterwards.Checked = true;
                    }
                    else
                    {
                        txt_run_cmd_afterwards.Enabled = false;
                        btn_run_cmd_afterwards.Enabled = false;
                        cb_run_cmd_afterwards_args.Enabled = false;
                        cb_run_cmd_afterwards_dont_start.Enabled = false;
                        cb_run_cmd_afterwards_run_as_administrator.Enabled = false;
                        cb_run_cmd_afterwards.Checked = false;
                    }

                    txt_run_cmd_afterwards.Text = _shortcutToEdit.StopPrograms[0].Executable;
                    if (_shortcutToEdit.StopPrograms[0].ExecutableArgumentsRequired)
                    {
                        cb_run_cmd_afterwards_args.Checked = true;
                        txt_run_cmd_afterwards_args.Text = _shortcutToEdit.StopPrograms[0].Arguments;
                    }
                    else
                    {
                        
                        cb_run_cmd_afterwards_args.Checked = false;
                        txt_run_cmd_afterwards_args.Text = "";
                        
                    }
                    if (_shortcutToEdit.StopPrograms[0].DontStartIfAlreadyRunning)
                    {
                        cb_run_cmd_afterwards_dont_start.Checked = true;
                    }
                    else
                    {
                        cb_run_cmd_afterwards_dont_start.Checked = false;
                    }
                    if (_shortcutToEdit.StopPrograms[0].RunAsAdministrator)
                    {
                        cb_run_cmd_afterwards_run_as_administrator.Checked = true;
                    }
                    else
                    {
                        cb_run_cmd_afterwards_run_as_administrator.Checked = false;
                    }                    

                }
                else
                {
                    txt_run_cmd_afterwards.Enabled = false;
                    btn_run_cmd_afterwards.Enabled = false;
                    cb_run_cmd_afterwards_args.Enabled = false;
                    cb_run_cmd_afterwards_dont_start.Enabled = false;
                    cb_run_cmd_afterwards_run_as_administrator.Enabled = false;

                    cb_run_cmd_afterwards.Checked = false;
                    cb_run_cmd_afterwards_args.Checked = false;
                    cb_run_cmd_afterwards_dont_start.Checked = false;
                    cb_run_cmd_afterwards_run_as_administrator.Checked = false;
                }


            }
            // =============================================
            // IF THE SHORTCUT IS A NEW SHORTCUT
            // =============================================
            else
            {
                // Not sure if I need this
                //ilv_games.ClearSelection();
                
                // We need to show the current profile
                // that we're running now (only if that's been saved)
                if (ProfileRepository.ProfileCount > 0)
                {
                    ProfileItem currentProfile = ProfileRepository.GetActiveProfile();
                    bool foundCurrentProfile = false;
                    foreach (ProfileItem profileToCheck in ProfileRepository.AllProfiles)
                    {
                        if (profileToCheck.Equals(currentProfile))
                        {
                            chosenProfile = currentProfile;
                            foundCurrentProfile = true;
                        }
                    }

                    // If we get here, and we still haven't matched the profile, then just pick the first one
                    if (!foundCurrentProfile)
                    {
                        if (ProfileRepository.ProfileCount > 0)
                        {
                            chosenProfile = ProfileRepository.AllProfiles[0];
                        }

                    }
                    _profileToUse = chosenProfile;

                }

                // Set up the new shortcut as a game
                _shortcutToEdit.Category = ShortcutCategory.Game;
                rb_launcher.Checked = true;

                // Set up display permanance as temporary
                _shortcutToEdit.DisplayPermanence = ShortcutPermanence.Temporary;
                rb_switch_display_temp.Checked = true;

                // Set up audio permanance as temporary
                _shortcutToEdit.AudioPermanence = ShortcutPermanence.Temporary;
                rb_switch_audio_temp.Checked = true;

                // Set up capture permanance as temporary
                _shortcutToEdit.CapturePermanence = ShortcutPermanence.Temporary;
                rb_switch_capture_temp.Checked = true;
            }


            // Refresh the Shortcut UI
            RefreshShortcutUI();
            ChangeSelectedProfile(chosenProfile);
            //RefreshImageListView(chosenProfile);

            _loadedShortcut = true;
            _isUnsaved = false;

            // Finally enable the save button if it's still valid
            EnableSaveButtonIfValid();

        }

        private void ShortcutForm_Load(object sender, EventArgs e)
        {            

            if (_firstShow)
            {
                // Parse the game bitmaps now the first time as we need them
                // We need to add a refresh button to the shortcut page now!
                if (!GameLibraries.GameLibrary.GamesImagesLoaded)
                {
                    GameLibraries.GameLibrary.RefreshGameBitmaps();
                }
                if (!AppLibraries.AppLibrary.AppImagesLoaded)
                {
                    AppLibraries.AppLibrary.RefreshAppBitmaps();
                }



                _firstShow = false;
            }
            // Load the shortcut info
            LoadShortcut();

            CloseTheSplashScreen();

            this.Focus();
            this.BringToFront();
        }

        private void CloseTheSplashScreen()
        {
            // Close the splash screen
            if (Program.AppShortcutLoadingSplashScreen != null && !Program.AppShortcutLoadingSplashScreen.Disposing && !Program.AppShortcutLoadingSplashScreen.IsDisposed)
                Program.AppShortcutLoadingSplashScreen.Invoke(new Action(() => Program.AppShortcutLoadingSplashScreen.Close()));
            this.Activate();
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
                p_game_list.Enabled = false;
                pb_game_icon.Enabled = false;
                pb_game_icon.Image = ImageUtils.ConvertBitmapToGrayscale(pb_game_icon.Image);

                // Empty the bitmaps
                // EmptyTheImages();
                if (!String.IsNullOrWhiteSpace(txt_executable.Text) && File.Exists(txt_executable.Text))
                {
                    UpdateExeImagesUI();
                }

                SuggestShortcutName();
                EnableSaveButtonIfValid();
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
                p_game_list.Enabled = true;
                // Disable the Standalone Panel
                p_standalone.Enabled = false;
                pb_exe_icon.Image = ImageUtils.ConvertBitmapToGrayscale(pb_exe_icon.Image);

                // Empty the bitmaps
                //EmptyTheImages();

                if (!String.IsNullOrWhiteSpace(txt_game_name.Text) && ilv_games.SelectedItems.Count == 1 && _selectedGame != null)
                {
                    _gameLauncher = _selectedGame.GameLibrary.ToString("G");
                    lbl_game_library.Text = $"Game Library: {_gameLauncher}";
                    _gameId = _selectedGame.Id;
                    _availableImages = _selectedGame.AvailableGameBitmaps;
                    _shortcutToEdit.AvailableImages = _selectedGame.AvailableGameBitmaps;
                    _selectedImage = ImageUtils.GetMeLargestAvailableBitmap(_availableImages);
                    _shortcutToEdit.SelectedImage = _selectedImage;
                    txt_game_name.Text = _selectedGame.Name;
                    pb_game_icon.Image = _selectedImage.Image;
                    btn_choose_game_icon.Enabled = true;
                }

                SuggestShortcutName();
                EnableSaveButtonIfValid();

            }
        }

        private void EmptyTheImages()
        {
            _availableImages.Clear();
            _selectedImage = new ShortcutBitmap();
            pb_exe_icon.Image = null;
            pb_game_icon.Image = null;
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
                pb_exe_icon.Image = ImageUtils.ConvertBitmapToGrayscale(pb_exe_icon.Image);
                // Disable the Game Panel
                p_game_list.Enabled = false;
                p_game.Enabled = false;
                pb_game_icon.Image = ImageUtils.ConvertBitmapToGrayscale(pb_game_icon.Image);

                SuggestShortcutName();
                EnableSaveButtonIfValid();

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

        private void rb_wait_alternative_executable_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_wait_alternative_executable.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_wait_executable.Checked = false;
                txt_alternative_executable.Enabled = true;
                btn_choose_alternative_executable.Enabled = true;
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
                btn_choose_alternative_executable.Enabled = false;
            }
        }


        private void btn_choose_alternative_executable_Click(object sender, EventArgs e)
        {
            dialog_open.InitialDirectory = Path.GetDirectoryName(_executableToUse.ExecutableNameAndPath);
            dialog_open.DefaultExt = "*.exe";
            dialog_open.Filter = "exe files (*.exe;*.com) | *.exe;*.com | All files(*.*) | *.*";
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                string fileExt = Path.GetExtension(dialog_open.FileName);
                if (File.Exists(dialog_open.FileName) && (fileExt == @".exe" || fileExt == @".com"))
                {
                    txt_alternative_executable.Text = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        Language.Selected_file_is_not_a_valid_file,
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

            SuggestShortcutName();
            EnableSaveButtonIfValid();

        }

        private void ChangeSelectedProfile(ProfileItem profile)
        {
            // If the profile is null then return
            // (this happens when a new blank shortcut is created
            if (profile == null)
                return;

            // And we need to update the actual selected profile too!
            _profileToUse = profile;

            // And show the logo for the driver
            if (_profileToUse.VideoMode == VIDEO_MODE.NVIDIA)
            {
                pbLogo.Image = PickBitmapBasedOnBgColour(BackColor, Properties.Resources.nvidiablack, Properties.Resources.nvidiawhite);
            }
            else if (_profileToUse.VideoMode == VIDEO_MODE.AMD)
            {
                pbLogo.Image = PickBitmapBasedOnBgColour(BackColor, Properties.Resources.amdblack, Properties.Resources.amdwhite);
            }
            else
            {
                pbLogo.Image = PickBitmapBasedOnBgColour(BackColor, Properties.Resources.winblack, Properties.Resources.winwhite);
            }

            // We also need to load the saved profile name to show the user
            lbl_profile_shown.Text = _profileToUse.Name;

            if (_profileToUse.Equals(ProfileRepository.CurrentProfile))
                lbl_profile_shown_subtitle.Text = "This is the Display Profile currently in use.";
            else
                lbl_profile_shown_subtitle.Text = "";

            // Refresh the image list view
            RefreshProfileImageListView(profile);

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
                        newItem = new ImageListViewItem(loadedProfile, loadedProfile.Name);
                        ilv_saved_profiles.Items.Add(newItem, _profileAdaptor);
                    }

                }

                // Restart updating the saved_profiles listview
                ilv_saved_profiles.ResumeLayout();
            }
            
            UpdateHotkeyLabel(_shortcutToEdit.Hotkey);
            EnableSaveButtonIfValid();
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Hide();
        }

        private void radiobutton_Paint(object sender, PaintEventArgs e)
        {
            
            base.OnPaint(e);

            RadioButton radiobutton = sender as RadioButton;

            if (!radiobutton.Enabled)
            {
                int x = ClientRectangle.X + CheckBoxRenderer.GetGlyphSize(
                    e.Graphics, CheckBoxState.UncheckedNormal).Width;
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
                    e.Graphics, CheckBoxState.UncheckedNormal).Width;
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
                int x = ClientRectangle.X - 3;
                int y = ClientRectangle.Y;

                TextRenderer.DrawText(e.Graphics, label.Text,
                    label.Font, new Point(x, y), Color.Gray,
                    TextFormatFlags.LeftAndRightPadding);
            }

        }

        private void rb_switch_display_temp_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_display_temp.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_switch_display_permanent.Checked = false;

                SuggestShortcutName();
            }
        }

        private void rb_switch_display_permanent_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_display_permanent.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_switch_display_temp.Checked = false;
                SuggestShortcutName();
            }
        }

        private void txt_shortcut_save_name_TextChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            EnableSaveButtonIfValid();
        }

        private void ShortcutForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (_isUnsaved && _loadedShortcut && CanEnableSaveButton())
            {
                // If the user doesn't want to close this window without saving (when they can save), then don't close the window.
                DialogResult result = MessageBox.Show(
                    @"You have unsaved changes! Do you want to save your changes?",
                    @"You have unsaved changes.",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes) 
                {
                    // Press the save button for the user
                    btn_save.PerformClick();
                }

            }

        }

        private void btn_exe_to_start_Click(object sender, EventArgs e)
        {
            /*txt_executable.Text = getExeFile();
            UpdateExeImagesUI();*/

            ChooseExecutableForm exeForm = new ChooseExecutableForm();  
            if (exeForm.ShowDialog() == DialogResult.OK)
            {
                _selectedApp = exeForm.AppToUse;
                txt_executable.Text = _selectedApp.ExePath;
                UpdateExeImagesUI(_selectedApp);
            }            
        }

        private void UpdateExeImagesUI(App selectedApp = null)
        {
            _availableImages = new List<ShortcutBitmap>();
            if (selectedApp is App)
            {
                _availableImages.AddRange(selectedApp.AvailableAppBitmaps);
            }            
            else
            {
                _availableImages.AddRange(ImageUtils.GetMeAllBitmapsFromFile(txt_executable.Text));
            }
            if (rb_wait_alternative_executable.Checked && File.Exists(txt_alternative_executable.Text))
            {
                _availableImages.AddRange(ImageUtils.GetMeAllBitmapsFromFile(txt_alternative_executable.Text));
            }
            _selectedImage = ImageUtils.GetMeLargestAvailableBitmap(_availableImages);
            _shortcutToEdit.SelectedImage = _selectedImage;
            pb_exe_icon.Image = _selectedImage.Image;
            btn_choose_exe_icon.Enabled = true;
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
                SuggestShortcutName();
            }
            else
                _autoName = false;
        }

        private string getExeFile()
        {
            dialog_open.InitialDirectory = Environment.SpecialFolder.ProgramFiles.ToString();
            dialog_open.DefaultExt = "*.exe";
            dialog_open.Filter = "exe files (*.exe;*.com) | *.exe;*.com | All files(*.*) | *.*";
            string textToReturn = "";
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(dialog_open.FileName))
                {
                    textToReturn = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        Language.Selected_file_is_not_a_valid_file,
                        Language.Executable,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
            return textToReturn;
        }
        
        private void rb_no_change_audio_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_no_change_audio.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                cb_audio_device.Enabled = false;
                btn_rescan_audio.Enabled = false;
                gb_audio_volume.Visible = false;
            }
        }

        private void rb_change_audio_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_change_audio.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                cb_audio_device.Enabled = true;
                btn_rescan_audio.Enabled = true;
                gb_audio_volume.Visible = true;
            }
        }

        private void rb_switch_audio_temp_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_audio_temp.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_switch_audio_permanent.Checked = false;
            }
        }

        private void rb_switch_audio_permanent_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_audio_permanent.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_switch_audio_temp.Checked = false;
            }
        }


        private void cb_audio_device_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;

            // Populate all the Audio devices in the audio devices list.
            // Set the Audio device to the shortcut audio device only if 
            // the Change Audio radiobutton is set
            audioDevices = audioController.GetPlaybackDevices().ToList();

            // If the shortcut is to change the audio device
            
            // Then we need to populate the list 
            bool foundAudioDevice = false;
            foreach (CoreAudioDevice audioDevice in audioDevices)
            {
                if (audioDevice.State == AudioSwitcher.AudioApi.DeviceState.Active)
                {
                    // Set the audio device to the default device by default
                    if (audioDevice.FullName.Equals(cb_audio_device.SelectedItem.ToString()))
                    {
                        foundAudioDevice = true;
                        selectedAudioDevice = audioDevice;
                        nud_audio_volume.Value = Convert.ToDecimal(audioDevice.Volume);
                    }
                }
            }

            // We need to handle the edgecase where the selected audio device
            // isn't currently plugged in. We don't want to break the shortcut
            // as it could be plugged in when it comes time to actually run
            // the shortcut, so we need to just add it to the list to not break
            // the UI.

            if (!foundAudioDevice)
            {
                selectedAudioDevice = null;
                nud_audio_volume.Value = 50;
            }
        }

        private void btn_rescan_audio_Click(object sender, EventArgs e)
        {
            // Populate all the Audio devices in the audio devices list.
            // Set the Audio device to the shortcut audio device only if 
            // the Change Audio radiobutton is set
            rb_change_audio.Checked = _shortcutToEdit.ChangeAudioDevice;
            cb_audio_device.Items.Clear();
            audioDevices = audioController.GetPlaybackDevices().ToList();

            // If the shortcut is to change the audio device
            if (_shortcutToEdit.ChangeAudioDevice)
            {
                // Then we need to populate thev list 
                bool foundAudioDevice = false;
                foreach (CoreAudioDevice audioDevice in audioDevices)
                {
                    if (audioDevice.State == AudioSwitcher.AudioApi.DeviceState.Active)
                    {
                        int index = cb_audio_device.Items.Add(audioDevice.FullName);
                        // Set the audio device to the default device by default
                        if (audioDevice.FullName.Equals(_shortcutToEdit.AudioDevice))
                        {
                            foundAudioDevice = true;
                            selectedAudioDevice = audioDevice;
                            cb_audio_device.SelectedIndex = index;
                            if (_shortcutToEdit.SetAudioVolume && _shortcutToEdit.AudioVolume >= 0 && _shortcutToEdit.AudioVolume <= 100)
                                nud_audio_volume.Value = _shortcutToEdit.AudioVolume;
                            else
                                nud_audio_volume.Value = Convert.ToDecimal(audioDevice.Volume);
                        }
                    }
                }

                // We need to handle the edgecase where the selected audio device
                // isn't currently plugged in. We don't want to break the shortcut
                // as it could be plugged in when it comes time to actually run
                // the shortcut, so we need to just add it to the list to not break
                // the UI.

                if (!foundAudioDevice)
                {
                    int index = cb_audio_device.Items.Add(_shortcutToEdit.AudioDevice);
                    cb_audio_device.SelectedIndex = index;
                    selectedAudioDevice = null;
                    if (_shortcutToEdit.SetAudioVolume && _shortcutToEdit.AudioVolume >= 0 && _shortcutToEdit.AudioVolume <= 100)
                    {
                        rb_set_audio_volume.Checked = true;
                        nud_audio_volume.Value = _shortcutToEdit.AudioVolume;
                    }
                    else
                    {
                        rb_keep_audio_volume.Checked = true;
                        nud_audio_volume.Value = 50;
                    }

                }
            }
            else
            {
                // Then we need to populate the list 
                foreach (CoreAudioDevice audioDevice in audioDevices)
                {
                    if (audioDevice.State == AudioSwitcher.AudioApi.DeviceState.Active)
                    {
                        int index = cb_audio_device.Items.Add(audioDevice.FullName);
                        // Set the audio device to the default device by default
                        if (audioDevice.IsDefaultDevice)
                        {
                            selectedAudioDevice = audioDevice;
                            cb_audio_device.SelectedIndex = index;
                            nud_audio_volume.Value = Convert.ToDecimal(audioDevice.Volume);
                        }
                    }
                }
                rb_keep_audio_volume.Checked = true;
            }

        }

        private void rb_keep_audio_volume_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            if (rb_set_audio_volume.Checked)
                nud_audio_volume.Enabled = false;

        }

        private void rb_set_audio_volume_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            if (rb_set_audio_volume.Checked)
                nud_audio_volume.Enabled = true;
        }

        private void btn_rescan_capture_Click(object sender, EventArgs e)
        {
            // Populate all the Capture devices in the capture devices list.
            // Set the capture  device to the shortcut capture  device only if 
            // the Change capture  radiobutton is set
            rb_change_capture.Checked = _shortcutToEdit.ChangeCaptureDevice;
            cb_capture_device.Items.Clear();
            captureDevices = audioController.GetCaptureDevices().ToList();

            // If the shortcut is to change the capture device
            if (_shortcutToEdit.ChangeCaptureDevice)
            {
                // Then we need to populate the list 
                bool foundCaptureDevice = false;
                foreach (CoreAudioDevice captureDevice in captureDevices)
                {
                    if (captureDevice.State == AudioSwitcher.AudioApi.DeviceState.Active)
                    {
                        int index = cb_capture_device.Items.Add(captureDevice.FullName);
                        // Set the capture device to the default device by default
                        if (captureDevice.FullName.Equals(_shortcutToEdit.CaptureDevice))
                        {
                            foundCaptureDevice = true;
                            selectedCaptureDevice = captureDevice;
                            cb_capture_device.SelectedIndex = index;
                            if (_shortcutToEdit.SetCaptureVolume && _shortcutToEdit.CaptureVolume >= 0 && _shortcutToEdit.CaptureVolume <= 100)
                                nud_capture_volume.Value = _shortcutToEdit.CaptureVolume;
                            else
                                nud_capture_volume.Value = Convert.ToDecimal(captureDevice.Volume);
                        }
                    }
                }

                // We need to handle the edgecase where the selected capture device
                // isn't currently plugged in. We don't want to break the shortcut
                // as it could be plugged in when it comes time to actually run
                // the shortcut, so we need to just add it to the list to not break
                // the UI.

                if (!foundCaptureDevice)
                {
                    int index = cb_capture_device.Items.Add(_shortcutToEdit.CaptureDevice);
                    cb_capture_device.SelectedIndex = index;
                    selectedCaptureDevice = null;
                    if (_shortcutToEdit.SetCaptureVolume && _shortcutToEdit.CaptureVolume >= 0 && _shortcutToEdit.CaptureVolume <= 100)
                    {
                        rb_set_capture_volume.Checked = true;
                        nud_capture_volume.Value = _shortcutToEdit.CaptureVolume;
                    }
                    else
                    {
                        rb_keep_capture_volume.Checked = true;
                        nud_capture_volume.Value = 50;
                    }

                }
            }
            else
            {
                // Then we need to populate the list 
                foreach (CoreAudioDevice captureDevice in captureDevices)
                {
                    if (captureDevice.State == AudioSwitcher.AudioApi.DeviceState.Active)
                    {
                        int index = cb_capture_device.Items.Add(captureDevice.FullName);
                        // Set the capture device to the default device by default
                        if (captureDevice.IsDefaultDevice)
                        {
                            selectedCaptureDevice = captureDevice;
                            cb_capture_device.SelectedIndex = index;
                            nud_capture_volume.Value = Convert.ToDecimal(captureDevice.Volume);
                        }
                    }
                }
                rb_keep_capture_volume.Checked = true;
            }
        }

        private void cb_capture_device_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;

            // Populate all the Capture devices in the capture devices list.
            // Set the Capture device to the shortcut capture device only if 
            // the Change Capture radiobutton is set
            captureDevices = audioController.GetCaptureDevices().ToList();

            // Then we need to populate the list 
            bool foundCaptureDevice = false;
            foreach (CoreAudioDevice captureDevice in captureDevices)
            {
                if (captureDevice.State == AudioSwitcher.AudioApi.DeviceState.Active)
                {
                    // Set the capture device to the default device by default
                    if (captureDevice.FullName.Equals(cb_capture_device.SelectedItem.ToString()))
                    {
                        foundCaptureDevice = true;
                        selectedCaptureDevice = captureDevice;
                        nud_capture_volume.Value = Convert.ToDecimal(captureDevice.Volume);
                    }
                }
            }

            // We need to handle the edgecase where the selected capture device
            // isn't currently plugged in. We don't want to break the shortcut
            // as it could be plugged in when it comes time to actually run
            // the shortcut, so we need to just add it to the list to not break
            // the UI.

            if (!foundCaptureDevice)
            {
                selectedCaptureDevice = null;
                nud_capture_volume.Value = 50;
            }            
        }

        private void rb_no_change_capture_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_no_change_capture.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                cb_capture_device.Enabled = false;
                btn_rescan_capture.Enabled = false;
                gb_capture_volume.Visible = false;
            }
        }

        private void rb_change_capture_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_change_capture.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                cb_capture_device.Enabled = true;
                btn_rescan_capture.Enabled = true;
                gb_capture_volume.Visible = true;
            }
        }

        private void rb_keep_capture_volume_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            if (rb_set_capture_volume.Checked)
                nud_capture_volume.Enabled = false;

        }

        private void rb_set_capture_volume_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            if (rb_set_capture_volume.Checked)
                nud_capture_volume.Enabled = true;
        }

        private void rb_switch_capture_temp_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_capture_temp.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_switch_capture_permanent.Checked = false;
            }
        }

        private void rb_switch_capture_permanent_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_switch_capture_permanent.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                rb_switch_capture_temp.Checked = false;
            }
        }

        private void nud_audio_volume_ValueChanged(object sender, EventArgs e)
        {
            _audioVolume = Convert.ToDecimal(nud_audio_volume.Value);
        }

        private void nud_capture_volume_ValueChanged(object sender, EventArgs e)
        {
            _captureVolume = Convert.ToDecimal(nud_capture_volume.Value);
        }


        private void cb_wait_alternative_game_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            if (cb_wait_alternative_game.Checked)
            {
                txt_alternative_game.Enabled = true;
                btn_choose_alternative_game.Enabled = true;
            }
            else
            {
                txt_alternative_game.Enabled = false;
                btn_choose_alternative_game.Enabled = false;
            }
        }


        private void btn_choose_alternative_game_Click(object sender, EventArgs e)
        {
            string gamePath = "";
            foreach (Game game in DisplayMagician.GameLibraries.GameLibrary.AllInstalledGamesInAllLibraries)
            {
                if (game.Name == txt_game_name.Text)
                {
                    gamePath = game.Directory;
                    break;
                }
            }

            dialog_open.InitialDirectory = gamePath;
            dialog_open.DefaultExt = "*.exe";
            dialog_open.Filter = "exe files (*.exe;*.com) | *.exe;*.com | All files(*.*) | *.*";
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                string fileExt = Path.GetExtension(dialog_open.FileName);
                if (File.Exists(dialog_open.FileName) && (fileExt == @".exe" || fileExt == @".com"))
                {
                    txt_alternative_game.Text = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        Language.Selected_file_is_not_a_valid_file,
                        Language.Executable,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }

        public void RedrawStartPrograms()
        {
            bool firstItem = true;
            Padding firstStartProgramMargin = new Padding(10) { };
            Padding otherStartProgramMargin = new Padding(10, 0, 10, 10) { };
            foreach (StartProgramControl myStartProgramControl in flp_start_programs.Controls)
            {
                if (firstItem)
                {
                    myStartProgramControl.Margin = firstStartProgramMargin;
                    firstItem = false;
                }
                else
                {
                    myStartProgramControl.Margin = otherStartProgramMargin;
                }
                int priority = flp_start_programs.Controls.GetChildIndex(myStartProgramControl) + 1;
                myStartProgramControl.ChangePriority(priority);
            }
        }


        public void RemoveStartProgram(StartProgramControl startProgramControlToRemove)
        {
            // If we find the start program then we need to remove it from the list (only if one was supplied)
            if (_shortcutToEdit.StartPrograms != null && _shortcutToEdit.StartPrograms.Count > 0)
                _shortcutToEdit.StartPrograms.Remove(startProgramControlToRemove.StartProgram);
            
            // And we remove the program control passed in to this function as well
            flp_start_programs.SuspendLayout();
            flp_start_programs.Controls.Remove(startProgramControlToRemove);
            RedrawStartPrograms();
            flp_start_programs.ResumeLayout();
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        public void StartProgramEarlier(StartProgramControl startProgramControlToRemove)
        {
            // And we move the program control passed in to this function earlier in the flow layout panel
            flp_start_programs.SuspendLayout();
            int currentIndex = flp_start_programs.Controls.GetChildIndex(startProgramControlToRemove);
            int newIndex = currentIndex - 1;
            if (newIndex <= 0)
                newIndex = 0;
            flp_start_programs.Controls.SetChildIndex(startProgramControlToRemove,newIndex);
            RedrawStartPrograms();
            flp_start_programs.ResumeLayout();
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        public void StartProgramLater(StartProgramControl startProgramControlToRemove)
        {
            // And we move the program control passed in to this function later in the flow layout panel
            flp_start_programs.SuspendLayout();
            int currentIndex = flp_start_programs.Controls.GetChildIndex(startProgramControlToRemove);
            int newIndex = currentIndex + 1;
            if (newIndex > flp_start_programs.Controls.Count - 1)
                newIndex = flp_start_programs.Controls.Count - 1;
            flp_start_programs.Controls.SetChildIndex(startProgramControlToRemove, newIndex);
            RedrawStartPrograms();
            flp_start_programs.ResumeLayout();
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        private void btn_hotkey_Click(object sender, EventArgs e)
        {
            Keys testHotkey;
            if (_shortcutToEdit.Hotkey != Keys.None)
                testHotkey = _shortcutToEdit.Hotkey;
            else
                testHotkey = Keys.None;
            string hotkeyHeading = $"Choose a '{_shortcutToEdit.Name}' Shortcut Hotkey";
            string hotkeyDescription = $"Choose a Hotkey (a keyboard shortcut) so that you can run this" + Environment.NewLine +
                "game shortcut using your keyboard. This must be a Hotkey that" + Environment.NewLine +
                "is unique across all your applications otherwise DisplayMagician" + Environment.NewLine +
                "might not see it.";
            HotkeyForm displayHotkeyForm = new HotkeyForm(testHotkey, hotkeyHeading, hotkeyDescription);
            //Program.HotkeyListener.SuspendOn(displayHotkeyForm);
            displayHotkeyForm.ShowDialog(this);
            if (displayHotkeyForm.DialogResult == DialogResult.OK)
            {
                // If the hotkey has changed, then set the unsaved warning to true
                if (!_hotkey.Equals(displayHotkeyForm.Hotkey))
                    _isUnsaved = true;
                // now we store the Hotkey to be saved later
                _hotkey = displayHotkeyForm.Hotkey;
                // And if we get back and this is a Hotkey with a value, we need to show that in the UI
                UpdateHotkeyLabel(_hotkey);
            }
        }

        private void lbl_hotkey_assigned_Click(object sender, EventArgs e)
        {
            btn_hotkey.PerformClick();
        }

        private void UpdateHotkeyLabel(Keys myHotkey)
        {
            // And if we get back and this is a Hotkey with a value, we need to show that in the UI
            if (myHotkey != Keys.None)
            {
                KeysConverter kc = new KeysConverter();

                lbl_hotkey_assigned.Text = "Hotkey: " + kc.ConvertToString(myHotkey);
                lbl_hotkey_assigned.Visible = true;
            }
            else
            {
                lbl_hotkey_assigned.Text = "Hotkey: None";
                lbl_hotkey_assigned.Visible = false;
            }

        }

#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public void OnWindowHotkeyPressed(object sender, HotkeyEventArgs e)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            if (ShortcutRepository.ContainsShortcut(e.Name))
            {
                string shortcutUUID = e.Name;
                ShortcutItem chosenShortcut = ShortcutRepository.GetShortcut(shortcutUUID);
                if (chosenShortcut is ShortcutItem)
                    Program.RunShortcutTask(chosenShortcut);
            }
        }

        private void StartProgramControl_MouseDown(object sender, MouseEventArgs e)
        {
            DoDragDrop(sender, DragDropEffects.Move);
        }

        private void StartProgramControl_DragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(StartProgramControl)) is StartProgramControl)
            {
                // We make it show some effects while moving
                e.Effect = DragDropEffects.Move;

                // We figure out the position of the thing we're dragging
                FlowLayoutPanel p = (FlowLayoutPanel)(sender as StartProgramControl).Parent;
                int myIndex = p.Controls.GetChildIndex((sender as StartProgramControl));

                // We figure out the position of the thing we're being dragged over
                StartProgramControl spc = (StartProgramControl)e.Data.GetData(typeof(StartProgramControl));
                // We set the position of the thing we're dragging to the position of the thing we're over
                p.Controls.SetChildIndex(spc, myIndex);              
            }
        }

        private void StartProgramControl_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(StartProgramControl)) is StartProgramControl)
            {
                // We figure out the position of the thing we're dragging
                FlowLayoutPanel p = (FlowLayoutPanel)(sender as StartProgramControl).Parent;
                int myIndex = p.Controls.GetChildIndex((sender as StartProgramControl));

                // We figure out the position of the thing we're being dropped onto
                StartProgramControl spc = (StartProgramControl)e.Data.GetData(typeof(StartProgramControl));
                // We set the position of the thing we're dragging to the position of the thing we've been dropped on
                p.Controls.SetChildIndex(spc, myIndex);

                // We then set the final startProgram position in the data we're storing
                StartProgram startProgram = spc.StartProgram;
                startProgram.Priority = myIndex + 1;
                spc.StartProgram = startProgram;

                // And now we also update all the UI for the items in the list
                // To reorder all of them
                flp_start_programs.SuspendLayout();
                RedrawStartPrograms();
                flp_start_programs.ResumeLayout();
                flp_start_programs.Invalidate();

                if (_loadedShortcut)
                    _isUnsaved = true;
            }

        }

        private void btn_add_new_start_program_Click(object sender, EventArgs e)
        {
            // Create a new startProgram with sensible defaults
            StartProgram newStartProgram = new StartProgram() {
                CloseOnFinish = true,
                Executable = "",
                Arguments = "",
            };
            StartProgramControl newStartProgramControl = new StartProgramControl(newStartProgram, flp_start_programs.Controls.Count);
            newStartProgramControl.Dock = DockStyle.None;
            newStartProgramControl.Width = flp_start_programs.Width - 40;
            newStartProgramControl.MouseDown += new MouseEventHandler(StartProgramControl_MouseDown);
            newStartProgramControl.DragOver += new DragEventHandler(StartProgramControl_DragOver);
            newStartProgramControl.DragDrop += new DragEventHandler(StartProgramControl_DragDrop);
            newStartProgramControl.AllowDrop = true;
            flp_start_programs.SuspendLayout();
            flp_start_programs.Controls.Add(newStartProgramControl);
            RedrawStartPrograms();
            flp_start_programs.ResumeLayout();
            flp_start_programs.Invalidate();
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        private void ilv_games_ItemClick(object sender, ItemClickEventArgs e)
        {
            txt_game_name.Text = e.Item.Text;
            foreach (Game game in DisplayMagician.GameLibraries.GameLibrary.AllInstalledGamesInAllLibraries)
            {
                if (game.Name == txt_game_name.Text)
                {
                    if (_loadedShortcut)
                        _isUnsaved = true;
                    _selectedGame = game;
                    _gameLauncher = game.GameLibrary.ToString("G");
                    lbl_game_library.Text = $"Game Library: {_gameLauncher}";
                    _gameId = game.Id;
                    _availableImages = game.AvailableGameBitmaps;
                    _shortcutToEdit.AvailableImages = game.AvailableGameBitmaps;
                    _selectedImage = ImageUtils.GetMeLargestAvailableBitmap(_availableImages);
                    _shortcutToEdit.SelectedImage = _selectedImage;
                    txt_game_name.Text = game.Name;
                    pb_game_icon.Image = _selectedImage.Image;
                    btn_choose_game_icon.Enabled = true;
                    break;
                }
            }

            try
            {
                SuggestShortcutName();
            }
            catch(Exception ex)
            {
                logger.Warn(ex, $"ShortcutForm/ilv_games_ItemClick: Exception while attempting to suggest shortcut name.");
            }

            try
            {
                EnableSaveButtonIfValid();
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutForm/ilv_games_ItemClick: Exception while figuring out if the save button shoud be enabled.");
            }            
        }


        private void btn_find_examples_startprograms_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/wiki/Start-Program-Examples";
            System.Diagnostics.Process.Start(targetURL);
        }

        private void btn_find_examples_game_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/wiki/Main-Game-and-Application-Examples";
            System.Diagnostics.Process.Start(targetURL);
        }

        private Bitmap PickBitmapBasedOnBgColour(Color bgColour, Bitmap lightBitmap, Bitmap darkBitmap)
        {
            if ((bgColour.R * 0.299 + bgColour.G * 0.587 + bgColour.B * 0.114) > 186)
            {
                return darkBitmap;
            }
            else
            {
                return lightBitmap;
            }
        }


        private void btn_choose_exe_icon_Click(object sender, EventArgs e)
        {
            if (rb_standalone.Checked && _availableImages.Count > 0)
            {
                ChooseImageForm exeIconForm = new ChooseImageForm();
                exeIconForm.AvailableImages = _availableImages;
                exeIconForm.SelectedImage = _selectedImage;
                if (exeIconForm.ShowDialog() == DialogResult.OK)
                {
                    if (_loadedShortcut)
                        _isUnsaved = true;
                    _availableImages = exeIconForm.AvailableImages;
                    _selectedImage = exeIconForm.SelectedImage;
                    pb_exe_icon.Image = exeIconForm.SelectedImage.Image;
                }
            }
            
        }

        private void btn_choose_game_icon_Click(object sender, EventArgs e)
        {
            if (rb_launcher.Checked && _shortcutToEdit.AvailableImages.Count > 0)
            {
                ChooseImageForm gameIconForm = new ChooseImageForm();
                gameIconForm.AvailableImages = _availableImages;
                gameIconForm.SelectedImage = _selectedImage;
                if (gameIconForm.ShowDialog() == DialogResult.OK)
                {
                    if (_loadedShortcut)
                        _isUnsaved = true;
                    _availableImages = gameIconForm.AvailableImages;
                    _selectedImage = gameIconForm.SelectedImage;
                    pb_game_icon.Image = gameIconForm.SelectedImage.Image;                     
                }
            }
        }

        private void cb_run_cmd_afterwards_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_run_cmd_afterwards.Checked)
            {
                txt_run_cmd_afterwards.Enabled = true;
                btn_run_cmd_afterwards.Enabled = true;
                cb_run_cmd_afterwards_args.Enabled = true;
                cb_run_cmd_afterwards_dont_start.Enabled = true;
                cb_run_cmd_afterwards_run_as_administrator.Enabled = true;
            }
            else
            {
                txt_run_cmd_afterwards.Enabled = false;
                btn_run_cmd_afterwards.Enabled = false;
                cb_run_cmd_afterwards_args.Enabled = false;
                cb_run_cmd_afterwards_dont_start.Enabled = false;
                cb_run_cmd_afterwards_run_as_administrator.Enabled = false;
            }
        }

        private void cb_run_cmd_afterwards_args_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_run_cmd_afterwards_args.Checked)
            {
                txt_run_cmd_afterwards_args.Enabled = true;
            }
            else
            {
                txt_run_cmd_afterwards_args.Enabled = false;
            }
        }

        private void btn_run_cmd_afterwards_Click(object sender, EventArgs e)
        {
            txt_run_cmd_afterwards.Text = getExeFile();
        }

        private void btn_refresh_games_list_Click(object sender, EventArgs e)
        {
            // Change the mouse crusor so the user knows something is happening
            this.Cursor = Cursors.WaitCursor;
            // Empty the games list
            GameLibraries.GameLibrary.AllInstalledGamesInAllLibraries.Clear();
            // Load all the new games
            GameLibraries.GameLibrary.LoadGamesInBackground();
            // Parse the libraries
            GameLibraries.GameLibrary.RefreshGameBitmaps();
            // Load all the Games into the Games ListView            
            ImageListViewItem previouslySelectedItem = null;
            if (ilv_games.SelectedItems.Count > 0)
            {
                previouslySelectedItem = ilv_games.SelectedItems[0];
            }
            ilv_games.Items.Clear();            
            foreach (var game in DisplayMagician.GameLibraries.GameLibrary.AllInstalledGamesInAllLibraries.OrderBy(game => game.Name))
            {
                // Add the game to the game array
                ImageListViewItem newItem = new ImageListViewItem(game, game.Name);
                if (previouslySelectedItem != null && newItem.Text.Equals(previouslySelectedItem.Text))
                {
                    newItem.Selected = true;
                }
                else if (_editingExistingShortcut && game.Name.Equals(_shortcutToEdit.GameName))
                {
                    newItem.Selected = true;                    
                }
                ilv_games.Items.Add(newItem, _gameAdaptor);
            }
            // Make sure that if the item is selected that it's visible
            if (ilv_games.SelectedItems.Count > 0)
            {
                int selectedIndex = ilv_games.SelectedItems[0].Index;
                ilv_games.EnsureVisible(selectedIndex);
            }
            
            // Change the user cursor back
            this.Cursor = Cursors.Default;
            // Show we're done
            MessageBox.Show(
                @"The list of available games has been updated.",
                @"Games List Updated",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);
        }

        private void pb_game_icon_Click(object sender, EventArgs e)
        {
            btn_choose_game_icon.PerformClick();
        }

        private void pb_exe_icon_Click(object sender, EventArgs e)
        {
            btn_choose_exe_icon.PerformClick();
        }

        private void txt_alternative_game_TextChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        private void btn_help_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/wiki/Initial-DisplayMagician-Setup";
            System.Diagnostics.Process.Start(targetURL);
        }
    }

    // Class used to populate combo boxes
    class ComboItem
    {
        public ProcessPriority Value { get; set; }
        public string Text { get; set; }
    }
}