using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Manina.Windows.Forms;

namespace DisplayMagician.UIForms
{
#pragma warning disable CS3009 // Base type is not CLS-compliant
    public class ShortcutILVRenderer : ImageListView.ImageListViewRenderer
#pragma warning restore CS3009 // Base type is not CLS-compliant
    {
        // Returns item size for the given view mode.
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public override Size MeasureItem(View view)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
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
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public override void DrawItem(Graphics g, ImageListViewItem item, ItemState state, Rectangle bounds)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            if (g is null)
            {
                throw new ArgumentNullException(nameof(g));
            }

            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

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

            if ((state & ItemState.Selected) != ItemState.None)
            {
                //using (Brush bSelected = new LinearGradientBrush(bounds, ImageListView.Colors.SelectedColor1, ImageListView.Colors.SelectedColor2, LinearGradientMode.Vertical))
                using (Brush bSelected = new LinearGradientBrush(bounds, Color.WhiteSmoke, Color.LightGray, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bSelected, bounds, 12);
                }
            }

            // Draw the image
            Image img = RoundCorners(item.GetCachedImage(CachedImageType.Thumbnail),20);
            if (img != null)
            {
                Rectangle pos = Utility.GetSizedImageBounds(img, new Rectangle(bounds.Location + itemPadding, ImageListView.ThumbnailSize));

                if (ShortcutLibraryForm.shortcutValidity[item.Text])
                {
                    // Draw the full color image as the shortcuts is not invalid
                    g.DrawImage(img, pos);
                }
                else
                {
                    // THe shortcut is invalid
                    // so we make the image grayscale
                    Image grayImg = MakeGrayscale(img);
                    g.DrawImage(grayImg, pos);

                    // Draw a warning triangle over it
                    // right in the centre
                    g.DrawImage(Properties.Resources.Warning, pos.X + 30, pos.Y + 30, 40, 40);
                }

                // Draw image border
                if (Math.Min(pos.Width, pos.Height) > 32)
                {
                    using (Pen pOuterBorder = new Pen(ImageListView.Colors.ImageOuterBorderColor))
                    {
                        //g.DrawRectangle(pOuterBorder, pos);
                        DrawRoundedRectangle(g, pOuterBorder, pos,9);
                    }
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

            if ((state & ItemState.Selected) != ItemState.None)
            {
                using (Pen pSelectedBorder = new Pen(Color.Brown,4))
                {
                    //DrawRoundedRectangle(g, pSelectedBorder, bounds, 9);
                    Utility.DrawRoundedRectangle(g, pSelectedBorder, bounds.Left+3, bounds.Top+3, bounds.Width - 5, bounds.Height - 5, 10);
                }
            }
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
    }

#pragma warning disable CS3009 // Base type is not CLS-compliant
    public class ProfileILVRenderer : ImageListView.ImageListViewRenderer
#pragma warning restore CS3009 // Base type is not CLS-compliant
    {
        // Returns item size for the given view mode.
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public override Size MeasureItem(View view)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
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
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public override void DrawItem(Graphics g, ImageListViewItem item, ItemState state, Rectangle bounds)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            if (g is null)
            {
                throw new ArgumentNullException(nameof(g));
            }

            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

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

            if ((state & ItemState.Selected) != ItemState.None)
            {
                //using (Brush bSelected = new LinearGradientBrush(bounds, ImageListView.Colors.SelectedColor1, ImageListView.Colors.SelectedColor2, LinearGradientMode.Vertical))
                using (Brush bSelected = new LinearGradientBrush(bounds, Color.WhiteSmoke, Color.LightGray, LinearGradientMode.Vertical))
                {
                    Utility.FillRoundedRectangle(g, bSelected, bounds, 12);
                }
            }

            // Draw the image
            Image img = RoundCorners(item.GetCachedImage(CachedImageType.Thumbnail), 20);
            if (img != null)
            {
                Rectangle pos = Utility.GetSizedImageBounds(img, new Rectangle(bounds.Location + itemPadding, ImageListView.ThumbnailSize));

                if (DisplayProfileForm.profileValidity[item.Text])
                {
                    // Draw the full color image as the shortcuts is not invalid
                    g.DrawImage(img, pos);
                }
                else
                {
                    // THe shortcut is invalid
                    // so we make the image grayscale
                    Image grayImg = MakeGrayscale(img);
                    g.DrawImage(grayImg, pos);

                    // Draw a warning triangle over it
                    // right in the centre
                    g.DrawImage(Properties.Resources.Warning, pos.X + 30, pos.Y + 30, 40, 40);
                }

                // Draw image border
                if (Math.Min(pos.Width, pos.Height) > 32)
                {
                    using (Pen pOuterBorder = new Pen(ImageListView.Colors.ImageOuterBorderColor))
                    {
                        //g.DrawRectangle(pOuterBorder, pos);
                        DrawRoundedRectangle(g, pOuterBorder, pos, 9);
                    }
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

            if ((state & ItemState.Selected) != ItemState.None)
            {
                using (Pen pSelectedBorder = new Pen(Color.Brown, 4))
                {
                    //DrawRoundedRectangle(g, pSelectedBorder, bounds, 9);
                    Utility.DrawRoundedRectangle(g, pSelectedBorder, bounds.Left + 3, bounds.Top + 3, bounds.Width - 5, bounds.Height - 5, 10);
                }
            }
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
    }


}

