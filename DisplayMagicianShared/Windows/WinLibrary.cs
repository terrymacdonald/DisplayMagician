using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Text.RegularExpressions;
using DisplayMagicianShared;
using System.IO;
using System.ComponentModel;
using Microsoft.Win32;
using System.Threading.Tasks;
using static DisplayMagicianShared.Windows.TaskBarLayout;
using System.Diagnostics;
using Windows.ApplicationModel;
using EDIDParser;
using static DisplayMagicianShared.NVIDIA.DisplayTopologyStatus;
using System.Runtime.Intrinsics.Arm;
using DisplayMagicianShared.Helpers;
using NLog.Targets;
using System.Threading;

namespace DisplayMagicianShared.Windows
{

    public class DisplayMonitorInfo
    {
        public string FriendlyName { get; set; }
        public ushort ManufacturerId { get; set; }
        public ushort ProductCodeId { get; set; }
        public string DevicePath { get; set; }
    }

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
    public struct DISPLAY_SOURCE : IEquatable<DISPLAY_SOURCE>
    {
        public LUID AdapterId;
        public UInt32 SourceId;
        public UInt32 TargetId;
        public string DevicePath;
        public DPIScalingInfo SourceDPIScalingInfo;

        public override bool Equals(object obj) => obj is DISPLAY_SOURCE other && this.Equals(other);
        public bool Equals(DISPLAY_SOURCE other)
        =>  //SourceId.Equals(other.SourceId) &&  // Source ID needs to be ignored in this case, as windows moves the source ids around :(
            TargetId.Equals(other.TargetId) &&
            DevicePath.Equals(other.DevicePath) &&
            SourceDPIScalingInfo.Equals(other.SourceDPIScalingInfo);
        //=> true;
        public override int GetHashCode()
        {
            return (TargetId, DevicePath, SourceDPIScalingInfo).GetHashCode();
        }

        public static bool operator ==(DISPLAY_SOURCE lhs, DISPLAY_SOURCE rhs) => lhs.Equals(rhs);

        public static bool operator !=(DISPLAY_SOURCE lhs, DISPLAY_SOURCE rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOWS_DISPLAY_CONFIG : IEquatable<WINDOWS_DISPLAY_CONFIG>
    {
        public Dictionary<ulong, string> DisplayAdapters;
        public DISPLAYCONFIG_PATH_INFO[] DisplayConfigPaths;
        public DISPLAYCONFIG_MODE_INFO[] DisplayConfigModes;
        public List<ADVANCED_HDR_INFO_PER_PATH> DisplayHDRStates;
        public Dictionary<string, GDI_DISPLAY_SETTING> GdiDisplaySettings;
        public bool IsCloned;
        // Note: We purposely have left out the DisplaySources from the Equals as it's order keeps changing after each reboot and after each profile swap
        // and it is informational only and doesn't contribute to the configuration (it's used for generating the Screens structure, and therefore for
        // generating the profile icon.
        public Dictionary<string, List<DISPLAY_SOURCE>> DisplaySources;
        public List<string> DisplayIdentifiers;

        public override bool Equals(object obj) => obj is WINDOWS_DISPLAY_CONFIG other && this.Equals(other);
        public bool Equals(WINDOWS_DISPLAY_CONFIG other)
        {
            if (!(IsCloned == other.IsCloned &&
           DisplayConfigPaths.SequenceEqual(other.DisplayConfigPaths) &&
           DisplayConfigModes.SequenceEqual(other.DisplayConfigModes) &&
           // The dictionary keys sometimes change after returning from NVIDIA Surround, so we need to only focus on comparing the values of the GDISettings.
           // Additionally, we had to disable the DEviceKey from the equality testing within the GDI library itself as that waould also change after changing back from NVIDIA surround
           // This still allows us to detect when refresh rates change, which will allow DisplayMagician to detect profile differences.
           GdiDisplaySettings.Values.SequenceEqual(other.GdiDisplaySettings.Values) &&
           DisplayIdentifiers.SequenceEqual(other.DisplayIdentifiers)))
            {
                return false;
            }

            // Now we need to go through the HDR states comparing vaues, as the order changes if there is a cloned display
            if (!WinLibrary.EqualButDifferentOrder<ADVANCED_HDR_INFO_PER_PATH>(DisplayHDRStates, other.DisplayHDRStates))
            {
                return false;
            }

            // Now we need to go through the values to make sure they are the same, but ignore the keys (as they change after each reboot!)
            for (int i = 0; i < DisplaySources.Count; i++)
            {
                if (!DisplaySources.ElementAt(i).Value.SequenceEqual(other.DisplaySources.ElementAt(i).Value))
                {
                    return false;
                }
            }
            return true;


        }

        public override int GetHashCode()
        {
            // Temporarily disabled this to make sure that the hashcode generation matched the equality tests.
            //return (DisplayConfigPaths, DisplayConfigModes, DisplayHDRStates, IsCloned, DisplayIdentifiers, TaskBarLayout, TaskBarSettings).GetHashCode();
            return (DisplayConfigPaths, DisplayConfigModes, DisplayHDRStates, IsCloned, DisplayIdentifiers).GetHashCode();
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
        private WINDOWS_DISPLAY_CONFIG _activeDisplayConfig;
        public List<DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY> SkippedColorConnectionTypes;
        public List<string> _allConnectedDisplayIdentifiers;

        // To detect redundant calls
        private bool _disposed = false;

        // Instantiate a SafeHandle instance.
        private SafeHandle _safeHandle = new SafeFileHandle(IntPtr.Zero, true);

        static WinLibrary() { }
        public WinLibrary()
        {
            // Populate the list of ConnectionTypes we want to skip as they don't support querying
            SkippedColorConnectionTypes = new List<DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY> {
                DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_HD15,
                DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPONENT_VIDEO,
                DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_COMPOSITE_VIDEO,
                DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI,
                DISPLAYCONFIG_VIDEO_OUTPUT_TECHNOLOGY.DISPLAYCONFIG_OUTPUT_TECHNOLOGY_SVIDEO
            };

            SharedLogger.logger.Trace("WinLibrary/WinLibrary: Intialising Windows CCD library interface");
            _initialised = true;

            // Set the DPI awareness for the process this thread is running within so that the DPI calls return the right values at the right times
            CCDImport.SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED);

            _activeDisplayConfig = GetActiveConfig();
            _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers();
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

        public WINDOWS_DISPLAY_CONFIG ActiveDisplayConfig
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
            myDefaultConfig.DisplayHDRStates = new List<ADVANCED_HDR_INFO_PER_PATH>();
            myDefaultConfig.DisplayIdentifiers = new List<string>();
            myDefaultConfig.DisplaySources = new Dictionary<string, List<DISPLAY_SOURCE>>();
            myDefaultConfig.GdiDisplaySettings = new Dictionary<string, GDI_DISPLAY_SETTING>();
            myDefaultConfig.IsCloned = false;

            return myDefaultConfig;
        }

        public void PatchWindowsDisplayConfig(ref WINDOWS_DISPLAY_CONFIG savedDisplayConfig)
        {

            Dictionary<ulong, ulong> adapterOldToNewMap = new Dictionary<ulong, ulong>();
            Dictionary<ulong, string> currentAdapterMap = GetAllAdapterIDs();
            try
            {
                SharedLogger.logger.Trace("WinLibrary/PatchWindowsDisplayConfig: Going through the list of adapters we stored in the config to figure out the old adapterIDs");
                foreach (KeyValuePair<ulong, string> savedAdapter in savedDisplayConfig.DisplayAdapters)
                {
                    bool adapterMatched = false;
                    foreach (KeyValuePair<ulong, string> currentAdapter in currentAdapterMap)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Checking if saved adapter {savedAdapter.Key} (AdapterName is {savedAdapter.Value}) is equal to current adapter id {currentAdapter.Key} (AdapterName is {currentAdapter.Value})");

                        if (currentAdapter.Value.Equals(savedAdapter.Value))
                        {
                            // we have found the new LUID Value for the same adapter
                            // So we want to store it
                            SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: We found that saved adapter {savedAdapter.Key} has now been assigned adapter id {currentAdapter.Key} (AdapterName is {savedAdapter.Value})");
                            adapterOldToNewMap.Add(savedAdapter.Key, currentAdapter.Key);
                            adapterMatched = true;
                        }
                    }
                    if (!adapterMatched)
                    {
                        SharedLogger.logger.Error($"WinLibrary/PatchWindowsDisplayConfig: Saved adapter {savedAdapter.Key} (AdapterName is {savedAdapter.Value}) doesn't have a current match! The adapters have changed since the configuration was last saved.");
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "WinLibrary/PatchWindowsDisplayConfig: Exception while going through the list of adapters we stored in the config to figure out the old adapterIDs");
            }

            ulong newAdapterValue = 0;
            ulong oldAdapterValue = 0;

            try
            {
                // Update the DisplayAdapters with the current adapter id
                SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Going through the display adatpers to update the adapter id");
                ulong[] currentKeys = savedDisplayConfig.DisplayAdapters.Keys.ToArray();
                var currentLength = savedDisplayConfig.DisplayAdapters.Count;
                for (int i = 0; i < currentLength; i++)
                {
                    oldAdapterValue = currentKeys[i];
                    // Change the Dictionary Key AdapterIDs
                    if (adapterOldToNewMap.ContainsKey(oldAdapterValue))
                    {
                        // We get here if there is a matching adapter
                        newAdapterValue = adapterOldToNewMap[oldAdapterValue];

                        // Skip if we've already replaced something!
                        if (!savedDisplayConfig.DisplayAdapters.ContainsKey(newAdapterValue))
                        {
                            // Add a new dictionary key with the old value
                            savedDisplayConfig.DisplayAdapters.Add(newAdapterValue, savedDisplayConfig.DisplayAdapters[oldAdapterValue]);
                            // Remove the old dictionary key
                            savedDisplayConfig.DisplayAdapters.Remove(oldAdapterValue);
                        }
                        SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Updated DisplayAdapter from adapter {oldAdapterValue} to adapter {newAdapterValue} instead.");
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "WinLibrary/PatchWindowsDisplayConfig: Exception while going through the display adapters update the adapter ids");
            }

            try
            {
                // Update the paths with the current adapter id
                SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Going through the display config paths to update the adapter id");
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
                        SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Updated DisplayConfig Path #{i} from adapter {savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId.Value} to adapter {newAdapterValue} instead.");
                    }
                    else
                    {
                        // if there isn't a matching adapter, then we just pick the first current one and hope that works!
                        // (it is highly likely to... its only if the user has multiple graphics cards with some weird config it may break)
                        newAdapterValue = currentAdapterMap.First().Key;
                        SharedLogger.logger.Warn($"WinLibrary/PatchWindowsDisplayConfig: Uh Oh. Adapter {savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId.Value} didn't have a current match! It's possible the adapter was swapped or disabled. Attempting to use adapter {newAdapterValue} instead.");
                        savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId = AdapterValueToLUID(newAdapterValue);
                        savedDisplayConfig.DisplayConfigPaths[i].TargetInfo.AdapterId = AdapterValueToLUID(newAdapterValue);
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "WinLibrary/PatchWindowsDisplayConfig: Exception while going through the display config paths to update the adapter id");
            }


            try
            {
                SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Going through the display config modes to update the adapter id");
                // Update the modes with the current adapter id
                for (int i = 0; i < savedDisplayConfig.DisplayConfigModes.Length; i++)
                {
                    // Change the Mode AdapterID
                    if (adapterOldToNewMap.ContainsKey(savedDisplayConfig.DisplayConfigModes[i].AdapterId.Value))
                    {
                        // We get here if there is a matching adapter
                        newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayConfigModes[i].AdapterId.Value];
                        savedDisplayConfig.DisplayConfigModes[i].AdapterId = AdapterValueToLUID(newAdapterValue);
                        SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Updated DisplayConfig Mode #{i} from adapter {savedDisplayConfig.DisplayConfigModes[i].AdapterId.Value} to adapter {newAdapterValue} instead.");
                    }
                    else
                    {
                        // if there isn't a matching adapter, then we just pick the first current one and hope that works!
                        // (it is highly likely to... its only if the user has multiple graphics cards with some weird config it may break)
                        newAdapterValue = currentAdapterMap.First().Key;
                        SharedLogger.logger.Warn($"WinLibrary/PatchWindowsDisplayConfig: Uh Oh. Adapter {savedDisplayConfig.DisplayConfigModes[i].AdapterId.Value} didn't have a current match! It's possible the adapter was swapped or disabled. Attempting to use adapter {newAdapterValue} instead.");
                        savedDisplayConfig.DisplayConfigModes[i].AdapterId = AdapterValueToLUID(newAdapterValue);
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "WinLibrary/PatchWindowsDisplayConfig: Exception while going through the display config modes to update the adapter id");
            }


            try
            {
                SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Going through the display config HDR info to update the adapter id");
                if (savedDisplayConfig.DisplayHDRStates.Count > 0)
                {
                    // Update the HDRInfo with the current adapter id
                    for (int i = 0; i < savedDisplayConfig.DisplayHDRStates.Count; i++)
                    {
                        ADVANCED_HDR_INFO_PER_PATH hdrInfo = savedDisplayConfig.DisplayHDRStates[i];
                        // Change the Mode AdapterID
                        if (adapterOldToNewMap.ContainsKey(savedDisplayConfig.DisplayHDRStates[i].AdapterId.Value))
                        {
                            SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: adapterOldToNewMap contains adapter {savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId.Value} so using the new adapter ID of {newAdapterValue} instead.");
                            // We get here if there is a matching adapter
                            newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayHDRStates[i].AdapterId.Value];
                            hdrInfo.AdapterId = AdapterValueToLUID(newAdapterValue);
                            newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayHDRStates[i].AdvancedColorInfo.Header.AdapterId.Value];
                            hdrInfo.AdvancedColorInfo.Header.AdapterId = AdapterValueToLUID(newAdapterValue);
                            newAdapterValue = adapterOldToNewMap[savedDisplayConfig.DisplayHDRStates[i].SDRWhiteLevel.Header.AdapterId.Value];
                            hdrInfo.SDRWhiteLevel.Header.AdapterId = AdapterValueToLUID(newAdapterValue);
                            SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Updated Display HDR state #{i} from adapter {savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId.Value} to adapter {newAdapterValue} instead.");
                        }
                        else
                        {
                            // if there isn't a matching adapter, then we just pick the first current one and hope that works!
                            // (it is highly likely to... its only if the user has multiple graphics cards with some weird config it may break)
                            newAdapterValue = currentAdapterMap.First().Key;
                            SharedLogger.logger.Warn($"WinLibrary/PatchWindowsDisplayConfig: Uh Oh. Adapter {savedDisplayConfig.DisplayHDRStates[i].AdapterId.Value} didn't have a current match! It's possible the adapter was swapped or disabled. Attempting to use adapter {newAdapterValue} instead.");
                            hdrInfo.AdapterId = AdapterValueToLUID(newAdapterValue);
                            hdrInfo.AdvancedColorInfo.Header.AdapterId = AdapterValueToLUID(newAdapterValue);
                            hdrInfo.SDRWhiteLevel.Header.AdapterId = AdapterValueToLUID(newAdapterValue);
                        }
                        savedDisplayConfig.DisplayHDRStates[i] = hdrInfo;
                    }
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/PatchWindowsDisplayConfig: There are no Display HDR states to update. Skipping.");
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "WinLibrary/PatchWindowsDisplayConfig: Exception while going through the display config HDR info to update the adapter id");
            }


            try
            {
                SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Going through the display sources list info to update the adapter id");
                // Update the DisplaySources with the current adapter id
                for (int i = 0; i < savedDisplayConfig.DisplaySources.Count; i++)
                {
                    List<DISPLAY_SOURCE> dsList = savedDisplayConfig.DisplaySources.ElementAt(i).Value;
                    if (dsList.Count > 0)
                    {
                        for (int j = 0; j < dsList.Count; j++)
                        {
                            DISPLAY_SOURCE ds = dsList[j];
                            // Change the Display Source AdapterID
                            if (adapterOldToNewMap.ContainsKey(ds.AdapterId.Value))
                            {
                                // We get here if there is a matching adapter
                                newAdapterValue = adapterOldToNewMap[ds.AdapterId.Value];
                                ds.AdapterId = AdapterValueToLUID(newAdapterValue);
                                SharedLogger.logger.Trace($"WinLibrary/PatchWindowsDisplayConfig: Updated DisplaySource #{i} from adapter {savedDisplayConfig.DisplayConfigPaths[i].SourceInfo.AdapterId.Value} to adapter {newAdapterValue} instead.");
                            }
                            else
                            {
                                // if there isn't a matching adapter, then we just pick the first current one and hope that works!
                                // (it is highly likely to... its only if the user has multiple graphics cards with some weird config it may break)
                                newAdapterValue = currentAdapterMap.First().Key;
                                SharedLogger.logger.Warn($"WinLibrary/PatchAdapterIDs: Uh Oh. Adapter {savedDisplayConfig.DisplayHDRStates[i].AdapterId.Value} didn't have a current match in Display Sources! It's possible the adapter was swapped or disabled. Attempting to use adapter {newAdapterValue} instead.");
                                ds.AdapterId = AdapterValueToLUID(newAdapterValue);
                            }
                            dsList[j] = ds;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Error(ex, "WinLibrary/PatchWindowsDisplayConfig: Exception while going through the display sources list info to update the adapter id");
            }

        }

        public DPIScalingInfo GetDPISettings(LUID pathSourceAdapterId, uint pathSourceId,  uint pathTargetId)
        {

            DPIScalingInfo sourceDPIScalingInfo = new DPIScalingInfo();
            DISPLAYCONFIG_SOURCE_DPI_SCALE_GET displayScalingInfo = new DISPLAYCONFIG_SOURCE_DPI_SCALE_GET();
            displayScalingInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_DPI_SCALE;
            displayScalingInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DPI_SCALE_GET>(); ;
            displayScalingInfo.Header.AdapterId = pathSourceAdapterId;
            displayScalingInfo.Header.Id = pathSourceId;
            WIN32STATUS err = CCDImport.DisplayConfigGetDeviceInfo(ref displayScalingInfo);
            if (Marshal.SizeOf(displayScalingInfo)!= 0x20)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetDPISettings: The size of the DPI structure returned from windows API is not 32 (0x20). It looks like windows has updated it's API, so this needs checking!");
            }

            if (err == WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Trace($"WinLibrary/GetDPISettings: Found Windows DPI scaling value for source {pathSourceId} is {displayScalingInfo.CurrrentScaleRel}.");
                // We just got a relative identifier for the scaling, but we need to derive the absolute value from it n order to be able to store it.
                // First up we make sure the value is within the limits of what the OS can show
                if (displayScalingInfo.CurrrentScaleRel < displayScalingInfo.MinScaleRel)
                {
                    displayScalingInfo.CurrrentScaleRel = displayScalingInfo.MinScaleRel;
                }
                else if (displayScalingInfo.CurrrentScaleRel > displayScalingInfo.MaxScaleRel)
                {
                    displayScalingInfo.CurrrentScaleRel = displayScalingInfo.MaxScaleRel;
                }

                Int32 minAbs = Math.Abs((int)displayScalingInfo.MinScaleRel);
                if (CCDImport.DPI_VALUE_LIST.Length >= (minAbs + displayScalingInfo.MaxScaleRel + 1))
                {//all ok
                    sourceDPIScalingInfo.Minimum = CCDImport.DPI_VALUE_LIST[minAbs + displayScalingInfo.MinScaleRel];
                    sourceDPIScalingInfo.Current = CCDImport.DPI_VALUE_LIST[minAbs + displayScalingInfo.CurrrentScaleRel];
                    sourceDPIScalingInfo.Recommended = CCDImport.DPI_VALUE_LIST[minAbs];
                    sourceDPIScalingInfo.Maximum = CCDImport.DPI_VALUE_LIST[minAbs + displayScalingInfo.MaxScaleRel];
                    SharedLogger.logger.Trace($"WinLibrary/GetDPISettings: Display {pathTargetId} is currently set to DPI value {sourceDPIScalingInfo.Current} and the DPI value recommended is {sourceDPIScalingInfo.Recommended}. The current DPI value offset is {displayScalingInfo.CurrrentScaleRel}");
                }
                else
                {
                    // Warning! The information returned from windows is different to what we were expecting.
                    SharedLogger.logger.Warn($"WinLibrary/GetDPISettings: WARNING - Windows DPI Scaling info returned from windows is different from its expected values for display {pathTargetId}.");
                }
            }
            else
            {
                SharedLogger.logger.Warn($"WinLibrary/GetDPISettings: WARNING - Unabled to get Windows DPI Scaling value for display {pathTargetId}.");
            }
            return sourceDPIScalingInfo;
        }

        public bool SetDPISettings(LUID pathSourceAdapterId, uint pathSourceId, uint pathTargetId, DPIScalingInfo suppliedDPIScalingInfo)
        {
            // Get the current settigns as we stand so we know how much we need to adjust by
            DPIScalingInfo currentDPIScalingInfo = GetDPISettings(pathSourceAdapterId, pathSourceId, pathTargetId);

            // Skip doing anything if we're the same DPI!
            if (suppliedDPIScalingInfo.Current == currentDPIScalingInfo.Current)
            {
                return true;
            }

            // Otherwise we need to figure out how how much relative scaling we need to do

            if (suppliedDPIScalingInfo.Current < currentDPIScalingInfo.Minimum)
            {
                suppliedDPIScalingInfo.Current = currentDPIScalingInfo.Minimum;
            }
            else if (suppliedDPIScalingInfo.Current > currentDPIScalingInfo.Maximum)
            {
                suppliedDPIScalingInfo.Current = currentDPIScalingInfo.Maximum;
            }

            // Get the indexes of the array items that we want  - the location of the value we want to set, and location of the recommended value
            int idxDPIValueWeWant = -1;
            int idxDPIRecommendedValue = -1;

            for (int i=0; i < CCDImport.DPI_VALUE_LIST.Length; i++)
            {
                if (CCDImport.DPI_VALUE_LIST[i] == suppliedDPIScalingInfo.Current)
                {
                    idxDPIValueWeWant = i;
                }
                if (CCDImport.DPI_VALUE_LIST[i] == currentDPIScalingInfo.Recommended)
                {
                    idxDPIRecommendedValue = i;
                }
            }

            if ((idxDPIValueWeWant == -1) || (idxDPIRecommendedValue == -1))
            {
                // Didn't manage to find the entries in the list, so there is something wrong!
                SharedLogger.logger.Warn($"WinLibrary/SetDPISettings: WARNING - Cannot find the DPI settings supplied in our known list of DPI settings for display {pathTargetId}. We were looking for {suppliedDPIScalingInfo.Current} in the following list {String.Join(",",CCDImport.DPI_VALUE_LIST)}");
                return false;
            }

            // Now calculate the all important relative scaling value we need to actually make the change
            int dpiRelativeVal = idxDPIValueWeWant - idxDPIRecommendedValue;
            SharedLogger.logger.Trace($"WinLibrary/SetDPISettings: Found the DPI value we want ({suppliedDPIScalingInfo.Current}) is at index {idxDPIValueWeWant}, and the value of the current recommended ({currentDPIScalingInfo.Recommended}) is at index {idxDPIRecommendedValue} for display {pathTargetId}. The DPI relative value we need is therefore {dpiRelativeVal}.");

            // We only need to set the source on the first display source
            // Set the Windows Scaling DPI per source
            DISPLAYCONFIG_SOURCE_DPI_SCALE_SET displayScalingInfo = new DISPLAYCONFIG_SOURCE_DPI_SCALE_SET();
            displayScalingInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_SET_DPI_SCALE;
            displayScalingInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DPI_SCALE_SET>(); ;
            displayScalingInfo.Header.AdapterId = pathSourceAdapterId;
            displayScalingInfo.Header.Id = pathSourceId;
            displayScalingInfo.ScaleRel = dpiRelativeVal;
            WIN32STATUS err = CCDImport.DisplayConfigSetDeviceInfo(ref displayScalingInfo);
            if (err == WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Trace($"WinLibrary/SetDPISettings: Successfully set the DPI relative value for source {pathSourceId} to {displayScalingInfo.ScaleRel} ({suppliedDPIScalingInfo.Current}).");
                return true;
            }
            else
            {
                SharedLogger.logger.Warn($"WinLibrary/SetDPISettings: WARNING - Unable to set DPI relative value for source {pathSourceId} to {displayScalingInfo.ScaleRel} ({suppliedDPIScalingInfo.Current}).");
                return false;

            }

        }

        public bool UpdateActiveConfig(bool fastScan = true)
        {
            SharedLogger.logger.Trace($"WinLibrary/UpdateActiveConfig: Updating the currently active config");
            try
            {
                _activeDisplayConfig = GetActiveConfig(fastScan);
                _allConnectedDisplayIdentifiers = GetAllConnectedDisplayIdentifiers();
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Trace(ex, $"WinLibrary/UpdateActiveConfig: Exception updating the currently active config");
                return false;
            }

            return true;
        }

        public WINDOWS_DISPLAY_CONFIG GetActiveConfig(bool fastScan = true)
        {
            SharedLogger.logger.Trace($"WinLibrary/GetActiveConfig: Getting the currently active config");
            // We'll leave virtual refresh rate aware until we can reliably detect Windows 11 versions.
            return GetWindowsDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, fastScan);
        }

        private WINDOWS_DISPLAY_CONFIG GetWindowsDisplayConfig(QDC selector = QDC.QDC_ONLY_ACTIVE_PATHS, bool fastScan = true)
        {

            // Forcing fastscan to stop the taskbar location scanning delaying the user experience
            // TODO: Find a replacement method of doing the taskbar location detection. Microsoft may have made things easier in Windows 11 by now....
            fastScan = true;

            // Prepare the empty windows display config
            WINDOWS_DISPLAY_CONFIG windowsDisplayConfig = CreateDefaultConfig();

            // Get the size of the largest Active Paths and Modes arrays
            SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the size of the largest Active Paths and Modes arrays");
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(selector, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(selector, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(selector, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(selector, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
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


            // First of all generate the current displayIdentifiers
            windowsDisplayConfig.DisplayIdentifiers = GetCurrentDisplayIdentifiers();

            // Next, extract the UID entries for the displays as that's what the Path IDs are normally supposed to be
            // This is how we know the actual target id's ofd the monitors currently connected
            Regex rx = new Regex(@"UID(?<uid>\d+)#", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            HashSet<uint> physicalTargetIdsAvailable = new HashSet<uint>();
            foreach (string displayIdentifier in windowsDisplayConfig.DisplayIdentifiers)
            {
                MatchCollection mc = rx.Matches(displayIdentifier);
                if (mc.Count > 0)
                {
                    physicalTargetIdsAvailable.Add(UInt32.Parse(mc[0].Groups["uid"].Value));
                }
            }

            // Now cycle through the paths and grab the state information we need
            // and map the adapter name to adapter id
            // and populate the display source information

            // Set the DPI awareness for the process this thread is running within so that the DPI calls return the right values
            CCDImport.SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);

            List<uint> targetPathIdsToChange = new List<uint>();
            List<uint> targetModeIdsToChange = new List<uint>();
            List<uint> targetIdsFound = new List<uint>();
            List<uint> replacementIds = new List<uint>();
            bool isClonedProfile = false;
            for (int i = 0; i < paths.Length; i++)
            {

                if (selector == QDC.QDC_ONLY_ACTIVE_PATHS && paths[i].TargetInfo.TargetInUse == false)
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Skipping display target {paths[i].TargetInfo.Id} as we only want displays currently in use");
                    continue;
                }

                if (selector == QDC.QDC_ALL_PATHS && paths[i].TargetInfo.TargetAvailable == false)
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Skipping display target {paths[i].TargetInfo.Id} as we want all available displays and this one isn't available");
                    continue;
                }

                //bool gotSourceDeviceName = false;
                //bool gotAdapterName = false;
                bool gotAdvancedColorInfo = false;
                bool gotSdrWhiteLevel = false;

                // Figure out if this path has a physical targetId, and if it doesn't store it
                if (physicalTargetIdsAvailable.Contains(paths[i].TargetInfo.Id))
                {
                    targetIdsFound.Add(paths[i].TargetInfo.Id);
                }
                else
                {
                    // Add to the list of physical path target ids we need to patch later
                    targetPathIdsToChange.Add(paths[i].TargetInfo.Id);
                }

                // Track if this display is a cloned path
                bool isClonedPath = false;

                // Get the Windows Scaling DPI per display
                DPIScalingInfo sourceDPIScalingInfo = GetDPISettings(paths[i].SourceInfo.AdapterId, paths[i].SourceInfo.Id, paths[i].TargetInfo.Id);

                // get display source name
                var sourceInfo = new DISPLAYCONFIG_SOURCE_DEVICE_NAME();
                sourceInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SOURCE_NAME;
                sourceInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SOURCE_DEVICE_NAME>();
                sourceInfo.Header.AdapterId = paths[i].SourceInfo.AdapterId;
                sourceInfo.Header.Id = paths[i].SourceInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref sourceInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {

                    //gotSourceDeviceName = true;
                    // Store it for later
                    if (windowsDisplayConfig.DisplaySources.ContainsKey(sourceInfo.ViewGdiDeviceName))
                    {
                        // We already have at least one display using this source, so we need to add the other cloned display to the existing list
                        DISPLAY_SOURCE ds = new DISPLAY_SOURCE();
                        ds.AdapterId = paths[i].SourceInfo.AdapterId;
                        ds.SourceId = paths[i].SourceInfo.Id;
                        ds.TargetId = paths[i].TargetInfo.Id;
                        ds.SourceDPIScalingInfo = sourceDPIScalingInfo;
                        windowsDisplayConfig.DisplaySources[sourceInfo.ViewGdiDeviceName].Add(ds);
                        isClonedPath = true;
                        isClonedProfile = true;
                        windowsDisplayConfig.IsCloned = true;
                    }
                    else
                    {
                        // This is the first display to use this source
                        List<DISPLAY_SOURCE> sources = new List<DISPLAY_SOURCE>();
                        DISPLAY_SOURCE ds = new DISPLAY_SOURCE();
                        ds.AdapterId = paths[i].SourceInfo.AdapterId;
                        ds.SourceId = paths[i].SourceInfo.Id;
                        ds.TargetId = paths[i].TargetInfo.Id;
                        ds.SourceDPIScalingInfo = sourceDPIScalingInfo;
                        sources.Add(ds);
                        windowsDisplayConfig.DisplaySources.Add(sourceInfo.ViewGdiDeviceName, sources);
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

                /*// Get adapter ID for later
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
                        //gotAdapterName = true;
                        // Store it for later
                        windowsDisplayConfig.DisplayAdapters.Add(paths[i].TargetInfo.AdapterId.Value, adapterInfo.AdapterDevicePath);
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found adapter name {adapterInfo.AdapterDevicePath} for adapter {paths[i].TargetInfo.AdapterId.Value}.");
                    }
                    else
                    {
                        SharedLogger.logger.Error($"WinLibrary/GetWindowsDisplayConfig: ERROR - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to query the adapter name for adapter {paths[i].TargetInfo.AdapterId.Value}.");
                    }
                }
                else
                {
                    // We already have the adapter name
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: We already have the adapter name {windowsDisplayConfig.DisplayAdapters[paths[i].TargetInfo.AdapterId.Value]} for adapter {paths[i].TargetInfo.AdapterId.Value} so skipping storing it.");
                    //gotAdapterName = true;
                }*/


                // Get advanced color info
                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Attempting to get advanced color info for display {paths[i].TargetInfo.Id}.");

                // We need to skip recording anything from a connection that doesn't support color communication
                if (!SkippedColorConnectionTypes.Contains(paths[i].TargetInfo.OutputTechnology))
                {
                    var colorInfo = new DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO();
                    colorInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_ADVANCED_COLOR_INFO;
                    colorInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_GET_ADVANCED_COLOR_INFO>();
                    colorInfo.Header.AdapterId = paths[i].TargetInfo.AdapterId;
                    colorInfo.Header.Id = paths[i].TargetInfo.Id;
                    err = CCDImport.DisplayConfigGetDeviceInfo(ref colorInfo);
                    if (err == WIN32STATUS.ERROR_SUCCESS)
                    {
                        gotAdvancedColorInfo = true;
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
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Attempting to get SDR white levels for display {paths[i].TargetInfo.Id}.");
                    var whiteLevelInfo = new DISPLAYCONFIG_SDR_WHITE_LEVEL();
                    whiteLevelInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_SDR_WHITE_LEVEL;
                    whiteLevelInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_SDR_WHITE_LEVEL>();
                    whiteLevelInfo.Header.AdapterId = paths[i].TargetInfo.AdapterId;
                    whiteLevelInfo.Header.Id = paths[i].TargetInfo.Id;
                    err = CCDImport.DisplayConfigGetDeviceInfo(ref whiteLevelInfo);
                    if (err == WIN32STATUS.ERROR_SUCCESS)
                    {
                        gotSdrWhiteLevel = true;
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Found SDR White levels for display {paths[i].TargetInfo.Id}.");
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: WARNING - Unabled to get SDR White levels for display {paths[i].TargetInfo.Id}.");
                    }

                    // Only create and add the ADVANCED_HDR_INFO_PER_PATH if the info is there
                    if (gotAdvancedColorInfo)
                    {
                        ADVANCED_HDR_INFO_PER_PATH hdrInfo = new ADVANCED_HDR_INFO_PER_PATH();
                        hdrInfo.AdapterId = paths[i].TargetInfo.AdapterId;
                        hdrInfo.Id = paths[i].TargetInfo.Id;
                        hdrInfo.AdvancedColorInfo = colorInfo;
                        if (gotSdrWhiteLevel)
                        {
                            hdrInfo.SDRWhiteLevel = whiteLevelInfo;
                        }
                        windowsDisplayConfig.DisplayHDRStates.Add(hdrInfo);
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Skipping getting HDR and SDR White levels information as display {paths[i].TargetInfo.Id} uses a {paths[i].TargetInfo.OutputTechnology} connector that doesn't support HDR.");
                }
            }

            // Set the DPI awareness for the process this thread is running within so that the DPI calls return the right values
            CCDImport.SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED);

            // Get all the DisplayAdapters currently in the system
            // This will be used for windows to translate the adapter details beween reboots
            windowsDisplayConfig.DisplayAdapters = GetAllAdapterIDs();

            // Go through the list of physicalTargetIdsAvailable
            // ignore the ones that were found
            // if one was not found, then
            // go through the modes
            // patch the target
            if (isClonedProfile)
            {
                // Figure out which available displays are unused (in path priority order)
                foreach (var physicalTargetId in physicalTargetIdsAvailable)
                {
                    if (!targetIdsFound.Contains(physicalTargetId))
                    {
                        // this is a candidate physical target id to use as a replacement
                        replacementIds.Add(physicalTargetId);
                    }
                }

                // Now go through and figure out a mapping of old target id to new replacement id
                Dictionary<uint, uint> targetIdMap = new Dictionary<uint, uint>();
                for (int i = 0; i < targetPathIdsToChange.Count; i++)
                {
                    uint targetPathId = targetPathIdsToChange[i];
                    if (i < replacementIds.Count)
                    {
                        targetIdMap[targetPathId] = replacementIds[i];
                    }
                }


                // Now we need to go through the list of paths again and patch the 'cloned' displays with a real display ID so the config works
                for (int i = 0; i < paths.Length; i++)
                {
                    if (targetIdMap.ContainsKey(paths[i].TargetInfo.Id))
                    {
                        // Patch the cloned ids with a real working one!
                        paths[i].TargetInfo.Id = targetIdMap[paths[i].TargetInfo.Id];
                    }
                }

                // And then we need to go through the list of modes again and patch the 'cloned' displays with a real display ID so the display layout is right in cloned displays
                for (int i = 0; i < modes.Length; i++)
                {
                    // We only change the ids that match in InfoType for target displays
                    if (modes[i].InfoType == DISPLAYCONFIG_MODE_INFO_TYPE.DISPLAYCONFIG_MODE_INFO_TYPE_TARGET && targetIdMap.ContainsKey(modes[i].Id))
                    {
                        // Patch the cloned ids with a real working one!
                        modes[i].Id = targetIdMap[modes[i].Id];
                    }
                }

                // And then we need to go through the list of display sources and patch the 'cloned' displays with a real display ID so the display layout is right in cloned displays
                for (int i = 0; i < windowsDisplayConfig.DisplaySources.Count; i++)
                {
                    string key = windowsDisplayConfig.DisplaySources.ElementAt(i).Key;
                    DISPLAY_SOURCE[] dsList = windowsDisplayConfig.DisplaySources.ElementAt(i).Value.ToArray();
                    for (int j = 0; j < dsList.Length; j++)
                    {
                        // We only change the ids that match in InfoType for target displays
                        if (targetIdMap.ContainsKey(dsList[j].TargetId))
                        {
                            // Patch the cloned ids with a real working one!
                            dsList[j].TargetId = targetIdMap[dsList[j].TargetId];

                        }
                    }
                    windowsDisplayConfig.DisplaySources[key] = dsList.ToList();
                }
            }

            // Now we need to find the DevicePaths for the DisplaySources (as at this point the cloned display sources have been corrected)
            for (int i = 0; i < windowsDisplayConfig.DisplaySources.Count; i++)
            {
                string key = windowsDisplayConfig.DisplaySources.ElementAt(i).Key;
                DISPLAY_SOURCE[] dsList = windowsDisplayConfig.DisplaySources.ElementAt(i).Value.ToArray();
                for (int j = 0; j < dsList.Length; j++)
                {
                    // get display target name
                    var targetInfo = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
                    targetInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                    targetInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
                    targetInfo.Header.AdapterId = dsList[j].AdapterId;
                    targetInfo.Header.Id = dsList[j].TargetId;
                    err = CCDImport.DisplayConfigGetDeviceInfo(ref targetInfo);
                    if (err == WIN32STATUS.ERROR_SUCCESS)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Successfully got the target info from {dsList[j].TargetId}.");
                        dsList[j].DevicePath = targetInfo.MonitorDevicePath;
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"WinLibrary/GetSomeDisplayIdentifiers: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{dsList[j].TargetId}");
                    }
                }
                windowsDisplayConfig.DisplaySources[key] = dsList.ToList();
            }


            /*Dictionary<string, TaskBarLayout> taskBarStuckRectangles = new Dictionary<string, TaskBarLayout>();

            // Now attempt to get the windows taskbar location for each display
            SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Attempting to get the Windows Taskbar Layouts.");
            bool retryNeeded = false;
            taskBarStuckRectangles = TaskBarLayout.GetAllCurrentTaskBarLayouts(windowsDisplayConfig.DisplaySources, out retryNeeded);
            // Check whether Windows has actually added the registry keys that outline the taskbar position
            if (retryNeeded)
            {
                if (fastScan)
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Skipping retrying Windows Taskbar Layouts and just accepting the first locations");
                }
                else
                {
                    // We wait until the reg key is populated                
                    for (int count = 1; count <= 4; count++)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: We were unable to get all the Windows Taskbar Layouts! So we need to try again. Attempt {count} of 4.");

                        // Wait 5 seconds
                        System.Threading.Thread.Sleep(5000);
                        // then try again
                        retryNeeded = false;
                        taskBarStuckRectangles = TaskBarLayout.GetAllCurrentTaskBarLayouts(windowsDisplayConfig.DisplaySources, out retryNeeded);

                        if (!retryNeeded)
                        {
                            SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: We successfully got the Windows Taskbar Layouts on attempt {count}! So we can stop trying to get them");
                            break;
                        }

                    }
                }
            }

            // Now we try to get the taskbar settings too
            SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Attempting to get the Windows Taskbar Settings.");
            TaskBarSettings taskBarSettings = TaskBarSettings.GetCurrent();*/

            // Store the active paths and modes in our display config object
            windowsDisplayConfig.DisplayConfigPaths = paths;
            windowsDisplayConfig.DisplayConfigModes = modes;
            windowsDisplayConfig.GdiDisplaySettings = GetGdiDisplaySettings();
            //windowsDisplayConfig.TaskBarLayout = taskBarStuckRectangles;
            //windowsDisplayConfig.TaskBarSettings = taskBarSettings;

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
                SharedLogger.logger.Trace($"WinLibrary/GetGdiDisplaySettings: Getting the current Display Settings for {displayDevice.DeviceName}");
                if (displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.AttachedToDesktop) || displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.MultiDriver))
                {
                    // If the display device is attached to the Desktop, or a type of display that is represented by a psudeo mirroring application, then skip this display
                    // e.g. some sort of software interfaced display that doesn't have a physical plug, or maybe uses USB for communication
                    SharedLogger.logger.Trace($"WinLibrary/GetGdiDisplaySettings: Getting the current Display Settings for {displayDevice.DeviceName}");
                    DEVICE_MODE currentMode = new DEVICE_MODE();
                    currentMode.Size = (UInt16)Marshal.SizeOf<DEVICE_MODE>();
                    bool gdiWorked = GDIImport.EnumDisplaySettings(displayDevice.DeviceName, DISPLAY_SETTINGS_MODE.CurrentSettings, ref currentMode);
                    if (gdiWorked)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetGdiDisplaySettings: Got the current Display Settings from display {displayDevice.DeviceName}.");
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
                        SharedLogger.logger.Warn($"WinLibrary/GetGdiDisplaySettings: WARNING - Unabled to get current display mode settings from display {displayDevice.DeviceName}.");
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetGdiDisplaySettings: The display {displayDevice.DeviceName} is either not attached to the desktop, or is not a mirroring driver. Skipping this display device as we cannot use it.");
                }

                displayDeviceNum++;
            }
            return gdiDeviceSettings;
        }

        public static Dictionary<string, List<uint>> GetDisplaySourceNames()
        {
            // Get the size of the largest Active Paths and Modes arrays
            SharedLogger.logger.Trace($"WinLibrary/GetDisplaySourceNames: Getting the size of the largest Active Paths and Modes arrays");
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetDisplaySourceNames: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"WinLibrary/GetDisplaySourceNames: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetDisplaySourceNames: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"WinLibrary/GetDisplaySourceNames: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ONLY_ACTIVE_PATHS, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetDisplaySourceNames: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"WinLibrary/GetDisplaySourceNames: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(QDC.QDC_ONLY_ACTIVE_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetDisplaySourceNames: ERROR - The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                    throw new WinLibraryException($"The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                }
                else if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetDisplaySourceNames: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again");
                    throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again.");
                }
            }
            else if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetDisplaySourceNames: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays");
                throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays.");
            }

            // Prepare the empty DisplaySources dictionary
            Dictionary<string, List<uint>> DisplaySources = new Dictionary<string, List<uint>>();

            // Now cycle through the paths and grab the HDR state information
            // and map the adapter name to adapter id
            //var hdrInfos = new ADVANCED_HDR_INFO_PER_PATH[pathCount];
            //int hdrInfoCount = 0;
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

                    SharedLogger.logger.Trace($"WinLibrary/GetDisplaySourceNames: Found Display Source {sourceInfo.ViewGdiDeviceName} for source {path.SourceInfo.Id}.");
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetDisplaySourceNames: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the source info for source adapter #{path.SourceInfo.AdapterId}");
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
            WINDOWS_DISPLAY_CONFIG displayConfig = ActiveDisplayConfig;

            WIN32STATUS err = WIN32STATUS.ERROR_GEN_FAILURE;
            stringToReturn += $"****** WINDOWS CCD CONFIGURATION *******\n";
            stringToReturn += $"Display profile contains cloned screens: {displayConfig.IsCloned}\n";
            stringToReturn += $"\n";

            // Get the size of the largest Active Paths and Modes arrays
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
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the adapter device path for target #{path.TargetInfo.AdapterId}");
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
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Target Preferred Width {targetPreferredInfo.Width} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Target Preferred Height {targetPreferredInfo.Height} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Target Video Signal Info Active Size: ({targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ActiveSize.Cx}x{targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ActiveSize.Cy} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Target Video Signal Info Total Size: ({targetPreferredInfo.TargetMode.TargetVideoSignalInfo.TotalSize.Cx}x{targetPreferredInfo.TargetMode.TargetVideoSignalInfo.TotalSize.Cy} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Target Video Signal Info HSync Frequency: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.HSyncFreq} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Target Video Signal Info VSync Frequency: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.VSyncFreq} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Target Video Signal Info Pixel Rate: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.PixelRate} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Target Video Signal Info Scan Line Ordering: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.ScanLineOrdering} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Target Video Signal Info Video Standard: {targetPreferredInfo.TargetMode.TargetVideoSignalInfo.VideoStandard} for target {path.TargetInfo.Id}.");

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
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the preferred target name for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Virtual Resolution is Disabled: {targetBaseTypeInfo.BaseOutputTechnology} for target {path.TargetInfo.Id}.");

                    stringToReturn += $"****** Interrogating Target Base Type for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" Base Output Technology: {targetBaseTypeInfo.BaseOutputTechnology}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target base type for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Base Output Technology: {supportVirtResInfo.IsMonitorVirtualResolutionDisabled} for target {path.TargetInfo.Id}.");
                    stringToReturn += $"****** Interrogating Target Supporting virtual resolution for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" Virtual Resolution is Disabled: {supportVirtResInfo.IsMonitorVirtualResolutionDisabled}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out the virtual resolution support for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Advanced Color Supported: {colorInfo.AdvancedColorSupported} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Advanced Color Enabled: {colorInfo.AdvancedColorEnabled} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Advanced Color Force Disabled: {colorInfo.AdvancedColorForceDisabled} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Bits per Color Channel: {colorInfo.BitsPerColorChannel} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Color Encoding: {colorInfo.ColorEncoding} for target {path.TargetInfo.Id}.");
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found Wide Color Enforced: {colorInfo.WideColorEnforced} for target {path.TargetInfo.Id}.");

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
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out the virtual resolution support for display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Found SDR White Level: {whiteLevelInfo.SDRWhiteLevel} for target {path.TargetInfo.Id}.");

                    stringToReturn += $"****** Interrogating SDR White Level for Display {path.TargetInfo.Id} *******\n";
                    stringToReturn += $" SDR White Level: {whiteLevelInfo.SDRWhiteLevel}\n";
                    stringToReturn += $"\n";
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out the SDL white level for display #{path.TargetInfo.Id}");
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


                SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Getting the current Display Settings for {displayDevice.DeviceName}");
                if (displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.AttachedToDesktop) || displayDevice.StateFlags.HasFlag(DISPLAY_DEVICE_STATE_FLAGS.MultiDriver))
                {
                    // If the display device is attached to the Desktop, or a type of display that is represented by a psudeo mirroring application, then skip this display
                    // e.g. some sort of software interfaced display that doesn't have a physical plug, or maybe uses USB for communication
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Getting the current Display Settings for {displayDevice.DeviceName}");
                    stringToReturn += $" Display Device Settings for attached Display {displayDevice.DeviceName} :\n";
                    DEVICE_MODE currentMode = new DEVICE_MODE();
                    currentMode.Size = (UInt16)Marshal.SizeOf<DEVICE_MODE>();
                    bool gdiWorked = GDIImport.EnumDisplaySettings(displayDevice.DeviceName, DISPLAY_SETTINGS_MODE.CurrentSettings, ref currentMode);
                    if (gdiWorked)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: Got the current Display Settings from display {displayDevice.DeviceName}.");
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
                        SharedLogger.logger.Warn($"WinLibrary/PrintActiveConfig: WARNING - Unabled to get current display mode settings from display {displayDevice.DeviceName}.");
                        stringToReturn += $" No display settings found.\n\n";
                    }
                }
                else
                {
                    SharedLogger.logger.Trace($"WinLibrary/PrintActiveConfig: The display {displayDevice.DeviceName} is either not attached to the desktop, or is not a mirroring driver. Skipping this display device as we cannot use it.");
                }

                displayDeviceNum++;
            }

            return stringToReturn;
        }

        public bool WakeUpAllDisplays()
        {
            // Poke all monitors using DDC/CI to wake them up
            DdcCiHelper.PokeAllMonitors();

            Thread.Sleep(100);

            // Attempt to wake any displays that are asleep by emulaing a Ctrl + Shift + Windows key + B kjeypress to reset the windows graphic display driver.
            // This is a workaround for a bug in Windows 10 where the display driver can sometimes go to sleep and not wake up. Here's what it does:
            // - It calls DxgkDdiResetFromTimeout() internally.
            // - Resets the GPU driver stack(WDDM).
            // - Reinitialises the display pipeline.
            // - Can "wake" sleeping or non - responding displays, including those with bad EDID or DP handshake issues.
            GDIImport.ResetGraphicsStack();

            Thread.Sleep(200);
            // Also reapply the current configuration to just wake up any monitors that are currently asleep.
            //CCDImport.SetDisplayConfig(0, null, 0, null, SDC.SDC_APPLY | SDC.SDC_USE_DATABASE_CURRENT);


            return true;
        }

        public bool SetActiveConfig(WINDOWS_DISPLAY_CONFIG displayConfig)
        {
            //bool needToRestartExplorer = false;

            // Get the all possible windows display configs
            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Generating a list of all the current display configs");
            WINDOWS_DISPLAY_CONFIG allWindowsDisplayConfig = GetWindowsDisplayConfig(QDC.QDC_ALL_PATHS);

            if (displayConfig.IsCloned)
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: We have a cloned display in this display profile");
            }
            else
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: We have no cloned displays in thus display profile");
            }

            // Now we go through the Paths to update the LUIDs as per Soroush's suggestion
            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Patching the adapter IDs to make the saved config valid");
            PatchWindowsDisplayConfig(ref displayConfig);

            uint myPathsCount = (uint)displayConfig.DisplayConfigPaths.Length;
            uint myModesCount = (uint)displayConfig.DisplayConfigModes.Length;

            // Now set the specified display configuration for this computer                    
            WIN32STATUS err = CCDImport.SetDisplayConfig(myPathsCount, displayConfig.DisplayConfigPaths, myModesCount, displayConfig.DisplayConfigModes, SDC.DISPLAYMAGICIAN_SET | SDC.SDC_FORCE_MODE_ENUMERATION);
            if (err == WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Successfully set the display configuration to the settings supplied!");
            }
            else if (err == WIN32STATUS.ERROR_INVALID_PARAMETER)
            {
                SharedLogger.logger.Warn($"WinLibrary/SetActiveConfig: The combination of parameters and flags specified is invalid. Display configuration not applied. So trying again without SDC_FORCE_MODE_ENUMERATION as that works on some computers.");
                // Try it again, because in some systems it doesn't work at the first try
                err = CCDImport.SetDisplayConfig(myPathsCount, displayConfig.DisplayConfigPaths, myModesCount, displayConfig.DisplayConfigModes, SDC.DISPLAYMAGICIAN_SET);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Retry. Successfully set the display configuration to the settings supplied!");
                }
                else if (err == WIN32STATUS.ERROR_INVALID_PARAMETER)
                {
                    SharedLogger.logger.Warn($"WinLibrary/SetActiveConfig: Retry. The combination of parameters and flags specified is invalid. Display configuration not applied. So trying again without any specific data other than the topology as that works on some computers.");
                    // Try it again, because in some systems it doesn't work at the 2nd try! This is a fallback mode just to get something on the screen!
                    err = CCDImport.SetDisplayConfig(myPathsCount, displayConfig.DisplayConfigPaths, myModesCount, displayConfig.DisplayConfigModes, SDC.SDC_APPLY | SDC.SDC_TOPOLOGY_SUPPLIED | SDC.SDC_ALLOW_CHANGES | SDC.SDC_ALLOW_PATH_ORDER_CHANGES);
                    if (err == WIN32STATUS.ERROR_SUCCESS)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Retry 2. Successfully set the display configuration to the settings supplied!");
                    }
                    else if (err == WIN32STATUS.ERROR_INVALID_PARAMETER)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry 2. The combination of parameters and flags specified is invalid. Display configuration not applied.");
                        return false;
                    }
                    else if (err == WIN32STATUS.ERROR_NOT_SUPPORTED)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry 2. The system is not running a graphics driver that was written according to the Windows Display Driver Model (WDDM). The function is only supported on a system with a WDDM driver running. Display configuration not applied.");
                        return false;
                    }
                    else if (err == WIN32STATUS.ERROR_ACCESS_DENIED)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry 2. The caller does not have access to the console session. This error occurs if the calling process does not have access to the current desktop or is running on a remote session. Display configuration not applied.");
                        return false;
                    }
                    else if (err == WIN32STATUS.ERROR_GEN_FAILURE)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry 2. An unspecified error occurred. Display configuration not applied.");
                        return false;
                    }
                    else if (err == WIN32STATUS.ERROR_BAD_CONFIGURATION)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry 2. The function could not find a workable solution for the source and target modes that the caller did not specify. Display configuration not applied.");
                        return false;
                    }
                    else
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry 2. SetDisplayConfig couldn't set the display configuration using the settings supplied. Display configuration not applied.");
                        return false;
                    }
                }
                else if (err == WIN32STATUS.ERROR_NOT_SUPPORTED)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry. The system is not running a graphics driver that was written according to the Windows Display Driver Model (WDDM). The function is only supported on a system with a WDDM driver running. Display configuration not applied.");
                    return false;
                }
                else if (err == WIN32STATUS.ERROR_ACCESS_DENIED)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry. The caller does not have access to the console session. This error occurs if the calling process does not have access to the current desktop or is running on a remote session. Display configuration not applied.");
                    return false;
                }
                else if (err == WIN32STATUS.ERROR_GEN_FAILURE)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry. An unspecified error occurred. Display configuration not applied.");
                    return false;
                }
                else if (err == WIN32STATUS.ERROR_BAD_CONFIGURATION)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry. The function could not find a workable solution for the source and target modes that the caller did not specify. Display configuration not applied.");
                    return false;
                }
                else
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Retry. SetDisplayConfig couldn't set the display configuration using the settings supplied. Display configuration not applied.");
                    return false;
                }
            }
            else if (err == WIN32STATUS.ERROR_NOT_SUPPORTED)
            {
                SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The system is not running a graphics driver that was written according to the Windows Display Driver Model (WDDM). The function is only supported on a system with a WDDM driver running. Display configuration not applied.");
                return false;
            }
            else if (err == WIN32STATUS.ERROR_ACCESS_DENIED)
            {
                SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The caller does not have access to the console session. This error occurs if the calling process does not have access to the current desktop or is running on a remote session. Display configuration not applied.");
                return false;
            }
            else if (err == WIN32STATUS.ERROR_GEN_FAILURE)
            {
                SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: An unspecified error occurred. Display configuration not applied.");
                return false;
            }
            else if (err == WIN32STATUS.ERROR_BAD_CONFIGURATION)
            {
                SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The function could not find a workable solution for the source and target modes that the caller did not specify. Display configuration not applied.");
                return false;
            }
            else
            {
                SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: SetDisplayConfig couldn't set the display configuration using the settings supplied. Display configuration not applied.");
                return false;
            }

            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: SUCCESS! The display configuration has been successfully applied");

            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Waiting 0.1 second to let the display change take place before adjusting the Windows CCD Source DPI scaling settings");
            System.Threading.Thread.Sleep(100);

            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Attempting to set Windows DPI Scaling setting for display sources.");
            CCDImport.SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
            foreach (var displaySourceEntry in displayConfig.DisplaySources)
            {
                // We only need to set the source on the first display source
                // Set the Windows Scaling DPI per source
                if (SetDPISettings(displaySourceEntry.Value[0].AdapterId, displaySourceEntry.Value[0].SourceId, displaySourceEntry.Value[0].TargetId, displaySourceEntry.Value[0].SourceDPIScalingInfo))
                {
                    SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Set the DPI scaling settings for display source {displaySourceEntry.Value[0].SourceId}");
                }
                else
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: ERROR - Unable to set the DPI scaling settings for display source {displaySourceEntry.Value[0].SourceId}");
                    return false;
                }

            }
            CCDImport.SetProcessDpiAwarenessContext(DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_UNAWARE_GDISCALED);


            // NOTE: There is currently no way within Windows CCD API to set the HDR settings to any particular setting
            // This code will only turn on the HDR setting.
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

                    if (myHDRstate.AdvancedColorInfo.AdvancedColorEnabled != colorInfo.AdvancedColorEnabled)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: HDR is available for use on Display {myHDRstate.Id}, and we want it set to {myHDRstate.AdvancedColorInfo.BitsPerColorChannel} but is currently {colorInfo.AdvancedColorEnabled}.");


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
                        SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Skipping setting HDR on Display {myHDRstate.Id} as it is already in the correct HDR mode: {colorInfo.AdvancedColorEnabled}");
                    }
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/SetActiveConfig: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to find out if HDR is supported for display #{myHDRstate.Id}");
                }

            }

/*
            // Get the existing displays config
            Dictionary<string, GDI_DISPLAY_SETTING> currentGdiDisplaySettings = GetGdiDisplaySettings();

            // Apply the previously saved display settings to the new displays (match them up)
            // NOTE: This may be the only mode needed once it's completed.
            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Attempting to change Display Device settings through GDI API using ChangeDisplaySettingsEx");
            bool appliedGdiDisplaySettings = false;
            foreach (var gdiDisplay in displayConfig.GdiDisplaySettings)
            {

                string displayDeviceKey = gdiDisplay.Key;
                GDI_DISPLAY_SETTING displayDeviceSettings = displayConfig.GdiDisplaySettings[displayDeviceKey];

                if (currentGdiDisplaySettings.ContainsKey(displayDeviceKey))
                {
                    SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Trying to change Device Mode for Display {displayDeviceKey}.");
                    GDI_DISPLAY_SETTING currentDeviceSetting = currentGdiDisplaySettings[displayDeviceKey];

                    // Use the current device as a base, but set some of the various settings we stored as part of the profile 
                    currentDeviceSetting.DeviceMode.BitsPerPixel = displayDeviceSettings.DeviceMode.BitsPerPixel;
                    currentDeviceSetting.DeviceMode.DisplayOrientation = displayDeviceSettings.DeviceMode.DisplayOrientation;
                    currentDeviceSetting.DeviceMode.DisplayFrequency = displayDeviceSettings.DeviceMode.DisplayFrequency;
                    // Sets the greyscale and interlaced settings
                    currentDeviceSetting.DeviceMode.DisplayFlags = displayDeviceSettings.DeviceMode.DisplayFlags;

                    SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Testing whether the GDI Device Mode will work for display {displayDeviceKey}.");
                    // First of all check that setting the GDI mode will work
                    CHANGE_DISPLAY_RESULTS result = GDIImport.ChangeDisplaySettingsEx(currentDeviceSetting.Device.DeviceName, ref currentDeviceSetting.DeviceMode, IntPtr.Zero, CHANGE_DISPLAY_SETTINGS_FLAGS.CDS_TEST, IntPtr.Zero);
                    if (result == CHANGE_DISPLAY_RESULTS.Successful)
                    {
                        SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Success. The GDI Device Mode will work for display {displayDeviceKey}.");
                        // Set the 
                        if (currentDeviceSetting.IsPrimary)
                        {
                            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Actually going to try to set the GDI Device Mode for display {displayDeviceKey} now (primary display).");
                            result = GDIImport.ChangeDisplaySettingsEx(currentDeviceSetting.Device.DeviceName, ref currentDeviceSetting.DeviceMode, IntPtr.Zero, (CHANGE_DISPLAY_SETTINGS_FLAGS.CDS_SET_PRIMARY | CHANGE_DISPLAY_SETTINGS_FLAGS.CDS_UPDATEREGISTRY | CHANGE_DISPLAY_SETTINGS_FLAGS.CDS_NORESET), IntPtr.Zero);
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Actually going to try to set the GDI Device Mode for display {displayDeviceKey} now (secondary display).");
                            result = GDIImport.ChangeDisplaySettingsEx(currentDeviceSetting.Device.DeviceName, ref currentDeviceSetting.DeviceMode, IntPtr.Zero, (CHANGE_DISPLAY_SETTINGS_FLAGS.CDS_UPDATEREGISTRY | CHANGE_DISPLAY_SETTINGS_FLAGS.CDS_NORESET), IntPtr.Zero);

                        }
                        if (result == CHANGE_DISPLAY_RESULTS.Successful)
                        {
                            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Successfully changed display {displayDeviceKey} to use the new mode!");
                            appliedGdiDisplaySettings = true;
                        }
                        else if (result == CHANGE_DISPLAY_RESULTS.BadDualView)
                        {
                            SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The settings change was unsuccessful because the system is DualView capable. Display {displayDeviceKey} not updated to new mode.");
                            //return false;
                        }
                        else if (result == CHANGE_DISPLAY_RESULTS.BadFlags)
                        {
                            SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: An invalid set of flags was passed in. Display {displayDeviceKey} not updated to use the new mode.");
                            //return false;
                        }
                        else if (result == CHANGE_DISPLAY_RESULTS.BadMode)
                        {
                            SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The graphics mode is not supported. Display {displayDeviceKey} not updated to use the new mode.");
                            //return false;
                        }
                        else if (result == CHANGE_DISPLAY_RESULTS.BadParam)
                        {
                            SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: An invalid parameter was passed in. This can include an invalid flag or combination of flags. Display {displayDeviceKey} not updated to use the new mode.");
                            //return false;
                        }
                        else if (result == CHANGE_DISPLAY_RESULTS.Failed)
                        {
                            SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The display driver failed to apply the specified graphics mode. Display {displayDeviceKey} not updated to use the new mode.");
                            //return false;
                        }
                        else if (result == CHANGE_DISPLAY_RESULTS.NotUpdated)
                        {
                            SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Unable to write new settings to the registry. Display {displayDeviceKey} not updated to use the new mode.");
                            //return false;
                        }
                        else if (result == CHANGE_DISPLAY_RESULTS.Restart)
                        {
                            SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The computer must be restarted for the graphics mode to work. Display {displayDeviceKey} not updated to use the new mode.");
                            //return false;
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Unknown error while trying to change Display {displayDeviceKey} to use the new mode.");
                            return false;
                        }
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.BadDualView)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The GDI mode change would be unsuccessful because the system is DualView capable. Skipping setting Display {displayDeviceKey}.");
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.BadFlags)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The GDI mode change would be unsuccessful because an invalid set of flags was passed in. Display {displayDeviceKey} not updated to use the new mode.");
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.BadMode)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The GDI mode change would be unsuccessful because the graphics mode is not supported. Display {displayDeviceKey} not updated to use the new mode.");
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.BadParam)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The GDI mode change would be unsuccessful because an invalid parameter was passed in. This can include an invalid flag or combination of flags. Display {displayDeviceKey} not updated to use the new mode.");
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.Failed)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The GDI mode change would be unsuccessful because the display driver failed to apply the specified graphics mode. Display {displayDeviceKey} not updated to use the new mode.");
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.NotUpdated)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The GDI mode change would be unsuccessful because we're unable to write new settings to the registry. Display {displayDeviceKey} not updated to use the new mode.");
                    }
                    else if (result == CHANGE_DISPLAY_RESULTS.Restart)
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The GDI mode change would be unsuccessful because the computer must be restarted for the graphics mode to work. Display {displayDeviceKey} not updated to use the new mode.");
                    }
                    else
                    {
                        SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: The GDI mode change would be unsuccessful because there was an unknown error testing if Display {displayDeviceKey} could use the new mode.");
                    }
                }
                else
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Display {displayDeviceKey} is not currently in use, so cannot set it!");
                }

            }

            // If we have applied GDI settings for multiple displays, then we need to run ChangeDisplaySettingsEx one more time
            // see https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-changedisplaysettingsexa
            if (appliedGdiDisplaySettings)
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Other display settings were changed, so applying all the changes now.");
                CHANGE_DISPLAY_RESULTS result = GDIImport.ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, CHANGE_DISPLAY_SETTINGS_FLAGS.CDS_NONE, IntPtr.Zero);
                if (result == CHANGE_DISPLAY_RESULTS.Successful)
                {
                    SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Successfully applied the new GDI modes!");
                }
                else if (result == CHANGE_DISPLAY_RESULTS.BadDualView)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Couldn't apply the new GDI modes because the system is DualView capable.");
                    return false;
                }
                else if (result == CHANGE_DISPLAY_RESULTS.BadFlags)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Couldn't apply the new GDI modes because an invalid set of flags was passed in.");
                    return false;
                }
                else if (result == CHANGE_DISPLAY_RESULTS.BadMode)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Couldn't apply the new GDI modes because the graphics mode is not supported.");
                    return false;
                }
                else if (result == CHANGE_DISPLAY_RESULTS.BadParam)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Couldn't apply the new GDI modes because an invalid parameter was passed in. This can include an invalid flag or combination of flags.");
                    return false;
                }
                else if (result == CHANGE_DISPLAY_RESULTS.Failed)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Couldn't apply the new GDI modes because the display driver failed to apply the specified graphics mode.");
                    return false;
                }
                else if (result == CHANGE_DISPLAY_RESULTS.NotUpdated)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Couldn't apply the new GDI modes because unable to write new settings to the registry.");
                    return false;
                }
                else if (result == CHANGE_DISPLAY_RESULTS.Restart)
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Couldn't apply the new GDI modes because the computer must be restarted for the graphics mode to work.");
                    return false;
                }
                else
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Unknown error while trying to apply the new GDI modes.");
                    return false;
                }
            }*/


            // NOTE: I have disabled the TaskBar setting logic for now due to errors I cannot fix in this code.
            // WinLibrary will still track the location of the taskbars, but won't actually set them as the setting of the taskbars doesnt work at the moment.           
            // I hope to eventually fix this code, but I don't want to hold up a new DisplayMagician release while troubleshooting this.
            /*
            // Now set the taskbar position for each screen
            if (displayConfig.TaskBarLayout.Count > 0 && allWindowsDisplayConfig.TaskBarLayout.Count > 0)
            {
                foreach (var tbrDictEntry in displayConfig.TaskBarLayout)
                {
                    // Look up the monitor location of the current monitor and find the matching taskbar location in the taskbar settings
                    if (allWindowsDisplayConfig.TaskBarLayout.ContainsKey(tbrDictEntry.Key))
                    {
                        // check the current monitor taskbar location
                        // if the current monitor location is the same as the monitor we want to set then
                        TaskBarLayout currentLayout = displayConfig.TaskBarLayout[tbrDictEntry.Key];
                        TaskBarLayout wantedLayout = allWindowsDisplayConfig.TaskBarLayout[tbrDictEntry.Key];
                        if (currentLayout.Equals(wantedLayout))
                        {
                            SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Display {tbrDictEntry.Key} ({tbrDictEntry.Value.RegKeyValue}) has the taskbar with the correct position, size and settings, so no need to move it");
                        }
                        else
                        {
                            // if the current monitor taskbar location is not where we want it then
                            // move the taskbar manually
                            TaskBarLayout tbr = tbrDictEntry.Value;
                            tbr.MoveTaskBar();
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Display {tbrDictEntry.Key} ({tbrDictEntry.Value.RegKeyValue}) is not currently in use, so cannot set any taskbars on it!");
                    }
                }

                // This will actually move the taskbars by forcing Explorer to read from registry key
                /*RestartManagerSession.RestartExplorer();
                Process[] explorers = Process.GetProcessesByName("Explorer");
                for (int i = 0; i < explorers.Length; i++)
                {
                    kill
                }
                // Enum all the monitors
                //List<MONITORINFOEX> currentMonitors = Utils.EnumMonitors();
                // Go through each monitor
                //foreach (MONITORINFOEX mi in currentMonitors)
                //{
                // Look up the monitor location of the current monitor and find the matching taskbar location in the taskbar settings
                //if (current)
                // check the current monitor taskbar location
                // if the current monitor location is the same as the monitor we want to set then
                // if the current monitor taskbar location where we want it then
                // move the taskbar manually
                // Find the registry key for the monitor we are modifying
                // save the taskbar position for the monitor in registry
                // else
                // log the fact that the monitor is in the right place so skipping moving it
                // if we didn't find a taskbar location for this monitor
                // log the fact that the taskbar location wasnt foound for this monitor
                //}

            }
            else
            {
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: No taskbar layout in display profile so skipping setting it!");
            }

            // Now set the taskbar settings
            TaskBarSettings currentTaskBarSettings = TaskBarSettings.GetCurrent();
            if (!displayConfig.TaskBarSettings.Equals(currentTaskBarSettings))
            {
                // The settings are different, so we should apply them
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Setting the taskbar settings .");
                if (displayConfig.TaskBarSettings.Apply())
                {
                    SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: Set the taskbar settings successfully!");
                    //needToRestartExplorer = true;
                }
                else
                {
                    SharedLogger.logger.Error($"WinLibrary/SetActiveConfig: Unable to set the taskbar settings.");
                }
            }
            else
            {
                // The settings are the same, so we should skip applying them
                SharedLogger.logger.Trace($"WinLibrary/SetActiveConfig: The current taskbar settings are the same as the one's we want, so skipping setting them!");
            }*/

            // Force a reapply of all taskbars (inn case NVIDIA Surround mucks it up)
            TaskbarHelper.ForceTaskbarRedraw();

            return true;
        }

        public bool IsActiveConfig(WINDOWS_DISPLAY_CONFIG displayConfig)
        {
            // Check whether the display config is in use now
            SharedLogger.logger.Trace($"WinLibrary/IsActiveConfig: Checking whether the display configuration is already being used.");
            if (displayConfig.Equals(ActiveDisplayConfig))
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
            PatchWindowsDisplayConfig(ref displayConfig);

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

            // CHeck that we have all the displayConfig DisplayIdentifiers we need available now
            //if (currentAllIds.Intersect(displayConfig.DisplayIdentifiers).Count() == displayConfig.DisplayIdentifiers.Count)
            if (displayConfig.DisplayIdentifiers.All(value => _allConnectedDisplayIdentifiers.Contains(value)))
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
            _allConnectedDisplayIdentifiers = GetSomeDisplayIdentifiers(QDC.QDC_ALL_PATHS);

            return _allConnectedDisplayIdentifiers;
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

        public List<string> GetAllPCIVideoCardVendors()
        {
            SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Getting the current PCI vendor ids for the videocards reported to Windows");
            List<string> videoCardVendorIds = new List<string>();


            SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Testing whether the display configuration is valid (allowing tweaks).");
            // Get the size of the largest Active Paths and Modes arrays
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ALL_PATHS, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetCurrentPCIVideoCardVendors: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ALL_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetCurrentPCIVideoCardVendors: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ALL_PATHS, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetCurrentPCIVideoCardVendors: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"WinLibrary/GetSomeDisplayIdentifiers: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(QDC.QDC_ALL_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
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
                /*if (path.TargetInfo.TargetAvailable == false)
                {
                    // We want to skip this one cause it's not valid
                    SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Skipping path due to TargetAvailable not existing in display #{path.TargetInfo.Id}");
                    continue;
                }*/

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
                    // The AdapterDevicePath is something like "\\?\PCI#VEN_10DE&DEV_2482&SUBSYS_408E1458&REV_A1#4&2283f625&0&0019#{5b45201d-f2f2-4f3b-85bb-30ff1f953599}" if it's a PCI card
                    // or it is something like "\\?\USB#VID_17E9&PID_430C&MI_00#8&d6f23a6&1&0000#{5b45201d-f2f2-4f3b-85bb-30ff1f953599}" if it's a USB card (or USB emulating)
                    // or it is something like "\\?\SuperDisplay#Display#1&3343b12b&0&1234#{5b45201d-f2f2-4f3b-85bb-30ff1f953599}" if it's a SuperDisplay device (allows Android tablet device to be used as directly attached screen)
                    // or it is something like "\\\\?\\SWD#{1BAAD4AC-CD9D-4207-B4FF-C4F160604B13}#0000#{5b45201d-f2f2-4f3b-85bb-30ff1f953599}" if it is a SpaceDesk Monitor
                    // We only want the vendor ID
                    SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The AdapterDevicePath for this path is :{adapterInfo.AdapterDevicePath}");
                    // Match against the vendor ID
                    string pattern = @"(PCI|USB)#(?:VEN|VID)_([\d\w]{4})&";
                    Match match = Regex.Match(adapterInfo.AdapterDevicePath, pattern);
                    if (match.Success)
                    {
                        string pciType = match.Groups[1].Value;
                        string vendorId = match.Groups[2].Value;
                        SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The matched PCI Vendor ID is :{vendorId} and the PCI device is a {pciType} device.");
                        if (!videoCardVendorIds.Contains(vendorId))
                        {
                            videoCardVendorIds.Add(vendorId);
                            SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Stored PCI vendor ID {vendorId} as we haven't already got it");
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The device is not a USB or PCI card, so trying to see if it is a SuperDisplay device.");
                        string pattern2 = @"SuperDisplay#";
                        Match match2 = Regex.Match(adapterInfo.AdapterDevicePath, pattern2);
                        if (match2.Success)
                        {
                            string pciType = "SuperDisplay";
                            string vendorId = "SuperDisplay";
                            SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The matched PCI Vendor ID is :{vendorId} and the PCI device is a {pciType} device.");
                            if (!videoCardVendorIds.Contains(vendorId))
                            {
                                videoCardVendorIds.Add(vendorId);
                                SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Stored PCI vendor ID {vendorId} as we haven't already got it");
                            }
                        }
                        else
                        {
                            SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The device is not a USB, PCI card or a SuperDisplay display, so trying to see if it is a SpaceDesk device.");
                            string pattern3 = @"SWD#";
                            Match match3 = Regex.Match(adapterInfo.AdapterDevicePath, pattern3);
                            if (match3.Success)
                            {
                                string pciType = "SWD";
                                string vendorId = "SWD";
                                SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The matched PCI Vendor ID is :{vendorId} and the PCI device is a {pciType} device.");
                                if (!videoCardVendorIds.Contains(vendorId))
                                {
                                    videoCardVendorIds.Add(vendorId);
                                    SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: Stored PCI vendor ID {vendorId} as we haven't already got it");
                                }
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"WinLibrary/GetCurrentPCIVideoCardVendors: The PCI Vendor ID pattern wasn't matched so we didn't record a vendor ID. AdapterDevicePath = {adapterInfo.AdapterDevicePath}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Warn(ex, $"WinLibrary/GetCurrentPCIVideoCardVendors: Exception getting PCI Vendor ID from Display Adapter {path.SourceInfo.AdapterId}.");
                }

            }

            return videoCardVendorIds;

        }

        /*public List<DisplayMonitorInfo> GetAllConnectedMonitors()
        {
            var monitorInfos = new List<DisplayMonitorInfo>();

            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ALL_PATHS, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new WinLibraryException($"GetAllAdapterIDs returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ALL_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetAllAdapterIDs: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ALL_PATHS, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(QDC.QDC_ALL_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                    throw new WinLibraryException($"The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                }
                else if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again");
                    throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again.");
                }
            }
            else if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays");
                throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays.");
            }

            foreach (var path in paths)
            {
                // get display adapter name
                var adapterInfo = new DISPLAYCONFIG_TARGET_DEVICE_NAME();
                adapterInfo.Header.Type = DISPLAYCONFIG_DEVICE_INFO_TYPE.DISPLAYCONFIG_DEVICE_INFO_GET_TARGET_NAME;
                adapterInfo.Header.Size = (uint)Marshal.SizeOf<DISPLAYCONFIG_TARGET_DEVICE_NAME>();
                adapterInfo.Header.AdapterId = path.TargetInfo.AdapterId;
                adapterInfo.Header.Id = path.TargetInfo.Id;
                err = CCDImport.DisplayConfigGetDeviceInfo(ref adapterInfo);
                if (err == WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Successfully got the display name info from {path.TargetInfo.Id}.");
                    monitorInfos.Add(new DisplayMonitorInfo
                    {
                        FriendlyName = adapterInfo.MonitorFriendlyDeviceName,
                        ManufacturerId = adapterInfo.EdidManufactureId,
                        ProductCodeId = adapterInfo.EdidProductCodeId,
                        DevicePath = adapterInfo.MonitorDevicePath
                    });
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetAllAdapterIDs: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.TargetInfo.Id}");
                }

            }

            return monitorInfos;
        }*/

        public Dictionary<ulong, string> GetAllAdapterIDs()
        {
            SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Getting the current adapter ids for the videocards Windows knows about");
            Dictionary<ulong, string> currentAdapterMap = new Dictionary<ulong, string>();

            SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Testing whether the display configuration is valid (allowing tweaks).");
            // Get the size of the largest All Paths and Modes arrays
            int pathCount = 0;
            int modeCount = 0;
            WIN32STATUS err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ALL_PATHS, out pathCount, out modeCount);
            if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
                throw new WinLibraryException($"GetAllAdapterIDs returned WIN32STATUS {err} when trying to get the maximum path and mode sizes");
            }

            SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Getting the current Display Config path and mode arrays");
            var paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
            var modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
            err = CCDImport.QueryDisplayConfig(QDC.QDC_ALL_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
            if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
            {
                SharedLogger.logger.Warn($"WinLibrary/GetAllAdapterIDs: The displays were modified between GetDisplayConfigBufferSizes and QueryDisplayConfig so we need to get the buffer sizes again.");
                SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Getting the size of the largest Active Paths and Modes arrays");
                // Screen changed in between GetDisplayConfigBufferSizes and QueryDisplayConfig, so we need to get buffer sizes again
                // as per https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-querydisplayconfig 
                err = CCDImport.GetDisplayConfigBufferSizes(QDC.QDC_ALL_PATHS, out pathCount, out modeCount);
                if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                    throw new WinLibraryException($"GetDisplayConfigBufferSizes returned WIN32STATUS {err} when trying to get the maximum path and mode sizes again");
                }
                SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Getting the current Display Config path and mode arrays");
                paths = new DISPLAYCONFIG_PATH_INFO[pathCount];
                modes = new DISPLAYCONFIG_MODE_INFO[modeCount];
                err = CCDImport.QueryDisplayConfig(QDC.QDC_ALL_PATHS, ref pathCount, paths, ref modeCount, modes, IntPtr.Zero);
                if (err == WIN32STATUS.ERROR_INSUFFICIENT_BUFFER)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                    throw new WinLibraryException($"The displays were still modified between GetDisplayConfigBufferSizes and QueryDisplayConfig, even though we tried twice. Something is wrong.");
                }
                else if (err != WIN32STATUS.ERROR_SUCCESS)
                {
                    SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again");
                    throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays again.");
                }
            }
            else if (err != WIN32STATUS.ERROR_SUCCESS)
            {
                SharedLogger.logger.Error($"WinLibrary/GetAllAdapterIDs: ERROR - QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays");
                throw new WinLibraryException($"QueryDisplayConfig returned WIN32STATUS {err} when trying to query all available displays.");
            }

            foreach (var path in paths)
            {
                if (path.TargetInfo.TargetAvailable == false)
                {
                    // We want to skip this one cause it's not valid
                    SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Skipping path due to TargetAvailable not existing in display #{path.TargetInfo.Id}");
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
                    SharedLogger.logger.Trace($"WinLibrary/GetAllAdapterIDs: Successfully got the display name info from {path.TargetInfo.Id}.");
                    currentAdapterMap[path.TargetInfo.AdapterId.Value] = adapterInfo.AdapterDevicePath;
                }
                else
                {
                    SharedLogger.logger.Warn($"WinLibrary/GetAllAdapterIDs: WARNING - DisplayConfigGetDeviceInfo returned WIN32STATUS {err} when trying to get the target info for display #{path.TargetInfo.Id}");
                }

            }

            return currentAdapterMap;

        }

        public static bool GDISettingsEqual(Dictionary<string, GDI_DISPLAY_SETTING> gdi1, Dictionary<string, GDI_DISPLAY_SETTING> gdi2)
        {
            if (gdi1.Count == gdi2.Count)
            {
                for (int i = 0; i < gdi1.Count; i++)
                {
                    if (gdi1.Values.ToList()[i] != gdi2.Values.ToList()[i])
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool RepositionMainTaskBar(TaskBarEdge edge)
        {
            // Tell Windows to refresh the Main Screen Windows Taskbar
            // Find the "Shell_TrayWnd" window 
            IntPtr systemTrayContainerHandle = Utils.FindWindow("Shell_TrayWnd", null);
            IntPtr startButtonHandle = Utils.FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "Start", null);
            IntPtr systemTrayHandle = Utils.FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "TrayNotifyWnd", null);
            IntPtr rebarWindowHandle = Utils.FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "ReBarWindow32", null);
            IntPtr taskBarPositionBuffer = new IntPtr((Int32)edge);
            IntPtr trayDesktopShowButtonHandle = Utils.FindWindowEx(systemTrayHandle, IntPtr.Zero, "TrayShowDesktopButtonWClass", null);
            IntPtr trayInputIndicatorHandle = Utils.FindWindowEx(systemTrayHandle, IntPtr.Zero, "TrayInputIndicatorWClass", null);

            // Send messages
            // Send the "TrayNotifyWnd" window a WM_USER+13 (0x040D) message with a wParameter of 0x0 and a lParameter of the position (e.g. 0x0000 for left, 0x0001 for top, 0x0002 for right and 0x0003 for bottom)
            Utils.SendMessage(systemTrayHandle, Utils.WM_USER_13, IntPtr.Zero, taskBarPositionBuffer);
            Utils.SendMessage(systemTrayHandle, Utils.WM_USER_100, (IntPtr)0x3e, (IntPtr)0x21c);
            Utils.SendMessage(systemTrayHandle, Utils.WM_THEMECHANGED, unchecked((IntPtr)0xffffffffffffffff), (IntPtr)0x000000008000001);
            // Next, send the "TrayShowDesktopButtonWClass" window a WM_USER+13 (0x040D) message with a wParameter of 0x0 and a lParameter of the position (e.g. 0x0000 for left, 0x0001 for top, 0x0002 for right and 0x0003 for bottom)
            Utils.SendMessage(trayDesktopShowButtonHandle, Utils.WM_USER_13, IntPtr.Zero, taskBarPositionBuffer);
            Utils.SendMessage(startButtonHandle, Utils.WM_USER_440, (IntPtr)0x0, (IntPtr)0x0);
            Utils.SendMessage(systemTrayHandle, Utils.WM_USER_1, (IntPtr)0x0, (IntPtr)0x0);
            Utils.SendMessage(systemTrayContainerHandle, Utils.WM_SETTINGCHANGE, (IntPtr)Utils.SPI_SETWORKAREA, (IntPtr)Utils.NULL);
            Utils.PostMessage(systemTrayHandle, Utils.WM_SETTINGCHANGE, Utils.SPI_SETWORKAREA, Utils.NULL);
            Utils.SendMessage(systemTrayContainerHandle, Utils.WM_SETTINGCHANGE, (IntPtr)Utils.SPI_SETWORKAREA, (IntPtr)Utils.NULL);
            Utils.SendMessage(rebarWindowHandle, Utils.WM_SETTINGCHANGE, (IntPtr)Utils.SPI_SETWORKAREA, (IntPtr)Utils.NULL);
            //Utils.SendMessage(trayInputIndicatorHandle, Utils.WM_USER_100, (IntPtr)0x3e, (IntPtr)0x21c);
            //Utils.SendMessage(systemTrayHandle, Utils.WM_USER_1, (IntPtr)0x0, (IntPtr)0x0);
            // Move all the taskbars to this location
            //Utils.SendMessage(systemTrayContainerHandle, Utils.WM_USER_REFRESHTASKBAR, (IntPtr)Utils.wParam_SHELLTRAY, taskBarPositionBuffer);
            //Utils.SendMessage(systemTrayContainerHandle, Utils.WM_USER_REFRESHTASKBAR, (IntPtr)Utils.wParam_SHELLTRAY, taskBarPositionBuffer);
            return true;
        }

        public static bool RepositionAllTaskBars(TaskBarEdge edge)
        {
            // Tell Windows to refresh the Main Screen Windows Taskbar
            // Find the "Shell_TrayWnd" window 
            IntPtr mainToolBarHWnd = Utils.FindWindow("Shell_TrayWnd", null);
            // Send the "Shell_TrayWnd" window a WM_USER_REFRESHTASKBAR with a wParameter of 0006 and a lParameter of the position (e.g. 0000 for left, 0001 for top, 0002 for right and 0003 for bottom)
            IntPtr taskBarPositionBuffer = new IntPtr((Int32)edge);
            Utils.SendMessage(mainToolBarHWnd, Utils.WM_USER_REFRESHTASKBAR, (IntPtr)Utils.wParam_SHELLTRAY, taskBarPositionBuffer);
            return true;
        }

        public static bool RepositionSecondaryTaskBars()
        {
            // Tell Windows to refresh the Other Windows Taskbars if needed
            IntPtr lastTaskBarWindowHwnd = (IntPtr)Utils.NULL;
            for (int i = 0; i < 100; i++)
            {
                // Find the next "Shell_SecondaryTrayWnd" window 
                IntPtr nextTaskBarWindowHwnd = Utils.FindWindowEx((IntPtr)Utils.NULL, lastTaskBarWindowHwnd, "Shell_SecondaryTrayWnd", null);
                if (nextTaskBarWindowHwnd == (IntPtr)Utils.NULL)
                {
                    // No more windows taskbars to notify
                    break;
                }
                // Send the "Shell_TrayWnd" window a WM_SETTINGCHANGE with a wParameter of SPI_SETWORKAREA
                Utils.SendMessage(lastTaskBarWindowHwnd, Utils.WM_SETTINGCHANGE, (IntPtr)Utils.SPI_SETWORKAREA, (IntPtr)Utils.NULL);
                lastTaskBarWindowHwnd = nextTaskBarWindowHwnd;
            }
            return true;
        }



        public static void RefreshTrayArea()
        {
            // Finds the Shell_TrayWnd -> TrayNotifyWnd -> SysPager -> "Notification Area" containing the visible notification area icons (windows 7 version)
            IntPtr systemTrayContainerHandle = Utils.FindWindow("Shell_TrayWnd", null);
            IntPtr systemTrayHandle = Utils.FindWindowEx(systemTrayContainerHandle, IntPtr.Zero, "TrayNotifyWnd", null);
            IntPtr sysPagerHandle = Utils.FindWindowEx(systemTrayHandle, IntPtr.Zero, "SysPager", null);
            IntPtr notificationAreaHandle = Utils.FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", "Notification Area");
            // If the visible notification area icons (Windows 7 aren't found, then we're on a later version of windows, and we need to look for different window names
            if (notificationAreaHandle == IntPtr.Zero)
            {
                // Finds the Shell_TrayWnd -> TrayNotifyWnd -> SysPager -> "User Promoted Notification Area" containing the visible notification area icons (windows 10+ version)
                notificationAreaHandle = Utils.FindWindowEx(sysPagerHandle, IntPtr.Zero, "ToolbarWindow32", "User Promoted Notification Area");
                // Also attempt to find the NotifyIconOverflowWindow -> "Overflow Notification Area' window which is the hidden windoww that notification icons live when they are 
                // too numberous or are hidden by the user.
                IntPtr notifyIconOverflowWindowHandle = Utils.FindWindow("NotifyIconOverflowWindow", null);
                IntPtr overflowNotificationAreaHandle = Utils.FindWindowEx(notifyIconOverflowWindowHandle, IntPtr.Zero, "ToolbarWindow32", "Overflow Notification Area");
                // Fool the "Overflow Notification Area' window into thinking the mouse is moving over it
                // which will force windows to refresh the "Overflow Notification Area' window and remove old icons.
                RefreshTrayArea(overflowNotificationAreaHandle);
                notifyIconOverflowWindowHandle = IntPtr.Zero;
                overflowNotificationAreaHandle = IntPtr.Zero;
            }
            // Fool the "Notification Area" or "User Promoted Notification Area" window (depends on the version of windows) into thinking the mouse is moving over it
            // which will force windows to refresh the "Notification Area" or "User Promoted Notification Area" window and remove old icons.
            RefreshTrayArea(notificationAreaHandle);
            systemTrayContainerHandle = IntPtr.Zero;
            systemTrayHandle = IntPtr.Zero;
            sysPagerHandle = IntPtr.Zero;
            notificationAreaHandle = IntPtr.Zero;

        }


        private static void RefreshTrayArea(IntPtr windowHandle)
        {
            // Moves the mouse around within the window area of the supplied window
            RECT rect;
            Utils.GetClientRect(windowHandle, out rect);
            for (var x = 0; x < rect.right; x += 5)
                for (var y = 0; y < rect.bottom; y += 5)
                    Utils.SendMessage(windowHandle, Utils.WM_MOUSEMOVE, 0, (y << 16) + x);
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