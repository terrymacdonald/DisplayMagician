using Microsoft.Win32;
using NLog;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Text;

namespace DisplayMagician
{ 

    static class Utils
    {
        // 1. Import InteropServices

        /// 2. Declare DownloadsFolder KNOWNFOLDERID
        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// 3. Import SHGetKnownFolderPath method
        /// <summary>
        /// Retrieves the full path of a known folder identified by the folder's KnownFolderID.
        /// </summary>
        /// <param name="id">A KnownFolderID that identifies the folder.</param>
        /// <param name="flags">Flags that specify special retrieval options. This value can be
        ///     0; otherwise, one or more of the KnownFolderFlag values.</param>
        /// <param name="token">An access token that represents a particular user. If this
        ///     parameter is NULL, which is the most common usage, the function requests the known
        ///     folder for the current user. Assigning a value of -1 indicates the Default User.
        ///     The default user profile is duplicated when any new user account is created.
        ///     Note that access to the Default User folders requires administrator privileges.
        ///     </param>
        /// <param name="path">When this method returns, contains the address of a string that
        ///     specifies the path of the known folder. The returned path does not include a
        ///     trailing backslash.</param>
        /// <returns>Returns S_OK if successful, or an error value otherwise.</returns>
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHGetKnownFolderPath(ref Guid id, int flags, IntPtr token, out IntPtr path);

        /// 4. Declare method that returns the Downloads Path as string
        /// <summary>
        /// Returns the absolute downloads directory specified on the system.
        /// </summary>
        /// <returns></returns>
        public static string GetDownloadsPath()
        {
            if (Environment.OSVersion.Version.Major < 6) throw new NotSupportedException();

            IntPtr pathPtr = IntPtr.Zero;

            try
            {
                SHGetKnownFolderPath(ref FolderDownloads, 0, IntPtr.Zero, out pathPtr);
                return Marshal.PtrToStringUni(pathPtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(pathPtr);
            }
        }

        public static void ActivateCenteredOnPrimaryScreen(this Form frm)
        {
            if (!(frm is Form))
            {
                logger.Trace($"Utils/ActivateCenteredOnPrimaryScreen: frm passed in is not a Form. Not able to center the form.");
                return;
            }
            frm.Top = (Screen.PrimaryScreen.Bounds.Height - frm.Height) / 2;
            frm.Left = (Screen.PrimaryScreen.Bounds.Width - frm.Width) / 2;
            frm.Visible = true;
            frm.Activate();
            //frm.BringToFront();
        }

        public static void ShowCenteredOnPrimaryScreen(this Form frm)
        {
            if (!(frm is Form))
            {
                logger.Trace($"Utils/ShowCenteredOnPrimaryScreen: frm passed in is not a Form. Not able to center the form.");
                return;
            }
            CenterOnPrimaryScreen(frm);
            frm.Show();
        }

        public static void CenterOnPrimaryScreen(this Form frm)
        {
            if (!(frm is Form))
            {
                logger.Trace($"Utils/CenterOnPrimaryScreen: frm passed in is not a Form. Not able to center the form.");
                return;
            }
            frm.Top = (Screen.PrimaryScreen.Bounds.Height - frm.Height) / 2;
            frm.Left = (Screen.PrimaryScreen.Bounds.Width - frm.Width) / 2;
        }



        public static void ShowDialogCentered(this Form frm, Form owner)
        {
            if (!(frm is Form))
            {
                logger.Trace($"Utils/ShowDialogCentered: frm passed in is not a Form. Not able to center the dialog.");
                return;
            }
            if (!(owner is Form))
            {
                logger.Trace($"Utils/ShowDialogCentered: owner passed in is not a Form. Not able to center the dialog.");
                return;
            }
            Rectangle ownerRect = GetOwnerRect(frm, owner);
            frm.Location = new Point(ownerRect.Left + (ownerRect.Width - frm.Width) / 2,
                                     ownerRect.Top + (ownerRect.Height - frm.Height) / 2);
            frm.ShowDialog(owner);
        }

        private static Rectangle GetOwnerRect(Form frm, Form owner)
        {
            return owner != null ? owner.DesktopBounds : Screen.GetWorkingArea(frm);
        }

        public static bool IsWindows11()
        {
            var reg = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");

            var currentBuildStr = (string)reg.GetValue("CurrentBuild");
            var currentBuild = int.Parse(currentBuildStr);

            return currentBuild >= 22000;
        }

        public static string NormaliseGameName(this string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            var newName = name;
            newName = RemoveTrademarks(newName);
            newName = newName.Replace("_", " ");
            newName = newName.Replace(".", " ");
            newName = newName.Replace('’', '\'');
            newName = RemoveUnlessThatEmptiesTheString(newName, @"\[.*?\]");
            newName = RemoveUnlessThatEmptiesTheString(newName, @"\(.*?\)");
            newName = Regex.Replace(newName, @"\s*:\s*", ": ");
            newName = Regex.Replace(newName, @"\s+", " ");
            if (Regex.IsMatch(newName, @",\s*The$"))
            {
                newName = "The " + Regex.Replace(newName, @",\s*The$", "", RegexOptions.IgnoreCase);
            }

            return newName.Trim();
        }

        public static string RemoveTrademarks(this string str, string remplacement = "")
        {
            if (String.IsNullOrEmpty(str))
            {
                return str;
            }

            return Regex.Replace(str, @"[™©®]", remplacement);
        }

        private static string RemoveUnlessThatEmptiesTheString(string input, string pattern)
        {
            string output = Regex.Replace(input, pattern, string.Empty);

            if (string.IsNullOrWhiteSpace(output))
            {
                return input;
            }
            return output;
        }

        public static void ExtractResource(string path, string name, string type, string destination)
        {
            IntPtr hMod = NativeMethods.LoadLibraryEx(path, IntPtr.Zero, 0x00000002);
            IntPtr hRes = NativeMethods.FindResource(hMod, name, type);
            uint size = NativeMethods.SizeofResource(hMod, hRes);
            IntPtr pt = NativeMethods.LoadResource(hMod, hRes);

            byte[] bPtr = new byte[size];
            Marshal.Copy(pt, bPtr, 0, (int)size);
            using (MemoryStream m = new MemoryStream(bPtr))
            {
                File.WriteAllBytes(destination, m.ToArray());
            }
        }

        public static long GetUriPackFileSize(string packUri)
        {
            var info = System.Windows.Application.GetResourceStream(new Uri(packUri));
            using (var stream = info.Stream)
            {
                return stream.Length;
            }
        }

        public static string ReadFileFromResource(string resource)
        {
            using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resource))
            {
                var tr = new StreamReader(stream);
                return tr.ReadToEnd();
            }
        }

        public static string GetIndirectResourceString(string fullName, string packageName, string resource)
        {
            var resUri = new Uri(resource);
            var resourceString = string.Empty;
            if (resource.StartsWith("ms-resource://"))
            {
                resourceString = $"@{{{fullName}? {resource}}}";
            }
            else if (resource.Contains('/'))
            {
                resourceString = $"@{{{fullName}? ms-resource://{packageName}/{resource.Replace("ms-resource:", "").Trim('/')}}}";
            }
            else
            {
                resourceString = $"@{{{fullName}? ms-resource://{packageName}/resources/{resUri.Segments.Last()}}}";
            }

            var sb = new StringBuilder(1024);
            var result = NativeMethods.SHLoadIndirectString(resourceString, sb, sb.Capacity, IntPtr.Zero);
            if (result == 0)
            {
                return sb.ToString();
            }

            resourceString = $"@{{{fullName}? ms-resource://{packageName}/{resUri.Segments.Last()}}}";
            result = NativeMethods.SHLoadIndirectString(resourceString, sb, sb.Capacity, IntPtr.Zero);
            if (result == 0)
            {
                return sb.ToString();
            }

            return string.Empty;
        }

        public static void AddAnimation(Button button)
        {
            var expandTimer = new System.Windows.Forms.Timer();
            var contractTimer = new System.Windows.Forms.Timer();
            var pauseTimer = new System.Windows.Forms.Timer();

            int buttonX = button.Location.X;
            int buttonY = button.Location.Y;

            expandTimer.Interval = 10;//can adjust to determine the refresh rate
            contractTimer.Interval = 10;
            pauseTimer.Interval = 1000;

            DateTime animationStarted = DateTime.Now;

            //TODO update as appropriate or make it a parameter
            TimeSpan animationDuration = TimeSpan.FromMilliseconds(250);
            int initialWidth = 88;
            int endWidth = 100;
            int initialHeight = 27;
            int endHeight = 35;
            bool expanding = false;
            int horDiff = endWidth - initialWidth;
            int vertDiff = endHeight - initialHeight;


            pauseTimer.Tick += (_, args) =>
            {
                animationStarted = DateTime.Now;
                if (!expanding)
                {
                    expandTimer.Start();
                    expanding = !expanding;
                }
                else
                {
                    contractTimer.Start();
                    expanding = !expanding;
                }
            };

            expandTimer.Tick += (_, args) =>
            {
                double percentComplete = (DateTime.Now - animationStarted).Ticks
                    / (double)animationDuration.Ticks;

                if (percentComplete >= 1)
                {
                    expandTimer.Stop();
                    //contractTimer.Start();
                }
                else
                {
                    button.Width = (int)(initialWidth +
                        horDiff * percentComplete);
                    button.Height = (int)(initialHeight +
                        vertDiff * percentComplete);
                    int x = buttonX - (int)(horDiff * percentComplete)/2;
                    int y = buttonY - (int)(vertDiff * percentComplete)/2;
                    button.Location = new Point(x,y);

                }
            };

            contractTimer.Tick += (_, args) =>
            {
                double percentComplete = (DateTime.Now - animationStarted).Ticks
                    / (double)animationDuration.Ticks;

                if (percentComplete >= 1)
                {
                    contractTimer.Stop();
                    //expandTimer.Start();
                }
                else
                {
                    button.Width = (int)(endWidth -
                        horDiff * percentComplete);
                    button.Height = (int)(endHeight -
                        vertDiff * percentComplete);
                    int x = buttonX - (int)(horDiff * (1-percentComplete)) / 2;
                    int y = buttonY - (int)(vertDiff * (1-percentComplete)) / 2;
                    button.Location = new Point(x, y);
                }
            };

            pauseTimer.Start();

            // Update the number of times the donation button animation has been run to zero, and record when it was run last
            Program.AppProgramSettings.LastDonateButtonAnimationDate = DateOnly.FromDateTime(DateTime.UtcNow); 
            Program.AppProgramSettings.NumberOfStartsSinceLastDonationButtonAnimation = 0;
            Program.AppProgramSettings.SaveSettings();
        }

        public static bool TimeToRunDonationAnimation()
        {
            // Run the donation animation if:
            // - the user has used DisplayMagician 5 times with no donations, or its longer than 5 days since the last donation animation
            // - the user has donated, but it was more than a year ago

            //Check if the user has donated
            if (Program.AppProgramSettings.NumberOfDonations == 0)
            {
                // User has not donated yet
                // If the user has used DisplayMagician 5 times with no donations, or its longer than 5 days since the last donation animation
                if (Program.AppProgramSettings.NumberOfStartsSinceLastDonationButtonAnimation >= 5 || Program.AppProgramSettings.LastDonateButtonAnimationDate.AddMonths(2) >= DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    return true;
                }
            }
            else
            {
                // User has donated, but it's been a year since the last donation
                if (Program.AppProgramSettings.LastDonationDate.AddYears(1) <= DateOnly.FromDateTime(DateTime.UtcNow))
                {
                    // If the user has used DisplayMagician 20 times with no donations, or its longer than 20 days since the last donation animation
                    if (Program.AppProgramSettings.NumberOfStartsSinceLastDonationButtonAnimation >= 20 || Program.AppProgramSettings.LastDonateButtonAnimationDate.AddMonths(2) >= DateOnly.FromDateTime(DateTime.UtcNow))
                    {
                        return true;
                    }
                }
            }

            // If we get to here, then no need for the donation animation
            return false;
        }

        public static void UserHasDonated()
        {
            Program.AppProgramSettings.LastDonationDate = DateOnly.FromDateTime(DateTime.UtcNow);
            Program.AppProgramSettings.NumberOfDonations++;
            Program.AppProgramSettings.SaveSettings();
        }


    }

    // Originally from https://stackoverflow.com/questions/9746538/fastest-safest-file-finding-parsing
    public class SafeFileEnumerator : IEnumerable<FileSystemInfo>
    {
        /// <summary>
        /// Helper class to enumerate the file system.
        /// </summary>
        private class Enumerator : IEnumerator<FileSystemInfo>
        {
            // Core enumerator that we will be walking though
            private IEnumerator<FileSystemInfo> fileEnumerator;
            // Directory enumerator to capture access errors
            private IEnumerator<DirectoryInfo> directoryEnumerator;

            private DirectoryInfo root;
            private string pattern;
            private SearchOption searchOption;
            private IList<Exception> errors;

            public Enumerator(DirectoryInfo root, string pattern, SearchOption option, IList<Exception> errors)
            {
                this.root = root;
                this.pattern = pattern;
                this.errors = errors;
                this.searchOption = option;

                Reset();
            }

            /// <summary>
            /// Current item the primary itterator is pointing to
            /// </summary>
            public FileSystemInfo Current
            {
                get
                {
                    //if (fileEnumerator == null) throw new ObjectDisposedException("FileEnumerator");
                    return fileEnumerator.Current as FileSystemInfo;
                }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public void Dispose()
            {
                Dispose(true, true);
            }

            private void Dispose(bool file, bool dir)
            {
                if (file)
                {
                    if (fileEnumerator != null)
                        fileEnumerator.Dispose();

                    fileEnumerator = null;
                }

                if (dir)
                {
                    if (directoryEnumerator != null)
                        directoryEnumerator.Dispose();

                    directoryEnumerator = null;
                }
            }

            public bool MoveNext()
            {
                // Enumerate the files in the current folder
                if ((fileEnumerator != null) && (fileEnumerator.MoveNext()))
                    return true;

                // Don't go recursive...
                if (searchOption == SearchOption.TopDirectoryOnly) { return false; }

                while ((directoryEnumerator != null) && (directoryEnumerator.MoveNext()))
                {
                    Dispose(true, false);

                    try
                    {
                        fileEnumerator = new SafeFileEnumerator(
                            directoryEnumerator.Current,
                            pattern,
                            SearchOption.AllDirectories,
                            errors
                            ).GetEnumerator();
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                        continue;
                    }

                    // Open up the current folder file enumerator
                    if (fileEnumerator.MoveNext())
                        return true;
                }

                Dispose(true, true);

                return false;
            }

            public void Reset()
            {
                Dispose(true, true);

                // Safely get the enumerators, including in the case where the root is not accessable
                if (root != null)
                {
                    try
                    {
                        fileEnumerator = root.GetFileSystemInfos(pattern, SearchOption.TopDirectoryOnly).AsEnumerable<FileSystemInfo>().GetEnumerator();
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                        fileEnumerator = null;
                    }

                    try
                    {
                        directoryEnumerator = root.GetDirectories("*", SearchOption.TopDirectoryOnly).AsEnumerable<DirectoryInfo>().GetEnumerator();
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex);
                        directoryEnumerator = null;
                    }
                }
            }
        }

        /// <summary>
        /// Starting directory to search from
        /// </summary>
        private DirectoryInfo root;

        /// <summary>
        /// Filter pattern
        /// </summary>
        private string pattern;

        /// <summary>
        /// Indicator if search is recursive or not
        /// </summary>
        private SearchOption searchOption;

        /// <summary>
        /// Any errors captured
        /// </summary>
        private IList<Exception> errors;

        /// <summary>
        /// Create an Enumerator that will scan the file system, skipping directories where access is denied
        /// </summary>
        /// <param name="root">Starting Directory</param>
        /// <param name="pattern">Filter pattern</param>
        /// <param name="option">Recursive or not</param>
        public SafeFileEnumerator(string root, string pattern, SearchOption option)
            : this(new DirectoryInfo(root), pattern, option)
        { }

        /// <summary>
        /// Create an Enumerator that will scan the file system, skipping directories where access is denied
        /// </summary>
        /// <param name="root">Starting Directory</param>
        /// <param name="pattern">Filter pattern</param>
        /// <param name="option">Recursive or not</param>
        public SafeFileEnumerator(DirectoryInfo root, string pattern, SearchOption option)
            : this(root, pattern, option, new List<Exception>())
        { }

        // Internal constructor for recursive itterator
        private SafeFileEnumerator(DirectoryInfo root, string pattern, SearchOption option, IList<Exception> errors)
        {
            if (root == null || !root.Exists)
            {
                throw new ArgumentException("Root directory is not set or does not exist.", "root");
            }
            this.root = root;
            this.searchOption = option;
            this.pattern = String.IsNullOrEmpty(pattern)
                ? "*"
                : pattern;
            this.errors = errors;
        }

        /// <summary>
        /// Errors captured while parsing the file system.
        /// </summary>
        public Exception[] Errors
        {
            get
            {
                return errors.ToArray();
            }
        }

        public IEnumerator<FileSystemInfo> GetEnumerator()
        {
            return new Enumerator(root, pattern, searchOption, errors);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
