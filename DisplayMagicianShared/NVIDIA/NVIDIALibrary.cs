using DisplayMagicianShared.Windows;
using NVAPIWrapper;
using EDIDParser;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Windows.Devices.I2c.Provider;
using Windows.Graphics;
using Windows.Storage.Provider;

namespace DisplayMagicianShared.NVIDIA
{

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_MOSAIC_CONFIG : IEquatable<NVIDIA_MOSAIC_CONFIG>
    {
        public bool IsMosaicEnabled;
        public NVAPIMosaicCurrentTopoDto MosaicCurrentTopo;
        public NVAPIMosaicGridTopologiesDto MosaicGridTopologies;
        //public _NV_MOSAIC_GRID_TOPO_V2[] MosaicGridTopos;
        //public UInt32 MosaicGridCount;

        public NVIDIA_MOSAIC_CONFIG()
        {
            IsMosaicEnabled = false;
            MosaicCurrentTopo = new NVAPIMosaicCurrentTopoDto();
            MosaicGridTopologies = new NVAPIMosaicGridTopologiesDto();
            //MosaicGridTopos = new _NV_MOSAIC_GRID_TOPO_V2[] { };
            //MosaicGridCount = 0;
        }

        public override bool Equals(object obj) => obj is NVIDIA_MOSAIC_CONFIG other && this.Equals(other);

        public bool Equals(NVIDIA_MOSAIC_CONFIG other)
        {
            try
            {
                if (IsMosaicEnabled != other.IsMosaicEnabled)
                {
                    SharedLogger.logger.Debug($"NVIDIA_MOSAIC_CONFIG/Equals: The IsMosaicEnabled fields don't match!");
                    return false;
                }
                if (!MosaicCurrentTopo.Equals(other.MosaicCurrentTopo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_MOSAIC_CONFIG/Equals: The MosaicCurrentTopo structs don't match!");
                    return false;
                }
                if (!MosaicGridTopologies.Equals(other.MosaicGridTopologies))
                {
                    SharedLogger.logger.Debug($"NVIDIA_MOSAIC_CONFIG/Equals: The MosaicGridTopologies structs don't match!");
                    return false;
                }

                // if (!MosaicTopologyBrief.Equals(other.MosaicTopologyBrief))
                // {
                //     SharedLogger.logger.Debug($"NVIDIA_MOSAIC_CONFIG/Equals: The MosaicTopologyBrief structs don't match!");
                //     return false;
                // }
                // if (!MosaicDisplaySettings.Equals(other.MosaicDisplaySettings))
                // {
                //     SharedLogger.logger.Debug($"NVIDIA_MOSAIC_CONFIG/Equals: The MosaicDisplaySettings structs don't match!");
                //     return false;
                // }
                // if (OverlapX != other.OverlapX)
                // {
                //     SharedLogger.logger.Debug($"NVIDIA_MOSAIC_CONFIG/Equals: The OverlapX fields don't match!");
                //     return false;
                // }
                // if (OverlapY != other.OverlapY)
                // {
                //     SharedLogger.logger.Debug($"NVIDIA_MOSAIC_CONFIG/Equals: The OverlapY fields don't match!");
                //     return false;
                // }
                // if (!MosaicGridTopos.SequenceEqual(other.MosaicGridTopos))
                // {
                //     SharedLogger.logger.Debug($"NVIDIA_MOSAIC_CONFIG/Equals: The MosaicGridTopos struct arrays don't match!");
                //     return false;
                // }
                // if (MosaicGridCount != other.MosaicGridCount)
                // {
                //     SharedLogger.logger.Debug($"NVIDIA_MOSAIC_CONFIG/Equals: The MosaicGridCount fields don't match!");
                //     return false;
                // }
                // If we make it here then the two configs are equal
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"NVIDIA_MOSAIC_CONFIG/Equals: Exception comparing the NVIDIA Mosaic Configs");
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (IsMosaicEnabled, MosaicCurrentTopo, MosaicGridTopologies).GetHashCode();
        }
        public static bool operator ==(NVIDIA_MOSAIC_CONFIG lhs, NVIDIA_MOSAIC_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_MOSAIC_CONFIG lhs, NVIDIA_MOSAIC_CONFIG rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_PER_DISPLAY_CONFIG : IEquatable<NVIDIA_PER_DISPLAY_CONFIG>
    {
        public uint DisplayId;
        public NV_MONITOR_CONN_TYPE ConnectorType;        

        // HDR capabilities and color data
        public bool HasNvHdrEnabled;
        public NVAPIHdrCapabilitiesDto HdrCapabilities;
        public NVAPIHdrColorDataDto HdrColorData;

        // Adaptive Sync
        public bool HasAdaptiveSync;
        public NVAPIAdaptiveSyncSetDataDto AdaptiveSyncConfig;

        // Color data
        public bool HasColorData;
        public NVAPIDisplayColorDataDto ColorData;

        // Custom display
        public bool HasCustomDisplay;
        public List<NVAPICustomDisplayDto> CustomDisplays;

        // DisplayPort info
        public bool HasDisplayPortInfo;
        public NVAPIDisplayPortInfoDto DisplayPortInfo;

        // Virtual Refresh Rate
        public bool HasVirtualRefreshRate;
        public NVAPIVirtualRefreshRateDataDto VirtualRefreshRateData;

        // Preferred Stereo Display
        public bool HasPreferredStereoDisplay;
        public NVAPIPreferredStereoDisplayDto PreferredStereoDisplay;

        // Source Color Space
        public bool HasSourceColorSpace;
        public _NV_COLORSPACE_TYPE SourceColorSpace;

        // Source HDR Metadata
        public bool HasSourceHdrMetadata;
        public NVAPIHdrMetadataDto SourceHdrMetadata;

        // Output Mode
        public bool HasOutputMode;
        public _NV_DISPLAY_OUTPUT_MODE OutputMode;

        // HDR Tone Mapping
        public bool HasHdrToneMapping;
        public _NV_HDR_TONEMAPPING_METHOD HdrToneMapping;

        // InfoFrame Data
        public bool HasInfoFrameData;
        public NVAPIInfoFrameDataDto InfoFrameData;

        // Display status properties
        public bool IsActive;
        public bool IsConnected;
        public bool IsPhysicallyConnected;
        public bool IsCluster;
        public bool IsDynamic;
        public bool IsMultiStreamRootNode;
        public bool IsOSVisible;
        public bool IsWfd;

        // Monitor Capabilities
        public bool HasMonitorCapabilities;
        public NVAPIMonitorCapabilitiesDto MonitorCapabilities;

        // Monitor Color Capabilities
        public bool HasMonitorColorCapabilities;
        public NVAPIMonitorColorCapabilitiesDto MonitorColorCapabilities;

        // HDMI Support Info
        public bool HasHdmiSupportInfo;
        public NVAPIDisplayHdmiSupportInfoDto HdmiSupportInfo;

        // VRR Info
        public bool HasVrrInfo;
        public NVAPIVrrInfoDto VrrInfo;

        // Display Colorimetry
        public bool HasDisplayColorimetry;
        public NVAPIDisplayColorimetryDto DisplayColorimetry;

        // Display ID Info
        public bool HasDisplayIdInfo;
        public NVAPIDisplayIdInfoDto DisplayIdInfo;

        // Timing
        public bool HasTiming;
        public NVAPITimingDto Timing;

        // Scanout Configuration
        public bool HasScanoutConfiguration;
        public NVAPIGpuScanoutConfigurationDto ScanoutConfiguration;
        
        public NVIDIA_PER_DISPLAY_CONFIG()
        {
            HasNvHdrEnabled = false;
            HdrCapabilities = new NVAPIHdrCapabilitiesDto();
            HdrColorData = new NVAPIHdrColorDataDto();
            HasAdaptiveSync = false;
            AdaptiveSyncConfig = new NVAPIAdaptiveSyncSetDataDto();
            HasColorData = false;
            ColorData = new NVAPIDisplayColorDataDto();
            HasCustomDisplay = false;
            CustomDisplays = new List<NVAPICustomDisplayDto>();
            HasDisplayPortInfo = false;
            DisplayPortInfo = new NVAPIDisplayPortInfoDto();
            HasVirtualRefreshRate = false;
            VirtualRefreshRateData = new NVAPIVirtualRefreshRateDataDto();
            HasPreferredStereoDisplay = false;
            PreferredStereoDisplay = new NVAPIPreferredStereoDisplayDto();
            HasSourceColorSpace = false;
            SourceColorSpace = default;
            HasSourceHdrMetadata = false;
            SourceHdrMetadata = new NVAPIHdrMetadataDto();
            HasOutputMode = false;
            OutputMode = default;
            HasHdrToneMapping = false;
            HdrToneMapping = default;
            HasInfoFrameData = false;
            InfoFrameData = new NVAPIInfoFrameDataDto();
            IsActive = false;
            IsConnected = false;
            IsPhysicallyConnected = false;
            IsCluster = false;
            IsDynamic = false;
            IsMultiStreamRootNode = false;
            IsOSVisible = false;
            IsWfd = false;
            HasMonitorCapabilities = false;
            MonitorCapabilities = new NVAPIMonitorCapabilitiesDto();
            HasMonitorColorCapabilities = false;
            MonitorColorCapabilities = new NVAPIMonitorColorCapabilitiesDto();
            HasHdmiSupportInfo = false;
            HdmiSupportInfo = new NVAPIDisplayHdmiSupportInfoDto();
            HasVrrInfo = false;
            VrrInfo = new NVAPIVrrInfoDto();
            HasDisplayColorimetry = false;
            DisplayColorimetry = new NVAPIDisplayColorimetryDto();
            HasDisplayIdInfo = false;
            DisplayIdInfo = new NVAPIDisplayIdInfoDto();
            HasTiming = false;
            Timing = new NVAPITimingDto();
            HasScanoutConfiguration = false;
            ScanoutConfiguration = new NVAPIGpuScanoutConfigurationDto();
        }

        public override bool Equals(object obj) => obj is NVIDIA_PER_DISPLAY_CONFIG other && this.Equals(other);

        public bool Equals(NVIDIA_PER_DISPLAY_CONFIG other)
        {
            try
            {
                if (HasNvHdrEnabled != other.HasNvHdrEnabled)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasNvHdrEnabled fields don't match!");
                    return false;
                }

                if (!HdrCapabilities.Equals(other.HdrCapabilities))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HdrCapabilities structs don't match!");
                    return false;
                }

                if (!HdrColorData.Equals(other.HdrColorData))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HdrColorData structs don't match!");
                    return false;
                }

                // Disabled the Adaptive Sync equality matching as we are having trouble applying it
                /*
                if (HasAdaptiveSync != other.HasAdaptiveSync)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasAdaptiveSync fields don't match!");
                    return false;
                }
                
                if (!AdaptiveSyncConfig.Equals(other.AdaptiveSyncConfig))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The AdaptiveSyncConfig structs don't match!");
                    return false;
                }
                */

                if (HasColorData != other.HasColorData)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasColorData fields don't match!");
                    return false;
                }

                if (!ColorData.Equals(other.ColorData))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The ColorData structs don't match!");
                    return false;
                }

                if (HasCustomDisplay != other.HasCustomDisplay)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasCustomDisplay fields don't match!");
                    return false;
                }

                if (!CustomDisplays.SequenceEqual(other.CustomDisplays))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The CustomDisplays lists don't match!");
                    return false;
                }

                if (HasDisplayPortInfo != other.HasDisplayPortInfo)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasDisplayPortInfo fields don't match!");
                    return false;
                }

                if (!DisplayPortInfo.Equals(other.DisplayPortInfo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The DisplayPortInfo structs don't match!");
                    return false;
                }

                if (HasVirtualRefreshRate != other.HasVirtualRefreshRate)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasVirtualRefreshRate fields don't match!");
                    return false;
                }

                if (!VirtualRefreshRateData.Equals(other.VirtualRefreshRateData))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The VirtualRefreshRateData structs don't match!");
                    return false;
                }

                if (HasPreferredStereoDisplay != other.HasPreferredStereoDisplay)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasPreferredStereoDisplay fields don't match!");
                    return false;
                }

                if (!PreferredStereoDisplay.Equals(other.PreferredStereoDisplay))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The PreferredStereoDisplay structs don't match!");
                    return false;
                }

                if (HasSourceColorSpace != other.HasSourceColorSpace)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasSourceColorSpace fields don't match!");
                    return false;
                }

                if (!SourceColorSpace.Equals(other.SourceColorSpace))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The SourceColorSpace fields don't match!");
                    return false;
                }

                if (HasSourceHdrMetadata != other.HasSourceHdrMetadata)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasSourceHdrMetadata fields don't match!");
                    return false;
                }

                if (!SourceHdrMetadata.Equals(other.SourceHdrMetadata))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The SourceHdrMetadata structs don't match!");
                    return false;
                }

                if (HasOutputMode != other.HasOutputMode)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasOutputMode fields don't match!");
                    return false;
                }

                if (!OutputMode.Equals(other.OutputMode))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The OutputMode fields don't match!");
                    return false;
                }

                if (HasHdrToneMapping != other.HasHdrToneMapping)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasHdrToneMapping fields don't match!");
                    return false;
                }

                if (!HdrToneMapping.Equals(other.HdrToneMapping))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HdrToneMapping fields don't match!");
                    return false;
                }

                if (HasInfoFrameData != other.HasInfoFrameData)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasInfoFrameData fields don't match!");
                    return false;
                }

                if (!InfoFrameData.Equals(other.InfoFrameData))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The InfoFrameData structs don't match!");
                    return false;
                }

                if (IsActive != other.IsActive)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The IsActive fields don't match!");
                    return false;
                }

                if (IsConnected != other.IsConnected)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The IsConnected fields don't match!");
                    return false;
                }

                if (IsPhysicallyConnected != other.IsPhysicallyConnected)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The IsPhysicallyConnected fields don't match!");
                    return false;
                }

                if (IsCluster != other.IsCluster)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The IsCluster fields don't match!");
                    return false;
                }

                if (IsDynamic != other.IsDynamic)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The IsDynamic fields don't match!");
                    return false;
                }

                if (IsMultiStreamRootNode != other.IsMultiStreamRootNode)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The IsMultiStreamRootNode fields don't match!");
                    return false;
                }

                if (IsOSVisible != other.IsOSVisible)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The IsOSVisible fields don't match!");
                    return false;
                }

                if (IsWfd != other.IsWfd)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The IsWfd fields don't match!");
                    return false;
                }

                if (HasMonitorCapabilities != other.HasMonitorCapabilities)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasMonitorCapabilities fields don't match!");
                    return false;
                }

                if (!MonitorCapabilities.Equals(other.MonitorCapabilities))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The MonitorCapabilities structs don't match!");
                    return false;
                }

                if (HasMonitorColorCapabilities != other.HasMonitorColorCapabilities)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasMonitorColorCapabilities fields don't match!");
                    return false;
                }

                if (!MonitorColorCapabilities.Equals(other.MonitorColorCapabilities))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The MonitorColorCapabilities structs don't match!");
                    return false;
                }

                if (HasHdmiSupportInfo != other.HasHdmiSupportInfo)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasHdmiSupportInfo fields don't match!");
                    return false;
                }

                if (!HdmiSupportInfo.Equals(other.HdmiSupportInfo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HdmiSupportInfo structs don't match!");
                    return false;
                }

                if (HasVrrInfo != other.HasVrrInfo)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasVrrInfo fields don't match!");
                    return false;
                }

                if (!VrrInfo.Equals(other.VrrInfo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The VrrInfo structs don't match!");
                    return false;
                }

                if (HasDisplayColorimetry != other.HasDisplayColorimetry)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasDisplayColorimetry fields don't match!");
                    return false;
                }

                if (!DisplayColorimetry.Equals(other.DisplayColorimetry))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The DisplayColorimetry structs don't match!");
                    return false;
                }

                if (HasDisplayIdInfo != other.HasDisplayIdInfo)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasDisplayIdInfo fields don't match!");
                    return false;
                }

                if (!DisplayIdInfo.Equals(other.DisplayIdInfo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The DisplayIdInfo structs don't match!");
                    return false;
                }

                if (HasTiming != other.HasTiming)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasTiming fields don't match!");
                    return false;
                }

                if (!Timing.Equals(other.Timing))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The Timing structs don't match!");
                    return false;
                }

                if (HasScanoutConfiguration != other.HasScanoutConfiguration)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The HasScanoutConfiguration fields don't match!");
                    return false;
                }

                if (!ScanoutConfiguration.Equals(other.ScanoutConfiguration))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_DISPLAY_CONFIG/Equals: The ScanoutConfiguration structs don't match!");
                    return false;
                }

                // If we make it here then the two configs are equal
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"NVIDIA_PER_DISPLAY_CONFIG/Equals: Exception comparing the NVIDIA Per Display Configs");
                return false;
            }
        }

        public override int GetHashCode()
        {
            // Disabled the Adaptive Sync equality matching as we are having trouble applying it, which is causing issues in profile matching in DisplayMagician
            // To fix this bit, we need to test the SetActiveConfigOverride Adaptive Sync part of the codebase to apply this properly.
            // But for now, we'll exclude it from the equality matching and also stop trying to use the adaptive sync config.
            //return (HasNvHdrEnabled, HdrCapabilities, HdrColorData, HasAdaptiveSync, AdaptiveSyncConfig, HasColorData, ColorData, HasCustomDisplay, CustomDisplays).GetHashCode();
            return (HasNvHdrEnabled, HdrCapabilities, HdrColorData, HasColorData, ColorData, HasCustomDisplay, CustomDisplays, 
                HasDisplayPortInfo, DisplayPortInfo, HasVirtualRefreshRate, VirtualRefreshRateData, HasPreferredStereoDisplay, PreferredStereoDisplay,
                HasSourceColorSpace, SourceColorSpace, HasSourceHdrMetadata, SourceHdrMetadata, HasOutputMode, OutputMode, 
                HasHdrToneMapping, HdrToneMapping, HasInfoFrameData, InfoFrameData,
                IsActive, IsConnected, IsPhysicallyConnected, IsCluster, IsDynamic, IsMultiStreamRootNode, IsOSVisible, IsWfd,
                HasMonitorCapabilities, MonitorCapabilities, HasMonitorColorCapabilities, MonitorColorCapabilities,
                HasHdmiSupportInfo, HdmiSupportInfo, HasVrrInfo, VrrInfo,
                HasDisplayColorimetry, DisplayColorimetry, HasDisplayIdInfo, DisplayIdInfo,
                HasTiming, Timing, HasScanoutConfiguration, ScanoutConfiguration).GetHashCode();
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

    public struct NVIDIA_DRS_CONFIG : IEquatable<NVIDIA_DRS_CONFIG>
    {
        //public bool HasDRSSettings;
        public bool IsBaseProfile;
        public NVAPIDrsProfileDto ProfileInfo;
        public List<NVAPIDrsSettingDto> DriverSettings;

        public NVIDIA_DRS_CONFIG()
        {
            //HasDRSSettings = false;
            IsBaseProfile = false;
            ProfileInfo = new NVAPIDrsProfileDto();
            DriverSettings = new List<NVAPIDrsSettingDto>();
        }

        public override bool Equals(object obj) => obj is NVIDIA_DRS_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_DRS_CONFIG other)
        {
            try
            {
                if (IsBaseProfile != other.IsBaseProfile)
                {
                    SharedLogger.logger.Debug($"NVIDIA_DRS_CONFIG/Equals: The IsBaseProfile fields don't match!");
                    return false;
                }

                if (!ProfileInfo.Equals(other.ProfileInfo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_DRS_CONFIG/Equals: The ProfileInfo structs don't match!");
                    return false;
                }

                if (!DriverSettings.SequenceEqual(other.DriverSettings))
                {
                    SharedLogger.logger.Debug($"NVIDIA_DRS_CONFIG/Equals: The DriverSettings lists don't match!");
                    return false;
                }

                // If we make it here then the two configs are equal
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"NVIDIA_DRS_CONFIG/Equals: Exception comparing the NVIDIA DRS Configs");
                return false;
            }
        }


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
        public string AdapterID;
        public bool IsQuadro;
        public bool HasLogicalGPU;
        public NV_SYSTEM_TYPE SystemType;        
        public string FullName;
        public _NV_GPU_TYPE GPUType;
        public _NV_GPU_BUS_TYPE BusType;
        public uint BusId;
        public uint BusSlotId;
        public NVAPIPciIdentifiers PciIdentifiers;

        public string VbiosVersionString;
        public NVAPINvLinkStatusDto NvLinkStatus;

        public NVAPIGpuInfoDto GpuInfo;

        public NVAPIGpuEccConfigurationInfoDto EccConfigurationInfo;

        public NVAPIDisplayConfigDto DisplayConfig;

        public NVAPIGpuArchInfoDto ArchInfo;

        public NVAPIGpuBoardInfoDto BoardInfo;

        public NVAPIGpuHdcpSupportStatusDto HdcpSupportStatus;

        public int DisplayCount;
        public Dictionary<string, NVIDIA_PER_DISPLAY_CONFIG> Displays;

        public NVIDIA_PER_ADAPTER_CONFIG()
        {
            AdapterID = string.Empty;
            IsQuadro = false;
            HasLogicalGPU = false;
            SystemType = NV_SYSTEM_TYPE.NV_SYSTEM_TYPE_UNKNOWN;
            FullName = string.Empty;
            GPUType = _NV_GPU_TYPE.NV_SYSTEM_TYPE_GPU_UNKNOWN;
            BusType = _NV_GPU_BUS_TYPE.NVAPI_GPU_BUS_TYPE_UNDEFINED;
            BusId = 0;
            BusSlotId = 0;
            PciIdentifiers = new NVAPIPciIdentifiers();
            VbiosVersionString = string.Empty;
            NvLinkStatus = new NVAPINvLinkStatusDto();
            GpuInfo = new NVAPIGpuInfoDto();
            EccConfigurationInfo = new NVAPIGpuEccConfigurationInfoDto();
            DisplayConfig = new NVAPIDisplayConfigDto();
            ArchInfo = new NVAPIGpuArchInfoDto();
            BoardInfo = new NVAPIGpuBoardInfoDto();
            HdcpSupportStatus = new NVAPIGpuHdcpSupportStatusDto();
            DisplayCount = 0;
            Displays = new Dictionary<string, NVIDIA_PER_DISPLAY_CONFIG>();
        }

        public override bool Equals(object obj) => obj is NVIDIA_PER_ADAPTER_CONFIG other && this.Equals(other);
        public bool Equals(NVIDIA_PER_ADAPTER_CONFIG other)
        {
            try
            {
                if (IsQuadro != other.IsQuadro)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The IsQuadro fields don't match!");
                    return false;
                }

                if (HasLogicalGPU != other.HasLogicalGPU)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The HasLogicalGPU fields don't match!");
                    return false;
                }

                if (SystemType != other.SystemType)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The SystemType fields don't match!");
                    return false;
                }

                if (!FullName.Equals(other.FullName))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The AdapterName fields don't match!");
                    return false;
                }

                if (GPUType != other.GPUType)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The GPUType fields don't match!");
                    return false;
                }

                if (BusType != other.BusType)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The BusType fields don't match!");
                    return false;
                }

                if (BusId != other.BusId)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The BusId fields don't match!");
                    return false;
                }

                if (BusSlotId != other.BusSlotId)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The BusSlotId fields don't match!");
                    return false;
                }
                if (!PciIdentifiers.Equals(other.PciIdentifiers))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The PciIdentifiers structs don't match!");
                    return false;
                }
                if (!GpuInfo.Equals(other.GpuInfo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The GpuInfo structs don't match!");
                    return false;
                }
                if (!EccConfigurationInfo.Equals(other.EccConfigurationInfo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The EccConfigurationInfo structs don't match!");
                    return false;
                }
                if (VbiosVersionString != other.VbiosVersionString)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The VbiosVersionString fields don't match!");
                    return false;
                }
                if (!NvLinkStatus.Equals(other.NvLinkStatus))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The NvLinkStatus structs don't match!");
                    return false;
                }
                // Broken needs fixing!
                if (!DisplayConfig.Equals(other.DisplayConfig))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The DisplayConfig structs don't match!");
                    return false;
                }
                if (!ArchInfo.Equals(other.ArchInfo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The ArchInfo structs don't match!");
                    return false;
                }
                // Broken needs fixing!
                if (!BoardInfo.Equals(other.BoardInfo))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The BoardInfo structs don't match!");
                    return false;
                }
                if (!HdcpSupportStatus.Equals(other.HdcpSupportStatus))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The HdcpSupportStatus structs don't match!");
                    return false;
                }
                if (DisplayCount != other.DisplayCount)
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The DisplayCount fields don't match!");
                    return false;
                }

                if (!CollectionComparer.AreEquivalent(Displays, other.Displays))
                {
                    SharedLogger.logger.Debug($"NVIDIA_PER_ADAPTER_CONFIG/Equals: The Displays dictionaries don't match!");
                    return false;
                }

                // If we make it here then the two configs are equal
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"NVIDIA_PER_ADAPTER_CONFIG/Equals: Exception comparing the NVIDIA Per Adapter Configs");
                return false;
            }
        }


        public override int GetHashCode()
        {
            return (IsQuadro, HasLogicalGPU, SystemType, FullName, GPUType, BusType, BusId, BusSlotId, DisplayConfig, ArchInfo, BoardInfo, HdcpSupportStatus, DisplayCount, Displays).GetHashCode();
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
        public Dictionary<string, NVIDIA_PER_ADAPTER_CONFIG> PhysicalAdapters;
        public List<NVIDIA_DRS_CONFIG> DRSSettings;
        // Note: We purposely have left out the DisplayNames from the Equals as it's order keeps changing after each reboot and after each profile swap
        // and it is informational only and doesn't contribute to the configuration (it's used for generating the Screens structure, and therefore for
        // generating the profile icon.
        public Dictionary<string, string> DisplayNames;
        public List<string> DisplayIdentifiers;
        public int DisplayCount;

        public NVIDIA_DISPLAY_CONFIG()
        {
            IsInUse = false;
            IsCloned = false;
            MosaicConfig = new NVIDIA_MOSAIC_CONFIG();
            PhysicalAdapters = new Dictionary<string, NVIDIA_PER_ADAPTER_CONFIG>();
            DRSSettings = new List<NVIDIA_DRS_CONFIG>();
            DisplayNames = new Dictionary<string, string>();
            DisplayIdentifiers = new List<string>();
            DisplayCount = 0;
        }

        public override bool Equals(object obj) => obj is NVIDIA_DISPLAY_CONFIG other && this.Equals(other);

        public bool Equals(NVIDIA_DISPLAY_CONFIG other)
        {
            try
            {
                if (IsInUse != other.IsInUse)
                {
                    SharedLogger.logger.Debug($"NVIDIA_DISPLAY_CONFIG/Equals: The IsInUse fields don't match!");
                    return false;
                }

                if (IsCloned != other.IsCloned)
                {
                    SharedLogger.logger.Debug($"NVIDIA_DISPLAY_CONFIG/Equals: The IsCloned fields don't match!");
                    return false;
                }

                if (!PhysicalAdapters.SequenceEqual(other.PhysicalAdapters))
                {
                    SharedLogger.logger.Debug($"NVIDIA_DISPLAY_CONFIG/Equals: The PhysicalAdapters dictionaries don't match!");
                    return false;
                }

                if (!MosaicConfig.Equals(other.MosaicConfig))
                {
                    SharedLogger.logger.Debug($"NVIDIA_DISPLAY_CONFIG/Equals: The MosaicConfig structs don't match!");
                    return false;
                }

                if (!DRSSettings.SequenceEqual(other.DRSSettings))
                {
                    SharedLogger.logger.Debug($"NVIDIA_DISPLAY_CONFIG/Equals: The DRSSettings lists don't match!");
                    return false;
                }

                if (!DisplayIdentifiers.SequenceEqual(other.DisplayIdentifiers))
                {
                    SharedLogger.logger.Debug($"NVIDIA_DISPLAY_CONFIG/Equals: The DisplayIdentifiers lists don't match!");
                    return false;
                }
                if (DisplayCount != other.DisplayCount)
                {
                    SharedLogger.logger.Debug($"NVIDIA_DISPLAY_CONFIG/Equals: The DisplayCount fields don't match!");
                    return false;
                }

                // If we make it here then the two configs are equal
                return true;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"NVIDIA_DISPLAY_CONFIG/Equals: Exception comparing the NVIDIA Display Configs");
                return false;
            }
        }


        public override int GetHashCode()
        {
            return (IsInUse, IsCloned, MosaicConfig, PhysicalAdapters, DisplayIdentifiers, DRSSettings, DisplayCount).GetHashCode();
        }
        public static bool operator ==(NVIDIA_DISPLAY_CONFIG lhs, NVIDIA_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVIDIA_DISPLAY_CONFIG lhs, NVIDIA_DISPLAY_CONFIG rhs) => !(lhs == rhs);
    }

    public class NVIDIALibrary : IDisposable
    {
    
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string dllToLoad);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
        
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static NVIDIALibrary _instance = new NVIDIALibrary();

        private bool _initialised = false;
        private NVIDIA_DISPLAY_CONFIG? _activeDisplayConfig;
        public List<string> _allConnectedDisplayIdentifiers;
        public List<uint> _allConnectedDisplayIds = new List<uint>();

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

        // NVAPI API Handle
        private NVAPIApiHelper _nvapiApiHelper;

        public IntPtr hNVAPIModule = IntPtr.Zero;
        public const string NVIDIA_NVAPI_DLL = "nvapi64.dll";
        public IntPtr hNVAPIBindingModule = IntPtr.Zero;
        public const string NVIDIA_NVAPI_BINDING_DLL = "NVAPIWrapper.dll";

        static NVIDIALibrary() { }
        public NVIDIALibrary()
        {
            _activeDisplayConfig = CreateDefaultConfig();
            _allConnectedDisplayIdentifiers = new List<string>();

            try
            {
                _initialised = false;
                SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Looking for NVIDIA PCI hardware...");
                // Check if there is NVIDIA hardware installed
                if (WinLibrary.IsPCIVideoCardVendorInstalled(PCIVendorIDs))
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: NVIDIA hardware detected");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: No NVIDIA hardware detected");
                    return;
                }

                 // Confirm the NVAPI DLL is available before attempting to initialise
                if (!NVAPIApiHelper.IsNVAPIDllAvailable(out string dllError))
                {
                    _initialised = false;
                    SharedLogger.logger.Error($"NVIDIALibrary/NVIDIALibrary: Failed to load the NVIDIA NVAPI DLL. {dllError}");
                    return;
                }

                 SharedLogger.logger.Trace("NVIDIALibrary/NVIDIALibrary: Intialising NVIDIA NVAPI library interface");
                _nvapiApiHelper = NVAPIApiHelper.Initialize();
                if (_nvapiApiHelper == null)
                {
                    _initialised = false;
                    SharedLogger.logger.Error("NVIDIALibrary/NVIDIALibrary: Failed to initialise NVIDIA NVAPI helper.");
                    return;
                }
                _initialised = true;
                SharedLogger.logger.Trace("NVIDIALibrary/NVIDIALibrary: Successfully initialised NVIDIA NVAPI helper.");               
            }
            catch (TypeInitializationException ex)
            {
                SharedLogger.logger.Info(ex, $"NVIDIALibrary/NVIDIALibrary: TypeInitializationException trying to load the NVIDIA NVAPI DLL {NVIDIA_NVAPI_DLL}. This generally means you don't have the NVIDIA NVAPI driver installed.");
                _initialised = false;
                return;
            }
            catch (DllNotFoundException ex)
            {
                // If we get here then the NVIDIA NVAPI DLL wasn't found. We can't continue to use it, so we log the error and exit
                SharedLogger.logger.Info(ex, $"NVIDIALibrary/NVIDIALibrary: DllNotFoundException trying to load the NVIDIA NVAPI DLL {NVIDIA_NVAPI_DLL}. This generally means you don't have the NVIDIA NVAPI driver installed.");
                _initialised = false;
                return;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Info(ex, $"NVIDIALibrary/NVIDIALibrary: A general exception trying to load the NVIDIA NVAPI DLL {NVIDIA_NVAPI_DLL}.");
                _initialised = false;
                return;
            }
            SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Automatically getting the NVIDIA Display Configuration");
            _activeDisplayConfig = GetActiveConfig();

            // If we failed to get the display config, then we can't continue to use the library, so we dispose of it to avoid memory leaks and exit
            if (_activeDisplayConfig == null)
            {
                _activeDisplayConfig = CreateDefaultConfig();
                SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: The active NVIDIA Display Configuration is null. Disposing the NVAPIHelper to avoid memory leaks");
                _nvapiApiHelper.Dispose();
                SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Setting NVAPIHelper to null");
                _nvapiApiHelper = null;
                _initialised = false;
                return;
            }

            // If we got a display config, but there are no displays, then we continue
            if (_activeDisplayConfig.Value.DisplayCount == 0 )
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: No displays connected to the NVIDIA GPU, so returning an empty display configuration");
                return;
            }

            SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Automatically getting the NVIDIA Connected Display Identifiers");
            _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers(out bool failure);
        
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

            // Free unmanaged resources (unmanaged objects) and override finalizer.
            if (_nvapiApiHelper != null)
            {
                _nvapiApiHelper.Dispose();
                _nvapiApiHelper = null;
            }


            if (hNVAPIModule != IntPtr.Zero)
            {
                SharedLogger.logger.Trace("NVIDIALibrary/Dispose: Freeing the NVIDIA NVAPI DLL");
                // Disabling as probably not needed now
                // FreeLibrary(hNVAPIModule);
                hNVAPIModule = IntPtr.Zero;
            }

            _initialised = false;
            _disposed = true;
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
                return new List<string>() { "10DE" };
            }
        }

        public NVIDIA_DISPLAY_CONFIG ActiveDisplayConfig
        {
            get
            {
                if (_activeDisplayConfig == null)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/ActiveDisplayConfig: ActiveDisplayConfig is null, so creating a new one");
                    _activeDisplayConfig = CreateDefaultConfig();
                }                    
                return _activeDisplayConfig.Value;
            }
        }

        public List<string> CurrentDisplayIdentifiers
        {
            get
            {
                if (_activeDisplayConfig == null)
                    _activeDisplayConfig = CreateDefaultConfig();
                return _activeDisplayConfig.Value.DisplayIdentifiers;
            }
        }

        public static void KeepVideoCardOn()
        {
            LoadLibrary("NVIDIAExportsDLL.dll");
        }

        public static NVIDIALibrary GetLibrary()
        {

            if (_instance == null)
            {
                _instance = new NVIDIALibrary();
            }

            return _instance;
        }

        public static void Shutdown()
        {
            if (_instance == null)
            {
                return;
            }

            _instance.Dispose();
            _instance = null;
        }

        public NVIDIA_DISPLAY_CONFIG CreateDefaultConfig()
        {
            NVIDIA_DISPLAY_CONFIG myDefaultConfig = new NVIDIA_DISPLAY_CONFIG();

            // Fill in the minimal amount we need to avoid null references
            // so that we won't break json.net when we save a default config

            myDefaultConfig.IsInUse = false;

            // THIS IS ALL TAKEN CARE OF IN THE STRUCT CONSTRUCTORS NOW \o/ yay!
            myDefaultConfig.MosaicConfig.IsMosaicEnabled = false;
            myDefaultConfig.PhysicalAdapters = new Dictionary<string, NVIDIA_PER_ADAPTER_CONFIG>();
            myDefaultConfig.DRSSettings = new List<NVIDIA_DRS_CONFIG>();
            myDefaultConfig.DisplayNames = new Dictionary<string, string>();
            myDefaultConfig.DisplayIdentifiers = new List<string>();
            myDefaultConfig.IsCloned = false;
            myDefaultConfig.IsInUse = false;
            myDefaultConfig.DisplayCount = 0;


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

            if (_initialised && _nvapiApiHelper != null)
            {
                // Enumerate the NVIDIA GPUs adapters in the sytem
                NVAPIPhysicalGpuHelper[] adapters;
                int adapterTotalCount = 0;
                int adapterNum = 0;
                try
                {
                    // Enumerate all the Physical GPUs
                    adapters = _nvapiApiHelper.EnumeratePhysicalGpus();
                    adapterTotalCount = adapters.Length;
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: EnumeratePhysicalGpus returned {adapterTotalCount} Physical GPUs");

                    // This check is to make sure that we only continue in this function if there are physical GPUs to actually do anything with
                    // If the driver is installed, but not physical GPUs are present then we just want to return a default blank config.
                    if (adapterTotalCount == 0)
                    {
                        // Return the default config
                        return CreateDefaultConfig();
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex,$"NVIDIALibrary/GetNVIDIADisplayConfig: Error getting physical GPU count.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to wake the NVIDIA video card up and try again.");
                    try
                    {
                        // Load the library that keeps the NVIDIA video card visible to this application (potentially wasting laptop power)
                        NVIDIALibrary.KeepVideoCardOn();

                        // Enumerate all the Physical GPUs
                        adapters = _nvapiApiHelper.EnumeratePhysicalGpus();
                        adapterTotalCount = adapters.Length;
                        adapterNum = 0;
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: EnumeratePhysicalGpus returned {adapterTotalCount} Physical GPUs (2nd attempt)");

                        // This check is to make sure that we only continue in this function if there are physical GPUs to actually do anything with
                        // If the driver is installed, but not physical GPUs are present then we just want to return a default blank config.
                        if (adapterTotalCount == 0)
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

                // If we get here we have one or more NVIDIA GPU adapter
                // Go through each adapter
                foreach (var adapter in adapters)
                {
                    adapterNum++;

                    //------------------------------------
                    // POPULATE ADAPTER-LEVEL CONFIG
                    //------------------------------------
                    NVIDIA_PER_ADAPTER_CONFIG myAdapter = new NVIDIA_PER_ADAPTER_CONFIG();

// Try to get the mosaic settings
                    try
                    {
                        // Get current Mosaic Topology settings in brief (check whether Mosaic is on)
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the current mosaic topology brief and mosaic display settings.");
                        var mosaicHelper = _nvapiApiHelper.GetMosaicHelper();
                        var mosaicDto = mosaicHelper.GetCurrentTopo();
                        if (!mosaicDto.HasValue)
                        {
                            // Mosaic is not enabled/supported, so set the mosaic config to reflect this
                            myDisplayConfig.MosaicConfig.IsMosaicEnabled = false;
                            myDisplayConfig.MosaicConfig.MosaicCurrentTopo = new  NVAPIMosaicCurrentTopoDto();
                            myDisplayConfig.MosaicConfig.MosaicGridTopologies = new NVAPIMosaicGridTopologiesDto();
                        }
                        else
                        {
                            myDisplayConfig.MosaicConfig.IsMosaicEnabled = mosaicDto.Value.TopoBrief.Enabled;
                            myDisplayConfig.MosaicConfig.MosaicCurrentTopo = mosaicDto.Value;
                            var mosaicGridToposDto = mosaicHelper.EnumDisplayGrids();
                            if (mosaicGridToposDto.HasValue)
                            {   
                                myDisplayConfig.MosaicConfig.MosaicGridTopologies = mosaicGridToposDto.Value;
                            }
                            else
                            {
                               SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Failed to grab the mosaic grid toplogy.");
                            }
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the current mosaic toplogy brief and mosaic display settings.");

                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception caused whilst getting current mosiac topology brief and mosaic display settings.");
                    }

                    // Get the GPU full name
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the full name of the physical GPU adapter {adapterNum}.");
                        myAdapter.FullName = adapter.GetFullName();
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the GPU full name '{myAdapter.FullName}' for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting full name for adapter {adapterNum}.");
                    }

                    // Get the GPU bus type
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the bus type of the physical GPU adapter {adapterNum}.");
                        var busTypeResult = adapter.GetBusType();
                        if (busTypeResult.HasValue)
                        {
                            myAdapter.BusType = busTypeResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the bus type '{myAdapter.BusType}' for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting bus type for adapter {adapterNum}.");
                    }

                    // Get the GPU bus ID
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the bus ID of the physical GPU adapter {adapterNum}.");
                        var busIdResult = adapter.GetBusId();
                        if (busIdResult.HasValue)
                        {
                            myAdapter.BusId = busIdResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the bus ID '{myAdapter.BusId}' for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting bus ID for adapter {adapterNum}.");
                    }

                    // Get the GPU bus slot ID
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the bus slot ID of the physical GPU adapter {adapterNum}.");
                        var busSlotIdResult = adapter.GetBusSlotId();
                        if (busSlotIdResult.HasValue)
                        {
                            myAdapter.BusSlotId = busSlotIdResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the bus slot ID '{myAdapter.BusSlotId}' for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting bus slot ID for adapter {adapterNum}.");
                    }

                    // Get the GPU type
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the GPU type of the physical GPU adapter {adapterNum}.");
                        var gpuTypeResult = adapter.GetGpuType();
                        if (gpuTypeResult.HasValue)
                        {
                            myAdapter.GPUType = gpuTypeResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the GPU type '{myAdapter.GPUType}' for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting GPU type for adapter {adapterNum}.");
                    }

                    // Get the system type (laptop/desktop)
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the system type of the physical GPU adapter {adapterNum}.");
                        var systemTypeResult = adapter.GetSystemType();
                        if (systemTypeResult.HasValue)
                        {
                            myAdapter.SystemType = systemTypeResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the system type '{myAdapter.SystemType}' for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting system type for adapter {adapterNum}.");
                    }

                    // Get PCI identifiers (used to build the composite adapter key)
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the PCI identifiers of the physical GPU adapter {adapterNum}.");
                        var pciIdResult = adapter.GetPCIIdentifiers();
                        if (pciIdResult.HasValue)
                        {
                            myAdapter.PciIdentifiers = pciIdResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the PCI identifiers for adapter {adapterNum}. DeviceId=0x{myAdapter.PciIdentifiers.DeviceId:X4}, SubSystemId=0x{myAdapter.PciIdentifiers.SubSystemId:X4}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting PCI identifiers for adapter {adapterNum}.");
                    }

                    // Build the composite adapter key from PCI identifiers and bus ID
                    string adapterDeviceID = $"{myAdapter.PciIdentifiers.DeviceId}_{myAdapter.PciIdentifiers.SubSystemId}_{myAdapter.BusId}";
                    myAdapter.AdapterID = adapterDeviceID;
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Adapter {adapterNum} has AdapterID '{adapterDeviceID}'.");

                    // Get the VBIOS version string
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the VBIOS version string of the physical GPU adapter {adapterNum}.");
                        myAdapter.VbiosVersionString = adapter.GetVbiosVersionString();
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the VBIOS version string '{myAdapter.VbiosVersionString}' for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting VBIOS version string for adapter {adapterNum}.");
                    }

                    // Get the NvLink status
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the NvLink status of the physical GPU adapter {adapterNum}.");
                        var nvLinkResult = adapter.GetNvlinkStatus();
                        if (nvLinkResult.HasValue)
                        {
                            myAdapter.NvLinkStatus = nvLinkResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the NvLink status for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting NvLink status for adapter {adapterNum}.");
                    }

                    // Get the GPU info
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the GPU info of the physical GPU adapter {adapterNum}.");
                        var gpuInfoResult = adapter.GetGpuInfo();
                        if (gpuInfoResult.HasValue)
                        {
                            myAdapter.GpuInfo = gpuInfoResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the GPU info for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting GPU info for adapter {adapterNum}.");
                    }

                    // Get the ECC configuration info
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the ECC configuration info of the physical GPU adapter {adapterNum}.");
                        var eccResult = adapter.GetEccConfigurationInfo();
                        if (eccResult.HasValue)
                        {
                            myAdapter.EccConfigurationInfo = eccResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the ECC configuration info for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting ECC configuration info for adapter {adapterNum}.");
                    }

                    // Check if this is a Quadro card
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to check if adapter {adapterNum} is a Quadro card.");
                        var isQuadroResult = adapter.QueryWorkstationFeatureSupport(_NV_GPU_WORKSTATION_FEATURE_TYPE.NV_GPU_WORKSTATION_FEATURE_TYPE_PROVIZ);
                        if (isQuadroResult.HasValue)
                        {
                            myAdapter.IsQuadro = isQuadroResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Adapter {adapterNum} IsQuadro={myAdapter.IsQuadro}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception checking Quadro status for adapter {adapterNum}.");
                    }

                    // Get the display config for this adapter
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the display config of the physical GPU adapter {adapterNum}.");
                        var displayConfigResult = adapter.GetDisplayConfig();
                        if (displayConfigResult.HasValue)
                        {
                            myAdapter.DisplayConfig = displayConfigResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the display config for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting display config for adapter {adapterNum}.");
                    }

                    // Get the GPU architecture info
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the architecture info of the physical GPU adapter {adapterNum}.");
                        var archInfoResult = adapter.GetArchInfo();
                        if (archInfoResult.HasValue)
                        {
                            myAdapter.ArchInfo = archInfoResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the architecture info for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting architecture info for adapter {adapterNum}.");
                    }

                    // Get the GPU board info
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the board info of the physical GPU adapter {adapterNum}.");
                        var boardInfoResult = adapter.GetBoardInfo();
                        if (boardInfoResult.HasValue)
                        {
                            myAdapter.BoardInfo = boardInfoResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the board info for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting board info for adapter {adapterNum}.");
                    }

                    // Get the HDCP support status
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the HDCP support status of the physical GPU adapter {adapterNum}.");
                        var hdcpResult = adapter.GetHdcpSupportStatus();
                        if (hdcpResult.HasValue)
                        {
                            myAdapter.HdcpSupportStatus = hdcpResult.Value;
                        }
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the HDCP support status for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting HDCP support status for adapter {adapterNum}.");
                    }

                    //------------------------------------
                    // ENUMERATE DISPLAYS FOR THIS ADAPTER
                    //------------------------------------
                    NVAPIDisplayHelper[] displays;
                    if (allDisplays)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Enumerating all displays for Adapter {adapterNum}.");
                        displays = adapter.EnumAllDisplays();
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Enumerating active displays for Adapter {adapterNum}.");
                        displays = adapter.EnumActiveDisplays();
                    }
                    int displayTotalCount = displays.Length;
                    int displayNum = 0;

                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Found {displayTotalCount} display(s) on adapter {adapterNum}.");

                    // Build a lookup of connector types per display ID. This gates settings collection
                    // by connection technology: only DisplayPort and HDMI support the full NVAPI settings
                    // query API; other types (DVI, VGA, LVDS, etc.) will throw exceptions if those calls
                    // are attempted on them.
                    var connectorTypeLookup = new Dictionary<uint, NV_MONITOR_CONN_TYPE>();
                    try
                    {
                        var displayIdDtos = allDisplays ? adapter.GetAllDisplayIds() : adapter.GetConnectedDisplayIds();
                        foreach (var d in displayIdDtos)
                            connectorTypeLookup[d.DisplayId] = d.ConnectorType;
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Built connector type lookup with {connectorTypeLookup.Count} entries for adapter {adapterNum}.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception building connector type lookup for adapter {adapterNum}. Settings requiring DP/HDMI will be skipped for all displays on this adapter.");
                    }

                    

                    foreach (var display in displays)
                    {
                        displayNum++;
                        var displayId = display.DisplayId;
                        var IsConnected = display.IsConnected;

                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Processing display {displayNum}/{displayTotalCount} (DisplayId={displayId}, Connected={IsConnected}) on adapter {adapterNum}.");

                        // Get the Windows display name (e.g. "\\.\DISPLAY1") for this display and map it to the DisplayId
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to get the Windows DisplayName for DisplayId {displayId} on adapter {adapterNum}.");
                            var nvidiaDisplayName = display.GetAssociatedNvidiaDisplayName();
                            if (!string.IsNullOrEmpty(nvidiaDisplayName))
                            {
                                if (!myDisplayConfig.DisplayNames.ContainsKey(displayId.ToString()))
                                {
                                    myDisplayConfig.DisplayNames.Add(displayId.ToString(), nvidiaDisplayName);
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully mapped DisplayId {displayId} to Windows DisplayName '{nvidiaDisplayName}' on adapter {adapterNum}.");
                                }
                                else
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: DisplayId {displayId} already has a DisplayName mapping. Skipping duplicate.");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: No Windows DisplayName available for DisplayId {displayId} on adapter {adapterNum}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Windows DisplayName for DisplayId {displayId} on adapter {adapterNum}.");
                        }

                        // Create per-display config
                        NVIDIA_PER_DISPLAY_CONFIG myDisplay = new NVIDIA_PER_DISPLAY_CONFIG();
                        myDisplay.DisplayId = displayId;

                        // Resolve the connector type for this display and derive capability booleans.
                        // UNKNOWN is the safe default if the lookup fails (safe-by-default: ungated calls only).
                        myDisplay.ConnectorType = connectorTypeLookup.TryGetValue(displayId, out var connType)
                            ? connType
                            : NV_MONITOR_CONN_TYPE.NV_MONITOR_CONN_TYPE_UNKNOWN;
                        bool isDisplayPort        = myDisplay.ConnectorType == NV_MONITOR_CONN_TYPE.NV_MONITOR_CONN_TYPE_DP;
                        bool isHdmi               = myDisplay.ConnectorType == NV_MONITOR_CONN_TYPE.NV_MONITOR_CONN_TYPE_HDMI;
                        bool isDigitalWithProtocol = isDisplayPort || isHdmi;
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Display {displayId} connector type is {myDisplay.ConnectorType}. isDisplayPort={isDisplayPort}, isHdmi={isHdmi}, isDigitalWithProtocol={isDigitalWithProtocol}.");

                        //------------------------------------
                        // GET DISPLAY STATUS PROPERTIES
                        //------------------------------------
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the display status properties for Display {displayNum} on Adapter {adapterNum}.");
                            myDisplay.IsActive = display.IsActive;
                            myDisplay.IsConnected = display.IsConnected;
                            myDisplay.IsPhysicallyConnected = display.IsPhysicallyConnected;
                            myDisplay.IsCluster = display.IsCluster;
                            myDisplay.IsDynamic = display.IsDynamic;
                            myDisplay.IsMultiStreamRootNode = display.IsMultiStreamRootNode;
                            myDisplay.IsOSVisible = display.IsOSVisible;
                            myDisplay.IsWfd = display.IsWfd;
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got display status properties for Display {displayNum} on Adapter {adapterNum}: IsActive={myDisplay.IsActive}, IsConnected={myDisplay.IsConnected}, IsPhysicallyConnected={myDisplay.IsPhysicallyConnected}, IsCluster={myDisplay.IsCluster}, IsDynamic={myDisplay.IsDynamic}, IsMultiStreamRootNode={myDisplay.IsMultiStreamRootNode}, IsOSVisible={myDisplay.IsOSVisible}, IsWfd={myDisplay.IsWfd}.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting display status properties for Display {displayNum} on Adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET EDID INFORMATION
                        //------------------------------------
                        string manufacturerName = "Unknown";
                        UInt32 productCode = 0;
                        UInt32 serialNumber = 0;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the EDID information for Display {displayNum} on Adapter {adapterNum}.");
                            var edidInfo = display.GetEdidData(NV_EDID_FLAG.NV_EDID_FLAG_DEFAULT);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the EDID information for Display {displayNum} on Adapter {adapterNum}.");
                            EDID edidParsedInfo = new EDID(edidInfo.Value.Data);
                            manufacturerName = edidParsedInfo.ManufacturerCode;
                            productCode = edidParsedInfo.ProductCode;
                            serialNumber = edidParsedInfo.SerialNumber;
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: EDID for Display {displayNum}: Manufacturer={manufacturerName}, ProductCode={productCode}, SerialNumber={serialNumber}.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting EDID for Display {displayNum} on Adapter {adapterNum}. This is unfortunately common and appears to be a bug in the NVIDIA driver.");
                        }

                        //------------------------------------
                        // GET COLOR DATA
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get the color data for Display {displayNum} on Adapter {adapterNum}.");
                                var colorDataNative = NVAPIDisplayHelper.CreateColorData();
                                colorDataNative.cmd = (byte)NV_COLOR_CMD.NV_COLOR_CMD_GET;
                                var colorDataInput = NVAPIDisplayColorDataDto.FromNative(colorDataNative);
                                var colorDataResult = display.ColorControl(colorDataInput);
                                if (colorDataResult.HasValue)
                                {
                                    var colorNative = colorDataResult.Value.Data;
                                    colorNative.cmd = (byte)NV_COLOR_CMD.NV_COLOR_CMD_SET;
                                    myDisplay.ColorData = NVAPIDisplayColorDataDto.FromNative(colorNative);
                                    myDisplay.HasColorData = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got color data for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting color data for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET HDR CAPABILITIES
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get HDR capabilities for Display {displayNum} on Adapter {adapterNum}.");
                                var hdrCapsResult = display.GetHdrCapabilities();
                                if (hdrCapsResult.HasValue)
                                {
                                    myDisplay.HdrCapabilities = hdrCapsResult.Value;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got HDR capabilities for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting HDR capabilities for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET HDR COLOR DATA
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get HDR color data for Display {displayNum} on Adapter {adapterNum}.");
                                var hdrColorDataInput = new NVAPIHdrColorDataDto();
                                var hdrColorDataResult = display.HdrColorControl(hdrColorDataInput);
                                if (hdrColorDataResult.HasValue)
                                {
                                    var hdrNative = hdrColorDataResult.Value.Data;
                                    hdrNative.cmd = NV_HDR_CMD.NV_HDR_CMD_SET;
                                    myDisplay.HdrColorData = NVAPIHdrColorDataDto.FromNative(hdrNative);
                                    myDisplay.HasNvHdrEnabled = true;
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: HDR color data retrieved for Display {displayNum} on Adapter {adapterNum}.");
                                }
                                else
                                {
                                    myDisplay.HasNvHdrEnabled = false;
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: HDR color data not available for Display {displayNum} on Adapter {adapterNum}.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting HDR color data for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET ADAPTIVE SYNC DATA
                        //------------------------------------
                        if (isDisplayPort)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Adaptive Sync data for Display {displayNum} on Adapter {adapterNum}.");
                                var getAdaptiveSyncData = display.GetAdaptiveSyncData();
                                if (getAdaptiveSyncData.HasValue)
                                {
                                    // Store a default Set DTO to indicate adaptive sync is available
                                    myDisplay.AdaptiveSyncConfig = new NVAPIAdaptiveSyncSetDataDto();
                                    myDisplay.HasAdaptiveSync = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Adaptive Sync data for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Adaptive Sync data for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET DISPLAYPORT INFO
                        //------------------------------------
                        if (isDisplayPort)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get DisplayPort info for Display {displayNum} on Adapter {adapterNum}.");
                                var dpInfoResult = display.GetDisplayPortInfo();
                                if (dpInfoResult.HasValue)
                                {
                                    myDisplay.DisplayPortInfo = dpInfoResult.Value;
                                    myDisplay.HasDisplayPortInfo = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got DisplayPort info for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting DisplayPort info for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET VIRTUAL REFRESH RATE DATA
                        //------------------------------------
                        if (isDisplayPort)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Virtual Refresh Rate data for Display {displayNum} on Adapter {adapterNum}.");
                                var vrrResult = display.GetVirtualRefreshRateData();
                                if (vrrResult.HasValue)
                                {
                                    myDisplay.VirtualRefreshRateData = vrrResult.Value;
                                    myDisplay.HasVirtualRefreshRate = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Virtual Refresh Rate data for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Virtual Refresh Rate data for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET PREFERRED STEREO DISPLAY
                        //------------------------------------
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Preferred Stereo Display for Display {displayNum} on Adapter {adapterNum}.");
                            var stereoResult = display.GetPreferredStereoDisplay();
                            if (stereoResult.HasValue)
                            {
                                myDisplay.PreferredStereoDisplay = stereoResult.Value;
                                myDisplay.HasPreferredStereoDisplay = true;
                            }
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Preferred Stereo Display for Display {displayNum} on Adapter {adapterNum}.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Preferred Stereo Display for Display {displayNum} on Adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET SOURCE COLOR SPACE
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Source Color Space for Display {displayNum} on Adapter {adapterNum}.");
                                var colorSpaceResult = display.GetSourceColorSpace(displayId);
                                if (colorSpaceResult.HasValue)
                                {
                                    myDisplay.SourceColorSpace = colorSpaceResult.Value;
                                    myDisplay.HasSourceColorSpace = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Source Color Space '{myDisplay.SourceColorSpace}' for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Source Color Space for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET SOURCE HDR METADATA
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Source HDR Metadata for Display {displayNum} on Adapter {adapterNum}.");
                                var hdrMetadataResult = display.GetSourceHdrMetadata(displayId);
                                if (hdrMetadataResult.HasValue)
                                {
                                    myDisplay.SourceHdrMetadata = hdrMetadataResult.Value;
                                    myDisplay.HasSourceHdrMetadata = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Source HDR Metadata for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Source HDR Metadata for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET OUTPUT MODE
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Output Mode for Display {displayNum} on Adapter {adapterNum}.");
                                var outputModeResult = display.GetOutputMode();
                                if (outputModeResult.HasValue)
                                {
                                    myDisplay.OutputMode = outputModeResult.Value;
                                    myDisplay.HasOutputMode = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Output Mode '{myDisplay.OutputMode}' for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Output Mode for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET HDR TONE MAPPING
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get HDR Tone Mapping for Display {displayNum} on Adapter {adapterNum}.");
                                var hdrToneMappingResult = display.GetHdrToneMapping();
                                if (hdrToneMappingResult.HasValue)
                                {
                                    myDisplay.HdrToneMapping = hdrToneMappingResult.Value;
                                    myDisplay.HasHdrToneMapping = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got HDR Tone Mapping '{myDisplay.HdrToneMapping}' for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting HDR Tone Mapping for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET INFOFRAME DATA
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get InfoFrame data for Display {displayNum} on Adapter {adapterNum}.");
                                var infoFrameDataInput = new NVAPIInfoFrameDataDto();
                                var infoFrameResult = display.InfoFrameControl(infoFrameDataInput);
                                if (infoFrameResult.HasValue)
                                {
                                    var infoNative = infoFrameResult.Value.Data;
                                    infoNative.cmd = (byte)NV_INFOFRAME_CMD.NV_INFOFRAME_CMD_SET;
                                    myDisplay.InfoFrameData = NVAPIInfoFrameDataDto.FromNative(infoNative);
                                    myDisplay.HasInfoFrameData = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got InfoFrame data for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting InfoFrame data for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET MONITOR CAPABILITIES
                        //------------------------------------
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Monitor Capabilities for Display {displayNum} on Adapter {adapterNum}.");
                            var monitorCapsResult = display.GetMonitorCapabilities();
                            if (monitorCapsResult.HasValue)
                            {
                                myDisplay.MonitorCapabilities = monitorCapsResult.Value;
                                myDisplay.HasMonitorCapabilities = true;
                            }
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Monitor Capabilities for Display {displayNum} on Adapter {adapterNum}.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Monitor Capabilities for Display {displayNum} on Adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET MONITOR COLOR CAPABILITIES
                        //------------------------------------
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Monitor Color Capabilities for Display {displayNum} on Adapter {adapterNum}.");
                            var monitorColorCapsResult = display.GetMonitorColorCapabilities();
                            if (monitorColorCapsResult.HasValue)
                            {
                                myDisplay.MonitorColorCapabilities = monitorColorCapsResult.Value;
                                myDisplay.HasMonitorColorCapabilities = true;
                            }
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Monitor Color Capabilities for Display {displayNum} on Adapter {adapterNum}.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Monitor Color Capabilities for Display {displayNum} on Adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET HDMI SUPPORT INFO
                        //------------------------------------
                        if (isHdmi)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get HDMI Support Info for Display {displayNum} on Adapter {adapterNum}.");
                                var hdmiSupportResult = display.GetHdmiSupportInfo(null);
                                if (hdmiSupportResult.HasValue)
                                {
                                    myDisplay.HdmiSupportInfo = hdmiSupportResult.Value;
                                    myDisplay.HasHdmiSupportInfo = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got HDMI Support Info for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting HDMI Support Info for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET VRR INFO
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get VRR Info for Display {displayNum} on Adapter {adapterNum}.");
                                var vrrInfoResult = display.GetVrrInfo();
                                if (vrrInfoResult.HasValue)
                                {
                                    myDisplay.VrrInfo = vrrInfoResult.Value;
                                    myDisplay.HasVrrInfo = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got VRR Info for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting VRR Info for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET DISPLAY COLORIMETRY
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Display Colorimetry for Display {displayNum} on Adapter {adapterNum}.");
                                var colorimetryResult = display.GetColorimetry();
                                if (colorimetryResult.HasValue)
                                {
                                    myDisplay.DisplayColorimetry = colorimetryResult.Value;
                                    myDisplay.HasDisplayColorimetry = true;
                                }
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Display Colorimetry for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Display Colorimetry for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET DISPLAY ID INFO
                        //------------------------------------
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Display ID Info for Display {displayNum} on Adapter {adapterNum}.");
                            var displayIdInfoResult = display.GetDisplayIdInfo(null);
                            if (displayIdInfoResult.HasValue)
                            {
                                myDisplay.DisplayIdInfo = displayIdInfoResult.Value;
                                myDisplay.HasDisplayIdInfo = true;
                            }
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Display ID Info for Display {displayNum} on Adapter {adapterNum}.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Display ID Info for Display {displayNum} on Adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET TIMING
                        //------------------------------------
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Timing for Display {displayNum} on Adapter {adapterNum}.");
                            var timingInput = new NVAPITimingInputDto(0, 0, 0f, new NV_TIMING_FLAG(), _NV_TIMING_OVERRIDE.NV_TIMING_OVERRIDE_CURRENT);
                            var timingResult = display.GetTiming(timingInput);
                            if (timingResult.HasValue)
                            {
                                myDisplay.Timing = timingResult.Value;
                                myDisplay.HasTiming = true;
                            }
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Timing for Display {displayNum} on Adapter {adapterNum}.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Timing for Display {displayNum} on Adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET SCANOUT CONFIGURATION
                        //------------------------------------
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to get Scanout Configuration for Display {displayNum} on Adapter {adapterNum}.");
                            var scanoutResult = display.GetScanoutConfiguration();
                            if (scanoutResult.HasValue)
                            {
                                myDisplay.ScanoutConfiguration = scanoutResult.Value;
                                myDisplay.HasScanoutConfiguration = true;
                            }
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got Scanout Configuration for Display {displayNum} on Adapter {adapterNum}.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception getting Scanout Configuration for Display {displayNum} on Adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // ENUM CUSTOM DISPLAYS
                        //------------------------------------
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Attempting to enumerate custom displays for Display {displayNum} on Adapter {adapterNum}.");
                            myDisplay.CustomDisplays = new List<NVAPICustomDisplayDto>();
                            for (uint customIndex = 0; customIndex < 100; customIndex++)
                            {
                                try
                                {
                                    var customDisplayResult = display.EnumCustomDisplay(customIndex);
                                    if (customDisplayResult.HasValue)
                                    {
                                        myDisplay.CustomDisplays.Add(customDisplayResult.Value);
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                catch
                                {
                                    // End of custom display enumeration
                                    break;
                                }
                            }
                            if (myDisplay.CustomDisplays.Count > 0)
                            {
                                myDisplay.HasCustomDisplay = true;
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Found {myDisplay.CustomDisplays.Count} custom display(s) for Display {displayNum} on Adapter {adapterNum}.");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: No custom displays found for Display {displayNum} on Adapter {adapterNum}.");
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception enumerating custom displays for Display {displayNum} on Adapter {adapterNum}.");
                        }

                        // Build a composite display key from adapter, manufacturer and display ID
                        string displayDeviceKey = $"{adapterDeviceID}|{manufacturerName}|{productCode}|{displayId}";
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Adding display '{displayDeviceKey}' to adapter {adapterNum} Displays dictionary.");

                        // Add the display to the adapter's Displays dictionary
                        if (!myAdapter.Displays.ContainsKey(displayDeviceKey))
                        {
                            myAdapter.Displays.Add(displayDeviceKey, myDisplay);
                        }
                        else
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Duplicate display key '{displayDeviceKey}' detected on adapter {adapterNum}. Skipping duplicate.");
                        }
                    }

                    // Set the display count for this adapter
                    myAdapter.DisplayCount = myAdapter.Displays.Count;
                    // Set the total number of displays
                    myDisplayConfig.DisplayCount += myAdapter.DisplayCount;

                    // Add the adapter to the config
                    if (!myDisplayConfig.PhysicalAdapters.ContainsKey(adapterDeviceID))
                    {
                        myDisplayConfig.PhysicalAdapters.Add(adapterDeviceID, myAdapter);
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Duplicate adapter key '{adapterDeviceID}' detected. Skipping duplicate adapter.");
                    }

                    // Check if there is a cloned display in the current layout by examining the DisplayConfig paths
                    // A path with multiple targets means one source is driving multiple displays (i.e. clone)
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Checking if there is a cloned display detected within NVIDIA Display Configuration.");
                    foreach (var adapterEntry in myDisplayConfig.PhysicalAdapters)
                    {
                        var paths = adapterEntry.Value.DisplayConfig.Paths;
                        if (paths != null)
                        {
                            foreach (var path in paths)
                            {
                                if (path.Targets != null && path.Targets.Length > 1)
                                {
                                    // This is a cloned display, we need to mark this NVIDIA display profile as cloned so we correct the profile later
                                    myDisplayConfig.IsCloned = true;
                                    break;
                                }
                            }
                        }
                        if (myDisplayConfig.IsCloned) break;
                    }
                    if (myDisplayConfig.IsCloned)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Cloned display detected within NVIDIA Display Configuration.");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Cloned display NOT detected within NVIDIA Display Configuration.");
                    }


                    // Get the DRS Settings using the NVAPIWrapper DRS helper
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to create a DRS session so we can get the DRS settings.");
                        using (var drsHelper = _nvapiApiHelper.CreateDrsSession())
                        {
                            if (drsHelper == null)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Failed to create a DRS session. DRS settings will not be captured.");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully created a DRS session.");

                                // Load the DRS Settings into memory
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to load the DRS Settings into memory.");
                                bool loaded = drsHelper.LoadSettings();
                                if (!loaded)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Failed to load DRS settings into memory.");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully loaded the DRS Settings into memory.");

                                    // Get the base DRS profile
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to get the base DRS Profile.");
                                    var baseProfile = drsHelper.GetBaseProfile();
                                    if (!baseProfile.HasValue)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/GetNVIDIADisplayConfig: Failed to get the base DRS profile. The DRS Settings may not have been loaded.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully got the base DRS Profile. Profile Name is {baseProfile.Value.ProfileName}.");

                                        // Create a DRS config entry to track the base profile
                                        NVIDIA_DRS_CONFIG drsConfig = new NVIDIA_DRS_CONFIG();
                                        drsConfig.IsBaseProfile = true;
                                        drsConfig.ProfileInfo = baseProfile.Value;

                                        if (baseProfile.Value.NumOfSettings > 0)
                                        {
                                            // Enumerate all settings in the base profile
                                            try
                                            {
                                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Trying to enumerate the DRS settings from the base DRS Profile {baseProfile.Value.ProfileName}.");
                                                var drsDriverSettings = drsHelper.EnumSettings(baseProfile.Value);
                                                drsConfig.DriverSettings = drsDriverSettings.ToList();
                                                SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: Successfully enumerated {drsConfig.DriverSettings.Count} DRS settings from the base DRS Profile {baseProfile.Value.ProfileName}.");
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst enumerating the DRS settings from the base DRS Profile {baseProfile.Value.ProfileName}.");
                                            }

                                            // Save the DRS Config to the main config so it gets saved
                                            myDisplayConfig.DRSSettings.Add(drsConfig);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetNVIDIADisplayConfig: Exception occurred whilst getting the DRS settings.");
                    }

                    // At this stage we should set the IsInUse flag to report that the NVIDIA config is in Use
                    myDisplayConfig.IsInUse = true;

                    // Get the display identifiers                
                    myDisplayConfig.DisplayIdentifiers = GetCurrentDisplayIdentifiers(out bool failure);

                }
            }
            else
            {
                SharedLogger.logger.Info($"NVIDIALibrary/GetNVIDIADisplayConfig: ERROR - Tried to run GetNVIDIADisplayConfig but the NVIDIA NVAPI library isn't initialised! This generally means you don't have a NVIDIA video card in your machine.");
                //throw new NVIDIALibraryException($"Tried to run GetNVIDIADisplayConfig but the NVIDIA NVAPI library isn't initialised!");
                return CreateDefaultConfig();
            }

            // Return the configuration
            return myDisplayConfig;
        }


        public string PrintActiveConfig()
        {
            NVIDIA_DISPLAY_CONFIG displayConfig = ActiveDisplayConfig;
            var sb = new StringBuilder();

            sb.AppendLine("****** NVIDIA VIDEO CARDS *******");

            if (displayConfig.PhysicalAdapters.Count == 0)
            {
                sb.AppendLine("No NVIDIA Video Cards detected.");
                sb.AppendLine();
                return sb.ToString();
            }

            sb.AppendLine($"Number of NVIDIA adapters found: {displayConfig.PhysicalAdapters.Count}");
            sb.AppendLine();

            // Physical Adapters
            foreach (var physicalGPU in displayConfig.PhysicalAdapters)
            {
                string adapterKey = physicalGPU.Key;
                NVIDIA_PER_ADAPTER_CONFIG myAdapter = physicalGPU.Value;

                sb.AppendLine($"Adapter: {adapterKey}");
                sb.AppendLine($"  Name: {myAdapter.FullName}");
                sb.AppendLine($"  AdapterID: {myAdapter.AdapterID}");
                sb.AppendLine($"  IsQuadro: {myAdapter.IsQuadro}");
                sb.AppendLine($"  HasLogicalGPU: {myAdapter.HasLogicalGPU}");
                sb.AppendLine($"  SystemType: {myAdapter.SystemType}");
                sb.AppendLine($"  GPUType: {myAdapter.GPUType}");
                sb.AppendLine($"  BusType: {myAdapter.BusType}");
                sb.AppendLine($"  BusId: {myAdapter.BusId} BusSlotId: {myAdapter.BusSlotId}");
                sb.AppendLine($"  PciIdentifiers: {myAdapter.PciIdentifiers}");
                sb.AppendLine($"  VbiosVersion: {myAdapter.VbiosVersionString}");
                sb.AppendLine($"  DisplayCount: {myAdapter.DisplayCount}");
                sb.AppendLine();

                // Per-display settings
                foreach (var displayDict in myAdapter.Displays)
                {
                    string displayKey = displayDict.Key;
                    NVIDIA_PER_DISPLAY_CONFIG myDisplay = displayDict.Value;

                    sb.AppendLine($"  Display: {displayKey}");
                    sb.AppendLine($"    DisplayId: {myDisplay.DisplayId}");
                    sb.AppendLine($"    ConnectorType: {myDisplay.ConnectorType}");
                    sb.AppendLine($"    IsActive: {myDisplay.IsActive} IsConnected: {myDisplay.IsConnected} IsPhysicallyConnected: {myDisplay.IsPhysicallyConnected}");
                    sb.AppendLine($"    IsCluster: {myDisplay.IsCluster} IsDynamic: {myDisplay.IsDynamic} IsMultiStreamRootNode: {myDisplay.IsMultiStreamRootNode}");
                    sb.AppendLine($"    IsOSVisible: {myDisplay.IsOSVisible} IsWfd: {myDisplay.IsWfd}");

                    // Color Data
                    if (myDisplay.HasColorData)
                    {
                        sb.AppendLine($"    ColorData: {myDisplay.ColorData}");
                    }

                    // HDR Capabilities and Color Data
                    if (myDisplay.HasNvHdrEnabled)
                    {
                        sb.AppendLine($"    HdrCapabilities: {myDisplay.HdrCapabilities}");
                        sb.AppendLine($"    HdrColorData: {myDisplay.HdrColorData}");
                    }

                    // Adaptive Sync
                    if (myDisplay.HasAdaptiveSync)
                    {
                        sb.AppendLine($"    AdaptiveSyncConfig: {myDisplay.AdaptiveSyncConfig}");
                    }

                    // Custom Displays
                    if (myDisplay.HasCustomDisplay)
                    {
                        sb.AppendLine($"    CustomDisplays: {myDisplay.CustomDisplays.Count} custom display(s)");
                        for (int i = 0; i < myDisplay.CustomDisplays.Count; i++)
                        {
                            sb.AppendLine($"      CustomDisplay[{i}]: {myDisplay.CustomDisplays[i]}");
                        }
                    }

                    // DisplayPort Info
                    if (myDisplay.HasDisplayPortInfo)
                    {
                        sb.AppendLine($"    DisplayPortInfo: {myDisplay.DisplayPortInfo}");
                    }

                    // Virtual Refresh Rate
                    if (myDisplay.HasVirtualRefreshRate)
                    {
                        sb.AppendLine($"    VirtualRefreshRateData: {myDisplay.VirtualRefreshRateData}");
                    }

                    // Preferred Stereo Display
                    if (myDisplay.HasPreferredStereoDisplay)
                    {
                        sb.AppendLine($"    PreferredStereoDisplay: {myDisplay.PreferredStereoDisplay}");
                    }

                    // Source Color Space
                    if (myDisplay.HasSourceColorSpace)
                    {
                        sb.AppendLine($"    SourceColorSpace: {myDisplay.SourceColorSpace}");
                    }

                    // Source HDR Metadata
                    if (myDisplay.HasSourceHdrMetadata)
                    {
                        sb.AppendLine($"    SourceHdrMetadata: {myDisplay.SourceHdrMetadata}");
                    }

                    // Output Mode
                    if (myDisplay.HasOutputMode)
                    {
                        sb.AppendLine($"    OutputMode: {myDisplay.OutputMode}");
                    }

                    // HDR Tone Mapping
                    if (myDisplay.HasHdrToneMapping)
                    {
                        sb.AppendLine($"    HdrToneMapping: {myDisplay.HdrToneMapping}");
                    }

                    // InfoFrame Data
                    if (myDisplay.HasInfoFrameData)
                    {
                        sb.AppendLine($"    InfoFrameData: {myDisplay.InfoFrameData}");
                    }

                    // Monitor Capabilities
                    if (myDisplay.HasMonitorCapabilities)
                    {
                        sb.AppendLine($"    MonitorCapabilities: {myDisplay.MonitorCapabilities}");
                    }

                    // Monitor Color Capabilities
                    if (myDisplay.HasMonitorColorCapabilities)
                    {
                        sb.AppendLine($"    MonitorColorCapabilities: {myDisplay.MonitorColorCapabilities}");
                    }

                    // HDMI Support Info
                    if (myDisplay.HasHdmiSupportInfo)
                    {
                        sb.AppendLine($"    HdmiSupportInfo: {myDisplay.HdmiSupportInfo}");
                    }

                    // VRR Info
                    if (myDisplay.HasVrrInfo)
                    {
                        sb.AppendLine($"    VrrInfo: {myDisplay.VrrInfo}");
                    }

                    // Display Colorimetry
                    if (myDisplay.HasDisplayColorimetry)
                    {
                        sb.AppendLine($"    DisplayColorimetry: {myDisplay.DisplayColorimetry}");
                    }

                    // Display ID Info
                    if (myDisplay.HasDisplayIdInfo)
                    {
                        sb.AppendLine($"    DisplayIdInfo: {myDisplay.DisplayIdInfo}");
                    }

                    // Timing
                    if (myDisplay.HasTiming)
                    {
                        sb.AppendLine($"    Timing: {myDisplay.Timing}");
                    }

                    // Scanout Configuration
                    if (myDisplay.HasScanoutConfiguration)
                    {
                        sb.AppendLine($"    ScanoutConfiguration: {myDisplay.ScanoutConfiguration}");
                    }

                    sb.AppendLine();
                }
            }

            // Mosaic Config
            sb.AppendLine("NVIDIA SURROUND/MOSAIC");
            if (displayConfig.MosaicConfig.IsMosaicEnabled)
            {
                sb.AppendLine("NVIDIA Surround/Mosaic is Enabled");
                sb.AppendLine($"  CurrentTopo: {displayConfig.MosaicConfig.MosaicCurrentTopo}");
                if (displayConfig.MosaicConfig.MosaicGridTopologies.Grids != null)
                {
                    sb.AppendLine($"  Grid Topologies: {displayConfig.MosaicConfig.MosaicGridTopologies.Grids.Length}");
                    int gridIdx = 0;
                    foreach (var grid in displayConfig.MosaicConfig.MosaicGridTopologies.Grids)
                    {
                        sb.AppendLine($"  Grid #{gridIdx}: {grid.Rows} Rows x {grid.Columns} Columns, Displays={grid.Displays?.Length ?? 0}");
                        sb.AppendLine($"    BezelCorrect={grid.ApplyWithBezelCorrect} ImmersiveGaming={grid.ImmersiveGaming} BaseMosaic={grid.BaseMosaic}");
                        sb.AppendLine($"    DriverReloadAllowed={grid.DriverReloadAllowed} AcceleratePrimaryDisplay={grid.AcceleratePrimaryDisplay} PixelShift={grid.PixelShift}");
                        sb.AppendLine($"    DisplaySettings: {grid.DisplaySettings}");
                        gridIdx++;
                    }
                }
            }
            else
            {
                sb.AppendLine("NVIDIA Surround/Mosaic is Disabled");
            }
            sb.AppendLine();

            // DRS Settings
            sb.AppendLine("NVIDIA DRS SETTINGS");
            if (displayConfig.DRSSettings != null && displayConfig.DRSSettings.Count > 0)
            {
                foreach (var drsConfig in displayConfig.DRSSettings)
                {
                    sb.AppendLine($"  Profile: {drsConfig.ProfileInfo} (IsBaseProfile={drsConfig.IsBaseProfile})");
                    if (drsConfig.DriverSettings != null)
                    {
                        sb.AppendLine($"  Settings count: {drsConfig.DriverSettings.Count}");
                        foreach (var setting in drsConfig.DriverSettings)
                        {
                            sb.AppendLine($"    {setting.SettingName} ({setting.SettingId}): Type={setting.SettingType} Dword={setting.CurrentDwordValue} String={setting.CurrentStringValue}");
                        }
                    }
                }
            }
            else
            {
                sb.AppendLine("  No DRS settings stored.");
            }
            sb.AppendLine();

            // Display Names
            if (displayConfig.DisplayNames != null && displayConfig.DisplayNames.Count > 0)
            {
                sb.AppendLine("NVIDIA DISPLAY NAMES");
                foreach (var kvp in displayConfig.DisplayNames)
                {
                    sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
                }
                sb.AppendLine();
            }

            sb.AppendLine();

            return sb.ToString();
        }

        public bool SetActiveConfig(NVIDIA_DISPLAY_CONFIG displayConfig, int delayInMs)
        {

            if (_initialised)
            {
                // We want to check if we need to apply a NVIDIA Surround (Mosaic) config
                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Testing whether the display configuration uses NVIDIA Surround");
                if (displayConfig.MosaicConfig.IsMosaicEnabled)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: The display configuration we want to use has NVIDIA Surround (Mosaic) enabled");
                    if (displayConfig.MosaicConfig.Equals(ActiveDisplayConfig.MosaicConfig))
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Mosaic current config is exactly the same as the one we want, so skipping applying the Mosaic config as we already have the right one!");
                    }                    
                    else
                    {
             
                        // If we are on a non-Mosaic profile now, then we need to set a 1x1 display grid just to wake up the displays properly
                        if (!ActiveDisplayConfig.MosaicConfig.IsMosaicEnabled)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: The current display configuration does not have NVIDIA Surround (Mosaic) enabled, so we need to set a 1x1 mosaic matrix to ensure that all displays are awake.");
                            TurnOffMosaic(delayInMs);
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: The current display configuration does have a NVIDIA Surround (Mosaic) enabled, so we will skip setting the 1x1 mosaice grid.");
                        }

                        // Now that the Mosaic is turned off, we can apply the new Mosaic Topology
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Next we apply the wanted Mosaic config now");
                        try
                        {
                            using (var mosaicHelper = _nvapiApiHelper.GetMosaicHelper())
                            {
                                // If we get here then the display is valid, so now we actually apply the new Mosaic Topology
                                mosaicHelper.SetDisplayGrids(displayConfig.MosaicConfig.MosaicGridTopologies, 0);
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: NvAPI_Mosaic_SetDisplayGrids returned OK.");
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Waiting {delayInMs * 3} milliseconds to let the Mosaic display change take place before continuing");
                                Thread.Sleep(delayInMs * 3);
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfig: Exception: The GPU DisplayGrid could not be made. SetDisplayGrids() returned error message {ex.Message}");
                            return false;
                        }

                    }

                }
                else if (!displayConfig.MosaicConfig.IsMosaicEnabled && ActiveDisplayConfig.MosaicConfig.IsMosaicEnabled)
                {
                    // We are on a Mosaic profile now, and we need to change to a non-Mosaic profile
                    TurnOffMosaic(delayInMs);

                }
                else if (!displayConfig.MosaicConfig.IsMosaicEnabled && !ActiveDisplayConfig.MosaicConfig.IsMosaicEnabled)
                {
                    // We are on a non-Mosaic profile now, and we are changing to a non-Mosaic profile
                    // so there is nothing to do as far as NVIDIA is concerned!
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: We are on a non-Mosaic profile now, and we are changing to a non-Mosaic profile so there is no need to modify Mosaic settings!");
                }

            }
            else
            {
                SharedLogger.logger.Info($"NVIDIALibrary/SetActiveConfig: Tried to run SetActiveConfig but the NVIDIA NvAPI library isn't initialised! This generally means you don't have a NVIDIA video card in your machine.");
            }

            return true;
        }

        public bool TurnOffMosaic(int delayInMs)
        {
            SharedLogger.logger.Trace($"NVIDIALibrary/TurnOffMosaic: Mosaic config that is currently set is no longer needed. Removing Mosaic config.");

            try
            {
                using (var mosaicHelper = _nvapiApiHelper.GetMosaicHelper())
                {
                    // First attempt: Create 1x1 grids for each display and apply them
                    SharedLogger.logger.Trace($"NVIDIALibrary/TurnOffMosaic: Trying to set a 1x1 DisplayGrid to disable Mosaic.");
                    NVAPIMosaicGridTopologiesDto? individualScreensTopology = CreateSingleScreenMosaicTopology(mosaicHelper);
                    if (individualScreensTopology.HasValue)
                    {
                        try
                        {
                            mosaicHelper.SetDisplayGrids(individualScreensTopology.Value, 0);
                            SharedLogger.logger.Trace($"NVIDIALibrary/TurnOffMosaic: NvAPI_Mosaic_SetDisplayGrids returned OK.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/TurnOffMosaic: Waiting {delayInMs * 3} milliseconds to let the Mosaic display change take place before continuing");
                            Thread.Sleep(delayInMs * 3);
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/TurnOffMosaic: Exception while trying to set a 1x1 DisplayGrid using SetDisplayGrids.");
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"NVIDIALibrary/TurnOffMosaic: Could not create a 1x1 DisplayGrid topology. Will try EnableCurrentTopo(false) instead.");
                    }

                    // Check if Mosaic is still on after the first attempt
                    NVAPIMosaicCurrentTopoDto? currentTopo = mosaicHelper.GetCurrentTopo();
                    bool mosaicStillOn = currentTopo.HasValue && currentTopo.Value.TopoBrief.Enabled;

                    if (mosaicStillOn)
                    {
                        // If the Mosaic is still on, then the last mosaic disable failed, so we need to then try turning it off using EnableCurrentTopo(false)
                        SharedLogger.logger.Trace($"NVIDIALibrary/TurnOffMosaic: Previous attempt to turn off Mosaic failed. Now trying to use EnableCurrentTopo(false) to disable Mosaic instead.");
                        try
                        {
                            mosaicHelper.EnableCurrentTopo(false);
                            SharedLogger.logger.Trace($"NVIDIALibrary/TurnOffMosaic: EnableCurrentTopo(false) returned OK.");
                            SharedLogger.logger.Trace($"NVIDIALibrary/TurnOffMosaic: Waiting {delayInMs * 3} milliseconds to let the Mosaic display change take place before continuing");
                            Thread.Sleep(delayInMs * 3);
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/TurnOffMosaic: Exception while trying to disable Mosaic using EnableCurrentTopo(false).");
                            return false;
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/TurnOffMosaic: Mosaic successfully disabled using SetDisplayGrids method.");
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, $"NVIDIALibrary/TurnOffMosaic: Exception while trying to turn off Mosaic.");
                return false;
            }

            return true;
        }

        public bool SetActiveConfigOverride(NVIDIA_DISPLAY_CONFIG displayConfig, int delayInMs)
        {

            if (_initialised)
            {

                // We need to first update the active config to make sure it's set
                UpdateActiveConfig();

                // Set the DRS Settings only if we need to
                if (displayConfig.DRSSettings.Count > 0)
                {
                    try
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Attempting to create a DRS session.");
                        using (var drsHelper = _nvapiApiHelper.CreateDrsSession())
                        {
                            if (drsHelper == null)
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: Failed to create a DRS session. DRS settings will not be applied.");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully created a DRS session.");

                                // Load the current DRS Settings into memory
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Attempting to load the current DRS settings into memory.");
                                bool loaded = drsHelper.LoadSettings();
                                if (!loaded)
                                {
                                    SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: Failed to load the current DRS settings into memory.");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully loaded the current DRS settings into memory.");

                                    // Get the base DRS profile
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Attempting to get the base DRS profile.");
                                    var baseProfile = drsHelper.GetBaseProfile();
                                    if (!baseProfile.HasValue)
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: Failed to get the base DRS profile. The DRS Settings may not have been loaded.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully got the base DRS Profile.");

                                        // Go through all the settings we have in the saved profile, and change the current profile settings to be the same
                                        if (displayConfig.DRSSettings.Count > 0)
                                        {
                                            bool needToSave = false;
                                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: There are {displayConfig.DRSSettings.Count} stored DRS settings in the base DRS profile so we need to process them");

                                            try
                                            {
                                                // Get the Base Profiles from the stored config and the active config
                                                NVIDIA_DRS_CONFIG storedBaseProfile = displayConfig.DRSSettings.Find(p => p.IsBaseProfile == true);
                                                NVIDIA_DRS_CONFIG activeBaseProfile = ActiveDisplayConfig.DRSSettings.Find(p => p.IsBaseProfile == true);
                                                foreach (var drsSetting in storedBaseProfile.DriverSettings)
                                                {
                                                    for (int i = 0; i < activeBaseProfile.DriverSettings.Count; i++)
                                                    {
                                                        NVAPIDrsSettingDto currentSetting = activeBaseProfile.DriverSettings[i];

                                                        // If the setting is also in the active base profile (it should be!), then we set it.
                                                        if (drsSetting.SettingId == currentSetting.SettingId)
                                                        {
                                                            // Compare only the current value based on the setting type (closest to old CurrentValue behavior)
                                                            bool currentValueMatches = drsSetting.SettingType switch
                                                            {
                                                                _NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE => drsSetting.CurrentDwordValue == currentSetting.CurrentDwordValue,
                                                                _NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE or _NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE => string.Equals(drsSetting.CurrentStringValue, currentSetting.CurrentStringValue, StringComparison.Ordinal),
                                                                _NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE => (drsSetting.CurrentBinaryValue == null && currentSetting.CurrentBinaryValue == null) ||
                                                                    (drsSetting.CurrentBinaryValue != null && currentSetting.CurrentBinaryValue != null && drsSetting.CurrentBinaryValue.SequenceEqual(currentSetting.CurrentBinaryValue)),
                                                                _ => drsSetting.Equals(currentSetting)
                                                            };

                                                            if (currentValueMatches)
                                                            {
                                                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: '{currentSetting.SettingName}' ({currentSetting.SettingId}) current value already matches the desired value, so skipping changing it.");
                                                            }
                                                            else
                                                            {
                                                                try
                                                                {
                                                                    drsHelper.SetSetting(baseProfile.Value, drsSetting);
                                                                    needToSave = true;
                                                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We changed setting '{currentSetting.SettingName}' ({currentSetting.SettingId}) using NVAPIDrsHelper.SetSetting()");
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception caused whilst changing setting '{currentSetting.SettingName}' ({currentSetting.SettingId}).");
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
                                                    if (storedBaseProfile.DriverSettings.Exists(ds => ds.SettingId == currentSetting.SettingId))
                                                    {
                                                        continue;
                                                    }

                                                    try
                                                    {
                                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Attempting to restore DRS setting '{currentSetting.SettingName}' ({currentSetting.SettingId}) to its default.");
                                                        drsHelper.RestoreProfileDefaultSetting(baseProfile.Value, currentSetting.SettingId);
                                                        needToSave = true;
                                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We restored active setting '{currentSetting.SettingName}' ({currentSetting.SettingId}) to its default value using NVAPIDrsHelper.RestoreProfileDefaultSetting()");
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while trying to restore setting '{currentSetting.SettingName}' ({currentSetting.SettingId}) to its default.");
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while trying to find base profiles in either the stored or active display configs.");
                                            }

                                            // Save the Settings if needed
                                            if (needToSave)
                                            {
                                                try
                                                {
                                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Attempting to save the current DRS settings.");
                                                    drsHelper.SaveSettings();
                                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We successfully saved the current DRS Settings.");
                                                }
                                                catch (Exception ex)
                                                {
                                                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while trying to save the current DRS settings.");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception occurred whilst applying the DRS settings.");
                    }
                }

                // Build a lookup of NVAPIDisplayHelper objects by DisplayId so we can find the right helper for each display
                Dictionary<uint, NVAPIDisplayHelper> displayHelperLookup = new Dictionary<uint, NVAPIDisplayHelper>();
                try
                {
                    var adapters = _nvapiApiHelper.EnumeratePhysicalGpus();
                    foreach (var adapter in adapters)
                    {
                        NVAPIDisplayHelper[] allDisplays = adapter.EnumAllDisplays();
                        foreach (var display in allDisplays)
                        {
                            displayHelperLookup[display.DisplayId] = display;
                        }
                    }
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Built display helper lookup with {displayHelperLookup.Count} displays.");
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception occurred whilst building the display helper lookup. Per-display settings will be skipped.");
                }

                // Go through the physical adapters and apply per-display settings
                foreach (var physicalGPU in displayConfig.PhysicalAdapters)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Processing settings for Physical GPU #{physicalGPU.Key}");
                    NVIDIA_PER_ADAPTER_CONFIG myAdapter = physicalGPU.Value;
                    string myAdapterIndex = physicalGPU.Key;

                    foreach (var displayDict in myAdapter.Displays)
                    {
                        NVIDIA_PER_DISPLAY_CONFIG myDisplay = displayDict.Value;
                        string displayKey = displayDict.Key;

                        // Parse displayId from the composite key for connected display check
                        UInt32 displayId = 0;
                        if (displayKey.Contains("|"))
                        {
                            var parts = displayKey.Split('|');
                            if (parts.Length >= 4) UInt32.TryParse(parts[3], out displayId);
                        }
                        else
                        {
                            UInt32.TryParse(displayKey, out displayId);
                        }

                        // Now we try to set each display settings
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: We want to process settings for display {displayId}.");

                        if (!_allConnectedDisplayIds.Contains(displayId))
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Display {displayId} doesn't exist in this setup, so skipping overriding any NVIDIA display Settings.");
                            continue;
                        }

                        // Get the NVAPIDisplayHelper for this display
                        if (!displayHelperLookup.TryGetValue(displayId, out NVAPIDisplayHelper displayHelper))
                        {
                            SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: Could not find NVAPIDisplayHelper for display {displayId}. Skipping per-display settings.");
                            continue;
                        }

                        // Get the active display config for comparison (only set things that are different)
                        NVIDIA_PER_DISPLAY_CONFIG activeDisplay;
                        bool hasActiveDisplay = false;
                        try
                        {
                            if (ActiveDisplayConfig.PhysicalAdapters.ContainsKey(myAdapterIndex) &&
                                ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays.ContainsKey(displayKey))
                            {
                                activeDisplay = ActiveDisplayConfig.PhysicalAdapters[myAdapterIndex].Displays[displayKey];
                                hasActiveDisplay = true;
                            }
                            else
                            {
                                activeDisplay = new NVIDIA_PER_DISPLAY_CONFIG();
                                SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: Could not find display {displayKey} in ActiveDisplayConfig. Will apply all settings unconditionally.");
                            }
                        }
                        catch (Exception ex)
                        {
                            activeDisplay = new NVIDIA_PER_DISPLAY_CONFIG();
                            SharedLogger.logger.Warn(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception getting active display config for {displayKey}. Will apply all settings unconditionally.");
                        }

                        // Derive connector type gating booleans from the saved display configuration.
                        // These gate which settings can be applied based on connection technology.
                        bool isDisplayPort        = myDisplay.ConnectorType == NV_MONITOR_CONN_TYPE.NV_MONITOR_CONN_TYPE_DP;
                        bool isHdmi               = myDisplay.ConnectorType == NV_MONITOR_CONN_TYPE.NV_MONITOR_CONN_TYPE_HDMI;
                        bool isDigitalWithProtocol = isDisplayPort || isHdmi;

                        // === Color Data ===
                        if (myDisplay.HasColorData && isDigitalWithProtocol)
                        {
                            try
                            {
                                bool colorDifferent = !hasActiveDisplay || !activeDisplay.HasColorData || !myDisplay.ColorData.Equals(activeDisplay.ColorData);
                                if (colorDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Color data is different for display {displayId}. Applying Color settings.");
                                    var result = displayHelper.ColorControl(myDisplay.ColorData);
                                    if (result.HasValue)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied Color settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: ColorControl returned null for display {displayId}. The Color settings may not have been applied.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Color data is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting Color data for display {displayId}.");
                            }
                        }

                        // === HDR Color Data ===
                        if (myDisplay.HasNvHdrEnabled && isDigitalWithProtocol)
                        {
                            try
                            {
                                bool hdrDifferent = !hasActiveDisplay || !activeDisplay.HasNvHdrEnabled || !myDisplay.HdrColorData.Equals(activeDisplay.HdrColorData);
                                if (hdrDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: HDR Color data is different for display {displayId}. Applying HDR Color settings.");
                                    var result = displayHelper.HdrColorControl(myDisplay.HdrColorData);
                                    if (result.HasValue)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied HDR Color settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: HdrColorControl returned null for display {displayId}. The HDR Color settings may not have been applied.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: HDR Color data is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting HDR Color data for display {displayId}.");
                            }
                        }

                        // === Virtual Refresh Rate ===
                        if (myDisplay.HasVirtualRefreshRate && isDisplayPort)
                        {
                            try
                            {
                                bool vrrDifferent = !hasActiveDisplay || !activeDisplay.HasVirtualRefreshRate || !myDisplay.VirtualRefreshRateData.Equals(activeDisplay.VirtualRefreshRateData);
                                if (vrrDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Virtual Refresh Rate data is different for display {displayId}. Applying settings.");
                                    bool success = displayHelper.SetVirtualRefreshRateData(myDisplay.VirtualRefreshRateData);
                                    if (success)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied Virtual Refresh Rate settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: SetVirtualRefreshRateData returned false for display {displayId}.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Virtual Refresh Rate data is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting Virtual Refresh Rate data for display {displayId}.");
                            }
                        }

                        // === Preferred Stereo Display ===
                        if (myDisplay.HasPreferredStereoDisplay)
                        {
                            try
                            {
                                bool stereoDifferent = !hasActiveDisplay || !activeDisplay.HasPreferredStereoDisplay || !myDisplay.PreferredStereoDisplay.Equals(activeDisplay.PreferredStereoDisplay);
                                if (stereoDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Preferred Stereo Display data is different for display {displayId}. Applying settings.");
                                    bool success = displayHelper.SetPreferredStereoDisplay(myDisplay.PreferredStereoDisplay);
                                    if (success)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied Preferred Stereo Display settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: SetPreferredStereoDisplay returned false for display {displayId}.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Preferred Stereo Display data is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting Preferred Stereo Display data for display {displayId}.");
                            }
                        }

                        // === Source Color Space ===
                        if (myDisplay.HasSourceColorSpace && isDigitalWithProtocol)
                        {
                            try
                            {
                                bool colorSpaceDifferent = !hasActiveDisplay || !activeDisplay.HasSourceColorSpace || !myDisplay.SourceColorSpace.Equals(activeDisplay.SourceColorSpace);
                                if (colorSpaceDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Source Color Space is different for display {displayId}. Applying settings.");
                                    bool success = displayHelper.SetSourceColorSpace(myDisplay.SourceColorSpace);
                                    if (success)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied Source Color Space settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: SetSourceColorSpace returned false for display {displayId}.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Source Color Space is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting Source Color Space for display {displayId}.");
                            }
                        }

                        // === Source HDR Metadata ===
                        if (myDisplay.HasSourceHdrMetadata && isDigitalWithProtocol)
                        {
                            try
                            {
                                bool hdrMetadataDifferent = !hasActiveDisplay || !activeDisplay.HasSourceHdrMetadata || !myDisplay.SourceHdrMetadata.Equals(activeDisplay.SourceHdrMetadata);
                                if (hdrMetadataDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Source HDR Metadata is different for display {displayId}. Applying settings.");
                                    bool success = displayHelper.SetSourceHdrMetadata(myDisplay.SourceHdrMetadata);
                                    if (success)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied Source HDR Metadata settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: SetSourceHdrMetadata returned false for display {displayId}.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Source HDR Metadata is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting Source HDR Metadata for display {displayId}.");
                            }
                        }

                        // === Output Mode ===
                        if (myDisplay.HasOutputMode && isDigitalWithProtocol)
                        {
                            try
                            {
                                bool outputModeDifferent = !hasActiveDisplay || !activeDisplay.HasOutputMode || !myDisplay.OutputMode.Equals(activeDisplay.OutputMode);
                                if (outputModeDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Output Mode is different for display {displayId}. Applying settings.");
                                    bool success = displayHelper.SetOutputMode(myDisplay.OutputMode);
                                    if (success)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied Output Mode settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: SetOutputMode returned false for display {displayId}.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Output Mode is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting Output Mode for display {displayId}.");
                            }
                        }

                        // === HDR Tone Mapping ===
                        if (myDisplay.HasHdrToneMapping && isDigitalWithProtocol)
                        {
                            try
                            {
                                bool toneMappingDifferent = !hasActiveDisplay || !activeDisplay.HasHdrToneMapping || !myDisplay.HdrToneMapping.Equals(activeDisplay.HdrToneMapping);
                                if (toneMappingDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: HDR Tone Mapping is different for display {displayId}. Applying settings.");
                                    bool success = displayHelper.SetHdrToneMapping(myDisplay.HdrToneMapping);
                                    if (success)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied HDR Tone Mapping settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: SetHdrToneMapping returned false for display {displayId}.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: HDR Tone Mapping is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting HDR Tone Mapping for display {displayId}.");
                            }
                        }

                        // === InfoFrame Data ===
                        if (myDisplay.HasInfoFrameData && isDigitalWithProtocol)
                        {
                            try
                            {
                                bool infoFrameDifferent = !hasActiveDisplay || !activeDisplay.HasInfoFrameData || !myDisplay.InfoFrameData.Equals(activeDisplay.InfoFrameData);
                                if (infoFrameDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: InfoFrame data is different for display {displayId}. Applying settings.");
                                    var result = displayHelper.InfoFrameControl(myDisplay.InfoFrameData);
                                    if (result.HasValue)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied InfoFrame settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: InfoFrameControl returned null for display {displayId}. The InfoFrame settings may not have been applied.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: InfoFrame data is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting InfoFrame data for display {displayId}.");
                            }
                        }

                        // Disabled the Adaptive Sync equality matching as we are having trouble applying it, which is causing issues in profile matching in DisplayMagician
                        // To fix this bit, we need to test the SetActiveConfigOverride Adaptive Sync part of the codebase to apply this properly.
                        // But for now, we'll exclude it from the equality matching and also stop trying to use the adaptive sync config.

                        /*// === Adaptive Sync ===
                        if (myDisplay.HasAdaptiveSync)
                        {
                            try
                            {
                                bool adaptiveSyncDifferent = !hasActiveDisplay || !activeDisplay.HasAdaptiveSync || !myDisplay.AdaptiveSyncConfig.Equals(activeDisplay.AdaptiveSyncConfig);
                                if (adaptiveSyncDifferent)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Adaptive Sync data is different for display {displayId}. Applying settings.");
                                    bool success = displayHelper.SetAdaptiveSyncData(myDisplay.AdaptiveSyncConfig);
                                    if (success)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Successfully applied Adaptive Sync settings for display {displayId}.");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Warn($"NVIDIALibrary/SetActiveConfigOverride: SetAdaptiveSyncData returned false for display {displayId}.");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfigOverride: Adaptive Sync data is already correct for display {displayId}. Skipping.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"NVIDIALibrary/SetActiveConfigOverride: Exception while setting Adaptive Sync data for display {displayId}.");
                            }
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

            if (!_initialised || !displayConfig.IsInUse)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsValidConfig: The NVIDIA display configuration is not in use, so it has no bearing in terms of whether it can be applied now. Returning true.");
                return true;
            }

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

            if (!_initialised || !displayConfig.IsInUse)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsPossibleConfig: The NVIDIA display configuration is not in use, so it has no bearing in terms of whether it can be applied now. Returning true.");
                return true;
            }

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

        // public static bool MosaicIsOn()
        // {
        //     PhysicalGPUHandle[] physicalGpus = new PhysicalGPUHandle[NvConstants.NVAPI_MAX_PHYSICAL_GPUS];
        //     try
        //     {
        //         SharedLogger.logger.Trace($"NVIDIALibrary/MosaicIsOn: Attempting to get the list of physical GPUs.");
        //         physicalGpus = NVAPI.EnumPhysicalGPUs();
        //         SharedLogger.logger.Trace($"NVIDIALibrary/MosaicIsOn: Successfully got the list of physical GPUS. There are {physicalGpus.Length} Physical GPUs.");

        //         // If we have a physical GPU
        //         if (physicalGpus.Length > 0)
        //         {
        //             // Get current Mosaic Topology settings in brief (check whether Mosaic is on)
        //             TopologyBrief mosaicTopoBrief = new TopologyBrief();
        //             IDisplaySettings mosaicDisplaySetting = new DisplaySettingsV2();
        //             int mosaicOverlapX = 0;
        //             int mosaicOverlapY = 0;
        //             try
        //             {
        //                 SharedLogger.logger.Trace($"NVIDIALibrary/MosaicIsOn: Attempting to get the mosaic topology.");
        //                 NVAPI.GetCurrentTopology(out mosaicTopoBrief, out mosaicDisplaySetting, out mosaicOverlapX, out mosaicOverlapY);
        //                 SharedLogger.logger.Trace($"NVIDIALibrary/MosaicIsOn: Successfully got the mosaic topology. The mosaic topology is {physicalGpus.Length} Physical GPUs.");
        //                 DisplaySettingsV2 mosaicDisplaySettingv2 = (DisplaySettingsV2)mosaicDisplaySetting;

        //                 // Check if there is a topology and that Mosaic is enabled
        //                 if (mosaicTopoBrief.Topology != Topology.None && mosaicTopoBrief.IsEnable)
        //                 {
        //                     return true;
        //                 }

        //             }
        //             catch (Exception ex)
        //             {
        //                 SharedLogger.logger.Error(ex, $"NVIDIALibrary/MosaicIsOn: Exception occurred whilst getting the list pf physical GPUs.");
        //                 return false;
        //             }
        //         }

        //     }
        //     catch (Exception ex)
        //     {
        //         SharedLogger.logger.Error(ex, $"NVIDIALibrary/MosaicIsOn: Exception occurred whilst getting the list pf physical GPUs.");
        //     }
        //     return false;
        // }

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
            SharedLogger.logger.Debug($"NVIDIALibrary/GetSomeDisplayIdentifiers: Generating the unique Display Identifiers for the currently active configuration");

            List<string> displayIdentifiers = new List<string>();
            failure = false;

            if (_initialised && _nvapiApiHelper != null)
            {
                // Enumerate the physical GPU adapters in the system
                var adapters = _nvapiApiHelper.EnumeratePhysicalGpus();
                int adapterNum = 0;

                // Go through each adapter
                foreach (var adapter in adapters)
                {
                    // Get display IDs directly from the physical GPU.
                    // GetAllDisplayIds / GetConnectedDisplayIds call NvAPI_GPU_GetAllDisplayIds /
                    // NvAPI_GPU_GetConnectedDisplayIds respectively, which enumerate individual
                    // physical display IDs even when a surround/mosaic group is active (unlike
                    // EnumAllDisplays which only sees the single virtual surround handle).
                    NVAPIGpuDisplayIdDto[] displayIds;
                    if (allDisplays)
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the list of all display IDs for physical GPU #{adapterNum + 1}.");
                        displayIds = adapter.GetAllDisplayIds();
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got {displayIds.Length} display IDs for physical GPU #{adapterNum + 1}.");

                        // Update the shared list of all connected display IDs
                        _allConnectedDisplayIds.Clear();
                        foreach (var d in displayIds)
                            _allConnectedDisplayIds.Add(d.DisplayId);
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the list of connected display IDs for physical GPU #{adapterNum + 1}.");
                        displayIds = adapter.GetConnectedDisplayIds();
                        SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got {displayIds.Length} connected display IDs for physical GPU #{adapterNum + 1}.");
                    }

                    // Set some adapter-specific items we will use later
                    var gpuName = adapter.GetFullName() ?? "";
                    var gpuBusType = adapter.GetBusType();
                    var gpuBusId = adapter.GetBusId();

                    int displayNum = 0;
                    foreach (var displayIdDto in displayIds)
                    {
                        // When allDisplays is false we only want active displays
                        if (!allDisplays && !displayIdDto.IsActive)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Skipping inactive display ID {displayIdDto.DisplayId} on GPU #{adapterNum + 1}.");
                            displayNum++;
                            continue;
                        }

                        // Resolve the physical output ID from the display ID so we can retrieve EDID.
                        // NvAPI_SYS_GetGpuAndOutputIdFromDisplayId works even in surround mode.
                        uint? outputId = null;
                        try
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the output ID for display ID {displayIdDto.DisplayId} on GPU #{adapterNum + 1}.");
                            outputId = adapter.GetOutputIdFromDisplayId(displayIdDto.DisplayId);
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Output ID for display ID {displayIdDto.DisplayId} is {outputId}.");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the output ID for display ID {displayIdDto.DisplayId} on GPU #{adapterNum + 1}.");
                        }

                        // The GetEDID function in NVIDIA doesn't work reliably, and often errors saying that the driver cannot get the EDID information.
                        // Lets set some EDID defaults in case the EDID doesn't work (which is likely to happen now as NVIDIA EDID is unreliable at best :( )
                        string manufacturerName = "Unknown";
                        UInt32 productCode = 0;
                        UInt32 serialNumber = 0;
                        if (outputId.HasValue)
                        {
                            try
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Attempting to get the EDID information for display index {displayNum} on adapter {adapterNum}.");
                                var edidInfo = adapter.GetEdid(outputId.Value);
                                if (edidInfo.HasValue)
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the EDID information for display index {displayNum} on adapter {adapterNum}. Parsing it now.");
                                    EDID edidParsedInfo = new EDID(edidInfo.Value.Data);
                                    manufacturerName = edidParsedInfo.ManufacturerCode;
                                    productCode = edidParsedInfo.ProductCode;
                                    serialNumber = edidParsedInfo.SerialNumber;
                                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Manufacturer name is {manufacturerName}, product code is {productCode}, serial number is {serialNumber}.");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception occurred whilst getting the EDID information for display index {displayNum} on adapter {adapterNum}. This is unfortunately common and appears to be a bug in the NVIDIA driver.");
                            }
                        }

                        // Create an array of all the important display info we need to record
                        List<string> displayInfo = new List<string>();
                        displayInfo.Add("NVIDIA");
                        try
                        {
                            displayInfo.Add(gpuName);
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting GPU Name from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(gpuBusType.HasValue ? gpuBusType.Value.ToString() : "#");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting GPU Bus Type from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(gpuBusId.HasValue ? gpuBusId.Value.ToString() : "#");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting GPU Bus ID from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(displayIdDto.ConnectorType.ToString("G"));
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting GPU Connector Type from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }
                        try
                        {
                            displayInfo.Add(manufacturerName);
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
                            displayInfo.Add(displayIdDto.DisplayId.ToString());
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting Display ID from video card. Substituting with a # instead");
                            displayInfo.Add("#");
                        }

                        // Create a display identifier out of it
                        string displayIdentifier = String.Join("|", displayInfo);
                        // Add it to the list of display identifiers so we can return it,
                        // but only add it if it doesn't already exist. Otherwise we get duplicates :/
                        if (!displayIdentifiers.Contains(displayIdentifier))
                        {
                            displayIdentifiers.Add(displayIdentifier);
                            SharedLogger.logger.Debug($"NVIDIALibrary/GetSomeDisplayIdentifiers: DisplayIdentifier detected: {displayIdentifier}");
                        }
                        else
                        {
                            SharedLogger.logger.Debug($"NVIDIALibrary/GetSomeDisplayIdentifiers: Duplicate DisplayIdentifier detected (not adding): {displayIdentifier}");
                        }

                        displayNum++;
                    }

                    adapterNum++;
                }
            }
            else
            {
                SharedLogger.logger.Error($"NVIDIALibrary/GetSomeDisplayIdentifiers: ERROR - Tried to get Displays but the NVIDIA NVAPI library isn't initialised!");
                throw new NVIDIALibraryException($"Tried to get Displays but the NVIDIA NVAPI library isn't initialised!");
            }

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;

        }

        // public static string DumpAllDRSSettings()
        // {
        //     // This bit of code dumps all the profiles in the DRS, and all the settings within that
        //     // This is really only used for debugging, but is still very useful to have!
        //     // Get the DRS Settings
        //     string stringToReturn = "";
        //     stringToReturn += $"\n****** CURRENTLY SET NVIDIA DRIVER SETTINGS (DRS) *******\n";

        //     // Set the DRS Settings
        //     DRSSessionHandle drsSessionHandle = new DRSSessionHandle();
        //     try
        //     {
        //         SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to create a DRS Session Handle.");
        //         drsSessionHandle = NVAPI.CreateSession();
        //         SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully created a DRS Session Handle.");

        //         // Load the current DRS Settings into memory
        //         SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to load the current DRS settings into memory.");
        //         NVAPI.LoadSettings(drsSessionHandle);
        //         SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully loaded the current DRS settings into memory.");


        //         // Get ALL available settings
        //         UInt32[] drsSettingIds = new UInt32[0];
        //         try
        //         {
        //             SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to enumerate all the available settings available in this NVIDIA Driver.");
        //             drsSettingIds = NVAPI.EnumAvailableSettingIds();
        //             SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully enumerated all the available settings available in this NVIDIA Driver. There are {drsSettingIds.Length} settings available");
        //             foreach (var drsSettingId in drsSettingIds)
        //             {
        //                 // Get the name of the DRS setting
        //                 string drsSettingName;
        //                 try
        //                 {
        //                     SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to get the name of this DRS setting from the NVIDIA Driver.");
        //                     drsSettingName = NVAPI.GetSettingNameFromId(drsSettingId);
        //                     SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully got the name of this DRS setting this NVIDIA Driver. THe name is '{drsSettingName}'.");
        //                     stringToReturn += $"DRS Setting: {drsSettingName}:\n";
        //                 }
        //                 catch (Exception ex)
        //                 {
        //                     SharedLogger.logger.Warn(ex, $"NVIDIALibrary/DumpAllDRSSettings: Exception getting the name of this DRS setting (ID#{drsSettingId}).");
        //                     stringToReturn += $"DRS Setting: UNKNOWN:\n";
        //                 }

        //                 // Now get the available options for this DRS setting
        //                 stringToReturn += $"OPTIONS:\n";
        //                 DRSSettingValues drsSettingValues = new DRSSettingValues();
        //                 try
        //                 {
        //                     SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to enumerate all the options a user could select for this DRS setting from the NVIDIA Driver.");
        //                     drsSettingValues = NVAPI.EnumAvailableSettingValues(drsSettingId);
        //                     SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully enumerated all the options a user could select for this DRS setting from the NVIDIA Driver.");
        //                     stringToReturn += $"    Default Value: {drsSettingValues.DefaultValueAsUnicodeString()}\n";
        //                     stringToReturn += $"    All Values: {String.Join(", ", drsSettingValues.Values)}\n";                           
        //                 }
        //                 catch (Exception ex)
        //                 {
        //                     SharedLogger.logger.Warn(ex, $"NVIDIALibrary/DumpAllDRSSettings: Exception getting the name of this DRS setting (ID#{drsSettingId}).");
        //                     stringToReturn += $"DRS Setting: UNKNOWN:\n";
        //                 }
        //             }

        //         }
        //         catch (Exception ex)
        //         {
        //             SharedLogger.logger.Warn(ex, $"NVIDIALibrary/DumpAllDRSSettings: Exception getting Display ID from video card. Substituting with a # instead");
        //         }
        //     }
        //     finally
        //     {
        //         // Destroy the DRS Session Handle to clean up
        //         SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Attempting to destroy the DRS Session handle.");
        //         NVAPI.DestroySession(drsSessionHandle);
        //         SharedLogger.logger.Trace($"NVIDIALibrary/DumpAllDRSSettings: Successfully destroyed our DRS Session Handle");
        //     }

        //     return stringToReturn;
        // }

        public NVAPIMosaicGridTopologiesDto? CreateSingleScreenMosaicTopology(NVAPIMosaicHelper mosaicHelper)
        {

            // Get Current Mosaic Grid settings
            NVAPIMosaicGridTopologiesDto? mosaicGridTopologies = null;
            try
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/CreateSingleScreenMosaicTopology: Attempting to get the current mosaic grid settings from the NVIDIA Driver.");
                mosaicGridTopologies = mosaicHelper.EnumDisplayGrids();
                SharedLogger.logger.Trace($"NVIDIALibrary/CreateSingleScreenMosaicTopology: Successfully got the current mosaic grid settings from the NVIDIA Driver.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"NVIDIALibrary/CreateSingleScreenMosaicTopology: Exception getting the current mosaic grid settings from the NVIDIA Driver.");
                return null;
            }

            if (!mosaicGridTopologies.HasValue)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/CreateSingleScreenMosaicTopology: No mosaic grid topologies were returned from the NVIDIA Driver.");
                return null;
            }

            List<NVAPIMosaicGridTopoDto> screensToReturn = new List<NVAPIMosaicGridTopoDto>();

            foreach (NVAPIMosaicGridTopoDto gridTopo in mosaicGridTopologies.Value.Grids)
            {
                for (int displayIndexToUse = 0; displayIndexToUse < gridTopo.Displays.Length; displayIndexToUse++)
                {
                    NVAPIMosaicGridTopoDisplayDto[] displayArray = new NVAPIMosaicGridTopoDisplayDto[1];
                    displayArray[0] = gridTopo.Displays[displayIndexToUse];

                    SharedLogger.logger.Trace($"NVIDIALibrary/CreateSingleScreenMosaicTopology: Creating new Grid Topology with multiple 1x1 grids based on each display in the current Mosaic grid. This will separate each display on its own.");
                    NVAPIMosaicGridTopoDto thisScreen = new NVAPIMosaicGridTopoDto(
                        1, 1,
                        false, false, false,
                        false, false, false,
                        displayArray, gridTopo.DisplaySettings);

                    screensToReturn.Add(thisScreen);
                }
            }

            return new NVAPIMosaicGridTopologiesDto(screensToReturn.ToArray());
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

        public void PatchNVIDADisplayConfig(ref NVIDIA_DISPLAY_CONFIG savedDisplayConfig, Dictionary<ulong, ulong> adapterOldToNewMap)
        {

            try
            {
                // Update the AdapterLuid in the DisplayIdInfo with the current adapter id
                SharedLogger.logger.Trace($"NVIDIALibrary/PatchNVIDADisplayConfig: Going through the display adapters to update the adapter id");
                string[] currentPAKeys = savedDisplayConfig.PhysicalAdapters.Keys.ToArray();
                var currentPALength = savedDisplayConfig.PhysicalAdapters.Count;
                for (int i = 0; i < currentPALength; i++)
                {
                    var displaysList = savedDisplayConfig.PhysicalAdapters[currentPAKeys[i]].Displays.Values.ToList();
                    if (displaysList != null && displaysList.Count > 0)
                    {
                        for (int j = 0; j < displaysList.Count; j++)
                        {
                            var displayIdInfo = displaysList[j];
                            var displayIdInfoAdapterLuid = displayIdInfo.DisplayIdInfo.AdapterLuid;

                            if (adapterOldToNewMap.ContainsKey((ulong)displayIdInfoAdapterLuid))
                            {
                                // We get here if there is a matching adapter
                                var newAdapterValue = adapterOldToNewMap[(ulong)displayIdInfoAdapterLuid];
                                displayIdInfo.DisplayIdInfo.AdapterLuid = (long)newAdapterValue;
                                SharedLogger.logger.Trace($"WinLibrary/PatchNVIDADisplayConfig: Updated DisplayIdInfo for display {displayIdInfo.DisplayId} from adapter {displayIdInfoAdapterLuid} to adapter {newAdapterValue} instead.");
                            }
                            else
                            {
                                // if there isn't a matching adapter, then we just pick the first current one and hope that works!
                                // (it is highly likely to... its only if the user has multiple graphics cards with some weird config it may break)
                                var newAdapterValue = adapterOldToNewMap.First().Value;
                                SharedLogger.logger.Warn($"WinLibrary/PatchNVIDADisplayConfig: Uh Oh. Adapter {displayIdInfoAdapterLuid} didn't have a current match! It's possible the adapter was swapped or disabled. Attempting to use adapter {newAdapterValue} instead.");
                                displayIdInfo.DisplayIdInfo.AdapterLuid = (long)newAdapterValue;
                            }
                            displaysList[j] = displayIdInfo;
                        }
                    }                   
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "WinLibrary/PatchWindowsDisplayConfig: Exception while going through the display adapters update the adapter ids");
            }
        }


    }


    [global::System.Serializable]
    public class NVIDIALibraryException : Exception
    {
        public NVIDIALibraryException() { }
        public NVIDIALibraryException(string message) : base(message) { }
        public NVIDIALibraryException(string message, Exception inner) : base(message, inner) { }
    }
}
