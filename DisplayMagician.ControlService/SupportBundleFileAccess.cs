using System.IO;

namespace DisplayMagician.ControlService;

internal static class SupportBundleFileAccess
{
    // Check every existing ancestor as well as the leaf. A lexical user-storage path
    // can otherwise redirect privileged staging writes or cleanup through a junction.
    internal static void EnsureUnlinkedPath(string path)
    {
        for (string? current = Path.GetFullPath(path); current != null; current = Path.GetDirectoryName(current))
        {
            try
            {
                if ((File.GetAttributes(current) & FileAttributes.ReparsePoint) != 0)
                {
                    throw new IOException("Support collection cannot use a filesystem link.");
                }
            }
            catch (FileNotFoundException)
            {
                // A staging leaf may not have been created yet; its ancestors still need checking.
            }
            catch (DirectoryNotFoundException)
            {
                // A new staging subtree may be missing; continue up to its existing ancestors.
            }
        }
    }
}
