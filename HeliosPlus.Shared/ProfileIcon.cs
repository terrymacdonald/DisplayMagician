using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.IconLib;
using System.Drawing.Imaging;
using System.Linq;
using HeliosPlus.Shared.Topology;

namespace HeliosPlus.Shared
{
    public class ProfileIcon
    {

        private Profile _profile;

        public ProfileIcon(Profile profile, int paddingX = 100, int paddingY = 100)
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
            ProfileViewport[] paths,
            bool withPadding = false,
            int paddingX = 0,
            int paddingY = 0)
        {
            var minX = 0;
            var maxX = 0;
            var minY = 0;
            var maxY = 0;

            foreach (var path in paths)
            {
                var res = NormalizeResolution(path);
                minX = Math.Min(minX, path.Position.X);
                maxX = Math.Max(maxX, res.Width + path.Position.X);
                minY = Math.Min(minY, path.Position.Y);
                maxY = Math.Max(maxY, res.Height + path.Position.Y);
            }

            if (withPadding)
            {
                minX -= paddingX;
                maxX += paddingX;
                minY -= paddingY;
                maxY += paddingY;
            }

            var size = new SizeF(Math.Abs(minX) + maxX, Math.Abs(minY) + maxY);
            var rect = new RectangleF(new PointF(minX, minY), size);

            return rect;
        }

        public static Size NormalizeResolution(Size resolution, Rotation rotation)
        {
            if (rotation == Rotation.Rotate90 || rotation == Rotation.Rotate270)
            {
                return new Size(resolution.Height, resolution.Width);
            }

            return resolution;
        }

        public static Size NormalizeResolution(ProfileViewport path)
        {
            var bigest = Size.Empty;

            foreach (var target in path.TargetDisplays)
            {
                var res = NormalizeResolution(path.Resolution, target.Rotation);

                if ((ulong) res.Width * (ulong) res.Height > (ulong) bigest.Width * (ulong) bigest.Height)
                {
                    bigest = res;
                }
            }

            return bigest.IsEmpty ? path.Resolution : bigest;
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

        public Bitmap ToBitmap(int width = 128, int height = 128, PixelFormat format = PixelFormat.Format32bppArgb)
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

        public Bitmap ToBitmapOverlay(Bitmap bitmap, int width = 0, int height = 0, PixelFormat format = PixelFormat.Format32bppArgb)
        {

            if (width == 0)
                width = bitmap.Width;

            if (height == 0)
                height = bitmap.Height;

            var viewSize = CalculateViewSize(_profile.Viewports, true, PaddingX, PaddingY);
            int viewSizeRatio = (int) Math.Round(viewSize.Width * viewSize.Height);
            int overlayWidth = (int) Math.Round(width * 0.7f,0);
            int overlayHeight = overlayWidth / viewSizeRatio;
            int overlayX = width - overlayWidth;
            int overlayY = height - overlayHeight;
            Point overlayPosition = new Point(overlayX, overlayY);
            Size overlaySize = new Size(overlayWidth, overlayHeight);
            Rectangle overlayRect = new Rectangle(overlayPosition, overlaySize);
            //var width = bitmap.Width * 0.7f;
            //var height = width / viewSize.Width * viewSize.Height;

            var combinedBitmap = new Bitmap(width, height, format);
            combinedBitmap.MakeTransparent();

            using (var g = Graphics.FromImage(combinedBitmap))
            {
                g.SmoothingMode = SmoothingMode.HighQuality;
                //g.DrawImage(bitmap, 0, 0, width, height);
                g.TranslateTransform(overlayX, overlayY);
                //Rectangle compressionRectangle = new Rectangle(300, 10,
                //myBitmap.Width / 2, myBitmap.Height / 2);
                g.DrawRectangle(new Pen(Color.FromArgb(125, 50, 50, 50), 2f), overlayRect);

                DrawView(g, overlayWidth, overlayHeight);
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

        private void DrawPath(Graphics g, ProfileViewport path)
        {
            var res = NormalizeResolution(path);
            var rect = new Rectangle(path.Position, res);
            var rows = rect.Width < rect.Height ? path.TargetDisplays.Length : 1;
            var cols = rect.Width >= rect.Height ? path.TargetDisplays.Length : 1;

            for (var i = 0; i < path.TargetDisplays.Length; i++)
            {
                DrawTarget(g, path, path.TargetDisplays[i],
                    new Rectangle(
                        rect.X + PaddingX,
                        rect.Y + PaddingY,
                        rect.Width - 2 * PaddingX,
                        rect.Height - 2 * PaddingY),
                    rows > 1 ? i : 0, cols > 1 ? i : 0, rows, cols);
            }
        }

        // ReSharper disable once TooManyArguments
        private void DrawTarget(
            Graphics g,
            ProfileViewport path,
            ProfileViewportTargetDisplay target,
            Rectangle rect,
            int row,
            int col,
            int rows,
            int cols)
        {
            var targetSize = new Size(rect.Width / cols, rect.Height / rows);
            var targetPosition = new Point(targetSize.Width * col + rect.X, targetSize.Height * row + rect.Y);
            var targetRect = new Rectangle(targetPosition, targetSize);

            if (target.SurroundTopology != null)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 106, 185, 0)), targetRect);
            }
            //else if (target.EyefinityTopology != null)
            //    g.FillRectangle(new SolidBrush(Color.FromArgb(255, 99, 0, 0)), targetRect);
            else if (path.TargetDisplays.Length > 1)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 255, 97, 27)), targetRect);
            }
            else if (path.Position == Point.Empty)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 0, 174, 241)), targetRect);
            }
            else
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 155, 155, 155)), targetRect);
            }

            g.DrawRectangle(new Pen(Color.FromArgb(125, 50, 50, 50), 2f), targetRect);
        }

        private void DrawView(Graphics g, float width, float height)
        {
            var viewSize = CalculateViewSize(_profile.Viewports, true, PaddingX, PaddingY);
            var standPadding = height * 0.005f;
            height -= standPadding * 8;
            var factor = Math.Min((width - 2 * standPadding - 1) / viewSize.Width,
                (height - 2 * standPadding - 1) / viewSize.Height);
            g.ScaleTransform(factor, factor);

            var xOffset = ((width - 1) / factor - viewSize.Width) / 2f;
            var yOffset = ((height - 1) / factor - viewSize.Height) / 2f;
            g.TranslateTransform(-viewSize.X + xOffset, -viewSize.Y + yOffset);

            if (standPadding * 6 >= 1)
            {
                using (var boundRect = RoundedRect(viewSize, 2 * standPadding / factor))
                {
                    g.FillPath(new SolidBrush(Color.FromArgb(200, 255, 255, 255)), boundRect);
                    g.DrawPath(new Pen(Color.FromArgb(170, 50, 50, 50), standPadding / factor), boundRect);
                }

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

            foreach (var path in _profile.Viewports)
            {
                DrawPath(g, path);
            }
        }
    }
}