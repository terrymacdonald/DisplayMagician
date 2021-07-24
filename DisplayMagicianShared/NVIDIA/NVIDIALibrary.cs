using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using DisplayMagicianShared;
using System.ComponentModel;
using DisplayMagicianShared.Windows;

namespace DisplayMagicianShared.NVIDIA
{
    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_ADAPTER_CONFIG : IEquatable<NVIDIA_ADAPTER_CONFIG>
    {
        public int AdapterDeviceNumber;
        public int AdapterBusNumber;
        public int AdapterIndex;
        public bool IsPrimaryAdapter;
        //public ADL_DISPLAY_MAP[] DisplayMaps;
        //public ADL_DISPLAY_TARGET[] DisplayTargets;
        public int SLSMapIndex;
        public bool IsSLSEnabled;
        //public ADL_SLS_MAP[] SLSMap;

        public bool Equals(NVIDIA_ADAPTER_CONFIG other)
        => AdapterIndex == other.AdapterIndex &&
           AdapterBusNumber == other.AdapterBusNumber &&
           AdapterDeviceNumber == other.AdapterDeviceNumber &&
           IsPrimaryAdapter == other.IsPrimaryAdapter &&
           //DisplayMaps.SequenceEqual(other.DisplayMaps) &&
           //DisplayTargets.SequenceEqual(other.DisplayTargets);
           SLSMapIndex == other.SLSMapIndex &&
           IsSLSEnabled == other.IsSLSEnabled;

        public override int GetHashCode()
        {
            return (AdapterIndex, AdapterBusNumber, AdapterDeviceNumber, IsPrimaryAdapter, SLSMapIndex, IsSLSEnabled).GetHashCode();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NVIDIA_DISPLAY_CONFIG : IEquatable<NVIDIA_DISPLAY_CONFIG>
    {
        //public Dictionary<ulong, string> DisplayAdapters;
        public List<NVIDIA_ADAPTER_CONFIG> AdapterConfigs;
        //public DISPLAYCONFIG_MODE_INFO[] DisplayConfigModes;
        //public ADVANCED_HDR_INFO_PER_PATH[] DisplayHDRStates;
        public WINDOWS_DISPLAY_CONFIG WindowsDisplayConfig;

        public bool Equals(NVIDIA_DISPLAY_CONFIG other)
        => AdapterConfigs.SequenceEqual(other.AdapterConfigs) &&
           //DisplayConfigPaths.SequenceEqual(other.DisplayConfigPaths) &&
           //DisplayConfigModes.SequenceEqual(other.DisplayConfigModes) &&
           //DisplayHDRStates.SequenceEqual(other.DisplayHDRStates) && 
           WindowsDisplayConfig.Equals(other.WindowsDisplayConfig);

        public override int GetHashCode()
        {
            return (AdapterConfigs, WindowsDisplayConfig).GetHashCode();
        }
    }

    class NVIDIALibrary : IDisposable
    {

        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static NVIDIALibrary _instance = new NVIDIALibrary();

        private static WinLibrary _winLibrary = new WinLibrary();

        private bool _initialised = false;
        private bool _haveSessionHandle = false;

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        private IntPtr _nvapiSessionHandle = IntPtr.Zero;

        static NVIDIALibrary() { }
        public NVIDIALibrary()
        {

            try
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Attempting to load the NVIDIA NVAPI DLL {NVImport.NVAPI_DLL}");
                // Attempt to prelink all of the NVAPI functions
                Marshal.PrelinkAll(typeof(NVImport));

                // If we get here then we definitely have the NVIDIA driver available.
                NVAPI_STATUS NVStatus;
                SharedLogger.logger.Trace("NVIDIALibrary/NVIDIALibrary: Intialising NVIDIA NVAPI library interface");
                // Step 1: Initialise the NVAPI
                try
                {
                    NVStatus = NVImport.NvAPI_Initialize();
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        _initialised = true;
                        SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: NVIDIA NVAPI library was initialised successfully");
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

                // Step 2: Get a session handle that we can use for all other interactions
                try
                {
                    NVStatus = NVImport.NvAPI_DRS_CreateSession(out _nvapiSessionHandle);
                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                    {
                        _haveSessionHandle = true;
                        SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: NVIDIA NVAPI library DRS session handle was created successfully");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Error creating a NVAPI library DRS session handle. NvAPI_DRS_CreateSession() returned error code {NVStatus}");
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Trace(ex, $"NVIDIALibrary/NVIDIALibrary: Exception creating a NVAPI library DRS session handle. NvAPI_DRS_CreateSession() caused an exception.");
                }

                _winLibrary = WinLibrary.GetLibrary();

            }
            catch (DllNotFoundException ex)
            {
                // If this fires, then the DLL isn't available, so we need don't try to do anything else
                SharedLogger.logger.Info(ex, $"NVIDIALibrary/NVIDIALibrary: Exception trying to load the NVIDIA NVAPI DLL {NVImport.NVAPI_DLL}. This generally means you don't have the NVIDIA driver installed.");
            }

        }

        ~NVIDIALibrary()
        {
            SharedLogger.logger.Trace("NVIDIALibrary/~NVIDIALibrary: Destroying NVIDIA NVAPI library interface");
            // If the NVAPI library was initialised, then we need to free it up.
            if (_initialised)
            {
                NVAPI_STATUS NVStatus;
                // If we have a session handle we need to free it up first
                if (_haveSessionHandle)
                {
                    try
                    {
                        NVStatus = NVImport.NvAPI_DRS_DestorySession(_nvapiSessionHandle);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            _haveSessionHandle = true;
                            SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: NVIDIA NVAPI library DRS session handle was successfully destroyed");
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: Error destroying the NVAPI library DRS session handle. NvAPI_DRS_DestorySession() returned error code {NVStatus}");
                        }
                    }
                    catch (Exception ex)
                    {
                        SharedLogger.logger.Trace(ex, $"NVIDIALibrary/NVIDIALibrary: Exception destroying the NVIDIA NVAPI library. NvAPI_DRS_DestorySession() caused an exception.");
                    }
                }

                try
                {
                    NVImport.NvAPI_Unload();
                    SharedLogger.logger.Trace($"NVIDIALibrary/NVIDIALibrary: NVIDIA NVAPI library was unloaded successfully");
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Trace(ex, $"NVIDIALibrary/NVIDIALibrary: Exception unloading the NVIDIA NVAPI library. NvAPI_Unload() caused an exception.");
                }

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

                //NVImport.ADL_Main_Control_Destroy();

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

        public static NVIDIALibrary GetLibrary()
        {
            return _instance;
        }



        public NVIDIA_DISPLAY_CONFIG GetActiveConfig()
        {
            SharedLogger.logger.Trace($"NVIDIALibrary/GetActiveConfig: Getting the currently active config");
            bool allDisplays = true;
            return GetNVIDIADisplayConfig(allDisplays);
        }

        private NVIDIA_DISPLAY_CONFIG GetNVIDIADisplayConfig(bool allDisplays = false)
        {
            NVIDIA_DISPLAY_CONFIG myDisplayConfig = new NVIDIA_DISPLAY_CONFIG();
            myDisplayConfig.AdapterConfigs = new List<NVIDIA_ADAPTER_CONFIG>();

            if (_initialised)
            {
                // We want to get the Windows CCD information and store it for later so that we record
                // display sizes, and screen positions and the like.
                //myDisplayConfig.WindowsDisplayConfig = _winLibrary.GetActiveConfig();
            }
            else
            {
                SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: ERROR - Tried to run GetNVIDIADisplayConfig but the NVIDIA ADL library isn't initialised!");
                throw new NVIDIALibraryException($"Tried to run GetNVIDIADisplayConfig but the NVIDIA ADL library isn't initialised!");
            }

            // Return the configuration
            return myDisplayConfig;
        }


        public string PrintActiveConfig()
        {
            string stringToReturn = "";

            // Get the size of the largest Active Paths and Modes arrays
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"NVIDIALibrary/PrintActiveConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new NVIDIALibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/PrintActiveConfig: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"NVIDIALibrary/PrintActiveConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new NVIDIALibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Error($"NVIDIALibrary/PrintActiveConfig: ERROR - The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                    throw new NVIDIALibraryException($"The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                }
                else if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"NVIDIALibrary/PrintActiveConfig: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again");
                    throw new NVIDIALibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again.");
                }
            }
            else if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"NVIDIALibrary/PrintActiveConfig: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays");
                throw new NVIDIALibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays.");
            }

            foreach (var path in paths)
            {
                stringToReturn += $"----++++==== Path ====++++----\n";

                // get display source name
                var sourceInfo = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
                sourceInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
                sourceInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DEVICE_NAME>();
                sourceInfo.Header.AdapterId = path.SourceInfo.AdapterId;
                sourceInfo.Header.Id = path.SourceInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref sourceInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Found Display Source {sourceInfo.ViewGdiDeviceName} for source {path.SourceInfo.Id}.");
                    stringToReturn += $"****** Interrogating Display Source {path.SourceInfo.Id} *******\n";
                    stringToReturn += $"Found Display Source {sourceInfo.ViewGdiDeviceName}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the source info for source adapter #{path.SourceInfo.AdapterId}");
                }


                // get display target name
                var targetInfo = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
                targetInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                targetInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
                targetInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                targetInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref targetInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Connector Instance: {targetInfo.ConnectorInstance} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: EDID Manufacturer ID: {targetInfo.EdidManufactureId} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: EDID Product Code ID: {targetInfo.EdidProductCodeId} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Flags Friendly Name from EDID: {targetInfo.Flags.FriendlyNameFromEdid} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Flags Friendly Name Forced: {targetInfo.Flags.FriendlyNameForced} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Flags EDID ID is Valid: {targetInfo.Flags.EdidIdsValid} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Monitor Device Path: {targetInfo.MonitorDevicePath} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Monitor Friendly Device Name: {targetInfo.MonitorFriendlyDeviceName} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Output Technology: {targetInfo.OutputTechnology} for source {path.TargetInfo.Id}.");

                    stringToReturn += $"****** Interrogating Display Target {targetInfo.MonitorFriendlyDeviceName} *******\n";
                    stringToReturn += $" Connector Instance: {targetInfo.ConnectorInstance}\n";
                    stringToReturn += $" EDID Manufacturer ID: {targetInfo.EdidManufactureId}\n";
                    stringToReturn += $" EDID Product Code ID: {targetInfo.EdidProductCodeId}\n";
                    stringToReturn += $" Flags Friendly Name from EDID: {targetInfo.Flags.FriendlyNameFromEdid}\n";
                    stringToReturn += $" Flags Friendly Name Forced: {targetInfo.Flags.FriendlyNameForced}\n";
                    stringToReturn += $" Flags EDID ID is Valid: {targetInfo.Flags.EdidIdsValid}\n";
                    stringToReturn += $" Monitor Device Path: {targetInfo.MonitorDevicePath}\n";
                    stringToReturn += $" Monitor Friendly Device Name: {targetInfo.MonitorFriendlyDeviceName}\n";
                    stringToReturn += $" Output Technology: {targetInfo.OutputTechnology}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.TargetInfo.Id}");
                }


                // get display adapter name
                var adapterInfo = new DISPLAYCONFIG_ADAPTER_NAME();
                adapterInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME;
                adapterInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_ADAPTER_NAME>();
                adapterInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                adapterInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref adapterInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/PrintActiveConfig: Found Adapter Device Path {adapterInfo.AdapterDevicePath} for source {path.TargetInfo.AdapterId}.");
                    stringToReturn += $"****** Interrogating Display Adapter {adapterInfo.AdapterDevicePath} *******\n";
                    stringToReturn += $" Display Adapter {adapterInfo.AdapterDevicePath}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the adapter device path for target #{path.TargetInfo.AdapterId}");
                }

                // get display target preferred mode
                var targetPreferredInfo = new DISPLAYCONFIG_TARGET_PREFERRED_MODE();
                targetPreferredInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE;
                targetPreferredInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_TARGET_PREFERRED_MODE>();
                targetPreferredInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                targetPreferredInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref targetPreferredInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Target Preferred Width {targetPreferredInfo.Width} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Target Preferred Height {targetPreferredInfo.Height} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Active Size: ({targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ActiveSize.Cx}x{targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ActiveSize.Cy} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Total Size: ({targetPreferredInfo.TargetMode.TargetVideoSignalInfo.TotalSize.Cx}x{targetPreferredInfo.TargetMode.TargetVideoSignalInfo.TotalSize.Cy} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Target Video Signal Info HSync Frequency: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.HSyncFreq} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Target Video Signal Info VSync Frequency: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.VSyncFreq} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Pixel Rate: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.PixelRate} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Scan Line Ordering: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ScanLineOrdering} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Video Standard: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.VideoStandard} for target {path.TargetInfo.Id}.");

                    stringToReturn += $"****** Interrogating Target Preferred Mode for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" Target Preferred Width {targetPreferredInfo.Width} for target {path.TargetInfo.Id}\n";
                    stringToReturn += $" Target Preferred Height {targetPreferredInfo.Height} for target {path.TargetInfo.Id}\n";
                    stringToReturn += $" Target Video Signal Info Active Size: ({targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ActiveSize.Cx}x{targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ActiveSize.Cy}\n";
                    stringToReturn += $" Target Video Signal Info Total Size: ({targetPreferredInfo.TargetMode.TargetVideoSignalInfo.TotalSize.Cx}x{targetPreferredInfo.TargetMode.TargetVideoSignalInfo.TotalSize.Cy}\n";
                    stringToReturn += $" Target Video Signal Info HSync Frequency: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.HSyncFreq}\n";
                    stringToReturn += $" Target Video Signal Info VSync Frequency: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.VSyncFreq}\n";
                    stringToReturn += $" Target Video Signal Info Pixel Rate: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.PixelRate}\n";
                    stringToReturn += $" Target Video Signal Info Scan Line Ordering: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ScanLineOrdering}\n";
                    stringToReturn += $" Target Video Signal Info Video Standard: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.VideoStandard}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the preferred target name for display #{path.TargetInfo.Id}");
                }

                // get display target base type
                var targetBaseTypeInfo = new DISPLAYCONFIG_TARGET_BASE_TYPE();
                targetBaseTypeInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_BASE_TYPE;
                targetBaseTypeInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_TARGET_BASE_TYPE>();
                targetBaseTypeInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                targetBaseTypeInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref targetBaseTypeInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Virtual Resolution is Disabled: {targetBaseTypeInfo.BaseOutputTechnology} for target {path.TargetInfo.Id}.");

                    stringToReturn += $"****** Interrogating Target Base Type for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" Base Output Technology: {targetBaseTypeInfo.BaseOutputTechnology}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target base type for display #{path.TargetInfo.Id}");
                }

                // get display support virtual resolution
                var supportVirtResInfo = new DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION();
                supportVirtResInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SUPPORT_VIRTUAL_RESOLUTION;
                supportVirtResInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SUPPORT_VIRTUAL_RESOLUTION>();
                supportVirtResInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                supportVirtResInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref supportVirtResInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Base Output Technology: {supportVirtResInfo.IsMonitorVirtualResolutionDisabled} for target {path.TargetInfo.Id}.");
                    stringToReturn += $"****** Interrogating Target Supporting virtual resolution for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" Virtual Resolution is Disabled: {supportVirtResInfo.IsMonitorVirtualResolutionDisabled}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out the virtual resolution support for display #{path.TargetInfo.Id}");
                }

                //get advanced color info
                var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                colorInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                colorInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                colorInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                colorInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref colorInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Advanced Color Supported: {colorInfo.AdvancedColorSupported} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Advanced Color Enabled: {colorInfo.AdvancedColorEnabled} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Advanced Color Force Disabled: {colorInfo.AdvancedColorForceDisabled} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Bits per Color Channel: {colorInfo.BitsPerColorChannel} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Color Encoding: {colorInfo.ColorEncoding} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found Wide Color Enforced: {colorInfo.WideColorEnforced} for target {path.TargetInfo.Id}.");

                    stringToReturn += $"****** Interrogating Advanced Color Info for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" Advanced Color Supported: {colorInfo.AdvancedColorSupported}\n";
                    stringToReturn += $" Advanced Color Enabled: {colorInfo.AdvancedColorEnabled}\n";
                    stringToReturn += $" Advanced Color Force Disabled: {colorInfo.AdvancedColorForceDisabled}\n";
                    stringToReturn += $" Bits per Color Channel: {colorInfo.BitsPerColorChannel}\n";
                    stringToReturn += $" Color Encoding: {colorInfo.ColorEncoding}\n";
                    stringToReturn += $" Wide Color Enforced: {colorInfo.WideColorEnforced}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out the virtual resolution support for display #{path.TargetInfo.Id}");
                }

                // get SDR white levels
                var whiteLevelInfo = new DISPLAYCONFIG_SDR_WHITE_LEVEL();
                whiteLevelInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL;
                whiteLevelInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SDR_WHITE_LEVEL>();
                whiteLevelInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                whiteLevelInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref whiteLevelInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetWindowsDisplayConfig: Found SDR White Level: {whiteLevelInfo.SDRWhiteLevel} for target {path.TargetInfo.Id}.");

                    stringToReturn += $"****** Interrogating SDR Whilte Level for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" SDR White Level: {whiteLevelInfo.SDRWhiteLevel}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out the SDL white level for display #{path.TargetInfo.Id}");
                }
            }
            return stringToReturn;
        }

        public bool SetActiveConfig(NVIDIA_DISPLAY_CONFIG displayConfig)
        {

            if (_initialised)
            {

                NVAPI_STATUS NVStatus = 0;
                // We want to get the current config
                //NVIDIA_DISPLAY_CONFIG currentDisplayConfig = GetNVIDIADisplayConfig(QDC.QDC_ALL_PATHS);

                // We want to check the NVIDIA Eyefinity (SLS) config is valid
                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Testing whether the display configuration is valid");
                //ADL2_Display_SLSMapConfig_Valid(ADL_CONTEXT_HANDLE context, int iAdapterIndex, ADLSLSMap slsMap, int iNumDisplayTarget, ADLSLSTarget * lpSLSTarget, int * lpSupportedSLSLayoutImageMode, int * lpReasonForNotSupportSLS, int iOption)
                foreach (var adapter in displayConfig.AdapterConfigs)
                {
                    // set the display locations
                    if (adapter.IsSLSEnabled)
                    {
                        // Turn the SLS based display map on
                        //NVStatus = NVImport.ADL2_Display_SLSMapConfig_SetState(_adlContextHandle, adapter.AdapterIndex, adapter.SLSMapIndex, NVImport.ADL_TRUE);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_SetState successfully set the SLSMAP with index {adapter.SLSMapIndex} to TRUE for adapter {adapter.AdapterIndex}.");
                        }
                        else
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_SetState returned NVAPI_STATUS {NVStatus} when trying to set the SLSMAP with index {adapter.SLSMapIndex} to TRUE for adapter {adapter.AdapterIndex}.");
                            throw new NVIDIALibraryException($"ADL2_Display_SLSMapConfig_SetState returned NVAPI_STATUS {NVStatus} when trying to set the SLSMAP with index {adapter.SLSMapIndex} to TRUE for adapter {adapter.AdapterIndex}");
                        }
                        /*

                                                foreach (var slsMap in adapter.SLSMap)
                                                {
                                                    // Check the SLS config is valid
                                                    int numDisplayTargets = 0;
                                                    int supportedSLSLayoutImageMode = 0;
                                                    int reasonForNotSupportingSLS = 0;
                                                    ADL_DISPLAY_TARGET[] displayTargetArray = { new ADL_DISPLAY_TARGET() };
                                                    IntPtr displayTargetBuffer = IntPtr.Zero;
                                                    int option = NVImport.ADL_DISPLAY_SLSGRID_CAP_OPTION_RELATIVETO_LANDSCAPE;
                                                    NVStatus = NVImport.ADL2_Display_SLSMapConfig_Valid(_adlContextHandle, adapter.AdapterIndex, slsMap, slsMap.NumSLSTarget, displayTargetArray, out supportedSLSLayoutImageMode, out reasonForNotSupportingSLS, option);
                                                    if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                                                    {
                                                        SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: ADL2_Display_SLSMapConfig_Valid confirmed the SLS configuration is valid for NVIDIA adapter {adapter.AdapterIndex}.");
                                                    }
                                                    else
                                                    {
                                                        SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: ERROR - ADL2_Display_SLSMapConfig_Valid returned NVAPI_STATUS {NVStatus} when trying to validate the SLS configuration for NVIDIA adapter {adapter.AdapterIndex} in the computer.");
                                                        throw new NVIDIALibraryException($"ADL2_Display_SLSMapConfig_Valid returned NVAPI_STATUS {NVStatus}when trying to validate the SLS configuration for NVIDIA adapter {adapter.AdapterIndex} in the computer");
                                                    }

                                                    if (numDisplayTargets > 0)
                                                    {
                                                        IntPtr currentDisplayTargetBuffer = displayTargetBuffer;
                                                        displayTargetArray = new ADL_DISPLAY_TARGET[numDisplayTargets];
                                                        for (int i = 0; i < numDisplayTargets; i++)
                                                        {
                                                            // build a structure in the array slot
                                                            displayTargetArray[i] = new ADL_DISPLAY_TARGET();
                                                            // fill the array slot structure with the data from the buffer
                                                            displayTargetArray[i] = (ADL_DISPLAY_TARGET)Marshal.PtrToStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                                                            // destroy the bit of memory we no longer need
                                                            Marshal.DestroyStructure(currentDisplayTargetBuffer, typeof(ADL_DISPLAY_TARGET));
                                                            // advance the buffer forwards to the next object
                                                            currentDisplayTargetBuffer = (IntPtr)((long)currentDisplayTargetBuffer + Marshal.SizeOf(displayTargetArray[i]));
                                                        }
                                                        // Free the memory used by the buffer                        
                                                        Marshal.FreeCoTaskMem(displayTargetBuffer);
                                                    }
                                                }*/

                    }
                    /*else
                    {
                        *//*// Do the non-SLS based display setup
                        NVIDIA_ADAPTER_CONFIG NVIDIAAdapterConfig = adapter;
                        //int numPossibleMapResult = 0;
                        //IntPtr possibleMapResultBuffer = IntPtr.Zero;
                        NVStatus = NVImport.ADL2_Display_DisplayMapConfig_Set(_adlContextHandle, NVIDIAAdapterConfig.AdapterIndex, NVIDIAAdapterConfig.DisplayMaps.Length, NVIDIAAdapterConfig.DisplayMaps, NVIDIAAdapterConfig.DisplayTargets.Length, NVIDIAAdapterConfig.DisplayTargets);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/GetNVIDIADisplayConfig: ADL2_Display_DisplayMapConfig_Set returned information about all displaytargets connected to NVIDIA adapter {NVIDIAAdapterConfig.AdapterIndex}.");
                        }
                        else
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/GetNVIDIADisplayConfig: ERROR - ADL2_Display_DisplayMapConfig_Get returned NVAPI_STATUS {NVStatus} when trying to get the display target info from NVIDIA adapter {NVIDIAAdapterConfig.AdapterIndex} in the computer.");
                            throw new NVIDIALibraryException($"ADL2_Display_DisplayMapConfig_Get returned NVAPI_STATUS {NVStatus} when trying to get the display target info from NVIDIA adapter {NVIDIAAdapterConfig.AdapterIndex} in the computer");
                        }*//*
                    }*/
                    else
                    {
                        // Turn the SLS based display map off
                        //NVStatus = NVImport.ADL2_Display_SLSMapConfig_SetState(_adlContextHandle, adapter.AdapterIndex, adapter.SLSMapIndex, NVImport.ADL_FALSE);
                        if (NVStatus == NVAPI_STATUS.NVAPI_OK)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: ADL2_Display_SLSMapConfig_SetState successfully set the SLSMAP with index {adapter.SLSMapIndex} to FALSE for adapter {adapter.AdapterIndex}.");
                        }
                        else
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: ERROR - ADL2_Display_SLSMapConfig_SetState returned NVAPI_STATUS {NVStatus} when trying to set the SLSMAP with index {adapter.SLSMapIndex} to FALSE for adapter {adapter.AdapterIndex}.");
                            throw new NVIDIALibraryException($"ADL2_Display_SLSMapConfig_SetState returned NVAPI_STATUS {NVStatus} when trying to set the SLSMAP with index {adapter.SLSMapIndex} to FALSE for adapter {adapter.AdapterIndex}");
                        }
                    }

                }

                // We want to set the NVIDIA HDR settings

                // We want to apply the Windows CCD layout info and HDR
            }
            else
            {
                SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: ERROR - Tried to run SetActiveConfig but the NVIDIA ADL library isn't initialised!");
                throw new NVIDIALibraryException($"Tried to run SetActiveConfig but the NVIDIA ADL library isn't initialised!");
            }


            /*
                        // Get the all possible windows display configs
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Generating a list of all the current display configs");
                        WINDOWS_DISPLAY_CONFIG allWindowsDisplayConfig = GetWindowsDisplayConfig(QDC.QDC_ALL_PATHS);

                        // Now we go through the Paths to update the LUIDs as per Soroush's suggestion
                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Patching the adapter IDs to make the saved config valid");
                        PatchAdapterIDs(ref displayConfig, allWindowsDisplayConfig.displayAdapters);

                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Testing whether the display configuration is valid");
                        // Test whether a specified display configuration is supported on the computer                    
                        uint myPathsCount = (uint)displayConfig.displayConfigPaths.Length;
                        uint myModesCount = (uint)displayConfig.displayConfigModes.Length;
                        WIN32STATUS err = CCDImport.SetDisplayConfig(myPathsCount, displayConfig.displayConfigPaths, myModesCount, displayConfig.displayConfigModes, SDC.DISPLAYMAGICIAN_VALIDATE);
                        if (err == WIN32STATUS.ERROR_SUCCESS)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Successfully validated that the display configuration supplied would work!");
                        }
                        else
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: ERROR - SetDisplayConfig couldn't validate the display configuration supplied. This display configuration wouldn't work.");
                            return false;
                        }

                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Yay! The display configuration is valid! Attempting to set the Display Config now");
                        // Now set the specified display configuration for this computer                    
                        err = CCDImport.SetDisplayConfig(myPathsCount, displayConfig.displayConfigPaths, myModesCount, displayConfig.displayConfigModes, SDC.DISPLAYMAGICIAN_SET);
                        if (err == WIN32STATUS.ERROR_SUCCESS)
                        {
                            SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Successfully set the display configuration to the settings supplied!");
                        }
                        else
                        {
                            SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: ERROR - SetDisplayConfig couldn't set the display configuration using the settings supplied. Something is wrong.");
                            throw new NVIDIALibraryException($"SetDisplayConfig couldn't set the display configuration using the settings supplied. Something is wrong.");
                        }

                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: SUCCESS! The display configuration has been successfully applied");

                        foreach (ADVANCED_HDR_INFO_PER_PATH myHDRstate in displayConfig.displayHDRStates)
                        {
                            SharedLogger.logger.Trace($"Trying to get information whether HDR color is in use now on Display {myHDRstate.Id}.");
                            // Get advanced HDR info
                            var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                            colorInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                            colorInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                            colorInfo.Header.AdapterId = myHDRstate.AdapterId;
                            colorInfo.Header.Id = myHDRstate.Id;
                            err = CCDImport.DisplayConfigGetDeviceInfo(ref colorInfo);
                            if (err == WIN32STATUS.ERROR_SUCCESS)
                            {
                                SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Advanced Color Info gathered from Display {myHDRstate.Id}");

                                if (myHDRstate.AdvancedColorInfo.AdvancedColorSupported && colorInfo.AdvancedColorEnabled != myHDRstate.AdvancedColorInfo.AdvancedColorEnabled)
                                {
                                    SharedLogger.logger.Trace($"HDR is available for use on Display {myHDRstate.Id}, and we want it set to {myHDRstate.AdvancedColorInfo.AdvancedColorEnabled} but is currently {colorInfo.AdvancedColorEnabled}.");

                                    var setColorState = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE();
                                    setColorState.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
                                    setColorState.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>();
                                    setColorState.Header.AdapterId = myHDRstate.AdapterId;
                                    setColorState.Header.Id = myHDRstate.Id;
                                    setColorState.EnableAdvancedColor = myHDRstate.AdvancedColorInfo.AdvancedColorEnabled;
                                    err = CCDImport.DisplayConfigSetDeviceInfo(ref setColorState);
                                    if (err == WIN32STATUS.ERROR_SUCCESS)
                                    {
                                        SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: SUCCESS! Set HDR successfully to {myHDRstate.AdvancedColorInfo.AdvancedColorEnabled} on Display {myHDRstate.Id}");
                                    }
                                    else
                                    {
                                        SharedLogger.logger.Error($"NVIDIALibrary/SetActiveConfig: ERROR - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to set the HDR settings for display #{myHDRstate.Id}");
                                        return false;
                                    }
                                }
                                else
                                {
                                    SharedLogger.logger.Trace($"NVIDIALibrary/SetActiveConfig: Skipping setting HDR on Display {myHDRstate.Id} as it does not support HDR");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Warn($"NVIDIALibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out if HDR is supported for display #{myHDRstate.Id}");
                            }

                        }*/
            return true;
        }

        public bool IsActiveConfig(NVIDIA_DISPLAY_CONFIG displayConfig)
        {
            // Get the current windows display configs to compare to the one we loaded
            bool allDisplays = false;
            NVIDIA_DISPLAY_CONFIG currentWindowsDisplayConfig = GetNVIDIADisplayConfig(allDisplays);

            // Check whether the display config is in use now
            SharedLogger.logger.Trace($"NVIDIALibrary/IsActiveConfig: Checking whether the display configuration is already being used.");
            if (displayConfig.Equals(currentWindowsDisplayConfig))
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsActiveConfig: The display configuration is already being used (supplied displayConfig Equals currentWindowsDisplayConfig");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsActiveConfig: The display configuration is NOT currently in use (supplied displayConfig Equals currentWindowsDisplayConfig");
                return false;
            }

        }

        public bool IsPossibleConfig(NVIDIA_DISPLAY_CONFIG displayConfig)
        {
            /*// Get the all possible windows display configs
            NVIDIA_DISPLAY_CONFIG allWindowsDisplayConfig = GetNVIDIADisplayConfig(QDC.QDC_ALL_PATHS);

            SharedLogger.logger.Trace("NVIDIALibrary/PatchAdapterIDs: Going through the list of adapters we stored in the config to make sure they still exist");
            // Firstly check that the Adapter Names are still currently available (i.e. the adapter hasn't been replaced).
            foreach (string savedAdapterName in displayConfig.displayAdapters.Values)
            {
                // If there is even one of the saved adapters that has changed, then it's no longer possible
                // to use this display config!
                if (!allWindowsDisplayConfig.displayAdapters.Values.Contains(savedAdapterName))
                {
                    SharedLogger.logger.Error($"NVIDIALibrary/PatchAdapterIDs: ERROR - Saved adapter {savedAdapterName} is not available right now! This display configuration won't work!");
                    return false;
                }
            }
            SharedLogger.logger.Trace($"NVIDIALibrary/PatchAdapterIDs: All teh adapters that the display configuration uses are still avilable to use now!");

            // Now we go through the Paths to update the LUIDs as per Soroush's suggestion
            SharedLogger.logger.Trace($"NVIDIALibrary/IsPossibleConfig: Attemptong to patch the saved display configuration's adapter IDs so that it will still work (these change at each boot)");
            PatchAdapterIDs(ref displayConfig, allWindowsDisplayConfig.displayAdapters);

            SharedLogger.logger.Trace($"NVIDIALibrary/IsPossibleConfig: Testing whether the display configuration is valid ");
            // Test whether a specified display configuration is supported on the computer                    
            uint myPathsCount = (uint)displayConfig.displayConfigPaths.Length;
            uint myModesCount = (uint)displayConfig.displayConfigModes.Length;
            WIN32STATUS err = CCDImport.SetDisplayConfig(myPathsCount, displayConfig.displayConfigPaths, myModesCount, displayConfig.displayConfigModes, SDC.DISPLAYMAGICIAN_VALIDATE);
            if (err == WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsPossibleConfig: SetDisplayConfig validated that the display configuration is valid and can be used!");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"NVIDIALibrary/IsPossibleConfig: SetDisplayConfig confirmed that the display configuration is invalid and cannot be used!");
                return false;
            }*/
            return true;
        }

        public List<string> GetCurrentDisplayIdentifiers()
        {
            SharedLogger.logger.Error($"NVIDIALibrary/GetCurrentDisplayIdentifiers: Getting the current display identifiers for the displays in use now");
            return GetSomeDisplayIdentifiers(QDC.QDC_ONLY_ACTIVE_PATHS);
        }

        public List<string> GetAllConnectedDisplayIdentifiers()
        {
            SharedLogger.logger.Error($"NVIDIALibrary/GetAllConnectedDisplayIdentifiers: Getting all the display identifiers that can possibly be used");
            return GetSomeDisplayIdentifiers(QDC.QDC_ALL_PATHS);
        }

        private List<string> GetSomeDisplayIdentifiers(QDC selector = QDC.QDC_ONLY_ACTIVE_PATHS)
        {
            SharedLogger.logger.Debug($"NVIDIALibrary/GetCurrentDisplayIdentifiers: Generating the unique Display Identifiers for the currently active configuration");

            List<string> displayIdentifiers = new List<string>();

            SharedLogger.logger.Trace($"NVIDIALibrary/GetCurrentDisplayIdentifiers: Testing whether the display configuration is valid (allowing tweaks).");
            // Get the size of the largest Active Paths and Modes arrays
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"NVIDIALibrary/PrintActiveConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new NVIDIALibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"NVIDIALibrary/GetSomeDisplayIdentifiers: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"NVIDIALibrary/GetSomeDisplayIdentifiers: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new NVIDIALibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Error($"NVIDIALibrary/GetSomeDisplayIdentifiers: ERROR - The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                    throw new NVIDIALibraryException($"The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                }
                else if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"NVIDIALibrary/GetSomeDisplayIdentifiers: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again");
                    throw new NVIDIALibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again.");
                }
            }
            else if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"NVIDIALibrary/GetSomeDisplayIdentifiers: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays");
                throw new NVIDIALibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays.");
            }

            foreach (var path in paths)
            {
                if (path.TargetInfo.TargetAvailable == false)
                {
                    // We want to skip this one cause it's not valid
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Skipping path due to TargetAvailable not existing in display #{path.TargetInfo.Id}");
                    continue;
                }

                // get display source name
                var sourceInfo = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
                sourceInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
                sourceInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DEVICE_NAME>();
                sourceInfo.Header.AdapterId = path.SourceInfo.AdapterId;
                sourceInfo.Header.Id = path.SourceInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref sourceInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the source info from {path.SourceInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetSomeDisplayIdentifiers: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.SourceInfo.Id}");
                }

                // get display target name
                var targetInfo = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
                targetInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                targetInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
                targetInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                targetInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref targetInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the target info from {path.TargetInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetSomeDisplayIdentifiers: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.TargetInfo.Id}");
                }

                // get display adapter name
                var adapterInfo = new DISPLAYCONFIG_ADAPTER_NAME();
                adapterInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME;
                adapterInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_ADAPTER_NAME>();
                adapterInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                adapterInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref adapterInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"NVIDIALibrary/GetSomeDisplayIdentifiers: Successfully got the display name info from {path.TargetInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"NVIDIALibrary/GetSomeDisplayIdentifiers: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.TargetInfo.Id}");
                }

                // Create an array of all the important display info we need to record
                List<string> displayInfo = new List<string>();
                displayInfo.Add("WINAPI");
                try
                {
                    displayInfo.Add(adapterInfo.AdapterDevicePath.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display Adapter Device Path from video card. Substituting with a # instead");
                    displayInfo.Add("#");
                }
                try
                {
                    displayInfo.Add(targetInfo.OutputTechnology.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display Connector Instance from video card. Substituting with a # instead");
                    displayInfo.Add("#");
                }
                try
                {
                    displayInfo.Add(targetInfo.EdidManufactureId.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display EDID Manufacturer Code from video card. Substituting with a # instead");
                    displayInfo.Add("#");
                }
                try
                {
                    displayInfo.Add(targetInfo.EdidProductCodeId.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display EDID Product Code from video card. Substituting with a # instead");
                    displayInfo.Add("#");
                }
                try
                {
                    displayInfo.Add(targetInfo.MonitorFriendlyDeviceName.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"NVIDIALibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display Target Friendly name from video card. Substituting with a # instead");
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

            // Sort the display identifiers
            displayIdentifiers.Sort();

            return displayIdentifiers;
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