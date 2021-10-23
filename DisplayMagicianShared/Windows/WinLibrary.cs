using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using DisplayMagicianShared;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace DisplayMagicianShared.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ADVANCED_HDR_INFO_PER_PATH : IEquatable<ADVANCED_HDR_INFO_PER_PATH>
    {
        public LUID AdapterId;
        public uint Id;
        public DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO AdvancedColorInfo;
        public DISPLAYCONFIG_SDR_WHITE_LEVEL SDRWhiteLevel;

        public override bool Equals(object obj) => obj is ADVANCED_HDR_INFO_PER_PATH other && this.Equals(other);
        public bool Equals(ADVANCED_HDR_INFO_PER_PATH other)
        => // AdapterId.Equals(other.AdapterId) && // Removed the AdapterId from the Equals, as it changes after reboot.
           //Id == other.Id && // Removed the ID too, as that changes if the user has a Clone!
           AdvancedColorInfo.Equals(other.AdvancedColorInfo) &&
           SDRWhiteLevel.Equals(other.SDRWhiteLevel);
        public override int GetHashCode()
        {
            return (Id, AdvancedColorInfo, SDRWhiteLevel).GetHashCode();
        }

        public static bool operator ==(ADVANCED_HDR_INFO_PER_PATH lhs, ADVANCED_HDR_INFO_PER_PATH rhs) => lhs.Equals(rhs);

        public static bool operator !=(ADVANCED_HDR_INFO_PER_PATH lhs, ADVANCED_HDR_INFO_PER_PATH rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWS_DISPLAY_CONFIG : IEquatable<WINDOWS_DISPLAY_CONFIG>
    {
        public Dictionary<ulong, string> DisplayAdapters;
        public DISPLAYCONFIG_PATH_INFO[] DisplayConfigPaths;
        public DISPLAYCONFIG_MODE_INFO[] DisplayConfigModes;
        public ADVANCED_HDR_INFO_PER_PATH[] DisplayHDRStates;
        public Dictionary<string, GDI_DISPLAY_SETTING> GdiDisplaySettings;
        public bool IsCloned;
        // Note: We purposely have left out the DisplaySources from the Equals as it's order keeps changing after each reboot and after each profile swap
        // and it is informational only and doesn't contribute to the configuration (it's used for generating the Screens structure, and therefore for
        // generating the profile icon.
        public Dictionary<string, List<uint>> DisplaySources;
        public List<string> DisplayIdentifiers;

        public override bool Equals(object obj) => obj is WINDOWS_DISPLAY_CONFIG other && this.Equals(other);
        public bool Equals(WINDOWS_DISPLAY_CONFIG other)
        => IsCloned == other.IsCloned &&
           DisplayConfigPaths.SequenceEqual(other.DisplayConfigPaths) &&
           DisplayConfigModes.SequenceEqual(other.DisplayConfigModes) &&
           DisplayHDRStates.SequenceEqual(other.DisplayHDRStates) &&
           GdiDisplaySettings.SequenceEqual(other.GdiDisplaySettings) &&
           DisplayIdentifiers.SequenceEqual(other.DisplayIdentifiers);

        public override int GetHashCode()
        {
            return (DisplayConfigPaths, DisplayConfigModes, DisplayHDRStates, GdiDisplaySettings, IsCloned, DisplayIdentifiers).GetHashCode();
        }
        public static bool operator ==(WINDOWS_DISPLAY_CONFIG lhs, WINDOWS_DISPLAY_CONFIG rhs) => lhs.Equals(rhs);

        public static bool operator !=(WINDOWS_DISPLAY_CONFIG lhs, WINDOWS_DISPLAY_CONFIG rhs) => !(lhs == rhs);
    }

    public class WinLibrary : IDisposable
    {

        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static WinLibrary _instance = new WinLibrary();

        private bool _initialised = false;

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);
        private IntPtr _adlContextHandle = IntPtr.Zero;

        static WinLibrary() { }
        public WinLibrary()
        {
            SharedLogger.logger.Trace("WinLibrary/WinLibrary: Intialising Windows CCD library interface");
            _initialised = true;

        }

        ~WinLibrary()
        {
            // The WinLibrary was initialised, but doesn't need to be freed.
            SharedLogger.logger.Trace("WinLibrary/~WinLibrary: Destroying Windows CCD library interface");
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

        public static WinLibrary GetLibrary()
        {
            return _instance;
        }

        public WINDOWS_DISPLAY_CONFIG CreateDefaultConfig()
        {
            WINDOWS_DISPLAY_CONFIG myDefaultConfig = new WINDOWS_DISPLAY_CONFIG();

            // Fill in the minimal amount we need to avoid null references
            // so that we won't break json.net when we save a default config

            myDefaultConfig.DisplayAdapters = new Dictionary<ulong, string>();
            myDefaultConfig.DisplayConfigModes = new DISPLAYCONFIG_MODE_INFO[0];
            myDefaultConfig.DisplayConfigPaths = new DISPLAYCONFIG_PATH_INFO[0];
            myDefaultConfig.DisplayHDRStates = new ADVANCED_HDR_INFO_PER_PATH[0];
            myDefaultConfig.DisplayIdentifiers = new List<string>();
            myDefaultConfig.DisplaySources = new Dictionary<string, List<uint>>();
            myDefaultConfig.IsCloned = false;
            myDefaultConfig.GdiDisplaySettings = new Dictionary<string, GDI_DISPLAY_SETTING>();

            return myDefaultConfig;
        }

        private void PatchAdapterIDs(ref WINDOWS_DISPLAY_CONFIG savedDisplayConfig, Dictionary<ulong, string> currentAdapterMap)
        {

            Dictionary<ulong, ulong> adapterOldToNewMap = new Dictionary<ulong, ulong>();

            SharedLogger.logger.Trace("WinLibrary/PatchAdapterIDs: Going through the list of adapters we stored in the config to figure out the old adapterIDs");
            foreach (KeyValuePair<ulong, string> savedAdapter in savedDisplayConfig.DisplayAdapters)
            {
                foreach (KeyValuePair<ulong, string> currentAdapter in currentAdapterMap)
                {
                    if (currentAdapter.Value.Equals(savedAdapter.Value))
                    {
                        // we have found the new LUID Value for the same adapter
                        // So we want to store it
                        SharedLogger.logger.Trace($"WinLibrary/PatchAdapterIDs: We found that saved adapter {savedAdapter.Key} has now been assigned adapter id {currentAdapter.Key} (AdapterName is {savedAdapter.Value})");
                        adapterOldToNewMap.Add(savedAdapter.Key, currentAdapter.Key);
                    }
                }
            }

            ulong newAdapterValue = 0;
            // Update the paths with the current adapter id
            SharedLogger.logger.Trace($"WinLibrary/PatchAdapterIDs: Going through the display config paths to update the adapter id");
            for (int i = 0; i < savedDisplayConfig.DisplayConfigPaths.Length; i++)
            {
                // Change the Path SourceInfo and TargetInfo AdapterIDs
                if (adapterOldToNewMap.ContainsKey(savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId.Value))
                {
                    // We get here if there is a matching adapter
                    newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId.Value];
                    savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId = AdapterValueToLUID(newAdapterValue);
                    newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayConfigPaths[i].TargetInfo.AdapterId.Value];
                    savedDisplayConfig.DisplayConfigPaths[i].TargetInfo.AdapterId = AdapterValueToLUID(newAdapterValue);
                }
                else
                {
                    // if there isn't a matching adapter, then we just pick the first current one and hope that works!
                    // (it is highly likely to... its only if the user has multiple graphics cards with some weird config it may break)
                    newAdapterValue = currentAdapterMap.First().Key;
                    SharedLogger.logger.Warn($"WinLibrary/PatchAdapterIDs: Uh Oh. Adapter {savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId.Value} didn't have a current match! It's possible the adapter was swapped or disabled. Attempting to use adapter {newAdapterValue} instead.");
                    savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId = AdapterValueToLUID(newAdapterValue);
                    savedDisplayConfig.DisplayConfigPaths[i].TargetInfo.AdapterId = AdapterValueToLUID(newAdapterValue);
                }
            }

            SharedLogger.logger.Trace($"WinLibrary/PatchAdapterIDs: Going through the display config modes to update the adapter id");
            // Update the modes with the current adapter id
            for (int i = 0; i < savedDisplayConfig.DisplayConfigModes.Length; i++)
            {
                // Change the Mode AdapterID
                if (adapterOldToNewMap.ContainsKey(savedDisplayConfig.DisplayConfigModes[i].AdapterId.Value))
                {
                    // We get here if there is a matching adapter
                    newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayConfigModes[i].AdapterId.Value];
                    savedDisplayConfig.DisplayConfigModes[i].AdapterId = AdapterValueToLUID(newAdapterValue);
                }
                else
                {
                    // if there isn't a matching adapter, then we just pick the first current one and hope that works!
                    // (it is highly likely to... its only if the user has multiple graphics cards with some weird config it may break)
                    newAdapterValue = currentAdapterMap.First().Key;
                    SharedLogger.logger.Warn($"WinLibrary/PatchAdapterIDs: Uh Oh. Adapter {savedDisplayConfig.DisplayConfigModes[i].AdapterId.Value} didn't have a current match! It's possible the adapter was swapped or disabled. Attempting to use adapter {newAdapterValue} instead.");
                    savedDisplayConfig.DisplayConfigModes[i].AdapterId = AdapterValueToLUID(newAdapterValue);
                }
            }

            SharedLogger.logger.Trace($"WinLibrary/PatchAdapterIDs: Going through the display config HDR info to update the adapter id");
            // Update the HDRInfo with the current adapter id
            for (int i = 0; i < savedDisplayConfig.DisplayHDRStates.Length; i++)
            {
                // Change the Mode AdapterID
                if (adapterOldToNewMap.ContainsKey(savedDisplayConfig.DisplayHDRStates[i].AdapterId.Value))
                {
                    // We get here if there is a matching adapter
                    newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayHDRStates[i].AdapterId.Value];
                    savedDisplayConfig.DisplayHDRStates[i].AdapterId = AdapterValueToLUID(newAdapterValue);
                    newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayHDRStates[i].AdvancedColorInfo.Header.AdapterId.Value];
                    savedDisplayConfig.DisplayHDRStates[i].AdvancedColorInfo.Header.AdapterId = AdapterValueToLUID(newAdapterValue);
                    newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayHDRStates[i].SDRWhiteLevel.Header.AdapterId.Value];
                    savedDisplayConfig.DisplayHDRStates[i].SDRWhiteLevel.Header.AdapterId = AdapterValueToLUID(newAdapterValue);
                }
                else
                {
                    // if there isn't a matching adapter, then we just pick the first current one and hope that works!
                    // (it is highly likely to... its only if the user has multiple graphics cards with some weird config it may break)
                    newAdapterValue = currentAdapterMap.First().Key;
                    SharedLogger.logger.Warn($"WinLibrary/PatchAdapterIDs: Uh Oh. Adapter {savedDisplayConfig.DisplayHDRStates[i].AdapterId.Value} didn't have a current match! It's possible the adapter was swapped or disabled. Attempting to use adapter {newAdapterValue} instead.");
                    savedDisplayConfig.DisplayHDRStates[i].AdapterId = AdapterValueToLUID(newAdapterValue);
                    savedDisplayConfig.DisplayHDRStates[i].AdvancedColorInfo.Header.AdapterId = AdapterValueToLUID(newAdapterValue);
                    savedDisplayConfig.DisplayHDRStates[i].SDRWhiteLevel.Header.AdapterId = AdapterValueToLUID(newAdapterValue);
                }
            }

        }

        public WINDOWS_DISPLAY_CONFIG GetActiveConfig()
        {
            SharedLogger.logger.Trace($"WinLibrary/GetActiveConfig: Getting the currently active config");
            return GetWindowsDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS);
        }

        private WINDOWS_DISPLAY_CONFIG GetWindowsDisplayConfig(QDC selector = QDC.QDC_ONLY_ACTIVE_PATHS)
        {
            // Get the size of the largest Active Paths and Modes arrays
            SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the size of the largest Active Paths and Modes arrays");
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                    throw new WinLibraryException($"The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                }
                else if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again");
                    throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again.");
                }
            }
            else if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays");
                throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays.");
            }

            // Prepare the empty windows display config
            WINDOWS_DISPLAY_CONFIG windowsDisplayConfig = new WINDOWS_DISPLAY_CONFIG();
            windowsDisplayConfig.DisplayAdapters = new Dictionary<ulong, string>();
            windowsDisplayConfig.DisplayHDRStates = new ADVANCED_HDR_INFO_PER_PATH[pathCount];
            windowsDisplayConfig.DisplaySources = new Dictionary<string, List<uint>>();
            windowsDisplayConfig.IsCloned = false;

            // First of all generate the current displayIdentifiers
            windowsDisplayConfig.DisplayIdentifiers = GetCurrentDisplayIdentifiers();

            // Next, extract the UID entries for the displays as that's what the Path IDs are normally supposed to be
            // This is how we know the actual target id's ofd the monitors currently connected
            Regex rx = new Regex(@"UID(?<uid>\d+)#", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            HashSet<uint> physicalTargetIdsToFind = new HashSet<uint>();
            foreach (string displayIdentifier in windowsDisplayConfig.DisplayIdentifiers)
            {
                MatchCollection mc = rx.Matches(displayIdentifier);
                if (mc.Count > 0)
                {
                    physicalTargetIdsToFind.Add(UInt32.Parse(mc[0].Groups["uid"].Value));
                }
            }

            // Now cycle through the paths and grab the HDR state information
            // and map the adapter name to adapter id
            HashSet<uint> targetIdsToChange = new HashSet<uint>();
            var hdrInfos = new ADVANCED_HDR_INFO_PER_PATH[pathCount];
            int hdrInfoCount = 0;
            for (int i = 0; i < paths.Length; i++)
            {
                // Figure out if this path has a physical targetId, and if it doesn't store it
                if (!physicalTargetIdsToFind.Contains(paths[i].TargetInfo.Id))
                {
                    // Add to the list of physical path target ids we need to patch later
                    targetIdsToChange.Add(paths[i].TargetInfo.Id);
                }

                // Track if this display is a cloned path
                bool isClonedPath = false;
                // get display source name
                var sourceInfo = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
                sourceInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
                sourceInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DEVICE_NAME>();
                sourceInfo.Header.AdapterId = paths[i].SourceInfo.AdapterId;
                sourceInfo.Header.Id = paths[i].SourceInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref sourceInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    // Store it for later
                    if (windowsDisplayConfig.DisplaySources.ContainsKey(sourceInfo.ViewGdiDeviceName))
                    {
                        // We already have at least one display using this source, so we need to add the other cloned display to the existing list
                        windowsDisplayConfig.DisplaySources[sourceInfo.ViewGdiDeviceName].Add(paths[i].SourceInfo.Id);
                        isClonedPath = true;
                        windowsDisplayConfig.IsCloned = true;
                    }
                    else
                    {
                        // This is the first display to use this source
                        List<uint> sourceIds = new List<uint>();
                        sourceIds.Add(paths[i].SourceInfo.Id);
                        windowsDisplayConfig.DisplaySources.Add(sourceInfo.ViewGdiDeviceName, sourceIds);
                    }

                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Display Source {sourceInfo.ViewGdiDeviceName} for source {paths[i].SourceInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the source info for source adapter #{paths[i].SourceInfo.AdapterId}");
                }

                // Check if this path is a cloned display path, and if so make some changes
                // so that the cloned display will be applied properly
                if (isClonedPath)
                {
                    // We need to make some modifications to this path so that we store as ready for being applied
                    // https://docs.microsoft.com/en-us/windows-hardware/drivers/display/ccd-example-code
                    paths[i].Flags |= DISPLAYCONFIG_PATH_FLAGS.DISPLAYCONFIG_PATH_ACTIVE;
                    paths[i].SourceInfo.ModeInfoIdx = CCDImport.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
                    paths[i].TargetInfo.ModeInfoIdx = CCDImport.DISPLAYCONFIG_PATH_MODE_IDX_INVALID;
                }

                // Get adapter ID for later
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Attempting to get adapter name for adapter {paths[i].TargetInfo.AdapterId.Value}.");
                if (!windowsDisplayConfig.DisplayAdapters.ContainsKey(paths[i].TargetInfo.AdapterId.Value))
                {
                    var adapterInfo = new DISPLAYCONFIG_ADAPTER_NAME();
                    adapterInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADAPTER_NAME;
                    adapterInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_ADAPTER_NAME>();
                    adapterInfo.Header.AdapterId = paths[i].TargetInfo.AdapterId;
                    adapterInfo.Header.Id = paths[i].TargetInfo.Id;
                    err = CCDImport.DisplayConfigGetDeviceInfo(ref adapterInfo);
                    if (err == WIN32STATUS.ERROR_SUCCESS)
                    {
                        // Store it for later
                        windowsDisplayConfig.DisplayAdapters.Add(paths[i].TargetInfo.AdapterId.Value, adapterInfo.AdapterDevicePath);
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found adapter name {adapterInfo.AdapterDevicePath} for adapter {paths[i].TargetInfo.AdapterId.Value}.");
                    }
                    else
                    {
                        SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to query the adapter name for adapter {paths[i].TargetInfo.AdapterId.Value}.");
                    }
                }

                // Get advanced color info
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Attempting to get advanced color info for display {paths[i].TargetInfo.Id}.");
                var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                colorInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                colorInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                colorInfo.Header.AdapterId = paths[i].TargetInfo.AdapterId;
                colorInfo.Header.Id = paths[i].TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref colorInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found color info for display {paths[i].TargetInfo.Id}.");
                    if (colorInfo.AdvancedColorSupported)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: HDR is supported for display {paths[i].TargetInfo.Id}.");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: HDR is NOT supported for display {paths[i].TargetInfo.Id}.");
                    }
                    if (colorInfo.AdvancedColorEnabled)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: HDR is enabled for display {paths[i].TargetInfo.Id}.");
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: HDR is NOT enabled for display {paths[i].TargetInfo.Id}.");
                    }
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - Unabled to get advanced color settings for display {paths[i].TargetInfo.Id}.");
                }

                // get SDR white levels
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Attempting to get SDR white levels for adapter {paths[i].TargetInfo.AdapterId.Value}.");
                var whiteLevelInfo = new DISPLAYCONFIG_SDR_WHITE_LEVEL();
                whiteLevelInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL;
                whiteLevelInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SDR_WHITE_LEVEL>();
                whiteLevelInfo.Header.AdapterId = paths[i].TargetInfo.AdapterId;
                whiteLevelInfo.Header.Id = paths[i].TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref whiteLevelInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found SDR White levels for display {paths[i].TargetInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - Unabled to get SDR White levels for display {paths[i].TargetInfo.Id}.");
                }

                hdrInfos[hdrInfoCount] = new ADVANCED_HDR_INFO_PER_PATH();
                hdrInfos[hdrInfoCount].AdapterId = paths[i].TargetInfo.AdapterId;
                hdrInfos[hdrInfoCount].Id = paths[i].TargetInfo.Id;
                hdrInfos[hdrInfoCount].AdvancedColorInfo = colorInfo;
                hdrInfos[hdrInfoCount].SDRWhiteLevel = whiteLevelInfo;
                hdrInfoCount++;
            }

            // Now we need to figure out the maps between the cloned ID and the real physical target id
            // the Advanced color info structure actually holds this information! So lets mine it.
            Dictionary<uint, uint> targetIdMap = new Dictionary<uint, uint>();
            foreach (var hdrInfo in hdrInfos)
            {
                targetIdMap[hdrInfo.Id] = hdrInfo.AdvancedColorInfo.Header.Id;
            }

            // Now we need to go through the list of paths again and patch the 'cloned' displays with a real display ID so the config works
            for (int i = 0; i < paths.Length; i++)
            {
                if (targetIdsToChange.Contains(paths[i].TargetInfo.Id))
                {
                    // Patch the cloned ids with a real working one!
                    paths[i].TargetInfo.Id = targetIdMap[paths[i].TargetInfo.Id];
                }
            }


            // Store the active paths and modes in our display config object
            windowsDisplayConfig.DisplayConfigPaths = paths;
            windowsDisplayConfig.DisplayConfigModes = modes;
            windowsDisplayConfig.DisplayHDRStates = hdrInfos;
            windowsDisplayConfig.GdiDisplaySettings = GetGdiDisplaySettings();


            return windowsDisplayConfig;
        }

        public Dictionary<string, GDI_DISPLAY_SETTING> GetGdiDisplaySettings()
        {
            // Get the list of all display adapters in this machine through GDI
            Dictionary<string, GDI_DISPLAY_SETTING> gdiDeviceSettings = new Dictionary<string, GDI_DISPLAY_SETTING>();
            Dictionary<string, string> gdiDeviceSources = new Dictionary<string, string>();
            UInt32 displayDeviceNum = 0;
            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
            displayDevice.Size = (UInt32)Marshal.SizeOf<DISPLAY_DEVICE>();
            while (GDIImport.EnumDisplayDevices(null, displayDeviceNum, ref displayDevice, 0))
            {
                // Now we try and grab the GDI Device Settings for each display device
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Settings for {displayDevice.DeviceName}");
                if (displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.AttachedToDesktop) || displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.MultiDriver))
                {
                    // If the display device is attached to the Desktop, or a type of display that is represented by a psudeo mirroring application, then skip this display
                    // e.g. some sort of software interfaced display that doesn't have a physical plug, or maybe uses USB for communication
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Settings for {displayDevice.DeviceName}");
                    DEVICE_MODE currentMode = new DEVICE_MODE();
                    currentMode.Size = (UInt16)Marshal.SizeOf<DEVICE_MODE>();
                    bool gdiWorked = GDIImport.EnumDisplaySettings(displayDevice.DeviceName, DISPLAY_SETTINGS_MODE.CurrentSettings, ref currentMode);
                    if (gdiWorked)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Got the current Display Settings from display {displayDevice.DeviceName}.");
                        GDI_DISPLAY_SETTING myDisplaySetting = new GDI_DISPLAY_SETTING();
                        myDisplaySetting.IsEnabled = true; // Always true if we get here
                        myDisplaySetting.Device = displayDevice;
                        myDisplaySetting.DeviceMode = currentMode;
                        if (displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.PrimaryDevice))
                        {
                            // This is a primary device, so we'll set that too.
                            myDisplaySetting.IsPrimary = true;
                        }
                        gdiDeviceSettings[displayDevice.DeviceKey] = myDisplaySetting;
                        gdiDeviceSources[displayDevice.DeviceName] = displayDevice.DeviceKey;
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - Unabled to get current display mode settings from display {displayDevice.DeviceName}.");
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: The display {displayDevice.DeviceName} is either not attached to the desktop, or is not a mirroring driver. Skipping this display device as we cannot use it.");
                }

                displayDeviceNum++;
            }
            return gdiDeviceSettings;
        }

        public static Dictionary<string, List<uint>> GetDisplaySourceNames()
        {
            // Get the size of the largest Active Paths and Modes arrays
            SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the size of the largest Active Paths and Modes arrays");
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                    throw new WinLibraryException($"The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                }
                else if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again");
                    throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again.");
                }
            }
            else if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays");
                throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays.");
            }

            // Prepare the empty DisplaySources dictionary
            Dictionary<string, List<uint>> DisplaySources = new Dictionary<string, List<uint>>();

            // Now cycle through the paths and grab the HDR state information
            // and map the adapter name to adapter id
            var hdrInfos = new ADVANCED_HDR_INFO_PER_PATH[pathCount];
            int hdrInfoCount = 0;
            foreach (var path in paths)
            {
                // get display source name
                var sourceInfo = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
                sourceInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
                sourceInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DEVICE_NAME>();
                sourceInfo.Header.AdapterId = path.SourceInfo.AdapterId;
                sourceInfo.Header.Id = path.SourceInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref sourceInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    // Store it for later
                    //DisplaySources.Add(sourceInfo.ViewGdiDeviceName, path.SourceInfo.Id);
                    if (DisplaySources.ContainsKey(sourceInfo.ViewGdiDeviceName))
                    {
                        // We want to add another cloned display
                        DisplaySources[sourceInfo.ViewGdiDeviceName].Add(path.SourceInfo.Id);
                    }
                    else
                    {
                        // We want to create a new list entry if there isn't one already there.
                        DisplaySources.Add(sourceInfo.ViewGdiDeviceName, new List<uint> { path.SourceInfo.Id });
                    }

                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Display Source {sourceInfo.ViewGdiDeviceName} for source {path.SourceInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the source info for source adapter #{path.SourceInfo.AdapterId}");
                }

            }

            return DisplaySources;
        }


        private LUID AdapterValueToLUID(ulong adapterValue)
        {
            LUID luid = new LUID();
            luid.LowPart = (uint)(adapterValue & uint.MaxValue);
            luid.HighPart = (uint)(adapterValue >> 32);
            return luid;
        }

        public string PrintActiveConfig()
        {
            string stringToReturn = "";

            // Get the current config
            WINDOWS_DISPLAY_CONFIG displayConfig = GetActiveConfig();

            WIN32STATUS err = WIN32STATUS.ERROR_GEN_FAILURE;
            stringToReturn += $"****** WINDOWS CCD CONFIGURATION *******\n";
            stringToReturn += $"Display profile contains cloned screens: {displayConfig.IsCloned}\n";
            stringToReturn += $"\n";

            // Get the size of the largest Active Paths and Modes arrays
            int pathCount = 0;
            int modeCount = 0;

            foreach (var path in displayConfig.DisplayConfigPaths)
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
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Display Source {sourceInfo.ViewGdiDeviceName} for source {path.SourceInfo.Id}.");
                    stringToReturn += $"****** Interrogating Display Source {path.SourceInfo.Id} *******\n";
                    stringToReturn += $"Found Display Source {sourceInfo.ViewGdiDeviceName}\n";
                    if (displayConfig.DisplaySources[sourceInfo.ViewGdiDeviceName].Count > 1)
                    {
                        stringToReturn += $"Display Source is Cloned: true\n";
                        stringToReturn += $"Number of Display Source clones: {displayConfig.DisplaySources[sourceInfo.ViewGdiDeviceName].Count - 1}\n";
                    }
                    else
                    {
                        stringToReturn += $"Display Source is Cloned: false\n";
                        stringToReturn += $"Number of Display Source clones: 0\n";

                    }
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the source info for source adapter #{path.SourceInfo.AdapterId}");
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
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Connector Instance: {targetInfo.ConnectorInstance} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: EDID Manufacturer ID: {targetInfo.EdidManufactureId} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: EDID Product Code ID: {targetInfo.EdidProductCodeId} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Flags Friendly Name from EDID: {targetInfo.Flags.FriendlyNameFromEdid} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Flags Friendly Name Forced: {targetInfo.Flags.FriendlyNameForced} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Flags EDID ID is Valid: {targetInfo.Flags.EdidIdsValid} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Monitor Device Path: {targetInfo.MonitorDevicePath} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Monitor Friendly Device Name: {targetInfo.MonitorFriendlyDeviceName} for source {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Output Technology: {targetInfo.OutputTechnology} for source {path.TargetInfo.Id}.");

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
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Adapter Device Path {adapterInfo.AdapterDevicePath} for source {path.TargetInfo.AdapterId}.");
                    stringToReturn += $"****** Interrogating Display Adapter {adapterInfo.AdapterDevicePath} *******\n";
                    stringToReturn += $" Display Adapter {adapterInfo.AdapterDevicePath}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the adapter device path for target #{path.TargetInfo.AdapterId}");
                }

                // show the 

                // get display target preferred mode
                var targetPreferredInfo = new DISPLAYCONFIG_TARGET_PREFERRED_MODE();
                targetPreferredInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_PREFERRED_MODE;
                targetPreferredInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_TARGET_PREFERRED_MODE>();
                targetPreferredInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                targetPreferredInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref targetPreferredInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Target Preferred Width {targetPreferredInfo.Width} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Target Preferred Height {targetPreferredInfo.Height} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Active Size: ({targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ActiveSize.Cx}x{targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ActiveSize.Cy} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Total Size: ({targetPreferredInfo.TargetMode.TargetVideoSignalInfo.TotalSize.Cx}x{targetPreferredInfo.TargetMode.TargetVideoSignalInfo.TotalSize.Cy} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Target Video Signal Info HSync Frequency: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.HSyncFreq} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Target Video Signal Info VSync Frequency: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.VSyncFreq} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Pixel Rate: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.PixelRate} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Scan Line Ordering: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ScanLineOrdering} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Target Video Signal Info Video Standard: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.VideoStandard} for target {path.TargetInfo.Id}.");

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
                    SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the preferred target name for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Virtual Resolution is Disabled: {targetBaseTypeInfo.BaseOutputTechnology} for target {path.TargetInfo.Id}.");

                    stringToReturn += $"****** Interrogating Target Base Type for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" Base Output Technology: {targetBaseTypeInfo.BaseOutputTechnology}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target base type for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Base Output Technology: {supportVirtResInfo.IsMonitorVirtualResolutionDisabled} for target {path.TargetInfo.Id}.");
                    stringToReturn += $"****** Interrogating Target Supporting virtual resolution for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" Virtual Resolution is Disabled: {supportVirtResInfo.IsMonitorVirtualResolutionDisabled}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out the virtual resolution support for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Advanced Color Supported: {colorInfo.AdvancedColorSupported} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Advanced Color Enabled: {colorInfo.AdvancedColorEnabled} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Advanced Color Force Disabled: {colorInfo.AdvancedColorForceDisabled} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Bits per Color Channel: {colorInfo.BitsPerColorChannel} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Color Encoding: {colorInfo.ColorEncoding} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found Wide Color Enforced: {colorInfo.WideColorEnforced} for target {path.TargetInfo.Id}.");

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
                    SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out the virtual resolution support for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found SDR White Level: {whiteLevelInfo.SDRWhiteLevel} for target {path.TargetInfo.Id}.");

                    stringToReturn += $"****** Interrogating SDR White Level for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" SDR White Level: {whiteLevelInfo.SDRWhiteLevel}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out the SDL white level for display #{path.TargetInfo.Id}");
                }
            }

            // Get the list of all display adapters in this machine through GDI
            Dictionary<string, GDI_DISPLAY_SETTING> gdiDeviceSettings = new Dictionary<string, GDI_DISPLAY_SETTING>();
            UInt32 displayDeviceNum = 0;
            DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
            displayDevice.Size = (UInt32)Marshal.SizeOf<DISPLAY_DEVICE>();
            stringToReturn += $"----++++==== GDI Device Information ====++++----\n";
            while (GDIImport.EnumDisplayDevices(null, displayDeviceNum, ref displayDevice, 0))
            {
                // Now we try and grab the GDI Device Info for each display device
                stringToReturn += $"****** Display Device Info for Display {displayDevice.DeviceName} *******\n";
                stringToReturn += $" Display Device ID: {displayDevice.DeviceId}\n";
                stringToReturn += $" Display Device Key: {displayDevice.DeviceKey}\n";
                stringToReturn += $" Display Device Name: {displayDevice.DeviceName}\n";
                stringToReturn += $" Display Device String: {displayDevice.DeviceString}\n";
                stringToReturn += $" Is Attached to Desktop: {displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.AttachedToDesktop)}\n";
                stringToReturn += $" Is Disconnected: {displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.Disconnect)}\n";
                stringToReturn += $" Is a Mirroing Device: {displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.MirroringDriver)}\n";
                stringToReturn += $" Has Modes Pruned: {displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.ModesPruned)}\n";
                stringToReturn += $" Is Multi-driver: {displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.MultiDriver)}\n";
                stringToReturn += $" Is Primary Display Device: {displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.PrimaryDevice)}\n";
                stringToReturn += $" Is Remote Display Device: {displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.Remote)}\n";
                stringToReturn += $" Is Removable Display Device: {displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.Removable)}\n";
                stringToReturn += $" Is VGA Compatible Display Device: {displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.VGACompatible)}\n";
                stringToReturn += $"\n";


                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Settings for {displayDevice.DeviceName}");
                if (displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.AttachedToDesktop) || displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.MultiDriver))
                {
                    // If the display device is attached to the Desktop, or a type of display that is represented by a psudeo mirroring application, then skip this display
                    // e.g. some sort of software interfaced display that doesn't have a physical plug, or maybe uses USB for communication
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Settings for {displayDevice.DeviceName}");
                    stringToReturn += $" Display Device Settings for attached Display {displayDevice.DeviceName} :\n";
                    DEVICE_MODE currentMode = new DEVICE_MODE();
                    currentMode.Size = (UInt16)Marshal.SizeOf<DEVICE_MODE>();
                    bool gdiWorked = GDIImport.EnumDisplaySettings(displayDevice.DeviceName, DISPLAY_SETTINGS_MODE.CurrentSettings, ref currentMode);
                    if (gdiWorked)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Got the current Display Settings from display {displayDevice.DeviceName}.");
                        // Now we try and grab the GDI Device Settings for each display device
                        stringToReturn += $" Bits Per Pixel: {currentMode.BitsPerPixel}\n";
                        stringToReturn += $" Device Name: {currentMode.DeviceName}\n";
                        stringToReturn += $" Display Fixed Output: {currentMode.DisplayFixedOutput}\n";
                        stringToReturn += $" Grayscale Display: {currentMode.DisplayFlags.HasFlag(DISPLAY_FLAGS.Grayscale)}\n";
                        stringToReturn += $" Interlaced Display: {currentMode.DisplayFlags.HasFlag(DISPLAY_FLAGS.Interlaced)}\n";
                        stringToReturn += $" Refresh Rate: {currentMode.DisplayFrequency}Hz\n";
                        stringToReturn += $" Display Rotation: {currentMode.DisplayOrientation.ToString("G")}\n";
                        stringToReturn += $" Driver Extra: {currentMode.DriverExtra}\n";
                        stringToReturn += $" Driver Version: {currentMode.DriverVersion}\n";
                        stringToReturn += $" All Display Fields populated by driver: {currentMode.Fields.HasFlag(DEVICE_MODE_FIELDS.AllDisplay)}\n";
                        stringToReturn += $" Display Width and Height in Pixels: {currentMode.PixelsWidth} x {currentMode.PixelsHeight}\n";
                        stringToReturn += $" Display Position: X:{currentMode.Position.X}, Y:{currentMode.Position.Y}\n";
                        stringToReturn += $" Specification Version: {currentMode.SpecificationVersion}\n";
                        stringToReturn += $"\n";
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - Unabled to get current display mode settings from display {displayDevice.DeviceName}.");
                        stringToReturn += $" No display settings found.\n\n";
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: The display {displayDevice.DeviceName} is either not attached to the desktop, or is not a mirroring driver. Skipping this display device as we cannot use it.");
                }

                displayDeviceNum++;
            }

            return stringToReturn;
        }

        public bool SetActiveConfig(WINDOWS_DISPLAY_CONFIG displayConfig)
        {
            // Get the all possible windows display configs
            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Generating a list of all the current display configs");
            WINDOWS_DISPLAY_CONFIG allWindowsDisplayConfig = GetWindowsDisplayConfig(QDC.QDC_ALL_PATHS);

            if (displayConfig.IsCloned)
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: We have a cloned display in this display profile, so using the Windows GDI to set the layout");
            }
            else
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: We have no cloned displays in thus display profile, so using the Windows CCD to set the layout");
            }

            // Now we go through the Paths to update the LUIDs as per Soroush's suggestion
            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Patching the adapter IDs to make the saved config valid");
            PatchAdapterIDs(ref displayConfig, allWindowsDisplayConfig.DisplayAdapters);

            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Testing whether the display configuration is valid");
            // Test whether a specified display configuration is supported on the computer                    
            uint myPathsCount = (uint)displayConfig.DisplayConfigPaths.Length;
            uint myModesCount = (uint)displayConfig.DisplayConfigModes.Length;
            WIN32STATUS err = CCDImport.SetDisplayConfig(myPathsCount, displayConfig.DisplayConfigPaths, myModesCount, displayConfig.DisplayConfigModes, SDC.DISPLAYMAGICIAN_VALIDATE);
            if (err == WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Successfully validated that the display configuration supplied would work!");
            }
            else
            {
                SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: ERROR - SetDisplayConfig couldn't validate the display configuration supplied. This display configuration won't work if applied.");
                return false;
            }

            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Yay! The display configuration is valid! Attempting to set the Display Config now");
            // Now set the specified display configuration for this computer                    
            err = CCDImport.SetDisplayConfig(myPathsCount, displayConfig.DisplayConfigPaths, myModesCount, displayConfig.DisplayConfigModes, SDC.DISPLAYMAGICIAN_SET | SDC.SDC_FORCE_MODE_ENUMERATION);
            if (err == WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Successfully set the display configuration to the settings supplied!");
            }
            else
            {
                SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: ERROR - SetDisplayConfig couldn't set the display configuration using the settings supplied. Something is wrong.");
                throw new WinLibraryException($"SetDisplayConfig couldn't set the display configuration using the settings supplied. Something is wrong.");
            }

            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: SUCCESS! The display configuration has been successfully applied");

            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Waiting 0.5 seconds to let the display change take place before adjusting the Windows CCD HDR settings");
            System.Threading.Thread.Sleep(500);

            foreach (ADVANCED_HDR_INFO_PER_PATH myHDRstate in displayConfig.DisplayHDRStates)
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Trying to get information whether HDR color is in use now on Display {myHDRstate.Id}.");
                // Get advanced HDR info
                var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                colorInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                colorInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                colorInfo.Header.AdapterId = myHDRstate.AdapterId;
                colorInfo.Header.Id = myHDRstate.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref colorInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Advanced Color Info gathered from Display {myHDRstate.Id}");

                    if (myHDRstate.AdvancedColorInfo.AdvancedColorSupported && colorInfo.AdvancedColorEnabled != myHDRstate.AdvancedColorInfo.AdvancedColorEnabled)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: HDR is available for use on Display {myHDRstate.Id}, and we want it set to {myHDRstate.AdvancedColorInfo.AdvancedColorEnabled} but is currently {colorInfo.AdvancedColorEnabled}.");

                        var setColorState = new DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE();
                        setColorState.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_ADVANCED_COLOR_STATE;
                        setColorState.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SET_ADVANCED_COLOR_STATE>();
                        setColorState.Header.AdapterId = myHDRstate.AdapterId;
                        setColorState.Header.Id = myHDRstate.Id;
                        setColorState.EnableAdvancedColor = myHDRstate.AdvancedColorInfo.AdvancedColorEnabled;
                        err = CCDImport.DisplayConfigSetDeviceInfo(ref setColorState);
                        if (err == WIN32STATUS.ERROR_SUCCESS)
                        {
                            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: SUCCESS! Set HDR successfully to {myHDRstate.AdvancedColorInfo.AdvancedColorEnabled} on Display {myHDRstate.Id}");
                        }
                        else
                        {
                            SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: ERROR - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to set the HDR settings for display #{myHDRstate.Id}");
                            return false;
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Skipping setting HDR on Display {myHDRstate.Id} as it does not support HDR");
                    }
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out if HDR is supported for display #{myHDRstate.Id}");
                }

            }

            // Get the existing displays
            Dictionary<string, GDI_DISPLAY_SETTING> currentGdiDisplaySettings = GetGdiDisplaySettings();

            // Apply the previously saved display settings to the new displays (match them up)
            // NOTE: This may be the only mode needed once it's completed.
            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Attempting to change Display Device settings through GDI API using ");
            foreach (var myGdiDisplaySettings in displayConfig.GdiDisplaySettings)
            {
                string displayDeviceKey = myGdiDisplaySettings.Key;
                GDI_DISPLAY_SETTING displayDeviceSettings = myGdiDisplaySettings.Value;
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Trying to change Device Mode for Display {displayDeviceKey}.");

                if (currentGdiDisplaySettings.ContainsKey(displayDeviceKey))
                {
                    string currentDeviceName = currentGdiDisplaySettings[displayDeviceKey].Device.DeviceName;
                    DEVICE_MODE currentModeToUse = currentGdiDisplaySettings[displayDeviceKey].DeviceMode;
                    DEVICE_MODE modeToUse = displayDeviceSettings.DeviceMode;

                    CHANGE_DISPLAY_RESULTS result = GDIImport.ChangeDisplaySettingsEx(currentDeviceName, ref modeToUse, IntPtr.Zero, CHANGE_DISPLAY_SETTINGS_FLAGS.CDS_UPDATEREGISTRY, IntPtr.Zero);
                    if (result == CHANGE_DISPLAY_RESULTS.Successful)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Successfully changed display {displayDeviceKey} to use the new mode!");
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.BadDualView)
                    {
                        SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: The settings change was unsuccessful because the system is DualView capable. Display {displayDeviceKey} not updated to new mode.");
                        return false;
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.BadFlags)
                    {
                        SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: An invalid set of flags was passed in. Display {displayDeviceKey} not updated to use the new mode.");
                        return false;
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.BadMode)
                    {
                        SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: The graphics mode is not supported. Display {displayDeviceKey} not updated to use the new mode.");
                        return false;
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.BadParam)
                    {
                        SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: An invalid parameter was passed in. This can include an invalid flag or combination of flags. Display {displayDeviceKey} not updated to use the new mode.");
                        return false;
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.Failed)
                    {
                        SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: The display driver failed to apply the specified graphics mode. Display {displayDeviceKey} not updated to use the new mode.");
                        return false;
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.NotUpdated)
                    {
                        SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: Unable to write new settings to the registry. Display {displayDeviceKey} not updated to use the new mode.");
                        return false;
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.Restart)
                    {
                        SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: The computer must be restarted for the graphics mode to work. Display {displayDeviceKey} not updated to use the new mode.");
                        return false;
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Display {displayDeviceKey} not updated to use the new mode.");
                    }
                }

            }

            return true;
        }

        public bool IsActiveConfig(WINDOWS_DISPLAY_CONFIG displayConfig)
        {
            // Get the current windows display configs to compare to the one we loaded
            WINDOWS_DISPLAY_CONFIG currentWindowsDisplayConfig = GetWindowsDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS);

            // Check whether the display config is in use now
            SharedLogger.logger.Trace($"WinLibrary/IsActiveConfig: Checking whether the display configuration is already being used.");
            if (displayConfig.Equals(currentWindowsDisplayConfig))
            {
                SharedLogger.logger.Trace($"WinLibrary/IsActiveConfig: The display configuration is already being used (supplied displayConfig Equals currentWindowsDisplayConfig");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"WinLibrary/IsActiveConfig: The display configuration is NOT currently in use (supplied displayConfig Equals currentWindowsDisplayConfig");
                return false;
            }

        }

        public bool IsValidConfig(WINDOWS_DISPLAY_CONFIG displayConfig)
        {
            // Get the current windows display configs
            WINDOWS_DISPLAY_CONFIG allWindowsDisplayConfig = GetWindowsDisplayConfig(QDC.QDC_ALL_PATHS);

            SharedLogger.logger.Trace("WinLibrary/PatchAdapterIDs: Going through the list of adapters we stored in the config to make sure they still exist");
            // Firstly check that the Adapter Names are still currently available (i.e. the adapter hasn't been replaced).
            foreach (string savedAdapterName in displayConfig.DisplayAdapters.Values)
            {
                // If there is even one of the saved adapters that has changed, then it's no longer possible
                // to use this display config!
                if (!allWindowsDisplayConfig.DisplayAdapters.Values.Contains(savedAdapterName))
                {
                    SharedLogger.logger.Error($"WinLibrary/PatchAdapterIDs: ERROR - Saved adapter {savedAdapterName} is not available right now! This display configuration won't work!");
                    return false;
                }
            }
            SharedLogger.logger.Trace($"WinLibrary/PatchAdapterIDs: All teh adapters that the display configuration uses are still avilable to use now!");

            // Now we go through the Paths to update the LUIDs as per Soroush's suggestion
            SharedLogger.logger.Trace($"WinLibrary/IsPossibleConfig: Attemptong to patch the saved display configuration's adapter IDs so that it will still work (these change at each boot)");
            PatchAdapterIDs(ref displayConfig, allWindowsDisplayConfig.DisplayAdapters);

            SharedLogger.logger.Trace($"WinLibrary/IsPossibleConfig: Testing whether the display configuration is valid ");
            // Test whether a specified display configuration is supported on the computer                    
            uint myPathsCount = (uint)displayConfig.DisplayConfigPaths.Length;
            uint myModesCount = (uint)displayConfig.DisplayConfigModes.Length;
            WIN32STATUS err = CCDImport.SetDisplayConfig(myPathsCount, displayConfig.DisplayConfigPaths, myModesCount, displayConfig.DisplayConfigModes, SDC.DISPLAYMAGICIAN_VALIDATE);
            if (err == WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Trace($"WinLibrary/IsPossibleConfig: SetDisplayConfig validated that the display configuration is valid and can be used!");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"WinLibrary/IsPossibleConfig: SetDisplayConfig confirmed that the display configuration is invalid and cannot be used!");
                return false;
            }

        }

        public bool IsPossibleConfig(WINDOWS_DISPLAY_CONFIG displayConfig)
        {
            // We want to check the Windows Display profile can be used now
            SharedLogger.logger.Trace($"WinLibrary/IsPossibleConfig: Testing whether the Windows display configuration is possible to be used now");

            // check what the currently available displays are (include the ones not active)
            List<string> currentAllIds = GetAllConnectedDisplayIdentifiers();

            // CHeck that we have all the displayConfig DisplayIdentifiers we need available now
            //if (currentAllIds.Intersect(displayConfig.DisplayIdentifiers).Count() == displayConfig.DisplayIdentifiers.Count)
            if (displayConfig.DisplayIdentifiers.All(value => currentAllIds.Contains(value)))
            {
                SharedLogger.logger.Trace($"WinLibrary/IsPossibleConfig: Success! THe Windows display configuration is possible to be used now");
                return true;
            }
            else
            {
                SharedLogger.logger.Trace($"WinLibrary/IsPossibleConfig: Uh oh! THe Windows display configuration is possible cannot be used now");
                return false;
            }

        }

        public List<string> GetCurrentDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"WinLibrary/GetCurrentDisplayIdentifiers: Getting the current display identifiers for the displays in use now");
            return GetSomeDisplayIdentifiers(QDC.QDC_ONLY_ACTIVE_PATHS);
        }

        public List<string> GetAllConnectedDisplayIdentifiers()
        {
            SharedLogger.logger.Trace($"WinLibrary/GetAllConnectedDisplayIdentifiers: Getting all the display identifiers that can possibly be used");
            return GetSomeDisplayIdentifiers(QDC.QDC_ALL_PATHS);
        }

        private List<string> GetSomeDisplayIdentifiers(QDC selector = QDC.QDC_ONLY_ACTIVE_PATHS)
        {
            SharedLogger.logger.Debug($"WinLibrary/GetCurrentDisplayIdentifiers: Generating the unique Display Identifiers for the currently active configuration");

            List<string> displayIdentifiers = new List<string>();

            SharedLogger.logger.Trace($"WinLibrary/GetCurrentDisplayIdentifiers: Testing whether the display configuration is valid (allowing tweaks).");
            // Get the size of the largest Active Paths and Modes arrays
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(selector, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/PrintActiveConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(selector, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetSomeDisplayIdentifiers: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(selector, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetSomeDisplayIdentifiers: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(selector, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetSomeDisplayIdentifiers: ERROR - The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                    throw new WinLibraryException($"The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                }
                else if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetSomeDisplayIdentifiers: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again");
                    throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again.");
                }
            }
            else if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetSomeDisplayIdentifiers: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays");
                throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays.");
            }

            foreach (var path in paths)
            {
                if (path.TargetInfo.TargetAvailable == false)
                {
                    // We want to skip this one cause it's not valid
                    SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Skipping path due to TargetAvailable not existing in display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Successfully got the source info from {path.SourceInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetSomeDisplayIdentifiers: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.SourceInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Successfully got the target info from {path.TargetInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetSomeDisplayIdentifiers: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Successfully got the display name info from {path.TargetInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetSomeDisplayIdentifiers: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Warn(ex, $"WinLibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display Adapter Device Path from video card. Substituting with a # instead");
                    displayInfo.Add("#");
                }
                try
                {
                    displayInfo.Add(targetInfo.OutputTechnology.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"WinLibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display Connector Instance from video card. Substituting with a # instead");
                    displayInfo.Add("#");
                }
                try
                {
                    displayInfo.Add(targetInfo.EdidManufactureId.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"WinLibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display EDID Manufacturer Code from video card. Substituting with a # instead");
                    displayInfo.Add("#");
                }
                try
                {
                    displayInfo.Add(targetInfo.EdidProductCodeId.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"WinLibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display EDID Product Code from video card. Substituting with a # instead");
                    displayInfo.Add("#");
                }
                try
                {
                    displayInfo.Add(targetInfo.MonitorDevicePath.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"WinLibrary/GetSomeDisplayIdentifiers: Exception getting Windows Path Target Info Id from video card. Substituting with a # instead");
                    displayInfo.Add("#");
                }
                try
                {
                    displayInfo.Add(targetInfo.MonitorFriendlyDeviceName.ToString());
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"WinLibrary/GetSomeDisplayIdentifiers: Exception getting Windows Display Target Friendly name from video card. Substituting with a # instead");
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

        public List<string> GetCurrentPCIVideoCardVendors()
        {
            SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Getting the current PCI vendor ids for the videocards reported to Windows");
            List<string> videoCardVendorIds = new List<string>();


            SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Testing whether the display configuration is valid (allowing tweaks).");
            // Get the size of the largest Active Paths and Modes arrays
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetCurrentPCIVideoCardVendors: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetCurrentPCIVideoCardVendors: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetCurrentPCIVideoCardVendors: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetCurrentPCIVideoCardVendors: ERROR - The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                    throw new WinLibraryException($"The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                }
                else if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetCurrentPCIVideoCardVendors: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again");
                    throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again.");
                }
            }
            else if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetCurrentPCIVideoCardVendors: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays");
                throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays.");
            }

            foreach (var path in paths)
            {
                if (path.TargetInfo.TargetAvailable == false)
                {
                    // We want to skip this one cause it's not valid
                    SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Skipping path due to TargetAvailable not existing in display #{path.TargetInfo.Id}");
                    continue;
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
                    SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Successfully got the display name info from {path.TargetInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetCurrentPCIVideoCardVendors: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.TargetInfo.Id}");
                }

                try
                {
                    // The AdapterDevicePath is something like "\\\\?\\PCI#VEN_10DE&DEV_2482&SUBSYS_408E1458&REV_A1#4&2283f625&0&0019#{5b45201d-f2f2-4f3b-85bb-30ff1f953599}"
                    // We only want the vendor ID
                    SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The AdapterDevicePath for this path is :{adapterInfo.AdapterDevicePath}");
                    // Match against the vendor ID
                    string pattern = @"VEN_([\d\w]{4})&";
                    Match match = Regex.Match(adapterInfo.AdapterDevicePath, pattern);
                    if (match.Success)
                    {
                        string VendorId = match.Groups[1].Value;
                        SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The matched PCI Vendor ID is :{VendorId }");
                        if (!videoCardVendorIds.Contains(VendorId))
                        {
                            videoCardVendorIds.Add(VendorId);
                            SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Stored PCI vendor ID {VendorId} as we haven't already got it");
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The PCI Vendor ID pattern wasn't matched so we didn't record a vendor ID.");
                    }

                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"WinLibrary/GetCurrentPCIVideoCardVendors: Exception getting PCI Vendor ID from Display Adapter {path.SourceInfo.AdapterId}.");
                }

            }

            return videoCardVendorIds;

        }

    }

    [global::System.Serializable]
    public class WinLibraryException : Exception
    {
        public WinLibraryException() { }
        public WinLibraryException(string message) : base(message) { }
        public WinLibraryException(string message, Exception inner) : base(message, inner) { }
        protected WinLibraryException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}