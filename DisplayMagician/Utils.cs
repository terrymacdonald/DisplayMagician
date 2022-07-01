using Microsoft.Win32;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace DisplayMagician
{ 

    static class Utils
    {
        // 1. Import InteropServices

        /// 2. Declare DownloadsFolder KNOWNFOLDERID
        private static Guid FolderDownloads = new Guid("374DE290-123F-4565-9164-39C4925E467B");
    
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
            CenterOnPrimaryScreen(frm);
            frm.Visible = true;
            frm.Activate();
            frm.BringToFront();
        }

        public static void ShowCenteredOnPrimaryScreen(this Form frm)
        {
            CenterOnPrimaryScreen(frm);
            frm.Show();
        }

        public static void CenterOnPrimaryScreen(this Form frm)
        {
            frm.Top = (Screen.PrimaryScreen.Bounds.Height - frm.Height) / 2;
            frm.Left = (Screen.PrimaryScreen.Bounds.Width - frm.Width) / 2;
        }



        public static void ShowDialogCentered(this Form frm, Form owner)
        {
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
