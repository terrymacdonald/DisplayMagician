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
    public struct NVIDIA_HDR_CONFIG : IEquatable<NVIDIA_HDR_CONFIG>
    {
        public Dictionary<string, NV_HDR_CAPABILITIES_V2> HdrCapabilities;
        public Dictionary<string, NV_HDR_COLOR_DATA_V2> HdrColorData;
        public bool IsNvHdrEnabled;

        public override bool Equals(object obj) => obj is NVIDIA_HDR_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_HDR_CONFIG other)
        => HdrCapabilities.SequenceEqual(other.HdrCapabilities) &&
           HdrColorData.SequenceEqual(other.HdrColorData) &&
           IsNvHdrEnabled == other.IsNvHdrEnabled;

        public override int GetHashCode()
        {
            return (HdrCapabilities, HdrColorData, IsNvHdrEnabled).GetHashCode();
        }
        public static bool operator ==(NVIDIA_HDR_CONFIG lhs, NVIDIA_HDR_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_HDR_CONFIG lhs, NVIDIA_HDR_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_COLOR_CONFIG : IEquatable<NVIDIA_COLOR_CONFIG>
    {
        public Dictionary<string, NV_COLOR_DATA_V5> ColorData;

        public override bool Equals(object obj) => obj is NVIDIA_COLOR_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_COLOR_CONFIG other)
        => ColorData.SequenceEqual(other.ColorData);

        public override int GetHashCode()
        {
            return (ColorData).GetHashCode();
        }
        public static bool operator ==(NVIDIA_COLOR_CONFIG lhs, NVIDIA_COLOR_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_COLOR_CONFIG lhs, NVIDIA_COLOR_CONFIG rhs) => !(lhs == rhs);
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
    public struct NVIDIA_DISPLAY_CONFIG : IEquatable<NVIDIA_DISPLAY_CONFIG>
    {
        public NVIDIA_MOSAIC_CONFIG MosaicConfig;
        public NVIDIA_HDR_CONFIG HdrConfig;
        public NVIDIA_COLOR_CONFIG ColorConfig;
        public Dictionary<string, List<NV_CUSTOM_DISPLAY_V1>> CustomDisplays;
        public List<NV_DISPLAYCONFIG_PATH_INFO_V2> DisplayConfigs;
        // Note: We purposely have left out the DisplayNames from the Equals as it's order keeps changing after each reboot and after each profile swap
        // and it is informational only and doesn't contribute to the configuration (it's used for generating the Screens structure, and therefore for
        // generating the profile icon.
        public Dictionary<string, string> DisplayNames;
        public List<string> DisplayIdentifiers;

        public override bool Equals(object obj) => obj is NVIDIA_DISPLAY_CONFIG other && this.Equals(other);

        public bool Equals(NVIDIA_DISPLAY_CONFIG other)
        => MosaicConfig.Equals(other.MosaicConfig) &&
           HdrConfig.Equals(other.HdrConfig) &&
           ColorConfig.Equals(other.ColorConfig) &&
           CustomDisplays.SequenceEqual(other.CustomDisplays) &&
           DisplayConfigs.SequenceEqual(other.DisplayConfigs) &&
           DisplayIdentifiers.SequenceEqual(other.DisplayIdentifiers);

        public override int GetHashCode()
        {
            return (MosaicConfig, HdrConfig, CustomDisplays, DisplayConfigs, DisplayIdentifiers, DisplayNames).GetHashCode();
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

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

        static NVIDIALibrary() { }
        public NVIDIALibrary()
        {
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

            myDefaultConfig.MosaicConfig.MosaicGridTopos = new NV_MOSAIC_GRID_TOPO_V2[0];
            myDefaultConfig.MosaicConfig.MosaicViewports = new List<NV_RECT[]>();
            myDefaultConfig.HdrConfig.HdrCapabilities = new Dictionary<string, NV_HDR_CAPABILITIES_V2>();
            myDefaultConfig.HdrConfig.HdrColorData = new Dictionary<string, NV_HDR_COLOR_DATA_V2>();
            myDefaultConfig.ColorConfig.ColorData = new Dictionary<string, NV_COLOR_DATA_V5>();
            myDefaultConfig.CustomDisplays = new Dictionary<string, List<NV_CUSTOM_DISPLAY_V1>>();
            myDefaultConfig.DisplayConfigs = new List<NV_DISPLAYCONFIG_PATH_INFO_V2>();
            myDefaultConfig.DisplayNames = new Dictionary<string, string>();
            myDefaultConfig.DisplayIdentifiers = new List<string>();

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
            NVIDIA_DISPLAY_CONFIG myDisplayConfig = new NVIDIA_DISPLAY_CONFIG();

            if (_initialised)
            {
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
                }

                // Get current Mosaic Topology settings in brief (check whether Mosaic is on)
                NV_MOSAIC_TOPO_BRIEF mosaicTopoBrief = new NV_MOSAIC_TOPO_BRIEF();
                NV_MOSAIC_DISPLAY_SETTING_V2 mosaicDisplaySettings = new NV_MOSAIC_DISPLAY_SETTING_V2();
                int mosaicOverlapX = 0;
                int mosaicOverlapY = 0;
                NVStatus = NVImport.NvAPI_Mosaic_GetCurrentTopo(ref mosaicTopoBrief, ref mosaicDisplaySettings, out mosaicOverlapX, out mosaicOverlapY);
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
                    // Mosaic is enabled!
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NVIDIA Mosaic is enabled.");
                    myDisplayConfig.MosaicConfig.MosaicTopologyBrief = mosaicTopoBrief;
                    myDisplayConfig.MosaicConfig.MosaicDisplaySettings = mosaicDisplaySettings;
                    myDisplayConfig.MosaicConfig.OverlapX = mosaicOverlapX;
                    myDisplayConfig.MosaicConfig.OverlapY = mosaicOverlapY;
                    myDisplayConfig.MosaicConfig.IsMosaicEnabled = true;

                    // Get more Mosaic Topology detailed settings
                    NV_MOSAIC_TOPO_GROUP mosaicTopoGroup = new NV_MOSAIC_TOPO_GROUP();
                    NVStatus = NVImport.NvAPI_Mosaic_GetTopoGroup(ref mosaicTopoBrief, ref mosaicTopoGroup);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_Mosaic_GetTopoGroup returned OK.");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_NOT_SUPPORTED)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Mosaic is not supported with the existing hardware. NvAPI_Mosaic_GetTopoGroup() returned error code {NVStatus}");
                    }
                    else if (NVStatus == NVAPI_STATUS.NVAPI_INVALID_ARGUMENT)
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: One or more argumentss passed in are invalid. NvAPI_Mosaic_GetTopoGroup() returned error code {NVStatus}");
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
                List<NV_DISPLAYCONFIG_PATH_INFO_V2> allDisplayConfigs = new List<NV_DISPLAYCONFIG_PATH_INFO_V2>();

                /*uint pathInfoCount = 0;
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
                else if (NVStatus == NVAPI_STATUS.NVAPI_ERROR)
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: A miscellaneous error occurred. NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Some non standard error occurred while getting NVIDIA Display Config! NvAPI_DISP_GetDisplayConfig() returned error code {NVStatus} on first pass");
                }*/

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
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: NvAPI_GPU_GetConnectedDisplayIds returned OK on second pass. We have {displayCount} physical GPUs");
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
                        Dictionary<string, NV_HDR_CAPABILITIES_V2> allHdrCapabilities = new Dictionary<string, NV_HDR_CAPABILITIES_V2>();
                        Dictionary<string, NV_HDR_COLOR_DATA_V2> allHdrColorData = new Dictionary<string, NV_HDR_COLOR_DATA_V2>();
                        Dictionary<string, NV_COLOR_DATA_V5> allColorData = new Dictionary<string, NV_COLOR_DATA_V5>();
                        Dictionary<string, List<NV_CUSTOM_DISPLAY_V1>> allCustomDisplays = new Dictionary<string, List<NV_CUSTOM_DISPLAY_V1>>();
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

                            // We get the Color Capabilities of the display
                            NV_COLOR_DATA_V5 colorData = new NV_COLOR_DATA_V5();
                            // Set the command as a 'GET'
                            colorData.Cmd = NV_COLOR_CMD.NV_COLOR_CMD_GET;
                            NVStatus = NVImport.NvAPI_Disp_ColorControl(displayIds[displayIndex].DisplayId, ref colorData);
                            if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Your monitor {displayIds[displayIndex].DisplayId} has the following color settings set. BPC = {colorData.Bpc.ToString("G")}. Color Format = {colorData.ColorFormat.ToString("G")}. Colorimetry = {colorData.Colorimetry.ToString("G")}. Color Selection Policy = {colorData.ColorSelectionPolicy.ToString("G")}. Color Depth = {colorData.Depth.ToString("G")}. Dynamic Range = {colorData.DynamicRange.ToString("G")}. NvAPI_Disp_ColorControl() returned error code {NVStatus}");
                                allColorData.Add(displayIds[displayIndex].DisplayId.ToString(), colorData);
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

                                allHdrCapabilities.Add(displayIds[displayIndex].DisplayId.ToString(), hdrCapabilities);
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
                                }
                                allHdrColorData.Add(displayIds[displayIndex].DisplayId.ToString(), hdrColorData);
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
                                    customDisplayConfig.Add(customDisplay);
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

                            }
                            if (customDisplayConfig.Count > 0)
                            {
                                allCustomDisplays.Add(displayIds[displayIndex].DisplayId, customDisplayConfig);
                            }*/
                        }

                        // Store the HDR information
                        myDisplayConfig.HdrConfig.IsNvHdrEnabled = isNvHdrEnabled;
                        myDisplayConfig.HdrConfig.HdrCapabilities = allHdrCapabilities;
                        myDisplayConfig.HdrConfig.HdrColorData = allHdrColorData;
                        myDisplayConfig.ColorConfig.ColorData = allColorData;
                        myDisplayConfig.CustomDisplays = allCustomDisplays;
                        myDisplayConfig.DisplayConfigs = allDisplayConfigs;
                    }

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

            // Start printing out things
            stringToReturn += $"\n****** NVIDIA COLOR CONFIG *******\n";
            foreach (KeyValuePair<string, NV_COLOR_DATA_V5> colorData in displayConfig.ColorConfig.ColorData)
            {
                string displayId = colorData.Key.ToString();
                stringToReturn += $"Display {displayId} BPC is {colorData.Value.Bpc.ToString("G")}.\n";
                stringToReturn += $"Display {displayId} ColorFormat is {colorData.Value.ColorFormat.ToString("G")}.\n";
                stringToReturn += $"Display {displayId} Colorimetry is {colorData.Value.Colorimetry.ToString("G")}.\n";
                stringToReturn += $"Display {displayId} ColorSelectionPolicy is {colorData.Value.ColorSelectionPolicy.ToString("G")}.\n";
                stringToReturn += $"Display {displayId} Depth is {colorData.Value.Depth.ToString("G")}.\n";
                stringToReturn += $"Display {displayId} DynamicRange is {colorData.Value.DynamicRange.ToString("G")}.\n";
            }

            // Start printing out HDR things
            stringToReturn += $"\n****** NVIDIA HDR CONFIG *******\n";
            if (displayConfig.HdrConfig.IsNvHdrEnabled)
            {
                stringToReturn += $"NVIDIA HDR is Enabled\n";
                if (displayConfig.HdrConfig.HdrCapabilities.Count > 0)
                {
                    stringToReturn += $"There are {displayConfig.HdrConfig.HdrCapabilities.Count} NVIDIA HDR devices in use.\n";
                }
                if (displayConfig.MosaicConfig.MosaicGridTopos.Length == 1)
                {
                    stringToReturn += $"There is 1 NVIDIA HDR devices in use.\n";
                }
                else
                {
                    stringToReturn += $"There are no NVIDIA HDR devices in use.\n";
                }

                foreach (KeyValuePair<string, NV_HDR_CAPABILITIES_V2> hdrCapabilityItem in displayConfig.HdrConfig.HdrCapabilities)
                {
                    string displayId = hdrCapabilityItem.Key.ToString();
                    if (hdrCapabilityItem.Value.IsDolbyVisionSupported)
                    {
                        stringToReturn += $"Display {displayId} supports DolbyVision HDR.\n";
                    }
                    else
                    {
                        stringToReturn += $"Display {displayId} DOES NOT support DolbyVision HDR.\n";
                    }
                    if (hdrCapabilityItem.Value.IsST2084EotfSupported)
                    {
                        stringToReturn += $"Display {displayId} supports ST2084EOTF HDR Mode.\n";
                    }
                    else
                    {
                        stringToReturn += $"Display {displayId} DOES NOT support ST2084EOTF HDR Mode.\n";
                    }
                    if (hdrCapabilityItem.Value.IsTraditionalHdrGammaSupported)
                    {
                        stringToReturn += $"Display {displayId} supports Traditional HDR Gamma.\n";
                    }
                    else
                    {
                        stringToReturn += $"Display {displayId} DOES NOT support Traditional HDR Gamma.\n";
                    }
                    if (hdrCapabilityItem.Value.IsEdrSupported)
                    {
                        stringToReturn += $"Display {displayId} supports EDR.\n";
                    }
                    else
                    {
                        stringToReturn += $"Display {displayId} DOES NOT support EDR.\n";
                    }
                    if (hdrCapabilityItem.Value.IsTraditionalSdrGammaSupported)
                    {
                        stringToReturn += $"Display {displayId} supports SDR Gamma.\n";
                    }
                    else
                    {
                        stringToReturn += $"Display {displayId} DOES NOT support SDR Gamma.\n";
                    }
                }
            }
            else
            {
                stringToReturn += $"NVIDIA HDR is Disabled (HDR may still be enabled within Windows itself)\n";
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
                // Now we try to set each display color
                foreach (var colorDataDict in displayConfig.ColorConfig.ColorData)
                {
                    NV_COLOR_DATA_V5 colorData = colorDataDict.Value;
                    string displayId = colorDataDict.Key;
                    try
                    {
                        UInt32 displayIdAsUInt32 = UInt32.Parse(displayId);

                        if (!ActiveDisplayConfig.ColorConfig.ColorData.ContainsKey(displayId))
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Display {displayId} doesn't exist in this setup, so skipping turning off prior NVIDIA Color Settings.");
                            continue;
                        }

                        // If the setting for this display is not the same as we want, then we set it to NV_COLOR_SELECTION_POLICY_BEST_QUALITY
                        if (ActiveDisplayConfig.ColorConfig.ColorData[displayId].ColorSelectionPolicy != NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_BEST_QUALITY)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off NVIDIA customer colour settings for display {displayId}.");

                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want the standard colour settings to be {displayConfig.ColorConfig.ColorData[displayId].ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}.");
                            // Force the colorData to be NV_COLOR_SELECTION_POLICY_BEST_QUALITY so that we return the color control to Windows
                            // We will change the colorData to whatever is required later on
                            colorData = displayConfig.ColorConfig.ColorData[displayId];
                            colorData.ColorSelectionPolicy = NV_COLOR_SELECTION_POLICY.NV_COLOR_SELECTION_POLICY_BEST_QUALITY;

                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want the standard colour settings to be {displayConfig.ColorConfig.ColorData[displayId].ColorSelectionPolicy.ToString("G")} and they are {ActiveDisplayConfig.ColorConfig.ColorData[displayId].ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off standard colour mode for Mosaic display {displayId}.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings Color selection policy {colorData.ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings BPC {colorData.Bpc} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings colour format {colorData.ColorFormat} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings colourimetry {colorData.Colorimetry} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings colour depth {colorData.Depth} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want standard colour settings dynamic range {colorData.DynamicRange} for Mosaic display {displayId}");

                            // Set the command as a 'SET'
                            colorData.Cmd = NV_COLOR_CMD.NV_COLOR_CMD_SET;
                            NVStatus = NVImport.NvAPI_Disp_ColorControl(displayIdAsUInt32, ref colorData);
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
                }

                // Remove any custom NVIDIA HDR Colour settings
                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn off HDR colour if it's user set HDR colour.");
                // Now we try to set each display color
                foreach (var hdrColorDataDict in displayConfig.HdrConfig.HdrColorData)
                {
                    NV_HDR_COLOR_DATA_V2 hdrColorData = hdrColorDataDict.Value;
                    string displayId = hdrColorDataDict.Key;

                    try
                    {

                        UInt32 displayIdAsUInt32 = UInt32.Parse(displayId);

                        if (!ActiveDisplayConfig.HdrConfig.HdrColorData.ContainsKey(displayId))
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Display {displayId} doesn't exist in this setup, so skipping turning off HDR.");
                            continue;
                        }

                        // if it's not the same HDR we want, then we turn off HDR (and will apply it if needed later on in SetActiveOverride)
                        if (ActiveDisplayConfig.HdrConfig.HdrColorData[displayId].HdrMode != NV_HDR_MODE.OFF)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want to turn on custom HDR mode for display {displayId}.");

                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: HDR mode is currently {ActiveDisplayConfig.HdrConfig.HdrColorData[displayId].HdrMode.ToString("G")} for Mosaic display {displayId}.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings BPC  {hdrColorData.HdrBpc} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings HDR Colour Format {hdrColorData.HdrColorFormat} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings HDR dynamic range {hdrColorData.HdrDynamicRange} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings HDR Mode {hdrColorData.HdrMode} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings Mastering Display Data {hdrColorData.MasteringDisplayData} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We want HDR settings Static Meradata Description ID {hdrColorData.StaticMetadataDescriptorId} for Mosaic display {displayId}");
                            // Apply the HDR removal
                            hdrColorData.Cmd = NV_HDR_CMD.CMD_SET;
                            hdrColorData.HdrMode = NV_HDR_MODE.OFF;
                            NVStatus = NVImport.NvAPI_Disp_HdrColorControl(displayIdAsUInt32, ref hdrColorData);
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
                        // We need to change to a Mosaic profile, so we need to apply the new Mosaic Topology
                        NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags = NV_MOSAIC_SETDISPLAYTOPO_FLAGS.NONE;

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

                    // Turn off Mosaic
                    uint enable = 0;
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
                    }
                }
                else if (!displayConfig.MosaicConfig.IsMosaicEnabled && !ActiveDisplayConfig.MosaicConfig.IsMosaicEnabled)
                {
                    // We are on a non-Mosaic profile now, and we are changing to a non-Mosaic profile
                    // so there is nothing to do as far as NVIDIA is concerned!
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We are on a non-Mosaic profile now, and we are changing to a non-Mosaic profile so there is nothing to do as far as NVIDIA is concerned!");
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

                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn on colour if it's user set colour.");
                // Now we try to set each display color
                foreach (var colorDataDict in displayConfig.ColorConfig.ColorData)
                {
                    NV_COLOR_DATA_V5 colorData = colorDataDict.Value;
                    string displayId = colorDataDict.Key;
                    try
                    {
                        UInt32 displayIdAsUInt32 = UInt32.Parse(displayId);

                        if (!ActiveDisplayConfig.ColorConfig.ColorData.ContainsKey(displayId))
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Display {displayId} doesn't exist in this setup, so skipping setting the NVIDIA Color Settings.");
                            continue;
                        }

                        // If this is a setting that says it uses user colour settings, then we turn it off
                        if (ActiveDisplayConfig.ColorConfig.ColorData[displayId].ColorSelectionPolicy != colorData.ColorSelectionPolicy)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to use custom NVIDIA HDR Colour for display {displayId}.");

                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want the standard colour settings to be {displayConfig.ColorConfig.ColorData[displayId].ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}.");
                            colorData = displayConfig.ColorConfig.ColorData[displayId];

                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want the standard colour settings to be {displayConfig.ColorConfig.ColorData[displayId].ColorSelectionPolicy.ToString("G")} and they are {ActiveDisplayConfig.ColorConfig.ColorData[displayId].ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn off standard colour mode for Mosaic display {displayId}.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings Color selection policy {colorData.ColorSelectionPolicy.ToString("G")} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings BPC {colorData.Bpc} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings colour format {colorData.ColorFormat} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings colourimetry {colorData.Colorimetry} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings colour depth {colorData.Depth} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want standard colour settings dynamic range {colorData.DynamicRange} for Mosaic display {displayId}");

                            // Set the command as a 'SET'
                            colorData.Cmd = NV_COLOR_CMD.NV_COLOR_CMD_SET;
                            NVStatus = NVImport.NvAPI_Disp_ColorControl(displayIdAsUInt32, ref colorData);
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
                }


                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn on NVIDIA HDR colour if it's user wants to use NVIDIA HDR colour.");
                // Now we try to set each display color
                foreach (var hdrColorDataDict in displayConfig.HdrConfig.HdrColorData)
                {
                    NV_HDR_COLOR_DATA_V2 hdrColorData = hdrColorDataDict.Value;
                    string displayId = hdrColorDataDict.Key;

                    try
                    {
                        UInt32 displayIdAsUInt32 = UInt32.Parse(displayId);

                        if (!ActiveDisplayConfig.HdrConfig.HdrColorData.ContainsKey(displayId))
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Display {displayId} doesn't exist in this setup, so skipping setting the HdrColorData.");
                            continue;
                        }

                        // if it's HDR and it's a different mode than what we are in now, then set HDR
                        if (ActiveDisplayConfig.HdrConfig.HdrColorData[displayId].HdrMode != hdrColorData.HdrMode)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to turn on user-set HDR mode for display {displayId} as it's supposed to be on.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: HDR mode is currently {ActiveDisplayConfig.HdrConfig.HdrColorData[displayId].HdrMode.ToString("G")} for Mosaic display {displayId}.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings BPC  {hdrColorData.HdrBpc} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings HDR Colour Format {hdrColorData.HdrColorFormat} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings HDR dynamic range {hdrColorData.HdrDynamicRange} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings HDR Mode {hdrColorData.HdrMode} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings Mastering Display Data {hdrColorData.MasteringDisplayData} for Mosaic display {displayId}");
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want HDR settings Static Meradata Description ID {hdrColorData.StaticMetadataDescriptorId} for Mosaic display {displayId}");
                            // Apply the HDR removal
                            hdrColorData.Cmd = NV_HDR_CMD.CMD_SET;
                            NVStatus = NVImport.NvAPI_Disp_HdrColorControl(displayIdAsUInt32, ref hdrColorData);
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