using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using DisplayMagicianShared;
using System.ComponentModel;
using DisplayMagicianShared.Windows;
using System.Threading;

namespace DisplayMagicianShared.AMD
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_ADAPTER_CONFIG : IEquatable<AMD_ADAPTER_CONFIG>
    {
        public int AdapterDeviceNumber;
        public int AdapterBusNumber;
        public int AdapterIndex;
        public bool IsPrimaryAdapter;
        public string DisplayName;
        public int OSDisplayIndex;

        public override bool Equals(object obj) => obj is AMD_ADAPTER_CONFIG other && this.Equals(other);

        public bool Equals(AMD_ADAPTER_CONFIG other)
        => AdapterIndex == other.AdapterIndex &&
           AdapterBusNumber == other.AdapterBusNumber &&
           AdapterDeviceNumber == other.AdapterDeviceNumber &&
           IsPrimaryAdapter == other.IsPrimaryAdapter &&
           DisplayName == other.DisplayName &&
           OSDisplayIndex == other.OSDisplayIndex;

        public override int GetHashCode()
        {
            return (AdapterIndex, AdapterBusNumber, AdapterDeviceNumber, IsPrimaryAdapter, DisplayName, OSDisplayIndex).GetHashCode();
        }

        public static bool operator ==(AMD_ADAPTER_CONFIG lhs, AMD_ADAPTER_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_ADAPTER_CONFIG lhs, AMD_ADAPTER_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_SLSMAP_CONFIG : IEquatable<AMD_SLSMAP_CONFIG>
    {
        public ADL_SLS_MAP SLSMap;
        public List<ADL_SLS_TARGET> SLSTargets;
        public List<ADL_SLS_MODE> NativeModes;
        public List<ADL_SLS_OFFSET> NativeModeOffsets;
        public List<ADL_BEZEL_TRANSIENT_MODE> BezelModes;
        public List<ADL_BEZEL_TRANSIENT_MODE> TransientModes;
        public List<ADL_SLS_OFFSET> SLSOffsets;
        public int BezelModePercent;

        public override bool Equals(object obj) => obj is AMD_SLS_CONFIG other && this.Equals(other);

        public bool Equals(AMD_SLSMAP_CONFIG other)
        => SLSMap == other.SLSMap &&
           SLSTargets.SequenceEqual(other.SLSTargets) &&
           NativeModes.SequenceEqual(other.NativeModes) &&
           NativeModeOffsets.SequenceEqual(other.NativeModeOffsets) &&
           BezelModes.SequenceEqual(other.BezelModes) &&
           TransientModes.SequenceEqual(other.TransientModes) &&
           SLSOffsets.SequenceEqual(other.SLSOffsets) &&
           BezelModePercent == other.BezelModePercent;

        public override int GetHashCode()
        {
            return (SLSMap, SLSTargets, NativeModes, NativeModeOffsets, BezelModes, TransientModes, SLSOffsets, BezelModePercent).GetHashCode();
        }
        public static bool operator ==(AMD_SLSMAP_CONFIG lhs, AMD_SLSMAP_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_SLSMAP_CONFIG lhs, AMD_SLSMAP_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_SLS_CONFIG : IEquatable<AMD_SLS_CONFIG>
    {
        public bool IsSlsEnabled;
        public List<AMD_SLSMAP_CONFIG> SLSMapConfigs;
        public List<ADL_MODE> SLSEnabledDisplayTargets;

        public override bool Equals(object obj) => obj is AMD_SLS_CONFIG other && this.Equals(other);

        public bool Equals(AMD_SLS_CONFIG other)
        => IsSlsEnabled == other.IsSlsEnabled &&
           SLSMapConfigs.SequenceEqual(other.SLSMapConfigs) &&
           SLSEnabledDisplayTargets.SequenceEqual(other.SLSEnabledDisplayTargets);

        public override int GetHashCode()
        {
            return (IsSlsEnabled, SLSMapConfigs, SLSEnabledDisplayTargets).GetHashCode();
        }
        public static bool operator ==(AMD_SLS_CONFIG lhs, AMD_SLS_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_SLS_CONFIG lhs, AMD_SLS_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_HDR_CONFIG : IEquatable<AMD_HDR_CONFIG>
    {
        public int AdapterIndex;
        public bool HDRSupported;
        public bool HDREnabled;

        public override bool Equals(object obj) => obj is AMD_HDR_CONFIG other && this.Equals(other);
        public bool Equals(AMD_HDR_CONFIG other)
        => AdapterIndex == other.AdapterIndex &&
           HDRSupported == other.HDRSupported &&
           HDREnabled == other.HDREnabled;

        public override int GetHashCode()
        {
            return (AdapterIndex, HDRSupported, HDREnabled).GetHashCode();
        }
        public static bool operator ==(AMD_HDR_CONFIG lhs, AMD_HDR_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_HDR_CONFIG lhs, AMD_HDR_CONFIG rhs) => !(lhs == rhs);
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct AMD_DISPLAY_CONFIG : IEquatable<AMD_DISPLAY_CONFIG>
    {
        public bool IsInUse;
        public List<AMD_ADAPTER_CONFIG> AdapterConfigs;
        public AMD_SLS_CONFIG SlsConfig;
        public List<ADL_DISPLAY_MAP> DisplayMaps;
        public List<ADL_DISPLAY_TARGET> DisplayTargets;
        public Dictionary<int, AMD_HDR_CONFIG> HdrConfigs;
        public List<string> DisplayIdentifiers;
        public override bool Equals(object obj) => obj is AMD_DISPLAY_CONFIG other && this.Equals(other);

        public bool Equals(AMD_DISPLAY_CONFIG other)
        => IsInUse == other.IsInUse &&
           AdapterConfigs.SequenceEqual(other.AdapterConfigs) &&
           SlsConfig.Equals(other.SlsConfig) &&
           DisplayMaps.SequenceEqual(other.DisplayMaps) &&
           DisplayTargets.SequenceEqual(other.DisplayTargets) &&
           HdrConfigs.SequenceEqual(other.HdrConfigs) &&
           DisplayIdentifiers.SequenceEqual(other.DisplayIdentifiers);

        public override int GetHashCode()
        {
            return (IsInUse, AdapterConfigs, SlsConfig, DisplayMaps, DisplayTargets, DisplayIdentifiers).GetHashCode();
        }

        public static bool operator ==(AMD_DISPLAY_CONFIG lhs, AMD_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(AMD_DISPLAY_CONFIG lhs, AMD_DISPLAY_CONFIG rhs) => !(lhs == rhs);
    }

    class AMDLibrary : IDisposable
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

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
        private AMD_DISPLAY_CONFIG _activeDisplayConfig;
        public List<ADL_DISPLAY_CONNECTION_TYPE> SkippedColorConnectionTypes;
        public List<string> _allConnectedDisplayIdentifiers;

        static AMDLibrary() { }
        public AMDLibrary()
        {
            // Populate the list of ConnectionTypes we want to skip as they don't support querying
            SkippedColorConnectionTypes = new List<ADL_DISPLAY_CONNECTION_TYPE> {
                ADL_DISPLAY_CONNECTION_TYPE.Composite,
                ADL_DISPLAY_CONNECTION_TYPE.DVI_D,
                ADL_DISPLAY_CONNECTION_TYPE.DVI_I,
                ADL_DISPLAY_CONNECTION_TYPE.RCA_3Component,
                ADL_DISPLAY_CONNECTION_TYPE.SVideo,
                ADL_DISPLAY_CONNECTION_TYPE.VGA
            };

            _activeDisplayConfig = CreateDefaultConfig();
            try
            {
                SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Attempting to load the AMD ADL DLL {ADLImport.ATI_ADL_DLL}");
                // Attempt to prelink all of the NVAPI functions
                Marshal.PrelinkAll(typeof(ADLImport));

                SharedLogger.logger.Trace("AMDLibrary/AMDLibrary: Intialising AMD ADL2 library interface");
                // Second parameter is 1 so that we only the get connected adapters in use now

                // We set the environment variable as a workaround so that ADL2_Display_SLSMapConfigX2_Get works :(
                // This is a weird thing that AMD even set in their own code! WTF! Who programmed that as a feature?
                Environment.SetEnvironmentVariable("ADL_4KWORKAROUND_CANCEL", "TRUE");

                try
                {
                    ADL_STATUS ADLRet;
                    ADLRet = ADLImport.ADL2_Main_Control_Create(ADLImport.ADL_Main_Memory_Alloc, ADLImport.ADL_TRUE, out _adlContextHandle);
                    if (ADLRet == ADL_STATUS.ADL_OK)
                    {
                        _initialised = true;
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: AMD ADL2 library was initialised successfully");
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Running UpdateActiveConfig to ensure there is a config to use later");

                        // Force the AMD Video card to stay on when AMDLibrary is runnning
                        KeepVideoCardOn();

                        _activeDisplayConfig = GetActiveConfig();
                        _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers();
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/AMDLibrary: Error intialising AMD ADL2 library. ADL2_Main_Control_Create() returned error code {ADLRet}");
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Trace(ex, $"AMDLibrary/AMDLibrary: Exception intialising AMD ADL2 library. ADL2_Main_Control_Create() caused an exception.");
                }

            }
            catch (DllNotFoundException ex)
            {
                // If we get here then the AMD ADL DLL wasn't found. We can't continue to use it, so we log the error and exit
                SharedLogger.logger.Info(ex, $"AMDLibrary/AMDLibrary: Exception trying to load the AMD ADL DLL {ADLImport.ATI_ADL_DLL}. This generally means you don't have the AMD ADL driver installed.");
            }

        }

        ~AMDLibrary()
        {
            SharedLogger.logger.Trace("AMDLibrary/~AMDLibrary: Destroying AMD Library");
            Dispose(false);
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

                //ADLImport.ADL_Main_Control_Destroy();

                // Dispose managed state (managed objects).
                _safeHandle?.Dispose();
            }

            // Dispose unmanaged resources
            if (_adlContextHandle != IntPtr.Zero)
            {

                SharedLogger.logger.Trace("AMDLibrary/Dispose: Destroying AMD ADL2 library interface");
                // If the ADL2 library was initialised, then we need to free it up.
                if (_initialised)
                {
                    try
                    {
                        ADLImport.ADL2_Main_Control_Destroy(_adlContextHandle);
                        _adlContextHandle = IntPtr.Zero;
                        SharedLogger.logger.Trace($"AMDLibrary/Dispose: AMD ADL2 library was destroyed successfully");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Trace(ex, $"AMDLibrary/Dispose: Exception destroying AMD ADL2 library. ADL2_Main_Control_Destroy() caused an exception.");
                    }

                }
            }

            _disposed = true;
        }

        public static void KeepVideoCardOn()
        {
            LoadLibrary("AMDExportsDLL.dll");
        }

        public bool IsInstalled
        {
            get
            {
                return _initialised;
            }
        }

        public List<string> PCIVendorIDs
        {
            get
            {
                // A list of all the matching PCI Vendor IDs are per https://www.pcilookup.com/?ven=amd&dev=&action=submit
                return new List<string>() { "1002" };
            }
        }

        public AMD_DISPLAY_CONFIG ActiveDisplayConfig
        {
            get
            {
                return _activeDisplayConfig;
            }
            set
            {
                _activeDisplayConfig = value;
            }
        }

        public List<string> CurrentDisplayIdentifiers
        {
            get
            {
                return _activeDisplayConfig.DisplayIdentifiers;
            }
        }

        public static AMDLibrary GetLibrary()
        {
            if (_instance == null)
            {
                _instance = new AMDLibrary();
            }

            return _instance;
        }

        public AMD_DISPLAY_CONFIG CreateDefaultConfig()
        {
            AMD_DISPLAY_CONFIG myDefaultConfig = new AMD_DISPLAY_CONFIG();

            // Fill in the minimal amount we need to avoid null references
            // so that we won't break json.net when we save a default config

            myDefaultConfig.AdapterConfigs = new List<AMD_ADAPTER_CONFIG>();
            myDefaultConfig.SlsConfig.IsSlsEnabled = false;
            myDefaultConfig.SlsConfig.SLSMapConfigs = new List<AMD_SLSMAP_CONFIG>();
            myDefaultConfig.SlsConfig.SLSEnabledDisplayTargets = new List<ADL_MODE>();
            myDefaultConfig.DisplayMaps = new List<ADL_DISPLAY_MAP>();
            myDefaultConfig.DisplayTargets = new List<ADL_DISPLAY_TARGET>();
            myDefaultConfig.HdrConfigs = new Dictionary<int, AMD_HDR_CONFIG>();
            myDefaultConfig.DisplayIdentifiers = new List<string>();
            myDefaultConfig.IsInUse = false;

            return myDefaultConfig;
        }

        public bool UpdateActiveConfig()
        {
            SharedLogger.logger.Trace($"AMDLibrary/UpdateActiveConfig: Updating the currently active config");
            try
            {
                _activeDisplayConfig = GetActiveConfig();
                _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers();
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Trace(ex, $"AMDLibrary/UpdateActiveConfig: Exception updating the currently active config");
                return false;
            }

            return true;
        }

        public AMD_DISPLAY_CONFIG GetActiveConfig()
        {
            SharedLogger.logger.Trace($"AMDLibrary/GetActiveConfig: Getting the currently active config");
            bool allDisplays = true;
            return GetAMDDisplayConfig(allDisplays);
        }

        private AMD_DISPLAY_CONFIG GetAMDDisplayConfig(bool allDisplays = false)
        {
            AMD_DISPLAY_CONFIG myDisplayConfig = CreateDefaultConfig();

            if (_initialised)
            {
                try
                {
                    // Get the Adapter info for ALL adapter and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: Running ADL2_Adapter_AdapterInfoX4_Get to get the information about all AMD Adapters.");
                    int numAdaptersInfo = 0;
                    IntPtr adapterInfoBuffer = IntPtr.Zero;
                    ADL_STATUS ADLRet = ADLImport.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, -1, out numAdaptersInfo, out adapterInfoBuffer);
                    if (ADLRet == ADL_STATUS.ADL_OK)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Adapter_AdapterInfoX4_Get returned information about all AMD Adapters.");
                    }
                    else
                    {
                        SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Adapter_AdapterInfoX4_Get returned ADL_STATUS {ADLRet} when trying to get the adapter info about all AMD Adapters. Trying to skip this adapter so something at least works.");
                        return myDisplayConfig;
                    }

                    ADL_ADAPTER_INFOX2[] adapterArray = new ADL_ADAPTER_INFOX2[numAdaptersInfo];
                    if (numAdaptersInfo > 0)
                    {
                        IntPtr currentDisplayTargetBuffer = adapterInfoBuffer;
                        for (int i = 0; i < numAdaptersInfo; i++)
                        {
                            // build a structure in the array slot
                            adapterArray[i] = new ADL_ADAPTER_INFOX2();
                            // fill the array slot structure with the data from the buffer
                            adapterArray[i] = (ADL_ADAPTER_INFOX2)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                            // destroy the bit of memory we no longer need
                            //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                            // advance the buffer forwards to the next object
                            currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(adapterArray[i]));
                        }
                        // Free the memory used by the buffer                        
                        Marshal.FreeCoTaskMem(adapterInfoBuffer);
                    }
                    else
                    {
                        // Free the memory used by the buffer                        
                        Marshal.FreeCoTaskMem(adapterInfoBuffer);
                        // Return the default config as there are no adapters to get info from
                        return myDisplayConfig;
                    }

                    // Now go through each adapter and get the information we need from it
                    for (int adapterIndex = 0; adapterIndex < numAdaptersInfo; adapterIndex++)
                    {
                        // Skip this adapter if it isn't active
                        ADL_ADAPTER_INFOX2 oneAdapter = adapterArray[adapterIndex]; // There is always just one as we asked for a specific one!
                        if (oneAdapter.Exist != ADLImport.ADL_TRUE)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                            continue;
                        }

                        // Only skip non-present displays if we want all displays information
                        if (oneAdapter.Present != ADLImport.ADL_TRUE)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
                            continue;
                        }

                        // Check if the adapter is active
                        // Skip this adapter if it isn't active
                        int adapterActiveStatus = ADLImport.ADL_FALSE;
                        ADLRet = ADLImport.ADL2_Adapter_Active_Get(_adlContextHandle, adapterIndex, out adapterActiveStatus);
                        if (ADLRet == ADL_STATUS.ADL_OK)
                        {
                            if (adapterActiveStatus == ADLImport.ADL_TRUE)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: ADL2_Adapter_Active_Get returned ADL_TRUE - AMD Adapter #{adapterIndex} is active! We can continue.");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: ADL2_Adapter_Active_Get returned ADL_FALSE - AMD Adapter #{adapterIndex} is NOT active, so skipping.");
                                continue;
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Warn($"AMDLibrary/GetSomeDisplayIdentifiers: WARNING - ADL2_Adapter_Active_Get returned ADL_STATUS {ADLRet} when trying to see if AMD Adapter #{adapterIndex} is active. Trying to skip this adapter so something at least works.");
                            continue;
                        }

                        // Go grab the DisplayMaps and DisplayTargets as that is useful infor for creating screens
                        int numDisplayTargets = 0;
                        int numDisplayMaps = 0;
                        IntPtr displayTargetBuffer = IntPtr.Zero;
                        IntPtr displayMapBuffer = IntPtr.Zero;
                        ADLRet = ADLImport.ADL2_Display_DisplayMapConfig_Get(_adlContextHandle, adapterIndex, out numDisplayMaps, out displayMapBuffer, out numDisplayTargets, out displayTargetBuffer, ADLImport.ADL_DISPLAY_DISPLAYMAP_OPTION_GPUINFO);
                        if (ADLRet == ADL_STATUS.ADL_OK)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_DisplayMapConfig_Get returned information about all displaytargets connected to AMD adapter {adapterIndex}.");
                        }
                        else
                        {
                            SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_DisplayMapConfig_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter {adapterIndex} in the computer.");
                            throw new AMDLibraryException($"ADL2_Display_DisplayMapConfig_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter {adapterIndex} in the computer");
                        }

                        ADL_DISPLAY_MAP[] displayMapArray = { };
                        if (numDisplayMaps > 0)
                        {

                            IntPtr currentDisplayMapBuffer = displayMapBuffer;
                            displayMapArray = new ADL_DISPLAY_MAP[numDisplayMaps];
                            for (int i = 0; i < numDisplayMaps; i++)
                            {
                                // build a structure in the array slot
                                displayMapArray[i] = new ADL_DISPLAY_MAP();
                                // fill the array slot structure with the data from the buffer
                                displayMapArray[i] = (ADL_DISPLAY_MAP)Marshal.PtrToStructure(currentDisplayMapBuffer, typeof(ADL_DISPLAY_MAP));
                                // destroy the bit of memory we no longer need
                                Marshal.DestroyStructure(currentDisplayMapBuffer, typeof(ADL_DISPLAY_MAP));
                                // advance the buffer forwards to the next object
                                currentDisplayMapBuffer = (IntPtr)((long)currentDisplayMapBuffer + Marshal.SizeOf(displayMapArray[i]));
                            }
                            // Free the memory used by the buffer                        
                            Marshal.FreeCoTaskMem(displayMapBuffer);
                            // Save the item
                            myDisplayConfig.DisplayMaps = displayMapArray.ToList<ADL_DISPLAY_MAP>();

                        }

                        ADL_DISPLAY_TARGET[] displayTargetArray = { };
                        if (numDisplayTargets > 0)
                        {
                            // At this point we know there is at least one screen connected to an adapter
                            myDisplayConfig.IsInUse = true;

                            IntPtr currentDisplayTargetBuffer = displayTargetBuffer;
                            //displayTargetArray = new ADL_DISPLAY_TARGET[numDisplayTargets];
                            displayTargetArray = new ADL_DISPLAY_TARGET[numDisplayTargets];
                            for (int i = 0; i < numDisplayTargets; i++)
                            {
                                // build a structure in the array slot
                                displayTargetArray[i] = new ADL_DISPLAY_TARGET();
                                //displayTargetArray[i] = new ADL_DISPLAY_TARGET();
                                // fill the array slot structure with the data from the buffer
                                displayTargetArray[i] = (ADL_DISPLAY_TARGET)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                                //displayTargetArray[i] = (ADL_DISPLAY_TARGET)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                                // destroy the bit of memory we no longer need
                                Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                                // advance the buffer forwards to the next object
                                currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));
                                //currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));

                            }
                            // Free the memory used by the buffer                        
                            Marshal.FreeCoTaskMem(displayTargetBuffer);
                            // Save the item                            
                            //savedAdapterConfig.DisplayTargets = new ADL_DISPLAY_TARGET[numDisplayTargets];
                            myDisplayConfig.DisplayTargets = displayTargetArray.ToList<ADL_DISPLAY_TARGET>();
                        }
                        else
                        {
                            // Free the memory used by the buffer                        
                            Marshal.FreeCoTaskMem(displayTargetBuffer);
                            // Return the default config as there are no display targets to get info from
                            return myDisplayConfig;
                        }

                        // Loop through all the displayTargets currently in use
                        foreach (var displayTarget in displayTargetArray)
                        {
                            if (displayTarget.DisplayID.DisplayLogicalAdapterIndex == oneAdapter.AdapterIndex)
                            {
                                // we only want to record the adapters that are currently in use as displayTargets
                                AMD_ADAPTER_CONFIG savedAdapterConfig = new AMD_ADAPTER_CONFIG();
                                savedAdapterConfig.AdapterBusNumber = oneAdapter.BusNumber;
                                savedAdapterConfig.AdapterDeviceNumber = oneAdapter.DeviceNumber;
                                savedAdapterConfig.AdapterIndex = oneAdapter.AdapterIndex;
                                savedAdapterConfig.DisplayName = oneAdapter.DisplayName;
                                savedAdapterConfig.OSDisplayIndex = oneAdapter.OSDisplayIndex;

                                // Save the AMD Adapter Config
                                if (!myDisplayConfig.AdapterConfigs.Contains(savedAdapterConfig))
                                {
                                    // Save the new adapter config only if we haven't already
                                    myDisplayConfig.AdapterConfigs.Add(savedAdapterConfig);
                                }

                            }
                        }

                        // Prep the SLSMapConfig list
                        myDisplayConfig.SlsConfig.SLSMapConfigs = new List<AMD_SLSMAP_CONFIG>();

                        // If there are more than 1 display targets then eyefinity is possible
                        if (numDisplayTargets > 1)
                        {
                            // Check if SLS is enabled for this adapter!
                            int matchingSLSMapIndex = -1;
                            ADLRet = ADLImport.ADL2_Display_SLSMapIndex_Get(_adlContextHandle, oneAdapter.AdapterIndex, numDisplayTargets, displayTargetArray, out matchingSLSMapIndex);
                            if (ADLRet == ADL_STATUS.ADL_OK && matchingSLSMapIndex != -1)
                            {
                                // We have a matching SLS index!
                                SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} has one or more SLS Maps that could be used with this display configuration! Eyefinity (SLS) could be enabled.");

                                AMD_SLSMAP_CONFIG mySLSMapConfig = new AMD_SLSMAP_CONFIG();

                                // We want to get the SLSMapConfig for this matching SLS Map to see if it is actually in use
                                int numSLSTargets = 0;
                                IntPtr slsTargetBuffer = IntPtr.Zero;
                                int numNativeMode = 0;
                                IntPtr nativeModeBuffer = IntPtr.Zero;
                                int numNativeModeOffsets = 0;
                                IntPtr nativeModeOffsetsBuffer = IntPtr.Zero;
                                int numBezelMode = 0;
                                IntPtr bezelModeBuffer = IntPtr.Zero;
                                int numTransientMode = 0;
                                IntPtr transientModeBuffer = IntPtr.Zero;
                                int numSLSOffset = 0;
                                IntPtr slsOffsetBuffer = IntPtr.Zero;
                                ADL_SLS_MAP slsMap = new ADL_SLS_MAP();
                                ADLRet = ADLImport.ADL2_Display_SLSMapConfigX2_Get(
                                                                                _adlContextHandle,
                                                                                    oneAdapter.AdapterIndex,
                                                                                    matchingSLSMapIndex,
                                                                                    ref slsMap,
                                                                                    out numSLSTargets,
                                                                                    out slsTargetBuffer,
                                                                                    out numNativeMode,
                                                                                    out nativeModeBuffer,
                                                                                    out numNativeModeOffsets,
                                                                                    out nativeModeOffsetsBuffer,
                                                                                    out numBezelMode,
                                                                                    out bezelModeBuffer,
                                                                                    out numTransientMode,
                                                                                    out transientModeBuffer,
                                                                                    out numSLSOffset,
                                                                                    out slsOffsetBuffer,
                                                                                    ADLImport.ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_CURRENTANGLE);
                                if (ADLRet == ADL_STATUS.ADL_OK)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_SLSMapConfigX2_Get returned information about the SLS Info connected to AMD adapter {adapterIndex}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_SLSMapConfigX2_Get returned ADL_STATUS {ADLRet} when trying to get the SLS Info from AMD adapter {adapterIndex} in the computer.");
                                    continue;
                                }

                                // First check that the number of grid entries is equal to the number
                                // of display targets associated with this adapter & SLS surface.
                                if (numDisplayTargets != (slsMap.Grid.SLSGridColumn * slsMap.Grid.SLSGridRow))
                                {
                                    //Number of display targets returned is not equal to the SLS grid size, so SLS can't be enabled fo this display
                                    //myDisplayConfig.SlsConfig.IsSlsEnabled = false; // This is already set to false at the start!
                                    break;
                                }

                                // Add the slsMap to the config we want to store
                                mySLSMapConfig.SLSMap = slsMap;

                                // Process the slsTargetBuffer
                                ADL_SLS_TARGET[] slsTargetArray = new ADL_SLS_TARGET[numSLSTargets];
                                if (numSLSTargets > 0)
                                {
                                    IntPtr currentSLSTargetBuffer = slsTargetBuffer;
                                    for (int i = 0; i < numSLSTargets; i++)
                                    {
                                        // build a structure in the array slot
                                        slsTargetArray[i] = new ADL_SLS_TARGET();
                                        // fill the array slot structure with the data from the buffer
                                        slsTargetArray[i] = (ADL_SLS_TARGET)Marshal.PtrToStructure(currentSLSTargetBuffer, typeof(ADL_SLS_TARGET));
                                        // destroy the bit of memory we no longer need
                                        //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                        // advance the buffer forwards to the next object
                                        currentSLSTargetBuffer = (IntPtr)((long)currentSLSTargetBuffer + Marshal.SizeOf(slsTargetArray[i]));
                                    }
                                    // Free the memory used by the buffer                        
                                    Marshal.FreeCoTaskMem(slsTargetBuffer);

                                    // Add the slsTarget to the config we want to store
                                    mySLSMapConfig.SLSTargets = slsTargetArray.ToList();

                                }
                                else
                                {
                                    // Add the slsTarget to the config we want to store
                                    mySLSMapConfig.SLSTargets = new List<ADL_SLS_TARGET>();
                                }

                                // Process the nativeModeBuffer
                                ADL_SLS_MODE[] nativeModeArray = new ADL_SLS_MODE[numNativeMode];
                                if (numNativeMode > 0)
                                {
                                    IntPtr currentNativeModeBuffer = nativeModeBuffer;
                                    for (int i = 0; i < numNativeMode; i++)
                                    {
                                        // build a structure in the array slot
                                        nativeModeArray[i] = new ADL_SLS_MODE();
                                        // fill the array slot structure with the data from the buffer
                                        nativeModeArray[i] = (ADL_SLS_MODE)Marshal.PtrToStructure(currentNativeModeBuffer, typeof(ADL_SLS_MODE));
                                        // destroy the bit of memory we no longer need
                                        //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                        // advance the buffer forwards to the next object
                                        currentNativeModeBuffer = (IntPtr)((long)currentNativeModeBuffer + Marshal.SizeOf(nativeModeArray[i]));
                                    }
                                    // Free the memory used by the buffer                        
                                    Marshal.FreeCoTaskMem(nativeModeBuffer);

                                    // Add the nativeMode to the config we want to store
                                    mySLSMapConfig.NativeModes = nativeModeArray.ToList();

                                }
                                else
                                {
                                    // Add the slsTarget to the config we want to store
                                    mySLSMapConfig.NativeModes = new List<ADL_SLS_MODE>();
                                }

                                // Process the nativeModeOffsetsBuffer
                                ADL_SLS_OFFSET[] nativeModeOffsetArray = new ADL_SLS_OFFSET[numNativeModeOffsets];
                                if (numNativeModeOffsets > 0)
                                {
                                    IntPtr currentNativeModeOffsetsBuffer = nativeModeOffsetsBuffer;
                                    for (int i = 0; i < numNativeModeOffsets; i++)
                                    {
                                        // build a structure in the array slot
                                        nativeModeOffsetArray[i] = new ADL_SLS_OFFSET();
                                        // fill the array slot structure with the data from the buffer
                                        nativeModeOffsetArray[i] = (ADL_SLS_OFFSET)Marshal.PtrToStructure(currentNativeModeOffsetsBuffer, typeof(ADL_SLS_OFFSET));
                                        // destroy the bit of memory we no longer need
                                        //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                        // advance the buffer forwards to the next object
                                        currentNativeModeOffsetsBuffer = (IntPtr)((long)currentNativeModeOffsetsBuffer + Marshal.SizeOf(nativeModeOffsetArray[i]));
                                    }
                                    // Free the memory used by the buffer                        
                                    Marshal.FreeCoTaskMem(nativeModeOffsetsBuffer);

                                    // Add the nativeModeOffsets to the config we want to store
                                    mySLSMapConfig.NativeModeOffsets = nativeModeOffsetArray.ToList();

                                }
                                else
                                {
                                    // Add the empty list to the config we want to store
                                    mySLSMapConfig.NativeModeOffsets = new List<ADL_SLS_OFFSET>();
                                }

                                // Process the bezelModeBuffer
                                ADL_BEZEL_TRANSIENT_MODE[] bezelModeArray = new ADL_BEZEL_TRANSIENT_MODE[numBezelMode];
                                if (numBezelMode > 0)
                                {
                                    IntPtr currentBezelModeBuffer = bezelModeBuffer;
                                    for (int i = 0; i < numBezelMode; i++)
                                    {
                                        // build a structure in the array slot
                                        bezelModeArray[i] = new ADL_BEZEL_TRANSIENT_MODE();
                                        // fill the array slot structure with the data from the buffer
                                        bezelModeArray[i] = (ADL_BEZEL_TRANSIENT_MODE)Marshal.PtrToStructure(currentBezelModeBuffer, typeof(ADL_BEZEL_TRANSIENT_MODE));
                                        // destroy the bit of memory we no longer need
                                        //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                        // advance the buffer forwards to the next object
                                        currentBezelModeBuffer = (IntPtr)((long)currentBezelModeBuffer + Marshal.SizeOf(bezelModeArray[i]));
                                    }
                                    // Free the memory used by the buffer                        
                                    Marshal.FreeCoTaskMem(bezelModeBuffer);

                                    // Add the bezelModes to the config we want to store
                                    mySLSMapConfig.BezelModes = bezelModeArray.ToList();

                                }
                                else
                                {
                                    // Add the slsTarget to the config we want to store
                                    mySLSMapConfig.BezelModes = new List<ADL_BEZEL_TRANSIENT_MODE>();
                                }

                                // Process the transientModeBuffer
                                ADL_BEZEL_TRANSIENT_MODE[] transientModeArray = new ADL_BEZEL_TRANSIENT_MODE[numTransientMode];
                                if (numTransientMode > 0)
                                {
                                    IntPtr currentTransientModeBuffer = transientModeBuffer;
                                    for (int i = 0; i < numTransientMode; i++)
                                    {
                                        // build a structure in the array slot
                                        transientModeArray[i] = new ADL_BEZEL_TRANSIENT_MODE();
                                        // fill the array slot structure with the data from the buffer
                                        transientModeArray[i] = (ADL_BEZEL_TRANSIENT_MODE)Marshal.PtrToStructure(currentTransientModeBuffer, typeof(ADL_BEZEL_TRANSIENT_MODE));
                                        // destroy the bit of memory we no longer need
                                        //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                        // advance the buffer forwards to the next object
                                        currentTransientModeBuffer = (IntPtr)((long)currentTransientModeBuffer + Marshal.SizeOf(transientModeArray[i]));
                                    }
                                    // Free the memory used by the buffer                        
                                    Marshal.FreeCoTaskMem(transientModeBuffer);

                                    // Add the transientModes to the config we want to store
                                    mySLSMapConfig.TransientModes = transientModeArray.ToList();
                                }
                                else
                                {
                                    // Add the slsTarget to the config we want to store
                                    mySLSMapConfig.TransientModes = new List<ADL_BEZEL_TRANSIENT_MODE>();
                                }

                                // Process the slsOffsetBuffer
                                ADL_SLS_OFFSET[] slsOffsetArray = new ADL_SLS_OFFSET[numSLSOffset];
                                if (numSLSOffset > 0)
                                {
                                    IntPtr currentSLSOffsetBuffer = slsOffsetBuffer;
                                    for (int i = 0; i < numSLSOffset; i++)
                                    {
                                        // build a structure in the array slot
                                        slsOffsetArray[i] = new ADL_SLS_OFFSET();
                                        // fill the array slot structure with the data from the buffer
                                        slsOffsetArray[i] = (ADL_SLS_OFFSET)Marshal.PtrToStructure(currentSLSOffsetBuffer, typeof(ADL_SLS_OFFSET));
                                        // destroy the bit of memory we no longer need
                                        //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                        // advance the buffer forwards to the next object
                                        currentSLSOffsetBuffer = (IntPtr)((long)currentSLSOffsetBuffer + Marshal.SizeOf(slsOffsetArray[i]));
                                    }
                                    // Free the memory used by the buffer                        
                                    Marshal.FreeCoTaskMem(slsOffsetBuffer);

                                    // Add the slsOffsets to the config we want to store
                                    mySLSMapConfig.SLSOffsets = slsOffsetArray.ToList();

                                }
                                else
                                {
                                    // Add the slsTarget to the config we want to store
                                    mySLSMapConfig.SLSOffsets = new List<ADL_SLS_OFFSET>();
                                }

                                // Now we try to calculate whether SLS is enabled
                                // NFI why they don't just add a ADL2_Display_SLSMapConfig_GetState function to make this easy for ppl :(
                                // NVIDIA make it easy, why can't you AMD?

                                // Logic cribbed from https://github.com/elitak/amd-adl-sdk/blob/master/Sample/Eyefinity/ati_eyefinity.c
                                // Go through each display Target
                                foreach (var displayTarget in displayTargetArray)
                                {
                                    // Get the current Display Modes for this adapter/display combination
                                    int numDisplayModes;
                                    IntPtr displayModeBuffer;
                                    ADLRet = ADLImport.ADL2_Display_Modes_Get(
                                                                                _adlContextHandle,
                                                                                    oneAdapter.AdapterIndex,
                                                                                    displayTarget.DisplayID.DisplayLogicalIndex,
                                                                                    out numDisplayModes,
                                                                                    out displayModeBuffer);
                                    if (ADLRet == ADL_STATUS.ADL_OK)
                                    {
                                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_Modes_Get returned information about the display modes used by display #{displayTarget.DisplayID.DisplayLogicalAdapterIndex} connected to AMD adapter {adapterIndex}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_Modes_Get returned ADL_STATUS {ADLRet} when trying to get the display modes from AMD adapter {adapterIndex} in the computer.");
                                        continue;
                                    }

                                    ADL_MODE[] displayModeArray = new ADL_MODE[numDisplayModes];
                                    if (numDisplayModes > 0)
                                    {
                                        IntPtr currentDisplayModeBuffer = displayModeBuffer;
                                        for (int i = 0; i < numDisplayModes; i++)
                                        {
                                            // build a structure in the array slot
                                            displayModeArray[i] = new ADL_MODE();
                                            // fill the array slot structure with the data from the buffer
                                            displayModeArray[i] = (ADL_MODE)Marshal.PtrToStructure(currentDisplayModeBuffer, typeof(ADL_MODE));
                                            // destroy the bit of memory we no longer need
                                            //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                                            // advance the buffer forwards to the next object
                                            currentDisplayModeBuffer = (IntPtr)((long)currentDisplayModeBuffer + Marshal.SizeOf(displayModeArray[i]));
                                        }
                                        // Free the memory used by the buffer                        
                                        Marshal.FreeCoTaskMem(displayModeBuffer);

                                        // Add the slsOffsets to the config we want to store
                                        //mySLSMapConfig.SLSOffsets = displayModeArray.ToList();

                                    }

                                    // If Eyefinity is enabled for this adapter, then the display mode of an
                                    // attached display target will match one of the SLS display modes reported by
                                    // ADL_Display_SLSMapConfig_Get(). The match will either be with "native" SLS 
                                    // modes (which are not bezel-compensated), or with "bezel" SLS modes which are.
                                    // 
                                    // So, simply compare current display mode against all the ones listed for the
                                    // SLS native or bezel-compensated modes: if there is a match, then the mode
                                    // currently used by this adapter is an Eyefinity/SLS mode, and Eyefinity is enabled.
                                    // First check the native SLS mode list
                                    // Process the slsOffsetBuffer
                                    bool isSlsEnabled = false;
                                    bool isBezelCompensatedDisplay = false;
                                    foreach (var displayMode in displayModeArray)
                                    {
                                        foreach (var nativeMode in nativeModeArray)
                                        {
                                            if (nativeMode.DisplayMode.XRes == displayMode.XRes && nativeMode.DisplayMode.YRes == displayMode.YRes)
                                            {
                                                isSlsEnabled = true;
                                                break;
                                            }

                                        }

                                        // If no match was found, check the bezel-compensated SLS mode list
                                        if (!isSlsEnabled)
                                        {
                                            foreach (var bezelMode in bezelModeArray)
                                            {
                                                if (bezelMode.DisplayMode.XRes == displayMode.XRes && bezelMode.DisplayMode.YRes == displayMode.YRes)
                                                {
                                                    isSlsEnabled = true;
                                                    isBezelCompensatedDisplay = true;
                                                    break;
                                                }
                                            }
                                        }

                                        // Now we check which slot we need to put this display into
                                        if (isSlsEnabled)
                                        {
                                            // SLS is enabled for this display
                                            if (!myDisplayConfig.SlsConfig.SLSEnabledDisplayTargets.Contains(displayMode))
                                            {
                                                myDisplayConfig.SlsConfig.SLSEnabledDisplayTargets.Add(displayMode);
                                            }
                                            // we also update the main IsSLSEnabled so that it is indicated at the top level too

                                            myDisplayConfig.SlsConfig.IsSlsEnabled = true;
                                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} has a matching SLS grid set! Eyefinity (SLS) is enabled. Setting IsSlsEnabled to true");

                                        }
                                    }

                                }

                                // Only Add the mySLSMapConfig to the displayConfig if SLS is enabled
                                if (myDisplayConfig.SlsConfig.IsSlsEnabled)
                                {
                                    myDisplayConfig.SlsConfig.SLSMapConfigs.Add(mySLSMapConfig);
                                }

                            }
                            else
                            {
                                // If we get here then there there was no active SLSGrid, meaning Eyefinity is disabled!
                                SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} has no active SLS grids set! Eyefinity (SLS) hasn't even been setup yet. Keeping the default IsSlsEnabled value of false.");
                            }
                        }
                        else
                        {
                            // If we get here then there are less than two displays connected. Eyefinity cannot be enabled in this case!
                            SharedLogger.logger.Info($"AMDLibrary/GetAMDDisplayConfig: There are less than two displays connected to this adapter so Eyefinity cannot be enabled.");
                        }


                        int forceDetect = 0;
                        int numDisplays;
                        IntPtr displayInfoBuffer;
                        ADLRet = ADLImport.ADL2_Display_DisplayInfo_Get(_adlContextHandle, adapterIndex, out numDisplays, out displayInfoBuffer, forceDetect);
                        if (ADLRet == ADL_STATUS.ADL_OK)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Display_DisplayInfo_Get returned information about all displaytargets connected to AMD adapter #{adapterIndex}.");
                        }
                        else if (ADLRet == ADL_STATUS.ADL_ERR_NULL_POINTER || ADLRet == ADL_STATUS.ADL_ERR_NOT_SUPPORTED)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Display_DisplayInfo_Get returned ADL_ERR_NULL_POINTER so skipping getting display info from AMD adapter #{adapterIndex}.");
                            continue;
                        }
                        else
                        {
                            SharedLogger.logger.Error($"AMDLibrary/PrintActiveConfig: ERROR - ADL2_Display_DisplayInfo_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter #{adapterIndex}.");
                        }

                        ADL_DISPLAY_INFO[] displayInfoArray = { };
                        if (numDisplays > 0)
                        {
                            IntPtr currentDisplayInfoBuffer = displayInfoBuffer;
                            displayInfoArray = new ADL_DISPLAY_INFO[numDisplays];
                            for (int i = 0; i < numDisplays; i++)
                            {
                                // build a structure in the array slot
                                displayInfoArray[i] = new ADL_DISPLAY_INFO();
                                // fill the array slot structure with the data from the buffer
                                displayInfoArray[i] = (ADL_DISPLAY_INFO)Marshal.PtrToStructure(currentDisplayInfoBuffer, typeof(ADL_DISPLAY_INFO));
                                // destroy the bit of memory we no longer need
                                Marshal.DestroyStructure(currentDisplayInfoBuffer, typeof(ADL_DISPLAY_INFO));
                                // advance the buffer forwards to the next object
                                currentDisplayInfoBuffer = (IntPtr)((long)currentDisplayInfoBuffer + Marshal.SizeOf(displayInfoArray[i]));
                                //currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));

                            }
                            // Free the memory used by the buffer                        
                            Marshal.FreeCoTaskMem(displayInfoBuffer);
                        }

                        myDisplayConfig.HdrConfigs = new Dictionary<int, AMD_HDR_CONFIG>();

                        // Now we need to get all the displays connected to this adapter so that we can get their HDR state
                        foreach (var displayTarget in displayTargetArray)
                        {
                            // We need to skip recording anything that doesn't support color communication
                            // Firstly find the display connector if we can
                            ADL_DISPLAY_CONNECTION_TYPE displayConnector;
                            try
                            {
                                displayConnector = displayInfoArray.First(d => d.DisplayID == displayTarget.DisplayID).DisplayConnector;
                            }
                            catch (Exception ex)
                            {
                                displayConnector = ADL_DISPLAY_CONNECTION_TYPE.Unknown;
                                SharedLogger.logger.Warn(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception caused whilst trying to find which display connector display {displayTarget.DisplayID} on adapter {adapterIndex} has:");
                            }
                            SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: Display {displayTarget.DisplayID} on AMD adapter #{adapterIndex} has a {displayConnector} connector.");
                            // Then only get the HDR config stuff if the connection actually suports getting the HDR info!
                            if (!SkippedColorConnectionTypes.Contains(displayConnector))
                            {
                                // Go through each display and see if HDR is supported
                                int supported = 0;
                                int enabled = 0;
                                ADLRet = ADLImport.ADL2_Display_HDRState_Get(_adlContextHandle, adapterIndex, displayTarget.DisplayID, out supported, out enabled);
                                if (ADLRet == ADL_STATUS.ADL_OK)
                                {
                                    if (supported > 0 && enabled > 0)
                                    {
                                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_HDRState_Get says that display {displayTarget.DisplayID.DisplayLogicalIndex} on adapter {adapterIndex} supports HDR and HDR is enabled.");
                                    }
                                    else if (supported > 0 && enabled == 0)
                                    {
                                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_HDRState_Get says that display {displayTarget.DisplayID.DisplayLogicalIndex} on adapter {adapterIndex} supports HDR and HDR is NOT enabled.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_HDRState_Get says that display {displayTarget.DisplayID.DisplayLogicalIndex} on adapter {adapterIndex} does NOT support HDR.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_HDRState_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter {adapterIndex} in the computer.");
                                    throw new AMDLibraryException($"ADL2_Display_HDRState_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter {adapterIndex} in the computer");
                                }

                                AMD_HDR_CONFIG hdrConfig = new AMD_HDR_CONFIG();
                                hdrConfig.AdapterIndex = displayTarget.DisplayID.DisplayPhysicalAdapterIndex;
                                hdrConfig.HDREnabled = enabled > 0 ? true : false;
                                hdrConfig.HDRSupported = supported > 0 ? true : false;

                                // Now add this to the HDR config list.                        
                                if (!myDisplayConfig.HdrConfigs.ContainsKey(displayTarget.DisplayID.DisplayLogicalIndex))
                                {
                                    // Save the new display config only if we haven't already
                                    myDisplayConfig.HdrConfigs.Add(displayTarget.DisplayID.DisplayLogicalIndex, hdrConfig);
                                }
                            }
                        }

                    }

                    // Add the AMD Display Identifiers
                    myDisplayConfig.DisplayIdentifiers = GetCurrentDisplayIdentifiers();

                    // At this stage we should set the IsInUse flag to report that the AMD config is in Use
                    myDisplayConfig.IsInUse = true;
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception trying to get the NVIDIA Configuration when we know there is an NVIDIA Physical GPU present.");
                    // Return the default config to see if we can keep going.
                    return CreateDefaultConfig();
                }
            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - Tried to run GetAMDDisplayConfig but the AMD ADL library isn't initialised!");
                throw new AMDLibraryException($"Tried to run GetAMDDisplayConfig but the AMD ADL library isn't initialised!");
            }
            
            // Return the configuration
            return myDisplayConfig;
        }


        public string PrintActiveConfig()
        {
            string stringToReturn = "";

            // Get the current config
            AMD_DISPLAY_CONFIG displayConfig = ActiveDisplayConfig;

            stringToReturn += $"****** AMD VIDEO CARDS *******\n";


            if (_initialised)
            {
                // Get the number of AMD adapters that the OS knows about
                int numAdapters = 0;
                ADL_STATUS ADLRet = ADLImport.ADL2_Adapter_NumberOfAdapters_Get(_adlContextHandle, out numAdapters);
                if (ADLRet == ADL_STATUS.ADL_OK)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Adapter_NumberOfAdapters_Get returned the number of AMD Adapters the OS knows about ({numAdapters}).");
                }
                else
                {
                    SharedLogger.logger.Error($"AMDLibrary/PrintActiveConfig: ERROR - ADL2_Adapter_NumberOfAdapters_Get returned ADL_STATUS {ADLRet} when trying to get number of AMD adapters in the computer.");
                }

                // Figure out primary adapter
                int primaryAdapterIndex = 0;
                ADLRet = ADLImport.ADL2_Adapter_Primary_Get(_adlContextHandle, out primaryAdapterIndex);
                if (ADLRet == ADL_STATUS.ADL_OK)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: The primary adapter has index {primaryAdapterIndex}.");
                }
                else
                {
                    SharedLogger.logger.Error($"AMDLibrary/PrintActiveConfig: ERROR - ADL2_Adapter_Primary_Get returned ADL_STATUS {ADLRet} when trying to get the primary adapter info from all the AMD adapters in the computer.");
                }

                // Now go through each adapter and get the information we need from it
                for (int adapterIndex = 0; adapterIndex < numAdapters; adapterIndex++)
                {
                    // Skip this adapter if it isn't active
                    int adapterActiveStatus = ADLImport.ADL_FALSE;
                    ADLRet = ADLImport.ADL2_Adapter_Active_Get(_adlContextHandle, adapterIndex, out adapterActiveStatus);
                    if (ADLRet == ADL_STATUS.ADL_OK)
                    {
                        if (adapterActiveStatus == ADLImport.ADL_TRUE)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Adapter_Active_Get returned ADL_TRUE - AMD Adapter #{adapterIndex} is active! We can continue.");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Adapter_Active_Get returned ADL_FALSE - AMD Adapter #{adapterIndex} is NOT active, so skipping.");
                            continue;
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"AMDLibrary/PrintActiveConfig: WARNING - ADL2_Adapter_Active_Get returned ADL_STATUS {ADLRet} when trying to see if AMD Adapter #{adapterIndex} is active. Trying to skip this adapter so something at least works.");
                        continue;
                    }

                    // Get the Adapter info for this adapter and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: Running ADL2_Adapter_AdapterInfoX4_Get to get the information about AMD Adapter #{adapterIndex}.");
                    int numAdaptersInfo = 0;
                    IntPtr adapterInfoBuffer = IntPtr.Zero;
                    ADLRet = ADLImport.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, adapterIndex, out numAdaptersInfo, out adapterInfoBuffer);
                    if (ADLRet == ADL_STATUS.ADL_OK)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Adapter_AdapterInfoX4_Get returned information about AMD Adapter #{adapterIndex}.");
                    }
                    else
                    {
                        SharedLogger.logger.Error($"AMDLibrary/PrintActiveConfig: ERROR - ADL2_Adapter_AdapterInfoX4_Get returned ADL_STATUS {ADLRet} when trying to get the adapter info from AMD Adapter #{adapterIndex}. Trying to skip this adapter so something at least works.");
                        continue;
                    }

                    ADL_ADAPTER_INFOX2[] adapterArray = new ADL_ADAPTER_INFOX2[numAdaptersInfo];
                    if (numAdaptersInfo > 0)
                    {
                        IntPtr currentDisplayTargetBuffer = adapterInfoBuffer;
                        for (int i = 0; i < numAdaptersInfo; i++)
                        {
                            // build a structure in the array slot
                            adapterArray[i] = new ADL_ADAPTER_INFOX2();
                            // fill the array slot structure with the data from the buffer
                            adapterArray[i] = (ADL_ADAPTER_INFOX2)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                            // destroy the bit of memory we no longer need
                            //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                            // advance the buffer forwards to the next object
                            currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(adapterArray[i]));
                        }
                        // Free the memory used by the buffer                        
                        Marshal.FreeCoTaskMem(adapterInfoBuffer);
                    }

                    SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: Converted ADL2_Adapter_AdapterInfoX4_Get memory buffer into a {adapterArray.Length} long array about AMD Adapter #{adapterIndex}.");

                    //AMD_ADAPTER_CONFIG savedAdapterConfig = new AMD_ADAPTER_CONFIG();
                    ADL_ADAPTER_INFOX2 oneAdapter = adapterArray[0];
                    if (oneAdapter.Exist != 1)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                        continue;
                    }

                    // Print out what we need
                    stringToReturn += $"Adapter #{adapterIndex}\n";
                    stringToReturn += $"Adapter Exists: {oneAdapter.Exist}\n";
                    stringToReturn += $"Adapter Present: {oneAdapter.Present}\n";
                    stringToReturn += $"Adapter Name: {oneAdapter.AdapterName}\n";
                    stringToReturn += $"Adapter Display Name: {oneAdapter.DisplayName}\n";
                    stringToReturn += $"Adapter Driver Path: {oneAdapter.DriverPath}\n";
                    stringToReturn += $"Adapter Driver Path Extension: {oneAdapter.DriverPathExt}\n";
                    stringToReturn += $"Adapter UDID: {oneAdapter.UDID}\n";
                    stringToReturn += $"Adapter Vendor ID: {oneAdapter.VendorID}\n";
                    stringToReturn += $"Adapter PNP String: {oneAdapter.PNPString}\n";
                    stringToReturn += $"Adapter PCI Device Number: {oneAdapter.DeviceNumber}\n";
                    stringToReturn += $"Adapter PCI Bus Number: {oneAdapter.BusNumber}\n";
                    stringToReturn += $"Adapter Windows OS Display Index: {oneAdapter.OSDisplayIndex}\n";
                    stringToReturn += $"Adapter Display Connected: {oneAdapter.DisplayConnectedSet}\n";
                    stringToReturn += $"Adapter Display Mapped in Windows: {oneAdapter.DisplayMappedSet}\n";
                    stringToReturn += $"Adapter Is Forcibly Enabled: {oneAdapter.ForcibleSet}\n";
                    stringToReturn += $"Adapter GetLock is Set: {oneAdapter.GenLockSet}\n";
                    stringToReturn += $"Adapter LDA Display is Set: {oneAdapter.LDADisplaySet}\n";
                    stringToReturn += $"Adapter Display Configuration is stretched horizontally across two displays: {oneAdapter.Manner2HStretchSet}\n";
                    stringToReturn += $"Adapter Display Configuration is stretched vertically across two displays: {oneAdapter.Manner2VStretchSet}\n";
                    stringToReturn += $"Adapter Display Configuration is a clone of another display: {oneAdapter.MannerCloneSet}\n";
                    stringToReturn += $"Adapter Display Configuration is an extension of another display: {oneAdapter.MannerExtendedSet}\n";
                    stringToReturn += $"Adapter Display Configuration is an N Strech across 1 GPU: {oneAdapter.MannerNStretch1GPUSet}\n";
                    stringToReturn += $"Adapter Display Configuration is an N Strech across more than one GPU: {oneAdapter.MannerNStretchNGPUSet}\n";
                    stringToReturn += $"Adapter Display Configuration is a single display: {oneAdapter.MannerSingleSet}\n";
                    stringToReturn += $"Adapter timing override: {oneAdapter.ModeTimingOverrideSet}\n";
                    stringToReturn += $"Adapter has MultiVPU set: {oneAdapter.MultiVPUSet}\n";
                    stringToReturn += $"Adapter has non-local set (it is a remote display): {oneAdapter.NonLocalSet}\n";
                    stringToReturn += $"Adapter is a Show Type Projector: {oneAdapter.ShowTypeProjectorSet}\n\n";

                }

                // Now we still try to get the information from each display we need to print 
                int numDisplayTargets = 0;
                int numDisplayMaps = 0;
                IntPtr displayTargetBuffer = IntPtr.Zero;
                IntPtr displayMapBuffer = IntPtr.Zero;
                ADLRet = ADLImport.ADL2_Display_DisplayMapConfig_Get(_adlContextHandle, -1, out numDisplayMaps, out displayMapBuffer, out numDisplayTargets, out displayTargetBuffer, ADLImport.ADL_DISPLAY_DISPLAYMAP_OPTION_GPUINFO);
                if (ADLRet == ADL_STATUS.ADL_OK)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Display_DisplayMapConfig_Get returned information about all displaytargets connected to all AMD adapters.");

                    // Free the memory used by the buffer to avoid heap corruption
                    Marshal.FreeCoTaskMem(displayMapBuffer);

                    ADL_DISPLAY_TARGET[] displayTargetArray = { };
                    if (numDisplayTargets > 0)
                    {
                        IntPtr currentDisplayTargetBuffer = displayTargetBuffer;
                        //displayTargetArray = new ADL_DISPLAY_TARGET[numDisplayTargets];
                        displayTargetArray = new ADL_DISPLAY_TARGET[numDisplayTargets];
                        for (int i = 0; i < numDisplayTargets; i++)
                        {
                            // build a structure in the array slot
                            displayTargetArray[i] = new ADL_DISPLAY_TARGET();
                            //displayTargetArray[i] = new ADL_DISPLAY_TARGET();
                            // fill the array slot structure with the data from the buffer
                            displayTargetArray[i] = (ADL_DISPLAY_TARGET)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                            //displayTargetArray[i] = (ADL_DISPLAY_TARGET)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                            // destroy the bit of memory we no longer need
                            Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                            // advance the buffer forwards to the next object
                            currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));
                            //currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));

                        }
                        // Free the memory used by the buffer                        
                        Marshal.FreeCoTaskMem(displayTargetBuffer);
                    }

                    foreach (var displayTarget in displayTargetArray)
                    {
                        int forceDetect = 0;
                        int numDisplays;
                        IntPtr displayInfoBuffer;
                        ADLRet = ADLImport.ADL2_Display_DisplayInfo_Get(_adlContextHandle, displayTarget.DisplayID.DisplayLogicalAdapterIndex, out numDisplays, out displayInfoBuffer, forceDetect);
                        if (ADLRet == ADL_STATUS.ADL_OK)
                        {
                            if (displayTarget.DisplayID.DisplayLogicalAdapterIndex == -1)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Display_DisplayInfo_Get returned information about all displaytargets connected to all AMD adapters.");
                                continue;
                            }
                            SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Display_DisplayInfo_Get returned information about all displaytargets connected to all AMD adapters.");
                        }
                        else if (ADLRet == ADL_STATUS.ADL_ERR_NULL_POINTER || ADLRet == ADL_STATUS.ADL_ERR_NOT_SUPPORTED)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Display_DisplayInfo_Get returned ADL_ERR_NULL_POINTER so skipping getting display info from all AMD adapters.");
                            continue;
                        }
                        else
                        {
                            SharedLogger.logger.Error($"AMDLibrary/PrintActiveConfig: ERROR - ADL2_Display_DisplayInfo_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from all AMD adapters in the computer.");
                        }

                        ADL_DISPLAY_INFO[] displayInfoArray = { };
                        if (numDisplays > 0)
                        {
                            IntPtr currentDisplayInfoBuffer = displayInfoBuffer;
                            displayInfoArray = new ADL_DISPLAY_INFO[numDisplays];
                            for (int i = 0; i < numDisplays; i++)
                            {
                                // build a structure in the array slot
                                displayInfoArray[i] = new ADL_DISPLAY_INFO();
                                // fill the array slot structure with the data from the buffer
                                displayInfoArray[i] = (ADL_DISPLAY_INFO)Marshal.PtrToStructure(currentDisplayInfoBuffer, typeof(ADL_DISPLAY_INFO));
                                // destroy the bit of memory we no longer need
                                Marshal.DestroyStructure(currentDisplayInfoBuffer, typeof(ADL_DISPLAY_INFO));
                                // advance the buffer forwards to the next object
                                currentDisplayInfoBuffer = (IntPtr)((long)currentDisplayInfoBuffer + Marshal.SizeOf(displayInfoArray[i]));
                                //currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));

                            }
                            // Free the memory used by the buffer                        
                            Marshal.FreeCoTaskMem(displayInfoBuffer);
                        }

                        // Now we need to get all the displays connected to this adapter so that we can get their HDR state
                        foreach (var displayInfoItem in displayInfoArray)
                        {

                            // Ignore the display if it isn't connected (note: we still need to see if it's actively mapped to windows!)
                            if (!displayInfoItem.DisplayConnectedSet)
                            {
                                continue;
                            }

                            // If the display is not mapped in windows then we only want to skip this display if all alldisplays is false
                            if (!displayInfoItem.DisplayMappedSet)
                            {
                                continue;
                            }

                            stringToReturn += $"\n****** AMD DISPLAY INFO *******\n";
                            stringToReturn += $"Display #{displayInfoItem.DisplayID.DisplayLogicalIndex}\n";
                            stringToReturn += $"Display connected via Adapter #{displayInfoItem.DisplayID.DisplayLogicalAdapterIndex}\n";
                            stringToReturn += $"Display Name: {displayInfoItem.DisplayName}\n";
                            stringToReturn += $"Display Manufacturer Name: {displayInfoItem.DisplayManufacturerName}\n";
                            stringToReturn += $"Display Type: {displayInfoItem.DisplayType.ToString("G")}\n";
                            stringToReturn += $"Display connector: {displayInfoItem.DisplayConnector.ToString("G")}\n";
                            stringToReturn += $"Display controller index: {displayInfoItem.DisplayControllerIndex}\n";
                            stringToReturn += $"Display Connected: {displayInfoItem.DisplayConnectedSet}\n";
                            stringToReturn += $"Display Mapped in Windows: {displayInfoItem.DisplayMappedSet}\n";
                            stringToReturn += $"Display Is Forcibly Enabled: {displayInfoItem.ForcibleSet}\n";
                            stringToReturn += $"Display GetLock is Set: {displayInfoItem.GenLockSet}\n";
                            stringToReturn += $"LDA Display is Set: {displayInfoItem.LDADisplaySet}\n";
                            stringToReturn += $"Display Configuration is stretched horizontally across two displays: {displayInfoItem.Manner2HStretchSet}\n";
                            stringToReturn += $"Display Configuration is stretched vertically across two displays: {displayInfoItem.Manner2VStretchSet}\n";
                            stringToReturn += $"Display Configuration is a clone of another display: {displayInfoItem.MannerCloneSet}\n";
                            stringToReturn += $"Display Configuration is an extension of another display: {displayInfoItem.MannerExtendedSet}\n";
                            stringToReturn += $"Display Configuration is an N Strech across 1 GPU: {displayInfoItem.MannerNStretch1GPUSet}\n";
                            stringToReturn += $"Display Configuration is an N Strech across more than one GPU: {displayInfoItem.MannerNStretchNGPUSet}\n";
                            stringToReturn += $"Display Configuration is a single display: {displayInfoItem.MannerSingleSet}\n";
                            stringToReturn += $"Display timing override: {displayInfoItem.ModeTimingOverrideSet}\n";
                            stringToReturn += $"Display has MultiVPU set: {displayInfoItem.MultiVPUSet}\n";
                            stringToReturn += $"Display has non-local set (it is a remote display): {displayInfoItem.NonLocalSet}\n";
                            stringToReturn += $"Display is a Show Type Projector: {displayInfoItem.ShowTypeProjectorSet}\n\n";

                            // Get some more Display Info (if we can!)
                            ADL_DDC_INFO2 ddcInfo;
                            ADLRet = ADLImport.ADL2_Display_DDCInfo2_Get(_adlContextHandle, displayInfoItem.DisplayID.DisplayLogicalAdapterIndex, displayInfoItem.DisplayID.DisplayLogicalIndex, out ddcInfo);
                            if (ADLRet == ADL_STATUS.ADL_OK)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Display_DDCInfo2_Get returned information about DDC Information for display {displayInfoItem.DisplayID.DisplayLogicalIndex} connected to AMD adapter {displayInfoItem.DisplayID.DisplayLogicalAdapterIndex}.");
                                if (ddcInfo.SupportsDDC == 1)
                                {
                                    // The display supports DDC and returned some data!
                                    SharedLogger.logger.Trace($"AMDLibrary/PrintActiveConfig: ADL2_Display_DDCInfo2_Get returned information about DDC Information for display {displayInfoItem.DisplayID.DisplayLogicalIndex} connected to AMD adapter {displayInfoItem.DisplayID.DisplayLogicalAdapterIndex}.");
                                    stringToReturn += $"DDC Information: \n";
                                    stringToReturn += $"- Display Name: {ddcInfo.DisplayName}\n";
                                    stringToReturn += $"- Display Manufacturer ID: {ddcInfo.ManufacturerID}\n";
                                    stringToReturn += $"- Display Product ID: {ddcInfo.ProductID}\n";
                                    stringToReturn += $"- Display Serial ID: {ddcInfo.SerialID}\n";
                                    stringToReturn += $"- Display FreeSync Flags: {ddcInfo.FreesyncFlags}\n";
                                    stringToReturn += $"- Display FreeSync HDR Supported: {ddcInfo.FreeSyncHDRSupported}\n";
                                    stringToReturn += $"- Display FreeSync HDR Backlight Supported: {ddcInfo.FreeSyncHDRBacklightSupported}\n";
                                    stringToReturn += $"- Display FreeSync HDR Local Dimming Supported: {ddcInfo.FreeSyncHDRLocalDimmingSupported}\n";
                                    stringToReturn += $"- Display is Digital Device: {ddcInfo.IsDigitalDevice}\n";
                                    stringToReturn += $"- Display is HDMI Audio Device: {ddcInfo.IsHDMIAudioDevice}\n";
                                    stringToReturn += $"- Display is Projector Device: {ddcInfo.IsProjectorDevice}\n";
                                    stringToReturn += $"- Display Supported Colourspace: {ddcInfo.SupportedColorSpace}\n";
                                    stringToReturn += $"- Display Supported HDR: {ddcInfo.SupportedHDR}\n";
                                    stringToReturn += $"- Display Supported Transfer Function: {ddcInfo.SupportedTransferFunction}\n";
                                    stringToReturn += $"- Display Supports AI: {ddcInfo.SupportsAI}\n";
                                    stringToReturn += $"- Display Supports DDC: {ddcInfo.SupportsDDC}\n";
                                    stringToReturn += $"- Display Supports DolbyVision: {ddcInfo.DolbyVisionSupported}\n";
                                    stringToReturn += $"- Display Supports CEA861_3: {ddcInfo.CEA861_3Supported}\n";
                                    stringToReturn += $"- Display Supports sxvYCC601: {ddcInfo.SupportsxvYCC601}\n";
                                    stringToReturn += $"- Display Supports sxvYCC709: {ddcInfo.SupportsxvYCC709}\n";
                                    stringToReturn += $"- Display Average Luminance Data: {ddcInfo.AvgLuminanceData}\n";
                                    stringToReturn += $"- Display Diffuse Screen Reflectance: {ddcInfo.DiffuseScreenReflectance}\n";
                                    stringToReturn += $"- Display Specular Screen Reflectance: {ddcInfo.SpecularScreenReflectance}\n";
                                    stringToReturn += $"- Display Max Backlight Min Luminance: {ddcInfo.MaxBacklightMinLuminanceData}\n";
                                    stringToReturn += $"- Display Max Backlight Max Luminance: {ddcInfo.MaxBacklightMaxLuminanceData}\n";
                                    stringToReturn += $"- Display Min Luminance: {ddcInfo.MinLuminanceData}\n";
                                    stringToReturn += $"- Display Max Luminance: {ddcInfo.MaxLuminanceData}\n";
                                    stringToReturn += $"- Display Min Backlight Min Luminance: {ddcInfo.MinBacklightMinLuminanceData}\n";
                                    stringToReturn += $"- Display Min Backlight Max Luminance: {ddcInfo.MinBacklightMaxLuminanceData}\n";
                                    stringToReturn += $"- Display Min Luminance No Dimming: {ddcInfo.MinLuminanceNoDimmingData}\n";
                                    stringToReturn += $"- Display Native Chromacity Red X: {ddcInfo.NativeDisplayChromaticityRedX}\n";
                                    stringToReturn += $"- Display Native Chromacity Red Y: {ddcInfo.NativeDisplayChromaticityRedY}\n";
                                    stringToReturn += $"- Display Native Chromacity Green X: {ddcInfo.NativeDisplayChromaticityGreenX}\n";
                                    stringToReturn += $"- Display Native Chromacity Green Y: {ddcInfo.NativeDisplayChromaticityGreenY}\n";
                                    stringToReturn += $"- Display Native Chromacity Blue X: {ddcInfo.NativeDisplayChromaticityBlueX}\n";
                                    stringToReturn += $"- Display Native Chromacity Blue Y: {ddcInfo.NativeDisplayChromaticityBlueY}\n";
                                    stringToReturn += $"- Display Native Chromacity White X: {ddcInfo.NativeDisplayChromaticityWhiteX}\n";
                                    stringToReturn += $"- Display Native Chromacity White Y: {ddcInfo.NativeDisplayChromaticityWhiteY}\n";
                                    stringToReturn += $"- Display Packed Pixel Supported: {ddcInfo.PackedPixelSupported}\n";
                                    stringToReturn += $"- Display Panel Pixel Format: {ddcInfo.PanelPixelFormat}\n";
                                    stringToReturn += $"- Display Pixel Format Limited Range: {ddcInfo.PixelFormatLimitedRange}\n";
                                    stringToReturn += $"- Display PTMCx: {ddcInfo.PTMCx}\n";
                                    stringToReturn += $"- Display PTMCy: {ddcInfo.PTMCy}\n";
                                    stringToReturn += $"- Display PTM Refresh Rate: {ddcInfo.PTMRefreshRate}\n";

                                    stringToReturn += $"- Display Serial ID: {ddcInfo.SerialID}\n";
                                }

                            }

                        }
                    }

                }

                stringToReturn += $"\n****** AMD EYEFINITY (SLS) *******\n";
                if (displayConfig.SlsConfig.IsSlsEnabled)
                {
                    stringToReturn += $"AMD Eyefinity is Enabled\n";
                    if (displayConfig.SlsConfig.SLSMapConfigs.Count > 1)
                    {
                        stringToReturn += $"There are {displayConfig.SlsConfig.SLSMapConfigs.Count} AMD Eyefinity (SLS) configurations in use.\n";
                    }
                    if (displayConfig.SlsConfig.SLSMapConfigs.Count == 1)
                    {
                        stringToReturn += $"There is 1 AMD Eyefinity (SLS) configurations in use.\n";
                    }
                    else
                    {
                        stringToReturn += $"There are no AMD Eyefinity (SLS) configurations in use.\n";
                    }

                    int count = 0;
                    foreach (var slsMap in displayConfig.SlsConfig.SLSMapConfigs)
                    {
                        stringToReturn += $"NOTE: This Eyefinity (SLS) screen will be treated as a single display by Windows.\n";
                        stringToReturn += $"The AMD Eyefinity (SLS) Grid Topology #{count} is {slsMap.SLSMap.Grid.SLSGridColumn} Columns x {slsMap.SLSMap.Grid.SLSGridRow} Rows\n";
                        stringToReturn += $"The AMD Eyefinity (SLS) Grid Topology #{count} involves {slsMap.SLSMap.NumSLSTarget} Displays\n";
                    }

                }
                else
                {
                    stringToReturn += $"AMD Eyefinity (SLS) is Disabled\n";
                }

            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/PrintActiveConfig: ERROR - Tried to run GetSomeDisplayIdentifiers but the AMD ADL library isn't initialised!");
                throw new AMDLibraryException($"Tried to run PrintActiveConfig but the AMD ADL library isn't initialised!");
            }



            stringToReturn += $"\n\n";
            // Now we also get the Windows CCD Library info, and add it to the above
            stringToReturn += WinLibrary.GetLibrary().PrintActiveConfig();

            return stringToReturn;
        }

        public bool SetActiveConfig(AMD_DISPLAY_CONFIG displayConfig)
        {

            if (_initialised)
            {
                // Set the initial state of the ADL_STATUS
                ADL_STATUS ADLRet = 0;

                // set the display locations
                if (displayConfig.SlsConfig.IsSlsEnabled)
                {
                    // We need to change to an Eyefinity (SLS) profile, so we need to apply the new SLS Topologies
                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: SLS is enabled in the new display configuration, so we need to set it");

                    foreach (AMD_SLSMAP_CONFIG slsMapConfig in displayConfig.SlsConfig.SLSMapConfigs)
                    {
                        // Attempt to turn on this SLS Map Config if it exists in the AMD Radeon driver config database
                        ADLRet = ADLImport.ADL2_Display_SLSMapConfig_SetState(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex, slsMapConfig.SLSMap.SLSMapIndex, ADLImport.ADL_TRUE);
                        if (ADLRet == ADL_STATUS.ADL_OK)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_SetState successfully set the SLSMAP with index {slsMapConfig.SLSMap.SLSMapIndex} to TRUE for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                        }
                        else
                        {
                            SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_SetState returned ADL_STATUS {ADLRet} when trying to set the SLSMAP with index {slsMapConfig.SLSMap.SLSMapIndex} to TRUE for adapter {slsMapConfig.SLSMap.AdapterIndex}.");

                            // If we get an error with just tturning it on, then we need to actually try to created a new Eyefinity map and then enable it
                            // If we reach this stage, then the user has discarded the AMD Eyefinity mode in AMD due to a bad UI design, and we need to work around that slight issue.
                            // (BTW that's FAR to easy to do in the AMD Radeon GUI)
                            // NOTE: There is a slight issue with way of doing things. Although we create a much more robust way of working, we also will never ever actually use the Eyefinity config as saved.
                            //       Instead, we will always drop through to creating an Eyefinity config each time, the only saving grace being that the AMD Driver is smart enough to notice this and it will reuse the same SLSMapIndex number.
                            //       This at least means that we won't keep filling the AMD Driver up with additional EYefinity configs! It will instaed only add one more additional AMD Config if it works this way.

                            int supportedSLSLayoutImageMode;
                            int reasonForNotSupportSLS;
                            ADLRet = ADLImport.ADL2_Display_SLSMapConfig_Valid(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex, slsMapConfig.SLSMap, slsMapConfig.SLSTargets.Count, slsMapConfig.SLSTargets.ToArray(), out supportedSLSLayoutImageMode, out reasonForNotSupportSLS, ADLImport.ADL_DISPLAY_SLSMAPCONFIG_CREATE_OPTION_RELATIVETO_CURRENTANGLE);
                            if (ADLRet == ADL_STATUS.ADL_OK)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_Valid successfully validated a new SLSMAP config for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                            }
                            else
                            {
                                SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_Valid returned ADL_STATUS {ADLRet} when trying to create a new SLSMAP for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                return false;
                            }

                            // Create and apply the new SLSMap
                            int newSlsMapIndex;
                            ADLRet = ADLImport.ADL2_Display_SLSMapConfig_Create(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex, slsMapConfig.SLSMap, slsMapConfig.SLSTargets.Count, slsMapConfig.SLSTargets.ToArray(), slsMapConfig.BezelModePercent, out newSlsMapIndex, ADLImport.ADL_DISPLAY_SLSMAPCONFIG_CREATE_OPTION_RELATIVETO_CURRENTANGLE);
                            if (ADLRet == ADL_STATUS.ADL_OK)
                            {
                                if (newSlsMapIndex != -1)
                                {
                                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_Create successfully created the new SLSMAP we just created with index {newSlsMapIndex} to TRUE for adapter {slsMapConfig.SLSMap.AdapterIndex}.");

                                    // At this point we have created a new AMD Eyefinity Config
                                }
                                else
                                {
                                    SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_Create returned ADL_STATUS {ADLRet} but the returned SLSMapIndex was -1, which indicates that the new SLSMAP failed to create for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_Create returned ADL_STATUS {ADLRet} when trying to create a new SLSMAP for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                return false;
                            }

                        }

                    }

                }
                else
                {
                    // We need to change to a plain, non-Eyefinity (SLS) profile, so we need to disable any SLS Topologies if they are being used
                    SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: SLS is not used in the new display configuration, so we need to set it to disabled if it's configured currently");

                    if (ActiveDisplayConfig.SlsConfig.IsSlsEnabled)
                    {
                        // We need to disable the current Eyefinity (SLS) profile to turn it off
                        SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: SLS is enabled in the current display configuration, so we need to turn it off");

                        foreach (AMD_SLSMAP_CONFIG slsMapConfig in ActiveDisplayConfig.SlsConfig.SLSMapConfigs)
                        {
                            // Turn off this SLS Map Config
                            ADLRet = ADLImport.ADL2_Display_SLSMapConfig_SetState(_adlContextHandle, slsMapConfig.SLSMap.AdapterIndex, slsMapConfig.SLSMap.SLSMapIndex, ADLImport.ADL_FALSE);
                            if (ADLRet == ADL_STATUS.ADL_OK)
                            {
                                SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_SetState successfully disabled the SLSMAP with index {slsMapConfig.SLSMap.SLSMapIndex} for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                            }
                            else
                            {
                                SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_SetState returned ADL_STATUS {ADLRet} when trying to set the SLSMAP with index {slsMapConfig.SLSMap.SLSMapIndex} to FALSE for adapter {slsMapConfig.SLSMap.AdapterIndex}.");
                                return false;
                            }

                        }
                    }

                }

            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - Tried to run SetActiveConfig but the AMD ADL library isn't initialised!");
                throw new AMDLibraryException($"Tried to run SetActiveConfig but the AMD ADL library isn't initialised!");
            }

            return true;
        }


        public bool SetActiveConfigOverride(AMD_DISPLAY_CONFIG displayConfig)
        {
            if (_initialised)
            {
                // Set the initial state of the ADL_STATUS
                ADL_STATUS ADLRet = 0;

                // We want to set the AMD HDR settings now
                // We got through each of the attached displays and set the HDR

                // Go through each of the HDR configs we have
                foreach (var hdrConfig in displayConfig.HdrConfigs)
                {
                    // Try and find the HDR config displays in the list of currently connected displays
                    foreach (var displayInfoItem in ActiveDisplayConfig.DisplayTargets)
                    {
                        try
                        {
                            // If we find the HDR config display in the list of currently connected displays then try to set the HDR setting we recorded earlier
                            if (hdrConfig.Key == displayInfoItem.DisplayID.DisplayLogicalIndex)
                            {
                                if (hdrConfig.Value.HDREnabled)
                                {
                                    ADLRet = ADLImport.ADL2_Display_HDRState_Set(_adlContextHandle, hdrConfig.Value.AdapterIndex, displayInfoItem.DisplayID, 1);
                                    if (ADLRet == ADL_STATUS.ADL_OK)
                                    {
                                        SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: ADL2_Display_HDRState_Set was able to turn on HDR for display {displayInfoItem.DisplayID.DisplayLogicalIndex}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Error($"AMDLibrary/SetActiveConfigOverride: ADL2_Display_HDRState_Set was NOT able to turn on HDR for display {displayInfoItem.DisplayID.DisplayLogicalIndex}.");
                                    }
                                }
                                else
                                {
                                    ADLRet = ADLImport.ADL2_Display_HDRState_Set(_adlContextHandle, hdrConfig.Value.AdapterIndex, displayInfoItem.DisplayID, 0);
                                    if (ADLRet == ADL_STATUS.ADL_OK)
                                    {
                                        SharedLogger.logger.Trace($"AMDLibrary/SetActiveConfigOverride: ADL2_Display_HDRState_Set was able to turn off HDR for display {displayInfoItem.DisplayID.DisplayLogicalIndex}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Error($"AMDLibrary/SetActiveConfigOverride: ADL2_Display_HDRState_Set was NOT able to turn off HDR for display {displayInfoItem.DisplayID.DisplayLogicalIndex}.");
                                    }
                                }
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"AMDLibrary/GetAMDDisplayConfig: Exception! ADL2_Display_HDRState_Set was NOT able to change HDR for display {displayInfoItem.DisplayID.DisplayLogicalIndex}.");
                            continue;
                        }
                    }

                }
            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/SetActiveConfig: ERROR - Tried to run SetActiveConfigOverride but the AMD ADL library isn't initialised!");
                throw new AMDLibraryException($"Tried to run SetActiveConfigOverride but the AMD ADL library isn't initialised!");
            }
            return true;
        }



        public bool IsActiveConfig(AMD_DISPLAY_CONFIG displayConfig)
        {

            // Check whether the display config is in use now
            SharedLogger.logger.Trace($"AMDLibrary/IsActiveConfig: Checking whether the display configuration is already being used.");
            if (displayConfig.Equals(_activeDisplayConfig))
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsActiveConfig: The display configuration is already being used (supplied displayConfig Equals currentWindowsDisplayConfig)");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsActiveConfig: The display configuration is NOT currently in use (supplied displayConfig Equals currentWindowsDisplayConfig)");
                return false;
            }

        }

        public bool IsValidConfig(AMD_DISPLAY_CONFIG displayConfig)
        {
            // We want to check the AMD Eyefinity (SLS) config is valid
            SharedLogger.logger.Trace($"AMDLibrary/IsValidConfig: Testing whether the display configuration is valid");
            // 
            if (displayConfig.SlsConfig.IsSlsEnabled)
            {
                // At the moment we just assume the config is true so we try to use it
                return true;
            }
            else
            {
                // Its not a Mosaic topology, so we just let it pass, as it's windows settings that matter.
                return true;
            }
        }

        public bool IsPossibleConfig(AMD_DISPLAY_CONFIG displayConfig)
        {
            // We want to check the AMD profile can be used now
            SharedLogger.logger.Trace($"AMDLibrary/IsPossibleConfig: Testing whether the AMD display configuration is possible to be used now");

            // If both display identifiers are 0 then no displays were connected via AMD and we should just return true.
            if (displayConfig.DisplayIdentifiers.Count == 0 && _allConnectedDisplayIdentifiers.Count == 0)
            {
                return true;
            }
            // but if only allconnected count is 0 then we have a problem
            else if (_allConnectedDisplayIdentifiers.Count == 0)
            {
                return false;
            }

            // Otherwise we need to actuially check through things
            // Check that we have all the displayConfig DisplayIdentifiers we need available now            
            if (displayConfig.DisplayIdentifiers.All(value => _allConnectedDisplayIdentifiers.Contains(value)))
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsPossibleConfig: Success! The AMD display configuration is possible to be used now");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"AMDLibrary/IsPossibleConfig: Uh oh! The AMDdisplay configuration is possible cannot be used now");
                return false;
            }
        }

        public List<string> GetCurrentDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"AMDLibrary/GetCurrentDisplayIdentifiers: Getting the current display identifiers for the displays in use now");
            bool allDisplays = false;
            return GetSomeDisplayIdentifiers(allDisplays);
        }

        public List<string> GetAllConnectedDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"AMDLibrary/GetAllConnectedDisplayIdentifiers: Getting all the display identifiers that can possibly be used");
            bool allDisplays = true;
            _allConnectedDisplayIdentifiers = GetSomeDisplayIdentifiers(allDisplays);

            return _allConnectedDisplayIdentifiers;
        }

        private List<string> GetSomeDisplayIdentifiers(bool allDisplays = false)
        {
            SharedLogger.logger.Debug($"AMDLibrary/GetSomeDisplayIdentifiers: Generating unique Display Identifiers");

            List<string> displayIdentifiers = new List<string>();

            if (_initialised)
            {
                // Get the number of AMD adapters that the OS knows about
                int numAdapters = 0;
                ADL_STATUS ADLRet = ADLImport.ADL2_Adapter_NumberOfAdapters_Get(_adlContextHandle, out numAdapters);
                if (ADLRet == ADL_STATUS.ADL_OK)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: ADL2_Adapter_NumberOfAdapters_Get returned the number of AMD Adapters the OS knows about ({numAdapters}).");
                }
                else
                {
                    SharedLogger.logger.Error($"AMDLibrary/GetSomeDisplayIdentifiers: ERROR - ADL2_Adapter_NumberOfAdapters_Get returned ADL_STATUS {ADLRet} when trying to get number of AMD adapters in the computer.");
                    throw new AMDLibraryException($"GetSomeDisplayIdentifiers returned ADL_STATUS {ADLRet} when trying to get number of AMD adapters in the computer");
                }

                // Figure out primary adapter
                int primaryAdapterIndex = 0;
                ADLRet = ADLImport.ADL2_Adapter_Primary_Get(_adlContextHandle, out primaryAdapterIndex);
                if (ADLRet == ADL_STATUS.ADL_OK)
                {
                    SharedLogger.logger.Trace($"AMDLibrary/ADL2_Adapter_Primary_Get: The primary adapter has index {primaryAdapterIndex}.");
                }
                else
                {
                    SharedLogger.logger.Error($"AMDLibrary/GetSomeDisplayIdentifiers: ERROR - ADL2_Adapter_Primary_Get returned ADL_STATUS {ADLRet} when trying to get the primary adapter info from all the AMD adapters in the computer.");
                    throw new AMDLibraryException($"GetSomeDisplayIdentifiers returned ADL_STATUS {ADLRet} when trying to get the adapter info from all the AMD adapters in the computer");
                }

                // Now go through each adapter and get the information we need from it
                for (int adapterIndex = 0; adapterIndex < numAdapters; adapterIndex++)
                {
                    // Skip this adapter if it isn't active
                    int adapterActiveStatus = ADLImport.ADL_FALSE;
                    ADLRet = ADLImport.ADL2_Adapter_Active_Get(_adlContextHandle, adapterIndex, out adapterActiveStatus);
                    if (ADLRet == ADL_STATUS.ADL_OK)
                    {
                        if (adapterActiveStatus == ADLImport.ADL_TRUE)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: ADL2_Adapter_Active_Get returned ADL_TRUE - AMD Adapter #{adapterIndex} is active! We can continue.");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: ADL2_Adapter_Active_Get returned ADL_FALSE - AMD Adapter #{adapterIndex} is NOT active, so skipping.");
                            continue;
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"AMDLibrary/GetSomeDisplayIdentifiers: WARNING - ADL2_Adapter_Active_Get returned ADL_STATUS {ADLRet} when trying to see if AMD Adapter #{adapterIndex} is active. Trying to skip this adapter so something at least works.");
                        continue;
                    }

                    // Get the Adapter info for this adapter and put it in the AdapterBuffer
                    SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: Running ADL2_Adapter_AdapterInfoX4_Get to get the information about AMD Adapter #{adapterIndex}.");
                    int numAdaptersInfo = 0;
                    IntPtr adapterInfoBuffer = IntPtr.Zero;
                    ADLRet = ADLImport.ADL2_Adapter_AdapterInfoX4_Get(_adlContextHandle, adapterIndex, out numAdaptersInfo, out adapterInfoBuffer);
                    if (ADLRet == ADL_STATUS.ADL_OK)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: ADL2_Adapter_AdapterInfoX4_Get returned information about AMD Adapter #{adapterIndex}.");
                    }
                    else
                    {
                        SharedLogger.logger.Error($"AMDLibrary/GetSomeDisplayIdentifiers: ERROR - ADL2_Adapter_AdapterInfoX4_Get returned ADL_STATUS {ADLRet} when trying to get the adapter info from AMD Adapter #{adapterIndex}. Trying to skip this adapter so something at least works.");
                        continue;
                    }

                    ADL_ADAPTER_INFOX2[] adapterArray = new ADL_ADAPTER_INFOX2[numAdaptersInfo];
                    if (numAdaptersInfo > 0)
                    {
                        IntPtr currentDisplayTargetBuffer = adapterInfoBuffer;
                        for (int i = 0; i < numAdaptersInfo; i++)
                        {
                            // build a structure in the array slot
                            adapterArray[i] = new ADL_ADAPTER_INFOX2();
                            // fill the array slot structure with the data from the buffer
                            adapterArray[i] = (ADL_ADAPTER_INFOX2)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                            // destroy the bit of memory we no longer need
                            //Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_ADAPTER_INFOX2));
                            // advance the buffer forwards to the next object
                            currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(adapterArray[i]));
                        }
                        // Free the memory used by the buffer                        
                        Marshal.FreeCoTaskMem(adapterInfoBuffer);
                    }

                    SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: Converted ADL2_Adapter_AdapterInfoX4_Get memory buffer into a {adapterArray.Length} long array about AMD Adapter #{adapterIndex}.");

                    //AMD_ADAPTER_CONFIG savedAdapterConfig = new AMD_ADAPTER_CONFIG();
                    ADL_ADAPTER_INFOX2 oneAdapter = adapterArray[0];
                    if (oneAdapter.Exist != 1)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} doesn't exist at present so skipping detection for this adapter.");
                        continue;
                    }

                    // Only skip non-present displays if we want all displays information
                    if (allDisplays && oneAdapter.Present != 1)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetSomeDisplayIdentifiers: AMD Adapter #{oneAdapter.AdapterIndex.ToString()} isn't enabled at present so skipping detection for this adapter.");
                        continue;
                    }

                    // Now we still try to get the information we need for the Display Identifiers
                    // Go grab the DisplayMaps and DisplayTargets as that is useful infor for creating screens
                    int numDisplayTargets = 0;
                    int numDisplayMaps = 0;
                    IntPtr displayTargetBuffer = IntPtr.Zero;
                    IntPtr displayMapBuffer = IntPtr.Zero;
                    ADLRet = ADLImport.ADL2_Display_DisplayMapConfig_Get(_adlContextHandle, adapterIndex, out numDisplayMaps, out displayMapBuffer, out numDisplayTargets, out displayTargetBuffer, ADLImport.ADL_DISPLAY_DISPLAYMAP_OPTION_GPUINFO);
                    if (ADLRet == ADL_STATUS.ADL_OK)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_DisplayMapConfig_Get returned information about all displaytargets connected to AMD adapter {adapterIndex}.");
                    }
                    else
                    {
                        SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_DisplayMapConfig_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter {adapterIndex} in the computer.");
                        continue;
                    }

                    ADL_DISPLAY_TARGET[] displayTargetArray = { };
                    if (numDisplayTargets > 0)
                    {
                        IntPtr currentDisplayTargetBuffer = displayTargetBuffer;
                        //displayTargetArray = new ADL_DISPLAY_TARGET[numDisplayTargets];
                        displayTargetArray = new ADL_DISPLAY_TARGET[numDisplayTargets];
                        for (int i = 0; i < numDisplayTargets; i++)
                        {
                            // build a structure in the array slot
                            displayTargetArray[i] = new ADL_DISPLAY_TARGET();
                            //displayTargetArray[i] = new ADL_DISPLAY_TARGET();
                            // fill the array slot structure with the data from the buffer
                            displayTargetArray[i] = (ADL_DISPLAY_TARGET)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                            //displayTargetArray[i] = (ADL_DISPLAY_TARGET)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                            // destroy the bit of memory we no longer need
                            Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                            // advance the buffer forwards to the next object
                            currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));
                            //currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));

                        }
                        // Free the memory used by the buffer                        
                        Marshal.FreeCoTaskMem(displayTargetBuffer);
                    }

                    int forceDetect = 0;
                    int numDisplays;
                    IntPtr displayInfoBuffer;
                    ADLRet = ADLImport.ADL2_Display_DisplayInfo_Get(_adlContextHandle, adapterIndex, out numDisplays, out displayInfoBuffer, forceDetect);
                    if (ADLRet == ADL_STATUS.ADL_OK)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_DisplayInfo_Get returned information about all displaytargets connected to AMD adapter {adapterIndex}.");
                    }
                    else if (ADLRet == ADL_STATUS.ADL_ERR_NULL_POINTER || ADLRet == ADL_STATUS.ADL_ERR_NOT_SUPPORTED)
                    {
                        SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_DisplayInfo_Get returned ADL_ERR_NULL_POINTER so skipping getting display info from this AMD adapter {adapterIndex}.");
                        continue;
                    }
                    else
                    {
                        SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_DisplayInfo_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter {adapterIndex} in the computer.");
                    }

                    ADL_DISPLAY_INFO[] displayInfoArray = { };
                    if (numDisplays > 0)
                    {
                        IntPtr currentDisplayInfoBuffer = displayInfoBuffer;
                        displayInfoArray = new ADL_DISPLAY_INFO[numDisplays];
                        for (int i = 0; i < numDisplays; i++)
                        {
                            // build a structure in the array slot
                            displayInfoArray[i] = new ADL_DISPLAY_INFO();
                            // fill the array slot structure with the data from the buffer
                            displayInfoArray[i] = (ADL_DISPLAY_INFO)Marshal.PtrToStructure(currentDisplayInfoBuffer, typeof(ADL_DISPLAY_INFO));
                            // destroy the bit of memory we no longer need
                            Marshal.DestroyStructure(currentDisplayInfoBuffer, typeof(ADL_DISPLAY_INFO));
                            // advance the buffer forwards to the next object
                            currentDisplayInfoBuffer = (IntPtr)((long)currentDisplayInfoBuffer + Marshal.SizeOf(displayInfoArray[i]));
                            //currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));

                        }
                        // Free the memory used by the buffer                        
                        Marshal.FreeCoTaskMem(displayInfoBuffer);
                    }


                    // Now we need to get all the displays connected to this adapter so that we can get their HDR state
                    foreach (var displayInfoItem in displayInfoArray)
                    {

                        // Ignore the display if it isn't connected (note: we still need to see if it's actively mapped to windows!)
                        if (!displayInfoItem.DisplayConnectedSet)
                        {
                            continue;
                        }

                        // If the display is not mapped in windows then we only want to skip this display if all alldisplays is false
                        if (!displayInfoItem.DisplayMappedSet && !allDisplays)
                        {
                            continue;
                        }

                        // Create an array of all the important display info we need to create the display identifier
                        List<string> displayInfo = new List<string>();
                        displayInfo.Add("AMD");
                        try
                        {
                            displayInfo.Add(oneAdapter.DeviceNumber.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting AMD Adapter Device Number from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(oneAdapter.AdapterName);
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting AMD Adapter Name from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(displayInfoItem.DisplayConnector.ToString("G"));
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting Display Connector from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }

                        // Get some more Display Info (if we can!)
                        ADL_DDC_INFO2 ddcInfo = new ADL_DDC_INFO2();
                        ADLRet = ADLImport.ADL2_Display_DDCInfo2_Get(_adlContextHandle, adapterIndex, displayInfoItem.DisplayID.DisplayLogicalIndex, out ddcInfo);
                        if (ADLRet == ADL_STATUS.ADL_OK)
                        {
                            SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_DDCInfo2_Get returned information about DDC Information for display {displayInfoItem.DisplayID.DisplayLogicalIndex} connected to AMD adapter {adapterIndex}.");
                            if (ddcInfo.SupportsDDC == 1)
                            {
                                // The display supports DDC and returned some data!
                                SharedLogger.logger.Trace($"AMDLibrary/GetAMDDisplayConfig: ADL2_Display_DDCInfo2_Get returned information about DDC Information for display {displayInfoItem.DisplayID.DisplayLogicalIndex} connected to AMD adapter {adapterIndex}.");

                                try
                                {
                                    displayInfo.Add(ddcInfo.ManufacturerID.ToString());
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting AMD Display EDID Manufacturer Code from video card. Substituting with a # instead");
                                    displayInfo.Add("#");
                                }
                                try
                                {
                                    displayInfo.Add(ddcInfo.ProductID.ToString());
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting AMD Display EDID Product Code from video card. Substituting with a # instead");
                                    displayInfo.Add("#");
                                }
                                try
                                {
                                    displayInfo.Add(ddcInfo.DisplayName.ToString());
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting AMD Display Name from video card. Substituting with a # instead");
                                    displayInfo.Add("#");
                                }
                            }
                            else
                            {
                                // The display does NOT support DDC and nothing was returned! We need to find the information some other way!

                                try
                                {
                                    displayInfo.Add(displayInfoItem.DisplayManufacturerName.ToString());
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting AMD Display Manufacturer Name 2 from video card. Substituting with a # instead");
                                    displayInfo.Add("#");
                                }
                                try
                                {
                                    displayInfo.Add(displayInfoItem.DisplayName.ToString());
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting AMD Display Name 2 from video card. Substituting with a # instead");
                                    displayInfo.Add("#");
                                }
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Error($"AMDLibrary/GetAMDDisplayConfig: ERROR - ADL2_Display_DDCInfo2_Get returned ADL_STATUS {ADLRet} when trying to get the display target info from AMD adapter {adapterIndex} in the computer.");

                            // ADL2_Display_DDCInfo2_Get had a problem and nothing was returned! We need to find the information some other way!

                            try
                            {
                                displayInfo.Add(displayInfoItem.DisplayManufacturerName.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting AMD Display Manufacturer Name 2 from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }
                            try
                            {
                                displayInfo.Add(displayInfoItem.DisplayName.ToString());
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"AMDLibrary/GetSomeDisplayIdentifiers: Exception getting AMD Display Name 2 from video card. Substituting with a # instead");
                                displayInfo.Add("#");
                            }
                        }



                        // Create a display identifier out of it
                        string displayIdentifier = String.Join("|", displayInfo);
                        // Add it to the list of display identifiers so we can return it
                        // but only add it if it doesn't already exist. Otherwise we get duplicates :/
                        if (!displayIdentifiers.Contains(displayIdentifier))
                        {
                            displayIdentifiers.Add(displayIdentifier);
                            SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                        }
                    }
                }
            }
            else
            {
                SharedLogger.logger.Error($"AMDLibrary/GetSomeDisplayIdentifiers: ERROR - Tried to run GetSomeDisplayIdentifiers but the AMD ADL library isn't initialised!");
                throw new AMDLibraryException($"Tried to run GetSomeDisplayIdentifiers but the AMD ADL library isn't initialised!");
            }


            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
        }

    }

    [global::System.Serializable]
    public class AMDLibraryException : Exception
    {
        public AMDLibraryException() { }
        public AMDLibraryException(string message) : base(message) { }
        public AMDLibraryException(string message, Exception inner) : base(message, inner) { }
        protected AMDLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}