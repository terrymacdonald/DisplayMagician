using System;
using System.Drawing;
using System.Windows.Forms;
using DisplayMagicianShared.Topology;

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

        private virtual void DrawScreen(Graphics g, ScreenPosition screen)
        {
            //var res = ProfileIcon.NormalizeResolution(screens);
            var rect = new Rectangle(screen.ScreenX, screen.ScreenY, screen.ScreenWidth, screen.ScreenHeight);
            g.FillRectangle(new SolidBrush(Color.FromArgb(15, Color.White)), rect);
            g.DrawRectangle(Pens.Black, rect);

            DrawString(g, path.Position.IsEmpty ? "[Primary]" : $"[{path.Position.X}, {path.Position.Y}]", rect.Size,
                new PointF(rect.X + PaddingY / 2, rect.Y + PaddingY / 2), StringAlignment.Near, StringAlignment.Near);

            var str = $"DISPLAY #{path.SourceId + 1}{Environment.NewLine}{rect.Width}×{rect.Height}";
            var strSize = DrawString(g, str, rect.Size, new PointF(rect.X - PaddingX / 2, rect.Y + PaddingY / 2),
                StringAlignment.Near, StringAlignment.Far);

            var rows = rect.Width < rect.Height ? path.TargetDisplays.Length : 1;
            var cols = rect.Width >= rect.Height ? path.TargetDisplays.Length : 1;

            for (var i = 0; i < path.TargetDisplays.Length; i++)
            {
                DrawTarget(g, path, path.TargetDisplays[i],
                    new Rectangle(rect.X + PaddingX, rect.Y + strSize.Height + PaddingY, rect.Width - 2 * PaddingX,
                        rect.Height - strSize.Height - 2 * PaddingY),
                    rows > 1 ? i : 0, cols > 1 ? i : 0, rows, cols);
            }
        }

        private Size DrawString(
            Graphics g,
            string str,
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
                g.DrawString(str, Font, new SolidBrush(ForeColor), new RectangleF(drawingPoint.Value, drawingSize),
                    format);
            }

            return new Size((int) stringSize.Width, (int) stringSize.Height);
        }

        private virtual void DrawSurroundTopology(Graphics g, PathTarget target, Rectangle rect)
        {
            g.DrawRectangle(Pens.Black, rect);

            var targetSize = new Size(rect.Width / target.SurroundTopology.Columns,
                rect.Height / target.SurroundTopology.Rows);

            for (var i = 0; i < target.SurroundTopology.Displays.Length; i++)
            {
                var display = target.SurroundTopology.Displays[i];
                var row = i / target.SurroundTopology.Columns;
                var col = i % target.SurroundTopology.Columns;
                var targetPosition = new Point(targetSize.Width * col + rect.X, targetSize.Height * row + rect.Y);
                var targetRect = new Rectangle(targetPosition, targetSize);

                g.DrawRectangle(Pens.Black, targetRect);

                switch (display.Rotation)
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
                }

                if (!display.Overlap.IsEmpty)
                {
                    DrawString(g, $"[{-display.Overlap.X}, {-display.Overlap.Y}]", targetRect.Size,
                        new PointF(targetRect.X + PaddingY / 2, targetRect.Y + PaddingY / 2), StringAlignment.Near,
                        StringAlignment.Near);
                }

                // Invert to real monitor resolution
                var res = ProfileIcon.NormalizeResolution(target.SurroundTopology.Resolution, display.Rotation);
                var str = $"{display.DisplayName}{Environment.NewLine}{res.Width}×{res.Height}";
                DrawString(g, str, targetRect.Size, targetRect.Location);
            }
        }

        private virtual void DrawTarget(
            Graphics g,
            Path path,
            PathTarget target,
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
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 106, 185, 0)), targetRect);
            }
            //else if (target.EyefinityTopology != null)
            //    g.FillRectangle(new SolidBrush(Color.FromArgb(150, 99, 0, 0)), targetRect);
            else if (path.TargetDisplays.Length > 1)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 255, 97, 27)), targetRect);
            }
            else if (path.Position == Point.Empty)
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(150, 0, 174, 241)), targetRect);
            }
            else
            {
                g.FillRectangle(new SolidBrush(Color.FromArgb(255, 155, 155, 155)), targetRect);
            }

            g.DrawRectangle(Pens.Black, targetRect);
            var str = $"{target.DisplayName}{Environment.NewLine}{path.Resolution.Width}×{path.Resolution.Height}";

            switch (target.Rotation)
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
            }

            if (target.SurroundTopology != null)
            {
                var strSize = DrawString(g, str, targetRect.Size,
                    new PointF(targetRect.X + PaddingX / 2, targetRect.Y + PaddingY / 2),
                    StringAlignment.Near, StringAlignment.Near);
                DrawSurroundTopology(g, target,
                    new Rectangle(
                        targetRect.X + PaddingX,
                        targetRect.Y + strSize.Height + PaddingY,
                        targetRect.Width - 2 * PaddingX,
                        targetRect.Height - strSize.Height - 2 * PaddingY));
            }
            else
            {
                DrawString(g, str, targetRect.Size, targetRect.Location);
            }
        }

        private void DrawView(Graphics g)
        {
            var viewSize = ProfileIcon.CalculateViewSize(_profile.Screens, true, PaddingX, PaddingY);
            var factor = Math.Min(Width / viewSize.Width, Height / viewSize.Height);
            g.ScaleTransform(factor, factor);

            var xOffset = (Width / factor - viewSize.Width) / 2f;
            var yOffset = (Height / factor - viewSize.Height) / 2f;
            g.TranslateTransform(-viewSize.X + xOffset, -viewSize.Y + yOffset);

            foreach (ScreenPosition screen in _profile.Screens)
            {
                DrawScreen(g, screen);
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