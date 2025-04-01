using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using DisplayMagicianShared.Windows;

namespace DisplayMagicianShared.NVIDIA
{
    public static class NVAPI
    {

        private static bool available = true;

        static NVAPI()
        {
            try
            {
                Marshal.PrelinkAll(typeof(GeneralDelegates));
                available = true;
            }
            catch(Exception) { }
        }

        // TODO: Fix this is available so it actually tests if nvidia dll is available.
        public static bool IsAvailable() { return available; }

        /// <summary>
        ///     This function returns information about the system's chipset.
        /// </summary>
        /// <returns>Information about the system's chipset</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid argument</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static IChipsetInfo GetChipsetInfo()
        {
            var getChipSetInfo = DelegateFactory.GetDelegate<GeneralDelegates.NvAPI_SYS_GetChipSetInfo>();

            foreach (var acceptType in getChipSetInfo.Accepts())
            {
                var instance = acceptType.Instantiate<IChipsetInfo>();

                using (var chipsetInfoReference = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getChipSetInfo(chipsetInfoReference);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return chipsetInfoReference.ToValueType<IChipsetInfo>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }


        /// <summary>
        ///     This API returns display driver version and driver-branch string.
        /// </summary>
        /// <param name="branchVersion">Contains the driver-branch string after successful return.</param>
        /// <returns>Returns driver version</returns>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: NVAPI not initialized</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred</exception>
        public static uint GetDriverAndBranchVersion(out string branchVersion)
        {
            var status = DelegateFactory.GetDelegate<GeneralDelegates.NvAPI_SYS_GetDriverAndBranchVersion>()(
                out var driverVersion, out var branchVersionShortString);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            branchVersion = branchVersionShortString.Value;

            return driverVersion;
        }

        /// <summary>
        ///     This function converts an NvAPI error code into a null terminated string.
        /// </summary>
        /// <param name="statusCode">The error code to convert</param>
        /// <returns>The string corresponding to the error code</returns>
        // ReSharper disable once FlagArgument
        public static string GetErrorMessage(Status statusCode)
        {
            statusCode =
                DelegateFactory.GetDelegate< GeneralDelegates.NvAPI_GetErrorMessage>()(statusCode, out var message);

            if (statusCode != Status.Ok)
            {
                return null;
            }

            return message.Value;
        }

        /// <summary>
        ///     This function returns a string describing the version of the NvAPI library. The contents of the string are human
        ///     readable. Do not assume a fixed format.
        /// </summary>
        /// <returns>User readable string giving NvAPI version information</returns>
        /// <exception cref="NVIDIAApiException">See NVIDIAApiException.Status for the reason of the exception.</exception>
        public static string GetInterfaceVersionString()
        {
            var status =
                DelegateFactory.GetDelegate<GeneralDelegates.NvAPI_GetInterfaceVersionString>()(out var version);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return version.Value;
        }

        /// <summary>
        ///     This function returns the current lid and dock information.
        /// </summary>
        /// <returns>Current lid and dock information</returns>
        /// <exception cref="NVIDIAApiException">Status.Error: Generic error</exception>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: Requested feature not supported</exception>
        /// <exception cref="NVIDIAApiException">Status.HandleInvalidated: Handle is no longer valid</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: NvAPI_Initialize() has not been called</exception>
        public static LidDockParameters GetLidAndDockInfo()
        {
            var dockInfo = typeof(LidDockParameters).Instantiate<LidDockParameters>();
            var status = DelegateFactory.GetDelegate<GeneralDelegates.NvAPI_SYS_GetLidAndDockInfo>()(ref dockInfo);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return dockInfo;
        }

        /// <summary>
        ///     This function initializes the NvAPI library (if not already initialized) but always increments the ref-counter.
        ///     This must be called before calling other NvAPI_ functions.
        /// </summary>
        /// <exception cref="NVIDIAApiException">Status.Error: Generic error</exception>
        /// <exception cref="NVIDIAApiException">Status.LibraryNotFound: nvapi.dll can not be loaded</exception>
        public static void Initialize()
        {
            var status = DelegateFactory.GetDelegate<GeneralDelegates.NvAPI_Initialize>()();

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
            else
            {
                available = true;
            }
        }

        /// <summary>
        ///     PRIVATE - Requests to restart the display driver
        /// </summary>
        public static void RestartDisplayDriver()
        {
            var status = DelegateFactory.GetDelegate<GeneralDelegates.NvAPI_RestartDisplayDriver>()();

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     Decrements the ref-counter and when it reaches ZERO, unloads NVAPI library.
        ///     This must be called in pairs with NvAPI_Initialize.
        ///     Note: By design, it is not mandatory to call NvAPI_Initialize before calling any NvAPI.
        ///     When any NvAPI is called without first calling NvAPI_Initialize, the internal ref-counter will be implicitly
        ///     incremented. In such cases, calling NvAPI_Initialize from a different thread will result in incrementing the
        ///     ref-count again and the user has to call NvAPI_Unload twice to unload the library. However, note that the implicit
        ///     increment of the ref-counter happens only once.
        ///     If the client wants unload functionality, it is recommended to always call NvAPI_Initialize and NvAPI_Unload in
        ///     pairs.
        ///     Unloading NvAPI library is not supported when the library is in a resource locked state.
        ///     Some functions in the NvAPI library initiates an operation or allocates certain resources and there are
        ///     corresponding functions available, to complete the operation or free the allocated resources. All such function
        ///     pairs are designed to prevent unloading NvAPI library.
        ///     For example, if NvAPI_Unload is called after NvAPI_XXX which locks a resource, it fails with NVAPI_ERROR.
        ///     Developers need to call the corresponding NvAPI_YYY to unlock the resources, before calling NvAPI_Unload again.
        /// </summary>
        /// <exception cref="NVIDIAApiException">Status.Error: Generic error</exception>
        /// <exception cref="NVIDIAApiException">
        ///     Status.ApiInUse: At least an API is still being called hence cannot unload NVAPI
        ///     library from process
        /// </exception>
        public static void Unload()
        {
            var status = DelegateFactory.GetDelegate<GeneralDelegates.NvAPI_Unload>()();
            
            available = false;
            
            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }


        /// <summary>
        ///     This API controls the display color configurations.
        /// </summary>
        /// <param name="displayId">The targeted display id.</param>
        /// <param name="colorData">The structure to be filled with information requested or applied on the display.</param>
        public static void ColorControl<TColorData>(uint displayId, ref TColorData colorData)
            where TColorData : struct, IColorData
        {
            var c = colorData as IColorData;
            ColorControl(displayId, ref c);
            colorData = (TColorData)c;
        }

        /// <summary>
        ///     This API controls the display color configurations.
        /// </summary>
        /// <param name="displayId">The targeted display id.</param>
        /// <param name="colorData">The structure to be filled with information requested or applied on the display.</param>
        public static void ColorControl(uint displayId, ref IColorData colorData)
        {
            var colorControl = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_Disp_ColorControl>();
            var type = colorData.GetType();

            if (!colorControl.Accepts().Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            using (var colorDataReference = ValueTypeReference.FromValueType(colorData, type))
            {
                var status = colorControl(displayId, colorDataReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                colorData = colorDataReference.ToValueType<IColorData>(type);
            }
        }

        /// <summary>
        ///     This API controls the display color configurations.
        /// </summary>
        /// <param name="displayId">The targeted display id.</param>
        /// <param name="colorDatas">The list of structures to be filled with information requested or applied on the display.</param>
        /// <returns>The structure that succeed in requesting information or used for applying configuration on the display.</returns>
        // ReSharper disable once IdentifierTypo
        public static IColorData ColorControl(uint displayId, IColorData[] colorDatas)
        {
            foreach (var colorData in colorDatas)
            {
                try
                {
                    var c = colorData;
                    ColorControl(displayId, ref c);

                    return c;
                }
                catch (NVIDIAApiException e)
                {
                    if (e.Status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    throw;
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This function converts the unattached display handle to an active attached display handle.
        ///     At least one GPU must be present in the system and running an NVIDIA display driver.
        /// </summary>
        /// <param name="display">An unattached display handle to convert.</param>
        /// <returns>Display handle of newly created display.</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid UnAttachedDisplayHandle handle.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static DisplayHandle CreateDisplayFromUnAttachedDisplay(UnAttachedDisplayHandle display)
        {
            var createDisplayFromUnAttachedDisplay =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_CreateDisplayFromUnAttachedDisplay>();
            var status = createDisplayFromUnAttachedDisplay(display, out var newDisplay);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return newDisplay;
        }

        /// <summary>
        ///     This function deletes the custom display configuration, specified from the registry for all the displays whose
        ///     display IDs are passed.
        /// </summary>
        /// <param name="displayIds">Array of display IDs on which custom display configuration should be removed.</param>
        /// <param name="customDisplay">The custom display to remove.</param>
        public static void DeleteCustomDisplay(uint[] displayIds, CustomDisplay customDisplay)
        {
            if (displayIds.Length == 0)
            {
                return;
            }

            using (var displayIdsReference = ValueTypeArray.FromArray(displayIds))
            {
                using (var customDisplayReference = ValueTypeReference.FromValueType(customDisplay))
                {
                    var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_DeleteCustomDisplay>()(
                        displayIdsReference,
                        (uint)displayIds.Length,
                        customDisplayReference
                    );

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }
                }
            }
        }

        /// <summary>
        ///     This API enumerates the custom timing specified by the enum index.
        /// </summary>
        /// <param name="displayId">The display id of the display.</param>
        /// <returns>A list of <see cref="CustomDisplay" /></returns>
        public static IEnumerable<CustomDisplay> EnumCustomDisplays(uint displayId)
        {
            var instance = typeof(CustomDisplay).Instantiate<CustomDisplay>();

            using (var customDisplayReference = ValueTypeReference.FromValueType(instance))
            {
                for (uint i = 0; i < uint.MaxValue; i++)
                {
                    var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_EnumCustomDisplay>()(
                        displayId,
                        i,
                        customDisplayReference
                    );

                    if (status == Status.EndEnumeration)
                    {
                        yield break;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    yield return customDisplayReference.ToValueType<CustomDisplay>().GetValueOrDefault();
                }
            }
        }

        /// <summary>
        ///     This function returns the handle of all NVIDIA displays
        ///     Note: Display handles can get invalidated on a mode-set, so the calling applications need to re-enum the handles
        ///     after every mode-set.
        /// </summary>
        /// <returns>Array of display handles.</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA device found in the system</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static DisplayHandle[] EnumNvidiaDisplayHandle()
        {
            var enumNvidiaDisplayHandle =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_EnumNvidiaDisplayHandle>();
            var results = new List<DisplayHandle>();
            uint i = 0;

            while (true)
            {
                var status = enumNvidiaDisplayHandle(i, out var displayHandle);

                if (status == Status.EndEnumeration)
                {
                    break;
                }

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                results.Add(displayHandle);
                i++;
            }

            return results.ToArray();
        }

        /// <summary>
        ///     This function returns the handle of all unattached NVIDIA displays
        ///     Note: Display handles can get invalidated on a mode-set, so the calling applications need to re-enum the handles
        ///     after every mode-set.
        /// </summary>
        /// <returns>Array of unattached display handles.</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA device found in the system</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static UnAttachedDisplayHandle[] EnumNvidiaUnAttachedDisplayHandle()
        {
            var enumNvidiaUnAttachedDisplayHandle =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_EnumNvidiaUnAttachedDisplayHandle>();
            var results = new List<UnAttachedDisplayHandle>();
            uint i = 0;

            while (true)
            {
                var status = enumNvidiaUnAttachedDisplayHandle(i, out var displayHandle);

                if (status == Status.EndEnumeration)
                {
                    break;
                }

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                results.Add(displayHandle);
                i++;
            }

            return results.ToArray();
        }

        /// <summary>
        ///     This function gets the active outputId associated with the display handle.
        /// </summary>
        /// <param name="display">
        ///     NVIDIA Display selection. It can be DisplayHandle.DefaultHandle or a handle enumerated from
        ///     DisplayApi.EnumNVidiaDisplayHandle().
        /// </param>
        /// <returns>
        ///     The active display output ID associated with the selected display handle hNvDisplay. The output id will have
        ///     only one bit set. In the case of Clone or Span mode, this will indicate the display outputId of the primary display
        ///     that the GPU is driving.
        /// </returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedDisplayHandle: display is not a valid display handle.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static OutputId GetAssociatedDisplayOutputId(DisplayHandle display)
        {
            var getAssociatedDisplayOutputId =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetAssociatedDisplayOutputId>();
            var status = getAssociatedDisplayOutputId(display, out var outputId);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return outputId;
        }

        /// <summary>
        ///     This function returns the handle of the NVIDIA display that is associated with the given display "name" (such as
        ///     "\\.\DISPLAY1").
        /// </summary>
        /// <param name="name">Display name</param>
        /// <returns>Display handle of associated display</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Display name is null.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA device maps to that display name.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static DisplayHandle GetAssociatedNvidiaDisplayHandle(string name)
        {
            var getAssociatedNvidiaDisplayHandle =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetAssociatedNvidiaDisplayHandle>();
            var status = getAssociatedNvidiaDisplayHandle(name, out var display);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return display;
        }

        /// <summary>
        ///     For a given NVIDIA display handle, this function returns a string (such as "\\.\DISPLAY1") to identify the display.
        /// </summary>
        /// <param name="display">Handle of the associated display</param>
        /// <returns>Name of the display</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Display handle is null.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA device maps to that display name.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static string GetAssociatedNvidiaDisplayName(DisplayHandle display)
        {
            var getAssociatedNvidiaDisplayName =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetAssociatedNvidiaDisplayName>();
            var status = getAssociatedNvidiaDisplayName(display, out var displayName);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return displayName.Value;
        }

        /// <summary>
        ///     This function returns the handle of an unattached NVIDIA display that is associated with the given display "name"
        ///     (such as "\\DISPLAY1").
        /// </summary>
        /// <param name="name">Display name</param>
        /// <returns>Display handle of associated unattached display</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Display name is null.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA device maps to that display name.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static UnAttachedDisplayHandle GetAssociatedUnAttachedNvidiaDisplayHandle(string name)
        {
            var getAssociatedUnAttachedNvidiaDisplayHandle =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_GetAssociatedUnAttachedNvidiaDisplayHandle>();
            var status = getAssociatedUnAttachedNvidiaDisplayHandle(name, out var display);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return display;
        }

        /// <summary>
        ///     This API lets caller retrieve the current global display configuration.
        ///     Note: User should dispose all returned PathInfo objects
        /// </summary>
        /// <returns>Array of path information</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid input parameter.</exception>
        /// <exception cref="NVIDIAApiException">Status.DeviceBusy: ModeSet has not yet completed. Please wait and call it again.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static PathInfoV2[] GetDisplayConfig()
        {
            var getDisplayConfig = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_GetDisplayConfig>();
            uint allAvailable = 0;
            // First call to GetDisplayConfig returns the number of available paths
            //var status = getDisplayConfig(ref allAvailable, ValueTypeArray.Null);
            var status = getDisplayConfig(ref allAvailable, ValueTypeArray.Null);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            if (allAvailable == 0)
            {
                return new PathInfoV2[0];
            }

            foreach (var acceptType in getDisplayConfig.Accepts())
            {
                var count = allAvailable;
                var instances = acceptType.Instantiate<PathInfoV2>().Repeat((int)allAvailable);

                //using (var pathInfos = ValueTypeArray.FromArray(instances, acceptType))
                using (var pathInfos = ValueTypeArray.FromArray(instances))
                {
                    // Second call to GetDisplayConfig returns the available paths in array, but missing the sourcemodeinfo
                    status = getDisplayConfig(ref count, pathInfos);

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    instances = pathInfos.ToArray<PathInfoV2>((int)count, acceptType);
                }

                if (instances.Length <= 0)
                {
                    return new PathInfoV2[0];
                }

                // This allocation of memory is needed to contain the sourcemodeinfo for each path
                // After allocation, we should make sure to dispose objects
                // In this case however, the responsibility is on the user shoulders
                instances = instances.AllocateAll().ToArray();

                using (var pathInfos = ValueTypeArray.FromArray(instances))
                {
                    // Third call to GetDisplayConfig returns the available paths in array, with sourcemodeinfo included
                    status = getDisplayConfig(ref count, pathInfos);

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return pathInfos.ToArray<PathInfoV2>((int)count, acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     Gets the build title of the Driver Settings Database for a display
        /// </summary>
        /// <param name="displayHandle">The display handle to get DRS build title.</param>
        /// <returns>The DRS build title.</returns>
        public static string GetDisplayDriverBuildTitle(DisplayHandle displayHandle)
        {
            var status =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetDisplayDriverBuildTitle>()(displayHandle,
                    out var name);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return name.Value;
        }

        /// <summary>
        ///     This function retrieves the available driver memory footprint for the GPU associated with a display.
        /// </summary>
        /// <param name="displayHandle">Handle of the display for which the memory information of its GPU is to be extracted.</param>
        /// <returns>The memory footprint available in the driver.</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static IDisplayDriverMemoryInfo GetDisplayDriverMemoryInfo(DisplayHandle displayHandle)
        {
            var getMemoryInfo = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetDisplayDriverMemoryInfo>();

            foreach (var acceptType in getMemoryInfo.Accepts())
            {
                var instance = acceptType.Instantiate<IDisplayDriverMemoryInfo>();

                using (var displayDriverMemoryInfo = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getMemoryInfo(displayHandle, displayDriverMemoryInfo);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return displayDriverMemoryInfo.ToValueType<IDisplayDriverMemoryInfo>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This API retrieves the Display Id of a given display by display name. The display must be active to retrieve the
        ///     displayId. In the case of clone mode or Surround gaming, the primary or top-left display will be returned.
        /// </summary>
        /// <param name="displayName">Name of display (Eg: "\\DISPLAY1" to retrieve the displayId for.</param>
        /// <returns>Display ID of the requested display.</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more args passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry-point not available</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static uint GetDisplayIdByDisplayName(string displayName)
        {
            var getDisplayIdByDisplayName =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_GetDisplayIdByDisplayName>();
            var status = getDisplayIdByDisplayName(displayName, out var display);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return display;
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API returns the current saturation level from the Digital Vibrance Control
        /// </summary>
        /// <param name="display">
        ///     The targeted display's handle.
        /// </param>
        /// <returns>An instance of the PrivateDisplayDVCInfo structure containing requested information.</returns>
        public static PrivateDisplayDVCInfo GetDVCInfo(DisplayHandle display)
        {
            var instance = typeof(PrivateDisplayDVCInfo).Instantiate<PrivateDisplayDVCInfo>();

            using (var displayDVCInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetDVCInfo>()(
                    display,
                    OutputId.Invalid,
                    displayDVCInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return displayDVCInfoReference.ToValueType<PrivateDisplayDVCInfo>().GetValueOrDefault();
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API returns the current saturation level from the Digital Vibrance Control
        /// </summary>
        /// <param name="displayId">
        ///     The targeted display output id.
        /// </param>
        /// <returns>An instance of the PrivateDisplayDVCInfo structure containing requested information.</returns>
        public static PrivateDisplayDVCInfo GetDVCInfo(OutputId displayId)
        {
            var instance = typeof(PrivateDisplayDVCInfo).Instantiate<PrivateDisplayDVCInfo>();

            using (var displayDVCInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetDVCInfo>()(
                    DisplayHandle.DefaultHandle,
                    displayId,
                    displayDVCInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return displayDVCInfoReference.ToValueType<PrivateDisplayDVCInfo>().GetValueOrDefault();
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API returns the current and the default saturation level from the Digital Vibrance Control.
        ///     The difference between this API and the 'GetDVCInfo()' includes the possibility to get the default
        ///     saturation level as well as to query under saturated configurations.
        /// </summary>
        /// <param name="display">
        ///     The targeted display's handle.
        /// </param>
        /// <returns>An instance of the PrivateDisplayDVCInfoEx structure containing requested information.</returns>
        public static PrivateDisplayDVCInfoEx GetDVCInfoEx(DisplayHandle display)
        {
            var instance = typeof(PrivateDisplayDVCInfoEx).Instantiate<PrivateDisplayDVCInfoEx>();

            using (var displayDVCInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetDVCInfoEx>()(
                    display,
                    OutputId.Invalid,
                    displayDVCInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return displayDVCInfoReference.ToValueType<PrivateDisplayDVCInfoEx>().GetValueOrDefault();
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API returns the current and the default saturation level from the Digital Vibrance Control.
        ///     The difference between this API and the 'GetDVCInfo()' includes the possibility to get the default
        ///     saturation level as well as to query under saturated configurations.
        /// </summary>
        /// <param name="displayId">
        ///     The targeted display output id.
        /// </param>
        /// <returns>An instance of the PrivateDisplayDVCInfoEx structure containing requested information.</returns>
        public static PrivateDisplayDVCInfoEx GetDVCInfoEx(OutputId displayId)
        {
            var instance = typeof(PrivateDisplayDVCInfoEx).Instantiate<PrivateDisplayDVCInfoEx>();

            using (var displayDVCInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetDVCInfoEx>()(
                    DisplayHandle.DefaultHandle,
                    displayId,
                    displayDVCInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return displayDVCInfoReference.ToValueType<PrivateDisplayDVCInfoEx>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This API returns the current info-frame data on the specified device (monitor).
        /// </summary>
        /// <param name="displayHandle">The display handle of the device to retrieve HDMI support information for.</param>
        /// <param name="outputId">The target display's output id, or <see cref="OutputId.Invalid"/> to determine automatically.</param>
        /// <returns>An instance of a type implementing the <see cref="IHDMISupportInfo" /> interface.</returns>
        public static IHDMISupportInfo GetHDMISupportInfo(DisplayHandle displayHandle, OutputId outputId = OutputId.Invalid)
        {
            var getHDMISupportInfo = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetHDMISupportInfo>();

            foreach (var acceptType in getHDMISupportInfo.Accepts())
            {
                var instance = acceptType.Instantiate<IHDMISupportInfo>();

                using (var supportInfoReference = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getHDMISupportInfo(displayHandle, (uint)outputId, supportInfoReference);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return supportInfoReference.ToValueType<IHDMISupportInfo>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This API returns the current info-frame data on the specified device (monitor).
        /// </summary>
        /// <param name="displayId">The display id of the device to retrieve HDMI support information for.</param>
        /// <returns>An instance of a type implementing the <see cref="IHDMISupportInfo" /> interface.</returns>
        public static IHDMISupportInfo GetHDMISupportInfo(uint displayId)
        {
            var getHDMISupportInfo = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetHDMISupportInfo>();

            foreach (var acceptType in getHDMISupportInfo.Accepts())
            {
                var instance = acceptType.Instantiate<IHDMISupportInfo>();

                using (var supportInfoReference = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getHDMISupportInfo(DisplayHandle.DefaultHandle, displayId, supportInfoReference);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return supportInfoReference.ToValueType<IHDMISupportInfo>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API returns the current default HUE angle
        /// </summary>
        /// <param name="display">
        ///     The targeted display's handle.
        /// </param>
        /// <returns>An instance of the PrivateDisplayHUEInfo structure containing requested information.</returns>
        public static PrivateDisplayHUEInfo GetHUEInfo(DisplayHandle display)
        {
            var instance = typeof(PrivateDisplayHUEInfo).Instantiate<PrivateDisplayHUEInfo>();

            using (var displayDVCInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetHUEInfo>()(
                    display,
                    OutputId.Invalid,
                    displayDVCInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return displayDVCInfoReference.ToValueType<PrivateDisplayHUEInfo>().GetValueOrDefault();
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API returns the current and default HUE angle
        /// </summary>
        /// <param name="displayId">
        ///     The targeted display output id.
        /// </param>
        /// <returns>An instance of the PrivateDisplayHUEInfo structure containing requested information.</returns>
        public static PrivateDisplayHUEInfo GetHUEInfo(OutputId displayId)
        {
            var instance = typeof(PrivateDisplayHUEInfo).Instantiate<PrivateDisplayHUEInfo>();

            using (var displayDVCInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetHUEInfo>()(
                    DisplayHandle.DefaultHandle,
                    displayId,
                    displayDVCInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return displayDVCInfoReference.ToValueType<PrivateDisplayHUEInfo>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This API returns all the monitor capabilities.
        /// </summary>
        /// <param name="displayId">The target display id.</param>
        /// <param name="capabilitiesType">The type of capabilities requested.</param>
        /// <returns>An instance of <see cref="MonitorCapabilities" />.</returns>
        public static MonitorCapabilities? GetMonitorCapabilities(
            uint displayId,
            MonitorCapabilitiesType capabilitiesType)
        {
            var instance = new MonitorCapabilities(capabilitiesType);

            using (var monitorCapReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_GetMonitorCapabilities>()(
                    displayId,
                    monitorCapReference
                );

                if (status == Status.Error)
                {
                    return null;
                }

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }


                instance = monitorCapReference.ToValueType<MonitorCapabilities>().GetValueOrDefault();

                if (!instance.IsValid)
                {
                    return null;
                }

                return instance;
            }
        }

        /// <summary>
        ///     This API returns all the color formats and bit depth values supported by a given display port monitor.
        /// </summary>
        /// <param name="displayId">The target display id.</param>
        /// <returns>A list of <see cref="MonitorColorData" /> instances.</returns>
        public static MonitorColorData[] GetMonitorColorCapabilities(uint displayId)
        {
            var getMonitorColorCapabilities =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_GetMonitorColorCapabilities>();
            var count = 0u;

            var status = getMonitorColorCapabilities(displayId, ValueTypeArray.Null, ref count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            if (count == 0)
            {
                return new MonitorColorData[0];
            }

            var array = typeof(MonitorColorData).Instantiate<MonitorColorData>().Repeat((int)count);

            using (var monitorCapsReference = ValueTypeArray.FromArray(array))
            {
                status = getMonitorColorCapabilities(displayId, monitorCapsReference, ref count);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return monitorCapsReference.ToArray<MonitorColorData>((int)count);

            }
        }

        /// <summary>
        ///     This API returns the Display ID of the GDI Primary.
        /// </summary>
        /// <returns>Display ID of the GDI Primary.</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: GDI Primary not on an NVIDIA GPU.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry-point not available</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static uint GetGDIPrimaryDisplayId()
        {
            var getGDIPrimaryDisplay =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_GetGDIPrimaryDisplayId>();
            var status = getGDIPrimaryDisplay(out var displayId);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return displayId;
        }

        /// <summary>
        ///     This API gets High Dynamic Range (HDR) capabilities of the display.
        /// </summary>
        /// <param name="displayId">The targeted display id.</param>
        /// <param name="driverExpandDefaultHDRParameters">
        ///     A boolean value indicating if the EDID HDR parameters should be expanded (true) or the actual current HDR
        ///     parameters should be reported (false).
        /// </param>
        /// <returns>HDR capabilities of the display</returns>
        public static HDRCapabilitiesV1 GetHDRCapabilities(uint displayId, bool driverExpandDefaultHDRParameters)
        {
            var hdrCapabilities = new HDRCapabilitiesV1(driverExpandDefaultHDRParameters);

            using (var hdrCapabilitiesReference = ValueTypeReference.FromValueType(hdrCapabilities))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_Disp_GetHdrCapabilities>()(
                    displayId,
                    hdrCapabilitiesReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return hdrCapabilitiesReference.ToValueType<HDRCapabilitiesV1>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This API queries current state of one of the various scan-out composition parameters on the specified display.
        /// </summary>
        /// <param name="displayId">Combined physical display and GPU identifier of the display to query the configuration.</param>
        /// <param name="parameter">Scan-out composition parameter to by queried.</param>
        /// <param name="container">Additional container containing the returning data associated with the specified parameter.</param>
        /// <returns>Scan-out composition parameter value.</returns>
        public static ScanOutCompositionParameterValue GetScanOutCompositionParameter(
            uint displayId,
            ScanOutCompositionParameter parameter,
            out float container)
        {
            var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_GetScanOutCompositionParameter>()(
                displayId,
                parameter,
                out var parameterValue,
                out container
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return parameterValue;
        }

        /// <summary>
        ///     This API queries the desktop and scan-out portion of the specified display.
        /// </summary>
        /// <param name="displayId">Combined physical display and GPU identifier of the display to query the configuration.</param>
        /// <returns>Desktop area to displayId mapping information.</returns>
        public static ScanOutInformationV1 GetScanOutConfiguration(uint displayId)
        {
            var instance = typeof(ScanOutInformationV1).Instantiate<ScanOutInformationV1>();

            using (var scanOutInformationReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_GetScanOutConfigurationEx>()(
                    displayId,
                    scanOutInformationReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return scanOutInformationReference.ToValueType<ScanOutInformationV1>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This API queries the desktop and scan-out portion of the specified display.
        /// </summary>
        /// <param name="displayId">Combined physical display and GPU identifier of the display to query the configuration.</param>
        /// <param name="desktopRectangle">Desktop area of the display in desktop coordinates.</param>
        /// <param name="scanOutRectangle">Scan-out area of the display relative to desktopRect.</param>
        public static void GetScanOutConfiguration(
            uint displayId,
            out Rectangle desktopRectangle,
            out Rectangle scanOutRectangle)
        {
            var instance1 = typeof(Rectangle).Instantiate<Rectangle>();
            var instance2 = typeof(Rectangle).Instantiate<Rectangle>();

            using (var desktopRectangleReference = ValueTypeReference.FromValueType(instance1))
            {
                using (var scanOutRectangleReference = ValueTypeReference.FromValueType(instance2))
                {
                    var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_GetScanOutConfiguration>()(
                        displayId,
                        desktopRectangleReference,
                        scanOutRectangleReference
                    );

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    desktopRectangle = desktopRectangleReference.ToValueType<Rectangle>().GetValueOrDefault();
                    scanOutRectangle = scanOutRectangleReference.ToValueType<Rectangle>().GetValueOrDefault();
                }
            }
        }

        /// <summary>
        ///     This API queries current state of the intensity feature on the specified display.
        /// </summary>
        /// <param name="displayId">Combined physical display and GPU identifier of the display to query the configuration.</param>
        /// <returns>Intensity state data.</returns>
        public static ScanOutIntensityStateV1 GetScanOutIntensityState(uint displayId)
        {
            var instance = typeof(ScanOutIntensityStateV1).Instantiate<ScanOutIntensityStateV1>();

            using (var scanOutIntensityReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_GetScanOutIntensityState>()(
                    displayId,
                    scanOutIntensityReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return scanOutIntensityReference.ToValueType<ScanOutIntensityStateV1>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This API queries current state of the warping feature on the specified display.
        /// </summary>
        /// <param name="displayId">Combined physical display and GPU identifier of the display to query the configuration.</param>
        /// <returns>The warping state data.</returns>
        public static ScanOutWarpingStateV1 GetScanOutWarpingState(uint displayId)
        {
            var instance = typeof(ScanOutWarpingStateV1).Instantiate<ScanOutWarpingStateV1>();

            using (var scanOutWarpingReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_GetScanOutWarpingState>()(
                    displayId,
                    scanOutWarpingReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return scanOutWarpingReference.ToValueType<ScanOutWarpingStateV1>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This API lets caller enumerate all the supported NVIDIA display views - nView and DualView modes.
        /// </summary>
        /// <param name="display">
        ///     NVIDIA Display selection. It can be DisplayHandle.DefaultHandle or a handle enumerated from
        ///     DisplayApi.EnumNVidiaDisplayHandle().
        /// </param>
        /// <returns>Array of supported views.</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid input parameter.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static TargetViewMode[] GetSupportedViews(DisplayHandle display)
        {
            var getSupportedViews = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetSupportedViews>();
            uint allAvailable = 0;
            var status = getSupportedViews(display, ValueTypeArray.Null, ref allAvailable);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            if (allAvailable == 0)
            {
                return new TargetViewMode[0];
            }

            if (!getSupportedViews.Accepts().Contains(typeof(TargetViewMode)))
            {
                throw new NVIDIANotSupportedException("This operation is not supported.");
            }

            using (
                var viewModes =
                    ValueTypeArray.FromArray(TargetViewMode.Standard.Repeat((int)allAvailable).Cast<object>(),
                        typeof(TargetViewMode).GetEnumUnderlyingType()))
            {
                status = getSupportedViews(display, viewModes, ref allAvailable);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return viewModes.ToArray<TargetViewMode>((int)allAvailable,
                    typeof(TargetViewMode).GetEnumUnderlyingType());
            }
        }

        /// <summary>
        ///     This function calculates the timing from the visible width/height/refresh-rate and timing type info.
        /// </summary>
        /// <param name="displayId">Display ID of the display.</param>
        /// <param name="timingInput">Inputs used for calculating the timing.</param>
        /// <returns>An instance of the <see cref="Timing" /> structure.</returns>
        public static Timing GetTiming(uint displayId, TimingInput timingInput)
        {
            var instance = typeof(Timing).Instantiate<Timing>();

            using (var timingInputReference = ValueTypeReference.FromValueType(timingInput))
            {
                using (var timingReference = ValueTypeReference.FromValueType(instance))
                {
                    var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_GetTiming>()(
                        displayId,
                        timingInputReference,
                        timingReference
                    );

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return timingReference.ToValueType<Timing>().GetValueOrDefault();
                }
            }
        }

        /// <summary>
        ///     This function is used to set data for Adaptive Sync Display.
        /// </summary>
        /// <param name="displayId">Display ID of the display.</param>
        /// <param name="adaptiveSyncData">SetAdaptiveSyncData containing the information about the values of parameters that are to be set on given display.</param>
        /// <returns>An instance of the <see cref="Timing" /> structure.</returns>
        public static void GetAdaptiveSyncData(uint displayId, out GetAdaptiveSyncData adaptiveSyncData)
        {
            var getAdaptiveSyncData = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_GetAdaptiveSyncData>();

            adaptiveSyncData = typeof(GetAdaptiveSyncData).Instantiate<GetAdaptiveSyncData>();

            using (var adaptiveSyncDataReference = ValueTypeReference.FromValueType(adaptiveSyncData))
            {
                var status = getAdaptiveSyncData(displayId, adaptiveSyncDataReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                adaptiveSyncData = adaptiveSyncDataReference.ToValueType<GetAdaptiveSyncData>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This function is used to set data for Adaptive Sync Display.
        /// </summary>
        /// <param name="displayId">Display ID of the display.</param>
        /// <param name="adaptiveSyncData">SetAdaptiveSyncData containing the information about the values of parameters that are to be set on given display.</param>
        /// <returns>An instance of the <see cref="Timing" /> structure.</returns>
        public static void SetAdaptiveSyncData(uint displayId, GetAdaptiveSyncData adaptiveSyncData)
        {
            var setAdaptiveSyncData = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_SetAdaptiveSyncData>();

            using (var adaptiveSyncDataReference = ValueTypeReference.FromValueType(adaptiveSyncData))
            {
                var status = setAdaptiveSyncData(displayId, adaptiveSyncDataReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                adaptiveSyncData = adaptiveSyncDataReference.ToValueType<GetAdaptiveSyncData>().GetValueOrDefault();
            }
        }



        /// <summary>
        ///     This function returns the display name given, for example, "\\DISPLAY1", using the unattached NVIDIA display handle
        /// </summary>
        /// <param name="display">Handle of the associated unattached display</param>
        /// <returns>Name of the display</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Display handle is null.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA device maps to that display name.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static string GetUnAttachedAssociatedDisplayName(UnAttachedDisplayHandle display)
        {
            var getUnAttachedAssociatedDisplayName =
                DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GetUnAttachedAssociatedDisplayName>();
            var status = getUnAttachedAssociatedDisplayName(display, out var displayName);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return displayName.Value;
        }

        /// <summary>
        ///     This API controls the InfoFrame values.
        /// </summary>
        /// <param name="displayId">The targeted display id.</param>
        /// <param name="infoFrameData">The structure to be filled with information requested or applied on the display.</param>
        public static void InfoFrameControl(uint displayId, ref InfoFrameData infoFrameData)
        {
            var infoFrameControl = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_Disp_InfoFrameControl>();

            using (var infoFrameDataReference = ValueTypeReference.FromValueType(infoFrameData))
            {
                var status = infoFrameControl(displayId, infoFrameDataReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                infoFrameData = infoFrameDataReference.ToValueType<InfoFrameData>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This API is used to restore the display configuration, that was changed by calling <see cref="TryCustomDisplay" />.
        ///     This function must be called only after a custom display configuration is tested on the hardware, using
        ///     <see cref="TryCustomDisplay" />, otherwise no action is taken.
        ///     On Vista, <see cref="RevertCustomDisplayTrial" /> should be called with an active display that was affected during
        ///     the <see cref="TryCustomDisplay" /> call, per GPU.
        /// </summary>
        /// <param name="displayIds">Array of display ids on which custom display configuration is to be reverted.</param>
        public static void RevertCustomDisplayTrial(uint[] displayIds)
        {
            if (displayIds.Length == 0)
            {
                return;
            }

            using (var displayIdsReference = ValueTypeArray.FromArray(displayIds))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_RevertCustomDisplayTrial>()(
                    displayIdsReference,
                    (uint)displayIds.Length
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     This API configures High Dynamic Range (HDR) and Extended Dynamic Range (EDR) output.
        /// </summary>
        /// <param name="displayId">The targeted display id.</param>
        /// <param name="hdrColorData">The structure to be filled with information requested or applied on the display.</param>
        public static void HDRColorControl<THDRColorData>(uint displayId, ref THDRColorData hdrColorData)
            where THDRColorData : struct, IHDRColorData
        {
            var c = hdrColorData as IHDRColorData;
            HDRColorControl(displayId, ref c);
            hdrColorData = (THDRColorData)c;
        }

        /// <summary>
        ///     This API configures High Dynamic Range (HDR) and Extended Dynamic Range (EDR) output.
        /// </summary>
        /// <param name="displayId">The targeted display id.</param>
        /// <param name="hdrColorData">The structure to be filled with information requested or applied on the display.</param>
        public static void HDRColorControl(uint displayId, ref IHDRColorData hdrColorData)
        {
            var hdrColorControl = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_Disp_HdrColorControl>();
            var type = hdrColorData.GetType();

            if (!hdrColorControl.Accepts().Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            using (var hdrColorDataReference = ValueTypeReference.FromValueType(hdrColorData, type))
            {
                var status = hdrColorControl(displayId, hdrColorDataReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                hdrColorData = hdrColorDataReference.ToValueType<IHDRColorData>(type);
            }
        }

        /// <summary>
        ///     This function saves the current hardware display configuration on the specified Display IDs as a custom display
        ///     configuration.
        ///     This function should be called right after <see cref="TryCustomDisplay" /> to save the custom display from the
        ///     current hardware context.
        ///     This function will not do anything if the custom display configuration is not tested on the hardware.
        /// </summary>
        /// <param name="displayIds">Array of display ids on which custom display configuration is to be saved.</param>
        /// <param name="isThisOutputIdOnly">
        ///     If set, the saved custom display will only be applied on the monitor with the same
        ///     outputId.
        /// </param>
        /// <param name="isThisMonitorOnly">
        ///     If set, the saved custom display will only be applied on the monitor with the same EDID
        ///     ID or the same TV connector in case of analog TV.
        /// </param>
        public static void SaveCustomDisplay(uint[] displayIds, bool isThisOutputIdOnly, bool isThisMonitorOnly)
        {
            if (displayIds.Length == 0)
            {
                return;
            }

            using (var displayIdsReference = ValueTypeArray.FromArray(displayIds))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_SaveCustomDisplay>()(
                    displayIdsReference,
                    (uint)displayIds.Length,
                    isThisOutputIdOnly ? 1 : (uint)0,
                    isThisMonitorOnly ? 1 : (uint)0
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     This API configures High Dynamic Range (HDR) and Extended Dynamic Range (EDR) output.
        /// </summary>
        /// <param name="displayId">The targeted display id.</param>
        /// <param name="colorDatas">The list of structures to be filled with information requested or applied on the display.</param>
        /// <returns>The structure that succeed in requesting information or used for applying configuration on the display.</returns>
        // ReSharper disable once IdentifierTypo
        public static IHDRColorData HDRColorControl(uint displayId, IHDRColorData[] colorDatas)
        {
            foreach (var colorData in colorDatas)
            {
                try
                {
                    var c = colorData;
                    HDRColorControl(displayId, ref c);

                    return c;
                }
                catch (NVIDIAApiException e)
                {
                    if (e.Status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    throw;
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This API lets caller apply a global display configuration across multiple GPUs.
        ///     If all sourceIds are zero, then NvAPI will pick up sourceId's based on the following criteria :
        ///     - If user provides SourceModeInfo then we are trying to assign 0th SourceId always to GDIPrimary.
        ///     This is needed since active windows always moves along with 0th sourceId.
        ///     - For rest of the paths, we are incrementally assigning the SourceId per adapter basis.
        ///     - If user doesn't provide SourceModeInfo then NVAPI just picks up some default SourceId's in incremental order.
        ///     Note : NVAPI will not intelligently choose the SourceIDs for any configs that does not need a mode-set.
        /// </summary>
        /// <param name="pathInfos">Array of path information</param>
        /// <param name="flags">Flags for applying settings</param>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: NVAPI not initialized</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid input parameter.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static void SetDisplayConfig(PathInfoV2[] pathInfos, DisplayConfigFlags flags)
        {
            var setDisplayConfig = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_SetDisplayConfig>();

            if (pathInfos.Length > 0 && !setDisplayConfig.Accepts().Contains(pathInfos[0].GetType()))
            {
                throw new NVIDIANotSupportedException("This operation is not supported.");
            }

            using (var arrayReference = ValueTypeArray.FromArray(pathInfos))
            {
                var status = setDisplayConfig((uint)pathInfos.Length, arrayReference, flags);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API sets the current saturation level for the Digital Vibrance Control
        /// </summary>
        /// <param name="display">
        ///     The targeted display's handle.
        /// </param>
        /// <param name="currentLevel">
        ///     The saturation level to be set.
        /// </param>
        public static void SetDVCLevel(DisplayHandle display, int currentLevel)
        {
            var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_SetDVCLevel>()(
                display,
                OutputId.Invalid,
                currentLevel
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API sets the current saturation level for the Digital Vibrance Control
        /// </summary>
        /// <param name="displayId">
        ///     The targeted display output id.
        /// </param>
        /// <param name="currentLevel">
        ///     The saturation level to be set.
        /// </param>
        public static void SetDVCLevel(OutputId displayId, int currentLevel)
        {
            var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_SetDVCLevel>()(
                DisplayHandle.DefaultHandle,
                displayId,
                currentLevel
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API sets the current saturation level for the Digital Vibrance Control.
        ///     The difference between this API and the 'SetDVCLevel()' includes the possibility to set under saturated
        ///     levels.
        /// </summary>
        /// <param name="display">
        ///     The targeted display's handle.
        /// </param>
        /// <param name="currentLevel">
        ///     The saturation level to be set.
        /// </param>
        public static void SetDVCLevelEx(DisplayHandle display, int currentLevel)
        {
            var instance = new PrivateDisplayDVCInfoEx(currentLevel);

            using (var displayDVCInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_SetDVCLevelEx>()(
                    display,
                    OutputId.Invalid,
                    displayDVCInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API sets the current saturation level for the Digital Vibrance Control.
        ///     The difference between this API and the 'SetDVCLevel()' includes the possibility to set under saturated
        ///     levels.
        /// </summary>
        /// <param name="displayId">
        ///     The targeted display output id.
        /// </param>
        /// <param name="currentLevel">
        ///     The saturation level to be set.
        /// </param>
        public static void SetDVCLevelEx(OutputId displayId, int currentLevel)
        {
            var instance = new PrivateDisplayDVCInfoEx(currentLevel);

            using (var displayDVCInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_SetDVCLevelEx>()(
                    DisplayHandle.DefaultHandle,
                    displayId,
                    displayDVCInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API sets the current HUE angle
        /// </summary>
        /// <param name="display">
        ///     The targeted display's handle.
        /// </param>
        /// <param name="currentAngle">
        ///     The HUE angle to be set.
        /// </param>
        public static void SetHUEAngle(DisplayHandle display, int currentAngle)
        {
            var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_SetHUEAngle>()(
                display,
                OutputId.Invalid,
                currentAngle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// [PRIVATE]
        /// <summary>
        ///     This API sets the current HUE angle
        /// </summary>
        /// <param name="displayId">
        ///     The targeted display output id.
        /// </param>
        /// <param name="currentAngle">
        ///     The HUE angle to be set.
        /// </param>
        public static void SetHUEAngle(OutputId displayId, int currentAngle)
        {
            var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_SetHUEAngle>()(
                DisplayHandle.DefaultHandle,
                displayId,
                currentAngle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This function overrides the refresh rate on the given display.
        ///     The new refresh rate can be applied right away in this API call or deferred to be applied with the next OS
        ///     mode-set.
        ///     The override is good for only one mode-set (regardless whether it's deferred or immediate).
        /// </summary>
        /// <param name="display">The display handle to override refresh rate of.</param>
        /// <param name="refreshRate">The override refresh rate.</param>
        /// <param name="isDeferred">
        ///     A boolean value indicating if the refresh rate override should be deferred to the next OS
        ///     mode-set.
        /// </param>
        public static void SetRefreshRateOverride(DisplayHandle display, float refreshRate, bool isDeferred)
        {
            var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_SetRefreshRateOverride>()(
                display,
                OutputId.Invalid,
                refreshRate,
                isDeferred ? 1u : 0
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This function overrides the refresh rate on the given output mask.
        ///     The new refresh rate can be applied right away in this API call or deferred to be applied with the next OS
        ///     mode-set.
        ///     The override is good for only one mode-set (regardless whether it's deferred or immediate).
        /// </summary>
        /// <param name="outputMask">The output(s) to override refresh rate of.</param>
        /// <param name="refreshRate">The override refresh rate.</param>
        /// <param name="isDeferred">
        ///     A boolean value indicating if the refresh rate override should be deferred to the next OS
        ///     mode-set.
        /// </param>
        public static void SetRefreshRateOverride(OutputId outputMask, float refreshRate, bool isDeferred)
        {
            var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_SetRefreshRateOverride>()(
                DisplayHandle.DefaultHandle,
                outputMask,
                refreshRate,
                isDeferred ? 1u : 0
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets various parameters that configure the scan-out composition feature on the specified display.
        /// </summary>
        /// <param name="displayId">Combined physical display and GPU identifier of the display to apply the intensity control.</param>
        /// <param name="parameter">The scan-out composition parameter to be set.</param>
        /// <param name="parameterValue">The value to be set for the specified parameter.</param>
        /// <param name="container">Additional container for data associated with the specified parameter.</param>
        // ReSharper disable once TooManyArguments
        public static void SetScanOutCompositionParameter(
            uint displayId,
            ScanOutCompositionParameter parameter,
            ScanOutCompositionParameterValue parameterValue,
            ref float container)
        {
            var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_SetScanOutCompositionParameter>()(
                displayId,
                parameter,
                parameterValue,
                ref container
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API enables and sets up per-pixel intensity feature on the specified display.
        /// </summary>
        /// <param name="displayId">Combined physical display and GPU identifier of the display to apply the intensity control.</param>
        /// <param name="scanOutIntensity">The intensity texture info.</param>
        /// <param name="isSticky">Indicates whether the settings will be kept over a reboot.</param>
        public static void SetScanOutIntensity(uint displayId, IScanOutIntensity scanOutIntensity, out bool isSticky)
        {
            Status status;
            int isStickyInt;

            if (scanOutIntensity == null)
            {
                status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_SetScanOutIntensity>()(
                    displayId,
                    ValueTypeReference.Null,
                    out isStickyInt
                );
            }
            else
            {
                using (var scanOutWarpingReference =
                    ValueTypeReference.FromValueType(scanOutIntensity, scanOutIntensity.GetType()))
                {
                    status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_SetScanOutIntensity>()(
                        displayId,
                        scanOutWarpingReference,
                        out isStickyInt
                    );
                }
            }

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            isSticky = isStickyInt > 0;
        }

        /// <summary>
        ///     This API enables and sets up the warping feature on the specified display.
        /// </summary>
        /// <param name="displayId">Combined physical display and GPU identifier of the display to apply the intensity control.</param>
        /// <param name="scanOutWarping">The warping data info.</param>
        /// <param name="maximumNumberOfVertices">The maximum number of vertices.</param>
        /// <param name="isSticky">Indicates whether the settings will be kept over a reboot.</param>
        // ReSharper disable once TooManyArguments
        public static void SetScanOutWarping(
            uint displayId,
            ScanOutWarpingV1? scanOutWarping,
            ref int maximumNumberOfVertices,
            out bool isSticky)
        {
            Status status;
            int isStickyInt;

            if (scanOutWarping == null || maximumNumberOfVertices == 0)
            {
                maximumNumberOfVertices = 0;
                status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_SetScanOutWarping>()(
                    displayId,
                    ValueTypeReference.Null,
                    ref maximumNumberOfVertices,
                    out isStickyInt
                );
            }
            else
            {
                using (var scanOutWarpingReference = ValueTypeReference.FromValueType(scanOutWarping.Value))
                {
                    status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_GPU_SetScanOutWarping>()(
                        displayId,
                        scanOutWarpingReference,
                        ref maximumNumberOfVertices,
                        out isStickyInt
                    );
                }
            }

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            isSticky = isStickyInt > 0;
        }

        /// <summary>
        ///     This API is used to set up a custom display without saving the configuration on multiple displays.
        /// </summary>
        /// <param name="displayIdCustomDisplayPairs">A list of display ids with corresponding custom display instances.</param>
        public static void TryCustomDisplay(IDictionary<uint, CustomDisplay> displayIdCustomDisplayPairs)
        {
            if (displayIdCustomDisplayPairs.Count == 0)
            {
                return;
            }

            using (var displayIdsReference = ValueTypeArray.FromArray(displayIdCustomDisplayPairs.Keys.ToArray()))
            {
                using (var customDisplaysReference =
                    ValueTypeArray.FromArray(displayIdCustomDisplayPairs.Values.ToArray()))
                {
                    var status = DelegateFactory.GetDelegate<DisplayDelegates.NvAPI_DISP_TryCustomDisplay>()(
                        displayIdsReference,
                        (uint)displayIdCustomDisplayPairs.Count,
                        customDisplaysReference
                    );

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }
                }
            }
        }


            /// <summary>
        ///     This API enables or disables the current Mosaic topology based on the setting of the incoming 'enable' parameter.
        ///     An "enable" setting enables the current (previously set) Mosaic topology.
        ///     Note that when the current Mosaic topology is retrieved, it must have an isPossible value of true or an error will
        ///     occur.
        ///     A "disable" setting disables the current Mosaic topology.
        ///     The topology information will persist, even across reboots.
        ///     To re-enable the Mosaic topology, call this function again with the enable parameter set to true.
        /// </summary>
        /// <param name="enable">true to enable the current Mosaic topo, false to disable it.</param>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: Mosaic is not supported with the existing hardware.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.TopologyNotPossible: The current topology is not currently possible.</exception>
        /// <exception cref="NVIDIAApiException">Status.ModeChangeFailed: There was an error changing the display mode.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        public static void EnableCurrentTopology(bool enable)
        {
            var status =
                DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_EnableCurrentTopo>()((uint)(enable ? 1 : 0));

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     Enumerates the current active grid topologies. This includes Mosaic, IG, and Panoramic topologies, as well as
        ///     single displays.
        /// </summary>
        /// <returns>The list of active grid topologies.</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static GridTopologyV2[] EnumDisplayGrids()
        {
            var mosaicEnumDisplayGrids =
                DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_EnumDisplayGrids>();

            var totalAvailable = 0u;
            var status = mosaicEnumDisplayGrids(ValueTypeArray.Null, ref totalAvailable);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            if (totalAvailable == 0)
            {
                return new GridTopologyV2[0];
            }

            var instance = typeof(GridTopologyV2).Instantiate<GridTopologyV2>().Repeat((int)totalAvailable);

            using (
                var gridTopologiesByRef = ValueTypeArray.FromArray(instance.Cast<object>().AsEnumerable(),typeof(GridTopologyV2)))
            {
                status = mosaicEnumDisplayGrids(gridTopologiesByRef, ref totalAvailable);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return gridTopologiesByRef.ToArray<GridTopologyV2>((int)totalAvailable);
                //return gridTopologiesByRef.ToArray<GridTopologyV2>((int)GridTopologyV2.MaxDisplays);

            }
            
            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     Determines the set of available display modes for a given grid topology.
        /// </summary>
        /// <param name="gridTopology">The grid topology to use.</param>
        /// <returns></returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static DisplaySettingsV2[] EnumDisplayModes(GridTopologyV2 gridTopology)
        {
            var mosaicEnumDisplayModes = DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_EnumDisplayModes>();

            using (var gridTopologyByRef = ValueTypeReference.FromValueType(gridTopology, gridTopology.GetType()))
            {
                var totalAvailable = 0u;
                var status = mosaicEnumDisplayModes(gridTopologyByRef, ValueTypeArray.Null, ref totalAvailable);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                if (totalAvailable == 0)
                {
                    return new DisplaySettingsV2[0];
                }

                foreach (var acceptType in mosaicEnumDisplayModes.Accepts(2))
                {
                    var counts = totalAvailable;
                    var instance = acceptType.Instantiate<IDisplaySettings>();

                    using (
                        var displaySettingByRef =
                            ValueTypeArray.FromArray(instance.Repeat((int)counts).AsEnumerable()))
                    {
                        status = mosaicEnumDisplayModes(gridTopologyByRef, displaySettingByRef, ref counts);

                        if (status == Status.IncompatibleStructureVersion)
                        {
                            continue;
                        }

                        if (status != Status.Ok)
                        {
                            throw new NVIDIAApiException(status);
                        }

                        return displaySettingByRef.ToArray<DisplaySettingsV2>((int)counts, acceptType);
                    }
                }

                throw new NVIDIANotSupportedException("This operation is not supported.");
            }
        }

        /// <summary>
        ///     This API returns information for the current Mosaic topology.
        ///     This includes topology, display settings, and overlap values.
        ///     You can call NvAPI_Mosaic_GetTopoGroup() with the topology if you require more information.
        ///     If there isn't a current topology, then TopologyBrief.Topology will be Topology.None.
        /// </summary>
        /// <param name="topoBrief">The current Mosaic topology</param>
        /// <param name="displaySettings">The current per-display settings</param>
        /// <param name="overlapX">The pixel overlap between horizontal displays</param>
        /// <param name="overlapY">The pixel overlap between vertical displays</param>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: Mosaic is not supported with the existing hardware.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        // ReSharper disable once TooManyArguments
        public static void GetCurrentTopology(
            out TopologyBrief topoBrief,
            out IDisplaySettings displaySettings,
            out int overlapX,
            out int overlapY)
        {
            var mosaicGetCurrentTopo = DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_GetCurrentTopo>();
            topoBrief = typeof(TopologyBrief).Instantiate<TopologyBrief>();

            foreach (var acceptType in mosaicGetCurrentTopo.Accepts())
            {
                displaySettings = acceptType.Instantiate<IDisplaySettings>();

                using (var displaySettingsByRef = ValueTypeReference.FromValueType(displaySettings, acceptType))
                {
                    var status = mosaicGetCurrentTopo(ref topoBrief, displaySettingsByRef, out overlapX, out overlapY);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    displaySettings = displaySettingsByRef.ToValueType<IDisplaySettings>(acceptType);

                    return;
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This API returns the X and Y overlap limits required if the given Mosaic topology and display settings are to be
        ///     used.
        /// </summary>
        /// <param name="topoBrief">
        ///     The topology for getting limits This must be one of the topo briefs returned from
        ///     GetSupportedTopoInfo().
        /// </param>
        /// <param name="displaySettings">
        ///     The display settings for getting the limits. This must be one of the settings returned
        ///     from GetSupportedTopoInfo().
        /// </param>
        /// <param name="minOverlapX">X overlap minimum</param>
        /// <param name="maxOverlapX">X overlap maximum</param>
        /// <param name="minOverlapY">Y overlap minimum</param>
        /// <param name="maxOverlapY">Y overlap maximum</param>
        /// <exception cref="ArgumentException">displaySettings is of invalid type.</exception>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: Mosaic is not supported with the existing hardware.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        // ReSharper disable once TooManyArguments
        public static void GetOverlapLimits(
            TopologyBrief topoBrief,
            IDisplaySettings displaySettings,
            out int minOverlapX,
            out int maxOverlapX,
            out int minOverlapY,
            out int maxOverlapY)
        {
            var mosaicGetOverlapLimits = DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_GetOverlapLimits>();

            if (!mosaicGetOverlapLimits.Accepts().Contains(displaySettings.GetType()))
            {
                throw new ArgumentException("Parameter type is not supported.", nameof(displaySettings));
            }

            using (
                var displaySettingsByRef = ValueTypeReference.FromValueType(displaySettings, displaySettings.GetType()))
            {
                var status = mosaicGetOverlapLimits(topoBrief, displaySettingsByRef, out minOverlapX, out maxOverlapX,
                    out minOverlapY, out maxOverlapY);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     This API returns information on the topologies and display resolutions supported by Mosaic mode.
        ///     NOTE: Not all topologies returned can be set immediately. Some of the topologies returned might not be valid for
        ///     one reason or another.  It could be due to mismatched or missing displays.  It could also be because the required
        ///     number of GPUs is not found.
        ///     Once you get the list of supported topologies, you can call GetTopologyGroup() with one of the Mosaic topologies if
        ///     you need more information about it.
        ///     It is possible for this function to return NVAPI_OK with no topologies listed in the return structure.  If this is
        ///     the case, it means that the current hardware DOES support Mosaic, but with the given configuration no valid
        ///     topologies were found.  This most likely means that SLI was not enabled for the hardware. Once enabled, you should
        ///     see valid topologies returned from this function.
        /// </summary>
        /// <param name="topologyType">The type of topologies the caller is interested in getting.</param>
        /// <returns>Information about what topologies and display resolutions are supported for Mosaic.</returns>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: Mosaic is not supported with the existing hardware.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: TopologyType is invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry-point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static ISupportedTopologiesInfo GetSupportedTopologiesInfo(TopologyType topologyType)
        {
            var mosaicGetSupportedTopoInfo =
                DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_GetSupportedTopoInfo>();

            foreach (var acceptType in mosaicGetSupportedTopoInfo.Accepts())
            {
                var instance = acceptType.Instantiate<ISupportedTopologiesInfo>();

                using (var supportedTopologiesInfoByRef = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = mosaicGetSupportedTopoInfo(supportedTopologiesInfoByRef, topologyType);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return supportedTopologiesInfoByRef.ToValueType<ISupportedTopologiesInfo>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This API returns a structure filled with the details of the specified Mosaic topology.
        ///     If the pTopoBrief passed in matches the current topology, then information in the brief and group structures will
        ///     reflect what is current. Thus the brief would have the current 'enable' status, and the group would have the
        ///     current overlap values. If there is no match, then the returned brief has an 'enable' status of FALSE (since it is
        ///     obviously not enabled), and the overlap values will be 0.
        /// </summary>
        /// <param name="topoBrief">
        ///     The topology for getting the details. This must be one of the topology briefs returned from
        ///     GetSupportedTopoInfo().
        /// </param>
        /// <returns>The topology details matching the brief</returns>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: Mosaic is not supported with the existing hardware.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        public static TopologyGroup GetTopologyGroup(TopologyBrief topoBrief)
        {
            var result = typeof(TopologyGroup).Instantiate<TopologyGroup>();
            var status =
                DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_GetTopoGroup>()(topoBrief, ref result);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return result;
        }


        /// <summary>
        ///     This API returns the viewports that would be applied on the requested display.
        /// </summary>
        /// <param name="displayId">
        ///     Display ID of a single display in the active mosaic topology to query.
        /// </param>
        /// <param name="srcWidth">
        ///     Width of full display topology. If both width and height are 0, the current resolution is used.
        /// </param>
        /// <param name="srcHeight">
        ///     Height of full display topology. If both width and height are 0, the current resolution is used.
        /// </param>
        /// <param name="viewports">
        ///     Array of NV_RECT viewports. SUPPORTED OS: Windows 10 and higher. If the requested resolution is a single-wide
        ///     resolution, only viewports[0] will contain the viewport details, regardless of which display is driving the display.
        /// </param>
        /// <param name="bezelCorrected">
        ///     Returns 1 if the requested resolution is bezel corrected. May be NULL.
        /// </param>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.MosaicNotACtive: The display does not belong to an active Mosaic Topology.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        public static void GetDisplayViewportsByResolution(UInt32 displayId, UInt32 srcWidth, UInt32 srcHeight, out ViewPortF[] viewports, out byte bezelCorrected)
        {
            int count = (int)NvConstants.NV_MOSAIC_MAX_DISPLAYS;
            //var instances = typeof(ViewPortF).Instantiate<ViewPortF>().Repeat(count);

            ValueTypeArray viewportReference = new ValueTypeArray();

            bezelCorrected = 0;

            var status =
            DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_GetDisplayViewportsByResolution>()(displayId, srcWidth, srcHeight, out viewportReference, out bezelCorrected);

            if (status != Status.Ok)
            {               
                throw new NVIDIAApiException(status);
            }

            viewports = viewportReference.ToArray<ViewPortF>(count);

        }


        /// <summary>
        ///     This API sets the Mosaic topology and performs a mode switch using the given display settings.
        /// </summary>
        /// <param name="topoBrief">
        ///     The topology to set. This must be one of the topologies returned from GetSupportedTopoInfo(),
        ///     and it must have an isPossible value of true.
        /// </param>
        /// <param name="displaySettings">
        ///     The per display settings to be used in the Mosaic mode. This must be one of the settings
        ///     returned from GetSupportedTopoInfo().
        /// </param>
        /// <param name="overlapX">
        ///     The pixel overlap to use between horizontal displays (use positive a number for overlap, or a
        ///     negative number to create a gap.) If the overlap is out of bounds for what is possible given the topo and display
        ///     setting, the overlap will be clamped.
        /// </param>
        /// <param name="overlapY">
        ///     The pixel overlap to use between vertical displays (use positive a number for overlap, or a
        ///     negative number to create a gap.) If the overlap is out of bounds for what is possible given the topo and display
        ///     setting, the overlap will be clamped.
        /// </param>
        /// <param name="enable">
        ///     If true, the topology being set will also be enabled, meaning that the mode set will occur. If
        ///     false, you don't want to be in Mosaic mode right now, but want to set the current Mosaic topology so you can enable
        ///     it later with EnableCurrentTopo()
        /// </param>
        /// <exception cref="ArgumentException">displaySettings is of invalid type.</exception>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: Mosaic is not supported with the existing hardware.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        // ReSharper disable once TooManyArguments
        public static void SetCurrentTopology(
            TopologyBrief topoBrief,
            IDisplaySettings displaySettings,
            int overlapX,
            int overlapY,
            bool enable)
        {
            var mosaicSetCurrentTopo = DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_SetCurrentTopo>();

            if (!mosaicSetCurrentTopo.Accepts().Contains(displaySettings.GetType()))
            {
                throw new ArgumentException("Parameter type is not supported.", nameof(displaySettings));
            }

            using (
                var displaySettingsByRef = ValueTypeReference.FromValueType(displaySettings, displaySettings.GetType()))
            {
                var status = mosaicSetCurrentTopo(topoBrief, displaySettingsByRef, overlapX, overlapY,
                    (uint)(enable ? 1 : 0));

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     Sets a new display topology, replacing any existing topologies that use the same displays.
        ///     This function will look for an SLI configuration that will allow the display topology to work.
        ///     To revert to a single display, specify that display as a 1x1 grid.
        /// </summary>
        /// <param name="gridTopologies">The topology details to set.</param>
        /// <param name="flags">One of the SetDisplayTopologyFlag flags</param>
        /// <exception cref="NVIDIAApiException">Status.TopologyNotPossible: One or more of the display grids are not valid.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoActiveSLITopology: No matching GPU topologies could be found.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        public static void SetDisplayGrids(
            GridTopologyV2[] gridTopologies,
            SetDisplayTopologyFlag flags = SetDisplayTopologyFlag.NoFlag)
        {

            using (var gridTopologiesByRef = ValueTypeArray.FromArray(gridTopologies))
            {
                var status =
                    DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_SetDisplayGrids>()(gridTopologiesByRef,
                        (uint)gridTopologies.Length,
                        flags);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     Determines if a list of grid topologies is valid. It will choose an SLI configuration in the same way that
        ///     SetDisplayGrids() does.
        ///     On return, each element in the pTopoStatus array will contain any errors or warnings about each grid topology. If
        ///     any error flags are set, then the topology is not valid. If any warning flags are set, then the topology is valid,
        ///     but sub-optimal.
        ///     If the ALLOW_INVALID flag is set, then it will continue to validate the grids even if no SLI configuration will
        ///     allow all of the grids. In this case, a grid grid with no matching GPU topology will have the error flags
        ///     NO_GPU_TOPOLOGY or NOT_SUPPORTED set.
        ///     If the ALLOW_INVALID flag is not set and no matching SLI configuration is found, then it will skip the rest of the
        ///     validation and throws a NVIDIAApiException with Status.NoActiveSLITopology.
        /// </summary>
        /// <param name="gridTopologies">The array of grid topologies to verify.</param>
        /// <param name="flags">One of the SetDisplayTopologyFlag flags</param>
        /// <exception cref="NVIDIAApiException">Status.NoActiveSLITopology: No matching GPU topologies could be found.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: One or more arguments passed in are invalid.</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: The NvAPI API needs to be initialized first.</exception>
        /// <exception cref="NVIDIAApiException">Status.NoImplementation: This entry point not available.</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred.</exception>
        public static DisplayTopologyStatus[] ValidateDisplayGrids(
            GridTopologyV2[] gridTopologies,
            SetDisplayTopologyFlag flags = SetDisplayTopologyFlag.NoFlag)
        {
            using (var gridTopologiesByRef = ValueTypeArray.FromArray(gridTopologies))
            {
                var statuses =
                    typeof(DisplayTopologyStatus).Instantiate<DisplayTopologyStatus>().Repeat(gridTopologies.Length);
                var status =
                    DelegateFactory.GetDelegate<MosaicDelegates.NvAPI_Mosaic_ValidateDisplayGrids>()(flags,
                        gridTopologiesByRef,
                        ref statuses, (uint)gridTopologies.Length);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return statuses;
            }
        }




        /// <summary>
        ///     This API adds an executable name to a profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <param name="application">Input <see cref="IDRSApplication" /> instance containing the executable name.</param>
        /// <returns>The newly created instance of <see cref="IDRSApplication" />.</returns>
        public static IDRSApplication CreateApplication(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle,
            IDRSApplication application)
        {
            using (var applicationReference = ValueTypeReference.FromValueType(application, application.GetType()))
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_CreateApplication>()(
                    sessionHandle,
                    profileHandle,
                    applicationReference
                );

                if (status == Status.IncompatibleStructureVersion)
                {
                    throw new NVIDIANotSupportedException("This operation is not supported.");
                }

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return applicationReference.ToValueType<IDRSApplication>(application.GetType());
            }
        }

        /// <summary>
        ///     This API creates an empty profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profile">Input to the <see cref="DRSProfileV1" /> instance.</param>
        /// <returns>The newly created profile handle.</returns>
        public static DRSProfileHandle CreateProfile(DRSSessionHandle sessionHandle, DRSProfileV1 profile)
        {
            using (var profileReference = ValueTypeReference.FromValueType(profile))
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_CreateProfile>()(
                    sessionHandle,
                    profileReference,
                    out var profileHandle
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return profileHandle;
            }
        }

        /// <summary>
        ///     This API allocates memory and initializes the session.
        /// </summary>
        /// <returns>The newly created session handle.</returns>
        public static DRSSessionHandle CreateSession()
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_CreateSession>()(out var sessionHandle);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return sessionHandle;
        }

        /// <summary>
        ///     This API removes an executable from a profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <param name="application">Input all the information about the application to be removed.</param>
        public static void DeleteApplication(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle,
            IDRSApplication application)
        {
            using (var applicationReference = ValueTypeReference.FromValueType(application, application.GetType()))
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_DeleteApplicationEx>()(
                    sessionHandle,
                    profileHandle,
                    applicationReference
                );

                if (status == Status.IncompatibleStructureVersion)
                {
                    throw new NVIDIANotSupportedException("This operation is not supported.");
                }

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     This API removes an executable name from a profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <param name="applicationName">Input the executable name to be removed.</param>
        public static void DeleteApplication(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle,
            string applicationName)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_DeleteApplication>()(
                sessionHandle,
                profileHandle,
                new UnicodeString(applicationName)
            );

            if (status == Status.IncompatibleStructureVersion)
            {
                throw new NVIDIANotSupportedException("This operation is not supported.");
            }

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API deletes a profile or sets it back to a predefined value.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        public static void DeleteProfile(DRSSessionHandle sessionHandle, DRSProfileHandle profileHandle)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_DeleteProfile>()(
                sessionHandle,
                profileHandle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API deletes a setting or sets it back to predefined value.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <param name="settingId">Input settingId to be deleted.</param>
        public static void DeleteProfileSetting(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle,
            uint settingId)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_DeleteProfileSetting>()(
                sessionHandle,
                profileHandle,
                settingId
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API frees the allocated resources for the session handle.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        public static void DestroySession(DRSSessionHandle sessionHandle)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_DestroySession>()(sessionHandle);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API enumerates all the applications in a given profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <returns>Instances of <see cref="IDRSApplication" /> with all the attributes filled.</returns>
        [SuppressMessage("ReSharper", "EventExceptionNotDocumented")]
        public static IEnumerable<IDRSApplication> EnumApplications(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle)
        {
            var maxApplicationsPerRequest = 8;
            var enumApplications = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_EnumApplications>();

            foreach (var acceptType in enumApplications.Accepts())
            {
                var i = 0u;

                while (true)
                {
                    var instances = acceptType.Instantiate<IDRSApplication>().Repeat(maxApplicationsPerRequest);

                    using (var applicationsReference = ValueTypeArray.FromArray(instances, acceptType))
                    {
                        var count = (uint)instances.Length;
                        var status = enumApplications(
                            sessionHandle,
                            profileHandle,
                            i,
                            ref count,
                            applicationsReference
                        );

                        if (status == Status.IncompatibleStructureVersion)
                        {
                            break;
                        }

                        if (status == Status.EndEnumeration)
                        {
                            yield break;
                        }

                        if (status != Status.Ok)
                        {
                            throw new NVIDIAApiException(status);
                        }

                        foreach (var application in applicationsReference.ToArray<IDRSApplication>(
                            (int)count,
                            acceptType))
                        {
                            yield return application;
                            i++;
                        }

                        if (count < maxApplicationsPerRequest)
                        {
                            yield break;
                        }
                    }
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This API enumerates all the Ids of all the settings recognized by NVAPI.
        /// </summary>
        /// <returns>An array of <see cref="uint" />s filled with the settings identification numbers of available settings.</returns>
        public static uint[] EnumAvailableSettingIds()
        {
            var settingIdsCount = (uint)ushort.MaxValue;
            var settingIds = 0u.Repeat((int)settingIdsCount);

            using (var settingIdsArray = ValueTypeArray.FromArray(settingIds))
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_EnumAvailableSettingIds>()(
                    settingIdsArray,
                    ref settingIdsCount
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return settingIdsArray.ToArray<uint>((int)settingIdsCount);
            }
        }

        /// <summary>
        ///     This API enumerates all available setting values for a given setting.
        /// </summary>
        /// <param name="settingId">Input settingId.</param>
        /// <returns>All available setting values.</returns>
        public static DRSSettingValues EnumAvailableSettingValues(uint settingId)
        {
            var settingValuesCount = (uint)DRSSettingValues.MaximumNumberOfValues;
            var settingValues = typeof(DRSSettingValues).Instantiate<DRSSettingValues>();

            using (var settingValuesReference = ValueTypeReference.FromValueType(settingValues))
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_EnumAvailableSettingValues>()(
                    settingId,
                    ref settingValuesCount,
                    settingValuesReference
                );

                if (status == Status.IncompatibleStructureVersion)
                {
                    throw new NVIDIANotSupportedException("This operation is not supported.");
                }

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return settingValuesReference.ToValueType<DRSSettingValues>(typeof(DRSSettingValues));
            }
        }

        /// <summary>
        ///     This API enumerates through all the profiles in the session.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <returns>Instances of <see cref="DRSProfileHandle" /> each representing a profile.</returns>
        public static IEnumerable<DRSProfileHandle> EnumProfiles(DRSSessionHandle sessionHandle)
        {
            var i = 0u;

            while (true)
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_EnumProfiles>()(
                    sessionHandle,
                    i,
                    out var profileHandle
                );

                if (status == Status.EndEnumeration)
                {
                    yield break;
                }

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                yield return profileHandle;
                i++;
            }
        }

        /// <summary>
        ///     This API enumerates all the settings of a given profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <returns>Instances of <see cref="DRSSettingV1" />.</returns>
        [SuppressMessage("ReSharper", "EventExceptionNotDocumented")]
        public static IEnumerable<DRSSettingV1> EnumSettings(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle)
        {
            var maxSettingsPerRequest = 32;
            var enumSettings = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_EnumSettings>();

            var i = 0u;

            while (true)
            {
                var instances = typeof(DRSSettingV1).Instantiate<DRSSettingV1>().Repeat(maxSettingsPerRequest);

                using (var applicationsReference = ValueTypeArray.FromArray(instances))
                {
                    var count = (uint)instances.Length;
                    var status = enumSettings(
                        sessionHandle,
                        profileHandle,
                        i,
                        ref count,
                        applicationsReference
                    );

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        throw new NVIDIANotSupportedException("This operation is not supported.");
                    }

                    if (status == Status.EndEnumeration)
                    {
                        yield break;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    foreach (var application in applicationsReference.ToArray<DRSSettingV1>(
                        (int)count,
                        typeof(DRSSettingV1))
                    )
                    {
                        yield return application;
                        i++;
                    }

                    if (count < maxSettingsPerRequest)
                    {
                        yield break;
                    }
                }
            }
        }

        /// <summary>
        ///     This API searches the application and the associated profile for the given application name.
        ///     If a fully qualified path is provided, this function will always return the profile
        ///     the driver will apply upon running the application (on the path provided).
        /// </summary>
        /// <param name="sessionHandle">Input to the hSession handle</param>
        /// <param name="applicationName">Input appName. For best results, provide a fully qualified path of the type</param>
        /// <param name="profileHandle">The profile handle of the profile that the found application belongs to.</param>
        /// <returns>An instance of <see cref="IDRSApplication" />.</returns>
        [SuppressMessage("ReSharper", "EventExceptionNotDocumented")]
        public static IDRSApplication FindApplicationByName(
            DRSSessionHandle sessionHandle,
            string applicationName,
            out DRSProfileHandle? profileHandle)
        {
            var findApplicationByName = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_FindApplicationByName>();

            foreach (var acceptType in findApplicationByName.Accepts())
            {
                var instance = acceptType.Instantiate<IDRSApplication>();

                using (var applicationReference = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = findApplicationByName(
                        sessionHandle,
                        new UnicodeString(applicationName),
                        out var applicationProfileHandle,
                        applicationReference
                    );

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status == Status.ExecutableNotFound)
                    {
                        profileHandle = null;

                        return null;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    profileHandle = applicationProfileHandle;

                    return applicationReference.ToValueType<IDRSApplication>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This API finds a profile in the current session.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileName">Input profileName.</param>
        /// <returns>The profile handle.</returns>
        public static DRSProfileHandle FindProfileByName(DRSSessionHandle sessionHandle, string profileName)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_FindProfileByName>()(
                sessionHandle,
                new UnicodeString(profileName),
                out var profileHandle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return profileHandle;
        }

        /// <summary>
        ///     This API gets information about the given application.  The input application name
        ///     must match exactly what the Profile has stored for the application.
        ///     This function is better used to retrieve application information from a previous
        ///     enumeration.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <param name="applicationName">Input application name.</param>
        /// <returns>
        ///     An instance of <see cref="IDRSApplication" /> with all attributes filled if found; otherwise
        ///     <see langword="null" />.
        /// </returns>
        [SuppressMessage("ReSharper", "EventExceptionNotDocumented")]
        public static IDRSApplication GetApplicationInfo(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle,
            string applicationName)
        {
            var getApplicationInfo = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_GetApplicationInfo>();

            foreach (var acceptType in getApplicationInfo.Accepts())
            {
                var instance = acceptType.Instantiate<IDRSApplication>();

                using (var applicationReference = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getApplicationInfo(
                        sessionHandle,
                        profileHandle,
                        new UnicodeString(applicationName),
                        applicationReference
                    );

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status == Status.ExecutableNotFound)
                    {
                        return null;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return applicationReference.ToValueType<IDRSApplication>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     Returns the handle to the current global profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <returns>Base profile handle.</returns>
        public static DRSProfileHandle GetBaseProfile(DRSSessionHandle sessionHandle)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_GetBaseProfile>()(
                sessionHandle,
                out var profileHandle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return profileHandle;
        }

        /// <summary>
        ///     This API returns the handle to the current global profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <returns>Current global profile handle.</returns>
        public static DRSProfileHandle GetCurrentGlobalProfile(DRSSessionHandle sessionHandle)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_GetCurrentGlobalProfile>()(
                sessionHandle,
                out var profileHandle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return profileHandle;
        }

        /// <summary>
        ///     This API obtains the number of profiles in the current session object.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <returns>Number of profiles in the current session.</returns>
        public static int GetNumberOfProfiles(DRSSessionHandle sessionHandle)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_GetNumProfiles>()(
                sessionHandle,
                out var profileCount
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return (int)profileCount;
        }

        /// <summary>
        ///     This API gets information about the given profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <returns>An instance of <see cref="DRSProfileV1" /> with all attributes filled.</returns>
        public static DRSProfileV1 GetProfileInfo(DRSSessionHandle sessionHandle, DRSProfileHandle profileHandle)
        {
            var profile = typeof(DRSProfileV1).Instantiate<DRSProfileV1>();

            using (var profileReference = ValueTypeReference.FromValueType(profile))
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_GetProfileInfo>()(
                    sessionHandle,
                    profileHandle,
                    profileReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return profileReference.ToValueType<DRSProfileV1>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This API gets information about the given setting.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <param name="settingId">Input settingId.</param>
        /// <returns>An instance of <see cref="DRSSettingV1" /> describing the setting if found; otherwise <see langword="null" />.</returns>
        public static DRSSettingV1? GetSetting(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle,
            uint settingId)
        {
            var instance = typeof(DRSSettingV1).Instantiate<DRSSettingV1>();

            using (var settingReference = ValueTypeReference.FromValueType(instance, typeof(DRSSettingV1)))
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_GetSetting>()(
                    sessionHandle,
                    profileHandle,
                    settingId,
                    settingReference
                );

                if (status == Status.IncompatibleStructureVersion)
                {
                    throw new NVIDIANotSupportedException("This operation is not supported.");
                }

                if (status == Status.SettingNotFound)
                {
                    return null;
                }

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return settingReference.ToValueType<DRSSettingV1>(typeof(DRSSettingV1));
            }
        }

        /// <summary>
        ///     This API gets the binary identification number of a setting given the setting name.
        /// </summary>
        /// <param name="settingName">Input Unicode settingName.</param>
        /// <returns>The corresponding settingId.</returns>
        public static uint GetSettingIdFromName(string settingName)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_GetSettingIdFromName>()(
                new UnicodeString(settingName),
                out var settingId
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return settingId;
        }

        /// <summary>
        ///     This API gets the setting name given the binary identification number.
        /// </summary>
        /// <param name="settingId">Input settingId.</param>
        /// <returns>Corresponding settingName.</returns>
        public static string GetSettingNameFromId(uint settingId)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_GetSettingNameFromId>()(
                settingId,
                out var settingName
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return settingName.Value;
        }

        /// <summary>
        ///     This API loads and parses the settings data.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        public static void LoadSettings(DRSSessionHandle sessionHandle)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_LoadSettings>()(sessionHandle);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API loads settings from the given file path.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle</param>
        /// <param name="fileName">Binary full file path.</param>
        public static void LoadSettings(DRSSessionHandle sessionHandle, string fileName)
        {
            var unicodeFileName = new UnicodeString(fileName);
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_LoadSettingsFromFile>()(
                sessionHandle,
                unicodeFileName
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API restores the whole system to predefined(default) values.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        public static void RestoreDefaults(DRSSessionHandle sessionHandle)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_RestoreAllDefaults>()(
                sessionHandle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }


        /// <summary>
        ///     This API restores the given profile to predefined(default) values.
        ///     Any and all user specified modifications will be removed.
        ///     If the whole profile was set by the user, the profile will be removed.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        public static void RestoreDefaults(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_RestoreProfileDefault>()(
                sessionHandle,
                profileHandle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API restores the given profile setting to predefined(default) values.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <param name="settingId">Input settingId.</param>
        public static void RestoreDefaults(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle,
            uint settingId)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_RestoreProfileDefaultSetting>()(
                sessionHandle,
                profileHandle,
                settingId
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API saves the settings data to the system.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        public static void SaveSettings(DRSSessionHandle sessionHandle)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_SaveSettings>()(sessionHandle);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API saves settings to the given file path.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="fileName">Binary full file path.</param>
        public static void SaveSettings(DRSSessionHandle sessionHandle, string fileName)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_SaveSettingsToFile>()(
                sessionHandle,
                new UnicodeString(fileName)
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets the current global profile in the driver.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileName">Input the new current global profile name.</param>
        public static void SetCurrentGlobalProfile(DRSSessionHandle sessionHandle, string profileName)
        {
            var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_SetCurrentGlobalProfile>()(
                sessionHandle,
                new UnicodeString(profileName)
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     Specifies flags for a given profile. Currently only the GPUSupport is
        ///     used to update the profile. Neither the name, number of settings or applications
        ///     or other profile information can be changed with this function.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <param name="profile">Input the new profile info.</param>
        public static void SetProfileInfo(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle,
            DRSProfileV1 profile)
        {
            using (var profileReference = ValueTypeReference.FromValueType(profile))
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_SetProfileInfo>()(
                    sessionHandle,
                    profileHandle,
                    profileReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     This API adds/modifies a setting to a profile.
        /// </summary>
        /// <param name="sessionHandle">Input to the session handle.</param>
        /// <param name="profileHandle">Input profile handle.</param>
        /// <param name="setting">
        ///     An instance of <see cref="DRSSettingV1" /> containing the setting identification number and new
        ///     value for the setting.
        /// </param>
        public static void SetSetting(
            DRSSessionHandle sessionHandle,
            DRSProfileHandle profileHandle,
            DRSSettingV1 setting)
        {
            using (var settingReference = ValueTypeReference.FromValueType(setting, setting.GetType()))
            {
                var status = DelegateFactory.GetDelegate<DRSDelegates.NvAPI_DRS_SetSetting>()(
                    sessionHandle,
                    profileHandle,
                    settingReference
                );

                if (status == Status.IncompatibleStructureVersion)
                {
                    throw new NVIDIANotSupportedException("This operation is not supported.");
                }

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }


        /// <summary>
        ///     This function returns an array of logical GPU handles.
        ///     Each handle represents one or more GPUs acting in concert as a single graphics device.
        ///     At least one GPU must be present in the system and running an NVIDIA display driver.
        ///     All logical GPUs handles get invalidated on a GPU topology change, so the calling application is required to
        ///     re-enum
        ///     the logical GPU handles to get latest physical handle mapping after every GPU topology change activated by a call
        ///     to SetGpuTopologies().
        ///     To detect if SLI rendering is enabled, use Direct3DApi.GetCurrentSLIState().
        /// </summary>
        /// <returns>Array of logical GPU handles.</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        public static LogicalGPUHandle[] EnumLogicalGPUs()
        {
            var gpuList =
                typeof(LogicalGPUHandle).Instantiate<LogicalGPUHandle>().Repeat(LogicalGPUHandle.MaxLogicalGPUs);
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_EnumLogicalGPUs>()(gpuList, out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpuList.Take((int)count).ToArray();
        }

        /// <summary>
        ///     This function returns an array of physical GPU handles.
        ///     Each handle represents a physical GPU present in the system.
        ///     That GPU may be part of an SLI configuration, or may not be visible to the OS directly.
        ///     At least one GPU must be present in the system and running an NVIDIA display driver.
        ///     Note: In drivers older than 105.00, all physical GPU handles get invalidated on a mode-set. So the calling
        ///     applications need to re-enum the handles after every mode-set. With drivers 105.00 and up, all physical GPU handles
        ///     are constant. Physical GPU handles are constant as long as the GPUs are not physically moved and the SBIOS VGA
        ///     order is unchanged.
        ///     For GPU handles in TCC MODE please use EnumTCCPhysicalGPUs()
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        public static PhysicalGPUHandle[] EnumPhysicalGPUs()
        {
            var gpuList =
                typeof(PhysicalGPUHandle).Instantiate<PhysicalGPUHandle>().Repeat(PhysicalGPUHandle.PhysicalGPUs);
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_EnumPhysicalGPUs>()(gpuList, out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpuList.Take((int)count).ToArray();
        }

        /// <summary>
        ///     This function returns an array of physical GPU handles that are in TCC Mode.
        ///     Each handle represents a physical GPU present in the system in TCC Mode.
        ///     That GPU may not be visible to the OS directly.
        ///     NOTE: Handles enumerated by this API are only valid for NvAPIs that are tagged as TCC_SUPPORTED If handle is passed
        ///     to any other API, it will fail with Status.InvalidHandle
        ///     For WDDM GPU handles please use EnumPhysicalGPUs()
        /// </summary>
        /// <returns>An array of physical GPU handles that are in TCC Mode.</returns>
        /// <exception cref="NVIDIAApiException">See NVIDIAApiException.Status for the reason of the exception.</exception>
        public static PhysicalGPUHandle[] EnumTCCPhysicalGPUs()
        {
            var gpuList =
                typeof(PhysicalGPUHandle).Instantiate<PhysicalGPUHandle>().Repeat(PhysicalGPUHandle.PhysicalGPUs);
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_EnumTCCPhysicalGPUs>()(gpuList, out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpuList.Take((int)count).ToArray();
        }

        //TODO: This GetLogicalGPUInfo won't work. It has an error with an invalid pointer when run which appears to
        //      be related to how the data in the physicalGPUHandle array is being marshalled. This is causing the 
        //      NVIDIA driver to believe there is an invaliud pointer in the logicalGPUData structure being passed to it.
        //      And it is returning an Invalid Pointer error.

        /*/// <summary>
        ///      This function is used to query Logical GPU information.
        /// </summary>
        /// <param name="gpuHandle">Logical GPU Handle.</param>
        /// <param name="logicalGPUData">Pointer to LogicalGPUData structure.</param>
        public static void GetLogicalGPUInfo(
            LogicalGPUHandle gpuHandle,
            out LogicalGPUData logicalGPUData)
        {

            logicalGPUData = typeof(LogicalGPUData).Instantiate<LogicalGPUData>();
            //logicalGPUData.OSAdapterId = new NvGUID(0, 0, 0, 0);
            logicalGPUData.PhysicalGPUHandles =
                typeof(PhysicalGPUHandle).Instantiate<PhysicalGPUHandle>().Repeat(PhysicalGPUHandle.MaxPhysicalGPUs);
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetLogicalGpuInfo>()(gpuHandle, ref logicalGPUData);


            if (status != Status.Ok)
            {
                // Free the memory we used so we don't get a memory leak
                *//*Marshal.DestroyStructure(memLocationLUID, typeof(LUID));
                Marshal.FreeHGlobal(memLocationLUID);*//*
                throw new NVIDIAApiException(status);
            }

            // Free the memory we used so we don't get a memory leak
            *//*Marshal.DestroyStructure(memLocationLUID, typeof(LUID));
            Marshal.FreeHGlobal(memLocationLUID);*//*


        }*/


        /// <summary>
        ///     This function returns the AGP aperture in megabytes.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <returns>AGP aperture in megabytes</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static int GetAGPAperture(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetAGPAperture>()(gpuHandle, out var agpAperture);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return (int)agpAperture;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the architect information for the passed physical GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The GPU handle to retrieve information for.</param>
        /// <returns>The GPU architect information.</returns>
        public static PrivateArchitectInfoV2 GetArchitectInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateArchitectInfoV2).Instantiate<PrivateArchitectInfoV2>();

            using (var architectInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetArchInfo>()(
                    gpuHandle,
                    architectInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return architectInfoReference.ToValueType<PrivateArchitectInfoV2>(
                    typeof(PrivateArchitectInfoV2));
            }
        }

        /// <summary>
        ///     This API Retrieves the Board information (a unique GPU Board Serial Number) stored in the InfoROM.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU Handle</param>
        /// <returns>Board Information</returns>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: Handle passed is not a physical GPU handle</exception>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: NVAPI not initialized</exception>
        public static BoardInfo GetBoardInfo(PhysicalGPUHandle gpuHandle)
        {
            var boardInfo = typeof(BoardInfo).Instantiate<BoardInfo>();
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetBoardInfo>()(gpuHandle, ref boardInfo);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return boardInfo;
        }

        /// <summary>
        ///     Returns the identification of the bus associated with this GPU.
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>Id of the bus</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static int GetBusId(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetBusId>()(gpuHandle, out var busId);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return (int)busId;
        }

        /// <summary>
        ///     Returns the identification of the bus slot associated with this GPU.
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>Identification of the bus slot associated with this GPU</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        public static int GetBusSlotId(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetBusSlotId>()(gpuHandle, out var busId);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return (int)busId;
        }

        /// <summary>
        ///     This function returns the type of bus associated with this GPU.
        ///     TCC_SUPPORTED
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>Type of bus associated with this GPU</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        public static GPUBusType GetBusType(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetBusType>()(gpuHandle, out var busType);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return busType;
        }

        /// <summary>
        ///     This function returns the current AGP Rate (0 = AGP not present, 1 = 1x, 2 = 2x, etc.).
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <returns>Current AGP rate</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static int GetCurrentAGPRate(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetCurrentAGPRate>()(gpuHandle, out var agpRate);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return (int)agpRate;
        }

        /// <summary>
        ///     This function returns the number of PCIE lanes being used for the PCIE interface downstream from the GPU.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <returns>PCIE lanes being used for the PCIE interface downstream</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static int GetCurrentPCIEDownStreamWidth(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetCurrentPCIEDownstreamWidth>()(gpuHandle,
                out var width);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return (int)width;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the driver model for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The driver model of the GPU.</returns>
        public static uint GetDriverModel(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GetDriverModel>()(gpuHandle, out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return count;
        }

        /// <summary>
        ///     This function returns ECC memory configuration information.
        /// </summary>
        /// <param name="gpuHandle">
        ///     handle identifying the physical GPU for which ECC configuration information is to be
        ///     retrieved.
        /// </param>
        /// <returns>An instance of <see cref="ECCConfigurationInfoV1" /></returns>
        public static ECCConfigurationInfoV1 GetECCConfigurationInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(ECCConfigurationInfoV1).Instantiate<ECCConfigurationInfoV1>();

            using (var configurationInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetECCConfigurationInfo>()(
                    gpuHandle,
                    configurationInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return configurationInfoReference.ToValueType<ECCConfigurationInfoV1>(typeof(ECCConfigurationInfoV1));
            }
        }

        /// <summary>
        ///     This function returns ECC memory error information.
        /// </summary>
        /// <param name="gpuHandle">A handle identifying the physical GPU for which ECC error information is to be retrieved.</param>
        /// <returns>An instance of <see cref="ECCErrorInfoV1" /></returns>
        public static ECCErrorInfoV1 GetECCErrorInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(ECCErrorInfoV1).Instantiate<ECCErrorInfoV1>();

            using (var errorInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetECCErrorInfo>()(
                    gpuHandle,
                    errorInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return errorInfoReference.ToValueType<ECCErrorInfoV1>(typeof(ECCErrorInfoV1));
            }
        }

        /// <summary>
        ///     This function returns ECC memory status information.
        /// </summary>
        /// <param name="gpuHandle">A handle identifying the physical GPU for which ECC status information is to be retrieved.</param>
        /// <returns>An instance of <see cref="ECCStatusInfoV1" /></returns>
        public static ECCStatusInfoV1 GetECCStatusInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(ECCStatusInfoV1).Instantiate<ECCStatusInfoV1>();

            using (var statusInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetECCStatusInfo>()(
                    gpuHandle,
                    statusInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return statusInfoReference.ToValueType<ECCStatusInfoV1>(typeof(ECCStatusInfoV1));
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the GPU manufacturing foundry of the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The GPU manufacturing foundry of the GPU.</returns>
        public static GPUFoundry GetFoundry(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetFoundry>()(gpuHandle, out var foundry);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return foundry;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the current frame buffer width and location for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="width">The frame buffer width.</param>
        /// <param name="location">The frame buffer location.</param>
        public static void GetFrameBufferWidthAndLocation(
            PhysicalGPUHandle gpuHandle,
            out uint width,
            out uint location)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetFBWidthAndLocation>()(gpuHandle, out width,
                    out location);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This function retrieves the full GPU name as an ASCII string - for example, "Quadro FX 1400".
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <returns>Full GPU name as an ASCII string</returns>
        /// <exception cref="NVIDIAApiException">See NVIDIAApiException.Status for the reason of the exception.</exception>
        public static string GetFullName(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetFullName>()(gpuHandle, out var name);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return name.Value;
        }

        /// <summary>
        ///     Retrieves the total number of cores defined for a GPU.
        ///     Returns 0 on architectures that don't define GPU cores.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <returns>Total number of cores</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: API call is not supported on current architecture</exception>
        public static uint GetGPUCoreCount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetGpuCoreCount>()(gpuHandle, out var cores);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return cores;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the GPUID of the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The GPU handle to get the GPUID for.</param>
        /// <returns>The GPU's GPUID.</returns>
        public static uint GetGPUIDFromPhysicalGPU(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GetGPUIDFromPhysicalGPU>()(gpuHandle, out var gpuId);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpuId;
        }

        /// <summary>
        ///     This function returns the GPU type (integrated or discrete).
        ///     TCC_SUPPORTED
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>GPU type</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        public static GPUType GetGPUType(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetGPUType>()(gpuHandle, out var type);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return type;
        }

        /// <summary>
        ///     This function returns the interrupt number associated with this GPU.
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>Interrupt number associated with this GPU</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        public static int GetIRQ(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetIRQ>()(gpuHandle, out var irq);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return (int)irq;
        }


        /// <summary>
        ///     This API converts a display ID to a Physical GPU handle and output ID.
        /// </summary>
        /// <returns>The GPU physical handle and the output ID</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid argument</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static void GetGpuAndOutputIdFromDisplayId(
            [In] uint displayId,
            [Out] out PhysicalGPUHandle physicalGPU,
            [Out] out OutputId outputId)
        {
            var status =
               DelegateFactory.GetDelegate<GeneralDelegates.NvAPI_SYS_GetGpuAndOutputIdFromDisplayId>()(displayId,
                   out physicalGPU, out outputId);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API converts Physical GPU handle and output ID to a display ID.
        /// </summary>
        /// <returns>The displayID connected to the physical GPU at the output that has the output ID supplied.</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid argument</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static void GetDisplayIdFromGpuAndOutputId(
            [In] PhysicalGPUHandle physicalGPU,
            [In] OutputId outputId,
            [Out] out uint displayId)
        {
            var status =
               DelegateFactory.GetDelegate<GeneralDelegates.NvAPI_SYS_GetDisplayIdFromGpuAndOutputId>()(physicalGPU,
                   outputId, out displayId);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }



        /// <summary>
        ///     [PRIVATE]
        ///     Gets the current frame buffer width and location for the passed logical GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the logical GPU to perform the operation on.</param>
        /// <param name="width">The frame buffer width.</param>
        /// <param name="location">The frame buffer location.</param>
        public static void GetLogicalFrameBufferWidthAndLocation(
            LogicalGPUHandle gpuHandle,
            out uint width,
            out uint location)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetLogicalFBWidthAndLocation>()(gpuHandle,
                    out width, out location);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This function returns the logical GPU handle associated with specified physical GPU handle.
        ///     At least one GPU must be present in the system and running an NVIDIA display driver.
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>Logical GPU handle associated with specified physical GPU handle</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        public static LogicalGPUHandle GetLogicalGPUFromPhysicalGPU(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GetLogicalGPUFromPhysicalGPU>()(gpuHandle, out var gpu);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpu;
        }

        /// <summary>
        ///     This function retrieves the available driver memory footprint for the specified GPU.
        ///     If the GPU is in TCC Mode, only dedicatedVideoMemory will be returned.
        /// </summary>
        /// <param name="physicalGPUHandle">Handle of the physical GPU for which the memory information is to be extracted.</param>
        /// <returns>The memory footprint available in the driver.</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static IDisplayDriverMemoryInfo GetMemoryInfo(PhysicalGPUHandle physicalGPUHandle)
        {
            var getMemoryInfo = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetMemoryInfo>();

            foreach (var acceptType in getMemoryInfo.Accepts())
            {
                var instance = acceptType.Instantiate<IDisplayDriverMemoryInfo>();

                using (var displayDriverMemoryInfo = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getMemoryInfo(physicalGPUHandle, displayDriverMemoryInfo);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return displayDriverMemoryInfo.ToValueType<IDisplayDriverMemoryInfo>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the number of GPC (Graphic Processing Clusters) of the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The number of GPC units for the GPU.</returns>
        public static uint GetPartitionCount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetPartitionCount>()(gpuHandle, out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return count;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets additional information about the PCIe interface and configuration for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>PCIe information and configurations.</returns>
        public static PrivatePCIeInfoV2 GetPCIEInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivatePCIeInfoV2).Instantiate<PrivatePCIeInfoV2>();

            using (var pcieInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status =
                    DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetPCIEInfo>()(gpuHandle, pcieInfoReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return pcieInfoReference.ToValueType<PrivatePCIeInfoV2>(typeof(PrivatePCIeInfoV2));
            }
        }

        /// <summary>
        ///     This function returns the PCI identifiers associated with this GPU.
        ///     TCC_SUPPORTED
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <param name="deviceId">The internal PCI device identifier for the GPU.</param>
        /// <param name="subSystemId">The internal PCI subsystem identifier for the GPU.</param>
        /// <param name="revisionId">The internal PCI device-specific revision identifier for the GPU.</param>
        /// <param name="extDeviceId">The external PCI device identifier for the GPU.</param>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle or an argument is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        // ReSharper disable once TooManyArguments
        public static void GetPCIIdentifiers(
            PhysicalGPUHandle gpuHandle,
            out uint deviceId,
            out uint subSystemId,
            out uint revisionId,
            out uint extDeviceId)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetPCIIdentifiers>()(gpuHandle,
                out deviceId,
                out subSystemId, out revisionId, out extDeviceId);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This function returns the physical size of frame buffer in KB.  This does NOT include any system RAM that may be
        ///     dedicated for use by the GPU.
        ///     TCC_SUPPORTED
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>Physical size of frame buffer in KB</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        public static int GetPhysicalFrameBufferSize(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetPhysicalFrameBufferSize>()(gpuHandle,
                    out var size);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return (int)size;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets a physical GPU handle from the passed GPUID
        /// </summary>
        /// <param name="gpuId">The GPUID to get the physical handle for.</param>
        /// <returns>The retrieved physical GPU handle.</returns>
        public static PhysicalGPUHandle GetPhysicalGPUFromGPUID(uint gpuId)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GetPhysicalGPUFromGPUID>()(gpuId, out var gpuHandle);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpuHandle;
        }

        /// <summary>
        ///     This function returns the physical GPU handles associated with the specified logical GPU handle.
        ///     At least one GPU must be present in the system and running an NVIDIA display driver.
        /// </summary>
        /// <param name="gpuHandle">Logical GPU handle to get information about</param>
        /// <returns>An array of physical GPU handles</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedLogicalGPUHandle: gpuHandle was not a logical GPU handle</exception>
        public static PhysicalGPUHandle[] GetPhysicalGPUsFromLogicalGPU(LogicalGPUHandle gpuHandle)
        {
            var gpuList =
                typeof(PhysicalGPUHandle).Instantiate<PhysicalGPUHandle>().Repeat(PhysicalGPUHandle.MaxPhysicalGPUs);
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GetPhysicalGPUsFromLogicalGPU>()(gpuHandle,
                gpuList,
                out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpuList.Take((int)count).ToArray();
        }

        /// <summary>
        ///     This function retrieves the Quadro status for the GPU (true if Quadro, false if GeForce)
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>true if Quadro, false if GeForce</returns>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred</exception>
        public static bool GetQuadroStatus(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetQuadroStatus>()(gpuHandle, out var isQuadro);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return isQuadro > 0;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the number of RAM banks for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The number of RAM memory banks.</returns>
        public static uint GetRAMBankCount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetRamBankCount>()(gpuHandle, out var bankCount);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return bankCount;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the RAM bus width for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The RAM memory bus width.</returns>
        public static uint GetRAMBusWidth(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetRamBusWidth>()(gpuHandle, out var busWidth);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return busWidth;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the RAM maker for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The RAM memory maker.</returns>
        public static GPUMemoryMaker GetRAMMaker(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetRamMaker>()(gpuHandle, out var ramMaker);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return ramMaker;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the RAM type for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The RAM memory type.</returns>
        public static GPUMemoryType GetRAMType(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetRamType>()(gpuHandle, out var ramType);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return ramType;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the ROP count for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The number of ROP units.</returns>
        public static uint GetROPCount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetROPCount>()(gpuHandle, out var ropCount);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return ropCount;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the number of shader pipe lines for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The number of shader pipelines.</returns>
        public static uint GetShaderPipeCount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetShaderPipeCount>()(gpuHandle, out var spCount);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return spCount;
        }

        /// <summary>
        ///     This function retrieves the number of Shader SubPipes on the GPU
        ///     On newer architectures, this corresponds to the number of SM units
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>Number of Shader SubPipes on the GPU</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        public static uint GetShaderSubPipeCount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetShaderSubPipeCount>()(gpuHandle, out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return count;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the GPU short name (code name) for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The GPU short name.</returns>
        public static string GetShortName(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetShortName>()(gpuHandle, out var name);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return name.Value;
        }

        /// <summary>
        ///     This function identifies whether the GPU is a notebook GPU or a desktop GPU.
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>GPU system type</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        public static SystemType GetSystemType(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetSystemType>()(gpuHandle, out var systemType);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return systemType;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the SM count for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The number of SM units.</returns>
        public static uint GetTotalSMCount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetTotalSMCount>()(gpuHandle, out var smCount);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return smCount;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the SP count for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The number of SP units.</returns>
        public static uint GetTotalSPCount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetTotalSPCount>()(gpuHandle, out var spCount);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return spCount;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the TPC count for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The number of TPC units.</returns>
        public static uint GetTotalTPCCount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetTotalTPCCount>()(gpuHandle, out var tpcCount);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return tpcCount;
        }

        /// <summary>
        ///     This function returns the OEM revision of the video BIOS associated with this GPU.
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>OEM revision of the video BIOS</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static uint GetVBIOSOEMRevision(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetVbiosOEMRevision>()(gpuHandle, out var revision);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return revision;
        }

        /// <summary>
        ///     This function returns the revision of the video BIOS associated with this GPU.
        ///     TCC_SUPPORTED
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>Revision of the video BIOS</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static uint GetVBIOSRevision(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetVbiosRevision>()(gpuHandle, out var revision);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return revision;
        }

        /// <summary>
        ///     This function returns the full video BIOS version string in the form of xx.xx.xx.xx.yy where xx numbers come from
        ///     GetVbiosRevision() and yy comes from GetVbiosOEMRevision().
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <returns>Full video BIOS version string</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static string GetVBIOSVersionString(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetVbiosVersionString>()(gpuHandle,
                    out var version);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return version.Value;
        }

        /// <summary>
        ///     This function returns the virtual size of frame buffer in KB. This includes the physical RAM plus any system RAM
        ///     that has been dedicated for use by the GPU.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <returns>Virtual size of frame buffer in KB</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static int GetVirtualFrameBufferSize(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetVirtualFrameBufferSize>()(gpuHandle,
                out var bufferSize);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return (int)bufferSize;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the VPE count for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to retrieve this information from.</param>
        /// <returns>The number of VPE units.</returns>
        public static uint GetVPECount(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetVPECount>()(gpuHandle, out var vpeCount);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return vpeCount;
        }

        /// <summary>
        ///     Reads data from I2C bus
        /// </summary>
        /// <param name="gpuHandle">The physical GPU to access I2C bus.</param>
        /// <param name="i2cInfo">The information required for the operation. Will be filled with data after retrieval.</param>
        // ReSharper disable once InconsistentNaming
        public static void I2CRead<TI2CInfo>(PhysicalGPUHandle gpuHandle, ref TI2CInfo i2cInfo)
            where TI2CInfo : struct, II2CInfo
        {
            var c = i2cInfo as II2CInfo;
            I2CRead(gpuHandle, ref c);
            i2cInfo = (TI2CInfo)c;
        }

        /// <summary>
        ///     Reads data from I2C bus
        /// </summary>
        /// <param name="gpuHandle">The physical GPU to access I2C bus.</param>
        /// <param name="i2cInfo">The information required for the operation. Will be filled with data after retrieval.</param>
        // ReSharper disable once InconsistentNaming
        public static void I2CRead(PhysicalGPUHandle gpuHandle, ref II2CInfo i2cInfo)
        {
            var type = i2cInfo.GetType();
            // ReSharper disable once InconsistentNaming
            var i2cRead = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_I2CRead>();

            if (!i2cRead.Accepts().Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            // ReSharper disable once InconsistentNaming
            using (var i2cInfoReference = ValueTypeReference.FromValueType(i2cInfo, type))
            {
                var status = i2cRead(gpuHandle, i2cInfoReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                i2cInfo = i2cInfoReference.ToValueType<II2CInfo>(type);
            }
        }

        /// <summary>
        ///     Writes data to I2C bus
        /// </summary>
        /// <param name="gpuHandle">The physical GPU to access I2C bus.</param>
        /// <param name="i2cInfo">The information required for the operation.</param>
        // ReSharper disable once InconsistentNaming
        public static void I2CWrite(PhysicalGPUHandle gpuHandle, II2CInfo i2cInfo)
        {
            var type = i2cInfo.GetType();
            // ReSharper disable once InconsistentNaming
            var i2cWrite = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_I2CWrite>();

            if (!i2cWrite.Accepts().Contains(type))
            {
                throw new ArgumentOutOfRangeException(nameof(type));
            }

            // ReSharper disable once InconsistentNaming
            using (var i2cInfoReference = ValueTypeReference.FromValueType(i2cInfo, type))
            {
                var status = i2cWrite(gpuHandle, i2cInfoReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     This function resets ECC memory error counters.
        /// </summary>
        /// <param name="gpuHandle">A handle identifying the physical GPU for which ECC error information is to be cleared.</param>
        /// <param name="resetCurrent">Reset the current ECC error counters.</param>
        /// <param name="resetAggregated">Reset the aggregate ECC error counters.</param>
        public static void ResetECCErrorInfo(
            PhysicalGPUHandle gpuHandle,
            bool resetCurrent,
            bool resetAggregated)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ResetECCErrorInfo>()(
                gpuHandle,
                (byte)(resetCurrent ? 1 : 0),
                (byte)(resetAggregated ? 1 : 0)
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This function updates the ECC memory configuration setting.
        /// </summary>
        /// <param name="gpuHandle">A handle identifying the physical GPU for which to update the ECC configuration setting.</param>
        /// <param name="isEnable">The new ECC configuration setting.</param>
        /// <param name="isEnableImmediately">Request that the new setting take effect immediately.</param>
        public static void SetECCConfiguration(
            PhysicalGPUHandle gpuHandle,
            bool isEnable,
            bool isEnableImmediately)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetECCConfiguration>()(
                gpuHandle,
                (byte)(isEnable ? 1 : 0),
                (byte)(isEnableImmediately ? 1 : 0)
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }


        /// <summary>
        ///     This function is the same as GetAllOutputs() but returns only the set of GPU output identifiers that are actively
        ///     driving display devices.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <returns>Active output identifications as a flag</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static OutputId GetActiveOutputs(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetActiveOutputs>()(gpuHandle, out var outputMask);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return outputMask;
        }

        /// <summary>
        ///     This API returns display IDs for all possible outputs on the GPU.
        ///     For DPMST connector, it will return display IDs for all the video sinks in the topology.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <returns>An array of display identifications and their attributes</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">See NVIDIAApiException.Status for the reason of the exception.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static DisplayIdsV2[] GetAllDisplayIds(PhysicalGPUHandle gpuHandle)
        {
            var gpuGetConnectedDisplayIds = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetAllDisplayIds>();

            if (!gpuGetConnectedDisplayIds.Accepts().Contains(typeof(DisplayIdsV2)))
            {
                throw new NVIDIANotSupportedException("This operation is not supported.");
            }

            uint count = 0;
            var status = gpuGetConnectedDisplayIds(gpuHandle, ValueTypeArray.Null, ref count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            if (count == 0)
            {
                return new DisplayIdsV2[0];
            }

            using (
                var displayIds =
                    ValueTypeArray.FromArray(typeof(DisplayIdsV2).Instantiate<DisplayIdsV2>().Repeat((int)count)))
            {
                status = gpuGetConnectedDisplayIds(gpuHandle, displayIds, ref count);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return displayIds.ToArray<DisplayIdsV2>((int)count);
            }
        }

        /// <summary>
        ///     Due to space limitation GetConnectedOutputs() can return maximum 32 devices, but this is no longer true for DPMST.
        ///     GetConnectedDisplayIds() will return all the connected display devices in the form of displayIds for the associated
        ///     gpuHandle.
        ///     This function can accept set of flags to request cached, un-cached, sli and lid to get the connected devices.
        ///     Default value for flags will be cached.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get information about</param>
        /// <param name="flags">ConnectedIdsFlag flags</param>
        /// <returns>An array of display identifications and their attributes</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is invalid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static DisplayIdsV2[] GetConnectedDisplayIds(PhysicalGPUHandle gpuHandle, ConnectedIdsFlag flags)
        {
            var gpuGetConnectedDisplayIds =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetConnectedDisplayIds>();

            if (!gpuGetConnectedDisplayIds.Accepts().Contains(typeof(DisplayIdsV2)))
            {
                throw new NVIDIANotSupportedException("This operation is not supported.");
            }

            uint count = 0;
            var status = gpuGetConnectedDisplayIds(gpuHandle, ValueTypeArray.Null, ref count, flags);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            if (count == 0)
            {
                return new DisplayIdsV2[0];
            }

            foreach (var acceptType in gpuGetConnectedDisplayIds.Accepts())
            {
                var instances = acceptType.Instantiate<DisplayIdsV2>().Repeat((int)count);

                using (var displayIds = ValueTypeArray.FromArray(instances))
                {
                    status = gpuGetConnectedDisplayIds(gpuHandle, displayIds, ref count, flags);

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    instances = displayIds.ToArray<DisplayIdsV2>((int)count, acceptType);
                }

                if (instances.Length <= 0)
                {
                    return new DisplayIdsV2[0];
                }

                // After allocation, we should make sure to dispose objects
                // In this case however, the responsibility is on the user shoulders
                //instances = instances.AllocateAll().ToArray();

                // Convert IDisplayIds instances to DisplayIdsV2 instances
                //var displayIdsV2 = instances.OfType<DisplayIdsV2>().ToArray();

                return instances;
                //return instances.ToArray<DisplayIdsV2>((int)count);
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");

        }

        /// <summary>
        ///     This API converts a Physical GPU handle and output ID to a display ID.
        /// </summary>
        /// <param name="gpuHandle">Handle to the physical GPU</param>
        /// <param name="outputId">Connected display output identification on the target GPU - must only have one bit set</param>
        /// <returns>Display identification</returns>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: NVAPI not initialized</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: miscellaneous error occurred</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid input parameter.</exception>
        public static uint GetDisplayIdFromGPUAndOutputId(PhysicalGPUHandle gpuHandle, OutputId outputId)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_SYS_GetDisplayIdFromGpuAndOutputId>()(
                gpuHandle,
                outputId, out var display);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return display;
        }

        /// <summary>
        ///     This function returns the EDID data for the specified GPU handle and connection bit mask.
        ///     outputId should have exactly 1 bit set to indicate a single display.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to check outputs</param>
        /// <param name="outputId">Output identification</param>
        /// <param name="offset">EDID offset</param>
        /// <param name="readIdentification">EDID read identification for multi part read, or zero for first run</param>
        /// <returns>Whole or a part of the EDID data</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">
        ///     Status.InvalidArgument: gpuHandle or edid is invalid, outputId has 0 or > 1 bits
        ///     set
        /// </exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        /// <exception cref="NVIDIAApiException">Status.DataNotFound: The requested display does not contain an EDID.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        // ReSharper disable once TooManyArguments
        public static EDIDV3 GetEDID(
            PhysicalGPUHandle gpuHandle,
            OutputId outputId,
            int offset,
            int readIdentification = 0)
        {
            var gpuGetEDID = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetEDID>();

            if (!gpuGetEDID.Accepts().Contains(typeof(EDIDV3)))
            {
                throw new NVIDIANotSupportedException("This operation is not supported.");
            }

            var instance = EDIDV3.CreateWithOffset((uint)readIdentification, (uint)offset);

            using (var edidReference = ValueTypeReference.FromValueType(instance))
            {
                var status = gpuGetEDID(gpuHandle, outputId, edidReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return edidReference.ToValueType<EDIDV3>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     This function returns the EDID data for the specified GPU handle and connection bit mask.
        ///     outputId should have exactly 1 bit set to indicate a single display.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to check outputs</param>
        /// <param name="outputId">Output identification</param>
        /// <returns>Whole or a part of the EDID data</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">
        ///     Status.InvalidArgument: gpuHandle or edid is invalid, outputId has 0 or > 1 bits
        ///     set
        /// </exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        /// <exception cref="NVIDIAApiException">Status.DataNotFound: The requested display does not contain an EDID.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static IEDID GetEDID(PhysicalGPUHandle gpuHandle, OutputId outputId)
        {
            var gpuGetEDID = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetEDID>();

            foreach (var acceptType in gpuGetEDID.Accepts())
            {
                using (var edidReference = ValueTypeReference.FromValueType(acceptType.Instantiate<IEDID>(), acceptType)
                )
                {
                    var status = gpuGetEDID(gpuHandle, outputId, edidReference);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return edidReference.ToValueType<IEDID>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This API converts a display ID to a Physical GPU handle and output ID.
        /// </summary>
        /// <param name="displayId">Display identification of display to retrieve GPU and outputId for</param>
        /// <param name="gpuHandle">Handle to the physical GPU</param>
        /// <returns>Connected display output identification on the target GPU will only have one bit set.</returns>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: NVAPI not initialized</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid input parameter</exception>
        /// <exception cref="NVIDIAApiException">
        ///     Status.IdOutOfRange: The DisplayId corresponds to a display which is not within
        ///     the normal outputId range.
        /// </exception>
        public static OutputId GetGPUAndOutputIdFromDisplayId(uint displayId, out PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_SYS_GetGpuAndOutputIdFromDisplayId>()(
                displayId,
                out gpuHandle, out var outputId);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return outputId;
        }

        /// <summary>
        ///     This function returns the logical GPU handle associated with the specified display.
        ///     At least one GPU must be present in the system and running an NVIDIA display driver.
        ///     display can be DisplayHandle.DefaultHandle or a handle enumerated from EnumNVidiaDisplayHandle().
        /// </summary>
        /// <param name="display">Display handle to get information about</param>
        /// <returns>Logical GPU handle associated with the specified display</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        public static LogicalGPUHandle GetLogicalGPUFromDisplay(DisplayHandle display)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GetLogicalGPUFromDisplay>()(display, out var gpu);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpu;
        }

        /// <summary>
        ///     This function returns the output type. User can either specify both 'physical GPU handle and outputId (exactly 1
        ///     bit set)' or a valid displayId in the outputId parameter.
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <param name="outputId">Output identification of the output to get information about</param>
        /// <returns>Type of the output</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        public static OutputType GetOutputType(PhysicalGPUHandle gpuHandle, OutputId outputId)
        {
            return GetOutputType(gpuHandle, (uint)outputId);
        }

        /// <summary>
        ///     This function returns the output type. User can either specify both 'physical GPU handle and outputId (exactly 1
        ///     bit set)' or a valid displayId in the outputId parameter.
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <param name="displayId">Display identification of the divide to get information about</param>
        /// <returns>Type of the output</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        public static OutputType GetOutputType(PhysicalGPUHandle gpuHandle, uint displayId)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetOutputType>()(gpuHandle, displayId,
                    out var type);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return type;
        }

        /// <summary>
        ///     This API retrieves the Physical GPU handle of the connected display
        /// </summary>
        /// <param name="displayId">Display identification of display to retrieve GPU handle</param>
        /// <returns>Handle to the physical GPU</returns>
        /// <exception cref="NVIDIAApiException">Status.ApiNotInitialized: NVAPI not initialized</exception>
        /// <exception cref="NVIDIAApiException">Status.Error: Miscellaneous error occurred</exception>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: Invalid input parameter</exception>
        public static PhysicalGPUHandle GetPhysicalGPUFromDisplayId(uint displayId)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_SYS_GetPhysicalGpuFromDisplayId>()(displayId,
                    out var gpu);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpu;
        }

        /// <summary>
        ///     This function returns a physical GPU handle associated with the specified unattached display.
        ///     The source GPU is a physical render GPU which renders the frame buffer but may or may not drive the scan out.
        ///     At least one GPU must be present in the system and running an NVIDIA display driver.
        /// </summary>
        /// <param name="display">Display handle to get information about</param>
        /// <returns>Physical GPU handle associated with the specified unattached display.</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        public static PhysicalGPUHandle GetPhysicalGPUFromUnAttachedDisplay(UnAttachedDisplayHandle display)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GetPhysicalGPUFromUnAttachedDisplay>()(display,
                    out var gpu);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpu;
        }

        /// <summary>
        ///     This function returns an array of physical GPU handles associated with the specified display.
        ///     At least one GPU must be present in the system and running an NVIDIA display driver.
        ///     If the display corresponds to more than one physical GPU, the first GPU returned is the one with the attached
        ///     active output.
        /// </summary>
        /// <param name="display">Display handle to get information about</param>
        /// <returns>An array of physical GPU handles</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        public static PhysicalGPUHandle[] GetPhysicalGPUsFromDisplay(DisplayHandle display)
        {
            var gpuList =
                typeof(PhysicalGPUHandle).Instantiate<PhysicalGPUHandle>().Repeat(PhysicalGPUHandle.MaxPhysicalGPUs);
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GetPhysicalGPUsFromDisplay>()(display, gpuList,
                out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return gpuList.Take((int)count).ToArray();
        }

        /// <summary>
        ///     Thus function sets the EDID data for the specified GPU handle and connection bit mask.
        ///     User can either send (Gpu handle and output id) or only display Id in variable outputId parameter and gpuHandle
        ///     parameter can be default handle.
        ///     Note: The EDID will be cached across the boot session and will be enumerated to the OS in this call. To remove the
        ///     EDID set size of EDID to zero. OS and NVAPI connection status APIs will reflect the newly set or removed EDID
        ///     dynamically.
        ///     This feature will NOT be supported on the following boards: GeForce, Quadro VX, Tesla
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to check outputs</param>
        /// <param name="outputId">Output identification</param>
        /// <param name="edid">EDID information</param>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">
        ///     Status.InvalidArgument: gpuHandle or edid is invalid, outputId has 0 or > 1 bits
        ///     set
        /// </exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: For the above mentioned GPUs</exception>
        public static void SetEDID(PhysicalGPUHandle gpuHandle, OutputId outputId, IEDID edid)
        {
            SetEDID(gpuHandle, (uint)outputId, edid);
        }

        /// <summary>
        ///     Thus function sets the EDID data for the specified GPU handle and connection bit mask.
        ///     User can either send (Gpu handle and output id) or only display Id in variable outputId parameter and gpuHandle
        ///     parameter can be default handle.
        ///     Note: The EDID will be cached across the boot session and will be enumerated to the OS in this call. To remove the
        ///     EDID set size of EDID to zero. OS and NVAPI connection status APIs will reflect the newly set or removed EDID
        ///     dynamically.
        ///     This feature will NOT be supported on the following boards: GeForce, Quadro VX, Tesla
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to check outputs</param>
        /// <param name="displayId">Output identification</param>
        /// <param name="edid">EDID information</param>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">
        ///     Status.InvalidArgument: gpuHandle or edid is invalid, outputId has 0 or > 1 bits
        ///     set
        /// </exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        /// <exception cref="NVIDIAApiException">Status.NotSupported: For the above mentioned GPUs</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static void SetEDID(PhysicalGPUHandle gpuHandle, uint displayId, IEDID edid)
        {
            var gpuSetEDID = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetEDID>();

            if (!gpuSetEDID.Accepts().Contains(edid.GetType()))
            {
                throw new NVIDIANotSupportedException("This operation is not supported.");
            }

            using (var edidReference = ValueTypeReference.FromValueType(edid, edid.GetType()))
            {
                var status = gpuSetEDID(gpuHandle, displayId, edidReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     This function determines if a set of GPU outputs can be active simultaneously.  While a GPU may have 'n' outputs,
        ///     typically they cannot all be active at the same time due to internal resource sharing.
        ///     Given a physical GPU handle and a mask of candidate outputs, this call will return true if all of the specified
        ///     outputs can be driven simultaneously. It will return false if they cannot.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to check outputs</param>
        /// <param name="outputIds">Output identification combination</param>
        /// <returns>true if all of the specified outputs can be driven simultaneously. It will return false if they cannot.</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: display is not valid</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static bool ValidateOutputCombination(PhysicalGPUHandle gpuHandle, OutputId outputIds)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ValidateOutputCombination>()(gpuHandle, outputIds);

            if (status == Status.InvalidCombination)
            {
                return false;
            }

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return true;
        }


        /// <summary>
        ///     Gets the control information about illumination devices on the given GPU.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <returns>An instance of <see cref="IlluminationDeviceControlParametersV1" />.</returns>
        public static IlluminationDeviceControlParametersV1 ClientIlluminationDevicesGetControl(
            PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(IlluminationDeviceControlParametersV1)
                .Instantiate<IlluminationDeviceControlParametersV1>();

            using (var deviceControlParametersReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientIlluminationDevicesGetControl>()(
                    gpuHandle,
                    deviceControlParametersReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return deviceControlParametersReference.ToValueType<IlluminationDeviceControlParametersV1>()
                    .GetValueOrDefault();
            }
        }

        /// <summary>
        ///     Returns static information about illumination devices on the given GPU.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <returns>An instance of <see cref="IlluminationDeviceInfoParametersV1" />.</returns>
        public static IlluminationDeviceInfoParametersV1 ClientIlluminationDevicesGetInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(IlluminationDeviceInfoParametersV1).Instantiate<IlluminationDeviceInfoParametersV1>();

            using (var deviceInfoParametersReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientIlluminationDevicesGetInfo>()(
                    gpuHandle,
                    deviceInfoParametersReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return deviceInfoParametersReference.ToValueType<IlluminationDeviceInfoParametersV1>()
                    .GetValueOrDefault();
            }
        }

        /// <summary>
        ///     Sets the control information about illumination devices on the given GPU.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <param name="deviceControlParameters">The new control illumination devices control information.</param>
        public static void ClientIlluminationDevicesSetControl(
            PhysicalGPUHandle gpuHandle,
            IlluminationDeviceControlParametersV1 deviceControlParameters)
        {
            using (var deviceControlParametersReference = ValueTypeReference.FromValueType(deviceControlParameters))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientIlluminationDevicesSetControl>()(
                    gpuHandle,
                    deviceControlParametersReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     Gets the control information about illumination zones on the given GPU.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <param name="valuesType">The type of settings to retrieve.</param>
        /// <returns>An instance of <see cref="IlluminationZoneControlParametersV1" />.</returns>
        public static IlluminationZoneControlParametersV1 ClientIlluminationZonesGetControl(
            PhysicalGPUHandle gpuHandle,
            IlluminationZoneControlValuesType valuesType)
        {
            var instance = new IlluminationZoneControlParametersV1(valuesType);

            using (var zoneControlParametersReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientIlluminationZonesGetControl>()(
                    gpuHandle,
                    zoneControlParametersReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return zoneControlParametersReference.ToValueType<IlluminationZoneControlParametersV1>()
                    .GetValueOrDefault();
            }
        }

        /// <summary>
        ///     Returns static information about illumination zones on the given GPU.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <returns>An instance of <see cref="IlluminationZoneInfoParametersV1" />.</returns>
        public static IlluminationZoneInfoParametersV1 ClientIlluminationZonesGetInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(IlluminationZoneInfoParametersV1).Instantiate<IlluminationZoneInfoParametersV1>();

            using (var zoneInfoParametersReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientIlluminationZonesGetInfo>()(
                    gpuHandle,
                    zoneInfoParametersReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return zoneInfoParametersReference.ToValueType<IlluminationZoneInfoParametersV1>().GetValueOrDefault();
            }
        }

        /// <summary>
        ///     Sets the control information about illumination zones on the given GPU.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <param name="zoneControlParameters">The new control illumination zones control information.</param>
        public static void ClientIlluminationZonesSetControl(
            PhysicalGPUHandle gpuHandle,
            IlluminationZoneControlParametersV1 zoneControlParameters)
        {
            using (var zoneControlParametersReference = ValueTypeReference.FromValueType(zoneControlParameters))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientIlluminationZonesSetControl>()(
                    gpuHandle,
                    zoneControlParametersReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     Reports value of the specified illumination attribute brightness.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <param name="attribute">The attribute to get the value of.</param>
        /// <returns>Brightness value in percentage.</returns>
        public static uint GetIllumination(PhysicalGPUHandle gpuHandle, IlluminationAttribute attribute)
        {
            var instance = new GetIlluminationParameterV1(gpuHandle, attribute);

            using (var getParameterReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetIllumination>()(
                    getParameterReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return getParameterReference.ToValueType<GetIlluminationParameterV1>()
                    .GetValueOrDefault()
                    .ValueInPercentage;
            }
        }

        /// <summary>
        ///     Queries a illumination attribute support status.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <param name="attribute">The attribute to get the support status of.</param>
        /// <returns>true if the attribute is supported on this GPU; otherwise false.</returns>
        public static bool QueryIlluminationSupport(PhysicalGPUHandle gpuHandle, IlluminationAttribute attribute)
        {
            var instance = new QueryIlluminationSupportParameterV1(gpuHandle, attribute);

            using (var querySupportParameterReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetIllumination>()(
                    querySupportParameterReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return querySupportParameterReference.ToValueType<QueryIlluminationSupportParameterV1>()
                    .GetValueOrDefault()
                    .IsSupported;
            }
        }


        /// <summary>
        ///     Indicates whether a queried workstation feature is supported by the requested GPU.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <param name="gpuWorkstationFeature">The feature for the GPU in question. One of the values from enum WorkstationFeatureType</param>
        /// <returns>Status.Ok if the queried workstation feature is supported on the given GPU, or Status.NotSUpported if the feature is not supported.</returns>
        public static bool QueryWorkstationFeatureSupport(PhysicalGPUHandle gpuHandle, WorkstationFeatureType gpuWorkstationFeature)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_QueryWorkstationFeatureSupport>()(
                gpuHandle,
                gpuWorkstationFeature
            );

            if (status == Status.Ok)
            {
                SharedLogger.logger.Trace($"NVAPI/QueryWorkstationFeatureSupport: NVIDIA Video Card is one from the Quadro range");
                return true;
            }
            else if (status == Status.NotSupported)
            {
                SharedLogger.logger.Trace($"NVAPI/QueryWorkstationFeatureSupport: NVIDIA Video Card is not from the Quadro range");
                return false;
            }
            else
            {
                if (status == Status.NoImplementation)
                {
                    SharedLogger.logger.Trace($"NVAPI/QueryWorkstationFeatureSupport: The current NVIDIA driver doesn't support this NvAPI_GPU_QueryWorkstationFeatureSupport interface.");
                }
                else if (status == Status.InvalidHandle)
                {
                    SharedLogger.logger.Trace($"NVAPI/QueryWorkstationFeatureSupport: The physical video card handle supplied to NvAPI_GPU_QueryWorkstationFeatureSupport was an invalid handle.");
                }
                else if (status == Status.NotSupported)
                {
                    SharedLogger.logger.Trace($"NVAPI/QueryWorkstationFeatureSupport: The requested gpuWorkstationFeature is not supported in the selected GPU.");
                }
                else if (status == Status.SettingNotFound)
                {
                    SharedLogger.logger.Trace($"NVAPI/QueryWorkstationFeatureSupport: The requested gpuWorkstationFeature is unknown to the current driver version.");
                }
                else
                {
                    SharedLogger.logger.Trace($"NVAPI/QueryWorkstationFeatureSupport: Error getting GPU Function status. NvAPI_GPU_QueryWorkstationFeatureSupport() returned error code {status}");
                }
                throw new NVIDIAApiException(status);
            }

        }

        /// <summary>
        ///     Sets the value of the specified illumination attribute brightness.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <param name="attribute">The attribute to set the value of.</param>
        /// <param name="valueInPercentage">Brightness value in percentage.</param>
        public static void SetIllumination(
            PhysicalGPUHandle gpuHandle,
            IlluminationAttribute attribute,
            uint valueInPercentage)
        {
            var instance = new SetIlluminationParameterV1(gpuHandle, attribute, valueInPercentage);

            using (var setParameterReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetIllumination>()(
                    setParameterReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }


        /// <summary>
        ///     [PRIVATE]
        ///     Enables the overclocked performance states
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        public static void EnableOverclockedPStates(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_EnableOverclockedPStates>()(
                gpuHandle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This function retrieves the clock frequencies information from an specific physical GPU and fills the structure
        /// </summary>
        /// <param name="physicalGPUHandle">
        ///     Handle of the physical GPU for which the clock frequency information is to be
        ///     retrieved.
        /// </param>
        /// <param name="clockFrequencyOptions">
        ///     The structure that holds options for the operations and should be filled with the
        ///     results, use null to return current clock frequencies
        /// </param>
        /// <returns>The device clock frequencies information.</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static IClockFrequencies GetAllClockFrequencies(
            PhysicalGPUHandle physicalGPUHandle,
            IClockFrequencies clockFrequencyOptions = null)
        {
            var getClocksInfo = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetAllClockFrequencies>();

            if (clockFrequencyOptions == null)
            {
                foreach (var acceptType in getClocksInfo.Accepts())
                {
                    var instance = acceptType.Instantiate<IClockFrequencies>();

                    using (var clockFrequenciesInfo = ValueTypeReference.FromValueType(instance, acceptType))
                    {
                        var status = getClocksInfo(physicalGPUHandle, clockFrequenciesInfo);

                        if (status == Status.IncompatibleStructureVersion)
                        {
                            continue;
                        }

                        if (status != Status.Ok)
                        {
                            throw new NVIDIAApiException(status);
                        }

                        return clockFrequenciesInfo.ToValueType<IClockFrequencies>(acceptType);
                    }
                }
            }
            else
            {
                using (var clockFrequenciesInfo =
                    ValueTypeReference.FromValueType(clockFrequencyOptions, clockFrequencyOptions.GetType()))
                {
                    var status = getClocksInfo(physicalGPUHandle, clockFrequenciesInfo);

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return clockFrequenciesInfo.ToValueType<IClockFrequencies>(clockFrequencyOptions.GetType());
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Gets the clock boost lock for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The GPU clock boost lock.</returns>
        public static PrivateClockBoostLockV2 GetClockBoostLock(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateClockBoostLockV2).Instantiate<PrivateClockBoostLockV2>();

            using (var clockLockReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetClockBoostLock>()(
                    gpuHandle,
                    clockLockReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return clockLockReference.ToValueType<PrivateClockBoostLockV2>(typeof(PrivateClockBoostLockV2));
            }
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Gets the clock boost mask for passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The GPI clock boost mask.</returns>
        public static PrivateClockBoostMasksV1 GetClockBoostMask(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateClockBoostMasksV1).Instantiate<PrivateClockBoostMasksV1>();

            using (var clockBoostReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetClockBoostMask>()(
                    gpuHandle,
                    clockBoostReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return clockBoostReference.ToValueType<PrivateClockBoostMasksV1>(typeof(PrivateClockBoostMasksV1));
            }
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Gets the clock boost ranges for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The GPU clock boost ranges.</returns>
        public static PrivateClockBoostRangesV1 GetClockBoostRanges(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateClockBoostRangesV1).Instantiate<PrivateClockBoostRangesV1>();

            using (var clockRangesReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetClockBoostRanges>()(
                    gpuHandle,
                    clockRangesReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return clockRangesReference.ToValueType<PrivateClockBoostRangesV1>(typeof(PrivateClockBoostRangesV1));
            }
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Gets the clock boost table for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The GPU clock boost table.</returns>
        public static PrivateClockBoostTableV1 GetClockBoostTable(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateClockBoostTableV1).Instantiate<PrivateClockBoostTableV1>();

            using (var clockTableReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetClockBoostTable>()(
                    gpuHandle,
                    clockTableReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return clockTableReference.ToValueType<PrivateClockBoostTableV1>(typeof(PrivateClockBoostTableV1));
            }
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Gets the core voltage boost percentage for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The voltage boost percentage.</returns>
        public static PrivateVoltageBoostPercentV1 GetCoreVoltageBoostPercent(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateVoltageBoostPercentV1).Instantiate<PrivateVoltageBoostPercentV1>();

            using (var voltageBoostReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetCoreVoltageBoostPercent>()(
                    gpuHandle,
                    voltageBoostReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return voltageBoostReference.ToValueType<PrivateVoltageBoostPercentV1>(
                    typeof(PrivateVoltageBoostPercentV1));
            }
        }

        /// <summary>
        ///     This function returns the current performance state (P-State).
        /// </summary>
        /// <param name="gpuHandle">GPU handle to get information about</param>
        /// <returns>The current performance state.</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        public static PerformanceStateId GetCurrentPerformanceState(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetCurrentPState>()(gpuHandle,
                    out var performanceState);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return performanceState;
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Gets the current voltage status for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The voltage status of the GPU.</returns>
        public static PrivateVoltageStatusV1 GetCurrentVoltage(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateVoltageStatusV1).Instantiate<PrivateVoltageStatusV1>();

            using (var voltageStatusReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetCurrentVoltage>()(
                    gpuHandle,
                    voltageStatusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return voltageStatusReference.ToValueType<PrivateVoltageStatusV1>(typeof(PrivateVoltageStatusV1));
            }
        }

        /// <summary>
        ///     This function retrieves all available performance states (P-States) information.
        ///     P-States are GPU active/executing performance capability and power consumption states.
        /// </summary>
        /// <param name="physicalGPUHandle">GPU handle to get information about.</param>
        /// <param name="flags">Flag to get specific information about a performance state.</param>
        /// <returns>Retrieved performance states information</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        [SuppressMessage("ReSharper", "EventExceptionNotDocumented")]
        public static IPerformanceStatesInfo GetPerformanceStates(
            PhysicalGPUHandle physicalGPUHandle,
            GetPerformanceStatesInfoFlags flags)
        {
            var getPerformanceStatesInfo = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetPStatesInfoEx>();

            foreach (var acceptType in getPerformanceStatesInfo.Accepts())
            {
                var instance = acceptType.Instantiate<IPerformanceStatesInfo>();

                using (var performanceStateInfo = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getPerformanceStatesInfo(physicalGPUHandle, performanceStateInfo, flags);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return performanceStateInfo.ToValueType<IPerformanceStatesInfo>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     This function retrieves all available performance states (P-States) 2.0 information.
        ///     P-States are GPU active/executing performance capability and power consumption states.
        /// </summary>
        /// <param name="physicalGPUHandle">GPU handle to get information about.</param>
        /// <returns>Retrieved performance states 2.0 information</returns>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        [SuppressMessage("ReSharper", "EventExceptionNotDocumented")]
        public static IPerformanceStates20Info GetPerformanceStates20(PhysicalGPUHandle physicalGPUHandle)
        {
            var getPerformanceStates20 = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetPStates20>();

            foreach (var acceptType in getPerformanceStates20.Accepts())
            {
                var instance = acceptType.Instantiate<IPerformanceStates20Info>();

                using (var performanceStateInfo = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getPerformanceStates20(physicalGPUHandle, performanceStateInfo);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return performanceStateInfo.ToValueType<IPerformanceStates20Info>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Gets the GPU boost frequency curve controls for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The retrieved VFP curve.</returns>
        public static PrivateVFPCurveV1 GetVFPCurve(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateVFPCurveV1).Instantiate<PrivateVFPCurveV1>();

            using (var vfpCurveReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetVFPCurve>()(
                    gpuHandle,
                    vfpCurveReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return vfpCurveReference.ToValueType<PrivateVFPCurveV1>(typeof(PrivateVFPCurveV1));
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the performance policies current information for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The performance policies information.</returns>
        public static PrivatePerformanceInfoV1 PerformancePoliciesGetInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivatePerformanceInfoV1).Instantiate<PrivatePerformanceInfoV1>();

            using (var performanceInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_PerfPoliciesGetInfo>()(
                    gpuHandle,
                    performanceInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return performanceInfoReference.ToValueType<PrivatePerformanceInfoV1>(typeof(PrivatePerformanceInfoV1));
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the performance policies status for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The performance policies status of the GPU.</returns>
        public static PrivatePerformanceStatusV1 PerformancePoliciesGetStatus(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivatePerformanceStatusV1).Instantiate<PrivatePerformanceStatusV1>();

            using (var performanceStatusReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_PerfPoliciesGetStatus>()(
                    gpuHandle,
                    performanceStatusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return performanceStatusReference.ToValueType<PrivatePerformanceStatusV1>(
                    typeof(PrivatePerformanceStatusV1));
            }
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Sets the clock boost lock status for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="clockBoostLock">The new clock boost lock status.</param>
        public static void SetClockBoostLock(PhysicalGPUHandle gpuHandle, PrivateClockBoostLockV2 clockBoostLock)
        {
            using (var clockLockReference = ValueTypeReference.FromValueType(clockBoostLock))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetClockBoostLock>()(
                    gpuHandle,
                    clockLockReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Sets the clock boost table for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="clockBoostTable">The new clock table.</param>
        public static void SetClockBoostTable(PhysicalGPUHandle gpuHandle, PrivateClockBoostTableV1 clockBoostTable)
        {
            using (var clockTableReference = ValueTypeReference.FromValueType(clockBoostTable))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetClockBoostTable>()(
                    gpuHandle,
                    clockTableReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     [PRIVATE] - [Pascal Only]
        ///     Sets the core voltage boost percentage
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="boostPercent">The voltage boost percentages.</param>
        public static void SetCoreVoltageBoostPercent(
            PhysicalGPUHandle gpuHandle,
            PrivateVoltageBoostPercentV1 boostPercent)
        {
            using (var boostPercentReference = ValueTypeReference.FromValueType(boostPercent))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetCoreVoltageBoostPercent>()(
                    gpuHandle,
                    boostPercentReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     This function sets the performance states (P-States) 2.0 information.
        ///     P-States are GPU active/executing performance capability and power consumption states.
        /// </summary>
        /// <param name="physicalGPUHandle">GPU handle to get information about.</param>
        /// <param name="performanceStates20Info">Performance status 2.0 information to set</param>
        /// <exception cref="NVIDIAApiException">Status.InvalidArgument: gpuHandle is NULL</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle</exception>
        public static void SetPerformanceStates20(
            PhysicalGPUHandle physicalGPUHandle,
            IPerformanceStates20Info performanceStates20Info)
        {
            using (var performanceStateInfo =
                ValueTypeReference.FromValueType(performanceStates20Info, performanceStates20Info.GetType()))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetPStates20>()(
                    physicalGPUHandle,
                    performanceStateInfo
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }



        /// <summary>
        ///     [PRIVATE]
        ///     Gets the current power policies information for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The current power policies information.</returns>
        public static PrivatePowerPoliciesInfoV1 ClientPowerPoliciesGetInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivatePowerPoliciesInfoV1).Instantiate<PrivatePowerPoliciesInfoV1>();

            using (var policiesInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status =
                    DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientPowerPoliciesGetInfo>()(gpuHandle,
                        policiesInfoReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return policiesInfoReference.ToValueType<PrivatePowerPoliciesInfoV1>(
                    typeof(PrivatePowerPoliciesInfoV1));
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the power policies status for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The power policies status.</returns>
        public static PrivatePowerPoliciesStatusV1 ClientPowerPoliciesGetStatus(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivatePowerPoliciesStatusV1).Instantiate<PrivatePowerPoliciesStatusV1>();

            using (var policiesStatusReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientPowerPoliciesGetStatus>()(
                    gpuHandle,
                    policiesStatusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return policiesStatusReference.ToValueType<PrivatePowerPoliciesStatusV1>(
                    typeof(PrivatePowerPoliciesStatusV1));
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Sets the power policies status for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="policiesStatus">The new power limiter policy.</param>
        public static void ClientPowerPoliciesSetStatus(
            PhysicalGPUHandle gpuHandle,
            PrivatePowerPoliciesStatusV1 policiesStatus)
        {
            using (var policiesStatusReference = ValueTypeReference.FromValueType(policiesStatus))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientPowerPoliciesSetStatus>()(
                    gpuHandle,
                    policiesStatusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the power topology status for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The power topology status.</returns>
        public static PrivatePowerTopologiesStatusV1 ClientPowerTopologyGetStatus(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivatePowerTopologiesStatusV1).Instantiate<PrivatePowerTopologiesStatusV1>();

            using (var topologiesStatusReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientPowerTopologyGetStatus>()(
                    gpuHandle,
                    topologiesStatusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return topologiesStatusReference.ToValueType<PrivatePowerTopologiesStatusV1>(
                    typeof(PrivatePowerTopologiesStatusV1));
            }
        }



        /// <summary>
        ///     [PRIVATE]
        ///     Gets the cooler policy table for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="policy">The cooler policy to get the table for.</param>
        /// <param name="index">The cooler index.</param>
        /// <param name="count">Number of policy table entries retrieved.</param>
        /// <returns>The cooler policy table for the GPU.</returns>
        // ReSharper disable once TooManyArguments
        public static PrivateCoolerPolicyTableV1 GetCoolerPolicyTable(
            PhysicalGPUHandle gpuHandle,
            CoolerPolicy policy,
            uint index,
            out uint count)
        {
            var instance = typeof(PrivateCoolerPolicyTableV1).Instantiate<PrivateCoolerPolicyTableV1>();
            instance._Policy = policy;

            using (var policyTableReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetCoolerPolicyTable>()(
                    gpuHandle,
                    index,
                    policyTableReference,
                    out count
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return policyTableReference.ToValueType<PrivateCoolerPolicyTableV1>(typeof(PrivateCoolerPolicyTableV1));
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the cooler settings for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="coolerTarget">The cooler targets to get settings.</param>
        /// <returns>The cooler settings.</returns>
        public static PrivateCoolerSettingsV1 GetCoolerSettings(
            PhysicalGPUHandle gpuHandle,
            CoolerTarget coolerTarget = CoolerTarget.All)
        {
            var instance = typeof(PrivateCoolerSettingsV1).Instantiate<PrivateCoolerSettingsV1>();

            using (var coolerSettingsReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetCoolerSettings>()(
                    gpuHandle,
                    coolerTarget,
                    coolerSettingsReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return coolerSettingsReference.ToValueType<PrivateCoolerSettingsV1>(typeof(PrivateCoolerSettingsV1));
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the current fan speed level for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The current fan speed level.</returns>
        public static uint GetCurrentFanSpeedLevel(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory
                    .GetDelegate<GPUDelegates.NvAPI_GPU_GetCurrentFanSpeedLevel>()(gpuHandle, out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return count;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the current thermal level for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The current thermal level.</returns>
        public static uint GetCurrentThermalLevel(PhysicalGPUHandle gpuHandle)
        {
            var status =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetCurrentThermalLevel>()(gpuHandle, out var count);

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return count;
        }

        /// <summary>
        ///     This function returns the fan speed tachometer reading for the specified physical GPU.
        /// </summary>
        /// <param name="gpuHandle">Physical GPU handle to get tachometer reading from</param>
        /// <returns>The GPU fan speed in revolutions per minute.</returns>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found</exception>
        /// <exception cref="NVIDIAApiException">Status.ExpectedPhysicalGPUHandle: gpuHandle was not a physical GPU handle.</exception>
        public static uint GetTachReading(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetTachReading>()(
                gpuHandle, out var value
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return value;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the current thermal policies information for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The current thermal policies information.</returns>
        public static PrivateThermalPoliciesInfoV2 GetThermalPoliciesInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateThermalPoliciesInfoV2).Instantiate<PrivateThermalPoliciesInfoV2>();

            using (var policiesInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status =
                    DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetThermalPoliciesInfo>()(gpuHandle,
                        policiesInfoReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return policiesInfoReference.ToValueType<PrivateThermalPoliciesInfoV2>(
                    typeof(PrivateThermalPoliciesInfoV2));
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the thermal policies status for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The thermal policies status.</returns>
        public static PrivateThermalPoliciesStatusV2 GetThermalPoliciesStatus(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateThermalPoliciesStatusV2).Instantiate<PrivateThermalPoliciesStatusV2>();

            using (var policiesStatusReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetThermalPoliciesStatus>()(
                    gpuHandle,
                    policiesStatusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return policiesStatusReference.ToValueType<PrivateThermalPoliciesStatusV2>(
                    typeof(PrivateThermalPoliciesStatusV2));
            }
        }

        /// <summary>
        ///     This function retrieves the thermal information of all thermal sensors or specific thermal sensor associated with
        ///     the selected GPU. To retrieve info for all sensors, set sensorTarget to ThermalSettingsTarget.All.
        /// </summary>
        /// <param name="physicalGPUHandle">Handle of the physical GPU for which the memory information is to be extracted.</param>
        /// <param name="sensorTarget">Specifies the requested thermal sensor target.</param>
        /// <returns>The device thermal sensors information.</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static IThermalSettings GetThermalSettings(
            PhysicalGPUHandle physicalGPUHandle,
            ThermalSettingsTarget sensorTarget = ThermalSettingsTarget.All)
        {
            var getThermalSettings = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetThermalSettings>();

            foreach (var acceptType in getThermalSettings.Accepts())
            {
                var instance = acceptType.Instantiate<IThermalSettings>();

                using (var gpuThermalSettings = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getThermalSettings(physicalGPUHandle, sensorTarget, gpuThermalSettings);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return gpuThermalSettings.ToValueType<IThermalSettings>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Restores the cooler policy table to default for the passed GPU handle and cooler index.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="policy">The cooler policy to restore to default.</param>
        /// <param name="indexes">The indexes of the coolers to restore their policy tables to default.</param>
        public static void RestoreCoolerPolicyTable(
            PhysicalGPUHandle gpuHandle,
            CoolerPolicy policy,
            uint[] indexes = null)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_RestoreCoolerPolicyTable>()(
                gpuHandle,
                indexes,
                (uint)(indexes?.Length ?? 0),
                policy
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Restores the cooler settings to default for the passed GPU handle and cooler index.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="indexes">The indexes of the coolers to restore their settings to default.</param>
        public static void RestoreCoolerSettings(
            PhysicalGPUHandle gpuHandle,
            uint[] indexes = null)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_RestoreCoolerSettings>()(
                gpuHandle,
                indexes,
                (uint)(indexes?.Length ?? 0)
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Sets the cooler levels for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="index">The cooler index.</param>
        /// <param name="coolerLevels">The cooler level information.</param>
        /// <param name="levelsCount">The number of entries in the cooler level information.</param>
        // ReSharper disable once TooManyArguments
        public static void SetCoolerLevels(
            PhysicalGPUHandle gpuHandle,
            uint index,
            PrivateCoolerLevelsV1 coolerLevels,
            uint levelsCount
        )
        {
            using (var coolerLevelsReference = ValueTypeReference.FromValueType(coolerLevels))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetCoolerLevels>()(
                    gpuHandle,
                    index,
                    coolerLevelsReference,
                    levelsCount
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Sets the cooler policy table for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="index">The cooler index.</param>
        /// <param name="coolerPolicyTable">The cooler policy table.</param>
        /// <param name="policyLevelsCount">The number of entries in the cooler policy table.</param>
        // ReSharper disable once TooManyArguments
        public static void SetCoolerPolicyTable(
            PhysicalGPUHandle gpuHandle,
            uint index,
            PrivateCoolerPolicyTableV1 coolerPolicyTable,
            uint policyLevelsCount
        )
        {
            var instance = typeof(PrivateCoolerPolicyTableV1).Instantiate<PrivateCoolerPolicyTableV1>();

            using (var policyTableReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetCoolerPolicyTable>()(
                    gpuHandle,
                    index,
                    policyTableReference,
                    policyLevelsCount
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Sets the thermal policies status for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <param name="thermalPoliciesStatus">The new thermal limiter policy to apply.</param>
        public static void SetThermalPoliciesStatus(
            PhysicalGPUHandle gpuHandle,
            PrivateThermalPoliciesStatusV2 thermalPoliciesStatus)
        {
            using (var policiesStatusReference = ValueTypeReference.FromValueType(thermalPoliciesStatus))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_SetThermalPoliciesStatus>()(
                    gpuHandle,
                    policiesStatusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }

        public static PrivateFanCoolersInfoV1 GetClientFanCoolersInfo(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateFanCoolersInfoV1).Instantiate<PrivateFanCoolersInfoV1>();

            using (var policiesInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status =
                    DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientFanCoolersGetInfo>()(gpuHandle,
                        policiesInfoReference);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return policiesInfoReference.ToValueType<PrivateFanCoolersInfoV1>(
                    typeof(PrivateFanCoolersInfoV1));
            }
        }

        public static PrivateFanCoolersStatusV1 GetClientFanCoolersStatus(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateFanCoolersStatusV1).Instantiate<PrivateFanCoolersStatusV1>();

            using (var policiesStatusReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientFanCoolersGetStatus>()(
                    gpuHandle,
                    policiesStatusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return policiesStatusReference.ToValueType<PrivateFanCoolersStatusV1>(
                    typeof(PrivateFanCoolersStatusV1));
            }
        }

        public static PrivateFanCoolersControlV1 GetClientFanCoolersControl(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateFanCoolersControlV1).Instantiate<PrivateFanCoolersControlV1>();
            using (var policiesStatusReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientFanCoolersGetControl>()(
                    gpuHandle,
                    policiesStatusReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return policiesStatusReference.ToValueType<PrivateFanCoolersControlV1>(
                    typeof(PrivateFanCoolersControlV1));
            }
        }

        public static void SetClientFanCoolersControl(PhysicalGPUHandle gpuHandle, PrivateFanCoolersControlV1 control)
        {
            using (var coolerLevelsReference = ValueTypeReference.FromValueType(control))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_ClientFanCoolersSetControl>()(
                    gpuHandle,
                    coolerLevelsReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }
            }
        }


        /// <summary>
        ///     [PRIVATE]
        ///     Enables the dynamic performance states
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        public static void EnableDynamicPStates(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_EnableDynamicPStates>()(
                gpuHandle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This function retrieves the dynamic performance states information from specific GPU
        /// </summary>
        /// <param name="physicalGPUHandle">Handle of the physical GPU for which the memory information is to be extracted.</param>
        /// <returns>The device utilizations information array.</returns>
        /// <exception cref="NVIDIANotSupportedException">This operation is not supported.</exception>
        /// <exception cref="NVIDIAApiException">Status.NvidiaDeviceNotFound: No NVIDIA GPU driving a display was found.</exception>
        /// <exception cref="Exception">A delegate callback throws an exception.</exception>
        public static DynamicPerformanceStatesInfoV1 GetDynamicPerformanceStatesInfoEx(
            PhysicalGPUHandle physicalGPUHandle)
        {
            var getDynamicPerformanceStatesInfoEx =
                DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetDynamicPStatesInfoEx>();

            foreach (var acceptType in getDynamicPerformanceStatesInfoEx.Accepts())
            {
                var instance = acceptType.Instantiate<DynamicPerformanceStatesInfoV1>();

                using (var gpuDynamicPStateInfo = ValueTypeReference.FromValueType(instance, acceptType))
                {
                    var status = getDynamicPerformanceStatesInfoEx(physicalGPUHandle, gpuDynamicPStateInfo);

                    if (status == Status.IncompatibleStructureVersion)
                    {
                        continue;
                    }

                    if (status != Status.Ok)
                    {
                        throw new NVIDIAApiException(status);
                    }

                    return gpuDynamicPStateInfo.ToValueType<DynamicPerformanceStatesInfoV1>(acceptType);
                }
            }

            throw new NVIDIANotSupportedException("This operation is not supported.");
        }

        /// <summary>
        ///     Gets the reason behind the current decrease in performance.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>A value indicating the reason of current performance decrease.</returns>
        public static PerformanceDecreaseReason GetPerformanceDecreaseInfo(PhysicalGPUHandle gpuHandle)
        {
            var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetPerfDecreaseInfo>()(
                gpuHandle,
                out var decreaseReason
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return decreaseReason;
        }

        /// <summary>
        ///     [PRIVATE]
        ///     Gets the GPU usage metrics for the passed GPU handle.
        /// </summary>
        /// <param name="gpuHandle">The handle of the GPU to perform the operation on.</param>
        /// <returns>The usage information for the selected GPU.</returns>
        public static PrivateUsagesInfoV1 GetUsages(PhysicalGPUHandle gpuHandle)
        {
            var instance = typeof(PrivateUsagesInfoV1).Instantiate<PrivateUsagesInfoV1>();

            using (var usageInfoReference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_GetUsages>()(
                    gpuHandle,
                    usageInfoReference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return usageInfoReference.ToValueType<PrivateUsagesInfoV1>(typeof(PrivateUsagesInfoV1));
            }
        }

        /// <summary>
        ///     Queries active applications.
        /// </summary>
        /// <param name="gpuHandle">The physical GPU handle.</param>
        /// <returns>The list of active applications.</returns>
        public static PrivateActiveApplicationV2[] QueryActiveApps(PhysicalGPUHandle gpuHandle)
        {
            var queryActiveApps = DelegateFactory.GetDelegate<GPUDelegates.NvAPI_GPU_QueryActiveApps>();

            // ReSharper disable once EventExceptionNotDocumented
            if (!queryActiveApps.Accepts().Contains(typeof(PrivateActiveApplicationV2)))
            {
                throw new NVIDIANotSupportedException("This operation is not supported.");
            }

            uint count = PrivateActiveApplicationV2.MaximumNumberOfApplications;
            var instances = typeof(PrivateActiveApplicationV2).Instantiate<PrivateActiveApplicationV2>()
                .Repeat((int)count);

            using (var applications = ValueTypeArray.FromArray(instances))
            {
                // ReSharper disable once EventExceptionNotDocumented
                var status = queryActiveApps(gpuHandle, applications, ref count);

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return applications.ToArray<PrivateActiveApplicationV2>((int)count);
            }
        }



        /// <summary>
        ///     This API activates stereo for the device interface corresponding to the given stereo handle.
        ///     Activating stereo is possible only if stereo was enabled previously in the registry.
        ///     If stereo is not activated, then calls to functions that require that stereo is activated have no effect,
        ///     and will return the appropriate error code.
        /// </summary>
        /// <param name="handle">Stereo handle corresponding to the device interface.</param>
        public static void ActivateStereo(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_Activate>()(
                handle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API captures the current stereo image in JPEG stereo format with the given quality.
        ///     Only the last capture call per flip will be effective.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <param name="quality">Quality of the JPEG image to be captured. Integer value between 0 and 100.</param>
        public static void CaptureJpegImage(StereoHandle handle, uint quality)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_CaptureJpegImage>()(
                handle,
                quality
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API captures the current stereo image in PNG stereo format.
        ///     Only the last capture call per flip will be effective.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        public static void CapturePngImage(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_CapturePngImage>()(
                handle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     Creates new configuration registry key for current application.
        ///     If there is no configuration profile prior to the function call,
        ///     this API tries to create a new configuration profile registry key
        ///     for a given application and fill it with the default values.
        ///     If an application already has a configuration profile registry key, the API does nothing.
        ///     The name of the key is automatically set to the name of the executable that calls this function.
        ///     Because of this, the executable should have a distinct and unique name.
        ///     If the application is using only one version of DirectX, then the default profile type will be appropriate.
        ///     If the application is using more than one version of DirectX from the same executable,
        ///     it should use the appropriate profile type for each configuration profile.
        /// </summary>
        /// <param name="registryProfileType">Type of profile the application wants to create.</param>
        public static void CreateConfigurationProfileRegistryKey(
            StereoRegistryProfileType registryProfileType)
        {
            var status = DelegateFactory
                .GetDelegate<StereoDelegates.NvAPI_Stereo_CreateConfigurationProfileRegistryKey>()(
                    registryProfileType
                );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API creates a stereo handle that is used in subsequent calls related to a given device interface.
        ///     This must be called before any other NvAPI_Stereo_ function for that handle.
        ///     Multiple devices can be used at one time using multiple calls to this function (one per each device).
        ///     HOW TO USE: After the Direct3D device is created, create the stereo handle.
        ///     On call success:
        ///     -# Use all other functions that have stereo handle as first parameter.
        ///     -# After the device interface that corresponds to the the stereo handle is destroyed,
        ///     the application should call NvAPI_DestroyStereoHandle() for that stereo handle.
        /// </summary>
        /// <param name="d3dDevice">Pointer to IUnknown interface that is IDirect3DDevice9* in DX9, ID3D10Device*.</param>
        /// <returns>Newly created stereo handle.</returns>
        // ReSharper disable once InconsistentNaming
        public static StereoHandle CreateHandleFromIUnknown(IntPtr d3dDevice)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_CreateHandleFromIUnknown>()(
                d3dDevice,
                out var stereoHandle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return stereoHandle;
        }

        // ReSharper disable once CommentTypo
        /// <summary>
        ///     This API allows the user to create a mono or a stereo swap chain.
        ///     NOTE: NvAPI_D3D1x_CreateSwapChain is a wrapper of the method IDXGIFactory::CreateSwapChain which
        ///     additionally notifies the D3D driver of the mode in which the swap chain is to be
        ///     created.
        /// </summary>
        /// <param name="handle">
        ///     Stereo handle that corresponds to the device interface. The device that will write 2D images to
        ///     the swap chain.
        /// </param>
        /// <param name="dxgiSwapChainDescription">
        ///     A pointer to the swap-chain description (DXGI_SWAP_CHAIN_DESC). This parameter
        ///     cannot be NULL.
        /// </param>
        /// <param name="swapChainMode">The stereo mode fot the swap chain.</param>
        /// <returns>A pointer to the swap chain created.</returns>
        public static IntPtr D3D1XCreateSwapChain(
            StereoHandle handle,
            IntPtr dxgiSwapChainDescription,
            StereoSwapChainMode swapChainMode)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_D3D1x_CreateSwapChain>()(
                handle,
                dxgiSwapChainDescription,
                out var dxgiSwapChain,
                swapChainMode
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return dxgiSwapChain;
        }

        /// <summary>
        ///     This API allows the user to create a mono or a stereo swap chain.
        ///     NOTE: NvAPI_D3D9_CreateSwapChain is a wrapper of the method IDirect3DDevice9::CreateAdditionalSwapChain which
        ///     additionally notifies the D3D driver if the swap chain creation mode must be stereo or mono.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <param name="d3dPresentParameters">A pointer to the swap-chain description (DXGI). This parameter cannot be NULL.</param>
        /// <param name="swapChainMode">The stereo mode for the swap chain.</param>
        /// <returns>A pointer to the swap chain created.</returns>
        public static IntPtr D3D9CreateSwapChain(
            StereoHandle handle,
            // ReSharper disable once InconsistentNaming
            IntPtr d3dPresentParameters,
            StereoSwapChainMode swapChainMode)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_D3D9_CreateSwapChain>()(
                handle,
                d3dPresentParameters,
                out var direct3DSwapChain9,
                swapChainMode
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return direct3DSwapChain9;
        }

        /// <summary>
        ///     This API deactivates stereo for the given device interface.
        ///     If stereo is not activated, then calls to functions that require that stereo is activated have no effect,
        ///     and will return the appropriate error code.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        public static void DeactivateStereo(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_Deactivate>()(
                handle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API decreases convergence for the given device interface (just like the Ctrl+F5 hot-key).
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        public static void DecreaseConvergence(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_DecreaseConvergence>()(
                handle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API decreases separation for the given device interface (just like the Ctrl+F3 hot-key).
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        public static void DecreaseSeparation(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_DecreaseSeparation>()(
                handle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     Removes configuration registry key for current application.
        ///     If an application already has a configuration profile prior to this function call,
        ///     the function attempts to remove the application's configuration profile registry key from the registry.
        ///     If there is no configuration profile registry key prior to the function call,
        ///     the function does nothing and does not report an error.
        /// </summary>
        /// <param name="registryProfileType">Type of profile that the application wants to delete.</param>
        public static void DeleteConfigurationProfileRegistryKey(
            StereoRegistryProfileType registryProfileType)
        {
            var status = DelegateFactory
                .GetDelegate<StereoDelegates.NvAPI_Stereo_DeleteConfigurationProfileRegistryKey>()(
                    registryProfileType
                );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API removes the given value from the application's configuration profile registry key.
        ///     If there is no such value, the function does nothing and does not report an error.
        /// </summary>
        /// <param name="registryProfileType">The type of profile the application wants to access.</param>
        /// <param name="registryId">ID of the value that is being deleted.</param>
        public static void DeleteConfigurationProfileValue(
            StereoRegistryProfileType registryProfileType,
            StereoRegistryIdentification registryId)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_DeleteConfigurationProfileValue>()(
                registryProfileType,
                registryId
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API destroys the stereo handle created with one of the NvAPI_Stereo_CreateHandleFrom() functions.
        ///     This should be called after the device corresponding to the handle has been destroyed.
        /// </summary>
        /// <param name="handle">Stereo handle that is to be destroyed.</param>
        public static void DestroyHandle(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_DestroyHandle>()(
                handle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API disables stereo mode in the registry.
        ///     Calls to this function affect the entire system.
        ///     If stereo is not enabled, then calls to functions that require that stereo is enabled have no effect,
        ///     and will return the appropriate error code.
        /// </summary>
        public static void DisableStereo()
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_Disable>()();

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This APU enables stereo mode in the registry.
        ///     Calls to this function affect the entire system.
        ///     If stereo is not enabled, then calls to functions that require that stereo is enabled have no effect,
        ///     and will return the appropriate error code.
        /// </summary>
        public static void EnableStereo()
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_Enable>()();

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API gets the current convergence value.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <returns>Current convergence value</returns>
        public static float GetConvergence(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_GetConvergence>()(
                handle,
                out var convergence
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return convergence;
        }

        /// <summary>
        ///     This API retrieves the current default stereo profile.
        /// </summary>
        /// <returns>Default stereo profile name.</returns>
        public static string GetDefaultProfile()
        {
            var stringCapacity = 256;
            var stringAddress = Marshal.AllocHGlobal(stringCapacity);

            try
            {
                var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_GetDefaultProfile>()(
                    (uint)stringCapacity,
                    stringAddress,
                    out var stringSize
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                if (stringSize == 0)
                {
                    return null;
                }

                return Marshal.PtrToStringAnsi(stringAddress, (int)stringSize);
            }
            finally
            {
                Marshal.FreeHGlobal(stringAddress);
            }
        }

        /// <summary>
        ///     This API returns eye separation as a ratio of [between eye distance]/[physical screen width].
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <returns>Eye separation</returns>
        public static float GetEyeSeparation(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_GetEyeSeparation>()(
                handle,
                out var eyeSeparation
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return eyeSeparation;
        }

        /// <summary>
        ///     This API gets the current frustum adjust mode value.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <returns>Current frustum value</returns>
        public static StereoFrustumAdjustMode GetFrustumAdjustMode(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_GetFrustumAdjustMode>()(
                handle,
                out var frustumAdjustMode
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return frustumAdjustMode;
        }

        /// <summary>
        ///     This API gets current separation value (in percents).
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <returns>Current separation percentage</returns>
        public static float GetSeparation(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_GetSeparation>()(
                handle,
                out var separationPercentage
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return separationPercentage;
        }

        /// <summary>
        ///     This API checks what kind of stereo support is currently supported on a particular display.
        ///     If the the display is prohibited from showing stereo (e.g. secondary in a multi-mon setup), we will
        ///     return 0 for all stereo modes (full screen exclusive, automatic windowed, persistent windowed).
        ///     Otherwise, we will check which stereo mode is supported. On 120Hz display, this will be what
        ///     the user chooses in control panel. On HDMI 1.4 display, persistent windowed mode is always assumed to be
        ///     supported. Note that this function does not check if the CURRENT RESOLUTION/REFRESH RATE can support
        ///     stereo. For HDMI 1.4, it is the application's responsibility to change the resolution/refresh rate to one that is
        ///     3D compatible. For 120Hz, the driver will ALWAYS force 120Hz anyway.
        /// </summary>
        /// <param name="monitorHandle">Monitor that app is going to run on</param>
        /// <returns>An instance of <see cref="StereoCapabilitiesV1" /> structure.</returns>
        public static StereoCapabilitiesV1 GetStereoSupport(IntPtr monitorHandle)
        {
            var instance = typeof(StereoCapabilitiesV1).Instantiate<StereoCapabilitiesV1>();

            using (var reference = ValueTypeReference.FromValueType(instance))
            {
                var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_GetStereoSupport>()(
                    monitorHandle,
                    reference
                );

                if (status != Status.Ok)
                {
                    throw new NVIDIAApiException(status);
                }

                return reference.ToValueType<StereoCapabilitiesV1>(typeof(StereoCapabilitiesV1));
            }
        }

        /// <summary>
        ///     This API gets surface creation mode for this device interface.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <returns>The current creation mode for this device interface.</returns>
        public static StereoSurfaceCreateMode GetSurfaceCreationMode(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_GetSurfaceCreationMode>()(
                handle,
                out var surfaceCreateMode
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return surfaceCreateMode;
        }

        /// <summary>
        ///     This API increases convergence for given the device interface (just like the Ctrl+F6 hot-key).
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        public static void IncreaseConvergence(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_IncreaseConvergence>()(
                handle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API increases separation for the given device interface (just like the Ctrl+F4 hot-key).
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        public static void IncreaseSeparation(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_IncreaseSeparation>()(
                handle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API allows an application to enable stereo viewing, without the need of a GUID/Key pair
        ///     This API cannot be used to enable stereo viewing on 3DTV.
        ///     HOW TO USE:    Call this function immediately after device creation, then follow with a reset. \n
        ///     Very generically:
        ///     Create Device->Create Stereo Handle->InitActivation->Reset Device
        /// </summary>
        /// <param name="handle">Stereo handle corresponding to the device interface.</param>
        /// <param name="activationFlag">Flags to enable or disable delayed activation.</param>
        public static void InitActivation(StereoHandle handle, StereoActivationFlag activationFlag)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_InitActivation>()(
                handle,
                activationFlag
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API checks if stereo is activated for the given device interface.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <returns>Address where result of the inquiry will be placed.</returns>
        public static bool IsStereoActivated(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_IsActivated>()(
                handle,
                out var isStereoActive
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return isStereoActive > 0;
        }

        /// <summary>
        ///     This API checks if stereo mode is enabled in the registry.
        /// </summary>
        /// <returns>true if the stereo is enable; otherwise false</returns>
        public static bool IsStereoEnabled()
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_IsEnabled>()(
                out var isEnable
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return isEnable > 0;
        }

        /// <summary>
        ///     This API returns availability of windowed mode stereo
        /// </summary>
        /// <returns>true if windowed mode is supported; otherwise false</returns>
        public static bool IsWindowedModeSupported()
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_IsWindowedModeSupported>()(
                out var supported
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return supported > 0;
        }

        /// <summary>
        ///     This API turns on/off reverse stereo blit.
        ///     After reversed stereo blit control is turned on, blits from the stereo surface will
        ///     produce the right-eye image in the left side of the destination surface and the left-eye
        ///     image in the right side of the destination surface.
        ///     In DirectX 9, the destination surface must be created as the render target, and StretchRect must be used.
        ///     Conditions:
        ///     - DstWidth == 2*SrcWidth
        ///     - DstHeight == SrcHeight
        ///     - Src surface is the stereo surface.
        ///     - SrcRect must be {0,0,SrcWidth,SrcHeight}
        ///     - DstRect must be {0,0,DstWidth,DstHeight}
        ///     In DirectX 10, ResourceCopyRegion must be used.
        ///     Conditions:
        ///     - DstWidth == 2*SrcWidth
        ///     - DstHeight == SrcHeight
        ///     - dstX == 0,
        ///     - dstY == 0,
        ///     - dstZ == 0,
        ///     - SrcBox: left=top=front==0; right==SrcWidth; bottom==SrcHeight; back==1;
        /// </summary>
        /// <param name="handle">Stereo handle corresponding to the device interface.</param>
        /// <param name="turnOn">A boolean value to enable or disable blit control</param>
        public static void ReverseStereoBlitControl(StereoHandle handle, bool turnOn)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_ReverseStereoBlitControl>()(
                handle,
                (byte)(turnOn ? 1 : 0)
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets the back buffer to left or right in Direct stereo mode.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <param name="activeEye">Defines active eye in Direct stereo mode</param>
        public static void SetActiveEye(StereoHandle handle, StereoActiveEye activeEye)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetActiveEye>()(
                handle,
                activeEye
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets the given parameter value under the application's registry key.
        ///     If the value does not exist under the application's registry key, the value will be created under the key.
        /// </summary>
        /// <param name="registryProfileType">The type of profile the application wants to access.</param>
        /// <param name="registryId">ID of the value that is being set.</param>
        /// <param name="value">Value that is being set.</param>
        public static void SetConfigurationProfileValue(
            StereoRegistryProfileType registryProfileType,
            StereoRegistryIdentification registryId,
            float value)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetConfigurationProfileValueFloat>()(
                registryProfileType,
                registryId,
                ref value
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets the given parameter value under the application's registry key.
        ///     If the value does not exist under the application's registry key, the value will be created under the key.
        /// </summary>
        /// <param name="registryProfileType">The type of profile the application wants to access.</param>
        /// <param name="registryId">ID of the value that is being set.</param>
        /// <param name="value">Value that is being set.</param>
        public static void SetConfigurationProfileValue(
            StereoRegistryProfileType registryProfileType,
            StereoRegistryIdentification registryId,
            int value)
        {
            var status =
                DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetConfigurationProfileValueInteger>()(
                    registryProfileType,
                    registryId,
                    ref value
                );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets convergence to the given value.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <param name="convergence">New value for convergence.</param>
        public static void SetConvergence(StereoHandle handle, float convergence)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetConvergence>()(
                handle,
                convergence
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API defines the stereo profile used by the driver in case the application has no associated profile.
        ///     To take effect, this API must be called before D3D device is created. Calling once a device has been created will
        ///     not affect the current device.
        /// </summary>
        /// <param name="profileName">Default profile name. </param>
        public static void SetDefaultProfile(string profileName)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetDefaultProfile>()(
                profileName
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets the 3D stereo driver mode: Direct or Automatic
        /// </summary>
        /// <param name="driverMode">Defines the 3D stereo driver mode: Direct or Automatic</param>
        public static void SetDriverMode(StereoDriverMode driverMode)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetDriverMode>()(
                driverMode
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets the current frustum adjust mode value.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <param name="frustumAdjustMode">New value for frustum adjust mode.</param>
        public static void SetFrustumAdjustMode(
            StereoHandle handle,
            StereoFrustumAdjustMode frustumAdjustMode)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetFrustumAdjustMode>()(
                handle,
                frustumAdjustMode
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API checks if the last draw call was stereoized. It is a very expensive to call and should be used for
        ///     debugging purpose *only*.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <returns>true if the last draw was a stereo draw; otherwise false</returns>
        public static bool WasLastDrawStereoizedDebug(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_Debug_WasLastDrawStereoized>()(
                handle,
                out var supported
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }

            return supported > 0;
        } // ReSharper disable CommentTypo
        /// <summary>
        ///     This API is a Setup notification message that the stereo driver uses to notify the application
        ///     when the user changes the stereo driver state.
        ///     When the user changes the stereo state (Activated or Deactivated, separation or conversion)
        ///     the stereo driver posts a defined message with the following parameters:
        ///     lParam  is the current conversion. (Actual conversion is *(float*)&amp;lParam )
        ///     wParam == MAKEWPARAM(l, h) where
        ///     - l == 0 if stereo is deactivated
        ///     - l == 1 if stereo is deactivated
        ///     - h is the current separation. (Actual separation is float(h*100.f/0xFFFF)
        ///     Call this API with NULL hWnd to prohibit notification.
        /// </summary>
        /// <param name="handle">Stereo handle corresponding to the device interface.</param>
        /// <param name="windowsHandle">
        ///     Window handle that will be notified when the user changes the stereo driver state. Actual
        ///     handle must be cast to an <see cref="ulong" />.
        /// </param>
        /// <param name="messageId">MessageID of the message that will be posted to window</param>
        public static void SetNotificationMessage(
            StereoHandle handle,
            ulong windowsHandle,
            ulong messageId)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetNotificationMessage>()(
                handle,
                windowsHandle,
                messageId
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets separation to given percentage.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <param name="separationPercentage">New value for separation percentage.</param>
        public static void SetSeparation(StereoHandle handle, float separationPercentage)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetSeparation>()(
                handle,
                separationPercentage
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API sets surface creation mode for this device interface.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        /// <param name="surfaceCreateMode">New surface creation mode for this device interface.</param>
        public static void SetSurfaceCreationMode(
            StereoHandle handle,
            StereoSurfaceCreateMode surfaceCreateMode)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_SetSurfaceCreationMode>()(
                handle,
                surfaceCreateMode
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

        /// <summary>
        ///     This API allows an application to trigger creation of a stereo desktop,
        ///     in case the creation was stopped on application launch.
        /// </summary>
        /// <param name="handle">Stereo handle that corresponds to the device interface.</param>
        public static void TriggerActivation(StereoHandle handle)
        {
            var status = DelegateFactory.GetDelegate<StereoDelegates.NvAPI_Stereo_Trigger_Activation>()(
                handle
            );

            if (status != Status.Ok)
            {
                throw new NVIDIAApiException(status);
            }
        }

    }
}
