using System;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;

namespace DisplayMagician.AppLibraries;

internal static class AppRuntimeEnvironment
{
    internal static bool HasPackageIdentity
    {
        get
        {
            try
            {
                return Package.Current != null;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }
    }

    internal static string NormaliseName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        return Regex.Replace(name.Replace('_', ' ').Replace('.', ' '), @"\s+", " ").Trim();
    }
}