using HeliosPlus.GameLibraries;
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
using System.Text;
using System.Threading.Tasks;

namespace HeliosPlus
{
    class Shortcut
    {

        private static List<Shortcut> _allSavedShortcuts = new List<Shortcut>();
        private MultiIcon _shortcutIcon, _originalIcon = null;
        private Bitmap _shortcutBitmap, _originalBitmap = null;

        public Shortcut( Profile profile)
        {
            ProfileToUse = profile;
        }

        public static Version Version = new Version(1, 0);

        public string Name { get; set; } = "Current Display Profile";

        public Profile ProfileToUse { get; set; } = null;

        public string ProcessNameToMonitor { get; set; } = "";

        public string ExecutableNameAndPath { get; set; } = "";

        public uint ExecutableTimeout { get; set; } = 0;

        public string ExecutableArguments { get; set; } = "";

        public uint GameAppId { get; set; } = 0;

        public string GameName { get; set; } = "";

        public SupportedGameLibrary GameLibrary { get; set; } = SupportedGameLibrary.Unknown;

        public uint GameTimeout { get; set; } = 0;

        public string GameArguments { get; set; } = "";

        public string OriginalIconPath { get; set; } = "";

        [JsonConverter(typeof(CustomBitmapConverter))]
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

        [JsonConverter(typeof(CustomBitmapConverter))]
        public Bitmap ShortcutBitmap
        {
            get
            {
                if (_shortcutBitmap != null)
                    return _shortcutBitmap;
                else
                {
                    if (OriginalBitmap == null)
                        return null;

                    _shortcutBitmap = new ProfileIcon(ProfileToUse).ToBitmapOverly(OriginalBitmap);
                    return _shortcutBitmap;
                }
            }

            set
            {
                _originalBitmap = value;
            }
        }

        public static string SavedShortcutsFilePath
        {
            get => Path.Combine(Program.AppDataPath, $"Shortcuts\\Shortcuts_{Version.ToString(2)}.json");
        }

        public static string SavedShortcutsPath
        {
            get => Path.Combine(Program.AppDataPath, $"Shortcuts");
        }

        public string SavedShortcutIconCacheFilename { get; set; }

        [JsonIgnore]
        public static List<Shortcut> AllSavedShortcuts
        {
            get => _allSavedShortcuts;
        }

        [JsonIgnore]
        public bool IsPossible
        {
            get
            {
                if (ProfileToUse != null)
                    return ProfileToUse.IsPossible;
                else
                    return false;
            }
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

        public void SaveShortcutIconToCache()
        {
            if (_shortcutIcon == null)
            {
                // Work out the name of the shortcut we'll save.
                SavedShortcutIconCacheFilename = Path.Combine(Program.ShortcutIconCachePath, String.Concat(GetValidFilename(Name).ToLower(CultureInfo.InvariantCulture), @".ico"));

                try
                {
                    _shortcutIcon = new ProfileIcon(ProfileToUse).ToIconOverly(OriginalIconPath);
                    _shortcutIcon.Save(SavedShortcutIconCacheFilename, MultiIconFormat.ICO);
                }
                catch (Exception ex)
                {
                    // If we fail to create an icon based on the original executable or game
                    // Then we use the standard HeliosPlus profile one.
                    _shortcutIcon = new ProfileIcon(ProfileToUse).ToIcon();
                    _shortcutIcon.Save(SavedShortcutIconCacheFilename, MultiIconFormat.ICO);
                }
            }
        }

        public static List<Shortcut> LoadAllShortcuts()
        {

            if (File.Exists(SavedShortcutsFilePath))
            {
                var json = File.ReadAllText(SavedShortcutsFilePath, Encoding.Unicode);

                if (!string.IsNullOrWhiteSpace(json))
                {
                    List<Shortcut> shortcuts = new List<Shortcut>();
                    try
                    {
                        shortcuts = JsonConvert.DeserializeObject<List<Shortcut>>(json, new JsonSerializerSettings
                        {
                            MissingMemberHandling = MissingMemberHandling.Ignore,
                            NullValueHandling = NullValueHandling.Ignore,
                            DefaultValueHandling = DefaultValueHandling.Include,
                            TypeNameHandling = TypeNameHandling.Auto
                        });
                    }
                    catch (Exception ex)
                    {
                        // ignored
                        Console.WriteLine("Unable to deserialize shortcut: " + ex.Message);
                    }

                    _allSavedShortcuts = shortcuts;

                    return _allSavedShortcuts;
                }
            }

            // If we get here, then we don't have any shortcuts saved!
            // So we gotta start from scratch
            // Create a new empty list of all our display profiles as we don't have any saved!
            _allSavedShortcuts = new List<Shortcut>();

            return _allSavedShortcuts;
        }

        public static bool SaveAllShortcuts()
        {
            if (SaveAllShortcuts(_allSavedShortcuts))
                return true;
            return false;
        }

        public static bool SaveAllShortcuts(List<Shortcut> shortcutsToSave)
        {

            if (!Directory.Exists(SavedShortcutsPath))
            {
                try
                {
                    Directory.CreateDirectory(SavedShortcutsPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to create Shortcut folder " + SavedShortcutsPath + ": " + ex.Message);
                }
            }


            // Now we loop over the profiles and save their images for later
            foreach (Shortcut shortcutToSave in shortcutsToSave)
                shortcutToSave.SaveShortcutIconToCache();

            try
            {
                var json = JsonConvert.SerializeObject(shortcutsToSave, Formatting.Indented, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    DefaultValueHandling = DefaultValueHandling.Populate,
                    TypeNameHandling = TypeNameHandling.Auto

                });


                if (!string.IsNullOrWhiteSpace(json))
                {
                    var dir = Path.GetDirectoryName(SavedShortcutsPath);

                    if (dir != null)
                    {
                        Directory.CreateDirectory(dir);
                        File.WriteAllText(SavedShortcutsFilePath, json, Encoding.Unicode);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to serialize profile: " + ex.Message);
            }

            // Overwrite the list of saved profiles as the new lot we received.
            _allSavedShortcuts = shortcutsToSave;

            return false;
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
