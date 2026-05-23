using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using DisplayMagicianShared.Windows;

namespace DisplayMagicianShared.UserControls
{
    public partial class DisplayView : UserControl
    {
        private ProfileItem _profile;
        private readonly Dictionary<string, Bitmap> _wallpaperCache = new Dictionary<string, Bitmap>();
        private readonly Dictionary<string, Color> _wallpaperAvgColorCache = new Dictionary<string, Color>();

        public DisplayView()
        {
            InitializeComponent();
            ResizeRedraw = true;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(100)]
        public int PaddingX { get; set; } = 100;
        
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(100)]
        public int PaddingY { get; set; } = 100;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ProfileItem Profile
        {
            get => _profile;
            set
            {
                // Dispose previously cached wallpaper bitmaps
                foreach (var bmp in _wallpaperCache.Values)
                    bmp?.Dispose();
                _wallpaperCache.Clear();
                _wallpaperAvgColorCache.Clear();

                _profile = value;

                // Pre-load wallpaper thumbnails when mode is Apply
                if (_profile?.WallpaperConfiguration?.WallpaperMode == Wallpaper.Mode.Apply)
                {
                    foreach (var mon in _profile.WallpaperConfiguration.MonitorWallpapers)
                    {
                        if (!string.IsNullOrEmpty(mon.WallpaperFilePath) &&
                            File.Exists(mon.WallpaperFilePath) &&
                            !_wallpaperCache.ContainsKey(mon.WallpaperFilePath))
                        {
                            try
                            {
                                var bmp = new Bitmap(mon.WallpaperFilePath);
                                _wallpaperCache[mon.WallpaperFilePath] = bmp;
                                _wallpaperAvgColorCache[mon.WallpaperFilePath] = GetAverageBitmapColor(bmp);
                            }
                            catch { /* ignore – solid colour fallback will be used */ }
                        }
                    }
                }

                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_profile != null)
            {
                DrawView(e.Graphics);
            }
            else
            {
                DrawEmptyView(e.Graphics);
            }
        }

        private Size DrawString(
            Graphics g,
            string str,
            Color colour,
            Font font,
            SizeF drawingSize = default(SizeF),
            PointF? drawingPoint = null,
            StringAlignment vertical = StringAlignment.Center,
            StringAlignment horizontal = StringAlignment.Center)
        {
            using var format = new StringFormat(StringFormat.GenericTypographic)
            {
                Alignment = horizontal,
                LineAlignment = vertical,
                FormatFlags = StringFormatFlags.NoClip               
            };
            var stringSize = g.MeasureString(str, Font, drawingSize, format);

            if (drawingPoint != null)
            {
                using (var textBrush = new SolidBrush(colour))
                    g.DrawString(str, font, textBrush, new RectangleF(drawingPoint.Value, drawingSize), format);
            }

            return new Size((int) stringSize.Width, (int) stringSize.Height);
        }

        private void DrawView(Graphics g)
        {

            var viewSize = ProfileIcon.CalculateViewSize(_profile.Screens, PaddingX, PaddingY);
            var factor = Math.Min(Width / viewSize.Width, Height / viewSize.Height);
            g.ScaleTransform(factor, factor);

            var xOffset = (Width / factor - viewSize.Width) / 2f;
            var yOffset = (Height / factor - viewSize.Height) / 2f;
            g.TranslateTransform(-viewSize.X + xOffset, -viewSize.Y + yOffset);

            // How wide the Bezel is on the screen graphics
            int screenBezel = 60;
            int screenWordBuffer = 60;

            Color lightTextColour = Color.White;
            Color darkTextColour = Color.Black;

            Font selectedWordFont;
            using Font normalWordFont = new Font(Font.FontFamily, 55);
            using Font bigWordFont = new Font(Font.FontFamily, 80);
            using Font hugeWordFont = new Font(Font.FontFamily, 110);


            // Figure out the sized font we need
            if (g.VisibleClipBounds.Width > 10000 || g.VisibleClipBounds.Height > 4000)
            {
                selectedWordFont = hugeWordFont;
            }
            else if (g.VisibleClipBounds.Width > 6000 || g.VisibleClipBounds.Height > 3500)
            {
                selectedWordFont = bigWordFont;
            }
            else
            {
                selectedWordFont = normalWordFont;
            }

            // Build a bounds→bitmap and bounds→avgColor lookup for wallpaper preview (Apply mode only;
            // DoNothing and Clear keep the existing solid colour)
            var wallpaperByBounds = new Dictionary<(int x, int y, int w, int h), Bitmap>();
            var wallpaperAvgColorByBounds = new Dictionary<(int x, int y, int w, int h), Color>();
            if (_profile.WallpaperConfiguration?.WallpaperMode == Wallpaper.Mode.Apply)
            {
                foreach (var mon in _profile.WallpaperConfiguration.MonitorWallpapers)
                {
                    int bw = mon.MonitorBounds.right - mon.MonitorBounds.left;
                    int bh = mon.MonitorBounds.bottom - mon.MonitorBounds.top;
                    var key = (mon.MonitorBounds.left, mon.MonitorBounds.top, bw, bh);
                    if (_wallpaperCache.TryGetValue(mon.WallpaperFilePath ?? "", out Bitmap cached))
                        wallpaperByBounds[key] = cached;
                    if (_wallpaperAvgColorCache.TryGetValue(mon.WallpaperFilePath ?? "", out Color avgColor))
                        wallpaperAvgColorByBounds[key] = avgColor;
                }
            }

            foreach (ScreenPosition screen in _profile.Screens)
            {

                Rectangle screenRect;
                Rectangle outlineRect;

                // draw the screen 
                // We do these things only if the screen isn't spanned!
                // Draw the outline of the monitor
                outlineRect = new Rectangle(screen.ScreenX, screen.ScreenY, screen.ScreenWidth, screen.ScreenHeight);
                using (var outlineBrush = new SolidBrush(Color.FromArgb(255, 33, 33, 33)))
                    g.FillRectangle(outlineBrush, outlineRect);
                g.DrawRectangle(Pens.Black, outlineRect);

                // Draw the screen of the monitor (wallpaper thumbnail when available, solid colour otherwise)
                screenRect = new Rectangle(screen.ScreenX + screenBezel, screen.ScreenY + screenBezel, screen.ScreenWidth - (screenBezel * 2), screen.ScreenHeight - (screenBezel * 2));
                if (wallpaperByBounds.TryGetValue((screen.ScreenX, screen.ScreenY, screen.ScreenWidth, screen.ScreenHeight), out Bitmap wallpaperBmp))
                    g.DrawImage(wallpaperBmp, screenRect);
                else
                    using (var screenBrush = new SolidBrush(screen.Colour))
                        g.FillRectangle(screenBrush, screenRect);
                g.DrawRectangle(Pens.Black, screenRect);

                // Draw the location of the taskbar for this screen
                Rectangle taskBarRect;
                Rectangle startButtonRect;
                int taskBarWidth = (int)(0.05 * screenRect.Height);
                int startButtonSpacer = (int)(0.25 * taskBarWidth);
                int startButtonSize = 2 * startButtonSpacer;
                switch (screen.TaskbarPosition)
                {
                    case TaskbarPosition.Left:
                        taskBarRect = new Rectangle(screenRect.X, screenRect.Y + 2, taskBarWidth, screenRect.Height - 4);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                    case TaskbarPosition.Top:
                        taskBarRect = new Rectangle(screenRect.X + 2, screenRect.Y, screenRect.Width - 4, taskBarWidth);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                    case TaskbarPosition.Right:
                        taskBarRect = new Rectangle(screenRect.X + screenRect.Width - taskBarWidth, screenRect.Y + 2, taskBarWidth, screenRect.Height - 4);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                    case TaskbarPosition.Bottom:
                        taskBarRect = new Rectangle(screenRect.X + 2, screenRect.Y + screenRect.Height - taskBarWidth, screenRect.Width - 4, taskBarWidth);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                    default:
                        taskBarRect = new Rectangle(screenRect.X + 2, screenRect.Y + screenRect.Height - taskBarWidth, screenRect.Width - 4, taskBarWidth);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                }
                using (var taskBarBrush = new SolidBrush(Color.FromArgb(255, 200, 200, 200)))
                    g.FillRectangle(taskBarBrush, taskBarRect);
                using (var startButtonBrush = new SolidBrush(Color.FromArgb(255, 3, 194, 252)))
                    g.FillRectangle(startButtonBrush, startButtonRect);
                //g.DrawRectangle(Pens.Black, outlineRect);

                Rectangle wordRect = new Rectangle(screen.ScreenX + screenBezel + screenWordBuffer, screen.ScreenY + screenBezel + screenWordBuffer, screen.ScreenWidth - (screenBezel * 2) - (screenWordBuffer * 2), screen.ScreenHeight - (screenBezel * 2) - (screenWordBuffer * 2));
                Color bgForContrast = wallpaperAvgColorByBounds.TryGetValue(
                    (screen.ScreenX, screen.ScreenY, screen.ScreenWidth, screen.ScreenHeight), out Color wallpaperAvg)
                    ? wallpaperAvg
                    : screen.Colour;
                Color wordTextColour = pickTextColorBasedOnBgColour(bgForContrast, lightTextColour, darkTextColour);
                // Draw the name of the screen and the size of it
                string str = $"";
                if (screen.IsPrimary)
                {
                    str = $"{screen.Library} Primary Display{Environment.NewLine}";
                }
                else
                {
                    str = $"{screen.Library} Display{Environment.NewLine}";
                }
                if (!String.IsNullOrEmpty(screen.AdapterName))
                {
                    str += $"({screen.AdapterName}){Environment.NewLine}";
                }
                str += $"{screen.Name}{Environment.NewLine}";
                // Resolution with optional refresh rate
                if (screen.RefreshRateHz > 0)
                    str += $"{screen.ScreenWidth}×{screen.ScreenHeight} @ {screen.RefreshRateHz:0.###}hz{Environment.NewLine}";
                else
                    str += $"{screen.ScreenWidth}×{screen.ScreenHeight}{Environment.NewLine}";
                // Colour encoding / depth
                if (!string.IsNullOrEmpty(screen.ColorEncoding) && screen.BitsPerColorChannel > 0)
                    str += $"{screen.ColorEncoding} {screen.BitsPerColorChannel * 3}-bit{Environment.NewLine}";
                str += screen.DisplayConnector;
                if (screen.IsClone)
                {
                    str += $"(+{screen.ClonedCopies-1} Clone)";
                }

                DrawString(g, str, wordTextColour, selectedWordFont, wordRect.Size, wordRect.Location);

                // Draw the position of the screen
                str = $"[{screen.ScreenX},{screen.ScreenY}]";
                DrawString(g, str, wordTextColour, selectedWordFont, wordRect.Size, wordRect.Location, StringAlignment.Near, StringAlignment.Near);
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

        private static Color GetAverageBitmapColor(Bitmap bmp)
        {
            using var small = new Bitmap(1, 1);
            using var g = Graphics.FromImage(small);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(bmp, 0, 0, 1, 1);
            return small.GetPixel(0, 0);
        }

        private void DrawEmptyView(Graphics g)
        {
            RectangleF rect = g.VisibleClipBounds;
            using (var emptyBrush = new SolidBrush(BackColor))
                g.FillRectangle(emptyBrush, rect);
            g.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}