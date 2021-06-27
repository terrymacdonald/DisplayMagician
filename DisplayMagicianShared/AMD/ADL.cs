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
    public delegate IntPtr ADL_Main_Memory_Alloc (int size);

    // ADL2 version of function delagates

    /// <summary> ADL2 Create Function to create ADL Data</summary>
    /// <param name="callback">Call back functin pointer which is ised to allocate memory </param>
    /// <param name="numConnectedAdapters">If it is 1, then ADL will only retuen the physical exist adapters </param>
    /// <param name="contextHandle">Handle to ADL client context.</param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int ADL2_Main_Control_Create(ADL_Main_Memory_Alloc callback, int numConnectedAdapters, out IntPtr contextHandle);

    /// <summary> ADL2 Destroy Function to free up ADL Data</summary>
    /// <param name="contextHandle">Handle to ADL client context.</param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int ADL2_Main_Control_Destroy(IntPtr contextHandle);

    /// <summary> ADL2 Function to get the number of adapters</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="numAdapters">return number of adapters</param>
    public delegate int ADL2_Adapter_NumberOfAdapters_Get(IntPtr ADLContextHandle, ref int numAdapters);

    /// <summary> ADL2 Function to save driver configuration so it survives a reboot</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex"> Adapter Index to save state.</param>
    public delegate int ADL2_Flush_Driver_Data(IntPtr ADLContextHandle, int adapterIndex);

    /// <summary> ADL2 Function to determine if the adapter is active or not.</summary>
    /// <remarks>The function is used to check if the adapter associated with iAdapterIndex is active</remarks>  
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="status"> Status of the adapter. True: Active; False: Disabled</param>
    /// <returns>Non zero is successful</returns> 
    public delegate int ADL2_Adapter_Active_Get(IntPtr ADLContextHandle, int adapterIndex, ref int status);

    /// <summary>ADL2 Function to retrieve adapter capability information.</summary>
    /// <remarks>This function implements a DI call to retrieve adapter capability information .</remarks>  
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="adapterCapabilities"> The pointer to the ADLAdapterCaps structure storing the retrieved adapter capability information.</param>
    /// <returns>return ADL Error Code</returns> 
    public delegate int ADL2_AdapterX2_Caps(IntPtr ADLContextHandle, int adapterIndex, out ADLAdapterCapsX2 adapterCapabilities);


    /// <summary>ADL2 Function to retrieve all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterInfoArray">return GPU adapter information</param>
    /// <param name="inputSize">the size of the GPU adapter struct</param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int ADL2_Adapter_AdapterInfo_Get(IntPtr ADLContextHandle, int inputSize, out IntPtr adapterInfoArray);

    /// <summary>ADL2 function retrieves all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterInfoArray">return GPU adapter information. Is a pointer to the pointer of AdapterInfo array. Initialize to NULL before calling this API. ADL will allocate the necessary memory, using the user provided callback function.</param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int ADL2_Adapter_AdapterInfoX2_Get(IntPtr ADLContextHandle, out IntPtr adapterInfoArray);

    /// <summary>ADL2 function retrieves all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">The ADL index handle of the desired adapter or -1 if all adapters are desired</param>
    /// <param name="numAdapters">Number of items in the AdapterInfo Array. Can pass NULL pointer if passign an adapter index (in which case only one AdapterInfo is returned)</param>
    /// <param name="adapterInfoArray">return GPU adapter information</param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int  ADL2_Adapter_AdapterInfoX3_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr adapterInfoArray);

    /// <summary>ADL2 function retrieves all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">The ADL index handle of the desired adapter or -1 if all adapters are desired</param>
    /// <param name="numAdapters">Number of items in the AdapterInfo Array. Can pass NULL pointer if passign an adapter index (in which case only one AdapterInfo is returned)</param>
    /// <param name="adapterInfoX2Array">return GPU adapter information in adapterInfoX2 array</param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate  int ADL2_Adapter_AdapterInfoX4_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr adapterInfoX2Array);

    /// <summary> ADL2 Create Function to create ADL Data</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">Display Index</param>
    /// <param name="displayDDCInfo2">The ADLDDCInfo2 structure storing all DDC retrieved from the driver.</param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int ADL2_Display_DDCInfo2_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out ADLDDCInfo2 displayDDCInfo2);

    /// <summary>ADL2 function to get display information based on adapter index</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="numDisplays">return the total number of supported displays</param>
    /// <param name="displayInfoArray">return ADLDisplayInfo Array for supported displays' information</param>
    /// <param name="forceDetect">force detect or not</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL2_Display_DisplayInfo_Get(IntPtr ADLContextHandle, int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

    /// <summary>This ADL2 function retrieves HDTV capability settings for a specified display.</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">Display Index</param>
    /// <param name="displayConfig">return ADLDisplayConfig with HDTV capability settings in it</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL2_Display_DeviceConfig_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out ADLDisplayConfig displayConfig);

    /// <summary>ADL2 function to query whether a display is HDR Supported and Enabled</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayID">DisplayID for the desired display</param>
    /// <param name="support">return a pointer to the int whose value is set to true if the display supports HDR</param>
    /// <param name="enable">return a pointer to the int whose value is set to true if HDR is enabled on this display</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL2_Display_HDRState_Get(IntPtr ADLContextHandle, int adapterIndex, ADLDisplayID displayID, out int support, out int enable);

    /// <summary>ADL2 function to retrieve the current display mode information</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">Display Index</param>
    /// <param name="numModes">return a pointer to the number of modes retrieved.</param>
    /// <param name="modes">return a pointer to the array of retrieved ADLMode display modes.</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL2_Display_Modes_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out int numModes, out IntPtr modes);

    /// <summary>ADL2 function to set display mode information</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">Display Index</param>
    /// <param name="numModes">	The number of modes to be set.</param>
    /// <param name="modes">The pointer to the display mode information to be set. Refer to the ADLMode structure for more information.</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL2_Display_Modes_Set(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, int numModes, ref ADLMode modes);

    /// <summary>ADL2 function to retrieve the current display mode information</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="numDisplayMap">The pointer to the number of retrieved display maps</param>
    /// <param name="displayMap">The pointer to the pointer to the display manner information. Refer to the ADLDisplayMap structure for more information.</param>
    /// <param name="numDisplayTarget">The pointer to the display target sets retrieved.</param>
    /// <param name="displayTarget">The pointer to the pointer to the display target buffer. Refer to the ADLDisplayTarget structure for more information.</param>
    /// <param name="options">The function option. ADL_DISPLAY_DISPLAYMAP_OPTION_GPUINFO.</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL2_Display_DisplayMapConfig_Get(IntPtr ADLContextHandle, int adapterIndex, out int numDisplayMap, out IntPtr displayMap, out int numDisplayTarget, out IntPtr displayTarget, int options);

    /// <summary>ADL2 function to set the current display mode information</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="numDisplayMap">The number of display maps to set</param>
    /// <param name="displayMap">The pointer to the display manner information. Refer to the ADLDisplayMap structure for more information.</param>
    /// <param name="numDisplayTarget">The number of display targets to set</param>
    /// <param name="displayTarget">The pointer to the display target object. Refer to the ADLDisplayTarget structure for more information.</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL2_Display_DisplayMapConfig_Set(IntPtr ADLContextHandle, int adapterIndex, int numDisplayMap, ref ADLDisplayMap displayMap, int numDisplayTarget, ref ADLDisplayTarget displayTarget);

    /// <summary>ADL2 function to set the current display mode information</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="numPossibleMap">The number of possible maps to be validated.</param>
    /// <param name="possibleMaps">The list of possible maps to be validated. Refer to the ADLPossibleMap structure for more information.</param>
    /// <param name="numPossibleMapResult">The pointer to the number of validated result list.</param>
    /// <param name="possibleMapResult">The pointer to the pointer to validation result list. Refer to the ADLPossibleMapResult structure for more information.</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL2_Display_DisplayMapConfig_Validate(IntPtr ADLContextHandle, int adapterIndex, int numPossibleMap, ref ADLPossibleMap possibleMaps, out int numPossibleMapResult, ref IntPtr possibleMapResult);

    /// <summary>ADL2 function to set the current display mode information</summary>
    /// <param name="ADLContextHandle">Handle to ADL client context.</param>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="numDisplayMap">The number of display maps to set</param>
    /// <param name="displayMap">The pointer to the display manner information. Refer to the ADLDisplayMap structure for more information.</param>
    /// <param name="numDisplayTarget">The number of display targets to set</param>
    /// <param name="displayTarget">The pointer to the display target object. Refer to the ADLDisplayTarget structure for more information.</param>
    /// <param name="numPossibleAddTarget">The number of display targets that can be added</param>
    /// <param name="possibleAddTarget">The list of display targets that can be added</param>
    /// <param name="numPossibleRemoveTarget">The number of display targets that can be removed</param>
    /// <param name="possibleRemoveTarget">The list of display targets that can be removed</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL2_Display_DisplayMapConfig_PossibleAddAndRemove(IntPtr ADLContextHandle, int adapterIndex, int numDisplayMap, ref ADLDisplayMap displayMap, int numDisplayTarget, ref ADLDisplayTarget displayTarget, out int numPossibleAddTarget, out IntPtr possibleAddTarget, out int numPossibleRemoveTarget, out IntPtr possibleRemoveTarget);

    // ADL version of function delegates

    /// <summary> ADL Create Function to create ADL Data</summary>
    /// <param name="callback">Call back functin pointer which is ised to allocate memeory </param>
    /// <param name="enumConnectedAdapters">If it is 1, then ADL will only retuen the physical exist adapters </param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int ADL_Main_Control_Create(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);   

    /// <summary> ADL Destroy Function to free up ADL Data</summary>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int ADL_Main_Control_Destroy ();  

    /// <summary> ADL Function to get the number of adapters</summary>
    /// <param name="numAdapters">return number of adapters</param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int ADL_Adapter_NumberOfAdapters_Get (ref int numAdapters);

    /// <summary> Function to determine if the adapter is active or not.</summary>
    /// <remarks>The function is used to check if the adapter associated with iAdapterIndex is active</remarks>  
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="status"> Status of the adapter. True: Active; False: Disabled</param>
    /// <returns>Non zero is successful</returns> 
    public delegate int ADL_Adapter_Active_Get(int adapterIndex, ref int status);

    /// <summary> Function to get the unique identifier of an adapter.</summary>
    /// <remarks>This function retrieves the unique identifier of a specified adapter. The adapter ID is a unique value and will be used to determine what other controllers share the same adapter. The desktop will use this to find which HDCs are associated with an adapter.</remarks>  
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="adapterId"> The pointer to the adapter identifier. Zero means: The adapter is not AMD.</param>
    /// <returns>return ADL Error Code</returns> 
    public delegate int ADL_Adapter_ID_Get(int adapterIndex, ref int adapterId);

    /// <summary>Function to retrieve adapter capability information.</summary>
    /// <remarks>This function implements a DI call to retrieve adapter capability information .</remarks>  
    /// <param name="adapterIndex"> Adapter Index.</param>
    /// <param name="adapterCapabilities"> The pointer to the ADLAdapterCaps structure storing the retrieved adapter capability information.</param>
    /// <returns>return ADL Error Code</returns> 
    public delegate int ADL_AdapterX2_Caps(int adapterIndex, out ADLAdapterCapsX2 adapterCapabilities);

    /// <summary>Retrieves all OS-known adapter information.</summary>
    /// <remarks>This function retrieves the adapter information of all OS-known adapters in the system. OS-known adapters can include adapters that are physically present in the system (logical adapters) as well as ones that are no longer present in the system but are still recognized by the OS.</remarks>
    /// <param name="adapterInfoArray">return GPU adapter information</param>
    /// <param name="inputSize">the size of the GPU adapter struct</param>
    /// <returns> retrun ADL Error Code</returns>
    public delegate int ADL_Adapter_AdapterInfo_Get(out IntPtr adapterInfoArray, int inputSize);


    /// <summary>Function to get the EDID data.</summary>
    /// <remarks>This function retrieves the EDID data for a specififed display.</remarks>  
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">The desired display index. It can be retrieved from the ADLDisplayInfo data structure.</param>
    /// <param name="EDIDData">return the ADLDisplayEDIDData structure storing the retrieved EDID data.</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL_Display_EdidData_Get(int adapterIndex, int displayIndex, ref ADLDisplayEDIDData EDIDData);

    /// <summary>Get display information based on adapter index</summary>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="numDisplays">return the total number of supported displays</param>
    /// <param name="displayInfoArray">return ADLDisplayInfo Array for supported displays' information</param>
    /// <param name="forceDetect">force detect or not</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

    /// <summary>This function retrieves HDTV capability settings for a specified display.</summary>
    /// <param name="adapterIndex">Adapter Index</param>
    /// <param name="displayIndex">Display Index</param>
    /// <param name="displayConfig">return ADLDisplayConfig with HDTV capability settings in it</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL_Display_DeviceConfig_Get(int adapterIndex, int displayIndex, out ADLDisplayConfig displayConfig);

    /// <summary>Function to retrieve current display map configurations.</summary>
    /// <remarks>This function retrieves the current display map configurations, including the controllers and adapters mapped to each display.</remarks>  
    /// <param name="adapterIndex">	The ADL index handle of the desired adapter. A value of -1 returns all display configurations for the system across multiple GPUs.</param>
    /// <param name="numDisplayMap">Number of returned Display Maps</param>
    /// <param name="displayMap">Array of ADLDisplayMap objects</param>
    /// <param name="numDisplayTarget">Number of Display Targets</param>
    /// <param name="displayTarget">Array of ADLDisplayTarget objects</param>
    /// <param name="options">Options supplied</param>
    /// <returns>return ADL Error Code</returns>
    public delegate int ADL_Display_DisplayMapConfig_Get(int adapterIndex, out int numDisplayMap, out IntPtr displayMap, out int numDisplayTarget, out IntPtr displayTarget, int options);

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
    public delegate int ADL_Display_DisplayMapConfig_PossibleAddAndRemove(int adapterIndex, int numDisplayMap, ADLDisplayMap displayMap, int numDisplayTarget, ADLDisplayTarget displayTarget, out int numPossibleAddTarget, out IntPtr possibleAddTarget, out int numPossibleRemoveTarget, out IntPtr possibleRemoveTarget);

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
    public delegate int ADL_Display_SLSMapConfig_Get(int adapterIndex, int SLSMapIndex, ref ADLSLSMap SLSMap, ref int numSLSTarget, out IntPtr SLSTargetArray, ref int numNativeMode, 
        out IntPtr SLSNativeMode, ref int numBezelMode, out IntPtr SLSBezelMode, ref int numTransientMode, out IntPtr SLSTransientMode, ref int numSLSOffset, out IntPtr SLSOffset, int option);

    #endregion Export Delegates

    #region Export Struct

    #region ADLMode
    /// <summary> ADLAdapterInfo Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLMode
    {
        /// <summary> Adapter index. </summary>
        public int AdapterIndex;
        /// <summary> Display IDs. </summary>
        public ADLDisplayID DisplayID;
        /// <summary> Screen position X coordinate. </summary>
        public int XPos;
        /// <summary> Screen position Y coordinate. </summary>
        public int YPos;
        /// <summary> Screen resolution Width.  </summary>
        public int XRes;
        /// <summary> Screen resolution Height. </summary>
        public int YRes;
        /// <summary> Screen Color Depth. E.g., 16, 32.  </summary>
        public int ColourDepth;
        /// <summary> Screen refresh rate. </summary>
        public float RefreshRate;
        /// <summary> Screen orientation. E.g., 0, 90, 180, 270. </summary>
        public int Orientation;
        /// <summary> Vista mode flag indicating Progressive or Interlaced mode.  </summary>
        public int ModeFlag;
        /// <summary> The bit mask identifying the number of bits this Mode is currently using. </summary>
        public int ModeMask;
        /// <summary> The bit mask identifying the display status. </summary>
        public int ModeValue;       
    }

    public struct ConvertedDisplayModeFlags
    {
        /// <summary> Indicates the display supports Colour Format 565.</summary>
        public bool COLOURFORMAT_565;
        /// <summary> Indicates the display supports Colour Format 8888.</summary>
        public bool COLOURFORMAT_8888;
        /// <summary> Indicates the display supports normal vertical orientation</summary>
        public bool ORIENTATION_SUPPORTED_000;
        /// <summary> Indicates the display supports 90 degree orientation</summary>
        public bool ORIENTATION_SUPPORTED_090;
        /// <summary> Indicates the display supports 180 degree orientation</summary>
        public bool ORIENTATION_SUPPORTED_180;
        /// <summary> Indicates the display supports 270 degree orientation</summary>
        public bool ORIENTATION_SUPPORTED_270;
        /// <summary> Indicates the display supports rounded refresh rates</summary>
        public bool REFRESHRATE_ROUNDED;
        /// <summary> Indicates the display supports exact refresh rates</summary>
        public bool REFRESHRATE_ONLY;        
    }
    #endregion ADLMode

    #region ADLDisplayTarget
    /// <summary> ADLDisplayTarget </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLDisplayTarget
    {
        /// <summary> Display IDs. </summary>
        public ADLDisplayID DisplayID;
        /// <summary> The display map index identify this manner and the desktop surface. </summary>
        public int DisplayMapIndex;
        /// <summary> The bit mask identifies the number of bits DisplayTarget is currently using. </summary>
        public int DisplayTargetMask;
        /// <summary> The bit mask identifies the display status. </summary>
        public int DisplayTargetValue;
    }

    /// <summary> ADLDisplayTargetArray Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLDisplayTargetArray
    {
        /// <summary> ADLDisplayTarget Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_DISPLAYS)]
        public ADLDisplayTarget[] ADLDisplayTarget;
    }
    #endregion ADLDisplayTarget

    #region ADLAdapterInfo
    /// <summary> ADLAdapterInfo Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLAdapterInfo
    {
        /// <summary>The size of the structure</summary>
        int Size;
        /// <summary> Adapter Index</summary>
        public int AdapterIndex;
        /// <summary> Adapter UDID</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string UDID;
        /// <summary> Adapter Bus Number</summary>
        public int BusNumber;
        /// <summary> Adapter Driver Number</summary>
        public int DriverNumber;
        /// <summary> Adapter Function Number</summary>
        public int FunctionNumber;
        /// <summary> Adapter Vendor ID</summary>
        public int VendorID;
        /// <summary> Adapter Adapter name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string AdapterName;
        /// <summary> Adapter Display name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string DisplayName;
        /// <summary> Adapter Present status</summary>
        public int Present;
        /// <summary> Adapter Exist status</summary>
        public int Exist;
        /// <summary> Adapter Driver Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string DriverPath;
        /// <summary> Adapter Driver Ext Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string DriverPathExt;
        /// <summary> Adapter PNP String</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string PNPString;
        /// <summary> OS Display Index</summary>
        public int OSDisplayIndex;
    }


    /// <summary> ADLAdapterInfo Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLAdapterInfoArray
    {
        /// <summary> ADLAdapterInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_ADAPTERS)]
        public ADLAdapterInfo[] ADLAdapterInfo;
    }

    /// <summary> ADLAdapterInfoX2 Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLAdapterInfoX2
    {
        /// <summary>The size of the structure</summary>
        public int Size;
        /// <summary> Adapter Index</summary>
        public int AdapterIndex;
        /// <summary> Adapter UDID</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string UDID;
        /// <summary> Adapter Bus Number</summary>
        public int BusNumber;
        /// <summary> Adapter Device Number</summary>
        public int DeviceNumber;
        /// <summary> Adapter Function Number</summary>
        public int FunctionNumber;
        /// <summary> Adapter Vendor ID</summary>
        public int VendorID;
        /// <summary> Adapter Adapter name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_DISPLAY_NAME)]
        public string AdapterName;
        /// <summary> Adapter Display name</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_DISPLAY_NAME)]
        public string DisplayName;
        /// <summary> Adapter Present status</summary>
        public int Present;
        /// <summary> Adapter Exist status</summary>
        public int Exist;
        /// <summary> Adapter Driver Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string DriverPath;
        /// <summary> Adapter Driver Ext Path</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string DriverPathExt;
        /// <summary> Adapter PNP String</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string PNPString;
        /// <summary> OS Display Index</summary>
        public int OSDisplayIndex;
        /// <summary> Display Info Mask</summary>
        public int InfoMask;
        /// <summary> Display Info Value</summary>
        public int InfoValue;
    }

    #endregion ADLAdapterInfo


    #region ADLDisplayInfo

    /// <summary> ADLDisplayEDIDData Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLDisplayEDIDData
    {
        /// <summary> Size</summary>
        public int Size;
        /// <summary> Flag</summary>
        public int Flag;
        /// <summary> EDIDSize </summary>
        public int EDIDSize;
        /// <summary> Block Index </summary>
        public int BlockIndex;
        /// <summary> EDIDData [256] </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_EDIDDATA_SIZE)]
        public string EDIDData;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] Reserved;

    }


    /// <summary> ADLDDCInfo2 Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLDDCInfo2
    {
        /// <summary> Size of the structure. </summary>
        public int Size;
        /// <summary> Whether this display device support DDC</summary>
        public int SupportsDDC;
        /// <summary> Returns the manufacturer ID of the display device. Should be zeroed if this information is not available.</summary>
        public int ManufacturerID;
        /// <summary> Returns the product ID of the display device. Should be zeroed if this informatiadlon is not available.</summary>
        public int ProductID;
        /// <summary> Returns the name of the display device. Should be zeroed if this information is not available.</summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_DISPLAY_NAME)]
        public string DisplayName;
        /// <summary> Returns the maximum Horizontal supported resolution. Should be zeroed if this information is not available.</summary>
        public int MaxHResolution;
        /// <summary> Returns the maximum Vertical supported resolution. Should be zeroed if this information is not available. </summary>
        public int MaxVResolution;
        /// <summary> Returns the maximum supported refresh rate. Should be zeroed if this information is not available. </summary>
        public int MaxRefresh;
        /// <summary> Returns the display device preferred timing mode's horizontal resolution.</summary>
        public int PTMCx;
        /// <summary> Returns the display device preferred timing mode's vertical resolution. </summary>
        public int PTMCy;
        /// <summary> Returns the display device preferred timing mode's refresh rate.</summary>
        public int PTMRefreshRate;
        /// <summary> Return EDID flags.</summary>
        public int DDCInfoFlag;
        /// <summary> Returns 1 if the display supported packed pixel, 0 otherwise. </summary>
        public int PackedPixelSupported;
        /// <summary> Returns the Pixel formats the display supports DDCInfo Pixel Formats.</summary>
        public int PanelPixelFormat;
        /// <summary> Return EDID serial ID.</summary>
        public int SerialID;
        /// <summary> Return minimum monitor luminance data.</summary>
        public int MinLuminanceData;
        /// <summary> Return average monitor luminance data. </summary>
        public int AvgLuminanceData;
        /// <summary> Return maximum monitor luminance data.</summary>
        public int MaxLuminanceData;
        /// <summary> Bit vector of supported transfer functions ADLSourceContentAttributes transfer functions (gamma). </summary>
        public int SupportedTransferFunction;
        /// <summary> Bit vector of supported color spaces ADLSourceContentAttributes color spaces.</summary>
        public int SupportedColorSpace;
        /// <summary> Display Red Chromaticity X coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityRedX;
        /// <summary> Display Red Chromaticity Y coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityRedY;
        /// <summary> Display Green Chromaticity X coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityGreenX;
        /// <summary> Display Green  Chromaticity Y coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityGreenY;
        /// <summary> Display Blue Chromaticity X coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityBlueX;
        /// <summary> Display Blue Chromaticity Y coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityBlueY;
        /// <summary> Display White Chromaticity X coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityWhiteX;
        /// <summary> Display White Chromaticity Y coordinate multiplied by 10000.</summary>
        public int NativeDisplayChromaticityWhiteY;
        /// <summary> Display diffuse screen reflectance 0-1 (100%) in units of 0.01.</summary>
        public int DiffuseScreenReflectance;
        /// <summary> Display specular screen reflectance 0-1 (100%) in units of 0.01.</summary>
        public int SpecularScreenReflectance;
        /// <summary> Bit vector of supported color spaces ADLDDCInfo2 HDR support options.</summary>
        public int SupportedHDR;
        /// <summary> Bit vector for freesync flags.</summary>
        public int FreesyncFlags;
        /// <summary> Return minimum monitor luminance without dimming data.</summary>
        public int MinLuminanceNoDimmingData;
        /// <summary> Returns the maximum backlight maximum luminance. Should be zeroed if this information is not available.</summary>
        public int MaxBacklightMaxLuminanceData;
        /// <summAry> Returns the minimum backlight maximum luminance. Should be zeroed if this information is not available.</summary>
        public int MinBacklightMaxLuminanceData;
        /// <summary> Returns the maximum backlight minimum luminance. Should be zeroed if this information is not available.</summary>
        public int MaxBacklightMinLuminanceData;
        /// <summary> Returns the minimum backlight maximum luminance. Should be zeroed if this information is not available.</summary>
        public int MinBacklightMinLuminanceData;
        /// <summary> Reserved </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public int[] Reserved;
    }

    public struct ConvertedDDCInfoFlag
    {
        /// <summary> Indicates the display is a projector .</summary>
        public bool PROJECTORDEVICE;
        /// <summary> Indicates the display has an EDID extension</summary>
        public bool EDIDEXTENSION;
        /// <summary> Indicates the display is a digital device</summary>
        public bool DIGITALDEVICE;
        /// <summary> Indicates the display has HDMI audio capabilities</summary>
        public bool HDMIAUDIODEVICE;
        /// <summary> Indicates the display supports AI</summary>
        public bool SUPPORTS_AI;
        /// <summary> Indicates the display supports xvYCC601</summary>
        public bool SUPPORT_xvYCC601;
        /// <summary> Indicates the display supports xvYCC709</summary>
        public bool SUPPORT_xvYCC709;
    }

    public struct ConvertedSupportedHDR
    {
        /// <summary> HDR10/CEA861.3 HDR supported</summary>
        public bool CEA861_3;
        /// <summary> DolbyVision HDR supported</summary>
        public bool DOLBYVISION;
        /// <summary> FreeSync HDR supported.</summary>
        public bool FREESYNC_HDR;
    }


    /// <summary> ADLDisplayID Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLDisplayID
    {
        /// <summary> Display Logical Index </summary>
        public int DisplayLogicalIndex;
        /// <summary> Display Physical Index </summary>
        public int DisplayPhysicalIndex;
        /// <summary> Adapter Logical Index </summary>
        public int DisplayLogicalAdapterIndex;
        /// <summary> Adapter Physical Index </summary>
        public int DisplayPhysicalAdapterIndex;
    }

    /// <summary> ADLDisplayInfo Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLDisplayInfo
    {
        /// <summary> Display Index </summary>
        public ADLDisplayID DisplayID;
        /// <summary> Display Controller Index </summary>
        public int DisplayControllerIndex;
        /// <summary> Display Name </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string DisplayName;
        /// <summary> Display Manufacturer Name </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)ADL.ADL_MAX_PATH)]
        public string DisplayManufacturerName;
        /// <summary> Display Type : The Display type. CRT, TV,CV,DFP are some of display types,</summary>
        public int DisplayType;
        /// <summary> Display output type </summary>
        public int DisplayOutputType;
        /// <summary> Connector type</summary>
        public int DisplayConnector;
        ///<summary> Indicating the display info bits' mask.<summary>
        public int DisplayInfoMask;
        ///<summary> Indicating the display info value.<summary>
        public int DisplayInfoValue;
    }

    public struct ConvertedDisplayInfoValue
    {
        /// <summary> Indicates the display is connected .</summary>
        public bool DISPLAYCONNECTED;
        /// <summary> Indicates the display is mapped within OS </summary>
        public bool DISPLAYMAPPED;
        /// <summary> Indicates the display can be forced </summary>
        public bool FORCIBLESUPPORTED;
        /// <summary> Indicates the display supports genlock </summary>
        public bool GENLOCKSUPPORTED;
        /// <summary> Indicates the display is an LDA display.</summary>
        public bool LDA_DISPLAY;
        /// <summary> Indicates the display supports 2x Horizontal stretch </summary>
        public bool MANNER_SUPPORTED_2HSTRETCH;
        /// <summIndicates the display supports 2x Vertical stretch </summary>
        public bool MANNER_SUPPORTED_2VSTRETCH;
        /// <summary> Indicates the display supports cloned desktops </summary>
        public bool MANNER_SUPPORTED_CLONE;
        /// <summary> Indicates the display supports extended desktops </summary>
        public bool MANNER_SUPPORTED_EXTENDED;
        /// <summary> Indicates the display supports N Stretched on 1 GPU</summary>
        public bool MANNER_SUPPORTED_NSTRETCH1GPU;
        /// <summary> Indicates the display supports N Stretched on N GPUs</summary>
        public bool MANNER_SUPPORTED_NSTRETCHNGPU;
        /// <summary> Reserved display info flag #2</summary>
        public bool MANNER_SUPPORTED_RESERVED2;
        /// <summary> Reserved display info flag #3</summary>
        public bool MANNER_SUPPORTED_RESERVED3;
        /// <summary> Indicates the display supports single desktop </summary>
        public bool MANNER_SUPPORTED_SINGLE;
        /// <summary> Indicates the display supports overriding the mode timing </summary>
        public bool MODETIMING_OVERRIDESSUPPORTED;
        /// <summary> Indicates the display supports multi-vpu</summary>
        public bool MULTIVPU_SUPPORTED;
        /// <summary> Indicates the display is non-local to this machine </summary>
        public bool NONLOCAL;
        /// <summary> Indicates the display is a projector </summary>
        public bool SHOWTYPE_PROJECTOR;
    }

    /// <summary> ADLDisplayConfig Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLDisplayConfig
    {
        /// <summary> Size of this data structure </summary>
        public long Size;
        /// <summary> HDTV Connector Type </summary>
        public long ConnectorType;
        /// <summary> HDTV Capabilities themselves </summary>
        public long DeviceData;
        /// <summary> Overridden HDTV capabilities</summary>
        public long OverriddedDeviceData;
        /// <summary> Reserved for future use</summary>
        public long Reserved;
        
    }

    /// <summary> ADLDisplayMap Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLDisplayMap
    {
        /// <summary> The current display map index. It is the OS desktop index. For example, if the OS index 1 is showing clone mode, the display map will be 1. </summary>
        public int DisplayMapIndex;
        /// <summary> The Display Mode for the current map.</summary>
        public ADLMode DisplayMode;
        /// <summary> The number of display targets belongs to this map </summary>
        public int NumDisplayTarget;
        /// <summary> The first target array index in the Target array </summary>
        public int FirstDisplayTargetArrayIndex;
        /// <summary> The bit mask identifies the number of bits DisplayMap is currently using. It is the sum of all the bit definitions defined in ADL_DISPLAY_DISPLAYMAP_MANNER_xxx.</summary>
        public int DisplayMapMask;
        /// <summary> The bit mask identifies the display status. The detailed definition is in ADL_DISPLAY_DISPLAYMAP_MANNER_xxx.</summary>
        public int DisplayMapValue;               
    }

    /// <summary> ADLDisplayMapArray Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLDisplayMapArray
    {
        /// <summary> ADLAdapterInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_DISPLAYS)]
        public ADLDisplayMap[] ADLDisplayMap;
    }

    /// <summary> ADLAdapterCaps Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLAdapterCapsX2
    {
        /// <summary> AdapterID for this adapter </summary>
        public int AdapterID;
        /// <summary> Number of controllers for this adapter. </summary>
        public int NumControllers;
        /// <summary> Number of displays for this adapter.</summary>
        public int NumDisplays;
        /// <summary> Number of overlays for this adapter.</summary>
        public int NumOverlays;
        /// <summary> Number of GLSyncConnectors. </summary>
        public int NumOfGLSyncConnectors;
        /// <summary> The bit mask identifies the adapter caps. </summary>
        public int CapsMask;
        /// <summary> The bit identifies the adapter caps define_adapter_caps. </summary>
        public int CapsValue;
        /// <summary> Number of Connectors for this adapter. </summary>
        public int NumConnectors;
    }

    /// <summary> ADLPossibleMap Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLPossibleMap
    { 
        /// <summary> Index</summary>
        public int Index;
        /// <summary> Adapter Index. </summary>
        public int AdapterIndex;
        /// <summary> Display Map Number</summary>
        public int NumDisplayMap;
        /// <summary> The DisplayMaps being tested</summary>
        public ADLDisplayMap DisplayMaps;
        /// <summary> Number of Display Targets</summary>
        public int NumDisplayTarget;
        /// <summary> The DisplayTargets being tested </summary>
        public ADLDisplayTarget DisplayTargets;
    }

    /// <summary> ADLPossibleMapping Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLPossibleMapping
    {
        /// <summary>Display Index</summary>
        public int DisplayIndex;
        /// <summary> Display Controller Index</summary>
        public int DisplayControllerIndex;
        /// <summary> The display manner options supported</summary>
        public int DisplayMannerSupported;
    }

    /// <summary> ADLPossibleMapResult Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLPossibleMapResult
    {
        /// <summary>Index</summary>
        public int Index;
        // The bit mask and value identifies the number of bits PossibleMapResult is currently using. It will be the sum all the bit definitions defined in ADL_DISPLAY_POSSIBLEMAPRESULT_VALID.
        /// <summary> Display Controller Index</summary>
        public int PossibleMapResultMask;
        /// <summary> The display manner options supported</summary>
        public int PossibleMapResulValue;
    }

    #endregion ADLDisplayInfo

    #region ADLSLS

    /// <summary> ADLSLSGrid Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLSLSGrid
    {
        /// <summary> The Adapter index </summary>
        public int AdapterIndex;
        /// <summary> The grid index </summary>
        public int SLSGridIndex;
        /// <summary>The grid row. </summary>
        public int SLSGridRow;
        /// <summary> The grid column </summary>
        public int SLSGridColumn;
        /// <summary> The grid bit mask identifies the number of bits DisplayMap is currently using. </summary>
        public int SLSGridMask;
        /// <summary> The grid bit value identifies the display status. </summary>
        public int SLSGridValue;
    }

    /// <summary> ADLSLSMap Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLSLSMap
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> The current display map index. It is the OS Desktop index. </summary>
        public int SLSMapIndex;
        /// <summary> The current grid </summary>
        public ADLSLSGrid Grid;
        /// <summary> OS Surface Index </summary>
        public int SurfaceMapIndex;
        /// <summary> Screen orientation. E.g., 0, 90, 180, 270. </summary>
        public int Orientation;
        /// <summary> The number of display targets belongs to this map. </summary>
        public int NumSLSTarget;
        /// <summary> The first target array index in the Target array. </summary>
        public int FirstSLSTargetArrayIndex;
        /// <summary> The number of native modes belongs to this map. </summary>
        public int NumNativeMode;
        /// <summary>The first native mode array index in the native mode array. </summary>
        public int FirstNativeModeArrayIndex;
        /// <summary> The number of bezel modes belongs to this map. </summary>
        public int NumBezelMode;
        /// <summary> The first bezel mode array index in the native mode array </summary>
        public int FirstBezelModeArrayIndex;
        /// <summary> The number of bezel offsets belongs to this map. </summary>
        public int NumBezelOffset;
        /// <summary> The first bezel offset array index in the native mode array </summary>
        public int FirstBezelOffsetArrayIndex;
        /// <summary> Bitmask identifies display map status </summary>
        public int SLSMapMask;
        /// <summary> Bitmask identifies display map status </summary>
        public int SLSMapValue;
        
    }

    /// <summary> ADLSLSTarget Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLSLSTarget
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> The SLS map index. </summary>
        public int SLSMapIndex;
        /// <summary> The target ID.  </summary>
        public ADLDisplayTarget DisplayTarget;
        /// <summary> Target postion X in SLS grid </summary>
        public int SLSGridPositionX;
        /// <summary> Target postion Y in SLS grid </summary>
        public int SLSGridPositionY;
        /// <summary> The view size width, height and rotation angle per SLS Target.  </summary>
        public ADLMode ViewSize;
        /// <summary> The bit mask identifies the bits in iSLSTargetValue are currently used.  </summary>
        public int SLSTargetMask;
        /// <summary> The bit mask identifies status info.  </summary>
        public int SLSTargetValue;        
    }

    /// <summary> ADLSLSTarget Array</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLSLSTargetArray
    {
        /// <summary> ADLAdapterInfo Array </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)ADL.ADL_MAX_ADAPTERS)]
        public ADLSLSTarget[] ADLSLSTarget;
    }

    /// <summary> ADLSLSMode Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLSLSMode
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> The current display map index. It is the OS Desktop index. </summary>
        public int SLSMapIndex;
        /// <summary> The mode index. </summary>
        public int SLSModeIndex;
        /// <summary> The target ID.  </summary>
        public ADLMode DisplayMode;
        /// <summary> The bit mask identifies the number of bits Mode is currently using. </summary>
        public int SLSNativeModeMask;
        /// <summary> The bit mask identifies the display status. </summary>
        public int SLSNativeModeValue;

    }

    /// <summary> ADLBezelTransientMode Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLBezelTransientMode
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> SLS Map Index. </summary>
        public int SLSMapIndex;
        /// <summary> SLS Mode Index. </summary>
        public int SLSModeIndex;
        /// <summary> The target ID.  </summary>
        public ADLMode DisplayMode;
        /// <summary> The number of bezel offsets belongs to this map.  </summary>
        public int NumBezelOffset;
        /// <summary> The first bezel offset array index in the native mode array. </summary>
        public int FirstBezelOffsetArrayIndex;
        /// <summary> The bit mask identifies the bits this structure is currently using. </summary>
        public int SLSBezelTransientModeMask;
        /// <summary> The bit mask identifies the display status.  </summary>
        public int SLSBezelTransientModeValue;
        
    }

    /// <summary> ADLSLSOffset Structure</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ADLSLSOffset
    {
        /// <summary> The Adapter Index </summary>
        public int AdapterIndex;
        /// <summary> The current display map index. It is the OS Desktop index. </summary>
        public int SLSMapIndex;
        /// <summary> The target ID.  </summary>
        public ADLDisplayID DisplayID;
        /// <summary> SLS Bezel Mode Index. </summary>
        public int BezelModeIndex;
        /// <summary>SLS Bezel Offset X. </summary>
        public int BezelOffsetX;
        /// <summary>SLS Bezel Offset Y. </summary>
        public int BezelOffsetY;
        /// <summary> SLS Display Width. </summary>
        public int DisplayWidth;
        /// <summary> SLS Display Height. </summary>
        public int DisplayHeight;
        /// <summary> The bit mask identifies the number of bits Offset is currently using. </summary>
        public int BezelOffsetMask;
        /// <summary> The bit mask identifies the display status. </summary>
        public int BezelOffsetValue;
    }

    #endregion ADLSLS

    #endregion Export Struct

    #region ADL Class
    /// <summary> ADL Class</summary>
    public static class ADL
    {
        #region Internal Constant
        /// <summary> Selects all adapters instead of aparticular single adapter</summary>
        public const int ADL_ADAPTER_INDEX_ALL = -1;
        /// <summary> Define the maximum char</summary>
        public const int ADL_MAX_CHAR = 4096;
        /// <summary> Define the maximum path</summary>
        public const int ADL_MAX_PATH = 256;
        /// <summary> Define the maximum adapters</summary>
        public const int ADL_MAX_ADAPTERS = 250;
        /// <summary> Define the maximum displays</summary>
        public const int ADL_MAX_DISPLAYS = 150;
        /// <summary> Define the maximum device name length</summary>
        public const int ADL_MAX_DEVICENAME = 32;
        /// <summary> Define the maximum EDID Data length</summary>
        public const int ADL_MAX_EDIDDATA_SIZE = 256; // number of UCHAR
        /// <summary> Define the maximum display names</summary>
        public const int ADL_MAX_DISPLAY_NAME = 256;

        // Result Codes
        /// <summary> ADL function completed successfully. </summary>                
        public const int ADL_OK = 0;
        /// <summary> Generic Error.Most likely one or more of the Escape calls to the driver failed!</summary>
        public const int ADL_ERR = -1;
        /// <summary> Call can't be made due to disabled adapter. </summary>
        public const int ADL_ERR_DISABLED_ADAPTER = -10;
        /// <summary> Invalid ADL index passed. </summary>
        public const int ADL_ERR_INVALID_ADL_IDX = -5;
        /// <summary> Invalid Callback. </summary>
        public const int ADL_ERR_INVALID_CALLBACK = -11;
        /// <summary> Invalid controller index passed.</summary>
        public const int ADL_ERR_INVALID_CONTROLLER_IDX = -6;
        /// <summary> Invalid display index passed.</summary>
        public const int ADL_ERR_INVALID_DISPLAY_IDX = -7;
        /// <summary> One of the parameter passed is invalid.</summary>
        public const int ADL_ERR_INVALID_PARAM = -3;
        /// <summary> One of the parameter size is invalid.</summary>
        public const int ADL_ERR_INVALID_PARAM_SIZE = -4;
        /// <summary> There's no Linux XDisplay in Linux Console environment.</summary>
        public const int ADL_ERR_NO_XDISPLAY = -21;
        /// <summary> ADL not initialized.</summary>
        public const int ADL_ERR_NOT_INIT = -2;
        /// <summary> Function not supported by the driver. </summary>
        public const int ADL_ERR_NOT_SUPPORTED = -8;
        /// <summary> Null Pointer error.</summary>
        public const int ADL_ERR_NULL_POINTER = -9;
        /// <summary> Display Resource conflict.</summary>
        public const int ADL_ERR_RESOURCE_CONFLICT = -12;
        /// <summary> Err Set incomplete</summary>
        public const int ADL_ERR_SET_INCOMPLETE = -20;
        /// <summary> All OK but need mode change. </summary>
        public const int ADL_OK_MODE_CHANGE = 2;
        /// <summary> All OK, but need restart.</summary>
        public const int ADL_OK_RESTART = 3;
        /// <summary> All OK, but need to wait</summary>
        public const int ADL_OK_WAIT = 4;
        /// <summary> All OK, but with warning.</summary>
        public const int ADL_OK_WARNING = 1;


        /// <summary> Define the driver ok</summary>
        public const int ADL_DRIVER_OK = 0;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        public const int ADL_MAX_GLSYNC_PORTS = 8;
        /// <summary> Maximum number of GL-Sync ports on the GL-Sync module </summary>
        public const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
        /// <summary> Maximum number of ADLModes for the adapter </summary>
        public const int ADL_MAX_NUM_DISPLAYMODES = 1024;
        /// <summary> Define true </summary>
        public const int ADL_TRUE = 1;
        /// <summary> Maximum number of ADLModes for the adapter </summary>
        public const int ADL_FALSE = 0;
        /// <summary> Indicates the active dongle, all types </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE = 12;
        /// <summary> Indicates the Active dongle DP->DVI(double link) connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_DVI_DL = 6;
        /// <summary> Indicates the Active dongle DP->DVI(single link) connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_DVI_SL = 5;
        /// <summary> Indicates the Active dongle DP->HDMI connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_HDMI = 7;
        /// <summary> Indicates the Active dongle DP->VGA connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_ACTIVE_DONGLE_DP_VGA = 8;
        /// <summary> Indicates the DISPLAY PORT connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_DISPLAY_PORT = 4;
        /// <summary> Indicates the DVI_I connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_DVI = 1;
        /// <summary> Indicates the DVI_SL connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_DVI_SL = 2;
        /// <summary> Indicates the HDMI connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_HDMI = 3;
        /// <summary> Indicates the MST type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_MST = 11;
        /// <summary> Indicates the Active dongle DP->VGA connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_PASSIVE_DONGLE_DP_DVI = 10;
        /// <summary> Indicates the Passive dongle DP->HDMI connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_PASSIVE_DONGLE_DP_HDMI = 9;
        /// <summary> Indicates the VGA connection type is valid. </summary>
        public const int ADL_CONNECTION_TYPE_VGA = 0;
        /// <summary> Indicates the Virtual Connection Type.</summary>
        public const int ADL_CONNECTION_TYPE_VIRTUAL = 13;
        /// <summary> Indicates Active Dongle-JP Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_ATICVDONGLE_JP = 5;
        /// <summary> Indicates Active Dongle-NA Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_ATICVDONGLE_NA = 4;
        /// <summary> Indicates Active Dongle-NONI2C Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_ATICVDONGLE_NONI2C = 6;
        /// <summary> Indicates Active Dongle-NONI2C-D Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_ATICVDONGLE_NONI2C_D = 7;
        /// <summary> Indicates Display port Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_DISPLAYPORT = 10;
        /// <summary> Indicates DVI-D Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_DVI_D = 2;
        /// <summary> Indicates DVI-I Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_DVI_I = 3;
        /// <summary> Indicates EDP Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_EDP = 11;
        /// <summary> Indicates HDMI-Type A Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_HDMI_TYPE_A = 8;
        /// <summary> Indicates HDMI-Type B Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_HDMI_TYPE_B = 9;
        /// <summary> Indicates MiniDP Connector type. </summary>
        public const int ADL_CONNECTOR_TYPE_MINI_DISPLAYPORT = 12;
        /// <summary> Indicates Unknown Connector type. </summary>
        public const int ADL_CONNECTOR_TYPE_UNKNOWN = 0;
        /// <summary> Indicates USB type C Connector type. </summary>
        public const int ADL_CONNECTOR_TYPE_USB_TYPE_C = 14;
        /// <summary> Indicates VGA Connector type.  </summary>
        public const int ADL_CONNECTOR_TYPE_VGA = 1;
        /// <summary> Indicates Virtual Connector type.</summary>
        public const int ADL_CONNECTOR_TYPE_VIRTUAL = 13;

        // ADL Display Connector Types
        /// <summary> Indicates Unknown Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_UNKNOWN = 0;
        /// <summary> Indicates VGA Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_VGA = 1;
        /// <summary> Indicates DVI-D Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_DVI_D = 2;
        /// <summary> Indicates DVI-I Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_DVI_I = 3;
        /// <summary> Indicates ATICV NTSC Dongle Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_ATICVDONGLE_NTSC = 4;
        /// <summary> Indicates ATICV Japanese Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_ATICVDONGLE_JPN = 5;
        /// <summary> Indicates ATICV non-I2C Japanese Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_ATICVDONGLE_NONI2C_JPN = 6;
        /// <summary> Indicates ATICV non-I2C NTSC Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_ATICVDONGLE_NONI2C_NTSC = 7;
        /// <summary> Indicates Proprietary Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_PROPRIETARY = 8;
        /// <summary> Indicates HDMI Type A Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_HDMI_TYPE_A = 10;
        /// <summary> Indicates HDMI Type B Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_HDMI_TYPE_B = 11;
        /// <summary> Indicates S-Video Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_SVIDEO = 12;
        /// <summary> Indicates Composite Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_COMPOSITE = 13;
        /// <summary> Indicates RCA 3-component Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_RCA_3COMPONENT = 14;
        /// <summary> Indicates DisplayPort Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_DISPLAYPORT = 15;
        /// <summary> Indicates EDP Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_EDP = 16;
        /// <summary> Indicates Wireless Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_WIRELESSDISPLAY = 17;
        /// <summary> Indicates USB Type-C Display Connector type.</summary>
        public const int ADL_DISPLAY_CONTYPE_USB_TYPE_C = 18;

        // Display Info Constants
        /// <summary> Indicates the display is connected .</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_DISPLAYCONNECTED = 0x00000001;
        /// <summary> Indicates the display is mapped within OS </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_DISPLAYMAPPED = 0x00000002;
        /// <summary> Indicates the display can be forced </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_FORCIBLESUPPORTED = 0x00000008;
        /// <summary> Indicates the display supports genlock </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_GENLOCKSUPPORTED = 0x00000010;
        /// <summary> Indicates the display is an LDA display.</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_LDA_DISPLAY = 0x00000040;
        /// <summary> Indicates the display supports 2x Horizontal stretch </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2HSTRETCH = 0x00000800;
        /// <summary> Indicates the display supports 2x Vertical stretch </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_2VSTRETCH = 0x00000400;
        /// <summary> Indicates the display supports cloned desktops </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_CLONE = 0x00000200;
        /// <summary> Indicates the display supports extended desktops </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_EXTENDED = 0x00001000;
        /// <summary> Indicates the display supports N Stretched on 1 GPU</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCH1GPU = 0x00010000;
        /// <summary> Indicates the display supports N Stretched on N GPUs</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_NSTRETCHNGPU = 0x00020000;
        /// <summary> Reserved display info flag #2</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED2 = 0x00040000;
        /// <summary> Reserved display info flag #3</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_RESERVED3 = 0x00080000;
        /// <summary> Indicates the display supports single desktop </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MANNER_SUPPORTED_SINGLE = 0x00000100;
        /// <summary> Indicates the display supports overriding the mode timing </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MODETIMING_OVERRIDESSUPPORTED = 0x00000080;
        /// <summary> Indicates the display supports multi-vpu</summary>
        public const int ADL_DISPLAY_DISPLAYINFO_MULTIVPU_SUPPORTED = 0x00000020;
        /// <summary> Indicates the display is non-local to this machine </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_NONLOCAL = 0x00000004;
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAY_DISPLAYINFO_SHOWTYPE_PROJECTOR = 0x00100000;

        // Display Mode Constants
        /// <summary> Indicates the display is in interlaced mode</summary>
        public const int ADL_DISPLAY_MODE_INTERLACED_FLAG = 2;
        /// <summary> Indicates the display is in progressive mode </summary>
        public const int ADL_DISPLAY_MODE_PROGRESSIVE_FLAG = 0;
        /// <summary> Indicates the display colour format is 565</summary>
        public const int ADL_DISPLAY_MODE_COLOURFORMAT_565 = 0x00000001;
        /// <summary> Indicates the display colour format is 8888 </summary>
        public const int ADL_DISPLAY_MODE_COLOURFORMAT_8888 = 0x00000002;
        /// <summary> Indicates the display orientation is normal position</summary>
        public const int ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_000 = 0x00000004;
        /// <summary> Indicates the display is in the 90 degree position</summary>
        public const int ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_090 = 0x00000008;
        /// <summary> Indicates the display in the 180 degree position</summary>
        public const int ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_180 = 0x00000010;
        /// <summary> Indicates the display is in the 270 degree position</summary>
        public const int ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_270 = 0x00000020;
        /// <summary> Indicates the display refresh rate is exact </summary>
        public const int ADL_DISPLAY_MODE_REFRESHRATE_ONLY = 0x00000080;
        /// <summary> Indicates the display refresh rate is rounded</summary>
        public const int ADL_DISPLAY_MODE_REFRESHRATE_ROUNDED = 0x00000040;

        // DDCInfoX2 DDCInfo Flag values
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_PROJECTORDEVICE = (1 << 0);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_EDIDEXTENSION = (1 << 1);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_DIGITALDEVICE = (1 << 2);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_HDMIAUDIODEVICE = (1 << 3);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_SUPPORTS_AI = (1 << 4);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC601 = (1 << 5);
        /// <summary> Indicates the display is a projector </summary>
        public const int ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC709 = (1 << 6);


        // HDR Constants
        /// <summary> HDR10/CEA861.3 HDR supported</summary>
        public const int ADL_HDR_CEA861_3 = 0x0001;
        /// <summary> DolbyVision HDR supported</summary>
        public const int ADL_HDR_DOLBYVISION = 0x0002;
        /// <summary> FreeSync HDR supported.</summary>
        public const int ADL_HDR_FREESYNC_HDR = 0x0004;

        // DisplayMap constants

        // ADL_DISPLAY_DISPLAYMAP_MANNER_ Definitions
        // for ADLDisplayMap.iDisplayMapMask and ADLDisplayMap.iDisplayMapValue
        // (bit-vector)
        /// <summary> Indicates the display map manner is reserved</summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_RESERVED = 0x00000001;
        /// <summary> Indicates the display map manner is not active </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_NOTACTIVE = 0x00000002;
        /// <summary> Indicates the display map manner is single screens </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_SINGLE = 0x00000004;
        /// <summary> Indicates the display map manner is clone of another display </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_CLONE = 0x00000008;
        /// <summary> Indicates the display map manner is reserved</summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_RESERVED1 = 0x00000010;  // Removed NSTRETCH
        /// <summary> Indicates the display map manner is horizontal stretch </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_HSTRETCH = 0x00000020;
        /// <summary> Indicates the display map manner is vertical stretch </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_VSTRETCH = 0x00000040;
        /// <summary> Indicates the display map manner is VLD </summary>
        public const int ADL_DISPLAY_DISPLAYMAP_MANNER_VLD = 0x00000080;


        // ADL_DISPLAY_DISPLAYMAP_OPTION_ Definitions
        // for iOption in function ADL_Display_DisplayMapConfig_Get
        // (bit-vector)
        /// <summary> Indicates the display map option is GPU Info</summary>
        public const int ADL_DISPLAY_DISPLAYMAP_OPTION_GPUINFO = 0x00000001;

        // ADL_DISPLAY_DISPLAYTARGET_ Definitions
        // for ADLDisplayTarget.iDisplayTargetMask and ADLDisplayTarget.iDisplayTargetValue
        // (bit-vector)
        /// <summary> Indicates the display target is preferred </summary>
        public const int ADL_DISPLAY_DISPLAYTARGET_PREFERRED = 0x00000001;

        // ADL_DISPLAY_POSSIBLEMAPRESULT_VALID Definitions
        // for ADLPossibleMapResult.iPossibleMapResultMask and ADLPossibleMapResult.iPossibleMapResultValue
        // (bit-vector)
        /// <summary> Indicates the display map result is valid</summary>
        public const int ADL_DISPLAY_POSSIBLEMAPRESULT_VALID = 0x00000001;
        /// <summary> Indicates the display map result supports bezels</summary>
        public const int ADL_DISPLAY_POSSIBLEMAPRESULT_BEZELSUPPORTED = 0x00000002;
        /// <summary> Indicates the display map result supports overlap</summary>
        public const int ADL_DISPLAY_POSSIBLEMAPRESULT_OVERLAPSUPPORTED = 0x00000004;

        #endregion Internal Constant

        #region Internal Enums

        public enum ADLConnectionType
        {
            VGA = 0, 
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

        public enum ADLDisplayConnectionType
        {
            Unknown = 0,
            VGA = 1,
            DVI_D = 2,
            DVI_I = 3,
            HDMI = 4,
            ATICV_NTSC_Dongle = 4,
            ATICV_JPN_Dongle = 5,
            ATICV_NONI2C_NTSC_Dongle = 6,
            ATICV_NONI2C_JPN_Dongle = 7,
            Proprietary = 8,
            HDMITypeA = 10,
            HTMITypeB = 11,
            SVideo = 12,
            Composite = 13,
            RCA_3Component = 14,
            DisplayPort = 15,
            EDP = 16,
            WirelessDisplay = 17,
            USBTypeC = 18
        }

        public enum ADLDisplayModeFlag
        {
            ColourFormat565 = 1,
            ColourFormat8888 = 2,
            Degrees0 = 4,
            Degrees90 = 8,
            Degrees180 = 10,
            Degrees270 = 20,
            ExactRefreshRate = 80,
            RoundedRefreshRate = 40
        }
        public enum ADLDisplayModeInterlacing
        {
            Progressive = 0,
            Interlaced = 2
        }
        #endregion Internal Enums

        #region Class ADLImport
        /// <summary> ADLImport class</summary>
        private static class ADLImport
        {
            #region Internal Constant
            /// <summary> Atiadlxx_FileName </summary>
            public const string Atiadlxx_FileName = "atiadlxx.dll";
            /// <summary> Kernel32_FileName </summary>
            public const string Kernel32_FileName = "kernel32.dll";
            #endregion Internal Constant

            #region DLLImport
            [DllImport(Kernel32_FileName)]
            public static extern HMODULE GetModuleHandle (string moduleName);

            [DllImport(Atiadlxx_FileName)] 
            public static extern int ADL2_Main_Control_Create(ADL_Main_Memory_Alloc callback, int enumConnectedAdapters, out IntPtr contextHandle);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Main_Control_Destroy(IntPtr contextHandle);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Flush_Driver_Data(IntPtr ADLContextHandle, int adapterIndex);


            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Adapter_NumberOfAdapters_Get(IntPtr contextHandle, ref int numAdapters);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Adapter_Active_Get(IntPtr ADLContextHandle, int adapterIndex, ref int status);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_AdapterX2_Caps(IntPtr ADLContextHandle, int adapterIndex, out ADLAdapterCapsX2 adapterCapabilities);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Adapter_AdapterInfo_Get(IntPtr ADLContextHandle, int inputSize, out IntPtr AdapterInfoArray);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Adapter_AdapterInfoX2_Get(IntPtr ADLContextHandle, out IntPtr AdapterInfoArray);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Adapter_AdapterInfoX3_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr AdapterInfoArray);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Adapter_AdapterInfoX4_Get(IntPtr ADLContextHandle, int adapterIndex, out int numAdapters, out IntPtr AdapterInfoX2Array);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Display_DDCInfo2_Get(IntPtr contextHandle, int adapterIndex, int displayIndex, out ADLDDCInfo2 DDCInfo);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Display_DisplayInfo_Get(IntPtr ADLContextHandle, int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Display_DeviceConfig_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out ADLDisplayConfig displayConfig);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Display_HDRState_Get(IntPtr ADLContextHandle, int adapterIndex, ADLDisplayID displayID, out int support, out int enable);

            [DllImport(Atiadlxx_FileName)] 
            public static extern int ADL2_Display_Modes_Get(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, out int numModes, out IntPtr modes);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Display_Modes_Set(IntPtr ADLContextHandle, int adapterIndex, int displayIndex, int numModes, ref ADLMode modes);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Display_DisplayMapConfig_Get(IntPtr ADLContextHandle, int adapterIndex, out int numDisplayMap, out IntPtr displayMap, out int numDisplayTarget, out IntPtr displayTarget, int options);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Display_DisplayMapConfig_Set(IntPtr ADLContextHandle, int adapterIndex, int numDisplayMap, ref ADLDisplayMap displayMap, int numDisplayTarget, ref ADLDisplayTarget displayTarget);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Display_DisplayMapConfig_Validate(IntPtr ADLContextHandle, int adapterIndex, int numPossibleMap, ref ADLPossibleMap possibleMaps, out int numPossibleMapResult, ref IntPtr possibleMapResult);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL2_Display_DisplayMapConfig_PossibleAddAndRemove(IntPtr ADLContextHandle, int adapterIndex, int numDisplayMap, ref ADLDisplayMap displayMap, int numDisplayTarget, ref ADLDisplayTarget displayTarget, out int numPossibleAddTarget, out IntPtr possibleAddTarget, out int numPossibleRemoveTarget, out IntPtr possibleRemoveTarget);


            // ======================================


            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Main_Control_Create (ADL_Main_Memory_Alloc callback, int enumConnectedAdapters);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Main_Control_Destroy ();
            
            
            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Main_Control_IsFunctionValid (HMODULE module, string procName);

            [DllImport(Atiadlxx_FileName)]
            public static extern FARPROC ADL_Main_Control_GetProcAddress (HMODULE module, string procName);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Adapter_NumberOfAdapters_Get (ref int numAdapters);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Adapter_AdapterInfo_Get (out IntPtr info, int inputSize);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Adapter_Active_Get(int adapterIndex, ref int status);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Adapter_ID_Get(int adapterIndex, ref int adapterId);
            
            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_AdapterX2_Caps(int adapterIndex, out ADLAdapterCapsX2 adapterCapabilities);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_DisplayInfo_Get(int adapterIndex, ref int numDisplays, out IntPtr displayInfoArray, int forceDetect);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_DeviceConfig_Get(int adapterIndex, int displayIndex, out ADLDisplayConfig displayConfig);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_EdidData_Get(int adapterIndex, int displayIndex, ref ADLDisplayEDIDData EDIDData);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_DisplayMapConfig_Get(int adapterIndex, out int numDisplayMap, out IntPtr displayMap, out int numDisplayTarget, out IntPtr displayTarget, int options);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_DisplayMapConfig_PossibleAddAndRemove(int adapterIndex, int numDisplayMap, ADLDisplayMap displayMap, int numDisplayTarget, ADLDisplayTarget displayTarget, out int numPossibleAddTarget, out IntPtr possibleAddTarget, out int numPossibleRemoveTarget, out IntPtr possibleRemoveTarget);

            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_SLSMapConfig_Get(int adapterIndex, int SLSMapIndex, ref ADLSLSMap SLSMap, ref int NumSLSTarget, out IntPtr SLSTargetArray, ref int lpNumNativeMode, out IntPtr NativeMode, ref int NumBezelMode, out IntPtr BezelMode, ref int NumTransientMode, out IntPtr TransientMode, ref int NumSLSOffset, out IntPtr SLSOffset, int iOption);

            // This is used to set the SLS Grid we want from the SLSMap by selecting the one we want and supplying that as an index.
            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_SLSMapConfig_SetState(int AdapterIndex, int SLSMapIndex, int State);

            // Function to get the current supported SLS grid patterns (MxN) for a GPU.
            // This function gets a list of supported SLS grids for a specified input adapter based on display devices currently connected to the GPU.
            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_SLSGrid_Caps(int adapterIndex, ref int NumSLSGrid, out IntPtr SLSGrid, int option);

            // Function to get the active SLS map index list for a given GPU.
            // This function retrieves a list of active SLS map indexes for a specified input GPU.            
            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_SLSMapIndexList_Get(int adapterIndex, ref int numSLSMapIndexList, out IntPtr SLSMapIndexList, int options);

            // Function to get the active SLS map index list for a given GPU.
            // This function retrieves a list of active SLS map indexes for a specified input GPU.            
            [DllImport(Atiadlxx_FileName)]
            public static extern int ADL_Display_SLSMapIndex_Get(int adapterIndex, int ADLNumDisplayTarget, ref ADLDisplayTarget displayTarget, ref int SLSMapIndex);

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
            public static bool IsFunctionValid (string functionName)
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
            public static FARPROC GetProcAddress (string functionName)
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
        public static ADL_Main_Memory_Alloc ADL_Main_Memory_Alloc = ADL_Main_Memory_Alloc_;
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
        public static void ADL_Main_Memory_Free (IntPtr buffer)
        {
            if (IntPtr.Zero != buffer)
            {
                Marshal.FreeCoTaskMem(buffer);
            }
        }
        #endregion ADL_Main_Memory_Free

        #region ADL2_Main_Control_Create
        /// <summary> ADL2_Main_Control_Create Delegates</summary>
        public static ADL2_Main_Control_Create ADL2_Main_Control_Create
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
        public static ADL2_Main_Control_Destroy ADL2_Main_Control_Destroy
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

        #region ADL2_Flush_Driver_Data
        /// <summary> ADL2_Flush_Driver_Data Delegates</summary>
        public static ADL2_Flush_Driver_Data ADL2_Flush_Driver_Data
        {
            get
            {
                if (!ADL2_Flush_Driver_Data_Check && null == ADL2_Flush_Driver_Data_)
                {
                    ADL2_Flush_Driver_Data_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Flush_Driver_Data"))
                    {
                        ADL2_Flush_Driver_Data_ = ADLImport.ADL2_Flush_Driver_Data;
                    }
                }
                return ADL2_Flush_Driver_Data_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Flush_Driver_Data ADL2_Flush_Driver_Data_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Flush_Driver_Data_Check = false;
        #endregion ADL2_Flush_Driver_Data


        #region ADL2_Adapter_NumberOfAdapters_Get
        /// <summary> ADL2_Adapter_NumberOfAdapters_Get Delegates</summary>
        public static ADL2_Adapter_NumberOfAdapters_Get ADL2_Adapter_NumberOfAdapters_Get
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
        public static ADL2_Adapter_Active_Get ADL2_Adapter_Active_Get
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
        public static ADL2_AdapterX2_Caps ADL2_AdapterX2_Caps
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
        public static ADL2_Adapter_AdapterInfo_Get ADL2_Adapter_AdapterInfo_Get
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
        public static ADL2_Adapter_AdapterInfoX2_Get ADL2_Adapter_AdapterInfoX2_Get
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
        public static ADL2_Adapter_AdapterInfoX3_Get ADL2_Adapter_AdapterInfoX3_Get
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
        public static ADL2_Adapter_AdapterInfoX4_Get ADL2_Adapter_AdapterInfoX4_Get
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
        public static ADL2_Display_DDCInfo2_Get ADL2_Display_DDCInfo2_Get
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
        public static ADL2_Display_DisplayInfo_Get ADL2_Display_DisplayInfo_Get
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
        public static ADL2_Display_DeviceConfig_Get ADL2_Display_DeviceConfig_Get
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

        #region ADL2_Display_HDRState_Get
        /// <summary> ADL2_Display_HDRState_Get Delegates</summary>
        public static ADL2_Display_HDRState_Get ADL2_Display_HDRState_Get
        {
            get
            {
                if (!ADL2_Display_HDRState_Get_Check && null == ADL2_Display_HDRState_Get_)
                {
                    ADL2_Display_HDRState_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_HDRState_Get"))
                    {
                        ADL2_Display_HDRState_Get_ = ADLImport.ADL2_Display_HDRState_Get;
                    }
                }
                return ADL2_Display_HDRState_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_HDRState_Get ADL2_Display_HDRState_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_HDRState_Get_Check = false;
        #endregion ADL2_Display_HDRState_Get

        #region ADL2_Display_Modes_Get
        /// <summary> ADL2_Display_Modes_Get Delegates</summary>
        public static ADL2_Display_Modes_Get ADL2_Display_Modes_Get
        {
            get
            {
                if (!ADL2_Display_Modes_Get_Check && null == ADL2_Display_Modes_Get_)
                {
                    ADL2_Display_Modes_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_Modes_Get"))
                    {
                        ADL2_Display_Modes_Get_ = ADLImport.ADL2_Display_Modes_Get;
                    }
                }
                return ADL2_Display_Modes_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_Modes_Get ADL2_Display_Modes_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_Modes_Get_Check = false;
        #endregion ADL2_Display_Modes_Get

        #region ADL2_Display_Modes_Set
        /// <summary> ADL2_Display_Modes_Set Delegates</summary>
        public static ADL2_Display_Modes_Set ADL2_Display_Modes_Set
        {
            get
            {
                if (!ADL2_Display_Modes_Set_Check && null == ADL2_Display_Modes_Set_)
                {
                    ADL2_Display_Modes_Set_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_Modes_Set"))
                    {
                        ADL2_Display_Modes_Set_ = ADLImport.ADL2_Display_Modes_Set;
                    }
                }
                return ADL2_Display_Modes_Set_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_Modes_Set ADL2_Display_Modes_Set_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_Modes_Set_Check = false;
        #endregion ADL2_Display_Modes_Set

        #region ADL2_Display_DisplayMapConfig_Get
        public static ADL2_Display_DisplayMapConfig_Get ADL2_Display_DisplayMapConfig_Get
        {
            get
            {
                if (!ADL2_Display_DisplayMapConfig_Get_Check && null == ADL2_Display_DisplayMapConfig_Get_)
                {
                    ADL2_Display_DisplayMapConfig_Get_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_DisplayMapConfig_Get"))
                    {
                        ADL2_Display_DisplayMapConfig_Get_ = ADLImport.ADL2_Display_DisplayMapConfig_Get;
                    }
                }
                return ADL2_Display_DisplayMapConfig_Get_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_DisplayMapConfig_Get ADL2_Display_DisplayMapConfig_Get_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_DisplayMapConfig_Get_Check = false;
        #endregion ADL2_Display_DisplayMapConfig_Get

        #region ADL2_Display_DisplayMapConfig_Set
        /// <summary> ADL2_Display_DisplayMapConfig_Set Delegates</summary>
        public static ADL2_Display_DisplayMapConfig_Set ADL2_Display_DisplayMapConfig_Set
        {
            get
            {
                if (!ADL2_Display_DisplayMapConfig_Set_Check && null == ADL2_Display_DisplayMapConfig_Set_)
                {
                    ADL2_Display_DisplayMapConfig_Set_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_DisplayMapConfig_Set"))
                    {
                        ADL2_Display_DisplayMapConfig_Set_ = ADLImport.ADL2_Display_DisplayMapConfig_Set;
                    }
                }
                return ADL2_Display_DisplayMapConfig_Set_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_DisplayMapConfig_Set ADL2_Display_DisplayMapConfig_Set_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_DisplayMapConfig_Set_Check = false;
        #endregion ADL2_Display_DisplayMapConfig_Set

        #region ADL2_Display_DisplayMapConfig_Validate
        /// <summary> ADL2_Display_DisplayMapConfig_Validate Delegates</summary>
        public static ADL2_Display_DisplayMapConfig_Validate ADL2_Display_DisplayMapConfig_Validate
        {
            get
            {
                if (!ADL2_Display_DisplayMapConfig_Validate_Check && null == ADL2_Display_DisplayMapConfig_Validate_)
                {
                    ADL2_Display_DisplayMapConfig_Validate_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_DisplayMapConfig_Validate"))
                    {
                        ADL2_Display_DisplayMapConfig_Validate_ = ADLImport.ADL2_Display_DisplayMapConfig_Validate;
                    }
                }
                return ADL2_Display_DisplayMapConfig_Validate_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_DisplayMapConfig_Validate ADL2_Display_DisplayMapConfig_Validate_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_DisplayMapConfig_Validate_Check = false;
        #endregion ADL2_Display_DisplayMapConfig_Validate

        #region ADL2_Display_DisplayMapConfig_PossibleAddAndRemove
        /// <summary> ADL2_Display_DisplayMapConfig_PossibleAddAndRemove Delegates</summary>
        public static ADL2_Display_DisplayMapConfig_PossibleAddAndRemove ADL2_Display_DisplayMapConfig_PossibleAddAndRemove
        {
            get
            {
                if (!ADL2_Display_DisplayMapConfig_PossibleAddAndRemove_Check && null == ADL2_Display_DisplayMapConfig_PossibleAddAndRemove_)
                {
                    ADL2_Display_DisplayMapConfig_PossibleAddAndRemove_Check = true;
                    if (ADLCheckLibrary.IsFunctionValid("ADL2_Display_DisplayMapConfig_PossibleAddAndRemove"))
                    {
                        ADL2_Display_DisplayMapConfig_PossibleAddAndRemove_ = ADLImport.ADL2_Display_DisplayMapConfig_PossibleAddAndRemove;
                    }
                }
                return ADL2_Display_DisplayMapConfig_PossibleAddAndRemove_;
            }
        }
        /// <summary> Private Delegate</summary>
        private static ADL2_Display_DisplayMapConfig_PossibleAddAndRemove ADL2_Display_DisplayMapConfig_PossibleAddAndRemove_ = null;
        /// <summary> check flag to indicate the delegate has been checked</summary>
        private static bool ADL2_Display_DisplayMapConfig_PossibleAddAndRemove_Check = false;
        #endregion ADL2_Display_DisplayMapConfig_PossibleAddAndRemove

        // ================================

        #region ADL_Main_Control_Create
        /// <summary> ADL_Main_Control_Create Delegates</summary>
        public static ADL_Main_Control_Create ADL_Main_Control_Create
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
        public static ADL_Main_Control_Destroy ADL_Main_Control_Destroy
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
        public static ADL_Adapter_NumberOfAdapters_Get ADL_Adapter_NumberOfAdapters_Get
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
        public static ADL_Adapter_ID_Get ADL_Adapter_ID_Get
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
        public static ADL_AdapterX2_Caps ADL_AdapterX2_Caps
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
        public static ADL_Adapter_AdapterInfo_Get ADL_Adapter_AdapterInfo_Get
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
        public static ADL_Adapter_Active_Get ADL_Adapter_Active_Get
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
        public static ADL_Display_DeviceConfig_Get ADL_Display_DeviceConfig_Get
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
        public static ADL_Display_DisplayMapConfig_Get ADL_Display_DisplayMapConfig_Get
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
        public static ADL_Display_DisplayMapConfig_PossibleAddAndRemove ADL_Display_DisplayMapConfig_PossibleAddAndRemove
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
        public static ADL_Display_EdidData_Get ADL_Display_EdidData_Get
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
        public static ADL_Display_DisplayInfo_Get ADL_Display_DisplayInfo_Get
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
        public static ADL_Display_SLSMapConfig_Get ADL_Display_SLSMapConfig_Get
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

        public static ConvertedDDCInfoFlag ConvertDDCInfoFlag(int DDCInfoValue)
        {
            ConvertedDDCInfoFlag expandedDDCInfoValue = new ConvertedDDCInfoFlag();

            // Indicates the display is a digital device
            if ((DDCInfoValue & ADL.ADL_DISPLAYDDCINFOEX_FLAG_DIGITALDEVICE) == ADL.ADL_DISPLAYDDCINFOEX_FLAG_DIGITALDEVICE)
                expandedDDCInfoValue.DIGITALDEVICE = true;
            // Indicates the display supports EDID queries
            if ((DDCInfoValue & ADL.ADL_DISPLAYDDCINFOEX_FLAG_EDIDEXTENSION) == ADL.ADL_DISPLAYDDCINFOEX_FLAG_EDIDEXTENSION)
                expandedDDCInfoValue.EDIDEXTENSION = true;
            // Indicates the display suports HDMI Audio
            if ((DDCInfoValue & ADL.ADL_DISPLAYDDCINFOEX_FLAG_HDMIAUDIODEVICE) == ADL.ADL_DISPLAYDDCINFOEX_FLAG_HDMIAUDIODEVICE)
                expandedDDCInfoValue.HDMIAUDIODEVICE = true;
            // Indicates the display is a projector
            if ((DDCInfoValue & ADL.ADL_DISPLAYDDCINFOEX_FLAG_PROJECTORDEVICE) == ADL.ADL_DISPLAYDDCINFOEX_FLAG_PROJECTORDEVICE)
                expandedDDCInfoValue.PROJECTORDEVICE = true;
            // Indicates the display supports AI
            if ((DDCInfoValue & ADL.ADL_DISPLAYDDCINFOEX_FLAG_SUPPORTS_AI) == ADL.ADL_DISPLAYDDCINFOEX_FLAG_SUPPORTS_AI)
                expandedDDCInfoValue.SUPPORTS_AI = true;
            // Indicates the display supports YCC601
            if ((DDCInfoValue & ADL.ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC601) == ADL.ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC601)
                expandedDDCInfoValue.SUPPORT_xvYCC601 = true;
            // Indicates the display supports YCC709
            if ((DDCInfoValue & ADL.ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC709) == ADL.ADL_DISPLAYDDCINFOEX_FLAG_SUPPORT_xvYCC709)
                expandedDDCInfoValue.SUPPORT_xvYCC709 = true;

            return expandedDDCInfoValue;

        }

        public static ConvertedSupportedHDR ConvertSupportedHDR(int supportedHDR)
        {
            ConvertedSupportedHDR expandedSupportedHDR = new ConvertedSupportedHDR();

            // Indicates the display is a digital device
            if ((supportedHDR & ADL.ADL_HDR_CEA861_3) == ADL.ADL_HDR_CEA861_3)
                expandedSupportedHDR.CEA861_3 = true;
            // Indicates the display supports EDID queries
            if ((supportedHDR & ADL.ADL_HDR_DOLBYVISION) == ADL.ADL_HDR_DOLBYVISION)
                expandedSupportedHDR.DOLBYVISION = true;
            // Indicates the display suports HDMI Audio
            if ((supportedHDR & ADL.ADL_HDR_FREESYNC_HDR) == ADL.ADL_HDR_FREESYNC_HDR)
                expandedSupportedHDR.FREESYNC_HDR = true;

            return expandedSupportedHDR;

        }

        public static ConvertedDisplayInfoValue ConvertDisplayInfoValue(int displayInfoValue)
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

        public static ConvertedDisplayModeFlags ConvertDisplayModeFlags(int displayModeFlag)
        {
            ConvertedDisplayModeFlags expandedDisplayModeFlags = new ConvertedDisplayModeFlags();

            // Indicates the display is a digital device
            if ((displayModeFlag & ADL.ADL_DISPLAY_MODE_COLOURFORMAT_565) == ADL.ADL_DISPLAY_MODE_COLOURFORMAT_565)
                expandedDisplayModeFlags.COLOURFORMAT_565 = true;
            // Indicates the display supports EDID queries
            if ((displayModeFlag & ADL.ADL_DISPLAY_MODE_COLOURFORMAT_8888) == ADL.ADL_DISPLAY_MODE_COLOURFORMAT_8888)
                expandedDisplayModeFlags.COLOURFORMAT_8888 = true;
            // Indicates the display supports normal vertical orientation
            if ((displayModeFlag & ADL.ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_000) == ADL.ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_000)
                expandedDisplayModeFlags.ORIENTATION_SUPPORTED_000 = true;
            // Indicates the display supports normal vertical orientation
            if ((displayModeFlag & ADL.ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_090) == ADL.ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_090)
                expandedDisplayModeFlags.ORIENTATION_SUPPORTED_090 = true;
            // Indicates the display supports normal vertical orientation
            if ((displayModeFlag & ADL.ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_180) == ADL.ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_180)
                expandedDisplayModeFlags.ORIENTATION_SUPPORTED_180 = true;
            // Indicates the display supports normal vertical orientation
            if ((displayModeFlag & ADL.ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_270) == ADL.ADL_DISPLAY_MODE_ORIENTATION_SUPPORTED_270)
                expandedDisplayModeFlags.ORIENTATION_SUPPORTED_270 = true;
            // Indicates the display supports normal vertical orientation
            if ((displayModeFlag & ADL.ADL_DISPLAY_MODE_REFRESHRATE_ROUNDED) == ADL.ADL_DISPLAY_MODE_REFRESHRATE_ROUNDED)
                expandedDisplayModeFlags.REFRESHRATE_ROUNDED = true;
            // Indicates the display supports normal vertical orientation
            if ((displayModeFlag & ADL.ADL_DISPLAY_MODE_REFRESHRATE_ONLY) == ADL.ADL_DISPLAY_MODE_REFRESHRATE_ONLY)
                expandedDisplayModeFlags.REFRESHRATE_ONLY = true;

            return expandedDisplayModeFlags;

        }


        public static string ConvertADLReturnValueIntoWords(int adlReturnValue)
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