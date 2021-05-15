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

namespace DisplayMagician
{
    public static class ImageUtils
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static Image RoundCorners(Image StartImage, int CornerRadius)
        {
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
            Size size = new Size(diameter, diameter);
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

 /*       public static Bitmap ToLargeBitmap(string fileNameAndPath)
        {
            Bitmap bmToReturn = null;

            try
            {
                if (String.IsNullOrWhiteSpace(fileNameAndPath))
                {
                    logger.Warn($"ShortcutItem/ToLargeBitmap: Bitmap fileNameAndPath is empty! Unable to get the large icon from the file (256px x 256px).");
                    return null;
                }

                if (fileNameAndPath.EndsWith(".ico"))
                {
                    logger.Trace($"ShortcutItem/ToLargeBitmap: The file we want to get the image from is an icon file. Attempting to extract the icon file from {fileNameAndPath}.");

                    Icon myIcon = new Icon(fileNameAndPath,128,128);
                    bmToReturn = myIcon.ToBitmap();
                    MultiIcon myMultiIcon = new MultiIcon();
                    SingleIcon mySingleIcon = myMultiIcon.Add("Icon1");
                    //mySingleIcon.CreateFrom(fileNameAndPath,IconOutputFormat.Vista);
                    mySingleIcon.Load(fileNameAndPath);
                    Bitmap bm = null;
                    foreach (IconImage myIconImage in mySingleIcon)
                    {
                        bm = myIconImage.Image;

                        if (bm.Width > bmToReturn.Width && bm.Height > bmToReturn.Height)
                        {
                            bmToReturn = bm;
                            logger.Trace($"ShortcutItem/ToLargeBitmap: This new bitmap from the icon file {fileNameAndPath} is larger than the previous one at {bm.Width} x {bm.Height}, so using that instead.");
                        }
                    }                                    
                }
                else
                {
                    Icon myIcon = Icon.ExtractAssociatedIcon(fileNameAndPath);
                    bmToReturn = myIcon.ToBitmap();

                    logger.Trace($"ShortcutItem/ToLargeBitmap: The file {fileNameAndPath} isn't an Icon file, so trying to use GetLargeBitmapFromFile to extract the image.");
                    Bitmap bm = null;
                    bm = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, false, false);

                    if (bm.Width > bmToReturn.Width && bm.Height > bmToReturn.Height)
                    {
                        bmToReturn = bm;
                        logger.Trace($"ShortcutItem/ToLargeBitmap: This new bitmap from the icon file {fileNameAndPath} is larger than the previous one at {bm.Width} x {bm.Height}, so using that instead.");
                    }
                }
                return bmToReturn;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutItem/ToLargeBitmap: Exception while trying to save the Shortcut icon initially. Trying again with GetLargeBitmapFromFile.");
                try
                {
                    logger.Trace($"ShortcutItem/ToLargeBitmap: Attempt2. The file {fileNameAndPath} isn't an Icon file, so trying to use GetLargeBitmapFromFile to extract the image.");
                    bmToReturn = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, true, false);
                    return bmToReturn;
                }
                catch (Exception innerex)
                {
                    logger.Warn(innerex, $"ShortcutItem/ToLargeBitmap: Exception while trying to save the Shortcut icon a second time. Giving up.");
                    return null;
                }
            }
        }

        public static Bitmap ToLargeBitmap(ArrayList fileNamesAndPaths)
        {
            Bitmap bmToReturn = null;

            
            if (fileNamesAndPaths.Count == 0)
            {
                logger.Warn($"ShortcutItem/ToLargeBitmap2: The fileNamesAndPaths list is empty! Can't get the large bitmap.");
                return null;
            }
            foreach (string fileNameAndPath in fileNamesAndPaths)
            {
                Bitmap bm = ToLargeBitmap(fileNameAndPath);

                if (bmToReturn == null)
                {
                    bmToReturn = bm;
                }
                if (bm.Width > bmToReturn.Width && bm.Height > bmToReturn.Height)
                {
                    bmToReturn = bm;
                }
            }
            return bmToReturn;

        }*/

        public static Bitmap GetMeABitmapFromFile(string fileNameAndPath)
        {
            Bitmap bmToReturn = null;
            try
            {

                if (String.IsNullOrWhiteSpace(fileNameAndPath))
                {
                    logger.Warn($"ShortcutItem/ToSmallBitmap: Bitmap fileNameAndPath is empty! Unable to get the small icon from the file (128px x 128px).");
                    return null;
                }

                Icon myIcon = null;
                Bitmap bm = null;

                if (fileNameAndPath.EndsWith(".ico"))
                {
                    logger.Trace($"ShortcutItem/ToSmallBitmap: The file we want to get the image from is an icon file. Attempting to extract the icon file from {fileNameAndPath}.");
                    myIcon = new Icon(fileNameAndPath, 128, 128);
                    //Icon myIcon = Icon.ExtractAssociatedIcon(fileNameAndPath);
                    bmToReturn = myIcon.ToBitmap();
                    MultiIcon myMultiIcon = new MultiIcon();
                    SingleIcon mySingleIcon = myMultiIcon.Add("Icon1");
                    mySingleIcon.CreateFrom(fileNameAndPath, IconOutputFormat.Vista);
                    
                    foreach (IconImage myIconImage in mySingleIcon)
                    {
                        bm = myIconImage.Image;

                        if (bm.Width > bmToReturn.Width && bm.Height > bmToReturn.Height)
                        {
                            bmToReturn = bm;
                            logger.Trace($"ShortcutItem/ToSmallBitmap: This new bitmap from the icon file {fileNameAndPath} is larger than the previous one at {bm.Width} x {bm.Height}, so using that instead.");
                        }
                    }

                    myIcon = Icon.ExtractAssociatedIcon(fileNameAndPath);
                    bm = myIcon.ToBitmap();

                    if (bm.Width > bmToReturn.Width && bm.Height > bmToReturn.Height)
                    {
                        bmToReturn = bm;
                        logger.Trace($"ShortcutItem/ToSmallBitmap: This new bitmap from the icon file {fileNameAndPath} is larger than the previous one at {bm.Width} x {bm.Height}, so using that instead.");
                    }
                }
                else
                {
                    myIcon = Icon.ExtractAssociatedIcon(fileNameAndPath);
                    bmToReturn = myIcon.ToBitmap();

                    logger.Trace($"ShortcutItem/ToSmallBitmap: The file {fileNameAndPath} isn't an Icon file, so trying to use GetLargeBitmapFromFile to extract the image.");
                    //bm = IconFromFile.GetSmallBitmapFromFile(fileNameAndPath, false, false, false);
                    bm = IconFromFile.GetSmallBitmapFromFile(fileNameAndPath, false, false, false);
                    
                    if (bm.Width > bmToReturn.Width && bm.Height > bmToReturn.Height)
                    {
                        bmToReturn = bm;
                        logger.Trace($"ShortcutItem/ToSmallBitmap: This new bitmap from the icon file {fileNameAndPath} is larger than the previous one at {bm.Width} x {bm.Height}, so using that instead.");
                    }

                    IconExtractor ie = new IconExtractor(fileNameAndPath);
                    Icon[] allIcons = ie.GetAllIcons();
                    foreach (Icon myExtractedIcon in allIcons)
                    {
                        bm = myExtractedIcon.ToBitmap();

                        if (bm.Width > bmToReturn.Width && bm.Height > bmToReturn.Height)
                        {
                            bmToReturn = bm;
                            logger.Trace($"ShortcutItem/ToSmallBitmap: This new bitmap from the icon file {fileNameAndPath} is larger than the previous one at {bm.Width} x {bm.Height}, so using that instead.");
                        }
                    }

                }
                return bmToReturn;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutItem/ToSmallBitmap: Exception while trying to save the Shortcut icon initially. Trying again with GetSmallBitmapFromFile.");
                try
                {
                    logger.Trace($"ShortcutItem/ToSmallBitmap: Attempt2. The file {fileNameAndPath} isn't an Icon file, so trying to use GetSmallBitmapFromFile to extract the image.");
                    bmToReturn = IconFromFile.GetSmallBitmapFromFile(fileNameAndPath, false, false, false);
                    return bmToReturn;
                }
                catch (Exception innerex)
                {
                    logger.Warn(innerex, $"ShortcutItem/ToSmallBitmap: Exception while trying to save the Shortcut icon a second time. Giving up.");
                    return null;
                }
            }
        }

        public static Bitmap GetMeABitmapFromFile(ArrayList fileNamesAndPaths)
        {
            Bitmap bmToReturn = null;


            if (fileNamesAndPaths.Count == 0)
            {
                logger.Warn($"ShortcutItem/ToSmallBitmap2: The fileNamesAndPaths list is empty! Can't get the large bitmap.");
                return null;
            }
            foreach (string fileNameAndPath in fileNamesAndPaths)
            {
                Bitmap bm = GetMeABitmapFromFile(fileNameAndPath);

                if (bmToReturn == null)
                {
                    bmToReturn = bm;
                }
                if (bm.Width > bmToReturn.Width && bm.Height > bmToReturn.Height)
                {
                    bmToReturn = bm;
                }
            }

            // Now we check if the icon is still too small. 

            return bmToReturn;

        }


        public static Bitmap ToBitmapOverlay(Bitmap originalBitmap, Bitmap overlayBitmap, int width, int height, PixelFormat format = PixelFormat.Format32bppArgb)
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
                Size targetSize = new Size(width, height);
                logger.Trace($"ShortcutItem/ToBitmapOverlay: TargetSize is {targetSize.Width}px x {targetSize.Height}px.");
                Size originalBitmapCurrentSize = new Size(originalBitmap.Width, originalBitmap.Height);
                logger.Trace($"ShortcutItem/ToBitmapOverlay: originalBitmapCurrentSize is {originalBitmapCurrentSize.Width}px x {originalBitmapCurrentSize.Height}px.");
                Size overlaylBitmapCurrentSize = new Size(overlayBitmap.Width, overlayBitmap.Height);
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
                    Size originalBitmapNewSize = ResizeDrawing.FitWithin(originalBitmapCurrentSize, targetSize);
                    logger.Trace($"ShortcutItem/ToBitmapOverlay: Resizing the original bitmap to fit in the new combined bitmap. Size is now {originalBitmapNewSize.Width}px x {originalBitmapNewSize.Height}px");
                    Point originalBitmapNewLocation = ResizeDrawing.AlignCenter(originalBitmapNewSize, targetSize);
                    logger.Trace($"ShortcutItem/ToBitmapOverlay: Drawing the original bitmap into the new combined bitmap at position {originalBitmapNewLocation.X},{originalBitmapNewLocation.Y}..");
                    g.DrawImage(originalBitmap, originalBitmapNewLocation.X, originalBitmapNewLocation.Y, originalBitmapNewSize.Width, originalBitmapNewSize.Height);

                    // Resize the overlayBitmap if needed then draw it in the bottom-right corner                    
                    Size overlayBitmapMaxSize = ResizeDrawing.FitWithin(overlaylBitmapCurrentSize, targetSize);
                    Size overlayBitmapNewSize = ResizeDrawing.MakeSmaller(overlayBitmapMaxSize, 70);
                    logger.Trace($"ShortcutItem/ToBitmapOverlay: Resize the overlay bitmap to fit in the bottom right corner of the new combined bitmap. Size is now {overlayBitmapNewSize.Width}px x {overlayBitmapNewSize.Height}px");
                    Point overlayBitmapNewLocation = ResizeDrawing.AlignBottomRight(overlayBitmapNewSize, targetSize);
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
        }
    }

}
