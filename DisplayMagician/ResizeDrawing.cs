using System;
using System.Drawing;

namespace DisplayMagician;

public static class ResizeDrawing
{
    public static Size FitWithin(Size currentSize, Size targetSize, bool maintainRatio = true)
    {
        if (!maintainRatio)
        {
            return currentSize;
        }

        float currentSizeRatio = (float)currentSize.Width / currentSize.Height;
        bool widerThanTaller = currentSize.Width >= currentSize.Height;
        float targetWidth = widerThanTaller ? targetSize.Width : targetSize.Height * currentSizeRatio;
        float targetHeight = widerThanTaller ? targetSize.Width / currentSizeRatio : targetSize.Height;

        return new Size(Convert.ToInt32(targetWidth), Convert.ToInt32(targetHeight));
    }

    public static Size MakeSmaller(Size currentSize, int percentageChange)
    {
        if (percentageChange <= 0 || percentageChange > 100)
        {
            return currentSize;
        }

        float percent = (float)percentageChange / 100;
        return new Size(Convert.ToInt32(currentSize.Width * percent), Convert.ToInt32(currentSize.Height * percent));
    }

    public static Size MakeBigger(Size currentSize, int percentageChange)
    {
        if (percentageChange <= 0 || percentageChange > 100)
        {
            return currentSize;
        }

        float percent = (float)percentageChange / 100;
        return new Size(Convert.ToInt32(currentSize.Width * percent), Convert.ToInt32(currentSize.Height * percent));
    }

    public static Point AlignCenter(Size itemSize, Size outerSize)
    {
        if (itemSize.Equals(outerSize))
        {
            return new Point(0, 0);
        }

        return new Point((outerSize.Width - itemSize.Width) / 2, (outerSize.Height - itemSize.Height) / 2);
    }

    public static Point AlignBottomRight(Size itemSize, Size outerSize)
    {
        return new Point(outerSize.Width - itemSize.Width, outerSize.Height - itemSize.Height);
    }
}