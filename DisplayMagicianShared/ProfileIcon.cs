using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.IconLib;
using System.Drawing.Imaging;
using System.Linq;

namespace DisplayMagicianShared
{
    public class ProfileIcon
    {

        private ProfileItem _profile;

        public ProfileIcon(ProfileItem profile, int paddingX = 100, int paddingY = 100)
        {
            _profile = profile;
            PaddingX = paddingX;
            PaddingY = paddingY;
        }

        public int PaddingX { get; }
        public int PaddingY { get; }

        //public Profile Profile { get; }

        // ReSharper disable once TooManyArguments
        public static RectangleF CalculateViewSize(
            List<ScreenPosition> screens,
            int outsidePaddingSides = 0,
            int outsidePaddingTopBottom = 0)
        {
            var minX = 0;
            var maxX = 0;
            var minY = 0;
            var maxY = 0;

            foreach (var screen in screens)
            {
                //var res = GetLargestResolution(screens);
                minX = Math.Min(minX, screen.ScreenX);
                maxX = Math.Max(maxX, screen.ScreenX + screen.ScreenWidth);
                minY = Math.Min(minY, screen.ScreenY);
                maxY = Math.Max(maxY, screen.ScreenY + screen.ScreenHeight);
            }

            if (outsidePaddingSides != 0)
            {
                minX -= outsidePaddingSides;
                maxX += outsidePaddingSides;
                minY -= outsidePaddingTopBottom;
                maxY += outsidePaddingTopBottom;
            }

            var size = new SizeF(Math.Abs(minX) + maxX, Math.Abs(minY) + maxY);
            var rect = new RectangleF(new PointF(minX, minY), size);

            return rect;
        }
       

        /// <summary>
        ///     Creates a rounded rectangle path
        ///     By @taffer
        ///     https://stackoverflow.com/questions/33853434/how-to-draw-a-rounded-rectangle-in-c-sharp
        /// </summary>
        public static GraphicsPath RoundedRect(RectangleF bounds, float radius)
        {
            var diameter = radius * 2;
            var size = new SizeF(diameter, diameter);
            var arc = new RectangleF(bounds.Location, size);
            var path = new GraphicsPath();

            if (radius < 0.01)
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

        public Bitmap ToBitmap(int width = 256, int height = 256, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            var bitmap = new Bitmap(width, height, format);
            bitmap.MakeTransparent();

            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                DrawView(g, width, height);
            }

            return bitmap;
        }

        public Bitmap ToTightestBitmap(int width = 256, int height = 0, PixelFormat format = PixelFormat.Format32bppArgb)
        {
            var viewSize = CalculateViewSize(_profile.Screens, 0, 0);
            double viewSizeRatio = viewSize.Width / viewSize.Height;

            if (height == 0)
                height = Convert.ToInt32((double)width / viewSizeRatio);

            var bitmap = new Bitmap(width, height, format);
            bitmap.MakeTransparent();

            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                DrawView(g, width, height);
            }

            return bitmap;
        }

        public Bitmap ToBitmapOverlay(Bitmap bitmap)
        {
            var viewSize = CalculateViewSize(_profile.Screens, PaddingX, PaddingY);
            var width = bitmap.Width * 0.7f;
            var height = width / viewSize.Width * viewSize.Height;

            using (var g = Graphics.FromImage(bitmap))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.TranslateTransform(bitmap.Width - width, bitmap.Height - height * 1.1f);
                DrawView(g, width, height);
            }

            return bitmap;
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
                icon.Add(ToBitmap(size.Width, size.Height));

                if (size.Width >= 256 && size.Height >= 256)
                {
                    icon[icon.Count - 1].IconImageFormat = IconImageFormat.PNG;
                }
            }

            multiIcon.SelectedIndex = 0;

            return multiIcon;
        }

        public MultiIcon ToIconOverlay(string iconAddress)
        {
            var multiIcon = new MultiIcon();
            var icon = multiIcon.Add("Icon1");
            var mainIcon = new MultiIcon();
            mainIcon.Load(iconAddress);

            foreach (var singleIcon in mainIcon[0].Where(image =>
                    image.PixelFormat == PixelFormat.Format16bppRgb565 ||
                    image.PixelFormat == PixelFormat.Format24bppRgb ||
                    image.PixelFormat == PixelFormat.Format32bppArgb)
                .OrderByDescending(
                    image =>
                        image.PixelFormat == PixelFormat.Format16bppRgb565
                            ? 1
                            : image.PixelFormat == PixelFormat.Format24bppRgb
                                ? 2
                                : 3)
                .ThenByDescending(image => image.Size.Width * image.Size.Height))
            {
                if (!icon.All(i => singleIcon.Size != i.Size || singleIcon.PixelFormat != i.PixelFormat))
                {
                    continue;
                }

                var bitmap = singleIcon.Icon.ToBitmap();

                if (bitmap.PixelFormat != singleIcon.PixelFormat)
                {
                    var clone = new Bitmap(bitmap.Width, bitmap.Height, singleIcon.PixelFormat);

                    using (var gr = Graphics.FromImage(clone))
                    {
                        gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
                    }

                    bitmap.Dispose();
                    bitmap = clone;
                }

                icon.Add(singleIcon.Size.Height * singleIcon.Size.Width < 24 * 24 ? bitmap : ToBitmapOverlay(bitmap));

                if (singleIcon.Size.Width >= 256 && singleIcon.Size.Height >= 256)
                {
                    icon[icon.Count - 1].IconImageFormat = IconImageFormat.PNG;
                }

                bitmap.Dispose();
            }

            if (icon.Count == 0)
            {
                throw new ArgumentException();
            }

            multiIcon.SelectedIndex = 0;

            return multiIcon;
        }


        private void DrawView(Graphics g, float width, float height)
        {
            // Figure out the sizes we need based on the total size of the screens
            var viewSize = ProfileIcon.CalculateViewSize(_profile.Screens, PaddingX, PaddingY);
            var standPadding = height * 0.005f;
            height -= standPadding * 8;
            var factor = Math.Min((width - 2 * standPadding - 1) / viewSize.Width,
                (height - 2 * standPadding - 1) / viewSize.Height);
            g.ScaleTransform(factor, factor);

            // Make space for the stand
            var xOffset = ((width - 1) / factor - viewSize.Width) / 2f;
            var yOffset = ((height - 1) / factor - viewSize.Height) / 2f;
            g.TranslateTransform(-viewSize.X + xOffset, -viewSize.Y + yOffset);

            // How wide the Bezel is on the screen graphics
            int screenBezel = 60;
            //int screenWordBuffer = 30;

            // Draw the stand
            if (standPadding * 6 >= 1)
            {
                using (
                    var boundRect =
                        RoundedRect(
                            new RectangleF(viewSize.Width * 0.375f + viewSize.X,
                                viewSize.Height + standPadding / factor,
                                viewSize.Width / 4, standPadding * 7 / factor), 2 * standPadding / factor))
                {
                    g.FillPath(new SolidBrush(Color.FromArgb(250, 50, 50, 50)), boundRect);
                    g.DrawPath(new Pen(Color.FromArgb(50, 255, 255, 255), 2 / factor), boundRect);
                }
            }
            else
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), viewSize);
                g.DrawRectangle(new Pen(Color.FromArgb(170, 50, 50, 50), standPadding / factor), viewSize.X, viewSize.Y,
                    viewSize.Width, viewSize.Height);
            }

            // Now go through and draw the screens
            foreach (ScreenPosition screen in _profile.Screens)
            {

                // draw the screen 
                if (screen.IsSpanned)
                {
                    // We do these things only if the screen IS spanned!
                    // Draw the outline of the spanned monitor
                    Rectangle outlineRect = new Rectangle(screen.ScreenX, screen.ScreenY, screen.ScreenWidth, screen.ScreenHeight);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(255, 33, 33, 33)), outlineRect);
                    g.DrawRectangle(Pens.Black, outlineRect);

                    // Draw the screen of the monitor
                    Rectangle screenRect = new Rectangle(screen.ScreenX + screenBezel, screen.ScreenY + screenBezel, screen.ScreenWidth - (screenBezel * 2), screen.ScreenHeight - (screenBezel * 2));

                    g.FillRectangle(new SolidBrush(screen.Colour), screenRect);
                    g.DrawRectangle(Pens.Black, screenRect);
                }
                else
                {

                    // Draw the outline of the monitor
                    Rectangle outlineRect = new Rectangle(screen.ScreenX, screen.ScreenY, screen.ScreenWidth, screen.ScreenHeight);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(255, 33, 33, 33)), outlineRect);
                    g.DrawRectangle(Pens.Black, outlineRect);

                    // Draw the screen of the monitor
                    Rectangle screenRect = new Rectangle(screen.ScreenX + screenBezel, screen.ScreenY + screenBezel, screen.ScreenWidth - (screenBezel * 2), screen.ScreenHeight - (screenBezel * 2));
                    
                    g.FillRectangle(new SolidBrush(screen.Colour), screenRect);
                    g.DrawRectangle(Pens.Black, screenRect);

                }
            }
        }


        private Color pickTextColorBasedOnBgColour(Color bgColour, Color lightColour, Color darkColour)
        {
            if ((bgColour.R * 0.299 + bgColour.G * 0.587 + bgColour.B * 0.114) > 186)
            {
                return darkColour;
            }
            else
            {
                return lightColour;
            }
        }

        private Bitmap pickBitmapBasedOnBgColour(Color bgColour, Bitmap lightBitmap, Bitmap darkBitmap)
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
    }
}