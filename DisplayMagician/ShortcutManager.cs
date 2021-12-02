/**
 * Copyright 2020 Google LLC.
 * SPDX-License-Identifier: MIT
 */
//using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using Microsoft.WindowsAPICodePack.PropertySystem;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace DisplayMagician
{
    internal static class UnsafeNativeMethods
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct WIN32_FIND_DATAW
        {
            internal uint dwFileAttributes;
            // ftCreationTime was a by-value FILETIME structure
            internal uint ftCreationTime_dwLowDateTime;
            internal uint ftCreationTime_dwHighDateTime;
            // ftLastAccessTime was a by-value FILETIME structure
            internal uint ftLastAccessTime_dwLowDateTime;
            internal uint ftLastAccessTime_dwHighDateTime;
            // ftLastWriteTime was a by-value FILETIME structure
            internal uint ftLastWriteTime_dwLowDateTime;
            internal uint ftLastWriteTime_dwHighDateTime;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal uint dwReserved0;
            internal uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string cAlternateFileName;
        }

        /// <summary>IShellLink.Resolve fFlags</summary>
        [Flags()]
        internal enum SLR_FLAGS
        {
            /// <summary>
            /// Do not display a dialog box if the link cannot be resolved. When SLR_NO_UI is set,
            /// the high-order word of fFlags can be set to a time-out value that specifies the
            /// maximum amount of time to be spent resolving the link. The function returns if the
            /// link cannot be resolved within the time-out duration. If the high-order word is set
            /// to zero, the time-out duration will be set to the default value of 3,000 milliseconds
            /// (3 seconds). To specify a value, set the high word of fFlags to the desired time-out
            /// duration, in milliseconds.
            /// </summary>
            SLR_NO_UI = 0x1,
            /// <summary>Obsolete and no longer used</summary>
            SLR_ANY_MATCH = 0x2,
            /// <summary>If the link object has changed, update its path and list of identifiers.
            /// If SLR_UPDATE is set, you do not need to call IPersistFile::IsDirty to determine
            /// whether or not the link object has changed.</summary>
            SLR_UPDATE = 0x4,
            /// <summary>Do not update the link information</summary>
            SLR_NOUPDATE = 0x8,
            /// <summary>Do not execute the search heuristics</summary>
            SLR_NOSEARCH = 0x10,
            /// <summary>Do not use distributed link tracking</summary>
            SLR_NOTRACK = 0x20,
            /// <summary>Disable distributed link tracking. By default, distributed link tracking tracks
            /// removable media across multiple devices based on the volume name. It also uses the
            /// Universal Naming Convention (UNC) path to track remote file systems whose drive letter
            /// has changed. Setting SLR_NOLINKINFO disables both types of tracking.</summary>
            SLR_NOLINKINFO = 0x40,
            /// <summary>Call the Microsoft Windows Installer</summary>
            SLR_INVOKE_MSI = 0x80
        }

        [Flags()]
        internal enum SLGP_FLAGS
        {
            /// <summary>Retrieves the standard short (8.3 format) file name</summary>
            SLGP_SHORTPATH = 0x1,
            /// <summary>Retrieves the Universal Naming Convention (UNC) path name of the file</summary>
            SLGP_UNCPRIORITY = 0x2,
            /// <summary>Retrieves the raw path name. A raw path is something that might not exist and may include environment variables that need to be expanded</summary>
            SLGP_RAWPATH = 0x4
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("ole32.dll")]
        public extern static int PropVariantClear(ref PROPVARIANT pvar);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CLIPDATA
    {
        public uint cbSize;         //ULONG
        public int ulClipFmt;       //long
        public IntPtr pClipData;    //BYTE*
    }

    // Credit: http://blogs.msdn.com/b/adamroot/archive/2008/04/11/interop-with-propvariants-in-net.aspx
    /// <summary>
    /// Represents the OLE struct PROPVARIANT.
    /// </summary>
    /// <remarks>
    /// Must call Clear when finished to avoid memory leaks. If you get the value of
    /// a VT_UNKNOWN prop, an implicit AddRef is called, thus your reference will
    /// be active even after the PropVariant struct is cleared.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    internal struct PROPVARIANT
    {
        #region struct fields

        // The layout of these elements needs to be maintained.
        //
        // NOTE: We could use LayoutKind.Explicit, but we want
        //       to maintain that the IntPtr may be 8 bytes on
        //       64-bit architectures, so we'll let the CLR keep
        //       us aligned.
        //
        // NOTE: In order to allow x64 compat, we need to allow for
        //       expansion of the IntPtr. However, the BLOB struct
        //       uses a 4-byte int, followed by an IntPtr, so
        //       although the p field catches most pointer values,
        //       we need an additional 4-bytes to get the BLOB
        //       pointer. The p2 field provides this, as well as
        //       the last 4-bytes of an 8-byte value on 32-bit
        //       architectures.

        // This is actually a VarEnum value, but the VarEnum type
        // shifts the layout of the struct by 4 bytes instead of the
        // expected 2.
        ushort vt;
        ushort wReserved1;
        ushort wReserved2;
        ushort wReserved3;
        public IntPtr p;
        int p2;

        #endregion // struct fields

        #region union members

        sbyte cVal // CHAR cVal;
        {
            get { return (sbyte)GetDataBytes()[0]; }
        }

        byte bVal // UCHAR bVal;
        {
            get { return GetDataBytes()[0]; }
        }

        short iVal // SHORT iVal;
        {
            get { return BitConverter.ToInt16(GetDataBytes(), 0); }
        }

        ushort uiVal // USHORT uiVal;
        {
            get { return BitConverter.ToUInt16(GetDataBytes(), 0); }
        }

        int lVal // LONG lVal;
        {
            get { return BitConverter.ToInt32(GetDataBytes(), 0); }
        }

        uint ulVal // ULONG ulVal;
        {
            get { return BitConverter.ToUInt32(GetDataBytes(), 0); }
        }

        long hVal // LARGE_INTEGER hVal;
        {
            get { return BitConverter.ToInt64(GetDataBytes(), 0); }
        }

        ulong uhVal // ULARGE_INTEGER uhVal;
        {
            get { return BitConverter.ToUInt64(GetDataBytes(), 0); }
        }

        float fltVal // FLOAT fltVal;
        {
            get { return BitConverter.ToSingle(GetDataBytes(), 0); }
        }

        double dblVal // DOUBLE dblVal;
        {
            get { return BitConverter.ToDouble(GetDataBytes(), 0); }
        }

        bool boolVal // VARIANT_BOOL boolVal;
        {
            get { return (iVal == 0 ? false : true); }
        }

        int scode // SCODE scode;
        {
            get { return lVal; }
        }

        decimal cyVal // CY cyVal;
        {
            get { return decimal.FromOACurrency(hVal); }
        }

        DateTime date // DATE date;
        {
            get { return DateTime.FromOADate(dblVal); }
        }

        #endregion // union members

        private byte[] GetBlobData()
        {
            var blobData = new byte[lVal];
            IntPtr pBlobData;

            try
            {
                switch (IntPtr.Size)
                {
                    case 4:
                        pBlobData = new IntPtr(p2);
                        break;

                    case 8:
                        pBlobData = new IntPtr(BitConverter.ToInt64(GetDataBytes(), sizeof(int)));
                        break;

                    default:
                        throw new NotSupportedException();
                }

                Marshal.Copy(pBlobData, blobData, 0, lVal);
            }
            catch
            {
                return null;
            }

            return blobData;
        }

        internal CLIPDATA GetCLIPDATA()
        {
            return (CLIPDATA)Marshal.PtrToStructure(p, typeof(CLIPDATA));
        }

        /// <summary>
        /// Gets a byte array containing the data bits of the struct.
        /// </summary>
        /// <returns>A byte array that is the combined size of the data bits.</returns>
        private byte[] GetDataBytes()
        {
            var ret = new byte[IntPtr.Size + sizeof(int)];

            if (IntPtr.Size == 4)
            {
                BitConverter.GetBytes(p.ToInt32()).CopyTo(ret, 0);
            }
            else if (IntPtr.Size == 8)
            {
                BitConverter.GetBytes(p2).CopyTo(ret, IntPtr.Size);
            }

            return ret;
        }

        /// <summary>
        /// Called to clear the PropVariant's referenced and local memory.
        /// </summary>
        /// <remarks>
        /// You must call Clear to avoid memory leaks.
        /// </remarks>
        public void Clear()
        {
            // Can't pass "this" by ref, so make a copy to call PropVariantClear with
            PROPVARIANT var = this;
            UnsafeNativeMethods.PropVariantClear(ref var);

            // Since we couldn't pass "this" by ref, we need to clear the member fields manually
            // NOTE: PropVariantClear already freed heap data for us, so we are just setting
            //       our references to null.
            vt = (ushort)VarEnum.VT_EMPTY;
            wReserved1 = wReserved2 = wReserved3 = 0;
            p = IntPtr.Zero;
            p2 = 0;
        }

        /// <summary>
        /// Gets the variant type.
        /// </summary>
        public VarEnum Type
        {
            get { return (VarEnum)vt; }
        }

        /// <summary>
        /// Gets the variant value.
        /// </summary>
        public object Value
        {
            get
            {
                switch ((VarEnum)vt)
                {
                    case VarEnum.VT_I1:
                        return cVal;
                    case VarEnum.VT_UI1:
                        return bVal;
                    case VarEnum.VT_I2:
                        return iVal;
                    case VarEnum.VT_UI2:
                        return uiVal;
                    case VarEnum.VT_I4:
                    case VarEnum.VT_INT:
                        return lVal;
                    case VarEnum.VT_UI4:
                    case VarEnum.VT_UINT:
                        return ulVal;
                    case VarEnum.VT_I8:
                        return hVal;
                    case VarEnum.VT_UI8:
                        return uhVal;
                    case VarEnum.VT_R4:
                        return fltVal;
                    case VarEnum.VT_R8:
                        return dblVal;
                    case VarEnum.VT_BOOL:
                        return boolVal;
                    case VarEnum.VT_ERROR:
                        return scode;
                    case VarEnum.VT_CY:
                        return cyVal;
                    case VarEnum.VT_DATE:
                        return date;
                    case VarEnum.VT_FILETIME:
                        if (hVal > 0)
                        {
                            return DateTime.FromFileTime(hVal);
                        }
                        else
                        {
                            return null;
                        }
                    case VarEnum.VT_BSTR:
                        return Marshal.PtrToStringBSTR(p);
                    case VarEnum.VT_LPSTR:
                        return Marshal.PtrToStringAnsi(p);
                    case VarEnum.VT_LPWSTR:
                        return Marshal.PtrToStringUni(p);
                    case VarEnum.VT_UNKNOWN:
                        return Marshal.GetObjectForIUnknown(p);
                    case VarEnum.VT_DISPATCH:
                        return p;
                    case VarEnum.VT_CLSID:
                        return Marshal.PtrToStructure(p, typeof(Guid));
                        //default:
                        //    throw new NotSupportedException("The type of this variable is not support ('" + vt.ToString() + "')");
                }

                return null;
            }
        }

        public PROPVARIANT(string value)
        {
            this.vt = (ushort)VarEnum.VT_LPWSTR;
            this.p = Marshal.StringToCoTaskMemUni(value);
            this.p2 = 0;
            this.wReserved1 = 0;
            this.wReserved2 = 0;
            this.wReserved3 = 0;
        }

        public PROPVARIANT(Guid value)
        {
            this.vt = (ushort)VarEnum.VT_CLSID;
            byte[] guid = value.ToByteArray();
            this.p = Marshal.AllocCoTaskMem(guid.Length);
            Marshal.Copy(guid, 0, p, guid.Length);
            this.p2 = 0;
            this.wReserved1 = 0;
            this.wReserved2 = 0;
            this.wReserved3 = 0;
        }
    }

    /// <summary>
    ///   This is the CoClass that impliments the shell link interfaces.
    /// </summary>
    [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
    internal class ShellLinkCoClass { }

    /// <summary>The IShellLink interface allows Shell links to be created, modified, and resolved</summary>
    [ComImport(), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("000214F9-0000-0000-C000-000000000046")]
    interface IShellLinkW
    {
        /// <summary>Retrieves the path and file name of a Shell link object</summary>
        void GetPath([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cchMaxPath, out UnsafeNativeMethods.WIN32_FIND_DATAW pfd, UnsafeNativeMethods.SLGP_FLAGS fFlags);
        /// <summary>Retrieves the list of item identifiers for a Shell link object</summary>
        void GetIDList(out IntPtr ppidl);
        /// <summary>Sets the pointer to an item identifier list (PIDL) for a Shell link object.</summary>
        void SetIDList(IntPtr pidl);
        /// <summary>Retrieves the description string for a Shell link object</summary>
        void GetDescription([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        /// <summary>Sets the description for a Shell link object. The description can be any application-defined string</summary>
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        /// <summary>Retrieves the name of the working directory for a Shell link object</summary>
        void GetWorkingDirectory([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        /// <summary>Sets the name of the working directory for a Shell link object</summary>
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        /// <summary>Retrieves the command-line arguments associated with a Shell link object</summary>
        void GetArguments([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        /// <summary>Sets the command-line arguments for a Shell link object</summary>
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        /// <summary>Retrieves the hot key for a Shell link object</summary>
        void GetHotkey(out short pwHotkey);
        /// <summary>Sets a hot key for a Shell link object</summary>
        void SetHotkey(short wHotkey);
        /// <summary>Retrieves the show command for a Shell link object</summary>
        void GetShowCmd(out int piShowCmd);
        /// <summary>Sets the show command for a Shell link object. The show command sets the initial show state of the window.</summary>
        void SetShowCmd(int iShowCmd);
        /// <summary>Retrieves the location (path and index) of the icon for a Shell link object</summary>
        void GetIconLocation([Out(), MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
            int cchIconPath, out int piIcon);
        /// <summary>Sets the location (path and index) of the icon for a Shell link object</summary>
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        /// <summary>Sets the relative path to the Shell link object</summary>
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, int dwReserved);
        /// <summary>Attempts to find the target of a Shell link, even if it has been moved or renamed</summary>
        void Resolve(IntPtr hwnd, UnsafeNativeMethods.SLR_FLAGS fFlags);
        /// <summary>Sets the path and file name of a Shell link object</summary>
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("0000010B-0000-0000-C000-000000000046")]
    internal interface IPersistFile
    {
        #region Methods inherited from IPersist
        void GetClassID(out Guid pClassID);
        #endregion

        [PreserveSig]
        int IsDirty();

        void Load(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            int dwMode);

        void Save(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [MarshalAs(UnmanagedType.Bool)] bool fRemember);

        void SaveCompleted(
            [MarshalAs(UnmanagedType.LPWStr)] string pszFileName);

        void GetCurFile(
            out IntPtr ppszFileName);
    }

    [ComImport, Guid("886D8EEB-8CF2-4446-8D02-CDBA1DBDCF99"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPropertyStore
    {
        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetCount([Out] out uint cProps);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetAt([In] uint iProp, out PropertyKey pkey);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void GetValue([In] ref PropertyKey key, out PROPVARIANT pv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void SetValue([In] ref PropertyKey key, [In] ref PROPVARIANT pv);

        [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
        void Commit();
    }

    class ShortcutManager
    {
        /// <summary>
        /// Creates a shortcut to enable the app to receive toast notifications.
        /// </summary>
        /// <remarks>
        /// Documentation: https://docs.microsoft.com/en-us/windows/win32/shell/enable-desktop-toast-with-appusermodelid
        /// </remarks>
        /// <param name="shortcutPath">Full path to the shortcut (including .lnk), must be in Program Files or Start Menu</param>
        /// <param name="appExecutablePath">Path the to app that receives notifications</param>
        /// <param name="arguments">Optional arguments</param>
        /// <param name="appName">The name of the app - used to create the toast</param>
        /// <param name="activatorId">The activation id</param>
        public static void RegisterAppForNotifications(string shortcutPath, string appExecutablePath, string arguments, string appName, string activatorId)
        {
            var shellLinkClass = new ShellLinkCoClass();
            IShellLinkW shellLink = (IShellLinkW)shellLinkClass;
            shellLink.SetPath(appExecutablePath);

            IPropertyStore propertyStore = (IPropertyStore)shellLinkClass;
            IPersistFile persistFile = (IPersistFile)shellLinkClass;

            if (arguments != null)
            {
                shellLink.SetArguments(arguments);
            }

            // https://docs.microsoft.com/en-us/windows/win32/properties/props-system-appusermodel-id
            propertyStore.SetValue(new PropertyKey("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3", 5), new PROPVARIANT(appName));

            // https://docs.microsoft.com/en-us/windows/win32/properties/props-system-appusermodel-toastactivatorclsid
            propertyStore.SetValue(new PropertyKey("9F4C2855-9F79-4B39-A8D0-E1D42DE1D5F3", 26), new PROPVARIANT(new Guid(activatorId)));
            propertyStore.Commit();

            persistFile.Save(shortcutPath, true);
        }
    }
}