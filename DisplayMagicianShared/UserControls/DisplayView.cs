using System;
using System.Drawing;
using System.Windows.Forms;
using DisplayMagicianShared.Windows;

namespace DisplayMagicianShared.UserControls
{
    public partial class DisplayView : UserControl
    {
        private ProfileItem _profile;

        public DisplayView()
        {
            InitializeComponent();
            ResizeRedraw = true;
        }

        public int PaddingX { get; set; } = 100;
        public int PaddingY { get; set; } = 100;

        public ProfileItem Profile
        {
            get => _profile;
            set
            {
                _profile = value;
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
            var format = new StringFormat(StringFormat.GenericTypographic)
            {
                Alignment = horizontal,
                LineAlignment = vertical,
                FormatFlags = StringFormatFlags.NoClip               
            };
            var stringSize = g.MeasureString(str, Font, drawingSize, format);

            if (drawingPoint != null)
            {
                g.DrawString(str, font, new SolidBrush(colour), new RectangleF(drawingPoint.Value, drawingSize),
                    format);
            }

            return new Size((int) stringSize.Width, (int) stringSize.Height);
        }

        public virtual void DrawSpannedTopology(Graphics g, ScreenPosition screen, Rectangle rect)
        {
            g.DrawRectangle(Pens.Black, rect);

            var targetSize = new Size(rect.Width / screen.SpannedColumns,
                rect.Height / screen.SpannedRows);

            for (var i = 0; i < screen.SpannedScreens.Count; i++)
            {
                var display = screen.SpannedScreens[i];
                var row = i / screen.SpannedColumns;
                var col = i % screen.SpannedColumns;
                var targetPosition = new Point(targetSize.Width * col + rect.X, targetSize.Height * row + rect.Y);
                var targetRect = new Rectangle(targetPosition, targetSize);

                g.DrawRectangle(Pens.Black, targetRect);

                /*switch (display.Rotation)
                {
                    case Rotation.Rotate90:
                        DrawString(g, "90°", targetRect.Size,
                            new PointF(targetRect.X - PaddingX / 2, targetRect.Y + PaddingY / 2), StringAlignment.Near,
                            StringAlignment.Far);

                        break;
                    case Rotation.Rotate180:
                        DrawString(g, "180°", targetRect.Size,
                            new PointF(targetRect.X - PaddingX / 2, targetRect.Y + PaddingY / 2), StringAlignment.Near,
                            StringAlignment.Far);

                        break;
                    case Rotation.Rotate270:
                        DrawString(g, "270°", targetRect.Size,
                            new PointF(targetRect.X - PaddingX / 2, targetRect.Y + PaddingY / 2), StringAlignment.Near,
                            StringAlignment.Far);

                        break;
                }*/

                /*if (!display.Overlap.IsEmpty)
                {
                    DrawString(g, $"[{-display.Overlap.X}, {-display.Overlap.Y}]", targetRect.Size,
                        new PointF(targetRect.X + PaddingY / 2, targetRect.Y + PaddingY / 2), StringAlignment.Near,
                        StringAlignment.Near);
                }*/

                // Invert to real monitor resolution
                //var res = ProfileIcon.NormalizeResolution(target.SurroundTopology.Resolution, display.Rotation);
                /*var str = $"{display.DisplayName}{Environment.NewLine}{res.Width}×{res.Height}";
                DrawString(g, str, targetRect.Size, targetRect.Location);*/
            }
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
            Font normalWordFont = new Font(Font.FontFamily, 55);
            Font bigWordFont = new Font(Font.FontFamily, 80);
            Font hugeWordFont = new Font(Font.FontFamily, 110);


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

            foreach (ScreenPosition screen in _profile.Screens)
            {

                Rectangle screenRect;
                Rectangle outlineRect;

                // draw the screen 
                if (screen.IsSpanned)
                {
                    // We do these things only if the screen IS spanned!                    
                    // Draw the outline of the spanned monitor
                    outlineRect = new Rectangle(screen.ScreenX, screen.ScreenY, screen.ScreenWidth, screen.ScreenHeight);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(255, 33, 33, 33)), outlineRect);
                    g.DrawRectangle(Pens.Black, outlineRect);

                    // Draw the screen of the monitor
                    screenRect = new Rectangle(screen.ScreenX + screenBezel, screen.ScreenY + screenBezel, screen.ScreenWidth - (screenBezel * 2), screen.ScreenHeight - (screenBezel * 2));                   
                    g.FillRectangle(new SolidBrush(screen.Colour), screenRect);
                    g.DrawRectangle(Pens.Black, screenRect);

                    // Temporarily disabling this dotted line as it really isn't great visually.
                    /*foreach (SpannedScreenPosition subScreen in screen.SpannedScreens)
                    {
                        Rectangle spannedScreenRect = new Rectangle(subScreen.ScreenX, subScreen.ScreenY, subScreen.ScreenWidth, subScreen.ScreenHeight);
                        Pen dashedLine = new Pen(Color.Black);
                        dashedLine.DashStyle = System.Drawing.Drawing2D.DashStyle.DashDotDot;
                        g.DrawRectangle(dashedLine, spannedScreenRect);
                    }*/

                }
                else
                {                   
                    // We do these things only if the screen isn't spanned!
                    // Draw the outline of the monitor
                    outlineRect = new Rectangle(screen.ScreenX, screen.ScreenY, screen.ScreenWidth, screen.ScreenHeight);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(255, 33, 33, 33)), outlineRect);
                    g.DrawRectangle(Pens.Black, outlineRect);

                    // Draw the screen of the monitor
                    screenRect = new Rectangle(screen.ScreenX + screenBezel, screen.ScreenY + screenBezel, screen.ScreenWidth - (screenBezel * 2), screen.ScreenHeight - (screenBezel * 2));
                    g.FillRectangle(new SolidBrush(screen.Colour), screenRect);
                    g.DrawRectangle(Pens.Black, screenRect);
                }

                // Draw the location of the taskbar for this screen
                Rectangle taskBarRect;
                Rectangle startButtonRect;
                int taskBarWidth = (int)(0.05 * screenRect.Height);
                int startButtonSpacer = (int)(0.25 * taskBarWidth);
                int startButtonSize = 2 * startButtonSpacer;
                switch (screen.TaskBarEdge)
                {
                    case TaskBarLayout.TaskBarEdge.Left:
                        taskBarRect = new Rectangle(screenRect.X, screenRect.Y + 2, taskBarWidth, screenRect.Height - 4);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                    case TaskBarLayout.TaskBarEdge.Top:
                        taskBarRect = new Rectangle(screenRect.X + 2, screenRect.Y, screenRect.Width - 4, taskBarWidth);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                    case TaskBarLayout.TaskBarEdge.Right:
                        taskBarRect = new Rectangle(screenRect.X + screenRect.Width - taskBarWidth, screenRect.Y + 2, taskBarWidth, screenRect.Height - 4);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                    case TaskBarLayout.TaskBarEdge.Bottom:
                        taskBarRect = new Rectangle(screenRect.X + 2, screenRect.Y + screenRect.Height - taskBarWidth, screenRect.Width - 4, taskBarWidth);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                    default:
                        taskBarRect = new Rectangle(screenRect.X + 2, screenRect.Y + screenRect.Height - taskBarWidth, screenRect.Width - 4, taskBarWidth);
                        startButtonRect = new Rectangle(taskBarRect.X + startButtonSpacer, taskBarRect.Y + startButtonSpacer, startButtonSize, startButtonSize);
                        break;
                }
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 200, 200, 200)), taskBarRect);
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 3, 194, 252)), startButtonRect);
                //g.DrawRectangle(Pens.Black, outlineRect);

                Rectangle wordRect = new Rectangle(screen.ScreenX + screenBezel + screenWordBuffer, screen.ScreenY + screenBezel + screenWordBuffer, screen.ScreenWidth - (screenBezel * 2) - (screenWordBuffer * 2), screen.ScreenHeight - (screenBezel * 2) - (screenWordBuffer * 2));
                Color wordTextColour = pickTextColorBasedOnBgColour(screen.Colour, lightTextColour, darkTextColour);
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
                    str = $"({screen.AdapterName}){Environment.NewLine}";
                }
                str += $"{screen.Name}{Environment.NewLine}";
                str += $"{screen.ScreenWidth}×{ screen.ScreenHeight}{ Environment.NewLine}{ screen.DisplayConnector}";
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

        private void DrawEmptyView(Graphics g)
        {
            RectangleF rect = g.VisibleClipBounds;
            g.FillRectangle(new SolidBrush(Color.FromArgb(15, Color.White)), rect);
            g.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width, rect.Height);
        }
    }
}