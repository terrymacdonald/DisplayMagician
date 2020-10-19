// -----------------------------------------------------------------------
// <copyright file="ShellIcon.cs" company="Mauricio DIAZ ORLICH (madd0@madd0.com)">
//   Distributed under Microsoft Public License (MS-PL).
//   http://www.opensource.org/licenses/MS-PL
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace HeliosPlus {

    /// <summary>
    /// Get a small or large Icon with an easy C# function call
    /// that returns a 32x32 or 16x16 System.Drawing.Icon depending on which function you call
    /// either GetSmallIcon(string fileName) or GetLargeIcon(string fileName)
    /// </summary>
    public static class ShellIcon
    {
        #region Interop constants

        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        #endregion

        #region Interop data types

        [StructLayout(LayoutKind.Sequential)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [Flags]
        private enum SHGFI : int
        {
            /// <summary>get icon</summary>
            Icon = 0x000000100,
            /// <summary>get display name</summary>
            DisplayName = 0x000000200,
            /// <summary>get type name</summary>
            TypeName = 0x000000400,
            /// <summary>get attributes</summary>
            Attributes = 0x000000800,
            /// <summary>get icon location</summary>
            IconLocation = 0x000001000,
            /// <summary>return exe type</summary>
            ExeType = 0x000002000,
            /// <summary>get system icon index</summary>
            SysIconIndex = 0x000004000,
            /// <summary>put a link overlay on icon</summary>
            LinkOverlay = 0x000008000,
            /// <summary>show icon in selected state</summary>
            Selected = 0x000010000,
            /// <summary>get only specified attributes</summary>
            Attr_Specified = 0x000020000,
            /// <summary>get large icon</summary>
            LargeIcon = 0x000000000,
            /// <summary>get small icon</summary>
            SmallIcon = 0x000000001,
            /// <summary>get open icon</summary>
            OpenIcon = 0x000000002,
            /// <summary>get shell size icon</summary>
            ShellIconSize = 0x000000004,
            /// <summary>pszPath is a pidl</summary>
            PIDL = 0x000000008,
            /// <summary>use passed dwFileAttribute</summary>
            UseFileAttributes = 0x000000010,
            /// <summary>apply the appropriate overlays</summary>
            AddOverlays = 0x000000020,
            /// <summary>Get the index of the overlay in the upper 8 bits of the iIcon</summary>
            OverlayIndex = 0x000000040,
        }

        #endregion

        private class Win32
        {
            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);

        }

        public static Icon GetSmallFolderIcon()
        {
            return GetIcon("folder", SHGFI.SmallIcon | SHGFI.UseFileAttributes, true);
        }

        public static Icon GetLargeFolderIcon()
        {
            return GetIcon("folder", SHGFI.LargeIcon | SHGFI.UseFileAttributes, true);
        }

        public static Icon GetSmallIcon(string fileName)
        {
            return GetIcon(fileName, SHGFI.SmallIcon);
        }

        public static Icon GetLargeIcon(string fileName)
        {
            return GetIcon(fileName, SHGFI.LargeIcon);
        }

        public static Icon GetSmallIconFromExtension(string extension)
        {
            return GetIcon(extension, SHGFI.SmallIcon | SHGFI.UseFileAttributes);
        }

        public static Icon GetLargeIconFromExtension(string extension)
        {
            return GetIcon(extension, SHGFI.LargeIcon | SHGFI.UseFileAttributes);
        }

        private static Icon GetIcon(string fileName, SHGFI flags, bool isFolder = false)
        {
            SHFILEINFO shinfo = new SHFILEINFO();

            IntPtr hImgSmall = Win32.SHGetFileInfo(fileName, isFolder ? FILE_ATTRIBUTE_DIRECTORY : FILE_ATTRIBUTE_NORMAL, ref shinfo, (uint)Marshal.SizeOf(shinfo), (uint)(SHGFI.Icon | flags));

            Icon icon = (Icon)System.Drawing.Icon.FromHandle(shinfo.hIcon).Clone();
            Win32.DestroyIcon(shinfo.hIcon);
            return icon;
        }
    }
}