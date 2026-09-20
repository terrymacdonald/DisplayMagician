using DisplayMagician.Contracts;
using DisplayMagician.GameLibraries;
//using DisplayMagician.Resources;
using System.Drawing;
using DisplayMagicianShared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using IconLib;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using TsudaKageyu;
using System.ComponentModel;
using System.Linq;
using DisplayMagicianShared.NVIDIA;
using DisplayMagicianShared.Windows;
using DisplayMagician.ConfigurationDefinitions;

namespace DisplayMagician
{
    public enum ShortcutValidity : int
    {
        Valid = 0,
        Warning = 1,
        Error = 2,
    }

    public struct StartProgram
    {
        public int Priority;
        public bool Disabled;
        public ProcessPriority ProcessPriority;
        public string Executable;
        public string ApplicationId;
        public string ApplicationName;
        public string Arguments;
        public bool ExecutableArgumentsRequired;
        public bool CloseOnFinish;
        public bool DontStartIfAlreadyRunning;
        public bool RunAsAdministrator;
    }

    public struct AfterProgram
    {
        public int Priority;
        public bool Disabled;
        public ProcessPriority ProcessPriority;
        public string Executable;
        public string Arguments;
        public bool ExecutableArgumentsRequired;
        public bool DontStartIfAlreadyRunning;
        public bool RunAsAdministrator;
    }

    public struct StopProgram
    {
        public int Priority;
        public bool Disabled;
        public string Executable;
        public bool RestartAfterwards;
        public ProcessPriority RestartProcessPriority;
        public bool RunAsAdministrator;
    }

    public struct ExecutableShortcutData
    {
        public string DifferentExecutableToMonitor;
        public string ExecutableNameAndPath;
        public bool RunAsAdministrator;
        public int ExecutableTimeout;
        public string ExecutableArguments;
        public bool ExecutableArgumentsRequired;
        public bool ProcessNameToMonitorUsesExecutable;
        public ProcessPriority ProcessPriority;
    }

    public struct AppShortcutData
    {
        public AppView AppToUse;
        public string DifferentExecutableToMonitor;
        public bool RunAsAdministrator;
        public int ExecutableTimeout;
        public bool ProcessNameToMonitorUsesExecutable;
        public ProcessPriority ProcessPriority;
    }

    public struct GameShorcutData
    {
        public GameView GameToPlay;
        public GameLaunchMode GameLaunchMode;
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

    public struct ShortcutBitmap
    {
        public string UUID;
        public string Name;
        public int Order;
        public string Source;
        [JsonConverter(typeof(CustomBitmapConverter))]
        public Bitmap Image;
        public Size Size;

        public ShortcutBitmap()
        {
            UUID = "";
            Name = "";
            Order = 0;
            Source = "";
            Size = new Size(1, 1);
            Image = new Bitmap(1, 1);
        }

        public override bool Equals(object obj) => obj is ShortcutBitmap other && this.Equals(other);

        public bool Equals(ShortcutBitmap other)
        => Size.Equals(other.Size) &&
            Image.Equals(other.Image);

        public override int GetHashCode()
        {
            return (Size, Image).GetHashCode();
        }
        public static bool operator ==(ShortcutBitmap lhs, ShortcutBitmap rhs) => lhs.Equals(rhs);

        public static bool operator !=(ShortcutBitmap lhs, ShortcutBitmap rhs) => !(lhs == rhs);
    }

    public class ShortcutItem : IComparable
    {

        private string _profileUuid = "";
        private ProfileItem _profileToUse;
        private string _uuid = "";
        private string _name = "";
        private ShortcutCategory _category = ShortcutCategory.Game;
        private string _differentExecutableToMonitor;
        private string _applicationId = "";
        private string _applicationName = "";
        private SupportedAppLibraryType _applicationLibrary = SupportedAppLibraryType.Unknown;
        private string _executableNameAndPath = "";
        private string _executableArguments = "";
        private bool _executableArgumentsRequired = false;
        private bool _runExeAsAdministrator = false;
        private bool _processNameToMonitorUsesExecutable = true;
        private ProcessPriority _processPriority = ProcessPriority.Normal;
        private string _gameAppId = "";
        private string _gameName = "";
        private SupportedGameLibraryType _gameLibrary = SupportedGameLibraryType.Unknown;
        private GameLaunchMode _gameLaunchMode = GameLaunchMode.StartGame;
        private int _startTimeout = 60;
        private string _gameArguments = "";
        private bool _gameArgumentsRequired = false;
        private string _differentGameExeToMonitor = "";
        private bool _monitorDifferentGameExe = false;
        public const string SkipAudioProfilesChangeUUID = "00000000-0000-4000-8000-000000000000";
        private string _audioProfileUUID = SkipAudioProfilesChangeUUID;
        private bool _overrideAudioSpeakerVolume = false;
        private bool _overrideAudioMicrophoneVolume = false;
        private int _overrideAudioSpeakerVolumeLevel = 50;
        private int _overrideAudioMicrophoneVolumeLevel = 50;
        private ShortcutPermanence _displayPermanence = ShortcutPermanence.Temporary;
        private ShortcutPermanence _audioPermanence = ShortcutPermanence.Temporary;
        private bool _autoName = true;
        private ShortcutValidity _isValid;
        private List<ShortcutError> _shortcutErrors = new List<ShortcutError>();
        private List<StartProgram> _startPrograms = new List<StartProgram> ();
        private List<AfterProgram> _afterPrograms = new List<AfterProgram>();
        private List<StopProgram> _stopPrograms = new List<StopProgram>();
        private Bitmap _shortcutBitmap, _originalBitmap;

#pragma warning disable CS3008 // Identifier is not CLS-compliant
        private string _originalIconPath;
        private ShortcutBitmap _selectedImage = new ShortcutBitmap();
        private List<ShortcutBitmap> _availableImages = new List<ShortcutBitmap>();
        public string _savedShortcutIconCacheFilename;
        private static ProfileItem _skipDisplayChangeProfile;
#pragma warning restore CS3008 // Identifier is not CLS-compliant

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ShortcutItem()
        {
            // Create a new UUID for the shortcut if one wasn't created already
            if (String.IsNullOrWhiteSpace(_uuid))
                _uuid = Guid.NewGuid().ToString("D");

            // Autocreate a name for the shortcut if AutoName is on
            // (and if we have a profile to use)
            if (AutoName && _profileToUse is ProfileItem)
            {
                // If Autoname is on, and then lets autoname it!
                // That populates all the right things
                AutoSuggestShortcutName();
            }
        }

        public static Version Version
        {
            get => new Version(1, 0);
        }

        [DefaultValue("")]
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

        [DefaultValue("")]
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

        [DefaultValue(true)]
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

        [DefaultValue("")]
        public string ProfileUUID
        {
            get
            {
                return _profileUuid;
            }
            set
            {
                _profileUuid = value;

                // The skip UUID is a special virtual profile - not in AllProfiles, handle it directly
                if (_profileUuid.Equals(ProfileItem.SkipDisplayChangeUUID, StringComparison.OrdinalIgnoreCase))
                {
                    _profileToUse = CreateSkipDisplayChangeProfile();
                    return;
                }

                // We try to find and set the ProfileToUse
                foreach (ProfileItem profileToTest in ProfileRepository.AllProfiles)
                {
                    if (profileToTest.UUID.Equals(_profileUuid, StringComparison.OrdinalIgnoreCase))
                        _profileToUse = profileToTest;
                }
            }
        }

        [DefaultValue(ShortcutPermanence.Temporary)]
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

        [DefaultValue(ShortcutPermanence.Temporary)]
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

        [DefaultValue(ShortcutCategory.Game)]
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

        [DefaultValue(ProcessPriority.Normal)]
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

        [DefaultValue("")]
        public string DifferentExecutableToMonitor
        {
            get
            {
                return _differentExecutableToMonitor;
            }

            set
            {
                if (value == null)
                {
                    _differentExecutableToMonitor = "";
                }
                else
                {
                    _differentExecutableToMonitor = value;
                }

            }
        }

        [DefaultValue("")]
        public string ApplicationId
        {
            get
            {
                return _applicationId;
            }

            set
            {
                _applicationId = value;

            }
        }

        [DefaultValue("")]
        public string ApplicationName
        {
            get
            {
                return _applicationName;
            }

            set
            {
                _applicationName = value;

            }
        }

        [DefaultValue(SupportedAppLibraryType.Unknown)]
        public SupportedAppLibraryType ApplicationLibrary
        {
            get
            {
                return _applicationLibrary;
            }

            set
            {
                _applicationLibrary = value;

            }
        }

        [DefaultValue("")]
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
                if (Category.Equals(ShortcutCategory.Executable))
                    _originalIconPath = value;

            }
        }

        [DefaultValue("")]
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

        [DefaultValue(false)]
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

        [DefaultValue(false)]
        public bool RunExeAsAdministrator
        {
            get
            {
                return _runExeAsAdministrator;
            }

            set
            {
                _runExeAsAdministrator = value;
            }
        }

        [DefaultValue(true)]
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

        [DefaultValue("")]
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

        [DefaultValue("")]
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

        [DefaultValue(SupportedGameLibraryType.Unknown)]
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

        [DefaultValue(GameLaunchMode.StartGame)]
        public GameLaunchMode GameLaunchMode
        {
            get => _gameLaunchMode;
            set => _gameLaunchMode = value;
        }

        [DefaultValue(60)]
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

        [DefaultValue("")]
        public string GameArguments
        {
            get
            {
                return _gameArguments;
            }

            set
            {
                if (value == null)
                {
                    _gameArguments = "";
                }
                else
                {
                    _gameArguments = value;
                }
            }
        }

        [DefaultValue(false)]
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

        [DefaultValue("")]
        public string DifferentGameExeToMonitor
        {
            get
            {
                return _differentGameExeToMonitor;
            }

            set
            {
                if (value == null)
                {
                    _differentGameExeToMonitor = "";
                }
                else
                {
                    _differentGameExeToMonitor = value;
                }
            }
        }

        [DefaultValue(false)]
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

        [DefaultValue("")]
        public string AudioProfileUUID
        {
            get
            {
                return _audioProfileUUID;
            }
            set
            {
                _audioProfileUUID = value;
            }
        }

        [DefaultValue(false)]
        public bool OverrideAudioSpeakerVolume
        {
            get
            {
                return _overrideAudioSpeakerVolume;
            }
            set
            {
                _overrideAudioSpeakerVolume = value;
            }
        }

        [DefaultValue(false)]
        public bool OverrideAudioMicrophoneVolume
        {
            get
            {
                return _overrideAudioMicrophoneVolume;
            }

            set
            {
                _overrideAudioMicrophoneVolume = value;
            }
        }

        [DefaultValue(50)]
        public int OverrideAudioSpeakerVolumeLevel
        {
            get
            {
                return _overrideAudioSpeakerVolumeLevel;
            }
            set
            {
                _overrideAudioSpeakerVolumeLevel = value;
            }
        }

        [DefaultValue(50)]
        public int OverrideAudioMicrophoneVolumeLevel
        {
            get
            {
                return _overrideAudioMicrophoneVolumeLevel;
            }
            set
            {
                _overrideAudioMicrophoneVolumeLevel = value;
            }
        }

        [DefaultValue(default(List<StartProgram>))]
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

        [DefaultValue(default(List<AfterProgram>))]
        public List<AfterProgram> AfterPrograms
        {
            get
            {
                return _afterPrograms;
            }

            set
            {
                _afterPrograms = value;
            }
        }

        [DefaultValue(default(List<StopProgram>))]
        public List<StopProgram> StopPrograms
        {
            get
            {
                return _stopPrograms;
            }

            set
            {
                _stopPrograms = value;
            }
        }

        [DefaultValue("")]
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

        [DefaultValue(default(Bitmap))]
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

        [DefaultValue(default(Bitmap))]
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

        [DefaultValue("")]
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

        [DefaultValue("")]
        public ShortcutBitmap SelectedImage
        {
            get
            {
                return _selectedImage;
            }

            set
            {
                _selectedImage = value;

            }
        }

        [DefaultValue(default(List<ShortcutBitmap>))]
        public List<ShortcutBitmap> AvailableImages
        {
            get
            {
                return _availableImages;
            }

            set
            {
                _availableImages = value;

            }
        }

        /// <summary>
        /// Creates the portable persisted definition used by the User Agent.
        /// WinForms-specific image data and live profile, audio, game, and app
        /// objects deliberately remain outside this definition.
        /// </summary>
        public ShortcutDefinition CreateConfigurationDefinition()
        {
            return new ShortcutDefinition
            {
                Id = UUID,
                Name = Name,
                Category = (ShortcutDefinitionCategory)(int)Category,
                AutoName = AutoName,
                ProfileId = ProfileUUID,
                AudioProfileId = AudioProfileUUID,
                DisplayPermanence = (ShortcutDefinitionPermanence)(int)DisplayPermanence,
                AudioPermanence = (ShortcutDefinitionPermanence)(int)AudioPermanence,
                ProcessPriority = (ShortcutDefinitionProcessPriority)(int)ProcessPriority,
                ExecutablePath = ExecutableNameAndPath,
                ExecutableArguments = ExecutableArguments,
                ExecutableArgumentsRequired = ExecutableArgumentsRequired,
                RunExecutableAsAdministrator = RunExeAsAdministrator,
                MonitorExecutablePath = ProcessNameToMonitorUsesExecutable,
                DifferentExecutablePathToMonitor = DifferentExecutableToMonitor,
                ApplicationId = ApplicationId,
                ApplicationName = ApplicationName,
                ApplicationLibrary = (int)ApplicationLibrary,
                GameAppId = GameAppId,
                GameName = GameName,
                GameLibrary = (int)GameLibrary,
                GameLaunchMode = GameLaunchMode,
                StartTimeoutSeconds = StartTimeout,
                GameArguments = GameArguments,
                GameArgumentsRequired = GameArgumentsRequired,
                DifferentGameExecutablePathToMonitor = DifferentGameExeToMonitor,
                MonitorDifferentGameExecutable = MonitorDifferentGameExe,
                OverrideAudioSpeakerVolume = OverrideAudioSpeakerVolume,
                OverrideAudioSpeakerVolumeLevel = OverrideAudioSpeakerVolumeLevel,
                OverrideAudioMicrophoneVolume = OverrideAudioMicrophoneVolume,
                OverrideAudioMicrophoneVolumeLevel = OverrideAudioMicrophoneVolumeLevel,
                StartPrograms = StartPrograms?.Select(program => new ShortcutStartProgramDefinition
                {
                    Priority = program.Priority,
                    Disabled = program.Disabled,
                    ProcessPriority = (ShortcutDefinitionProcessPriority)(int)program.ProcessPriority,
                    ExecutablePath = program.Executable,
                    ApplicationId = program.ApplicationId,
                    ApplicationName = program.ApplicationName,
                    Arguments = program.Arguments,
                    ArgumentsRequired = program.ExecutableArgumentsRequired,
                    CloseOnFinish = program.CloseOnFinish,
                    DoNotStartIfAlreadyRunning = program.DontStartIfAlreadyRunning,
                    RunAsAdministrator = program.RunAsAdministrator
                }).ToArray() ?? Array.Empty<ShortcutStartProgramDefinition>(),
                AfterPrograms = AfterPrograms?.Select(program => new ShortcutAfterProgramDefinition
                {
                    Priority = program.Priority,
                    Disabled = program.Disabled,
                    ProcessPriority = (ShortcutDefinitionProcessPriority)(int)program.ProcessPriority,
                    ExecutablePath = program.Executable,
                    Arguments = program.Arguments,
                    ArgumentsRequired = program.ExecutableArgumentsRequired,
                    DoNotStartIfAlreadyRunning = program.DontStartIfAlreadyRunning,
                    RunAsAdministrator = program.RunAsAdministrator
                }).ToArray() ?? Array.Empty<ShortcutAfterProgramDefinition>(),
                StopPrograms = StopPrograms?.Select(program => new ShortcutStopProgramDefinition
                {
                    Priority = program.Priority,
                    Disabled = program.Disabled,
                    ExecutablePath = program.Executable,
                    RestartAfterwards = program.RestartAfterwards,
                    RestartProcessPriority = (ShortcutDefinitionProcessPriority)(int)program.RestartProcessPriority,
                    RunAsAdministrator = program.RunAsAdministrator
                }).ToArray() ?? Array.Empty<ShortcutStopProgramDefinition>()
            };
        }


        [JsonIgnore]
        public static ProfileItem SkipDisplayChangeProfile
        {
            get
            {
                if (_skipDisplayChangeProfile == null)
                {
                    _skipDisplayChangeProfile  = CreateSkipDisplayChangeProfile();
                }
                return _skipDisplayChangeProfile;
            }
        }

        public void UpdateNoGameShortcut(
            string name,
#pragma warning disable CS3001 // Argument type is not CLS-compliant
            ProfileItem profile,
#pragma warning restore CS3001 // Argument type is not CLS-compliant
            ShortcutPermanence displayPermanence,
            ShortcutPermanence audioPermanence,
            string audioProfileId = null,
            bool overrideAudioSpeakerVolume = false,
            int overrideAudioSpeakerVolumeLevel = 50,
            bool overrideAudioMicrophoneVolume = false,
            int overrideAudioMicrophoneVolumeLevel = 50,
            List<StartProgram> startPrograms = null,
            List<AfterProgram> afterPrograms = null,
            List<StopProgram> stopPrograms = null,
            bool autoName = true,
            string uuid = ""
            )
        {
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name;
            _category = ShortcutCategory.NoGame;
            _profileToUse = profile;
            _audioProfileUUID = string.IsNullOrWhiteSpace(audioProfileId) ? SkipAudioProfilesChangeUUID : audioProfileId;
            _displayPermanence = displayPermanence;
            _audioPermanence = audioPermanence;
            _overrideAudioSpeakerVolume = overrideAudioSpeakerVolume;
            _overrideAudioSpeakerVolumeLevel = overrideAudioSpeakerVolumeLevel;
            _overrideAudioMicrophoneVolume = overrideAudioMicrophoneVolume;
            _overrideAudioMicrophoneVolumeLevel = overrideAudioMicrophoneVolumeLevel;

            _autoName = autoName;
            _startPrograms = startPrograms;
            _afterPrograms = afterPrograms;
            _stopPrograms = stopPrograms;
            _originalIconPath = "";

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;
            _originalBitmap = profile.ProfileBitmap;
            _shortcutBitmap = profile.ProfileBitmap;

            // Empty out the unused shortcut data
            _executableNameAndPath = "";
            _executableArgumentsRequired = false;
            _executableArguments = "";
            _runExeAsAdministrator = false;
            _processNameToMonitorUsesExecutable = false;
            _differentExecutableToMonitor = "";

            _applicationId = "";
            _applicationName = "";
            _applicationLibrary = SupportedAppLibraryType.Unknown;

            _gameAppId = "";
            _gameArgumentsRequired = false;
            _gameArguments = "";
            _gameLibrary = SupportedGameLibraryType.Unknown;
            _gameLaunchMode = GameLaunchMode.StartGame;
            _monitorDifferentGameExe = false;
            _differentGameExeToMonitor = "";
            _processPriority = ProcessPriority.Normal;
            _startTimeout = 60;

            ReplaceShortcutIconInCache();
            RefreshValidity();
        }

        public void UpdateGameShortcut(
            string name,
#pragma warning disable CS3001 // Argument type is not CLS-compliant
            ProfileItem profile,
#pragma warning restore CS3001 // Argument type is not CLS-compliant
            GameShorcutData game,
            ShortcutPermanence displayPermanence,
            ShortcutPermanence audioPermanence,
            string originalIconPath,
            ShortcutBitmap selectedImage,
            List<ShortcutBitmap> availableImages,
            string audioProfileId = null,
            bool overrideAudioSpeakerVolume = false,
            int overrideAudioSpeakerVolumeLevel = 50,
            bool overrideAudioMicrophoneVolume = false,
            int overrideAudioMicrophoneVolumeLevel = 50,
            List<StartProgram> startPrograms = null,
            List<AfterProgram> afterPrograms = null,
            List<StopProgram> stopPrograms = null,
            bool autoName = true,
            string uuid = ""
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
            _gameLibrary = (SupportedGameLibraryType)game.GameToPlay.Library;
            _gameLaunchMode = game.GameLaunchMode;
            _startTimeout = game.StartTimeout;
            _gameArguments = game.GameArguments;
            _gameArgumentsRequired = game.GameArgumentsRequired;
            _differentGameExeToMonitor = game.DifferentGameExeToMonitor;
            _monitorDifferentGameExe = game.MonitorDifferentGameExe;
            _processPriority = game.ProcessPriority;
            _audioProfileUUID = string.IsNullOrWhiteSpace(audioProfileId) ? SkipAudioProfilesChangeUUID : audioProfileId;
            _displayPermanence = displayPermanence;
            _audioPermanence = audioPermanence;
            _overrideAudioSpeakerVolume = overrideAudioSpeakerVolume;
            _overrideAudioSpeakerVolumeLevel = overrideAudioSpeakerVolumeLevel;
            _overrideAudioMicrophoneVolume = overrideAudioMicrophoneVolume;
            _overrideAudioMicrophoneVolumeLevel = overrideAudioMicrophoneVolumeLevel;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _afterPrograms = afterPrograms;
            _stopPrograms = stopPrograms;
            _originalIconPath = originalIconPath;
            _selectedImage = selectedImage;
            _availableImages = availableImages;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;

            // We create the Bitmaps for the game
            _originalBitmap = selectedImage.Image;
            // Now we use the originalBitmap or userBitmap, and create the shortcutBitmap from it
            _shortcutBitmap = ImageUtils.MakeBitmapOverlay(_originalBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);

            // Empty out the unused shortcut data
            _executableNameAndPath = "";
            _executableArgumentsRequired = false;
            _executableArguments = "";
            _runExeAsAdministrator = false;
            _processNameToMonitorUsesExecutable = false;
            _differentExecutableToMonitor = "";

            _applicationId = "";
            _applicationName = "";
            _applicationLibrary = SupportedAppLibraryType.Unknown;

            ReplaceShortcutIconInCache();
            RefreshValidity();
        }

        public void UpdateExecutableShortcut(
            string name,
#pragma warning disable CS3001 // Argument type is not CLS-compliant
            ProfileItem profile,
#pragma warning restore CS3001 // Argument type is not CLS-compliant
            ExecutableShortcutData executable,
            ShortcutPermanence displayPermanence,
            ShortcutPermanence audioPermanence,
            string originalIconPath,
            ShortcutBitmap selectedImage,
            List<ShortcutBitmap> availableImages,
            string audioProfileId = null,
            bool overrideAudioSpeakerVolume = false,
            int overrideAudioSpeakerVolumeLevel = 50,
            bool overrideAudioMicrophoneVolume = false,
            int overrideAudioMicrophoneVolumeLevel = 50,
            List<StartProgram> startPrograms = null,
            List<AfterProgram> afterPrograms = null,
            List<StopProgram> stopPrograms = null,
            bool autoName = true,
            string uuid = ""
            )
        {
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name;
            _profileToUse = profile;
            _category = ShortcutCategory.Executable;
            _differentExecutableToMonitor = executable.DifferentExecutableToMonitor;
            _executableNameAndPath = executable.ExecutableNameAndPath;
            _runExeAsAdministrator = executable.RunAsAdministrator;
            _startTimeout = executable.ExecutableTimeout;
            _executableArguments = executable.ExecutableArguments;
            _executableArgumentsRequired = executable.ExecutableArgumentsRequired;
            _processNameToMonitorUsesExecutable = executable.ProcessNameToMonitorUsesExecutable;
            _processPriority = executable.ProcessPriority;
            _audioProfileUUID = string.IsNullOrWhiteSpace(audioProfileId) ? SkipAudioProfilesChangeUUID : audioProfileId;
            _displayPermanence = displayPermanence;
            _audioPermanence = audioPermanence;
            _overrideAudioSpeakerVolume = overrideAudioSpeakerVolume;
            _overrideAudioSpeakerVolumeLevel = overrideAudioSpeakerVolumeLevel;
            _overrideAudioMicrophoneVolume = overrideAudioMicrophoneVolume;
            _overrideAudioMicrophoneVolumeLevel = overrideAudioMicrophoneVolumeLevel;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _afterPrograms = afterPrograms;
            _stopPrograms = stopPrograms;
            _originalIconPath = originalIconPath;
            _selectedImage = selectedImage;
            _availableImages = availableImages;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;

            // We create the Bitmaps for the executable
            _originalBitmap = selectedImage.Image;
            // Now we use the originalBitmap or userBitmap, and create the shortcutBitmap from it
            _shortcutBitmap = ImageUtils.MakeBitmapOverlay(_originalBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);

            // Empty out the unused shortcut data
            _gameAppId = "";
            _gameArgumentsRequired = false;
            _gameArguments = "";
            _gameLibrary = SupportedGameLibraryType.Unknown;
            _gameLaunchMode = GameLaunchMode.StartGame;
            _monitorDifferentGameExe = false;
            _differentGameExeToMonitor = "";

            _applicationId = "";
            _applicationName = "";
            _applicationLibrary = SupportedAppLibraryType.Unknown;

            ReplaceShortcutIconInCache();
            RefreshValidity();
        }

        public void UpdateAppShortcut(
            string name,
#pragma warning disable CS3001 // Argument type is not CLS-compliant
            ProfileItem profile,
#pragma warning restore CS3001 // Argument type is not CLS-compliant
            AppShortcutData app,
            ShortcutPermanence displayPermanence,
            ShortcutPermanence audioPermanence,
            ShortcutBitmap selectedImage,
            List<ShortcutBitmap> availableImages,
            SupportedAppLibraryType supportedAppLibraryType,
            string audioProfileId = null,
            bool overrideAudioSpeakerVolume = false,
            int overrideAudioSpeakerVolumeLevel = 50,
            bool overrideAudioMicrophoneVolume = false,
            int overrideAudioMicrophoneVolumeLevel = 50,
            List<StartProgram> startPrograms = null,
            List<AfterProgram> afterPrograms = null,
            List<StopProgram> stopPrograms = null,
            bool autoName = true,
            string uuid = ""
            )
        {
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name;
            _profileToUse = profile;
            _category = ShortcutCategory.Application;
            _applicationId = app.AppToUse.Id;
            _applicationName = app.AppToUse.Name;
            _applicationLibrary = (SupportedAppLibraryType)app.AppToUse.Library;
            _differentExecutableToMonitor = app.DifferentExecutableToMonitor;
            _executableNameAndPath = app.AppToUse.ExecutablePath;
            _runExeAsAdministrator = app.RunAsAdministrator;
            _startTimeout = app.ExecutableTimeout;
            _executableArguments = app.AppToUse.Arguments;
            _executableArgumentsRequired = app.AppToUse.ExecutableArgumentsRequired;
            _processNameToMonitorUsesExecutable = app.ProcessNameToMonitorUsesExecutable;
            _processPriority = app.ProcessPriority;
            _audioProfileUUID = string.IsNullOrWhiteSpace(audioProfileId) ? SkipAudioProfilesChangeUUID : audioProfileId;
            _displayPermanence = displayPermanence;
            _audioPermanence = audioPermanence;
            _overrideAudioSpeakerVolume = overrideAudioSpeakerVolume;
            _overrideAudioSpeakerVolumeLevel = overrideAudioSpeakerVolumeLevel;
            _overrideAudioMicrophoneVolume = overrideAudioMicrophoneVolume;
            _overrideAudioMicrophoneVolumeLevel = overrideAudioMicrophoneVolumeLevel;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _afterPrograms = afterPrograms;
            _stopPrograms = stopPrograms;
            _selectedImage = selectedImage;
            _availableImages = availableImages;

            _originalIconPath = app.AppToUse.IconPath;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;


            // We create the Bitmaps for the executable
            _originalBitmap = selectedImage.Image;
            // Now we use the originalBitmap or userBitmap, and create the shortcutBitmap from it
            _shortcutBitmap = ImageUtils.MakeBitmapOverlay(_originalBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);

            // Empty out the unused shortcut data
            _gameAppId = "";
            _gameArgumentsRequired = false;
            _gameArguments = "";
            _gameLibrary = SupportedGameLibraryType.Unknown;
            _gameLaunchMode = GameLaunchMode.StartGame;
            _monitorDifferentGameExe = false;
            _differentGameExeToMonitor = "";

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
            shortcut.Name = Name + " (Copy)";
            shortcut.AutoName = false; // Force the autoname to be off, as it's a copy.
            shortcut.DisplayPermanence = DisplayPermanence;
            shortcut.AudioPermanence = AudioPermanence;
            shortcut.Category = Category;
            shortcut.DifferentExecutableToMonitor = DifferentExecutableToMonitor;
            shortcut.ExecutableNameAndPath = ExecutableNameAndPath;
            shortcut.ExecutableArguments = ExecutableArguments;
            shortcut.ExecutableArgumentsRequired = ExecutableArgumentsRequired;
            shortcut.RunExeAsAdministrator = RunExeAsAdministrator;
            shortcut.ProcessNameToMonitorUsesExecutable = ProcessNameToMonitorUsesExecutable;
            shortcut.ApplicationId = ApplicationId;
            shortcut.ApplicationName = ApplicationName;
            shortcut.ApplicationLibrary = ApplicationLibrary;
            shortcut.ProcessPriority = ProcessPriority;
            shortcut.GameAppId = GameAppId;
            shortcut.GameName = GameName;
            shortcut.GameLibrary = GameLibrary;
            shortcut.GameLaunchMode = GameLaunchMode;
            shortcut.StartTimeout = StartTimeout;
            shortcut.GameArguments = GameArguments;
            shortcut.GameArgumentsRequired = GameArgumentsRequired;
            shortcut.OriginalIconPath = OriginalIconPath;
            shortcut.IsValid = IsValid;
            shortcut.Errors.AddRange(Errors);
            shortcut.AudioProfileUUID = AudioProfileUUID;
            shortcut.OverrideAudioSpeakerVolume = OverrideAudioSpeakerVolume;
            shortcut.OverrideAudioSpeakerVolumeLevel = OverrideAudioSpeakerVolumeLevel;
            shortcut.OverrideAudioMicrophoneVolume = OverrideAudioMicrophoneVolume;
            shortcut.OverrideAudioMicrophoneVolumeLevel = OverrideAudioMicrophoneVolumeLevel;

            // Duplicate the Images

            shortcut.OriginalLargeBitmap = (Bitmap)OriginalLargeBitmap.Clone();
            shortcut.ShortcutBitmap = (Bitmap)ShortcutBitmap.Clone();
            //shortcut.SavedShortcutIconCacheFilename = SavedShortcutIconCacheFilename; // We want a new shortcut icon!
            shortcut.SelectedImage = ImageUtils.ShortcutBitmapClone(SelectedImage);
            shortcut.AvailableImages = ImageUtils.ShortcutBitmapClone(AvailableImages);

            // Duplicate the start programs
            shortcut.StartPrograms = new List<StartProgram>();
            foreach (StartProgram sp in StartPrograms)
            {
                StartProgram copiedStartProgram = new StartProgram();
                copiedStartProgram.Arguments = sp.Arguments;
                copiedStartProgram.CloseOnFinish = sp.CloseOnFinish;
                copiedStartProgram.Disabled = sp.Disabled;
                copiedStartProgram.DontStartIfAlreadyRunning = sp.DontStartIfAlreadyRunning;
                copiedStartProgram.Executable = sp.Executable;
                copiedStartProgram.ApplicationId = sp.ApplicationId;
                copiedStartProgram.ApplicationName = sp.ApplicationName;
                copiedStartProgram.ExecutableArgumentsRequired = sp.ExecutableArgumentsRequired;
                copiedStartProgram.Priority = sp.Priority;
                copiedStartProgram.ProcessPriority = sp.ProcessPriority;
                copiedStartProgram.RunAsAdministrator = sp.RunAsAdministrator;
                shortcut.StartPrograms.Add(copiedStartProgram);
            }

            // Duplicate the stop programs
            shortcut.AfterPrograms = new List<AfterProgram>();
            foreach (AfterProgram sp in AfterPrograms)
            {
                AfterProgram copiedStopProgram = new AfterProgram();
                copiedStopProgram.Arguments = sp.Arguments;
                copiedStopProgram.Disabled = sp.Disabled;
                copiedStopProgram.DontStartIfAlreadyRunning = sp.DontStartIfAlreadyRunning;
                copiedStopProgram.Executable = sp.Executable;
                copiedStopProgram.ExecutableArgumentsRequired = sp.ExecutableArgumentsRequired;
                copiedStopProgram.Priority = sp.Priority;
                copiedStopProgram.ProcessPriority = sp.ProcessPriority;
                shortcut.AfterPrograms.Add(copiedStopProgram);
            }

            // Duplicate the stop-before programs
            shortcut.StopPrograms = new List<StopProgram>();
            foreach (StopProgram sp in StopPrograms)
            {
                StopProgram copiedEntry = new StopProgram();
                copiedEntry.Disabled = sp.Disabled;
                copiedEntry.Executable = sp.Executable;
                copiedEntry.Priority = sp.Priority;
                copiedEntry.RestartAfterwards = sp.RestartAfterwards;
                copiedEntry.RestartProcessPriority = sp.RestartProcessPriority;
                copiedEntry.RunAsAdministrator = sp.RunAsAdministrator;
                shortcut.StopPrograms.Add(copiedEntry);
            }

            // Do the display and audio profiles last as AutoName will error if done earlier
            shortcut.ProfileToUse = ProfileToUse;
            shortcut.ProfileUUID = ProfileUUID;
            shortcut.AudioProfileUUID = AudioProfileUUID;

            // Save the shortcut incon to the icon cache
            shortcut.SaveShortcutIconToCache();
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
                shortcutIcon.Save(_savedShortcutIconCacheFilename);

            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutItem/SaveShortcutIconToCache: Exception while trying to save the Shortcut icon.");
                shortcutIcon.Clear();
                // If we fail to create an icon any other way, then we use the default profile icon
                logger.Trace($"ShortcutItem/SaveShortcutIconToCache: Using the Display Profile icon for {_profileToUse.Name} as the icon instead.");
                SingleIcon si = shortcutIcon.Add("icon2");
                si.Add(Properties.Resources.displaymagician);
                shortcutIcon.SelectedIndex = 0;
                logger.Trace($"ShortcutItem/SaveShortcutIconToCache: Saving the Display Profile icon for {_profileToUse.Name} to {_savedShortcutIconCacheFilename}.");
                shortcutIcon.Save(_savedShortcutIconCacheFilename);
            }

        }

        public void RefreshValidity()
        {
            // Do some validation checks to make sure the shortcut is sensible
            // And that we have enough to try and action within the shortcut
            // (in other words check everything in the shortcut is still valid)

            Errors.Clear();
            ShortcutValidity worstError = ShortcutValidity.Valid;

            logger.Trace($"ShortcutItem/RefreshValidity: This shortcut is named: {Name}");

            // Does the profile we want to Use still exist?
            if (ProfileUUID == ProfileItem.SkipDisplayChangeUUID)
            {
                // Skip Display Change is a special virtual profile - always valid, never needs checking
                logger.Trace($"ShortcutItem/RefreshValidity: ProfileUUID is SkipDisplayChangeUUID - skipping display profile checks.");
            }
            else
            {
                ProfileItem profileToValidate = ProfileRepository.GetProfile(ProfileUUID);
                if (profileToValidate == null)
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
                else if (!profileToValidate.HasUsableSavedConfiguration(out string profileErrorMessage))
                {
                    logger.Warn($"ShortcutItem/RefreshValidity: The profile '{profileToValidate.Name}' has invalid saved configuration data. {profileErrorMessage}");
                    ShortcutError error = new ShortcutError();
                    error.Name = "InvalidDisplayProfileConfiguration";
                    error.Validity = ShortcutValidity.Error;
                    error.Message = $"The display profile '{profileToValidate.Name}' contains errors and cannot be applied. {profileErrorMessage}";
                    _shortcutErrors.Add(error);
                    worstError = ShortcutValidity.Error;
                }
                else
                {
                    List<string> undetectedDisplays = profileToValidate.GetUndetectedDisplayDescriptions();
                    if (undetectedDisplays.Count > 0)
                    {
                        logger.Warn($"ShortcutItem/RefreshValidity: The display profile '{profileToValidate.Name}' could not detect: {String.Join(", ", undetectedDisplays)}.");
                        ShortcutError error = new ShortcutError();
                        error.Name = "DisplayProfileDetectionAdvisory";
                        error.Validity = ShortcutValidity.Warning;
                        error.Message = $"The display profile '{profileToValidate.Name}' could not detect: {String.Join(", ", undetectedDisplays)}. You can still run the shortcut if you want to but it may not work as expected.";
                        _shortcutErrors.Add(error);
                        if (worstError == ShortcutValidity.Valid)
                            worstError = ShortcutValidity.Warning;
                    }
                }
            }

            // The User Agent owns audio profile validation. WinForms only verifies that this
            // persisted shortcut contains either the skip sentinel or a meaningful profile ID.
            if (string.Equals(AudioProfileUUID, SkipAudioProfilesChangeUUID, StringComparison.OrdinalIgnoreCase))
            {
                // Skip Audio Change is a special virtual profile - always valid, never needs checking
                logger.Trace($"ShortcutItem/RefreshValidity: AudioProfileUUID is SkipAudioProfilesChangeUUID - skipping audio profile existence check.");
            }
            else if (string.IsNullOrWhiteSpace(AudioProfileUUID))
            {
                logger.Warn("ShortcutItem/RefreshValidity: AudioProfileUUID is empty.");
                ShortcutError error = new ShortcutError();
                error.Name = "AudioProfileIdMissing";
                error.Validity = ShortcutValidity.Error;
                error.Message = "The audio profile selected by this shortcut is missing. Please edit the shortcut and select an audio profile or choose not to change audio settings.";
                _shortcutErrors.Add(error);
                worstError = ShortcutValidity.Error;
            }

            // Is the main application still installed?
            if (Category.Equals(ShortcutCategory.Executable))
            {
                logger.Trace($"ShortcutItem/RefreshValidity: This shortcut is an Executable");
                // We need to check if the Application still exists
                if (!System.IO.File.Exists(ExecutableNameAndPath))
                {
                    logger.Warn($"ShortcutItem/RefreshValidity: The executable {ExecutableNameAndPath} DOES NOT exist");
                    ShortcutError error = new ShortcutError();
                    error.Name = "InvalidExecutableNameAndPath";
                    error.Validity = ShortcutValidity.Error;
                    error.Message = $"The executable '{ExecutableNameAndPath}' does not exist, or cannot be accessed by DisplayMagician.";
                    _shortcutErrors.Add(error);
                    if (worstError != ShortcutValidity.Error)
                        worstError = ShortcutValidity.Error;
                }
                else
                {
                    logger.Trace($"ShortcutItem/RefreshValidity: The Executable {ExecutableNameAndPath} exists");
                }

            }
            // Is the main application still installed?
            else if (Category.Equals(ShortcutCategory.Application))
            {
                logger.Trace($"ShortcutItem/RefreshValidity: This shortcut is an Application");
                bool isPackagedApplication = ApplicationLibrary == SupportedAppLibraryType.LocalUWPApp;
                bool isValidApplication = !String.IsNullOrWhiteSpace(ApplicationId) &&
                    !String.IsNullOrWhiteSpace(ApplicationName) &&
                    (!ExecutableArgumentsRequired || !String.IsNullOrWhiteSpace(ExecutableArguments)) &&
                    (isPackagedApplication || (!String.IsNullOrWhiteSpace(ExecutableNameAndPath) && System.IO.File.Exists(ExecutableNameAndPath)));
                if (!isValidApplication)
                {
                    logger.Warn($"ShortcutItem/RefreshValidity: The application '{ApplicationName}' (ID: {ApplicationId}) does not have runnable persisted launch data.");
                    ShortcutError error = new ShortcutError();
                    error.Name = "ApplicationNotInstalled";
                    error.Validity = ShortcutValidity.Error;
                    error.Message = $"The application '{ApplicationName}' does not have valid launch information.";
                    _shortcutErrors.Add(error);
                    if (worstError != ShortcutValidity.Error)
                        worstError = ShortcutValidity.Error;
                }
                else
                {
                    logger.Trace($"ShortcutItem/RefreshValidity: The Application {ExecutableNameAndPath} exists");
                }
            }
            else if (Category.Equals(ShortcutCategory.Game))
            {
                if (string.IsNullOrWhiteSpace(GameAppId) || string.IsNullOrWhiteSpace(GameName))
                {
                    logger.Warn($"ShortcutItem/RefreshValidity: The game shortcut has no persisted game identity (ID: {GameAppId}).");
                    ShortcutError error = new ShortcutError();
                    error.Name = $"{GameName}NotInstalled";
                    error.Validity = ShortcutValidity.Error;
                    error.Message = "The game shortcut does not identify a game to launch.";
                    _shortcutErrors.Add(error);
                    if (worstError != ShortcutValidity.Error)
                        worstError = ShortcutValidity.Error;
                }
            }
            else
            {
                logger.Warn($"ShortcutItem/RefreshValidity: The shortcut '{Name}' has an unknown category '{Category}'.");
                ShortcutError error = new ShortcutError();
                error.Name = "UnknownShortcutCategory";
                error.Validity = ShortcutValidity.Error;
                error.Message = "The shortcut has an unknown type and cannot be run.";
                _shortcutErrors.Add(error);
                worstError = ShortcutValidity.Error;
            }
            // Do all the active/enabled specified start programs still exist?
            foreach (StartProgram sp in StartPrograms)
            {
                if (sp.Disabled)
                    continue;

                if (!String.IsNullOrWhiteSpace(sp.ApplicationId))
                {
                    if (sp.ExecutableArgumentsRequired && String.IsNullOrWhiteSpace(sp.Arguments))
                    {
                        logger.Warn($"ShortcutItem/RefreshValidity: The packaged start program '{sp.ApplicationName}' (ID: {sp.ApplicationId}) requires arguments but none were supplied.");
                        ShortcutError error = new ShortcutError();
                        error.Name = "PackagedStartProgramArgumentsMissing";
                        error.Validity = ShortcutValidity.Warning;
                        error.Message = $"The packaged start program '{sp.ApplicationName}' requires launch arguments, but none were supplied.";
                        _shortcutErrors.Add(error);
                        if (worstError != ShortcutValidity.Error)
                            worstError = ShortcutValidity.Warning;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(sp.Executable))
                {
                    if (!System.IO.File.Exists(sp.Executable))
                    {
                        logger.Warn($"ShortcutItem/RefreshValidity: The start program executable '{sp.Executable}' does not exist");
                        ShortcutError error = new ShortcutError();
                        error.Name = "StartProgramNotExist";
                        error.Validity = ShortcutValidity.Warning;
                        error.Message = $"The start program '{Path.GetFileName(sp.Executable)}' does not exist.";
                        _shortcutErrors.Add(error);
                        if (worstError != ShortcutValidity.Error)
                            worstError = ShortcutValidity.Warning;
                    }
                }
            }

            // Do all the active/enabled specified stop programs still exist?
            foreach (StopProgram sp in StopPrograms)
            {
                if (!sp.Disabled && !string.IsNullOrWhiteSpace(sp.Executable))
                {
                    if (!System.IO.File.Exists(sp.Executable))
                    {
                        logger.Warn($"ShortcutItem/RefreshValidity: The stop program executable '{sp.Executable}' does not exist");
                        ShortcutError error = new ShortcutError();
                        error.Name = "StopProgramNotExist";
                        error.Validity = ShortcutValidity.Warning;
                        error.Message = $"The stop program '{Path.GetFileName(sp.Executable)}' does not exist.";
                        _shortcutErrors.Add(error);
                        if (worstError != ShortcutValidity.Error)
                            worstError = ShortcutValidity.Warning;
                    }
                }
            }

            // After Programs do not prevent the target application or game from running,
            // but the user should know they will be skipped after it closes.
            foreach (AfterProgram ap in AfterPrograms)
            {
                if (!ap.Disabled && !string.IsNullOrWhiteSpace(ap.Executable) && !System.IO.File.Exists(ap.Executable))
                {
                    logger.Warn($"ShortcutItem/RefreshValidity: The after program executable '{ap.Executable}' does not exist");
                    ShortcutError error = new ShortcutError();
                    error.Name = "AfterProgramNotExist";
                    error.Validity = ShortcutValidity.Warning;
                    error.Message = $"The after program '{Path.GetFileName(ap.Executable)}' does not exist.";
                    _shortcutErrors.Add(error);
                    if (worstError != ShortcutValidity.Error)
                        worstError = ShortcutValidity.Warning;
                }
            }

            if ((Category.Equals(ShortcutCategory.Executable) || Category.Equals(ShortcutCategory.Application)) && !ProcessNameToMonitorUsesExecutable &&
                (String.IsNullOrWhiteSpace(DifferentExecutableToMonitor) || !System.IO.File.Exists(DifferentExecutableToMonitor)))
            {
                ShortcutError error = new ShortcutError();
                error.Name = "AlternativeExecutableToMonitorNotExist";
                error.Validity = ShortcutValidity.Warning;
                error.Message = String.IsNullOrWhiteSpace(DifferentExecutableToMonitor)
                    ? "An alternative executable to monitor has not been selected."
                    : $"The alternative executable to monitor '{Path.GetFileName(DifferentExecutableToMonitor)}' does not exist.";
                _shortcutErrors.Add(error);
                if (worstError != ShortcutValidity.Error)
                    worstError = ShortcutValidity.Warning;
            }

            if (Category.Equals(ShortcutCategory.Game) && MonitorDifferentGameExe &&
                (String.IsNullOrWhiteSpace(DifferentGameExeToMonitor) || !System.IO.File.Exists(DifferentGameExeToMonitor)))
            {
                ShortcutError error = new ShortcutError();
                error.Name = "AlternativeGameExecutableToMonitorNotExist";
                error.Validity = ShortcutValidity.Warning;
                error.Message = String.IsNullOrWhiteSpace(DifferentGameExeToMonitor)
                    ? "An alternative game executable to monitor has not been selected."
                    : $"The alternative game executable to monitor '{Path.GetFileName(DifferentGameExeToMonitor)}' does not exist.";
                _shortcutErrors.Add(error);
                if (worstError != ShortcutValidity.Error)
                    worstError = ShortcutValidity.Warning;
            }

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
                if (Category == ShortcutCategory.Executable)
                {
                    // Prepare text for the shortcut description field
                    shortcutDescription = string.Format("Running '{0}' with '{1}' profile.", programName, ProfileToUse.Name);

                }
                else
                {
                    // Prepare text for the shortcut description field
                    shortcutDescription = string.Format("Running '{0}' with '{1}' profile.", GameName, ProfileToUse.Name);
                }

            }
            // Only add the rest of the options if the permanent switch radio button is set
            else
            {
                // Prepare text for the shortcut description field
                shortcutDescription = string.Format("Switching display profile to '{0}'.", ProfileToUse.Name);
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

                    Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                    if (shellType == null)
                        throw new InvalidOperationException("WScript.Shell COM type is unavailable.");

                    dynamic shell = Activator.CreateInstance(shellType);
                    dynamic shortcut = shell.CreateShortcut(shortcutFileName);

                    shortcut.TargetPath = Environment.ProcessPath;
                    shortcut.Arguments = string.Join(" ", shortcutArgs);
                    shortcut.Description = shortcutDescription;
                    shortcut.WorkingDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty;
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

        public string CreateCommand()
        {
            return $"{System.Windows.Forms.Application.ExecutablePath} {DisplayMagicianStartupAction.RunShortcut} \"{UUID}\"";
        }

        public static ProfileItem CreateSkipDisplayChangeProfile()
        {
            return new ProfileItem
            {
                Name = ProfileItem.SkipDisplayChangeName,
                UUID = ProfileItem.SkipDisplayChangeUUID,
                ProfileBitmap = Properties.Resources.skipdisplaychange,
                // The virtual profile has no display layout to render. Reuse its
                // canonical bitmap for the shortcut's bottom-right overlay.
                ProfileTightestBitmap = Properties.Resources.skipdisplaychange
            };
        }


        public void AutoSuggestShortcutName()
        {
            if (AutoName && _profileToUse is ProfileItem)
            {
               if (Category.Equals(ShortcutCategory.Game) && GameName.Length > 0)
                {
                    _name = $"{GameName} ({_profileToUse.Name})";
                }
                else if (Category.Equals(ShortcutCategory.Executable) && ExecutableNameAndPath.Length > 0)
                {
                    string baseName = Path.GetFileNameWithoutExtension(ExecutableNameAndPath);
                    _name = $"{baseName} ({_profileToUse.Name})";
                }
                else if (Category.Equals(ShortcutCategory.Application) && !String.IsNullOrWhiteSpace(ApplicationName))
                {
                    string baseName = Path.GetFileNameWithoutExtension(ExecutableNameAndPath);
                    _name = $"{ApplicationName} ({_profileToUse.Name})";
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

            if (string.IsNullOrEmpty(image))
            {
                return (Bitmap)default(Bitmap);
            }

            byte[] byteBuffer = Convert.FromBase64String(image);
            using (MemoryStream memoryStream = new MemoryStream(byteBuffer))
            using (Image decodedImage = Image.FromStream(memoryStream))
            {
                return new Bitmap(decodedImage);
            }
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
