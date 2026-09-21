namespace DisplayMagician;

public sealed class SharedLogger
{
    public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

    public SharedLogger(NLog.Logger parentLogger)
    {
        logger = parentLogger;
    }
}