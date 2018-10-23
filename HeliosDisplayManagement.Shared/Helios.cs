using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace HeliosDisplayManagement.Shared
{
    public class Helios
    {
        public static string Address
        {
            get
            {
                try
                {
                    using (
                        var key =
                            Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Helios Display Management", false))
                    {
                        var executableAddress = key?.GetValue(@"ExecutableAddress", null) as string;

                        if (!string.IsNullOrWhiteSpace(executableAddress) && File.Exists(executableAddress))
                        {
                            return executableAddress;
                        }
                    }
                }
                catch
                {
                    // ignored
                }

                return null;
            }
        }

        public static bool IsInstalled
        {
            get => !string.IsNullOrWhiteSpace(Address);
        }

        // ReSharper disable once MethodTooLong
        // ReSharper disable once TooManyArguments
        public static void Open(
            HeliosStartupAction action = HeliosStartupAction.None,
            Profile profile = null,
            string programAddress = null,
            bool asAdmin = false)
        {
            try
            {
                if (!IsInstalled)
                {
                    return;
                }

                var args = new List<string> {$"-a {action}"};

                if (profile != null)
                {
                    args.Add($"-p \"{profile.Id}\"");
                }

                if (!string.IsNullOrWhiteSpace(programAddress))
                {
                    args.Add($"-e \"{programAddress}\"");
                }

                var processInfo = new ProcessStartInfo(Address, string.Join(" ", args))
                {
                    UseShellExecute = true
                };

                if (asAdmin)
                {
                    processInfo.Verb = @"runas";
                }

                Process.Start(processInfo);
            }
            catch (Exception e)
            {
                // Check if operation canceled by user
                if ((e as Win32Exception)?.NativeErrorCode == 1223)
                {
                    return;
                }

                throw;
            }
        }

        public static void OpenSteamGame(
            HeliosStartupAction action = HeliosStartupAction.None,
            Profile profile = null,
            uint steamAppId = 0)
        {
            try
            {
                if (!IsInstalled)
                {
                    return;
                }

                var args = new List<string> {$@"-a {action}"};

                if (profile != null)
                {
                    args.Add($"-p \"{profile.Id}\"");
                }

                if (steamAppId > 0)
                {
                    args.Add($"-s \"{steamAppId}\"");
                }

                var processInfo = new ProcessStartInfo(Address, string.Join(" ", args))
                {
                    UseShellExecute = true
                };
                Process.Start(processInfo);
            }
            catch (Exception e)
            {
                // Check if operation canceled by user
                if ((e as Win32Exception)?.NativeErrorCode == 1223)
                {
                    return;
                }

                throw;
            }
        }
    }
}