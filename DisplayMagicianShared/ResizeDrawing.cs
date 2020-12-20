using System;
using System.Drawing;

namespace DisplayMagician.Shared
{
    public static class ResizeDrawing
    {
        public static Size FitWithin(Size currentSize, Size targetSize, bool maintainRatio = true)
        {
            if (maintainRatio == false)
                return currentSize;
            float currentSizeRatio = (float)currentSize.Width / (float)currentSize.Height;
            float targetWidth = 0;
            float targetHeight = 0;
            bool widerThanTaller = false;

            if (currentSize.Width >= currentSize.Height)
                widerThanTaller = true;
            else
                widerThanTaller = false;

            if (widerThanTaller)
            {
                // We need to simply shrink down until Width fits
                targetWidth = targetSize.Width;
                targetHeight = targetSize.Width / currentSizeRatio;
            }
            else if (!widerThanTaller)
            {
                targetHeight = targetSize.Height;
                targetWidth = targetHeight * currentSizeRatio;
            }
            return new Size(Convert.ToInt32(targetWidth), Convert.ToInt32(targetHeight));
        }

        public static Size MakeSmaller(Size currentSize, int percentageChange)
        {
            if (percentageChange <= 0 || percentageChange > 100)
                return currentSize;
            float percent = (float) percentageChange / 100;
            int newWidth = Convert.ToInt32(currentSize.Width * percent);
            int newHeight = Convert.ToInt32(currentSize.Height * percent);
            return new Size(newWidth, newHeight);
        }

        public static Size MakeBigger(Size currentSize, int percentageChange)
        {
            if (percentageChange <= 0 || percentageChange > 100)
                return currentSize;
            float percent = percentageChange / 100;
            return new Size(Convert.ToInt32(currentSize.Width * percent), Convert.ToInt32(currentSize.Height * percent));
        }

        public static Point AlignCenter(Size itemSize, Size outerSize)
        {
            int itemX = 0, itemY = 0;

            if (itemSize.Equals(outerSize))
                return new Point(0, 0);

            // Center align in the X Axis
            itemX = (outerSize.Width - itemSize.Width) / 2;
            // Center align in the Y Axis
            itemY = (outerSize.Height - itemSize.Height) / 2;

            return new Point(itemX, itemY);
        }

        public static Point AlignBottomRight(Size itemSize, Size outerSize)
        {
            int itemX = 0, itemY = 0;

            // Right align in the X Axis
            itemX = outerSize.Width - itemSize.Width;

            // Bottom align in the Y Axis
            itemY = outerSize.Height - itemSize.Height;

            return new Point(itemX, itemY);
        }

    }
}
