using HeliosPlus.GameLibraries;
using HeliosPlus.Resources;
using HeliosPlus.Shared;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.IconLib;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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

    public class Shortcut
    {
        
        private static List<Shortcut> _allSavedShortcuts = new List<Shortcut>();
        private MultiIcon _shortcutIcon, _originalIcon = null;
        private Bitmap _shortcutBitmap, _originalBitmap = null;
        private Profile _profileToUse = null;
        private uint _id = 0;
        private string _profileName = "";
        private bool _isPossible = false;

        public Shortcut()
        {
        }

        public Shortcut(Profile profile) : this()
        {
            ProfileToUse = profile;
        }

        public static Version Version = new Version(1, 0);

        public uint Id
        {
            get
            {
                if (_id == 0)
                    _id = ShortcutRepository.GetNextAvailableShortcutId();
                return _id;
            }
            set
            {
                _id = value;
            }
        }
        
        public string Name { get; set; } = "";

        [JsonIgnore]
        public Profile ProfileToUse { get; set; } = null;

        public string ProfileName { 
            get 
            {
                if (ProfileToUse is Profile)
                    _profileName = ProfileToUse.Name;
                return _profileName;
            }
            set
            {
                _profileName = value;
            }
        }

        public ShortcutPermanence Permanence { get; set; } = ShortcutPermanence.Temporary;

        public ShortcutCategory Category { get; set; } = ShortcutCategory.Game;

        public string DifferentExecutableToMonitor { get; set; } = "";

        public string ExecutableNameAndPath { get; set; } = "";

        public uint ExecutableTimeout { get; set; } = 30;

        public string ExecutableArguments { get; set; } = "";

        public bool ExecutableArgumentsRequired { get; set; } = false;

        public bool ProcessNameToMonitorUsesExecutable { get; set; } = true;

        public uint GameAppId { get; set; } = 0;

        public string GameName { get; set; } = "";

        public SupportedGameLibrary GameLibrary { get; set; } = SupportedGameLibrary.Unknown;

        public uint GameTimeout { get; set; } = 30;

        public string GameArguments { get; set; } = "";

        public bool GameArgumentsRequired { get; set; } = false;

        public string OriginalIconPath { get; set; } = "";

        //[JsonConverter(typeof(CustomBitmapConverter))]
        [JsonIgnore]
        public Bitmap OriginalBitmap
        {
            get
            {
                if (_originalBitmap != null)
                    return _originalBitmap;
                else
                {
                    if (String.IsNullOrEmpty(OriginalIconPath))
                        return null;
                    Icon icoAppIcon = Icon.ExtractAssociatedIcon(OriginalIconPath);
                    // We first try high quality icons
                    _originalBitmap = ExtractVistaIcon(icoAppIcon);
                    if (_originalBitmap == null)
                        _originalBitmap = icoAppIcon.ToBitmap();
                    return _originalBitmap;
                }
            }

            set
            {
                _originalBitmap = value;
            }
        }

        //[JsonConverter(typeof(CustomBitmapConverter))]
        [JsonIgnore]
        public Bitmap ShortcutBitmap
        {
            get
            {
                if (_shortcutBitmap != null)
                    return _shortcutBitmap;
                else
                {

                    if (ProfileToUse == null)
                        return null;

                    if (OriginalBitmap == null)
                        return null;

                    _shortcutBitmap = new ProfileIcon(ProfileToUse).ToBitmapOverlay(OriginalBitmap,128 ,128);
                    return _shortcutBitmap;
                }
            }

            set
            {
                _originalBitmap = value;
            }
        }

        public string SavedShortcutIconCacheFilename { get; set; }


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


        public  bool CopyTo (Shortcut shortcut, bool overwriteId = false)
        {
            if (!(shortcut is Shortcut))
                return false;

            if (overwriteId)
                shortcut.Id = Id;

            // Copy all the shortcut data over to the other Shortcut
            shortcut.Name = Name;
            shortcut.ProfileToUse = ProfileToUse;
            shortcut.ProfileName = ProfileName;
            shortcut.Permanence = Permanence;
            shortcut.Category = Category;
            shortcut.DifferentExecutableToMonitor = DifferentExecutableToMonitor;
            shortcut.ExecutableNameAndPath = ExecutableNameAndPath;
            shortcut.ExecutableTimeout = ExecutableTimeout;
            shortcut.ExecutableArguments = ExecutableArguments;
            shortcut.ExecutableArgumentsRequired = ExecutableArgumentsRequired;
            shortcut.ProcessNameToMonitorUsesExecutable = ProcessNameToMonitorUsesExecutable;
            shortcut.GameAppId = GameAppId;
            shortcut.GameName = GameName;
            shortcut.GameLibrary = GameLibrary;
            shortcut.GameTimeout = GameTimeout;
            shortcut.GameArguments = GameArguments;
            shortcut.GameArgumentsRequired = GameArgumentsRequired;
            shortcut.OriginalIconPath = OriginalIconPath;
            shortcut.OriginalBitmap = OriginalBitmap;
            shortcut.ShortcutBitmap = ShortcutBitmap;
            shortcut.SavedShortcutIconCacheFilename = SavedShortcutIconCacheFilename;
            shortcut.IsPossible = IsPossible;

            return true;
        }


        public static Bitmap ExtractVistaIcon(Icon icoIcon)
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
                $"{HeliosStartupAction.SwitchProfile}",
                $"\"{Name}\""
            };

            // Only add the rest of the options if the permanence is temporary
            if (Permanence == ShortcutPermanence.Temporary)
            {
                // Only add this set of options if the shortcut is to an standalone application
                if (Category == ShortcutCategory.Application)
                {
                    // Prepare text for the shortcut description field
                    shortcutDescription = string.Format(Language.Executing_application_with_profile, programName, Name);

                }
                // Only add the rest of the options if the temporary switch radio button is set
                // and if the game launching radio button is set
                else if (Permanence == ShortcutPermanence.Temporary)
                {
                    // Prepare text for the shortcut description field
                    shortcutDescription = string.Format(Language.Executing_application_with_profile, GameName, Name);
                }

            }
            // Only add the rest of the options if the permanent switch radio button is set
            else
            {
                // Prepare text for the shortcut description field
                shortcutDescription = string.Format(Language.Switching_display_profile_to_profile, Name);
            }

            // Now we are ready to create a shortcut based on the filename the user gave us
            shortcutFileName = Path.ChangeExtension(shortcutFileName, @"lnk");

            // If the user supplied a file
            if (shortcutFileName != null)
            {
                try
                {
                    // Remove the old file to replace it
                    if (File.Exists(shortcutFileName))
                    {
                        File.Delete(shortcutFileName);
                    }

                    // Actually create the shortcut!
                    var wshShellType = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
                    dynamic wshShell = Activator.CreateInstance(wshShellType);

                    try
                    {
                        var shortcut = wshShell.CreateShortcut(shortcutFileName);

                        try
                        {
                            shortcut.TargetPath = Application.ExecutablePath;
                            shortcut.Arguments = string.Join(" ", shortcutArgs);
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
                    if (File.Exists(shortcutFileName))
                    {
                        File.Delete(shortcutFileName);
                    }
                }
            }

            // Return a status on how it went
            // true if it was a success or false if it was not
            return shortcutFileName != null && File.Exists(shortcutFileName);
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
