using DisplayMagicianShared.Windows;
using EDIDParser;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using Windows.Devices.I2c.Provider;
using Windows.Graphics;
using Windows.Storage.Provider;
using static DisplayMagicianShared.NVIDIA.DisplayTopologyStatus;

namespace DisplayMagicianShared.NVIDIA
{

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_MOSAIC_CONFIG : IEquatable<NVIDIA_MOSAIC_CONFIG>
    {
        public bool IsMosaicEnabled;
        public TopologyBrief  MosaicTopologyBrief;
        public DisplaySettingsV2 MosaicDisplaySettings;
        public Int32 OverlapX;
        public Int32 OverlapY;
        public GridTopologyV2[] MosaicGridTopos;
        public UInt32 MosaicGridCount;

        public override bool Equals(object obj) => obj is NVIDIA_MOSAIC_CONFIG other && this.Equals(other);

        public bool Equals(NVIDIA_MOSAIC_CONFIG other)
        => IsMosaicEnabled == other.IsMosaicEnabled &&
           MosaicTopologyBrief.Equals(other.MosaicTopologyBrief) &&
           MosaicDisplaySettings.Equals(other.MosaicDisplaySettings) &&
           OverlapX == other.OverlapX &&
           OverlapY == other.OverlapY &&
           MosaicGridTopos.SequenceEqual(other.MosaicGridTopos) &&
           MosaicGridCount == other.MosaicGridCount;

        public override int GetHashCode()
        {
            return (IsMosaicEnabled, MosaicTopologyBrief, MosaicDisplaySettings, OverlapX, OverlapY, MosaicGridTopos, MosaicGridCount).GetHashCode();
        }
        public static bool operator ==(NVIDIA_MOSAIC_CONFIG lhs, NVIDIA_MOSAIC_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_MOSAIC_CONFIG lhs, NVIDIA_MOSAIC_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_PER_DISPLAY_CONFIG : IEquatable<NVIDIA_PER_DISPLAY_CONFIG>
    {
        public bool HasNvHdrEnabled;
        public IHDRCapabilities HdrCapabilities;
        public IHDRColorData HdrColorData;
        public bool HasAdaptiveSync;
        public SetAdaptiveSyncData AdaptiveSyncConfig;
        public bool HasColorData;
        public IColorData ColorData;
        public bool HasCustomDisplay;
        public List<CustomDisplay> CustomDisplays;


        public override bool Equals(object obj) => obj is NVIDIA_PER_DISPLAY_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_PER_DISPLAY_CONFIG other)
        => HasNvHdrEnabled == other.HasNvHdrEnabled &&
            HdrCapabilities.Equals(other.HdrCapabilities) &&
            HdrColorData.Equals(other.HdrColorData) &&
            // Disabled the Adaptive Sync equality matching as we are having trouble applying it, which is causing issues in profile matching in DisplayMagician
            // To fix this bit, we need to test the SetActiveConfigOverride Adaptive Sync part of the codebase to apply this properly.
            // But for now, we'll exclude it from the equality matching and also stop trying to use the adaptive sync config.
            //HasAdaptiveSync == other.HasAdaptiveSync &&
            //AdaptiveSyncConfig.Equals(other.AdaptiveSyncConfig) &&
            HasColorData == other.HasColorData &&
            ColorData.Equals(other.ColorData) &&
            HasCustomDisplay == other.HasCustomDisplay &&
            CustomDisplays.SequenceEqual(other.CustomDisplays);

        public override int GetHashCode()
        {
            // Disabled the Adaptive Sync equality matching as we are having trouble applying it, which is causing issues in profile matching in DisplayMagician
            // To fix this bit, we need to test the SetActiveConfigOverride Adaptive Sync part of the codebase to apply this properly.
            // But for now, we'll exclude it from the equality matching and also stop trying to use the adaptive sync config.
            //return (HasNvHdrEnabled, HdrCapabilities, HdrColorData, HasAdaptiveSync, AdaptiveSyncConfig, HasColorData, ColorData, HasCustomDisplay, CustomDisplays).GetHashCode();
            return (HasNvHdrEnabled, HdrCapabilities, HdrColorData, HasColorData, ColorData, HasCustomDisplay, CustomDisplays).GetHashCode();
        }
        public static bool operator ==(NVIDIA_PER_DISPLAY_CONFIG lhs, NVIDIA_PER_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_PER_DISPLAY_CONFIG lhs, NVIDIA_PER_DISPLAY_CONFIG rhs) => !(lhs == rhs);
    }


    /*[StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_CUSTOM_DISPLAY_CONFIG : IEquatable<NVIDIA_CUSTOM_DISPLAY_CONFIG>
    {
        public List<NV_CUSTOM_DISPLAY_V1> CustomDisplay;

        public override bool Equals(object obj) => obj is NVIDIA_CUSTOM_DISPLAY_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_CUSTOM_DISPLAY_CONFIG other)
        => CustomDisplay.SequenceEqual(other.CustomDisplay);

        public override int GetHashCode()
        {
            return (CustomDisplay).GetHashCode();
        }
        public static bool operator ==(NVIDIA_CUSTOM_DISPLAY_CONFIG lhs, NVIDIA_CUSTOM_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_CUSTOM_DISPLAY_CONFIG lhs, NVIDIA_CUSTOM_DISPLAY_CONFIG rhs) => !(lhs == rhs);
    }*/

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_DRS_CONFIG : IEquatable<NVIDIA_DRS_CONFIG>
    {
        //public bool HasDRSSettings;
        public bool IsBaseProfile;
        public DRSProfileV1 ProfileInfo;
        public List<DRSSettingV1> DriverSettings;

        public override bool Equals(object obj) => obj is NVIDIA_DRS_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_DRS_CONFIG other)
        => IsBaseProfile == other.IsBaseProfile &&
            ProfileInfo == other.ProfileInfo &&
            DriverSettings.SequenceEqual(other.DriverSettings);

        public override int GetHashCode()
        {
            return (IsBaseProfile, ProfileInfo, DriverSettings).GetHashCode();
            //return (HasDRSSettings, ProfileInfo, DriverSettings).GetHashCode();
        }
        public static bool operator ==(NVIDIA_DRS_CONFIG lhs, NVIDIA_DRS_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_DRS_CONFIG lhs, NVIDIA_DRS_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_PER_ADAPTER_CONFIG : IEquatable<NVIDIA_PER_ADAPTER_CONFIG>
    {
        public bool IsQuadro;
        public bool HasLogicalGPU;
        public SystemType SystemType;        
        public string AdapterName;
        public GPUType GPUType;
        public GPUBusType BusType;
        public Int32 BusId;
        public Int32 BusSlotId;
        public UInt32 DisplayCount;
        public Dictionary<UInt32, NVIDIA_PER_DISPLAY_CONFIG> Displays;

        public override bool Equals(object obj) => obj is NVIDIA_PER_ADAPTER_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_PER_ADAPTER_CONFIG other)
        => IsQuadro == other.IsQuadro &&
            HasLogicalGPU == other.HasLogicalGPU &&
            SystemType == other.SystemType &&
            AdapterName.Equals(other.AdapterName) &&
            GPUType == other.GPUType &&
            BusType == other.BusType &&
            BusId == other.BusId &&
            BusSlotId == other.BusSlotId &&
            DisplayCount == other.DisplayCount &&
            NVIDIALibrary.EqualButDifferentOrder<uint, NVIDIA_PER_DISPLAY_CONFIG>(Displays, other.Displays);

        public override int GetHashCode()
        {
            return (IsQuadro, HasLogicalGPU, SystemType, AdapterName, GPUType, BusType, BusId, BusSlotId, DisplayCount, Displays).GetHashCode();
        }
        public static bool operator ==(NVIDIA_PER_ADAPTER_CONFIG lhs, NVIDIA_PER_ADAPTER_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_PER_ADAPTER_CONFIG lhs, NVIDIA_PER_ADAPTER_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_DISPLAY_CONFIG : IEquatable<NVIDIA_DISPLAY_CONFIG>
    {
        public bool IsInUse;
        public bool IsCloned;
        public NVIDIA_MOSAIC_CONFIG MosaicConfig;
        public Dictionary<UInt32, NVIDIA_PER_ADAPTER_CONFIG> PhysicalAdapters;
        public List<PathInfoV2> DisplayConfigs;
        public List<NVIDIA_DRS_CONFIG> DRSSettings;
        // Note: We purposely have left out the DisplayNames from the Equals as it's order keeps changing after each reboot and after each profile swap
        // and it is informational only and doesn't contribute to the configuration (it's used for generating the Screens structure, and therefore for
        // generating the profile icon.
        public Dictionary<string, string> DisplayNames;
        public List<string> DisplayIdentifiers;

        public override bool Equals(object obj) => obj is NVIDIA_DISPLAY_CONFIG other && this.Equals(other);

        public bool Equals(NVIDIA_DISPLAY_CONFIG other)
        {
            if (!(IsInUse == other.IsInUse && 
            IsCloned == other.IsCloned &&
            PhysicalAdapters.SequenceEqual(other.PhysicalAdapters) &&
            MosaicConfig.Equals(other.MosaicConfig) &&
            DRSSettings.SequenceEqual(other.DRSSettings) &&
            DisplayIdentifiers.SequenceEqual(other.DisplayIdentifiers)))
            {
                return false;
            }

            // Now we need to go through the display configs comparing values, as the order changes if there is a cloned display
            if (!NVIDIALibrary.EqualButDifferentOrder<PathInfoV2>(DisplayConfigs, other.DisplayConfigs))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return (IsInUse, IsCloned, MosaicConfig, PhysicalAdapters, DisplayConfigs, DisplayIdentifiers, DRSSettings).GetHashCode();
        }
        public static bool operator ==(NVIDIA_DISPLAY_CONFIG lhs, NVIDIA_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_DISPLAY_CONFIG lhs, NVIDIA_DISPLAY_CONFIG rhs) => !(lhs == rhs);
    }

    public class NVIDIALibrary : IDisposable
    {

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr LoadLibrary(string lpFileName);

        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static NVIDIALibrary _instance = new NVIDIALibrary();

        private bool _initialised = false;
        private NVIDIA_DISPLAY_CONFIG _activeDisplayConfig;
        public List<MonitorConnectionType> SkippedColorConnectionTypes;
        public List<string> _allConnectedDisplayIdentifiers;
        public List<uint> _allConnectedDisplayIds = new List<uint>();
        private bool _mosaic_supported = true;

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

        static NVIDIALibrary() { }
        public NVIDIALibrary()
        {
            // Populate the list of ConnectionTypes we want to skip as they don't support querying
            SkippedColorConnectionTypes = new List<MonitorConnectionType> {
                MonitorConnectionType.VGA,
                MonitorConnectionType.Component,
                MonitorConnectionType.Composite,
                MonitorConnectionType.SVideo,
                MonitorConnectionType.DVI,
            };

            _activeDisplayConfig = CreateDefaultConfig();
            try
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Attempting to load the NVIDIA NVAPI DLL");

                // If we get here then we definitely have the NVIDIA driver available.
                Status status = Status.Error;
                SharedLogger.logger.Trace("NVIDIALibrary/NVIDIALibrary: Intialising NVIDIA NVAPI library interface");
                // Step 1: Initialise the NVAPI
                _initialised = false;
                try
                {
                    if (NVAPI.IsAvailable())
                    {
                        _initialised = true;
                        SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: NVIDIA NVAPI library was initialised successfully");
                        SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Running UpdateActiveConfig to ensure there is a config to use later");
                        _activeDisplayConfig = GetActiveConfig();
                        _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers(out bool failure);
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Error intialising NVIDIA NVAPI library. NvAPI_Initialize() returned error code {status}");
                    }

                }
                catch (DllNotFoundException ex)
                {
                    // If this fires, then the DLL isn't available, so we need don't try to do anything else
                    SharedLogger.logger.Info(ex, $"NVIDIALibrary/NVIDIALibrary: Exception trying to load the NVIDIA NVAPI DLLs nvapi64.dll or nvapi.dll. This generally means you don't have the NVIDIA driver installed.");
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/NVIDIALibrary: Exception intialising NVIDIA NVAPI library. NvAPI_Initialize() caused an exception.");
                }

            }
            catch (ArgumentNullException ex)
            {
                // If we get here then the PrelinkAll didn't work, meaning the AMD ADL DLL links don't work. We can't continue to use it, so we log the error and exit
                SharedLogger.logger.Info(ex, $"NVIDIALibrary/NVIDIALibrary: Exception2 trying to load the NVIDIA NVAPI DLLs nvapi64.dll or nvapi.dll. This generally means you don't have the NVIDIA driver installed.");
            }
            catch (Exception ex)
            {
                // If we get here then something else didn't work. We can't continue to use the AMD library, so we log the error and exit
                SharedLogger.logger.Info(ex, $"NVIDIALibrary/NVIDIALibrary: Exception3 trying to load the NVIDIA NVAPI DLLs nvapi64.dll or nvapi.dll. This generally means you don't have the NVIDIA driver installed.");
            }

        }

        ~NVIDIALibrary()
        {
            SharedLogger.logger.Trace("NVIDIALibrary/~NVIDIALibrary: Destroying NVIDIA NVAPI library interface");
            // The NVAPI library automatically runs NVAPI_Unload on Exit, so no need for anything here.
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

                // Dispose managed state (managed objects).
                _safeHandle?.Dispose();
            }

            _disposed = true;
        }


        public bool IsInstalled
        {
            get
            {
                return _initialised;
            }
        }

        public NVIDIA_DISPLAY_CONFIG ActiveDisplayConfig
        {
            get
            {
                if (_activeDisplayConfig == null)
                    _activeDisplayConfig = CreateDefaultConfig();
                return _activeDisplayConfig;
            }
        }

        public List<string> CurrentDisplayIdentifiers
        {
            get
            {
                if (_activeDisplayConfig == null)
                    _activeDisplayConfig = CreateDefaultConfig();
                return _activeDisplayConfig.DisplayIdentifiers;
            }
        }

        public List<string> PCIVendorIDs
        {
            get
            {
                return new List<string>() { "10DE" };
            }
        }

        public static void KeepVideoCardOn()
        {
            LoadLibrary("NVIDIAExportsDLL.dll");
        }

        public static NVIDIALibrary GetLibrary()
        {
            return _instance;
        }

        public NVIDIA_DISPLAY_CONFIG CreateDefaultConfig()
        {
            NVIDIA_DISPLAY_CONFIG myDefaultConfig = new NVIDIA_DISPLAY_CONFIG();

            // Fill in the minimal amount we need to avoid null references
            // so that we won't break json.net when we save a default config

            myDefaultConfig.MosaicConfig.IsMosaicEnabled = false;
            myDefaultConfig.MosaicConfig.MosaicGridTopos = new GridTopologyV2[] { };
            myDefaultConfig.MosaicConfig.MosaicGridCount = 0;
            //myDefaultConfig.MosaicConfig.MosaicViewports = new List<ViewPortF[]>();
            //myDefaultConfig.MosaicConfig.MosaicDisplaySettings = new DisplaySettingsV2();
            myDefaultConfig.PhysicalAdapters = new Dictionary<UInt32, NVIDIA_PER_ADAPTER_CONFIG>();
            myDefaultConfig.DisplayConfigs = new List<PathInfoV2>();
            myDefaultConfig.DRSSettings = new List<NVIDIA_DRS_CONFIG>();
            myDefaultConfig.DisplayNames = new Dictionary<string, string>();
            myDefaultConfig.DisplayIdentifiers = new List<string>();
            myDefaultConfig.IsCloned = false;
            myDefaultConfig.IsInUse = false;

            return myDefaultConfig;
        }

        public bool UpdateActiveConfig()
        {
            SharedLogger.logger.Trace($"NVIDIALibrary/UpdateActiveConfig: Updating the currently active config");
            try
            {
                _activeDisplayConfig = GetActiveConfig();
                _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers(out bool failure);
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Trace(ex, $"NVIDIALibrary/UpdateActiveConfig: Exception updating the currently active config");
                return false;
            }

            return true;
        }

        public NVIDIA_DISPLAY_CONFIG GetActiveConfig()
        {
            SharedLogger.logger.Trace($"NVIDIALibrary/GetActiveConfig: Getting the currently active config");
            bool allDisplays = false;
            return GetNVIDIADisplayConfig(allDisplays);
        }

        private NVIDIA_DISPLAY_CONFIG GetNVIDIADisplayConfig(bool allDisplays = false)
        {
            NVIDIA_DISPLAY_CONFIG myDisplayConfig = CreateDefaultConfig();

            if (_initialised)
            {

                // Store all the found display IDs so we can use them later
                List<UInt32> foundDisplayIds = new List<uint>();
                int physicalGpuCount = 0;
                PhysicalGPUHandle[] physicalGpus = new PhysicalGPUHandle[PhysicalGPUHandle.MaxPhysicalGPUs];

                try
                {
                    // Enumerate all the Physical GPUs
                    physicalGpus = NVAPI.EnumPhysicalGPUs();
                    physicalGpuCount = physicalGpus.Length;
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_EnumPhysicalGPUs returned {physicalGpuCount} Physical GPUs");

                    // This check is to make sure that we only continue in this function if there are physical GPUs to actually do anything with
                    // If the driver is installed, but not physical GPUs are present then we just want to return a default blank config.
                    if (physicalGpuCount == 0)
                    {
                        // Return the default config
                        return CreateDefaultConfig();
                    }

                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex,$"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting physical GPU count.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Returning the blank NVIDIA config to try and allow other video libraries to work.");
                    try
                    {
                        // Load the library that keeps the NVIDIA video card visible to this application (potentially wasting laptop power)
                        NVIDIALibrary.KeepVideoCardOn();

                        // Enumerate all the Physical GPUs
                        physicalGpus = NVAPI.EnumPhysicalGPUs();
                        physicalGpuCount = physicalGpus.Length;
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_EnumPhysicalGPUs returned {physicalGpuCount} Physical GPUs");

                        // This check is to make sure that we only continue in this function if there are physical GPUs to actually do anything with
                        // If the driver is installed, but not physical GPUs are present then we just want to return a default blank config.
                        if (physicalGpuCount == 0)
                        {
                            // Return the default config
                            return CreateDefaultConfig();
                        }

                    }
                    catch (Exception nex)
                    {
                        SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting physical GPU count.");
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Returning the blank NVIDIA config to try and allow other video libraries to work.");
                        // Return the default config to see if we can keep going.
                        return myDisplayConfig;
                    }
                }

                try
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the number of displays connected to the NVIDIA cards.");
                    DisplayHandle[] connectedDisplays = NVAPI.EnumNvidiaDisplayHandle();
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the number of displays connected to the NVIDIA cards.");
                    // If there are no NVIDIA connected displays
                    if (connectedDisplays.Length == 0)
                    {
                        // Return the default config.
                        return myDisplayConfig;
                        // The IsInUse will not be the set to true, and will stay false.
                    }

                }
                catch (NVIDIAApiException nex)
                {
                    if (nex.Status == Status.NvidiaDeviceNotFound)
                    {
                        SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Device not found when trying to get the number of displays connected to the NVIDIA card(s). This typically happens if the PC is a laptop with a separate discrete NVIDIA GPU and the laptop has no external monitors connected to it. ");
                    }
                    else
                    {
                        SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Exception caused whilst trying to get the number of displays connected to the NVIDIA cards.");
                    }
                    // Return the default config.
                    return myDisplayConfig;
                    // The IsInUse will not be the set to true, and will stay false.
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception caused whilst trying to get the number of displays connected to the NVIDIA cards.");
                    // Return the default config.
                    return myDisplayConfig;
                    // The IsInUse will not be the set to true, and will stay false.
                }


                // This try/catch is to handle the case where there is an NVIDIA GPU in the machine but it's not being used! e.g. display not connected to it
                try
                {
                    // If we reach here, then the nmvidia display is in use!
                    myDisplayConfig.IsInUse = true;

                    // Go through the Physical GPUs one by one to get the logical adapter information
                    for (uint physicalGpuIndex = 0; physicalGpuIndex < physicalGpuCount; physicalGpuIndex++)
                    {
                        // Prepare the physicalGPU per adapter structure to use later
                        NVIDIA_PER_ADAPTER_CONFIG myAdapter = new NVIDIA_PER_ADAPTER_CONFIG();
                        //myAdapter.LogicalGPU.PhysicalGPUHandles = new PhysicalGPUHandle[0];
                        myAdapter.IsQuadro = false;
                        myAdapter.HasLogicalGPU = false;
                        myAdapter.Displays = new Dictionary<uint, NVIDIA_PER_DISPLAY_CONFIG>();

                        //We want to get the name of the physical device
                        myAdapter.AdapterName = "";
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the name of the physical GPU #{physicalGpuIndex + 1}.");
                            myAdapter.AdapterName = NVAPI.GetFullName(physicalGpus[physicalGpuIndex]);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the GPU fullname of the physical GPU #{physicalGpuIndex + 1}. The GPU Full Name is '{myAdapter.AdapterName}'");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the fullname of the physical GPU #{physicalGpuIndex + 1}.");
                        }

                        // We want to get the physical details of the physical device
                        // This is the Host System Type Laptop/desktop/Unknown
                        myAdapter.SystemType = SystemType.Unknown;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the host system type of the physical GPU #{physicalGpuIndex + 1}.");
                            myAdapter.GPUType = NVAPI.GetGPUType(physicalGpus[physicalGpuIndex]);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the host system type of the physical GPU #{physicalGpuIndex + 1}. The host system type is {myAdapter.SystemType.ToString()}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the host system type of the physical GPU #{physicalGpuIndex + 1}.");
                        }

                        // This is the GPU Bus Type
                        myAdapter.GPUType = GPUType.Unknown;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the GPU type of the physical GPU #{physicalGpuIndex + 1}.");
                            myAdapter.GPUType = NVAPI.GetGPUType(physicalGpus[physicalGpuIndex]);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the GPU type of the physical GPU #{physicalGpuIndex + 1}. The bus type is {myAdapter.GPUType.ToString()}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the GPU type of the physical GPU #{physicalGpuIndex + 1}.");
                        }

                        // This is the GPU Bus Type
                        myAdapter.BusType = GPUBusType.Undefined;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the bus type of the physical GPU #{physicalGpuIndex + 1}.");
                            myAdapter.BusType = NVAPI.GetBusType(physicalGpus[physicalGpuIndex]);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the bus type of the physical GPU #{physicalGpuIndex + 1}. The bus type is {myAdapter.BusType.ToString()}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the bus type of the physical GPU #{physicalGpuIndex + 1}.");
                        }

                        // This is the GPU Bus ID
                        myAdapter.BusId = 0;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the bus ID of the physical GPU #{physicalGpuIndex + 1}.");
                            myAdapter.BusId = NVAPI.GetBusId(physicalGpus[physicalGpuIndex]);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the bus ID of the physical GPU #{physicalGpuIndex + 1}. The bus ID is {myAdapter.BusId}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the bus ID of the physical GPU #{physicalGpuIndex + 1}.");
                        }

                        // This is the GPU Bus Slot ID
                        myAdapter.BusSlotId = 0;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the bus slot ID of the physical GPU #{physicalGpuIndex + 1}.");
                            myAdapter.BusSlotId = NVAPI.GetBusSlotId(physicalGpus[physicalGpuIndex]);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the bus slot ID of the physical GPU #{physicalGpuIndex + 1}. The bus slot ID is {myAdapter.BusId}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the bus slot ID of the physical GPU #{physicalGpuIndex + 1}.");
                        }

                        try
                        {
                            if (NVAPI.QueryWorkstationFeatureSupport(physicalGpus[physicalGpuIndex], WorkstationFeatureType.Proviz))
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Video Card is one from the Quadro range");
                                myAdapter.IsQuadro = true;
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Video Card is not a Quadro range video card.");
                            }

                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex,$"NVIDIALibrary/GetNVIDIADisplayConfig: Exception caused whilst trying to find out if the card is from the Quadro range.");
                        }


                        try
                        {
                            // Firstly let's get the logical GPU from the Physical handle
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the Logical GPU Handle");
                            LogicalGPUHandle logicalGPUHandle = NVAPI.GetLogicalGPUFromPhysicalGPU(physicalGpus[physicalGpuIndex]);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Logical GPU Handle from physical GPU. It means there is a Logical GPU in use.");
                            myAdapter.HasLogicalGPU = true;
                            /*SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Now attempting to get the Logical GPU Information");
                            LogicalGPUData logicalGPUData = new LogicalGPUData();
                            NVAPI.GetLogicalGPUInfo(logicalGPUHandle, out logicalGPUData);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the Logical GPU information from the NVIDIA driver!");*/
                            //myAdapter.LogicalGPU = logicalGPUData;                            
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception caused whilstgetting Logical GPU handle from Physical GPU using NvAPI_GetLogicalGPUFromPhysicalGPU().");
                            myAdapter.HasLogicalGPU = false;
                        }

                        myDisplayConfig.PhysicalAdapters[physicalGpuIndex] = myAdapter;
                    }


                    TopologyBrief mosaicTopoBrief = new TopologyBrief();
                    IDisplaySettings mosaicDisplaySettings = new DisplaySettingsV2();
                    int mosaicOverlapX = 0;
                    int mosaicOverlapY = 0;

                    try
                    {
                        // Get current Mosaic Topology settings in brief (check whether Mosaic is on)
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the current mosaic topology brief and mosaic display settings.");
                        NVAPI.GetCurrentTopology(out mosaicTopoBrief, out mosaicDisplaySettings, out mosaicOverlapX, out mosaicOverlapY);
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the current mosaic toplogy brief and mosaic display settings.");

                        myDisplayConfig.MosaicConfig.MosaicTopologyBrief = mosaicTopoBrief;
                        myDisplayConfig.MosaicConfig.MosaicDisplaySettings = (DisplaySettingsV2)mosaicDisplaySettings;
                        myDisplayConfig.MosaicConfig.OverlapX = mosaicOverlapX;
                        myDisplayConfig.MosaicConfig.OverlapY = mosaicOverlapY;
                    }
                    catch (NVIDIAApiException nex)
                    {
                        if (nex.Status == Status.NotSupported)
                        {
                            _mosaic_supported = false;
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported by this GPU.");
                        }
                        else
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Exception caused whilst getting current mosiac topology brief and mosaic display settings.");
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception caused whilst getting current mosiac topology brief and mosaic display settings.");
                    }

                    if (_mosaic_supported)
                    {
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetTopoGroup returned OK.");
                            if (mosaicTopoBrief.IsPossible)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: The current Mosaic Topology of {mosaicTopoBrief.Topology} is possible to use");
                                //myDisplayConfig.MosaicConfig.IsMosaicPossible = true;
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: The current Mosaic Topology of {mosaicTopoBrief.Topology} is NOT possible to use");
                                //myDisplayConfig.MosaicConfig.IsMosaicPossible = false;
                            }
                            if (mosaicTopoBrief.IsEnable)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: The current Mosaic Topology of {mosaicTopoBrief.Topology} is enabled right now");
                                myDisplayConfig.MosaicConfig.IsMosaicEnabled = true;
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: The current Mosaic Topology of {mosaicTopoBrief.Topology} is NOT enabled right now");
                                myDisplayConfig.MosaicConfig.IsMosaicEnabled = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception caused whilst getting current mosiac topology group.");
                        }

                    }
                    else
                    {
                        // Mosaic isn't possible/supported
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Mosaic is NOT enabled.");
                        myDisplayConfig.MosaicConfig.MosaicTopologyBrief = mosaicTopoBrief;
                        myDisplayConfig.MosaicConfig.IsMosaicEnabled = false;
                        //myDisplayConfig.MosaicConfig.IsMosaicPossible = false;
                        myDisplayConfig.MosaicConfig.MosaicGridTopos = new GridTopologyV2[] { };
                        //myDisplayConfig.MosaicConfig.MosaicViewports = new List<ViewPortF[]>();
                    }

                    // Get Mosaic Grid settings!
                    GridTopologyV2[] mosaicGridTopos;
                    try
                    {
                        // Figure out how many Mosaic Grid topoligies there are                    
                        mosaicGridTopos = NVAPI.EnumDisplayGrids();
                        /*for (var i = 0; i < mosaicGridTopos.Length; i++)
                        {
                            GridTopologyDisplayV2[] gtdlist = mosaicGridTopos[i].Displays.Cast<GridTopologyDisplayV2>().ToArray<GridTopologyDisplayV2>();

                            for (var j = 0; j<gtdlist.Length; j++)
                            {
                                gtdlist[i].Version = new StructureVersion(2, typeof(GridTopologyDisplayV2));
                            }

                            mosaicGridTopos[i].Displays = gtdlist.ToList();
                        }*/
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");


                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred while getting Mosaic Topology! NvAPI_Mosaic_EnumDisplayGrids() returned error.");
                        mosaicGridTopos = new GridTopologyV2[0];
                    }

                    myDisplayConfig.MosaicConfig.MosaicGridTopos = mosaicGridTopos;
                    myDisplayConfig.MosaicConfig.MosaicGridCount = (uint)mosaicGridTopos.Length;

                    /*//List<ViewPortF[]> allViewports = new List<ViewPortF[]>();
                    foreach (GridTopologyV2 gridTopo in mosaicGridTopos)
                    {
                        *//*// Get Current Mosaic Grid settings using the Grid topologies numbers we got before
                        ViewPortF[] viewports = new ViewPortF[0];
                        byte bezelCorrected = 0;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get mosaic display viewport details by resolution.");
                            NVAPI.GetDisplayViewportsByResolution(gridTopo.Displays.FirstOrDefault().DisplayId, 0, 0, out viewports, out bezelCorrected);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got mosaic display viewport details by resolution.");
                        }
                        catch (NVIDIAApiException nex)
                        {
                            if (nex.Status == Status.MosaicNotActive)
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not currently in use, so unable to get the list of ViewportsF.");
                            }
                            else
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIAApiException occurred whilst getting display viewport details by resolution.");
                            }
                        }
                        catch(Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting display viewport details by resolution.");
                        }

                        // Save the viewports to the List
                        allViewports.Add(viewports);*//*

                        // Get Current Mosaic Display Topology mode settings using the Grid topology we matched before before
                        IDisplaySettings[] mosaicDisplaySettings;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Getting mosaic display modes from the current display topology.");
                            mosaicDisplaySettings = NVAPI.EnumDisplayModes(gridTopo);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got mosaic display modes from the current display topology.");

                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting display modes from current display topology");
                            mosaicDisplaySettings = new IDisplaySettings[0];
                        }
                    }*/

                    //myDisplayConfig.MosaicConfig.MosaicViewports = allViewports;



                    // Now we try to get the NVIDIA Windows Display Config. This is needed for handling some of the advanced scaling settings that some advanced users make use of
                    PathInfoV2[] pathInfos;
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get NVIDIA display configuration.");
                        pathInfos = NVAPI.GetDisplayConfig();
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got NVIDIA display configuration..");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting NVIDIA display configuration.");
                        pathInfos = new PathInfoV2[0];
                    }

                    // Now try and see if we have a cloned display in the current layout
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Checking if there is a cloned display detected within NVIDIA Display Configuration.");
                    int pathInfoCount = pathInfos.Length;
                    for (int x = 0; x < pathInfoCount; x++)
                    {
                        if (pathInfos[x].TargetsInfo.Count() > 1)
                        {
                            // This is a cloned display, we need to mark this NVIDIA display profile as cloned so we correct the profile later
                            myDisplayConfig.IsCloned = true;

                        }
                    }
                    if (myDisplayConfig.IsCloned)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Cloned display detected within NVIDIA Display Configuration.");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Cloned display NOT detected within NVIDIA Display Configuration.");
                    }

                    myDisplayConfig.DisplayConfigs = pathInfos.ToList();
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_GetDisplayConfig returned OK on third pass.");

                    // I don't think this is worth recording any more and we should remove the code. It isn't set anywhere.
                    /*// We want to get the primary monitor
                    UInt32 primaryDisplayId = 0;
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the primary windows display id.");
                        primaryDisplayId = NVAPI.GetGDIPrimaryDisplayId();
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the primary windows display id.");
                    }
                    catch (NVIDIAApiException nex)
                    {
                        if (nex.Status == Status.NvidiaDeviceNotFound)
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: An NVIDIA device is not the primary display.");
                        }
                        else
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Exception occurred whilst getting the primary windows display id.");
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the primary windows display id.");
                    }
                    myDisplayConfig.MosaicConfig.PrimaryDisplayId = primaryDisplayId;*/

                    // We want to get the number of displays we have
                    // Go through the Physical GPUs one by one
                    for (uint physicalGpuIndex = 0; physicalGpuIndex < physicalGpuCount; physicalGpuIndex++)
                    {

                        // Get a new variable to the PhysicalAdapters to make easier to use
                        // NOTE: This struct was filled in earlier by code further up
                        NVIDIA_PER_ADAPTER_CONFIG myAdapter = myDisplayConfig.PhysicalAdapters[physicalGpuIndex];
                        myAdapter.Displays = new Dictionary<uint, NVIDIA_PER_DISPLAY_CONFIG>();

                        //This function retrieves the number of display IDs we know about
                        DisplayIdsV2[] displayIds;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the list of connected display ids that VIDIA knows about.");
                            displayIds = NVAPI.GetConnectedDisplayIds(physicalGpus[physicalGpuIndex], ConnectedIdsFlag.UnCached | ConnectedIdsFlag.SLI | ConnectedIdsFlag.Fake);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the list of connected display ids that VIDIA knows about.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the list of connected display ids that VIDIA knows about.");
                            displayIds = new DisplayIdsV2[0];
                        }

                        // Time to get the color settings, HDR capabilities and settings for each display
                        //bool isNvHdrEnabled = false;
                        for (int displayIndex = 0; displayIndex < displayIds.Length; displayIndex++)
                        {
                            if (allDisplays)
                            {
                                // We want all physicallyconnected or connected displays
                                if (!(displayIds[displayIndex].IsConnected || displayIds[displayIndex].IsPhysicallyConnected))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                // We want only active displays, so skip any non-active ones
                                if (!displayIds[displayIndex].IsActive)
                                {
                                    continue;
                                }
                            }

                            // Record this as an active display ID
                            foundDisplayIds.Add(displayIds[displayIndex].DisplayId);

                            // Prepare the config structure for us to fill it in
                            NVIDIA_PER_DISPLAY_CONFIG myDisplay = new NVIDIA_PER_DISPLAY_CONFIG();
                            myDisplay.ColorData = new ColorDataV5();
                            myDisplay.HdrColorData = new HDRColorDataV2();
                            myDisplay.HdrCapabilities = new HDRCapabilitiesV3();
                            myDisplay.AdaptiveSyncConfig = new SetAdaptiveSyncData();
                            myDisplay.CustomDisplays = new List<CustomDisplay>();
                            myDisplay.HasNvHdrEnabled = false;
                            myDisplay.HasAdaptiveSync = false;
                            myDisplay.HasCustomDisplay = false;

                            // We need to skip recording anything that doesn't support color communication
                            if (!SkippedColorConnectionTypes.Contains(displayIds[displayIndex].ConnectionType))
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: This display supports color information, so attempting to get the various color configuration settings from it.");

                                // skip this monitor connection type as it won't provide the data in the section, and just creates errors                                
                                // We get the Color Capabilities of the display, by setting the command to GET
                                ColorDataV5 colorData5 = new ColorDataV5(ColorDataCommand.Get);
                                try
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the standard  color data from the display.");
                                    NVAPI.ColorControl(displayIds[displayIndex].DisplayId, ref colorData5);
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the standard  color data from the display.");
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Your monitor {displayIds[displayIndex].DisplayId} has the following color settings set. BPC = {colorData5.DesktopColorDepth.ToString()}. Color Format = {colorData5.ColorFormat.ToString()}. Colorimetry = {colorData5.Colorimetry.ToString("G")}. Color Selection Policy = {colorData5.SelectionPolicy.ToString()}. Color Depth = {colorData5.ColorDepth.ToString()}. Dynamic Range = {colorData5.DynamicRange.ToString()}. ");
                                    myDisplay.ColorData = colorData5;
                                    myDisplay.HasColorData = true;

                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the standard  color data from the display.");
                                    ColorDataV4 colorData4 = new ColorDataV4(ColorDataCommand.Get);
                                    try
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the standard  color data from the display.");
                                        NVAPI.ColorControl(displayIds[displayIndex].DisplayId, ref colorData4);
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the standard  color data from the display.");
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Your monitor {displayIds[displayIndex].DisplayId} has the following color settings set. BPC = {colorData4.DesktopColorDepth.ToString()}. Color Format = {colorData4.ColorFormat.ToString("G")}. Colorimetry = {colorData4.Colorimetry.ToString("G")}. Color Selection Policy = {colorData4.SelectionPolicy.ToString()}. Color Depth = {colorData4.ColorDepth.ToString()}. Dynamic Range = {colorData4.DynamicRange.ToString()}.");
                                        myDisplay.ColorData = colorData4;
                                        myDisplay.HasColorData = true;

                                    }
                                    catch (Exception nex)
                                    {
                                        SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the standard  color data from the display.");
                                    }
                                }

                                // Now we get the HDR capabilities of the display
                                // TODO: CHange to HDRCapabilitiesV3 once the v3 struct is completed and tested
                                IHDRCapabilities hdrCapabilities;
                                try
                                {
                                    hdrCapabilities = NVAPI.GetHDRCapabilities(displayIds[displayIndex].DisplayId, false);
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Disp_GetHdrCapabilities returned OK.");
                                    if (hdrCapabilities.IsST2084EOTFSupported)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports HDR mode ST2084 EOTF");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support HDR mode ST2084 EOTF");
                                    }
                                    if (hdrCapabilities.IsDolbyVisionSupported)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports DolbyVision HDR");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support DolbyVision HDR");
                                    }
                                    if (hdrCapabilities.IsEDRSupported)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports EDR");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support EDR");
                                    }
                                    if (hdrCapabilities.IsTraditionalHDRGammaSupported)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports Traditional HDR Gama");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support Traditional HDR Gama");
                                    }

                                    if (hdrCapabilities.IsTraditionalSDRGammaSupported)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports Traditional SDR Gama");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT supports Traditional SDR Gama");
                                    }
                                    if (hdrCapabilities.IsDriverDefaultHDRParametersExpanded)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports Driver Expanded Default HDR Parameters");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support Driver Expanded Default HDR Parameters ");
                                    }

                                }
                                catch (Exception nex)
                                {
                                    SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the standard  color data from the display.");
                                    hdrCapabilities = new HDRCapabilitiesV3();
                                }

                                myDisplay.HdrCapabilities = hdrCapabilities;
                            
 
                                // Now we get the HDR colour settings of the display
                                IHDRColorData hdrColorData;
                                try
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to get the HDR Color Mode for Display ID# {displayIds[displayIndex].DisplayId}.");
                                    hdrColorData = new HDRColorDataV2(ColorDataHDRCommand.Get);
                                    NVAPI.HDRColorControl(displayIds[displayIndex].DisplayId, ref hdrColorData);
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the HDR Color Mode for Display ID# {displayIds[displayIndex].DisplayId} is set to {hdrColorData.HDRMode.ToString("G")}.");
                                    if (hdrColorData.HDRMode != ColorDataHDRMode.Off)
                                    {
                                        myDisplay.HasNvHdrEnabled = true;
                                    }
                                    else
                                    {
                                        myDisplay.HasNvHdrEnabled = false;
                                    }

                                }
                                catch (Exception nex)
                                {
                                    SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the HDR Color Mode for Display ID# {displayIds[displayIndex].DisplayId}.");
                                    hdrColorData = new HDRColorDataV2();
                                }
                                myDisplay.HdrColorData = hdrColorData;
                            
                                // Now we get the Adaptive Sync Settings from the display
                                GetAdaptiveSyncData getAdaptiveSyncData  = typeof(GetAdaptiveSyncData).Instantiate<GetAdaptiveSyncData>();
                                try
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to get the Adaptive Sync Settings for Display ID# {displayIds[displayIndex].DisplayId}.");
                                    NVAPI.GetAdaptiveSyncData(displayIds[displayIndex].DisplayId, out getAdaptiveSyncData);
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the Adaptive Sync Settings for Display ID# {displayIds[displayIndex].DisplayId} is set to {hdrColorData.HDRMode.ToString("G")}.");
                                    // Copy the AdaptiveSync Data we got into a NV_SET_ADAPTIVE_SYNC_DATA_V1 object so that it can be used without conversion
                                    SetAdaptiveSyncData setAdaptiveSyncData = new SetAdaptiveSyncData();
                                    setAdaptiveSyncData.Flags = getAdaptiveSyncData.Flags;
                                    setAdaptiveSyncData.MaxFrameInterval = getAdaptiveSyncData.MaxFrameInterval;

                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_GetAdaptiveSyncData returned OK.");
                                    if (getAdaptiveSyncData.DisableAdaptiveSync)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: AdaptiveSync is DISABLED for Display {displayIds[displayIndex].DisplayId} .");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: AdaptiveSync is ENABLED for Display {displayIds[displayIndex].DisplayId} .");
                                    }
                                    if (getAdaptiveSyncData.DisableFrameSplitting)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: FrameSplitting is DISABLED for Display {displayIds[displayIndex].DisplayId} .");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: FrameSplitting is ENABLED for Display {displayIds[displayIndex].DisplayId} .");
                                    }
                                    myDisplay.AdaptiveSyncConfig = setAdaptiveSyncData;
                                    myDisplay.HasAdaptiveSync = true;
                                }
                                catch (Exception nex)
                                {
                                    SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the Adaptive Sync Settings for Display ID# {displayIds[displayIndex].DisplayId}.");
                                }


                                // TEMPORARILY DISABLING THE CUSTOM DISPLAY CODE FOR NOW, AS NOT SURE WHAT NVIDIA SETTINGS IT TRACKS
                                // KEEPING IT IN CASE I NEED IT FOR LATER. I ORIGINALLY THOUGHT THAT IS WHERE INTEGER SCALING SETTINGS LIVED< BUT WAS WRONG
                                /*// Now we get the Custom Display settings of the display (if there are any)
                                //NVIDIA_CUSTOM_DISPLAY_CONFIG customDisplayConfig = new NVIDIA_CUSTOM_DISPLAY_CONFIG();
                                List<NV_CUSTOM_DISPLAY_V1> customDisplayConfig = new List<NV_CUSTOM_DISPLAY_V1>();
                                for (UInt32 d = 0; d < UInt32.MaxValue; d++)
                                {
                                    NV_CUSTOM_DISPLAY_V1 customDisplay = new NV_CUSTOM_DISPLAY_V1();
                                    status = NVAPI.EnumCustomDisplay(displayIds[displayIndex].DisplayId, d, ref customDisplay);
                                    if (status == Status.Ok)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_EnumCustomDisplay returned OK. Custom Display settings retrieved.");
                                        myDisplay.CustomDisplay = customDisplay;
                                        myDisplay.HasCustomDisplay = true;
                                    }
                                    else if (status == Status.NVAPI_END_ENUMERATION)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: We've reached the end of the list of Custom Displays. Breaking the polling loop.");
                                        break;
                                    }
                                    else if (status == Status.InvalidDisplayId)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_DISP_EnumCustomDisplay() returned error code {status}");
                                        break;
                                    }
                                    else if (status == Status.ApiNotInitialized)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_EnumCustomDisplay() returned error code {status}");
                                        break;
                                    }
                                    else if (status == Status.NoImplementation)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_EnumCustomDisplay() returned error code {status}");
                                        break;
                                    }
                                    else if (status == Status.IncompatibleStructureVersion)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The supplied struct is incompatible. NvAPI_DISP_EnumCustomDisplay() returned error code {status}");
                                        break;
                                    }
                                    else if (status == Status.Error)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_DISP_EnumCustomDisplay() returned error code {status}.");
                                        break;
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while enumerating the custom displays! NvAPI_DISP_EnumCustomDisplay() returned error code {status}.");
                                        break;
                                    }

                                }*/

                                myAdapter.Displays.Add(displayIds[displayIndex].DisplayId, myDisplay);
                                
                            }
                        }

                        myAdapter.DisplayCount = (UInt32)myAdapter.Displays.Count();
                        myDisplayConfig.PhysicalAdapters[physicalGpuIndex] = myAdapter;

                    }


                    // Now we need to loop through each of the windows paths so we can record the Windows DisplayName to DisplayID mapping
                    // This is needed for us to piece together the Screen layout for when we draw the NVIDIA screens!
                    myDisplayConfig.DisplayNames = new Dictionary<string, string>();
                    foreach (KeyValuePair<string, List<uint>> displaySource in WinLibrary.GetDisplaySourceNames())
                    {
                        // Now we try to get the information about the displayIDs and map them to windows \\DISPLAY names e.g. \\DISPLAY1
                        string displayName = displaySource.Key;
                        UInt32 displayId = 0;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to get the Windows DisplayName to DisplayID mappings for Display ID {displayName}.");
                            displayId = NVAPI.GetDisplayIdByDisplayName(displayName);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the Windows DisplayName to DisplayID mappings for Display ID {displayName} is set to {displayId}.");
                            myDisplayConfig.DisplayNames.Add(displayId.ToString(), displayName);
                        }
                        catch (NVIDIAApiException nex)
                        {
                            if (nex.Status == Status.NvidiaDeviceNotFound)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: The display named '{displayName}' is not connected via an NVIDIA device. Skipping adding this Display Name.");
                            }
                            else
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Exception occurred whilst getting the Windows DisplayName to DisplayID mappings for Display ID {displayName}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the Windows DisplayName to DisplayID mappings for Display ID {displayName}.");
                        }
                        
                    }

                    // Get the display identifiers                
                    myDisplayConfig.DisplayIdentifiers = GetCurrentDisplayIdentifiers(out bool failure);



                    // Get the DRS Settings
                    DRSSessionHandle drsSessionHandle = new DRSSessionHandle();
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to get the DRS Session Handle so we can get the DRS settings.");
                        drsSessionHandle = NVAPI.CreateSession(); 
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the DRS Session Handle so we can get the DRS settings.");

                        // Load the DRS Settings into memory
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to load the DRS Settings into memory.");
                        NVAPI.LoadSettings(drsSessionHandle);
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully loaded the DRS Settings into memory.");

                        // Now we try to start getting the DRS Settings we need
                        // Firstly, we get the profile handle to the global DRS Profile currently in use
                        DRSProfileHandle drsProfileHandle = new DRSProfileHandle();
                        try
                        {
                            //status = NVAPI.GetCurrentGlobalProfile(drsSessionHandle, out drsProfileHandle);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to get the profile handle to the global DRS Profile currently in use.");
                            drsProfileHandle = NVAPI.GetBaseProfile(drsSessionHandle);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the profile handle to the global DRS Profile currently in use.");
                            
                            if (drsProfileHandle.IsNull)
                            {
                                // There isn't a custom global profile set yet, so we ignore it
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DRS_GetCurrentGlobalProfile returned OK, but there was no process handle set. THe DRS Settings may not have been loaded.");
                            }
                            else
                            {
                                // There is a custom global profile set, so we continue
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DRS_GetCurrentGlobalProfile returned OK. We got the DRS Profile Handle for the current global profile");

                                // Next, we make a single DRS setting to track the global profile
                                NVIDIA_DRS_CONFIG drsConfig = new NVIDIA_DRS_CONFIG();
                                drsConfig.IsBaseProfile = true;

                                // Next we grab the Profile Info and store it
                                DRSProfileV1 drsProfileInfo = new DRSProfileV1();
                                drsProfileInfo = NVAPI.GetProfileInfo(drsSessionHandle, drsProfileHandle);
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DRS_GetProfileInfo returned OK. We got the DRS Profile info for the current global profile. Profile Name is {drsProfileInfo.Name}.");
                                drsConfig.ProfileInfo = drsProfileInfo;
                                

                                if (drsProfileInfo.NumberOfSettings > 0)
                                {
                                    // Next we grab the Profile Settings and store them
                                    List<DRSSettingV1> drsDriverSettings = new List<DRSSettingV1> {};
                                    //NVDRS_SETTING_V1 drsDriverSetting = new NVDRS_SETTING_V1();
                                    try 
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to get the next DRS setting handle from the DRS Profile {drsProfileInfo.Name}.");
                                        drsDriverSettings = NVAPI.EnumSettings(drsSessionHandle, drsProfileHandle).ToList();
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the next DRS setting handle from the DRS Profile {drsProfileInfo.Name}.");
                                        drsConfig.DriverSettings = drsDriverSettings.ToList();
                                    }
                                    catch (Exception ex)
                                    {
                                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the  next DRS setting handle from the DRS Profile {drsProfileInfo.Name}.");
                                    }

                                    // And then we save the DRS Config to the main config so it gets saved
                                    myDisplayConfig.DRSSettings.Add(drsConfig);

                                }

                            }

                        }
                        catch(Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the profile handle to the global DRS Profile currently in use.");
                        }
                        finally
                        {
                            // Destroy the DRS Session Handle to clean up
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to clean up and destroy our DRS Session Handle");
                            NVAPI.DestroySession(drsSessionHandle);
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the DRS Session Handle so we can get the DRS settings or whilst loading the DRS settings into memory.");
                    }

                    // At this stage we should set the IsInUse flag to report that the NVIDIA config is in Use
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
                SharedLogger.logger.Info($"NVIDIALibrary/GetNVIDIADisplayConfig: Tried to run GetNVIDIADisplayConfig but the NVIDIA NVAPI library isn't initialised! This generally means you don't have a NVIDIA video card in your machine.");
                //throw new NVIDIALibraryException($"Tried to run GetNVIDIADisplayConfig but the NVIDIA NVAPI library isn't initialised!");
            }

            // Return the configuration
            return myDisplayConfig;
        }


        public string PrintActiveConfig()
        {
            string stringToReturn = "";

            // Get the current config
            NVIDIA_DISPLAY_CONFIG displayConfig = ActiveDisplayConfig;

            stringToReturn += $"****** NVIDIA VIDEO CARDS *******\n";

            // Enumerate all the Physical GPUs
            PhysicalGPUHandle[] physicalGpus = new PhysicalGPUHandle[NvConstants.NV_MAX_PHYSICAL_GPUS];
            uint physicalGpuCount = 0;
            try 
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Attempting to get the physical GPU count.");
                physicalGpus = NVAPI.EnumPhysicalGPUs();
                SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: NvAPI_EnumPhysicalGPUs returned {physicalGpuCount} Physical GPUs");
                stringToReturn += $"Number of NVIDIA Video cards found: {physicalGpuCount}\n";
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the physical GPU count.");
            }
        
            // This check is to make sure that if there aren't any physical GPUS then we exit!
            if (physicalGpuCount == 0)
            {
                // Print out that there aren't any video cards detected
                stringToReturn += "No NVIDIA Video Cards detected.";
                SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: No NVIDIA Videocards detected");
                return stringToReturn;
            }

            // Go through the Physical GPUs one by one
            for (uint physicalGpuIndex = 0; physicalGpuIndex < physicalGpuCount; physicalGpuIndex++)
            {
                //We want to get the name of the physical device
                string gpuName = "";
                try
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Attempting to get the physical GPU name for GPU #{physicalGpuIndex}.");
                    gpuName = NVAPI.GetFullName(physicalGpus[physicalGpuIndex]);
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Successfully got the physical GPU name for GPU #{physicalGpuIndex}. The GPU Full Name is {gpuName}");
                    stringToReturn += $"NVIDIA Video card #{physicalGpuIndex} is a {gpuName}\n";
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the physical GPU name for GPU #{physicalGpuIndex}.");
                }

                //This function retrieves the Quadro status for the GPU (1 if Quadro, 0 if GeForce)
                bool quadroStatus = false;
                try
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Attempting to find out if the GPU is from the Quadro range.");
                    quadroStatus = NVAPI.GetQuadroStatus(physicalGpus[physicalGpuIndex]);
                    if (quadroStatus)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: NVIDIA Video Card is one from the GeForce range");
                        stringToReturn += $"NVIDIA Video card #{physicalGpuIndex} is in the GeForce range\n";
                    }
                    else                     {
                        SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: NVIDIA Video Card is NOT one from the Quadro range");
                        stringToReturn += $"NVIDIA Video card #{physicalGpuIndex} is NOT in the Quadro range\n";
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst finding out if the GPU is from the Quadro range.");
                }
            }

            stringToReturn += $"\n****** NVIDIA SURROUND/MOSAIC *******\n";
            if (displayConfig.MosaicConfig.IsMosaicEnabled)
            {
                stringToReturn += $"NVIDIA Surround/Mosaic is Enabled\n";
                if (displayConfig.MosaicConfig.MosaicGridTopos.Length > 1)
                {
                    stringToReturn += $"There are {displayConfig.MosaicConfig.MosaicGridTopos.Length} NVIDIA Surround/Mosaic Grid Topologies in use.\n";
                }
                if (displayConfig.MosaicConfig.MosaicGridTopos.Length == 1)
                {
                    stringToReturn += $"There is 1 NVIDIA Surround/Mosaic Grid Topology in use.\n";
                }
                else
                {
                    stringToReturn += $"There are no NVIDIA Surround/Mosaic Grid Topologies in use.\n";
                }

                int count = 0;
                foreach (GridTopologyV2 gridTopology in displayConfig.MosaicConfig.MosaicGridTopos)
                {
                    stringToReturn += $"NOTE: This Surround/Mosaic screen will be treated as a single display by Windows.\n";
                    stringToReturn += $"The NVIDIA Surround/Mosaic Grid Topology #{count} is {gridTopology.Rows} Rows x {gridTopology.Columns} Columns\n";
                    stringToReturn += $"The NVIDIA Surround/Mosaic Grid Topology #{count} involves {gridTopology.Displays.Count()} Displays\n";
                    count++;
                }
            }
            else
            {
                stringToReturn += $"NVIDIA Surround/Mosaic is Disabled\n";
            }

            // Start printing out things for the physical GPU
            foreach (KeyValuePair<UInt32, NVIDIA_PER_ADAPTER_CONFIG> physicalGPU in displayConfig.PhysicalAdapters)
            {
                stringToReturn += $"\n****** NVIDIA PHYSICAL ADAPTER {physicalGPU.Key} *******\n";

                NVIDIA_PER_ADAPTER_CONFIG myAdapter = physicalGPU.Value;

                foreach (KeyValuePair<UInt32, NVIDIA_PER_DISPLAY_CONFIG> myDisplayItem in myAdapter.Displays)
                {
                    string displayId = myDisplayItem.Key.ToString();
                    NVIDIA_PER_DISPLAY_CONFIG myDisplay = myDisplayItem.Value;

                    stringToReturn += $"\n****** NVIDIA PER DISPLAY CONFIG {displayId} *******\n";

                    stringToReturn += $"\n****** NVIDIA COLOR CONFIG *******\n";
                    ColorDataV5 colorData = (ColorDataV5)myDisplay.ColorData;
                    stringToReturn += $"Display {displayId} BPC is {colorData.DesktopColorDepth.ToString()}.\n";
                    stringToReturn += $"Display {displayId} ColorFormat is {colorData.ColorFormat.ToString("G")}.\n";
                    stringToReturn += $"Display {displayId} Colorimetry is {colorData.Colorimetry.ToString("G")}.\n";
                    stringToReturn += $"Display {displayId} ColorSelectionPolicy is {colorData.SelectionPolicy.Value.ToString()}.\n";
                    stringToReturn += $"Display {displayId} Depth is {colorData.ColorDepth.ToString()}.\n";
                    stringToReturn += $"Display {displayId} DynamicRange is {colorData.DynamicRange.ToString()}.\n";

                    // Start printing out HDR things
                    stringToReturn += $"\n****** NVIDIA HDR CONFIG *******\n";
                    if (myDisplay.HasNvHdrEnabled)
                    {
                        stringToReturn += $"NVIDIA HDR is Enabled\n";
                        if (displayConfig.MosaicConfig.MosaicGridTopos.Length == 1)
                        {
                            stringToReturn += $"There is 1 NVIDIA HDR devices in use.\n";
                        }
                        else
                        {
                            stringToReturn += $"There are no NVIDIA HDR devices in use.\n";
                        }

                        HDRCapabilitiesV3 hdrCap = (HDRCapabilitiesV3)myDisplay.HdrCapabilities;

                        if (hdrCap.IsDolbyVisionSupported)
                        {
                            stringToReturn += $"Display {displayId} supports DolbyVision HDR.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support DolbyVision HDR.\n";
                        }
                        if (hdrCap.IsST2084EOTFSupported)
                        {
                            stringToReturn += $"Display {displayId} supports ST2084EOTF HDR Mode.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support ST2084EOTF HDR Mode.\n";
                        }
                        if (hdrCap.IsTraditionalHDRGammaSupported)
                        {
                            stringToReturn += $"Display {displayId} supports Traditional HDR Gamma.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support Traditional HDR Gamma.\n";
                        }
                        if (hdrCap.IsEDRSupported)
                        {
                            stringToReturn += $"Display {displayId} supports EDR.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support EDR.\n";
                        }
                        if (hdrCap.IsTraditionalSDRGammaSupported)
                        {
                            stringToReturn += $"Display {displayId} supports SDR Gamma.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support SDR Gamma.\n";
                        }
                        if (hdrCap.IsDolbyVisionSupported)
                        {
                            stringToReturn += $"Display {displayId} supports Dolby Vision.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support Dolby Vision.\n";
                        }
                        if (hdrCap.isHdr10PlusSupported)
                        {
                            stringToReturn += $"Display {displayId} supports HDR10Plus.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support HDR10Plus.\n";
                        }
                        if (hdrCap.isHdr10PlusGamingSupported)
                        {
                            stringToReturn += $"Display {displayId} supports HDR10Plus Gaming.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support HDR10Plus Gaming.\n";
                        }
                        if (hdrCap.IsDriverDefaultHDRParametersExpanded)
                        {
                            stringToReturn += $"Display {displayId} supports driver default HDR Parameters expanded.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support driver default HDR Parameters expanded.\n";
                        }

                    }
                    else
                    {
                        stringToReturn += $"NVIDIA HDR is Disabled (HDR may still be enabled within Windows itself)\n";
                    }
                }
            }

            // I have to disable this as NvAPI_DRS_EnumAvailableSettingIds function can't be found within the NVAPI.DLL
            // It's looking like it is a problem with the NVAPI.DLL rather than with my code, but I need to do more testing to be sure.
            // Disabling this for now.
            //stringToReturn += DumpAllDRSSettings();

            stringToReturn += $"\n\n";
            // Now we also get the Windows CCD Library info, and add it to the above
            stringToReturn += WinLibrary.GetLibrary().PrintActiveConfig();

            return stringToReturn;
        }

        public bool SetActiveConfig(NVIDIA_DISPLAY_CONFIG displayConfig)
        {

            if (_initialised)
            {

                bool logicalGPURefreshNeeded = false;

                // Remove any custom NVIDIA Colour settings
                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off colour if it's default set colour.");
                foreach (var physicalGPU in displayConfig.PhysicalAdapters)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Processing settings for Physical GPU #{physicalGPU.Key}");
                    NVIDIA_PER_ADAPTER_CONFIG myAdapter = physicalGPU.Value;
                    UInt32 myAdapterIndex = physicalGPU.Key;
                    foreach (var displayDict in myAdapter.Displays)
                    {
                        NVIDIA_PER_DISPLAY_CONFIG myDisplay = displayDict.Value;
                        UInt32 displayId = displayDict.Key;

                        if (!_allConnectedDisplayIds.Contains(displayId))
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Display {displayId} doesn't exist in this setup, so skipping changing any NVIDIA display Settings.");
                            continue;
                        }

                        /*// Remove any custom NVIDIA Colour settings
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off colour if it's user set colour.");

                        ColorDataV5 colorData = (ColorDataV5)myDisplay.ColorData;
                        try
                        {
                            ColorDataV5 activeColorData = (ColorDataV5)ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].ColorData;
                            // If the setting for this display is not the same as we want, then we set it to NV_COLOR_SELECTION_POLICY_BEST_QUALITY
                            if (activeColorData.SelectionPolicy != ColorDataSelectionPolicy.BestQuality)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off NVIDIA customer colour settings for display {displayId}.");

                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want the standard colour settings to be {colorData.SelectionPolicy.ToString()} for Mosaic display {displayId}.");
                                // Force the colorData to be NV_COLOR_SELECTION_POLICY_BEST_QUALITY so that we return the color control to Windows
                                // We will change the colorData to whatever is required later on
                                //colorData = myDisplay.ColorData;
                                //TODO - Fix this color data so that it can be written to.

                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want the standard colour settings to be {colorData.SelectionPolicy.ToString()} and they are currently {activeColorData.SelectionPolicy.ToString()} for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off standard colour mode for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings Color selection policy {colorData.SelectionPolicy.ToString()} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings BPC {colorData.DesktopColorDepth} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings colour format {colorData.ColorFormat} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings colourimetry {colorData.Colorimetry} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings colour depth {colorData.ColorDepth} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings dynamic range {colorData.DynamicRange} for Mosaic display {displayId}");

                                // Set the command as a 'SET'
                                //colorData.Cmd = NV_COLOR_CMD.NV_COLOR_CMD_SET;
                                // TODO - set the command to set the color data!
                                try
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Attempting to remove any custom NVIDIA Color settings.");
                                    ColorDataV5 newColorData = new ColorDataV5(ColorDataCommand.Set,ColorDataFormat.Default,ColorDataColorimetry.Default, ColorDataDynamicRange.Auto, ColorDataDepth.Default, ColorDataSelectionPolicy.Default, ColorDataDesktopDepth.Default);

                                    NVAPI.ColorControl(displayId, ref newColorData);
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Successfully removed any custom NVIDIA Color settings. BPC is set to {colorData.DesktopColorDepth.ToString()}. Color Format is set to {colorData.ColorFormat.ToString("G")}. Colorimetry is set to {colorData.Colorimetry.ToString("G")}. Color Selection Policy is set to {colorData.SelectionPolicy.ToString()}. Color Depth is set to {colorData.ColorDepth.ToString()}. Dynamic Range is set to {colorData.DynamicRange.ToString()}");
                                    switch (colorData.SelectionPolicy)
                                    {
                                        case ColorDataSelectionPolicy.User:
                                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Color Selection Policy is set to NV_COLOR_SELECTION_POLICY_USER so the color settings have been set by the user in the NVIDIA Control Panel.");
                                            break;
                                        case ColorDataSelectionPolicy.BestQuality: // Also matches NV_COLOR_SELECTION_POLICY_DEFAULT as it is 1
                                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Color Selection Policy is set to NV_COLOR_SELECTION_POLICY_BEST_QUALITY so the color settings are being handled by the Windows OS.");
                                            break;
                                        case ColorDataSelectionPolicy.Unknown:
                                            SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: Color Selection Policy is set to NV_COLOR_SELECTION_POLICY_UNKNOWN so the color settings aren't being handled by either the Windows OS or the NVIDIA Setup!");
                                            break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst finding out if the GPU is from the Quadro range.");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want only want to turn off custom NVIDIA colour settings if needed for display {displayId}, and that currently isn't required. Skipping changing NVIDIA colour mode.");
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception caused while turning off prior NVIDIA specific colour settings for display {displayId}.");
                        }

                        // Remove any custom NVIDIA HDR Colour settings
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off HDR colour if it's user set HDR colour.");

                        HDRColorDataV2 hdrColorData = (HDRColorDataV2)myDisplay.HdrColorData;
                        try
                        {

                            // if it's not the same HDR we want, then we turn off HDR (and will apply it if needed later on in SetActiveOverride)
                            HDRColorDataV2 activeHdrColorData = (HDRColorDataV2)ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].HdrColorData;
                            if (activeHdrColorData.HDRMode != ColorDataHDRMode.Off)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn on custom HDR mode for display {displayId}.");

                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: HDR mode is currently {activeHdrColorData.HDRMode.ToString()} for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings BPC  {hdrColorData.ColorDepth} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings HDR Colour Format {hdrColorData.ColorFormat} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings HDR dynamic range {hdrColorData.DynamicRange} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings HDR Mode {hdrColorData.HDRMode} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings Mastering Display Data {hdrColorData.MasteringDisplayData} for Mosaic display {displayId}");
                                // Apply the HDR removal
                                HDRColorDataV2 newHdrColorData = new HDRColorDataV2(ColorDataHDRCommand.Set, ColorDataHDRMode.Off);                                
                                NVAPI.HDRColorControl(displayId, ref newHdrColorData);
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Disp_HdrColorControl returned OK. We just successfully turned off the HDR mode for Mosaic display {displayId}.");                                
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want only want to turn off custom NVIDIA HDR settings if needed for display {displayId}, and that currently isn't required. Skipping changing NVIDIA HDR mode.");
                            }

                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception caused while turning off prior NVIDIA HDR colour settings for display {displayId}.");
                        } */
                    } 
                }

                // If we get to here and there are no displays connected, then we need to return, as all the following settings back out custom settings per display
                // which are obviously not needed if the screens are off!
                if (!ActiveDisplayConfig.IsInUse)
                {
                    // we need to return true as everything is working as it should!
                    return true;
                }

                // Set the DRS Settings only if we need to
                if (displayConfig.DRSSettings.Count > 0)
                {
                    DRSSessionHandle drsSessionHandle = new DRSSessionHandle();
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Attempting to create a DRS Session Handle.");
                        drsSessionHandle = NVAPI.CreateSession();
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Successfully created a DRS Session Handle.");

                        // Load the current DRS Settings into memory
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Attempting to load the current DRS settings into memory.");
                        NVAPI.LoadSettings(drsSessionHandle);
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Successfully loaded the current DRS settings into memory.");


                        // Now we try to start getting the DRS Settings we need
                        // Firstly, we get the profile handle to the global DRS Profile currently in use
                        DRSProfileHandle drsProfileHandle = new DRSProfileHandle();
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Attempting to get the base DRS profile handle.");
                        drsProfileHandle = NVAPI.GetBaseProfile(drsSessionHandle);
                        if (drsProfileHandle.IsNull)
                        {
                            // There isn't a custom global profile set yet, so we ignore it
                            SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: NvAPI_DRS_GetCurrentGlobalProfile returned OK, but there was no process handle set. The DRS Settings may not have been loaded.");
                        }
                        else
                        {
                            // There is a custom global profile, so we continue
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_DRS_GetCurrentGlobalProfile returned OK. We got the DRS Profile Handle for the current global profile");

                            // Next, we go through all the settings we have in the saved profile, and we change the current profile settings to be the same
                            if (displayConfig.DRSSettings.Count > 0)
                            {
                                bool needToSave = false;
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: There are {displayConfig.DRSSettings.Count} stored DRS settings in the base DRS profile so we need to process them");

                                try
                                {
                                    // Get the Base Profiles from the stored config and the active config
                                    NVIDIA_DRS_CONFIG storedBaseProfile = displayConfig.DRSSettings.Find(p => p.IsBaseProfile == true);
                                    NVIDIA_DRS_CONFIG activeBaseProfile = ActiveDisplayConfig.DRSSettings.Find(p => p.IsBaseProfile == true);
                                    foreach (var drsSetting in storedBaseProfile.DriverSettings)
                                    {
                                        for (int i = 0; i < activeBaseProfile.DriverSettings.Count; i++)
                                        {
                                            DRSSettingV1 currentSetting = activeBaseProfile.DriverSettings[i];

                                            // If the setting is also in the active base profile (it should be!), then we set it.
                                            if (drsSetting.Id == currentSetting.Id)
                                            {
                                                if (drsSetting.CurrentValue.Equals(currentSetting.CurrentValue))
                                                {
                                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: '{currentSetting.Name}' ({currentSetting.Id}) is set to the same value as the one we want, so skipping changing it.");
                                                }
                                                else
                                                {
                                                    try
                                                    {
                                                        NVAPI.SetSetting(drsSessionHandle, drsProfileHandle, drsSetting);
                                                        needToSave = true;
                                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We changed setting '{currentSetting.Name}' ({currentSetting.Id}) from {currentSetting.CurrentValue} to {drsSetting.CurrentValue} using NvAPI_DRS_SetSetting()");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception caused whilst changing setting '{currentSetting.Name}' ({currentSetting.Id}) from {currentSetting.CurrentValue} to {drsSetting.CurrentValue}.");
                                                    }
                                                }
                                                break;
                                            }
                                        }
                                    }

                                    // Now go through and revert any unset settings to defaults. This guards against new settings being added by other profiles
                                    // after we've created a display profile. If we didn't do this those newer settings would stay set.                                        
                                    foreach (var currentSetting in activeBaseProfile.DriverSettings)
                                    {
                                        // Skip any settings that we've already set
                                        if (storedBaseProfile.DriverSettings.Exists(ds => ds.Id == currentSetting.Id))
                                        {
                                            continue;
                                        }

                                        try
                                        {
                                            // TODO: Need to create this function within the NVAPI codebase we ported from NvAPIWrapper code
                                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Attempting to restore the DRS settings to the defaults.");
                                            NVAPI.RestoreDefaults(drsSessionHandle, drsProfileHandle, currentSetting.Id);
                                            needToSave = true;
                                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We changed active setting '{currentSetting.Name}' ({currentSetting.Id}) from {currentSetting.CurrentValue} to it's default  value using NvAPI_DRS_RestoreProfileDefaultSetting()");
                                        }
                                        catch (Exception ex)
                                        {
                                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception while trying to find base profiles in either the stored or active display configs.");
                                        }

                                    }
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception while trying to find base profiles in either the stored or active display configs.");
                                }

                                // Next we save the Settings if needed
                                if (needToSave)
                                {
                                    // Save the current DRS Settings as we changed them
                                    try
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Attempting to save the current DRS settings.");
                                        NVAPI.SaveSettings(drsSessionHandle);
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We successfully saved the current DRS Settings.");
                                    }
                                    catch (Exception ex)
                                    {
                                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception while trying to save the current DRS settings.");
                                    }
                                }
                            }
                        }


                    }
                    finally
                    {
                        // Destroy the DRS Session Handle to clean up
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Attempting to destroy the DRS Session handle.");
                        NVAPI.DestroySession(drsSessionHandle);
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Successfully destroyed our DRS Session Handle");
                    }
                }

                /*// Now we set the NVIDIA Display Config (if we have one!)
                // If the display profile is a cloned config then NVIDIA GetDisplayConfig doesn't work
                // so we need to check for that. We just skip the SetDisplayConfig as it won't exist
                if (displayConfig.DisplayConfigs.Count > 0)
                {
                    try
                    {
                        // Set up the most basic path info that we can, as that is all that SetDisplayConfig will support
                        PathInfoV2[] myPathInfos = new PathInfoV2[1];
                        myPathInfos[0] = new PathInfoV2();
                       
                        PathTargetInfoV2 pti = new PathTargetInfoV2();
                        PathTargetInfoV2 dcpti = (PathTargetInfoV2)displayConfig.DisplayConfigs[0].TargetsInfo[0];

                        pti.DisplayId = dcpti.DisplayId;
                        //pti.Details = dcpti.Details;
                        pti.WindowsCCDTargetId = 0;

                        List<PathTargetInfoV2> ptiList = new List<PathTargetInfoV2>();
                        ptiList.Add(pti);

                        myPathInfos[0].Version = displayConfig.DisplayConfigs[0].Version;
                        myPathInfos[0].TargetsInfo = ptiList;
                        myPathInfos[0].TargetInfoCount = (uint)ptiList.Count;
                        
                        NVAPI.SetDisplayConfig(myPathInfos, DisplayConfigFlags.SaveToPersistence);
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Successfully set the displayconfig layout.");

                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception while trying to set the displayconfig layout.");
                        // We sometimes get an invalid argument here if NVIDIA has just disolved a mosaic surround screen into indivudal screens
                        // THis is because if there are any additional screens from other adapters, nvidia tells windows to disable them
                        // We need to wait until the Windows library applies the screen before the DisplayConfig will be applied.
                        if (displayConfig.DisplayConfigs.Count != displayConfig.MosaicConfig.MosaicGridCount)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: NvAPI_DISP_SetDisplayConfig() returned error code, but this is expected as there is a missing screen. Ignoring this error.");
                        }
                        else
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: One or more arguments passed in were invalid when we tried to set the display config.");
                            //TODO - Disabling the return for now as this isn't a critical error. Continuing allows us to complete the rest of the NVIDIA stuff without issue, which in turn allows WinLibrary to 
                            //       apply the display config properly. We do have to try and figure out why NVIDIA SetDisplayConfig is faulting. It is currently complaining of a NVAPI_INVALID_ARGUMENT.
                            //return false;
                        }
                    }

                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Skipping setting the NVIDIA Display Config as there isn't one provided in the configuration.");
                }*/

               /* try
                {
                    //NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags = NV_MOSAIC_SETDISPLAYTOPO_FLAGS.MAXIMIZE_PERFORMANCE;
                    SetDisplayTopologyFlag setTopoFlags = SetDisplayTopologyFlag.NoFlag;
                    // Attempt to set the displaygrids
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Attempting to set the mosaic display grids.");
                    GridTopologyV2[] gt = displayConfig.MosaicConfig.MosaicGridTopos;
                    NVAPI.SetDisplayGrids(gt, setTopoFlags);
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Successfully set the mosaic display grids.");

                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                    System.Threading.Thread.Sleep(500);
                    logicalGPURefreshNeeded = true;
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception while trying to set the mosaic display grids.");
                }*/


                // Now we've set the color the way we want it, lets do the thing
                // We want to check the NVIDIA Surround (Mosaic) config is valid
                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Testing whether the display configuration is valid");
                // 
                if (displayConfig.MosaicConfig.IsMosaicEnabled)
                {
                    if (displayConfig.MosaicConfig.Equals(ActiveDisplayConfig.MosaicConfig))
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic current config is exactly the same as the one we want, so skipping applying the Mosaic config");
                    }
                    else
                    {
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic current config is different as the one we want, so preparing the Mosaic config now, ready to be enabled.");
                            // If we get here then the display is valid, so now we actually apply the new Mosaic Topology
                            NVAPI.SetCurrentTopology(displayConfig.MosaicConfig.MosaicTopologyBrief, (IDisplaySettings)displayConfig.MosaicConfig.MosaicDisplaySettings, displayConfig.MosaicConfig.OverlapX, displayConfig.MosaicConfig.OverlapY, false);
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_SetCurrentTopo returned OK.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                            System.Threading.Thread.Sleep(500);
                        }
                        catch(Exception ex)
                        {
                            /*if (NVStatus == NVAPI_STATUS.NVAPI_NO_ACTIVE_SLI_TOPOLOGY)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: No matching GPU topologies could be found. NvAPI_Mosaic_SetCurrentTopo() returned error code {NVStatus}");
                                return false;
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_TOPO_NOT_POSSIBLE)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_SetCurrentTopo() returned error code {NVStatus}");
                                return false;
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_SetCurrentTopo() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_SetCurrentTopo() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_SetCurrentTopo() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_SetCurrentTopo() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_MODE_CHANGE_FAILED)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: There was an error changing the display mode. NvAPI_Mosaic_SetCurrentTopo() returned error code {NVStatus}");
                                return false;
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_SetCurrentTopo() returned error code {NVStatus}");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Display Grids! NvAPI_Mosaic_SetCurrentTopo() returned error code {NVStatus}");
                            }*/
                        }
                        
                        // Turn on the selected Mosaic
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Enabling the Grid Topology we just set up.");
                            NVAPI.EnableCurrentTopology(true);
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_EnableCurrentTopo returned OK. Previously set Mosiac config re-enabled.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                            System.Threading.Thread.Sleep(500);
                        }
                        catch (Exception ex)
                        {
                            /*if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_TOPO_NOT_POSSIBLE)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                                return false;
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_MODE_CHANGE_FAILED)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: There was an error disabling the display mode. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                                return false;
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            }*/
                        }                        

                        //NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags = NV_MOSAIC_SETDISPLAYTOPO_FLAGS.MAXIMIZE_PERFORMANCE;
                        SetDisplayTopologyFlag setTopoFlags = SetDisplayTopologyFlag.NoFlag;

                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic current config is different as the one we want, so applying the Mosaic config now");
                            // If we get here then the display is valid, so now we actually apply the new Mosaic Topology
                            NVAPI.SetDisplayGrids(displayConfig.MosaicConfig.MosaicGridTopos, setTopoFlags);
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_SetDisplayGrids returned OK.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                            System.Threading.Thread.Sleep(500);
                            logicalGPURefreshNeeded = true;
                        }
                        catch (Exception ex)
                        {
                            /*if (NVStatus == NVAPI_STATUS.NVAPI_NO_ACTIVE_SLI_TOPOLOGY)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: No matching GPU topologies could be found. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                                return false;
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_TOPO_NOT_POSSIBLE)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                                return false;
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: One or more arguments passed in are invalid. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}. This is often caused by new NVIDIA settings from an NVIDIA driver update. You may ned to recreate your Surround layout.");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_MODE_CHANGE_FAILED)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: There was an error changing the display mode. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                                return false;
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_OUT_OF_MEMORY)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The NvAPI driver is out of memory and is unable to allocate more. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                                return false;
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Display Grids! NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                            }*/
                        }
                        
                        
                    }

                }
                else if (!displayConfig.MosaicConfig.IsMosaicEnabled && ActiveDisplayConfig.MosaicConfig.IsMosaicEnabled)
                {
                    // We are on a Mosaic profile now, and we need to change to a non-Mosaic profile
                    // We need to disable the Mosaic Topology

                    //NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags = NV_MOSAIC_SETDISPLAYTOPO_FLAGS.ALLOW_INVALID;
                    //NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags = NV_MOSAIC_SETDISPLAYTOPO_FLAGS.NONE;
                    SetDisplayTopologyFlag setTopoFlags = SetDisplayTopologyFlag.NoFlag;

                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic config that is currently set is no longer needed. Removing Mosaic config.");
                    GridTopologyV2[] individualScreensTopology = CreateSingleScreenMosaicTopology();

                    // WARNING - Validation is disabled at present. This is mostly because there are errors in my NvAPI_Mosaic_ValidateDisplayGrids,
                    // but also because the config is coming from the NVIDIA Control Panel which will already do it's own validation checks.
                    /*// Firstly try to see if the oneScreenTopology is a valid config
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Checking if the 1x1 DisplayGrid we chose is valid for the NvAPI_Mosaic_SetDisplayGrids mosaic layout.");
                    NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[] individualScreensStatuses = new NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[(UInt32)individualScreensTopology.Length];
                    NVStatus = NVImport.NvAPI_Mosaic_ValidateDisplayGrids(setTopoFlags, individualScreensTopology, ref individualScreensStatuses, (UInt32)individualScreensTopology.Length);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_ValidateDisplayGrids returned OK.");
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                        System.Threading.Thread.Sleep(500);
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_ACTIVE_SLI_TOPOLOGY)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: No matching GPU topologies could be found. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_TOPO_NOT_POSSIBLE)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The Display ID of the first display is not currently possible to use. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}. Trying again with the next display.");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: One or more arguments passed in are invalid. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_MODE_CHANGE_FAILED)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: There was an error changing the display mode. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Display Grids! NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }*/


                    // If we get here then the display is valid, so now we actually apply the new Mosaic Topology
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Trying to set a 1x1 DisplayGrid for the NvAPI_Mosaic_SetDisplayGrids mosaic layout.");
                    try
                    {
                        NVAPI.SetDisplayGrids(individualScreensTopology, setTopoFlags);
                    }
                    catch (NVIDIAApiException nex)
                    {
                        if (nex.Status == Status.Ok)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_SetDisplayGrids returned OK.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                            System.Threading.Thread.Sleep(500);
                        }
                        else if (nex.Status == Status.NoActiveSLITopology)
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: No matching GPU topologies could be found. NvAPI_Mosaic_SetDisplayGrids() returned error code NoActiveSLITopology");
                            return false;
                        }
                        else if (nex.Status == Status.TopologyNotPossible)
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_SetDisplayGrids() returned error code TopologyNotPossible");
                            return false;
                        }
                        else if (nex.Status == Status.InvalidDisplayId)
                        {
                            SharedLogger.logger.Warn(nex, $"NVIDIALibrary/SetActiveConfig: The Display ID of the first display is not currently possible to use. NvAPI_Mosaic_SetDisplayGrids() returned error code InvalidDisplayId. Trying again with the next display.");
                            return false;
                        }
                        else if (nex.Status == Status.InvalidArgument)
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: One or more arguments passed in are invalid. NvAPI_Mosaic_SetDisplayGrids() returned error code InvalidArgument");
                            return false;
                        }
                        else if (nex.Status == Status.ApiNotInitialized)
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_SetDisplayGrids() returned error code ApiNotInitialized");
                            return false;
                        }
                        else if (nex.Status == Status.NoImplementation)
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_SetDisplayGrids() returned error code NoImplementation");
                            return false;
                        }
                        else if (nex.Status == Status.IncompatibleStructureVersion)
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_SetDisplayGrids() returned error code IncompatibleStructureVersion");
                            return false;
                        }
                        else if (nex.Status == Status.ModeChangeFailed)
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: There was an error changing the display mode. NvAPI_Mosaic_SetDisplayGrids() returned error code ModeChangeFailed");
                            return false;
                        }
                        else if (nex.Status == Status.Error)
                        {
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_SetDisplayGrids() returned error code Error");
                            return false;
                        }
                        else
                        {
                            // If we get here, we may have an error, or it may have worked successfully! So we need to check again :( 
                            SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: NVIDIAApiException while trying to set a 1x1 DisplayGrid for the NvAPI_Mosaic_SetDisplayGrids mosaic layout.");
                        }
                    }
                    catch (Exception ex)
                    {
                        // If we get here, we may have an error, or it may have worked successfully! So we need to check again :( 
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: General Exception while trying to set a 1x1 DisplayGrid for the NvAPI_Mosaic_SetDisplayGrids mosaic layout.");;
                    }
                    // If we get here, it may or it may not have worked successfully! So we need to check again :( 
                    // We don't want to do a full ceck, so we do a quick check instead.
                    if (MosaicIsOn())
                    {
                        // If the Mosaic is still on, then the last mosaic disable failed, so we need to then try turning it off this using NvAPI_Mosaic_EnableCurrentTopo(0)
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Previous attempt to turn off Mosaic. Now trying to use NvAPI_Mosaic_EnableCurrentTopo to disable Mosaic instead.");
                        try
                        {
                            NVAPI.EnableCurrentTopology(false);
                        }
                        catch (NVIDIAApiException nex)
                        {
                            if (nex.Status == Status.Ok)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_SetDisplayGrids attempt 2 returned OK.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                                System.Threading.Thread.Sleep(500);
                            }
                            else if (nex.Status == Status.NoActiveSLITopology)
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: No matching GPU topologies could be found. NvAPI_Mosaic_EnableCurrentTopo() attempt 2 returned error code NoActiveSLITopology");
                                return false;
                            }
                            else if (nex.Status == Status.TopologyNotPossible)
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_EnableCurrentTopo() attempt 2 returned error code TopologyNotPossible");
                                return false;
                            }
                            else if (nex.Status == Status.InvalidDisplayId)
                            {
                                SharedLogger.logger.Warn(nex, $"NVIDIALibrary/SetActiveConfig: The Display ID of the first display is not currently possible to use. NvAPI_Mosaic_EnableCurrentTopo() attempt 2 returned error code InvalidDisplayId. Trying again with the next display.");
                                return false;
                            }
                            else if (nex.Status == Status.InvalidArgument)
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: One or more arguments passed in are invalid. NvAPI_Mosaic_EnableCurrentTopo() attempt 2 returned error code InvalidArgument");
                                return false;
                            }
                            else if (nex.Status == Status.ApiNotInitialized)
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_EnableCurrentTopo() attempt 2 returned error code ApiNotInitialized");
                                return false;
                            }
                            else if (nex.Status == Status.NoImplementation)
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_EnableCurrentTopo() attempt 2 returned error code NoImplementation");
                                return false;
                            }
                            else if (nex.Status == Status.IncompatibleStructureVersion)
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_EnableCurrentTopo() attempt 2 returned error code IncompatibleStructureVersion");
                                return false;
                            }
                            else if (nex.Status == Status.ModeChangeFailed)
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: There was an error changing the display mode. NvAPI_Mosaic_EnableCurrentTopo() attempt 2 returned error code ModeChangeFailed");
                                return false;
                            }
                            else if (nex.Status == Status.Error)
                            {
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_EnableCurrentTopo() attempt 2 returned error code Error");
                                return false;
                            }
                            else
                            {
                                // If we get here, we may have an error, or it may have worked successfully! So we need to check again :( 
                                SharedLogger.logger.Error(nex, $"NVIDIALibrary/SetActiveConfig: NVIDIAApiException while trying to set a 1x1 DisplayGrid for the NvAPI_Mosaic_EnableCurrentTopo attempt 2 mosaic layout.");
                            }
                        }
                        catch (Exception ex)
                        {
                            // If we get here, we may have an error, or it may have worked successfully! So we need to check again :( 
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: General Exception while trying to set a 1x1 DisplayGrid for the NvAPI_Mosaic_EnableCurrentTopo attempt 2 mosaic layout."); ;
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic successfully disabled using NvAPI_Mosaic_EnableCurrentTopo attempt 2  method.");
                    }
                }
                else if (!displayConfig.MosaicConfig.IsMosaicEnabled && !ActiveDisplayConfig.MosaicConfig.IsMosaicEnabled)
                {
                    // We are on a non-Mosaic profile now, and we are changing to a non-Mosaic profile
                    // so there is nothing to do as far as NVIDIA is concerned!
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We are on a non-Mosaic profile now, and we are changing to a non-Mosaic profile so there is no need to modify Mosaic settings!");
                }

                // If the NVIDIA topology has changed, then we need to refresh our active config so it stays valid. 
                //if (logicalGPURefreshNeeded)
                //{
                //    UpdateActiveConfig();
                //}


            }
            else
            {
                SharedLogger.logger.Info($"NVIDIALibrary/SetActiveConfig: Tried to run SetActiveConfig but the NVIDIA NvAPI library isn't initialised! This generally means you don't have a NVIDIA video card in your machine.");
                //throw new NVIDIALibraryException($"Tried to run SetActiveConfig but the NVIDIA NvAPI library isn't initialised!");
            }

            return true;
        }

        public bool SetActiveConfigOverride(NVIDIA_DISPLAY_CONFIG displayConfig)
        {

            if (_initialised)
            {

                // We need to first update the active config to make sure it's set
                UpdateActiveConfig();

                // Go through the physical adapters
                foreach (var physicalGPU in displayConfig.PhysicalAdapters)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Processing settings for Physical GPU #{physicalGPU.Key}");
                    NVIDIA_PER_ADAPTER_CONFIG myAdapter = physicalGPU.Value;
                    UInt32 myAdapterIndex = physicalGPU.Key;

                    foreach (var displayDict in myAdapter.Displays)
                    {
                        NVIDIA_PER_DISPLAY_CONFIG myDisplay = displayDict.Value;
                        UInt32 displayId = displayDict.Key;

                        // Now we try to set each display settings
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to process settings for display {displayId}.");

                        if (!_allConnectedDisplayIds.Contains(displayId))
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Display {displayId} doesn't exist in this setup, so skipping overriding any NVIDIA display Settings.");
                            continue;
                        }

                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn on colour if it's user set colour.");
                        // Now we try to set each display color



                        ColorDataV5 colorData = (ColorDataV5)myDisplay.ColorData;
                        ColorDataV5 activeColorData = (ColorDataV5)ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].ColorData;
                        // If the setting for this display is not the same as we want, then we set it to NV_COLOR_SELECTION_POLICY_BEST_QUALITY
                        if (ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].ColorData.SelectionPolicy != colorData.SelectionPolicy)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to set the NVIDIA custom colour settings for display {displayId} to what the user wants them to be.");

                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to use custom NVIDIA HDR Colour for display {displayId}.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want the standard colour settings to be {myDisplay.ColorData.SelectionPolicy.ToString()} and they are {ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].ColorData.SelectionPolicy.ToString()} for Mosaic display {displayId}.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn off standard colour mode for Mosaic display {displayId}.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings Color selection policy {colorData.SelectionPolicy.ToString()} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings Desktop Colour Depth {colorData.DesktopColorDepth} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings colour format {colorData.ColorFormat} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings colourimetry {colorData.Colorimetry} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings colour depth {colorData.ColorDepth} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings dynamic range {colorData.DynamicRange} for Mosaic display {displayId}");

                            // Set the command as a 'SET'
                            //colorData.Cmd = NV_COLOR_CMD.NV_COLOR_CMD_SET;
                            // TODO - set the command to set the color data!
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Attempting to set the custom NVIDIA Color settings to what the user wants.");
                                ColorDataV5 newColorData = new ColorDataV5(ColorDataCommand.Set, colorData.ColorFormat, colorData.Colorimetry, colorData.DynamicRange.Value, colorData.ColorDepth.Value, colorData.SelectionPolicy.Value, colorData.DesktopColorDepth.Value);
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Attempting to set the displayconfig layout.");
                                NVAPI.ColorControl(displayId, ref newColorData);
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully changed to the user's custom NVIDIA Color settings.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception occurred whilst trying to dset the user's custom color settings.");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want only want to set the user's custom NVIDIA colour settings if needed for display {displayId}, and that currently isn't required. Skipping changing NVIDIA colour mode.");
                        }

                        // Apply any custom NVIDIA HDR Colour settings
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn on NVIDIA HDR colour if it's user wants to use NVIDIA HDR colour.");

                        HDRColorDataV2 hdrColorData = (HDRColorDataV2)myDisplay.HdrColorData;
                        try
                        {

                            // if it's not the same HDR we want, then we turn off HDR (and will apply it if needed later on in SetActiveOverride)
                            HDRColorDataV2 activeHdrColorData = (HDRColorDataV2)ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].HdrColorData;
                            // if it's HDR and it's a different mode than what we are in now, then set HDR
                            if (activeHdrColorData.HDRMode != hdrColorData.HDRMode)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn on user-set HDR mode for display {displayId} as it's supposed to be on.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: HDR mode is currently {activeHdrColorData.HDRMode.ToString("G")} for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings ColorDepth  {hdrColorData.ColorDepth} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings HDR Colour Format {hdrColorData.ColorFormat} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings HDR dynamic range {hdrColorData.DynamicRange} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings HDR Mode {hdrColorData.HDRMode} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings Mastering Display Data {hdrColorData.MasteringDisplayData} for Mosaic display {displayId}");
                                // Apply the HDR removal
                                HDRColorDataV2 newHdrColorData = new HDRColorDataV2(ColorDataHDRCommand.Set,
                                    hdrColorData.HDRMode,
                                    hdrColorData.MasteringDisplayData,
                                    hdrColorData.ColorFormat.Value,
                                    hdrColorData.DynamicRange.Value,
                                    hdrColorData.ColorDepth.Value);
                                NVAPI.HDRColorControl(displayId, ref newHdrColorData);
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Attempting to set the HDR settings that the user wants.");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want only want to turn on custom NVIDIA HDR settings if the settings the user wants for display {displayId} are different to those already set. The settings are the same, so skipping changing NVIDIA HDR mode.");
                            }

                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception caused while attempting to set the user's NVIDIA HDR colour settings for display {displayId}.");
                        }

                        // Disabled the Adaptive Sync equality matching as we are having trouble applying it, which is causing issues in profile matching in DisplayMagician
                        // To fix this bit, we need to test the SetActiveConfigOverride Adaptive Sync part of the codebase to apply this properly.
                        // But for now, we'll exclude it from the equality matching and also stop trying to use the adaptive sync config.

                        /*// Set any AdaptiveSync settings
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to set any adaptive Sync settings if in use.");

                        NV_SET_ADAPTIVE_SYNC_DATA_V1 adaptiveSyncData = myDisplay.AdaptiveSyncConfig;
                        try
                        {
                            if (myDisplay.AdaptiveSyncConfig.DisableAdaptiveSync)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to DISABLE Adaptive Sync for display {displayId}.");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to ENABLE Adaptive Sync for display {displayId}.");
                            }

                            if (myDisplay.AdaptiveSyncConfig.DisableFrameSplitting)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to DISABLE Frame Splitting for display {displayId}.");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to ENABLE Frame Splitting for display {displayId}.");
                            }
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to set the Adaptice Sync Max Frame Interval to {myDisplay.AdaptiveSyncConfig.MaxFrameInterval}ms for display {displayId}.");

                            // Apply the AdaptiveSync settings
                            status = NVAPI.SetAdaptiveSyncData(displayId, ref adaptiveSyncData);
                            if (status == Status.Ok)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_DISP_SetAdaptiveSyncData returned OK. We just successfully set the Adaptive Sync settings for display {displayId}.");
                            }
                            else if (status == Status.InsufficientBuffer)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The input buffer is not large enough to hold it's contents. NvAPI_DISP_SetAdaptiveSyncData() returned error code {status}");
                            }
                            else if (status == Status.InvalidDisplayId)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_DISP_SetAdaptiveSyncData() returned error code {status}");
                            }
                            else if (status == Status.ApiNotInitialized)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_SetAdaptiveSyncData() returned error code {status}");
                            }
                            else if (status == Status.NoImplementation)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_SetAdaptiveSyncData() returned error code {status}");
                            }
                            else if (status == Status.Error)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_DISP_SetAdaptiveSyncData() returned error code {status}");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_DISP_SetAdaptiveSyncData() returned error code {status}. It's most likely that your monitor {displayId} doesn't support HDR.");
                            }

                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception caused while trying to set NVIDIA Adaptive Sync settings for display {displayId}.");
                        }*/
                    }

                }

            }
            else
            {
                SharedLogger.logger.Info($"NVIDIALibrary/SetActiveConfigOverride: Tried to run SetActiveConfig but the NVIDIA NVAPI library isn't initialised! This generally means you don't have a NVIDIA video card in your machine.");
                //throw new NVIDIALibraryException($"Tried to run SetActiveConfigOverride but the NVIDIA NVAPI library isn't initialised!");
            }

            return true;
        }

        public bool IsActiveConfig(NVIDIA_DISPLAY_CONFIG displayConfig)
        {
            // Check whether the display config is in use now
            SharedLogger.logger.Trace($"NVIDIALibrary/IsActiveConfig: Checking whether the display configuration is already being used.");
            if (displayConfig.Equals(_activeDisplayConfig))
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsActiveConfig: The display configuration is already being used (supplied displayConfig Equals currentDisplayConfig");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsActiveConfig: The display configuration is NOT currently in use (supplied displayConfig does NOT equal currentDisplayConfig");
                return false;
            }

        }

        public bool IsValidConfig(NVIDIA_DISPLAY_CONFIG displayConfig)
        {
            // We want to check the NVIDIA Surround (Mosaic) config is valid
            SharedLogger.logger.Trace($"NVIDIALibrary/IsValidConfig: Testing whether the display configuration is valid");
            // 
            if (displayConfig.MosaicConfig.IsMosaicEnabled)
            {

                // ===================================================================================================================================
                // Important! ValidateDisplayGrids does not work at the moment. It errors when supplied with a Grid Topology that works in SetDisplaGrids
                // We therefore cannot use ValidateDisplayGrids to actually validate the config before it's use. We instead need to rely on SetDisplaGrids reporting an
                // error if it is unable to apply the requested configuration. While this works fine, it's not optimal.
                // TODO: Test ValidateDisplayGrids in a future NVIDIA driver release to see if they fixed it.
                // ===================================================================================================================================
                return true;

                /*// Figure out how many Mosaic Grid topoligies there are                    
                uint mosaicGridCount = 0;
                Status status = NVAPI.EnumDisplayGrids(ref mosaicGridCount);
                if (status == Status.Ok)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");
                }

                // Get Current Mosaic Grid settings using the Grid topologies fnumbers we got before
                //NV_MOSAIC_GRID_TOPO_V2[] mosaicGridTopos = new NV_MOSAIC_GRID_TOPO_V2[mosaicGridCount];
                NV_MOSAIC_GRID_TOPO_V1[] mosaicGridTopos = new NV_MOSAIC_GRID_TOPO_V1[mosaicGridCount];
                status = NVAPI.EnumDisplayGrids(ref mosaicGridTopos, ref mosaicGridCount);
                if (status == Status.Ok)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");
                }
                else if (status == Status.NotSupported)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_GetCurrentTopo() returned error code {status}");
                }
                else if (status == Status.InvalidArgument)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_GetCurrentTopo() returned error code {status}");
                }
                else if (status == Status.ApiNotInitialized)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_GetCurrentTopo() returned error code {status}");
                }
                else if (status == Status.NoImplementation)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_GetCurrentTopo() returned error code {status}");
                }
                else if (status == Status.Error)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_GetCurrentTopo() returned error code {status}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_GetCurrentTopo() returned error code {status}");
                }
                */

                /*NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags = NV_MOSAIC_SETDISPLAYTOPO_FLAGS.NONE;
                bool topoValid = false;
                NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[] topoStatuses = new NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[displayConfig.MosaicConfig.MosaicGridCount];
                Status status = NVAPI.ValidateDisplayGrids(setTopoFlags, ref displayConfig.MosaicConfig.MosaicGridTopos, ref topoStatuses, displayConfig.MosaicConfig.MosaicGridCount);
                //NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[] topoStatuses = new NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[mosaicGridCount];
                //status = NVAPI.ValidateDisplayGrids(setTopoFlags, ref mosaicGridTopos, ref topoStatuses, mosaicGridCount);
                if (status == Status.Ok)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");

                    for (int i = 0; i < topoStatuses.Length; i++)
                    {
                        // If there is an error then we need to log it!
                        // And make it not be used
                        if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Congratulations! No error flags for GridTopology #{i}");
                            topoValid = true;
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.DISPLAY_ON_INVALID_GPU)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: Display is on an invalid GPU");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.DISPLAY_ON_WRONG_CONNECTOR)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: Display is on the wrong connection. It was on a different connection when the display profile was saved.");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.ECC_ENABLED)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: ECC has been enabled, and Mosaic/Surround doesn't work with ECC");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.GPU_TOPOLOGY_NOT_SUPPORTED)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: This GPU topology is not supported.");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.MISMATCHED_OUTPUT_TYPE)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: The output type has changed for the display. The display was connected through another output type when the display profile was saved.");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.NOT_SUPPORTED)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: This Grid Topology is not supported on this video card.");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.NO_COMMON_TIMINGS)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: Couldn't find common timings that suit all the displays in this Grid Topology.");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.NO_DISPLAY_CONNECTED)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: No display connected.");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.NO_EDID_AVAILABLE)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: Your display didn't provide any information when we attempted to query it. Your display either doesn't support support EDID querying or has it a fault. ");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.NO_GPU_TOPOLOGY)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: There is no GPU topology provided.");
                        }
                        else if (topoStatuses[i].ErrorFlags == NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS.NO_SLI_BRIDGE)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Error with the GridTopology #{i}: There is no SLI bridge, and there was one when the display profile was created.");
                        }

                        // And now we also check to see if there are any warnings we also need to log
                        if (topoStatuses[i].WarningFlags == NV_MOSAIC_DISPLAYTOPO_WARNING_FLAGS.NONE)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Congratulations! No warning flags for GridTopology #{i}");
                        }
                        else if (topoStatuses[i].WarningFlags == NV_MOSAIC_DISPLAYTOPO_WARNING_FLAGS.DISPLAY_POSITION)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: Warning for the GridTopology #{i}: The display position has changed, and this may affect your display view.");
                        }
                        else if (topoStatuses[i].WarningFlags == NV_MOSAIC_DISPLAYTOPO_WARNING_FLAGS.DRIVER_RELOAD_REQUIRED)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: Warning for the GridTopology #{i}: Your computer needs to be restarted before your NVIDIA device driver can use this Grid Topology.");
                        }
                    }

                }
                else if (status == Status.NotSupported)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }
                else if (status == Status.NVAPI_NO_ACTIVE_SLI_TOPOLOGY)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: No matching GPU topologies could be found. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }
                else if (status == Status.NVAPI_TOPO_NOT_POSSIBLE)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }
                else if (status == Status.InvalidArgument)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }
                else if (status == Status.ApiNotInitialized)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }
                else if (status == Status.NoImplementation)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }
                else if (status == Status.IncompatibleStructureVersion)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }
                else if (status == Status.ModeChangeFailed)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: There was an error changing the display mode. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }
                else if (status == Status.Error)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_ValidateDisplayGrids() returned error code {status}");
                }


                // Cancel the screen change if there was an error with anything above this.
                if (topoValid)
                {
                    // If there was an issue then we need to return false
                    // to indicate that the display profile can't be applied
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: The display settings are valid.");
                    return true;
                }
                else
                {
                    // If there was an issue then we need to return false
                    // to indicate that the display profile can't be applied
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: There was an error when validating the requested grid topology that prevents us from using the display settings provided. THe display setttings are NOT valid.");
                    return false;
                }*/
            }
            else
            {
                // Its not a Mosaic topology, so we just let it pass, as it's windows settings that matter.
                return true;
            }
        }

        public bool IsPossibleConfig(NVIDIA_DISPLAY_CONFIG displayConfig)
        {
            // We want to check the NVIDIA profile can be used now
            SharedLogger.logger.Trace($"NVIDIALibrary/IsPossibleConfig: Testing whether the NVIDIA display configuration is possible to be used now");

            // CHeck that we have all the displayConfig DisplayIdentifiers we need available now
            if (displayConfig.DisplayIdentifiers.All(value => _allConnectedDisplayIdentifiers.Contains(value)))
            //if (currentAllIds.Intersect(displayConfig.DisplayIdentifiers).Count() == displayConfig.DisplayIdentifiers.Count)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsPossibleConfig: Success! The NVIDIA display configuration is possible to be used now");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsPossibleConfig: Uh oh! The NVIDIA display configuration is possible cannot be used now");
                return false;
            }

        }

        public static bool MosaicIsOn()
        {
            PhysicalGPUHandle[] physicalGpus = new PhysicalGPUHandle[NvConstants.NVAPI_MAX_PHYSICAL_GPUS];
            try
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/MosaicIsOn: Attempting to get the list of physical GPUs.");
                physicalGpus = NVAPI.EnumPhysicalGPUs();
                SharedLogger.logger.Trace($"NVIDIALibrary/MosaicIsOn: Successfully got the list of physical GPUS. There are {physicalGpus.Length} Physical GPUs.");

                // If we have a physical GPU
                if (physicalGpus.Length > 0)
                {
                    // Get current Mosaic Topology settings in brief (check whether Mosaic is on)
                    TopologyBrief mosaicTopoBrief = new TopologyBrief();
                    IDisplaySettings mosaicDisplaySetting = new DisplaySettingsV2();
                    int mosaicOverlapX = 0;
                    int mosaicOverlapY = 0;
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/MosaicIsOn: Attempting to get the mosaic topology.");
                        NVAPI.GetCurrentTopology(out mosaicTopoBrief, out mosaicDisplaySetting, out mosaicOverlapX, out mosaicOverlapY);
                        SharedLogger.logger.Trace($"NVIDIALibrary/MosaicIsOn: Successfully got the mosaic topology. The mosaic topology is {physicalGpus.Length} Physical GPUs.");
                        DisplaySettingsV2 mosaicDisplaySettingv2 = (DisplaySettingsV2)mosaicDisplaySetting;

                        // Check if there is a topology and that Mosaic is enabled
                        if (mosaicTopoBrief.Topology != Topology.None && mosaicTopoBrief.IsEnable)
                        {
                            return true;
                        }

                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/MosaicIsOn: Exception occurred whilst getting the list pf physical GPUs.");
                        return false;
                    }
                }

            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"NVIDIALibrary/MosaicIsOn: Exception occurred whilst getting the list pf physical GPUs.");
            }
            return false;
        }

        public List<string> GetCurrentDisplayIdentifiers(out bool failure)
        {
            SharedLogger.logger.Trace($"NVIDIALibrary/GetCurrentDisplayIdentifiers: Getting the current display identifiers for the displays in use now");
            return GetSomeDisplayIdentifiers(out failure, false);
        }

        public List<string> GetAllConnectedDisplayIdentifiers(out bool failure)
        {
            SharedLogger.logger.Trace($"NVIDIALibrary/GetAllConnectedDisplayIdentifiers: Getting all the display identifiers that can possibly be used");
            _allConnectedDisplayIdentifiers = GetSomeDisplayIdentifiers(out failure, true);

            return _allConnectedDisplayIdentifiers;
        }

        private List<string> GetSomeDisplayIdentifiers(out bool failure, bool allDisplays = true)
        {
            SharedLogger.logger.Debug($"NVIDIALibrary/GetCurrentDisplayIdentifiers: Generating the unique Display Identifiers for the currently active configuration");

            List<string> displayIdentifiers = new List<string>();
            failure = false;

            // Enumerate all the Physical GPUs
            PhysicalGPUHandle[] physicalGpus = new PhysicalGPUHandle[NvConstants.NV_MAX_PHYSICAL_GPUS];
            try
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the list of physical GPUs.");
                physicalGpus = NVAPI.EnumPhysicalGPUs();
                SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the list of physical GPUs. There are {physicalGpus.Length} Physical GPUs.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the list of physical GPUs. There is either no NVIDIA video card, or you have a laptop with an eGPU and no displays connected to it are currently enabled.");
                failure = true;
            }
 
            // This check is to make sure that if there aren't any physical GPUS then we exit!
            if (physicalGpus.Length == 0)
            {
                // If there aren't any video cards detected, then return that empty list.
                SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: No Videocards detected so returning empty list");
                return new List<string>();
            }

            // Now we need to loop through each of the windows paths so we can record the Windows DisplayName to DisplayID mapping
            // This is needed for us to piece together the Screen layout for when we draw the NVIDIA screens!
            Dictionary<string, string> DisplayNames = new Dictionary<string, string>();
            foreach (KeyValuePair<string, List<uint>> displaySource in WinLibrary.GetDisplaySourceNames())
            {
                // Now we try to get the information about the displayIDs and map them to windows \\DISPLAY names e.g. \\DISPLAY1
                string displayName = displaySource.Key;
                UInt32 displayId = 0;
                try
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to get the Windows DisplayName to DisplayID mappings for Display ID {displayName}.");
                    displayId = NVAPI.GetDisplayIdByDisplayName(displayName);
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the Windows DisplayName to DisplayID mappings for Display ID {displayName} is set to {displayId}.");
                    DisplayNames.Add(displayId.ToString(), displayName);
                }
                catch (NVIDIAApiException nex)
                {
                    if (nex.Status == Status.NvidiaDeviceNotFound)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: The display named '{displayName}' is not connected via an NVIDIA device. Skipping adding this Display Name.");
                    }
                    else
                    {
                        SharedLogger.logger.Error(nex, $"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Exception occurred whilst getting the Windows DisplayName to DisplayID mappings for Display ID {displayName}.");
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the Windows DisplayName to DisplayID mappings for Display ID {displayName}.");
                }

            }

            // Go through the Physical GPUs one by one
            for (uint physicalGpuIndex = 0; physicalGpuIndex < physicalGpus.Length; physicalGpuIndex++)
            {
                //We want to get the name of the physical device
                string gpuName = "";
                try {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the name of the physical GPU #{physicalGpuIndex+1}.");
                    gpuName = NVAPI.GetFullName(physicalGpus[physicalGpuIndex]);
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the GPU fullname of the physical GPU #{physicalGpuIndex + 1}. The GPU Full Name is {gpuName.ToString()}");
                }
                catch (Exception ex) 
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the fullname of the physical GPU #{physicalGpuIndex + 1}.");
                }

                // We want to get the physical details of the physical device
                // This is the GPU Bus Type
                GPUBusType busType = GPUBusType.Undefined;
                try
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the bus type of the physical GPU #{physicalGpuIndex + 1}.");
                    busType = NVAPI.GetBusType(physicalGpus[physicalGpuIndex]);
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the bus type of the physical GPU #{physicalGpuIndex + 1}. The bus type is {busType.ToString()}");
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the bustype of the physical GPU #{physicalGpuIndex + 1}.");
                }

                // This is the GPU Bus ID
                Int32 busId = 0;
                try
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the bus ID of the physical GPU #{physicalGpuIndex + 1}.");
                    busId = NVAPI.GetBusId(physicalGpus[physicalGpuIndex]);
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the bus ID of the physical GPU #{physicalGpuIndex + 1}. The bus ID is {busId}");
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the bus ID of the physical GPU #{physicalGpuIndex + 1}.");
                }


                // Next, we need to get all the connected Display IDs. 
                //This function retrieves the number of display IDs we know about
                DisplayIdsV2[] displayIds = new DisplayIdsV2[0];
                if (allDisplays)
                {
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the list of all displays connected to the physical GPU #{physicalGpus[physicalGpuIndex]}.");
                        displayIds = NVAPI.GetAllDisplayIds(physicalGpus[physicalGpuIndex]);
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the list of all displays connected to the physical GPU #{physicalGpus[physicalGpuIndex]}. There are currently {displayIds.Length} displays connected.");
                        // Update the latest list of all connected display ids
                        _allConnectedDisplayIds.Clear();
                        foreach (DisplayIdsV2 displayId in displayIds) {
                            _allConnectedDisplayIds.Add(displayId.DisplayId);
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the list of all displays connected to the physical GPU #{physicalGpus[physicalGpuIndex]}.");
                    }
                }
                else
                {
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the list of displays currently connected to the physical GPU #{physicalGpus[physicalGpuIndex]}.");
                        displayIds = NVAPI.GetConnectedDisplayIds(physicalGpus[physicalGpuIndex], ConnectedIdsFlag.None);
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the list of displays currently connected to the physical GPU #{physicalGpus[physicalGpuIndex]}. There are currently {displayIds.Length} displays connected.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the list of displays currently connected to the physical GPU #{physicalGpus[physicalGpuIndex]}.");
                    }
                }
                    


                // Now, we want to go through the displays as we ONLY want to record the GPUs and displays that are available now 
                foreach (DisplayIdsV2 oneDisplay in displayIds)
                {
                    // If alldisplays is false, then we only want the active displays. We need to skip this one if it is not active
                    if (allDisplays == false && oneDisplay.IsActive == false)
                    {
                        // We want to skip this display as it is non-active, and we only want active displays in this mode
                        continue;
                    }


                    // Now we try to get the GPU and Output ID from the DisplayID
                    PhysicalGPUHandle physicalGpu = new PhysicalGPUHandle();
                    OutputId gpuOutputId = OutputId.Invalid;
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the GPU output ID of display ID #{oneDisplay.DisplayId} currently connected to the physical GPU #{physicalGpus[physicalGpuIndex]}.");
                        NVAPI.GetGpuAndOutputIdFromDisplayId(oneDisplay.DisplayId, out physicalGpu, out gpuOutputId);
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the GPU output ID of display ID #{oneDisplay.DisplayId} currently connected to the physical GPU #{physicalGpu} on Output ID #{gpuOutputId}. There are currently {displayIds.Length} displays connected.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the GPU output ID of display ID #{oneDisplay.DisplayId} currently connected to the physical GPU #{physicalGpuIndex + 1}.");
                    }

                    // The GetEDID function in NVIDIA doesn't work reliably, and often errors saying that the driver cannot get the EDDID information. 
                    // Lets set some EDID default in case the EDID doesn't work (which is likely to happen now as NVIDIA EDID is unreliable at best :( )
                    string manufacturerName = "Unknown";
                    UInt32 productCode = 0;
                    UInt32 serialNumber = 0;
                    // We try to get an EDID block and extract the info                        
                    EDIDV3 edidInfo = new EDIDV3();
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the EDID information from display ID #{oneDisplay.DisplayId} currently connected to the physical GPU #{physicalGpus[physicalGpuIndex]}.");
                        edidInfo = (EDIDV3)NVAPI.GetEDID(physicalGpu, gpuOutputId, 0);
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the EDID information from display ID #{oneDisplay.DisplayId} currently connected to the physical GPU #{physicalGpus[physicalGpuIndex]}. There are currently {displayIds.Length} displays connected.");
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to parse the EDID information from display ID #{oneDisplay.DisplayId} so that we can read it.");
                        EDID edidParsedInfo = new EDID(edidInfo.Data);
                        manufacturerName = edidParsedInfo.ManufacturerCode;
                        productCode = edidParsedInfo.ProductCode;
                        serialNumber = edidParsedInfo.SerialNumber;
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Found that the manufacturer name is {manufacturerName}, the product code is {productCode}, and the serial numver is {serialNumber}.");

                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the EDID information from display ID #{oneDisplay.DisplayId} currently connected to the physical GPU #{physicalGpus[physicalGpuIndex]}. This is unfortuntately common now, and appears to be a bug in the NVIDIA driver.");
                    }

                    // Create an array of all the important display info we need to record
                    List<string> displayInfo = new List<string>();
                    displayInfo.Add("NVIDIA");
                    try
                    {
                        displayInfo.Add(gpuName.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting GPU Name from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(busType.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting GPU Bus Type from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(busId.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting GPU Bus ID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(oneDisplay.ConnectionType.ToString("G"));
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting GPU Output ID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(manufacturerName.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting NVIDIA EDID Manufacturer Name for the display from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(productCode.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting NVIDIA EDID Product Code for the display from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(serialNumber.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting NVIDIA EDID Serial Number for the display from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    try
                    {
                        displayInfo.Add(oneDisplay.DisplayId.ToString());
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting Display ID from video card. Substituting with a # instead");
                        displayInfo.Add("#");
                    }
                    // Create a display identifier out of it
                    string displayIdentifier = String.Join("|", displayInfo);
                    // Add it to the list of display identifiers so we can return it
                    // but only add it if it doesn't already exist. Otherwise we get duplicates :/
                    if (!displayIdentifiers.Contains(displayIdentifier))
                    {
                        displayIdentifiers.Add(displayIdentifier);
                        SharedLogger.logger.Debug($"NVIDIALibrary/GetSomeDisplayIdentifiers: DisplayIdentifier detected: {displayIdentifier}");
                    }
                }
            }

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
        }

        public static string DumpAllDRSSettings()
        {
            // This bit of code dumps all the profiles in the DRS, and all the settings within that
            // This is really only used for debugging, but is still very useful to have!
            // Get the DRS Settings
            string stringToReturn = "";
            stringToReturn += $"\n****** CURRENTLY SET NVIDIA DRIVER SETTINGS (DRS) *******\n";

            // Set the DRS Settings
            DRSSessionHandle drsSessionHandle = new DRSSessionHandle();
            try
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to create a DRS Session Handle.");
                drsSessionHandle = NVAPI.CreateSession();
                SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully created a DRS Session Handle.");

                // Load the current DRS Settings into memory
                SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to load the current DRS settings into memory.");
                NVAPI.LoadSettings(drsSessionHandle);
                SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully loaded the current DRS settings into memory.");


                // Get ALL available settings
                UInt32[] drsSettingIds = new UInt32[0];
                try
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to enumerate all the available settings available in this NVIDIA Driver.");
                    drsSettingIds = NVAPI.EnumAvailableSettingIds();
                    SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully enumerated all the available settings available in this NVIDIA Driver. There are {drsSettingIds.Length} settings available");
                    foreach (var drsSettingId in drsSettingIds)
                    {
                        // Get the name of the DRS setting
                        string drsSettingName;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to get the name of this DRS setting from the NVIDIA Driver.");
                            drsSettingName = NVAPI.GetSettingNameFromId(drsSettingId);
                            SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully got the name of this DRS setting this NVIDIA Driver. THe name is '{drsSettingName}'.");
                            stringToReturn += $"DRS Setting: {drsSettingName}:\n";
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"NVIDIALibrary/DumpAllDRSSettings: Exception getting the name of this DRS setting (ID#{drsSettingId}).");
                            stringToReturn += $"DRS Setting: UNKNOWN:\n";
                        }

                        // Now get the available options for this DRS setting
                        stringToReturn += $"OPTIONS:\n";
                        DRSSettingValues drsSettingValues = new DRSSettingValues();
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to enumerate all the options a user could select for this DRS setting from the NVIDIA Driver.");
                            drsSettingValues = NVAPI.EnumAvailableSettingValues(drsSettingId);
                            SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully enumerated all the options a user could select for this DRS setting from the NVIDIA Driver.");
                            stringToReturn += $"    Default Value: {drsSettingValues.DefaultValueAsUnicodeString()}\n";
                            stringToReturn += $"    All Values: {String.Join(", ", drsSettingValues.Values)}\n";                           
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"NVIDIALibrary/DumpAllDRSSettings: Exception getting the name of this DRS setting (ID#{drsSettingId}).");
                            stringToReturn += $"DRS Setting: UNKNOWN:\n";
                        }
                    }

                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"NVIDIALibrary/DumpAllDRSSettings: Exception getting Display ID from video card. Substituting with a # instead");
                }
            }
            finally
            {
                // Destroy the DRS Session Handle to clean up
                SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to destroy the DRS Session handle.");
                NVAPI.DestroySession(drsSessionHandle);
                SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully destroyed our DRS Session Handle");
            }

            return stringToReturn;
        }

        public static GridTopologyV2[] CreateSingleScreenMosaicTopology()
        {

            // Get Current Mosaic Grid settings using the Grid topologies fnumbers we got before
            GridTopologyV2[] mosaicGridTopos = new GridTopologyV2[0];
            try
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/CreateSingleScreenMosaicTopology: Attempting to get the current mosaic grid settings from the NVIDIA Driver.");
                mosaicGridTopos = NVAPI.EnumDisplayGrids();
                SharedLogger.logger.Trace($"NVIDIALibrary/CreateSingleScreenMosaicTopology: Successfully got the current mosaic grid settings from the NVIDIA Driver.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"NVIDIALibrary/CreateSingleScreenMosaicTopology: Exception getting the current mosaic grid settings from the NVIDIA Driver.");
            }

            // Sum up all the screens we have
            //int totalScreenCount = mosaicGridTopos.Select(tp => tp.Displays).Sum(d => d.Count());
            List<GridTopologyV2> screensToReturn = new List<GridTopologyV2>();

            foreach (GridTopologyV2 gridTopo in mosaicGridTopos)
            {
                // Get Current Mosaic Display Topology settings using the Grid topologies numbers we got before
                //NV_MOSAIC_TOPO myGridTopo = gridTopo;
                DisplaySettingsV2[] mosaicDisplaySettings = new DisplaySettingsV2[0];
                try
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/CreateSingleScreenMosaicTopology: Attempting to get the current mosaic display modes for the current mosaic grid topology from the NVIDIA Driver.");
                    mosaicDisplaySettings = NVAPI.EnumDisplayModes(gridTopo);
                    SharedLogger.logger.Trace($"NVIDIALibrary/CreateSingleScreenMosaicTopology: Successfully got the current mosaic display modes for the current mosaic grid topology from the NVIDIA Driver.");
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"NVIDIALibrary/CreateSingleScreenMosaicTopology: Exception getting the current mosaic display modes for the current mosaic grid topology from the NVIDIA Driver.");
                }

                for (int displayIndexToUse = 0; displayIndexToUse < gridTopo.Displays.Count(); displayIndexToUse++)
                {
                    GridTopologyDisplayV2[] displayArray = new GridTopologyDisplayV2[1];
                    displayArray[0] = gridTopo.Displays[displayIndexToUse];

                    SharedLogger.logger.Trace($"NVIDIALibrary/CreateSingleScreenMosaicTopology: Creating new Grid Topology with multiple 1x1 grids based on each display in the current Moasiac grid. This will separate each display on its own.");
                    GridTopologyV2 thisScreen = new GridTopologyV2(1,1, displayArray, gridTopo.DisplaySettings,false,false,false,false,false,false);

                    screensToReturn.Add(thisScreen);
                }

            }

            return screensToReturn.ToArray();
        }

        public static bool ListOfArraysEqual(List<Rectangle[]> a1, List<Rectangle[]> a2)
        {
            if (a1.Count == a2.Count)
            {
                for (int i = 0; i < a1.Count; i++)
                {
                    if (a1[i].Length == a2[i].Length)
                    {
                        for (int j = 0; j < a1[i].Length; j++)
                        {
                            if (!a1[i][j].Equals(a2[i][j]))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool ListOfArraysEqual(List<ViewPortF[]> a1, List<ViewPortF[]> a2)
        {
            if (a1.Count == a2.Count)
            {
                for (int i = 0; i < a1.Count; i++)
                {
                    if (a1[i].Length == a2[i].Length)
                    {
                        for (int j = 0; j < a1[i].Length; j++)
                        {
                            if (!a1[i][j].Equals(a2[i][j]))
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }


        public static bool Arrays2DEqual(int[][] a1, int[][] a2)
        {
            if (a1.Length == a2.Length)
            {
                for (int i = 0; i < a1.Length; i++)
                {
                    if (a1[i].Length == a2[i].Length)
                    {
                        for (int j = 0; j < a1[i].Length; j++)
                        {
                            if (a1[i][j] != a2[i][j])
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool EqualButDifferentOrder<T>(IList<T> list1, IList<T> list2)
        {

            if (list1.Count != list2.Count)
            {
                return false;
            }

            // Now we need to go through the list1, checking that all it's items are in list2
            foreach (T item1 in list1)
            {
                bool foundIt = false;
                foreach (T item2 in list2)
                {
                    if (item1.Equals(item2))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    return false;
                }
            }

            // Now we need to go through the list2, checking that all it's items are in list1
            foreach (T item2 in list2)
            {
                bool foundIt = false;
                foreach (T item1 in list1)
                {
                    if (item1.Equals(item2))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    return false;
                }
            }

            return true;
        }


        public static bool EqualButDifferentOrder<TKey, TValue>(IDictionary<TKey, TValue> dict1, IDictionary<TKey, TValue> dict2)
        {

            if (dict1.Count != dict2.Count)
            {
                return false;
            }

            // Now we need to go through the dict1, checking that all it's items are in dict2
            foreach (KeyValuePair<TKey, TValue> item1 in dict1)
            {
                bool foundIt = false;
                foreach (KeyValuePair<TKey, TValue> item2 in dict2)
                {
                    if (item1.Key.Equals(item2.Key) && item1.Value.Equals(item2.Value))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    return false;
                }
            }

            // Now we need to go through the dict2, checking that all it's items are in dict1
            foreach (KeyValuePair<TKey, TValue> item2 in dict2)
            {
                bool foundIt = false;
                foreach (KeyValuePair<TKey, TValue> item1 in dict1)
                {
                    if (item1.Key.Equals(item2.Key) && item1.Value.Equals(item2.Value))
                    {
                        foundIt = true;
                        break;
                    }
                }
                if (!foundIt)
                {
                    return false;
                }
            }

            return true;
        }
    }





    [global::System.Serializable]
    public class NVIDIALibraryException : Exception
    {
        public NVIDIALibraryException() { }
        public NVIDIALibraryException(string message) : base(message) { }
        public NVIDIALibraryException(string message, Exception inner) : base(message, inner) { }
        protected NVIDIALibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}