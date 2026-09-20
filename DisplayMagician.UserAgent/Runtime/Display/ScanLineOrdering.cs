namespace DisplayMagicianShared
{
    public enum ScanLineOrdering
    {
        NotSpecified = 0,
        Progressive = 1,
        // InterlacedWithUpperFieldFirst shares value 2 with the DISPLAYCONFIG_SCANLINE_ORDERING_INTERLACED alias
        InterlacedWithUpperFieldFirst = 2,
        InterlacedWithLowerFieldFirst = 3
    }
}