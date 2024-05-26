using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Drawing.IconLib;
using System.Text;
using System.Threading.Tasks;
using TsudaKageyu;
using DisplayMagicianShared;
using MintPlayer.IconUtils;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Printing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Windows;
//using Windows.UI.Xaml.Media.Imaging;
using BitmapImage = System.Windows.Media.Imaging.BitmapImage;
using Microsoft.WindowsAPICodePack.Win32Native;
using System.Runtime.InteropServices.ComTypes;

namespace DisplayMagician
{
    public static class ImageUtils
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static Image RoundCorners(Image StartImage, int CornerRadius)
        {
            if (StartImage == null)
            {
                throw new ArgumentNullException("StartImage");
            }

            CornerRadius *= 2;
            Bitmap RoundedImage = new Bitmap(StartImage.Width, StartImage.Height);
            using (Graphics g = Graphics.FromImage(RoundedImage))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.HighQuality;
                Brush brush = new TextureBrush(StartImage);
                GraphicsPath gp = new GraphicsPath();
                gp.AddArc(0, 0, CornerRadius, CornerRadius, 180, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0, CornerRadius, CornerRadius, 270, 90);
                gp.AddArc(0 + RoundedImage.Width - CornerRadius, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 0, 90);
                gp.AddArc(0, 0 + RoundedImage.Height - CornerRadius, CornerRadius, CornerRadius, 90, 90);
                g.FillPath(brush, gp);
                return RoundedImage;
            }
        }

        public static void DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (pen == null)
                throw new ArgumentNullException("pen");

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.DrawPath(pen, path);
            }
        }

        public static void FillRoundedRectangle(Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            if (brush == null)
                throw new ArgumentNullException("brush");

            using (GraphicsPath path = RoundedRect(bounds, cornerRadius))
            {
                graphics.FillPath(brush, path);
            }
        }

        public static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int diameter = radius * 2;
            System.Drawing.Size size = new System.Drawing.Size(diameter, diameter);
            Rectangle arc = new Rectangle(bounds.Location, size);
            GraphicsPath path = new GraphicsPath();

            if (radius == 0)
            {
                path.AddRectangle(bounds);
                return path;
            }

            // top left arc  
            path.AddArc(arc, 180, 90);

            // top right arc  
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);

            // bottom right arc  
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);

            // bottom left arc 
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);

            path.CloseFigure();
            return path;
        }

        public static Image MakeGrayscale(Image original)
        {
            if (original == null)
            {
                throw new ArgumentNullException("original");
            }

            //create a blank bitmap the same size as original
            Image newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {

                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
             new float[] {.3f, .3f, .3f, 0, 0},
             new float[] {.59f, .59f, .59f, 0, 0},
             new float[] {.11f, .11f, .11f, 0, 0},
             new float[] {0, 0, 0, 1, 0},
             new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }

        public static List<ShortcutBitmap> GetMeAllBitmapsFromFile(string fileNameAndPath)
        {
            if (String.IsNullOrWhiteSpace(fileNameAndPath))
            {
                logger.Warn($"ShortcutItem/GetMeAllBitmapsFromFile: Bitmap fileNameAndPath is empty! Unable to get the bitmaps from the file.");
                return new List<ShortcutBitmap>();
            }

            List<ShortcutBitmap> bmList = new List<ShortcutBitmap>();
            int bmCount = 0;
            string fileNameOnly = Path.GetFileName(fileNameAndPath);

            // Look for any single bitmap files, and extract the single image and return it as a list of one.
            if (fileNameAndPath.ToLower().EndsWith(".jpg") || fileNameAndPath.ToLower().EndsWith(".gif") || fileNameAndPath.ToLower().EndsWith(".tif") || fileNameAndPath.ToLower().EndsWith(".png") || fileNameAndPath.ToLower().EndsWith(".bmp") ||
                fileNameAndPath.ToLower().EndsWith(".jpeg") || fileNameAndPath.ToLower().EndsWith(".tiff"))
            {

                try
                {

                    logger.Trace($"ShortcutItem/GetMeABitmapFromFile: The file we want to get the image from is an image file. Attempting to extract the image from {fileNameAndPath}.");

                    Bitmap bmap = new Bitmap(fileNameAndPath);
                    ShortcutBitmap bm = CreateShortcutBitmap(bmap, fileNameOnly, fileNameAndPath, bmCount++);
                    // Add the shortcutbitmap to the list
                    if (!bmList.Contains(bm))
                    {
                        logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Added new bitmap from the image file {fileNameAndPath} using standard bitmap decoder access method.");
                        bmList.Add(bm);
                    }
                    else
                    {
                        logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Skipping adding new bitmap from the image file {fileNameAndPath} using standard bitmap decoder access method.");
                    }
                    
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/GetMeABitmapFromFile: Exception while trying to extract the bitmap from an image ({fileNameAndPath})using standard bitmap decoder tools.");
                }
            }

            // Any ico or exe or com files fall through to here
            if (fileNameAndPath.ToLower().EndsWith(".ico"))
            {

                try
                {

                    logger.Trace($"ShortcutItem/GetMeABitmapFromFile: The file we want to get the image from is an icon file. Attempting to extract the icon file from {fileNameAndPath}.");

                    Icon myIcon = new Icon(fileNameAndPath, 256, 256);
                    myIcon = Icon.ExtractAssociatedIcon(fileNameAndPath);
                    ShortcutBitmap bm = CreateShortcutBitmap(myIcon.ToBitmap(), fileNameOnly, fileNameAndPath, bmCount++);
                    // Add the shortcutbitmap to the list
                    if (!bmList.Contains(bm))
                    {
                        logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Added new bitmap from the icon file {fileNameAndPath} using standard Icon access method.");
                        bmList.Add(bm);
                    }
                    else
                    {
                        logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Skipped adding new bitmap from the icon file {fileNameAndPath} using standard Icon access method.");
                    }
                    
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/GetMeABitmapFromFile: Exception while trying to extract the icon from a *.ico using Standard Icon tools for file {fileNameAndPath}.");
                }

                try
                {
                    MultiIcon myMultiIcon = new MultiIcon();
                    SingleIcon mySingleIcon = myMultiIcon.Add("Icon1");
                    mySingleIcon.Load(fileNameAndPath);
                    foreach (IconImage myIconImage in mySingleIcon)
                    {
                        ShortcutBitmap bm = CreateShortcutBitmap(myIconImage.Image, fileNameOnly, fileNameAndPath, bmCount++);
                        // Add the shortcutbitmap to the list
                        if (!bmList.Contains(bm))
                        {
                            logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Added new bitmap from the icon file {fileNameAndPath} using MultiIcon access method.");
                            bmList.Add(bm);
                        }
                        else
                        {
                            logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Skipped adding new bitmap from the icon file {fileNameAndPath} using MultiIcon access method.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/GetMeABitmapFromFile: Exception while trying to extract the icon from a *.ico using MultiIcon tools for file {fileNameAndPath}.");
                }
            }




            // Any ico or exe or com files fall through to here
            if (fileNameAndPath.ToLower().EndsWith(".exe"))
            {
              

                /*try
                {
                    Icon anIcon = (Icon)null;
                    anIcon = IconFromFile.GetLargeIconFromFile(fileNameAndPath, true, true);
                    //anIcon = Icon.ExtractAssociatedIcon(fileNameAndPath);
                    ShortcutBitmap bm = CreateShortcutBitmap(anIcon.ToBitmap(), fileNameOnly, fileNameAndPath, bmCount++);
                    // Add the shortcutbitmap to the list
                    if (!bmList.Contains(bm))
                    {
                        logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Added new bitmap from the exe file {fileNameAndPath} using Icon.ExtractAssociatedIcon access method.");
                        bmList.Add(bm);
                    }
                    else
                    {
                        logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Skipped adding new bitmap from the exe file {fileNameAndPath} using Icon.ExtractAssociatedIcon access method.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/GetMeABitmapFromFile: Exception while trying to extract the icon from an *.exe or *.dll using Icon.ExtractAssociatedIcon.");
                }*/

                /*try
                {
                    List<Icon> myIcons = ImageUtils.ExtractIconsFromExe(fileNameAndPath, true);
                    if (myIcons != null && myIcons.Count > 0)
                    {
                        foreach (Icon myExtractedIcon in myIcons)
                        {
                            ShortcutBitmap bm = CreateShortcutBitmap(myExtractedIcon.ToBitmap(), fileNameOnly, fileNameAndPath, bmCount++);
                            // Add the shortcutbitmap to the list
                            if (!bmList.Contains(bm))
                            {
                                logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Added new bitmap from the exe file {fileNameAndPath} using ImageUtils.ExtractIconsFromExe access method.");
                                bmList.Add(bm);
                            }
                            else
                            {
                                logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Skipped adding new bitmap from the exe file {fileNameAndPath} using ImageUtils.ExtractIconsFromExe access method.");
                            }
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/GetMeABitmapFromFile: Exception while trying to extract the icon from an *.exe or *.dll using ImageUtils.ExtractIconsFromExe.");
                }*/

                try
                {
                    var ie = new TsudaKageyu.IconExtractor(fileNameAndPath);
                    Icon[] allIcons = ie.GetAllIcons();
                    foreach (Icon myExtractedIcon in allIcons)
                    {
                        ShortcutBitmap bm = CreateShortcutBitmap(myExtractedIcon.ToBitmap(), fileNameOnly, fileNameAndPath, bmCount++);
                        // Add the shortcutbitmap to the list
                        if (!bmList.Contains(bm))
                        {
                            logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Added new bitmap from the exe file {fileNameAndPath} using TsudaKageyu.IconExtractor access method.");
                            bmList.Add(bm);
                        }
                        else
                        {
                            logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Skipped adding new bitmap from the exe file {fileNameAndPath} using TsudaKageyu.IconExtractor access method.");
                        }                        
                        
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/GetMeABitmapFromFile: Exception while trying to extract the icon from an *.exe or *.dll using TsudaKageyu.IconExtractor for file {fileNameAndPath}.");
                }

                
            }

        
            // Any ico or exe or com files fall through to here
            if (fileNameAndPath.ToLower().EndsWith(".exe") || fileNameAndPath.ToLower().EndsWith(".ico"))
            {
                try
                {
                    Bitmap originalImage = WindowsThumbnailProvider.GetThumbnail(fileNameAndPath, 96, 96, ThumbnailOptions.BiggerSizeOk);
                    ShortcutBitmap bm = CreateShortcutBitmap(originalImage, fileNameOnly, fileNameAndPath, bmCount++);
                    // Add the shortcutbitmap to the list
                    if (!bmList.Contains(bm))
                    {
                        logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Added new bitmap from the exe file {fileNameAndPath} using WindowsThumbnailProvider.GetThumbnail access method.");
                        bmList.Add(bm);
                    }
                    else
                    {
                        logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Skipped adding new bitmap from the exe file {fileNameAndPath} using WindowsThumbnailProvider.GetThumbnail access method.");
                    }
                }
                catch (Exception ex)
                {
                    logger.Trace(ex, $"ShortcutItem/GetMeABitmapFromFile: Exception while trying to extract the thumbnail from an *.exe or *.dll using WindowsThumbnailProvider.GetThumbnail. Means the file {fileNameAndPath} could not be processed so skipping it with this method");                                             
                }

                try
                {

                    List<Icon> myExtractedIcons = MintPlayer.IconUtils.IconExtractor.Split(fileNameAndPath.ToLower()).Result;
                    System.Drawing.Size largeSize = new System.Drawing.Size(256, 256);
                    if (myExtractedIcons.Count > 0)
                    {
                        Icon myIcon = null;

                        myIcon = (Icon)IconUtil.TryGetIcon(myExtractedIcons.ToArray(), largeSize, 32, true, true);

                        if (myIcon != null)
                        {
                            ShortcutBitmap bm = CreateShortcutBitmap(myIcon.ToBitmap(), fileNameOnly, fileNameAndPath, bmCount++);
                            // Add the shortcutbitmap to the list
                            if (!bmList.Contains(bm))
                            {
                                logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Added new bitmap from the file {fileNameAndPath} using MintPlayer.IconUtils.IconExtractor access method.");
                                bmList.Add(bm);
                            }
                            else
                            {
                                logger.Trace($"ShortcutItem/GetMeABitmapFromFile: Skipped adding new bitmap from the file {fileNameAndPath} using MintPlayer.IconUtils.IconExtractor access method.");
                            }
                        }
                        else
                        {
                            logger.Warn($"ShortcutItem/GetMeABitmapFromFile: Couldn't extract an Icon from the file {fileNameAndPath} using MintPlayer.IconUtils.IconExtractor access method, so can't try to get the Icon using IconUtils.TryGetIcon.");
                        }
                    }

                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/GetMeABitmapFromFile: Exception while trying to Split the icon using MintPlayer IconExtractor on file {fileNameAndPath}! ");
                }
            }

            return bmList;

        }

        public static List<ShortcutBitmap> GetMeAllBitmapsFromFile(ArrayList fileNamesAndPaths)
        {
            List<ShortcutBitmap> bmToReturn = null;

            if (fileNamesAndPaths.Count == 0)
            {
                logger.Warn($"ShortcutItem/GetMeAllBitmapsFromFile: The fileNamesAndPaths list is empty! Can't get the bitmap from the files.");
                return null;
            }
            logger.Trace($"ShortcutItem/GetMeAllBitmapsFromFile: We have {fileNamesAndPaths.Count} files to try and extract a bitmap from.");
            foreach (string fileNameAndPath in fileNamesAndPaths)
            {
                logger.Trace($"ShortcutItem/GetMeAllBitmapsFromFile: Getting a bitmap from {fileNameAndPath} by running GetMeABitmapFromFile.");
                bmToReturn.AddRange(GetMeAllBitmapsFromFile(fileNameAndPath));
            }

            return bmToReturn;

        }

        public static ShortcutBitmap GetMeLargestAvailableBitmap(List<ShortcutBitmap> shortcutBitmaps)
        {
            // Returns the first ShortcutBitmap with the largest size

            ShortcutBitmap scToReturn = new ShortcutBitmap();

            logger.Trace($"ShortcutItem/GetMeLargestAvailableBitmap: We have {shortcutBitmaps.Count} bitmaps to find the largest one within.");
            foreach (ShortcutBitmap sc in shortcutBitmaps)
            {
                if (sc.Size.Width > scToReturn.Size.Width && sc.Size.Height > scToReturn.Size.Height)
                {
                    scToReturn = sc;
                }
            }

            return scToReturn;

        }

        public static ShortcutBitmap CreateShortcutBitmap(Bitmap bitmap, string name = "", string source = "", int order = 0)
        {
            ShortcutBitmap sc = new ShortcutBitmap();
            sc.UUID = Guid.NewGuid().ToString("D");
            sc.Name = name;
            sc.Order = order;
            sc.Source = source;
            sc.Image = bitmap;
            sc.Size = new System.Drawing.Size(sc.Image.Width, sc.Image.Height);
            return sc;
        }

        public static List<ShortcutBitmap> ShortcutBitmapClone(List<ShortcutBitmap> shortcutBitmaps)
        {
            // Clones the List<ShortcutBitmap>

            List<ShortcutBitmap> scListToReturn = new List<ShortcutBitmap>();
            foreach (ShortcutBitmap sc in shortcutBitmaps)
            {
                scListToReturn.Add(ImageUtils.ShortcutBitmapClone(sc));
            }

            return scListToReturn;
        }

        public static ShortcutBitmap ShortcutBitmapClone(ShortcutBitmap shortcutBitmap)
        {
            // Clones the ShortcutBitmap

            ShortcutBitmap scToReturn = new ShortcutBitmap();
            scToReturn.UUID = Guid.NewGuid().ToString("D"); 
            scToReturn.Image = (Bitmap)shortcutBitmap.Image.Clone();
            scToReturn.Name = shortcutBitmap.Name;
            scToReturn.Order = shortcutBitmap.Order;
            scToReturn.Size = shortcutBitmap.Size;
            scToReturn.Source = shortcutBitmap.Source;
            
            return scToReturn;
        }

        public static bool ImagesAreEqual(Bitmap imageA, Bitmap imageB)
        {
            byte[] image1Bytes;
            byte[] image2Bytes;

            if (imageA == null || imageB == null)
                return false;

            using (var mstream = new MemoryStream())
            {
                imageA.Save(mstream,ImageFormat.Bmp);
                image1Bytes = mstream.ToArray();
            }

            using (var mstream = new MemoryStream())
            {
                imageB.Save(mstream, ImageFormat.Bmp);
                image2Bytes = mstream.ToArray();
            }

            var image164 = Convert.ToBase64String(image1Bytes);
            var image264 = Convert.ToBase64String(image2Bytes);

            return string.Equals(image164, image264);
        }


        public static Bitmap MakeBitmapOverlay(Bitmap originalBitmap, Bitmap overlayBitmap, int width, int height, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            if (originalBitmap == null)
            {
                logger.Trace($"ShortcutItem/ToBitmapOverlay: OriginalBitmap is null, so we'll try to make the BitmapOverlay using GameLibrary Icon.");
                return null;
            }

            if (overlayBitmap == null)
            {
                logger.Trace($"ShortcutItem/ToBitmapOverlay: overlayBitmap is null, so we'll just return the original bitmap without a profile overlay.");
                return originalBitmap;
            }

            if (width <= 0 || width > 256)
            {
                logger.Trace($"ShortcutItem/ToBitmapOverlay: Width is out of range so setting to 256.");
                width = 256;
            }

            if (height <= 0 || height > 256)
            {
                logger.Trace($"ShortcutItem/ToBitmapOverlay: Height is out of range so setting to 256.");
                height = 256;
            }

            // Figure out sizes and positions
            try
            {
                System.Drawing.Size targetSize = new System.Drawing.Size(width, height);
                logger.Trace($"ShortcutItem/ToBitmapOverlay: TargetSize is {targetSize.Width}px x {targetSize.Height}px.");
                System.Drawing.Size originalBitmapCurrentSize = new System.Drawing.Size(originalBitmap.Width, originalBitmap.Height);
                logger.Trace($"ShortcutItem/ToBitmapOverlay: originalBitmapCurrentSize is {originalBitmapCurrentSize.Width}px x {originalBitmapCurrentSize.Height}px.");
                System.Drawing.Size overlaylBitmapCurrentSize = new System.Drawing.Size(overlayBitmap.Width, overlayBitmap.Height);
                logger.Trace($"ShortcutItem/ToBitmapOverlay: overlaylBitmapCurrentSize is {overlaylBitmapCurrentSize.Width}px x {overlaylBitmapCurrentSize.Height}px.");

                // Make a new empty bitmap of the wanted size
                logger.Trace($"ShortcutItem/ToBitmapOverlay: Making a new combined bitmap as the base for the image.");
                var combinedBitmap = new Bitmap(targetSize.Width, targetSize.Height, format);
                combinedBitmap.MakeTransparent();

                using (var g = Graphics.FromImage(combinedBitmap))
                {
                    logger.Trace($"ShortcutItem/ToBitmapOverlay: Setting smoothing mode, Interpolation mode, pixel offset mode and compositing quality.");
                    g.SmoothingMode = SmoothingMode.None;
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.AssumeLinear;

                    // Resize the originalBitmap if needed then draw it
                    System.Drawing.Size originalBitmapNewSize = ResizeDrawing.FitWithin(originalBitmapCurrentSize, targetSize);
                    logger.Trace($"ShortcutItem/ToBitmapOverlay: Resizing the original bitmap to fit in the new combined bitmap. Size is now {originalBitmapNewSize.Width}px x {originalBitmapNewSize.Height}px");
                    System.Drawing.Point originalBitmapNewLocation = ResizeDrawing.AlignCenter(originalBitmapNewSize, targetSize);
                    logger.Trace($"ShortcutItem/ToBitmapOverlay: Drawing the original bitmap into the new combined bitmap at position {originalBitmapNewLocation.X},{originalBitmapNewLocation.Y}..");
                    g.DrawImage(originalBitmap, originalBitmapNewLocation.X, originalBitmapNewLocation.Y, originalBitmapNewSize.Width, originalBitmapNewSize.Height);

                    // Resize the overlayBitmap if needed then draw it in the bottom-right corner                    
                    System.Drawing.Size overlayBitmapMaxSize = ResizeDrawing.FitWithin(overlaylBitmapCurrentSize, targetSize);
                    System.Drawing.Size overlayBitmapNewSize = ResizeDrawing.MakeSmaller(overlayBitmapMaxSize, 70);
                    logger.Trace($"ShortcutItem/ToBitmapOverlay: Resize the overlay bitmap to fit in the bottom right corner of the new combined bitmap. Size is now {overlayBitmapNewSize.Width}px x {overlayBitmapNewSize.Height}px");
                    System.Drawing.Point overlayBitmapNewLocation = ResizeDrawing.AlignBottomRight(overlayBitmapNewSize, targetSize);
                    // Now we need to adjust that slightly up and in a bit
                    overlayBitmapNewLocation.Offset(0, -5);
                    logger.Trace($"ShortcutItem/ToBitmapOverlay: Drawing the overlay bitmap into the new combined bitmap at position {overlayBitmapNewLocation.X},{overlayBitmapNewLocation.Y}.");
                    g.DrawImage(overlayBitmap, overlayBitmapNewLocation.X, overlayBitmapNewLocation.Y, overlayBitmapNewSize.Width, overlayBitmapNewSize.Height);

                }
                return combinedBitmap;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutItem/ToBitmapOverlay: Exception while trying to add the overlay to the Bitmap. Returning null");
                return null;
            }

        }


        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));
            if (bitmapImage == null)
            {
                return null;
            }

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
{
            if (bitmap == null)
            {
                return null;
            }

            IntPtr hBitmap = bitmap.GetHbitmap();
            BitmapImage retval;

            try
            {
                retval = (BitmapImage)Imaging.CreateBitmapSourceFromHBitmap(
                             hBitmap,
                             IntPtr.Zero,
                             Int32Rect.Empty,
                             BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(hBitmap);
            }

            return retval;
        }


        /*
        #pragma warning disable CS3002 // Return type is not CLS-compliant
                public static MultiIcon ToIconOverlay(Bitmap originalBitmap, Bitmap overlayBitmap)
        #pragma warning restore CS3002 // Return type is not CLS-compliant
                {
                    try
                    {
                        Size[] iconSizes = new[]
                        {
                            new Size(256, 256),
                            new Size(64, 64),
                            new Size(48, 48),
                            new Size(32, 32),
                            new Size(24, 24),
                            new Size(16, 16)
                        };
                        logger.Trace($"ShortcutItem/ToIconOverlay: Creating the new Multi image Icon.");
                        MultiIcon multiIcon = new MultiIcon();
                        logger.Trace($"ShortcutItem/ToIconOverlay: Adding a single icon to the multi image icon.");
                        SingleIcon icon = multiIcon.Add("Icon1");

                        foreach (Size size in iconSizes)
                        {
                            logger.Trace($"ShortcutItem/ToIconOverlay: Creating a new image layer of size {size.Width}px x {size.Height}px.");
                            Bitmap bitmapOverlay = ToBitmapOverlay(originalBitmap, overlayBitmap, size.Width, size.Height);
                            if (bitmapOverlay == null)
                            {
                                logger.Warn($"ShortcutItem/ToIconOverlay: bitmapOverlay is null, so we can't turn it into an Icon Overlay. Returning null");
                                return null;
                            }
                            logger.Trace($"ShortcutItem/ToIconOverlay: Adding the new image layer of size {size.Width}px x {size.Height}px to the multi image icon.");
                            icon.Add(bitmapOverlay);

                            if (size.Width >= 256 && size.Height >= 256)
                            {
                                logger.Trace($"ShortcutItem/ToIconOverlay: The image is > 256px x 256px so making it a PNG layer in the icon file.");
                                icon[icon.Count - 1].IconImageFormat = IconImageFormat.PNG;
                            }

                            logger.Trace($"ShortcutItem/ToIconOverlay: Disposing of the Bitmap data we just used as the source (stops memory leaks).");
                            bitmapOverlay.Dispose();
                        }

                        logger.Trace($"ShortcutItem/ToIconOverlay: Make the top layer image of the Multi image icon the default one.");
                        multiIcon.SelectedIndex = 0;

                        return multiIcon;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, $"ShortcutItem/ToIconOverlay: Exeception occurred while trying to convert the Shortcut Bitmap to an Icon Overlay to store in the shortcut cache directory. Returning null");
                        return null;
                    }
                }*/

        [DllImport("Shell32", CharSet = CharSet.Auto)]
        private static  extern int ExtractIconEx(string lpszFile, int nIconIndex, IntPtr[] phIconLarge, IntPtr[] phIconSmall, int nIcons);

        [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
        private static  extern int DestroyIcon(IntPtr hIcon);

        public static List<Icon> ExtractIconsFromExe(string file, bool large)
        {
            int readIconCount = 0;
            IntPtr[] hLargeIconEx = new IntPtr[] { IntPtr.Zero };
            IntPtr[] hSmallIconEx = new IntPtr[] { IntPtr.Zero };

            try
            {
                List<Icon> extractedIcons = new List<Icon>() { };
                // First we get the total number of icons using ExtractIconEx
                int totalIconCount = ExtractIconEx(file, -1, null, null, 0);
                if (totalIconCount > 0)
                {
                    for (int iconNum = 0; iconNum < totalIconCount; iconNum++)
                    {
                        hLargeIconEx = new IntPtr[] { IntPtr.Zero };
                        hSmallIconEx = new IntPtr[] { IntPtr.Zero };
                        //if (large)
                        //readIconCount = ExtractIconEx(file, 0, hIconEx, hDummy, 1);
                        //    readIconCount = ExtractIconEx(file, iconNum, hIconEx, null, 1);
                        //else
                        //readIconCount = ExtractIconEx(file, 0, hDummy, hIconEx, 1);
                        //    readIconCount = ExtractIconEx(file, iconNum, null, hIconEx, 1);

                        readIconCount = ExtractIconEx(file, iconNum, hLargeIconEx, hSmallIconEx, 1);

                        if (readIconCount > 0)
                        {
                            if (hLargeIconEx[0] != IntPtr.Zero)
                            {
                                Icon extractedIcon = (Icon)Icon.FromHandle(hLargeIconEx[0]).Clone();
                                extractedIcons.Add(extractedIcon);
                            }
                            else if (hSmallIconEx[0] != IntPtr.Zero)
                            {
                                Icon extractedIcon = (Icon)Icon.FromHandle(hSmallIconEx[0]).Clone();
                                extractedIcons.Add(extractedIcon);
                            }
                        }
                    }

                    /*foreach (IntPtr ptr in hLargeIconEx)
                        if (ptr != IntPtr.Zero)
                            DestroyIcon(ptr);

                    foreach (IntPtr ptr in hSmallIconEx)
                        if (ptr != IntPtr.Zero)
                            DestroyIcon(ptr);*/

                    return extractedIcons;
                }
                else
                    return null;

            }
            catch (Exception ex)
            {
                /* EXTRACT ICON ERROR */

                // BUBBLE UP
                throw new ApplicationException("Could not extract icon", ex);
            }
            finally
            {
                // RELEASE RESOURCES
                foreach (IntPtr ptr in hLargeIconEx)
                    if (ptr != IntPtr.Zero)
                        DestroyIcon(ptr);

                foreach (IntPtr ptr in hSmallIconEx)
                    if (ptr != IntPtr.Zero)
                        DestroyIcon(ptr);
            }
            
        }

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType,
            int cxDesired, int cyDesired, uint fuLoad);

        /*[DllImport("user32.dll", SetLastError = true)]
        private static extern int DestroyIcon(IntPtr hIcon);*/

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hFile, LoadLibraryFlags dwFlags);

        private enum LoadLibraryFlags : uint
        {
            DONT_RESOLVE_DLL_REFERENCES = 0x00000001,
            LOAD_IGNORE_CODE_AUTHZ_LEVEL = 0x00000010,
            LOAD_LIBRARY_AS_DATAFILE = 0x00000002,
            LOAD_LIBRARY_AS_DATAFILE_EXCLUSIVE = 0x00000040,
            LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020,
            LOAD_WITH_ALTERED_SEARCH_PATH = 0x00000008
        }

        /// <summary>
        /// Returns an icon of given size.
        /// </summary>
        /// <param name="path">Path to a file (.exe/.dll) that contains the icons.
        ///        Skip it or use <c>null</c> to use current application's file.</param>
        /// <param name="resId">Name of the resource icon that should be loaded.
        ///        Skip it to use the default <c>#32512</c> (value of <c>IDI_APPLICATION</c>) to use
        ///        the application's icon.</param>
        /// <param name="size">Size of the icon to load. If there is no such size available, a larger or smaller
        ///        sized-icon is scaled.</param>
        /// <returns>List of all icons.</returns>
        public static Icon GetIconFromExe(string path = null, int size = 256, string resId = "#32512")
        {
            // load module
            IntPtr h;
            if (path == null)
                //h = Marshal.GetHINSTANCE(Assembly.GetEntryAssembly().GetModules()[0]);
                return null;
            else
            {
                h = LoadLibraryEx(path, IntPtr.Zero, LoadLibraryFlags.LOAD_LIBRARY_AS_DATAFILE);
                if (h == IntPtr.Zero)
                    return null;
            }

            // 1 is IMAGE_ICON
            IntPtr ptr = LoadImage(h, resId, 1, size, size, 0);
            if (ptr != IntPtr.Zero)
            {
                try
                {
                    Icon icon = (Icon)Icon.FromHandle(ptr).Clone();
                    return icon;
                }
                finally
                {
                    DestroyIcon(ptr);
                }
            }
            return null;            
        }

        // Convert the Bitmap to grayscale.
        public static Bitmap ConvertBitmapToGrayscale(Bitmap bm, bool use_average=false)
        {
            if (bm == null)
            {
                return null;
            }

            Bitmap greyBm = new Bitmap(bm);

            for (int i = 0; i < greyBm.Width; i++)
            {
                for (int j = 0; j < greyBm.Height; j++)
                {
                    Color c = greyBm.GetPixel(i, j);

                    //Apply conversion equation
                    byte gray = (byte)(.21 * c.R + .71 * c.G + .071 * c.B);

                    //Set the color of this pixel
                    greyBm.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }

            return greyBm;
        }

        public static Image ConvertBitmapToGrayscale(Image image, bool use_average = false)
        {
            if (image == null)
            {
                return null;
            }
            Bitmap greyBm = ConvertBitmapToGrayscale((Bitmap)image);
            return (Image)greyBm;
        }
    }
}
