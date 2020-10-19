using HeliosPlus.GameLibraries;
using HeliosPlus.Resources;
using HeliosPlus.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.IconLib;
using System.Drawing.Imaging;
using TsudaKageyu;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NvAPIWrapper.Native.Display.Structures;
using System.Text.RegularExpressions;
using IWshRuntimeLibrary;
using System.Diagnostics;
using System.Threading;
using HeliosPlus.InterProcess;
using HeliosPlus.UIForms;
using ComponentFactory.Krypton.Toolkit;
using MintPlayer.IconUtils;
using System.Windows.Media.Imaging;

namespace HeliosPlus
{
    public enum ShortcutPermanence
    {
        Permanent,
        Temporary,
    }

    public enum ShortcutCategory
    {
        Application,
        Game,
        NoGame,
    }

    public struct StartProgram
    {
        public int Priority;
        public bool Enabled;
        public string Executable;
        public string Arguments;
        public bool ExecutableArgumentsRequired;
        public bool CloseOnFinish;
    }

    public struct Executable
    {
        public string DifferentExecutableToMonitor;
        public string ExecutableNameAndPath;
        public uint ExecutableTimeout;
        public string ExecutableArguments;
        public bool ExecutableArgumentsRequired;
        public bool ProcessNameToMonitorUsesExecutable;
    }

    public struct GameStruct
    {
        public Game GameToPlay;
        public uint StartTimeout;
        public string GameArguments;
        public bool GameArgumentsRequired;
    }

    public class ShortcutItem
    {
        
        //private static List<ShortcutItem> _allSavedShortcuts = new List<ShortcutItem>();
        //private MultiIcon _shortcutIcon, _originalIcon = null;
        //private string _savedShortcutIconCacheFilename = "";
        private string _profileUuid = "";
        private ProfileItem _profileToUse;
        private string _uuid = "";
        private string _name = "";
        private ShortcutCategory _category = ShortcutCategory.NoGame;
        private string _differentExecutableToMonitor;
        private string _executableNameAndPath = "";
        private string _executableArguments;
        private bool _executableArgumentsRequired;
        private bool _processNameToMonitorUsesExecutable;
        private uint _gameAppId;
        private string _gameName;
        private SupportedGameLibrary _gameLibrary;
        private uint _startTimeout;
        private string _gameArguments;
        private bool _gameArgumentsRequired;
        private ShortcutPermanence _permanence;
        private bool _autoName;
        private bool _isPossible;
        private List<StartProgram> _startPrograms;
        [JsonIgnore]
        public string _originalIconPath;
        private Bitmap _shortcutBitmap, _originalLargeBitmap, _originalSmallBitmap;
        [JsonIgnore]
        public string _savedShortcutIconCacheFilename;


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

        public ShortcutItem(
            string name,
            ProfileItem profile, 
            uint gameAppId,
            string gameName,
            SupportedGameLibrary gameLibrary,
            uint gameTimeout,
            string gameArguments,
            bool gameArgumentsRequired,
            ShortcutPermanence permanence,
            string originalIconPath,
            List<StartProgram> startPrograms = null,
            bool autoName = true,
            string uuid = ""
            ) : this()
        {
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name; 
            _profileToUse = profile;
            _category = ShortcutCategory.Game;
            _profileToUse = profile;
            _gameAppId = gameAppId;
            _gameName = gameName;
            _gameLibrary = gameLibrary;
            _startTimeout = gameTimeout;
            _gameArguments = gameArguments;
            _gameArgumentsRequired = gameArgumentsRequired;
            _permanence = permanence;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _originalIconPath = originalIconPath;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;

            // We create the OriginalLargeBitmap from the IconPath
            _originalLargeBitmap = ToLargeBitmap(_originalIconPath);
            _originalSmallBitmap = ToSmallBitmap(_originalIconPath);

            // We create the ShortcutBitmap from the OriginalBitmap 
            // (We only do it if there is a valid profile)
            if (_profileToUse is ProfileItem)
                _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);

        }

        public ShortcutItem(string name, ProfileItem profile, GameStruct game, ShortcutPermanence permanence, string originalIconPath,
            List<StartProgram> startPrograms = null, bool autoName = true, string uuid = "") : this()
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
            _permanence = permanence;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _originalIconPath = originalIconPath;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;

            // We create the OriginalBitmap from the IconPath
            _originalLargeBitmap = ToLargeBitmap(_originalIconPath);

            // We create the ShortcutBitmap from the OriginalBitmap 
            // (We only do it if there is a valid profile)
            if (_profileToUse is ProfileItem)
                _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
        }



        public ShortcutItem(string name, string profileUuid, GameStruct game, ShortcutPermanence permanence, string originalIconPath,
            List<StartProgram> startPrograms = null, bool autoName = true, string uuid = "") : this()
        {
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name;
            _profileUuid = profileUuid;
            _category = ShortcutCategory.Game;
            _gameAppId = game.GameToPlay.Id;
            _gameName = game.GameToPlay.Name;
            _gameLibrary = game.GameToPlay.GameLibrary;
            _startTimeout = game.StartTimeout;
            _gameArguments = game.GameArguments;
            _gameArgumentsRequired = game.GameArgumentsRequired;
            _gameArgumentsRequired = false;
            _permanence = permanence;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _originalIconPath = originalIconPath;

            // Now we need to find and populate the profileToUse
            foreach (ProfileItem profileToTest in ProfileRepository.AllProfiles)
            {
                if (profileToTest.UUID.Equals(_profileUuid,StringComparison.InvariantCultureIgnoreCase))
                {
                    _profileToUse = profileToTest;
                    break;
                }

            }

            if (_profileToUse == null)
            {
                throw new Exception($"Trying to create a ShortcutItem and cannot find a loaded profile with UUID {uuid}.");
            }

            // We create the OriginalBitmap from the IconPath
            _originalLargeBitmap = ToLargeBitmap(_originalIconPath);

            // We create the ShortcutBitmap from the OriginalBitmap 
            // (We only do it if there is a valid profile)
            if (_profileToUse is ProfileItem)
                _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
        }

        public ShortcutItem(
            string name,
            ProfileItem profile,
            string differentExecutableToMonitor,
            string executableNameAndPath,
            uint executableTimeout,
            string executableArguments,
            bool executableArgumentsRequired,
            bool processNameToMonitorUsesExecutable,
            ShortcutPermanence permanence,
            string originalIconPath,
            List<StartProgram> startPrograms = null,
            bool autoName = true,
            string uuid = ""
            ) : this()
        {
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name;
            _profileToUse = profile;
            _category = ShortcutCategory.Application;
            _differentExecutableToMonitor = differentExecutableToMonitor;
            _executableNameAndPath = executableNameAndPath;
            _startTimeout = executableTimeout;
            _executableArguments = executableArguments;
            _executableArgumentsRequired = executableArgumentsRequired;
            _processNameToMonitorUsesExecutable = processNameToMonitorUsesExecutable;
            _permanence = permanence;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _originalIconPath = originalIconPath;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;

            // We create the OriginalBitmap from the IconPath
            _originalLargeBitmap = ToLargeBitmap(_originalIconPath);

            // We create the ShortcutBitmap from the OriginalBitmap 
            // (We only do it if there is a valid profile)
            //if (_profileToUse is ProfileItem)
            //     _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
            _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);

        }

        public ShortcutItem(string name, ProfileItem profile, Executable executable, ShortcutPermanence permanence, string originalIconPath,
            List<StartProgram> startPrograms = null, bool autoName = true, string uuid = "") : this()
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
            _permanence = permanence;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _originalIconPath = originalIconPath;

            // Now we need to find and populate the profileUuid
            _profileUuid = profile.UUID;

            // We create the OriginalBitmap from the IconPath
            _originalLargeBitmap = ToLargeBitmap(_originalIconPath);

            // We create the ShortcutBitmap from the OriginalBitmap 
            // (We only do it if there is a valid profile)
            //if (_profileToUse is ProfileItem)
            //    _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
            _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
        }

        public ShortcutItem(string name, string profileUuid, Executable executable, ShortcutPermanence permanence, string originalIconPath,
            List<StartProgram> startPrograms = null, bool autoName = true, string uuid = "") : this()
        {
            if (!String.IsNullOrWhiteSpace(uuid))
                _uuid = uuid;
            _name = name;
            _profileUuid = profileUuid;
            _category = ShortcutCategory.Application;
            _differentExecutableToMonitor = executable.DifferentExecutableToMonitor;
            _executableNameAndPath = executable.ExecutableNameAndPath;
            _startTimeout = executable.ExecutableTimeout;
            _executableArguments = executable.ExecutableArguments;
            _executableArgumentsRequired = executable.ExecutableArgumentsRequired;
            _processNameToMonitorUsesExecutable = executable.ProcessNameToMonitorUsesExecutable;
            _permanence = permanence;
            _autoName = autoName;
            _startPrograms = startPrograms;
            _originalIconPath = originalIconPath;

            // Now we need to find and populate the profileToUse
            foreach (ProfileItem profileToTest in ProfileRepository.AllProfiles)
            {
                if (profileToTest.UUID.Equals(_profileUuid, StringComparison.InvariantCultureIgnoreCase))
                {
                    _profileToUse = profileToTest;
                    break;
                }

            }

            if (_profileToUse == null)
            {
                throw new Exception($"Trying to create a ShortcutItem and cannot find a loaded profile with UUID {uuid}.");
            }

            // We create the OriginalBitmap from the IconPath
            if (_originalIconPath.EndsWith(".ico"))
            {
                Icon icoIcon = new Icon(_originalIconPath, 256, 256);
                //_originalBitmap = ExtractVistaIcon(biggestIcon);
                _originalLargeBitmap = icoIcon.ToBitmap();
                icoIcon.Dispose();
            }
            else
            {
                _originalLargeBitmap = ToLargeBitmap(_originalIconPath);
            }
            _originalLargeBitmap = ToLargeBitmap(_originalIconPath);

            // We create the ShortcutBitmap from the OriginalBitmap 
            // (We only do it if there is a valid profile)
            //if (_profileToUse is ProfileItem)
            //    _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
            _shortcutBitmap = ToBitmapOverlay(_originalLargeBitmap, _profileToUse.ProfileTightestBitmap, 256, 256);
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
        public ProfileItem ProfileToUse {
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
                    if (profileToTest.UUID.Equals(_profileUuid, StringComparison.InvariantCultureIgnoreCase))
                        _profileToUse = profileToTest;
                }
            }
        }

        public ShortcutPermanence Permanence 
        { 
            get 
            {
                return _permanence;
            }

            set 
            {
                _permanence = value;
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

        public uint GameAppId
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

        public SupportedGameLibrary GameLibrary
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

        public uint StartTimeout
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
                return _originalLargeBitmap;
            }

            set
            {
                _originalLargeBitmap = value;

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
        public bool IsPossible
        {
            get
            {
                return _isPossible;
            }
            set
            {
                _isPossible = value;
            }
        }

        public  bool CopyTo (ShortcutItem shortcut, bool overwriteUUID = false)
        {
            if (!(shortcut is ShortcutItem))
                return false;

            if (overwriteUUID)
                shortcut.UUID = UUID;

            // Copy all the shortcut data over to the other Shortcut
            shortcut.Name = Name;
            shortcut.ProfileToUse = ProfileToUse;
            shortcut.ProfileUUID = ProfileUUID;
            shortcut.Permanence = Permanence;
            shortcut.Category = Category;
            shortcut.DifferentExecutableToMonitor = DifferentExecutableToMonitor;
            shortcut.ExecutableNameAndPath = ExecutableNameAndPath;
            shortcut.ExecutableArguments = ExecutableArguments;
            shortcut.ExecutableArgumentsRequired = ExecutableArgumentsRequired;
            shortcut.ProcessNameToMonitorUsesExecutable = ProcessNameToMonitorUsesExecutable;
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
            shortcut.IsPossible = IsPossible;
            shortcut.StartPrograms = StartPrograms;

            return true;
        }

        public void SaveShortcutIconToCache()
        {

            // Only add this set of options if the shortcut is to an standalone application
            if (_category == ShortcutCategory.Application)
            {
                // Work out the name of the shortcut we'll save.
                _savedShortcutIconCacheFilename = Path.Combine(Program.AppShortcutPath, String.Concat(@"executable-", _profileToUse.UUID, "-", Path.GetFileNameWithoutExtension(_executableNameAndPath), @".ico"));

            }
            else 
            {
                // Work out the name of the shortcut we'll save.
                _savedShortcutIconCacheFilename = Path.Combine(Program.AppShortcutPath, String.Concat(_gameLibrary.ToString().ToLower(CultureInfo.InvariantCulture),@"-", _profileToUse.UUID, "-", _gameAppId.ToString(), @".ico"));
            }

            MultiIcon shortcutIcon;
            try
            {
                //shortcutIcon = new ProfileIcon(shortcut.ProfileToUse).ToIconOverlay(shortcut.OriginalIconPath);
                shortcutIcon = ToIconOverlay();
                shortcutIcon.Save(_savedShortcutIconCacheFilename, MultiIconFormat.ICO);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ShortcutRepository/SaveShortcutIconToCache exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");

                // If we fail to create an icon based on the original executable or game
                // Then we use the standard HeliosPlus profile one.
                shortcutIcon = _profileToUse.ProfileIcon.ToIcon();
                shortcutIcon.Save(_savedShortcutIconCacheFilename, MultiIconFormat.ICO);
            }
        }

       /* public static Bitmap ExtractVistaIcon(Icon icoIcon)
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
            catch (Exception ex)
            {
                Console.WriteLine($"ShortcutItem/ExtractVisataIcon exception: {ex.Message}: {ex.StackTrace} - {ex.InnerException}");
                return null; 
            }
            return bmpPngExtracted;
        }*/

        /*        public Bitmap ToBitmap(int width = 256, int height = 256, PixelFormat format = PixelFormat.Format32bppArgb)
                {
                    var bitmap = new Bitmap(width, height, format);
                    bitmap.MakeTransparent();

                    using (var g = Graphics.FromImage(bitmap))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.DrawImage(g, width, height);
                    }

                    return bitmap;
                }*/

        private const string Shell32 = "shell32.dll";

        /*private Bitmap ToBitmapFromExe(string fileNameAndPath) 
        {
            if (String.IsNullOrWhiteSpace(fileNameAndPath))
                return null;

            *//*IconActions ia = new IconActions();

            int index = 0;
            var sb = new StringBuilder(fileNameAndPath, 500);
            IconReference iconReference = new IconReference(sb.ToString(), index);

            var largeIcons = new IntPtr[1];
            var smallIcons = new IntPtr[1];
            ia.ExtractIcon(iconReference.FilePath, iconReference.IconIndex, largeIcons, smallIcons, 1);

            System.Windows.

            BitmapSource bitmapSource;
            try
            {
                bitmapSource = Imaging.CreateBitmapSourceFromHIcon(largeIcons[0], Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            catch
            {
                return null;
            }

            ia.DestroyIconAtHandle(largeIcons[0]);
            ia.DestroyIconAtHandle(smallIcons[0]);

            return bitmapSource;
*//*
            //IconFromFile iconFromFile = new IconFromFile();
            Bitmap bm = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, true, true);
            return bm;

            //var icons = MintPlayer.IconUtils.IconExtractor.Split(fileNameAndPath);

            *//*var folder = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(fileNameAndPath), "Split");
            if (!System.IO.Directory.Exists(folder)) System.IO.Directory.CreateDirectory(folder);
            var index = 1;
            foreach (var icon in icons)
            {
                var filename = System.IO.Path.Combine(folder, "icon_" + (index++).ToString() + ".ico");
                using (var fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))
                {
                    icon.Save(fs);
                }
            }
*//*

            Icon ExeIcon = ExtractIcon.ExtractIconFromExecutable(fileNameAndPath);
            FileStream fs = new FileStream(fileNameAndPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            MultiIcon mi = new MultiIcon();
            mi.Load(fs);
            int count = mi.Count;
            TsudaKageyu.IconExtractor ie = new TsudaKageyu.IconExtractor(fileNameAndPath);
            Icon[] allIcons = ie.GetAllIcons();
            Icon biggestIcon = allIcons.OrderByDescending(item => item.Size).First();
            //_originalBitmap = ExtractVistaIcon(biggestIcon);
            Bitmap bitmapToReturn = IconUtil.ToBitmap(biggestIcon);
            if (bitmapToReturn == null)
                bitmapToReturn = biggestIcon.ToBitmap();

            // Only gets the 32x32 icon!
            //Icon exeIcon = IconUtils.ExtractIcon.ExtractIconFromExecutable(fileNameAndPath);
            //Bitmap bitmapToReturn = exeIcon.ToBitmap();
            //exeIcon.Dispose();
            return bitmapToReturn;
        }*/

/*        public static BitmapSource ConvertBitmap(Bitmap source)
        {
            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                          source.GetHbitmap(),
                          IntPtr.Zero,
                          Int32Rect.Empty,
                          BitmapSizeOptions.FromEmptyOptions());
        }*/

        /*public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }*/
/*
        public Bitmap ToBitmapFromIcon(string fileNameAndPath)
        {
            if (String.IsNullOrWhiteSpace(fileNameAndPath))
                return null;
            Icon icoIcon = new Icon(fileNameAndPath, 256, 256);
            //_originalBitmap = ExtractVistaIcon(biggestIcon);
            Bitmap bitmapToReturn = icoIcon.ToBitmap();
            icoIcon.Dispose();
            return bitmapToReturn;
        }*/

        public static Bitmap ToLargeBitmap(string fileNameAndPath)
        {
            if (String.IsNullOrWhiteSpace(fileNameAndPath))
                return null;

            Bitmap bm = null
;            if (fileNameAndPath.EndsWith(".ico"))
            {
                Icon icoIcon = new Icon(fileNameAndPath, 256, 256);
                bm = icoIcon.ToBitmap();
                icoIcon.Dispose();
            }
            else
            {
                bm = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, true, true);
            }           
            return bm;
        }

        public static Bitmap ToSmallBitmap(string fileNameAndPath)
        {
            if (String.IsNullOrWhiteSpace(fileNameAndPath))
                return null;

            Bitmap bm = null; 
            if (fileNameAndPath.EndsWith(".ico"))
            {
                Icon icoIcon = new Icon(fileNameAndPath, 128, 128);
                bm = icoIcon.ToBitmap();
                icoIcon.Dispose();
            }
            else
            {
                bm = IconFromFile.GetSmallBitmapFromFile(fileNameAndPath, false, true, false);
            }
            return bm;
        }


        public Bitmap ToBitmapOverlay(Bitmap originalBitmap, Bitmap overlayBitmap, int width, int height, PixelFormat format = PixelFormat.Format32bppArgb)
        {

            if (!(width is int) || width <= 0)
                return null;

            if (!(height is int) || height <= 0)
                return null;

            // Figure out sizes and positions
            Size targetSize = new Size(width,height);
            Size originalBitmapCurrentSize = new Size(originalBitmap.Width, originalBitmap.Height);
            Size overlaylBitmapCurrentSize = new Size(overlayBitmap.Width, overlayBitmap.Height);

            // Make a new empty bitmap of the wanted size
            var combinedBitmap = new Bitmap(targetSize.Width, targetSize.Height, format);
            combinedBitmap.MakeTransparent();

            using (var g = Graphics.FromImage(combinedBitmap))
            {

                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.AssumeLinear;

                // Resize the originalBitmap if needed then draw it
                Size originalBitmapNewSize = ResizeDrawing.FitWithin(originalBitmapCurrentSize, targetSize);
                Point originalBitmapNewLocation = ResizeDrawing.AlignCenter(originalBitmapNewSize, targetSize);
                g.DrawImage(originalBitmap, originalBitmapNewLocation.X, originalBitmapNewLocation.Y, originalBitmapNewSize.Width, originalBitmapNewSize.Height);

                // Resize the overlayBitmap if needed then draw it in the bottom-right corner
                Size overlayBitmapMaxSize = ResizeDrawing.FitWithin(overlaylBitmapCurrentSize, targetSize);
                Size overlayBitmapNewSize = ResizeDrawing.MakeSmaller(overlayBitmapMaxSize,70);
                Point overlayBitmapNewLocation = ResizeDrawing.AlignBottomRight(overlayBitmapNewSize, targetSize);

                g.DrawImage(overlayBitmap, overlayBitmapNewLocation.X, overlayBitmapNewLocation.Y, overlayBitmapNewSize.Width, overlayBitmapNewSize.Height);

            }
            return combinedBitmap;
        }

        /*public MultiIcon ToIcon()
        {
            var iconSizes = new[]
            {
                new Size(256, 256),
                new Size(64, 64),
                new Size(48, 48),
                new Size(32, 32),
                new Size(24, 24),
                new Size(16, 16)
            };
            var multiIcon = new MultiIcon();
            var icon = multiIcon.Add("Icon1");

            foreach (var size in iconSizes)
            {
                Bitmap bitmap = new Bitmap(size.Width, size.Height);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.SmoothingMode = SmoothingMode.None;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.AssumeLinear;
                    g.DrawImage(_originalLargeBitmap, new Rectangle(0, 0, size.Width, size.Height));
                }

                icon.Add(bitmap);

                if (size.Width >= 256 && size.Height >= 256)
                {
                    icon[icon.Count - 1].IconImageFormat = IconImageFormat.PNG;
                }
                bitmap.Dispose();
            }

            multiIcon.SelectedIndex = 0;

            return multiIcon;
        }*/

        public MultiIcon ToIconOverlay()
        {
            var iconSizes = new[]
            {
                new Size(256, 256),
                new Size(64, 64),
                new Size(48, 48),
                new Size(32, 32),
                new Size(24, 24),
                new Size(16, 16)
            };
            var multiIcon = new MultiIcon();
            var icon = multiIcon.Add("Icon1");

            foreach (var size in iconSizes)
            {
                Bitmap bitmapOverlay = ToBitmapOverlay(_originalLargeBitmap, ProfileToUse.ProfileTightestBitmap, size.Width, size.Height);
                icon.Add(bitmapOverlay);

                if (size.Width >= 256 && size.Height >= 256)
                {
                    icon[icon.Count - 1].IconImageFormat = IconImageFormat.PNG;
                }

                bitmapOverlay.Dispose();
            }

            multiIcon.SelectedIndex = 0;

            return multiIcon;
        }

        /*internal static class ExtractIcon
        {
            [UnmanagedFunctionPointer(CallingConvention.Winapi, SetLastError = true, CharSet = CharSet.Unicode)]
            //[SuppressUnmanagedCodeSecurity]
            internal delegate bool ENUMRESNAMEPROC(IntPtr hModule, IntPtr lpszType, IntPtr lpszName, IntPtr lParam);
            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, uint dwFlags);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr FindResource(IntPtr hModule, IntPtr lpName, IntPtr lpType);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr LoadResource(IntPtr hModule, IntPtr hResInfo);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern IntPtr LockResource(IntPtr hResData);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern uint SizeofResource(IntPtr hModule, IntPtr hResInfo);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            //[SuppressUnmanagedCodeSecurity]
            public static extern bool EnumResourceNames(IntPtr hModule, IntPtr lpszType, ENUMRESNAMEPROC lpEnumFunc, IntPtr lParam);


            private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
            private readonly static IntPtr RT_ICON = (IntPtr)3;
            private readonly static IntPtr RT_GROUP_ICON = (IntPtr)14;

            public static Icon ExtractIconFromExecutable(string path)
            {
                IntPtr hModule = LoadLibraryEx(path, IntPtr.Zero, LOAD_LIBRARY_AS_DATAFILE);
                var tmpData = new List<byte[]>();

                ENUMRESNAMEPROC callback = (h, t, name, l) =>
                {
                    var dir = GetDataFromResource(hModule, RT_GROUP_ICON, name);

                    // Calculate the size of an entire .icon file.

                    int count = BitConverter.ToUInt16(dir, 4);  // GRPICONDIR.idCount
                    int len = 6 + 16 * count;                   // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count
                    for (int i = 0; i < count; ++i)
                        len += BitConverter.ToInt32(dir, 6 + 14 * i + 8);   // GRPICONDIRENTRY.dwBytesInRes

                    using (var dst = new BinaryWriter(new MemoryStream(len)))
                    {
                        // Copy GRPICONDIR to ICONDIR.

                        dst.Write(dir, 0, 6);

                        int picOffset = 6 + 16 * count; // sizeof(ICONDIR) + sizeof(ICONDIRENTRY) * count

                        for (int i = 0; i < count; ++i)
                        {
                            // Load the picture.

                            ushort id = BitConverter.ToUInt16(dir, 6 + 14 * i + 12);    // GRPICONDIRENTRY.nID
                            var pic = GetDataFromResource(hModule, RT_ICON, (IntPtr)id);

                            // Copy GRPICONDIRENTRY to ICONDIRENTRY.

                            dst.Seek(6 + 16 * i, 0);

                            dst.Write(dir, 6 + 14 * i, 8);  // First 8bytes are identical.
                            dst.Write(pic.Length);          // ICONDIRENTRY.dwBytesInRes
                            dst.Write(picOffset);           // ICONDIRENTRY.dwImageOffset

                            // Copy a picture.

                            dst.Seek(picOffset, 0);
                            dst.Write(pic, 0, pic.Length);

                            picOffset += pic.Length;
                        }

                        tmpData.Add(((MemoryStream)dst.BaseStream).ToArray());
                    }
                    return true;
                };
                EnumResourceNames(hModule, RT_GROUP_ICON, callback, IntPtr.Zero);
                byte[][] iconData = tmpData.ToArray();
                using (var ms = new MemoryStream(iconData[0]))
                {
                    return new Icon(ms);
                }
            }
            private static byte[] GetDataFromResource(IntPtr hModule, IntPtr type, IntPtr name)
            {
                // Load the binary data from the specified resource.

                IntPtr hResInfo = FindResource(hModule, name, type);

                IntPtr hResData = LoadResource(hModule, hResInfo);

                IntPtr pResData = LockResource(hResData);

                uint size = SizeofResource(hModule, hResInfo);

                byte[] buf = new byte[size];
                Marshal.Copy(pResData, buf, 0, buf.Length);

                return buf;
            }
        }
*/

        public (bool,string) IsValid()
        {
            // Do some validation checks to make sure the shortcut is sensible
            // And that we have enough to try and action within the shortcut
            // (in other words check everything in the shortcut is still valid)

            // Does the profile we want to Use still exist?
            // Is the profile still valid right now? i.e. are all the screens available?
            if (!ProfileToUse.IsPossible)
            {
                return (false,string.Format("The profile '{0}' is not valid right now and cannot be used.",ProfileToUse.Name));
            }
            // Is the main application still installed?
            if (Category.Equals(ShortcutCategory.Application))
            {
                // We need to check if the Application still exists
                if (!System.IO.File.Exists(ExecutableNameAndPath))
                {
                    return (false, string.Format("The application executable '{0}' does not exist, or cannot be accessed by HeliosPlus.", ExecutableNameAndPath));
                }

            } else if (Category.Equals(ShortcutCategory.Game))
            {
                // If the game is a Steam Game we check for that
                if (GameLibrary.Equals(SupportedGameLibrary.Steam))
                {

                    // First check if Steam is installed
                    // Check if Steam is installed and error if it isn't
                    if (!SteamLibrary.IsSteamInstalled)
                    {
                        return (false, Language.Steam_executable_file_not_found);
                    }

                    // We need to look up details about the game
                    if (!SteamLibrary.ContainsSteamGame(GameAppId))
                    {
                        return (false, string.Format("The Steam Game with AppID '{0}' is not installed on this computer.", GameAppId));
                    }
                }
                // If the game is a Uplay Game we check for that
                /*else if (GameLibrary.Equals(SupportedGameLibrary.Uplay))
                {
                    // We need to look up details about the game
                    if (!UplayGame.IsInstalled(GameAppId))
                    {
                        return (false, string.Format("The Uplay Game with AppID '{0}' is not installed on this computer.", GameAppId));
                    }

                }*/


            }
            // Do all the specified pre-start apps still exist?

            return (true, "Shortcut is valid");

        }

        // ReSharper disable once FunctionComplexityOverflow
        // ReSharper disable once CyclomaticComplexity
        public bool CreateShortcut(string shortcutFileName)
        {
            string programName = Path.GetFileNameWithoutExtension(ExecutableNameAndPath);
            string shortcutDescription = string.Empty;
            string shortcutIconFileName = string.Empty;

            var shortcutArgs = new List<string>
            {
                // Add the SwitchProfile command as the first argument to start to switch to another profile
                $"{HeliosStartupAction.RunShortcut}",
                $"\"{UUID}\""
            };

            // Only add the rest of the options if the permanence is temporary
            if (Permanence == ShortcutPermanence.Temporary)
            {
                // Only add this set of options if the shortcut is to an standalone application
                if (Category == ShortcutCategory.Application)
                {
                    // Prepare text for the shortcut description field
                    shortcutDescription = string.Format(Language.Execute_application_with_profile, programName, ProfileToUse.Name);

                }
                // Only add the rest of the options if the temporary switch radio button is set
                // and if the game launching radio button is set
                else if (Permanence == ShortcutPermanence.Temporary)
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

                    // Actually create the shortcut!
                    //var wshShellType = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
                    //dynamic wshShell = Activator.CreateInstance(wshShellType);
                    

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
                    Console.WriteLine($"ShortcutItem/CreateShortcut exception (deleting old shortcut)");
                   
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
                if (Category.Equals(ShortcutCategory.NoGame))
                {
                    if (Permanence.Equals(ShortcutPermanence.Permanent))
                        _name = $"{_profileToUse.Name}";
                    else if (Permanence.Equals(ShortcutPermanence.Temporary))
                        _name = $"{_profileToUse.Name} (Temporary)";
                }
                else if (Category.Equals(ShortcutCategory.Game) && GameName.Length > 0)
                {
                    _name = $"{GameName} ({_profileToUse.Name})";
                }
                else if (Category.Equals(ShortcutCategory.Application) && ExecutableNameAndPath.Length > 0)
                {
                    string baseName = Path.GetFileNameWithoutExtension(ExecutableNameAndPath);
                    _name = $"{baseName} ({_profileToUse.Name})";
                }
            }
        }

    }

    /*internal class IconActions
    {
        //  Constants
        //  =========

        private const string Shell32 = "shell32.dll";
        private const string User32 = "user32.dll";

        //  External Methods
        //  ================

        [DllImport(Shell32, CharSet = CharSet.Auto)]
        private static extern int PickIconDlg(IntPtr hwndOwner, StringBuilder lpstrFile, int nMaxFile, ref int lpdwIconIndex);

        [DllImport(Shell32, CharSet = CharSet.Auto)]
        private static extern uint ExtractIconEx(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);

        [DllImport(User32, CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        //  Methods
        //  =======

        public int PickIconDialog(IntPtr hwndOwner, StringBuilder lpstrFile, int nMaxFile, ref int lpdwIconIndex)
        {
            return PickIconDlg(hwndOwner, lpstrFile, nMaxFile, ref lpdwIconIndex);
        }

        public uint ExtractIcon(string szFileName, int nIconIndex, IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons)
        {
            return ExtractIconEx(szFileName, nIconIndex, phiconLarge, phiconSmall, nIcons);
        }

        public bool DestroyIconAtHandle(IntPtr handle)
        {
            return DestroyIcon(handle);
        }
    }

    public class IconReference
    {
        //  Constants
        //  =========

        private const string comma = ",";

        //  Variables
        //  =========

        private static readonly Regex regex = new Regex(@".+\,[0-9]+$");

        //  Properties
        //  ==========

        /// <summary>
        /// File path to the icon.
        /// </summary>
        public string FilePath { get; private set; }

        /// <summary>
        /// Index of the icon within the file.
        /// </summary>
        public int IconIndex { get; private set; }

        //  Constructors
        //  ============

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="reference">A reference for an icon within either an .ico, .exe, or .dll. Must be a valid file location followed by a comma and then an int.</param>
        /// <exception cref="RegexMatchTimeoutException">Ignore.</exception>
        public IconReference(string reference)
        {
            if (!regex.IsMatch(reference))
            {
                throw new ArgumentException("[reference] must be a valid file location followed by a comma and then an int");
            }

            string[] split = reference.Split(',');
            string index = split[split.Length - 1];
            string filePath = reference.Substring(0, reference.Length - index.Length - 1);

            Setup(filePath, index);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">A valid file location for an .ico, .exe, or .dll.</param>
        /// <param name="index">The index of the icon wanted within the file.</param>
        public IconReference(string filePath, string index)
        {
            Setup(filePath, index);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="filePath">A valid file location for an .ico, .exe, or .dll.</param>
        /// <param name="index">The index of the icon wanted within the file.</param>
        public IconReference(string filePath, int index)
        {
            Setup(filePath, index);
        }

        /// <summary>
        /// Returns the FileName and the IconIndex separated by a comma
        /// </summary>
        /// <returns>Returns the FileName and the IconIndex separated by a comma</returns>
        public override string ToString()
        {
            return (FilePath ?? string.Empty) + comma + (IconIndex.ToString() ?? string.Empty);
        }

        private void Setup(string filepath, string index)
        {
            if (!int.TryParse(index, out int iconIndex))
            {
                throw new ArgumentException("Parameter [index] needs to be castable to an integer");
            }

            Setup(filepath, iconIndex);
        }

        private void Setup(string filepath, int index)
        {
            if (index < 0)
            {
                throw new ArgumentException("Parameter [index] needs to be greater than or equal to zero");
            }

            FilePath = filepath;
            IconIndex = index;
        }
    }*/


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
            MemoryStream memoryStream = new MemoryStream(byteBuffer);
            memoryStream.Position = 0;

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
