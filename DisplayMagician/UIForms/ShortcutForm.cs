using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
//using DisplayMagician.Resources;
using DisplayMagicianShared;
using DisplayMagician.GameLibraries;
using Manina.Windows.Forms;
using System.Windows.Forms.VisualStyles;
//using NHotkey.WindowsForms;
//using NHotkey;
using DisplayMagician;
using System.Threading;
using DisplayMagician.AppLibraries;
using static DisplayMagician.GameLibraries.ProductInformation;
using System.ComponentModel;
using DisplayMagician.Processes;
using System.Globalization;

namespace DisplayMagician.UIForms
{

    public partial class ShortcutForm : Form
    {

        private ProfileAdaptor _profileAdaptor;
        private GameAdaptor _gameAdaptor;
        private bool _editingExistingShortcut = false;
        private ShortcutCategory _shortcutCategory = ShortcutCategory.Game;
        //private List<ProfileItem> _loadedProfiles = new List<ProfileItem>();
        private ProfileItem _profileToUse = null;
        private ImageListViewItem _skipDisplayChangeILVItem;

        private string _gameLauncher = "";
        private GameShorcutData _gameToUse;
        private ExecutableShortcutData _executableToUse;
        private AppShortcutData _appToUse;
        private ShortcutPermanence _displayPermanence = ShortcutPermanence.Temporary;
        private ShortcutPermanence _audioPermanence = ShortcutPermanence.Temporary;
        List<StartProgram> _startPrograms = new List<StartProgram>();
        List<AfterProgram> _afterPrograms = new List<AfterProgram>();
        List<StopProgram> _stopPrograms = new List<StopProgram>();
        private AudioProfileItem _audioProfileToUse = null;
        private ShortcutItem _shortcutToEdit = null;
        private bool _overrideAudioSpeakerVolume = false;
        private bool _overrideAudioMicrophoneVolume = false;
        private int _overrideAudioSpeakerVolumeLevel = 50;
        private int _overrideAudioMicrophoneVolumeLevel = 50;
        private Game _selectedGame = null;
        private App _selectedApp = null;
        private string _selectedAppId = "";
        private bool _isUnsaved = true;
        private bool _loadedShortcut = false;
        private bool _autoName = true;
        private string _gameId = "0";
        private string _uuid = "";

        //private Keys _hotkey = Keys.None;
        //private bool _userChoseOwnGameIcon = false;
        //private string _userGameIconPath = "";
        //private bool _userChoseOwnExeIcon = false;
        //private string _userExeIconPath = "";

        private List<HotkeyKeyboard> _shownKeyboardHotkeys = new();
        private List<HotkeyJoystick> _shownJoystickHotkeys = new();

        private List<ShortcutBitmap> _availableImages = new List<ShortcutBitmap>();
        private ShortcutBitmap _selectedImage = new ShortcutBitmap();
        private bool _firstShow = true;

        // Debounce timer: delays the icon scan until the user stops typing in txt_executable
        private readonly System.Windows.Forms.Timer _exePathDebounceTimer = new System.Windows.Forms.Timer { Interval = 600 };

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ShortcutForm()
        {
            InitializeComponent();

            // Fire icon scan only once the user has stopped typing for 600 ms
            _exePathDebounceTimer.Tick += (s, e) =>
            {
                _exePathDebounceTimer.Stop();
                // Don't override the app's bitmap when the exe path was set by the app picker
                if (_shortcutCategory == ShortcutCategory.Application)
                    return;
                string path = txt_executable.Text.Trim();
                if (File.Exists(path) && ProcessUtils.IsExecutableFileType(path))
                {
                    logger.Debug($"ShortcutForm/ExePathDebounceTimer: Scanning icons for '{path}'.");
                    UpdateExeImagesUI(null);
                }
            };
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

            _skipDisplayChangeILVItem = new ImageListViewItem(ShortcutItem.SkipDisplayChangeProfile, ProfileItem.SkipDisplayChangeName);

            lbl_profile_shown.Text = "No Display Profiles available";
            lbl_profile_shown_subtitle.Text = "Please go back to the main window, click on 'Display Profiles', and save a new Display Profile. Then come back here.";
            lbl_profile_shown_subtitle.Visible = true;

            cb_dont_change_audio.Checked = true;

            // Center the form on the primary screen
            //Utils.CenterOnPrimaryScreen(this);

        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ShortcutItem Shortcut
        {
            get => _shortcutToEdit;
            set => _shortcutToEdit = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool EditingExistingShortcut
        {
            get => _editingExistingShortcut;
            set => _editingExistingShortcut = value;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
            if (!AllowedToSave(true))
            {
                return;
            }

            // Check the display permanence requirements
            if (rb_switch_display_temp.Checked)
                _displayPermanence = ShortcutPermanence.Temporary;

            if (rb_switch_display_permanent.Checked)
                _displayPermanence = ShortcutPermanence.Permanent;

            // Save the audio profile choice and permanence
            if (rb_switch_audio_temp.Checked)
                _audioPermanence = ShortcutPermanence.Temporary;
            if (rb_switch_audio_permanent.Checked)
                _audioPermanence = ShortcutPermanence.Permanent;

            if (cb_dont_change_audio.Checked)
            {
                _audioProfileToUse = null;
            }
            else
            {
                _audioProfileToUse = lb_audio_profiles.SelectedItem as AudioProfileItem;
                if (_audioProfileToUse == null)
                {
                    MessageBox.Show(this,
                        "Please select an Audio Profile or tick 'Don't change audio settings for this shortcut'.",
                        "Audio Profile",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            // Add the startprograms to the list
            List<StartProgram> newStartPrograms = new List<StartProgram>() { };
            List<StopProgram> newStopPrograms = new List<StopProgram>() { };
            foreach (Control ctrl in flp_start_programs.Controls)
            {
                if (ctrl is StartProgramControl myStartProgramControl)
                    newStartPrograms.Add(myStartProgramControl.StartProgram);
                else if (ctrl is StopProgramControl myStopProgramControl)
                    newStopPrograms.Add(myStopProgramControl.StopProgram);
            }

            // Replace the old start programs and stop programs with the ones we've created now
            _startPrograms = newStartPrograms;
            _stopPrograms = newStopPrograms;

            // Store the single stop program if it's set (but wth lots of defaults)
            if (!String.IsNullOrWhiteSpace(txt_run_cmd_afterwards.Text) && File.Exists(txt_run_cmd_afterwards.Text))
            {
                _afterPrograms = new List<AfterProgram>();
                AfterProgram stopProgram = new AfterProgram();
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

                _afterPrograms.Add(stopProgram);
            }

            // Now we create the Shortcut Object ready to save
            // If we're launching a game
            if (_shortcutCategory == ShortcutCategory.Game)
            {
                logger.Trace($"ShortcutForm/btn_save_Click: We're saving a game!");

                _gameToUse = new GameShorcutData
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
                    _gameToUse.GameToPlay = (from steamGame in SteamLibrary.GetLibrary().AllInstalledGames where steamGame.Id == _gameId select steamGame).FirstOrDefault();
                }
                // If the game is a UplayGame
                else if (_gameLauncher == SupportedGameLibraryType.Uplay.ToString())
                {
                    logger.Trace($"ShortcutForm/btn_save_Click: We're saving a Uplay game!");
                    _gameToUse.GameToPlay = (from uplayGame in UplayLibrary.GetLibrary().AllInstalledGames where uplayGame.Id == _gameId select uplayGame).FirstOrDefault();
                }
                // If the game is an Origin Game
                else if (_gameLauncher == SupportedGameLibraryType.Origin.ToString())
                {
                    logger.Trace($"ShortcutForm/btn_save_Click: We're saving an Origin game!");
                    _gameToUse.GameToPlay = (from originGame in OriginLibrary.GetLibrary().AllInstalledGames where originGame.Id == _gameId select originGame).FirstOrDefault();
                }
                // If the game is an Epic Game
                else if (_gameLauncher == SupportedGameLibraryType.Epic.ToString())
                {
                    logger.Trace($"ShortcutForm/btn_save_Click: We're saving an Epic game!");
                    _gameToUse.GameToPlay = (from epicGame in EpicLibrary.GetLibrary().AllInstalledGames where epicGame.Id == _gameId select epicGame).FirstOrDefault();
                }
                // If the game is an GOG Game
                else if (_gameLauncher == SupportedGameLibraryType.GOG.ToString())
                {
                    logger.Trace($"ShortcutForm/btn_save_Click: We're saving an GOG game!");
                    _gameToUse.GameToPlay = (from gogGame in GogLibrary.GetLibrary().AllInstalledGames where gogGame.Id == _gameId select gogGame).FirstOrDefault();
                }
                else
                {
                    logger.Error($"ShortcutForm/btn_save_Click: Unknown game launcher type '{_gameLauncher}' — cannot resolve game to save.");
                }

                if (_gameToUse.GameToPlay == null)
                {
                    logger.Error($"ShortcutForm/btn_save_Click: Could not find game with ID '{_gameId}' in library '{_gameLauncher}'. The game may have been uninstalled. Aborting save.");
                    MessageBox.Show(
                        $"The selected game could not be found in the '{_gameLauncher}' library. It may have been uninstalled since this shortcut was created. Please re-select the game.",
                        "Game Not Found",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                try
                {
                    _shortcutToEdit.UpdateGameShortcut(
                        txt_shortcut_save_name.Text,
                        _profileToUse,
                        _gameToUse,
                        _displayPermanence,
                        _audioPermanence,
                        _gameToUse.GameToPlay.IconPath,
                        _selectedImage,
                        _availableImages,
                        _audioProfileToUse,
                        _overrideAudioSpeakerVolume,
                        _overrideAudioSpeakerVolumeLevel,
                        _overrideAudioMicrophoneVolume,
                        _overrideAudioMicrophoneVolumeLevel,
                        _startPrograms,
                        _afterPrograms,
                        _stopPrograms,
                        _autoName,
                        _uuid
                    );
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutForm/btn_save_Click: Exception while trying to update a game shortcut.");
                }

            }
            // If we're saving an executable
            else if (_shortcutCategory == ShortcutCategory.Executable)
            {
                logger.Trace($"ShortcutForm/btn_save_Click: We're saving a standalone executable!");
                _executableToUse = new ExecutableShortcutData
                {
                    ExecutableArguments = txt_args_executable.Text,
                    ExecutableArgumentsRequired = cb_args_executable.Checked,
                    ExecutableNameAndPath = txt_executable.Text,
                    RunAsAdministrator = cb_run_exe_as_administrator.Checked,
                    ExecutableTimeout = Convert.ToInt32(nud_timeout_executable.Value),
                    ProcessPriority = (ProcessPriority)cbx_exe_priority.SelectedValue,
                    DifferentExecutableToMonitor = txt_alternative_executable.Text,
                    ProcessNameToMonitorUsesExecutable = rb_wait_executable.Checked,
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
                        _executableToUse.ExecutableNameAndPath,
                        _selectedImage,
                        _availableImages,
                        _audioProfileToUse,
                        _overrideAudioSpeakerVolume,
                        _overrideAudioSpeakerVolumeLevel,
                        _overrideAudioMicrophoneVolume,
                        _overrideAudioMicrophoneVolumeLevel,
                        _startPrograms,
                        _afterPrograms,
                        _stopPrograms,
                        _autoName
                    );
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutForm/btn_save_Click: Exception while trying to update an executable shortcut.");
                }
            }
            else if (_shortcutCategory == ShortcutCategory.Application)
            {
                // Otherwise we're saving an app selected from the list
                logger.Trace($"ShortcutForm/btn_save_Click: We're saving an app!");

                if (_selectedApp == null)
                {
                    _selectedApp = _appToUse.AppToUse;
                }

                // Update the two details that may have been updated by the user into the App data
                _selectedApp.ExecutableArgumentsRequired = cb_args_executable.Checked;
                _selectedApp.Arguments = txt_args_executable.Text;

                _appToUse = new AppShortcutData
                {
                    AppToUse = _selectedApp,
                    RunAsAdministrator = cb_run_exe_as_administrator.Checked,
                    ExecutableTimeout = Convert.ToInt32(nud_timeout_executable.Value),
                    ProcessPriority = (ProcessPriority)cbx_exe_priority.SelectedValue,
                    DifferentExecutableToMonitor = txt_alternative_executable.Text,
                    ProcessNameToMonitorUsesExecutable = rb_wait_executable.Checked,
                };


                if (rb_wait_alternative_executable.Checked && !String.IsNullOrWhiteSpace(txt_alternative_executable.Text))
                {
                    _appToUse.ProcessNameToMonitorUsesExecutable = false;
                    _appToUse.DifferentExecutableToMonitor = txt_alternative_executable.Text;
                }
                else
                {
                    _appToUse.ProcessNameToMonitorUsesExecutable = true;
                }


                try
                {
                    _shortcutToEdit.UpdateAppShortcut(
                        txt_shortcut_save_name.Text,
                        _profileToUse,
                        _appToUse,
                        _displayPermanence,
                        _audioPermanence,
                        _selectedImage,
                        _availableImages,
                        _appToUse.AppToUse.AppLibraryType,
                        _audioProfileToUse,
                        _overrideAudioSpeakerVolume,
                        _overrideAudioSpeakerVolumeLevel,
                        _overrideAudioMicrophoneVolume,
                        _overrideAudioMicrophoneVolumeLevel,
                        _startPrograms,
                        _afterPrograms,
                        _stopPrograms,
                        _autoName
                    );
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutForm/btn_save_Click: Exception while trying to update an application shortcut.");
                }
            }
            else if (_shortcutCategory == ShortcutCategory.NoGame)
            {
                logger.Trace($"ShortcutForm/btn_save_Click: We're not saving any game or executable to start!");
                try
                {
                    _shortcutToEdit.UpdateNoGameShortcut(
                        txt_shortcut_save_name.Text,
                        _profileToUse,
                        _displayPermanence,
                        _audioPermanence,
                        _audioProfileToUse,
                        _overrideAudioSpeakerVolume,
                        _overrideAudioSpeakerVolumeLevel,
                        _overrideAudioMicrophoneVolume,
                        _overrideAudioMicrophoneVolumeLevel,
                        _startPrograms,
                        _afterPrograms,
                        _stopPrograms,
                        _autoName
                    );
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"ShortcutForm/btn_save_Click: Exception while trying to update a shortcut that doesn't run anything.");
                }
            }
            else
            {
                logger.Error($"ShortcutForm/btn_save_Click: We're unable to save as the Shortut Category isn't a category we support! {_shortcutCategory.ToString("G")}");
            }

            /*if (_hotkey == Keys.None)
                // Remove the Hotkey if it needs to be removed
                HotkeyManager.Current.Remove(_shortcutToEdit.UUID);
            else
                // Set the hokey if there is one
                HotkeyManager.Current.AddOrReplace(_shortcutToEdit.UUID, _shortcutToEdit.Hotkey, OnWindowHotkeyPressed);*/

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
                if (File.Exists(dialog_open.FileName) && ProcessUtils.IsExecutableFileType(dialog_open.FileName))
                {
                    txt_executable.Text = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        "Selected file is not a valid file.",
                        "Executable",
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

            // If the user has typed or pasted a valid exe path directly (rather than using the browse button),
            // we still need to scan it for icons — otherwise ShortcutBitmap will be null and the renderer crashes.
            // Restart the debounce timer so we only scan once the user stops typing.
            string typedPath = txt_executable.Text.Trim();
            if (File.Exists(typedPath) && ProcessUtils.IsExecutableFileType(typedPath))
            {
                _exePathDebounceTimer.Stop();
                _exePathDebounceTimer.Start();
            }
        }

        private bool AllowedToSave(bool showErrorsToUser = false)
        {
            // initialise errors list
            List<string> errors = new List<string>();

            // Check the name is valid to save
            if (String.IsNullOrWhiteSpace(txt_shortcut_save_name.Text))
            {
                logger.Error($"ShortcutForm/AllowedToSave: The shortcut doesn't have a name yet!");
                errors.Add("You need to specify a name for this Shortcut before it can be saved.");
            }

            // Check the profile is set and that it's still valid
            if (!(_profileToUse is ProfileItem))
            {
                logger.Error($"ShortcutForm/AllowedToSave: The shortcut doesn't have a display profile selected!");
                errors.Add("You need to select a Display Profile to use with this shortcut. Please select one from the list of Display Profiles on the left of the screen.");
            }

            // Check the Shortcut Category to see if it's application
            if (rb_standalone.Checked)
            {
                if (_shortcutCategory == ShortcutCategory.Executable)
                {
                    if (!File.Exists(txt_executable.Text))
                    {
                        logger.Error($"ShortcutForm/AllowedToSave: The executable {txt_executable.Text} doesn't exist. Please check the file '{txt_executable.Text}' is still there, and that the file has the correct permissions.");
                        errors.Add("The executable you have chosen does not exist! Please reselect the executable using the Choose button, verify the path entered is correct, or check that you have permissions to view it.");
                    }
                    else if (!ProcessUtils.IsExecutableFileType(txt_executable.Text))
                    {
                        logger.Error($"ShortcutForm/AllowedToSave: The file '{txt_executable.Text}' is not a supported executable type.");
                        errors.Add($"The file '{Path.GetFileName(txt_executable.Text)}' is not a supported executable type. Please choose an executable file (.exe, .com, .msi, .bat, .cmd, .ps1, .lnk, or .url).");
                    }


                    if (cb_args_executable.Checked && String.IsNullOrWhiteSpace(txt_args_executable.Text))
                    {
                        logger.Error($"ShortcutForm/AllowedToSave: The user wants to provide executable args but hasn't provided any.");
                        errors.Add("If you have chosen to pass extra arguments to the executable when it is run, then you need to enter them in the 'Pass arguments to Executable' field.If you didn't want to pass extra arguments then please uncheck the 'Pass arguments to Executable' checkbox.");
                    }


                    if (rb_wait_alternative_executable.Checked)
                    {
                        if (String.IsNullOrWhiteSpace(txt_alternative_executable.Text))
                        {
                            logger.Error($"ShortcutForm/AllowedToSave: The user asked to wait for an alternative executable as part of an executable shortcut, but failed to provide one!");
                            errors.Add("If you have chosen to monitor an alternative executable when this Executable shortcut is run, then you need to select that alternative executable using the Choose button or paste in a valid path into the alternative executable textbox. If you didn't want to monitor an alternative executable then please select the 'Wait until the executable above is closed before continuing' option instead.");
                        }
                        else if (!File.Exists(txt_alternative_executable.Text))
                        {
                            logger.Error($"ShortcutForm/AllowedToSave: The alternative executable the user wants to monitor as part of an executable shortcut doesn't exist. Please check the file '{txt_alternative_executable.Text}' is still there, and that the file has the correct permissions.");
                            errors.Add("The alternative executable you have chosen does not exist! Please reselect the alternative executable using the Choose button, verify the path entered is correct, or check that you have permissions to view it.");
                        }
                        else if (!ProcessUtils.IsExecutableFileType(txt_alternative_executable.Text))
                        {
                            logger.Error($"ShortcutForm/AllowedToSave: The alternative executable file '{txt_alternative_executable.Text}' is not a supported executable type.");
                            errors.Add($"The alternative executable file '{Path.GetFileName(txt_alternative_executable.Text)}' is not a supported executable type. Please choose a file with an executable extension (.exe, .com, .msi, .bat, .cmd, .ps1, .lnk, or .url).");
                        }
                    }
                }
                else if (_shortcutCategory == ShortcutCategory.Application)
                {
                    // If it is a UWP application
                    if (txt_executable.Text.EndsWith("explorer.exe") && txt_args_executable.Text.StartsWith("shell:AppsFolder"))
                    {
                        if (!File.Exists(txt_executable.Text))
                        {
                            logger.Error($"ShortcutForm/AllowedToSave: The explorer executable {txt_executable.Text} used to launch UWP apps doesn't exist. Please check the file '{txt_executable.Text}' is still there, and that the file has the correct permissions.");
                            errors.Add("The explorer executable {txt_executable.Text} used to launch UWP apps does not exist! Please reselect the Application you want to launch using the Choose button.");
                        }

                        if (!cb_args_executable.Checked)
                        {
                            logger.Error($"ShortcutForm/AllowedToSave: The user has selected an UWP Application but the executable args aoption isn't selected which it has to be.");
                            errors.Add("You have chosen a UWP Application to run and monitor for this shortcut, but you have not selected to use the executable args option. This option is required in order to be valid. Please reselect the Application you want to launch using the Choose button.");
                        }

                        if (cb_args_executable.Checked && !txt_args_executable.Text.StartsWith("shell:AppsFolder"))
                        {
                            logger.Error($"ShortcutForm/AllowedToSave: The user has selected an UWP Application but the executable args doesn't start with 'shell:AppsFolder' which they have to do.");
                            errors.Add("You have chosen a UWP Application to run and monitor for this shortcut, but the executable args don't start with 'shell:AppsFolder' which they have to do in order to be valid. Please reselect the Application you want to launch using the Choose button.");
                        }

                    }

                    if (rb_wait_alternative_executable.Checked)
                    {
                        if (String.IsNullOrWhiteSpace(txt_alternative_executable.Text))
                        {
                            logger.Error($"ShortcutForm/AllowedToSave: The user asked to wait for an alternative executable as part of an application shortcut, but failed to provide one!");
                            errors.Add("If you have chosen to monitor an alternative executable when this Application shortcut is run, then you need to select that alternative executable using the Choose button or paste in a valid path into the alternative executable textbox. If you didn't want to monitor an alternative executable then please select the 'Wait until the executable above is closed before continuing' option instead.");
                        }
                        else if (!File.Exists(txt_alternative_executable.Text))
                        {
                            logger.Error($"ShortcutForm/AllowedToSave: The alternative executable the user wants to monitor as part of an application shortcut doesn't exist. Please check the file '{txt_alternative_executable.Text}' is still there, and that the file has the correct permissions.");
                            errors.Add("The alternative executable you have chosen does not exist! Please reselect the alternative executable using the Choose button, verify the path entered is correct, or check that you have permissions to view it.");
                        }
                    }
                }
            }
            else if (rb_launcher.Checked)
            {

                if (cb_args_game.Checked && String.IsNullOrWhiteSpace(txt_args_game.Text))
                {
                    logger.Error($"ShortcutForm/AllowedToSave: The user wanted to pass arguments to the game executable, but failed to provide any!");
                    errors.Add("If you have chosen to pass extra arguments to the game when it is run, then you need to enter them in the 'Pass arguments to Game' field. If you didn't want to pass extra arguments then please uncheck the 'Pass arguments to Game' checkbox.");
                }

                if (_gameId.Equals("0"))
                {
                    logger.Error($"ShortcutForm/AllowedToSave: The game ID provided is 0, and this is invalid. We cannot run the game.");
                    errors.Add("No game has been selected. Please choose a game from the list of games shown in the list.");
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
                    logger.Error($"ShortcutForm/AllowedToSave: The {_gameLauncher} game with ID {_gameId} isn't installed at present, so can't be used!");
                    errors.Add("This shortcut uses a game that is no longer installed on this computer. Please choose a different game from the list of games shown in the list, or reinstall the game this shortcut uses.");
                }

                if (cb_wait_alternative_game.Checked)
                {
                    if (String.IsNullOrWhiteSpace(txt_alternative_game.Text))
                    {
                        logger.Error($"ShortcutForm/AllowedToSave: The user asked to monitor an alternative game executable, but failed to provide one!");
                        errors.Add("If you have chosen to monitor a different game executable, then you need to select that different game executable using the Choose button or paste in a valid path into the 'Monitor different executable' textbox. If you didn't want to monitor a different game executable then please uncheck the 'Monitor different executable' checkbox.");
                    }

                    if (!File.Exists(txt_alternative_game.Text))
                    {
                        logger.Error($"ShortcutForm/AllowedToSave: The alternative game executable the user wants to monitor doesn't exist. Please check the file '{txt_alternative_game.Text}' is still there, and that the file has the correct permissions.");
                        errors.Add("The different game executable you have chosen to monitor does not exist! Please reselect the different game executable using the Choose button, verify the path entered is correct, or check that you have permissions to view it.");
                    }
                    else if (!ProcessUtils.IsExecutableFileType(txt_alternative_game.Text))
                    {
                        logger.Error($"ShortcutForm/AllowedToSave: The alternative game executable '{txt_alternative_game.Text}' is not a valid executable file type.");
                        errors.Add("The different game executable you have chosen to monitor is not a valid executable file type. Please select a valid executable (.exe, .com, .bat, .cmd, .ps1, .lnk, .url, .msi) using the Choose button.");
                    }

                }

            }

            // Look for any start or stop programs without an exe
            foreach (Control ctrl in flp_start_programs.Controls)
            {
                if (ctrl is StartProgramControl spControl)
                {
                    if (String.IsNullOrWhiteSpace(spControl.StartProgram.Executable))
                    {
                        int priority = flp_start_programs.Controls.GetChildIndex(spControl) + 1;
                        logger.Error($"ShortcutForm/AllowedToSave: The start program at position #{priority} doesn't have an executable listed");
                        errors.Add($"The start program executable at position #{priority} doesn't have an executable listed. Please either add a valid path to an executable, or remove the start program from the list.");
                    }
                }
                else if (ctrl is StopProgramControl stpControl)
                {
                    if (String.IsNullOrWhiteSpace(stpControl.StopProgram.Executable))
                    {
                        int priority = flp_start_programs.Controls.GetChildIndex(stpControl) + 1;
                        logger.Error($"ShortcutForm/AllowedToSave: The stop program at position #{priority} doesn't have an executable listed");
                        errors.Add($"The stop program executable at position #{priority} doesn't have an executable listed. Please either add a valid path to an executable, or remove the stop program from the list.");
                    }
                }
            }

            // Check the stop program has an exe in there
            if (cb_run_cmd_afterwards.Checked)
            {
                if (String.IsNullOrWhiteSpace(txt_run_cmd_afterwards.Text))
                {
                    logger.Error($"ShortcutForm/AllowedToSave: The run command afterwards command is selected, yet doesn't have an executable listed");
                    errors.Add($"The 'Run this program' option in the 'Run a program or command afterwards' section is selected, but the textbox is empty. Please either paste in a valid executable path into the textbox, or uncheck the 'Run this program' checkbox.");
                }

                if (!File.Exists(txt_run_cmd_afterwards.Text))
                {
                    logger.Error($"ShortcutForm/AllowedToSave: run command afterwards command is selected but the run cmd afterwards doesn't exist. Please check the file '{txt_run_cmd_afterwards.Text}' is still there, and that the file has the correct permissions.");
                    errors.Add($"You have checked the 'Run this program' option in the 'Run a program or command afterwards' section, but the '{txt_run_cmd_afterwards.Text}' executable you have chosen to run afterwards does not exist! Please select an executable using the Choose button, verify the path entered is correct, or check that you have permissions to view it.");
                }

                if (cb_run_cmd_afterwards_args.Checked)
                {
                    if (String.IsNullOrWhiteSpace(txt_run_cmd_afterwards_args.Text))
                    {
                        logger.Error($"ShortcutForm/AllowedToSave: The user asked to pass arguments to the run_cmd_afterwards '{txt_run_cmd_afterwards.Text}' executable, but failed to provide the args!");
                        errors.Add($"If you have chosen to pass arguments to the '{txt_run_cmd_afterwards.Text}' executable, then you need to actually provide arguments. If you didn't want to provide args then please uncheck the 'Pass Arguments' checkbox in the 'Run a program or command afterwards' section.");
                    }
                }
            }

            // Show the errors if we have any
            // and then return how the checks all went.
            if (errors.Count > 0)
            {
                if (showErrorsToUser)
                {
                    ShortcutErrorForm errorForm = new ShortcutErrorForm();
                    errorForm.Errors = errors;
                    errorForm.ShowDialog();

                }
                return false;
            }
            else
            {
                return true;
            }
        }

        private void SuggestShortcutName()
        {
            if (_autoName && _profileToUse is ProfileItem)
            {
                if (_shortcutCategory == ShortcutCategory.NoGame)
                {
                    if (rb_switch_display_permanent.Checked)
                        txt_shortcut_save_name.Text = $"{_profileToUse.Name}";
                    else if (rb_switch_display_temp.Checked)
                        txt_shortcut_save_name.Text = $"{_profileToUse.Name} (Temporary)";
                }
                else if (_shortcutCategory == ShortcutCategory.Game && _selectedGame is Game)
                {
                    txt_shortcut_save_name.Text = $"{_selectedGame.Name} ({_profileToUse.Name})";
                }
                else if (_shortcutCategory == ShortcutCategory.Application && _selectedApp is App)
                {
                    txt_shortcut_save_name.Text = $"{_selectedApp.Name} ({_profileToUse.Name})";
                }
                else if (_shortcutCategory == ShortcutCategory.Executable && !String.IsNullOrWhiteSpace(txt_executable.Text))
                {
                    string baseName = Path.GetFileNameWithoutExtension(txt_executable.Text);
                    txt_shortcut_save_name.Text = $"{baseName} ({_profileToUse.Name})";
                }


            }
        }


        private void UpdateProfileImageListView(ProfileItem profile)
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
            _shortcutCategory = ShortcutCategory.Game;
            _displayPermanence = ShortcutPermanence.Temporary;
            _audioPermanence = ShortcutPermanence.Temporary;
            _startPrograms = new List<StartProgram>();
            _afterPrograms = new List<AfterProgram>();
            _stopPrograms = new List<StopProgram>();
            _audioProfileToUse = null;
            _selectedGame = null;
            _selectedApp = null;
            _selectedAppId = "";
            _isUnsaved = true;
            _loadedShortcut = false;
            _autoName = true;
            _gameId = "0";
            _uuid = "";


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
            RefreshAudioProfilesList();


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

            // Set audio defaults
            cb_dont_change_audio.Checked = true;
            rb_switch_audio_temp.Checked = true;

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
            // Set the shortcut mode
            _shortcutCategory = _shortcutToEdit.Category;

            _isUnsaved = false;

            // Populate all the Games into the Games ListView            
            ilv_games.Enabled = true;
            ilv_games.Visible = true;
            ilv_games.SuspendLayout();
            ilv_games.Items.Clear();

            // Add the rest of the true profiles
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

            // =============================================
            // IF THE SHORTCUT IS AN EXISTING SHORTCUT
            // =============================================
            bool shortcutTweakChangesName = false;
            if (_editingExistingShortcut && _shortcutToEdit is ShortcutItem)
            {
                // *** Main Shortcut controls ***
                // Set the shortcut name
                txt_shortcut_save_name.Text = _shortcutToEdit.Name;
                // Set the autoname checkbox
                cb_autosuggest.Checked = _shortcutToEdit.AutoName;

                // Set the Hotkey text
                //UpdateHotkeyLabel(_shortcutToEdit.Hotkey);

                // *** 1. Choose Display Profile Tab ***
                // Find the profile
                if (_shortcutToEdit.ProfileUUID.Equals(ProfileItem.SkipDisplayChangeUUID, StringComparison.InvariantCulture))
                {
                    chosenProfile = ShortcutItem.SkipDisplayChangeProfile;
                    foundChosenProfileInLoadedProfiles = true;
                }
                else if (ProfileRepository.ContainsProfile(_shortcutToEdit.ProfileUUID))
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
                        @"The Display Profile used by this Shortcut no longer exists and cannot be used. We have selected the current Display Profile instead. You can choose a different Display Profile for this Shortcut if you wish.",
                        @"Display Profile no longer exists",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    chosenProfile = ProfileRepository.CurrentProfile;
                    shortcutTweakChangesName = true;
                    _isUnsaved = true;

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
                            shortcutTweakChangesName = true;
                            _isUnsaved = true;
                        }

                    }

                }

                _profileToUse = chosenProfile;
                // Also need to select the chosenProfile in the UI so it gets saved properly
                if (chosenProfile != null)
                {
                    foreach (var item in ilv_saved_profiles.Items)
                    {
                        if (item.Text == chosenProfile.Name)
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                }

                // *** 2. Choose Audio Tab ***
                RefreshAudioProfilesList();
                if (_shortcutToEdit.AudioProfileUUID.Equals(AudioProfileItem.SkipAudioProfilesChangeUUID, StringComparison.OrdinalIgnoreCase))
                {
                    cb_dont_change_audio.Checked = true;
                    lb_audio_profiles.ClearSelected();
                    _audioProfileToUse = null;
                }
                else
                {
                    cb_dont_change_audio.Checked = false;
                    AudioProfileItem selectedAudioProfile = AudioProfileRepository.GetAudioProfile(_shortcutToEdit.AudioProfileUUID);
                    if (selectedAudioProfile != null)
                    {
                        _audioProfileToUse = selectedAudioProfile;
                        lb_audio_profiles.SelectedItem = selectedAudioProfile;
                    }
                }

                // *** 3. Choose what happens before Tab ***
                // Set up the start programs
                if (_shortcutToEdit.StartPrograms is List<StartProgram> && _shortcutToEdit.StartPrograms.Count > 0)
                {
                    flp_start_programs.Controls.Clear();

                    Padding firstStartProgramMargin = new Padding(10) { };
                    Padding otherStartProgramMargin = new Padding(10, 0, 10, 10) { };

                    // Order the initial list in order of priority
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

                    // Load StopPrograms
                    if (_shortcutToEdit.StopPrograms != null)
                    {
                        foreach (StopProgram myStopProgram in _shortcutToEdit.StopPrograms.OrderBy(sp => sp.Priority))
                        {
                            if (String.IsNullOrWhiteSpace(myStopProgram.Executable))
                                continue;

                            StopProgramControl stopProgramControl = new StopProgramControl(myStopProgram, spOrder);
                            stopProgramControl.Dock = DockStyle.None;
                            stopProgramControl.Margin = spOrder == 1 ? firstStartProgramMargin : otherStartProgramMargin;
                            stopProgramControl.Width = flp_start_programs.Width - 40;
                            stopProgramControl.MouseDown += new MouseEventHandler(StartProgramControl_MouseDown);
                            stopProgramControl.DragOver += new DragEventHandler(StartProgramControl_DragOver);
                            stopProgramControl.DragDrop += new DragEventHandler(StartProgramControl_DragDrop);
                            stopProgramControl.AllowDrop = true;
                            flp_start_programs.Controls.Add(stopProgramControl);
                            spOrder++;
                        }
                    }
                }
                else
                {
                    flp_start_programs.Controls.Clear();
                }

                // =============================================
                // IF THE EXISTING SHORTCUT IS AN EXECUTABLE
                // =============================================
                if (_shortcutCategory == ShortcutCategory.Executable)
                {

                    rb_standalone.Checked = true;

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
                        if (_shortcutToEdit.DifferentExecutableToMonitor != null && !String.IsNullOrWhiteSpace(_shortcutToEdit.DifferentExecutableToMonitor) && File.Exists(_shortcutToEdit.DifferentExecutableToMonitor))
                        {
                            _availableImages.AddRange(ImageUtils.GetMeAllBitmapsFromFile(_shortcutToEdit.DifferentExecutableToMonitor));
                        }

                        if (_availableImages.Count == 0)
                        {
                            logger.Trace($"ShortcutForm/ShortcutForm_Load: Unknown Game Library, so using the DisplayMagician icon as the icon instead.");
                            ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.displaymagician.ToBitmap(), "DisplayMagician Icon", "", 0);
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
                    _selectedAppId = "";
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
                    if (_shortcutToEdit.DifferentExecutableToMonitor == null || String.IsNullOrWhiteSpace(_shortcutToEdit.DifferentExecutableToMonitor))
                    {
                        txt_alternative_executable.Text = "";
                    }
                    else
                    {
                        txt_alternative_executable.Text = _shortcutToEdit.DifferentExecutableToMonitor;
                    }

                }
                // =============================================
                // IF THE EXISTING SHORTCUT IS AN APPLICATION
                // =============================================
                else if (_shortcutCategory == ShortcutCategory.Application)
                {

                    rb_standalone.Checked = true;

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
                            ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.displaymagician.ToBitmap(), "DisplayMagician Icon", "", 0);
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
                    _selectedAppId = _shortcutToEdit.ApplicationId;
                    // Now lets try and find the games
                    _selectedApp = LocalLibrary.GetAnyAppById(_selectedAppId);


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
                    if (_shortcutToEdit.DifferentExecutableToMonitor == null || String.IsNullOrWhiteSpace(_shortcutToEdit.DifferentExecutableToMonitor))
                    {
                        txt_alternative_executable.Text = "";
                    }
                    else
                    {
                        txt_alternative_executable.Text = _shortcutToEdit.DifferentExecutableToMonitor;
                    }

                }
                // =============================================
                // IF THE EXISTING SHORTCUT IS A GAME
                // =============================================
                else if (_shortcutCategory == ShortcutCategory.Game)
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
                        if (String.IsNullOrWhiteSpace(_shortcutToEdit.DifferentGameExeToMonitor))
                        {
                            txt_alternative_game.Text = "";
                        }
                        else
                        {
                            txt_alternative_game.Text = _shortcutToEdit.DifferentGameExeToMonitor;
                        }
                    }
                    else
                    {
                        cb_wait_alternative_game.Checked = false;
                        txt_alternative_game.Text = "";
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
                                ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.displaymagician.ToBitmap(), "DisplayMagician Icon", "", 0);
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
                    rb_no_game.Checked = true;

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

                // Setup the single stop program we're beginning with
                if (_shortcutToEdit.AfterPrograms is List<AfterProgram> && _shortcutToEdit.AfterPrograms.Count > 0)
                {
                    if (_shortcutToEdit.AfterPrograms[0].Disabled == false)
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

                    txt_run_cmd_afterwards.Text = _shortcutToEdit.AfterPrograms[0].Executable;
                    if (_shortcutToEdit.AfterPrograms[0].ExecutableArgumentsRequired)
                    {
                        cb_run_cmd_afterwards_args.Checked = true;
                        txt_run_cmd_afterwards_args.Text = _shortcutToEdit.AfterPrograms[0].Arguments;
                    }
                    else
                    {

                        cb_run_cmd_afterwards_args.Checked = false;
                        txt_run_cmd_afterwards_args.Text = "";

                    }
                    if (_shortcutToEdit.AfterPrograms[0].DontStartIfAlreadyRunning)
                    {
                        cb_run_cmd_afterwards_dont_start.Checked = true;
                    }
                    else
                    {
                        cb_run_cmd_afterwards_dont_start.Checked = false;
                    }
                    if (_shortcutToEdit.AfterPrograms[0].RunAsAdministrator)
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
                            shortcutTweakChangesName = true;
                        }

                    }
                    _profileToUse = chosenProfile;
                    // Also need to select the chosenProfile in the UI so it gets saved properly
                    foreach (var item in ilv_saved_profiles.Items)
                    {
                        if (item.Text == chosenProfile.Name)
                        {
                            item.Selected = true;
                            break;
                        }
                    }
                }

                // Set up the new shortcut as a game
                _shortcutToEdit.Category = ShortcutCategory.Game;
                _shortcutCategory = ShortcutCategory.Game;
                rb_launcher.Checked = true;

                // Set up display permanance as temporary
                _shortcutToEdit.DisplayPermanence = ShortcutPermanence.Temporary;
                rb_switch_display_temp.Checked = true;

                // Set up audio permanance as temporary
                _shortcutToEdit.AudioPermanence = ShortcutPermanence.Temporary;
                rb_switch_audio_temp.Checked = true;

                // Set up the Game Timeouts
                nud_timeout_game.Value = _shortcutToEdit.StartTimeout;
            }

            if (shortcutTweakChangesName && cb_autosuggest.Checked)
            {
                SuggestShortcutName();
            }

            // Refresh the Shortcut UI
            RefreshShortcutUI();
            ChangeSelectedProfile(chosenProfile);
            //RefreshImageListView(chosenProfile);

            _loadedShortcut = true;

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

            //Utils.CenterOnPrimaryScreen(this);
            //this.Focus();
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

                if (_shortcutCategory == ShortcutCategory.Application)
                {
                    _shortcutCategory = ShortcutCategory.Application;
                }
                else
                {
                    _shortcutCategory = ShortcutCategory.Executable;
                }

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
            }

        }

        private void rb_launcher_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_launcher.Checked)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;

                _shortcutCategory = ShortcutCategory.Game;

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
                    _gameLauncher = _selectedGame.GameLibraryType.ToString("G");
                    lbl_game_library.Text = $"Game Library: {_gameLauncher}";
                    _gameId = _selectedGame.Id;
                    _availableImages = _selectedGame.AvailableGameBitmaps ?? new List<ShortcutBitmap>();
                    _shortcutToEdit.AvailableImages = _availableImages;
                    _selectedImage = _availableImages.Count > 0
                        ? ImageUtils.GetMeLargestAvailableBitmap(_availableImages)
                        : new ShortcutBitmap();
                    _shortcutToEdit.SelectedImage = _selectedImage;
                    txt_game_name.Text = _selectedGame.Name;
                    pb_game_icon.Image = _selectedImage.Image;
                    btn_choose_game_icon.Enabled = true;
                }

                SuggestShortcutName();
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

                _shortcutCategory = ShortcutCategory.NoGame;

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
            SuggestShortcutName();
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
            // _executableToUse is only populated after Save; fall back to the textbox or ProgramFiles
            string initialDir = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            if (!String.IsNullOrWhiteSpace(txt_executable.Text) && File.Exists(txt_executable.Text))
                initialDir = Path.GetDirectoryName(txt_executable.Text);
            else if (!String.IsNullOrWhiteSpace(_executableToUse.ExecutableNameAndPath))
                initialDir = Path.GetDirectoryName(_executableToUse.ExecutableNameAndPath);
            dialog_open.InitialDirectory = initialDir;
            dialog_open.DefaultExt = "*.exe";
            dialog_open.Filter = "Executable files (*.exe;*.com;*.msi;*.bat;*.cmd;*.ps1;*.lnk;*.url) | *.exe;*.com;*.msi;*.bat;*.cmd;*.ps1;*.lnk;*.url | All files(*.*) | *.*";
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                if (File.Exists(dialog_open.FileName) && ProcessUtils.IsExecutableFileType(dialog_open.FileName))
                {
                    txt_alternative_executable.Text = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        "Selected file is not a valid file.",
                        "Executable",
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
            }
            else
            {
                txt_args_game.Enabled = false;
            }
        }

        private void ilv_saved_profiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            // Check if the user clicked the special "Skip Display Change" item first
            if (e.Item.EquipmentModel == ProfileItem.SkipDisplayChangeUUID)
            {
                ChangeSelectedProfile(ShortcutItem.SkipDisplayChangeProfile);
                SuggestShortcutName();
                return;
            }

            foreach (ProfileItem savedProfile in ProfileRepository.AllProfiles)
            {
                if (savedProfile.Name == e.Item.Text)
                {
                    ChangeSelectedProfile(savedProfile);
                    break;
                }
            }

            SuggestShortcutName();

        }

        private void ChangeSelectedProfile(ProfileItem profile)
        {
            // If the profile is null then return
            // (this happens when a new blank shortcut is created)
            if (profile == null)
                return;

            // And we need to update the actual selected profile too!
            _profileToUse = profile;

            // Handle the special "Skip Display Change" profile
            if (profile.UUID == ProfileItem.SkipDisplayChangeUUID)
            {
                lbl_profile_shown.Text = ProfileItem.SkipDisplayChangeName;
                lbl_profile_shown_subtitle.Text = "The display configuration will not be changed when this shortcut runs.";
                lbl_profile_shown_subtitle.Visible = true;

                // Clear the display view - DrawEmptyView will be called automatically
                dv_profile.Profile = null;
                dv_profile.Refresh();

                UpdateProfileImageListView(profile);
                return;
            }

            // We also need to load the saved profile name to show the user
            lbl_profile_shown.Text = _profileToUse.Name;

            if (_profileToUse.Equals(ProfileRepository.CurrentProfile))
            {
                lbl_profile_shown_subtitle.Text = "This is the Display Profile currently in use.";
                lbl_profile_shown_subtitle.Visible = true;
            }
            else
            {
                lbl_profile_shown_subtitle.Text = "";
                lbl_profile_shown_subtitle.Visible = false;
            }

            // Refresh the image list view
            UpdateProfileImageListView(profile);

            // And finally show the profile in the display view
            dv_profile.Profile = profile;
            dv_profile.Refresh();
        }


        private void RefreshShortcutUI()
        {


            //if (ProfileRepository.ProfileCount > 0)
            //{

            // Temporarily stop updating the saved_profiles listview
            ilv_saved_profiles.SuspendLayout();

            // Clear the saved_profiles listview so we can refill it with all the profiles (including the new one if we just created a new shortcut)
            ilv_saved_profiles.Items.Clear();

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

            // Add the Skip Display Change option last
            ilv_saved_profiles.Items.Add(_skipDisplayChangeILVItem, _profileAdaptor);


            // Restart updating the saved_profiles listview
            ilv_saved_profiles.ResumeLayout();
            //}

            //UpdateHotkeyLabel(_shortcutToEdit.Hotkey);
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

        private void groupbox_Paint(object sender, PaintEventArgs e)
        {

            base.OnPaint(e);

            GroupBox groupbox = sender as GroupBox;

            if (!groupbox.Enabled)
            {
                int x = ClientRectangle.X + 3;
                int y = ClientRectangle.Y;

                TextRenderer.DrawText(e.Graphics, groupbox.Text,
                    groupbox.Font, new Point(x, y), Color.Gray,
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
        }

        private void ShortcutForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            if (_isUnsaved && _loadedShortcut)
            {
                // If the user doesn't want to close this window without saving (when they can save), then don't close the window.
                DialogResult result = MessageBox.Show(
                    @"You have unsaved changes! Do you want to save your changes?",
                    @"You have unsaved changes.",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes)
                {
                    logger.Trace($"ShortcutForm/ShortcutForm_FormClosing: User said yes they want us to save their changes. SO attempting to save changes");
                    if (AllowedToSave(false))
                    {
                        // Press the save button for the user as we're allowed to save
                        btn_save.PerformClick();
                    }
                    else
                    {
                        // We're not allowed to save so record this in the logs
                        logger.Warn($"ShortcutForm/ShortcutForm_FormClosing: The shortcut isn't valid, so we'll skip closing the form to let the user modify it without losing their changes.");
                        AllowedToSave(true);
                        // Cancel the event!
                        e.Cancel = true;
                        this.Show();
                    }
                }
                else
                {
                    logger.Trace($"ShortcutForm/ShortcutForm_FormClosing: User said no, they do not want us to save their changes. So just closing the shortcut form.");
                }

            }

        }

        private void btn_exe_to_start_Click(object sender, EventArgs e)
        {
            ChooseExecutableForm exeForm = new ChooseExecutableForm();
            if (!String.IsNullOrWhiteSpace(txt_executable.Text))
            {
                if (_shortcutCategory == ShortcutCategory.Application)
                {
                    exeForm.Mode = ChooseExecutableFormMode.AppMode;
                    exeForm.PreviousAppId = _selectedAppId;
                    exeForm.PreviousExe = txt_executable.Text;
                }
                else if (_shortcutCategory == ShortcutCategory.Executable)
                {
                    exeForm.Mode = ChooseExecutableFormMode.ExeMode;
                    exeForm.PreviousExe = txt_executable.Text;
                    exeForm.PreviousAppId = "";

                }
            }

            if (exeForm.ShowDialog() == DialogResult.OK)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                if (exeForm.Mode == ChooseExecutableFormMode.AppMode)
                {
                    _shortcutCategory = ShortcutCategory.Application;
                    _selectedApp = exeForm.AppToUse;
                    _selectedAppId = exeForm.AppToUse.Id;
                    txt_executable.Text = _selectedApp.ExePath;
                    if (!String.IsNullOrEmpty(_selectedApp.Arguments))
                    {
                        txt_args_executable.Text = _selectedApp.Arguments;
                        cb_args_executable.Checked = true;
                    }
                    UpdateExeImagesUI(_selectedApp);

                    // TODO: Possibly re-enable this messaage if we are still haviung issues after testing.
                    /*if (txt_executable.Text.EndsWith("explorer.exe") && txt_args_executable.Text.StartsWith("shell:AppsFolder"))
                    {
                        // At this point DisplayMagician can't track the processes of UWP apps due to WIndows Permission Model. It only allows applications that are installed with a
                        // PackageIdentity to access the Process information for UWP apps. This can't be done with .msi installer files :(.
                        MessageBox.Show(
                        $"You have selected a UWP-style application. Windows Permissions Model prevents DisplayMagician from monitoring UWP applications. This means that DisplayMagician will not wait for your UWP application, and will assume it wasn't opened (even though Windows did start it!). This will be fixed in a future version of DisplayMagician.",
                        @"DisplayMagician cannot monitor UWP applications",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                    }*/
                }
                else
                {
                    _shortcutCategory = ShortcutCategory.Executable;
                    _selectedApp = null;
                    _selectedAppId = "";
                    txt_executable.Text = exeForm.ExeToUse;
                    UpdateExeImagesUI(_selectedApp);
                }
                SuggestShortcutName();
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
            if (_availableImages.Count > 0)
            {
                _selectedImage = ImageUtils.GetMeLargestAvailableBitmap(_availableImages);
                _shortcutToEdit.SelectedImage = _selectedImage;
            }
            else
            {
                ShortcutBitmap bm = ImageUtils.CreateShortcutBitmap(Properties.Resources.displaymagician.ToBitmap(), "DisplayMagician Icon", "", 0);
                _availableImages.Add(bm);
                _selectedImage = bm;
                _shortcutToEdit.SelectedImage = _selectedImage;
            }
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
            dialog_open.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            dialog_open.DefaultExt = "*.exe";
            dialog_open.Filter = "Executable files (*.exe;*.com;*.msi;*.bat;*.cmd;*.ps1;*.lnk;*.url) | *.exe;*.com;*.msi;*.bat;*.cmd;*.ps1;*.lnk;*.url | All files(*.*) | *.*";
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
                        "Selected file is not a valid file.",
                        "Executable",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
            return textToReturn;
        }

        private void RefreshAudioProfilesList()
        {
            lb_audio_profiles.Items.Clear();
            foreach (AudioProfileItem audioProfile in AudioProfileRepository.AllAudioProfiles.OrderBy(p => p.Name))
            {
                lb_audio_profiles.Items.Add(audioProfile);
            }
        }

        private void SetAudioProfileUiEnabled(bool enabled)
        {
            gb_audio_overrides.Enabled = enabled;
        }

        private string PromptForAudioProfileName(string title, string currentValue = "")
        {
            using (Form prompt = new Form())
            {
                prompt.Width = 520;
                prompt.Height = 180;
                prompt.FormBorderStyle = FormBorderStyle.FixedDialog;
                prompt.Text = title;
                prompt.StartPosition = FormStartPosition.CenterParent;
                prompt.MinimizeBox = false;
                prompt.MaximizeBox = false;

                Label textLabel = new Label() { Left = 12, Top = 12, Width = 480, Text = "Audio Profile name:" };
                TextBox inputBox = new TextBox() { Left = 12, Top = 40, Width = 480, Text = currentValue ?? string.Empty };
                Button confirmation = new Button() { Text = "OK", Left = 326, Width = 80, Top = 80, DialogResult = DialogResult.OK };
                Button cancel = new Button() { Text = "Cancel", Left = 412, Width = 80, Top = 80, DialogResult = DialogResult.Cancel };

                prompt.Controls.Add(textLabel);
                prompt.Controls.Add(inputBox);
                prompt.Controls.Add(confirmation);
                prompt.Controls.Add(cancel);
                prompt.AcceptButton = confirmation;
                prompt.CancelButton = cancel;

                return prompt.ShowDialog(this) == DialogResult.OK ? inputBox.Text?.Trim() : string.Empty;
            }
        }

        private void cb_dont_change_audio_CheckedChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;

            bool changeAudio = !cb_dont_change_audio.Checked;
            SetAudioProfileUiEnabled(changeAudio);
            if (!changeAudio)
            {
                lb_audio_profiles.ClearSelected();
                _audioProfileToUse = null;
                gb_audio_profile.Enabled = false;
                txt_audio_profile_settings.Text = string.Empty;
            }
            else
            {
                gb_audio_profile.Enabled = true;
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

        private void lb_audio_profiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadedShortcut)
                _isUnsaved = true;
            _audioProfileToUse = lb_audio_profiles.SelectedItem as AudioProfileItem;
            btn_update_audio_profile.Enabled = _audioProfileToUse != null;
            btn_delete_audio_profile.Enabled = _audioProfileToUse != null;
            gb_audio_profile.Enabled = _audioProfileToUse != null;
            if (_audioProfileToUse != null)
            {
                txt_audio_profile_settings.Text = _audioProfileToUse.GenerateSettingsText();
            }
        }

        private void btn_create_audio_profile_Click(object sender, EventArgs e)
        {
            string profileName = PromptForAudioProfileName("Create Audio Profile");
            if (string.IsNullOrWhiteSpace(profileName))
                return;

            if (!AudioProfileItem.IsValidName(profileName))
            {
                MessageBox.Show(this, "That Audio Profile name already exists. Please choose a unique name.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AudioProfileItem newAudioProfile = new AudioProfileItem
            {
                Name = profileName
            };

            if (AudioProfileRepository.AddAudioProfile(newAudioProfile))
            {
                RefreshAudioProfilesList();
                lb_audio_profiles.SelectedItem = newAudioProfile;
                _audioProfileToUse = newAudioProfile;
                btn_update_audio_profile.Enabled = true;
                btn_delete_audio_profile.Enabled = true;
                cb_dont_change_audio.Checked = false;

                if (_loadedShortcut)
                    _isUnsaved = true;
            }
            else
            {
                MessageBox.Show(this, "Unable to create the Audio Profile.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_update_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileItem selected = lb_audio_profiles.SelectedItem as AudioProfileItem;
            if (selected == null)
                return;

            if (selected.CreateProfileFromCurrentAudioSettings())
            {
                AudioProfileRepository.SaveAudioProfiles();
                RefreshAudioProfilesList();
                lb_audio_profiles.SelectedItem = selected;
                _audioProfileToUse = selected;
                if (_loadedShortcut)
                    _isUnsaved = true;
            }
            else
            {
                MessageBox.Show(this, "Unable to update the Audio Profile from current settings.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_delete_audio_profile_Click(object sender, EventArgs e)
        {
            AudioProfileItem selected = lb_audio_profiles.SelectedItem as AudioProfileItem;
            if (selected == null)
                return;

            DialogResult result = MessageBox.Show(this,
                $"Delete the Audio Profile '{selected.Name}'?",
                "Delete Audio Profile",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            if (AudioProfileRepository.RemoveAudioProfile(selected))
            {
                if (_audioProfileToUse != null && _audioProfileToUse.UUID.Equals(selected.UUID, StringComparison.OrdinalIgnoreCase))
                {
                    _audioProfileToUse = null;
                }

                RefreshAudioProfilesList();
                btn_update_audio_profile.Enabled = false;
                btn_delete_audio_profile.Enabled = false;
                if (_loadedShortcut)
                    _isUnsaved = true;
            }
            else
            {
                MessageBox.Show(this, "Unable to delete the selected Audio Profile.", "Audio Profile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

            // Fall back to the existing text or Program Files if the game directory is not found
            if (string.IsNullOrWhiteSpace(gamePath))
            {
                if (!string.IsNullOrWhiteSpace(txt_alternative_game.Text) && File.Exists(txt_alternative_game.Text))
                    gamePath = Path.GetDirectoryName(txt_alternative_game.Text);
                else
                    gamePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            }

            dialog_open.InitialDirectory = gamePath;
            dialog_open.DefaultExt = "*.exe";
            dialog_open.Filter = "Executable files (*.exe;*.com;*.msi;*.bat;*.cmd;*.ps1;*.lnk;*.url) | *.exe;*.com;*.msi;*.bat;*.cmd;*.ps1;*.lnk;*.url | All files(*.*) | *.*";
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                if (File.Exists(dialog_open.FileName) && ProcessUtils.IsExecutableFileType(dialog_open.FileName))
                {
                    txt_alternative_game.Text = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        "Selected file is not a valid file.",
                        "Executable",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }

        public void RedrawStartPrograms()
        {
            RedrawProgramsList();
        }

        public void RedrawProgramsList()
        {
            bool firstItem = true;
            Padding firstMargin = new Padding(10) { };
            Padding otherMargin = new Padding(10, 0, 10, 10) { };
            foreach (Control ctrl in flp_start_programs.Controls)
            {
                if (ctrl is IProgramControl programControl)
                {
                    if (firstItem)
                    {
                        ctrl.Margin = firstMargin;
                        firstItem = false;
                    }
                    else
                    {
                        ctrl.Margin = otherMargin;
                    }
                    int priority = flp_start_programs.Controls.GetChildIndex(ctrl) + 1;
                    programControl.ChangePriority(priority);
                }
            }
        }


        public void RemoveStartProgram(StartProgramControl startProgramControlToRemove)
        {
            if (_shortcutToEdit.StartPrograms != null && _shortcutToEdit.StartPrograms.Count > 0)
                _shortcutToEdit.StartPrograms.Remove(startProgramControlToRemove.StartProgram);

            flp_start_programs.SuspendLayout();
            flp_start_programs.Controls.Remove(startProgramControlToRemove);
            RedrawProgramsList();
            flp_start_programs.ResumeLayout();
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        public void RemoveStopProgram(StopProgramControl stopProgramControlToRemove)
        {
            if (_shortcutToEdit.StopPrograms != null && _shortcutToEdit.StopPrograms.Count > 0)
                _shortcutToEdit.StopPrograms.Remove(stopProgramControlToRemove.StopProgram);

            flp_start_programs.SuspendLayout();
            flp_start_programs.Controls.Remove(stopProgramControlToRemove);
            RedrawProgramsList();
            flp_start_programs.ResumeLayout();
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        public void ProgramEarlier(IProgramControl programControlToMove)
        {
            Control ctrl = programControlToMove as Control;
            if (ctrl == null) return;
            flp_start_programs.SuspendLayout();
            int currentIndex = flp_start_programs.Controls.GetChildIndex(ctrl);
            int newIndex = Math.Max(0, currentIndex - 1);
            flp_start_programs.Controls.SetChildIndex(ctrl, newIndex);
            RedrawProgramsList();
            flp_start_programs.ResumeLayout();
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        public void ProgramLater(IProgramControl programControlToMove)
        {
            Control ctrl = programControlToMove as Control;
            if (ctrl == null) return;
            flp_start_programs.SuspendLayout();
            int currentIndex = flp_start_programs.Controls.GetChildIndex(ctrl);
            int newIndex = Math.Min(flp_start_programs.Controls.Count - 1, currentIndex + 1);
            flp_start_programs.Controls.SetChildIndex(ctrl, newIndex);
            RedrawProgramsList();
            flp_start_programs.ResumeLayout();
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        public void StartProgramEarlier(StartProgramControl startProgramControlToMove)
        {
            ProgramEarlier(startProgramControlToMove);
        }

        public void StartProgramLater(StartProgramControl startProgramControlToMove)
        {
            ProgramLater(startProgramControlToMove);
        }

        private void btn_hotkey_Click(object sender, EventArgs e)
        {
            // Find the matching hotkeys so that we can load them in
            // and then show the hotkey form
            /*List<HotkeyKeyboard> _keyboardHotkeys = new List<HotkeyKeyboard>();
            _keyboardHotkeys.AddRange(Program.AppDirectInputManager.GetKeyboardHotkeysByUUID(_shortcutToEdit.UUID));
            List<HotkeyJoystick> _joystickHotkeys = new List<HotkeyJoystick>();
            _joystickHotkeys.AddRange(Program.AppDirectInputManager.GetJoystickHotkeysByUUID(_shortcutToEdit.UUID));*/

            string hotkeyHeading = $"Manage your '{_shortcutToEdit.Name}' Game Shortcut Hotkeys";
            string hotkeyDescription = $"Choose one or more Hotkeys so that you can run this Game Shortcut using your keyboard, joystick or button box. " +
                "This must be a Hotkey that is unique across all your applications otherwise DisplayMagician might not see it. " +
                "Click Add to add it to the list or click the trashcan to remove it from the list. To see all your hotkeys " +
                "go to the Main Window and click the Settings button. ";
            HotkeyForm displayHotkeyForm = new HotkeyForm(HotkeyTask.RunGameShortcut, _shortcutToEdit.UUID, hotkeyHeading, hotkeyDescription);
            //Program.HotkeyListener.SuspendOn(displayHotkeyForm);
            displayHotkeyForm.ShowDialog(this);
            if (displayHotkeyForm.Changed)
            {
                UpdateHotkeyText();
            }
        }

        private void UpdateHotkeyText()
        {

            try
            {
                _shownKeyboardHotkeys = Program.AppProgramSettings.KeyboardHotkeys.Where(k => k.Task == HotkeyTask.RunGameShortcut && k.UUID == _shortcutToEdit.UUID).ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"DisplayProfileForm/UpdateHotkeyText: Exception attempting to get the keyboard hotkeys from the settings file that match this taskmode RunGameShortcut and UUID {_shortcutToEdit.UUID}.");
            }
            try
            {
                _shownJoystickHotkeys = Program.AppProgramSettings.JoystickHotkeys.Where(k => k.Task == HotkeyTask.RunGameShortcut && k.UUID == _shortcutToEdit.UUID).ToList();
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"DisplayProfileForm/UpdateHotkeyText: Exception attempting to get the joystick hotkeys from the settings file that match this taskmode RunGameShortcut and UUID {_shortcutToEdit.UUID}.");
            }

            // We want the keyboard hotkeys to win if both are provided. Joystick and keyboard hotkeys do not mix and cannot be used together.
            List<string> hotkeyList = new List<string>();
            if (_shownKeyboardHotkeys.Count > 0)
            {
                foreach (HotkeyKeyboard kb in _shownKeyboardHotkeys)
                {
                    hotkeyList.Add(Program.AppDirectInputManager.GetNameOfKeyboardHotkey(kb));
                }
            }
            else if (_shownJoystickHotkeys.Count > 0)
            {
                foreach (HotkeyJoystick kb in _shownJoystickHotkeys)
                {
                    hotkeyList.Add(Program.AppDirectInputManager.GetNameOfJoystickHotkey(kb));
                }
            }
            string hotkeyText = string.Join(", ", hotkeyList);
            if (hotkeyList.Count > 0)
            {
                if (lbl_hotkey_assigned.InvokeRequired)
                {
                    lbl_hotkey_assigned.Invoke(new Action(() =>
                    {
                        lbl_hotkey_assigned.Text = $"Hotkeys: {hotkeyText}";
                        lbl_hotkey_assigned.Visible = true;
                    }));
                }
                else
                {
                    lbl_hotkey_assigned.Text = $"Hotkeys: {hotkeyText}";
                    lbl_hotkey_assigned.Visible = true;
                }
            }
            else
            {
                if (lbl_hotkey_assigned.InvokeRequired)
                {
                    lbl_hotkey_assigned.Invoke(new Action(() =>
                    {
                        lbl_hotkey_assigned.Text = "Hotkeys: None";
                        lbl_hotkey_assigned.Visible = false;
                    }));
                }
                else
                {
                    lbl_hotkey_assigned.Text = "Hotkeys: None";
                    lbl_hotkey_assigned.Visible = false;
                }

            }

        }


        private void lbl_hotkey_assigned_Click(object sender, EventArgs e)
        {
            btn_hotkey.PerformClick();
        }


        private void StartProgramControl_MouseDown(object sender, MouseEventArgs e)
        {
            DoDragDrop(sender, DragDropEffects.Move);
        }

        private void StartProgramControl_DragOver(object sender, DragEventArgs e)
        {
            Control senderCtrl = sender as Control;
            if (senderCtrl == null) return;

            if (e.Data.GetData(typeof(StartProgramControl)) is StartProgramControl draggedStart)
            {
                e.Effect = DragDropEffects.Move;
                FlowLayoutPanel p = senderCtrl.Parent as FlowLayoutPanel;
                if (p == null) return;
                int myIndex = p.Controls.GetChildIndex(senderCtrl);
                p.Controls.SetChildIndex(draggedStart, myIndex);
            }
            else if (e.Data.GetData(typeof(StopProgramControl)) is StopProgramControl draggedStop)
            {
                e.Effect = DragDropEffects.Move;
                FlowLayoutPanel p = senderCtrl.Parent as FlowLayoutPanel;
                if (p == null) return;
                int myIndex = p.Controls.GetChildIndex(senderCtrl);
                p.Controls.SetChildIndex(draggedStop, myIndex);
            }
        }

        private void StartProgramControl_DragDrop(object sender, DragEventArgs e)
        {
            Control senderCtrl = sender as Control;
            FlowLayoutPanel p = senderCtrl?.Parent as FlowLayoutPanel;
            if (p == null) return;
            int myIndex = p.Controls.GetChildIndex(senderCtrl);

            if (e.Data.GetData(typeof(StartProgramControl)) is StartProgramControl spc)
            {
                p.Controls.SetChildIndex(spc, myIndex);
                StartProgram startProgram = spc.StartProgram;
                startProgram.Priority = myIndex + 1;
                spc.StartProgram = startProgram;
            }
            else if (e.Data.GetData(typeof(StopProgramControl)) is StopProgramControl stpc)
            {
                p.Controls.SetChildIndex(stpc, myIndex);
                StopProgram stopProgram = stpc.StopProgram;
                stopProgram.Priority = myIndex + 1;
                stpc.StopProgram = stopProgram;
            }
            else
            {
                return;
            }

            flp_start_programs.SuspendLayout();
            RedrawProgramsList();
            flp_start_programs.ResumeLayout();
            flp_start_programs.Invalidate();

            if (_loadedShortcut)
                _isUnsaved = true;
        }

        private void btn_add_new_start_program_Click(object sender, EventArgs e)
        {
            // Create a new startProgram with sensible defaults
            StartProgram newStartProgram = new StartProgram()
            {
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
            RedrawProgramsList();
            flp_start_programs.ResumeLayout();
            flp_start_programs.Invalidate();
            if (_loadedShortcut)
                _isUnsaved = true;
        }

        private void btn_add_new_stop_program_Click(object sender, EventArgs e)
        {
            StopProgram newStopProgram = new StopProgram()
            {
                Executable = "",
                RestartAfterwards = false,
                RestartProcessPriority = ProcessPriority.Normal,
            };
            StopProgramControl newStopProgramControl = new StopProgramControl(newStopProgram, flp_start_programs.Controls.Count);
            newStopProgramControl.Dock = DockStyle.None;
            newStopProgramControl.Width = flp_start_programs.Width - 40;
            newStopProgramControl.MouseDown += new MouseEventHandler(StartProgramControl_MouseDown);
            newStopProgramControl.DragOver += new DragEventHandler(StartProgramControl_DragOver);
            newStopProgramControl.DragDrop += new DragEventHandler(StartProgramControl_DragDrop);
            newStopProgramControl.AllowDrop = true;
            flp_start_programs.SuspendLayout();
            flp_start_programs.Controls.Add(newStopProgramControl);
            RedrawProgramsList();
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
                    _gameLauncher = game.GameLibraryType.ToString("G");
                    lbl_game_library.Text = $"Game Library: {_gameLauncher}";
                    _gameId = game.Id;
                    _availableImages = game.AvailableGameBitmaps ?? new List<ShortcutBitmap>();
                    _shortcutToEdit.AvailableImages = _availableImages;
                    _selectedImage = _availableImages.Count > 0
                        ? ImageUtils.GetMeLargestAvailableBitmap(_availableImages)
                        : ImageUtils.CreateShortcutBitmap(Properties.Resources.exe, "Default", game.ExePath);
                    if (_selectedImage.Image == null)
                    {
                        logger.Warn($"ShortcutForm/ilv_games_ItemClick: No image resolved for game '{game.Name}'; using default exe icon.");
                        _selectedImage = ImageUtils.CreateShortcutBitmap(Properties.Resources.exe, "Default", game.ExePath);
                    }
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
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutForm/ilv_games_ItemClick: Exception while attempting to suggest shortcut name.");
            }

        }


        private void btn_find_examples_startprograms_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/wiki/Start-Program-Examples";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
        }

        private void btn_find_examples_game_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/wiki/Main-Game-and-Application-Examples";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
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
            if (_loadedShortcut)
                _isUnsaved = true;
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
            if (_loadedShortcut)
                _isUnsaved = true;
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
            if (_loadedShortcut)
                _isUnsaved = true;
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
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
        }

        private void cb_override_speaker_volume_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox cb)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                bool overrideVolume = cb.Checked;
                nud_speaker_volume.Enabled = overrideVolume;
            }
        }

        private void cb_override_microphone_volume_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is CheckBox cb)
            {
                if (_loadedShortcut)
                    _isUnsaved = true;
                bool overrideVolume = cb.Checked;
                nud_microphone_volume.Enabled = overrideVolume;
            }
        }
    }

    // Class used to populate combo boxes
    class ComboItem
    {
        public ProcessPriority Value { get; set; }
        public string Text { get; set; }
    }
}