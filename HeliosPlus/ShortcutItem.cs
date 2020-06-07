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

    public class ShortcutItem
    {
        
        private static List<ShortcutItem> _allSavedShortcuts = new List<ShortcutItem>();
        private MultiIcon _shortcutIcon, _originalIcon = null;
        private Bitmap _shortcutBitmap, _originalBitmap = null;
        private ProfileItem _profileToUse = null;
        private string _originalIconPath = "";
        private uint _id = 0;
        private string _profileName = "";
        private bool _isPossible = false;

        public ShortcutItem()
        {
        }

        public ShortcutItem(ProfileItem profile) : this()
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
                    _profileName = _profileToUse.Name;
                    // And if we have the _originalBitmap we can also save the Bitmap overlay, but only if the ProfileToUse is set
                    if (_originalBitmap is Bitmap)
                        _shortcutBitmap = ToBitmapOverlay(_originalBitmap, ProfileToUse.ProfileTightestBitmap,256,256);
                }
            }
        }

        public string ProfileName { 
            get 
            {
                return _profileName;
            }
            set
            {
                _profileName = value;

                // We try to find and set the ProfileTouse
                foreach (ProfileItem profileToTest in ProfileItem.AllSavedProfiles)
                {
                    if (profileToTest.Name.Equals(_profileName))
                        _profileToUse = profileToTest;
                }
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

        public string OriginalIconPath {
            get
            {
                if (String.IsNullOrEmpty(_originalIconPath))
                    return null;

                return _originalIconPath;
            }

            set
            {
                _originalIconPath = value;

                // We now force creation of the bitmap
                // straight away, so we know it has already been done.
                _originalBitmap = ToBitmapFromIcon(_originalIconPath);

                // And we do the same for the Bitmap overlay, but only if the ProfileToUse is set
                if (ProfileToUse is ProfileItem)
                    _shortcutBitmap = ToBitmapOverlay(_originalBitmap, ProfileToUse.ProfileTightestBitmap, 256, 256);
            }
        }

        //[JsonConverter(typeof(CustomBitmapConverter))]
        [JsonIgnore]
        public Bitmap OriginalBitmap
        {
            get
            {
                if (_originalBitmap is Bitmap)
                    return _originalBitmap;
                else
                {
                    if (String.IsNullOrEmpty(OriginalIconPath))
                        return null;

                    return ToBitmapFromIcon(OriginalIconPath);
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
                if (_shortcutBitmap is Bitmap)
                    return _shortcutBitmap;
                else
                {

                    if (ProfileToUse == null)
                        return null;

                    if (OriginalBitmap == null)
                        return null;

                    //_shortcutBitmap = new ProfileIcon(ProfileToUse).ToBitmapOverlay(OriginalBitmap,128 ,128);
                    _shortcutBitmap = ToBitmapOverlay(_originalBitmap, ProfileToUse.ProfileTightestBitmap, 256, 256);
                    _shortcutBitmap.Save(Path.Combine(Program.AppDataPath, @"ShortcutOverlay.png"), ImageFormat.Png);
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

        public  bool CopyTo (ShortcutItem shortcut, bool overwriteId = false)
        {
            if (!(shortcut is ShortcutItem))
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

        private Bitmap ToBitmapFromExe(string fileNameAndPath) 
        {
            /*            IconExtractor ie = new IconExtractor(fileNameAndPath);
                        Icon[] allIcons = ie.GetAllIcons();
                        Icon biggestIcon = allIcons.OrderByDescending(item => item.Size).First();
                        //_originalBitmap = ExtractVistaIcon(biggestIcon);
                        Bitmap bitmapToReturn = IconUtil.ToBitmap(biggestIcon);
                        if (bitmapToReturn == null)
                            bitmapToReturn = biggestIcon.ToBitmap();
                        return bitmapToReturn;
            */

            Icon exeIcon = IconUtils.ExtractIcon.ExtractIconFromExecutable(fileNameAndPath);
            Bitmap bitmapToReturn = exeIcon.ToBitmap();
            exeIcon.Dispose();
            return bitmapToReturn;
        }

        private Bitmap ToBitmapFromIcon(string fileNameAndPath)
        {
            Icon icoIcon = new Icon(fileNameAndPath, 256, 256);
            //_originalBitmap = ExtractVistaIcon(biggestIcon);
            Bitmap bitmapToReturn = icoIcon.ToBitmap();
            icoIcon.Dispose();
            return bitmapToReturn;
        }


        private Bitmap ToBitmapOverlay(Bitmap originalBitmap, Bitmap overlayBitmap, int width, int height, PixelFormat format = PixelFormat.Format32bppArgb)
        {

            if (!(width is int) || width <= 0)
                return null;

            if (!(height is int) || height <= 0)
                return null;

            // Make a new empoty bitmap of the wanted size
            var combinedBitmap = new Bitmap(width, height, format);
            combinedBitmap.MakeTransparent();

            using (var g = Graphics.FromImage(combinedBitmap))
            {

                g.SmoothingMode = SmoothingMode.None;
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.AssumeLinear;

                // Resize the originalBitmap if needed
                if (originalBitmap.Width > width || originalBitmap.Height > height)
                {


                    float originalBitmapRatio = (float) originalBitmap.Width / (float) originalBitmap.Height;
                    float newWidth, newHeight, newX, newY;
                    if (originalBitmap.Width > width)
                    {
                        // We need to shrink down until Height fits
                        newWidth = width;
                        newHeight = originalBitmap.Height * originalBitmapRatio;
                        newX = 0;
                        newY = (height - newHeight) / 2; 
                    } else
                    {
                        // We need to shrink down until Width fits
                        newWidth = originalBitmap.Width * originalBitmapRatio;
                        newHeight = height;
                        newX = (width - newWidth) / 2;
                        newY = 0;
                    }
                    g.DrawImage(originalBitmap, newX, newY, newWidth, newHeight);
                }
                else
                    g.DrawImage(originalBitmap, 0, 0, width, height);

                float overlayBitmapRatio = (float) overlayBitmap.Width / (float) overlayBitmap.Height;
                float overlayWidth, overlayHeight, overlayX, overlayY;
                string mode = 
                if (overlayBitmap.Width > width && overlayBitmap.Height < height)
                {
                    // We need to shrink down until Height fits
                    
                    overlayHeight = overlayWidth * overlayBitmapRatio;
                    overlayX = width - overlayWidth;
                    overlayY = height - overlayHeight;
                }
                else if (overlayBitmap.Width < width && overlayBitmap.Height > height)
                {
                    // We need to shrink down until Width fits
                    overlayHeight = (height * 0.7F);
                    overlayWidth = overlayHeight * (1 / overlayBitmapRatio);
                    overlayX = width - overlayWidth;
                    overlayY = height - overlayHeight;
                }
                else if (overlayBitmap.Width > width && overlayBitmap.Height > height)
                {
                    // We need to shrink down until Width and Height fits

                    overlayHeight = (height * 0.7F);
                    overlayWidth = overlayHeight * (1 / overlayBitmapRatio);
                    overlayX = width - overlayWidth;
                    overlayY = height - overlayHeight;
                }


                if (overlayBitmap.Width > width && overlayBitmap.Height < height)
                {
                    // We need to shrink down until Height fits
                    overlayWidth = (width * 0.7F);
                    overlayHeight = overlayWidth * overlayBitmapRatio;
                    overlayX = width - overlayWidth;
                    overlayY = height - overlayHeight;
                }
                else if (overlayBitmap.Width < width && overlayBitmap.Height > height)
                {
                    // We need to shrink down until Width fits
                    overlayHeight = (height * 0.7F);
                    overlayWidth = overlayHeight * (1/overlayBitmapRatio);
                    overlayX = width - overlayWidth;
                    overlayY = height - overlayHeight;
                }
                else if (overlayBitmap.Width > width && overlayBitmap.Height > height)
                {
                    // We need to shrink down until Width and Height fits

                    overlayHeight = (height * 0.7F);
                    overlayWidth = overlayHeight * (1 / overlayBitmapRatio);
                    overlayX = width - overlayWidth;
                    overlayY = height - overlayHeight;
                }
                g.DrawImage(overlayBitmap, overlayX, overlayY, overlayWidth, overlayHeight);

            }
            return combinedBitmap;
        }

        public MultiIcon ToIcon()
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
                    g.DrawImage(_originalBitmap, new Rectangle(0, 0, size.Width, size.Height));
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
        }

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
                Bitmap bitmapOverlay = ToBitmapOverlay(_originalBitmap, ProfileToUse.ProfileTightestBitmap, size.Width, size.Height);
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
