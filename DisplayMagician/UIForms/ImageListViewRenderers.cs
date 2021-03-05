using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagicianShared;
using Manina.Windows.Forms;

namespace DisplayMagician.UIForms
{
#pragma warning disable CS3009 // Base type is not CLS-compliant
    public class ShortcutILVRenderer : ImageListView.ImageListViewRenderer
#pragma warning restore CS3009 // Base type is not CLS-compliant
    {
        // Returns item size for the given view mode.
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public override Size MeasureItem(Manina.Windows.Forms.View view)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            // Reference text height
            int textHeight = ImageListView.Font.Height;

            Size itemSize = new Size();

            itemSize.Height = ImageListView.ThumbnailSize.Height + 2 * textHeight + 4 * 3;
            itemSize.Width = ImageListView.ThumbnailSize.Width + 4 * 3;
            return itemSize;
        }
        // Draws the background of the control.
        public override void DrawBackground(Graphics g, Rectangle bounds)
        {
            if (ImageListView.View == Manina.Windows.Forms.View.Thumbnails)
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
            Point imagePoint = new Point(bounds.X+3, bounds.Y+3);
            Size imageSize = new Size();
            imageSize.Height = ImageListView.ThumbnailSize.Height;
            imageSize.Width = ImageListView.ThumbnailSize.Width;
            Rectangle imageBounds = new Rectangle(imagePoint, imageSize);

            // Paint background
            if (ImageListView.Enabled)
            {
                using (Brush bItemBack = new SolidBrush(alternate && ImageListView.View == Manina.Windows.Forms.View.Details ?
                    ImageListView.Colors.AlternateBackColor : ImageListView.Colors.BackColor))
                {
                    //g.FillRectangle(bItemBack, bounds);
                    g.FillRectangle(bItemBack, bounds);
                }
            }
            else
            {
                using (Brush bItemBack = new SolidBrush(ImageListView.Colors.DisabledBackColor))
                {
                    //g.FillRectangle(bItemBack, bounds);
                    g.FillRectangle(bItemBack, bounds);
                }
            }

            if ((state & ItemState.Selected) != ItemState.None)
            {
                //using (Brush bSelected = new LinearGradientBrush(bounds, Color.WhiteSmoke, Color.LightGray, LinearGradientMode.Vertical))
                using (Brush bSelected = new LinearGradientBrush(bounds, Color.WhiteSmoke, Color.LightGray, LinearGradientMode.Vertical))
                {
                    //Utility.FillRoundedRectangle(g, bSelected, bounds, 12);
                    Utility.FillRoundedRectangle(g, bSelected, imageBounds, 12);
                }
            }

            // Draw the image
            Image img = ImageUtils.RoundCorners(item.GetCachedImage(CachedImageType.Thumbnail),20);
            if (img != null)
            {
                Rectangle pos = Utility.GetSizedImageBounds(img, new Rectangle(bounds.Location + itemPadding, ImageListView.ThumbnailSize));

                if (ShortcutRepository.ShortcutValidityLookup[item.Text])
                {
                    // Draw the full color image as the shortcuts is not invalid
                    g.DrawImage(img, pos);
                }
                else
                {
                    // THe shortcut is invalid
                    // so we make the image grayscale
                    Image grayImg = ImageUtils.MakeGrayscale(img);
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
                        ImageUtils.DrawRoundedRectangle(g, pOuterBorder, pos,9);
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
            Size szt = TextRenderer.MeasureText(item.Text, ImageListView.Font);
            Rectangle rt = new Rectangle(bounds.Left + itemPadding.Width, bounds.Top + itemPadding.Height + ImageListView.ThumbnailSize.Height, ImageListView.ThumbnailSize.Width, 3 * szt.Height);
            TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
            TextRenderer.DrawText(g, item.Text, ImageListView.Font, rt, foreColor, flags);


            if ((state & ItemState.Selected) != ItemState.None)
            {
                using (Pen pSelectedBorder = new Pen(Color.Brown,4))
                {
                    //DrawRoundedRectangle(g, pSelectedBorder, bounds, 9);
                    //Utility.DrawRoundedRectangle(g, pSelectedBorder, bounds.Left+3, bounds.Top+3, bounds.Width - 5, bounds.Height - 5, 10);
                    Utility.DrawRoundedRectangle(g, pSelectedBorder, imageBounds.Left, imageBounds.Top, imageBounds.Width, imageBounds.Height, 10);
                }
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
    }

#pragma warning disable CS3009 // Base type is not CLS-compliant
    public class ProfileILVRenderer : ImageListView.ImageListViewRenderer
#pragma warning restore CS3009 // Base type is not CLS-compliant
    {
        // Returns item size for the given view mode.
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public override Size MeasureItem(Manina.Windows.Forms.View view)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            // Reference text height
            int textHeight = ImageListView.Font.Height;

            Size itemSize = new Size();

            itemSize.Height = ImageListView.ThumbnailSize.Height + 2 * textHeight + 4 * 3;
            itemSize.Width = ImageListView.ThumbnailSize.Width + 4 * 3;
            return itemSize;
        }
        // Draws the background of the control.
        public override void DrawBackground(Graphics g, Rectangle bounds)
        {
            if (ImageListView.View == Manina.Windows.Forms.View.Thumbnails)
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
            Point imagePoint = new Point(bounds.X + 3, bounds.Y + 3);
            Size imageSize = new Size();
            imageSize.Height = ImageListView.ThumbnailSize.Height;
            imageSize.Width = ImageListView.ThumbnailSize.Width;
            Rectangle imageBounds = new Rectangle(imagePoint, imageSize);

            // Paint background
            if (ImageListView.Enabled)
            {
                using (Brush bItemBack = new SolidBrush(alternate && ImageListView.View == Manina.Windows.Forms.View.Details ?
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
                    Utility.FillRoundedRectangle(g, bSelected, imageBounds, 12);
                }
            }

            // Draw the image
            Image img = ImageUtils.RoundCorners(item.GetCachedImage(CachedImageType.Thumbnail), 20);
            if (img != null)
            {
                Rectangle pos = Utility.GetSizedImageBounds(img, new Rectangle(bounds.Location + itemPadding, ImageListView.ThumbnailSize));

                if (ProfileRepository.ProfileValidityLookup[item.Text])
                {
                    // Draw the full color image as the shortcuts is not invalid
                    g.DrawImage(img, pos);
                }
                else
                {
                    // THe shortcut is invalid
                    // so we make the image grayscale
                    Image grayImg = ImageUtils.MakeGrayscale(img);
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
                        ImageUtils.DrawRoundedRectangle(g, pOuterBorder, pos, 9);
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
            Size szt = TextRenderer.MeasureText(item.Text, ImageListView.Font);
            Rectangle rt = new Rectangle(bounds.Left + itemPadding.Width, bounds.Top + itemPadding.Height + ImageListView.ThumbnailSize.Height + 4, ImageListView.ThumbnailSize.Width, 3 * szt.Height);
            TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.WordBreak;
            TextRenderer.DrawText(g, item.Text, ImageListView.Font, rt, foreColor, flags);

            if ((state & ItemState.Selected) != ItemState.None)
            {
                using (Pen pSelectedBorder = new Pen(Color.Brown, 4))
                {
                    //DrawRoundedRectangle(g, pSelectedBorder, bounds, 9);
                    Utility.DrawRoundedRectangle(g, pSelectedBorder, imageBounds.Left + 1, imageBounds.Top + 1, imageBounds.Width, imageBounds.Height, 10);
                }
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
    }
}

