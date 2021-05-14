using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public static Bitmap ToLargeBitmap(string fileNameAndPath)
        {
            Bitmap bm = null;

            try
            {
                if (String.IsNullOrWhiteSpace(fileNameAndPath))
                {
                    logger.Warn($"ShortcutItem/ToLargeBitmap: Bitmap fileNameAndPath is empty! Unable to get the large icon from the file (256px x 256px).");
                    return null;
                }

                if (fileNameAndPath.EndsWith(".ico"))
                {
                    logger.Trace($"ShortcutItem/ToLargeBitmap: The file we want to get the image from is an icon file. Attempting to load the icon file from {fileNameAndPath}.");
                    Icon icoIcon = new Icon(fileNameAndPath, 256, 256);
                    logger.Trace($"ShortcutItem/ToLargeBitmap: Attempting to convert the icon file {fileNameAndPath} to a bitmap.");
                    bm = icoIcon.ToBitmap();
                    logger.Trace($"ShortcutItem/ToLargeBitmap: Emptying the memory area used but the icon to stop memory leaks.");
                    icoIcon.Dispose();
                }
                else
                {
                    logger.Trace($"ShortcutItem/ToLargeBitmap: The file {fileNameAndPath} isn't an Icon file, so trying to use GetLargeBitmapFromFile to extract the image.");
                    bm = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, true, false);
                }
                return bm;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutItem/ToLargeBitmap: Exception while trying to save the Shortcut icon initially. Trying again with GetLargeBitmapFromFile.");
                try
                {
                    logger.Trace($"ShortcutItem/ToLargeBitmap: Attempt2. The file {fileNameAndPath} isn't an Icon file, so trying to use GetLargeBitmapFromFile to extract the image.");
                    bm = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, true, false);
                    return bm;
                }
                catch (Exception innerex)
                {
                    logger.Warn(innerex, $"ShortcutItem/ToLargeBitmap: Exception while trying to save the Shortcut icon a second time. Giving up.");
                    return null;
                }
            }
        }

        public static Bitmap ToLargeBitmap(List<string> fileNamesAndPaths)
        {
            Bitmap bmToReturn = null;

            
            if (fileNamesAndPaths.Count == 0)
            {
                logger.Warn($"ShortcutItem/ToLargeBitmap2: The fileNamesAndPaths list is empty! Can't get the large bitmap.");
                return null;
            }
            foreach (string fileNameAndPath in fileNamesAndPaths)
            {
                Bitmap bm = null;
                try
                {

                    if (String.IsNullOrWhiteSpace(fileNameAndPath))
                    {
                        logger.Warn($"ShortcutItem/ToLargeBitmap2: Bitmap fileNameAndPath is empty! Trying next file in fileNamesAndPaths list.");
                        continue;
                    }                    

                    if (fileNameAndPath.EndsWith(".ico"))
                    {
                        logger.Trace($"ShortcutItem/ToLargeBitmap2: The file we want to get the image from is an icon file. Attempting to load the icon file from {fileNameAndPath}.");
                        Icon icoIcon = new Icon(fileNameAndPath, 256, 256);
                        logger.Trace($"ShortcutItem/ToLargeBitmap2: Attempting to convert the icon file {fileNameAndPath} to a bitmap.");
                        bm = icoIcon.ToBitmap();
                        logger.Trace($"ShortcutItem/ToLargeBitmap2: Emptying the memory area used but the icon to stop memory leaks.");
                        icoIcon.Dispose();
                    }
                    else
                    {
                        logger.Trace($"ShortcutItem/ToLargeBitmap2: The file {fileNameAndPath} isn't an Icon file, so trying to use GetLargeBitmapFromFile to extract the image.");
                        bm = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, true, true);
                    }                    
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/ToLargeBitmap2: Exception while trying to save the Shortcut icon initially. Trying again with GetLargeBitmapFromFile.");
                    try
                    {
                        logger.Trace($"ShortcutItem/ToLargeBitmap2: Attempt2. The file {fileNameAndPath} isn't an Icon file, so trying to use GetLargeBitmapFromFile to extract the image.");
                        bm = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, true, true);
                        bmToReturn = bm;
                    }
                    catch (Exception innerex)
                    {
                        logger.Warn(innerex, $"ShortcutItem/ToLargeBitmap2: Exception while trying to save the Shortcut icon a second time. Giving up.");
                        continue;
                    }
                }

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

        }

        public static Bitmap ToSmallBitmap(string fileNameAndPath)
        {
            Bitmap bm = null;
            try
            {

                if (String.IsNullOrWhiteSpace(fileNameAndPath))
                {
                    logger.Warn($"ShortcutItem/ToSmallBitmap: Bitmap fileNameAndPath is empty! Unable to get the small icon from the file (128px x 128px).");
                    return null;
                }


                if (fileNameAndPath.EndsWith(".ico"))
                {
                    logger.Trace($"ShortcutItem/ToSmallBitmap: The file we want to get the image from is an icon file. Attempting to load the icon file from {fileNameAndPath}.");
                    Size iconSize = new Size(128, 128);
                    Icon iconToReturn = new Icon(fileNameAndPath, iconSize);
                    /*if (iconToReturn.Size.Width < iconSize.Width || iconToReturn.Size.Height < iconSize.Height)
                    {
                        // If the Icon is too small then we should try the Exe itself to see if its bigger
                        bm = IconFromFile.GetSmallBitmapFromFile(fileNameAndPath, false, false, false);
                    }*/
                    //Icon iconToReturn = IconFromFile.GetLargeIconFromFile(fileNameAndPath, true, true);
                    //Icon iconToReturn = IconUtil.TryGetIcon(myIcon,iconSize,24,true,true);
                    logger.Trace($"ShortcutItem/ToSmallBitmap: Attempting to convert the icon file {fileNameAndPath} to a bitmap.");
                    bm = iconToReturn.ToBitmap();
                    logger.Trace($"ShortcutItem/ToSmallBitmap: Emptying the memory area used but the icon to stop memory leaks.");
                    iconToReturn.Dispose();
                }
                else
                {
                    logger.Trace($"ShortcutItem/ToSmallBitmap: The file {fileNameAndPath} isn't an Icon file, so trying to use GetSmallBitmapFromFile to extract the image.");
                    bm = IconFromFile.GetSmallBitmapFromFile(fileNameAndPath, false, false, false);
                }
                return bm;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutItem/ToSmallBitmap: Exception while trying to save the Shortcut icon initially. Trying again with GetSmallBitmapFromFile.");
                try
                {
                    logger.Trace($"ShortcutItem/ToSmallBitmap: Attempt2. The file {fileNameAndPath} isn't an Icon file, so trying to use GetSmallBitmapFromFile to extract the image.");
                    bm = IconFromFile.GetSmallBitmapFromFile(fileNameAndPath, false, false, false);
                    return bm;
                }
                catch (Exception innerex)
                {
                    logger.Warn(innerex, $"ShortcutItem/ToSmallBitmap: Exception while trying to save the Shortcut icon a second time. Giving up.");
                    return null;
                }
            }
        }

        public static Bitmap ToSmallBitmap(List<string> fileNamesAndPaths)
        {
            Bitmap bmToReturn = null;


            if (fileNamesAndPaths.Count == 0)
            {
                logger.Warn($"ShortcutItem/ToSmallBitmap2: The fileNamesAndPaths list is empty! Can't get the large bitmap.");
                return null;
            }
            foreach (string fileNameAndPath in fileNamesAndPaths)
            {
                Bitmap bm = null;
                try
                {

                    if (String.IsNullOrWhiteSpace(fileNameAndPath))
                    {
                        logger.Warn($"ShortcutItem/ToSmallBitmap2: Bitmap fileNameAndPath is empty! Trying next file in fileNamesAndPaths list.");
                        continue;
                    }

                    if (fileNameAndPath.EndsWith(".ico"))
                    {
                        logger.Trace($"ShortcutItem/ToSmallBitmap2: The file we want to get the image from is an icon file. Attempting to load the icon file from {fileNameAndPath}.");
                        Icon icoIcon = new Icon(fileNameAndPath, 128, 128);
                        logger.Trace($"ShortcutItem/ToSmallBitmap2: Attempting to convert the icon file {fileNameAndPath} to a bitmap.");
                        bm = icoIcon.ToBitmap();
                        logger.Trace($"ShortcutItem/ToSmallBitmap2: Emptying the memory area used but the icon to stop memory leaks.");
                        icoIcon.Dispose();
                    }
                    else
                    {
                        logger.Trace($"ShortcutItem/ToSmallBitmap2: The file {fileNameAndPath} isn't an Icon file, so trying to use GetLargeBitmapFromFile to extract the image.");
                        bm = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, true, false);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, $"ShortcutItem/ToSmallBitmap2: Exception while trying to save the Shortcut icon initially. Trying again with GetLargeBitmapFromFile.");
                    try
                    {
                        logger.Trace($"ShortcutItem/ToSmallBitmap2: Attempt2. The file {fileNameAndPath} isn't an Icon file, so trying to use GetLargeBitmapFromFile to extract the image.");
                        bm = IconFromFile.GetLargeBitmapFromFile(fileNameAndPath, true, false);
                        bmToReturn = bm;
                    }
                    catch (Exception innerex)
                    {
                        logger.Warn(innerex, $"ShortcutItem/ToSmallBitmap2: Exception while trying to save the Shortcut icon a second time. Giving up.");
                        continue;
                    }
                }

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
    }
}
