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
    internal delegate IntPtr ADL_Main_Memory_Alloc (int size);

    // ADL2 version of function delagates

    /// <summary> ADL2 Create Function to create ADL Data</summary>
    /// <param name="callback">Call back functin pointer which is ised to allocate memory </param>
    /// <param name="numConnectedAdapters">If it is 1, then ADL will only retuen the physical exist adapters </param>
    /// <param name="contextHandle">Handle to ADL client context.</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL2_Main_Control_Create(ADL_Main_Memory_Alloc callback, int numConnectedAdapters, out IntPtr contextHandle);

    /// <summary> ADL2 Destroy Function to free up ADL Data</summary>
    /// <param name="contextHandle">Handle to ADL client context.</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL2_Main_Control_Destroy(IntPtr contextHandle);

    /// <summary> ADL2 Function to get the number of adapters</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="numAdapters">return number of adapters</param>
    internal delegate int ADL2_Adapter_NumberOfAdapters_Get(IntPtr ADLContextHandle, ref int numAdapters);

    /// <summary> ADL2 Function to determine if the adapter is active or not.</summary>
    /// <remarks>The function is used to check if the adapter associated with iAdapterIndex is active</remarks>  
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="status"> Status of the adapter. True: Active; False: Disabled</param>
    /// <returns>Non zero is successful</returns> 
    internal delegate int ADL2_Adapter_Active_Get(IntPtr ADLContextHandle, int adapterIndex, ref int status);

    /// <summary>ADL2 Function to retrieve adapter capability information.</summary>
    /// <remarks>This function implements a DI call to retrieve adapter capability information .</remarks>  
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="adapterCapabilities"> The pointer to the ADLAdapterCaps structure storing the retrieved adapter capability information.</param>
    /// <returns>return ADL Error Code</returns> 
    internal delegate int ADL2_AdapterX2_Caps(IntPtr ADLContextHandle, int adapterIndex, out ADLAdapterCapsX2 adapterCapabilities);


    /// <summary>ADL2 Function to retrieve all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterInfoArray">return GPU adapter information</param>
    /// <param name="inputSize">the size of the GPU adapter struct</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL2_Adapter_AdapterInfo_Get(IntPtr ADLContextHandle, int inputSize, out IntPtr adapterInfoArray);

    /// <summary>ADL2 function retrieves all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterInfoArray">return GPU adapter information. Is a pointer to the pointer of AdapterInfo array. Initialize to NULL before calling this API. ADL will allocate the necessary memory, using the user provided callback function.</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL2_Adapter_AdapterInfoX2_Get(IntPtr ADLContextHandle, out IntPtr adapterInfoArray);

    /// <summary>ADL2 function retrieves all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">The ADL index handle of the desired adapter or -1 if all adapters are desired</param>
    /// <param name="numAdapters">Number of items in the AdapterInfo Array. Can pass NULL pointer if passign an adapter index (in which case only one AdapterInfo is returned)</param>
    /// <param name="adapterInfoArray">return GPU adapter information</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int  ADL2_Adapter_AdapterInfoX3_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr adapterInfoArray);

    /// <summary>ADL2 function retrieves all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">The ADL index handle of the desired adapter or -1 if all adapters are desired</param>
    /// <param name="numAdapters">Number of items in the AdapterInfo Array. Can pass NULL pointer if passign an adapter index (in which case only one AdapterInfo is returned)</param>
    /// <param name="adapterInfoX2Array">return GPU adapter information in adapterInfoX2 array</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate  int ADL2_Adapter_AdapterInfoX4_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr adapterInfoX2Array);

    /// <summary> ADL2 Create Function to create ADL Data</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">Display Index</param>
    /// <param name="displayDDCInfo2">The ADLDDCInfo2 structure storing all DDC retrieved from the driver.</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL2_Display_DDCInfo2_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out ADLDDCInfo2 displayDDCInfo2);

    /// <summary>ADL2 function to get display information based on adapter index</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="numDisplays">return the total number of supported displays</param>
    /// <param name="displayInfoArray">return ADLDisplayInfo Array for supported displays' information</param>
    /// <param name="forceDetect">force detect or not</param>
    /// <returns>return ADL Error Code</returns>
    internal delegate int ADL2_Display_DisplayInfo_Get(IntPtr ADLContextHandle, int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

    /// <summary>This ADL2 function retrieves HDTV capability settings for a specified display.</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">Display Index</param>
    /// <param name="displayConfig">return ADLDisplayConfig with HDTV capability settings in it</param>
    /// <returns>return ADL Error Code</returns>
    internal delegate int ADL2_Display_DeviceConfig_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out ADLDisplayConfig displayConfig);

    // ADL version of function delagates

    /// <summary> ADL Create Function to create ADL Data</summary>
    /// <param name="callback">Call back functin pointer which is ised to allocate memeory </param>
    /// <param name="enumConnectedAdapters">If it is 1, then ADL will only retuen the physical exist adapters </param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL_Main_Control_Create(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);   

    /// <summary> ADL Destroy Function to free up ADL Data</summary>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL_Main_Control_Destroy ();  

    /// <summary> ADL Function to get the number of adapters</summary>
    /// <param name="numAdapters">return number of adapters</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL_Adapter_NumberOfAdapters_Get (ref int numAdapters);

    /// <summary> Function to determine if the adapter is active or not.</summary>
    /// <remarks>The function is used to check if the adapter associated with iAdapterIndex is active</remarks>  
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="status"> Status of the adapter. True: Active; False: Disabled</param>
    /// <returns>Non zero is successful</returns> 
    internal delegate int ADL_Adapter_Active_Get(int adapterIndex, ref int status);

    /// <summary> Function to get the unique identifier of an adapter.</summary>
    /// <remarks>This function retrieves the unique identifier of a specified adapter. The adapter ID is a unique value and will be used to determine what other controllers share the same adapter. The desktop will use this to find which HDCs are associated with an adapter.</remarks>  
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="adapterId"> The pointer to the adapter identifier. Zero means: The adapter is not AMD.</param>
    /// <returns>return ADL Error Code</returns> 
    internal delegate int ADL_Adapter_ID_Get(int adapterIndex, ref int adapterId);

    /// <summary>Function to retrieve adapter capability information.</summary>
    /// <remarks>This function implements a DI call to retrieve adapter capability information .</remarks>  
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="adapterCapabilities"> The pointer to the ADLAdapterCaps structure storing the retrieved adapter capability information.</param>
    /// <returns>return ADL Error Code</returns> 
    internal delegate int ADL_AdapterX2_Caps(int adapterIndex, out ADLAdapterCapsX2 adapterCapabilities);

    /// <summary>Retrieves all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="adapterInfoArray">return GPU adapter information</param>
    /// <param name="inputSize">the size of the GPU adapter struct</param>
    /// <returns> retrun ADL Error Code</returns>
    internal delegate int ADL_Adapter_AdapterInfo_Get(out IntPtr adapterInfoArray, int inputSize);


    /// <summary>Function to get the EDID data.</summary>
    /// <remarks>This function retrieves the EDID data for a specififed display.</remarks>  
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">The desired display index. It can be retrieved from the ADLDisplayInfo data structure.</param>
    /// <param name="EDIDData">return the ADLDisplayEDIDData structure storing the retrieved EDID data.</param>
    /// <returns>return ADL Error Code</returns>
    internal delegate int ADL_Display_EdidData_Get(int adapterIndex, int displayIndex, ref ADLDisplayEDIDData EDIDData);

    /// <summary>Get display information based on adapter index</summary>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="numDisplays">return the total number of supported displays</param>
    /// <param name="displayInfoArray">return ADLDisplayInfo Array for supported displays' information</param>
    /// <param name="forceDetect">force detect or not</param>
    /// <returns>return ADL Error Code</returns>
    internal delegate int ADL_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

    /// <summary>This function retrieves HDTV capability settings for a specified display.</summary>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">Display Index</param>
    /// <param name="displayConfig">return ADLDisplayConfig with HDTV capability settings in it</param>
    /// <returns>return ADL Error Code</returns>
    internal delegate int ADL_Display_DeviceConfig_Get(int adapterIndex, int displayIndex, out ADLDisplayConfig displayConfig);

    /// <summary>Function to retrieve current display map configurations.</summary>
    /// <remarks>This function retrieves the current display map configurations, including the controllers and adapters mapped to each display.</remarks>  
    /// <param name="adapterIndex">	The ADL index handle of the desired adapter. A value of -1 returns all display configurations for the system across multiple GPUs.</param>
    /// <param name="numDisplayMap">Number of returned Display Maps</param>
    /// <param name="displayMap">Array of ADLDisplayMap objects</param>
    /// <param name="numDisplayTarget">Number of Display Targets</param>
    /// <param name="displayTarget">Array of ADLDisplayTarget objects</param>
    /// <param name="options">Options supplied</param>
    /// <returns>return ADL Error Code</returns>
    internal delegate int ADL_Display_DisplayMapConfig_Get(int adapterIndex, out int numDisplayMap, out IntPtr displayMap, out int numDisplayTarget, out IntPtr displayTarget, int options);

    /// <summary>Function to validate a list of display configurations.</summary>
    /// <remarks>This function allows the user to input a potential displays map and its targets. The function can also be used to obtain a list of display targets that can be added to this given topology and a list of display targets that can be removed from this given topology.</remarks>  
    /// <param name="adapterIndex">	The ADL index handle of the desired adapter. Cannot be set to -1 </param>
    /// <param name="numDisplayMap">Number of Display Map</param>
    /// <param name="displayMap">Number of Display Map</param>
    /// <param name="numDisplayTarget">Number of Display Map</param>
    /// <param name="displayTarget">Number of Display Map</param>
    /// <param name="numPossibleAddTarget">Number of Display Map</param>
    /// <param name="possibleAddTarget">Number of Display Map</param>
    /// <param name="numPossibleRemoveTarget">Number of Display Map</param>
    /// <param name="possibleRemoveTarget">Number of Display Map</param>
    /// <returns>return ADL Error Code</returns>
    internal delegate int ADL_Display_DisplayMapConfig_PossibleAddAndRemove(int adapterIndex, int numDisplayMap, ADLDisplayMap displayMap, int numDisplayTarget, ADLDisplayTarget displayTarget, out int numPossibleAddTarget, out IntPtr possibleAddTarget, out int numPossibleRemoveTarget, out IntPtr possibleRemoveTarget);

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
    internal delegate int ADL_Display_SLSMapConfig_Get(int adapterIndex, int SLSMapIndex, ref ADLSLSMap SLSMap, ref int numSLSTarget, out IntPtr SLSTargetArray, ref int numNativeMode, 
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
    /// <summary> ADLDisplayTarget </summary>
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

    /// <summary> ADLDisplayTargetArray Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayTargetArray
    {
        /// <summary> ADLDisplayTarget Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_DISPLAYS)]
        internal ADLDisplayTarget[] ADLDisplayTarget;
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

    /// <summary> ADLAdapterInfoX2 Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfoX2
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
        /// <summary> Adapter Device Number</summary>
        internal int DeviceNumber;
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
        /// <summary> Display Info Mask</summary>
        internal int InfoMask;
        /// <summary> Display Info Value</summary>
        internal int InfoValue;
    }

    /// <summary> ADLAdapterInfoX2 Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterInfoX2Array
    {
        /// <summary> ADLAdapterInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_ADAPTERS)]
        internal ADLAdapterInfoX2[] ADLAdapterInfoX2;
    }

    #endregion ADLAdapterInfo


    #region ADLDisplayInfo

    /// <summary> ADLDisplayEDIDData Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayEDIDData
    {
        /// <summary> EDIDData [256] </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_EDIDDATA_SIZE)]
        internal string EDIDData;
        /// <summary> Block Index </summary>
        internal int BlockIndex;
        /// <summary> EDIDSize </summary>
        internal int EDIDSize;
        /// <summary> Flag</summary>
        internal int Flag;
        /// <summary> Reserved </summary>
        internal int Reserved;
        /// <summary> Size</summary>
        internal int Size;
    }


    /// <summary> ADLDDCInfo2 Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDDCInfo2
    {
        /// <summary> Size of the structure. </summary>
        internal int Size;
        /// <summary> Whether this display device support DDC</summary>
        internal int SupportsDDC;
        /// <summary> Returns the manufacturer ID of the display device. Should be zeroed if this information is not available.</summary>
        internal int ManufacturerID;
        /// <summary> Returns the product ID of the display device. Should be zeroed if this informatiadlon is not available.</summary>
        internal int ProductID;
        /// <summary> Returns the name of the display device. Should be zeroed if this information is not available.</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_DISPLAY_NAME)]
        internal string DisplayName;
        /// <summary> Returns the maximum Horizontal supported resolution. Should be zeroed if this information is not available.</summary>
        internal int MaxHResolution;
        /// <summary> Returns the maximum Vertical supported resolution. Should be zeroed if this information is not available. </summary>
        internal int MaxVResolution;
        /// <summary> Returns the maximum supported refresh rate. Should be zeroed if this information is not available. </summary>
        internal int MaxRefresh;
        /// <summary> Returns the display device preferred timing mode's horizontal resolution.</summary>
        internal int PTMCx;
        /// <summary> Returns the display device preferred timing mode's vertical resolution. </summary>
        internal int PTMCy;
        /// <summary> Returns the display device preferred timing mode's refresh rate.</summary>
        internal int PTMRefreshRate;
        /// <summary> Return EDID flags.</summary>
        internal int DDCInfoFlag;
        /// <summary> Returns 1 if the display supported packed pixel, 0 otherwise. </summary>
        internal int PackedPixelSupported;
        /// <summary> Returns the Pixel formats the display supports DDCInfo Pixel Formats.</summary>
        internal int PanelPixelFormat;
        /// <summary> Return EDID serial ID.</summary>
        internal int SerialID;
        /// <summary> Return minimum monitor luminance data.</summary>
        internal int MinLuminanceData;
        /// <summary> Return average monitor luminance data. </summary>
        internal int AvgLuminanceData;
        /// <summary> Return maximum monitor luminance data.</summary>
        internal int MaxLuminanceData;
        /// <summary> Bit vector of supported transfer functions ADLSourceContentAttributes transfer functions (gamma). </summary>
        internal int SupportedTransferFunction;
        /// <summary> Bit vector of supported color spaces ADLSourceContentAttributes color spaces.</summary>
        internal int SupportedColorSpace;
        /// <summary> Display Red Chromaticity X coordinate multiplied by 10000.</summary>
        internal int NativeDisplayChromaticityRedX;
        /// <summary> Display Red Chromaticity Y coordinate multiplied by 10000.</summary>
        internal int NativeDisplayChromaticityRedY;
        /// <summary> Display Green Chromaticity X coordinate multiplied by 10000.</summary>
        internal int NativeDisplayChromaticityGreenX;
        /// <summary> Display Green  Chromaticity Y coordinate multiplied by 10000.</summary>
        internal int NativeDisplayChromaticityGreenY;
        /// <summary> Display Blue Chromaticity X coordinate multiplied by 10000.</summary>
        internal int NativeDisplayChromaticityBlueX;
        /// <summary> Display Blue Chromaticity Y coordinate multiplied by 10000.</summary>
        internal int NativeDisplayChromaticityBlueY;
        /// <summary> Display White Chromaticity X coordinate multiplied by 10000.</summary>
        internal int NativeDisplayChromaticityWhiteX;
        /// <summary> Display White Chromaticity Y coordinate multiplied by 10000.</summary>
        internal int NativeDisplayChromaticityWhiteY;
        /// <summary> Display diffuse screen reflectance 0-1 (100%) in units of 0.01.</summary>
        internal int DiffuseScreenReflectance;
        /// <summary> Display specular screen reflectance 0-1 (100%) in units of 0.01.</summary>
        internal int SpecularScreenReflectance;
        /// <summary> Bit vector of supported color spaces ADLDDCInfo2 HDR support options.</summary>
        internal int SupportedHDR;
        /// <summary> Bit vector for freesync flags.</summary>
        internal int FreesyncFlags;
        /// <summary> Return minimum monitor luminance without dimming data.</summary>
        internal int MinLuminanceNoDimmingData;
        /// <summary> Returns the maximum backlight maximum luminance. Should be zeroed if this information is not available.</summary>
        internal int MaxBacklightMaxLuminanceData;
        /// <summAry> Returns the minimum backlight maximum luminance. Should be zeroed if this information is not available.</summary>
        internal int MinBacklightMaxLuminanceData;
        /// <summary> Returns the maximum backlight minimum luminance. Should be zeroed if this information is not available.</summary>
        internal int MaxBacklightMinLuminanceData;
        /// <summary> Returns the minimum backlight maximum luminance. Should be zeroed if this information is not available.</summary>
        internal int MinBacklightMinLuminanceData;
        /// <summary> Reserved </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        internal int[] Reserved;
    }

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

    internal struct ConvertedDisplayInfoValue
    {
        /// <summary> Indicates the display is connected .</summary>
        internal bool DISPLAYCONNECTED;
        /// <summary> Indicates the display is mapped within OS </summary>
        internal bool DISPLAYMAPPED;
        /// <summary> Indicates the display can be forced </summary>
        internal bool FORCIBLESUPPORTED;
        /// <summary> Indicates the display supports genlock </summary>
        internal bool GENLOCKSUPPORTED;
        /// <summary> Indicates the display is an LDA display.</summary>
        internal bool LDA_DISPLAY;
        /// <summary> Indicates the display supports 2x Horizontal stretch </summary>
        internal bool MANNER_SUPPORTED_2HSTRETCH;
        /// <summIndicates the display supports 2x Vertical stretch </summary>
        internal bool MANNER_SUPPORTED_2VSTRETCH;
        /// <summary> Indicates the display supports cloned desktops </summary>
        internal bool MANNER_SUPPORTED_CLONE;
        /// <summary> Indicates the display supports extended desktops </summary>
        internal bool MANNER_SUPPORTED_EXTENDED;
        /// <summary> Indicates the display supports N Stretched on 1 GPU</summary>
        internal bool MANNER_SUPPORTED_NSTRETCH1GPU;
        /// <summary> Indicates the display supports N Stretched on N GPUs</summary>
        internal bool MANNER_SUPPORTED_NSTRETCHNGPU;
        /// <summary> Reserved display info flag #2</summary>
        internal bool MANNER_SUPPORTED_RESERVED2;
        /// <summary> Reserved display info flag #3</summary>
        internal bool MANNER_SUPPORTED_RESERVED3;
        /// <summary> Indicates the display supports single desktop </summary>
        internal bool MANNER_SUPPORTED_SINGLE;
        /// <summary> Indicates the display supports overriding the mode timing </summary>
        internal bool MODETIMING_OVERRIDESSUPPORTED;
        /// <summary> Indicates the display supports multi-vpu</summary>
        internal bool MULTIVPU_SUPPORTED;
        /// <summary> Indicates the display is non-local to this machine </summary>
        internal bool NONLOCAL;
        /// <summary> Indicates the display is a projector </summary>
        internal bool SHOWTYPE_PROJECTOR;
    }

    /// <summary> ADLDisplayInfo Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayInfoArray
    {
        /// <summary> ADLDisplayInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_DISPLAYS)]
        internal ADLDisplayInfo[] ADLDisplayInfo;
    }

    /// <summary> ADLDisplayConfig Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayConfig
    {
        /// <summary> HDTV Connector Type </summary>
        internal long ConnectorType;
        /// <summary> HDTV Capabilities themselves </summary>
        internal long DeviceData;
        /// <summary> Overridden HDTV capabilities</summary>
        internal long OverriddedDeviceData;
        /// <summary> Reserved for future use</summary>
        internal long Reserved;
        /// <summary> Size of this data structure </summary>
        internal long Size;
    }

    /// <summary> ADLDisplayMap Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayMap
    {
        /// <summary> The Display Mode for the current map.</summary>
        internal ADLMode DisplayMode;
        /// <summary> The current display map index. It is the OS desktop index. For example, if the OS index 1 is showing clone mode, the display map will be 1. </summary>
        internal int DisplayMapIndex;
        /// <summary> The bit mask identifies the number of bits DisplayMap is currently using. It is the sum of all the bit definitions defined in ADL_DISPLAY_DISPLAYMAP_MANNER_xxx.</summary>
        internal int DisplayMapMask;
        /// <summary> The bit mask identifies the display status. The detailed definition is in ADL_DISPLAY_DISPLAYMAP_MANNER_xxx.</summary>
        internal int DisplayMapValue;
        /// <summary> The first target array index in the Target array </summary>
        internal int FirstDisplayTargetArrayIndex;
        /// <summary> The number of display targets belongs to this map </summary>
        internal int NumDisplayTarget;
    }

    /// <summary> ADLDisplayMapArray Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLDisplayMapArray
    {
        /// <summary> ADLAdapterInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_DISPLAYS)]
        internal ADLDisplayMap[] ADLDisplayMap;
    }

    /// <summary> ADLAdapterCaps Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct ADLAdapterCapsX2
    {
        /// <summary> AdapterID for this adapter </summary>
        internal int AdapterID;
        /// <summary> Number of controllers for this adapter. </summary>
        internal int NumControllers;
        /// <summary> Number of displays for this adapter.</summary>
        internal int NumDisplays;
        /// <summary> Number of overlays for this adapter.</summary>
        internal int NumOverlays;
        /// <summary> Number of GLSyncConnectors. </summary>
        internal int NumOfGLSyncConnectors;
        /// <summary> The bit mask identifies the adapter caps. </summary>
        internal int CapsMask;
        /// <summary> The bit identifies the adapter caps define_adapter_caps. </summary>
        internal int CapsValue;
        /// <summary> Number of Connectors for this adapter. </summary>
        internal int NumConnectors;
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
        /// <summary> Selects all adapters instead of aparticular single adapter</summary>
        internal const int ADL_ADAPTER_INDEX_ALL = -1;
        /// <summary> Define the maximum path</summary>
        internal const int ADL_MAX_PATH = 256;
        /// <summary> Define the maximum adapters</summary>
        internal const int ADL_MAX_ADAPTERS = 40 /* 150 */;
        /// <summary> Define the maximum displays</summary>
        internal const int ADL_MAX_DISPLAYS = 40 /* 150 */;
        /// <summary> Define the maximum device name length</summary>
        internal const int ADL_MAX_DEVICENAME = 32;
        /// <summary> Define the maximum EDID Data length</summary>
        internal const int ADL_MAX_EDIDDATA_SIZE = 256;
        /// <summary> Define the maximum display names</summary>
        internal const int ADL_MAX_DISPLAY_NAME = 256;

        // Result Codes
        /// <summary> ADL function completed successfully. </summary>                
        internal const int ADL_OK = 0;
        /// <summary> Generic Error.Most likely one or more of the Escape calls to the driver failed!</summary>
        internal const int ADL_ERR = -1;
        /// <summary> Call can't be made due to disabled adapter. </summary>
        internal const int ADL_ERR_DISABLED_ADAPTER = -10;
        /// <summary> Invalid ADL index passed. </summary>
        internal const int ADL_ERR_INVALID_ADL_IDX = -5;
        /// <summary> Invalid Callback. </summary>
        internal const int ADL_ERR_INVALID_CALLBACK = -11;
        /// <summary> Invalid controller index passed.</summary>
        internal const int ADL_ERR_INVALID_CONTROLLER_IDX = -6;
        /// <summary> Invalid display index passed.</summary>
        internal const int ADL_ERR_INVALID_DISPLAY_IDX = -7;
        /// <summary> One of the parameter passed is invalid.</summary>
        internal const int ADL_ERR_INVALID_PARAM = -3;
        /// <summary> One of the parameter size is invalid.</summary>
        internal const int ADL_ERR_INVALID_PARAM_SIZE = -4;
        /// <summary> There's no Linux XDisplay in Linux Console environment.</summary>
        internal const int ADL_ERR_NO_XDISPLAY = -21;
        /// <summary> ADL not initialized.</summary>
        internal const int ADL_ERR_NOT_INIT = -2;
        /// <summary> Function not supported by the driver. </summary>
        internal const int ADL_ERR_NOT_SUPPORTED = -8;
        /// <summary> Null Pointer error.</summary>
        internal const int ADL_ERR_NULL_POINTER = -9;
        /// <summary> Display Resource conflict.</summary>
        internal const int ADL_ERR_RESOURCE_CONFLICT = -12;
        /// <summary> Err Set incomplete</summary>
        internal const int ADL_ERR_SET_INCOMPLETE = -20;
        /// <summary> All OK but need mode change. </summary>
        internal const int ADL_OK_MODE_CHANGE = 2;
        /// <summary> All OK, but need restart.</summary>
        internal const int ADL_OK_RESTART = 3;
        /// <summary> All OK, but need to wait</summary>
        internal const int ADL_OK_WAIT = 4;
        /// <summary> All OK, but with warning.</summary>
        internal const int ADL_OK_WARNING = 1;


        /// <summary> Define the driver ok</summary>
        internal const int ADL_DRIVER_OK = 0;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        internal const int ADL_MAX_GLSYNC_PORTS = 8;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        internal const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
        /// <summary> Maximum number of ADLModes for the adapter </summary>
        internal const int ADL_MAX_NUM_DISPLAYMODES = 1024;
        /// <summary> Define true </summary>
        internal const int ADL_TRUE = 1;
        /// <summary> Maximum number of ADLModes for the adapter </summary>
        internal const int ADL_FALSE = 0;
        /// <summary> Indicates the active dongle, all types </summary>
        internal const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE = 12;
        /// <summary> Indicates the Active dongle DP->DVI(double link) connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_DVI_DL = 6;
        /// <summary> Indicates the Active dongle DP->DVI(single link) connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_DVI_SL = 5;
        /// <summary> Indicates the Active dongle DP->HDMI connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_HDMI = 7;
        /// <summary> Indicates the Active dongle DP->VGA connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_VGA = 8;
        /// <summary> Indicates the DISPLAY PORT connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_DISPLAY_PORT = 4;
        /// <summary> Indicates the DVI_I connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_DVI = 1;
        /// <summary> Indicates the DVI_SL connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_DVI_SL = 2;
        /// <summary> Indicates the HDMI connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_HDMI = 3;
        /// <summary> Indicates the MST type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_MST = 11;
        /// <summary> Indicates the Active dongle DP->VGA connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_PASSIVE_DONGLE_DP_DVI = 10;
        /// <summary> Indicates the Passive dongle DP->HDMI connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_PASSIVE_DONGLE_DP_HDMI = 9;
        /// <summary> Indicates the VGA connection type is valid. </summary>
        internal const int ADL_CONNECTION_TYPE_VGA = 0;
        /// <summary> Indicates the Virtual Connection Type.</summary>
        internal const int ADL_CONNECTION_TYPE_VIRTUAL = 13;
        /// <summary> Indicates Active Dongle-JP Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_ATICVDONGLE_JP = 5;
        /// <summary> Indicates Active Dongle-NA Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_ATICVDONGLE_NA = 4;
        /// <summary> Indicates Active Dongle-NONI2C Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_ATICVDONGLE_NONI2C = 6;
        /// <summary> Indicates Active Dongle-NONI2C-D Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_ATICVDONGLE_NONI2C_D = 7;
        /// <summary> Indicates Display port Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_DISPLAYPORT = 10;
        /// <summary> Indicates DVI-D Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_DVI_D = 2;
        /// <summary> Indicates DVI-I Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_DVI_I = 3;
        /// <summary> Indicates EDP Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_EDP = 11;
        /// <summary> Indicates HDMI-Type A Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_HDMI_TYPE_A = 8;
        /// <summary> Indicates HDMI-Type B Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_HDMI_TYPE_B = 9;
        /// <summary> Indicates MiniDP Connector type. </summary>
        internal const int ADL_CONNECTOR_TYPE_MINI_DISPLAYPORT = 12;
        /// <summary> Indicates Unknown Connector type. </summary>
        internal const int ADL_CONNECTOR_TYPE_UNKNOWN = 0;
        /// <summary> Indicates USB type C Connector type. </summary>
        internal const int ADL_CONNECTOR_TYPE_USB_TYPE_C = 14;
        /// <summary> Indicates VGA Connector type.  </summary>
        internal const int ADL_CONNECTOR_TYPE_VGA = 1;
        /// <summary> Indicates Virtual Connector type.</summary>
        internal const int ADL_CONNECTOR_TYPE_VIRTUAL = 13;

        // Display Info Constants
        /// <summary> Indicates the display is connected .</summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED = 0x00000001;
        /// <summary> Indicates the display is mapped within OS </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_DISPLAYMAPPED = 0x00000002;
        /// <summary> Indicates the display can be forced </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_FORCIBLESUPPORTED = 0x00000008;
        /// <summary> Indicates the display supports genlock </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_GENLOCKSUPPORTED = 0x00000010;
        /// <summary> Indicates the display is an LDA display.</summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_LDA_DISPLAY = 0x00000040;
        /// <summary> Indicates the display supports 2x Horizontal stretch </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2HSTRETCH = 0x00000800;
        /// <summary> Indicates the display supports 2x Vertical stretch </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2VSTRETCH = 0x00000400;
        /// <summary> Indicates the display supports cloned desktops </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_CLONE = 0x00000200;
        /// <summary> Indicates the display supports extended desktops </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_EXTENDED = 0x00001000;
        /// <summary> Indicates the display supports N Stretched on 1 GPU</summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCH1GPU = 0x00010000;
        /// <summary> Indicates the display supports N Stretched on N GPUs</summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCHNGPU = 0x00020000;
        /// <summary> Reserved display info flag #2</summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED2 = 0x00040000;
        /// <summary> Reserved display info flag #3</summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED3 = 0x00080000;
        /// <summary> Indicates the display supports single desktop </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_SINGLE = 0x00000100;
        /// <summary> Indicates the display supports overriding the mode timing </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MODETIMING_OVERRIDESSUPPORTED = 0x00000080;
        /// <summary> Indicates the display supports multi-vpu</summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_MULTIVPU_SUPPORTED = 0x00000020;
        /// <summary> Indicates the display is non-local to this machine </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_NONLOCAL = 0x00000004;
        /// <summary> Indicates the display is a projector </summary>
        internal const int ADL_DISPLAY_DISPLAYINFO_SHOWTYPE_PROJECTOR = 0x00100000;


        #endregion Internal Constant

        #region Internal Enums

        internal enum ADLConnectionType
        {
            Unknown = 1, 
            DVI = 1,
            DVI_SL = 2,
            HDMI = 4,
            DisplayPort = 4,
            ActiveDongleDPToDVI_SL = 5,
            ActiveDongleDPToDVI_DL = 6,
            ActiveDongleDPToHDMI = 7,
            ActiveDongleDPToVGA = 8,
            PassiveDongleDPToHDMI = 9,
            PassiveDongleDPToDVI = 10,
            MST = 11,
            ActiveDongle = 12,
            Virtual = 13
        }
        #endregion Internal Enums

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
            internal static extern int ADL2_Main_Control_Create(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters, out IntPtr contextHandle);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Main_Control_Destroy(IntPtr contextHandle);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Adapter_NumberOfAdapters_Get(IntPtr contextHandle, ref int numAdapters);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Adapter_Active_Get(IntPtr ADLContextHandle, int adapterIndex, ref int status);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_AdapterX2_Caps(IntPtr ADLContextHandle, int adapterIndex, out ADLAdapterCapsX2 adapterCapabilities);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Adapter_AdapterInfo_Get(IntPtr ADLContextHandle, int inputSize, out IntPtr AdapterInfoArray);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Adapter_AdapterInfoX2_Get(IntPtr ADLContextHandle, out IntPtr AdapterInfoArray);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Adapter_AdapterInfoX3_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr AdapterInfoArray);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Adapter_AdapterInfoX4_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr AdapterInfoX2Array);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Display_DDCInfo2_Get(IntPtr contextHandle, int adapterIndex, int displayIndex, out ADLDDCInfo2 DDCInfo);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Display_DisplayInfo_Get(IntPtr ADLContextHandle, int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL2_Display_DeviceConfig_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out ADLDisplayConfig displayConfig);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Main_Control_Create (ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Main_Control_Destroy ();

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Main_Control_IsFunctionValid (HMODULE module, string procName);

            [DllImport(Atiadlxx_FileName)]
            internal static extern FARPROC ADL_Main_Control_GetProcAddress (HMODULE module, string procName);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Adapter_NumberOfAdapters_Get (ref int numAdapters);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Adapter_AdapterInfo_Get (out IntPtr info, int inputSize);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Adapter_Active_Get(int adapterIndex, ref int status);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Adapter_ID_Get(int adapterIndex, ref int adapterId);
            
            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_AdapterX2_Caps(int adapterIndex, out ADLAdapterCapsX2 adapterCapabilities);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_DeviceConfig_Get(int adapterIndex, int displayIndex, out ADLDisplayConfig displayConfig);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_EdidData_Get(int adapterIndex, int displayIndex, ref ADLDisplayEDIDData EDIDData);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_DisplayMapConfig_Get(int adapterIndex, out int numDisplayMap, out IntPtr displayMap, out int numDisplayTarget, out IntPtr displayTarget, int options);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_DisplayMapConfig_PossibleAddAndRemove(int adapterIndex, int numDisplayMap, ADLDisplayMap displayMap, int numDisplayTarget, ADLDisplayTarget displayTarget, out int numPossibleAddTarget, out IntPtr possibleAddTarget, out int numPossibleRemoveTarget, out IntPtr possibleRemoveTarget);

            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_SLSMapConfig_Get(int adapterIndex, int SLSMapIndex, ref ADLSLSMap SLSMap, ref int NumSLSTarget, out IntPtr SLSTargetArray, ref int lpNumNativeMode, out IntPtr NativeMode, ref int NumBezelMode, out IntPtr BezelMode, ref int NumTransientMode, out IntPtr TransientMode, ref int NumSLSOffset, out IntPtr SLSOffset, int iOption);

            // This is used to set the SLS Grid we want from the SLSMap by selecting the one we want and supplying that as an index.
            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_SLSMapConfig_SetState(int AdapterIndex, int SLSMapIndex, int State);

            // Function to get the current supported SLS grid patterns (MxN) for a GPU.
            // This function gets a list of supported SLS grids for a specified input adapter based on display devices currently connected to the GPU.
            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_SLSGrid_Caps(int adapterIndex, ref int NumSLSGrid, out IntPtr SLSGrid, int option);

            // Function to get the active SLS map index list for a given GPU.
            // This function retrieves a list of active SLS map indexes for a specified input GPU.            
            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_SLSMapIndexList_Get(int adapterIndex, ref int numSLSMapIndexList, out IntPtr SLSMapIndexList, int options);

            // Function to get the active SLS map index list for a given GPU.
            // This function retrieves a list of active SLS map indexes for a specified input GPU.            
            [DllImport(Atiadlxx_FileName)]
            internal static extern int ADL_Display_SLSMapIndex_Get(int adapterIndex, int ADLNumDisplayTarget, ref ADLDisplayTarget displayTarget, ref int SLSMapIndex);

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
                    if (1 == ADLImport.ADL_Main_Control_IsFunctionValid(IntPtr.Zero, "ADL_Main_Control_Create"))
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
                    ADLImport.ADL_Main_Control_Destroy();
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
                    if (1 == ADLImport.ADL_Main_Control_IsFunctionValid(ADLCheckLibrary_.ADLLibrary, functionName))
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
                    result = ADLImport.ADL_Main_Control_GetProcAddress(ADLCheckLibrary_.ADLLibrary, functionName);
                }
                return result;
            }
            #endregion Static GetProcAddress
        }
        #endregion Class ADLCheckLibrary

        #region Export Functions

        #region ADL_Main_Memory_Alloc
        /// <summary> Build in memory allocation function</summary>
        internal static ADL_Main_Memory_Alloc ADL_Main_Memory_Alloc = ADL_Main_Memory_Alloc_;
        /// <summary> Build in memory allocation function</summary>
        /// <param name="size">input size</param>
        /// <returns>return the memory buffer</returns>
        private static IntPtr ADL_Main_Memory_Alloc_ (int size)
        {
            IntPtr result = Marshal.AllocCoTaskMem(size);
            return result;
        }
        #endregion ADL_Main_Memory_Alloc

        #region ADL_Main_Memory_Free
        /// <summary> Build in memory free function</summary>
        /// <param name="buffer">input buffer</param>
        internal static void ADL_Main_Memory_Free (IntPtr buffer)
        {
            if (IntPtr.Zero != buffer)
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }
        #endregion ADL_Main_Memory_Free

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

        #region ADL2_AdapterX2_Caps
        /// <summary> ADL2_AdapterX2_Caps Delegates</summary>
        internal static ADL2_AdapterX2_Caps ADL2_AdapterX2_Caps
        {
            get
            {
                if (!ADL2_AdapterX2_Caps_Check && null == ADL2_AdapterX2_Caps_)
                {
                    ADL2_AdapterX2_Caps_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_AdapterX2_Caps"))
                    {
                        ADL2_AdapterX2_Caps_ = ADLImport.ADL2_AdapterX2_Caps;
                    }
                }
                return ADL2_AdapterX2_Caps_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_AdapterX2_Caps ADL2_AdapterX2_Caps_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_AdapterX2_Caps_Check = false;
        #endregion ADL2_AdapterX2_Caps


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

        #region ADL2_Adapter_AdapterInfoX2_Get
        /// <summary> ADL2_Adapter_AdapterInfoX2_Get Delegates</summary>
        internal static ADL2_Adapter_AdapterInfoX2_Get ADL2_Adapter_AdapterInfoX2_Get
        {
            get
            {
                if (!ADL2_Adapter_AdapterInfoX2_Get_Check && null == ADL2_Adapter_AdapterInfoX2_Get_)
                {
                    ADL2_Adapter_AdapterInfoX2_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Adapter_AdapterInfoX2_Get"))
                    {
                        ADL2_Adapter_AdapterInfoX2_Get_ = ADLImport.ADL2_Adapter_AdapterInfoX2_Get;
                    }
                }
                return ADL2_Adapter_AdapterInfoX2_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Adapter_AdapterInfoX2_Get ADL2_Adapter_AdapterInfoX2_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Adapter_AdapterInfoX2_Get_Check = false;
        #endregion ADL2_Adapter_AdapterInfoX2_Get

        #region ADL2_Adapter_AdapterInfoX3_Get
        /// <summary> ADL2_Adapter_AdapterInfoX3_Get Delegates</summary>
        internal static ADL2_Adapter_AdapterInfoX3_Get ADL2_Adapter_AdapterInfoX3_Get
        {
            get
            {
                if (!ADL2_Adapter_AdapterInfoX3_Get_Check && null == ADL2_Adapter_AdapterInfoX3_Get_)
                {
                    ADL2_Adapter_AdapterInfoX3_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Adapter_AdapterInfoX3_Get"))
                    {
                        ADL2_Adapter_AdapterInfoX3_Get_ = ADLImport.ADL2_Adapter_AdapterInfoX3_Get;
                    }
                }
                return ADL2_Adapter_AdapterInfoX3_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Adapter_AdapterInfoX3_Get ADL2_Adapter_AdapterInfoX3_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Adapter_AdapterInfoX3_Get_Check = false;
        #endregion ADL2_Adapter_AdapterInfoX3_Get

        #region ADL2_Adapter_AdapterInfoX4_Get
        /// <summary> ADL2_Adapter_AdapterInfoX4_Get Delegates</summary>
        internal static ADL2_Adapter_AdapterInfoX4_Get ADL2_Adapter_AdapterInfoX4_Get
        {
            get
            {
                if (!ADL2_Adapter_AdapterInfoX4_Get_Check && null == ADL2_Adapter_AdapterInfoX4_Get_)
                {
                    ADL2_Adapter_AdapterInfoX4_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Adapter_AdapterInfoX4_Get"))
                    {
                        ADL2_Adapter_AdapterInfoX4_Get_ = ADLImport.ADL2_Adapter_AdapterInfoX4_Get;
                    }
                }
                return ADL2_Adapter_AdapterInfoX4_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Adapter_AdapterInfoX4_Get ADL2_Adapter_AdapterInfoX4_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Adapter_AdapterInfoX4_Get_Check = false;
        #endregion ADL2_Adapter_AdapterInfoX4_Get

        #region ADL2_Display_DDCInfo2_Get
        /// <summary> ADL2_Display_DDCInfo2_Get Delegates</summary>
        internal static ADL2_Display_DDCInfo2_Get ADL2_Display_DDCInfo2_Get
        {
            get
            {
                if (!ADL2_Display_DDCInfo2_Get_Check && null == ADL2_Display_DDCInfo2_Get_)
                {
                    ADL2_Display_DDCInfo2_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_DDCInfo2_Get"))
                    {
                        ADL2_Display_DDCInfo2_Get_ = ADLImport.ADL2_Display_DDCInfo2_Get;
                    }
                }
                return ADL2_Display_DDCInfo2_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_DDCInfo2_Get ADL2_Display_DDCInfo2_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_DDCInfo2_Get_Check = false;
        #endregion ADL2_Display_DDCInfo2_Get

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

        #region ADL2_Display_DeviceConfig_Get
        /// <summary> ADL2_Display_DeviceConfig_Get Delegates</summary>
        internal static ADL2_Display_DeviceConfig_Get ADL2_Display_DeviceConfig_Get
        {
            get
            {
                if (!ADL2_Display_DeviceConfig_Get_Check && null == ADL2_Display_DeviceConfig_Get_)
                {
                    ADL2_Display_DeviceConfig_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_DeviceConfig_Get"))
                    {
                        ADL2_Display_DeviceConfig_Get_ = ADLImport.ADL2_Display_DeviceConfig_Get;
                    }
                }
                return ADL2_Display_DeviceConfig_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_DeviceConfig_Get ADL2_Display_DeviceConfig_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_DeviceConfig_Get_Check = false;
        #endregion ADL2_Display_DeviceConfig_Get

        #region ADL_Main_Control_Create
        /// <summary> ADL_Main_Control_Create Delegates</summary>
        internal static ADL_Main_Control_Create ADL_Main_Control_Create
        {
            get
            {
                if (!ADL_Main_Control_Create_Check && null == ADL_Main_Control_Create_)
                {
                    ADL_Main_Control_Create_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Main_Control_Create"))
                    {
                        ADL_Main_Control_Create_ = ADLImport.ADL_Main_Control_Create;
                    }
                }
                return ADL_Main_Control_Create_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Main_Control_Create ADL_Main_Control_Create_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Main_Control_Create_Check = false;
        #endregion ADL_Main_Control_Create

        #region ADL_Main_Control_Destroy
        /// <summary> ADL_Main_Control_Destroy Delegates</summary>
        internal static ADL_Main_Control_Destroy ADL_Main_Control_Destroy
        {
            get
            {
                if (!ADL_Main_Control_Destroy_Check && null == ADL_Main_Control_Destroy_)
                {
                    ADL_Main_Control_Destroy_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Main_Control_Destroy"))
                    {
                        ADL_Main_Control_Destroy_ = ADLImport.ADL_Main_Control_Destroy;
                    }
                }
                return ADL_Main_Control_Destroy_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Main_Control_Destroy ADL_Main_Control_Destroy_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Main_Control_Destroy_Check = false;
        #endregion ADL_Main_Control_Destroy

        #region ADL_Adapter_NumberOfAdapters_Get
        /// <summary> ADL_Adapter_NumberOfAdapters_Get Delegates</summary>
        internal static ADL_Adapter_NumberOfAdapters_Get ADL_Adapter_NumberOfAdapters_Get
        {
            get
            {
                if (!ADL_Adapter_NumberOfAdapters_Get_Check && null == ADL_Adapter_NumberOfAdapters_Get_)
                {
                    ADL_Adapter_NumberOfAdapters_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Adapter_NumberOfAdapters_Get"))
                    {
                        ADL_Adapter_NumberOfAdapters_Get_ = ADLImport.ADL_Adapter_NumberOfAdapters_Get;
                    }
                }
                return ADL_Adapter_NumberOfAdapters_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Adapter_NumberOfAdapters_Get ADL_Adapter_NumberOfAdapters_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Adapter_NumberOfAdapters_Get_Check = false;
        #endregion ADL_Adapter_NumberOfAdapters_Get

        #region ADL_Adapter_ID_Get
        
        /// <summary> ADL_Adapter_Active_Get Delegates</summary>
        internal static ADL_Adapter_ID_Get ADL_Adapter_ID_Get
        {
            get
            {
                if (!ADL_Adapter_ID_Get_Check && null == ADL_Adapter_ID_Get_)
                {
                    ADL_Adapter_ID_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Adapter_ID_Get"))
                    {
                        ADL_Adapter_ID_Get_ = ADLImport.ADL_Adapter_ID_Get;
                    }
                }
                return ADL_Adapter_ID_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Adapter_ID_Get ADL_Adapter_ID_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Adapter_ID_Get_Check = false;
        #endregion ADL_Adapter_ID_Get

        #region ADL_AdapterX2_Caps
        /// <summary> ADL_AdapterX2_Caps Delegates</summary>
        internal static ADL_AdapterX2_Caps ADL_AdapterX2_Caps
        {
            get
            {
                if (!ADL_AdapterX2_Caps_Check && null == ADL_AdapterX2_Caps_)
                {
                    ADL_AdapterX2_Caps_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_AdapterX2_Caps"))
                    {
                        ADL_AdapterX2_Caps_ = ADLImport.ADL_AdapterX2_Caps;
                    }
                }
                return ADL_AdapterX2_Caps_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_AdapterX2_Caps ADL_AdapterX2_Caps_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_AdapterX2_Caps_Check = false;
        #endregion ADL_AdapterX2_Caps

        #region ADL_Adapter_AdapterInfo_Get
        /// <summary> ADL_Adapter_AdapterInfo_Get Delegates</summary>
        internal static ADL_Adapter_AdapterInfo_Get ADL_Adapter_AdapterInfo_Get
        {
            get
            {
                if (!ADL_Adapter_AdapterInfo_Get_Check && null == ADL_Adapter_AdapterInfo_Get_)
                {
                    ADL_Adapter_AdapterInfo_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Adapter_AdapterInfo_Get"))
                    {
                        ADL_Adapter_AdapterInfo_Get_ = ADLImport.ADL_Adapter_AdapterInfo_Get;
                    }
                }
                return ADL_Adapter_AdapterInfo_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Adapter_AdapterInfo_Get ADL_Adapter_AdapterInfo_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Adapter_AdapterInfo_Get_Check = false;
        #endregion ADL_Adapter_AdapterInfo_Get

        #region ADL_Adapter_Active_Get
        /// <summary> ADL_Adapter_Active_Get Delegates</summary>
        internal static ADL_Adapter_Active_Get ADL_Adapter_Active_Get
        {
            get
            {
                if (!ADL_Adapter_Active_Get_Check && null == ADL_Adapter_Active_Get_)
                {
                    ADL_Adapter_Active_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Adapter_Active_Get"))
                    {
                        ADL_Adapter_Active_Get_ = ADLImport.ADL_Adapter_Active_Get;
                    }
                }
                return ADL_Adapter_Active_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Adapter_Active_Get ADL_Adapter_Active_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Adapter_Active_Get_Check = false;
        #endregion ADL_Adapter_Active_Get

        #region ADL_Display_DeviceConfig_Get
        /// <summary> ADL_Display_DeviceConfig_Get Delegates</summary>
        internal static ADL_Display_DeviceConfig_Get ADL_Display_DeviceConfig_Get
        {
            get
            {
                if (!ADL_Display_DeviceConfig_Get_Check && null == ADL_Display_DeviceConfig_Get_)
                {
                    ADL_Display_DeviceConfig_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Display_DeviceConfig_Get"))
                    {
                        ADL_Display_DeviceConfig_Get_ = ADLImport.ADL_Display_DeviceConfig_Get;
                    }
                }
                return ADL_Display_DeviceConfig_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Display_DeviceConfig_Get ADL_Display_DeviceConfig_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Display_DeviceConfig_Get_Check = false;
        #endregion ADL_Display_DeviceConfig_Get

        #region ADL_Display_DisplayMapConfig_Get
        /// <summary> ADL_Display_DisplayMapConfig_Get Delegates</summary>
        internal static ADL_Display_DisplayMapConfig_Get ADL_Display_DisplayMapConfig_Get
        {
            get
            {
                if (!ADL_Display_DisplayMapConfig_Get_Check && null == ADL_Display_DisplayMapConfig_Get_)
                {
                    ADL_Display_DisplayMapConfig_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Display_DisplayMapConfig_Get"))
                    {
                        ADL_Display_DisplayMapConfig_Get_ = ADLImport.ADL_Display_DisplayMapConfig_Get;
                    }
                }
                return ADL_Display_DisplayMapConfig_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Display_DisplayMapConfig_Get ADL_Display_DisplayMapConfig_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Display_DisplayMapConfig_Get_Check = false;
        #endregion ADL_Display_DisplayMapConfig_Get

        #region ADL_Display_DisplayMapConfig_PossibleAddAndRemove
        /// <summary> ADL_Display_DisplayMapConfig_PossibleAddAndRemove Delegates</summary>
        internal static ADL_Display_DisplayMapConfig_PossibleAddAndRemove ADL_Display_DisplayMapConfig_PossibleAddAndRemove
        {
            get
            {
                if (!ADL_Display_DisplayMapConfig_PossibleAddAndRemove_Check && null == ADL_Display_DisplayMapConfig_PossibleAddAndRemove_)
                {
                    ADL_Display_DisplayMapConfig_PossibleAddAndRemove_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Display_DisplayMapConfig_PossibleAddAndRemove"))
                    {
                        ADL_Display_DisplayMapConfig_PossibleAddAndRemove_ = ADLImport.ADL_Display_DisplayMapConfig_PossibleAddAndRemove;
                    }
                }
                return ADL_Display_DisplayMapConfig_PossibleAddAndRemove_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Display_DisplayMapConfig_PossibleAddAndRemove ADL_Display_DisplayMapConfig_PossibleAddAndRemove_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Display_DisplayMapConfig_PossibleAddAndRemove_Check = false;
        #endregion ADL_Display_DisplayMapConfig_PossibleAddAndRemove


        #region ADL_Display_EdidData_Get
        /// <summary> ADL_Display_EdidData_Get Delegates</summary>
        internal static ADL_Display_EdidData_Get ADL_Display_EdidData_Get
        {
            get
            {
                if (!ADL_Display_EdidData_Get_Check && null == ADL_Display_EdidData_Get_)
                {
                    ADL_Display_EdidData_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Display_EdidData_Get"))
                    {
                        ADL_Display_EdidData_Get_ = ADLImport.ADL_Display_EdidData_Get;
                    }
                }
                return ADL_Display_EdidData_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Display_EdidData_Get ADL_Display_EdidData_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Display_EdidData_Get_Check = false;
        #endregion ADL_Display_EdidData_Get


        #region ADL_Display_DisplayInfo_Get
        /// <summary> ADL_Display_DisplayInfo_Get Delegates</summary>
        internal static ADL_Display_DisplayInfo_Get ADL_Display_DisplayInfo_Get
        {
            get
            {
                if (!ADL_Display_DisplayInfo_Get_Check && null == ADL_Display_DisplayInfo_Get_)
                {
                    ADL_Display_DisplayInfo_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Display_DisplayInfo_Get"))
                    {
                        ADL_Display_DisplayInfo_Get_ = ADLImport.ADL_Display_DisplayInfo_Get;
                    }
                }
                return ADL_Display_DisplayInfo_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Display_DisplayInfo_Get ADL_Display_DisplayInfo_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Display_DisplayInfo_Get_Check = false;
        #endregion ADL_Display_DisplayInfo_Get


        #region ADL_Display_SLSMapConfig_Get
        /// <summary> ADL_Display_SLSMapConfig_Get Delegates</summary>
        internal static ADL_Display_SLSMapConfig_Get ADL_Display_SLSMapConfig_Get
        {
            get
            {
                if (!ADL_Display_SLSMapConfig_Get_Check && null == ADL_Display_SLSMapConfig_Get_)
                {
                    ADL_Display_SLSMapConfig_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL_Display_SLSMapConfig_Get"))
                    {
                        ADL_Display_SLSMapConfig_Get_ = ADLImport.ADL_Display_SLSMapConfig_Get;
                    }
                }
                return ADL_Display_SLSMapConfig_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL_Display_SLSMapConfig_Get ADL_Display_SLSMapConfig_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL_Display_SLSMapConfig_Get_Check = false;
        #endregion ADL_Display_SLSMapConfig_Get


        #endregion Export Functions

        #region ADL Helper Functions

        internal static ConvertedDisplayInfoValue ConvertDisplayInfoValue(int displayInfoValue)
        {
            ConvertedDisplayInfoValue expandedDisplayInfoValue = new ConvertedDisplayInfoValue();

            // Indicates the display is connected
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED) == ADL.ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED)           
                expandedDisplayInfoValue.DISPLAYCONNECTED = true;
            // Indicates the display is mapped within OS
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_DISPLAYMAPPED) == ADL.ADL_DISPLAY_DISPLAYINFO_DISPLAYMAPPED)           
                expandedDisplayInfoValue.DISPLAYMAPPED = true;
            // Indicates the display can be forced
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_FORCIBLESUPPORTED) == ADL.ADL_DISPLAY_DISPLAYINFO_FORCIBLESUPPORTED)           
                expandedDisplayInfoValue.FORCIBLESUPPORTED = true;
            // Indicates the display supports genlock 
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_GENLOCKSUPPORTED) == ADL.ADL_DISPLAY_DISPLAYINFO_GENLOCKSUPPORTED)           
                expandedDisplayInfoValue.GENLOCKSUPPORTED = true;
            // Indicates the display is an LDA display.
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_LDA_DISPLAY) == ADL.ADL_DISPLAY_DISPLAYINFO_LDA_DISPLAY)           
                expandedDisplayInfoValue.LDA_DISPLAY = true;
            // Indicates the display supports 2x Horizontal stretch
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2HSTRETCH) == ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2HSTRETCH)           
                expandedDisplayInfoValue.MANNER_SUPPORTED_2HSTRETCH = true;
            // Indicates the display supports 2x Vertical stretch
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2VSTRETCH) == ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2VSTRETCH)           
                expandedDisplayInfoValue.MANNER_SUPPORTED_2VSTRETCH = true;
            // Indicates the display supports cloned desktops 
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_CLONE) == ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_CLONE)           
                expandedDisplayInfoValue.MANNER_SUPPORTED_CLONE = true;
            // Indicates the display supports extended desktops
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_EXTENDED) == ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_EXTENDED)           
                expandedDisplayInfoValue.MANNER_SUPPORTED_EXTENDED = true;
            // Indicates the display supports N Stretched on 1 GPU
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCH1GPU) == ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCH1GPU)           
                expandedDisplayInfoValue.MANNER_SUPPORTED_NSTRETCH1GPU = true;
            // Indicates the display supports N Stretched on N GPUs
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCHNGPU) == ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCHNGPU)           
                expandedDisplayInfoValue.MANNER_SUPPORTED_NSTRETCHNGPU = true;
            // Reserved display info flag #2
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED2) == ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED2)           
                expandedDisplayInfoValue.MANNER_SUPPORTED_RESERVED2 = true;
            // Reserved display info flag #3
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED3) == ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED3)           
                expandedDisplayInfoValue.MANNER_SUPPORTED_RESERVED3 = true;
            // Indicates the display supports single desktop
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_SINGLE) == ADL.ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_SINGLE)           
                expandedDisplayInfoValue.MANNER_SUPPORTED_SINGLE = true;
            // Indicates the display supports overriding the mode timing
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MODETIMING_OVERRIDESSUPPORTED) == ADL.ADL_DISPLAY_DISPLAYINFO_MODETIMING_OVERRIDESSUPPORTED)           
                expandedDisplayInfoValue.MODETIMING_OVERRIDESSUPPORTED = true;
            // Indicates the display supports multi-vpu
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_MULTIVPU_SUPPORTED) == ADL.ADL_DISPLAY_DISPLAYINFO_MULTIVPU_SUPPORTED)           
                expandedDisplayInfoValue.MULTIVPU_SUPPORTED = true;
            // Indicates the display is non-local to this machine 
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_NONLOCAL) == ADL.ADL_DISPLAY_DISPLAYINFO_NONLOCAL)           
                expandedDisplayInfoValue.NONLOCAL = true;
            // Indicates the display is a projector
            if ((displayInfoValue & ADL.ADL_DISPLAY_DISPLAYINFO_SHOWTYPE_PROJECTOR) == ADL.ADL_DISPLAY_DISPLAYINFO_SHOWTYPE_PROJECTOR)           
                expandedDisplayInfoValue.SHOWTYPE_PROJECTOR = true;

            return expandedDisplayInfoValue;

        }

        internal static string ConvertADLReturnValueIntoWords(int adlReturnValue)
        {
            if (adlReturnValue == ADL.ADL_OK)
                return "Success. Function worked as intended.";
            if (adlReturnValue == ADL.ADL_ERR)
                return "Generic Error.Most likely one or more of the Escape calls to the driver failed!";
            if (adlReturnValue == ADL.ADL_ERR_DISABLED_ADAPTER)
                return "Call can't be made due to disabled adapter.";
            if (adlReturnValue == ADL.ADL_ERR_INVALID_ADL_IDX)
                return "Invalid ADL index passed.";
            if (adlReturnValue == ADL.ADL_ERR_INVALID_CALLBACK)
                return "Invalid Callback passed.";
            if (adlReturnValue == ADL.ADL_ERR_INVALID_CONTROLLER_IDX)
                return "Invalid controller index passed.";
            if (adlReturnValue == ADL.ADL_ERR_INVALID_DISPLAY_IDX)
                return "Invalid display index passed.";
            if (adlReturnValue == ADL.ADL_ERR_INVALID_PARAM)
                return "One of the parameter passed is invalid.";
            if (adlReturnValue == ADL.ADL_ERR_INVALID_PARAM_SIZE)
                return "One of the parameter size is invalid.";
            if (adlReturnValue == ADL.ADL_ERR_NO_XDISPLAY)
                return "There's no Linux XDisplay in Linux Console environment.";
            if (adlReturnValue == ADL.ADL_ERR_NOT_INIT)
                return "ADL not initialized. You need to run ADL_Main_Control_Create.";
            if (adlReturnValue == ADL.ADL_ERR_NOT_SUPPORTED)
                return "Function not supported by the driver.";
            if (adlReturnValue == ADL.ADL_ERR_NULL_POINTER)
                return "Null Pointer error.";
            if (adlReturnValue == ADL.ADL_ERR_RESOURCE_CONFLICT)
                return "Display Resource conflict.";
            if (adlReturnValue == ADL.ADL_ERR_SET_INCOMPLETE)
                return "Err Set incomplete";
            if (adlReturnValue == ADL.ADL_OK_MODE_CHANGE)
                return "All OK but need mode change.";
            if (adlReturnValue == ADL.ADL_OK_RESTART)
                return "All OK, but need to restart.";
            if (adlReturnValue == ADL.ADL_OK_WAIT)
                return "All OK, but need to wait.";
            if (adlReturnValue == ADL.ADL_OK_WARNING)
                return "All OK, but with warning..";
            // If we get here, then we've got an ADL Return value that we don't understand!
            return "ADL Return value not recognised. Your driver is likely newer than this code can understand.";
    }


        #endregion ADL Helper Functions

    }
    #endregion ADL Class
}

#endregion ATI.ADL