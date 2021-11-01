using DisplayMagician.GameLibraries;
using DisplayMagician.Resources;
using DisplayMagicianShared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.IconLib;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using IWshRuntimeLibrary;
using AudioSwitcher.AudioApi.CoreAudio;
using AudioSwitcher.AudioApi;
using TsudaKageyu;

namespace DisplayMagician
{
    public enum ShortcutPermanence : int
    {
        Permanent = 0,
        Temporary = 1,
    }

    public enum ShortcutCategory : int
    {
        Application = 0,
        Game = 1,
        NoGame = 2,
    }

    public enum ShortcutValidity : int
    {
        Valid = 0,
        Warning = 1,
        Error = 2,
    }

    public enum ProcessPriority : int
    {
        High = 2,
        AboveNormal = 1,
        Normal = 0,
        BelowNormal =-1,
        Idle = -24,
    }


    public struct StartProgram
    {
        public int Priority;
        public bool Disabled;
        public ProcessPriority ProcessPriority;
        public string Executable;
        public string Arguments;
        public bool ExecutableArgumentsRequired;
        public bool CloseOnFinish;
        public bool DontStartIfAlreadyRunning;
    }

    public struct Executable
    {
        public string DifferentExecutableToMonitor;
        public string ExecutableNameAndPath;
        public int ExecutableTimeout;
        public string ExecutableArguments;
        public bool ExecutableArgumentsRequired;
        public bool ProcessNameToMonitorUsesExecutable;
        public ProcessPriority ProcessPriority;
    }

    public struct GameStruct
    {
        public Game GameToPlay;
        public int StartTimeout;
        public string GameArguments;
        public bool GameArgumentsRequired;
        public string DifferentGameExeToMonitor;
        public bool MonitorDifferentGameExe;
        public ProcessPriority ProcessPriority;
    }

    public struct ShortcutError
    {
        public string Name;
        public ShortcutValidity Validity;
        public string Message;
    }

    public class ShortcutItem : IComparable
    {
        
        private string _profileUuid = "";
        private ProfileItem _profileToUse;
        private string _uuid = "";
        private string _name = "";
        private ShortcutCategory _category = ShortcutCategory.Game;
        private string _differentExecutableToMonitor;
        private string _executableNameAndPath = "";
        private string _executableArguments;
        private bool _executableArgumentsRequired = false;
        private bool _processNameToMonitorUsesExecutable = true;
        private ProcessPriority _processPriority = ProcessPriority.Normal;
        private string _gameAppId;
        private string _gameName;
        private SupportedGameLibraryType _gameLibrary = SupportedGameLibraryType.Unknown;
        private int _startTimeout = 20;
        private string _gameArguments;
        private bool _gameArgumentsRequired;
        private string _differentGameExeToMonitor;
        private bool _monitorDifferentGameExe = false;
        private string _audioDevice;
        private bool _changeAudioDevice;
        private bool _setAudioVolume = false;
        private decimal _audioVolume = -1;
        private string _captureDevice;
        private bool _changeCaptureDevice;
        private bool _setCaptureVolume = false;
        private decimal _captureVolume = -1;
        private ShortcutPermanence _displayPermanence = ShortcutPermanence.Temporary;
        private ShortcutPermanence _audioPermanence = ShortcutPermanence.Temporary;
        private ShortcutPermanence _capturePermanence = ShortcutPermanence.Temporary;
        private Keys _hotkey = Keys.None;
        private bool _autoName = true;
        private ShortcutValidity _isValid;
        private List<ShortcutError> _shortcutErrors = new List<ShortcutError>();
        private List<StartProgram> _startPrograms;
        private Bitmap _shortcutBitmap, _originalBitmap;
        [JsonIgnore]
#pragma warning disable CS3008 // Identifier is not CLS-compliant
        private string _originalIconPath;
        private bool _userChoseOwnIcon = false;
        private string _userIconPath;
        private Bitmap _userIconBitmap;
        [JsonIgnore]
        public string _savedShortcutIconCacheFilename;
#pragma warning restore CS3008 // Identifier is not CLS-compliant
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ShortcutItem()
        {
            // Create a new UUID for the shortcut if one wasn't created already
            if (String.IsNullOrWhiteSpace(_uuid))
                _uuid = Guid.NewGuid().ToString("D");

            // If there are no GameLibraries then choose executable instead
            if (!(UplayLibrary.GetLibrary().IsGameLibraryInstalled && 
                SteamLibrary.GetLibrary().IsGameLibraryInstalled &&
                GogLibrary.GetLibrary().IsGameLibraryInstalled &&
                EpicLibrary.GetLibrary().IsGameLibraryInstalled &&
                OriginLibrary.GetLibrary().IsGameLibraryInstalled))
            {
                _gameLibrary = SupportedGameLibraryType.Unknown;
                _gameName = "";
                _gameArguments = "";
                _category = ShortcutCategory.Application;
            }
            // Autocreate a name for the shortcut if AutoName is on
            // (and if we have a profile to use)
            if (AutoName && _profileToUse is ProfileItem)
            {
                // If Autoname is on, and then lets autoname it!
                // That populates all the right things
                AutoSuggestShortcutName();
            }

            //RefreshValidity();

        }

        public static Version Version
        {
            get => new Version(1, 0);
        }


        public string UUID
        {
            get
            {
                return _uuid;
            }
            set
            {
                string uuidV4Regex = @"[0-9A-F]{8}-[0-9A-F]{4}-4[0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}";
                Match match = Regex.Match(value, uuidV4Regex, RegexOptions.IgnoreCase);
                if (match.Success)
                    _uuid = value;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
            }
        }

        public bool AutoName
        {
            get
            {
                return _autoName;
            }
            set
            {
                _autoName = value;
            }
        }   


        [JsonIgnore]
#pragma warning disable CS3003 // Type is not CLS-compliant
        public ProfileItem ProfileToUse {
#pragma warning restore CS3003 // Type is not CLS-compliant
            get
            {
                return _profileToUse;
            }
            set
            {
                if (value is ProfileItem)
                {
                    _profileToUse = value;
                    _profileUuid = _profileToUse.UUID;
                    // We should try to set the Profile
                    // And we rename the shortcut if the AutoName is on
                    if (AutoName)
                        AutoSuggestShortcutName();
                }
            }
        }

        public string ProfileUUID { 
            get 
            {
                return _profileUuid;
            }
            set
            {
                _profileUuid = value;

                // We try to find and set the ProfileTouse
                foreach (ProfileItem profileToTest in ProfileRepository.AllProfiles)
                {
                    if (profileToTest.UUID.Equals(_profileUuid, StringComparison.OrdinalIgnoreCase))
                        _profileToUse = profileToTest;
                }
            }
        }

        public ShortcutPermanence DisplayPermanence 
        { 
            get 
            {
                return _displayPermanence;
            }

            set 
            {
                _displayPermanence = value;
            }
        }

        public ShortcutPermanence AudioPermanence
        {
            get
            {
                return _audioPermanence;
            }

            set
            {
                _audioPermanence = value;
            }
        }

        public ShortcutPermanence CapturePermanence
        {
            get
            {
                return _capturePermanence;
            }

            set
            {
                _capturePermanence = value;
            }
        }

        public ShortcutCategory Category
        {
            get
            {
                return _category;
            }

            set
            {
                _category = value;
            }
        }

        public ProcessPriority ProcessPriority
        {
            get
            {
                return _processPriority;
            }

            set
            {
                _processPriority = value;
            }
        }

        public string DifferentExecutableToMonitor
        {
            get
            {
                return _differentExecutableToMonitor;
            }

            set
            {
                _differentExecutableToMonitor = value;
            }
        }

        public string ExecutableNameAndPath
        {
            get
            {
                return _executableNameAndPath;
            }

            set
            {
                _executableNameAndPath = value;

                // If the executableNameandPath is set then we also want to update the originalIconPath
                // so it's the path to the application. This will kick of the icon grabbing processes
                if (Category.Equals(ShortcutCategory.Application))
                    _originalIconPath = value;

            }
        }

        public string ExecutableArguments
        {
            get
            {
                return _executableArguments;
            }

            set
            {
                _executableArguments = value;
            }
        }

        public bool ExecutableArgumentsRequired
        {
            get
            {
                return _executableArgumentsRequired;
            }

            set
            {
                _executableArgumentsRequired = value;
            }
        }

        public bool ProcessNameToMonitorUsesExecutable
        {
            get
            {
                return _processNameToMonitorUsesExecutable;
            }

            set
            {
                _processNameToMonitorUsesExecutable = value;
            }
        }

        public string GameAppId
        {
            get
            {
                return _gameAppId;
            }

            set
            {
                _gameAppId = value;
            }
        }

        public string GameName
        {
            get
            {
                return _gameName;
            }

            set
            {
                _gameName = value;
            }
        }

        public SupportedGameLibraryType GameLibrary
        {
            get
            {
                return _gameLibrary;
            }

            set
            {
                _gameLibrary = value;
            }
        }

#pragma warning disable CS3003 // Type is not CLS-compliant
        public Keys Hotkey
#pragma warning restore CS3003 // Type is not CLS-compliant
        {
            get
            {
                return _hotkey;
            }
            set
            {
                _hotkey = value;
            }
        }

        public int StartTimeout
        {
            get
            {
                return _startTimeout;
            }

            set
            {
                _startTimeout = value;
            }
        }

        public string GameArguments
        {
            get
            {
                return _gameArguments;
            }

            set
            {
                _gameArguments = value;
            }
        }

        public bool GameArgumentsRequired
        {
            get
            {
                return _gameArgumentsRequired;
            }

            set
            {
                _gameArgumentsRequired = value;
            }
        }

        public string DifferentGameExeToMonitor
        {
            get
            {
                return _differentGameExeToMonitor;
            }

            set
            {
                _differentGameExeToMonitor = value;
            }
        }

        public bool MonitorDifferentGameExe
        {
            get
            {
                return _monitorDifferentGameExe;
            }

            set
            {
                _monitorDifferentGameExe = value;
            }
        }


        public string AudioDevice
        {
            get
            {
                return _audioDevice;
            }

            set
            {
                _audioDevice = value;
            }
        }

        public bool ChangeAudioDevice
        {
            get
            {
                return _changeAudioDevice;
            }

            set
            {
                _changeAudioDevice = value;
            }
        }

        public decimal AudioVolume
        {
            get
            {
                return _audioVolume;
            }

            set
            {
                _audioVolume = value;
            }
        }

        public bool SetAudioVolume
        {
            get
            {
                return _setAudioVolume;
            }

            set
            {
                _setAudioVolume = value;
            }
        }

        public string CaptureDevice
        {
            get
            {
                return _captureDevice;
            }

            set
            {
                _captureDevice = value;
            }
        }

        public bool ChangeCaptureDevice
        {
            get
            {
                return _changeCaptureDevice;
            }

            set
            {
                _changeCaptureDevice = value;
            }
        }

        public decimal CaptureVolume
        {
            get
            {
                return _captureVolume;
            }

            set
            {
                _captureVolume = value;
            }
        }

        public bool SetCaptureVolume
        {
            get
            {
                return _setCaptureVolume;
            }

            set
            {
                _setCaptureVolume = value;
            }
        }

        public List<StartProgram> StartPrograms
        {
            get
            {
                return _startPrograms;
            }

            set
            {
                _startPrograms = value;
            }
        }

        public string OriginalIconPath {
            get
            {
                return _originalIconPath;
            }

            set
            {
                _originalIconPath = value;

                // And we do the same for the OriginalBitmap 
                //_originalLargeBitmap = ToLargeBitmap(_originalIconPath);                
            }
        }

        [JsonConverter(typeof(CustomBitmapConverter))]
        public Bitmap OriginalLargeBitmap
        {
            get
            {
                return _originalBitmap;
            }

            set
            {
                _originalBitmap = value;

                // And we do the same for the Bitmap overlay, but only if the ProfileToUse is set
                //if (_profileToUse is ProfileItem)
                //    _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);

            }
        }

        [JsonConverter(typeof(CustomBitmapConverter))]
        public Bitmap ShortcutBitmap
        {
            get
            {
                return _shortcutBitmap;
            }

            set
            {
                _shortcutBitmap = value;
            }
        }

        public string SavedShortcutIconCacheFilename
        {
            get
            {
                return _savedShortcutIconCacheFilename;
            }
            set
            {
                _savedShortcutIconCacheFilename = value;
            }
        }


        [JsonIgnore]
        public ShortcutValidity IsValid
        {
            get
            {
                return _isValid;
            }
            set
            {
                _isValid = value;
            }
        }

        [JsonIgnore]
        public List<ShortcutError> Errors
        {
            get
            {
                return _shortcutErrors;
            }
            set
            {
                _shortcutErrors = value;
            }
        }

        public bool UserChoseOwnIcon
        {
            get
            {
                return _userChoseOwnIcon;
            }

            set
            {
                _userChoseOwnIcon = value;
            }
        }

        public string UserIconPath
        {
            get
            {
                return _userIconPath;
            }

            set
            {
                _userIconPath = value;

                // And we do the same for the UserLargeBitmap 
                //_userIconBitmap = ToLargeBitmap(_userIconPath);                
            }
        }

        [JsonConverter(typeof(CustomBitmapConverter))]
        public Bitmap UserLargeBitmap
        {
            get
            {
                return _userIconBitmap;
            }

            set
            {
                _userIconBitmap = value;

            }
        }


        public void UpdateNoGameShortcut(
            string name,
#pragma warning disable CS3001 // Argument type is not CLS-compliant
            ProfileItem profile,
#pragma warning restore CS3001 // Argument type is not CLS-compliant
            ShortcutPermanence displayPermanence,
            ShortcutPermanence audioPermanence,
            ShortcutPermanence capturePermanence,
            string originalIconPath,
            bool changeAudioDevice = false,
            string audioDevice = "",
            bool setAudioVolume = false,
            decimal audioVolume = -1,
            bool changeCaptureDevice = false,
            string captureDevice = "",
            bool setCaptureVolume = false,
            decimal captureVolume = -1,
            List<StartProgram> startPrograms = null,
            bool autoName = true,
            Keys hotkey = Keys.None,
            string uuid = ""
            )
        {
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name;
            _category = ShortcutCategory.NoGame;
            _profileToUse = profile;
            _changeAudioDevice = changeAudioDevice;
            _audioDevice = audioDevice;
            _setAudioVolume = setAudioVolume;
            _audioVolume = audioVolume;
            _changeCaptureDevice = changeCaptureDevice;
            _captureDevice = captureDevice;
            _setCaptureVolume = setCaptureVolume;
            _captureVolume = captureVolume;
            _displayPermanence = displayPermanence;
            _audioPermanence = audioPermanence;
            _capturePermanence = capturePermanence;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _originalIconPath = originalIconPath;
            _hotkey = hotkey;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;
            _originalBitmap = profile.ProfileBitmap;
            _shortcutBitmap = profile.ProfileBitmap;

            ReplaceShortcutIconInCache();
            RefreshValidity();
        }

        public void UpdateGameShortcut(
            string name, 
#pragma warning disable CS3001 // Argument type is not CLS-compliant
            ProfileItem profile, 
#pragma warning restore CS3001 // Argument type is not CLS-compliant
            GameStruct game, 
            ShortcutPermanence displayPermanence,
            ShortcutPermanence audioPermanence, 
            ShortcutPermanence capturePermanence,
            string originalIconPath,
            bool userChoseOwnIcon = false,
            string userIconPath = "",
            bool changeAudioDevice = false,
            string audioDevice = "",
            bool setAudioVolume = false,
            decimal audioVolume = -1,
            bool changeCaptureDevice = false,
            string captureDevice = "",
            bool setCaptureVolume = false,
            decimal captureVolume = -1,
            List<StartProgram> startPrograms = null, 
            bool autoName = true, 
            string uuid = "",
            Keys hotkey = Keys.None
            )
        {
            // Create a new UUID for the shortcut if one wasn't created already
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name;
            _profileToUse = profile;
            _category = ShortcutCategory.Game;
            _gameAppId = game.GameToPlay.Id;
            _gameName = game.GameToPlay.Name;
            _gameLibrary = game.GameToPlay.GameLibrary;
            _startTimeout = game.StartTimeout;
            _gameArguments = game.GameArguments;
            _gameArgumentsRequired = game.GameArgumentsRequired;
            _differentGameExeToMonitor = game.DifferentGameExeToMonitor;
            _monitorDifferentGameExe = game.MonitorDifferentGameExe;
            _processPriority = game.ProcessPriority;
            _changeAudioDevice = changeAudioDevice;
            _audioDevice = audioDevice;
            _setAudioVolume = setAudioVolume;
            _audioVolume = audioVolume;
            _changeCaptureDevice = changeCaptureDevice;
            _captureDevice = captureDevice;
            _setCaptureVolume = setCaptureVolume;
            _captureVolume = captureVolume;
            _displayPermanence = displayPermanence;
            _audioPermanence = audioPermanence;
            _capturePermanence = capturePermanence;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _originalIconPath = originalIconPath;
            _userChoseOwnIcon = userChoseOwnIcon;
            _userIconPath = userIconPath;
            _hotkey = hotkey;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;

            // We create the Bitmaps for the game
            SetBitmapsForGame();

            ReplaceShortcutIconInCache();
            RefreshValidity();
        }

        public void UpdateExecutableShortcut(
            string name, 
#pragma warning disable CS3001 // Argument type is not CLS-compliant
            ProfileItem profile, 
#pragma warning restore CS3001 // Argument type is not CLS-compliant
            Executable executable, 
            ShortcutPermanence displayPermanence,
            ShortcutPermanence audioPermanence, 
            ShortcutPermanence capturePermanence,
            string originalIconPath,
            bool userChoseOwnIcon = false,
            string userIconPath = "",
            bool changeAudioDevice = false,
            string audioDevice = "",
            bool setAudioVolume = false,
            decimal audioVolume = -1,
            bool changeCaptureDevice = false,
            string captureDevice = "",
            bool setCaptureVolume = false,
            decimal captureVolume = -1,
            List<StartProgram> startPrograms = null, 
            bool autoName = true,
            Keys hotkey = Keys.None,
            string uuid = ""
            )
        {
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name;
            _profileToUse = profile;
            _category = ShortcutCategory.Application;
            _differentExecutableToMonitor = executable.DifferentExecutableToMonitor;
            _executableNameAndPath = executable.ExecutableNameAndPath;
            _startTimeout = executable.ExecutableTimeout;
            _executableArguments = executable.ExecutableArguments;
            _executableArgumentsRequired = executable.ExecutableArgumentsRequired;
            _processNameToMonitorUsesExecutable = executable.ProcessNameToMonitorUsesExecutable;
            _processPriority = executable.ProcessPriority;
            _changeAudioDevice = changeAudioDevice;
            _audioDevice = audioDevice;
            _setAudioVolume = setAudioVolume;
            _audioVolume = audioVolume;
            _changeCaptureDevice = changeCaptureDevice;
            _captureDevice = captureDevice;
            _setCaptureVolume = setCaptureVolume;
            _captureVolume = captureVolume;
            _displayPermanence = displayPermanence;
            _audioPermanence = audioPermanence;
            _capturePermanence = capturePermanence;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _originalIconPath = originalIconPath;
            _userChoseOwnIcon = userChoseOwnIcon;
            _userIconPath = userIconPath;
            _hotkey = hotkey;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;

            // We create the Bitmaps for the executable
            SetBitmapsForExecutable();

            ReplaceShortcutIconInCache();
            RefreshValidity();
        }

        public bool CopyTo (ShortcutItem shortcut, bool overwriteUUID = true)
        {
            if (!(shortcut is ShortcutItem))
                return false;

            if (overwriteUUID)
                shortcut.UUID = UUID;

            // Copy all the shortcut data over to the other Shortcut
            shortcut.Name = Name;
            shortcut.ProfileToUse = ProfileToUse;
            shortcut.ProfileUUID = ProfileUUID;
            shortcut.DisplayPermanence = DisplayPermanence;
            shortcut.AudioPermanence = AudioPermanence;
            shortcut.CapturePermanence = CapturePermanence;
            shortcut.Category = Category;
            shortcut.DifferentExecutableToMonitor = DifferentExecutableToMonitor;
            shortcut.ExecutableNameAndPath = ExecutableNameAndPath;
            shortcut.ExecutableArguments = ExecutableArguments;
            shortcut.ExecutableArgumentsRequired = ExecutableArgumentsRequired;
            shortcut.ProcessNameToMonitorUsesExecutable = ProcessNameToMonitorUsesExecutable;
            shortcut.ProcessPriority = ProcessPriority;
            shortcut.GameAppId = GameAppId;
            shortcut.GameName = GameName;
            shortcut.GameLibrary = GameLibrary;
            shortcut.StartTimeout = StartTimeout;
            shortcut.GameArguments = GameArguments;
            shortcut.GameArgumentsRequired = GameArgumentsRequired;
            shortcut.OriginalIconPath = OriginalIconPath;
            shortcut.OriginalLargeBitmap = OriginalLargeBitmap;
            shortcut.ShortcutBitmap = ShortcutBitmap;
            shortcut.SavedShortcutIconCacheFilename = SavedShortcutIconCacheFilename;
            shortcut.UserChoseOwnIcon = UserChoseOwnIcon;
            shortcut.UserIconPath = UserIconPath;
            shortcut.UserLargeBitmap = UserLargeBitmap;
            shortcut.IsValid = IsValid;
            shortcut.Errors.AddRange(Errors);
            shortcut.StartPrograms = StartPrograms;
            shortcut.ChangeAudioDevice = ChangeAudioDevice;
            shortcut.AudioDevice = AudioDevice;
            shortcut.SetAudioVolume = SetAudioVolume;
            shortcut.AudioVolume = AudioVolume;
            shortcut.ChangeCaptureDevice = ChangeCaptureDevice;
            shortcut.CaptureDevice = CaptureDevice;
            shortcut.SetCaptureVolume = SetCaptureVolume;
            shortcut.CaptureVolume = CaptureVolume;
            shortcut.Hotkey = Hotkey;

            // Save the shortcut incon to the icon cache
            shortcut.ReplaceShortcutIconInCache();
            shortcut.RefreshValidity();

            return true;
        }

        public void ReplaceShortcutIconInCache()
        {
            // Figure out if we need to remove the old file
            if (_savedShortcutIconCacheFilename != null)
            {
                // Work out the name of the shortcut we'll save.
                string oldShortcutIconFilename = _savedShortcutIconCacheFilename;
                logger.Trace($"ShortcutItem/ReplaceShortcutIconInCache: Old shortcut Icon filename is {oldShortcutIconFilename}.");
                if (System.IO.File.Exists(oldShortcutIconFilename))
                {
                    logger.Trace($"ShortcutItem/ReplaceShortcutIconInCache: Deleting old shortcut Icon filename {oldShortcutIconFilename}.");
                    System.IO.File.Delete(oldShortcutIconFilename);
                }

            }
            // Now we save the new file
            SaveShortcutIconToCache();
        }


        public void SaveShortcutIconToCache()
        {

            // Work out the name of the shortcut we'll save.
            _savedShortcutIconCacheFilename = Path.Combine(Program.AppShortcutPath, $"{UUID}.ico");
            logger.Trace($"ShortcutItem/SaveShortcutIconToCache: Planning on saving shortcut icon to cache as {_savedShortcutIconCacheFilename}.");
            MultiIcon shortcutIcon = new MultiIcon(); 
            try
            {
                logger.Trace($"ShortcutItem/SaveShortcutIconToCache: Creating Icon from Shortcut bitmap.");
                // Create a new 
                SingleIcon si = shortcutIcon.Add("icon");
                si.Add(_shortcutBitmap);
                shortcutIcon.SelectedIndex = 0;
                logger.Trace($"ShortcutItem/SaveShortcutIconToCache: Saving shortcut icon to cache with {_savedShortcutIconCacheFilename} as the name.");
                shortcutIcon.Save(_savedShortcutIconCacheFilename, MultiIconFormat.ICO);

            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutItem/SaveShortcutIconToCache: Exception while trying to save the Shortcut icon.");
                shortcutIcon.Clear();
                // If we fail to create an icon any other way, then we use the default profile icon
                logger.Trace($"ShortcutItem/SaveShortcutIconToCache: Using the Display Profile icon for {_profileToUse.Name} as the icon instead.");
                SingleIcon si = shortcutIcon.Add("icon2");
                si.Add(Properties.Resources.DisplayMagician);
                shortcutIcon.SelectedIndex = 0; 
                logger.Trace($"ShortcutItem/SaveShortcutIconToCache: Saving the Display Profile icon for {_profileToUse.Name} to {_savedShortcutIconCacheFilename}.");
                shortcutIcon.Save(_savedShortcutIconCacheFilename, MultiIconFormat.ICO);
            }

        }

        public void SetBitmapsForGame()
        {
            // Get the user icon bitmap if its set.
            if (_userChoseOwnIcon)
            {
                logger.Trace($"ShortcutItem/ToBitmapOverlay: Using the user set icon as the game icon instead (from {_userIconPath}).");
                _userIconBitmap = ImageUtils.GetMeABitmapFromFile(_userIconPath);
            }

            // Get the game icon bitmap if we can find it.
            logger.Trace($"ShortcutItem/ToBitmapOverlay: Using the game executable icon as the game icon instead from {_originalIconPath}.");
            // Find the game bitmap that matches the game name we just got
            foreach (var aGame in GameLibraries.GameLibrary.AllInstalledGamesInAllLibraries)
            {
                if (aGame.Name.Equals(_gameName))
                {
                    _originalBitmap = aGame.GameBitmap;
                }
            }
            // If we can't find the game icon bitmap then we try the icons for the game libraries themselves
            if (_originalBitmap == null)
            {
                if (_gameLibrary == SupportedGameLibraryType.Steam)
                {
                    logger.Trace($"ShortcutItem/GetOriginalBitmapFromGame: Using the Steam icon as the icon instead.");
                    _originalBitmap = Properties.Resources.Steam;
                }
                else if (_gameLibrary == SupportedGameLibraryType.Uplay)
                {
                    logger.Trace($"ShortcutItem/GetOriginalBitmapFromGame: Using the Uplay icon as the icon instead.");
                    _originalBitmap = Properties.Resources.Uplay;
                }
                else if (_gameLibrary == SupportedGameLibraryType.Origin)
                {
                    logger.Trace($"ShortcutItem/GetOriginalBitmapFromGame: Using the Origin icon as the icon instead.");
                    _originalBitmap = Properties.Resources.Origin;
                }
                else if (_gameLibrary == SupportedGameLibraryType.Epic)
                {
                    logger.Trace($"ShortcutItem/GetOriginalBitmapFromGame: Using the Epic icon as the icon instead.");
                    _originalBitmap = Properties.Resources.Epic;
                }
                else if (_gameLibrary == SupportedGameLibraryType.GOG)
                {
                    logger.Trace($"ShortcutItem/GetOriginalBitmapFromGame: Using the GOG icon as the icon instead.");
                    _originalBitmap = Properties.Resources.GOG;
                }
                else
                {
                    logger.Trace($"ShortcutItem/GetOriginalBitmapFromGame: Unknown Game Library, so using the DisplayMagician icon as the icon instead.");
                    _originalBitmap = Properties.Resources.DisplayMagician.ToBitmap();
                }
            }

            // Now we use the originalBitmap or userBitmap, and create the shortcutBitmap from it
            if (_userChoseOwnIcon)
            {
                _shortcutBitmap = ImageUtils.ToBitmapOverlay(_userIconBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
            }
            else
            {
                _shortcutBitmap = ImageUtils.ToBitmapOverlay(_originalBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
            }

        }

        public void SetBitmapsForExecutable()
        {
            
            if (_userChoseOwnIcon)
            {
                logger.Trace($"ShortcutItem/SetBitmapsForExecutable: Using the user set icon as the app icon instead (from {_userIconPath}).");
                _userIconBitmap = ImageUtils.GetMeABitmapFromFile(_userIconPath);
            }

            logger.Trace($"ShortcutItem/SetBitmapsForExecutable: Using the executable icon as the app icon instead from {_executableNameAndPath}.");
            _originalBitmap = ImageUtils.GetMeABitmapFromFile(_executableNameAndPath);

            if (_originalBitmap == null)
            {
                logger.Trace($"ShortcutItem/SetBitmapsForExecutable: Unknown Game Library, so using the DisplayMagician icon as the icon instead.");
                _originalBitmap = ImageUtils.ToBitmapOverlay(Properties.Resources.DisplayMagician.ToBitmap(), _profileToUse.ProfileIcon.ToBitmap(), 256, 256);
            }

            // Now we use the originalBitmap or userBitmap, and create the shortcutBitmap from it
            if (_userChoseOwnIcon)
            {
                logger.Trace($"ShortcutItem/SetBitmapsForExecutable: Unknown Game Library, so using the DisplayMagician icon as the icon instead.");
                _shortcutBitmap = ImageUtils.ToBitmapOverlay(_userIconBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
            }
            else
            {
                _shortcutBitmap = ImageUtils.ToBitmapOverlay(_originalBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
            }

        }

        public void RefreshValidity()
        {
            // Do some validation checks to make sure the shortcut is sensible
            // And that we have enough to try and action within the shortcut
            // (in other words check everything in the shortcut is still valid)

            Errors.Clear();
            ShortcutValidity worstError = ShortcutValidity.Valid;

            // Does the profile we want to Use still exist?
            if (!ProfileRepository.ContainsProfile(ProfileUUID))
            {
                logger.Warn($"ShortcutItem/RefreshValidity: The profile UUID {ProfileUUID} isn't in the ProfileRepository");
                ShortcutError error = new ShortcutError();
                error.Name = "ProfileNotExist";
                error.Validity = ShortcutValidity.Error;
                error.Message = $"The profile does not exist (probably deleted) and cannot be used.";
                _shortcutErrors.Add(error);
                if (worstError != ShortcutValidity.Error)
                    worstError = ShortcutValidity.Error;
            }
            // Is the profile still valid right now? i.e. are all the screens available?
            if (ProfileToUse != null && !ProfileToUse.IsPossible)
            {
                logger.Warn($"ShortcutItem/RefreshValidity: The profile {ProfileToUse} isn't possible to use right now!");
                ShortcutError error = new ShortcutError();
                error.Name = "InvalidProfile";
                error.Validity = ShortcutValidity.Warning;
                error.Message = $"The profile '{ProfileToUse.Name}' is not valid right now and cannot be used.";
                _shortcutErrors.Add(error);
                if (worstError != ShortcutValidity.Error)
                    worstError = ShortcutValidity.Warning;
            }
            // Is the main application still installed?
            if (Category.Equals(ShortcutCategory.Application))
            {
                // We need to check if the Application still exists
                if (!System.IO.File.Exists(ExecutableNameAndPath))
                {
                    logger.Warn($"ShortcutItem/RefreshValidity: The Application executable {ExecutableNameAndPath} DOES NOT exist");
                    ShortcutError error = new ShortcutError();
                    error.Name = "InvalidExecutableNameAndPath";
                    error.Validity = ShortcutValidity.Error;
                    error.Message = $"The application executable '{ExecutableNameAndPath}' does not exist, or cannot be accessed by DisplayMagician.";
                    _shortcutErrors.Add(error);
                    if (worstError != ShortcutValidity.Error)
                        worstError = ShortcutValidity.Error;
                }
                else
                {
                    logger.Trace($"ShortcutItem/RefreshValidity: The Application executable {ExecutableNameAndPath} exists");
                }

            }
            else if (Category.Equals(ShortcutCategory.Game))
            {
                GameLibrary gameLibraryToUse = null;

                // If the game is a Steam Game we check for that
                if (GameLibrary.Equals(SupportedGameLibraryType.Steam))
                {
                    logger.Trace($"ShortcutItem/RefreshValidity: The game library is Steam");
                    // We now need to get the SteamGame info
                    gameLibraryToUse = SteamLibrary.GetLibrary();
                }
                // If the game is a Uplay Uplay Game we check for that
                else if (GameLibrary.Equals(SupportedGameLibraryType.Uplay))
                {
                    logger.Trace($"ShortcutItem/RefreshValidity: The game library is Uplay");
                    // We now need to get the Uplay Game  info
                    gameLibraryToUse = UplayLibrary.GetLibrary();
                }
                // If the game is an Origin Game we check for that
                else if (GameLibrary.Equals(SupportedGameLibraryType.Origin))
                {
                    logger.Trace($"ShortcutItem/RefreshValidity: The game library is Origin");
                    // We now need to get the Uplay Game  info
                    gameLibraryToUse = OriginLibrary.GetLibrary();
                }
                // If the game is an Epic Game we check for that
                else if (GameLibrary.Equals(SupportedGameLibraryType.Epic))
                {
                    logger.Trace($"ShortcutItem/RefreshValidity: The game library is Epic");
                    // We now need to get the Epic Game  info
                    gameLibraryToUse = EpicLibrary.GetLibrary();
                }
                // If the game is an GOG Game we check for that
                else if (GameLibrary.Equals(SupportedGameLibraryType.GOG))
                {
                    logger.Trace($"ShortcutItem/RefreshValidity: The game library is GOG");
                    // We now need to get the GOG Game  info
                    gameLibraryToUse = GogLibrary.GetLibrary();
                }
                else
                {
                    gameLibraryToUse = null;
                    logger.Warn($"ShortcutItem/RefreshValidity: The game shortcut uses an unsupported game library! (You've probably downgraded DisplayMagician to an earlier version)");
                    ShortcutError error = new ShortcutError();
                    error.Name = $"UnknownGameLibrary";
                    error.Validity = ShortcutValidity.Error;
                    error.Message = $"The game shortcut uses an unsupported game library.";
                    _shortcutErrors.Add(error);
                    if (worstError != ShortcutValidity.Error)
                        worstError = ShortcutValidity.Error;
                }

                if (gameLibraryToUse != null)
                {
                    // Check if Gamelibrary is installed and error if it isn't
                    if (!gameLibraryToUse.IsGameLibraryInstalled)
                    {
                        logger.Warn($"ShortcutItem/RefreshValidity: The game library is not installed!");
                        ShortcutError error = new ShortcutError();
                        error.Name = $"{gameLibraryToUse.GameLibraryName}NotInstalled";
                        error.Validity = ShortcutValidity.Error;
                        error.Message = $"{gameLibraryToUse.GameLibraryName} is not installed on this computer.";
                        _shortcutErrors.Add(error);
                        if (worstError != ShortcutValidity.Error)
                            worstError = ShortcutValidity.Error;
                    }

                    // We need to look up details about the game
                    if (!gameLibraryToUse.ContainsGameById(GameAppId))
                    {
                        logger.Warn($"ShortcutItem/RefreshValidity: The game library does not have Game ID {GameAppId} installed!");
                        ShortcutError error = new ShortcutError();
                        error.Name = "{gameLibraryToUse.GameLibraryName}GameNotInstalled";
                        error.Validity = ShortcutValidity.Error;
                        error.Message = $"The {gameLibraryToUse.GameLibraryName} Game with AppID '{GameAppId}' is not installed on this computer.";
                        _shortcutErrors.Add(error);
                        if (worstError != ShortcutValidity.Error)
                            worstError = ShortcutValidity.Error;
                    }
                }                
            }
            // Check the Audio Device is still valid (if one is specified)
            CoreAudioController audioController = ShortcutRepository.AudioController;
            if (ChangeAudioDevice)
            {
                IEnumerable<CoreAudioDevice> audioDevices = null;
                if (audioController != null)
                {
                    try
                    {
                        audioDevices = audioController.GetPlaybackDevices();
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"ShortcutRepository/RefreshValidity: Exception trying to get all playback devices!");
                    }
                    if (audioDevices != null)
                    {
                        foreach (CoreAudioDevice audioDevice in audioDevices)
                        {
                            logger.Trace($"ShortcutItem/RefreshValidity: Detected audio playback device {audioDevice.FullName}");
                            if (audioDevice.FullName.Equals(AudioDevice))
                            {
                                logger.Trace($"ShortcutItem/RefreshValidity: Detected audio playback device {audioDevice.FullName} is the one we want!");
                                if (audioDevice.State == DeviceState.Disabled)
                                {
                                    logger.Warn($"ShortcutRepository/RefreshValidity: Detected audio playback device {audioDevice.FullName} is the one we want, but it is disabled!");
                                    ShortcutError error = new ShortcutError();
                                    error.Name = "AudioDeviceDisabled";
                                    error.Validity = ShortcutValidity.Warning;
                                    error.Message = $"The Audio Device { AudioDevice} is disabled, so the shortcut '{Name}' cannot be used.You need to enable the audio device to use this shortcut, or edit the shortcut to change the audio device.";
                                    _shortcutErrors.Add(error);
                                    if (worstError != ShortcutValidity.Error)
                                        worstError = ShortcutValidity.Warning;
                                }
                                if (audioDevice.State == DeviceState.NotPresent)
                                {
                                    logger.Warn($"ShortcutRepository/RefreshValidity: Detected audio playback device {audioDevice.FullName} is the one we want, but it is not present!");
                                    ShortcutError error = new ShortcutError();
                                    error.Name = "AudioDeviceNotPresent";
                                    error.Validity = ShortcutValidity.Error;
                                    error.Message = $"The Audio Device {AudioDevice} is not present, so the shortcut '{Name}' cannot be used.";
                                    _shortcutErrors.Add(error);
                                    if (worstError != ShortcutValidity.Error)
                                        worstError = ShortcutValidity.Error;
                                }
                                // As per Issue #39, this causes issues on HDMI audio devices and others that *could* work if the screen was enabled.
                                // Disabling this code as it is too much error checking for audio devices. The user can plug these in after the chagne and they will work.
                                /*if (audioDevice.State == DeviceState.Unplugged)
                                {
                                    logger.Warn($"ShortcutRepository/RefreshValidity: Detected audio playback device {audioDevice.FullName} is the one we want, but it is unplugged!");
                                    ShortcutError error = new ShortcutError();
                                    error.Name = "AudioDeviceUnplugged";
                                    error.Validity = ShortcutValidity.Warning;
                                    error.Message = $"The Audio Device {AudioDevice} is unplugged, so the shortcut '{Name}' cannot be used. You need to plug in the audio device to use this shortcut, or edit the shortcut to change the audio device.";
                                    _shortcutErrors.Add(error);
                                    if (worstError != ShortcutValidity.Error)
                                        worstError = ShortcutValidity.Warning;
                                }*/
                                break;
                            }
                        }
                    }                    
                }
                else
                {
                    logger.Error($"ShortcutRepository/RefreshValidity: The audio device chipset is not supported by DisplayMagician!");
                    ShortcutError error = new ShortcutError();
                    error.Name = "AudioChipsetNotSupported";
                    error.Validity = ShortcutValidity.Warning;
                    error.Message = $"The Audio chipset isn't supported by DisplayMagician. You need to edit the shortcut to not change the audio output settings.";
                    _shortcutErrors.Add(error);
                    if (worstError != ShortcutValidity.Error)
                        worstError = ShortcutValidity.Warning;
                }
            }
            // Check the Capture Device is still valid (if one is specified)
            if (ChangeCaptureDevice)
            {
                IEnumerable<CoreAudioDevice> captureDevices = null;
                if (audioController != null)
                {
                    try
                    {
                        captureDevices = audioController.GetCaptureDevices();
                    }
                    catch(Exception ex)
                    {
                        logger.Warn(ex, $"ShortcutRepository/RefreshValidity: Exception trying to get all capture devices!");
                    }

                    if (captureDevices != null)
                    {
                        foreach (CoreAudioDevice captureDevice in captureDevices)
                        {
                            logger.Trace($"ShortcutItem/RefreshValidity: Detected capture device {captureDevice.FullName}");
                            if (captureDevice.FullName.Equals(CaptureDevice))
                            {
                                logger.Trace($"ShortcutItem/RefreshValidity: Detected capture device {captureDevice.FullName} is the one we want!");
                                if (captureDevice.State == DeviceState.Disabled)
                                {
                                    logger.Warn($"ShortcutRepository/RefreshValidity: Detected capture device {captureDevice.FullName} is the one we want, but it is disabled!");
                                    ShortcutError error = new ShortcutError();
                                    error.Name = "CaptureDeviceDisabled";
                                    error.Validity = ShortcutValidity.Warning;
                                    error.Message = $"The Capture Device {CaptureDevice} is disabled, so the shortcut '{Name}' cannot be used. You need to enable the capture device to use this shortcut, or edit the shortcut to change the capture device.";
                                    _shortcutErrors.Add(error);
                                    if (worstError != ShortcutValidity.Error)
                                        worstError = ShortcutValidity.Warning;
                                }
                                if (captureDevice.State == DeviceState.NotPresent)
                                {
                                    logger.Warn($"ShortcutRepository/RefreshValidity: Detected capture device {captureDevice.FullName} is the one we want, but it is not present!");
                                    ShortcutError error = new ShortcutError();
                                    error.Name = "CaptureDeviceNotPresent";
                                    error.Validity = ShortcutValidity.Error;
                                    error.Message = $"The Capture Device {CaptureDevice} is not present, so the shortcut '{Name}' cannot be used.";
                                    _shortcutErrors.Add(error);
                                    if (worstError != ShortcutValidity.Error)
                                        worstError = ShortcutValidity.Error;
                                }
                                // As per Issue #39, this causes issues on HDMI audiodevices and others that *could* work if the screen was enabled.
                                // Disabling this code as it is too much error checking for capture devices. The user can plug these in after the chagne and they will work.
                                /*if (captureDevice.State == DeviceState.Unplugged)
                                {
                                    logger.Warn($"ShortcutRepository/RefreshValidity: Detected capture device {captureDevice.FullName} is the one we want, but it is unplugged!");
                                    ShortcutError error = new ShortcutError();
                                    error.Name = "CaptureDeviceUnplugged";
                                    error.Validity = ShortcutValidity.Warning;
                                    error.Message = $"The Capture Device {CaptureDevice} is unplugged, so the shortcut '{Name}' cannot be used. You need to plug in the capture device to use this shortcut, or edit the shortcut to change the capture device.";
                                    _shortcutErrors.Add(error);
                                    if (worstError != ShortcutValidity.Error)
                                        worstError = ShortcutValidity.Warning;
                                }*/
                                break;
                            }
                        }
                    }                    
                } 
                else
                {
                    logger.Error($"ShortcutRepository/RefreshValidity: The capture device chipset is not supported by DisplayMagician!");
                    ShortcutError error = new ShortcutError();
                    error.Name = "AudioChipsetNotSupported";
                    error.Validity = ShortcutValidity.Warning;
                    error.Message = $"The Audio chipset isn't supported by DisplayMagician. You need to edit the shortcut to not change the microphone input settings.";
                    _shortcutErrors.Add(error);
                    if (worstError != ShortcutValidity.Error)
                        worstError = ShortcutValidity.Warning;
                }
            }

            // TODO Do all the specified pre-start apps still exist?

            // Save the worst error level to IsValid property
            IsValid = worstError;

        }


        // ReSharper disable once FunctionComplexityOverflow
        // ReSharper disable once CyclomaticComplexity
        public bool CreateShortcut(string shortcutFileName)
        {
            string programName = Path.GetFileNameWithoutExtension(ExecutableNameAndPath);
            string shortcutDescription = string.Empty;
            string shortcutIconFileName;

            var shortcutArgs = new List<string>
            {
                // Add the SwitchProfile command as the first argument to start to switch to another profile
                $"{DisplayMagicianStartupAction.RunShortcut}",
                $"\"{UUID}\""
            };

            // Only add the rest of the options if the permanence is temporary
            if (DisplayPermanence == ShortcutPermanence.Temporary)
            {
                // Only add this set of options if the shortcut is to an standalone application
                if (Category == ShortcutCategory.Application)
                {
                    // Prepare text for the shortcut description field
                    shortcutDescription = string.Format(Language.Execute_application_with_profile, programName, ProfileToUse.Name);

                }
                // Only add the rest of the options if the temporary switch radio button is set
                // and if the game launching radio button is set
                else if (DisplayPermanence == ShortcutPermanence.Temporary)
                {
                    // Prepare text for the shortcut description field
                    shortcutDescription = string.Format(Language.Execute_application_with_profile, GameName, ProfileToUse.Name);
                }

            }
            // Only add the rest of the options if the permanent switch radio button is set
            else
            {
                // Prepare text for the shortcut description field
                shortcutDescription = string.Format(Language.Switching_display_profile_to_profile, ProfileToUse.Name);
            }

            // Now we are ready to create a shortcut based on the filename the user gave us
            shortcutFileName = Path.ChangeExtension(shortcutFileName, @"lnk");

            // And we use the Icon from the shortcutIconCache
            //SaveShortcutIconToCache();
            shortcutIconFileName = SavedShortcutIconCacheFilename;

            // If the user supplied a file
            if (shortcutFileName != null)
            {
                try
                {
                    // Remove the old file if it exists to replace it
                    if (System.IO.File.Exists(shortcutFileName))
                    {
                        System.IO.File.Delete(shortcutFileName);
                    }                   

                    WshShell shell = new WshShell();
                    IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutFileName);

                    shortcut.TargetPath = Application.ExecutablePath;
                    shortcut.Arguments = string.Join(" ", shortcutArgs);
                    shortcut.Description = shortcutDescription;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) ??
                                                string.Empty;

                    shortcut.IconLocation = shortcutIconFileName;
                    shortcut.Save();
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/CreateShortcut: Execption while creating desktop shortcut!");

                    // Clean up a failed attempt
                    if (System.IO.File.Exists(shortcutFileName))
                    {
                        System.IO.File.Delete(shortcutFileName);
                    }
                }
            }

            // Return a status on how it went
            // true if it was a success or false if it was not
            return shortcutFileName != null && System.IO.File.Exists(shortcutFileName);
        }


        

        public void AutoSuggestShortcutName()
        {
            if (AutoName && _profileToUse is ProfileItem)
            {
               if (Category.Equals(ShortcutCategory.Game) && GameName.Length > 0)
                {
                    _name = $"{GameName} ({_profileToUse.Name})";
                }
                else if (Category.Equals(ShortcutCategory.Application) && ExecutableNameAndPath.Length > 0)
                {
                    string baseName = Path.GetFileNameWithoutExtension(ExecutableNameAndPath);
                    _name = $"{baseName} ({_profileToUse.Name})";
                }
                else
                {
                    if (DisplayPermanence.Equals(ShortcutPermanence.Permanent))
                        _name = $"{_profileToUse.Name}";
                    else if (DisplayPermanence.Equals(ShortcutPermanence.Temporary))
                        _name = $"{_profileToUse.Name} (Temporary)";
                }
            }
        }

        public int CompareTo(object obj)
        {
            if (!(obj is ShortcutItem)) throw new ArgumentException("Object to CompareTo is not a Shortcut"); ;

            ShortcutItem otherShortcut = (ShortcutItem) obj;
            return this.Name.CompareTo(otherShortcut.Name);                
        }

    }


    #region JsonConverterBitmap
    internal class CustomBitmapConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        //convert from byte to bitmap (deserialize)

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string image = (string)reader.Value;

            byte[] byteBuffer = Convert.FromBase64String(image);
            MemoryStream memoryStream = new MemoryStream(byteBuffer)
            {
                Position = 0
            };

            return (Bitmap)Bitmap.FromStream(memoryStream);
        }

        //convert bitmap to byte (serialize)
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Bitmap bitmap = (Bitmap)value;

            ImageConverter converter = new ImageConverter();
            writer.WriteValue((byte[])converter.ConvertTo(bitmap, typeof(byte[])));
        }

        public static System.Drawing.Imaging.ImageFormat GetImageFormat(Bitmap bitmap)
        {
            ImageFormat img = bitmap.RawFormat;

            if (img.Equals(System.Drawing.Imaging.ImageFormat.Jpeg))
                return System.Drawing.Imaging.ImageFormat.Jpeg;
            if (img.Equals(System.Drawing.Imaging.ImageFormat.Bmp))
                return System.Drawing.Imaging.ImageFormat.Bmp;
            if (img.Equals(System.Drawing.Imaging.ImageFormat.Png))
                return System.Drawing.Imaging.ImageFormat.Png;
            if (img.Equals(System.Drawing.Imaging.ImageFormat.Emf))
                return System.Drawing.Imaging.ImageFormat.Emf;
            if (img.Equals(System.Drawing.Imaging.ImageFormat.Exif))
                return System.Drawing.Imaging.ImageFormat.Exif;
            if (img.Equals(System.Drawing.Imaging.ImageFormat.Gif))
                return System.Drawing.Imaging.ImageFormat.Gif;
            if (img.Equals(System.Drawing.Imaging.ImageFormat.Icon))
                return System.Drawing.Imaging.ImageFormat.Icon;
            if (img.Equals(System.Drawing.Imaging.ImageFormat.MemoryBmp))
                return System.Drawing.Imaging.ImageFormat.MemoryBmp;
            if (img.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
                return System.Drawing.Imaging.ImageFormat.Tiff;
            else
                return System.Drawing.Imaging.ImageFormat.Wmf;
        }

    }

    #endregion
}
