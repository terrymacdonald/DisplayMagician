﻿using Microsoft.Win32;
using NLog;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
            CenterOnPrimaryScreen(frm);
            frm.Visible = true;
            frm.Activate();
            frm.BringToFront();
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


    }



}
