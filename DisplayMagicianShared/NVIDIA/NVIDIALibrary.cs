using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using DisplayMagicianShared;
using System.ComponentModel;
using DisplayMagicianShared.Windows;
using EDIDParser;
using System.Threading.Tasks;

namespace DisplayMagicianShared.NVIDIA
{

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_MOSAIC_CONFIG : IEquatable<NVIDIA_MOSAIC_CONFIG>
    {
        public bool IsMosaicEnabled;
        public NV_MOSAIC_TOPO_BRIEF MosaicTopologyBrief;
        public NV_MOSAIC_DISPLAY_SETTING_V2 MosaicDisplaySettings;
        public Int32 OverlapX;
        public Int32 OverlapY;
        public NV_MOSAIC_GRID_TOPO_V2[] MosaicGridTopos;
        public UInt32 MosaicGridCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)NVImport.NV_MOSAIC_MAX_DISPLAYS)]
        public List<NV_RECT[]> MosaicViewports;
        public UInt32 PrimaryDisplayId;

        public override bool Equals(object obj) => obj is NVIDIA_MOSAIC_CONFIG other && this.Equals(other);

        public bool Equals(NVIDIA_MOSAIC_CONFIG other)
        => IsMosaicEnabled == other.IsMosaicEnabled &&
           MosaicTopologyBrief.Equals(other.MosaicTopologyBrief) &&
           MosaicDisplaySettings.Equals(other.MosaicDisplaySettings) &&
           OverlapX == other.OverlapX &&
           OverlapY == other.OverlapY &&
           MosaicGridTopos.SequenceEqual(other.MosaicGridTopos) &&
           MosaicGridCount == other.MosaicGridCount &&
           NVIDIALibrary.ListOfArraysEqual(MosaicViewports, other.MosaicViewports) &&
           PrimaryDisplayId == other.PrimaryDisplayId;

        public override int GetHashCode()
        {
            return (IsMosaicEnabled, MosaicTopologyBrief, MosaicDisplaySettings, OverlapX, OverlapY, MosaicGridTopos, MosaicGridCount, MosaicViewports, PrimaryDisplayId).GetHashCode();
        }
        public static bool operator ==(NVIDIA_MOSAIC_CONFIG lhs, NVIDIA_MOSAIC_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_MOSAIC_CONFIG lhs, NVIDIA_MOSAIC_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_PER_DISPLAY_CONFIG : IEquatable<NVIDIA_PER_DISPLAY_CONFIG>
    {
        public bool HasNvHdrEnabled;
        public NV_HDR_CAPABILITIES_V2 HdrCapabilities;
        public NV_HDR_COLOR_DATA_V2 HdrColorData;
        public bool HasAdaptiveSync;
        public NV_SET_ADAPTIVE_SYNC_DATA_V1 AdaptiveSyncConfig;
        public bool HasColorData;
        public NV_COLOR_DATA_V5 ColorData;
        public bool HasCustomDisplay;
        public List<NV_CUSTOM_DISPLAY_V1> CustomDisplays;


        public override bool Equals(object obj) => obj is NVIDIA_PER_DISPLAY_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_PER_DISPLAY_CONFIG other)
        => HasNvHdrEnabled == other.HasNvHdrEnabled &&
            HdrCapabilities.Equals(other.HdrCapabilities) &&
            HdrColorData.Equals(other.HdrColorData) &&
            HasAdaptiveSync == other.HasAdaptiveSync &&
            AdaptiveSyncConfig.Equals(other.AdaptiveSyncConfig) &&
            HasColorData == other.HasColorData &&
            ColorData.Equals(other.ColorData) &&
            HasCustomDisplay == other.HasCustomDisplay &&
            CustomDisplays.SequenceEqual(other.CustomDisplays);

        public override int GetHashCode()
        {
            return (HasNvHdrEnabled, HdrCapabilities, HdrColorData, HasAdaptiveSync, AdaptiveSyncConfig, HasColorData, ColorData, HasCustomDisplay, CustomDisplays).GetHashCode();
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
    public struct NVIDIA_PER_ADAPTER_CONFIG : IEquatable<NVIDIA_PER_ADAPTER_CONFIG>
    {
        public bool IsQuadro;
        public bool HasLogicalGPU;
        public NV_LOGICAL_GPU_DATA_V1 LogicalGPU;
        public UInt32 DisplayCount;
        public Dictionary<UInt32, NVIDIA_PER_DISPLAY_CONFIG> Displays;

        public override bool Equals(object obj) => obj is NVIDIA_PER_ADAPTER_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_PER_ADAPTER_CONFIG other)
        => IsQuadro == other.IsQuadro &&
            HasLogicalGPU == other.HasLogicalGPU &&
            LogicalGPU.Equals(other.LogicalGPU) &&
            DisplayCount == other.DisplayCount &&
            Displays.SequenceEqual(other.Displays);

        public override int GetHashCode()
        {
            return (IsQuadro, HasLogicalGPU, LogicalGPU, DisplayCount, Displays).GetHashCode();
        }
        public static bool operator ==(NVIDIA_PER_ADAPTER_CONFIG lhs, NVIDIA_PER_ADAPTER_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_PER_ADAPTER_CONFIG lhs, NVIDIA_PER_ADAPTER_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_DISPLAY_CONFIG : IEquatable<NVIDIA_DISPLAY_CONFIG>
    {
        public bool IsCloned;
        public NVIDIA_MOSAIC_CONFIG MosaicConfig;
        public Dictionary<UInt32, NVIDIA_PER_ADAPTER_CONFIG> PhysicalAdapters;
        public List<NV_DISPLAYCONFIG_PATH_INFO_V2> DisplayConfigs;
        // Note: We purposely have left out the DisplayNames from the Equals as it's order keeps changing after each reboot and after each profile swap
        // and it is informational only and doesn't contribute to the configuration (it's used for generating the Screens structure, and therefore for
        // generating the profile icon.
        public Dictionary<string, string> DisplayNames;
        public List<string> DisplayIdentifiers;

        public override bool Equals(object obj) => obj is NVIDIA_DISPLAY_CONFIG other && this.Equals(other);

        public bool Equals(NVIDIA_DISPLAY_CONFIG other)
           => IsCloned == other.IsCloned &&
            PhysicalAdapters.SequenceEqual(other.PhysicalAdapters) &&
            MosaicConfig.Equals(other.MosaicConfig) &&
            DisplayConfigs.SequenceEqual(other.DisplayConfigs) &&
            DisplayIdentifiers.SequenceEqual(other.DisplayIdentifiers);


        public override int GetHashCode()
        {
            return (IsCloned, MosaicConfig, PhysicalAdapters, DisplayConfigs, DisplayIdentifiers, DisplayNames).GetHashCode();
        }
        public static bool operator ==(NVIDIA_DISPLAY_CONFIG lhs, NVIDIA_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_DISPLAY_CONFIG lhs, NVIDIA_DISPLAY_CONFIG rhs) => !(lhs == rhs);
    }

    public class NVIDIALibrary : IDisposable
    {

        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static NVIDIALibrary _instance = new NVIDIALibrary();

        private bool _initialised = false;
        private NVIDIA_DISPLAY_CONFIG _activeDisplayConfig;
        public List<NV_MONITOR_CONN_TYPE> SkippedColorConnectionTypes;

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

        static NVIDIALibrary() { }
        public NVIDIALibrary()
        {
            // Populate the list of ConnectionTypes we want to skip as they don't support querying
            SkippedColorConnectionTypes = new List<NV_MONITOR_CONN_TYPE> {
                NV_MONITOR_CONN_TYPE.VGA,
                NV_MONITOR_CONN_TYPE.COMPONENT,
                NV_MONITOR_CONN_TYPE.SVIDEO,
                NV_MONITOR_CONN_TYPE.DVI,
                NV_MONITOR_CONN_TYPE.COMPOSITE,
            };

            _activeDisplayConfig = CreateDefaultConfig();
            try
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Attempting to load the NVIDIA NVAPI DLL");
                // Attempt to prelink all of the NVAPI functions
                //Marshal.PrelinkAll(typeof(NVImport));

                // If we get here then we definitely have the NVIDIA driver available.
                NVAPI_STATUS NVStatus = NVAPI_STATUS.NVAPI_ERROR;
                SharedLogger.logger.Trace("NVIDIALibrary/NVIDIALibrary: Intialising NVIDIA NVAPI library interface");
                // Step 1: Initialise the NVAPI
                try
                {
                    if (NVImport.IsAvailable())
                    {
                        _initialised = true;
                        SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: NVIDIA NVAPI library was initialised successfully");
                        SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Running UpdateActiveConfig to ensure there is a config to use later");
                        _activeDisplayConfig = GetActiveConfig();
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Error intialising NVIDIA NVAPI library. NvAPI_Initialize() returned error code {NVStatus}");
                    }

                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Trace(ex, $"NVIDIALibrary/NVIDIALibrary: Exception intialising NVIDIA NVAPI library. NvAPI_Initialize() caused an exception.");
                }

            }
            catch (DllNotFoundException ex)
            {
                // If this fires, then the DLL isn't available, so we need don't try to do anything else
                SharedLogger.logger.Info(ex, $"NVIDIALibrary/NVIDIALibrary: Exception trying to load the NVIDIA NVAPI DLL. This generally means you don't have the NVIDIA driver installed.");
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
                return _activeDisplayConfig;
            }
        }

        public List<string> CurrentDisplayIdentifiers
        {
            get
            {
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
            myDefaultConfig.MosaicConfig.MosaicGridTopos = new NV_MOSAIC_GRID_TOPO_V2[0];
            myDefaultConfig.MosaicConfig.MosaicViewports = new List<NV_RECT[]>();
            myDefaultConfig.PhysicalAdapters = new Dictionary<UInt32, NVIDIA_PER_ADAPTER_CONFIG>();
            myDefaultConfig.DisplayConfigs = new List<NV_DISPLAYCONFIG_PATH_INFO_V2>();
            myDefaultConfig.DisplayNames = new Dictionary<string, string>();
            myDefaultConfig.DisplayIdentifiers = new List<string>();
            myDefaultConfig.IsCloned = false;

            return myDefaultConfig;
        }

        public bool UpdateActiveConfig()
        {
            SharedLogger.logger.Trace($"NVIDIALibrary/UpdateActiveConfig: Updating the currently active config");
            try
            {
                _activeDisplayConfig = GetActiveConfig();
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

                // Enumerate all the Physical GPUs
                PhysicalGpuHandle[] physicalGpus = new PhysicalGpuHandle[NVImport.NVAPI_MAX_PHYSICAL_GPUS];
                uint physicalGpuCount = 0;
                NVAPI_STATUS NVStatus = NVImport.NvAPI_EnumPhysicalGPUs(ref physicalGpus, out physicalGpuCount);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_EnumPhysicalGPUs returned {physicalGpuCount} Physical GPUs");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting physical GPU count. NvAPI_EnumPhysicalGPUs() returned error code {NVStatus}");
                }

                // Go through the Physical GPUs one by one
                for (uint physicalGpuIndex = 0; physicalGpuIndex < physicalGpuCount; physicalGpuIndex++)
                {
                    // Prepare the physicalGPU per adapter structure to use later
                    NVIDIA_PER_ADAPTER_CONFIG myAdapter = new NVIDIA_PER_ADAPTER_CONFIG();
                    myAdapter.LogicalGPU.PhysicalGPUHandles = new PhysicalGpuHandle[0];
                    myAdapter.IsQuadro = false;
                    myAdapter.HasLogicalGPU = false;
                    myAdapter.Displays = new Dictionary<uint, NVIDIA_PER_DISPLAY_CONFIG>();

                    //This function retrieves the Quadro status for the GPU (1 if Quadro, 0 if GeForce)
                    uint quadroStatus = 0;
                    NVStatus = NVImport.NvAPI_GPU_GetQuadroStatus(physicalGpus[physicalGpuIndex], out quadroStatus);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        if (quadroStatus == 0)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Video Card is one from the GeForce range");
                        }
                        else if (quadroStatus == 1)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Video Card is one from the Quadro range");
                            myAdapter.IsQuadro = true;
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Video Card is neither a GeForce or Quadro range vodeo card (QuadroStatus = {quadroStatus})");
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting Quadro status. NvAPI_GPU_GetQuadroStatus() returned error code {NVStatus}");
                    }

                    // Firstly let's get the logical GPU from the Physical handle
                    LogicalGpuHandle logicalGPUHandle;
                    NVStatus = NVImport.NvAPI_GetLogicalGPUFromPhysicalGPU(physicalGpus[physicalGpuIndex], out logicalGPUHandle);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Logical GPU Handle from physical GPU.");
                        NV_LOGICAL_GPU_DATA_V1 logicalGPUData = new NV_LOGICAL_GPU_DATA_V1();
                        NVStatus = NVImport.NvAPI_GPU_GetLogicalGpuInfo(logicalGPUHandle, ref logicalGPUData);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the Logical GPU information from the NVIDIA driver!");
                            myAdapter.HasLogicalGPU = true;
                            myAdapter.LogicalGPU = logicalGPUData;
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_POINTER)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: No Logical GPU found so no logicalGPUData available. NvAPI_GPU_GetLogicalGpuInfo() returned error code {NVStatus}");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting Logical GPU information from NVIDIA driver. NvAPI_GPU_GetLogicalGpuInfo() returned error code {NVStatus}");
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting Logical GPU handle from Physical GPU. NvAPI_GetLogicalGPUFromPhysicalGPU() returned error code {NVStatus}");
                    }

                    myDisplayConfig.PhysicalAdapters[physicalGpuIndex] = myAdapter;
                }

                // Get current Supported Mosaic Topology info (check whether Mosaic is on)
                NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 mosaicSupportedTopoInfo = new NV_MOSAIC_SUPPORTED_TOPO_INFO_V2();
                NVStatus = NVImport.NvAPI_Mosaic_GetSupportedTopoInfo(ref mosaicSupportedTopoInfo, NV_MOSAIC_TOPO_TYPE.ALL);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetSupportedTopoInfo returned OK.");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_GetSupportedTopoInfo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_GetSupportedTopoInfo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_GetSupportedTopoInfo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_GetSupportedTopoInfo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The version of the structure passed in is not supported by this driver. NvAPI_Mosaic_GetSupportedTopoInfo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_GetSupportedTopoInfo() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_GetSupportedTopoInfo() returned error code {NVStatus}");
                }

                if (mosaicSupportedTopoInfo != null && mosaicSupportedTopoInfo.TopoBriefsCount > 0)
                {
                    int numValidTopos = mosaicSupportedTopoInfo.TopoBriefs.Count(tb => tb.IsPossible == 1);
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: There are {numValidTopos} valid Mosaic Topologies available with this display layout.");
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: There are no valid Mosaic Topologies available with this display layout.");
                }


                // Get current Mosaic Topology settings in brief (check whether Mosaic is on)
                NV_MOSAIC_TOPO_BRIEF mosaicTopoBrief = new NV_MOSAIC_TOPO_BRIEF();
                NV_MOSAIC_DISPLAY_SETTING_V2 mosaicDisplaySetting = new NV_MOSAIC_DISPLAY_SETTING_V2();
                int mosaicOverlapX = 0;
                int mosaicOverlapY = 0;
                NVStatus = NVImport.NvAPI_Mosaic_GetCurrentTopo(ref mosaicTopoBrief, ref mosaicDisplaySetting, out mosaicOverlapX, out mosaicOverlapY);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }

                // Get more Mosaic Topology detailed settings
                NV_MOSAIC_TOPO_GROUP mosaicTopoGroup = new NV_MOSAIC_TOPO_GROUP();
                NVStatus = NVImport.NvAPI_Mosaic_GetTopoGroup(ref mosaicTopoBrief, ref mosaicTopoGroup);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetTopoGroup returned OK.");
                    if (mosaicTopoBrief.IsPossible == 1)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: The current Mosaic Topology of {mosaicTopoBrief.Topo} is possible to use");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: The current Mosaic Topology of {mosaicTopoBrief.Topo} is NOT possible to use");
                    }
                    if (mosaicTopoGroup.Count > 0)
                    {
                        int m = 1;
                        foreach (var mosaicTopoDetail in mosaicTopoGroup.Topos)
                        {

                            if (mosaicTopoDetail.TopologyValid)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: The returned Mosaic Topology Group #{m} is VALID.");
                            }
                            else
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: The returned Mosaic Topology Group #{m} is NOT VALID and cannot be used.");
                            }
                            if (mosaicTopoDetail.TopologyMissingGPU)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: The returned Mosaic Topology Group #{m} is MISSING THE GPU it was created with.");
                            }
                            if (mosaicTopoDetail.TopologyMissingDisplay)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: The returned Mosaic Topology Group #{m} is MISSING ONE OR MORE DISPLAYS it was created with.");
                            }
                            if (mosaicTopoDetail.TopologyMixedDisplayTypes)
                            {
                                SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: The returned Mosaic Topology Group #{m} is USING MIXED DISPLAY TYPES and NVIDIA don't support that at present.");
                            }
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The returned Mosaic Topology Group doesn't have any returned Topo Groups. We expect there should be at least one if the Mosaic display layout is configured correctly.");
                    }
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_GetTopoGroup() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more arguments passed in are invalid. NvAPI_Mosaic_GetTopoGroup() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_GetTopoGroup() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_GetTopoGroup() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The version of the structure passed in is not supported by this driver. NvAPI_Mosaic_GetTopoGroup() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_GetTopoGroup() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_GetTopoGroup() returned error code {NVStatus}");
                }

                // Check if there is a topology and that Mosaic is enabled
                if (mosaicTopoBrief.Topo != NV_MOSAIC_TOPO.TOPO_NONE && mosaicTopoBrief.Enabled == 1)
                {
                    // Mosaic is enabled!
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Mosaic is enabled.");
                    myDisplayConfig.MosaicConfig.MosaicTopologyBrief = mosaicTopoBrief;
                    myDisplayConfig.MosaicConfig.MosaicDisplaySettings = mosaicDisplaySetting;
                    myDisplayConfig.MosaicConfig.OverlapX = mosaicOverlapX;
                    myDisplayConfig.MosaicConfig.OverlapY = mosaicOverlapY;
                    myDisplayConfig.MosaicConfig.IsMosaicEnabled = true;


                    // Figure out how many Mosaic Grid topoligies there are                    
                    uint mosaicGridCount = 0;
                    NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayGrids(ref mosaicGridCount);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");
                    }

                    // Get Current Mosaic Grid settings using the Grid topologies fnumbers we got before
                    NV_MOSAIC_GRID_TOPO_V2[] mosaicGridTopos = new NV_MOSAIC_GRID_TOPO_V2[mosaicGridCount];
                    NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayGrids(ref mosaicGridTopos, ref mosaicGridCount);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_EnumDisplayGrids returned OK.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
                    }

                    myDisplayConfig.MosaicConfig.MosaicGridTopos = mosaicGridTopos;
                    myDisplayConfig.MosaicConfig.MosaicGridCount = mosaicGridCount;

                    List<NV_RECT[]> allViewports = new List<NV_RECT[]>();
                    foreach (NV_MOSAIC_GRID_TOPO_V2 gridTopo in mosaicGridTopos)
                    {
                        // Get Current Mosaic Grid settings using the Grid topologies numbers we got before
                        NV_RECT[] viewports = new NV_RECT[NVImport.NV_MOSAIC_MAX_DISPLAYS];
                        byte bezelCorrected = 0;
                        NVStatus = NVImport.NvAPI_Mosaic_GetDisplayViewportsByResolution(gridTopo.Displays[0].DisplayId, 0, 0, ref viewports, ref bezelCorrected);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetDisplayViewportsByResolution returned OK.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_GetDisplayViewportsByResolution() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_GetDisplayViewportsByResolution() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_GetDisplayViewportsByResolution() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_MOSAIC_NOT_ACTIVE)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The requested action cannot be performed without Mosaic being enabled. NvAPI_Mosaic_GetDisplayViewportsByResolution() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_GetDisplayViewportsByResolution() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_GetDisplayViewportsByResolution() returned error code {NVStatus}");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_GetDisplayViewportsByResolution() returned error code {NVStatus}");
                        }
                        // Save the viewports to the List
                        allViewports.Add(viewports);

                        // Figure out how many Mosaic Display topologies there are                    
                        UInt32 mosaicDisplayModesCount = 0;
                        NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayModes(gridTopo, ref mosaicDisplayModesCount);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_EnumDisplayModes returned OK.");
                        }

                        // Get Current Mosaic Display Topology settings using the Grid topologies numbers we got before
                        //NV_MOSAIC_TOPO myGridTopo = gridTopo;
                        NV_MOSAIC_DISPLAY_SETTING_V2[] mosaicDisplaySettings = new NV_MOSAIC_DISPLAY_SETTING_V2[mosaicDisplayModesCount];
                        NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayModes(gridTopo, ref mosaicDisplaySettings, ref mosaicDisplayModesCount);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_EnumDisplayModes returned OK.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology Display Settings! NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                        }

                    }

                    myDisplayConfig.MosaicConfig.MosaicViewports = allViewports;
                }
                else
                {
                    // Mosaic isn't enabled
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Mosaic is NOT enabled.");
                    myDisplayConfig.MosaicConfig.MosaicTopologyBrief = mosaicTopoBrief;
                    myDisplayConfig.MosaicConfig.IsMosaicEnabled = false;
                    myDisplayConfig.MosaicConfig.MosaicGridTopos = new NV_MOSAIC_GRID_TOPO_V2[] { };
                    myDisplayConfig.MosaicConfig.MosaicViewports = new List<NV_RECT[]>();
                }

                // Check if Mosaic is possible and log that so we know if troubleshooting bugs
                if (mosaicTopoBrief.IsPossible == 1)
                {
                    // Mosaic is possible!
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Mosaic is possible. Mosaic topology would be {mosaicTopoBrief.Topo.ToString("G")}.");
                }
                else
                {
                    // Mosaic isn't possible
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Mosaic is NOT possible.");
                }

                // Now we try to get the NVIDIA Windows Display Config. This is needed for handling some of the advanced scaling settings that some advanced users make use of
                ///////////////////////////////////////////////////////////////////////////////
                // FUNCTION NAME:   NvAPI_DISP_GetDisplayConfig
                //
                //! DESCRIPTION:     This API lets caller retrieve the current global display
                //!                  configuration.
                //!       USAGE:     The caller might have to call this three times to fetch all the required configuration details as follows:
                //!                  First  Pass: Caller should Call NvAPI_DISP_GetDisplayConfig() with pathInfo set to NULL to fetch pathInfoCount.
                //!                  Second Pass: Allocate memory for pathInfo with respect to the number of pathInfoCount(from First Pass) to fetch
                //!                               targetInfoCount. If sourceModeInfo is needed allocate memory or it can be initialized to NULL.
                //!             Third  Pass(Optional, only required if target information is required): Allocate memory for targetInfo with respect
                //!                               to number of targetInfoCount(from Second Pass).
                //! SUPPORTED OS:  Windows 7 and higher
                // First pass: Figure out how many pathInfo objects there are
                uint pathInfoCount = 0;
                NVStatus = NVImport.NvAPI_DISP_GetDisplayConfig(ref pathInfoCount);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK && pathInfoCount > 0)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_GetDisplayConfig returned OK on first pass. We know we have {pathInfoCount} pathInfo objects to get");
                    // Second pass: Now get the number of targetInfoCount for each returned pathInfoCount object
                    NV_DISPLAYCONFIG_PATH_INFO_V2[] pathInfos = new NV_DISPLAYCONFIG_PATH_INFO_V2[pathInfoCount];
                    NVStatus = NVImport.NvAPI_DISP_GetDisplayConfig(ref pathInfoCount, ref pathInfos);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_GetDisplayConfig returned OK on second pass.");
                        // Third pass: Now we send the same partially filled object back in a third time to get the target information
                        NVStatus = NVImport.NvAPI_DISP_GetDisplayConfig(ref pathInfoCount, ref pathInfos, true);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_GetDisplayConfig returned OK on third and final pass.");
                            // If this worked, we need to check for and handle cloned displays if there are any
                            // They need to be set in a special way (see DisplayConfiguration.cpp from the DisplayConfiguration sample from NVIDIA)
                            for (int x = 0; x < pathInfoCount; x++)
                            {
                                if (pathInfos[x].TargetInfoCount > 1)
                                {
                                    // This is a cloned display, we need to mark this NVIDIA display profile as cloned so we correct the profile later
                                    myDisplayConfig.IsCloned = true;
                                }
                            }

                            myDisplayConfig.DisplayConfigs = pathInfos.ToList();
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_GetDisplayConfig returned OK on third pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on third pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on third pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on third pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on third pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_DEVICE_BUSY)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: ModeSet has not yet completed. Please wait and call it again. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on third pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on third pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on third pass.");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting NVIDIA Display Config! NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on third pass.");
                        }
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on second pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on second pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on second pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on second pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_DEVICE_BUSY)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: ModeSet has not yet completed. Please wait and call it again. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on second pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on second pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on second pass.");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting NVIDIA Display Config! NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on second pass.");
                    }

                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_OK && pathInfoCount == 0)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The call was successful but no display paths were found. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more arguments passed in are invalid. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_DEVICE_BUSY)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: ModeSet has not yet completed. Please wait and call it again. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                {
                    SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass.");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting NVIDIA Display Config! NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass");
                }

                // We want to get the primary monitor
                NVStatus = NVImport.NvAPI_DISP_GetGDIPrimaryDisplayId(out UInt32 primaryDisplayId);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_GetGDIPrimaryDisplayId returned OK.");
                    myDisplayConfig.MosaicConfig.PrimaryDisplayId = primaryDisplayId;
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NVIDIA_DEVICE_NOT_FOUND)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: There are no NVIDIA video cards in this computer. NvAPI_DISP_GetGDIPrimaryDisplayId() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_DISP_GetGDIPrimaryDisplayId() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_GetGDIPrimaryDisplayId() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_GetGDIPrimaryDisplayId() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_DISP_GetGDIPrimaryDisplayId() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting primary display id! NvAPI_DISP_GetGDIPrimaryDisplayId() returned error code {NVStatus}");
                }

                // We want to get the number of displays we have
                // Go through the Physical GPUs one by one
                for (uint physicalGpuIndex = 0; physicalGpuIndex < physicalGpuCount; physicalGpuIndex++)
                {

                    // Get a new variable to the PhysicalAdapters to make easier to use
                    // NOTE: This struct was filled in earlier by code further up
                    NVIDIA_PER_ADAPTER_CONFIG myAdapter = myDisplayConfig.PhysicalAdapters[physicalGpuIndex];
                    myAdapter.Displays = new Dictionary<uint, NVIDIA_PER_DISPLAY_CONFIG>();

                    //This function retrieves the number of display IDs we know about
                    UInt32 displayCount = 0;
                    NVStatus = NVImport.NvAPI_GPU_GetConnectedDisplayIds(physicalGpus[physicalGpuIndex], ref displayCount, 0);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_GPU_GetConnectedDisplayIds returned OK on first pass. We have {displayCount} physical GPUs");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input buffer is not large enough to hold it's contents. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on first pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on first pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on first pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on first pass.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on first pass.");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting connected display ids! NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on first pass.");
                    }

                    if (displayCount > 0)
                    {
                        // Now we try to get the information about the displayIDs
                        NV_GPU_DISPLAYIDS_V2[] displayIds = new NV_GPU_DISPLAYIDS_V2[displayCount];
                        NVStatus = NVImport.NvAPI_GPU_GetConnectedDisplayIds(physicalGpus[physicalGpuIndex], ref displayIds, ref displayCount, 0);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_GPU_GetConnectedDisplayIds returned OK on second pass. We have {displayCount} physical displays");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input buffer is not large enough to hold it's contents. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on second pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on second pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on second pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on second pass.");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus} on second pass.");
                        }
                        else
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting connected display ids! NvAPI_DISP_GetGDIPrimaryDisplayId() returned error code {NVStatus} on second pass.");
                        }


                        // Time to get the color settings, HDR capabilities and settings for each display
                        bool isNvHdrEnabled = false;
                        for (int displayIndex = 0; displayIndex < displayCount; displayIndex++)
                        {
                            if (allDisplays)
                            {
                                // We want all physicallyconnected or connected displays
                                if (!(displayIds[displayIndex].isConnected || displayIds[displayIndex].isPhysicallyConnected))
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
                            myDisplay.ColorData = new NV_COLOR_DATA_V5();
                            myDisplay.HdrColorData = new NV_HDR_COLOR_DATA_V2();
                            myDisplay.HdrCapabilities = new NV_HDR_CAPABILITIES_V2();
                            myDisplay.AdaptiveSyncConfig = new NV_SET_ADAPTIVE_SYNC_DATA_V1();
                            myDisplay.CustomDisplays = new List<NV_CUSTOM_DISPLAY_V1>();
                            myDisplay.HasNvHdrEnabled = false;
                            myDisplay.HasAdaptiveSync = false;
                            myDisplay.HasCustomDisplay = false;

                            // We need to skip recording anything that doesn't support color communication
                            if (!SkippedColorConnectionTypes.Contains(displayIds[displayIndex].ConnectorType))
                            {
                                // skip this monitor connection type as it won't provide the data in the section, and just creates errors                                
                                // We get the Color Capabilities of the display
                                NV_COLOR_DATA_V5 colorData = new NV_COLOR_DATA_V5();
                                // Set the command as a 'GET'
                                colorData.Cmd = NV_COLOR_CMD.NV_COLOR_CMD_GET;
                                NVStatus = NVImport.NvAPI_Disp_ColorControl(displayIds[displayIndex].DisplayId, ref colorData);
                                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Your monitor {displayIds[displayIndex].DisplayId} has the following color settings set. BPC = {colorData.Bpc.ToString("G")}. Color Format = {colorData.ColorFormat.ToString("G")}. Colorimetry = {colorData.Colorimetry.ToString("G")}. Color Selection Policy = {colorData.ColorSelectionPolicy.ToString("G")}. Color Depth = {colorData.Depth.ToString("G")}. Dynamic Range = {colorData.DynamicRange.ToString("G")}. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                    myDisplay.ColorData = colorData;
                                    myDisplay.HasColorData = true;
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input buffer is not large enough to hold it's contents. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Disp_ColorControl() returned error code {NVStatus}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting HDR color settings! NvAPI_Disp_ColorControl() returned error code {NVStatus}. It's most likely that your monitor {displayIds[displayIndex].DisplayId} doesn't support HDR.");
                                }

                                // Now we get the HDR capabilities of the display
                                NV_HDR_CAPABILITIES_V2 hdrCapabilities = new NV_HDR_CAPABILITIES_V2();
                                NVStatus = NVImport.NvAPI_Disp_GetHdrCapabilities(displayIds[displayIndex].DisplayId, ref hdrCapabilities);
                                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Disp_GetHdrCapabilities returned OK.");
                                    if (hdrCapabilities.SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsST2084EotfSupported))
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports HDR mode ST2084 EOTF");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support HDR mode ST2084 EOTF");
                                    }
                                    if (hdrCapabilities.SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsDolbyVisionSupported))
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports DolbyVision HDR");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support DolbyVision HDR");
                                    }
                                    if (hdrCapabilities.SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsEdrSupported))
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports EDR");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support EDR");
                                    }
                                    if (hdrCapabilities.SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsTraditionalHdrGammaSupported))
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports Traditional HDR Gama");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support Traditional HDR Gama");
                                    }

                                    if (hdrCapabilities.SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsTraditionalSdrGammaSupported))
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports Traditional SDR Gama");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT supports Traditional SDR Gama");
                                    }
                                    if (hdrCapabilities.SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.DriverExpandDefaultHdrParameters))
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} supports Driver Expanded Default HDR Parameters");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayIds[displayIndex].DisplayId} DOES NOT support Driver Expanded Default HDR Parameters ");
                                    }

                                    myDisplay.HdrCapabilities = hdrCapabilities;
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input buffer is not large enough to hold it's contents. NvAPI_Disp_GetHdrCapabilities() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_Disp_GetHdrCapabilities() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Disp_GetHdrCapabilities() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Disp_GetHdrCapabilities() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Disp_GetHdrCapabilities() returned error code {NVStatus}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting HDR color capabilities from your display! NvAPI_Disp_GetHdrCapabilities() returned error code {NVStatus}. It's most likely that your monitor {displayIds[displayIndex].DisplayId} doesn't support HDR.");
                                }

                                // Now we get the HDR colour settings of the display
                                NV_HDR_COLOR_DATA_V2 hdrColorData = new NV_HDR_COLOR_DATA_V2();
                                hdrColorData.Cmd = NV_HDR_CMD.CMD_GET;
                                NVStatus = NVImport.NvAPI_Disp_HdrColorControl(displayIds[displayIndex].DisplayId, ref hdrColorData);
                                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Disp_HdrColorControl returned OK. HDR mode is set to {hdrColorData.HdrMode.ToString("G")}.");
                                    if (hdrColorData.HdrMode != NV_HDR_MODE.OFF)
                                    {
                                        isNvHdrEnabled = true;
                                        myDisplay.HasNvHdrEnabled = true;
                                    }
                                    myDisplay.HdrColorData = hdrColorData;
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input buffer is not large enough to hold it's contents. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting HDR color settings! NvAPI_Disp_HdrColorControl() returned error code {NVStatus}. It's most likely that your monitor {displayIds[displayIndex].DisplayId} doesn't support HDR.");
                                }

                                // Now we get the Adaptive Sync Settings from the display
                                NV_GET_ADAPTIVE_SYNC_DATA_V1 getAdaptiveSyncData = new NV_GET_ADAPTIVE_SYNC_DATA_V1();
                                getAdaptiveSyncData.Version = NVImport.NV_GET_ADAPTIVE_SYNC_DATA_V1_VER;
                                NVStatus = NVImport.NvAPI_DISP_GetAdaptiveSyncData(displayIds[displayIndex].DisplayId, ref getAdaptiveSyncData);
                                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                {
                                    // Copy the AdaptiveSync Data we got into a NV_SET_ADAPTIVE_SYNC_DATA_V1 object so that it can be used without conversion
                                    NV_SET_ADAPTIVE_SYNC_DATA_V1 setAdaptiveSyncData = new NV_SET_ADAPTIVE_SYNC_DATA_V1();
                                    setAdaptiveSyncData.Version = NVImport.NV_SET_ADAPTIVE_SYNC_DATA_V1_VER;
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
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input buffer is not large enough to hold it's contents. NvAPI_DISP_GetAdaptiveSyncData() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor {displayIds[displayIndex].DisplayId} is either not connected or is not a DP or HDMI panel. NvAPI_DISP_GetAdaptiveSyncData() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_GetAdaptiveSyncData() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_GetAdaptiveSyncData() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_DISP_GetAdaptiveSyncData() returned error code {NVStatus}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting HDR color settings! NvAPI_DISP_GetAdaptiveSyncData() returned error code {NVStatus}. It's most likely that your monitor {displayIds[displayIndex].DisplayId} doesn't support HDR.");
                                }


                                // TEMPORARILY DISABLING THE CUSTOM DISPLAY CODE FOR NOW, AS NOT SURE WHAT NVIDIA SETTINGS IT TRACKS
                                // KEEPING IT IN CASE I NEED IT FOR LATER. I ORIGINALLY THOUGHT THAT IS WHERE INTEGER SCALING SETTINGS LIVED< BUT WAS WRONG
                                /*// Now we get the Custom Display settings of the display (if there are any)
                                //NVIDIA_CUSTOM_DISPLAY_CONFIG customDisplayConfig = new NVIDIA_CUSTOM_DISPLAY_CONFIG();
                                List<NV_CUSTOM_DISPLAY_V1> customDisplayConfig = new List<NV_CUSTOM_DISPLAY_V1>();
                                for (UInt32 d = 0; d < UInt32.MaxValue; d++)
                                {
                                    NV_CUSTOM_DISPLAY_V1 customDisplay = new NV_CUSTOM_DISPLAY_V1();
                                    NVStatus = NVImport.NvAPI_DISP_EnumCustomDisplay(displayIds[displayIndex].DisplayId, d, ref customDisplay);
                                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_EnumCustomDisplay returned OK. Custom Display settings retrieved.");
                                        myDisplay.CustomDisplay = customDisplay;
                                        myDisplay.HasCustomDisplay = true;
                                    }
                                    else if (NVStatus == NVAPI_STATUS.NVAPI_END_ENUMERATION)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: We've reached the end of the list of Custom Displays. Breaking the polling loop.");
                                        break;
                                    }
                                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_DISP_EnumCustomDisplay() returned error code {NVStatus}");
                                        break;
                                    }
                                    else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_EnumCustomDisplay() returned error code {NVStatus}");
                                        break;
                                    }
                                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_EnumCustomDisplay() returned error code {NVStatus}");
                                        break;
                                    }
                                    else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The supplied struct is incompatible. NvAPI_DISP_EnumCustomDisplay() returned error code {NVStatus}");
                                        break;
                                    }
                                    else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_DISP_EnumCustomDisplay() returned error code {NVStatus}.");
                                        break;
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while enumerating the custom displays! NvAPI_DISP_EnumCustomDisplay() returned error code {NVStatus}.");
                                        break;
                                    }

                                }*/

                                myAdapter.Displays.Add(displayIds[displayIndex].DisplayId, myDisplay);
                            }
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
                    NVStatus = NVImport.NvAPI_DISP_GetDisplayIdByDisplayName(displayName, out displayId);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_GetDisplayIdByDisplayName returned OK. The display {displayName} has NVIDIA DisplayID {displayId}");
                        myDisplayConfig.DisplayNames.Add(displayId.ToString(), displayName);
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NVIDIA_DEVICE_NOT_FOUND)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: GDI Primary not on an NVIDIA GPU. NvAPI_DISP_GetDisplayIdByDisplayName() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more args passed in are invalid. NvAPI_DISP_GetDisplayIdByDisplayName() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_GetDisplayIdByDisplayName() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_GetDisplayIdByDisplayName() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_DISP_GetDisplayIdByDisplayName() returned error code {NVStatus}");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_DISP_GetDisplayIdByDisplayName() returned error code {NVStatus}");
                    }
                }

                // Get the display identifiers                
                myDisplayConfig.DisplayIdentifiers = GetCurrentDisplayIdentifiers();

                // Go through and find the list of displayIDs
                // ignore the ones that were found
                // if one was not found, then
                // go through the modes
                // patch the target
                if (myDisplayConfig.IsCloned)
                {
                    List<UInt32> clonedIdsWeKnow = new List<uint>();
                    List<UInt32> missingIdsWeWant = new List<uint>();
                    // Find all displays in the displayconfig
                    foreach (var displayConfig in myDisplayConfig.DisplayConfigs)
                    {
                        foreach (var targetInfo in displayConfig.TargetInfo)
                        {
                            if (foundDisplayIds.Contains(targetInfo.DisplayId))
                            {
                                // We have this foundId
                                clonedIdsWeKnow.Add(targetInfo.DisplayId);
                            }
                        }
                    }

                    // Now go through and figure out which foundDisplayId we're missing
                    foreach (var foundDisplayId in foundDisplayIds)
                    {
                        if (!clonedIdsWeKnow.Contains(foundDisplayId))
                        {
                            // We found a cloned display id \o/
                            missingIdsWeWant.Add(foundDisplayId);
                        }
                    }

                    int clonedIdOffset = 0;
                    // Now we go through the list of missing cloned id's and we fill them in
                    for (int x = 0; x < myDisplayConfig.DisplayConfigs.Count; x++)
                    {
                        // We go through all the displayconfigs, but we want to only change the cloned displays (those with > 1 targetInfo)
                        if (myDisplayConfig.DisplayConfigs[x].TargetInfoCount > 1)
                        {
                            // We only want to change the cloned displays, so we start at index 1 (the clones themselves)
                            for (int y = 1; y < myDisplayConfig.DisplayConfigs[x].TargetInfoCount; y++)
                            {
                                // We want to assign the cloned display the display ID from the missing display
                                myDisplayConfig.DisplayConfigs[x].TargetInfo[y].DisplayId = missingIdsWeWant[clonedIdOffset++];
                                // We also want to clone the Details struct from the base display (the first display) and replicate them on the clone
                                // This copies the process used within the DisplayCOnfiguration C++ Sample released by NVIDIA
                                myDisplayConfig.DisplayConfigs[x].TargetInfo[y].Details = (NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1)myDisplayConfig.DisplayConfigs[0].TargetInfo[0].Details.Clone();
                            }
                        }
                    }
                }

            }
            else
            {
                SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: ERROR - Tried to run GetNVIDIADisplayConfig but the NVIDIA NVAPI library isn't initialised!");
                throw new NVIDIALibraryException($"Tried to run GetNVIDIADisplayConfig but the NVIDIA NVAPI library isn't initialised!");
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
            PhysicalGpuHandle[] physicalGpus = new PhysicalGpuHandle[NVImport.NV_MAX_PHYSICAL_GPUS];
            uint physicalGpuCount = 0;
            NVAPI_STATUS NVStatus = NVImport.NvAPI_EnumPhysicalGPUs(ref physicalGpus, out physicalGpuCount);
            if (NVStatus == NVAPI_STATUS.NVAPI_OK)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_EnumPhysicalGPUs returned {physicalGpuCount} Physical GPUs");
                stringToReturn += $"Number of NVIDIA Video cards found: {physicalGpuCount}\n";
            }
            else
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting physical GPU count. NvAPI_EnumPhysicalGPUs() returned error code {NVStatus}");
            }

            // Go through the Physical GPUs one by one
            for (uint physicalGpuIndex = 0; physicalGpuIndex < physicalGpuCount; physicalGpuIndex++)
            {
                //We want to get the name of the physical device
                string gpuName = "";
                NVStatus = NVImport.NvAPI_GPU_GetFullName(physicalGpus[physicalGpuIndex], ref gpuName);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_GPU_GetFullName returned OK. The GPU Full Name is {gpuName}");
                    stringToReturn += $"NVIDIA Video card #{physicalGpuIndex} is a {gpuName}\n";
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting the GPU full name! NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }

                //This function retrieves the Quadro status for the GPU (1 if Quadro, 0 if GeForce)
                uint quadroStatus = 0;
                NVStatus = NVImport.NvAPI_GPU_GetQuadroStatus(physicalGpus[physicalGpuIndex], out quadroStatus);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    if (quadroStatus == 0)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Video Card is one from the GeForce range");
                        stringToReturn += $"NVIDIA Video card #{physicalGpuIndex} is in the GeForce range\n";
                    }
                    else if (quadroStatus == 1)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Video Card is one from the Quadro range");
                        stringToReturn += $"NVIDIA Video card #{physicalGpuIndex} is in the Quadro range\n";
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Video Card is neither a GeForce or Quadro range vodeo card (QuadroStatus = {quadroStatus})");
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Error GETTING qUADRO STATUS. NvAPI_GPU_GetQuadroStatus() returned error code {NVStatus}");
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
                foreach (NV_MOSAIC_GRID_TOPO_V2 gridTopology in displayConfig.MosaicConfig.MosaicGridTopos)
                {
                    stringToReturn += $"NOTE: This Surround/Mosaic screen will be treated as a single display by Windows.\n";
                    stringToReturn += $"The NVIDIA Surround/Mosaic Grid Topology #{count} is {gridTopology.Rows} Rows x {gridTopology.Columns} Columns\n";
                    stringToReturn += $"The NVIDIA Surround/Mosaic Grid Topology #{count} involves {gridTopology.DisplayCount} Displays\n";
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
                    stringToReturn += $"Display {displayId} BPC is {myDisplay.ColorData.Bpc.ToString("G")}.\n";
                    stringToReturn += $"Display {displayId} ColorFormat is {myDisplay.ColorData.ColorFormat.ToString("G")}.\n";
                    stringToReturn += $"Display {displayId} Colorimetry is {myDisplay.ColorData.Colorimetry.ToString("G")}.\n";
                    stringToReturn += $"Display {displayId} ColorSelectionPolicy is {myDisplay.ColorData.ColorSelectionPolicy.ToString("G")}.\n";
                    stringToReturn += $"Display {displayId} Depth is {myDisplay.ColorData.Depth.ToString("G")}.\n";
                    stringToReturn += $"Display {displayId} DynamicRange is {myDisplay.ColorData.DynamicRange.ToString("G")}.\n";

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

                        if (myDisplay.HdrCapabilities.IsDolbyVisionSupported)
                        {
                            stringToReturn += $"Display {displayId} supports DolbyVision HDR.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support DolbyVision HDR.\n";
                        }
                        if (myDisplay.HdrCapabilities.IsST2084EotfSupported)
                        {
                            stringToReturn += $"Display {displayId} supports ST2084EOTF HDR Mode.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support ST2084EOTF HDR Mode.\n";
                        }
                        if (myDisplay.HdrCapabilities.IsTraditionalHdrGammaSupported)
                        {
                            stringToReturn += $"Display {displayId} supports Traditional HDR Gamma.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support Traditional HDR Gamma.\n";
                        }
                        if (myDisplay.HdrCapabilities.IsEdrSupported)
                        {
                            stringToReturn += $"Display {displayId} supports EDR.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support EDR.\n";
                        }
                        if (myDisplay.HdrCapabilities.IsTraditionalSdrGammaSupported)
                        {
                            stringToReturn += $"Display {displayId} supports SDR Gamma.\n";
                        }
                        else
                        {
                            stringToReturn += $"Display {displayId} DOES NOT support SDR Gamma.\n";
                        }
                    }
                    else
                    {
                        stringToReturn += $"NVIDIA HDR is Disabled (HDR may still be enabled within Windows itself)\n";
                    }
                }
            }

            stringToReturn += $"\n\n";
            // Now we also get the Windows CCD Library info, and add it to the above
            stringToReturn += WinLibrary.GetLibrary().PrintActiveConfig();

            return stringToReturn;
        }

        public bool SetActiveConfig(NVIDIA_DISPLAY_CONFIG displayConfig)
        {

            if (_initialised)
            {

                NVAPI_STATUS NVStatus = NVAPI_STATUS.NVAPI_ERROR;

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

                        if (!ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays.ContainsKey(displayId))
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Display {displayId} doesn't exist in this setup, so skipping changing any NVIDIA display Settings.");
                            continue;
                        }

                        // Remove any custom NVIDIA Colour settings
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off colour if it's user set colour.");

                        NV_COLOR_DATA_V5 colorData = myDisplay.ColorData;
                        try
                        {
                            // If the setting for this display is not the same as we want, then we set it to NV_COLOR_SELECTION_POLICY_BEST_QUALITY
                            if (ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].ColorData.ColorSelectionPolicy != NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_BEST_QUALITY)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off NVIDIA customer colour settings for display {displayId}.");

                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want the standard colour settings to be {myDisplay.ColorData.ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}.");
                                // Force the colorData to be NV_COLOR_SELECTION_POLICY_BEST_QUALITY so that we return the color control to Windows
                                // We will change the colorData to whatever is required later on
                                //colorData = myDisplay.ColorData;
                                colorData.ColorSelectionPolicy = NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_BEST_QUALITY;

                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want the standard colour settings to be {myDisplay.ColorData.ColorSelectionPolicy.ToString("G")} and they are currently {ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].ColorData.ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off standard colour mode for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings Color selection policy {colorData.ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings BPC {colorData.Bpc} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings colour format {colorData.ColorFormat} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings colourimetry {colorData.Colorimetry} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings colour depth {colorData.Depth} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings dynamic range {colorData.DynamicRange} for Mosaic display {displayId}");

                                // Set the command as a 'SET'
                                colorData.Cmd = NV_COLOR_CMD.NV_COLOR_CMD_SET;
                                NVStatus = NVImport.NvAPI_Disp_ColorControl(displayId, ref colorData);
                                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Disp_ColorControl returned OK. BPC is set to {colorData.Bpc.ToString("G")}. Color Format is set to {colorData.ColorFormat.ToString("G")}. Colorimetry is set to {colorData.Colorimetry.ToString("G")}. Color Selection Policy is set to {colorData.ColorSelectionPolicy.ToString("G")}. Color Depth is set to {colorData.Depth.ToString("G")}. Dynamic Range is set to {colorData.DynamicRange.ToString("G")}");
                                    switch (colorData.ColorSelectionPolicy)
                                    {
                                        case NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_USER:
                                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Color Selection Policy is set to NV_COLOR_SELECTION_POLICY_USER so the color settings have been set by the user in the NVIDIA Control Panel.");
                                            break;
                                        case NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_BEST_QUALITY: // Also matches NV_COLOR_SELECTION_POLICY_DEFAULT as it is 1
                                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Color Selection Policy is set to NV_COLOR_SELECTION_POLICY_BEST_QUALITY so the color settings are being handled by the Windows OS.");
                                            break;
                                        case NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_UNKNOWN:
                                            SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: Color Selection Policy is set to NV_COLOR_SELECTION_POLICY_UNKNOWN so the color settings aren't being handled by either the Windows OS or the NVIDIA Setup!");
                                            break;
                                    }
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: Your monitor {displayId} doesn't support the requested color settings. BPC = {colorData.Bpc.ToString("G")}. Color Format = {colorData.ColorFormat.ToString("G")}. Colorimetry = {colorData.Colorimetry.ToString("G")}. Color Selection Policy = {colorData.ColorSelectionPolicy.ToString("G")}. Color Depth = {colorData.Depth.ToString("G")}. Dynamic Range = {colorData.DynamicRange.ToString("G")}. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The input buffer is not large enough to hold it's contents. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while seting the color settings! NvAPI_Disp_ColorControl() returned error code {NVStatus}. It's most likely that your monitor {displayId} doesn't support this color mode.");
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

                        NV_HDR_COLOR_DATA_V2 hdrColorData = myDisplay.HdrColorData;
                        try
                        {

                            // if it's not the same HDR we want, then we turn off HDR (and will apply it if needed later on in SetActiveOverride)
                            if (ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].HdrColorData.HdrMode != NV_HDR_MODE.OFF)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn on custom HDR mode for display {displayId}.");

                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: HDR mode is currently {ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].HdrColorData.HdrMode.ToString("G")} for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings BPC  {hdrColorData.HdrBpc} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings HDR Colour Format {hdrColorData.HdrColorFormat} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings HDR dynamic range {hdrColorData.HdrDynamicRange} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings HDR Mode {hdrColorData.HdrMode} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings Mastering Display Data {hdrColorData.MasteringDisplayData} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings Static Meradata Description ID {hdrColorData.StaticMetadataDescriptorId} for Mosaic display {displayId}");
                                // Apply the HDR removal
                                hdrColorData.Cmd = NV_HDR_CMD.CMD_SET;
                                hdrColorData.HdrMode = NV_HDR_MODE.OFF;
                                NVStatus = NVImport.NvAPI_Disp_HdrColorControl(displayId, ref hdrColorData);
                                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Disp_HdrColorControl returned OK. We just successfully turned off the HDR mode for Mosaic display {displayId}.");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The input buffer is not large enough to hold it's contents. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Disp_HdrColorControl() returned error code {NVStatus}. It's most likely that your monitor {displayId} doesn't support HDR.");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want only want to turn off custom NVIDIA HDR settings if needed for display {displayId}, and that currently isn't required. Skipping changing NVIDIA HDR mode.");
                            }

                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception caused while turning off prior NVIDIA HDR colour settings for display {displayId}.");
                        }

                        // Set any AdaptiveSync settings
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
                            NVStatus = NVImport.NvAPI_DISP_SetAdaptiveSyncData(displayId, ref adaptiveSyncData);
                            if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_DISP_SetAdaptiveSyncData returned OK. We just successfully set the Adaptive Sync settings for display {displayId}.");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The input buffer is not large enough to hold it's contents. NvAPI_DISP_SetAdaptiveSyncData() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_DISP_SetAdaptiveSyncData() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_SetAdaptiveSyncData() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_SetAdaptiveSyncData() returned error code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_DISP_SetAdaptiveSyncData() returned error code {NVStatus}");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_DISP_SetAdaptiveSyncData() returned error code {NVStatus}. It's most likely that your monitor {displayId} doesn't support HDR.");
                            }

                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception caused while trying to set NVIDIA Adaptive Sync settings for display {displayId}.");
                        }
                    }

                }


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
                        /*// We need to change to a Mosaic profile, so we need to apply the new Mosaic Topology
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic current config is different as the one we want, so applying the Mosaic config now");
                        // If we get here then the display is valid, so now we actually apply the new Mosaic Topology
                        NVStatus = NVImport.NvAPI_Mosaic_SetCurrentTopo(displayConfig.MosaicConfig.MosaicTopologyBrief, displayConfig.MosaicConfig.MosaicDisplaySettings, displayConfig.MosaicConfig.OverlapX, displayConfig.MosaicConfig.OverlapY, 0);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_SetCurrentTopo returned OK.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                            System.Threading.Thread.Sleep(500);
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NO_ACTIVE_SLI_TOPOLOGY)
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
                        }

                        // Turn on the selected Mosaic
                        uint enable = 1;
                        NVStatus = NVImport.NvAPI_Mosaic_EnableCurrentTopo(enable);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_EnableCurrentTopo returned OK. Previously set Mosiac config re-enabled.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                            System.Threading.Thread.Sleep(500);
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
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

                        NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags = NV_MOSAIC_SETDISPLAYTOPO_FLAGS.MAXIMIZE_PERFORMANCE;

                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic current config is different as the one we want, so applying the Mosaic config now");
                        // If we get here then the display is valid, so now we actually apply the new Mosaic Topology
                        NVStatus = NVImport.NvAPI_Mosaic_SetDisplayGrids(displayConfig.MosaicConfig.MosaicGridTopos, displayConfig.MosaicConfig.MosaicGridCount, setTopoFlags);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_SetDisplayGrids returned OK.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                            System.Threading.Thread.Sleep(500);
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NO_ACTIVE_SLI_TOPOLOGY)
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
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
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
                        else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Display Grids! NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        }
                    }

                }
                else if (!displayConfig.MosaicConfig.IsMosaicEnabled && ActiveDisplayConfig.MosaicConfig.IsMosaicEnabled)
                {
                    // We are on a Mosaic profile now, and we need to change to a non-Mosaic profile
                    // We need to disable the Mosaic Topology

                    NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags = NV_MOSAIC_SETDISPLAYTOPO_FLAGS.ALLOW_INVALID;

                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic config that is currently set is no longer needed. Removing Mosaic config.");
                    NV_MOSAIC_GRID_TOPO_V2[] individualScreensTopology = CreateSingleScreenMosaicTopology();

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
                    NVStatus = NVImport.NvAPI_Mosaic_SetDisplayGrids(individualScreensTopology, (UInt32)individualScreensTopology.Length, setTopoFlags);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_SetDisplayGrids returned OK.");
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                        System.Threading.Thread.Sleep(500);
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_ACTIVE_SLI_TOPOLOGY)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: No matching GPU topologies could be found. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_TOPO_NOT_POSSIBLE)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The Display ID of the first display is not currently possible to use. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}. Trying again with the next display.");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: One or more arguments passed in are invalid. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_MODE_CHANGE_FAILED)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: There was an error changing the display mode. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }
                    else
                    {
                        // If we get here, we may have an error, or it may have worked successfully! So we need to check again :( 
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Display Grids! NvAPI_Mosaic_SetDisplayGrids() returned error code {NVStatus}");
                        return false;
                    }

                    // If we get here, it may or it may not have worked successfully! So we need to check again :( 
                    // We don't want to do a full ceck, so we do a quick check instead.
                    if (MosaicIsOn())
                    {
                        // If the Mosaic is still on, then the last mosaic disable failed, so we need to then try turning it off this using NvAPI_Mosaic_EnableCurrentTopo(0)
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Previous attempt to turn off Mosaic. Now trying to use NvAPI_Mosaic_EnableCurrentTopo to disable Mosaic instead.");
                        uint enable = 0;
                        NVStatus = NVImport.NvAPI_Mosaic_EnableCurrentTopo(enable);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_EnableCurrentTopo returned OK. Previously set Mosiac config now disabled");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Mosaic display change take place before continuing");
                            System.Threading.Thread.Sleep(500);
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            return false;
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_TOPO_NOT_POSSIBLE)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            return false;
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            return false;
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            return false;
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            return false;
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            return false;
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_MODE_CHANGE_FAILED)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: There was an error disabling the display mode. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            return false;
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            return false;
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_EnableCurrentTopo() returned error code {NVStatus}");
                            return false;
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic successfully disabled using NvAPI_Mosaic_SetDisplayGrids method.");
                    }
                }
                else if (!displayConfig.MosaicConfig.IsMosaicEnabled && !ActiveDisplayConfig.MosaicConfig.IsMosaicEnabled)
                {
                    // We are on a non-Mosaic profile now, and we are changing to a non-Mosaic profile
                    // so there is nothing to do as far as NVIDIA is concerned!
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We are on a non-Mosaic profile now, and we are changing to a non-Mosaic profile so there is no need to modify Mosaic settings!");
                }

                // Now we set the NVIDIA Display Config (if we have one!)
                // If the display profile is a cloned config then NVIDIA GetDisplayConfig doesn't work
                // so we need to check for that. We just skip the SetDisplayConfig as it won't exist
                if (displayConfig.DisplayConfigs.Count > 0)
                {
                    NVStatus = NVImport.NvAPI_DISP_SetDisplayConfig((UInt32)displayConfig.DisplayConfigs.Count, displayConfig.DisplayConfigs.ToArray(), NV_DISPLAYCONFIG_FLAGS.SAVE_TO_PERSISTENCE);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_DISP_SetDisplayConfig returned OK.");
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting 0.5 second to let the Display Config layout change take place before continuing");
                        System.Threading.Thread.Sleep(500);
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_ACTIVE_SLI_TOPOLOGY)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: No matching GPU topologies could be found. NvAPI_DISP_SetDisplayConfig() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The Display ID of the first display is not currently possible to use. NvAPI_DISP_SetDisplayConfig() returned error code {NVStatus}. Trying again with the next display.");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: One or more arguments passed in are invalid. NvAPI_DISP_SetDisplayConfig() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_DISP_SetDisplayConfig() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_DISP_SetDisplayConfig() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_DISP_SetDisplayConfig() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_MODE_CHANGE_FAILED)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: There was an error changing the display mode. NvAPI_DISP_SetDisplayConfig() returned error code {NVStatus}");
                        return false;
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                    {
                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_DISP_SetDisplayConfig() returned error code {NVStatus}");
                        return false;
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Setting the NVIDIA Display Config! NvAPI_DISP_SetDisplayConfig() returned error code {NVStatus}");
                        return false;
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Skipping setting the NVIDIA Display Config as there isn't one provided in the configuration.");
                }

            }
            return true;
        }

        public bool SetActiveConfigOverride(NVIDIA_DISPLAY_CONFIG displayConfig)
        {

            if (_initialised)
            {
                // Force another scan of what the display config is so that the following logic works
                UpdateActiveConfig();

                NVAPI_STATUS NVStatus = NVAPI_STATUS.NVAPI_ERROR;

                // Go through the physical adapters
                foreach (var physicalGPU in displayConfig.PhysicalAdapters)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Processing settings for Physical GPU #{physicalGPU.Key}");
                    NVIDIA_PER_ADAPTER_CONFIG myAdapter = physicalGPU.Value;
                    UInt32 myAdapterIndex = physicalGPU.Key;

                    foreach (var displayDict in myAdapter.Displays)
                    {
                        NVIDIA_PER_DISPLAY_CONFIG myDisplay = displayDict.Value;
                        UInt32 displayId = displayDict.Key;

                        // Now we try to set each display settings
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to process settings for display {displayId}.");

                        if (!ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays.ContainsKey(displayId))
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Display {displayId} doesn't exist in this setup, so skipping overriding any NVIDIA display Settings.");
                            continue;
                        }

                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn on colour if it's user set colour.");
                        // Now we try to set each display color

                        NV_COLOR_DATA_V5 colorData = myDisplay.ColorData;
                        try
                        {
                            // If this is a setting that says it uses user colour settings, then we turn it off
                            if (ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].ColorData.ColorSelectionPolicy != colorData.ColorSelectionPolicy)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to use custom NVIDIA HDR Colour for display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want the standard colour settings to be {myDisplay.ColorData.ColorSelectionPolicy.ToString("G")} and they are {ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].ColorData.ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn off standard colour mode for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings Color selection policy {colorData.ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings BPC {colorData.Bpc} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings colour format {colorData.ColorFormat} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings colourimetry {colorData.Colorimetry} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings colour depth {colorData.Depth} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings dynamic range {colorData.DynamicRange} for Mosaic display {displayId}");

                                // Set the command as a 'SET'
                                colorData.Cmd = NV_COLOR_CMD.NV_COLOR_CMD_SET;
                                NVStatus = NVImport.NvAPI_Disp_ColorControl(displayId, ref colorData);
                                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: NvAPI_Disp_ColorControl returned OK. BPC is set to {colorData.Bpc.ToString("G")}. Color Format is set to {colorData.ColorFormat.ToString("G")}. Colorimetry is set to {colorData.Colorimetry.ToString("G")}. Color Selection Policy is set to {colorData.ColorSelectionPolicy.ToString("G")}. Color Depth is set to {colorData.Depth.ToString("G")}. Dynamic Range is set to {colorData.DynamicRange.ToString("G")}");
                                    switch (colorData.ColorSelectionPolicy)
                                    {
                                        case NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_USER:
                                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Color Selection Policy is set to NV_COLOR_SELECTION_POLICY_USER so the color settings have been set by the user in the NVIDIA Control Panel.");
                                            break;
                                        case NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_BEST_QUALITY: // Also matches NV_COLOR_SELECTION_POLICY_DEFAULT as it is 1
                                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Color Selection Policy is set to NV_COLOR_SELECTION_POLICY_BEST_QUALITY so the color settings are being handled by the Windows OS.");
                                            break;
                                        case NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_UNKNOWN:
                                            SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: Color Selection Policy is set to NV_COLOR_SELECTION_POLICY_UNKNOWN so the color settings aren't being handled by either the Windows OS or the NVIDIA Setup!");
                                            break;
                                    }
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: Your monitor {displayId} doesn't support the requested color settings. BPC = {colorData.Bpc.ToString("G")}. Color Format = {colorData.ColorFormat.ToString("G")}. Colorimetry = {colorData.Colorimetry.ToString("G")}. Color Selection Policy = {colorData.ColorSelectionPolicy.ToString("G")}. Color Depth = {colorData.Depth.ToString("G")}. Dynamic Range = {colorData.DynamicRange.ToString("G")}. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: The input buffer is not large enough to hold it's contents. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: The NvAPI API needs to be initialized first. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: This entry point not available in this NVIDIA Driver. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: A miscellaneous error occurred. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Some non standard error occurred while seting the color settings! NvAPI_Disp_ColorControl() returned error code {NVStatus}. It's most likely that your monitor {displayId} doesn't support this color mode.");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want only want to turn on custom NVIDIA colour settings if needed for display {displayId}, and that currently isn't required. Skipping changing NVIDIA colour mode.");
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception caused while turning on NVIDIA custom colour settings for display {displayId}.");
                        }



                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn on NVIDIA HDR colour if it's user wants to use NVIDIA HDR colour.");
                        // Now we try to set each display color

                        NV_HDR_COLOR_DATA_V2 hdrColorData = myDisplay.HdrColorData;
                        try
                        {

                            // if it's HDR and it's a different mode than what we are in now, then set HDR
                            if (ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].HdrColorData.HdrMode != hdrColorData.HdrMode)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn on user-set HDR mode for display {displayId} as it's supposed to be on.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: HDR mode is currently {ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayId].HdrColorData.HdrMode.ToString("G")} for Mosaic display {displayId}.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings BPC  {hdrColorData.HdrBpc} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings HDR Colour Format {hdrColorData.HdrColorFormat} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings HDR dynamic range {hdrColorData.HdrDynamicRange} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings HDR Mode {hdrColorData.HdrMode} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings Mastering Display Data {hdrColorData.MasteringDisplayData} for Mosaic display {displayId}");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings Static Meradata Description ID {hdrColorData.StaticMetadataDescriptorId} for Mosaic display {displayId}");
                                // Apply the HDR removal
                                hdrColorData.Cmd = NV_HDR_CMD.CMD_SET;
                                NVStatus = NVImport.NvAPI_Disp_HdrColorControl(displayId, ref hdrColorData);
                                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: NvAPI_Disp_HdrColorControl returned OK. We just successfully turned off the HDR mode for Mosaic display {displayId}.");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: The input buffer is not large enough to hold it's contents. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: The NvAPI API needs to be initialized first. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: This entry point not available in this NVIDIA Driver. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: A miscellaneous error occurred. NvAPI_Disp_HdrColorControl() returned error code {NVStatus}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Some non standard error occurred while getting Mosaic Topology! NvAPI_Disp_HdrColorControl() returned error code {NVStatus}. It's most likely that your monitor {displayId} doesn't support HDR.");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want only want to turn on custom NVIDIA HDR if needed for display {displayId} and that currently isn't required. Skipping changing NVIDIA HDR mode.");
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception caused while turning on custom NVIDIA HDR colour settings for display {displayId}.");
                        }

                    }


                }

            }
            else
            {
                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfigOverride: ERROR - Tried to run SetActiveConfig but the NVIDIA NVAPI library isn't initialised!");
                throw new NVIDIALibraryException($"Tried to run SetActiveConfigOverride but the NVIDIA NVAPI library isn't initialised!");
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
                NVAPI_STATUS NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayGrids(ref mosaicGridCount);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");
                }

                // Get Current Mosaic Grid settings using the Grid topologies fnumbers we got before
                //NV_MOSAIC_GRID_TOPO_V2[] mosaicGridTopos = new NV_MOSAIC_GRID_TOPO_V2[mosaicGridCount];
                NV_MOSAIC_GRID_TOPO_V1[] mosaicGridTopos = new NV_MOSAIC_GRID_TOPO_V1[mosaicGridCount];
                NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayGrids(ref mosaicGridTopos, ref mosaicGridCount);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                */

                /*NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags = NV_MOSAIC_SETDISPLAYTOPO_FLAGS.NONE;
                bool topoValid = false;
                NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[] topoStatuses = new NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[displayConfig.MosaicConfig.MosaicGridCount];
                NVAPI_STATUS NVStatus = NVImport.NvAPI_Mosaic_ValidateDisplayGrids(setTopoFlags, ref displayConfig.MosaicConfig.MosaicGridTopos, ref topoStatuses, displayConfig.MosaicConfig.MosaicGridCount);
                //NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[] topoStatuses = new NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[mosaicGridCount];
                //NVStatus = NVImport.NvAPI_Mosaic_ValidateDisplayGrids(setTopoFlags, ref mosaicGridTopos, ref topoStatuses, mosaicGridCount);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
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
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_ACTIVE_SLI_TOPOLOGY)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: No matching GPU topologies could be found. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_TOPO_NOT_POSSIBLE)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The topology passed in is not currently possible. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INCOMPATIBLE_STRUCT_VERSION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: The version of the structure passed in is not compatible with this entrypoint. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_MODE_CHANGE_FAILED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: There was an error changing the display mode. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfig: A miscellaneous error occurred. NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_ValidateDisplayGrids() returned error code {NVStatus}");
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

            // check what the currently available displays are (include the ones not active)
            List<string> currentAllIds = GetAllConnectedDisplayIdentifiers();

            // CHeck that we have all the displayConfig DisplayIdentifiers we need available now
            if (displayConfig.DisplayIdentifiers.All(value => currentAllIds.Contains(value)))
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

        /*public bool IsEquivalentConfig(NVIDIA_DISPLAY_CONFIG displayConfig, NVIDIA_DISPLAY_CONFIG otherDisplayConfig)
        {
            // We want to check if the NVIDIA configurations are the equiavalent of each other
            // IMPORTANT: This function differs from Equals in that Equivalent allows some fields to differ in order to still match.
            // The goal is to identify when two display configurations would be the same if they were applied.

            SharedLogger.logger.Trace($"NVIDIALibrary/IsEquivalentConfig: Testing whether the NVIDIA display configuration is equivalent to another");
            if (_initialised)
            {
                NVAPI_STATUS NVStatus = NVAPI_STATUS.NVAPI_ERROR;

                // Check that displayConfig DisplayIdentifiers match
                if (!displayConfig.DisplayIdentifiers.All(value => otherDisplayConfig.DisplayIdentifiers.Contains(value)))
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/IsEquivalentConfig: Uh oh! The NVIDIA display identifiers don't match so NVIDIA Config is not equivalent to the other one.");
                    return false;
                }

                // Check that displayConfig Mosaic Configs match
                if (!displayConfig.MosaicConfig.Equals(otherDisplayConfig.MosaicConfig))
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/IsEquivalentConfig: Uh oh! The NVIDIA Mosaic Configs don't match so NVIDIA Config is not equivalent to the other one.");
                    return false;
                }

                // Check that displayConfig Hdr Configs match
                if (!displayConfig.HdrConfig.Equals(otherDisplayConfig.HdrConfig))
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/IsEquivalentConfig: Uh oh! The NVIDIA Hdr Configs don't match so NVIDIA Config is not equivalent to the other one.");
                    return false;
                }

                SharedLogger.logger.Trace($"NVIDIALibrary/IsEquivalentConfig: Success! The NVIDIA display configuration is possible to be used now");
                return true;
            }
            else
            {
                return false;
            }
        }*/

        public static bool MosaicIsOn()
        {
            PhysicalGpuHandle[] physicalGpus = new PhysicalGpuHandle[NVImport.NVAPI_MAX_PHYSICAL_GPUS];
            uint physicalGpuCount = 0;
            NVAPI_STATUS NVStatus = NVImport.NvAPI_EnumPhysicalGPUs(ref physicalGpus, out physicalGpuCount);
            if (NVStatus == NVAPI_STATUS.NVAPI_OK)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_EnumPhysicalGPUs returned {physicalGpuCount} Physical GPUs");
            }
            else
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting physical GPU count. NvAPI_EnumPhysicalGPUs() returned error code {NVStatus}");
            }

            // Go through the Physical GPUs one by one
            for (uint physicalGpuIndex = 0; physicalGpuIndex < physicalGpuCount; physicalGpuIndex++)
            {
                // Get current Mosaic Topology settings in brief (check whether Mosaic is on)
                NV_MOSAIC_TOPO_BRIEF mosaicTopoBrief = new NV_MOSAIC_TOPO_BRIEF();
                NV_MOSAIC_DISPLAY_SETTING_V2 mosaicDisplaySetting = new NV_MOSAIC_DISPLAY_SETTING_V2();
                int mosaicOverlapX = 0;
                int mosaicOverlapY = 0;
                NVStatus = NVImport.NvAPI_Mosaic_GetCurrentTopo(ref mosaicTopoBrief, ref mosaicDisplaySetting, out mosaicOverlapX, out mosaicOverlapY);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
                }

                // Check if there is a topology and that Mosaic is enabled
                if (mosaicTopoBrief.Topo != NV_MOSAIC_TOPO.TOPO_NONE && mosaicTopoBrief.Enabled == 1)
                {
                    return true;
                }
            }

            return false;
        }

        public List<string> GetCurrentDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"NVIDIALibrary/GetCurrentDisplayIdentifiers: Getting the current display identifiers for the displays in use now");
            return GetSomeDisplayIdentifiers(false);
        }

        public List<string> GetAllConnectedDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"NVIDIALibrary/GetAllConnectedDisplayIdentifiers: Getting all the display identifiers that can possibly be used");
            return GetSomeDisplayIdentifiers(true);
        }

        private List<string> GetSomeDisplayIdentifiers(bool allDisplays = true)
        {
            SharedLogger.logger.Debug($"NVIDIALibrary/GetCurrentDisplayIdentifiers: Generating the unique Display Identifiers for the currently active configuration");

            List<string> displayIdentifiers = new List<string>();

            // Enumerate all the Physical GPUs
            PhysicalGpuHandle[] physicalGpus = new PhysicalGpuHandle[NVImport.NV_MAX_PHYSICAL_GPUS];
            uint physicalGpuCount = 0;
            NVAPI_STATUS NVStatus = NVImport.NvAPI_EnumPhysicalGPUs(ref physicalGpus, out physicalGpuCount);
            if (NVStatus == NVAPI_STATUS.NVAPI_OK)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_EnumPhysicalGPUs returned {physicalGpuCount} Physical GPUs");
            }
            else
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting physical GPU count. NvAPI_EnumPhysicalGPUs() returned error code {NVStatus}");
            }

            // Go through the Physical GPUs one by one
            for (uint physicalGpuIndex = 0; physicalGpuIndex < physicalGpuCount; physicalGpuIndex++)
            {
                //We want to get the name of the physical device
                string gpuName = "";
                NVStatus = NVImport.NvAPI_GPU_GetFullName(physicalGpus[physicalGpuIndex], ref gpuName);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_GPU_GetFullName returned OK. The GPU Full Name is {gpuName}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting the GPU full name! NvAPI_GPU_GetFullName() returned error code {NVStatus}");
                }

                //We want to get the physical details of the physical device
                NV_GPU_BUS_TYPE busType = NV_GPU_BUS_TYPE.UNDEFINED;
                NVStatus = NVImport.NvAPI_GPU_GetBusType(physicalGpus[physicalGpuIndex], ref busType);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_GPU_GetBoardInfo returned OK. THe GPU BusType is {busType.ToString("G")}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_GPU_GetBoardInfo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_GPU_GetBoardInfo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetBoardInfo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetBoardInfo() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetBoardInfo() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_GPU_GetBoardInfo() returned error code {NVStatus}");
                }

                //We want to get the physical details of the physical device
                UInt32 busId = 0;
                NVStatus = NVImport.NvAPI_GPU_GetBusId(physicalGpus[physicalGpuIndex], ref busId);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_GPU_GetBusId returned OK. The GPU Bus ID was {busId}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_GPU_GetBusId() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_GPU_GetBusId() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetBusId() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetBusId() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetBusId() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_GPU_GetBusId() returned error code {NVStatus}");
                }

                // Next, we need to get all the connected Display IDs. 
                //This function retrieves the number of display IDs we know about
                UInt32 displayCount = 0;
                NVStatus = NVImport.NvAPI_GPU_GetConnectedDisplayIds(physicalGpus[physicalGpuIndex], ref displayCount, 0);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_DISP_GetGDIPrimaryDisplayId returned OK. We have {displayCount} connected displays detected.");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input buffer is not large enough to hold it's contents. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                }

                if (displayCount > 0)
                {
                    // Now we try to get the information about the displayIDs
                    NV_GPU_DISPLAYIDS_V2[] displayIds = new NV_GPU_DISPLAYIDS_V2[displayCount];
                    NVStatus = NVImport.NvAPI_GPU_GetConnectedDisplayIds(physicalGpus[physicalGpuIndex], ref displayIds, ref displayCount, 0);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_GPU_GetConnectedDisplayIds returned OK. We have {displayCount} physical GPUs");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input buffer is not large enough to hold it's contents. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_DISP_GetGDIPrimaryDisplayId() returned error code {NVStatus}");
                    }

                    // Now, we want to go through the displays 
                    foreach (NV_GPU_DISPLAYIDS_V2 oneDisplay in displayIds)
                    {
                        // If alldisplays is false, then we only want the active displays. We need to skip this one if it is not active
                        if (allDisplays == false && oneDisplay.IsActive == false)
                        {
                            // We want to skip this display as it is non-active, and we only want active displays
                            continue;
                        }


                        // Now we try to get the GPU and Output ID from the DisplayID
                        PhysicalGpuHandle physicalGpu = new PhysicalGpuHandle();
                        UInt32 gpuOutputId = 0;
                        NVStatus = NVImport.NvAPI_SYS_GetGpuAndOutputIdFromDisplayId(oneDisplay.DisplayId, out physicalGpu, out gpuOutputId);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_SYS_GetGpuAndOutputIdFromDisplayId returned OK. We received Physical GPU ID {physicalGpu} and GPU Output ID {gpuOutputId}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INSUFFICIENT_BUFFER)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input buffer is not large enough to hold it's contents. NvAPI_SYS_GetGpuAndOutputIdFromDisplayId() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_DISPLAY_ID)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The input monitor is either not connected or is not a DP or HDMI panel. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                        }
                        else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetConnectedDisplayIds() returned error code {NVStatus}");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_SYS_GetGpuAndOutputIdFromDisplayId() returned error code {NVStatus}");
                        }

                        // Lets set some EDID default in case the EDID doesn't work
                        string manufacturerName = "Unknown";
                        UInt32 productCode = 0;
                        UInt32 serialNumber = 0;
                        // We try to get an EDID block and extract the info                        
                        NV_EDID_V3 edidInfo = new NV_EDID_V3();
                        NVStatus = NVImport.NvAPI_GPU_GetEDID(physicalGpu, gpuOutputId, ref edidInfo);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_GPU_GetEDID returned OK. We have got an EDID Block.");
                            EDID edidParsedInfo = new EDID(edidInfo.EDID_Data);
                            manufacturerName = edidParsedInfo.ManufacturerCode;
                            productCode = edidParsedInfo.ProductCode;
                            serialNumber = edidParsedInfo.SerialNumber;
                        }
                        else
                        {
                            if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Either edidInfo was null when it was supplied, or gpuOutputId . NvAPI_GPU_GetEDID() returned status  code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_NVIDIA_DEVICE_NOT_FOUND)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: No active GPU was found. NvAPI_GPU_GetEDID() returned status  code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_EXPECTED_PHYSICAL_GPU_HANDLE)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The GPU Handle supplied was not a valid GPU Handle. NvAPI_GPU_GetEDID() returned status  code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_DATA_NOT_FOUND)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The display does not support EDID. NvAPI_GPU_GetEDID() returned status code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_GPU_GetEDID() returned status  code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_GPU_GetEDID() returned status  code {NVStatus}");
                            }
                            else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_GPU_GetEDID() returned error code {NVStatus}");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_GPU_GetEDID() returned error code {NVStatus}");
                            }
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
                            displayInfo.Add(oneDisplay.ConnectorType.ToString("G"));
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
                            SharedLogger.logger.Debug($"ProfileRepository/GenerateProfileDisplayIdentifiers: DisplayIdentifier: {displayIdentifier}");
                        }
                    }
                }
            }

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
        }


        public static NV_MOSAIC_GRID_TOPO_V2[] CreateSingleScreenMosaicTopology()
        {
            /*// Get current Mosaic Topology settings in brief (check whether Mosaic is on)
            NV_MOSAIC_TOPO_BRIEF mosaicTopoBrief = new NV_MOSAIC_TOPO_BRIEF();
            NV_MOSAIC_DISPLAY_SETTING_V2 mosaicDisplaySetting = new NV_MOSAIC_DISPLAY_SETTING_V2();
            int mosaicOverlapX = 0;
            int mosaicOverlapY = 0;
            NVAPI_STATUS NVStatus = NVImport.NvAPI_Mosaic_GetCurrentTopo(ref mosaicTopoBrief, ref mosaicDisplaySetting, out mosaicOverlapX, out mosaicOverlapY);
            if (NVStatus == NVAPI_STATUS.NVAPI_OK)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
            }
            else
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_GetCurrentTopo() returned error code {NVStatus}");
            }*/

            // Figure out how many Mosaic Grid topoligies there are                    
            uint mosaicGridCount = 0;
            NVAPI_STATUS NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayGrids(ref mosaicGridCount);
            if (NVStatus == NVAPI_STATUS.NVAPI_OK)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetCurrentTopo returned OK.");
            }

            // Get Current Mosaic Grid settings using the Grid topologies fnumbers we got before
            NV_MOSAIC_GRID_TOPO_V2[] mosaicGridTopos = new NV_MOSAIC_GRID_TOPO_V2[mosaicGridCount];
            NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayGrids(ref mosaicGridTopos, ref mosaicGridCount);
            if (NVStatus == NVAPI_STATUS.NVAPI_OK)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_EnumDisplayGrids returned OK.");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
            }
            else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
            }
            else
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology! NvAPI_Mosaic_EnumDisplayGrids() returned error code {NVStatus}");
            }

            // Sum up all the screens we have
            //int totalScreenCount = mosaicGridTopos.Select(tp => tp.Displays).Sum(d => d.Count());
            List<NV_MOSAIC_GRID_TOPO_V2> screensToReturn = new List<NV_MOSAIC_GRID_TOPO_V2>();

            foreach (NV_MOSAIC_GRID_TOPO_V2 gridTopo in mosaicGridTopos)
            {
                // Figure out how many Mosaic Display topologies there are                    
                UInt32 mosaicDisplayModesCount = 0;
                NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayModes(gridTopo, ref mosaicDisplayModesCount);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_EnumDisplayModes returned OK.");
                }

                // Get Current Mosaic Display Topology settings using the Grid topologies numbers we got before
                //NV_MOSAIC_TOPO myGridTopo = gridTopo;
                NV_MOSAIC_DISPLAY_SETTING_V2[] mosaicDisplaySettings = new NV_MOSAIC_DISPLAY_SETTING_V2[mosaicDisplayModesCount];
                NVStatus = NVImport.NvAPI_Mosaic_EnumDisplayModes(gridTopo, ref mosaicDisplaySettings, ref mosaicDisplayModesCount);
                if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_EnumDisplayModes returned OK.");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_API_NOT_INITIALIZED)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: The NvAPI API needs to be initialized first. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_NO_IMPLEMENTATION)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: This entry point not available in this NVIDIA Driver. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                }
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting Mosaic Topology Display Settings! NvAPI_Mosaic_EnumDisplayModes() returned error code {NVStatus}");
                }

                for (int displayIndexToUse = 0; displayIndexToUse < gridTopo.DisplayCount; displayIndexToUse++)
                {
                    NV_MOSAIC_GRID_TOPO_V2 thisScreen = new NV_MOSAIC_GRID_TOPO_V2();
                    thisScreen.Version = NVImport.NV_MOSAIC_GRID_TOPO_V2_VER;
                    thisScreen.Rows = 1;
                    thisScreen.Columns = 1;
                    thisScreen.DisplayCount = 1;
                    thisScreen.Flags = 0;
                    thisScreen.Displays = new NV_MOSAIC_GRID_TOPO_DISPLAY_V2[NVImport.NV_MOSAIC_MAX_DISPLAYS];
                    thisScreen.Displays[0].Version = NVImport.NV_MOSAIC_GRID_TOPO_DISPLAY_V2_VER;
                    thisScreen.Displays[0].DisplayId = gridTopo.Displays[displayIndexToUse].DisplayId;
                    thisScreen.Displays[0].CloneGroup = gridTopo.Displays[displayIndexToUse].CloneGroup;
                    thisScreen.Displays[0].OverlapX = gridTopo.Displays[displayIndexToUse].OverlapX;
                    thisScreen.Displays[0].OverlapY = gridTopo.Displays[displayIndexToUse].OverlapY;
                    thisScreen.Displays[0].PixelShiftType = gridTopo.Displays[displayIndexToUse].PixelShiftType;
                    thisScreen.Displays[0].Rotation = gridTopo.Displays[displayIndexToUse].Rotation;
                    thisScreen.DisplaySettings = new NV_MOSAIC_DISPLAY_SETTING_V1();
                    thisScreen.DisplaySettings.Version = gridTopo.DisplaySettings.Version;
                    thisScreen.DisplaySettings.Bpp = gridTopo.DisplaySettings.Bpp;
                    thisScreen.DisplaySettings.Freq = gridTopo.DisplaySettings.Freq;
                    thisScreen.DisplaySettings.Height = gridTopo.DisplaySettings.Height;
                    thisScreen.DisplaySettings.Width = gridTopo.DisplaySettings.Width;
                    screensToReturn.Add(thisScreen);
                }

            }
            /*


                        // Selected the best display settings to use
                        NV_MOSAIC_DISPLAY_SETTING_V2 bestSetting = mosaicDisplaySettings.OrderByDescending(
                                        settings => (long)settings.Width *
                                                    settings.Height *
                                                    settings.Bpp *
                                                    settings.Freq).First();
            */

            return screensToReturn.ToArray();
        }

        public static bool ListOfArraysEqual(List<NV_RECT[]> a1, List<NV_RECT[]> a2)
        {
            if (a1.Count == a2.Count)
            {
                for (int i = 0; i < a1.Count; i++)
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