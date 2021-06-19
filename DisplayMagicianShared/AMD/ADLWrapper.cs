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
                SharedLogger.logger.Error("ADLWrapper/ADLWrapper: Exception intialising ADL library. ADL_Main_Control_Create() caused an exception");
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
                SharedLogger.logger.Error("ADLWrapper/ADLWrapper: Exception intialising ADL2 library. ADL2_Main_Control_Create() caused an exception");
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

                                Console.WriteLine($"### Adapter Info for Adapter #{oneAdapter.AdapterIndex} ###");
                                Console.WriteLine($"Adapter AdapterIndex = {oneAdapter.AdapterIndex}");
                                Console.WriteLine($"Adapter AdapterName = {oneAdapter.AdapterName}");
                                Console.WriteLine($"Adapter BusNumber = {oneAdapter.BusNumber}");
                                Console.WriteLine($"Adapter DeviceNumber = {oneAdapter.DeviceNumber}");
                                Console.WriteLine($"Adapter DisplayName = {oneAdapter.DisplayName}");
                                Console.WriteLine($"Adapter DriverPath = {oneAdapter.DriverPath}");
                                Console.WriteLine($"Adapter DriverPathExt = {oneAdapter.DriverPathExt}");
                                Console.WriteLine($"Adapter Exist = {oneAdapter.Exist}");
                                Console.WriteLine($"Adapter FunctionNumber = {oneAdapter.FunctionNumber}");
                                Console.WriteLine($"Adapter InfoMask = {oneAdapter.InfoMask}");
                                Console.WriteLine($"Adapter InfoValue = {oneAdapter.InfoValue}");
                                Console.WriteLine($"Adapter OSDisplayIndex = {oneAdapter.OSDisplayIndex}");
                                Console.WriteLine($"Adapter PNPString = {oneAdapter.PNPString}");
                                Console.WriteLine($"Adapter Present = {oneAdapter.Present}");
                                Console.WriteLine($"Adapter Size = {oneAdapter.Size}");
                                Console.WriteLine($"Adapter UDID = {oneAdapter.UDID}");
                                Console.WriteLine($"Adapter VendorID = {oneAdapter.VendorID}");

                                // Get the Adapter Capabilities
                                ADLAdapterCapsX2 AdapterCapabilities = new ADLAdapterCapsX2();
                                if (ADL.ADL2_AdapterX2_Caps != null)
                                {
                                    ADLRet = ADL.ADL2_AdapterX2_Caps(_adlContextHandle, oneAdapter.AdapterIndex, out AdapterCapabilities);
                                }

                                //ADLAdapterCapsX2 AdapterCapabilities = (ADLAdapterCapsX2)Marshal.PtrToStructure(AdapterCapabilitiesBuffer, typeof(ADLAdapterCapsX2));
                                Console.WriteLine($"### Adapter Capabilities for Adapter #{oneAdapter.AdapterIndex} ###");
                                Console.WriteLine($"Adapter ID = {AdapterCapabilities.AdapterID}");
                                Console.WriteLine($"Adapter Capabilities Mask = {AdapterCapabilities.CapsMask}");
                                Console.WriteLine($"Adapter Capabilities Value = {AdapterCapabilities.CapsValue}");
                                Console.WriteLine($"Adapter Num of Connectors = {AdapterCapabilities.NumConnectors}");
                                Console.WriteLine($"Adapter Num of Controllers = {AdapterCapabilities.NumControllers}");
                                Console.WriteLine($"Adapter Num of Displays = {AdapterCapabilities.NumDisplays}");
                                Console.WriteLine($"Adapter Num of GL Sync Connectors = {AdapterCapabilities.NumOfGLSyncConnectors}");
                                Console.WriteLine($"Adapter Num of Overlays = {AdapterCapabilities.NumOverlays}");

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

                                                ADL.ADLDisplayConnectionType displayConnector = (ADL.ADLDisplayConnectionType)oneDisplayInfo.DisplayConnector;

                                                Console.WriteLine($"### Display Info for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                Console.WriteLine($"Display Connector = {displayConnector.ToString("G")}");
                                                Console.WriteLine($"Display Controller Index = {oneDisplayInfo.DisplayControllerIndex}");
                                                Console.WriteLine($"Display Logical Adapter Index = {oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex}");
                                                Console.WriteLine($"Display Logical Index = {oneDisplayInfo.DisplayID.DisplayLogicalIndex}");
                                                Console.WriteLine($"Display Physical Adapter Index = {oneDisplayInfo.DisplayID.DisplayPhysicalAdapterIndex}");
                                                Console.WriteLine($"Display Physical Index = {oneDisplayInfo.DisplayID.DisplayPhysicalIndex}");
                                                Console.WriteLine($"Display Info Mask = {oneDisplayInfo.DisplayInfoMask}");
                                                Console.WriteLine($"Display Info Value = {oneDisplayInfo.DisplayInfoValue}");
                                                Console.WriteLine($"Display Manufacturer Name = {oneDisplayInfo.DisplayManufacturerName}");
                                                Console.WriteLine($"Display Name = {oneDisplayInfo.DisplayName}");
                                                Console.WriteLine($"Display Output Type = {oneDisplayInfo.DisplayOutputType}");
                                                Console.WriteLine($"Display Type = {oneDisplayInfo.DisplayType}");

                                                Console.WriteLine($"Display Info Value DISPLAYCONNECTED = {displayInfoValue.DISPLAYCONNECTED}");
                                                Console.WriteLine($"Display Info Value DISPLAYMAPPED = {displayInfoValue.DISPLAYMAPPED}");
                                                Console.WriteLine($"Display Info Value FORCIBLESUPPORTED = {displayInfoValue.FORCIBLESUPPORTED}");
                                                Console.WriteLine($"Display Info Value GENLOCKSUPPORTED = {displayInfoValue.GENLOCKSUPPORTED}");
                                                Console.WriteLine($"Display Info Value LDA_DISPLAY = {displayInfoValue.LDA_DISPLAY}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_2HSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2HSTRETCH}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_2VSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2VSTRETCH}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_CLONE = {displayInfoValue.MANNER_SUPPORTED_CLONE}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_EXTENDED = {displayInfoValue.MANNER_SUPPORTED_EXTENDED}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_NSTRETCH1GPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCH1GPU}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_NSTRETCHNGPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCHNGPU}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_SINGLE = {displayInfoValue.MANNER_SUPPORTED_SINGLE}");
                                                Console.WriteLine($"Display Info Value MODETIMING_OVERRIDESSUPPORTED = {displayInfoValue.MODETIMING_OVERRIDESSUPPORTED}");
                                                Console.WriteLine($"Display Info Value MULTIVPU_SUPPORTED = {displayInfoValue.MULTIVPU_SUPPORTED}");
                                                Console.WriteLine($"Display Info Value NONLOCAL = {displayInfoValue.NONLOCAL}");
                                                Console.WriteLine($"Display Info Value SHOWTYPE_PROJECTOR = {displayInfoValue.SHOWTYPE_PROJECTOR}");

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

                                                        Console.WriteLine($"### Display Device Config for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        Console.WriteLine($"Display Connector Type = {displayConnectionType.ToString("G")}");
                                                        Console.WriteLine($"Display Device Data = {displayConfig.DeviceData}");
                                                        Console.WriteLine($"Display Overridded Device Data = {displayConfig.OverriddedDeviceData}");
                                                        Console.WriteLine($"Display Reserved Data = {displayConfig.Reserved}");
                                                        Console.WriteLine($"Display Size = {displayConfig.Size}");
                                                    }
                                                }

                                                ADLDDCInfo2 displayDDCInfo2 = new ADLDDCInfo2();
                                                displayDDCInfo2.Size = Marshal.SizeOf(displayDDCInfo2);
                                                // Create a stringbuilder buffer that EDID can be loaded into
                                                //displayEDIDData.EDIDData = new StringBuilder(256);

                                                if (ADL.ADL2_Display_DDCInfo2_Get != null)
                                                {
                                                    // Get the EDID Data from the Display
                                                    ADLRet = ADL.ADL2_Display_DDCInfo2_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID.DisplayPhysicalIndex, out displayDDCInfo2);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {

                                                        // Convert the DDCInfoFlag to something usable using a library function I made
                                                        ConvertedDDCInfoFlag DDCInfoFlag = ADL.ConvertDDCInfoFlag(displayDDCInfo2.DDCInfoFlag);

                                                        // Convert the DDCInfoFlag to something usable using a library function I made
                                                        ConvertedSupportedHDR supportedHDR = ADL.ConvertSupportedHDR(displayDDCInfo2.SupportedHDR);

                                                        Console.WriteLine($"### Display DDCInfo2 for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        Console.WriteLine($"Display AvgLuminanceData = {displayDDCInfo2.AvgLuminanceData}");
                                                        Console.WriteLine($"Display DDCInfoFlag = {displayDDCInfo2.DDCInfoFlag}");
                                                        Console.WriteLine($"Display DiffuseScreenReflectance = {displayDDCInfo2.DiffuseScreenReflectance}");
                                                        Console.WriteLine($"Display DisplayName = {displayDDCInfo2.DisplayName}");
                                                        Console.WriteLine($"Display FreesyncFlags = {displayDDCInfo2.FreesyncFlags}");
                                                        Console.WriteLine($"Display ManufacturerID = {displayDDCInfo2.ManufacturerID}");
                                                        Console.WriteLine($"Display MaxBacklightMaxLuminanceData = {displayDDCInfo2.MaxBacklightMaxLuminanceData}");
                                                        Console.WriteLine($"Display MaxBacklightMinLuminanceData = {displayDDCInfo2.MaxBacklightMinLuminanceData}");
                                                        Console.WriteLine($"Display MaxHResolution = {displayDDCInfo2.MaxHResolution}");
                                                        Console.WriteLine($"Display MaxLuminanceData = {displayDDCInfo2.MaxLuminanceData}");
                                                        Console.WriteLine($"Display MaxRefresh = {displayDDCInfo2.MaxRefresh}");
                                                        Console.WriteLine($"Display MaxVResolution = {displayDDCInfo2.MaxVResolution}");
                                                        Console.WriteLine($"Display MinBacklightMaxLuminanceData = {displayDDCInfo2.MinBacklightMaxLuminanceData}");
                                                        Console.WriteLine($"Display MinBacklightMinLuminanceData = {displayDDCInfo2.MinBacklightMinLuminanceData}");
                                                        Console.WriteLine($"Display MinLuminanceData = {displayDDCInfo2.MinLuminanceData}");
                                                        Console.WriteLine($"Display MinLuminanceNoDimmingData = {displayDDCInfo2.MinLuminanceNoDimmingData}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityBlueX = {displayDDCInfo2.NativeDisplayChromaticityBlueX}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityBlueY = {displayDDCInfo2.NativeDisplayChromaticityBlueY}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityGreenX = {displayDDCInfo2.NativeDisplayChromaticityGreenX}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityGreenY = {displayDDCInfo2.NativeDisplayChromaticityGreenY}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityRedX = {displayDDCInfo2.NativeDisplayChromaticityRedX}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityRedY = {displayDDCInfo2.NativeDisplayChromaticityRedY}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityWhiteX = {displayDDCInfo2.NativeDisplayChromaticityWhiteX}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityWhiteY = {displayDDCInfo2.NativeDisplayChromaticityWhiteY}");
                                                        Console.WriteLine($"Display PackedPixelSupported = {displayDDCInfo2.PackedPixelSupported}");
                                                        Console.WriteLine($"Display PanelPixelFormat = {displayDDCInfo2.PanelPixelFormat}");
                                                        Console.WriteLine($"Display ProductID = {displayDDCInfo2.ProductID}");
                                                        Console.WriteLine($"Display PTMCx = {displayDDCInfo2.PTMCx}");
                                                        Console.WriteLine($"Display PTMCy = {displayDDCInfo2.PTMCy}");
                                                        Console.WriteLine($"Display PTMRefreshRate = {displayDDCInfo2.PTMRefreshRate}");
                                                        Console.WriteLine($"Display SerialID = {displayDDCInfo2.SerialID}");
                                                        Console.WriteLine($"Display Size = {displayDDCInfo2.Size}");
                                                        Console.WriteLine($"Display SpecularScreenReflectance = {displayDDCInfo2.SpecularScreenReflectance}");
                                                        Console.WriteLine($"Display SupportedColorSpace = {displayDDCInfo2.SupportedColorSpace}");
                                                        Console.WriteLine($"Display SupportedHDR = {displayDDCInfo2.SupportedHDR}");
                                                        Console.WriteLine($"Display SupportedTransferFunction = {displayDDCInfo2.SupportedTransferFunction}");
                                                        Console.WriteLine($"Display SupportsDDC = {displayDDCInfo2.SupportsDDC}");
                                                        Console.WriteLine($"Display DDCInfoFlag Digital Device  = {DDCInfoFlag.DIGITALDEVICE}");
                                                        Console.WriteLine($"Display DDCInfoFlag EDID Extension = {DDCInfoFlag.EDIDEXTENSION}");
                                                        Console.WriteLine($"Display DDCInfoFlag HDMI Audio Device  = {DDCInfoFlag.HDMIAUDIODEVICE}");
                                                        Console.WriteLine($"Display DDCInfoFlag Projector Device = {DDCInfoFlag.PROJECTORDEVICE}");
                                                        Console.WriteLine($"Display DDCInfoFlag Supports AI = {DDCInfoFlag.SUPPORTS_AI}");
                                                        Console.WriteLine($"Display DDCInfoFlag Supports xvYCC601 = {DDCInfoFlag.SUPPORT_xvYCC601}");
                                                        Console.WriteLine($"Display DDCInfoFlag Supports xvYCC709 = {DDCInfoFlag.SUPPORT_xvYCC709}");
                                                        Console.WriteLine($"Display SupportedHDR Supports CEA861_3 = {supportedHDR.CEA861_3}");
                                                        Console.WriteLine($"Display SupportedHDR Supports DOLBYVISION = {supportedHDR.DOLBYVISION}");
                                                        Console.WriteLine($"Display SupportedHDR Supports FREESYNC_HDR = {supportedHDR.FREESYNC_HDR}");
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine($"Error running ADL_vDisplay_EdidData_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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
                                                        Console.WriteLine($"### Display HDR State for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        Console.WriteLine($"Display HDR Supported = {HDRSupported}");
                                                        Console.WriteLine($"Display HDR Enabled = {HDREnabled}");
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

                                                Console.WriteLine($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                                                SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Exception caused trying to access attached displays");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("ADL_Display_DisplayInfo_Get() returned error code " + ADLRet.ToString());
                                    }
                                    // Release the memory for the DisplayInfo structure
                                    if (IntPtr.Zero != DisplayBuffer)
                                        Marshal.FreeCoTaskMem(DisplayBuffer);
                                }
                            }
                            else
                            {
                                Console.WriteLine("ADL_Adapter_Active_Get() returned error code " + ADLRet.ToString());
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("ADL_Adapter_AdapterInfo_Get() returned error code " + ADLRet.ToString());
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
                SharedLogger.logger.Error($"ADLWrapper/GenerateProfileDisplayIdentifiers: There were no AMD adapters found by AMD ADL.");
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

                                Console.WriteLine($"### Adapter Info for Adapter #{oneAdapter.AdapterIndex} ###");
                                Console.WriteLine($"Adapter AdapterIndex = {oneAdapter.AdapterIndex}");
                                Console.WriteLine($"Adapter AdapterName = {oneAdapter.AdapterName}");
                                Console.WriteLine($"Adapter BusNumber = {oneAdapter.BusNumber}");
                                Console.WriteLine($"Adapter DeviceNumber = {oneAdapter.DeviceNumber}");
                                Console.WriteLine($"Adapter DisplayName = {oneAdapter.DisplayName}");
                                Console.WriteLine($"Adapter DriverPath = {oneAdapter.DriverPath}");
                                Console.WriteLine($"Adapter DriverPathExt = {oneAdapter.DriverPathExt}");
                                Console.WriteLine($"Adapter Exist = {oneAdapter.Exist}");
                                Console.WriteLine($"Adapter FunctionNumber = {oneAdapter.FunctionNumber}");
                                Console.WriteLine($"Adapter InfoMask = {oneAdapter.InfoMask}");
                                Console.WriteLine($"Adapter InfoValue = {oneAdapter.InfoValue}");
                                Console.WriteLine($"Adapter OSDisplayIndex = {oneAdapter.OSDisplayIndex}");
                                Console.WriteLine($"Adapter PNPString = {oneAdapter.PNPString}");
                                Console.WriteLine($"Adapter Present = {oneAdapter.Present}");
                                Console.WriteLine($"Adapter Size = {oneAdapter.Size}");
                                Console.WriteLine($"Adapter UDID = {oneAdapter.UDID}");
                                Console.WriteLine($"Adapter VendorID = {oneAdapter.VendorID}");

                                // Get the Adapter Capabilities
                                ADLAdapterCapsX2 AdapterCapabilities = new ADLAdapterCapsX2();
                                if (ADL.ADL2_AdapterX2_Caps != null)
                                {
                                    ADLRet = ADL.ADL2_AdapterX2_Caps(_adlContextHandle, oneAdapter.AdapterIndex, out AdapterCapabilities);
                                }

                                //ADLAdapterCapsX2 AdapterCapabilities = (ADLAdapterCapsX2)Marshal.PtrToStructure(AdapterCapabilitiesBuffer, typeof(ADLAdapterCapsX2));
                                Console.WriteLine($"### Adapter Capabilities for Adapter #{oneAdapter.AdapterIndex} ###");
                                Console.WriteLine($"Adapter ID = {AdapterCapabilities.AdapterID}");
                                Console.WriteLine($"Adapter Capabilities Mask = {AdapterCapabilities.CapsMask}");
                                Console.WriteLine($"Adapter Capabilities Value = {AdapterCapabilities.CapsValue}");
                                Console.WriteLine($"Adapter Num of Connectors = {AdapterCapabilities.NumConnectors}");
                                Console.WriteLine($"Adapter Num of Controllers = {AdapterCapabilities.NumControllers}");
                                Console.WriteLine($"Adapter Num of Displays = {AdapterCapabilities.NumDisplays}");
                                Console.WriteLine($"Adapter Num of GL Sync Connectors = {AdapterCapabilities.NumOfGLSyncConnectors}");
                                Console.WriteLine($"Adapter Num of Overlays = {AdapterCapabilities.NumOverlays}");

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

                                                // Skip the non-connected displays, but still show the non-mapped displays (as they are still able to be used with a display config change)
                                                if (!displayInfoValue.DISPLAYCONNECTED)
                                                {
                                                    SharedLogger.logger.Trace($"ADLWrapper/GenerateProfileDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} ({oneAdapter.AdapterName}) AdapterID display ID#{oneDisplayInfo.DisplayID.DisplayLogicalIndex} is not connected");
                                                    continue;
                                                }
                                                
                                                ADL.ADLDisplayConnectionType displayConnector = (ADL.ADLDisplayConnectionType)oneDisplayInfo.DisplayConnector;

                                                Console.WriteLine($"### Display Info for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                Console.WriteLine($"Display Connector = {displayConnector.ToString("G")}");
                                                Console.WriteLine($"Display Controller Index = {oneDisplayInfo.DisplayControllerIndex}");
                                                Console.WriteLine($"Display Logical Adapter Index = {oneDisplayInfo.DisplayID.DisplayLogicalAdapterIndex}");
                                                Console.WriteLine($"Display Logical Index = {oneDisplayInfo.DisplayID.DisplayLogicalIndex}");
                                                Console.WriteLine($"Display Physical Adapter Index = {oneDisplayInfo.DisplayID.DisplayPhysicalAdapterIndex}");
                                                Console.WriteLine($"Display Physical Index = {oneDisplayInfo.DisplayID.DisplayPhysicalIndex}");
                                                Console.WriteLine($"Display Info Mask = {oneDisplayInfo.DisplayInfoMask}");
                                                Console.WriteLine($"Display Info Value = {oneDisplayInfo.DisplayInfoValue}");
                                                Console.WriteLine($"Display Manufacturer Name = {oneDisplayInfo.DisplayManufacturerName}");
                                                Console.WriteLine($"Display Name = {oneDisplayInfo.DisplayName}");
                                                Console.WriteLine($"Display Output Type = {oneDisplayInfo.DisplayOutputType}");
                                                Console.WriteLine($"Display Type = {oneDisplayInfo.DisplayType}");

                                                Console.WriteLine($"Display Info Value DISPLAYCONNECTED = {displayInfoValue.DISPLAYCONNECTED}");
                                                Console.WriteLine($"Display Info Value DISPLAYMAPPED = {displayInfoValue.DISPLAYMAPPED}");
                                                Console.WriteLine($"Display Info Value FORCIBLESUPPORTED = {displayInfoValue.FORCIBLESUPPORTED}");
                                                Console.WriteLine($"Display Info Value GENLOCKSUPPORTED = {displayInfoValue.GENLOCKSUPPORTED}");
                                                Console.WriteLine($"Display Info Value LDA_DISPLAY = {displayInfoValue.LDA_DISPLAY}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_2HSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2HSTRETCH}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_2VSTRETCH = {displayInfoValue.MANNER_SUPPORTED_2VSTRETCH}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_CLONE = {displayInfoValue.MANNER_SUPPORTED_CLONE}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_EXTENDED = {displayInfoValue.MANNER_SUPPORTED_EXTENDED}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_NSTRETCH1GPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCH1GPU}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_NSTRETCHNGPU = {displayInfoValue.MANNER_SUPPORTED_NSTRETCHNGPU}");
                                                Console.WriteLine($"Display Info Value MANNER_SUPPORTED_SINGLE = {displayInfoValue.MANNER_SUPPORTED_SINGLE}");
                                                Console.WriteLine($"Display Info Value MODETIMING_OVERRIDESSUPPORTED = {displayInfoValue.MODETIMING_OVERRIDESSUPPORTED}");
                                                Console.WriteLine($"Display Info Value MULTIVPU_SUPPORTED = {displayInfoValue.MULTIVPU_SUPPORTED}");
                                                Console.WriteLine($"Display Info Value NONLOCAL = {displayInfoValue.NONLOCAL}");
                                                Console.WriteLine($"Display Info Value SHOWTYPE_PROJECTOR = {displayInfoValue.SHOWTYPE_PROJECTOR}");

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

                                                        Console.WriteLine($"### Display Device Config for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        Console.WriteLine($"Display Connector Type = {displayConnectionType.ToString("G")}");
                                                        Console.WriteLine($"Display Device Data = {displayConfig.DeviceData}");
                                                        Console.WriteLine($"Display Overridded Device Data = {displayConfig.OverriddedDeviceData}");
                                                        Console.WriteLine($"Display Reserved Data = {displayConfig.Reserved}");
                                                        Console.WriteLine($"Display Size = {displayConfig.Size}");
                                                    }
                                                }

                                                ADLDDCInfo2 displayDDCInfo2 = new ADLDDCInfo2();
                                                displayDDCInfo2.Size = Marshal.SizeOf(displayDDCInfo2);
                                                // Create a stringbuilder buffer that EDID can be loaded into
                                                //displayEDIDData.EDIDData = new StringBuilder(256);

                                                if (ADL.ADL2_Display_DDCInfo2_Get != null)
                                                {
                                                    // Get the EDID Data from the Display
                                                    ADLRet = ADL.ADL2_Display_DDCInfo2_Get(_adlContextHandle, oneAdapter.AdapterIndex, oneDisplayInfo.DisplayID.DisplayPhysicalIndex, out displayDDCInfo2);
                                                    if (ADLRet == ADL.ADL_OK)
                                                    {

                                                        // Convert the DDCInfoFlag to something usable using a library function I made
                                                        ConvertedDDCInfoFlag DDCInfoFlag = ADL.ConvertDDCInfoFlag(displayDDCInfo2.DDCInfoFlag);

                                                        // Convert the DDCInfoFlag to something usable using a library function I made
                                                        ConvertedSupportedHDR supportedHDR = ADL.ConvertSupportedHDR(displayDDCInfo2.SupportedHDR);

                                                        Console.WriteLine($"### Display DDCInfo2 for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        Console.WriteLine($"Display AvgLuminanceData = {displayDDCInfo2.AvgLuminanceData}");
                                                        Console.WriteLine($"Display DDCInfoFlag = {displayDDCInfo2.DDCInfoFlag}");
                                                        Console.WriteLine($"Display DiffuseScreenReflectance = {displayDDCInfo2.DiffuseScreenReflectance}");
                                                        Console.WriteLine($"Display DisplayName = {displayDDCInfo2.DisplayName}");
                                                        Console.WriteLine($"Display FreesyncFlags = {displayDDCInfo2.FreesyncFlags}");
                                                        Console.WriteLine($"Display ManufacturerID = {displayDDCInfo2.ManufacturerID}");
                                                        Console.WriteLine($"Display MaxBacklightMaxLuminanceData = {displayDDCInfo2.MaxBacklightMaxLuminanceData}");
                                                        Console.WriteLine($"Display MaxBacklightMinLuminanceData = {displayDDCInfo2.MaxBacklightMinLuminanceData}");
                                                        Console.WriteLine($"Display MaxHResolution = {displayDDCInfo2.MaxHResolution}");
                                                        Console.WriteLine($"Display MaxLuminanceData = {displayDDCInfo2.MaxLuminanceData}");
                                                        Console.WriteLine($"Display MaxRefresh = {displayDDCInfo2.MaxRefresh}");
                                                        Console.WriteLine($"Display MaxVResolution = {displayDDCInfo2.MaxVResolution}");
                                                        Console.WriteLine($"Display MinBacklightMaxLuminanceData = {displayDDCInfo2.MinBacklightMaxLuminanceData}");
                                                        Console.WriteLine($"Display MinBacklightMinLuminanceData = {displayDDCInfo2.MinBacklightMinLuminanceData}");
                                                        Console.WriteLine($"Display MinLuminanceData = {displayDDCInfo2.MinLuminanceData}");
                                                        Console.WriteLine($"Display MinLuminanceNoDimmingData = {displayDDCInfo2.MinLuminanceNoDimmingData}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityBlueX = {displayDDCInfo2.NativeDisplayChromaticityBlueX}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityBlueY = {displayDDCInfo2.NativeDisplayChromaticityBlueY}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityGreenX = {displayDDCInfo2.NativeDisplayChromaticityGreenX}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityGreenY = {displayDDCInfo2.NativeDisplayChromaticityGreenY}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityRedX = {displayDDCInfo2.NativeDisplayChromaticityRedX}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityRedY = {displayDDCInfo2.NativeDisplayChromaticityRedY}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityWhiteX = {displayDDCInfo2.NativeDisplayChromaticityWhiteX}");
                                                        Console.WriteLine($"Display NativeDisplayChromaticityWhiteY = {displayDDCInfo2.NativeDisplayChromaticityWhiteY}");
                                                        Console.WriteLine($"Display PackedPixelSupported = {displayDDCInfo2.PackedPixelSupported}");
                                                        Console.WriteLine($"Display PanelPixelFormat = {displayDDCInfo2.PanelPixelFormat}");
                                                        Console.WriteLine($"Display ProductID = {displayDDCInfo2.ProductID}");
                                                        Console.WriteLine($"Display PTMCx = {displayDDCInfo2.PTMCx}");
                                                        Console.WriteLine($"Display PTMCy = {displayDDCInfo2.PTMCy}");
                                                        Console.WriteLine($"Display PTMRefreshRate = {displayDDCInfo2.PTMRefreshRate}");
                                                        Console.WriteLine($"Display SerialID = {displayDDCInfo2.SerialID}");
                                                        Console.WriteLine($"Display Size = {displayDDCInfo2.Size}");
                                                        Console.WriteLine($"Display SpecularScreenReflectance = {displayDDCInfo2.SpecularScreenReflectance}");
                                                        Console.WriteLine($"Display SupportedColorSpace = {displayDDCInfo2.SupportedColorSpace}");
                                                        Console.WriteLine($"Display SupportedHDR = {displayDDCInfo2.SupportedHDR}");
                                                        Console.WriteLine($"Display SupportedTransferFunction = {displayDDCInfo2.SupportedTransferFunction}");
                                                        Console.WriteLine($"Display SupportsDDC = {displayDDCInfo2.SupportsDDC}");
                                                        Console.WriteLine($"Display DDCInfoFlag Digital Device  = {DDCInfoFlag.DIGITALDEVICE}");
                                                        Console.WriteLine($"Display DDCInfoFlag EDID Extension = {DDCInfoFlag.EDIDEXTENSION}");
                                                        Console.WriteLine($"Display DDCInfoFlag HDMI Audio Device  = {DDCInfoFlag.HDMIAUDIODEVICE}");
                                                        Console.WriteLine($"Display DDCInfoFlag Projector Device = {DDCInfoFlag.PROJECTORDEVICE}");
                                                        Console.WriteLine($"Display DDCInfoFlag Supports AI = {DDCInfoFlag.SUPPORTS_AI}");
                                                        Console.WriteLine($"Display DDCInfoFlag Supports xvYCC601 = {DDCInfoFlag.SUPPORT_xvYCC601}");
                                                        Console.WriteLine($"Display DDCInfoFlag Supports xvYCC709 = {DDCInfoFlag.SUPPORT_xvYCC709}");
                                                        Console.WriteLine($"Display SupportedHDR Supports CEA861_3 = {supportedHDR.CEA861_3}");
                                                        Console.WriteLine($"Display SupportedHDR Supports DOLBYVISION = {supportedHDR.DOLBYVISION}");
                                                        Console.WriteLine($"Display SupportedHDR Supports FREESYNC_HDR = {supportedHDR.FREESYNC_HDR}");
                                                    }
                                                    else
                                                    {
                                                        Console.WriteLine($"Error running ADL_vDisplay_EdidData_Get on Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex}: {ADL.ConvertADLReturnValueIntoWords(ADLRet)}");
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
                                                        Console.WriteLine($"### Display HDR State for Display #{oneDisplayInfo.DisplayID.DisplayLogicalIndex} on Adapter #{oneAdapter.AdapterIndex} ###");
                                                        Console.WriteLine($"Display HDR Supported = {HDRSupported}");
                                                        Console.WriteLine($"Display HDR Enabled = {HDREnabled}");
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

                                                Console.WriteLine($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                                                SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Exception caused trying to access attached displays");
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("ADL_Display_DisplayInfo_Get() returned error code " + ADLRet.ToString());
                                    }
                                    // Release the memory for the DisplayInfo structure
                                    if (IntPtr.Zero != DisplayBuffer)
                                        Marshal.FreeCoTaskMem(DisplayBuffer);
                                }
                            }
                            else
                            {
                                Console.WriteLine("ADL_Adapter_Active_Get() returned error code " + ADLRet.ToString());
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("ADL_Adapter_AdapterInfo_Get() returned error code " + ADLRet.ToString());
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
                SharedLogger.logger.Error($"ADLWrapper/GenerateProfileDisplayIdentifiers: There were no AMD adapters found by AMD ADL.");
                return null;
            }
        }
    }
}
