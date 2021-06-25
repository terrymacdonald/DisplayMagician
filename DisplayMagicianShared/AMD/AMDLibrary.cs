using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ATI.ADL;
using Microsoft.Win32.SafeHandles;

namespace DisplayMagicianShared.AMD
{
    public  class AMDLibrary : IDisposable
    {
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static AMDLibrary _instance = new AMDLibrary();

        private bool _initialised = false;

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        private IntPtr _adlContextHandle = IntPtr.Zero;

        // Struct to be used as the AMD Profile
        public struct AMDProfile
        {
            public List<AMDAdapter> Adapters;
        }

        // Struct to store the Display
        public struct AMDAdapter
        {
            internal ADLAdapterInfoX2 AdapterInfoX2;
            internal List<AMDDisplay> Displays;
        }

        // Struct to store the Display
        public struct AMDDisplay
        {
            internal List<ADLMode> DisplayModes;
        }


        static AMDLibrary() { }
        public AMDLibrary()
        {
            int ADLRet = ADL.ADL_ERR;

            SharedLogger.logger.Trace("AMDLibrary/AMDLibrary: Intialising ADL2 library interface");
            try
            {
                if (ADL.ADL2_Main_Control_Create != null)
                {
                    // Second parameter is 1: Get only the present adapters
                    ADLRet = ADL.ADL2_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 1, out _adlContextHandle);
                }

                if (ADLRet == ADL.ADL_OK)
                {
                    _initialised = true;
                    SharedLogger.logger.Trace("AMDLibrary/AMDLibrary: ADL2 library was initialised successfully");
                }
                else
                {
                    SharedLogger.logger.Error("AMDLibrary/AMDLibrary: Error intialising ADL2 library. ADL2_Main_Control_Create() returned error code " + ADL.ConvertADLReturnValueIntoWords(ADLRet));
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "AMDLibrary/AMDLibrary: Exception intialising ADL2 library. ADL2_Main_Control_Create() caused an exception");
            }

        }

        ~AMDLibrary()
        {
            // If the ADL2 library was initialised, then we need to free it up.
            if (_initialised)
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
            get { return _initialised; }
        }

        public static AMDLibrary GetLibrary()
        {
            return _instance;
        }

        public List<string> GenerateProfileDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Getting AMD active adapter count");

            int ADLRet = ADL.ADL_ERR;
            int NumberOfAdapters = 0;

            List<string> displayIdentifiers = new List<string>();

            if (null != ADL.ADL2_Adapter_NumberOfAdapters_Get)
            {
                ADL.ADL2_Adapter_NumberOfAdapters_Get(_adlContextHandle, ref NumberOfAdapters);
                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Number Of Adapters: {NumberOfAdapters.ToString()} ");
            }

            if (NumberOfAdapters > 0)
            {

                IntPtr AdapterBuffer = IntPtr.Zero;
                if (ADL.ADL2_Adapter_AdapterInfoX4_Get != null)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: ADL2_Adapter_AdapterInfoX4_Get DLL function exists.");

                    // Get the Adapter info and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Running ADL2_Adapter_AdapterInfoX4_Get to find all known AMD adapters.");
                    //ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, AdapterBuffer, size);
                    int numAdapters = 0;
                    ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, ADL.ADL_ADAPTER_INDEX_ALL, out numAdapters, out AdapterBuffer);
                    if (ADLRet == ADL.ADL_OK)
                    {                        

                        int IsActive = ADL.ADL_TRUE; // We only want to search for active adapters

                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Successfully run ADL2_Adapter_AdapterInfoX4_Get to find information about all known AMD adapters.");

                        ADLAdapterInfoX2 oneAdapter = new ADLAdapterInfoX2();
                        // Go through each adapter
                        for (int adapterLoop = 0; adapterLoop < numAdapters; adapterLoop++)
                        {
                            oneAdapter = (ADLAdapterInfoX2)Marshal.PtrToStructure(new IntPtr(AdapterBuffer.ToInt64() + (adapterLoop * Marshal.SizeOf(oneAdapter))), oneAdapter.GetType());

                            if (oneAdapter.Exist != 1)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                                continue;
                            }

                            if (oneAdapter.Present != 1)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
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
                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't active ({oneAdapter.AdapterName}).");
                                    continue;
                                }

                                // Only continue if the adapter index is > 0
                                if (oneAdapter.AdapterIndex < 0)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: AMD Adapter has an adapter index of {oneAdapter.AdapterIndex.ToString()} which indicates it is not a real adapter.");
                                    continue;
                                }

                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} is active! ({oneAdapter.AdapterName}).");

                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: ### Adapter Info for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter AdapterIndex = {oneAdapter.AdapterIndex}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter AdapterName = {oneAdapter.AdapterName}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter BusNumber = {oneAdapter.BusNumber}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter DeviceNumber = {oneAdapter.DeviceNumber}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter DisplayName = {oneAdapter.DisplayName}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter DriverPath = {oneAdapter.DriverPath}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter DriverPathExt = {oneAdapter.DriverPathExt}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Exist = {oneAdapter.Exist}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter FunctionNumber = {oneAdapter.FunctionNumber}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter InfoMask = {oneAdapter.InfoMask}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter InfoValue = {oneAdapter.InfoValue}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter OSDisplayIndex = {oneAdapter.OSDisplayIndex}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter PNPString = {oneAdapter.PNPString}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Present = {oneAdapter.Present}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Size = {oneAdapter.Size}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter UDID = {oneAdapter.UDID}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter VendorID = {oneAdapter.VendorID}");

                                // Get the Adapter Capabilities
                                ADLAdapterCapsX2 AdapterCapabilities = new ADLAdapterCapsX2();
                                if (ADL.ADL2_AdapterX2_Caps != null)
                                {
                                    ADLRet = ADL.ADL2_AdapterX2_Caps(_adlContextHandle, oneAdapter.AdapterIndex, out AdapterCapabilities);
                                }

                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: ### Adapter Capabilities for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter ID = {AdapterCapabilities.AdapterID}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Capabilities Mask = {AdapterCapabilities.CapsMask}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Capabilities Value = {AdapterCapabilities.CapsValue}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Num of Connectors = {AdapterCapabilities.NumConnectors}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Num of Controllers = {AdapterCapabilities.NumControllers}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Num of Displays = {AdapterCapabilities.NumDisplays}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Num of GL Sync Connectors = {AdapterCapabilities.NumOfGLSyncConnectors}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Adapter Num of Overlays = {AdapterCapabilities.NumOverlays}");

                                // Obtain information about displays
                                //ADLDisplayInfoArray displayInfoArray = new ADLDisplayInfoArray();

                                if (ADL.ADL2_Display_DisplayInfo_Get != null)
                                {
                                    IntPtr DisplayBuffer = IntPtr.Zero;
                                    int numDisplays = 0;
                                    // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                    ADLRet = ADL.ADL2_Display_DisplayInfo_Get(_adlContextHandle, oneAdapter.AdapterIndex, ref numDisplays, out DisplayBuffer, 0);
                                    if (ADLRet == ADL.ADL_OK)
                                    {

                                        try
                                        {
                                            ADLDisplayInfo oneDisplayInfo = new ADLDisplayInfo();

                                            for (int displayLoop = 0; displayLoop < numDisplays; displayLoop++)
                                            {
                                                oneDisplayInfo = (ADLDisplayInfo)Marshal.PtrToStructure(new IntPtr(DisplayBuffer.ToInt64() + (displayLoop * Marshal.SizeOf(oneDisplayInfo))), oneDisplayInfo.GetType());

                                                // Is the display mapped to this adapter? If not we skip it!
                                                if (oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex != oneAdapter.AdapterIndex)
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not a real display as its DisplayID.DisplayLogicalAdapterIndex is -1");
                                                    continue;
                                                }

                                                // Convert the displayInfoValue to something usable using a library function I made
                                                ConvertedDisplayInfoValue displayInfoValue = ADL.ConvertDisplayInfoValue(oneDisplayInfo.DisplayInfoValue);

                                                if (!displayInfoValue.DISPLAYCONNECTED)
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not connected");
                                                    continue;
                                                }

                                                // Skip connected but non-mapped displays (not mapped in windows) - we want all displays currently visible in the OS
                                                if (!displayInfoValue.DISPLAYMAPPED)
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not mapped in Windows OS");
                                                    continue;
                                                }

                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is connected and mapped in Windows OS");

                                                ADL.ADLDisplayConnectionType displayConnector = (ADL.ADLDisplayConnectionType)oneDisplayInfo.DisplayConnector;

                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: ### Display Info for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Connector = {displayConnector.ToString("G")}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Controller Index = {oneDisplayInfo.DisplayControllerIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Logical Adapter Index = {oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Logical Index = {oneDisplayInfo.DisplayID.DisplayLogicalIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Physical Adapter Index = {oneDisplayInfo.DisplayID.DisplayPhysicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Physical Index = {oneDisplayInfo.DisplayID.DisplayPhysicalIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Mask = {oneDisplayInfo.DisplayInfoMask}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value = {oneDisplayInfo.DisplayInfoValue}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Manufacturer Name = {oneDisplayInfo.DisplayManufacturerName}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Name = {oneDisplayInfo.DisplayName}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Output Type = {oneDisplayInfo.DisplayOutputType}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Type = {oneDisplayInfo.DisplayType}");

                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value DISPLAYCONNECTED = {displayInfoValue.DISPLAYCONNECTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value DISPLAYMAPPED = {displayInfoValue.DISPLAYMAPPED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value FORCIBLESUPPORTED = {displayInfoValue.FORCIBLESUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value GENLOCKSUPPORTED = {displayInfoValue.GENLOCKSUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value LDA_DISPLAY = {displayInfoValue.LDA_DISPLAY}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_2HSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2HSTRETCH}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_2VSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2VSTRETCH}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_CLONE = {displayInfoValue.MANNER_SUPPORTED_CLONE}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_EXTENDED = {displayInfoValue.MANNER_SUPPORTED_EXTENDED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_NSTRETCH1GPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCH1GPU}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_NSTRETCHNGPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCHNGPU}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_SINGLE = {displayInfoValue.MANNER_SUPPORTED_SINGLE}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value MODETIMING_OVERRIDESSUPPORTED = {displayInfoValue.MODETIMING_OVERRIDESSUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value MULTIVPU_SUPPORTED = {displayInfoValue.MULTIVPU_SUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value NONLOCAL = {displayInfoValue.NONLOCAL}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Info Value SHOWTYPE_PROJECTOR = {displayInfoValue.SHOWTYPE_PROJECTOR}");

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

                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: ### Display Device Config for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Connector Type = {displayConnectionType.ToString("G")}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Device Data = {displayConfig.DeviceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Overridded Device Data = {displayConfig.OverriddedDeviceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Reserved Data = {displayConfig.Reserved}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Size = {displayConfig.Size}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"AMDLibrary/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_DeviceConfig_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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

                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: ### Display DDCInfo2 for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display AvgLuminanceData = {displayDDCInfo2.AvgLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag = {displayDDCInfo2.DDCInfoFlag}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DiffuseScreenReflectance = {displayDDCInfo2.DiffuseScreenReflectance}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DisplayName = {displayDDCInfo2.DisplayName}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display FreesyncFlags = {displayDDCInfo2.FreesyncFlags}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display ManufacturerID = {displayDDCInfo2.ManufacturerID}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MaxBacklightMaxLuminanceData = {displayDDCInfo2.MaxBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MaxBacklightMinLuminanceData = {displayDDCInfo2.MaxBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MaxHResolution = {displayDDCInfo2.MaxHResolution}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MaxLuminanceData = {displayDDCInfo2.MaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MaxRefresh = {displayDDCInfo2.MaxRefresh}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MaxVResolution = {displayDDCInfo2.MaxVResolution}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MinBacklightMaxLuminanceData = {displayDDCInfo2.MinBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MinBacklightMinLuminanceData = {displayDDCInfo2.MinBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MinLuminanceData = {displayDDCInfo2.MinLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display MinLuminanceNoDimmingData = {displayDDCInfo2.MinLuminanceNoDimmingData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityBlueX = {displayDDCInfo2.NativeDisplayChromaticityBlueX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityBlueY = {displayDDCInfo2.NativeDisplayChromaticityBlueY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityGreenX = {displayDDCInfo2.NativeDisplayChromaticityGreenX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityGreenY = {displayDDCInfo2.NativeDisplayChromaticityGreenY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityRedX = {displayDDCInfo2.NativeDisplayChromaticityRedX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityRedY = {displayDDCInfo2.NativeDisplayChromaticityRedY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityWhiteX = {displayDDCInfo2.NativeDisplayChromaticityWhiteX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display NativeDisplayChromaticityWhiteY = {displayDDCInfo2.NativeDisplayChromaticityWhiteY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display PackedPixelSupported = {displayDDCInfo2.PackedPixelSupported}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display PanelPixelFormat = {displayDDCInfo2.PanelPixelFormat}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display ProductID = {displayDDCInfo2.ProductID}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display PTMCx = {displayDDCInfo2.PTMCx}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display PTMCy = {displayDDCInfo2.PTMCy}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display PTMRefreshRate = {displayDDCInfo2.PTMRefreshRate}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display SerialID = {displayDDCInfo2.SerialID}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display Size = {displayDDCInfo2.Size}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display SpecularScreenReflectance = {displayDDCInfo2.SpecularScreenReflectance}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display SupportedColorSpace = {displayDDCInfo2.SupportedColorSpace}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display SupportedHDR = {displayDDCInfo2.SupportedHDR}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display SupportedTransferFunction = {displayDDCInfo2.SupportedTransferFunction}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display SupportsDDC = {displayDDCInfo2.SupportsDDC}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Digital Device  = {DDCInfoFlag.DIGITALDEVICE}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag EDID Extension = {DDCInfoFlag.EDIDEXTENSION}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag HDMI Audio Device  = {DDCInfoFlag.HDMIAUDIODEVICE}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Projector Device = {DDCInfoFlag.PROJECTORDEVICE}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Supports AI = {DDCInfoFlag.SUPPORTS_AI}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Supports xvYCC601 = {DDCInfoFlag.SUPPORT_xvYCC601}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display DDCInfoFlag Supports xvYCC709 = {DDCInfoFlag.SUPPORT_xvYCC709}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display SupportedHDR Supports CEA861_3 = {supportedHDR.CEA861_3}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display SupportedHDR Supports DOLBYVISION = {supportedHDR.DOLBYVISION}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display SupportedHDR Supports FREESYNC_HDR = {supportedHDR.FREESYNC_HDR}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"AMDLibrary/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_DDCInfo2_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: ### Display HDR State for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display HDR Supported = {HDRSupported}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Display HDR Enabled = {HDREnabled}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"AMDLibrary/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_HDRState_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateProfileDisplayIdentifiers: Exception getting AMD Vendor ID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.AdapterName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateProfileDisplayIdentifiers: Exception getting AMD Adapter Name from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.VendorID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateProfileDisplayIdentifiers: Exception getting AMD VendorID  from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("1002");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(AdapterCapabilities.AdapterID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateProfileDisplayIdentifiers: Exception getting AMD AdapterID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayConnector.ToString("G"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateProfileDisplayIdentifiers: Exception getting AMD Display Connector from video card to display. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneDisplayInfo.DisplayName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateProfileDisplayIdentifiers: Exception getting Display Name from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ManufacturerID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateProfileDisplayIdentifiers: Exception getting Manufacturer ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ProductID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateProfileDisplayIdentifiers: Exception getting Product ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.SerialID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateProfileDisplayIdentifiers: Exception getting Serial ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                // Create a display identifier out of it
                                                string displayIdentifier = String.Join("|", displayInfoIdentifierSection);

                                                // Check first to see if there is already an existing display identifier the same!
                                                // This appears to be a bug with the AMD driver, or with the install on my test machine
                                                // Either way, it is potentially going to happen in the wild, so I will filter it out if it does
                                                if (displayIdentifiers.Contains(displayIdentifier))
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateProfileDisplayIdentifiers: Your AMD driver reported the following Display Identifier multiple times, so ignoring it as we already have it: {displayIdentifier}");
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
                                        SharedLogger.logger.Warn($"AMDLibrary/GenerateProfileDisplayIdentifiers: Error running ADL2_Display_DisplayInfo_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                    }
                                    // Release the memory for the DisplayInfo structure
                                    if (IntPtr.Zero != DisplayBuffer)
                                        Marshal.FreeCoTaskMem(DisplayBuffer);
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"AMDLibrary/GenerateProfileDisplayIdentifiers: Error running ADL2_Adapter_Active_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                            }
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"AMDLibrary/GenerateProfileDisplayIdentifiers: Error running ADL2_Adapter_AdapterInfoX4_Get on AMD Video card: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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
                SharedLogger.logger.Warn($"AMDLibrary/GenerateProfileDisplayIdentifiers: There were no AMD adapters found by AMD ADL.");
                return null;
            }
        }

        public List<string> GenerateAllAvailableDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Getting AMD active adapter count");

            int ADLRet = ADL.ADL_ERR;
            int NumberOfAdapters = 0;

            List<string> displayIdentifiers = new List<string>();

            if (null != ADL.ADL2_Adapter_NumberOfAdapters_Get)
            {
                ADL.ADL2_Adapter_NumberOfAdapters_Get(_adlContextHandle, ref NumberOfAdapters);
                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Number Of Adapters: {NumberOfAdapters.ToString()} ");
            }

            if (NumberOfAdapters > 0)
            {

                IntPtr AdapterBuffer = IntPtr.Zero;
                if (ADL.ADL2_Adapter_AdapterInfoX4_Get != null)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: ADL2_Adapter_AdapterInfoX4_Get DLL function exists.");

                    // Get the Adapter info and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Running ADL2_Adapter_AdapterInfoX4_Get to find all known AMD adapters.");
                    //ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, AdapterBuffer, size);
                    int numAdapters = 0;
                    ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, ADL.ADL_ADAPTER_INDEX_ALL, out numAdapters, out AdapterBuffer);
                    if (ADLRet == ADL.ADL_OK)
                    {

                        int IsActive = ADL.ADL_TRUE; // We only want to search for active adapters

                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Successfully run ADL2_Adapter_AdapterInfoX4_Get to find information about all known AMD adapters.");

                        ADLAdapterInfoX2 oneAdapter = new ADLAdapterInfoX2();
                        // Go through each adapter
                        for (int adapterLoop = 0; adapterLoop < numAdapters; adapterLoop++)
                        {
                            oneAdapter = (ADLAdapterInfoX2)Marshal.PtrToStructure(new IntPtr(AdapterBuffer.ToInt64() + (adapterLoop * Marshal.SizeOf(oneAdapter))), oneAdapter.GetType());

                            if (oneAdapter.Exist != 1)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                                continue;
                            }

                            if (oneAdapter.Present != 1)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
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
                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't active ({oneAdapter.AdapterName}).");
                                    continue;
                                }

                                // Only continue if the adapter index is > 0
                                if (oneAdapter.AdapterIndex < 0)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: AMD Adapter has an adapter index of {oneAdapter.AdapterIndex.ToString()} which indicates it is not a real adapter.");
                                    continue;
                                }

                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} is active! ({oneAdapter.AdapterName}).");

                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: ### Adapter Info for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter AdapterIndex = {oneAdapter.AdapterIndex}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter AdapterName = {oneAdapter.AdapterName}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter BusNumber = {oneAdapter.BusNumber}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter DeviceNumber = {oneAdapter.DeviceNumber}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter DisplayName = {oneAdapter.DisplayName}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter DriverPath = {oneAdapter.DriverPath}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter DriverPathExt = {oneAdapter.DriverPathExt}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Exist = {oneAdapter.Exist}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter FunctionNumber = {oneAdapter.FunctionNumber}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter InfoMask = {oneAdapter.InfoMask}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter InfoValue = {oneAdapter.InfoValue}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter OSDisplayIndex = {oneAdapter.OSDisplayIndex}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter PNPString = {oneAdapter.PNPString}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Present = {oneAdapter.Present}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Size = {oneAdapter.Size}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter UDID = {oneAdapter.UDID}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter VendorID = {oneAdapter.VendorID}");

                                // Get the Adapter Capabilities
                                ADLAdapterCapsX2 AdapterCapabilities = new ADLAdapterCapsX2();
                                if (ADL.ADL2_AdapterX2_Caps != null)
                                {
                                    ADLRet = ADL.ADL2_AdapterX2_Caps(_adlContextHandle, oneAdapter.AdapterIndex, out AdapterCapabilities);
                                }

                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: ### Adapter Capabilities for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter ID = {AdapterCapabilities.AdapterID}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Capabilities Mask = {AdapterCapabilities.CapsMask}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Capabilities Value = {AdapterCapabilities.CapsValue}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Num of Connectors = {AdapterCapabilities.NumConnectors}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Num of Controllers = {AdapterCapabilities.NumControllers}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Num of Displays = {AdapterCapabilities.NumDisplays}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Num of GL Sync Connectors = {AdapterCapabilities.NumOfGLSyncConnectors}");
                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Adapter Num of Overlays = {AdapterCapabilities.NumOverlays}");

                                if (ADL.ADL2_Display_DisplayInfo_Get != null)
                                {
                                    IntPtr DisplayBuffer = IntPtr.Zero;
                                    int numDisplays = 0;
                                    // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                    ADLRet = ADL.ADL2_Display_DisplayInfo_Get(_adlContextHandle, oneAdapter.AdapterIndex, ref numDisplays, out DisplayBuffer, 1);
                                    if (ADLRet == ADL.ADL_OK)
                                    {

                                        try
                                        {
                                            ADLDisplayInfo oneDisplayInfo = new ADLDisplayInfo();

                                            for (int displayLoop = 0; displayLoop < numDisplays; displayLoop++)
                                            {
                                                oneDisplayInfo = (ADLDisplayInfo)Marshal.PtrToStructure(new IntPtr(DisplayBuffer.ToInt64() + (displayLoop * Marshal.SizeOf(oneDisplayInfo))), oneDisplayInfo.GetType());

                                                // Is the display mapped to this adapter? If not we skip it!
                                                if (oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex != oneAdapter.AdapterIndex)
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not a real display as its DisplayID.DisplayLogicalAdapterIndex is -1");
                                                    continue;
                                                }

                                                // Convert the displayInfoValue to something usable using a library function I made
                                                ConvertedDisplayInfoValue displayInfoValue = ADL.ConvertDisplayInfoValue(oneDisplayInfo.DisplayInfoValue);

                                                // Is the display mapped to this adapter? If not we skip it!
                                                if (!displayInfoValue.DISPLAYCONNECTED)
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not connected");
                                                    continue;
                                                }

                                                // We want connected displays  whether they are mapped or not mapped in Windows OS

                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is connected and mapped in Windows OS");

                                                ADL.ADLDisplayConnectionType displayConnector = (ADL.ADLDisplayConnectionType)oneDisplayInfo.DisplayConnector;

                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: ### Display Info for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Connector = {displayConnector.ToString("G")}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Controller Index = {oneDisplayInfo.DisplayControllerIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Logical Adapter Index = {oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Logical Index = {oneDisplayInfo.DisplayID.DisplayLogicalIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Physical Adapter Index = {oneDisplayInfo.DisplayID.DisplayPhysicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Physical Index = {oneDisplayInfo.DisplayID.DisplayPhysicalIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Mask = {oneDisplayInfo.DisplayInfoMask}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value = {oneDisplayInfo.DisplayInfoValue}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Manufacturer Name = {oneDisplayInfo.DisplayManufacturerName}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Name = {oneDisplayInfo.DisplayName}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Output Type = {oneDisplayInfo.DisplayOutputType}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Type = {oneDisplayInfo.DisplayType}");

                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value DISPLAYCONNECTED = {displayInfoValue.DISPLAYCONNECTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value DISPLAYMAPPED = {displayInfoValue.DISPLAYMAPPED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value FORCIBLESUPPORTED = {displayInfoValue.FORCIBLESUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value GENLOCKSUPPORTED = {displayInfoValue.GENLOCKSUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value LDA_DISPLAY = {displayInfoValue.LDA_DISPLAY}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_2HSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2HSTRETCH}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_2VSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2VSTRETCH}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_CLONE = {displayInfoValue.MANNER_SUPPORTED_CLONE}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_EXTENDED = {displayInfoValue.MANNER_SUPPORTED_EXTENDED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_NSTRETCH1GPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCH1GPU}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_NSTRETCHNGPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCHNGPU}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value MANNER_SUPPORTED_SINGLE = {displayInfoValue.MANNER_SUPPORTED_SINGLE}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value MODETIMING_OVERRIDESSUPPORTED = {displayInfoValue.MODETIMING_OVERRIDESSUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value MULTIVPU_SUPPORTED = {displayInfoValue.MULTIVPU_SUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value NONLOCAL = {displayInfoValue.NONLOCAL}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Info Value SHOWTYPE_PROJECTOR = {displayInfoValue.SHOWTYPE_PROJECTOR}");

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

                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: ### Display Device Config for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Connector Type = {displayConnectionType.ToString("G")}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Device Data = {displayConfig.DeviceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Overridded Device Data = {displayConfig.OverriddedDeviceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Reserved Data = {displayConfig.Reserved}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Size = {displayConfig.Size}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Error running ADL2_Display_DeviceConfig_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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

                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: ### Display DDCInfo2 for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display AvgLuminanceData = {displayDDCInfo2.AvgLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DDCInfoFlag = {displayDDCInfo2.DDCInfoFlag}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DiffuseScreenReflectance = {displayDDCInfo2.DiffuseScreenReflectance}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DisplayName = {displayDDCInfo2.DisplayName}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display FreesyncFlags = {displayDDCInfo2.FreesyncFlags}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display ManufacturerID = {displayDDCInfo2.ManufacturerID}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MaxBacklightMaxLuminanceData = {displayDDCInfo2.MaxBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MaxBacklightMinLuminanceData = {displayDDCInfo2.MaxBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MaxHResolution = {displayDDCInfo2.MaxHResolution}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MaxLuminanceData = {displayDDCInfo2.MaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MaxRefresh = {displayDDCInfo2.MaxRefresh}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MaxVResolution = {displayDDCInfo2.MaxVResolution}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MinBacklightMaxLuminanceData = {displayDDCInfo2.MinBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MinBacklightMinLuminanceData = {displayDDCInfo2.MinBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MinLuminanceData = {displayDDCInfo2.MinLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display MinLuminanceNoDimmingData = {displayDDCInfo2.MinLuminanceNoDimmingData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display NativeDisplayChromaticityBlueX = {displayDDCInfo2.NativeDisplayChromaticityBlueX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display NativeDisplayChromaticityBlueY = {displayDDCInfo2.NativeDisplayChromaticityBlueY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display NativeDisplayChromaticityGreenX = {displayDDCInfo2.NativeDisplayChromaticityGreenX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display NativeDisplayChromaticityGreenY = {displayDDCInfo2.NativeDisplayChromaticityGreenY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display NativeDisplayChromaticityRedX = {displayDDCInfo2.NativeDisplayChromaticityRedX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display NativeDisplayChromaticityRedY = {displayDDCInfo2.NativeDisplayChromaticityRedY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display NativeDisplayChromaticityWhiteX = {displayDDCInfo2.NativeDisplayChromaticityWhiteX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display NativeDisplayChromaticityWhiteY = {displayDDCInfo2.NativeDisplayChromaticityWhiteY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display PackedPixelSupported = {displayDDCInfo2.PackedPixelSupported}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display PanelPixelFormat = {displayDDCInfo2.PanelPixelFormat}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display ProductID = {displayDDCInfo2.ProductID}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display PTMCx = {displayDDCInfo2.PTMCx}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display PTMCy = {displayDDCInfo2.PTMCy}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display PTMRefreshRate = {displayDDCInfo2.PTMRefreshRate}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display SerialID = {displayDDCInfo2.SerialID}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display Size = {displayDDCInfo2.Size}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display SpecularScreenReflectance = {displayDDCInfo2.SpecularScreenReflectance}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display SupportedColorSpace = {displayDDCInfo2.SupportedColorSpace}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display SupportedHDR = {displayDDCInfo2.SupportedHDR}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display SupportedTransferFunction = {displayDDCInfo2.SupportedTransferFunction}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display SupportsDDC = {displayDDCInfo2.SupportsDDC}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DDCInfoFlag Digital Device  = {DDCInfoFlag.DIGITALDEVICE}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DDCInfoFlag EDID Extension = {DDCInfoFlag.EDIDEXTENSION}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DDCInfoFlag HDMI Audio Device  = {DDCInfoFlag.HDMIAUDIODEVICE}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DDCInfoFlag Projector Device = {DDCInfoFlag.PROJECTORDEVICE}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DDCInfoFlag Supports AI = {DDCInfoFlag.SUPPORTS_AI}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DDCInfoFlag Supports xvYCC601 = {DDCInfoFlag.SUPPORT_xvYCC601}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display DDCInfoFlag Supports xvYCC709 = {DDCInfoFlag.SUPPORT_xvYCC709}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display SupportedHDR Supports CEA861_3 = {supportedHDR.CEA861_3}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display SupportedHDR Supports DOLBYVISION = {supportedHDR.DOLBYVISION}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display SupportedHDR Supports FREESYNC_HDR = {supportedHDR.FREESYNC_HDR}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Error running ADL2_Display_DDCInfo2_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: ### Display HDR State for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display HDR Supported = {HDRSupported}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Display HDR Enabled = {HDREnabled}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Error running ADL2_Display_HDRState_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Exception getting AMD Vendor ID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.AdapterName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Exception getting AMD Adapter Name from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.VendorID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Exception getting AMD VendorID  from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("1002");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(AdapterCapabilities.AdapterID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Exception getting AMD AdapterID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayConnector.ToString("G"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Exception getting AMD Display Connector from video card to display. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneDisplayInfo.DisplayName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Exception getting Display Name from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ManufacturerID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Exception getting Manufacturer ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ProductID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Exception getting Product ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.SerialID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Exception getting Serial ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                // Create a display identifier out of it
                                                string displayIdentifier = String.Join("|", displayInfoIdentifierSection);

                                                // Check first to see if there is already an existing display identifier the same!
                                                // This appears to be a bug with the AMD driver, or with the install on my test machine
                                                // Either way, it is potentially going to happen in the wild, so I will filter it out if it does
                                                if (displayIdentifiers.Contains(displayIdentifier))
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Your AMD driver reported the following Display Identifier multiple times, so ignoring it as we already have it: {displayIdentifier}");
                                                    continue;
                                                }

                                                // Add it to the list of display identifiers so we can return it
                                                displayIdentifiers.Add(displayIdentifier);

                                                SharedLogger.logger.Debug($"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GenerateAllAvailableDisplayIdentifiers: Exception caused trying to access attached displays");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Error running ADL2_Display_DisplayInfo_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                    }
                                    // Release the memory for the DisplayInfo structure
                                    if (IntPtr.Zero != DisplayBuffer)
                                        Marshal.FreeCoTaskMem(DisplayBuffer);
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Error running ADL2_Adapter_Active_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                            }
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: Error running ADL2_Adapter_AdapterInfoX4_Get on AMD Video card: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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
                SharedLogger.logger.Warn($"AMDLibrary/GenerateAllAvailableDisplayIdentifiers: There were no AMD adapters found by AMD ADL.");
                return null;
            }
        }

        public AMDProfile GetActiveProfile()
        {
            SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Getting AMD active adapter count");

            int ADLRet = ADL.ADL_ERR;
            int NumberOfAdapters = 0;

            List<string> displayIdentifiers = new List<string>();
            AMDProfile profileToCreate = new AMDProfile();
            

            if (null != ADL.ADL2_Adapter_NumberOfAdapters_Get)
            {
                ADL.ADL2_Adapter_NumberOfAdapters_Get(_adlContextHandle, ref NumberOfAdapters);
                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Number Of Adapters: {NumberOfAdapters.ToString()} ");
            }

            if (NumberOfAdapters > 0)
            {
                profileToCreate.Adapters = new List<AMDAdapter>();
                IntPtr AdapterBuffer = IntPtr.Zero;
                if (ADL.ADL2_Adapter_AdapterInfoX4_Get != null)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: ADL2_Adapter_AdapterInfoX4_Get DLL function exists.");

                    // Get the Adapter info and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Running ADL2_Adapter_AdapterInfoX4_Get to find all known AMD adapters.");
                    //ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, AdapterBuffer, size);
                    int numAdapters = 0;
                    ADLRet = ADL.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, ADL.ADL_ADAPTER_INDEX_ALL, out numAdapters, out AdapterBuffer);
                    if (ADLRet == ADL.ADL_OK)
                    {

                        int IsActive = ADL.ADL_TRUE; // We only want to search for active adapters

                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Successfully run ADL2_Adapter_AdapterInfoX4_Get to find information about all known AMD adapters.");

                        ADLAdapterInfoX2 oneAdapter = new ADLAdapterInfoX2();
                        // Go through each adapter
                        for (int adapterLoop = 0; adapterLoop < numAdapters; adapterLoop++)
                        {
                            oneAdapter = (ADLAdapterInfoX2)Marshal.PtrToStructure(new IntPtr(AdapterBuffer.ToInt64() + (adapterLoop * Marshal.SizeOf(oneAdapter))), oneAdapter.GetType());

                            if (oneAdapter.Exist != 1)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                                continue;
                            }

                            if (oneAdapter.Present != 1)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
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
                                    SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't active ({oneAdapter.AdapterName}).");
                                    continue;
                                }

                                // Only continue if the adapter index is > 0
                                if (oneAdapter.AdapterIndex < 0)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/GenerateAllAGetActiveProfilevailableDisplayIdentifiers: AMD Adapter has an adapter index of {oneAdapter.AdapterIndex.ToString()} which indicates it is not a real adapter.");
                                    continue;
                                }

                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} is active! ({oneAdapter.AdapterName}).");

                                // Store the Adapter information for later
                                AMDAdapter adapterToCreate = new AMDAdapter();
                                adapterToCreate.AdapterInfoX2 = oneAdapter;
                                adapterToCreate.Displays = new List<AMDDisplay>();


                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: ### Adapter Info for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter AdapterIndex = {oneAdapter.AdapterIndex}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter AdapterName = {oneAdapter.AdapterName}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter BusNumber = {oneAdapter.BusNumber}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter DeviceNumber = {oneAdapter.DeviceNumber}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter DisplayName = {oneAdapter.DisplayName}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter DriverPath = {oneAdapter.DriverPath}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter DriverPathExt = {oneAdapter.DriverPathExt}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Exist = {oneAdapter.Exist}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter FunctionNumber = {oneAdapter.FunctionNumber}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter InfoMask = {oneAdapter.InfoMask}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter InfoValue = {oneAdapter.InfoValue}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter OSDisplayIndex = {oneAdapter.OSDisplayIndex}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter PNPString = {oneAdapter.PNPString}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Present = {oneAdapter.Present}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Size = {oneAdapter.Size}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter UDID = {oneAdapter.UDID}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter VendorID = {oneAdapter.VendorID}");

                                // Get the Adapter Capabilities
                                ADLAdapterCapsX2 AdapterCapabilities = new ADLAdapterCapsX2();
                                if (ADL.ADL2_AdapterX2_Caps != null)
                                {
                                    ADLRet = ADL.ADL2_AdapterX2_Caps(_adlContextHandle, oneAdapter.AdapterIndex, out AdapterCapabilities);
                                }

                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: ### Adapter Capabilities for Adapter #{oneAdapter.AdapterIndex} ###");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter ID = {AdapterCapabilities.AdapterID}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Capabilities Mask = {AdapterCapabilities.CapsMask}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Capabilities Value = {AdapterCapabilities.CapsValue}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Num of Connectors = {AdapterCapabilities.NumConnectors}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Num of Controllers = {AdapterCapabilities.NumControllers}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Num of Displays = {AdapterCapabilities.NumDisplays}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Num of GL Sync Connectors = {AdapterCapabilities.NumOfGLSyncConnectors}");
                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: Adapter Num of Overlays = {AdapterCapabilities.NumOverlays}");

                                // Store the Adapters Info for later
                                profileToCreate.Adapters.Add(adapterToCreate);

                                if (ADL.ADL2_Display_DisplayInfo_Get != null)
                                {
                                    IntPtr DisplayBuffer = IntPtr.Zero;
                                    int numDisplays = 0;
                                    // Force the display detection and get the Display Info. Use 0 as last parameter to NOT force detection
                                    ADLRet = ADL.ADL2_Display_DisplayInfo_Get(_adlContextHandle, oneAdapter.AdapterIndex, ref numDisplays, out DisplayBuffer, 1);
                                    if (ADLRet == ADL.ADL_OK)
                                    {

                                        try
                                        {                                      

                                            for (int displayLoop = 0; displayLoop < numDisplays; displayLoop++)
                                            {
                                                ADLDisplayInfo oneDisplayInfo = new ADLDisplayInfo();
                                                oneDisplayInfo = (ADLDisplayInfo)Marshal.PtrToStructure(new IntPtr(DisplayBuffer.ToInt64() + (displayLoop * Marshal.SizeOf(oneDisplayInfo))), oneDisplayInfo.GetType());

                                                // Is the display mapped to this adapter? If not we skip it!
                                                if (oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex != oneAdapter.AdapterIndex)
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not a real display as its DisplayID.DisplayLogicalAdapterIndex is -1");
                                                    continue;
                                                }

                                                // Convert the displayInfoValue to something usable using a library function I made
                                                ConvertedDisplayInfoValue displayInfoValue = ADL.ConvertDisplayInfoValue(oneDisplayInfo.DisplayInfoValue);

                                                if (!displayInfoValue.DISPLAYCONNECTED)
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not connected");
                                                    continue;
                                                }

                                                // Skip connected but non-mapped displays (not mapped in windows) - we only want displays currently visible in the OS because they're in use
                                                if (!displayInfoValue.DISPLAYMAPPED)
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not mapped in Windows OS");
                                                    continue;
                                                }

                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveProfile: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is connected and mapped in Windows OS");

                                                // Store the Display information for later
                                                
                                                AMDDisplay displayToCreate = new AMDDisplay();                                                
                                                displayToCreate.DisplayModes = new List<ADLMode>();

                                                ADL.ADLDisplayConnectionType displayConnector = (ADL.ADLDisplayConnectionType)oneDisplayInfo.DisplayConnector;

                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: ### Display Info for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Connector = {displayConnector.ToString("G")}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Controller Index = {oneDisplayInfo.DisplayControllerIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Logical Adapter Index = {oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Logical Index = {oneDisplayInfo.DisplayID.DisplayLogicalIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Physical Adapter Index = {oneDisplayInfo.DisplayID.DisplayPhysicalAdapterIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Physical Index = {oneDisplayInfo.DisplayID.DisplayPhysicalIndex}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Mask = {oneDisplayInfo.DisplayInfoMask}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value = {oneDisplayInfo.DisplayInfoValue}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Manufacturer Name = {oneDisplayInfo.DisplayManufacturerName}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Name = {oneDisplayInfo.DisplayName}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Output Type = {oneDisplayInfo.DisplayOutputType}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Type = {oneDisplayInfo.DisplayType}");

                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value DISPLAYCONNECTED = {displayInfoValue.DISPLAYCONNECTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value DISPLAYMAPPED = {displayInfoValue.DISPLAYMAPPED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value FORCIBLESUPPORTED = {displayInfoValue.FORCIBLESUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value GENLOCKSUPPORTED = {displayInfoValue.GENLOCKSUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value LDA_DISPLAY = {displayInfoValue.LDA_DISPLAY}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value MANNER_SUPPORTED_2HSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2HSTRETCH}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value MANNER_SUPPORTED_2VSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2VSTRETCH}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value MANNER_SUPPORTED_CLONE = {displayInfoValue.MANNER_SUPPORTED_CLONE}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value MANNER_SUPPORTED_EXTENDED = {displayInfoValue.MANNER_SUPPORTED_EXTENDED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value MANNER_SUPPORTED_NSTRETCH1GPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCH1GPU}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value MANNER_SUPPORTED_NSTRETCHNGPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCHNGPU}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value MANNER_SUPPORTED_SINGLE = {displayInfoValue.MANNER_SUPPORTED_SINGLE}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value MODETIMING_OVERRIDESSUPPORTED = {displayInfoValue.MODETIMING_OVERRIDESSUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value MULTIVPU_SUPPORTED = {displayInfoValue.MULTIVPU_SUPPORTED}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value NONLOCAL = {displayInfoValue.NONLOCAL}");
                                                SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Info Value SHOWTYPE_PROJECTOR = {displayInfoValue.SHOWTYPE_PROJECTOR}");
                                                                                                
                                                IntPtr displayModeBuffer = IntPtr.Zero;
                                                int numModes = 0;
                                                if (ADL.ADL2_Display_Modes_Get != null)
                                                {
                                                    // Get the ADLModes from the Display
                                                    ADLRet = ADL.ADL2_Display_Modes_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID.DisplayPhysicalIndex, out numModes, out displayModeBuffer);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {
                                                        for (int displayModeLoop = 0; displayModeLoop < numModes; displayModeLoop++)
                                                        {
                                                            ADLMode oneDisplayMode = new ADLMode();
                                                            oneDisplayMode = (ADLMode)Marshal.PtrToStructure(new IntPtr(displayModeBuffer.ToInt64() + (displayModeLoop * Marshal.SizeOf(oneDisplayMode))), oneDisplayMode.GetType());

                                                            displayToCreate.DisplayModes.Add(oneDisplayMode);

                                                            //displayConnectionType = (ADL.ADLDisplayConnectionType)displayConfig.ConnectorType;
                                                            ConvertedDisplayModeFlags displayModeFlag = ADL.ConvertDisplayModeFlags(oneDisplayMode.ModeFlag);
                                                            ConvertedDisplayModeFlags displayModeMask = ADL.ConvertDisplayModeFlags(oneDisplayMode.ModeMask);
                                                            ConvertedDisplayModeFlags displayModeValue = ADL.ConvertDisplayModeFlags(oneDisplayMode.ModeValue);

                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: ### Display Modes for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Colour Depth = {oneDisplayMode.ColourDepth}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Flag = {oneDisplayMode.ModeFlag}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Flag ColourFormat 565 = {displayModeFlag.COLOURFORMAT_565}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Flag ColourFormat 8888 = {displayModeFlag.COLOURFORMAT_8888}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Flag ORIENTATION_SUPPORTED_000 = {displayModeFlag.ORIENTATION_SUPPORTED_000}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Flag ORIENTATION_SUPPORTED_090 = {displayModeFlag.ORIENTATION_SUPPORTED_090}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Flag ORIENTATION_SUPPORTED_180 = {displayModeFlag.ORIENTATION_SUPPORTED_180}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Flag ORIENTATION_SUPPORTED_270 = {displayModeFlag.ORIENTATION_SUPPORTED_270}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Flag REFRESHRATE_ROUNDED = {displayModeFlag.REFRESHRATE_ROUNDED}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Flag REFRESHRATE_ONLY = {displayModeFlag.REFRESHRATE_ONLY}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Mask = {oneDisplayMode.ModeMask}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Mode Value = {oneDisplayMode.ModeValue}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Orientation = {oneDisplayMode.Orientation}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Refresh Rate = {oneDisplayMode.RefreshRate}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode X Position = {oneDisplayMode.XPos}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode X Resolution = {oneDisplayMode.XRes}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Y Position = {oneDisplayMode.YPos}");
                                                            SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: DisplayMode Y Resolution = {oneDisplayMode.YRes}");
                                                        }                                                            
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"AMDLibrary/GetActiveprofile: Error running ADL2_Display_DeviceConfig_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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

                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: ### Display DDCInfo2 for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display AvgLuminanceData = {displayDDCInfo2.AvgLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DDCInfoFlag = {displayDDCInfo2.DDCInfoFlag}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DiffuseScreenReflectance = {displayDDCInfo2.DiffuseScreenReflectance}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DisplayName = {displayDDCInfo2.DisplayName}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display FreesyncFlags = {displayDDCInfo2.FreesyncFlags}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display ManufacturerID = {displayDDCInfo2.ManufacturerID}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MaxBacklightMaxLuminanceData = {displayDDCInfo2.MaxBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MaxBacklightMinLuminanceData = {displayDDCInfo2.MaxBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MaxHResolution = {displayDDCInfo2.MaxHResolution}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MaxLuminanceData = {displayDDCInfo2.MaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MaxRefresh = {displayDDCInfo2.MaxRefresh}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MaxVResolution = {displayDDCInfo2.MaxVResolution}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MinBacklightMaxLuminanceData = {displayDDCInfo2.MinBacklightMaxLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MinBacklightMinLuminanceData = {displayDDCInfo2.MinBacklightMinLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MinLuminanceData = {displayDDCInfo2.MinLuminanceData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display MinLuminanceNoDimmingData = {displayDDCInfo2.MinLuminanceNoDimmingData}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display NativeDisplayChromaticityBlueX = {displayDDCInfo2.NativeDisplayChromaticityBlueX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display NativeDisplayChromaticityBlueY = {displayDDCInfo2.NativeDisplayChromaticityBlueY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display NativeDisplayChromaticityGreenX = {displayDDCInfo2.NativeDisplayChromaticityGreenX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display NativeDisplayChromaticityGreenY = {displayDDCInfo2.NativeDisplayChromaticityGreenY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display NativeDisplayChromaticityRedX = {displayDDCInfo2.NativeDisplayChromaticityRedX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display NativeDisplayChromaticityRedY = {displayDDCInfo2.NativeDisplayChromaticityRedY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display NativeDisplayChromaticityWhiteX = {displayDDCInfo2.NativeDisplayChromaticityWhiteX}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display NativeDisplayChromaticityWhiteY = {displayDDCInfo2.NativeDisplayChromaticityWhiteY}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display PackedPixelSupported = {displayDDCInfo2.PackedPixelSupported}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display PanelPixelFormat = {displayDDCInfo2.PanelPixelFormat}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display ProductID = {displayDDCInfo2.ProductID}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display PTMCx = {displayDDCInfo2.PTMCx}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display PTMCy = {displayDDCInfo2.PTMCy}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display PTMRefreshRate = {displayDDCInfo2.PTMRefreshRate}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display SerialID = {displayDDCInfo2.SerialID}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display Size = {displayDDCInfo2.Size}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display SpecularScreenReflectance = {displayDDCInfo2.SpecularScreenReflectance}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display SupportedColorSpace = {displayDDCInfo2.SupportedColorSpace}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display SupportedHDR = {displayDDCInfo2.SupportedHDR}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display SupportedTransferFunction = {displayDDCInfo2.SupportedTransferFunction}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display SupportsDDC = {displayDDCInfo2.SupportsDDC}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DDCInfoFlag Digital Device  = {DDCInfoFlag.DIGITALDEVICE}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DDCInfoFlag EDID Extension = {DDCInfoFlag.EDIDEXTENSION}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DDCInfoFlag HDMI Audio Device  = {DDCInfoFlag.HDMIAUDIODEVICE}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DDCInfoFlag Projector Device = {DDCInfoFlag.PROJECTORDEVICE}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DDCInfoFlag Supports AI = {DDCInfoFlag.SUPPORTS_AI}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DDCInfoFlag Supports xvYCC601 = {DDCInfoFlag.SUPPORT_xvYCC601}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display DDCInfoFlag Supports xvYCC709 = {DDCInfoFlag.SUPPORT_xvYCC709}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display SupportedHDR Supports CEA861_3 = {supportedHDR.CEA861_3}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display SupportedHDR Supports DOLBYVISION = {supportedHDR.DOLBYVISION}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display SupportedHDR Supports FREESYNC_HDR = {supportedHDR.FREESYNC_HDR}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"AMDLibrary/GetActiveprofile: Error running ADL2_Display_DDCInfo2_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                                    }


                                                    // Add the things we learnt about the Display to the AMDProfile.
                                                    adapterToCreate.Displays.Add(displayToCreate);
                                                }

                                                int HDRSupported = 0;
                                                int HDREnabled = 0;
                                                if (ADL.ADL2_Display_HDRState_Get != null)
                                                {
                                                    // Get the HDR State from the Display
                                                    ADLRet = ADL.ADL2_Display_HDRState_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID, out HDRSupported, out HDREnabled);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: ### Display HDR State for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display HDR Supported = {HDRSupported}");
                                                        SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Display HDR Enabled = {HDREnabled}");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Warn($"AMDLibrary/GetActiveprofile: Error running ADL2_Display_HDRState_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetActiveprofile: Exception getting AMD Vendor ID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.AdapterName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetActiveprofile: Exception getting AMD Adapter Name from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneAdapter.VendorID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetActiveprofile: Exception getting AMD VendorID  from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("1002");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(AdapterCapabilities.AdapterID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetActiveprofile: Exception getting AMD AdapterID from video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayConnector.ToString("G"));
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetActiveprofile: Exception getting AMD Display Connector from video card to display. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(oneDisplayInfo.DisplayName);
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetActiveprofile: Exception getting Display Name from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ManufacturerID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetActiveprofile: Exception getting Manufacturer ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.ProductID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetActiveprofile: Exception getting Product ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                try
                                                {
                                                    displayInfoIdentifierSection.Add(displayDDCInfo2.SerialID.ToString());
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetActiveprofile: Exception getting Serial ID from display connected to AMD video card. Substituting with a # instead");
                                                    displayInfoIdentifierSection.Add("#");
                                                }

                                                // Create a display identifier out of it
                                                string displayIdentifier = String.Join("|", displayInfoIdentifierSection);

                                                // Check first to see if there is already an existing display identifier the same!
                                                // This appears to be a bug with the AMD driver, or with the install on my test machine
                                                // Either way, it is potentially going to happen in the wild, so I will filter it out if it does
                                                if (displayIdentifiers.Contains(displayIdentifier))
                                                {
                                                    SharedLogger.logger.Trace($"AMDLibrary/GetActiveprofile: Your AMD driver reported the following Display Identifier multiple times, so ignoring it as we already have it: {displayIdentifier}");
                                                    continue;
                                                }

                                                // Add it to the list of display identifiers so we can return it
                                                displayIdentifiers.Add(displayIdentifier);

                                                SharedLogger.logger.Debug($"ProfileRepository/GetActiveprofile: DisplayIdentifier: {displayIdentifier}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            SharedLogger.logger.Warn(ex, $"ProfileRepository/GetActiveprofile: Exception caused trying to access attached displays");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"AMDLibrary/GetActiveprofile: Error running ADL2_Display_DisplayInfo_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                                    }
                                    // Release the memory for the DisplayInfo structure
                                    if (IntPtr.Zero != DisplayBuffer)
                                        Marshal.FreeCoTaskMem(DisplayBuffer);
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"AMDLibrary/GetActiveprofile: Error running ADL2_Adapter_Active_Get on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                            }
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"AMDLibrary/GetActiveprofile: Error running ADL2_Adapter_AdapterInfoX4_Get on AMD Video card: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
                    }
                }
                // Release the memory for the AdapterInfo structure
                if (IntPtr.Zero != AdapterBuffer)
                {
                    Marshal.FreeCoTaskMem(AdapterBuffer);
                }

            }
            else
            {
                SharedLogger.logger.Warn($"AMDLibrary/GetActiveprofile: There were no AMD adapters found by AMD ADL.");
                
            }

            // Return the profile
            return profileToCreate;
        }

        public bool SetActiveProfile(AMDProfile profileToUse)
        {
            return true;
        }

        public bool IsActiveProfile(AMDProfile profileToTest)
        {
            return true;
        }

        public bool IsValidProfile(AMDProfile profileToTest)
        {
            return true;
        }

        public List<ScreenPosition> GenerateScreenPositions()
        {
            List<ScreenPosition> screens = new List<ScreenPosition>();



            return screens;
        }
    }
}
