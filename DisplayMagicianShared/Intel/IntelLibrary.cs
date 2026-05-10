using DisplayMagicianShared;
using DisplayMagicianShared.Windows;
using IGCLWrapper;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using EDIDParser;
using Windows.Graphics.Display;

namespace DisplayMagicianShared.Intel
{
    #region Data Structures

    [StructLayout(LayoutKind.Sequential)]
    public struct INTEL_DISPLAY_WITH_SETTINGS : IEquatable<INTEL_DISPLAY_WITH_SETTINGS>
    {
        public string Name;
        public string DisplayDeviceID;
        public uint DisplayIndex;
        public uint AdapterIndex;

        public byte[] Edid; // EDID data for the display
        
        // Integer Scaling (Retro Scaling)
        public bool IsSupportedIntegerScaling;
        
        // GPU Scaling
        public bool IsSupportedGPUScaling;
        
        // Image Sharpening
        public bool IsSupportedImageSharpening;

        // Display-related DTOs
        public bool IsSupportedDisplaySettings;
        public DisplaySettingsDto DisplaySettings;
        public ScalingSettingsDto ScalingSettings;
        public SharpnessSettingsDto SharpnessSettings;
        public RetroScalingSettingsDto RetroScalingSettings;
        public DceArgsDto DynamicContrastEnhancement;
        public bool IsSupportedDynamicContrastEnhancement;
        public uint[] DynamicContrastEnhancementHistogram;
        public bool IsSupportedPowerOptimization;
        public bool IsEnabledPowerOptimization;
        public PowerOptimizationSettingsDto PowerOptimizationSettings;
        public bool IsEnabledLaceConfig;
        public LaceConfigDto LaceConfig;

        public bool IsEnabledSoftwarePsrSettings;
        public SwPsrSettingsDto SoftwarePsrSettings;
        public GenlockArgsDto GenlockArgs;
        public IntelArcSyncMonitorParamsDto IntelArcSyncMonitorParams;
        public bool IsSupportedIntelArcSync;
        public AdapterDisplayEncoderPropertiesDto AdapterDisplayEncoderProperties;

        // Display getter outputs
        public DisplayPropertiesDto DisplayProperties;
        //public ctl_device_adapter_properties_t DeviceProperties;
        public string DeviceID;
        public DisplayTimingDto DisplayTiming;
        public bool IsSupportedWireFormat;
        public WireFormatConfigDto WireFormat;
        public BrightnessGetDto Brightness;
        public ScalingCapsDto ScalingCaps;
        public SharpnessCapsDto SharpnessCaps;
        public RetroScalingCapsDto RetroScalingCaps;
        public PowerOptimizationCapsDto PowerOptimizationCaps;
        public IntelArcSyncProfileParamsDto IntelArcSyncProfile;
        public CustomModeArgsDto CustomModeArgs;
        public List<CustomSourceModeDto> CustomModes;
        public LinkedDisplayAdaptersResultDto LinkedDisplayAdapters;
        public MuxPropertiesDto MuxProperties;
        public VblankTimestampArgsDto VblankTimestamp;
        //public IntPtr ZeDeviceHandle;
        //public IntPtr ZeDriverHandle;
        public double RefreshRateHz;
        public uint ResolutionWidth;
        public uint ResolutionHeight;
        public bool IsActive;

        public INTEL_DISPLAY_WITH_SETTINGS()
        {
            Name = "";
            DisplayDeviceID = "";
            DisplayIndex = 0;
            AdapterIndex = 0;
            Edid = Array.Empty<byte>();
            IsSupportedIntegerScaling = false;
            IsSupportedGPUScaling = false;
            IsSupportedImageSharpening = false;

            IsSupportedDisplaySettings = false;
            DisplaySettings = new DisplaySettingsDto();
            ScalingSettings = new ScalingSettingsDto();
            SharpnessSettings = new SharpnessSettingsDto();
            RetroScalingSettings = new RetroScalingSettingsDto();
            DynamicContrastEnhancement = new DceArgsDto();
            IsSupportedDynamicContrastEnhancement = false;
            DynamicContrastEnhancementHistogram = Array.Empty<uint>();
            IsSupportedPowerOptimization = false;
            IsEnabledPowerOptimization = false;
            PowerOptimizationSettings = new PowerOptimizationSettingsDto();
            IsEnabledLaceConfig = false;
            LaceConfig = new LaceConfigDto();
            IsEnabledSoftwarePsrSettings = false;
            SoftwarePsrSettings = new SwPsrSettingsDto();
            GenlockArgs = new GenlockArgsDto();
            IntelArcSyncMonitorParams = new IntelArcSyncMonitorParamsDto();
            IsSupportedIntelArcSync = false;
            AdapterDisplayEncoderProperties = new AdapterDisplayEncoderPropertiesDto();

            DisplayProperties = new DisplayPropertiesDto();
            //DeviceProperties = new ctl_device_adapter_properties_t();
            DeviceID = "";
            DisplayTiming = new DisplayTimingDto();
            IsSupportedWireFormat = false;
            WireFormat = new WireFormatConfigDto();
            Brightness = new BrightnessGetDto();
            ScalingCaps = new ScalingCapsDto();
            SharpnessCaps = new SharpnessCapsDto();
            RetroScalingCaps = new RetroScalingCapsDto();
            PowerOptimizationCaps = new PowerOptimizationCapsDto();
            IntelArcSyncProfile = new IntelArcSyncProfileParamsDto();
            CustomModeArgs = new CustomModeArgsDto();
            CustomModes = new List<CustomSourceModeDto>();
            LinkedDisplayAdapters = new LinkedDisplayAdaptersResultDto();
            MuxProperties = new MuxPropertiesDto();
            VblankTimestamp = new VblankTimestampArgsDto();
            //ZeDeviceHandle = IntPtr.Zero;
            //ZeDriverHandle = IntPtr.Zero;
            RefreshRateHz = 0.0;
            ResolutionWidth = 0;
            ResolutionHeight = 0;
            IsActive = false;
        }

        internal static bool AreDisplaySettingsEqual(DisplaySettingsDto left, DisplaySettingsDto right)
        {
            return left.Size == right.Size &&
                left.Version == right.Version &&
                left.Set == right.Set &&
                left.SupportedFlags == right.SupportedFlags &&
                left.ControllableFlags == right.ControllableFlags &&
                left.ValidFlags == right.ValidFlags &&
                left.LowLatency == right.LowLatency &&
                left.SourceTm == right.SourceTm &&
                left.ContentType == right.ContentType &&
                left.QuantizationRange == right.QuantizationRange &&
                left.SupportedPictureAr == right.SupportedPictureAr &&
                left.PictureAr == right.PictureAr &&
                left.AudioSettings == right.AudioSettings;
        }

        internal static int GetDisplaySettingsHash(DisplaySettingsDto settings)
        {
            var hash = new HashCode();
            hash.Add(settings.Size);
            hash.Add(settings.Version);
            hash.Add(settings.Set);
            hash.Add(settings.SupportedFlags);
            hash.Add(settings.ControllableFlags);
            hash.Add(settings.ValidFlags);
            hash.Add(settings.LowLatency);
            hash.Add(settings.SourceTm);
            hash.Add(settings.ContentType);
            hash.Add(settings.QuantizationRange);
            hash.Add(settings.SupportedPictureAr);
            hash.Add(settings.PictureAr);
            hash.Add(settings.AudioSettings);
            return hash.ToHashCode();
        }

        public override bool Equals(object obj) => obj is INTEL_DISPLAY_WITH_SETTINGS other && Equals(other);
        
        public bool Equals(INTEL_DISPLAY_WITH_SETTINGS other)
        {
            if (Name != other.Name)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The Name values don't equal each other");
                return false;
            }
            if (DisplayDeviceID != other.DisplayDeviceID)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The DisplayDeviceID values don't equal each other");
                return false;
            }
            if (DisplayIndex != other.DisplayIndex)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The DisplayIndex values don't equal each other");
                return false;
            }
            if (AdapterIndex != other.AdapterIndex)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The AdapterIndex values don't equal each other");
                return false;
            }
            if (Edid.Length != other.Edid.Length || !Edid.SequenceEqual(other.Edid))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The Edid values don't equal each other");
                return false;
            }
            if (IsSupportedIntegerScaling != other.IsSupportedIntegerScaling)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedIntegerScaling values don't equal each other");
                return false;
            }
            if (IsSupportedGPUScaling != other.IsSupportedGPUScaling)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedGPUScaling values don't equal each other");
                return false;
            }
            if (IsSupportedImageSharpening != other.IsSupportedImageSharpening)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedImageSharpening values don't equal each other");
                return false;
            }
            if (IsSupportedDisplaySettings != other.IsSupportedDisplaySettings)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedDisplaySettings values don't equal each other");
                return false;
            }
            if (!AreDisplaySettingsEqual(DisplaySettings, other.DisplaySettings))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The DisplaySettings values don't equal each other");
                return false;
            }
            if (!EqualityComparer<ScalingSettingsDto>.Default.Equals(ScalingSettings, other.ScalingSettings))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The ScalingSettings values don't equal each other");
                return false;
            }
            if (!EqualityComparer<SharpnessSettingsDto>.Default.Equals(SharpnessSettings, other.SharpnessSettings))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The SharpnessSettings values don't equal each other");
                return false;
            }
            if (!EqualityComparer<RetroScalingSettingsDto>.Default.Equals(RetroScalingSettings, other.RetroScalingSettings))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The RetroScalingSettings values don't equal each other");
                return false;
            }
            if (IsSupportedDynamicContrastEnhancement != other.IsSupportedDynamicContrastEnhancement)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedDynamicContrastEnhancement values don't equal each other");
                return false;
            }
            if (!EqualityComparer<DceArgsDto>.Default.Equals(DynamicContrastEnhancement, other.DynamicContrastEnhancement))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The DynamicContrastEnhancement values don't equal each other");
                return false;
            }
            if (!DynamicContrastEnhancementHistogram.SequenceEqual(other.DynamicContrastEnhancementHistogram))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The DynamicContrastEnhancementHistogram values don't equal each other");
                return false;
            }
            if (IsSupportedPowerOptimization != other.IsSupportedPowerOptimization)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedPowerOptimization values don't equal each other");
                return false;
            }
            if (IsEnabledPowerOptimization != other.IsEnabledPowerOptimization)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsEnabledPowerOptimization values don't equal each other");
                return false;
            }
            if (!EqualityComparer<PowerOptimizationSettingsDto>.Default.Equals(PowerOptimizationSettings, other.PowerOptimizationSettings))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The PowerOptimizationSettings values don't equal each other");
                return false;
            }
            if (IsEnabledLaceConfig != other.IsEnabledLaceConfig)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedLaceConfig values don't equal each other");
                return false;
            }
            if (!EqualityComparer<LaceConfigDto>.Default.Equals(LaceConfig, other.LaceConfig))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The LaceConfig values don't equal each other");
                return false;
            }
            if (IsEnabledSoftwarePsrSettings != other.IsEnabledSoftwarePsrSettings)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsEnabledSoftwarePsrSettings values don't equal each other");
                return false;
            }
            if (!EqualityComparer<SwPsrSettingsDto>.Default.Equals(SoftwarePsrSettings, other.SoftwarePsrSettings))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The SoftwarePsrSettings values don't equal each other");
                return false;
            }
            if (!EqualityComparer<GenlockArgsDto>.Default.Equals(GenlockArgs, other.GenlockArgs))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The GenlockArgs values don't equal each other");
                return false;
            }
            if (IsSupportedIntelArcSync != other.IsSupportedIntelArcSync)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedIntelArcSync values don't equal each other");
                return false;
            }
            if (!EqualityComparer<IntelArcSyncMonitorParamsDto>.Default.Equals(IntelArcSyncMonitorParams, other.IntelArcSyncMonitorParams))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IntelArcSyncMonitorParams values don't equal each other");
                return false;
            }
            if (!EqualityComparer<AdapterDisplayEncoderPropertiesDto>.Default.Equals(AdapterDisplayEncoderProperties, other.AdapterDisplayEncoderProperties))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The AdapterDisplayEncoderProperties values don't equal each other");
                return false;
            }
            if (!DisplayProperties.Equals(other.DisplayProperties))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The DisplayProperties values don't equal each other");
                return false;
            }
            // if (!EqualityComparer<ctl_device_adapter_properties_t>.Default.Equals(DeviceProperties, other.DeviceProperties))
            // {
            //     SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The DeviceProperties values don't equal each other");
            //     return false;
            // }
            if (DeviceID != other.DeviceID)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The DeviceID values don't equal each other");
                return false;
            }
            if (!EqualityComparer<DisplayTimingDto>.Default.Equals(DisplayTiming, other.DisplayTiming))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The DisplayTiming values don't equal each other");
                return false;
            }
            if (IsSupportedWireFormat != other.IsSupportedWireFormat)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsSupportedWireFormat values don't equal each other");
                return false;
            }
            if (!WireFormat.Equals(other.WireFormat))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The WireFormat values don't equal each other");
                return false;
            }
            if (!EqualityComparer<BrightnessGetDto>.Default.Equals(Brightness, other.Brightness))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The Brightness values don't equal each other");
                return false;
            }
            if (!EqualityComparer<ScalingCapsDto>.Default.Equals(ScalingCaps, other.ScalingCaps))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The ScalingCaps values don't equal each other");
                return false;
            }
            if (!EqualityComparer<SharpnessCapsDto>.Default.Equals(SharpnessCaps, other.SharpnessCaps))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The SharpnessCaps values don't equal each other");
                return false;
            }
            if (!EqualityComparer<RetroScalingCapsDto>.Default.Equals(RetroScalingCaps, other.RetroScalingCaps))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The RetroScalingCaps values don't equal each other");
                return false;
            }
            if (!EqualityComparer<PowerOptimizationCapsDto>.Default.Equals(PowerOptimizationCaps, other.PowerOptimizationCaps))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The PowerOptimizationCaps values don't equal each other");
                return false;
            }
            if (!EqualityComparer<IntelArcSyncProfileParamsDto>.Default.Equals(IntelArcSyncProfile, other.IntelArcSyncProfile))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IntelArcSyncProfile values don't equal each other");
                return false;
            }
            if (!EqualityComparer<CustomModeArgsDto>.Default.Equals(CustomModeArgs, other.CustomModeArgs))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The CustomModeArgs values don't equal each other");
                return false;
            }
            if (!(CustomModes ?? new List<CustomSourceModeDto>()).SequenceEqual(other.CustomModes ?? new List<CustomSourceModeDto>()))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The CustomModes values don't equal each other");
                return false;
            }
            if (!EqualityComparer<LinkedDisplayAdaptersResultDto>.Default.Equals(LinkedDisplayAdapters, other.LinkedDisplayAdapters))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The LinkedDisplayAdapters values don't equal each other");
                return false;
            }
            if (!EqualityComparer<MuxPropertiesDto>.Default.Equals(MuxProperties, other.MuxProperties))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The MuxProperties values don't equal each other");
                return false;
            }
            if (!EqualityComparer<VblankTimestampArgsDto>.Default.Equals(VblankTimestamp, other.VblankTimestamp))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The VblankTimestamp values don't equal each other");
                return false;
            }
            // if (ZeDeviceHandle != other.ZeDeviceHandle)
            // {
            //     SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The ZeDeviceHandle values don't equal each other");
            //     return false;
            // }
            // if (ZeDriverHandle != other.ZeDriverHandle)
            // {
            //     SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The ZeDriverHandle values don't equal each other");
            //     return false;
            // }
            if (Math.Abs(RefreshRateHz - other.RefreshRateHz) > 0.001)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The RefreshRateHz values don't equal each other");
                return false;
            }
            if (ResolutionWidth != other.ResolutionWidth)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The ResolutionWidth values don't equal each other");
                return false;
            }
            if (ResolutionHeight != other.ResolutionHeight)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The ResolutionHeight values don't equal each other");
                return false;
            }
            if (IsActive != other.IsActive)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_WITH_SETTINGS/Equals: The IsActive values don't equal each other");
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {

            return (Name, DisplayDeviceID, DisplayIndex, AdapterIndex, IsSupportedIntegerScaling, IsSupportedGPUScaling, IsSupportedImageSharpening,
                IsSupportedDisplaySettings, GetDisplaySettingsHash(DisplaySettings), ScalingSettings, SharpnessSettings, RetroScalingSettings, IsSupportedDynamicContrastEnhancement, DynamicContrastEnhancement, DynamicContrastEnhancementHistogram?.Length, PowerOptimizationSettings, LaceConfig, SoftwarePsrSettings, GenlockArgs, IsSupportedIntelArcSync, IntelArcSyncMonitorParams, AdapterDisplayEncoderProperties, 
                DisplayProperties, /*DeviceProperties,*/ DeviceID, DisplayTiming, WireFormat, Brightness, ScalingCaps, SharpnessCaps, RetroScalingCaps, PowerOptimizationCaps, IntelArcSyncProfile, CustomModeArgs, CustomModes?.Count, LinkedDisplayAdapters, MuxProperties, 
                VblankTimestamp, /*ZeDeviceHandle, ZeDriverHandle,*/ RefreshRateHz, ResolutionWidth, ResolutionHeight, IsActive).GetHashCode();
        }

        public static bool operator ==(INTEL_DISPLAY_WITH_SETTINGS lhs, INTEL_DISPLAY_WITH_SETTINGS rhs) => lhs.Equals(rhs);
        public static bool operator !=(INTEL_DISPLAY_WITH_SETTINGS lhs, INTEL_DISPLAY_WITH_SETTINGS rhs) => !(lhs == rhs);
    }

     [StructLayout(LayoutKind.Sequential)]
    public struct INTEL_ADAPTER : IEquatable<INTEL_ADAPTER>
    {
        public string AdapterID;
        public string Name;
        public uint AdapterIndex;
        public DeviceAdapterPropertiesDto AdapterProperties;
        public bool CombinedDisplayIsSupported;
        public bool IsCombinedDisplay;
        //public INTEL_COMBINED_DISPLAY CombinedDisplay;
        public CombinedDisplayArgsDto CombinedDisplay;
        // Per-adapter 3D feature settings
        public bool IsSupportedThreeDSettings;
        public List<ThreeDFeatureGetSetDto> ThreeDSettings;
        // Per-adapter video processing (media) settings
        public bool IsSupportedVideoProcessing;
        public List<VideoProcessingFeatureGetSetDto> VideoProcessingSettings;

        public INTEL_ADAPTER()
        {
            AdapterID = "";
            Name = "";
            AdapterIndex = 0;
            AdapterProperties = new DeviceAdapterPropertiesDto();
            CombinedDisplayIsSupported = false;
            IsCombinedDisplay = false;
            //CombinedDisplay = new INTEL_COMBINED_DISPLAY();
            CombinedDisplay = new CombinedDisplayArgsDto();
            CombinedDisplay.ChildInfos = new List<CombinedDisplayChildInfoDto>();
            CombinedDisplay.IsSupported = false;
            IsSupportedThreeDSettings = false;
            ThreeDSettings = new List<ThreeDFeatureGetSetDto>();
            IsSupportedVideoProcessing = false;
            VideoProcessingSettings = new List<VideoProcessingFeatureGetSetDto>();
        }
        public override bool Equals(object obj) => obj is INTEL_ADAPTER other && Equals(other);
        
        public  bool Equals(INTEL_ADAPTER other)
        {
            if (AdapterID != other.AdapterID)
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The AdapterID values don't equal each other");
                return false;
            }
            if (Name != other.Name)
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The Name values don't equal each other");
                return false;
            }   
            if (AdapterIndex != other.AdapterIndex)
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The AdapterIndex values don't equal each other");
                return false;
            }
            if (!AdapterProperties.Equals(other.AdapterProperties))
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The AdapterProperties values don't equal each other");
                return false;
            }
            if (CombinedDisplayIsSupported != other.CombinedDisplayIsSupported)
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The CombinedDisplayIsSupported values don't equal each other");
                return false;
            }
            if (IsCombinedDisplay != other.IsCombinedDisplay)
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The IsCombinedDisplay values don't equal each other");
                return false;
            }
            if (!CombinedDisplay.Equals(other.CombinedDisplay))
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The CombinedDisplay values don't equal each other");
                return false;
            }
            if (IsSupportedThreeDSettings != other.IsSupportedThreeDSettings)
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The IsSupportedThreeDSettings values don't equal each other");
                return false;
            }
            if (!(ThreeDSettings ?? new List<ThreeDFeatureGetSetDto>()).SequenceEqual(other.ThreeDSettings ?? new List<ThreeDFeatureGetSetDto>()))
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The ThreeDSettings values don't equal each other");
                return false;
            }
            if (IsSupportedVideoProcessing != other.IsSupportedVideoProcessing)
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The IsSupportedVideoProcessing values don't equal each other");
                return false;
            }
            if (!(VideoProcessingSettings ?? new List<VideoProcessingFeatureGetSetDto>()).SequenceEqual(other.VideoProcessingSettings ?? new List<VideoProcessingFeatureGetSetDto>()))
            {
                SharedLogger.logger.Trace($"INTEL_ADAPTER/Equals: The VideoProcessingSettings values don't equal each other");
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (AdapterID, Name, AdapterIndex, AdapterProperties, CombinedDisplayIsSupported, IsCombinedDisplay, CombinedDisplay, IsSupportedThreeDSettings, ThreeDSettings?.Count, IsSupportedVideoProcessing, VideoProcessingSettings?.Count).GetHashCode();
        }

        public static bool operator ==(INTEL_ADAPTER lhs, INTEL_ADAPTER rhs) => lhs.Equals(rhs);
        public static bool operator !=(INTEL_ADAPTER lhs, INTEL_ADAPTER rhs) => !(lhs == rhs);
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct INTEL_DISPLAY_CONFIG : IEquatable<INTEL_DISPLAY_CONFIG>
    {
        public bool IsInUse;
        public bool CombinedDisplayIsInUse;
        public Dictionary<string, INTEL_ADAPTER> PhysicalAdapters;  // Key is adapter ID
        public Dictionary<string, INTEL_DISPLAY_WITH_SETTINGS> Displays;  // Key is display ID
        public List<string> DisplayIdentifiers;

        public INTEL_DISPLAY_CONFIG()
        {
            IsInUse = false;
            CombinedDisplayIsInUse = false;
            PhysicalAdapters = new Dictionary<string, INTEL_ADAPTER>();
            Displays = new Dictionary<string, INTEL_DISPLAY_WITH_SETTINGS>();
            DisplayIdentifiers = new List<string>();
        }

        public override bool Equals(object obj) => obj is INTEL_DISPLAY_CONFIG other && Equals(other);
        
        public bool Equals(INTEL_DISPLAY_CONFIG other)
        {
            if (IsInUse != other.IsInUse)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_CONFIG/Equals: The IsInUse values don't equal each other");
                return false;
            }
            if (CombinedDisplayIsInUse != other.CombinedDisplayIsInUse)
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_CONFIG/Equals: The CombinedDisplayIsInUse values don't equal each other");
                return false;
            }            
            if (!PhysicalAdapters.SequenceEqual(other.PhysicalAdapters))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_CONFIG/Equals: The PhysicalAdapters values don't equal each other");
                return false;
            }
            if (!Displays.SequenceEqual(other.Displays))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_CONFIG/Equals: The Displays values don't equal each other");
                return false;
            }            
            if (!DisplayIdentifiers.SequenceEqual(other.DisplayIdentifiers))
            {
                SharedLogger.logger.Trace($"INTEL_DISPLAY_CONFIG/Equals: The DisplayIdentifiers values don't equal each other");
                return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return (IsInUse, CombinedDisplayIsInUse, PhysicalAdapters, Displays, DisplayIdentifiers).GetHashCode();
        }

        public static bool operator ==(INTEL_DISPLAY_CONFIG lhs, INTEL_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);
        public static bool operator !=(INTEL_DISPLAY_CONFIG lhs, INTEL_DISPLAY_CONFIG rhs) => !(lhs == rhs);
    }

    #endregion

    public class IntelLibrary : IDisposable
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
        private static IntelLibrary _instance = new IntelLibrary();

        private bool _initialised = false;
        
        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        
        // IGCL API Handle
        private IGCLApiHelper _igclApiHelper;
        
        private INTEL_DISPLAY_CONFIG? _activeDisplayConfig;
        public List<string> _allConnectedDisplayIdentifiers;
        public IntPtr hIGCLModule = IntPtr.Zero;
        public const string Intel_IGCL_DLL = "ControlLib.dll";
        public IntPtr hIGCLBindingModule = IntPtr.Zero;
        public const string INTEL_IGCL_BINDING_DLL = "IGCLWrapper.dll";

        const uint IGCL_IMPL_MAJOR = 1;
        const uint IGCL_IMPL_MINOR = 1;
        const uint IGCL_VERSION = (IGCL_IMPL_MAJOR << 16) | (IGCL_IMPL_MINOR & 0x0000FFFF);

        static IntelLibrary() { }
        
        public IntelLibrary()
        {
            _activeDisplayConfig = CreateDefaultConfig();
            _allConnectedDisplayIdentifiers = new List<string>();
            
            try
            {
                _initialised = false;
                
                // Check if there is Intel hardware installed
                SharedLogger.logger.Trace($"IntelLibrary/IntelLibrary: Looking for Intel PCI hardware...");
                if (!WinLibrary.IsPCIVideoCardVendorInstalled(PCIVendorIDs))
                {
                    SharedLogger.logger.Trace($"IntelLibrary/IntelLibrary: No Intel GPU hardware detected.");
                    return;
                }
                else
                {
                    SharedLogger.logger.Trace($"IntelLibrary/IntelLibrary: Intel GPU hardware detected");
                }

                // Confirm the IGCL DLL is available before attempting to initialise
                if (!IGCLApiHelper.IsIGCLDllAvailable(out string dllError))
                {
                    _initialised = false;
                    SharedLogger.logger.Error($"IntelLibrary/IntelLibrary: Failed to load the Intel IGCL DLL. {dllError}");
                    return;
                }

                SharedLogger.logger.Trace("IntelLibrary/IntelLibrary: Intialising Intel IGCL facade helper");

                _igclApiHelper = IGCLApiHelper.Initialize();
                if (_igclApiHelper == null)
                {
                    _initialised = false;
                    SharedLogger.logger.Error("IntelLibrary/IntelLibrary: Failed to initialise Intel IGCL helper.");
                    return;
                }
                _initialised = true;
                SharedLogger.logger.Trace("IntelLibrary/IntelLibrary: Successfully initialised Intel IGCL helper.");
            }
            catch (TypeInitializationException ex)
            {
                SharedLogger.logger.Info(ex, $"IntelLibrary/IntelLibrary: TypeInitializationException trying to load the Intel IGCL DLL {Intel_IGCL_DLL}. This generally means you don't have the Intel IGCL driver installed.");
                _initialised = false;
                return;
            }
            catch (DllNotFoundException ex)
            {
                // If we get here then the Intel IGCL DLL wasn't found. We can't continue to use it, so we log the error and exit
                SharedLogger.logger.Info(ex, $"IntelLibrary/IntelLibrary: DllNotFoundException trying to load the Intel IGCL DLL {Intel_IGCL_DLL}. This generally means you don't have the Intel IGCL driver installed.");
                _initialised = false;
                return;
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Info(ex, $"IntelLibrary/IntelLibrary: A general exception trying to load the Intel IGCL DLL {Intel_IGCL_DLL}.");
                _initialised = false;
                return;
            }

            SharedLogger.logger.Trace($"IntelLibrary/IntelLibrary: Automatically getting the Intel Display Configuration");
            _activeDisplayConfig = GetActiveConfig();

            // If we failed to get the display config, then we can't continue to use the library, so we dispose of it to avoid memory leaks and exit
            if (_activeDisplayConfig == null)
            {
                _activeDisplayConfig = CreateDefaultConfig();
                SharedLogger.logger.Trace($"IntelLibrary/IntelLibrary: The active Intel Display Configuration is null. Disposing the IGCL helper to avoid memory leaks");
                _igclApiHelper.Dispose();
                SharedLogger.logger.Trace($"IntelLibrary/IntelLibrary: Setting IGCL helper to null");
                _igclApiHelper = null;
                _initialised = false;
                return;
            }

            // If we got a display config, but there are no displays, then we continue
            if (_activeDisplayConfig.Value.Displays.Count == 0 )
            {
                SharedLogger.logger.Trace($"IntelLibrary/IntelLibrary: No displays connected to the Intel GPU, so returning an empty display configuration");
                return;
            }

            SharedLogger.logger.Trace($"IntelLibrary/IntelLibrary: Automatically getting the Intel Connected Display Identifiers");
            _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers(out bool failure);
        
        }

        ~IntelLibrary()
        {
            SharedLogger.logger.Trace("IntelLibrary/~IntelLibrary: Destroying IGCL Library");
            Dispose(true);
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
            if (_igclApiHelper != null)
            {
                _igclApiHelper.Dispose();
                _igclApiHelper = null;
            }


            if (hIGCLModule != IntPtr.Zero)
            {
                SharedLogger.logger.Trace("IntelLibrary/Dispose: Freeing the Intel IGCL DLL");
                FreeLibrary(hIGCLModule);
                hIGCLModule = IntPtr.Zero;
            }

            _initialised = false;
            _disposed = true;
        }

        public static void KeepVideoCardOn()
        {
            // Not needed for Intel
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
                // Intel PCI Vendor ID
                return new List<string>() { "8086" };
            }
        }

        public INTEL_DISPLAY_CONFIG ActiveDisplayConfig
        {
            get
            {
                if(_activeDisplayConfig == null)
                {
                    SharedLogger.logger.Trace($"IntelLibrary/ActiveDisplayConfig: ActiveDisplayConfig is null, so creating a new one");
                    _activeDisplayConfig = CreateDefaultConfig();
                }
                return _activeDisplayConfig.Value;
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
                if (_activeDisplayConfig == null)
                    _activeDisplayConfig = CreateDefaultConfig();
                return _activeDisplayConfig.Value.DisplayIdentifiers;
            }
        }

        public static IntelLibrary GetLibrary()
        {
            if (_instance == null)
            {
                _instance = new IntelLibrary();
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

        public INTEL_DISPLAY_CONFIG CreateDefaultConfig()
        {
            INTEL_DISPLAY_CONFIG myDefaultConfig = new INTEL_DISPLAY_CONFIG
            {
                IsInUse = false
            };

            return myDefaultConfig;
        }

        public bool UpdateActiveConfig()
        {
            SharedLogger.logger.Trace($"IntelLibrary/UpdateActiveConfig: Updating the currently active config");
            try
            {
                _activeDisplayConfig = GetActiveConfig();
                _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers(out bool failure);
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Trace(ex, $"IntelLibrary/UpdateActiveConfig: Exception updating the currently active config");
                return false;
            }

            return true;
        }

        public INTEL_DISPLAY_CONFIG GetActiveConfig()
        {
            SharedLogger.logger.Trace($"IntelLibrary/GetActiveConfig: Getting the currently active config");
            bool allDisplays = true;
            return GetIntelDisplayConfig(allDisplays);
        }

        private INTEL_DISPLAY_CONFIG GetIntelDisplayConfig(bool allDisplays = false)
        {
            // Create empty config struct so we know there are no nulls in there to break the json serializer
            INTEL_DISPLAY_CONFIG myDisplayConfig = CreateDefaultConfig();

            if (_initialised && _igclApiHelper != null)
            {
                
                // Enumerate the Intel GPUs adapters in the sytem
                var adapters = _igclApiHelper.EnumerateAdapters();
                int adapterTotalCount = adapters.Count;
                int adapterNum = 0;

                SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Found {adapterTotalCount} Intel GPU adapter(s)");


                // Go through each adapter
                foreach (var adapter in adapters)
                {
                    adapterNum++;
                    // Get adapter properties
                    var adapterProperties = adapter.GetProperties();
                    var adapterDeviceID = $"{adapterProperties.PciVendorId:X4}_{adapterProperties.PciDeviceId:X4}_{adapterProperties.PciSubsysVendorId:X4}_{adapterProperties.PciSubsysId:X4}_{adapterProperties.RevId:X2}";

                    SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Processing Intel GPU adapter {adapterNum}({adapterProperties.Name}), PCI Device ID: 0x{adapterProperties.PciDeviceId:X4}, device type {adapterProperties.DeviceType} ({adapterNum}/{adapterTotalCount}");                    


                    // Create adapter storage struct
                    INTEL_ADAPTER newAdapter = new INTEL_ADAPTER();
                    newAdapter.AdapterID = adapterDeviceID;
                    newAdapter.Name = adapter.Name;
                    newAdapter.AdapterProperties = adapterProperties;                    

                    //------------------------------------
                    // CHECK FOR COMBINED DISPLAY CONFIGURATION PER ADAPTER
                    //------------------------------------
                    

                    try
                    {
                        var combinedDisplay = adapter.GetCombinedDisplay();
                        SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got Combined Display settings for adapter {adapterNum}");

                        if (combinedDisplay.NumOutputs > 1)
                        {
                            newAdapter.IsCombinedDisplay = true;
                            myDisplayConfig.CombinedDisplayIsInUse = true;
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Adapter {adapterNum} has a Combined Display with {combinedDisplay.NumOutputs} outputs, {combinedDisplay.CombinedDesktopWidth}x{combinedDisplay.CombinedDesktopHeight}");
                        }
                        else
                        {
                            newAdapter.IsCombinedDisplay = false;
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Adapter {adapterNum} does not currently have a Combined Display.");
                        }                            
                        newAdapter.CombinedDisplay= combinedDisplay;   
                        newAdapter.CombinedDisplayIsSupported = true;                    
                    }
                    catch (IGCLException ex)
                    {
                         if (ex.Result == ctl_result_t.CTL_RESULT_ERROR_UNSUPPORTED_FEATURE ||
                                               ex.Result == ctl_result_t.CTL_RESULT_ERROR_UNSUPPORTED_VERSION ||
                                               ex.Result == ctl_result_t.CTL_RESULT_ERROR_INVALID_OPERATION_TYPE ||
                                               ex.Result == ctl_result_t.CTL_RESULT_ERROR_INVALID_ARGUMENT)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: IGCLException: Combined Display not supported for adapter {adapterNum}. Skipping adapter.");
                            newAdapter.CombinedDisplayIsSupported = false;                            
                        }
                        else
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: IGCLException getting Combined Display settings for adapter {adapterNum}.");                        
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting Combined Display settings for adapter {adapterNum}.");
                    }

                    //------------------------------------
                    // READ PER-ADAPTER 3D SETTINGS
                    //------------------------------------
                    try
                    {
                        var threeDHelper = _igclApiHelper.Get3DHelper(adapter);
                        var threeDCaps = threeDHelper.GetSupported3DCapabilities();
                        if (threeDCaps.NumSupportedFeatures > 0)
                        {
                            newAdapter.IsSupportedThreeDSettings = true;
                            // Attempt to read each known settable 3D feature. Value types are based on the Intel
                            // IGCL specification for each feature. If a feature is not supported by the current
                            // adapter, the IGCLException is caught and the feature is skipped gracefully.
                            (ctl_3d_feature_t feature, ctl_property_value_type_t valueType)[] features3D = new[]
                            {
                                (ctl_3d_feature_t.CTL_3D_FEATURE_FRAME_PACING,              ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_BOOL),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_ENDURANCE_GAMING,          ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_ENUM),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_FRAME_LIMIT,               ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_UINT32),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_ANISOTROPIC,               ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_ENUM),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_CMAA,                      ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_ENUM),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_TEXTURE_FILTERING_QUALITY, ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_ENUM),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_ADAPTIVE_TESSELLATION,     ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_BOOL),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_SHARPENING_FILTER,         ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_FLOAT),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_MSAA,                      ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_ENUM),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_GAMING_FLIP_MODES,         ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_ENUM),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_ADAPTIVE_SYNC_PLUS,        ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_BOOL),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_LOW_LATENCY,               ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_ENUM),
                                (ctl_3d_feature_t.CTL_3D_FEATURE_FRAME_GENERATION,          ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_BOOL),
                            };
                            foreach (var (featureType, valueType) in features3D)
                            {
                                try
                                {
                                    var getRequest = IGCL3DHelper.Create3DFeatureGetRequest(featureType, valueType);
                                    var result = threeDHelper.Get3DFeature(getRequest);
                                    newAdapter.ThreeDSettings.Add(result);
                                    SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Got 3D feature {featureType} for adapter {adapterNum}");
                                }
                                catch (IGCLException ex) when (
                                    ex.Result == ctl_result_t.CTL_RESULT_ERROR_UNSUPPORTED_FEATURE ||
                                    ex.Result == ctl_result_t.CTL_RESULT_ERROR_UNSUPPORTED_VERSION ||
                                    ex.Result == ctl_result_t.CTL_RESULT_ERROR_INVALID_OPERATION_TYPE ||
                                    ex.Result == ctl_result_t.CTL_RESULT_ERROR_INVALID_ARGUMENT)
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: 3D feature {featureType} not supported for adapter {adapterNum}, skipping.");
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting 3D feature {featureType} for adapter {adapterNum}.");
                                }
                            }
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Got {newAdapter.ThreeDSettings.Count} 3D feature(s) for adapter {adapterNum}");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: No 3D features supported for adapter {adapterNum}");
                        }
                    }
                    catch (IGCLException ex)
                    {
                        SharedLogger.logger.Warn(ex, $"IntelLibrary/GetIntelDisplayConfig: IGCLException getting 3D capabilities for adapter {adapterNum}, skipping.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting 3D capabilities for adapter {adapterNum}, skipping.");
                    }

                    //------------------------------------
                    // READ PER-ADAPTER VIDEO PROCESSING (MEDIA) SETTINGS
                    //------------------------------------
                    try
                    {
                        var mediaHelper = _igclApiHelper.GetMediaHelper(adapter);
                        var mediaCaps = mediaHelper.GetSupportedVideoProcessingCapabilities();
                        if (mediaCaps.NumSupportedFeatures > 0)
                        {
                            newAdapter.IsSupportedVideoProcessing = true;
                            // Attempt to read each known video processing feature. Most video processing features
                            // are simple boolean enable/disable flags. Features that are not supported are
                            // skipped gracefully via exception handling.
                            ctl_video_processing_feature_t[] mediaFeatures = new[]
                            {
                                ctl_video_processing_feature_t.CTL_VIDEO_PROCESSING_FEATURE_FILM_MODE_DETECTION,
                                ctl_video_processing_feature_t.CTL_VIDEO_PROCESSING_FEATURE_NOISE_REDUCTION,
                                ctl_video_processing_feature_t.CTL_VIDEO_PROCESSING_FEATURE_SHARPNESS,
                                ctl_video_processing_feature_t.CTL_VIDEO_PROCESSING_FEATURE_ADAPTIVE_CONTRAST_ENHANCEMENT,
                                ctl_video_processing_feature_t.CTL_VIDEO_PROCESSING_FEATURE_SUPER_RESOLUTION,
                                ctl_video_processing_feature_t.CTL_VIDEO_PROCESSING_FEATURE_STANDARD_COLOR_CORRECTION,
                                ctl_video_processing_feature_t.CTL_VIDEO_PROCESSING_FEATURE_TOTAL_COLOR_CORRECTION,
                                ctl_video_processing_feature_t.CTL_VIDEO_PROCESSING_FEATURE_SKIN_TONE_ENHANCEMENT,
                            };
                            foreach (var featureType in mediaFeatures)
                            {
                                try
                                {
                                    var getRequest = IGCLMediaHelper.CreateVideoProcessingFeatureGetRequest(featureType, ctl_property_value_type_t.CTL_PROPERTY_VALUE_TYPE_BOOL);
                                    var result = mediaHelper.GetVideoProcessingFeature(getRequest);
                                    newAdapter.VideoProcessingSettings.Add(result);
                                    SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Got video processing feature {featureType} for adapter {adapterNum}");
                                }
                                catch (IGCLException ex) when (
                                    ex.Result == ctl_result_t.CTL_RESULT_ERROR_UNSUPPORTED_FEATURE ||
                                    ex.Result == ctl_result_t.CTL_RESULT_ERROR_UNSUPPORTED_VERSION ||
                                    ex.Result == ctl_result_t.CTL_RESULT_ERROR_INVALID_OPERATION_TYPE ||
                                    ex.Result == ctl_result_t.CTL_RESULT_ERROR_INVALID_ARGUMENT)
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Video processing feature {featureType} not supported for adapter {adapterNum}, skipping.");
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting video processing feature {featureType} for adapter {adapterNum}.");
                                }
                            }
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Got {newAdapter.VideoProcessingSettings.Count} video processing feature(s) for adapter {adapterNum}");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: No video processing features supported for adapter {adapterNum}");
                        }
                    }
                    catch (IGCLException ex)
                    {
                        SharedLogger.logger.Warn(ex, $"IntelLibrary/GetIntelDisplayConfig: IGCLException getting video processing capabilities for adapter {adapterNum}, skipping.");
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Warn(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting video processing capabilities for adapter {adapterNum}, skipping.");
                    }

                    // Add adapter to config
                    myDisplayConfig.PhysicalAdapters.Add(adapterDeviceID, newAdapter);

                    // Enumerate displays for this adapter
                    var displays = adapter.EnumerateDisplayOutputs();
                    int displayTotalCount = displays.Count;
                    int displayCount = 0;
                    
                    SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Found {displayTotalCount} display(s) on adapter {adapterNum}");

                    foreach (var display in displays)
                    {

                        displayCount++;

                        DisplayPropertiesDto displayProperties;
                        try
                        {
                            displayProperties = display.GetProperties();
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting display properties for display {display.Name} on adapter {adapterNum}. Skipping display.");
                            continue;
                        }

                        string logDisplayId = $"{display.Name}|{displayProperties.OsDisplayEncoderHandle.WindowsDisplayEncoderId}";

                        // Skip inactive displays (inactive displays are not part of the current desktop so are not in use now)
                        if (((uint)displayProperties.DisplayConfigFlags & (uint)ctl_display_config_flag_t.CTL_DISPLAY_CONFIG_FLAG_DISPLAY_ACTIVE) != (uint)ctl_display_config_flag_t.CTL_DISPLAY_CONFIG_FLAG_DISPLAY_ACTIVE)
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/GetCurrentDisplayIdentifiers: Skipping inactive display {logDisplayId} on Adapter {adapterNum}");
                            continue;
                        }

                        displayCount++;
                        SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Processing display {displayCount}/{displayTotalCount} on adapter {adapterNum}");

                        // Create display with settings
                        INTEL_DISPLAY_WITH_SETTINGS newDisplay = new INTEL_DISPLAY_WITH_SETTINGS();
                        



                        // Set basic info                     
                        newDisplay.DisplayProperties = displayProperties;
                        // Derive connector type gating booleans. DP and HDMI support the full IGCL feature set;
                        // other types (DVI, CRT/VGA, MIPI internal panel, INVALID) do not.
                        bool isDisplayPort        = displayProperties.Type == ctl_display_output_types_t.CTL_DISPLAY_OUTPUT_TYPES_DISPLAYPORT;
                        bool isHdmi               = displayProperties.Type == ctl_display_output_types_t.CTL_DISPLAY_OUTPUT_TYPES_HDMI;
                        bool isDigitalWithProtocol = isDisplayPort || isHdmi;
                        SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Display {logDisplayId} output type is {displayProperties.Type}. isDisplayPort={isDisplayPort}, isHdmi={isHdmi}, isDigitalWithProtocol={isDigitalWithProtocol}.");
                        // make up a adapter DeviceID that includes the PCI device and subsystem IDs that we can match on.
                        newDisplay.DeviceID = adapterDeviceID;
                        
                        // Get display settings
                        try
                        {
                            newDisplay.DisplaySettings = display.GetDisplaySettings();
                            newDisplay.IsSupportedDisplaySettings = Convert.ToUInt64((object)newDisplay.DisplaySettings.ValidFlags) != 0 ||
                                Convert.ToUInt64((object)newDisplay.DisplaySettings.ControllableFlags) != 0;
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got display settings for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting display settings for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        // Make a display name that works across reboots
                        try
                        {
                            
                            var manufacturerCode = "";
                            var productCode = "";
                            try {
                                var edidBytes = display.GetPanelEdidData();
                                var edid = new EDID(edidBytes);
                                newDisplay.Edid = edidBytes;
                                manufacturerCode = edid.ManufacturerCode.ToString();
                                productCode = edid.ProductCode.ToString();
                                SharedLogger.logger.Trace($"IntelLibrary/GetAllConnectedDisplayIdentifiers: Successfully got EDID for display Index {displayCount} on Adapter {adapterNum}");                            
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error($"IntelLibrary/GetAllConnectedDisplayIdentifiers: ERROR - Failed to get EDID for display Index {displayCount} on Adapter {adapterNum}: {ex.Message}");
                            }
                            newDisplay.Name = $"Display_{manufacturerCode}_{productCode}_{displayProperties.OsDisplayEncoderHandle.WindowsDisplayEncoderId}";
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting display settings for display {logDisplayId} on adapter {adapterNum}.");
                            // Backup Display name if EDID lookup fails
                            newDisplay.Name = $"Display_{logDisplayId}_{displayProperties.OsDisplayEncoderHandle.WindowsDisplayEncoderId}";
                        }


                        //------------------------------------
                        // GET DISPLAY TIMING / RESOLUTION / REFRESH / ACTIVE STATE
                        //------------------------------------
                        try
                        {
                            newDisplay.DisplayTiming = display.GetTiming();
                            (newDisplay.ResolutionWidth, newDisplay.ResolutionHeight) = display.GetResolution();
                            newDisplay.RefreshRateHz = display.GetRefreshRateHz();
                            newDisplay.IsActive = display.IsActive();
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got timing/resolution/refresh/active for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting timing/resolution/refresh/active for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET DISPLAY ENCODER PROPERTIES
                        //------------------------------------
                        try
                        {
                            newDisplay.AdapterDisplayEncoderProperties = display.GetAdapterDisplayEncoderProperties();
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got adapter display encoder properties for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting adapter display encoder properties for display {logDisplayId} on adapter {adapterNum}.");
                        }
                        
                        //------------------------------------
                        // GET INTEGER SCALING (RETRO SCALING) SETTINGS
                        //------------------------------------
                        try
                        {
                            newDisplay.RetroScalingCaps = display.GetSupportedRetroScalingCapability();
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got retro scaling caps for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                            newDisplay.RetroScalingSettings = display.GetRetroScalingSettings();
                            uint retroScalingMask =
                                (uint)(ctl_retro_scaling_type_flag_t.CTL_RETRO_SCALING_TYPE_FLAG_INTEGER |
                                    ctl_retro_scaling_type_flag_t.CTL_RETRO_SCALING_TYPE_FLAG_NEAREST_NEIGHBOUR);

                            newDisplay.IsSupportedIntegerScaling = (newDisplay.RetroScalingCaps.SupportedRetroScaling & retroScalingMask) != 0;
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Integer scaling settings for display {logDisplayId}: Supported={newDisplay.RetroScalingCaps.SupportedRetroScaling}, Enabled={newDisplay.RetroScalingSettings.Enable}, Type={newDisplay.RetroScalingSettings.RetroScalingType}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting retro scaling settings for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET GPU SCALING SETTINGS
                        //------------------------------------
                        try
                        {
                            newDisplay.ScalingCaps = display.GetSupportedScalingCapability();
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got scaling caps for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                            newDisplay.ScalingSettings = display.GetCurrentScaling();
                            uint gpuScalingMask =
                                (uint)(ctl_scaling_type_flag_t.CTL_SCALING_TYPE_FLAG_CENTERED |
                                    ctl_scaling_type_flag_t.CTL_SCALING_TYPE_FLAG_STRETCHED |
                                    ctl_scaling_type_flag_t.CTL_SCALING_TYPE_FLAG_ASPECT_RATIO_CENTERED_MAX |
                                    ctl_scaling_type_flag_t.CTL_SCALING_TYPE_FLAG_CUSTOM);

                            newDisplay.IsSupportedGPUScaling = (newDisplay.ScalingCaps.SupportedScaling & gpuScalingMask) != 0;
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: GPU scaling settings for display {logDisplayId}: Supported={newDisplay.ScalingCaps.SupportedScaling}, Enabled={newDisplay.ScalingSettings.Enable}, Type={newDisplay.ScalingSettings.ScalingType}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting scaling settings for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET IMAGE SHARPENING SETTINGS
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                newDisplay.SharpnessCaps = display.GetSharpnessCaps();
                                SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got sharpness caps for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                                newDisplay.SharpnessSettings = display.GetCurrentSharpness();
                                newDisplay.IsSupportedImageSharpening = newDisplay.SharpnessCaps.SupportedFilterFlags != 0;
                                SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Image sharpening settings for display {logDisplayId}: Enabled={newDisplay.SharpnessSettings.Enable}, FilterType={newDisplay.SharpnessSettings.FilterType}, Intensity={newDisplay.SharpnessSettings.Intensity}");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting image sharpening settings for display {logDisplayId} on adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET POWER OPTIMIZATION SETTINGS
                        //------------------------------------
                        try
                        {
                            newDisplay.PowerOptimizationCaps = display.GetPowerOptimizationCaps();
                            newDisplay.IsSupportedPowerOptimization = newDisplay.PowerOptimizationCaps.SupportedFeatures != 0;
                            if (newDisplay.IsSupportedPowerOptimization)
                            {
                                newDisplay.PowerOptimizationSettings = display.GetPowerOptimizationSetting(newDisplay.PowerOptimizationSettings);
                                newDisplay.IsEnabledPowerOptimization = newDisplay.PowerOptimizationSettings.Enable;
                            }
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got power optimization settings for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting power optimization settings for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET BRIGHTNESS SETTINGS
                        //------------------------------------
                        try
                        {
                            newDisplay.Brightness = display.GetBrightnessSetting();
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got brightness settings for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting brightness settings for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET LACE CONFIG
                        //------------------------------------
                        try
                        {
                            newDisplay.LaceConfig = display.GetLACEConfig();
                            newDisplay.IsEnabledLaceConfig = newDisplay.LaceConfig.Enabled;
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got LACE config for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting LACE config for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET SOFTWARE PSR SETTINGS
                        //------------------------------------
                        try
                        {
                            newDisplay.SoftwarePsrSettings = display.SoftwarePSR(newDisplay.SoftwarePsrSettings);
                            newDisplay.IsEnabledSoftwarePsrSettings = newDisplay.SoftwarePsrSettings.Enable;
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got Software PSR settings for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting Software PSR settings for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET INTEL ARC SYNC SETTINGS
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                newDisplay.IntelArcSyncMonitorParams = display.GetIntelArcSyncInfoForMonitor();
                                newDisplay.IsSupportedIntelArcSync = newDisplay.IntelArcSyncMonitorParams.IsIntelArcSyncSupported;
                                if (newDisplay.IsSupportedIntelArcSync)
                                {
                                    newDisplay.IntelArcSyncProfile = display.GetIntelArcSyncProfile();
                                }
                                SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got Intel Arc Sync settings for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting Intel Arc Sync settings for display {logDisplayId} on adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET WIRE FORMAT SETTINGS
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                newDisplay.WireFormat = display.GetWireFormat();
                                bool wireFormatSupported = false;
                                var supportedWireFormats = newDisplay.WireFormat.SupportedWireFormat;

                                if (supportedWireFormats != null)
                                {
                                    foreach (var supportedWireFormat in supportedWireFormats)
                                    {
                                        if (supportedWireFormat.ColorDepth != 0)
                                        {
                                            wireFormatSupported = true;
                                            break;
                                        }
                                    }
                                }

                                newDisplay.IsSupportedWireFormat = wireFormatSupported;

                                SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got wire format settings for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting wire format settings for display {logDisplayId} on adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET DYNAMIC CONTRAST ENHANCEMENT SETTINGS
                        //------------------------------------
                        if (isDigitalWithProtocol)
                        {
                            try
                            {
                                (newDisplay.DynamicContrastEnhancement, newDisplay.DynamicContrastEnhancementHistogram) = display.GetDynamicContrastEnhancement();
                                newDisplay.IsSupportedDynamicContrastEnhancement = newDisplay.DynamicContrastEnhancement.IsSupported;
                                SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got dynamic contrast enhancement settings for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting dynamic contrast enhancement settings for display {logDisplayId} on adapter {adapterNum}.");
                            }
                        }

                        //------------------------------------
                        // GET CUSTOM MODES
                        //------------------------------------
                        try
                        {
                            var customModesResult = display.GetCustomModes();
                            newDisplay.CustomModeArgs = customModesResult.Args;
                            newDisplay.CustomModes = customModesResult.Modes ?? new List<CustomSourceModeDto>();
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got custom modes for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting custom modes for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET VBLANK TIMESTAMP
                        //------------------------------------
                        try
                        {
                            newDisplay.VblankTimestamp = display.GetVblankTimestamp();
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got vblank timestamp for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting vblank timestamp for display {logDisplayId} on adapter {adapterNum}.");
                        }

                        //------------------------------------
                        // GET MUX PROPERTIES
                        //------------------------------------
                        try
                        {
                            var muxHandles = display.EnumerateMuxDevices();
                            if (muxHandles != null && muxHandles.Length > 0)
                            {
                                newDisplay.MuxProperties = display.GetMuxProperties(muxHandles[0]);
                                if (muxHandles.Length > 1)
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Multiple mux devices detected ({muxHandles.Length}); storing properties for the first one only.");
                                }
                            }
                            SharedLogger.logger.Trace($"IntelLibrary/GetIntelDisplayConfig: Successfully got mux properties for display {logDisplayId} ({displayCount}/{displayTotalCount}) on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/GetIntelDisplayConfig: Exception getting mux properties for display {logDisplayId} on adapter {adapterNum}.");
                        }


                        // 3. Create a unique Hardware PCI ID + Target ID
                        // Format: VEN_8086&DEV_XXXX&REV_XX-PORT_X
                        
                        newDisplay.DisplayDeviceID = $"VEN_{adapterProperties.PciVendorId:X4}&DEV_{adapterProperties.PciDeviceId:X4}&REV_{adapterProperties.RevId:X2}-PORT_{displayProperties.OsDisplayEncoderHandle.WindowsDisplayEncoderId}";

                        // Add display to configuration
                        myDisplayConfig.Displays.Add(newDisplay.DisplayDeviceID, newDisplay);
                    }
                    
                }

                // Get display identifiers
                myDisplayConfig.DisplayIdentifiers = GetCurrentDisplayIdentifiers(out bool failure);
                myDisplayConfig.IsInUse = true;
            }
            else
            {
                SharedLogger.logger.Error($"IntelLibrary/GetIntelDisplayConfig: ERROR - Tried to run GetIntelDisplayConfig but the Intel IGCL library isn't initialised! This generally means you don't have an Intel video card in your machine.");
                return CreateDefaultConfig();
            }
            
            // Return the configuration
            return myDisplayConfig;
        }

        public string PrintActiveConfig()
        {
            INTEL_DISPLAY_CONFIG displayConfig = ActiveDisplayConfig;
            var sb = new StringBuilder();

            sb.AppendLine("****** INTEL VIDEO CARDS *******");

            if (displayConfig.PhysicalAdapters.Count == 0)
            {
                sb.AppendLine("No Intel Video Cards detected.");
                sb.AppendLine();
                return sb.ToString();
            }

            sb.AppendLine($"Number of Intel adapters found: {displayConfig.PhysicalAdapters.Count}");
            sb.AppendLine($"Combined Display in use: {displayConfig.CombinedDisplayIsInUse}");
            sb.AppendLine();

            // Physical Adapters
            foreach (var adapterKvp in displayConfig.PhysicalAdapters)
            {
                string adapterKey = adapterKvp.Key;
                INTEL_ADAPTER myAdapter = adapterKvp.Value;

                sb.AppendLine($"Adapter: {adapterKey}");
                sb.AppendLine($"  Name: {myAdapter.Name}");
                sb.AppendLine($"  AdapterID: {myAdapter.AdapterID}");
                sb.AppendLine($"  AdapterIndex: {myAdapter.AdapterIndex}");
                sb.AppendLine($"  AdapterProperties: {myAdapter.AdapterProperties}");
                sb.AppendLine($"  CombinedDisplayIsSupported: {myAdapter.CombinedDisplayIsSupported}");
                sb.AppendLine($"  IsCombinedDisplay: {myAdapter.IsCombinedDisplay}");
                if (myAdapter.IsCombinedDisplay)
                {
                    sb.AppendLine($"  CombinedDisplay: {myAdapter.CombinedDisplay}");
                }
                // 3D Settings
                sb.AppendLine($"  IsSupportedThreeDSettings: {myAdapter.IsSupportedThreeDSettings}");
                if (myAdapter.IsSupportedThreeDSettings && myAdapter.ThreeDSettings != null && myAdapter.ThreeDSettings.Count > 0)
                {
                    sb.AppendLine($"  ThreeDSettings ({myAdapter.ThreeDSettings.Count} feature(s)):");
                    for (int i = 0; i < myAdapter.ThreeDSettings.Count; i++)
                    {
                        sb.AppendLine($"    3DFeature[{i}]: Feature={myAdapter.ThreeDSettings[i].FeatureType} ValueType={myAdapter.ThreeDSettings[i].ValueType} Value={myAdapter.ThreeDSettings[i].Value}");
                    }
                }
                // Video Processing Settings
                sb.AppendLine($"  IsSupportedVideoProcessing: {myAdapter.IsSupportedVideoProcessing}");
                if (myAdapter.IsSupportedVideoProcessing && myAdapter.VideoProcessingSettings != null && myAdapter.VideoProcessingSettings.Count > 0)
                {
                    sb.AppendLine($"  VideoProcessingSettings ({myAdapter.VideoProcessingSettings.Count} feature(s)):");
                    for (int i = 0; i < myAdapter.VideoProcessingSettings.Count; i++)
                    {
                        sb.AppendLine($"    VideoProcessingFeature[{i}]: Feature={myAdapter.VideoProcessingSettings[i].FeatureType} ValueType={myAdapter.VideoProcessingSettings[i].ValueType} Value={myAdapter.VideoProcessingSettings[i].Value}");
                    }
                }
                sb.AppendLine();
            }

            // Displays
            sb.AppendLine("INTEL DISPLAYS");
            foreach (var displayKvp in displayConfig.Displays)
            {
                string displayKey = displayKvp.Key;
                INTEL_DISPLAY_WITH_SETTINGS display = displayKvp.Value;

                sb.AppendLine($"Display: {displayKey}");
                sb.AppendLine($"  Name: {display.Name}");
                sb.AppendLine($"  DisplayDeviceID: {display.DisplayDeviceID}");
                sb.AppendLine($"  DeviceID: {display.DeviceID}");
                sb.AppendLine($"  DisplayIndex: {display.DisplayIndex} AdapterIndex: {display.AdapterIndex}");
                sb.AppendLine($"  IsActive: {display.IsActive}");
                sb.AppendLine($"  Resolution: {display.ResolutionWidth}x{display.ResolutionHeight} @ {display.RefreshRateHz}Hz");
                sb.AppendLine($"  DisplayProperties: {display.DisplayProperties}");
                sb.AppendLine($"  DisplayTiming: {display.DisplayTiming}");

                // Integer Scaling
                sb.AppendLine($"  IntegerScaling: Supported={display.IsSupportedIntegerScaling} Enabled={display.RetroScalingSettings.Enable} Type={display.RetroScalingSettings.RetroScalingType}");
                sb.AppendLine($"  RetroScalingSettings: {display.RetroScalingSettings}");
                sb.AppendLine($"  RetroScalingCaps: {display.RetroScalingCaps}");

                // GPU Scaling
                sb.AppendLine($"  GPUScaling: Supported={display.IsSupportedGPUScaling} Enabled={display.ScalingSettings.Enable} Type={display.ScalingSettings.ScalingType}");
                sb.AppendLine($"  ScalingSettings: {display.ScalingSettings}");
                sb.AppendLine($"  ScalingCaps: {display.ScalingCaps}");

                // Image Sharpening
                sb.AppendLine($"  ImageSharpening: Supported={display.IsSupportedImageSharpening} Enabled={display.SharpnessSettings.Enable} FilterType={display.SharpnessSettings.FilterType} Intensity={display.SharpnessSettings.Intensity}");
                sb.AppendLine($"  SharpnessSettings: {display.SharpnessSettings}");
                sb.AppendLine($"  SharpnessCaps: {display.SharpnessCaps}");

                // Display Settings
                if (display.IsSupportedDisplaySettings)
                {
                    sb.AppendLine($"  DisplaySettings: {display.DisplaySettings}");
                }

                // Wire Format
                if (display.IsSupportedWireFormat)
                {
                    sb.AppendLine($"  WireFormat: {display.WireFormat}");
                }

                // Brightness
                sb.AppendLine($"  Brightness: {display.Brightness}");

                // Power Optimization
                sb.AppendLine($"  PowerOptimization: Supported={display.IsSupportedPowerOptimization} Enabled={display.IsEnabledPowerOptimization}");
                sb.AppendLine($"  PowerOptimizationSettings: {display.PowerOptimizationSettings}");
                sb.AppendLine($"  PowerOptimizationCaps: {display.PowerOptimizationCaps}");

                // LACE Config
                sb.AppendLine($"  LaceConfig: Enabled={display.IsEnabledLaceConfig} Config={display.LaceConfig}");

                // Software PSR
                sb.AppendLine($"  SoftwarePsr: Enabled={display.IsEnabledSoftwarePsrSettings} Settings={display.SoftwarePsrSettings}");

                // Dynamic Contrast Enhancement
                sb.AppendLine($"  DynamicContrastEnhancement: Supported={display.IsSupportedDynamicContrastEnhancement} Settings={display.DynamicContrastEnhancement}");

                // Intel Arc Sync
                sb.AppendLine($"  IntelArcSync: Supported={display.IsSupportedIntelArcSync} MonitorParams={display.IntelArcSyncMonitorParams} Profile={display.IntelArcSyncProfile}");

                // Genlock
                sb.AppendLine($"  GenlockArgs: {display.GenlockArgs}");

                // Adapter Display Encoder Properties
                sb.AppendLine($"  AdapterDisplayEncoderProperties: {display.AdapterDisplayEncoderProperties}");

                // Custom Modes
                sb.AppendLine($"  CustomModeArgs: {display.CustomModeArgs}");
                if (display.CustomModes != null && display.CustomModes.Count > 0)
                {
                    for (int i = 0; i < display.CustomModes.Count; i++)
                    {
                        sb.AppendLine($"    CustomMode[{i}]: {display.CustomModes[i]}");
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine();

            return sb.ToString();
        }

        public bool SetActiveConfig(INTEL_DISPLAY_CONFIG displayConfig, int delayInMs)
        {
            if (_initialised && _igclApiHelper != null)
            {
                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: Managing Intel Combined Display configuration");

                var adapters = _igclApiHelper.EnumerateAdapters();
                if (adapters == null || adapters.Count == 0)
                {
                    SharedLogger.logger.Error($"IntelLibrary/SetActiveConfig: No Intel adapters found or error getting adapter count.");
                    return false;
                }

                int adapterNum = 0;
                foreach (var adapter in adapters)
                {
                    adapterNum++;

                    var adapterProperties = adapter.GetProperties();
                    string adapterDeviceID = $"{adapterProperties.PciVendorId:X4}_{adapterProperties.PciDeviceId:X4}_{adapterProperties.PciSubsysVendorId:X4}_{adapterProperties.PciSubsysId:X4}_{adapterProperties.RevId:X2}";

                    if (!displayConfig.PhysicalAdapters.TryGetValue(adapterDeviceID, out INTEL_ADAPTER desiredAdapter))
                    {
                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: No stored settings found for adapter {adapterDeviceID}, skipping");
                        continue;
                    }

                    CombinedDisplayArgsDto currentCombinedDisplay;
                    try
                    {
                        currentCombinedDisplay = adapter.GetCombinedDisplay();
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfig: Exception getting Combined Display settings for adapter {adapterNum}.");
                        continue;
                    }


                    bool currentCombinedDisplayInUse = currentCombinedDisplay.NumOutputs > 1;
                    bool desiredCombinedDisplayInUse = desiredAdapter.IsCombinedDisplay;

                    if (desiredCombinedDisplayInUse)
                    {
                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: New display layout requires a Combined Display on adapter {adapterNum}");

                        if (currentCombinedDisplay.Equals(desiredAdapter.CombinedDisplay))
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: Combined Display layout matches desired configuration, skipping");
                            continue;
                        }

                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: Attempting to create the Intel Combined Display on adapter {adapterNum}");

                        if (currentCombinedDisplayInUse)
                        {
                            CombinedDisplayArgsDto disableCombinedDisplayArgs = currentCombinedDisplay;
                            disableCombinedDisplayArgs.OpType = ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_DISABLE;

                            try
                            {
                                adapter.SetCombinedDisplay(disableCombinedDisplayArgs);
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: Disabled the current Intel Combined Display on adapter {adapterNum} before creating a new one");
                                Thread.Sleep(delayInMs); // Wait a moment to ensure the disable takes effect
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfig: Error disabling the current Intel Combined Display on adapter {adapterNum}");
                                return false;
                            }
                        }

                        CombinedDisplayArgsDto combinedDisplayArgs = desiredAdapter.CombinedDisplay;
                        combinedDisplayArgs.OpType = ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_ENABLE;

                        if (combinedDisplayArgs.ChildInfos == null || combinedDisplayArgs.ChildInfos.Count == 0)
                        {
                            SharedLogger.logger.Error($"IntelLibrary/SetActiveConfig: The desired combined display on adapter {adapterNum} is missing ChildInfos.");
                            return false;
                        }

                        try
                        {
                            adapter.SetCombinedDisplay(combinedDisplayArgs);
                            SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: Successfully created the Intel Combined Display on adapter {adapterNum}");
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfig: Error creating the Intel Combined Display on adapter {adapterNum}");
                            return false;
                        }

                        try
                        {
                            var updatedCombinedDisplay = adapter.GetCombinedDisplay();
                            if (updatedCombinedDisplay.NumOutputs == desiredAdapter.CombinedDisplay.NumOutputs &&
                                updatedCombinedDisplay.CombinedDesktopWidth == desiredAdapter.CombinedDisplay.CombinedDesktopWidth &&
                                updatedCombinedDisplay.CombinedDesktopHeight == desiredAdapter.CombinedDisplay.CombinedDesktopHeight)
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: This new Combined Display layout matches the desired configuration.");
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"IntelLibrary/SetActiveConfig: This new Combined Display layout is different from the one originally saved. You may need to update this desktop profile.");
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfig: Exception verifying Combined Display settings for adapter {adapterNum}.");
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: Combined Display in use but new display layout does NOT require a Combined Display on adapter {adapterNum}");

                        if (currentCombinedDisplayInUse)
                        {
                            CombinedDisplayArgsDto disableCombinedDisplayArgs = currentCombinedDisplay;
                            disableCombinedDisplayArgs.OpType = ctl_combined_display_optype_t.CTL_COMBINED_DISPLAY_OPTYPE_DISABLE;

                            try
                            {
                                adapter.SetCombinedDisplay(disableCombinedDisplayArgs);
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: Disabled the current Intel Combined Display on adapter {adapterNum} as it is not needed in the new display layout");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfig: Error disabling the current Intel Combined Display on adapter {adapterNum}");
                                return false;
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfig: Combined Display layout is not currently in use and is NOT required, so leaving things as they are.");
                        }
                    }
                }
            }
            else
            {
                SharedLogger.logger.Error($"IntelLibrary/SetActiveConfig: ERROR - Tried to run SetActiveConfig but the Intel IGCL library isn't initialised!");
                throw new IntelLibraryException($"Tried to run SetActiveConfig but the Intel IGCL library isn't initialised!");
            }

            return true;

        }

        public bool SetActiveConfigOverride(INTEL_DISPLAY_CONFIG displayConfig, int delayInMs)
        {
            if (_initialised && _igclApiHelper != null)
            {
                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Applying display settings stored in the display configuration");
                var currentDisplayConfig = GetIntelDisplayConfig();

                var adapters = _igclApiHelper.EnumerateAdapters();
                if (adapters == null || adapters.Count == 0)
                {
                    SharedLogger.logger.Error($"IntelLibrary/SetActiveConfigOverride: No Intel adapters found or error getting adapter count.");
                    return false;
                }

                bool success = true;
                int adapterNum = 0;
                bool IsUnsupportedResult(ctl_result_t result)
                {
                    return result == ctl_result_t.CTL_RESULT_ERROR_UNSUPPORTED_FEATURE ||
                        result == ctl_result_t.CTL_RESULT_ERROR_UNSUPPORTED_VERSION ||
                        result == ctl_result_t.CTL_RESULT_ERROR_INVALID_OPERATION_TYPE ||
                        result == ctl_result_t.CTL_RESULT_ERROR_INVALID_ARGUMENT;
                }

                foreach (var adapter in adapters)
                {
                    adapterNum++;
                    DeviceAdapterPropertiesDto adapterProperties;

                    try
                    {
                        adapterProperties = adapter.GetProperties();
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Exception getting adapter properties for adapter {adapterNum}.");
                        success = false;
                        continue;
                    }

                    var displays = adapter.EnumerateDisplayOutputs();
                    if (displays == null || displays.Count == 0)
                    {
                        continue;
                    }

                    int displayNum = 0;
                    foreach (var display in displays)
                    {
                        displayNum++;
                        DisplayPropertiesDto displayProperties;

                        try
                        {
                            displayProperties = display.GetProperties();
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Exception getting display properties for display {display.Name} on adapter {adapterNum}.");
                            success = false;
                            continue;
                        }

                        string logDisplayId = $"{display.Name}|{displayProperties.OsDisplayEncoderHandle.WindowsDisplayEncoderId}";

                        if (((uint)displayProperties.DisplayConfigFlags & (uint)ctl_display_config_flag_t.CTL_DISPLAY_CONFIG_FLAG_DISPLAY_ACTIVE) != (uint)ctl_display_config_flag_t.CTL_DISPLAY_CONFIG_FLAG_DISPLAY_ACTIVE)
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Skipping inactive display {logDisplayId} on adapter {adapterNum}");
                            continue;
                        }
                    
                        string displayDeviceId = $"VEN_{adapterProperties.PciVendorId:X4}&DEV_{adapterProperties.PciDeviceId:X4}&REV_{adapterProperties.RevId:X2}-PORT_{displayProperties.OsDisplayEncoderHandle.WindowsDisplayEncoderId}";

                        if (!displayConfig.Displays.TryGetValue(displayDeviceId, out INTEL_DISPLAY_WITH_SETTINGS storedSettings))
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: No stored settings found for display {logDisplayId} (ID {displayDeviceId}), skipping");
                            continue;
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Found stored settings for display {logDisplayId} (ID {displayDeviceId})");
                        }

                        if (!currentDisplayConfig.Displays.TryGetValue(displayDeviceId, out INTEL_DISPLAY_WITH_SETTINGS currentSettings))
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: No current settings found for display {logDisplayId} (ID {displayDeviceId}), skipping");
                            continue;
                        }

                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Applying settings for display {logDisplayId}");

                        //------------------------------------
                        // SET INTEGER SCALING IF NEEDED
                        //------------------------------------
                        if (storedSettings.IsSupportedIntegerScaling)
                        {
                            try
                            {
                                if (currentSettings.IsSupportedIntegerScaling)
                                {
                                    var retroScalingSettings = currentSettings.RetroScalingSettings;
                                    if (retroScalingSettings.Enable != storedSettings.RetroScalingSettings.Enable ||
                                        retroScalingSettings.RetroScalingType != storedSettings.RetroScalingSettings.RetroScalingType)
                                    {
                                        retroScalingSettings.Get = false;
                                        retroScalingSettings.Enable = storedSettings.RetroScalingSettings.Enable;
                                        retroScalingSettings.RetroScalingType = storedSettings.RetroScalingSettings.RetroScalingType;
                                        display.SetRetroScalingSettings(retroScalingSettings);
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set Integer Scaling to Enabled={storedSettings.RetroScalingSettings.Enable}, Type={storedSettings.RetroScalingSettings.RetroScalingType}");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Integer Scaling already set to desired values, skipping");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Integer Scaling not supported by current hardware, skipping");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Integer Scaling for display {logDisplayId}");
                                success = false;
                            }
                        }

                        //------------------------------------
                        // SET GPU SCALING IF NEEDED
                        //------------------------------------
                        if (storedSettings.IsSupportedGPUScaling)
                        {
                            try
                            {
                                if (currentSettings.IsSupportedGPUScaling)
                                {
                                    var scalingSettings = currentSettings.ScalingSettings;
                                    if (scalingSettings.Enable != storedSettings.ScalingSettings.Enable ||
                                        scalingSettings.ScalingType != storedSettings.ScalingSettings.ScalingType)
                                    {
                                        scalingSettings.Enable = storedSettings.ScalingSettings.Enable;
                                        scalingSettings.ScalingType = storedSettings.ScalingSettings.ScalingType;
                                        display.SetCurrentScaling(scalingSettings);
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set GPU Scaling to Enabled={storedSettings.ScalingSettings.Enable}, Type={storedSettings.ScalingSettings.ScalingType}");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: GPU Scaling already set to desired values, skipping");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: GPU Scaling not supported by current hardware, skipping");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying GPU Scaling for display {logDisplayId}");
                                success = false;
                            }
                        }

                        //------------------------------------
                        // SET IMAGE SHARPENING IF NEEDED
                        //------------------------------------
                        if (storedSettings.IsSupportedImageSharpening)
                        {
                            try
                            {
                                if (currentSettings.IsSupportedImageSharpening)
                                {
                                    var sharpnessSettings = currentSettings.SharpnessSettings;
                                    if (sharpnessSettings.Enable != storedSettings.SharpnessSettings.Enable ||
                                        sharpnessSettings.FilterType != storedSettings.SharpnessSettings.FilterType ||
                                        Math.Abs(sharpnessSettings.Intensity - storedSettings.SharpnessSettings.Intensity) > 0.001f)
                                    {
                                        sharpnessSettings.Enable = storedSettings.SharpnessSettings.Enable;
                                        sharpnessSettings.FilterType = storedSettings.SharpnessSettings.FilterType;
                                        sharpnessSettings.Intensity = storedSettings.SharpnessSettings.Intensity;
                                        display.SetCurrentSharpness(sharpnessSettings);
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set Image Sharpening to Enabled={storedSettings.SharpnessSettings.Enable}, Intensity={storedSettings.SharpnessSettings.Intensity}");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Image Sharpening already set to desired values, skipping");
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Image Sharpening not supported by current hardware, skipping");
                                }
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Image Sharpening for display {logDisplayId}");
                                success = false;
                            }
                        }

                        //------------------------------------
                        // SET DISPLAY SETTINGS IF NEEDED
                        //------------------------------------
                        try
                        {
                            var currentDisplaySettings = currentSettings.DisplaySettings;
                            if (currentSettings.IsSupportedDisplaySettings)
                            {
                                if (!INTEL_DISPLAY_WITH_SETTINGS.AreDisplaySettingsEqual(currentDisplaySettings, storedSettings.DisplaySettings))
                                {
                                    var desiredDisplaySettings = storedSettings.DisplaySettings;
                                    desiredDisplaySettings.Set = true;
                                    display.SetDisplaySettings(desiredDisplaySettings);
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set DisplaySettings for display {logDisplayId}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: DisplaySettings already set to desired values, skipping");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: DisplaySettings not supported by current hardware, skipping");
                            }
                        }
                        catch (IGCLException ex)
                        {
                            if (IsUnsupportedResult(ex.Result))
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: DisplaySettings not supported by current hardware, skipping");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying DisplaySettings for display {logDisplayId}");
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying DisplaySettings for display {logDisplayId}");
                            success = false;
                        }

                        //------------------------------------
                        // SET WIRE FORMAT IF NEEDED
                        //------------------------------------
                        try
                        {
                            var currentWireFormat = currentSettings.WireFormat;
                            if (currentSettings.IsSupportedWireFormat)
                            {
                                if (!currentWireFormat.Equals(storedSettings.WireFormat.WireFormat))
                                {
                                    var desiredWireFormat = storedSettings.WireFormat;
                                    desiredWireFormat.Operation = ctl_wire_format_operation_type_t.CTL_WIRE_FORMAT_OPERATION_TYPE_SET;
                                    display.SetWireFormat(desiredWireFormat);
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set WireFormat for display {logDisplayId}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: WireFormat already set to desired values, skipping");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: WireFormat not supported by current hardware, skipping");
                            }
                        }
                        catch (IGCLException ex)
                        {
                            if (IsUnsupportedResult(ex.Result))
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: WireFormat not supported by current hardware, skipping");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying WireFormat for display {logDisplayId}");
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying WireFormat for display {logDisplayId}");
                            success = false;
                        }

                        //------------------------------------
                        // SET BRIGHTNESS IF NEEDED
                        //------------------------------------
                        try
                        {
                            var currentBrightness = currentSettings.Brightness;
                            if (currentBrightness.TargetBrightness != storedSettings.Brightness.TargetBrightness)
                            {
                                var brightnessToSet = new BrightnessSetDto
                                {
                                    TargetBrightness = storedSettings.Brightness.TargetBrightness,
                                    SmoothTransitionTimeInMs = 0
                                };
                                display.SetBrightnessSetting(brightnessToSet);
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set Brightness to {storedSettings.Brightness.TargetBrightness} for display {logDisplayId}");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Brightness already set to desired values, skipping");
                            }
                        }
                        catch (IGCLException ex)
                        {
                            if (IsUnsupportedResult(ex.Result))
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Brightness not supported by current hardware, skipping");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Brightness for display {logDisplayId}");
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Brightness for display {logDisplayId}");
                            success = false;
                        }

                        //------------------------------------
                        // SET POWER OPTIMIZATION IF NEEDED
                        //------------------------------------
                        try
                        {
                            if (currentSettings.IsSupportedPowerOptimization)
                            {
                                var currentPowerOptimization = currentSettings.PowerOptimizationSettings;
                                if (!currentPowerOptimization.Equals(storedSettings.PowerOptimizationSettings))
                                {
                                    display.SetPowerOptimizationSetting(storedSettings.PowerOptimizationSettings);
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set PowerOptimization settings for display {logDisplayId}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: PowerOptimization settings already set to desired values, skipping");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: PowerOptimization not supported by current hardware, skipping");
                            }
                        }
                        catch (IGCLException ex)
                        {
                            if (IsUnsupportedResult(ex.Result))
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: PowerOptimization not supported by current hardware, skipping");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying PowerOptimization settings for display {logDisplayId}");
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying PowerOptimization settings for display {logDisplayId}");
                            success = false;
                        }

                        //------------------------------------
                        // SET LACE CONFIG IF NEEDED
                        //------------------------------------
                        try
                        {
                            var currentLaceConfig = currentSettings.LaceConfig;
                            if (!currentLaceConfig.Equals(storedSettings.LaceConfig))
                            {
                                display.SetLACEConfig(storedSettings.LaceConfig);
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set LACE config for display {logDisplayId}");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: LACE config already set to desired values, skipping");
                            }
                        }
                        catch (IGCLException ex)
                        {
                            if (IsUnsupportedResult(ex.Result))
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: LACE not supported by current hardware, skipping");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying LACE config for display {logDisplayId}");
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying LACE config for display {logDisplayId}");
                            success = false;
                        }

                        //------------------------------------
                        // SET SOFTWARE PSR IF NEEDED
                        //------------------------------------
                        try
                        {
                            var currentPsrSettings = currentSettings.SoftwarePsrSettings;
                            if (currentPsrSettings.Supported)
                            {
                                if (!currentPsrSettings.Equals(storedSettings.SoftwarePsrSettings))
                                {
                                    var desiredPsrSettings = storedSettings.SoftwarePsrSettings;
                                    desiredPsrSettings.Set = true;
                                    display.SoftwarePSR(desiredPsrSettings);
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set Software PSR settings for display {logDisplayId}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Software PSR settings already set to desired values, skipping");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Software PSR not supported by current hardware, skipping");
                            }
                        }
                        catch (IGCLException ex)
                        {
                            if (IsUnsupportedResult(ex.Result))
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Software PSR not supported by current hardware, skipping");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Software PSR settings for display {logDisplayId}");
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Software PSR settings for display {logDisplayId}");
                            success = false;
                        }

                        //------------------------------------
                        // SET DYNAMIC CONTRAST ENHANCEMENT IF NEEDED
                        //------------------------------------
                        try
                        {
                            var currentDceArgs = currentSettings.DynamicContrastEnhancement;
                            if (currentSettings.IsSupportedDynamicContrastEnhancement)
                            {
                                bool dceDifferent =
                                    currentDceArgs.Enable != storedSettings.DynamicContrastEnhancement.Enable ||
                                    currentDceArgs.TargetBrightnessPercent != storedSettings.DynamicContrastEnhancement.TargetBrightnessPercent ||
                                    currentDceArgs.PhaseinSpeedMultiplier != storedSettings.DynamicContrastEnhancement.PhaseinSpeedMultiplier ||
                                    currentDceArgs.NumBins != storedSettings.DynamicContrastEnhancement.NumBins;

                                if (dceDifferent)
                                {
                                    var desiredDceArgs = storedSettings.DynamicContrastEnhancement;
                                    desiredDceArgs.Set = true;
                                    var desiredHistogram = storedSettings.DynamicContrastEnhancementHistogram ?? Array.Empty<uint>();
                                    display.SetDynamicContrastEnhancement(desiredDceArgs, desiredHistogram);
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set Dynamic Contrast Enhancement for display {logDisplayId}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Dynamic Contrast Enhancement already set to desired values, skipping");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Dynamic Contrast Enhancement not supported by current hardware, skipping");
                            }
                        }
                        catch (IGCLException ex)
                        {
                            if (IsUnsupportedResult(ex.Result))
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Dynamic Contrast Enhancement not supported by current hardware, skipping");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Dynamic Contrast Enhancement for display {logDisplayId}");
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Dynamic Contrast Enhancement for display {logDisplayId}");
                            success = false;
                        }

                        //------------------------------------
                        // SET INTEL ARC SYNC PROFILE IF NEEDED
                        //------------------------------------
                        try
                        {
                            if (currentSettings.IsSupportedIntelArcSync)
                            {
                                var currentArcSyncProfile = currentSettings.IntelArcSyncProfile;
                                if (!currentArcSyncProfile.Equals(storedSettings.IntelArcSyncProfile))
                                {
                                    display.SetIntelArcSyncProfile(storedSettings.IntelArcSyncProfile);
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set Intel Arc Sync profile for display {logDisplayId}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Intel Arc Sync profile already set to desired values, skipping");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Intel Arc Sync not supported by current hardware, skipping");
                            }
                        }
                        catch (IGCLException ex)
                        {
                            if (IsUnsupportedResult(ex.Result))
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Intel Arc Sync not supported by current hardware, skipping");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Intel Arc Sync profile for display {logDisplayId}");
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Intel Arc Sync profile for display {logDisplayId}");
                            success = false;
                        }

                        //------------------------------------
                        // SET CUSTOM MODES IF NEEDED
                        //------------------------------------
                        try
                        {
                            var storedModes = storedSettings.CustomModes;
                            if (storedModes != null && storedModes.Count > 0)
                            {
                                var currentModes = currentSettings.CustomModes;
                                bool customModesDifferent = currentModes == null ||
                                    currentModes.Count != storedModes.Count ||
                                    !currentModes.SequenceEqual(storedModes);

                                if (customModesDifferent)
                                {
                                    var desiredCustomModeArgs = storedSettings.CustomModeArgs;
                                    desiredCustomModeArgs.CustomModeOpType = ctl_custom_mode_operation_types_t.CTL_CUSTOM_MODE_OPERATION_TYPES_ADD_CUSTOM_SOURCE_MODE;
                                    desiredCustomModeArgs.NumOfModes = (uint)storedModes.Count;
                                    display.SetCustomModes(desiredCustomModeArgs, (IReadOnlyList<CustomSourceModeDto>)storedModes);
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set Custom Modes for display {logDisplayId}");
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Custom Modes already set to desired values, skipping");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: No Custom Modes stored for display {logDisplayId}, skipping");
                            }
                        }
                        catch (IGCLException ex)
                        {
                            if (IsUnsupportedResult(ex.Result))
                            {
                                SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Custom Modes not supported by current hardware, skipping");
                            }
                            else
                            {
                                SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Custom Modes for display {logDisplayId}");
                                success = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying Custom Modes for display {logDisplayId}");
                            success = false;
                        }

                        if (delayInMs > 0)
                        {
                            Thread.Sleep(delayInMs);
                        }
                    }

                    // Compute the adapter device ID to look up stored adapter settings
                    string adapterDeviceId = $"{adapterProperties.PciVendorId:X4}_{adapterProperties.PciDeviceId:X4}_{adapterProperties.PciSubsysVendorId:X4}_{adapterProperties.PciSubsysId:X4}_{adapterProperties.RevId:X2}";
                    bool hasStoredAdapter = displayConfig.PhysicalAdapters.TryGetValue(adapterDeviceId, out INTEL_ADAPTER storedAdapter);

                    //------------------------------------
                    // APPLY PER-ADAPTER 3D SETTINGS IF NEEDED
                    //------------------------------------
                    if (hasStoredAdapter && storedAdapter.IsSupportedThreeDSettings &&
                        storedAdapter.ThreeDSettings != null && storedAdapter.ThreeDSettings.Count > 0)
                    {
                        try
                        {
                            var threeDHelper = _igclApiHelper.Get3DHelper(adapter);
                            foreach (var stored3DFeature in storedAdapter.ThreeDSettings)
                            {
                                try
                                {
                                    var currentRequest = IGCL3DHelper.Create3DFeatureGetRequest(stored3DFeature.FeatureType, stored3DFeature.ValueType);
                                    var currentFeature = threeDHelper.Get3DFeature(currentRequest);
                                    if (!currentFeature.Equals(stored3DFeature))
                                    {
                                        var setRequest = stored3DFeature;
                                        setRequest.Set = true;
                                        threeDHelper.Set3DFeature(setRequest);
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set 3D feature {stored3DFeature.FeatureType} for adapter {adapterNum}");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: 3D feature {stored3DFeature.FeatureType} already at desired value for adapter {adapterNum}, skipping");
                                    }
                                }
                                catch (IGCLException ex) when (IsUnsupportedResult(ex.Result))
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: 3D feature {stored3DFeature.FeatureType} not supported by current hardware for adapter {adapterNum}, skipping");
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying 3D feature {stored3DFeature.FeatureType} for adapter {adapterNum}");
                                    success = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"IntelLibrary/SetActiveConfigOverride: Exception applying 3D settings for adapter {adapterNum}, skipping.");
                        }
                    }

                    //------------------------------------
                    // APPLY PER-ADAPTER VIDEO PROCESSING (MEDIA) SETTINGS IF NEEDED
                    //------------------------------------
                    if (hasStoredAdapter && storedAdapter.IsSupportedVideoProcessing &&
                        storedAdapter.VideoProcessingSettings != null && storedAdapter.VideoProcessingSettings.Count > 0)
                    {
                        try
                        {
                            var mediaHelper = _igclApiHelper.GetMediaHelper(adapter);
                            foreach (var storedMediaFeature in storedAdapter.VideoProcessingSettings)
                            {
                                try
                                {
                                    var currentRequest = IGCLMediaHelper.CreateVideoProcessingFeatureGetRequest(storedMediaFeature.FeatureType, storedMediaFeature.ValueType);
                                    var currentFeature = mediaHelper.GetVideoProcessingFeature(currentRequest);
                                    if (!currentFeature.Equals(storedMediaFeature))
                                    {
                                        var setRequest = storedMediaFeature;
                                        setRequest.Set = true;
                                        mediaHelper.SetVideoProcessingFeature(setRequest);
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Successfully set video processing feature {storedMediaFeature.FeatureType} for adapter {adapterNum}");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Video processing feature {storedMediaFeature.FeatureType} already at desired value for adapter {adapterNum}, skipping");
                                    }
                                }
                                catch (IGCLException ex) when (IsUnsupportedResult(ex.Result))
                                {
                                    SharedLogger.logger.Trace($"IntelLibrary/SetActiveConfigOverride: Video processing feature {storedMediaFeature.FeatureType} not supported by current hardware for adapter {adapterNum}, skipping");
                                }
                                catch (Exception ex)
                                {
                                    SharedLogger.logger.Error(ex, $"IntelLibrary/SetActiveConfigOverride: Error applying video processing feature {storedMediaFeature.FeatureType} for adapter {adapterNum}");
                                    success = false;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Warn(ex, $"IntelLibrary/SetActiveConfigOverride: Exception applying video processing settings for adapter {adapterNum}, skipping.");
                        }
                    }
                }

                return success;
            }
            else
            {
                SharedLogger.logger.Error($"IntelLibrary/SetActiveConfigOverride: ERROR - Tried to run SetActiveConfigOverride but the Intel IGCL library isn't initialised!");
                throw new IntelLibraryException($"Tried to run SetActiveConfigOverride but the Intel IGCL library isn't initialised!");
            }
        }

        public bool IsActiveConfig(INTEL_DISPLAY_CONFIG displayConfig)
        {
            SharedLogger.logger.Trace($"IntelLibrary/IsActiveConfig: Checking whether the display configuration is already being used.");
            if (displayConfig.Equals(_activeDisplayConfig))
            {
                SharedLogger.logger.Trace($"IntelLibrary/IsActiveConfig: The display configuration is already being used");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"IntelLibrary/IsActiveConfig: The display configuration is NOT currently in use");
                return false;
            }
        }

        public bool IsValidConfig(INTEL_DISPLAY_CONFIG displayConfig)
        {
            SharedLogger.logger.Trace($"IntelLibrary/IsValidConfig: Testing whether the display configuration is valid");

            if (!_initialised || !displayConfig.IsInUse)
            {
                SharedLogger.logger.Trace($"IntelLibrary/IsValidConfig: The Intel display configuration is not in use, so it has no bearing in terms of whether it can be applied now. Returning true.");
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsPossibleConfig(INTEL_DISPLAY_CONFIG displayConfig)
        {
            SharedLogger.logger.Trace($"IntelLibrary/IsPossibleConfig: Testing whether the Intel display configuration is possible to be used now");

            if (!_initialised || !displayConfig.IsInUse)
            {
                SharedLogger.logger.Trace($"IntelLibrary/IsPossibleConfig: The Intel display configuration is not in use, so it has no bearing in terms of whether it can be applied now. Returning true.");
                return true;
            }

            // If both display identifiers are 0 then no displays were connected via Intel and we should just return true.
            if (displayConfig.DisplayIdentifiers.Count == 0 && _allConnectedDisplayIdentifiers.Count == 0)
            {
                return true;
            }
            // but if only allconnected count is 0 then we have a problem
            else if (_allConnectedDisplayIdentifiers.Count == 0)
            {
                return false;
            }

            // IGCL does not support identifying the displays that are part of a combined display. We have no way of knowing this,
            // so we will just assume that if any of the display identifiers in the config are part of a combined display,
            // then they are available now. This is not ideal, but there is no other way to do this with IGCL.
            // TODO: Find a way to use the Windows IDs to identify combined displays in IGCL.
            if (displayConfig.CombinedDisplayIsInUse || _activeDisplayConfig.Value.CombinedDisplayIsInUse)
            {
                return true;
            }

            // Check that we have all the displayConfig DisplayIdentifiers we need available now            
            if (displayConfig.DisplayIdentifiers.All(value => _allConnectedDisplayIdentifiers.Contains(value)))
            {
                SharedLogger.logger.Trace($"IntelLibrary/IsPossibleConfig: Success! The Intel display configuration is possible to be used now");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"IntelLibrary/IsPossibleConfig: Uh oh! The Intel display configuration cannot be used now");
                return false;
            }
        }

        public List<string> GetCurrentDisplayIdentifiers(out bool failure)
        {
            SharedLogger.logger.Trace($"IntelLibrary/GetCurrentDisplayIdentifiers: Getting the current display identifiers for the displays in use now");

            List<string> displayIdentifiers = new List<string>();
            failure = false;

            if (_initialised && _igclApiHelper != null)
            {
                
                // Enumerate the Intel GPUs adapters in the sytem
                var adapters = _igclApiHelper.EnumerateAdapters();
                int adapterTotalCount = adapters.Count;
                int adapterNum = 0;

                SharedLogger.logger.Trace($"IntelLibrary/GetCurrentDisplayIdentifiers: Found {adapterTotalCount} Intel GPU adapter(s)");


                // Go through each adapter
                foreach (var adapter in adapters)
                {
                    var displays = adapter.EnumerateDisplayOutputs();
                    int displayTotalCount = displays.Count;
                    int displayNum = 0;

                    var adapterDeviceProperties = adapter.GetDeviceProperties();

                    foreach (var display in displays)
                    {
                        displayNum++;
                        var displayProperties = display.GetProperties();

                        // Skip inactive displays (inactive displays are not part of the current desktop so are not in use now)
                        if (((uint)displayProperties.DisplayConfigFlags & (uint)ctl_display_config_flag_t.CTL_DISPLAY_CONFIG_FLAG_DISPLAY_ACTIVE) != (uint)ctl_display_config_flag_t.CTL_DISPLAY_CONFIG_FLAG_DISPLAY_ACTIVE)
                        {
                            SharedLogger.logger.Trace($"IntelLibrary/GetCurrentDisplayIdentifiers: Skipping inactive display Index {displayNum} on Adapter {adapterNum}");
                            continue;
                        }

                        var manufacturerCode = "";
                        var productCode = "";
                        try {
                            var edidBytes = display.GetPanelEdidData();
                            var edid = new EDID(edidBytes);
                            manufacturerCode = edid.ManufacturerCode.ToString();
                            productCode = edid.ProductCode.ToString();
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error($"IntelLibrary/GetAllConnectedDisplayIdentifiers: ERROR - Failed to get EDID for display Index {displayNum} on Adapter {adapterNum}: {ex.Message}");
                        }
                        List<string> displayInfo = new List<string>
                        {
                            "IntelIGCL",
                            adapter.Name,
                            adapterDeviceProperties.PciDeviceId.ToString("G"),
                            adapterDeviceProperties.PciSubsysId.ToString("G"),
                            displayProperties.Type.ToString(),
                            displayProperties.AttachedDisplayMuxType.ToString(),
                            displayProperties.ProtocolConverterOutput.ToString(),
                            displayProperties.ProtocolConverterType.ToString(),
                            manufacturerCode,
                            productCode,
                            displayProperties.OsDisplayEncoderHandle.WindowsDisplayEncoderId.ToString("G")                          
                        };
                        
                        string displayIdentifier = String.Join("#", displayInfo);
                        if (!displayIdentifiers.Contains(displayIdentifier))
                        {
                            displayIdentifiers.Add(displayIdentifier);
                            SharedLogger.logger.Debug($"IntelLibrary/GetCurrentDisplayIdentifiers: DisplayIdentifier detected: {displayIdentifier}");
                        }
                    }
                }
            }
            else
            {
                SharedLogger.logger.Error($"IntelLibrary/GetCurrentDisplayIdentifiers: ERROR - Tried to get Displays but the Intel IGCL library isn't initialised!");
                throw new IntelLibraryException($"Tried to get Displays but the Intel IGCL library isn't initialised!");
            }

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
        }

        public List<string> GetAllConnectedDisplayIdentifiers(out bool failure)
        {
            SharedLogger.logger.Trace($"IntelLibrary/GetAllConnectedDisplayIdentifiers: Getting all the display identifiers that can possibly be used");

            List<string> displayIdentifiers = new List<string>();
            failure = false;

            if (_initialised && _igclApiHelper != null)
            {
                
                // Enumerate the Intel GPUs adapters in the sytem
                var adapters = _igclApiHelper.EnumerateAdapters();
                int adapterTotalCount = adapters.Count;
                int adapterNum = 0;

                SharedLogger.logger.Trace($"IntelLibrary/GetAllConnectedDisplayIdentifiers: Found {adapterTotalCount} Intel GPU adapter(s)");


                // Go through each adapter
                foreach (var adapter in adapters)
                {
                    var displays = adapter.EnumerateDisplayOutputs();
                    int displayTotalCount = displays.Count;
                    int displayNum = 0;

                    var adapterDeviceProperties = adapter.GetDeviceProperties();

                    foreach (var display in displays)
                    {
                        displayNum++;
                        var displayProperties = display.GetProperties();

                        // Skip displays unless they are attached and windows knows about them (these displays could be attached to the desktop if we wanted to use them)
                        if (((uint)displayProperties.DisplayConfigFlags & (uint)ctl_display_config_flag_t.CTL_DISPLAY_CONFIG_FLAG_DISPLAY_ATTACHED) != (uint)ctl_display_config_flag_t.CTL_DISPLAY_CONFIG_FLAG_DISPLAY_ATTACHED)

                        {
                            SharedLogger.logger.Trace($"IntelLibrary/GetCurrentDisplayIdentifiers: Skipping inactive display Index {displayNum} on Adapter {adapterNum}");
                            continue;
                        }                        

                        var manufacturerCode = "";
                        var productCode = "";

                        try {
                            var edidBytes = display.GetPanelEdidData();
                            var edid = new EDID(edidBytes);
                            manufacturerCode = edid.ManufacturerCode.ToString();
                            productCode = edid.ProductCode.ToString();
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error($"IntelLibrary/GetAllConnectedDisplayIdentifiers: ERROR - Failed to get EDID for display Index {displayNum} on Adapter {adapterNum}: {ex.Message}");
                        }
                        List<string> displayInfo = new List<string>
                        {
                            "IntelIGCL",
                            adapter.Name,
                            adapterDeviceProperties.PciDeviceId.ToString("G"),
                            adapterDeviceProperties.PciSubsysId.ToString("G"),
                            displayProperties.Type.ToString(),
                            displayProperties.AttachedDisplayMuxType.ToString(),
                            displayProperties.ProtocolConverterOutput.ToString(),
                            displayProperties.ProtocolConverterType.ToString(),
                            manufacturerCode,
                            productCode,
                            displayProperties.OsDisplayEncoderHandle.WindowsDisplayEncoderId.ToString("G")                          
                        };
                        
                        string displayIdentifier = String.Join("#", displayInfo);
                        if (!displayIdentifiers.Contains(displayIdentifier))
                        {
                            displayIdentifiers.Add(displayIdentifier);
                            SharedLogger.logger.Debug($"IntelLibrary/GetAllConnectedDisplayIdentifiers: DisplayIdentifier detected: {displayIdentifier}");
                        }
                    }
                }
            }
            else
            {
                SharedLogger.logger.Error($"IntelLibrary/GetCurrentDisplaGetAllConnectedDisplayIdentifiersyIdentifiers: ERROR - Tried to get Displays but the Intel IGCL library isn't initialised!");
                throw new IntelLibraryException($"Tried to get Displays but the Intel IGCL library isn't initialised!");
            }

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
        }

    }

    [Serializable]
    public class IntelLibraryException : Exception
    {
        public IntelLibraryException() { }
        public IntelLibraryException(string message) : base(message) { }
        public IntelLibraryException(string message, Exception inner) : base(message, inner) { }
    }
}

