using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using ATI.ADL;
using Microsoft.Win32.SafeHandles;
using DisplayMagicianShared;

namespace DisplayMagicianShared.AMD
{
    public class ADLWrapper : IDisposable
    {
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static ADLWrapper _instance = new ADLWrapper();

        private bool _initialised = false;
        private bool _initialisedADL2 = false;

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        private IntPtr _adlContextHandle = IntPtr.Zero;


        static ADLWrapper() { }
        public ADLWrapper()
        {
            int ADLRet = ADL.ADL_ERR;

            SharedLogger.logger.Trace("ADLWrapper/ADLWrapper: Intialising ADL library");
            try
            {
                if (ADL.ADL_Main_Control_Create != null)
                {
                    // Second parameter is 1: Get only the present adapters
                    ADLRet = ADL.ADL_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1);
                }

                if (ADLRet == ADL.ADL_OK)
                {
                    _initialised = true;
                    SharedLogger.logger.Trace("ADLWrapper/ADLWrapper: ADL library was initialised successfully");
                }
                else
                {
                    SharedLogger.logger.Error("ADLWrapper/ADLWrapper: Error intialising ADL library. ADL_Main_Control_Create() returned error code " + ADLRet.ToString());
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "ADLWrapper/ADLWrapper: Exception intialising ADL library. ADL_Main_Control_Create() caused an exception");
            }

            try
            {
                if (ADL.ADL2_Main_Control_Create != null)
                {
                    // Second parameter is 1: Get only the present adapters
                    ADLRet = ADL.ADL2_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1, out _adlContextHandle);
                }

                if (ADLRet == ADL.ADL_OK)
                {
                    _initialisedADL2 = true;
                    SharedLogger.logger.Trace("ADLWrapper/ADLWrapper: ADL2 library was initialised successfully");
                }
                else
                {
                    SharedLogger.logger.Error("ADLWrapper/ADLWrapper: Error intialising ADL2 library. ADL2_Main_Control_Create() returned error code " + ADL.ConvertADLReturnValueIntoWords(ADLRet));
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "ADLWrapper/ADLWrapper: Exception intialising ADL2 library. ADL2_Main_Control_Create() caused an exception");
            }

        }

        ~ADLWrapper()
        {
            // If the ADL library was initialised, then we need to free it up.
            if (_initialised)
            {
                if (null != ADL.ADL_Main_Control_Destroy)
                    ADL.ADL_Main_Control_Destroy();
            }
            // If the ADL2 library was initialised, then we need to free it up.
            if (_initialisedADL2)
            {
                if (null != ADL.ADL2_Main_Control_Destroy)
                    ADL.ADL2_Main_Control_Destroy(_adlContextHandle);
            }
        }

        // Public implementation of Dispose pattern callable by consumers.
        public void Dispose() => Dispose(true);

        // Protected implementation of Dispose pattern.
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (null != ADL.ADL_Main_Control_Destroy)
                    ADL.ADL_Main_Control_Destroy();

                // Dispose managed state (managed objects).
                _safeHandle?.Dispose();
            }

            _disposed = true;
        }


        public bool IsInstalled
        {
            get { return _initialised || _initialisedADL2; }
        }

        public static ADLWrapper GetLibrary()
        {
            return _instance;
        }

        public List<string> GenerateProfileDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Getting AMD active adapter count");

            int ADLRet = ADL.ADL_ERR;
            int NumberOfAdapters = 0;
            int NumberOfDisplays = 0;

            List<string> displayIdentifiers = new List<string>();

            if (null != ADL.ADL2_Adapter_NumberOfAdapters_Get)
            {
                ADL.ADL2_Adapter_NumberOfAdapters_Get(_adlContextHandle, ref NumberOfAdapters);
                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Number Of Adapters: {NumberOfAdapters.ToString()} ");
            }

            if (NumberOfAdapters > 0)
            {
                // Get OS adpater info from ADL
                ADLAdapterInfoX2Array OSAdapterInfoData;
                OSAdapterInfoData = new ADLAdapterInfoX2Array();

                IntPtr AdapterBuffer = IntPtr.Zero;
                if (ADL.ADL2_Adapter_AdapterInfoX4_Get != null)
                {
                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ADL2_Adapter_AdapterInfoX4_Get DLL function exists.");
                    // Figure out the size of the AdapterBuffer we need to build                    
                    int size = Marshal.SizeOf(OSAdapterInfoData);
                    AdapterBuffer = Marshal.AllocCoTaskMem((int)size);
                    Marshal.StructureToPtr(OSAdapterInfoData, AdapterBuffer, false);

                    // Get the Adapter info and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Running ADL2_Adapter_AdapterInfoX4_Get to find all known AMD adapters.");
                    //ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, AdapterBuffer, size);
                    int numAdapters = 0;
                    ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, ADL.ADL_ADAPTER_INDEX_ALL, out numAdapters, out AdapterBuffer);
                    if (ADLRet == ADL.ADL_OK)
                    {
                        // Use the AdapterBuffer pointer to marshal the OS Adapter Info into a structure
                        OSAdapterInfoData = (ADLAdapterInfoX2Array)Marshal.PtrToStructure(AdapterBuffer, OSAdapterInfoData.GetType());
                        int IsActive = ADL.ADL_TRUE; // We only want to search for active adapters

                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Successfully run ADL2_Adapter_AdapterInfoX4_Get to find information about all known AMD adapters.");

                        // Go through each adapter
                        foreach (ADLAdapterInfoX2 oneAdapter in OSAdapterInfoData.ADLAdapterInfoX2)
                        {
                            if (oneAdapter.Exist != 1)
                            {
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                                Console.WriteLine($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                                continue;
                            }

                            if (oneAdapter.Present != 1)
                            {
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
                                Console.WriteLine($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
                                continue;
                            }

                            // Check if the adapter is active
                            if (ADL.ADL2_Adapter_Active_Get != null)
                                ADLRet = ADL.ADL2_Adapter_Active_Get(_adlContextHandle, oneAdapter.AdapterIndex, ref IsActive);

                            if (ADLRet == ADL.ADL_OK)
                            {
                                // Only continue if the adapter is enabled
                                if (IsActive != ADL.ADL_TRUE)
                                {
                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't active ({oneAdapter.AdapterName}).");
                                    continue;
                                }

                                // Only continue if the adapter index is > 0
                                if (oneAdapter.AdapterIndex < 0)
                                {
                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter has an adapter index of {oneAdapter.AdapterIndex.ToString()} which indicates it is not a real adapter.");
                                    continue;
                                }

                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} is active! ({oneAdapter.AdapterName}).");

                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Adapter Info for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter AdapterIndex = {oneAdapter.AdapterIndex}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter AdapterName = {oneAdapter.AdapterName}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter BusNumber = {oneAdapter.BusNumber}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter DeviceNumber = {oneAdapter.DeviceNumber}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter DisplayName = {oneAdapter.DisplayName}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter DriverPath = {oneAdapter.DriverPath}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter DriverPathExt = {oneAdapter.DriverPathExt}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Exist = {oneAdapter.Exist}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter FunctionNumber = {oneAdapter.FunctionNumber}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter InfoMask = {oneAdapter.InfoMask}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter InfoValue = {oneAdapter.InfoValue}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter OSDisplayIndex = {oneAdapter.OSDisplayIndex}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter PNPString = {oneAdapter.PNPString}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Present = {oneAdapter.Present}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Size = {oneAdapter.Size}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter UDID = {oneAdapter.UDID}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter VendorID = {oneAdapter.VendorID}");

                                // Get the Adapter Capabilities
                                ADLAdapterCapsX2 AdapterCapabilities = new ADLAdapterCapsX2();
                                if (ADL.ADL2_AdapterX2_Caps != null)
                                {
                                    ADLRet = ADL.ADL2_AdapterX2_Caps(_adlContextHandle, oneAdapter.AdapterIndex, out AdapterCapabilities);
                                }

                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Adapter Capabilities for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter ID = {AdapterCapabilities.AdapterID}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Capabilities Mask = {AdapterCapabilities.CapsMask}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Capabilities Value = {AdapterCapabilities.CapsValue}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of Connectors = {AdapterCapabilities.NumConnectors}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of Controllers = {AdapterCapabilities.NumControllers}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of Displays = {AdapterCapabilities.NumDisplays}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of GL Sync Connectors = {AdapterCapabilities.NumOfGLSyncConnectors}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of Overlays = {AdapterCapabilities.NumOverlays}");

                                // Obtain information about displays
                                ADLDisplayInfoArray displayInfoArray = new ADLDisplayInfoArray();

                                if (ADL.ADL2_Display_DisplayInfo_Get != null)
                                {
                                    IntPtr DisplayBuffer = IntPtr.Zero;

                                    // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                    ADLRet = ADL.ADL2_Display_DisplayInfo_Get(_adlContextHandle, oneAdapter.AdapterIndex, ref NumberOfDisplays, out DisplayBuffer, 0);
                                    if (ADLRet == ADL.ADL_OK)
                                    {

                                        try
                                        {
                                            displayInfoArray = (ADLDisplayInfoArray)Marshal.PtrToStructure(DisplayBuffer, displayInfoArray.GetType());

                                            foreach (ADLDisplayInfo oneDisplayInfo in displayInfoArray.ADLDisplayInfo)
                                            {

                                                if (oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex == -1)
                                                {
                                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not a real display as its DisplayID.DisplayLogicalAdapterIndex is -1");
                                                    continue;
                                                }

                                                // Convert the displayInfoValue to something usable using a library function I made
                                                ConvertedDisplayInfoValue displayInfoValue = ADL.ConvertDisplayInfoValue(oneDisplayInfo.DisplayInfoValue);

                                                if (!displayInfoValue.DISPLAYCONNECTED)
                                                {
                                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not connected");
                                                    continue;
                                                }

                                                // Skip connected but non-mapped displays (not mapped in windows) - we want all displays currently visible in the OS
                                                if (!displayInfoValue.DISPLAYMAPPED)
                                                {
                                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not mapped in Windows OS");
                                                    continue;
                                                }

                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is connected and mapped in Windows OS");

                                                ADL.ADLDisplayConnectionType displayConnector = (ADL.ADLDisplayConnectionType)oneDisplayInfo.DisplayConnector;

                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Display Info for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Connector = {displayConnector.ToString("G")}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Controller Index = {oneDisplayInfo.DisplayControllerIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Logical Adapter Index = {oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Logical Index = {oneDisplayInfo.DisplayID.DisplayLogicalIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Physical Adapter Index = {oneDisplayInfo.DisplayID.DisplayPhysicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Physical Index = {oneDisplayInfo.DisplayID.DisplayPhysicalIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Mask = {oneDisplayInfo.DisplayInfoMask}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value = {oneDisplayInfo.DisplayInfoValue}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Manufacturer Name = {oneDisplayInfo.DisplayManufacturerName}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Name = {oneDisplayInfo.DisplayName}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Output Type = {oneDisplayInfo.DisplayOutputType}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Type = {oneDisplayInfo.DisplayType}");

                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value DISPLAYCONNECTED = {displayInfoValue.DISPLAYCONNECTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value DISPLAYMAPPED = {displayInfoValue.DISPLAYMAPPED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value FORCIBLESUPPORTED = {displayInfoValue.FORCIBLESUPPORTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value GENLOCKSUPPORTED = {displayInfoValue.GENLOCKSUPPORTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value LDA_DISPLAY = {displayInfoValue.LDA_DISPLAY}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_2HSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2HSTRETCH}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_2VSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2VSTRETCH}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_CLONE = {displayInfoValue.MANNER_SUPPORTED_CLONE}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_EXTENDED = {displayInfoValue.MANNER_SUPPORTED_EXTENDED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_NSTRETCH1GPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCH1GPU}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_NSTRETCHNGPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCHNGPU}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_SINGLE = {displayInfoValue.MANNER_SUPPORTED_SINGLE}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MODETIMING_OVERRIDESSUPPORTED = {displayInfoValue.MODETIMING_OVERRIDESSUPPORTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MULTIVPU_SUPPORTED = {displayInfoValue.MULTIVPU_SUPPORTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value NONLOCAL = {displayInfoValue.NONLOCAL}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value SHOWTYPE_PROJECTOR = {displayInfoValue.SHOWTYPE_PROJECTOR}");

                                                ADL.ADLDisplayConnectionType displayConnectionType = ADL.ADLDisplayConnectionType.Unknown;
                                                ADLDisplayConfig displayConfig = new ADLDisplayConfig();
                                                displayConfig.Size = Marshal.SizeOf(displayConfig);
                                                if (ADL.ADL2_Display_DeviceConfig_Get != null)
                                                {
                                                    // Get the DisplayConfig from the Display
                                                    ADLRet = ADL.ADL2_Display_DeviceConfig_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID.DisplayPhysicalIndex, out displayConfig);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {
                                                        displayConnectionType = (ADL.ADLDisplayConnectionType)displayConfig.ConnectorType;

                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Display Device Config for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Connector Type = {displayConnectionType.ToString("G")}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Device Data = {displayConfig.DeviceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Overridded Device Data = {displayConfig.OverriddedDeviceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Reserved Data = {displayConfig.Reserved}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Size = {displayConfig.Size}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_DeviceConfig_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                                    }
                                                }

                                                ADLDDCInfo2 displayDDCInfo2 = new ADLDDCInfo2();
                                                displayDDCInfo2.Size = Marshal.SizeOf(displayDDCInfo2);
                                                // Create a stringbuilder buffer that EDID can be loaded into
                                                //displayEDIDData.EDIDData = new StringBuilder(256);

                                                if (ADL.ADL2_Display_DDCInfo2_Get != null)
                                                {
                                                    // Get the DDC Data from the Display
                                                    ADLRet = ADL.ADL2_Display_DDCInfo2_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID.DisplayPhysicalIndex, out displayDDCInfo2);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {

                                                        // Convert the DDCInfoFlag to something usable using a library function I made
                                                        ConvertedDDCInfoFlag DDCInfoFlag = ADL.ConvertDDCInfoFlag(displayDDCInfo2.DDCInfoFlag);

                                                        // Convert the DDCInfoFlag to something usable using a library function I made
                                                        ConvertedSupportedHDR supportedHDR = ADL.ConvertSupportedHDR(displayDDCInfo2.SupportedHDR);

                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Display DDCInfo2 for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display AvgLuminanceData = {displayDDCInfo2.AvgLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag = {displayDDCInfo2.DDCInfoFlag}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DiffuseScreenReflectance = {displayDDCInfo2.DiffuseScreenReflectance}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DisplayName = {displayDDCInfo2.DisplayName}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display FreesyncFlags = {displayDDCInfo2.FreesyncFlags}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display ManufacturerID = {displayDDCInfo2.ManufacturerID}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxBacklightMaxLuminanceData = {displayDDCInfo2.MaxBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxBacklightMinLuminanceData = {displayDDCInfo2.MaxBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxHResolution = {displayDDCInfo2.MaxHResolution}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxLuminanceData = {displayDDCInfo2.MaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxRefresh = {displayDDCInfo2.MaxRefresh}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxVResolution = {displayDDCInfo2.MaxVResolution}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MinBacklightMaxLuminanceData = {displayDDCInfo2.MinBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MinBacklightMinLuminanceData = {displayDDCInfo2.MinBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MinLuminanceData = {displayDDCInfo2.MinLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MinLuminanceNoDimmingData = {displayDDCInfo2.MinLuminanceNoDimmingData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityBlueX = {displayDDCInfo2.NativeDisplayChromaticityBlueX}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityBlueY = {displayDDCInfo2.NativeDisplayChromaticityBlueY}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityGreenX = {displayDDCInfo2.NativeDisplayChromaticityGreenX}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityGreenY = {displayDDCInfo2.NativeDisplayChromaticityGreenY}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityRedX = {displayDDCInfo2.NativeDisplayChromaticityRedX}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityRedY = {displayDDCInfo2.NativeDisplayChromaticityRedY}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityWhiteX = {displayDDCInfo2.NativeDisplayChromaticityWhiteX}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityWhiteY = {displayDDCInfo2.NativeDisplayChromaticityWhiteY}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PackedPixelSupported = {displayDDCInfo2.PackedPixelSupported}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PanelPixelFormat = {displayDDCInfo2.PanelPixelFormat}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display ProductID = {displayDDCInfo2.ProductID}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PTMCx = {displayDDCInfo2.PTMCx}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PTMCy = {displayDDCInfo2.PTMCy}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PTMRefreshRate = {displayDDCInfo2.PTMRefreshRate}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SerialID = {displayDDCInfo2.SerialID}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Size = {displayDDCInfo2.Size}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SpecularScreenReflectance = {displayDDCInfo2.SpecularScreenReflectance}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedColorSpace = {displayDDCInfo2.SupportedColorSpace}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedHDR = {displayDDCInfo2.SupportedHDR}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedTransferFunction = {displayDDCInfo2.SupportedTransferFunction}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportsDDC = {displayDDCInfo2.SupportsDDC}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Digital Device  = {DDCInfoFlag.DIGITALDEVICE}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag EDID Extension = {DDCInfoFlag.EDIDEXTENSION}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag HDMI Audio Device  = {DDCInfoFlag.HDMIAUDIODEVICE}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Projector Device = {DDCInfoFlag.PROJECTORDEVICE}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Supports AI = {DDCInfoFlag.SUPPORTS_AI}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Supports xvYCC601 = {DDCInfoFlag.SUPPORT_xvYCC601}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Supports xvYCC709 = {DDCInfoFlag.SUPPORT_xvYCC709}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedHDR Supports CEA861_3 = {supportedHDR.CEA861_3}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedHDR Supports DOLBYVISION = {supportedHDR.DOLBYVISION}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedHDR Supports FREESYNC_HDR = {supportedHDR.FREESYNC_HDR}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_DDCInfo2_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                                    }
                                                }

                                                int HDRSupported = 0;
                                                int HDREnabled = 0;
                                                if (ADL.ADL2_Display_HDRState_Get != null)
                                                {
                                                    // Get the HDR State from the Display
                                                    ADLRet = ADL.ADL2_Display_HDRState_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID, out HDRSupported, out HDREnabled);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Display HDR State for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display HDR Supported = {HDRSupported}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display HDR Enabled = {HDREnabled}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_HDRState_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                                    }
                                                }


                                                // Create an array of all the important display info we need to record
                                                List<string> displayInfoIdentifierSection = new List<string>();
                                                displayInfoIdentifierSection.Add("AMD");
                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.VendorID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Vendor ID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.AdapterName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Adapter Name from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.VendorID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD VendorID  from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("1002");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(AdapterCapabilities.AdapterID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD AdapterID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayConnector.ToString("G"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Display Connector from video card to display. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneDisplayInfo.DisplayName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Display Name from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ManufacturerID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Manufacturer ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ProductID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Product ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.SerialID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Serial ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                // Create a display identifier out of it
                                                string displayIdentifier = String.Join("|", displayInfoIdentifierSection);

                                                // Check first to see if there is already an existing display identifier the same!
                                                // This appears to be a bug with the AMD driver, or with the install on my test machine
                                                // Either way, it is potentially going to happen in the wild, so I will filter it out if it does
                                                if (displayIdentifiers.Contains(displayIdentifier))
                                                {
                                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Your AMD driver reported the following Display Identifier multiple times, so ignoring it as we already have it: {displayIdentifier}");
                                                    continue;
                                                }

                                                // Add it to the list of display identifiers so we can return it
                                                displayIdentifiers.Add(displayIdentifier);

                                                SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception caused trying to access attached displays");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_DisplayInfo_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                    }
                                    // Release the memory for the DisplayInfo structure
                                    if (IntPtr.Zero != DisplayBuffer)
                                        Marshal.FreeCoTaskMem(DisplayBuffer);
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Adapter_Active_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                            }
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Adapter_AdapterInfoX4_Get on AMD Video card: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                    }
                }
                // Release the memory for the AdapterInfo structure
                if (IntPtr.Zero != AdapterBuffer)
                {
                    Marshal.FreeCoTaskMem(AdapterBuffer);
                }

                // Return all the identifiers we've found
                return displayIdentifiers;
            }
            else
            {
                SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: There were no AMD adapters found by AMD ADL.");
                return null;
            }
        }

        public List<string> GenerateAllAvailableDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Getting AMD active adapter count");

            int ADLRet = ADL.ADL_ERR;
            int NumberOfAdapters = 0;
            int NumberOfDisplays = 0;

            List<string> displayIdentifiers = new List<string>();

            if (null != ADL.ADL2_Adapter_NumberOfAdapters_Get)
            {
                ADL.ADL2_Adapter_NumberOfAdapters_Get(_adlContextHandle, ref NumberOfAdapters);
                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Number Of Adapters: {NumberOfAdapters.ToString()} ");
            }

            if (NumberOfAdapters > 0)
            {
                // Get OS adpater info from ADL
                ADLAdapterInfoX2Array OSAdapterInfoData;
                OSAdapterInfoData = new ADLAdapterInfoX2Array();

                IntPtr AdapterBuffer = IntPtr.Zero;
                if (ADL.ADL2_Adapter_AdapterInfoX4_Get != null)
                {
                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ADL2_Adapter_AdapterInfoX4_Get DLL function exists.");
                    // Figure out the size of the AdapterBuffer we need to build                    
                    int size = Marshal.SizeOf(OSAdapterInfoData);
                    AdapterBuffer = Marshal.AllocCoTaskMem((int)size);
                    Marshal.StructureToPtr(OSAdapterInfoData, AdapterBuffer, false);

                    // Get the Adapter info and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Running ADL2_Adapter_AdapterInfoX4_Get to find all known AMD adapters.");
                    //ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, AdapterBuffer, size);
                    int numAdapters = 0;
                    ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, ADL.ADL_ADAPTER_INDEX_ALL, out numAdapters, out AdapterBuffer);
                    if (ADLRet == ADL.ADL_OK)
                    {
                        // Use the AdapterBuffer pointer to marshal the OS Adapter Info into a structure
                        OSAdapterInfoData = (ADLAdapterInfoX2Array)Marshal.PtrToStructure(AdapterBuffer, OSAdapterInfoData.GetType());
                        int IsActive = ADL.ADL_TRUE; // We only want to search for active adapters

                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Successfully run ADL2_Adapter_AdapterInfoX4_Get to find information about all known AMD adapters.");

                        // Go through each adapter
                        foreach (ADLAdapterInfoX2 oneAdapter in OSAdapterInfoData.ADLAdapterInfoX2)
                        {
                            if (oneAdapter.Exist != 1)
                            {
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                                Console.WriteLine($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                                continue;
                            }

                            if (oneAdapter.Present != 1)
                            {
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
                                Console.WriteLine($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
                                continue;
                            }

                            // Check if the adapter is active
                            if (ADL.ADL2_Adapter_Active_Get != null)
                                ADLRet = ADL.ADL2_Adapter_Active_Get(_adlContextHandle, oneAdapter.AdapterIndex, ref IsActive);

                            if (ADLRet == ADL.ADL_OK)
                            {
                                // Only continue if the adapter is enabled
                                if (IsActive != ADL.ADL_TRUE)
                                {
                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't active ({oneAdapter.AdapterName}).");
                                    continue;
                                }

                                // Only continue if the adapter index is > 0
                                if (oneAdapter.AdapterIndex < 0)
                                {
                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter has an adapter index of {oneAdapter.AdapterIndex.ToString()} which indicates it is not a real adapter.");
                                    continue;
                                }

                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} is active! ({oneAdapter.AdapterName}).");

                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Adapter Info for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter AdapterIndex = {oneAdapter.AdapterIndex}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter AdapterName = {oneAdapter.AdapterName}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter BusNumber = {oneAdapter.BusNumber}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter DeviceNumber = {oneAdapter.DeviceNumber}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter DisplayName = {oneAdapter.DisplayName}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter DriverPath = {oneAdapter.DriverPath}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter DriverPathExt = {oneAdapter.DriverPathExt}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Exist = {oneAdapter.Exist}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter FunctionNumber = {oneAdapter.FunctionNumber}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter InfoMask = {oneAdapter.InfoMask}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter InfoValue = {oneAdapter.InfoValue}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter OSDisplayIndex = {oneAdapter.OSDisplayIndex}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter PNPString = {oneAdapter.PNPString}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Present = {oneAdapter.Present}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Size = {oneAdapter.Size}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter UDID = {oneAdapter.UDID}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter VendorID = {oneAdapter.VendorID}");

                                // Get the Adapter Capabilities
                                ADLAdapterCapsX2 AdapterCapabilities = new ADLAdapterCapsX2();
                                if (ADL.ADL2_AdapterX2_Caps != null)
                                {
                                    ADLRet = ADL.ADL2_AdapterX2_Caps(_adlContextHandle, oneAdapter.AdapterIndex, out AdapterCapabilities);
                                }

                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Adapter Capabilities for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter ID = {AdapterCapabilities.AdapterID}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Capabilities Mask = {AdapterCapabilities.CapsMask}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Capabilities Value = {AdapterCapabilities.CapsValue}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of Connectors = {AdapterCapabilities.NumConnectors}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of Controllers = {AdapterCapabilities.NumControllers}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of Displays = {AdapterCapabilities.NumDisplays}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of GL Sync Connectors = {AdapterCapabilities.NumOfGLSyncConnectors}");
                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Adapter Num of Overlays = {AdapterCapabilities.NumOverlays}");

                                // Obtain information about displays
                                ADLDisplayInfoArray displayInfoArray = new ADLDisplayInfoArray();

                                if (ADL.ADL2_Display_DisplayInfo_Get != null)
                                {
                                    IntPtr DisplayBuffer = IntPtr.Zero;

                                    // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                    ADLRet = ADL.ADL2_Display_DisplayInfo_Get(_adlContextHandle, oneAdapter.AdapterIndex, ref NumberOfDisplays, out DisplayBuffer, 0);
                                    if (ADLRet == ADL.ADL_OK)
                                    {

                                        try
                                        {
                                            displayInfoArray = (ADLDisplayInfoArray)Marshal.PtrToStructure(DisplayBuffer, displayInfoArray.GetType());

                                            foreach (ADLDisplayInfo oneDisplayInfo in displayInfoArray.ADLDisplayInfo)
                                            {

                                                if (oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex == -1)
                                                {
                                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not a real display as its DisplayID.DisplayLogicalAdapterIndex is -1");
                                                    continue;
                                                }

                                                // Convert the displayInfoValue to something usable using a library function I made
                                                ConvertedDisplayInfoValue displayInfoValue = ADL.ConvertDisplayInfoValue(oneDisplayInfo.DisplayInfoValue);

                                                if (!displayInfoValue.DISPLAYCONNECTED)
                                                {
                                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not connected");
                                                    continue;
                                                }

                                                // We want any display that is connected, but we don't care if they are mapped or not, as we'll be potentially changing the display profile

                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is connected and mapped in Windows OS");

                                                ADL.ADLDisplayConnectionType displayConnector = (ADL.ADLDisplayConnectionType)oneDisplayInfo.DisplayConnector;

                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Display Info for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Connector = {displayConnector.ToString("G")}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Controller Index = {oneDisplayInfo.DisplayControllerIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Logical Adapter Index = {oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Logical Index = {oneDisplayInfo.DisplayID.DisplayLogicalIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Physical Adapter Index = {oneDisplayInfo.DisplayID.DisplayPhysicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Physical Index = {oneDisplayInfo.DisplayID.DisplayPhysicalIndex}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Mask = {oneDisplayInfo.DisplayInfoMask}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value = {oneDisplayInfo.DisplayInfoValue}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Manufacturer Name = {oneDisplayInfo.DisplayManufacturerName}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Name = {oneDisplayInfo.DisplayName}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Output Type = {oneDisplayInfo.DisplayOutputType}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Type = {oneDisplayInfo.DisplayType}");

                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value DISPLAYCONNECTED = {displayInfoValue.DISPLAYCONNECTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value DISPLAYMAPPED = {displayInfoValue.DISPLAYMAPPED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value FORCIBLESUPPORTED = {displayInfoValue.FORCIBLESUPPORTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value GENLOCKSUPPORTED = {displayInfoValue.GENLOCKSUPPORTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value LDA_DISPLAY = {displayInfoValue.LDA_DISPLAY}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_2HSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2HSTRETCH}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_2VSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2VSTRETCH}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_CLONE = {displayInfoValue.MANNER_SUPPORTED_CLONE}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_EXTENDED = {displayInfoValue.MANNER_SUPPORTED_EXTENDED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_NSTRETCH1GPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCH1GPU}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_NSTRETCHNGPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCHNGPU}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_SINGLE = {displayInfoValue.MANNER_SUPPORTED_SINGLE}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MODETIMING_OVERRIDESSUPPORTED = {displayInfoValue.MODETIMING_OVERRIDESSUPPORTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value MULTIVPU_SUPPORTED = {displayInfoValue.MULTIVPU_SUPPORTED}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value NONLOCAL = {displayInfoValue.NONLOCAL}");
                                                SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Info Value SHOWTYPE_PROJECTOR = {displayInfoValue.SHOWTYPE_PROJECTOR}");

                                                ADL.ADLDisplayConnectionType displayConnectionType = ADL.ADLDisplayConnectionType.Unknown;
                                                ADLDisplayConfig displayConfig = new ADLDisplayConfig();
                                                displayConfig.Size = Marshal.SizeOf(displayConfig);
                                                if (ADL.ADL2_Display_DeviceConfig_Get != null)
                                                {
                                                    // Get the DisplayConfig from the Display
                                                    ADLRet = ADL.ADL2_Display_DeviceConfig_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID.DisplayPhysicalIndex, out displayConfig);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {
                                                        displayConnectionType = (ADL.ADLDisplayConnectionType)displayConfig.ConnectorType;

                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Display Device Config for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Connector Type = {displayConnectionType.ToString("G")}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Device Data = {displayConfig.DeviceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Overridded Device Data = {displayConfig.OverriddedDeviceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Reserved Data = {displayConfig.Reserved}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Size = {displayConfig.Size}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_DeviceConfig_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                                    }
                                                }

                                                ADLDDCInfo2 displayDDCInfo2 = new ADLDDCInfo2();
                                                displayDDCInfo2.Size = Marshal.SizeOf(displayDDCInfo2);
                                                // Create a stringbuilder buffer that EDID can be loaded into
                                                //displayEDIDData.EDIDData = new StringBuilder(256);

                                                if (ADL.ADL2_Display_DDCInfo2_Get != null)
                                                {
                                                    // Get the DDC Data from the Display
                                                    ADLRet = ADL.ADL2_Display_DDCInfo2_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID.DisplayPhysicalIndex, out displayDDCInfo2);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {

                                                        // Convert the DDCInfoFlag to something usable using a library function I made
                                                        ConvertedDDCInfoFlag DDCInfoFlag = ADL.ConvertDDCInfoFlag(displayDDCInfo2.DDCInfoFlag);

                                                        // Convert the DDCInfoFlag to something usable using a library function I made
                                                        ConvertedSupportedHDR supportedHDR = ADL.ConvertSupportedHDR(displayDDCInfo2.SupportedHDR);

                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Display DDCInfo2 for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display AvgLuminanceData = {displayDDCInfo2.AvgLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag = {displayDDCInfo2.DDCInfoFlag}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DiffuseScreenReflectance = {displayDDCInfo2.DiffuseScreenReflectance}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DisplayName = {displayDDCInfo2.DisplayName}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display FreesyncFlags = {displayDDCInfo2.FreesyncFlags}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display ManufacturerID = {displayDDCInfo2.ManufacturerID}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxBacklightMaxLuminanceData = {displayDDCInfo2.MaxBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxBacklightMinLuminanceData = {displayDDCInfo2.MaxBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxHResolution = {displayDDCInfo2.MaxHResolution}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxLuminanceData = {displayDDCInfo2.MaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxRefresh = {displayDDCInfo2.MaxRefresh}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MaxVResolution = {displayDDCInfo2.MaxVResolution}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MinBacklightMaxLuminanceData = {displayDDCInfo2.MinBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MinBacklightMinLuminanceData = {displayDDCInfo2.MinBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MinLuminanceData = {displayDDCInfo2.MinLuminanceData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display MinLuminanceNoDimmingData = {displayDDCInfo2.MinLuminanceNoDimmingData}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityBlueX = {displayDDCInfo2.NativeDisplayChromaticityBlueX}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityBlueY = {displayDDCInfo2.NativeDisplayChromaticityBlueY}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityGreenX = {displayDDCInfo2.NativeDisplayChromaticityGreenX}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityGreenY = {displayDDCInfo2.NativeDisplayChromaticityGreenY}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityRedX = {displayDDCInfo2.NativeDisplayChromaticityRedX}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityRedY = {displayDDCInfo2.NativeDisplayChromaticityRedY}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityWhiteX = {displayDDCInfo2.NativeDisplayChromaticityWhiteX}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityWhiteY = {displayDDCInfo2.NativeDisplayChromaticityWhiteY}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PackedPixelSupported = {displayDDCInfo2.PackedPixelSupported}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PanelPixelFormat = {displayDDCInfo2.PanelPixelFormat}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display ProductID = {displayDDCInfo2.ProductID}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PTMCx = {displayDDCInfo2.PTMCx}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PTMCy = {displayDDCInfo2.PTMCy}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display PTMRefreshRate = {displayDDCInfo2.PTMRefreshRate}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SerialID = {displayDDCInfo2.SerialID}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display Size = {displayDDCInfo2.Size}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SpecularScreenReflectance = {displayDDCInfo2.SpecularScreenReflectance}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedColorSpace = {displayDDCInfo2.SupportedColorSpace}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedHDR = {displayDDCInfo2.SupportedHDR}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedTransferFunction = {displayDDCInfo2.SupportedTransferFunction}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportsDDC = {displayDDCInfo2.SupportsDDC}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Digital Device  = {DDCInfoFlag.DIGITALDEVICE}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag EDID Extension = {DDCInfoFlag.EDIDEXTENSION}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag HDMI Audio Device  = {DDCInfoFlag.HDMIAUDIODEVICE}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Projector Device = {DDCInfoFlag.PROJECTORDEVICE}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Supports AI = {DDCInfoFlag.SUPPORTS_AI}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Supports xvYCC601 = {DDCInfoFlag.SUPPORT_xvYCC601}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Supports xvYCC709 = {DDCInfoFlag.SUPPORT_xvYCC709}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedHDR Supports CEA861_3 = {supportedHDR.CEA861_3}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedHDR Supports DOLBYVISION = {supportedHDR.DOLBYVISION}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display SupportedHDR Supports FREESYNC_HDR = {supportedHDR.FREESYNC_HDR}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_DDCInfo2_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                                    }
                                                }

                                                int HDRSupported = 0;
                                                int HDREnabled = 0;
                                                if (ADL.ADL2_Display_HDRState_Get != null)
                                                {
                                                    // Get the HDR State from the Display
                                                    ADLRet = ADL.ADL2_Display_HDRState_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID, out HDRSupported, out HDREnabled);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: ### Display HDR State for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display HDR Supported = {HDRSupported}");
                                                        SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Display HDR Enabled = {HDREnabled}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_HDRState_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                                    }
                                                }


                                                // Create an array of all the important display info we need to record
                                                List<string> displayInfoIdentifierSection = new List<string>();
                                                displayInfoIdentifierSection.Add("AMD");
                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.VendorID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Vendor ID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.AdapterName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Adapter Name from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.VendorID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD VendorID  from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("1002");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(AdapterCapabilities.AdapterID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD AdapterID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayConnector.ToString("G"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting AMD Display Connector from video card to display. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneDisplayInfo.DisplayName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Display Name from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ManufacturerID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Manufacturer ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ProductID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Product ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.SerialID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"ADLWrapper/GenerateProfileDisplayIdentifiers: Exception getting Serial ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                // Create a display identifier out of it
                                                string displayIdentifier = String.Join("|", displayInfoIdentifierSection);

                                                // Check first to see if there is already an existing display identifier the same!
                                                // This appears to be a bug with the AMD driver, or with the install on my test machine
                                                // Either way, it is potentially going to happen in the wild, so I will filter it out if it does
                                                if (displayIdentifiers.Contains(displayIdentifier))
                                                {
                                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: Your AMD driver reported the following Display Identifier multiple times, so ignoring it as we already have it: {displayIdentifier}");
                                                    continue;
                                                }

                                                // Add it to the list of display identifiers so we can return it
                                                displayIdentifiers.Add(displayIdentifier);

                                                SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateProfileDisplayIdentifiers: Exception caused trying to access attached displays");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_DisplayInfo_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                    }
                                    // Release the memory for the DisplayInfo structure
                                    if (IntPtr.Zero != DisplayBuffer)
                                        Marshal.FreeCoTaskMem(DisplayBuffer);
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Adapter_Active_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                            }
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: Error running ADL2_Adapter_AdapterInfoX4_Get on AMD Video card: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                    }
                }
                // Release the memory for the AdapterInfo structure
                if (IntPtr.Zero != AdapterBuffer)
                {
                    Marshal.FreeCoTaskMem(AdapterBuffer);
                }

                // Return all the identifiers we've found
                return displayIdentifiers;
            }
            else
            {
                SharedLogger.logger.Warn($"ADLWrapper/GenerateProfileDisplayIdentifiers: There were no AMD adapters found by AMD ADL.");
                return null;
            }
        }
    }
}
