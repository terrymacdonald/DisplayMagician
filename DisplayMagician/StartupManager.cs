using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace DisplayMagician
{
    public static class StartupManager
    {
        private const string RegistryRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
        private static readonly string AppName = "DisplayMagician";
        private static readonly string ExecutablePath = Environment.ProcessPath;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Adds DisplayMagician to Windows startup with optional command-line arguments.
        /// </summary>
        /// <param name="arguments">Optional command-line arguments to pass on startup.</param>
        public static bool EnableStartup(string arguments = null)
        {
            try
            {
                logger.Trace($"StartupManager/EnableStartup: Attempting to set DisplayMagician to automatically start when the computer first boots up.");
                string command = $"\"{ExecutablePath}\"";
                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    command += $" {arguments}";
                }

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(RegistryRunKey, true))
                {
                    logger.Trace($"StartupManager/EnableStartup: Creating the {AppName} registry value in the HKCU{RegistryRunKey} registry key and setting it to {command}.");
                    key.SetValue(AppName, command);
                }
                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                logger.Error(ex, $"StartupManager/EnableStartup: Failed to add DisplayMagician startup key to registry.");
                return false;
            }
        }

        /// <summary>
        /// Removes DisplayMagician from Windows startup.
        /// </summary>
        public static bool DisableStartup()
        {
            try
            {
                logger.Trace($"StartupManager/DisableStartup: Attempting to stop DisplayMagician from automatically starting when the computer first boots up.");
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, true))
                {
                    if (key == null)
                        return true;

                    if (key.GetValue(AppName) != null)
                    {
                        logger.Trace($"StartupManager/DisableStartup: Deleting the {AppName} registry value in the HKCU{RegistryRunKey} registry key.");
                        key.DeleteValue(AppName);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log the error)
                logger.Error(ex, $"StartupManager/DisableStartup: Failed to remove DisplayMagician startup key to registry.");
                return false;
            }
        }

        /// <summary>
        /// Checks if DisplayMagician is set to run at Windows startup.
        /// </summary>
        /// <returns>True if set to run at startup; otherwise, false.</returns>
        public static bool IsStartupEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryRunKey, false))
                {
                    if (key == null)
                        return false;

                    var value = key.GetValue(AppName) as string;
                    if (string.IsNullOrEmpty(value))
                        return false;

                    // Extract the executable path from the registry value
                    string[] quotedParts = value.Split('\"');
                    if (quotedParts.Length < 2)
                        return false;

                    string exePath = quotedParts[1];
                    return exePath.Equals(ExecutablePath, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
