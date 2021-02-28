using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manina.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public class ShortcutILVRenderer : ImageListView.ImageListViewRenderer
    {
        // Returns item size for the given view mode.
        public override Size MeasureItem(View view)
        {
            Size itemPadding = new Size(4, 4);
            Size sz = ImageListView.ThumbnailSize +
                      itemPadding + itemPadding;
            return sz;

            /*if (view == View.Thumbnails)
             {
                 //Size itemPadding = new Size(4, 4);
                 //Size sz = ImageListView.ThumbnailSize +
                 //          itemPadding + itemPadding;
                 Size sz = ImageListView.ThumbnailSize;
                 return sz;
             }
             else
                 return base.MeasureItem(view);*/
        }
        // Draws the background of the control.
        public override void DrawBackground(Graphics g, Rectangle bounds)
        {
            if (ImageListView.View == View.Thumbnails)
                g.Clear(Color.FromArgb(255, 255, 255));
            else
                base.DrawBackground(g, bounds);
        }
        // Draws the specified item on the given graphics.
        public override void DrawItem(Graphics g, ImageListViewItem item,
            ItemState state, Rectangle bounds)
        {
            Size itemPadding = new Size(4, 4);
            bool alternate = (item.Index % 2 == 1);

            // Paint background
            if (ImageListView.Enabled)
            {
                using (Brush bItemBack = new SolidBrush(alternate && ImageListView.View == View.Details ?
                    ImageListView.Colors.AlternateBackColor : ImageListView.Colors.BackColor))
                {
                    g.FillRectangle(bItemBack, bounds);
                }
            }
            else
            {
                using (Brush bItemBack = new SolidBrush(ImageListView.Colors.DisabledBackColor))
                {
                    g.FillRectangle(bItemBack, bounds);
                }
            }

            // Paint background Disabled
            /*if ((state & ItemState.Disabled) != ItemState.None)
            {
                using (Brush bDisabled = new LinearGradientBrush(bounds, ImageListView.Colors.DisabledColor1, ImageListView.Colors.DisabledColor2, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bDisabled, bounds, 4);
                }
            }*/

            // Paint background Selected
            /*if ((ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None)) ||
                (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None) && ((state & ItemState.Hovered) != ItemState.None)))*/
            if ((state & ItemState.Selected) != ItemState.None)
            {
                //using (Brush bSelected = new LinearGradientBrush(bounds, ImageListView.Colors.SelectedColor1, ImageListView.Colors.SelectedColor2, LinearGradientMode.Vertical))
                using (Brush bSelected = new LinearGradientBrush(bounds, Color.WhiteSmoke, Color.LightGray, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bSelected, bounds, 12);
                }
            }

            // Paint background unfocused
            /*else if (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
            {
                using (Brush bGray64 = new LinearGradientBrush(bounds, ImageListView.Colors.UnFocusedColor1, ImageListView.Colors.UnFocusedColor2, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bGray64, bounds, 4);
                }
            }*/

            // Paint background Hovered
            /*if ((state & ItemState.Hovered) != ItemState.None)
            {
                using (Brush bHovered = new LinearGradientBrush(bounds, ImageListView.Colors.HoverColor1, ImageListView.Colors.HoverColor2, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bHovered, bounds, 4);
                }
            }*/

            // Draw the image
            Image img = RoundCorners(item.GetCachedImage(CachedImageType.Thumbnail),20);
            if (img != null)
            {
                Rectangle pos = Utility.GetSizedImageBounds(img, new Rectangle(bounds.Location + itemPadding, ImageListView.ThumbnailSize));
                g.DrawImage(img, pos);
                // Draw image border
                if (Math.Min(pos.Width, pos.Height) > 32)
                {
                    using (Pen pOuterBorder = new Pen(ImageListView.Colors.ImageOuterBorderColor))
                    {
                        //g.DrawRectangle(pOuterBorder, pos);
                        DrawRoundedRectangle(g, pOuterBorder, pos,9);
                    }
                    /*if (System.Math.Min(ImageListView.ThumbnailSize.Width, ImageListView.ThumbnailSize.Height) > 32)
                    {
                        using (Pen pInnerBorder = new Pen(ImageListView.Colors.ImageInnerBorderColor))
                        {
                            g.DrawRectangle(pInnerBorder, Rectangle.Inflate(pos, -1, -1));
                        }
                    }*/
                }
            }

            // Draw item text
            Color foreColor = ImageListView.Colors.ForeColor;
            if ((state & ItemState.Disabled) != ItemState.None)
            {
                foreColor = ImageListView.Colors.DisabledForeColor;
            }
            else if ((state & ItemState.Selected) != ItemState.None)
            {
                if (ImageListView.Focused)
                    foreColor = ImageListView.Colors.SelectedForeColor;
                else
                    foreColor = ImageListView.Colors.UnFocusedForeColor;
            }


            // Item border
            /*if (ImageListView.View != View.Details)
            {
                //using (Pen pWhite128 = new Pen(Color.FromArgb(255, ImageListView.Colors.ControlBackColor)))
                using (Pen pWhite128 = new Pen(Color.FromArgb(255, ImageListView.Colors.ControlBackColor)))
                {
                    Utility.DrawRoundedRectangle(g, pWhite128, bounds.Left + 1, bounds.Top + 1, bounds.Width - 3, bounds.Height - 3, (ImageListView.View == View.Details ? 2 : 4));
                }
            }*/
            /*if (((state & ItemState.Disabled) != ItemState.None))
            {
                using (Pen pHighlight128 = new Pen(ImageListView.Colors.DisabledBorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pHighlight128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                }
            }*/
            /*            else if (ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
                        {
                            using (Pen pHighlight128 = new Pen(ImageListView.Colors.SelectedBorderColor))
                            {
                                Utility.DrawRoundedRectangle(g, pHighlight128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                            }
                        }*/
            /*else if (!ImageListView.Focused && ((state & ItemState.Selected) != ItemState.None))
            {
                using (Pen pGray128 = new Pen(ImageListView.Colors.UnFocusedBorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pGray128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                }
            }*/
            if ((state & ItemState.Selected) != ItemState.None)
            {
                /*using (Pen pGray128 = new Pen(ImageListView.Colors.UnFocusedBorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pGray128, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                }*/
                using (Pen pSelectedBorder = new Pen(Color.Brown,4))
                {
                    //DrawRoundedRectangle(g, pSelectedBorder, bounds, 9);
                    Utility.DrawRoundedRectangle(g, pSelectedBorder, bounds.Left+3, bounds.Top+3, bounds.Width - 5, bounds.Height - 5, 10);
                }
            }
            /*else if (ImageListView.View != View.Details && (state & ItemState.Selected) == ItemState.None)
            {
                using (Pen pGray64 = new Pen(ImageListView.Colors.BorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pGray64, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                }
            }*/

            /*if (ImageListView.Focused && ((state & ItemState.Hovered) != ItemState.None))
            {
                using (Pen pHighlight64 = new Pen(ImageListView.Colors.HoverBorderColor))
                {
                    Utility.DrawRoundedRectangle(g, pHighlight64, bounds.Left, bounds.Top, bounds.Width - 1, bounds.Height - 1, 4);
                }
            }

            // Focus rectangle
            if (ImageListView.Focused && ((state & ItemState.Focused) != ItemState.None))
            {
                //ControlPaint.DrawFocusRectangle(g, bounds);
            }*/
        }

        public Image RoundCorners(Image StartImage, int CornerRadius)
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

        // Draws the selection rectangle.
        public override void DrawSelectionRectangle(Graphics g, Rectangle selection)
        {
            using (Brush b = new HatchBrush(
                HatchStyle.DarkDownwardDiagonal,
                Color.FromArgb(128, Color.Black),
                Color.FromArgb(128, SystemColors.Highlight)))
            {
                g.FillRectangle(b, selection);
            }
            using (Pen p = new Pen(SystemColors.Highlight))
            {
                g.DrawRectangle(p, selection.X, selection.Y,
                    selection.Width, selection.Height);
            }
        }

        public void DrawRoundedRectangle(Graphics graphics, Pen pen, Rectangle bounds, int cornerRadius)
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

        public void FillRoundedRectangle(Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
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

        public GraphicsPath RoundedRect(Rectangle bounds, int radius)
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
    }
}
