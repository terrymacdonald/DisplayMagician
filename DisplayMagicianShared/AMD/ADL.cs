#region Copyright

/*******************************************************************************
 Copyright(c) 2008 - 2009 Advanced Micro Devices, Inc. All Rights Reserved.
 Copyright (c) 2002 - 2006  ATI Technologies Inc. All Rights Reserved.
 
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
 ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDED BUT NOT LIMITED TO
 THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
 PARTICULAR PURPOSE.
 
 File:        ADL.cs
 
 Purpose:     Implements ADL interface 
 
 Description: Implements some of the methods defined in ADL interface.
              
 ********************************************************************************/

#endregion Copyright

#region Using

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using FARPROC = System.IntPtr;
using HMODULE = System.IntPtr;

#endregion Using

#region ATI.ADL

namespace ATI.ADL
{
    #region Export Delegates
    /// <summary> ADL Memory allocation function allows ADL to callback for memory allocation</summary>
    /// <param name="size">input size</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate IntPtr ADL2_Main_Memory_Alloc (int size);

    // ///// <summary> ADL Create Function to create ADL Data</summary>
    /// <param name="callback">Call back functin pointer which is ised to allocate memeory </param>
    /// <param name="enumConnectedAdapters">If it is 1, then ADL will only retuen the physical exist adapters </param>
    ///// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL2_Main_Control_Create(ADL2_Main_Memory_Alloc callback, int enumConnectedAdapters);

    /// <summary> ADL Destroy Function to free up ADL Data</summary>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL2_Main_Control_Destroy ();

    /// <summary> ADL Function to get the number of adapters</summary>
    /// <param name="numAdapters">return number of adapters</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL2_Adapter_NumberOfAdapters_Get (ref int numAdapters);

    /// <summary> ADL Function to get the GPU adapter information</summary>
    /// <param name="info">return GPU adapter information</param>
    /// <param name="inputSize">the size of the GPU adapter struct</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL2_Adapter_AdapterInfo_Get (IntPtr info, int inputSize);

    /// <summary> Function to determine if the adapter is active or not.</summary>
    /// <remarks>The function is used to check if the adapter associated with iAdapterIndex is active</remarks>  
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="status"> Status of the adapter. True: Active; False: Dsiabled</param>
    /// <returns>Non zero is successfull</returns> 
    internal delegate int ADL2_Adapter_Active_Get(int adapterIndex, ref int status);

    /// <summary>Get display information based on adapter index</summary>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="numDisplays">return the total number of supported displays</param>
    /// <param name="displayInfoArray">return ADLDisplayInfo Array for supported displays' information</param>
    /// <param name="forceDetect">force detect or not</param>
    /// <returns>return ADL Error Code</returns>
    internal delegate int ADL2_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

    /// <summary>Function to retrieve an SLS configuration.</summary>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="SLSMapIndex">Specifies the SLS map index to be queried.</param>
    /// <param name="SLSMap">return ADLSLSMap Array for supported displays' information</param>
    /// <param name="numSLSTarget">return number of targets in the SLS mapo</param>
    /// <param name="SLSTargetArray">return ADLSLSTarget Array </param>
    /// <param name="numNativeMode">return the number of native modes</param>
    /// <param name="SLSNativeMode">return ADLSLSMode Array that contains the native modes</param>
    /// <param name="numBezelMode">return the number of bezel modes</param>
    /// <param name="SLSBezelMode">return ADLSLSMode Array that contains the bezel modes</param>
    /// <param name="numTransientMode">return the number of transient modes</param>
    /// <param name="SLSTransientMode">return ADLSLSMode Array that contains the transient modes</param>
    /// <param name="numSLSOffset">return the number of SLS offsets</param>
    /// <param name="SLSOffset">return ADLSLSOffset Array that contains the SLS offsets</param>
    /// <param name="option">Specifies the layout type of SLS grid data. It is bit vector. There are two types of SLS layout:s, relative to landscape (ref \ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_LANDSCAPE) and relative to current angle (ref \ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_CURRENTANGLE).</param>
    /// <returns>return ADL Error Code</returns>
    internal delegate int ADL2_Display_SLSMapConfig_Get(int adapterIndex, int SLSMapIndex, ref ADLSLSMap SLSMap, ref int numSLSTarget, out IntPtr SLSTargetArray, ref int numNativeMode, 
        out IntPtr SLSNativeMode, ref int numBezelMode, out IntPtr SLSBezelMode, ref int numTransientMode, out IntPtr SLSTransientMode, ref int numSLSOffset, out IntPtr SLSOffset, int option);

    #endregion Export Delegates

    #region Export Struct

    #region ADLMode
    /// <summary> ADLAdapterInfo Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLMode
    {
        /// <summary> Display IDs. </summary>
        internal ADLDisplayID DisplayID;
        /// <summary> Screen refresh rate. </summary>
        internal float RefreshRate;
        /// <summary> Adapter index. </summary>
        internal int AdapterIndex;
        /// <summary> Screen Color Depth. E.g., 16, 32.  </summary>
        internal int ColourDepth;
        /// <summary> Vista mode flag indicating Progressive or Interlaced mode.  </summary>
        internal int ModeFlag;
        /// <summary> The bit mask identifying the number of bits this Mode is currently using. </summary>
        internal int ModeMask;
        /// <summary> The bit mask identifying the display status. </summary>
        internal int ModeValue;
        /// <summary> Screen orientation. E.g., 0, 90, 180, 270. </summary>
        internal int Orientation;
        /// <summary> Screen position X coordinate. </summary>
        internal int XPos;
        /// <summary> Screen resolution Width.  </summary>
        internal int XRes;
        /// <summary> Screen position Y coordinate. </summary>
        internal int YPos;
        /// <summary> Screen resolution Height. </summary>
        internal int YRes;
    }
    #endregion ADLMode

    #region ADLDisplayTarget
    /// <summary> ADLDisplayTarget Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayTarget
    {
        /// <summary> Display IDs. </summary>
        internal ADLDisplayID DisplayID;
        /// <summary> The display map index identify this manner and the desktop surface. </summary>
        internal int DisplayMapIndex;
        /// <summary> The bit mask identifies the number of bits DisplayTarget is currently using. </summary>
        internal int DisplayTargetMask;
        /// <summary> The bit mask identifies the display status. </summary>
        internal int DisplayTargetValue;
    }
    #endregion ADLDisplayTarget

    #region ADLAdapterInfo
    /// <summary> ADLAdapterInfo Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfo
    {
        /// <summary>The size of the structure</summary>
        int Size;
        /// <summary> Adapter Index</summary>
        internal int AdapterIndex;
        /// <summary> Adapter UDID</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string UDID;
        /// <summary> Adapter Bus Number</summary>
        internal int BusNumber;
        /// <summary> Adapter Driver Number</summary>
        internal int DriverNumber;
        /// <summary> Adapter Function Number</summary>
        internal int FunctionNumber;
        /// <summary> Adapter Vendor ID</summary>
        internal int VendorID;
        /// <summary> Adapter Adapter name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string AdapterName;
        /// <summary> Adapter Display name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DisplayName;
        /// <summary> Adapter Present status</summary>
        internal int Present;
        /// <summary> Adapter Exist status</summary>
        internal int Exist;
        /// <summary> Adapter Driver Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DriverPath;
        /// <summary> Adapter Driver Ext Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DriverPathExt;
        /// <summary> Adapter PNP String</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string PNPString;
        /// <summary> OS Display Index</summary>
        internal int OSDisplayIndex;
    }


    /// <summary> ADLAdapterInfo Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfoArray
    {
        /// <summary> ADLAdapterInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_ADAPTERS)]
        internal ADLAdapterInfo[] ADLAdapterInfo;
    }
    #endregion ADLAdapterInfo

        
    #region ADLDisplayInfo
    /// <summary> ADLDisplayID Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayID
    {
        /// <summary> Display Logical Index </summary>
        internal int DisplayLogicalIndex;
        /// <summary> Display Physical Index </summary>
        internal int DisplayPhysicalIndex;
        /// <summary> Adapter Logical Index </summary>
        internal int DisplayLogicalAdapterIndex;
        /// <summary> Adapter Physical Index </summary>
        internal int DisplayPhysicalAdapterIndex;
    }

    /// <summary> ADLDisplayInfo Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayInfo
    {
        /// <summary> Display Index </summary>
        internal ADLDisplayID DisplayID;
        /// <summary> Display Controller Index </summary>
        internal int DisplayControllerIndex;
        /// <summary> Display Name </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DisplayName;
        /// <summary> Display Manufacturer Name </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        internal string DisplayManufacturerName;
        /// <summary> Display Type : < The Display type. CRT, TV,CV,DFP are some of display types,</summary>
        internal int DisplayType;
        /// <summary> Display output type </summary>
        internal int DisplayOutputType;
        /// <summary> Connector type</summary>
        internal int DisplayConnector;
        ///<summary> Indicating the display info bits' mask.<summary>
        internal int DisplayInfoMask;
        ///<summary> Indicating the display info value.<summary>
        internal int DisplayInfoValue;
    }
    #endregion ADLDisplayInfo

    #region ADLSLS

    /// <summary> ADLSLSGrid Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLSLSGrid
    {
        /// <summary> The Adapter index </summary>
        internal int AdapterIndex;
        /// <summary> The grid column </summary>
        internal int SLSGridColumn;
        /// <summary> The grid index </summary>
        internal int SLSGridIndex;
        /// <summary> The grid bit mask identifies the number of bits DisplayMap is currently using. </summary>
        internal int SLSGridMask;
        /// <summary>The grid row. </summary>
        internal int SLSGridRow;
        /// <summary> The grid bit value identifies the display status. </summary>
        internal int SLSGridValue;
    }

    /// <summary> ADLSLSMap Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLSLSMap
    {
        /// <summary> The current grid </summary>
        internal ADLSLSGrid Grid;
        /// <summary> The Adapter Index </summary>
        internal int AdapterIndex;
        /// <summary> The first bezel mode array index in the native mode array </summary>
        internal int FirstBezelModeArrayIndex;
        /// <summary> The first bezel offset array index in the native mode array </summary>
        internal int FirstBezelOffsetArrayIndex;
        /// <summary>The first native mode array index in the native mode array. </summary>
        internal int FirstNativeModeArrayIndex;
        /// <summary> The first target array index in the Target array. </summary>
        internal int FirstSLSTargetArrayIndex;
        /// <summary> The number of bezel modes belongs to this map. </summary>
        internal int NumBezelMode;
        /// <summary> The number of bezel offsets belongs to this map. </summary>
        internal int NumBezelOffset;
        /// <summary> The number of native modes belongs to this map. </summary>
        internal int NumNativeMode;
        /// <summary> The number of display targets belongs to this map. </summary>
        internal int NumSLSTarget;
        /// <summary> Screen orientation. E.g., 0, 90, 180, 270. </summary>
        internal int Orientation;
        /// <summary> The current display map index. It is the OS Desktop index. </summary>
        internal int SLSMapIndex;
        /// <summary> Bitmask identifies display map status </summary>
        internal int SLSMapMask;
        /// <summary> Bitmask identifies display map status </summary>
        internal int SLSMapValue;
        /// <summary> OS Surface Index </summary>
        internal int SurfaceMapIndex;
    }

    /// <summary> ADLSLSTarget Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLSLSTarget
    {
        /// <summary> The target ID.  </summary>
        internal ADLDisplayTarget DisplayTarget;
        /// <summary> The Adapter Index </summary>
        internal int AdapterIndex;
        /// <summary> Target postion X in SLS grid </summary>
        internal int SLSGridPositionX;
        /// <summary> Target postion Y in SLS grid </summary>
        internal int SLSGridPositionY;
        /// <summary> The SLS map index. </summary>
        internal int SLSMapIndex;
        /// <summary> The bit mask identifies the bits in iSLSTargetValue are currently used.  </summary>
        internal int SLSTargetMask;
        /// <summary> The bit mask identifies status info.  </summary>
        internal int SLSTargetValue;
        /// <summary> The view size width, height and rotation angle per SLS Target.  </summary>
        internal ADLMode ViewSize;
    }

    /// <summary> ADLSLSTarget Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLSLSTargetArray
    {
        /// <summary> ADLAdapterInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_ADAPTERS)]
        internal ADLSLSTarget[] ADLSLSTarget;
    }

    /// <summary> ADLSLSMode Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLSLSMode
    {
        /// <summary> The target ID.  </summary>
        internal ADLMode DisplayMode;
        /// <summary> The Adapter Index </summary>
        internal int AdapterIndex;
        /// <summary> The current display map index. It is the OS Desktop index. </summary>
        internal int SLSMapIndex;
        /// <summary> The mode index. </summary>
        internal int SLSModeIndex;
        /// <summary> The bit mask identifies the number of bits Mode is currently using. </summary>
        internal int SLSNativeModeMask;
        /// <summary> The bit mask identifies the display status. </summary>
        internal int SLSNativeModeValue;

    }

    /// <summary> ADLBezelTransientMode Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLBezelTransientMode
    {
        /// <summary> The target ID.  </summary>
        internal ADLMode DisplayMode;
        /// <summary> The Adapter Index </summary>
        internal int AdapterIndex;
        /// <summary> The first bezel offset array index in the native mode array. </summary>
        internal int FirstBezelOffsetArrayIndex;
        /// <summary> The number of bezel offsets belongs to this map.  </summary>
        internal int NumBezelOffset;
        /// <summary> The bit mask identifies the bits this structure is currently using. </summary>
        internal int SLSBezelTransientModeMask;
        /// <summary> The bit mask identifies the display status.  </summary>
        internal int SLSBezelTransientModeValue;
        /// <summary> SLS Map Index. </summary>
        internal int SLSMapIndex;
        /// <summary> SLS Mode Index. </summary>
        internal int SLSModeIndex;
    }

    /// <summary> ADLSLSOffset Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLSLSOffset
    {
        /// <summary> The target ID.  </summary>
        internal ADLMode DisplayMode;
        /// <summary> The Adapter Index </summary>
        internal int AdapterIndex;
        /// <summary> The bit mask identifies the display status. </summary>
        internal int BezelOffsetValue;
        /// <summary> SLS Bezel Mode Index. </summary>
        internal int BezelModeIndex;
        /// <summary> The bit mask identifies the number of bits Offset is currently using. </summary>
        internal int BezelOffsetMask;
        /// <summary>SLS Bezel Offset X. </summary>
        internal int BezelOffsetX;
        /// <summary>SLS Bezel Offset Y. </summary>
        internal int BezelOffsetY;
        /// <summary> SLS Display Height. </summary>
        internal int DisplayHeight;
        /// <summary> SLS Display Width. </summary>
        internal int DisplayWidth;
        /// <summary> The current display map index. It is the OS Desktop index. </summary>
        internal int SLSMapIndex;
    }

    #endregion ADLSLS

    #endregion Export Struct

    #region ADL Class
    /// <summary> ADL Class</summary>
    internal static class ADL
    {
        #region Internal Constant
        /// <summary> Define the maximum path</summary>
        internal const int ADL_MAX_PATH = 256;
        /// <summary> Define the maximum adapters</summary>
        internal const int ADL_MAX_ADAPTERS = 40 /* 150 */;
        /// <summary> Define the maximum displays</summary>
        internal const int ADL_MAX_DISPLAYS = 40 /* 150 */;
        /// <summary> Define the maximum device name length</summary>
        internal const int ADL_MAX_DEVICENAME = 32;
        /// <summary> Define the successful</summary>
        internal const int ADL_SUCCESS = 0;
        /// <summary> Define the failure</summary>
        internal const int ADL_FAIL = -1;
        /// <summary> Define the driver ok</summary>
        internal const int ADL_DRIVER_OK = 0;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        internal const int ADL_MAX_GLSYNC_PORTS = 8;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        internal const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
        /// <summary> Maximum number of ADLMOdes for the adapter </summary>
        internal const int ADL_MAX_NUM_DISPLAYMODES = 1024; 

        #endregion Internal Constant

        #region Class ADLImport
        /// <summary> ADLImport class</summary>
        private static class ADLImport
        {
            #region Internal Constant
            /// <summary> Atiadlxx_FileName </summary>
            internal const string Atiadlxx_FileName = "atiadlxx.dll";
            /// <summary> Kernel32_FileName </summary>
            internal const string Kernel32_FileName = "kernel32.dll";
            #endregion Internal Constant

            #region DLLImport
            [DllImport(Kernel32_FileName)]
            internal static extern HMODULE GetModuleHandle (string moduleName);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Main_Control_Create (ADL2_Main_Memory_Alloc callback, int enumConnectedAdapters);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Main_Control_Destroy ();

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Main_Control_IsFunctionValid (HMODULE module, string procName);

            [DllImport(Atiadlxx_FileName)]
            internal static extern FARPROC ADL2_Main_Control_GetProcAddress (HMODULE module, string procName);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Adapter_NumberOfAdapters_Get (ref int numAdapters);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Adapter_AdapterInfo_Get (IntPtr info, int inputSize);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Adapter_Active_Get(int adapterIndex, ref int status);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Display_SLSMapConfig_Get(int adapterIndex, int SLSMapIndex, ref ADLSLSMap SLSMap, ref int NumSLSTarget, out IntPtr SLSTargetArray, ref int lpNumNativeMode, out IntPtr NativeMode, ref int NumBezelMode, out IntPtr BezelMode, ref int NumTransientMode, out IntPtr TransientMode, ref int NumSLSOffset, out IntPtr SLSOffset, int iOption);

            // This is used to set the SLS Grid we want from the SLSMap by selecting the one we want and supplying that as an index.
            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Display_SLSMapConfig_SetState(int AdapterIndex, int SLSMapIndex, int State);

            // Function to get the current supported SLS grid patterns (MxN) for a GPU.
            // This function gets a list of supported SLS grids for a specified input adapter based on display devices currently connected to the GPU.
            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Display_SLSGrid_Caps(int adapterIndex, ref int NumSLSGrid, out IntPtr SLSGrid, int option);

            // Function to get the active SLS map index list for a given GPU.
            // This function retrieves a list of active SLS map indexes for a specified input GPU.            
            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Display_SLSMapIndexList_Get(int adapterIndex, ref int numSLSMapIndexList, out IntPtr SLSMapIndexList, int options);

            // Function to get the active SLS map index list for a given GPU.
            // This function retrieves a list of active SLS map indexes for a specified input GPU.            
            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Display_SLSMapIndex_Get(int adapterIndex, int ADLNumDisplayTarget, ref ADLDisplayTarget displayTarget, ref int SLSMapIndex);

            #endregion DLLImport
        }
        #endregion Class ADLImport

        #region Class ADLCheckLibrary
        /// <summary> ADLCheckLibrary class</summary>
        private class ADLCheckLibrary
        {
            #region Private Members
            private HMODULE ADLLibrary = System.IntPtr.Zero;
            #endregion Private Members

            #region Static Members
            /// <summary> new a private instance</summary>
            private static ADLCheckLibrary ADLCheckLibrary_ = new ADLCheckLibrary();
            #endregion Static Members

            #region Constructor
            /// <summary> Constructor</summary>
            private ADLCheckLibrary ()
            {
                try
                {
                    if (1 == ADLImport.ADL2_Main_Control_IsFunctionValid(IntPtr.Zero, "ADL2_Main_Control_Create"))
                    {
                        ADLLibrary = ADLImport.GetModuleHandle(ADLImport.Atiadlxx_FileName);
                    }
                }
                catch (DllNotFoundException) { }
                catch (EntryPointNotFoundException) { }
                catch (Exception) { }
            }
            #endregion Constructor

            #region Destructor
            /// <summary> Destructor to force calling ADL Destroy function before free up the ADL library</summary>
            ~ADLCheckLibrary ()
            {
                if (System.IntPtr.Zero != ADLCheckLibrary_.ADLLibrary)
                {
                    ADLImport.ADL2_Main_Control_Destroy();
                }
            }
            #endregion Destructor

            #region Static IsFunctionValid
            /// <summary> Check the import function to see it exists or not</summary>
            /// <param name="functionName"> function name</param>
            /// <returns>return true, if function exists</returns>
            internal static bool IsFunctionValid (string functionName)
            {
                bool result = false;
                if (System.IntPtr.Zero != ADLCheckLibrary_.ADLLibrary)
                {
                    if (1 == ADLImport.ADL2_Main_Control_IsFunctionValid(ADLCheckLibrary_.ADLLibrary, functionName))
                    {
                        result = true;
                    }
                }
                return result;
            }
            #endregion Static IsFunctionValid

            #region Static GetProcAddress
            /// <summary> Get the unmanaged function pointer </summary>
            /// <param name="functionName"> function name</param>
            /// <returns>return function pointer, if function exists</returns>
            internal static FARPROC GetProcAddress (string functionName)
            {
                FARPROC result = System.IntPtr.Zero;
                if (System.IntPtr.Zero != ADLCheckLibrary_.ADLLibrary)
                {
                    result = ADLImport.ADL2_Main_Control_GetProcAddress(ADLCheckLibrary_.ADLLibrary, functionName);
                }
                return result;
            }
            #endregion Static GetProcAddress
        }
        #endregion Class ADLCheckLibrary

        #region Export Functions

        #region ADL2_Main_Memory_Alloc
        /// <summary> Build in memory allocation function</summary>
        internal static ADL2_Main_Memory_Alloc ADL2_Main_Memory_Alloc = ADL2_Main_Memory_Alloc_;
        /// <summary> Build in memory allocation function</summary>
        /// <param name="size">input size</param>
        /// <returns>return the memory buffer</returns>
        private static IntPtr ADL2_Main_Memory_Alloc_ (int size)
        {
            IntPtr result = Marshal.AllocCoTaskMem(size);
            return result;
        }
        #endregion ADL2_Main_Memory_Alloc

        #region ADL2_Main_Memory_Free
        /// <summary> Build in memory free function</summary>
        /// <param name="buffer">input buffer</param>
        internal static void ADL2_Main_Memory_Free (IntPtr buffer)
        {
            if (IntPtr.Zero != buffer)
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }
        #endregion ADL2_Main_Memory_Free

        #region ADL2_Main_Control_Create
        /// <summary> ADL2_Main_Control_Create Delegates</summary>
        internal static ADL2_Main_Control_Create ADL2_Main_Control_Create
        {
            get
            {
                if (!ADL2_Main_Control_Create_Check && null == ADL2_Main_Control_Create_)
                {
                    ADL2_Main_Control_Create_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Main_Control_Create"))
                    {
                        ADL2_Main_Control_Create_ = ADLImport.ADL2_Main_Control_Create;
                    }
                }
                return ADL2_Main_Control_Create_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Main_Control_Create ADL2_Main_Control_Create_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Main_Control_Create_Check = false;
        #endregion ADL2_Main_Control_Create

        #region ADL2_Main_Control_Destroy
        /// <summary> ADL2_Main_Control_Destroy Delegates</summary>
        internal static ADL2_Main_Control_Destroy ADL2_Main_Control_Destroy
        {
            get
            {
                if (!ADL2_Main_Control_Destroy_Check && null == ADL2_Main_Control_Destroy_)
                {
                    ADL2_Main_Control_Destroy_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Main_Control_Destroy"))
                    {
                        ADL2_Main_Control_Destroy_ = ADLImport.ADL2_Main_Control_Destroy;
                    }
                }
                return ADL2_Main_Control_Destroy_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Main_Control_Destroy ADL2_Main_Control_Destroy_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Main_Control_Destroy_Check = false;
        #endregion ADL2_Main_Control_Destroy

        #region ADL2_Adapter_NumberOfAdapters_Get
        /// <summary> ADL2_Adapter_NumberOfAdapters_Get Delegates</summary>
        internal static ADL2_Adapter_NumberOfAdapters_Get ADL2_Adapter_NumberOfAdapters_Get
        {
            get
            {
                if (!ADL2_Adapter_NumberOfAdapters_Get_Check && null == ADL2_Adapter_NumberOfAdapters_Get_)
                {
                    ADL2_Adapter_NumberOfAdapters_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Adapter_NumberOfAdapters_Get"))
                    {
                        ADL2_Adapter_NumberOfAdapters_Get_ = ADLImport.ADL2_Adapter_NumberOfAdapters_Get;
                    }
                }
                return ADL2_Adapter_NumberOfAdapters_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Adapter_NumberOfAdapters_Get ADL2_Adapter_NumberOfAdapters_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Adapter_NumberOfAdapters_Get_Check = false;
        #endregion ADL2_Adapter_NumberOfAdapters_Get

        #region ADL2_Adapter_AdapterInfo_Get
        /// <summary> ADL2_Adapter_AdapterInfo_Get Delegates</summary>
        internal static ADL2_Adapter_AdapterInfo_Get ADL2_Adapter_AdapterInfo_Get
        {
            get
            {
                if (!ADL2_Adapter_AdapterInfo_Get_Check && null == ADL2_Adapter_AdapterInfo_Get_)
                {
                    ADL2_Adapter_AdapterInfo_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Adapter_AdapterInfo_Get"))
                    {
                        ADL2_Adapter_AdapterInfo_Get_ = ADLImport.ADL2_Adapter_AdapterInfo_Get;
                    }
                }
                return ADL2_Adapter_AdapterInfo_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Adapter_AdapterInfo_Get ADL2_Adapter_AdapterInfo_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Adapter_AdapterInfo_Get_Check = false;
        #endregion ADL2_Adapter_AdapterInfo_Get

        #region ADL2_Adapter_Active_Get
        /// <summary> ADL2_Adapter_Active_Get Delegates</summary>
        internal static ADL2_Adapter_Active_Get ADL2_Adapter_Active_Get
        {
            get
            {
                if (!ADL2_Adapter_Active_Get_Check && null == ADL2_Adapter_Active_Get_)
                {
                    ADL2_Adapter_Active_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Adapter_Active_Get"))
                    {
                        ADL2_Adapter_Active_Get_ = ADLImport.ADL2_Adapter_Active_Get;
                    }
                }
                return ADL2_Adapter_Active_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Adapter_Active_Get ADL2_Adapter_Active_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Adapter_Active_Get_Check = false;
        #endregion ADL2_Adapter_Active_Get

        #region ADL2_Display_DisplayInfo_Get
        /// <summary> ADL2_Display_DisplayInfo_Get Delegates</summary>
        internal static ADL2_Display_DisplayInfo_Get ADL2_Display_DisplayInfo_Get
        {
            get
            {
                if (!ADL2_Display_DisplayInfo_Get_Check && null == ADL2_Display_DisplayInfo_Get_)
                {
                    ADL2_Display_DisplayInfo_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_DisplayInfo_Get"))
                    {
                        ADL2_Display_DisplayInfo_Get_ = ADLImport.ADL2_Display_DisplayInfo_Get;
                    }
                }
                return ADL2_Display_DisplayInfo_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_DisplayInfo_Get ADL2_Display_DisplayInfo_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_DisplayInfo_Get_Check = false;
        #endregion ADL2_Display_DisplayInfo_Get

        #region ADL2_Display_SLSMapConfig_Get
        /// <summary> ADL2_Display_SLSMapConfig_Get Delegates</summary>
        internal static ADL2_Display_SLSMapConfig_Get ADL2_Display_SLSMapConfig_Get
        {
            get
            {
                if (!ADL2_Display_SLSMapConfig_Get_Check && null == ADL2_Display_SLSMapConfig_Get_)
                {
                    ADL2_Display_SLSMapConfig_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_SLSMapConfig_Get"))
                    {
                        ADL2_Display_SLSMapConfig_Get_ = ADLImport.ADL2_Display_SLSMapConfig_Get;
                    }
                }
                return ADL2_Display_SLSMapConfig_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_SLSMapConfig_Get ADL2_Display_SLSMapConfig_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_SLSMapConfig_Get_Check = false;
        #endregion ADL2_Display_SLSMapConfig_Get
        

        #endregion Export Functions
    }
    #endregion ADL Class
}

#endregion ATI.ADL