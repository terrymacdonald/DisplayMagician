using DisplayMagicianShared.NVIDIA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.NVIDIA
{
    public static class GeneralDelegates
    {
        [FunctionId(FunctionId.NvAPI_GetErrorMessage)]
        public delegate Status NvAPI_GetErrorMessage([In] Status status, out ShortString message);

        [FunctionId(FunctionId.NvAPI_GetInterfaceVersionString)]
        public delegate Status NvAPI_GetInterfaceVersionString(out ShortString version);

        [FunctionId(FunctionId.NvAPI_Initialize)]
        public delegate Status NvAPI_Initialize();


        [FunctionId(FunctionId.NvAPI_RestartDisplayDriver)]
        public delegate Status NvAPI_RestartDisplayDriver();

        [FunctionId(FunctionId.NvAPI_SYS_GetChipSetInfo)]
        public delegate Status NvAPI_SYS_GetChipSetInfo(
            [In] [Accepts(typeof(ChipsetInfoV4), typeof(ChipsetInfoV3), typeof(ChipsetInfoV2), typeof(ChipsetInfoV1))]
            ValueTypeReference chipsetInfo);

        //NVAPI_INTERFACE NvAPI_SYS_GetDisplayIdFromGpuAndOutputId(NvPhysicalGpuHandle hPhysicalGpu, NvU32 outputId, NvU32* displayId);
        [FunctionId(FunctionId.NvAPI_SYS_GetDisplayIdFromGpuAndOutputId)]
        public delegate Status NvAPI_SYS_GetDisplayIdFromGpuAndOutputId(
            [In] PhysicalGPUHandle physicalGPU,
            [In] OutputId outputId,
            [Out] out uint displayId);


        [FunctionId(FunctionId.NvAPI_SYS_GetDriverAndBranchVersion)]
        public delegate Status NvAPI_SYS_GetDriverAndBranchVersion(
            out uint driverVersion,
            out ShortString buildBranchString);

        //NVAPI_INTERFACE NvAPI_SYS_GetGpuAndOutputIdFromDisplayId(NvU32 displayId, NvPhysicalGpuHandle* hPhysicalGpu, NvU32* outputId);
        [FunctionId(FunctionId.NvAPI_SYS_GetGpuAndOutputIdFromDisplayId)]
        public delegate Status NvAPI_SYS_GetGpuAndOutputIdFromDisplayId(
            [In] uint displayId,
            [Out] out PhysicalGPUHandle physicalGPU,
            [Out] out OutputId outputId);


        [FunctionId(FunctionId.NvAPI_SYS_GetLidAndDockInfo)]
        public delegate Status NvAPI_SYS_GetLidAndDockInfo([In][Out] ref LidDockParameters lidAndDock);

        [FunctionId(FunctionId.NvAPI_Unload)]
        public delegate Status NvAPI_Unload();
    }

    public static class DisplayDelegates
    {
        [FunctionId(FunctionId.NvAPI_CreateDisplayFromUnAttachedDisplay)]
        public delegate Status NvAPI_CreateDisplayFromUnAttachedDisplay(
            [In] UnAttachedDisplayHandle display,
            [Out] out DisplayHandle newDisplay
        );

        [FunctionId(FunctionId.NvAPI_Disp_ColorControl)]
        public delegate Status NvAPI_Disp_ColorControl(
            [In] uint displayId,
            [In]
            [Out]
            [Accepts(
                typeof(ColorDataV5),
                typeof(ColorDataV4),
                typeof(ColorDataV3),
                typeof(ColorDataV2),
                typeof(ColorDataV1)
            )]
            ValueTypeReference colorData
        );

        [FunctionId(FunctionId.NvAPI_DISP_DeleteCustomDisplay)]
        public delegate Status NvAPI_DISP_DeleteCustomDisplay(
            [In][Accepts(typeof(uint))] ValueTypeArray displayIds,
            [In] uint count,
            [In][Accepts(typeof(CustomDisplay))] ValueTypeReference customDisplay
        );

        [FunctionId(FunctionId.NvAPI_DISP_EnumCustomDisplay)]
        public delegate Status NvAPI_DISP_EnumCustomDisplay(
            [In] uint displayId,
            [In] uint index,
            [In][Accepts(typeof(CustomDisplay))] ValueTypeReference customDisplay
        );

        [FunctionId(FunctionId.NvAPI_DISP_GetAssociatedUnAttachedNvidiaDisplayHandle)]
        public delegate Status NvAPI_DISP_GetAssociatedUnAttachedNvidiaDisplayHandle(
            [In][MarshalAs(UnmanagedType.LPStr)] string displayName,
            [Out] out UnAttachedDisplayHandle display
        );

        [FunctionId(FunctionId.NvAPI_DISP_GetDisplayConfig)]
        public delegate Status NvAPI_DISP_GetDisplayConfig(
            [In][Out] ref uint pathInfoCount,
            [In] [Accepts(typeof(PathInfoV2), typeof(PathInfoV1))]
            ValueTypeArray pathInfos
        );

        [FunctionId(FunctionId.NvAPI_DISP_GetDisplayIdByDisplayName)]
        public delegate Status NvAPI_DISP_GetDisplayIdByDisplayName([In] string displayName, [Out] out uint displayId);

        [FunctionId(FunctionId.NvAPI_DISP_GetGDIPrimaryDisplayId)]
        public delegate Status NvAPI_DISP_GetGDIPrimaryDisplayId([Out] out uint displayId);

        [FunctionId(FunctionId.NvAPI_Disp_GetHdrCapabilities)]
        public delegate Status NvAPI_Disp_GetHdrCapabilities(
            [In] uint displayId,
            [In] [Out] [Accepts(typeof(HDRCapabilitiesV1))]
            ValueTypeReference hdrCapabilities
        );

        [FunctionId(FunctionId.NvAPI_DISP_GetMonitorCapabilities)]
        public delegate Status NvAPI_DISP_GetMonitorCapabilities(
            [In] uint displayId,
            [In] [Accepts(typeof(MonitorCapabilities))]
            ValueTypeReference capabilities
        );

        [FunctionId(FunctionId.NvAPI_DISP_GetMonitorColorCapabilities)]
        public delegate Status NvAPI_DISP_GetMonitorColorCapabilities(
            [In] uint displayId,
            [In] [Accepts(typeof(MonitorColorData))]
            ValueTypeArray capabilities,
            [In][Out] ref uint count
        );

        [FunctionId(FunctionId.NvAPI_DISP_GetTiming)]
        public delegate Status NvAPI_DISP_GetTiming(
            [In] uint displayId,
            [In][Accepts(typeof(TimingInput))] ValueTypeReference timingInput,
            [In][Accepts(typeof(Timing))] ValueTypeReference timing
        );

        [FunctionId(FunctionId.NvAPI_Disp_HdrColorControl)]
        public delegate Status NvAPI_Disp_HdrColorControl(
            [In] uint displayId,
            [In] [Out] [Accepts(typeof(HDRColorDataV2), typeof(HDRColorDataV1))]
            ValueTypeReference hdrColorData
        );

        [FunctionId(FunctionId.NvAPI_Disp_InfoFrameControl)]
        public delegate Status NvAPI_Disp_InfoFrameControl(
            [In] uint displayId,
            [In][Accepts(typeof(InfoFrameData))] ValueTypeReference infoFrameData
        );

        [FunctionId(FunctionId.NvAPI_DISP_RevertCustomDisplayTrial)]
        public delegate Status NvAPI_DISP_RevertCustomDisplayTrial(
            [In][Accepts(typeof(uint))] ValueTypeArray displayIds,
            [In] uint count
        );

        [FunctionId(FunctionId.NvAPI_DISP_SaveCustomDisplay)]
        public delegate Status NvAPI_DISP_SaveCustomDisplay(
            [In][Accepts(typeof(uint))] ValueTypeArray displayIds,
            [In] uint count,
            [In] uint isThisOutputIdOnly,
            [In] uint isThisMonitorIdOnly
        );

        [FunctionId(FunctionId.NvAPI_DISP_SetDisplayConfig)]
        public delegate Status NvAPI_DISP_SetDisplayConfig(
            [In] uint pathInfoCount,
            [In] [Accepts(typeof(PathInfoV2), typeof(PathInfoV1))]
            ValueTypeArray pathInfos,
            [In] DisplayConfigFlags flags
        );

        [FunctionId(FunctionId.NvAPI_DISP_TryCustomDisplay)]
        public delegate Status NvAPI_DISP_TryCustomDisplay(
            [In][Accepts(typeof(uint))] ValueTypeArray displayIds,
            [In] uint count,
            [In][Accepts(typeof(CustomDisplay))] ValueTypeArray customDisplays
        );

        [FunctionId(FunctionId.NvAPI_DISP_GetAdaptiveSyncData)]
        public delegate Status NvAPI_DISP_GetAdaptiveSyncData(
            [In] uint displayId,
            [In][Accepts(typeof(GetAdaptiveSyncData))] ValueTypeReference adaptiveSyncData
        );

        [FunctionId(FunctionId.NvAPI_DISP_SetAdaptiveSyncData)]
        public delegate Status NvAPI_DISP_SetAdaptiveSyncData(
            [In] uint displayId,
            [In][Accepts(typeof(GetAdaptiveSyncData))] ValueTypeReference adaptiveSyncData
        );

        [FunctionId(FunctionId.NvAPI_EnumNvidiaDisplayHandle)]
        public delegate Status NvAPI_EnumNvidiaDisplayHandle(
            [In] uint enumId,
            [Out] out DisplayHandle display
        );

        [FunctionId(FunctionId.NvAPI_EnumNvidiaUnAttachedDisplayHandle)]
        public delegate Status NvAPI_EnumNvidiaUnAttachedDisplayHandle(
            [In] uint enumId,
            [Out] out UnAttachedDisplayHandle display
        );

        [FunctionId(FunctionId.NvAPI_GetAssociatedDisplayOutputId)]
        public delegate Status NvAPI_GetAssociatedDisplayOutputId(
            [In] DisplayHandle display,
            [Out] out OutputId outputId
        );

        [FunctionId(FunctionId.NvAPI_GetAssociatedNvidiaDisplayHandle)]
        public delegate Status NvAPI_GetAssociatedNvidiaDisplayHandle(
            [In][MarshalAs(UnmanagedType.LPStr)] string displayName,
            [Out] out DisplayHandle display
        );

        [FunctionId(FunctionId.NvAPI_GetAssociatedNvidiaDisplayName)]
        public delegate Status NvAPI_GetAssociatedNvidiaDisplayName(
            [In] DisplayHandle display,
            [Out] out ShortString displayName
        );

        [FunctionId(FunctionId.NvAPI_GetDisplayDriverBuildTitle)]
        public delegate Status NvAPI_GetDisplayDriverBuildTitle(
            [In] DisplayHandle displayHandle,
            [Out] out ShortString name
        );

        [FunctionId(FunctionId.NvAPI_GetDisplayDriverMemoryInfo)]
        public delegate Status NvAPI_GetDisplayDriverMemoryInfo(
            [In] DisplayHandle displayHandle,
            [In]
            [Accepts(
                typeof(DisplayDriverMemoryInfoV3),
                typeof(DisplayDriverMemoryInfoV2),
                typeof(DisplayDriverMemoryInfoV1)
            )]
            ValueTypeReference memoryInfo
        );

        [FunctionId(FunctionId.NvAPI_GetDVCInfo)]
        public delegate Status NvAPI_GetDVCInfo(
            [In] DisplayHandle displayHandle,
            [In] OutputId displayId,
            [In] [Accepts(typeof(PrivateDisplayDVCInfo))]
            ValueTypeReference dvcInfo
        );

        [FunctionId(FunctionId.NvAPI_GetDVCInfoEx)]
        public delegate Status NvAPI_GetDVCInfoEx(
            [In] DisplayHandle displayHandle,
            [In] OutputId displayId,
            [In] [Accepts(typeof(PrivateDisplayDVCInfoEx))]
            ValueTypeReference dvcInfo
        );

        [FunctionId(FunctionId.NvAPI_GetHDMISupportInfo)]
        public delegate Status NvAPI_GetHDMISupportInfo(
            [In] DisplayHandle displayHandle,
            [In] uint displayIdOrOutputId,
            [In] [Accepts(typeof(HDMISupportInfoV2), typeof(HDMISupportInfoV1))]
            ValueTypeReference supportInfo
        );

        [FunctionId(FunctionId.NvAPI_GetHUEInfo)]
        public delegate Status NvAPI_GetHUEInfo(
            [In] DisplayHandle displayHandle,
            [In] OutputId displayId,
            [In] [Accepts(typeof(PrivateDisplayHUEInfo))]
            ValueTypeReference hueInfo
        );

        [FunctionId(FunctionId.NvAPI_GetSupportedViews)]
        public delegate Status NvAPI_GetSupportedViews(
            [In] DisplayHandle display,
            [In][Accepts(typeof(TargetViewMode))] ValueTypeArray viewModes,
            [Out][In] ref uint viewCount
        );

        [FunctionId(FunctionId.NvAPI_GetUnAttachedAssociatedDisplayName)]
        public delegate Status NvAPI_GetUnAttachedAssociatedDisplayName(
            [In] UnAttachedDisplayHandle display,
            [Out] out ShortString displayName
        );

        [FunctionId(FunctionId.NvAPI_GPU_GetScanoutCompositionParameter)]
        public delegate Status NvAPI_GPU_GetScanOutCompositionParameter(
            [In] uint displayId,
            [In] ScanOutCompositionParameter parameter,
            [Out] out ScanOutCompositionParameterValue parameterValue,
            [Out] out float container
        );

        [FunctionId(FunctionId.NvAPI_GPU_GetScanoutConfiguration)]
        public delegate Status NvAPI_GPU_GetScanOutConfiguration(
            [In] uint displayId,
            [In][Accepts(typeof(Rectangle))] ValueTypeReference desktopRectangle,
            [In][Accepts(typeof(Rectangle))] ValueTypeReference scanOutRectangle
        );

        [FunctionId(FunctionId.NvAPI_GPU_GetScanoutConfigurationEx)]
        public delegate Status NvAPI_GPU_GetScanOutConfigurationEx(
            [In] uint displayId,
            [In] [Accepts(typeof(ScanOutInformationV1))]
            ValueTypeReference scanOutInformation
        );

        [FunctionId(FunctionId.NvAPI_GPU_GetScanoutIntensityState)]
        public delegate Status NvAPI_GPU_GetScanOutIntensityState(
            [In] uint displayId,
            [In] [Accepts(typeof(ScanOutIntensityStateV1))]
            ValueTypeReference scanOutIntensityState
        );

        [FunctionId(FunctionId.NvAPI_GPU_GetScanoutWarpingState)]
        public delegate Status NvAPI_GPU_GetScanOutWarpingState(
            [In] uint displayId,
            [In] [Accepts(typeof(ScanOutWarpingStateV1))]
            ValueTypeReference scanOutWarpingState
        );

        [FunctionId(FunctionId.NvAPI_GPU_SetScanoutCompositionParameter)]
        public delegate Status NvAPI_GPU_SetScanOutCompositionParameter(
            [In] uint displayId,
            [In] ScanOutCompositionParameter parameter,
            [In] ScanOutCompositionParameterValue parameterValue,
            [In] ref float container
        );

        [FunctionId(FunctionId.NvAPI_GPU_SetScanoutIntensity)]
        public delegate Status NvAPI_GPU_SetScanOutIntensity(
            [In] uint displayId,
            [In] [Accepts(typeof(ScanOutIntensityV2), typeof(ScanOutIntensityV1))]
            ValueTypeReference scanOutIntensityData,
            [Out] out int isSticky
        );

        [FunctionId(FunctionId.NvAPI_GPU_SetScanoutWarping)]
        public delegate Status NvAPI_GPU_SetScanOutWarping(
            [In] uint displayId,
            [In] [Accepts(typeof(ScanOutWarpingV1))]
            ValueTypeReference scanOutWarping,
            [In][Out] ref int maximumNumberOfVertices,
            [Out] out int isSticky
        );


        [FunctionId(FunctionId.NvAPI_SetDVCLevel)]
        public delegate Status NvAPI_SetDVCLevel(
            [In] DisplayHandle displayHandle,
            [In] OutputId displayId,
            [In] int dvcLevel
        );

        [FunctionId(FunctionId.NvAPI_SetDVCLevelEx)]
        public delegate Status NvAPI_SetDVCLevelEx(
            [In] DisplayHandle displayHandle,
            [In] OutputId displayId,
            [In] [Accepts(typeof(PrivateDisplayDVCInfoEx))]
            ValueTypeReference dvcInfo
        );

        [FunctionId(FunctionId.NvAPI_SetHUEAngle)]
        public delegate Status NvAPI_SetHUEAngle(
            [In] DisplayHandle displayHandle,
            [In] OutputId displayId,
            [In] int hueAngle
        );

        [FunctionId(FunctionId.NvAPI_SetRefreshRateOverride)]
        public delegate Status NvAPI_SetRefreshRateOverride(
            [In] DisplayHandle displayHandle,
            [In] OutputId outputMask,
            [In] float refreshRate,
            [In] uint isDeferred
        );
    }

    public static class MosaicDelegates
    {
        [FunctionId(FunctionId.NvAPI_Mosaic_EnableCurrentTopo)]
        public delegate Status NvAPI_Mosaic_EnableCurrentTopo(uint enable);

        [FunctionId(FunctionId.NvAPI_Mosaic_EnumDisplayGrids)]
        public delegate Status NvAPI_Mosaic_EnumDisplayGrids(
            [Accepts(typeof(GridTopologyV2), typeof(GridTopologyV1))] [In] [Out]
            ValueTypeArray gridTopology,
            [In][Out] ref uint gridCount);

        [FunctionId(FunctionId.NvAPI_Mosaic_EnumDisplayModes)]
        public delegate Status NvAPI_Mosaic_EnumDisplayModes(
            [Accepts(typeof(GridTopologyV2), typeof(GridTopologyV1))] [In]
            ValueTypeReference gridTopology,
            [Accepts(typeof(DisplaySettingsV2), typeof(DisplaySettingsV1))] [In] [Out]
            ValueTypeArray
                displaysSettings,
            [In][Out] ref uint displaysCount);

        [FunctionId(FunctionId.NvAPI_Mosaic_GetCurrentTopo)]
        public delegate Status NvAPI_Mosaic_GetCurrentTopo(
            [In][Out] ref TopologyBrief topoBrief,
            [Accepts(typeof(DisplaySettingsV2), typeof(DisplaySettingsV1))] [In] [Out]
            ValueTypeReference displaySetting,
            [Out] out int overlapX,
            [Out] out int overlapY);

        [FunctionId(FunctionId.NvAPI_Mosaic_GetOverlapLimits)]
        public delegate Status NvAPI_Mosaic_GetOverlapLimits(
            [In] TopologyBrief topoBrief,
            [Accepts(typeof(DisplaySettingsV2), typeof(DisplaySettingsV1))] [In]
            ValueTypeReference displaySetting,
            [Out] out int minOverlapX,
            [Out] out int maxOverlapX,
            [Out] out int minOverlapY,
            [Out] out int maxOverlapY);

        [FunctionId(FunctionId.NvAPI_Mosaic_GetSupportedTopoInfo)]
        public delegate Status NvAPI_Mosaic_GetSupportedTopoInfo(
            [Accepts(typeof(SupportedTopologiesInfoV2), typeof(SupportedTopologiesInfoV1))] [In] [Out]
            ValueTypeReference
                supportedTopoInfo,
            TopologyType topologyType);

        [FunctionId(FunctionId.NvAPI_Mosaic_GetTopoGroup)]
        public delegate Status NvAPI_Mosaic_GetTopoGroup(
            [In] TopologyBrief topoBrief,
            [In][Out] ref TopologyGroup topoGroup);

        [FunctionId(FunctionId.NvAPI_Mosaic_SetCurrentTopo)]
        public delegate Status NvAPI_Mosaic_SetCurrentTopo(
            [In] TopologyBrief topoBrief,
            [Accepts(typeof(DisplaySettingsV2), typeof(DisplaySettingsV1))] [In]
            ValueTypeReference displaySetting,
            int overlapX,
            int overlapY,
            uint enable
        );

        [FunctionId(FunctionId.NvAPI_Mosaic_SetDisplayGrids)]
        public delegate Status NvAPI_Mosaic_SetDisplayGrids(
            [Accepts(typeof(GridTopologyV2), typeof(GridTopologyV1))] [In]
            ValueTypeArray gridTopologies,
            [In] uint gridCount,
            [In] SetDisplayTopologyFlag setTopoFlags);

        [FunctionId(FunctionId.NvAPI_Mosaic_ValidateDisplayGrids)]
        public delegate Status NvAPI_Mosaic_ValidateDisplayGrids(
            [In] SetDisplayTopologyFlag setTopoFlags,
            [Accepts(typeof(GridTopologyV2), typeof(GridTopologyV1))] [In]
            ValueTypeArray gridTopologies,
            [In][Out] ref DisplayTopologyStatus[] topoStatuses,
            [In] uint gridCount);

        //NVAPI_INTERFACE NvAPI_Mosaic_GetDisplayViewportsByResolution(NvU32 displayId, NvU32 srcWidth, NvU32 srcHeight, NV_RECT viewports[NV_MOSAIC_MAX_DISPLAYS], NvU8* bezelCorrected);
        [FunctionId(FunctionId.NvAPI_Mosaic_GetDisplayViewportsByResolution)]
        public delegate Status NvAPI_Mosaic_GetDisplayViewportsByResolution(
            [In] UInt32 sdisplayId,
            [In] UInt32 srcWidth,
            [In] UInt32 srcHeight,
            [Out] 
            [Accepts(typeof(ViewPortF))]
            out ValueTypeArray viewports,
            [Out] out byte bezelCorrected);

    }

    // ReSharper disable InconsistentNaming
    public static class DRSDelegates    {
        [FunctionId(FunctionId.NvAPI_DRS_CreateApplication)]
        public delegate Status NvAPI_DRS_CreateApplication(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In]
            [Accepts(
                typeof(DRSApplicationV4),
                typeof(DRSApplicationV3),
                typeof(DRSApplicationV2),
                typeof(DRSApplicationV1)
            )]
            ValueTypeReference application
        );

        [FunctionId(FunctionId.NvAPI_DRS_CreateProfile)]
        public delegate Status NvAPI_DRS_CreateProfile(
            [In] DRSSessionHandle sessionHandle,
            [In][Accepts(typeof(DRSProfileV1))] ValueTypeReference profile,
            [Out] out DRSProfileHandle profileHandle
        );

        [FunctionId(FunctionId.NvAPI_DRS_CreateSession)]
        public delegate Status NvAPI_DRS_CreateSession([Out] out DRSSessionHandle sessionHandle);

        [FunctionId(FunctionId.NvAPI_DRS_DeleteApplication)]
        public delegate Status NvAPI_DRS_DeleteApplication(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In] UnicodeString applicationName
        );

        [FunctionId(FunctionId.NvAPI_DRS_DeleteApplicationEx)]
        public delegate Status NvAPI_DRS_DeleteApplicationEx(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In]
            [Accepts(typeof(DRSApplicationV1), typeof(DRSApplicationV2), typeof(DRSApplicationV3),
                typeof(DRSApplicationV4))]
            ValueTypeReference application
        );

        [FunctionId(FunctionId.NvAPI_DRS_DeleteProfile)]
        public delegate Status NvAPI_DRS_DeleteProfile(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle
        );

        [FunctionId(FunctionId.NvAPI_DRS_DeleteProfileSetting)]
        public delegate Status NvAPI_DRS_DeleteProfileSetting(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In] uint settingId
        );

        [FunctionId(FunctionId.NvAPI_DRS_DestroySession)]
        public delegate Status NvAPI_DRS_DestroySession([In] DRSSessionHandle sessionHandle);

        [FunctionId(FunctionId.NvAPI_DRS_EnumApplications)]
        public delegate Status NvAPI_DRS_EnumApplications(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In] uint index,
            [In][Out] ref uint count,
            [In]
            [Accepts(
                typeof(DRSApplicationV4),
                typeof(DRSApplicationV3),
                typeof(DRSApplicationV2),
                typeof(DRSApplicationV1)
            )]
            ValueTypeArray applications
        );

        [FunctionId(FunctionId.NvAPI_DRS_EnumAvailableSettingIds)]
        public delegate Status NvAPI_DRS_EnumAvailableSettingIds(
            [In][Accepts(typeof(uint))] ValueTypeArray settingIds,
            [In][Out] ref uint count
        );

        [FunctionId(FunctionId.NvAPI_DRS_EnumAvailableSettingValues)]
        public delegate Status NvAPI_DRS_EnumAvailableSettingValues(
            [In] uint settingId,
            [In][Out] ref uint count,
            [In] [Out] [Accepts(typeof(DRSSettingValues))]
            ValueTypeReference settingValues
        );

        [FunctionId(FunctionId.NvAPI_DRS_EnumProfiles)]
        public delegate Status NvAPI_DRS_EnumProfiles(
            [In] DRSSessionHandle sessionHandle,
            [In] uint index,
            [Out] out DRSProfileHandle profileHandle
        );

        [FunctionId(FunctionId.NvAPI_DRS_EnumSettings)]
        public delegate Status NvAPI_DRS_EnumSettings(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In] uint index,
            [In][Out] ref uint count,
            [In] [Out] [Accepts(typeof(DRSSettingV1))]
            ValueTypeArray settings
        );

        [FunctionId(FunctionId.NvAPI_DRS_FindApplicationByName)]
        public delegate Status NvAPI_DRS_FindApplicationByName(
            [In] DRSSessionHandle sessionHandle,
            [In] UnicodeString applicationName,
            [Out] out DRSProfileHandle profileHandle,
            [In]
            [Accepts(
                typeof(DRSApplicationV4),
                typeof(DRSApplicationV3),
                typeof(DRSApplicationV2),
                typeof(DRSApplicationV1)
            )]
            ValueTypeReference application
        );

        [FunctionId(FunctionId.NvAPI_DRS_FindProfileByName)]
        public delegate Status NvAPI_DRS_FindProfileByName(
            [In] DRSSessionHandle sessionHandle,
            [In] UnicodeString profileName,
            [Out] out DRSProfileHandle profileHandle
        );

        [FunctionId(FunctionId.NvAPI_DRS_GetApplicationInfo)]
        public delegate Status NvAPI_DRS_GetApplicationInfo(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In] UnicodeString applicationName,
            [In]
            [Accepts(
                typeof(DRSApplicationV4),
                typeof(DRSApplicationV3),
                typeof(DRSApplicationV2),
                typeof(DRSApplicationV1)
            )]
            ValueTypeReference application
        );

        [FunctionId(FunctionId.NvAPI_DRS_GetBaseProfile)]
        public delegate Status NvAPI_DRS_GetBaseProfile(
            [In] DRSSessionHandle sessionHandle,
            [Out] out DRSProfileHandle profileHandle
        );

        [FunctionId(FunctionId.NvAPI_DRS_GetCurrentGlobalProfile)]
        public delegate Status NvAPI_DRS_GetCurrentGlobalProfile(
            [In] DRSSessionHandle sessionHandle,
            [Out] out DRSProfileHandle profileHandle
        );

        [FunctionId(FunctionId.NvAPI_DRS_GetNumProfiles)]
        public delegate Status NvAPI_DRS_GetNumProfiles([In] DRSSessionHandle sessionHandle, [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_DRS_GetProfileInfo)]
        public delegate Status NvAPI_DRS_GetProfileInfo(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In][Accepts(typeof(DRSProfileV1))] ValueTypeReference profile
        );

        [FunctionId(FunctionId.NvAPI_DRS_GetSetting)]
        public delegate Status NvAPI_DRS_GetSetting(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In] uint settingId,
            [Out][Accepts(typeof(DRSSettingV1))] ValueTypeReference setting
        );

        [FunctionId(FunctionId.NvAPI_DRS_GetSettingIdFromName)]
        public delegate Status NvAPI_DRS_GetSettingIdFromName(
            [In] UnicodeString settingName,
            [Out] out uint settingId
        );

        [FunctionId(FunctionId.NvAPI_DRS_GetSettingNameFromId)]
        public delegate Status NvAPI_DRS_GetSettingNameFromId(
            [In] uint settingId,
            [Out] out UnicodeString settingName
        );

        [FunctionId(FunctionId.NvAPI_DRS_LoadSettings)]
        public delegate Status NvAPI_DRS_LoadSettings([In] DRSSessionHandle sessionHandle);

        [FunctionId(FunctionId.NvAPI_DRS_LoadSettingsFromFile)]
        public delegate Status NvAPI_DRS_LoadSettingsFromFile(
            [In] DRSSessionHandle sessionHandle,
            [In] UnicodeString fileName
        );


        [FunctionId(FunctionId.NvAPI_DRS_RestoreAllDefaults)]
        public delegate Status NvAPI_DRS_RestoreAllDefaults(
            [In] DRSSessionHandle sessionHandle
        );


        [FunctionId(FunctionId.NvAPI_DRS_RestoreProfileDefault)]
        public delegate Status NvAPI_DRS_RestoreProfileDefault(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle
        );

        [FunctionId(FunctionId.NvAPI_DRS_RestoreProfileDefaultSetting)]
        public delegate Status NvAPI_DRS_RestoreProfileDefaultSetting(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In] uint settingId
        );

        [FunctionId(FunctionId.NvAPI_DRS_SaveSettings)]
        public delegate Status NvAPI_DRS_SaveSettings([In] DRSSessionHandle sessionHandle);

        [FunctionId(FunctionId.NvAPI_DRS_SaveSettingsToFile)]
        public delegate Status NvAPI_DRS_SaveSettingsToFile(
            [In] DRSSessionHandle sessionHandle,
            [In] UnicodeString fileName
        );

        [FunctionId(FunctionId.NvAPI_DRS_SetCurrentGlobalProfile)]
        public delegate Status NvAPI_DRS_SetCurrentGlobalProfile(
            [In] DRSSessionHandle sessionHandle,
            [In] UnicodeString profileName
        );

        [FunctionId(FunctionId.NvAPI_DRS_SetProfileInfo)]
        public delegate Status NvAPI_DRS_SetProfileInfo(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In][Accepts(typeof(DRSProfileV1))] ValueTypeReference profile
        );

        [FunctionId(FunctionId.NvAPI_DRS_SetSetting)]
        public delegate Status NvAPI_DRS_SetSetting(
            [In] DRSSessionHandle sessionHandle,
            [In] DRSProfileHandle profileHandle,
            [In][Accepts(typeof(DRSSettingV1))] ValueTypeReference setting
        );
    }

    public static class GPUDelegates
    {
        [FunctionId(FunctionId.NvAPI_EnumLogicalGPUs)]
        public delegate Status NvAPI_EnumLogicalGPUs(
            [In] [Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = LogicalGPUHandle.MaxLogicalGPUs)]
            LogicalGPUHandle[]
                gpuHandles,
            [Out] out uint gpuCount);

        [FunctionId(FunctionId.NvAPI_EnumPhysicalGPUs)]
        public delegate Status NvAPI_EnumPhysicalGPUs(
            [In] [Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = PhysicalGPUHandle.MaxPhysicalGPUs)]
            PhysicalGPUHandle[]
                gpuHandles,
            [Out] out uint gpuCount);

        [FunctionId(FunctionId.NvAPI_EnumTCCPhysicalGPUs)]
        public delegate Status NvAPI_EnumTCCPhysicalGPUs(
            [In] [Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = PhysicalGPUHandle.MaxPhysicalGPUs)]
            PhysicalGPUHandle[]
                gpuHandles,
            [Out] out uint gpuCount);

        /*[FunctionId(FunctionId.NvAPI_GPU_GetLogicalGpuInfo)]
        public delegate Status NvAPI_GPU_GetLogicalGpuInfo(
            [In] LogicalGPUHandle gpuHandle,
            [In][Out] ref LogicalGPUData logicalGPUData
        );*/

        [FunctionId(FunctionId.NvAPI_GetDriverModel)]
        public delegate Status NvAPI_GetDriverModel(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint model);

        [FunctionId(FunctionId.NvAPI_GetGPUIDfromPhysicalGPU)]
        public delegate Status NvAPI_GetGPUIDFromPhysicalGPU(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint gpuId);

        [FunctionId(FunctionId.NvAPI_GetLogicalGPUFromDisplay)]
        public delegate Status NvAPI_GetLogicalGPUFromDisplay(
            [In] DisplayHandle displayHandle,
            [Out] out LogicalGPUHandle gpuHandle);

        [FunctionId(FunctionId.NvAPI_GetLogicalGPUFromPhysicalGPU)]
        public delegate Status NvAPI_GetLogicalGPUFromPhysicalGPU(
            [In] PhysicalGPUHandle physicalGPUHandle,
            [Out] out LogicalGPUHandle logicalGPUHandle);

        [FunctionId(FunctionId.NvAPI_GetPhysicalGPUFromGPUID)]
        public delegate Status NvAPI_GetPhysicalGPUFromGPUID(
            [In] uint gpuId,
            [Out] out PhysicalGPUHandle physicalGpu);

        [FunctionId(FunctionId.NvAPI_GetPhysicalGPUFromUnAttachedDisplay)]
        public delegate Status NvAPI_GetPhysicalGPUFromUnAttachedDisplay(
            [In] UnAttachedDisplayHandle displayHandle,
            [Out] out PhysicalGPUHandle gpuHandle);

        [FunctionId(FunctionId.NvAPI_GetPhysicalGPUsFromDisplay)]
        public delegate Status NvAPI_GetPhysicalGPUsFromDisplay(
            [In] DisplayHandle displayHandle,
            [In] [Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = PhysicalGPUHandle.MaxPhysicalGPUs)]
            PhysicalGPUHandle[]
                gpuHandles,
            [Out] out uint gpuCount);

        [FunctionId(FunctionId.NvAPI_GetPhysicalGPUsFromLogicalGPU)]
        public delegate Status NvAPI_GetPhysicalGPUsFromLogicalGPU(
            [In] LogicalGPUHandle logicalGPUHandle,
            [In] [Out] [MarshalAs(UnmanagedType.LPArray, SizeConst = PhysicalGPUHandle.MaxPhysicalGPUs)]
            PhysicalGPUHandle[]
                gpuHandles,
            [Out] out uint gpuCount);

        [FunctionId(FunctionId.NvAPI_GPU_ClientFanCoolersGetControl)]
        public delegate Status NvAPI_GPU_ClientFanCoolersGetControl(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivateFanCoolersControlV1))] [In]
            ValueTypeReference control);

        [FunctionId(FunctionId.NvAPI_GPU_ClientFanCoolersGetInfo)]
        public delegate Status NvAPI_GPU_ClientFanCoolersGetInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivateFanCoolersInfoV1))] [In]
            ValueTypeReference info);

        [FunctionId(FunctionId.NvAPI_GPU_ClientFanCoolersGetStatus)]
        public delegate Status NvAPI_GPU_ClientFanCoolersGetStatus(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivateFanCoolersStatusV1))] [In]
            ValueTypeReference status);

        [FunctionId(FunctionId.NvAPI_GPU_ClientFanCoolersSetControl)]
        public delegate Status NvAPI_GPU_ClientFanCoolersSetControl(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivateFanCoolersControlV1))] [In]
            ValueTypeReference control);

        [FunctionId(FunctionId.NvAPI_GPU_ClientIllumDevicesGetControl)]
        public delegate Status NvAPI_GPU_ClientIlluminationDevicesGetControl(
            [In] PhysicalGPUHandle gpu,
            [Accepts(typeof(IlluminationDeviceControlParametersV1))] [In]
            ValueTypeReference illuminationDeviceControlInfo
        );

        [FunctionId(FunctionId.NvAPI_GPU_ClientIllumDevicesGetInfo)]
        public delegate Status NvAPI_GPU_ClientIlluminationDevicesGetInfo(
            [In] PhysicalGPUHandle gpu,
            [Accepts(typeof(IlluminationDeviceInfoParametersV1))] [In]
            ValueTypeReference illuminationDevicesInfo
        );

        [FunctionId(FunctionId.NvAPI_GPU_ClientIllumDevicesSetControl)]
        public delegate Status NvAPI_GPU_ClientIlluminationDevicesSetControl(
            [In] PhysicalGPUHandle gpu,
            [Accepts(typeof(IlluminationDeviceControlParametersV1))] [In]
            ValueTypeReference illuminationDeviceControlInfo
        );

        [FunctionId(FunctionId.NvAPI_GPU_ClientIllumZonesGetControl)]
        public delegate Status NvAPI_GPU_ClientIlluminationZonesGetControl(
            [In] PhysicalGPUHandle gpu,
            [Accepts(typeof(IlluminationZoneControlParametersV1))] [In]
            ValueTypeReference illuminationZoneControlInfo
        );

        [FunctionId(FunctionId.NvAPI_GPU_ClientIllumZonesGetInfo)]
        public delegate Status NvAPI_GPU_ClientIlluminationZonesGetInfo(
            [In] PhysicalGPUHandle gpu,
            [Accepts(typeof(IlluminationZoneInfoParametersV1))] [In]
            ValueTypeReference illuminationZoneInfo
        );

        [FunctionId(FunctionId.NvAPI_GPU_ClientIllumZonesSetControl)]
        public delegate Status NvAPI_GPU_ClientIlluminationZonesSetControl(
            [In] PhysicalGPUHandle gpu,
            [Accepts(typeof(IlluminationZoneControlParametersV1))] [In]
            ValueTypeReference illuminationZoneControlInfo
        );

        [FunctionId(FunctionId.NvAPI_GPU_ClientPowerPoliciesGetInfo)]
        public delegate Status NvAPI_GPU_ClientPowerPoliciesGetInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivatePowerPoliciesInfoV1))] [In]
            ValueTypeReference powerInfo);

        [FunctionId(FunctionId.NvAPI_GPU_ClientPowerPoliciesGetStatus)]
        public delegate Status NvAPI_GPU_ClientPowerPoliciesGetStatus(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivatePowerPoliciesStatusV1))] [In]
            ValueTypeReference status);

        [FunctionId(FunctionId.NvAPI_GPU_ClientPowerPoliciesSetStatus)]
        public delegate Status NvAPI_GPU_ClientPowerPoliciesSetStatus(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivatePowerPoliciesStatusV1))] [In]
            ValueTypeReference status);

        [FunctionId(FunctionId.NvAPI_GPU_ClientPowerTopologyGetStatus)]
        public delegate Status NvAPI_GPU_ClientPowerTopologyGetStatus(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivatePowerTopologiesStatusV1))] [In]
            ValueTypeReference status);

        [FunctionId(FunctionId.NvAPI_GPU_EnableDynamicPstates)]
        public delegate Status NvAPI_GPU_EnableDynamicPStates([In] PhysicalGPUHandle physicalGpu);

        [FunctionId(FunctionId.NvAPI_GPU_EnableOverclockedPstates)]
        public delegate Status NvAPI_GPU_EnableOverclockedPStates([In] PhysicalGPUHandle physicalGpu);

        [FunctionId(FunctionId.NvAPI_GPU_GetActiveOutputs)]
        public delegate Status NvAPI_GPU_GetActiveOutputs(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out OutputId outputMask);

        [FunctionId(FunctionId.NvAPI_GPU_GetAGPAperture)]
        public delegate Status NvAPI_GPU_GetAGPAperture(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint size);

        [FunctionId(FunctionId.NvAPI_GPU_GetAllClockFrequencies)]
        public delegate Status NvAPI_GPU_GetAllClockFrequencies(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(ClockFrequenciesV3), typeof(ClockFrequenciesV2), typeof(ClockFrequenciesV1))]
            ValueTypeReference nvClocks);

        [FunctionId(FunctionId.NvAPI_GPU_GetAllDisplayIds)]
        public delegate Status NvAPI_GPU_GetAllDisplayIds(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(DisplayIdsV2))] [In] [Out]
            ValueTypeArray pDisplayIds,
            [In][Out] ref uint displayIdCount);

        [FunctionId(FunctionId.NvAPI_GPU_GetArchInfo)]
        public delegate Status NvAPI_GPU_GetArchInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivateArchitectInfoV2))] [In]
            ValueTypeReference info);

        [FunctionId(FunctionId.NvAPI_GPU_GetBoardInfo)]
        public delegate Status NvAPI_GPU_GetBoardInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [Out][In] ref BoardInfo info);

        [FunctionId(FunctionId.NvAPI_GPU_GetBusId)]
        public delegate Status NvAPI_GPU_GetBusId(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint gpuBusId);

        [FunctionId(FunctionId.NvAPI_GPU_GetBusSlotId)]
        public delegate Status NvAPI_GPU_GetBusSlotId(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint gpuBusSlotId);

        [FunctionId(FunctionId.NvAPI_GPU_GetBusType)]
        public delegate Status NvAPI_GPU_GetBusType(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out GPUBusType gpuBusType);

        [FunctionId(FunctionId.NvAPI_GPU_GetClockBoostLock)]
        public delegate Status NvAPI_GPU_GetClockBoostLock(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateClockBoostLockV2))]
            ValueTypeReference clockLocks);

        [FunctionId(FunctionId.NvAPI_GPU_GetClockBoostMask)]
        public delegate Status NvAPI_GPU_GetClockBoostMask(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateClockBoostMasksV1))]
            ValueTypeReference clockMasks);

        [FunctionId(FunctionId.NvAPI_GPU_GetClockBoostRanges)]
        public delegate Status NvAPI_GPU_GetClockBoostRanges(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateClockBoostRangesV1))]
            ValueTypeReference clockRanges);

        [FunctionId(FunctionId.NvAPI_GPU_GetClockBoostTable)]
        public delegate Status NvAPI_GPU_GetClockBoostTable(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateClockBoostTableV1))]
            ValueTypeReference boostTable);

        [FunctionId(FunctionId.NvAPI_GPU_GetConnectedDisplayIds)]
        public delegate Status NvAPI_GPU_GetConnectedDisplayIds(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(DisplayIdsV2))] [In] [Out]
            ValueTypeArray pDisplayIds,
            [In][Out] ref uint displayIdCount,
            [In] ConnectedIdsFlag flags);

        [FunctionId(FunctionId.NvAPI_GPU_GetCoolerPolicyTable)]
        public delegate Status NvAPI_GPU_GetCoolerPolicyTable(
            [In] PhysicalGPUHandle physicalGpu,
            [In] uint index,
            [In] [Accepts(typeof(PrivateCoolerPolicyTableV1))]
            ValueTypeReference coolerPolicyTable,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetCoolerSettings)]
        public delegate Status NvAPI_GPU_GetCoolerSettings(
            [In] PhysicalGPUHandle physicalGpu,
            [In] CoolerTarget coolerIndex,
            [In] [Accepts(typeof(PrivateCoolerSettingsV1))]
            ValueTypeReference coolerSettings);

        [FunctionId(FunctionId.NvAPI_GPU_GetCoreVoltageBoostPercent)]
        public delegate Status NvAPI_GPU_GetCoreVoltageBoostPercent(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateVoltageBoostPercentV1))]
            ValueTypeReference voltageBoostPercent);

        [FunctionId(FunctionId.NvAPI_GPU_GetCurrentAGPRate)]
        public delegate Status NvAPI_GPU_GetCurrentAGPRate(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint rate);

        [FunctionId(FunctionId.NvAPI_GPU_GetCurrentFanSpeedLevel)]
        public delegate Status NvAPI_GPU_GetCurrentFanSpeedLevel(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint fanLevel);

        [FunctionId(FunctionId.NvAPI_GPU_GetCurrentPCIEDownstreamWidth)]
        public delegate Status NvAPI_GPU_GetCurrentPCIEDownstreamWidth(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint width);

        [FunctionId(FunctionId.NvAPI_GPU_GetCurrentPstate)]
        public delegate Status NvAPI_GPU_GetCurrentPState(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out PerformanceStateId performanceStateId);

        [FunctionId(FunctionId.NvAPI_GPU_GetCurrentThermalLevel)]
        public delegate Status NvAPI_GPU_GetCurrentThermalLevel(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint thermalLevel);

        [FunctionId(FunctionId.NvAPI_GPU_GetCurrentVoltage)]
        public delegate Status NvAPI_GPU_GetCurrentVoltage(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateVoltageStatusV1))]
            ValueTypeReference voltageStatus);

        [FunctionId(FunctionId.NvAPI_GPU_GetDynamicPstatesInfoEx)]
        public delegate Status NvAPI_GPU_GetDynamicPStatesInfoEx(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(DynamicPerformanceStatesInfoV1))]
            ValueTypeReference performanceStatesInfoEx);

        [FunctionId(FunctionId.NvAPI_GPU_GetECCConfigurationInfo)]
        public delegate Status NvAPI_GPU_GetECCConfigurationInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(ECCConfigurationInfoV1))]
            ValueTypeReference eccConfigurationInfo);

        [FunctionId(FunctionId.NvAPI_GPU_GetECCErrorInfo)]
        public delegate Status NvAPI_GPU_GetECCErrorInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [In][Accepts(typeof(ECCErrorInfoV1))] ValueTypeReference eccErrorInfo);

        [FunctionId(FunctionId.NvAPI_GPU_GetECCStatusInfo)]
        public delegate Status NvAPI_GPU_GetECCStatusInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(ECCStatusInfoV1))]
            ValueTypeReference eccStatusInfo);

        [FunctionId(FunctionId.NvAPI_GPU_GetEDID)]
        public delegate Status NvAPI_GPU_GetEDID(
            [In] PhysicalGPUHandle physicalGpu,
            [In] OutputId outputId,
            [Accepts(typeof(EDIDV3), typeof(EDIDV2), typeof(EDIDV1))] [In]
            ValueTypeReference edid);

        [FunctionId(FunctionId.NvAPI_GPU_GetFBWidthAndLocation)]
        public delegate Status NvAPI_GPU_GetFBWidthAndLocation(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint width,
            [Out] out uint location);

        [FunctionId(FunctionId.NvAPI_GPU_GetFoundry)]
        public delegate Status NvAPI_GPU_GetFoundry(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out GPUFoundry pFoundry);

        [FunctionId(FunctionId.NvAPI_GPU_GetFullName)]
        public delegate Status NvAPI_GPU_GetFullName(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out ShortString name);

        [FunctionId(FunctionId.NvAPI_GPU_GetGpuCoreCount)]
        public delegate Status NvAPI_GPU_GetGpuCoreCount(
            [In] PhysicalGPUHandle gpuHandle,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetGPUType)]
        public delegate Status NvAPI_GPU_GetGPUType(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out GPUType gpuType);

        [FunctionId(FunctionId.NvAPI_GPU_GetIllumination)]
        public delegate Status NvAPI_GPU_GetIllumination(
            [Accepts(typeof(GetIlluminationParameterV1))] [In]
            ValueTypeReference illuminationInfo);

        [FunctionId(FunctionId.NvAPI_GPU_GetIRQ)]
        public delegate Status NvAPI_GPU_GetIRQ(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint gpuIRQ);

        [FunctionId(FunctionId.NvAPI_GPU_GetLogicalFBWidthAndLocation)]
        public delegate Status NvAPI_GPU_GetLogicalFBWidthAndLocation(
            [In] LogicalGPUHandle logicalGpu,
            [Out] out uint width,
            [Out] out uint location);

        [FunctionId(FunctionId.NvAPI_GPU_GetMemoryInfo)]
        public delegate Status NvAPI_GPU_GetMemoryInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [In]
            [Accepts(typeof(DisplayDriverMemoryInfoV3), typeof(DisplayDriverMemoryInfoV2),
                typeof(DisplayDriverMemoryInfoV1))]
            ValueTypeReference memoryInfo);

        [FunctionId(FunctionId.NvAPI_GPU_GetOutputType)]
        public delegate Status NvAPI_GPU_GetOutputType(
            [In] PhysicalGPUHandle physicalGpu,
            [In] uint outputId,
            [Out] out OutputType outputType);


        [FunctionId(FunctionId.NvAPI_GPU_GetPartitionCount)]
        public delegate Status NvAPI_GPU_GetPartitionCount(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetPCIEInfo)]
        public delegate Status NvAPI_GPU_GetPCIEInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivatePCIeInfoV2))] [In]
            ValueTypeReference pcieInfo);

        [FunctionId(FunctionId.NvAPI_GPU_GetPCIIdentifiers)]
        public delegate Status NvAPI_GPU_GetPCIIdentifiers(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint deviceId,
            [Out] out uint subSystemId,
            [Out] out uint revisionId,
            [Out] out uint extDeviceId);

        [FunctionId(FunctionId.NvAPI_GPU_GetPerfDecreaseInfo)]
        public delegate Status NvAPI_GPU_GetPerfDecreaseInfo(
            [In] PhysicalGPUHandle gpu,
            [Out] out PerformanceDecreaseReason performanceDecreaseReason);

        [FunctionId(FunctionId.NvAPI_GPU_GetPhysicalFrameBufferSize)]
        public delegate Status NvAPI_GPU_GetPhysicalFrameBufferSize(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint size);

        [FunctionId(FunctionId.NvAPI_GPU_GetPstates20)]
        public delegate Status NvAPI_GPU_GetPStates20(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(
                typeof(PerformanceStates20InfoV3),
                typeof(PerformanceStates20InfoV2),
                typeof(PerformanceStates20InfoV1)
            )]
            [In]
            ValueTypeReference performanceStatesInfo);

        [FunctionId(FunctionId.NvAPI_GPU_GetPstatesInfoEx)]
        public delegate Status NvAPI_GPU_GetPStatesInfoEx(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(
                typeof(PerformanceStatesInfoV3),
                typeof(PerformanceStatesInfoV2),
                typeof(PerformanceStatesInfoV1)
            )]
            [In]
            ValueTypeReference performanceStatesInfo,
            [In] GetPerformanceStatesInfoFlags flags);

        [FunctionId(FunctionId.NvAPI_GPU_GetQuadroStatus)]
        public delegate Status NvAPI_GPU_GetQuadroStatus(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint isQuadro);

        [FunctionId(FunctionId.NvAPI_GPU_GetRamBankCount)]
        public delegate Status NvAPI_GPU_GetRamBankCount(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetRamBusWidth)]
        public delegate Status NvAPI_GPU_GetRamBusWidth(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint busWidth);

        [FunctionId(FunctionId.NvAPI_GPU_GetRamMaker)]
        public delegate Status NvAPI_GPU_GetRamMaker(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out GPUMemoryMaker maker);

        [FunctionId(FunctionId.NvAPI_GPU_GetRamType)]
        public delegate Status NvAPI_GPU_GetRamType(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out GPUMemoryType type);

        [FunctionId(FunctionId.NvAPI_GPU_GetROPCount)]
        public delegate Status NvAPI_GPU_GetROPCount(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetShaderPipeCount)]
        public delegate Status NvAPI_GPU_GetShaderPipeCount(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetShaderSubPipeCount)]
        public delegate Status NvAPI_GPU_GetShaderSubPipeCount(
            [In] PhysicalGPUHandle gpuHandle,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetShortName)]
        public delegate Status NvAPI_GPU_GetShortName(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out ShortString name);

        [FunctionId(FunctionId.NvAPI_GPU_GetSystemType)]
        public delegate Status NvAPI_GPU_GetSystemType(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out SystemType systemType);

        [FunctionId(FunctionId.NvAPI_GPU_GetTachReading)]
        public delegate Status NvAPI_GPU_GetTachReading(
            [In] PhysicalGPUHandle gpuHandle,
            [Out] out uint value);

        [FunctionId(FunctionId.NvAPI_GPU_GetThermalPoliciesInfo)]
        public delegate Status NvAPI_GPU_GetThermalPoliciesInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivateThermalPoliciesInfoV2))] [In]
            ValueTypeReference info);

        [FunctionId(FunctionId.NvAPI_GPU_GetThermalPoliciesStatus)]
        public delegate Status NvAPI_GPU_GetThermalPoliciesStatus(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivateThermalPoliciesStatusV2))] [In]
            ValueTypeReference info);

        [FunctionId(FunctionId.NvAPI_GPU_GetThermalSettings)]
        public delegate Status NvAPI_GPU_GetThermalSettings(
            [In] PhysicalGPUHandle physicalGpu,
            [In] ThermalSettingsTarget sensorIndex,
            [In] [Accepts(typeof(ThermalSettingsV2), typeof(ThermalSettingsV1))]
            ValueTypeReference thermalSettings);

        [FunctionId(FunctionId.NvAPI_GPU_GetTotalSMCount)]
        public delegate Status NvAPI_GPU_GetTotalSMCount(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetTotalSPCount)]
        public delegate Status NvAPI_GPU_GetTotalSPCount(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetTotalTPCCount)]
        public delegate Status NvAPI_GPU_GetTotalTPCCount(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_GetUsages)]
        public delegate Status NvAPI_GPU_GetUsages(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateUsagesInfoV1))]
            ValueTypeReference usageInfo);

        [FunctionId(FunctionId.NvAPI_GPU_GetVbiosOEMRevision)]
        public delegate Status NvAPI_GPU_GetVbiosOEMRevision(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint biosOEMRevision);

        [FunctionId(FunctionId.NvAPI_GPU_GetVbiosRevision)]
        public delegate Status NvAPI_GPU_GetVbiosRevision(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint biosRevision);

        [FunctionId(FunctionId.NvAPI_GPU_GetVbiosVersionString)]
        public delegate Status NvAPI_GPU_GetVbiosVersionString(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out ShortString biosVersion);

        [FunctionId(FunctionId.NvAPI_GPU_GetVFPCurve)]
        public delegate Status NvAPI_GPU_GetVFPCurve(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateVFPCurveV1))]
            ValueTypeReference vfpCurve);

        [FunctionId(FunctionId.NvAPI_GPU_GetVirtualFrameBufferSize)]
        public delegate Status NvAPI_GPU_GetVirtualFrameBufferSize(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint size);

        [FunctionId(FunctionId.NvAPI_GPU_GetVPECount)]
        public delegate Status NvAPI_GPU_GetVPECount(
            [In] PhysicalGPUHandle physicalGpu,
            [Out] out uint count);

        [FunctionId(FunctionId.NvAPI_GPU_PerfPoliciesGetInfo)]
        public delegate Status NvAPI_GPU_PerfPoliciesGetInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivatePerformanceInfoV1))]
            ValueTypeReference performanceInfo);

        [FunctionId(FunctionId.NvAPI_GPU_PerfPoliciesGetStatus)]
        public delegate Status NvAPI_GPU_PerfPoliciesGetStatus(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivatePerformanceStatusV1))]
            ValueTypeReference performanceStatus);

        [FunctionId(FunctionId.NvAPI_GPU_QueryActiveApps)]
        public delegate Status NvAPI_GPU_QueryActiveApps(
            [In] PhysicalGPUHandle gpu,
            [In] [Accepts(typeof(PrivateActiveApplicationV2))]
            ValueTypeArray applications,
            [In][Out] ref uint numberOfApplications
        );


        [FunctionId(FunctionId.NvAPI_GPU_QueryIlluminationSupport)]
        public delegate Status NvAPI_GPU_QueryIlluminationSupport(
            [Accepts(typeof(QueryIlluminationSupportParameterV1))] [In]
            ValueTypeReference illuminationSupportInfo);


        ///////////////////////////////////////////////////////////////////////////////
        // FUNCTION NAME:   NvAPI_GPU_QueryWorkstationFeatureSupport
        //
        //! \fn NvAPI_GPU_QueryWorkstationFeatureSupport(NvPhysicalGpuHandle physicalGpu, NV_GPU_WORKSTATION_FEATURE_TYPE gpuWorkstationFeature)
        //! \code
        //! DESCRIPTION:     Indicates whether a queried workstation feature is supported by the requested GPU.
        //!
        //! SUPPORTED OS:  Windows 10 and higher
        //!
        //! \since Release: 440
        //!
        //! DESCRIPTION:     This API, when called with a valid physical gpu handle as Input, lets caller know whether the given workstation feature is supported by this GPU.
        //!
        //! PARAMETERS:      physicalGpu(IN)            : The handle of the GPU for the which caller wants to get the support information.
        //!                  gpuWorkstationFeature(IN ) : The feature for the GPU in question. One of the values from enum NV_GPU_WORKSTATION_FEATURE_TYPE.
        //!
        //! \return  This API can return any of the error codes enumerated in #NvAPI_Status listed below
        //!
        //! \retval ::NVAPI_OK the queried workstation feature is supported on the given GPU.
        //! \retval ::NVAPI_NO_IMPLEMENTATION the current driver doesn't support this interface.
        //! \retval ::NVAPI_INVALID_HANDLE the incoming physicalGpu handle is invalid.
        //! \retval ::NVAPI_NOT_SUPPORTED the requested gpuWorkstationFeature is not supported in the selected GPU.
        //! \retval ::NVAPI_SETTING_NOT_FOUND the requested gpuWorkstationFeature is unknown to the current driver version.
        //!
        //! \endcode
        //! \ingroup gpu
        ///////////////////////////////////////////////////////////////////////////////
        //API_INTERFACE NvAPI_GPU_QueryWorkstationFeatureSupport(NvPhysicalGpuHandle physicalGpu, NV_GPU_WORKSTATION_FEATURE_TYPE gpuWorkstationFeature);
        [FunctionId(FunctionId.NvAPI_GPU_QueryWorkstationFeatureSupport)]
        public delegate Status NvAPI_GPU_QueryWorkstationFeatureSupport(
            [In] PhysicalGPUHandle physicalGpu,
            [In] WorkstationFeatureType gpuWorkstationFeature
        );

        [FunctionId(FunctionId.NvAPI_GPU_ResetECCErrorInfo)]
        public delegate Status NvAPI_GPU_ResetECCErrorInfo(
            [In] PhysicalGPUHandle physicalGpu,
            [In] byte resetCurrent,
            [In] byte resetAggregated
        );

        [FunctionId(FunctionId.NvAPI_GPU_RestoreCoolerPolicyTable)]
        public delegate Status NvAPI_GPU_RestoreCoolerPolicyTable(
            [In] PhysicalGPUHandle physicalGpu,
            [In] uint[] indexes,
            [In] uint indexesCount,
            [In] CoolerPolicy policy);

        [FunctionId(FunctionId.NvAPI_GPU_RestoreCoolerSettings)]
        public delegate Status NvAPI_GPU_RestoreCoolerSettings(
            [In] PhysicalGPUHandle physicalGpu,
            [In] uint[] indexes,
            [In] uint indexesCount);

        [FunctionId(FunctionId.NvAPI_GPU_SetClockBoostLock)]
        public delegate Status NvAPI_GPU_SetClockBoostLock(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateClockBoostLockV2))]
            ValueTypeReference clockLocks);

        [FunctionId(FunctionId.NvAPI_GPU_SetClockBoostTable)]
        public delegate Status NvAPI_GPU_SetClockBoostTable(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateClockBoostTableV1))]
            ValueTypeReference boostTable);

        [FunctionId(FunctionId.NvAPI_GPU_SetCoolerLevels)]
        public delegate Status NvAPI_GPU_SetCoolerLevels(
            [In] PhysicalGPUHandle physicalGpu,
            [In] uint index,
            [In] [Accepts(typeof(PrivateCoolerLevelsV1))]
            ValueTypeReference coolerLevels,
            [In] uint count);

        [FunctionId(FunctionId.NvAPI_GPU_SetCoolerPolicyTable)]
        public delegate Status NvAPI_GPU_SetCoolerPolicyTable(
            [In] PhysicalGPUHandle physicalGpu,
            [In] uint index,
            [In] [Accepts(typeof(PrivateCoolerPolicyTableV1))]
            ValueTypeReference coolerLevels,
            [In] uint count);

        [FunctionId(FunctionId.NvAPI_GPU_SetCoreVoltageBoostPercent)]
        public delegate Status NvAPI_GPU_SetCoreVoltageBoostPercent(
            [In] PhysicalGPUHandle physicalGpu,
            [In] [Accepts(typeof(PrivateVoltageBoostPercentV1))]
            ValueTypeReference voltageBoostPercent);

        [FunctionId(FunctionId.NvAPI_GPU_SetECCConfiguration)]
        public delegate Status NvAPI_GPU_SetECCConfiguration(
            [In] PhysicalGPUHandle physicalGpu,
            [In] byte isEnable,
            [In] byte isEnableImmediately
        );

        [FunctionId(FunctionId.NvAPI_GPU_SetEDID)]
        public delegate Status NvAPI_GPU_SetEDID(
            [In] PhysicalGPUHandle physicalGpu,
            [In] uint outputId,
            [Accepts(typeof(EDIDV3), typeof(EDIDV2), typeof(EDIDV1))] [In]
            ValueTypeReference edid);


        [FunctionId(FunctionId.NvAPI_GPU_SetIllumination)]
        public delegate Status NvAPI_GPU_SetIllumination(
            [Accepts(typeof(SetIlluminationParameterV1))] [In]
            ValueTypeReference illuminationInfo);

        [FunctionId(FunctionId.NvAPI_GPU_SetPstates20)]
        public delegate Status NvAPI_GPU_SetPStates20(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PerformanceStates20InfoV3), typeof(PerformanceStates20InfoV2),
                typeof(PerformanceStates20InfoV1))]
            [In]
            ValueTypeReference performanceStatesInfo);

        [FunctionId(FunctionId.NvAPI_GPU_SetThermalPoliciesStatus)]
        public delegate Status NvAPI_GPU_SetThermalPoliciesStatus(
            [In] PhysicalGPUHandle physicalGpu,
            [Accepts(typeof(PrivateThermalPoliciesStatusV2))] [In]
            ValueTypeReference info);

        [FunctionId(FunctionId.NvAPI_GPU_ValidateOutputCombination)]
        public delegate Status NvAPI_GPU_ValidateOutputCombination(
            [In] PhysicalGPUHandle physicalGpu,
            [In] OutputId outputMask);

        [FunctionId(FunctionId.NvAPI_I2CRead)]
        public delegate Status NvAPI_I2CRead(
            [In] PhysicalGPUHandle physicalGpu,
            [In][Accepts(typeof(I2CInfoV3), typeof(I2CInfoV2))] ValueTypeReference i2cInfo
        );

        [FunctionId(FunctionId.NvAPI_I2CWrite)]
        public delegate Status NvAPI_I2CWrite(
            [In] PhysicalGPUHandle physicalGpu,
            [In][Accepts(typeof(I2CInfoV3), typeof(I2CInfoV2))] ValueTypeReference i2cInfo
        );

        [FunctionId(FunctionId.NvAPI_SYS_GetDisplayIdFromGpuAndOutputId)]
        public delegate Status NvAPI_SYS_GetDisplayIdFromGpuAndOutputId(
            [In] PhysicalGPUHandle gpu,
            [In] OutputId outputId,
            [Out] out uint displayId);

        [FunctionId(FunctionId.NvAPI_SYS_GetGpuAndOutputIdFromDisplayId)]
        public delegate Status NvAPI_SYS_GetGpuAndOutputIdFromDisplayId(
            [In] uint displayId,
            [Out] out PhysicalGPUHandle gpu,
            [Out] out OutputId outputId);

        [FunctionId(FunctionId.NvAPI_SYS_GetPhysicalGpuFromDisplayId)]
        public delegate Status NvAPI_SYS_GetPhysicalGpuFromDisplayId(
            [In] uint displayId,
            [Out] out PhysicalGPUHandle gpu);
    }


    public static class StereoDelegates
    {
        [FunctionId(FunctionId.NvAPI_D3D1x_CreateSwapChain)]
        public delegate Status NvAPI_D3D1x_CreateSwapChain(
            [In] StereoHandle stereoHandle,
            [In] IntPtr dxgiSwapChainDescription,
            [Out] out IntPtr dxgiSwapChain,
            [In] StereoSwapChainMode mode
        );

        [FunctionId(FunctionId.NvAPI_D3D9_CreateSwapChain)]
        public delegate Status NvAPI_D3D9_CreateSwapChain(
            [In] StereoHandle stereoHandle,
            [In] IntPtr d3dPresentParameters,
            [Out] out IntPtr direct3DSwapChain9,
            [In] StereoSwapChainMode mode
        );

        [FunctionId(FunctionId.NvAPI_Stereo_Activate)]
        public delegate Status NvAPI_Stereo_Activate(
            [In] StereoHandle stereoHandle
        );

        [FunctionId(FunctionId.NvAPI_Stereo_CaptureJpegImage)]
        public delegate Status NvAPI_Stereo_CaptureJpegImage(
            [In] StereoHandle stereoHandle,
            [In] uint quality
        );

        [FunctionId(FunctionId.NvAPI_Stereo_CapturePngImage)]
        public delegate Status NvAPI_Stereo_CapturePngImage(
            [In] StereoHandle stereoHandle
        );

        [FunctionId(FunctionId.NvAPI_Stereo_CreateConfigurationProfileRegistryKey)]
        public delegate Status NvAPI_Stereo_CreateConfigurationProfileRegistryKey(
            [In] StereoRegistryProfileType registryProfileType
        );

        [FunctionId(FunctionId.NvAPI_Stereo_CreateHandleFromIUnknown)]
        public delegate Status NvAPI_Stereo_CreateHandleFromIUnknown(
            [In] IntPtr d3dDevice,
            [Out] out StereoHandle stereoHandle
        );

        [FunctionId(FunctionId.NvAPI_Stereo_Deactivate)]
        public delegate Status NvAPI_Stereo_Deactivate(
            [In] StereoHandle stereoHandle
        );

        [FunctionId(FunctionId.NvAPI_Stereo_Debug_WasLastDrawStereoized)]
        public delegate Status NvAPI_Stereo_Debug_WasLastDrawStereoized(
            [In] StereoHandle stereoHandle,
            [Out] out byte wasStereo
        );

        [FunctionId(FunctionId.NvAPI_Stereo_DecreaseConvergence)]
        public delegate Status NvAPI_Stereo_DecreaseConvergence(
            [In] StereoHandle stereoHandle
        );

        [FunctionId(FunctionId.NvAPI_Stereo_DecreaseSeparation)]
        public delegate Status NvAPI_Stereo_DecreaseSeparation(
            [In] StereoHandle stereoHandle
        );

        [FunctionId(FunctionId.NvAPI_Stereo_DeleteConfigurationProfileRegistryKey)]
        public delegate Status NvAPI_Stereo_DeleteConfigurationProfileRegistryKey(
            [In] StereoRegistryProfileType registryProfileType
        );

        [FunctionId(FunctionId.NvAPI_Stereo_DeleteConfigurationProfileValue)]
        public delegate Status NvAPI_Stereo_DeleteConfigurationProfileValue(
            [In] StereoRegistryProfileType registryProfileType,
            [In] StereoRegistryIdentification registryId
        );

        [FunctionId(FunctionId.NvAPI_Stereo_DestroyHandle)]
        public delegate Status NvAPI_Stereo_DestroyHandle(
            [In] StereoHandle stereoHandle
        );

        [FunctionId(FunctionId.NvAPI_Stereo_Disable)]
        public delegate Status NvAPI_Stereo_Disable();

        [FunctionId(FunctionId.NvAPI_Stereo_Enable)]
        public delegate Status NvAPI_Stereo_Enable();


        [FunctionId(FunctionId.NvAPI_Stereo_GetConvergence)]
        public delegate Status NvAPI_Stereo_GetConvergence(
            [In] StereoHandle stereoHandle,
            [Out] out float convergence
        );

        [FunctionId(FunctionId.NvAPI_Stereo_GetDefaultProfile)]
        public delegate Status NvAPI_Stereo_GetDefaultProfile(
            [In] uint bufferSize,
            [In] IntPtr stringBuffer,
            [Out] out uint stringSize
        );

        [FunctionId(FunctionId.NvAPI_Stereo_GetEyeSeparation)]
        public delegate Status NvAPI_Stereo_GetEyeSeparation(
            [In] StereoHandle stereoHandle,
            [Out] out float separation
        );

        [FunctionId(FunctionId.NvAPI_Stereo_GetFrustumAdjustMode)]
        public delegate Status NvAPI_Stereo_GetFrustumAdjustMode(
            [In] StereoHandle stereoHandle,
            [Out] out StereoFrustumAdjustMode frustumAdjustMode
        );

        [FunctionId(FunctionId.NvAPI_Stereo_GetSeparation)]
        public delegate Status NvAPI_Stereo_GetSeparation(
            [In] StereoHandle stereoHandle,
            [Out] out float separationPercentage
        );


        [FunctionId(FunctionId.NvAPI_Stereo_GetStereoSupport)]
        public delegate Status NvAPI_Stereo_GetStereoSupport(
            [In] IntPtr monitorHandle,
            [In] [Accepts(typeof(StereoCapabilitiesV1))]
            ValueTypeReference capabilities
        );

        [FunctionId(FunctionId.NvAPI_Stereo_GetSurfaceCreationMode)]
        public delegate Status NvAPI_Stereo_GetSurfaceCreationMode(
            [In] StereoHandle stereoHandle,
            [Out] out StereoSurfaceCreateMode surfaceCreateMode
        );

        [FunctionId(FunctionId.NvAPI_Stereo_IncreaseConvergence)]
        public delegate Status NvAPI_Stereo_IncreaseConvergence(
            [In] StereoHandle stereoHandle
        );

        [FunctionId(FunctionId.NvAPI_Stereo_IncreaseSeparation)]
        public delegate Status NvAPI_Stereo_IncreaseSeparation(
            [In] StereoHandle stereoHandle
        );

        [FunctionId(FunctionId.NvAPI_Stereo_InitActivation)]
        public delegate Status NvAPI_Stereo_InitActivation(
            [In] StereoHandle stereoHandle,
            [In] StereoActivationFlag flag
        );

        [FunctionId(FunctionId.NvAPI_Stereo_IsActivated)]
        public delegate Status NvAPI_Stereo_IsActivated(
            [In] StereoHandle stereoHandle,
            [Out] out byte isStereoActive
        );

        [FunctionId(FunctionId.NvAPI_Stereo_IsEnabled)]
        public delegate Status NvAPI_Stereo_IsEnabled([Out] out byte isEnable);

        [FunctionId(FunctionId.NvAPI_Stereo_IsWindowedModeSupported)]
        public delegate Status NvAPI_Stereo_IsWindowedModeSupported([Out] out byte isEnable);

        [FunctionId(FunctionId.NvAPI_Stereo_ReverseStereoBlitControl)]
        public delegate Status NvAPI_Stereo_ReverseStereoBlitControl(
            [In] StereoHandle stereoHandle,
            [In] byte turnOn
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetActiveEye)]
        public delegate Status NvAPI_Stereo_SetActiveEye(
            [In] StereoHandle stereoHandle,
            [In] StereoActiveEye activeEye
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetConfigurationProfileValue)]
        public delegate Status NvAPI_Stereo_SetConfigurationProfileValueFloat(
            [In] StereoRegistryProfileType registryProfileType,
            [In] StereoRegistryIdentification registryId,
            [In] ref float value
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetConfigurationProfileValue)]
        public delegate Status NvAPI_Stereo_SetConfigurationProfileValueInteger(
            [In] StereoRegistryProfileType registryProfileType,
            [In] StereoRegistryIdentification registryId,
            [In] ref int value
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetConvergence)]
        public delegate Status NvAPI_Stereo_SetConvergence(
            [In] StereoHandle stereoHandle,
            [In] float newConvergence
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetDefaultProfile)]
        public delegate Status NvAPI_Stereo_SetDefaultProfile(
            [In][MarshalAs(UnmanagedType.LPStr)] string profileName
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetDriverMode)]
        public delegate Status NvAPI_Stereo_SetDriverMode(
            [In] StereoDriverMode driverMode
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetFrustumAdjustMode)]
        public delegate Status NvAPI_Stereo_SetFrustumAdjustMode(
            [In] StereoHandle stereoHandle,
            [In] StereoFrustumAdjustMode frustumAdjustMode
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetNotificationMessage)]
        public delegate Status NvAPI_Stereo_SetNotificationMessage(
            [In] StereoHandle stereoHandle,
            [In] ulong windowHandle,
            [In] ulong messageId
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetSeparation)]
        public delegate Status NvAPI_Stereo_SetSeparation(
            [In] StereoHandle stereoHandle,
            [In] float newSeparationPercentage
        );

        [FunctionId(FunctionId.NvAPI_Stereo_SetSurfaceCreationMode)]
        public delegate Status NvAPI_Stereo_SetSurfaceCreationMode(
            [In] StereoHandle stereoHandle,
            [In] StereoSurfaceCreateMode newSurfaceCreateMode
        );

        [FunctionId(FunctionId.NvAPI_Stereo_Trigger_Activation)]
        public delegate Status NvAPI_Stereo_Trigger_Activation(
            [In] StereoHandle stereoHandle
        );
    }
}
