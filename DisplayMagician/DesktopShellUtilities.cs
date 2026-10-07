using System;
using System.Diagnostics;
using System.IO;

namespace DisplayMagician
{
    internal static class DesktopShellUtilities
    {
        public static void OpenUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return;

            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        public static bool IsPEExecutable(string path)
        {
            string extension = Path.GetExtension(path ?? string.Empty);
            return extension.Equals(".exe", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".com", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".msi", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsExecutableFileType(string path)
        {
            if (IsPEExecutable(path))
                return true;

            string extension = Path.GetExtension(path ?? string.Empty);
            return extension.Equals(".bat", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".cmd", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".ps1", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".lnk", StringComparison.OrdinalIgnoreCase)
                || extension.Equals(".url", StringComparison.OrdinalIgnoreCase);
        }

        public static ProcessPriority TranslateNameToPriority(string priorityName)
        {
            return priorityName?.ToLowerInvariant() switch
            {
                "high" => ProcessPriority.High,
                "abovenormal" => ProcessPriority.AboveNormal,
                "belownormal" => ProcessPriority.BelowNormal,
                "idle" => ProcessPriority.Idle,
                _ => ProcessPriority.Normal
            };
        }
    }
}