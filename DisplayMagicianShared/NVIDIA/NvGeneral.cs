using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.NVIDIA
{
    /// <summary>
    ///     NvAPI status codes
    /// </summary>
    public enum Status
    {
        /// <summary>
        ///     Success. Request is completed.
        /// </summary>
        Ok = 0,

        /// <summary>
        ///     Generic error
        /// </summary>
        Error = -1,

        /// <summary>
        ///     NVAPI support library cannot be loaded.
        /// </summary>
        LibraryNotFound = -2,

        /// <summary>
        ///     Not implemented in current driver installation
        /// </summary>
        NoImplementation = -3,

        /// <summary>
        ///     NvAPI_Initialize() has not been called (successfully)
        /// </summary>
        ApiNotInitialized = -4,

        /// <summary>
        ///     Invalid argument
        /// </summary>
        InvalidArgument = -5,

        /// <summary>
        ///     No NVIDIA display driver was found
        /// </summary>
        NvidiaDeviceNotFound = -6,

        /// <summary>
        ///     No more to enumerate
        /// </summary>
        EndEnumeration = -7,

        /// <summary>
        ///     Invalid handle
        /// </summary>
        InvalidHandle = -8,

        /// <summary>
        ///     An argument's structure version is not supported
        /// </summary>
        IncompatibleStructureVersion = -9,

        /// <summary>
        ///     Handle is no longer valid (likely due to GPU or display re-configuration)
        /// </summary>
        HandleInvalidated = -10,

        /// <summary>
        ///     No NVIDIA OpenGL context is current (but needs to be)
        /// </summary>
        OpenGLContextNotCurrent = -11,

        /// <summary>
        ///     An invalid pointer, usually NULL, was passed as a parameter
        /// </summary>
        InvalidPointer = -14,

        /// <summary>
        ///     OpenGL Expert is not supported by the current drivers
        /// </summary>
        NoGLExpert = -12,

        /// <summary>
        ///     OpenGL Expert is supported, but driver instrumentation is currently disabled
        /// </summary>
        InstrumentationDisabled = -13,

        /// <summary>
        ///     Expected a logical GPU handle for one or more parameters
        /// </summary>
        ExpectedLogicalGPUHandle = -100,

        /// <summary>
        ///     Expected a physical GPU handle for one or more parameters
        /// </summary>
        ExpectedPhysicalGPUHandle = -101,

        /// <summary>
        ///     Expected an NV display handle for one or more parameters
        /// </summary>
        ExpectedDisplayHandle = -102,

        /// <summary>
        ///     Used in some commands to indicate that the combination of parameters is not valid
        /// </summary>
        InvalidCombination = -103,

        /// <summary>
        ///     Requested feature not supported in the selected GPU
        /// </summary>
        NotSupported = -104,

        /// <summary>
        ///     NO port Id found for I2C transaction
        /// </summary>
        PortIdNotFound = -105,

        /// <summary>
        ///     Expected an unattached display handle as one of the input param
        /// </summary>
        ExpectedUnattachedDisplayHandle = -106,

        /// <summary>
        ///     Invalid performance level
        /// </summary>
        InvalidPerformanceLevel = -107,

        /// <summary>
        ///     Device is busy, request not fulfilled
        /// </summary>
        DeviceBusy = -108,

        /// <summary>
        ///     NVIDIA persist file is not found
        /// </summary>
        NvPersistFileNotFound = -109,

        /// <summary>
        ///     NVIDIA persist data is not found
        /// </summary>
        PersistDataNotFound = -110,

        /// <summary>
        ///     Expected TV output display
        /// </summary>
        ExpectedTVDisplay = -111,

        /// <summary>
        ///     Expected TV output on D Connector - HDTV_EIAJ4120.
        /// </summary>
        ExpectedTVDisplayOnDConnector = -112,

        /// <summary>
        ///     SLI is not active on this device
        /// </summary>
        NoActiveSLITopology = -113,

        /// <summary>
        ///     Setup of SLI rendering mode is not possible right now
        /// </summary>
        SLIRenderingModeNotAllowed = -114,

        /// <summary>
        ///     Expected digital flat panel
        /// </summary>
        ExpectedDigitalFlatPanel = -115,

        /// <summary>
        ///     Argument exceeds expected size
        /// </summary>
        ArgumentExceedMaxSize = -116,

        /// <summary>
        ///     Inhibit ON due to one of the flags in NV_GPU_DISPLAY_CHANGE_INHIBIT or SLI Active
        /// </summary>
        DeviceSwitchingNotAllowed = -117,

        /// <summary>
        ///     Testing clocks not supported
        /// </summary>
        TestingClocksNotSupported = -118,

        /// <summary>
        ///     The specified underscan config is from an unknown source (e.g. INF)
        /// </summary>
        UnknownUnderScanConfig = -119,

        /// <summary>
        ///     Timeout while reconfiguring GPUs
        /// </summary>
        TimeoutReConfiguringGPUTopology = -120,

        /// <summary>
        ///     Requested data was not found
        /// </summary>
        DataNotFound = -121,

        /// <summary>
        ///     Expected analog display
        /// </summary>
        ExpectedAnalogDisplay = -122,

        /// <summary>
        ///     No SLI video bridge present
        /// </summary>
        NoVideoLink = -123,

        /// <summary>
        ///     NvAPI requires reboot for its settings to take effect
        /// </summary>
        RequiresReboot = -124,

        /// <summary>
        ///     The function is not supported with the current hybrid mode.
        /// </summary>
        InvalidHybridMode = -125,

        /// <summary>
        ///     The target types are not all the same
        /// </summary>
        MixedTargetTypes = -126,

        /// <summary>
        ///     The function is not supported from 32-bit on a 64-bit system
        /// </summary>
        SYSWOW64NotSupported = -127,

        /// <summary>
        ///     There is any implicit GPU topology active. Use NVAPI_SetHybridMode to change topology.
        /// </summary>
        ImplicitSetGPUTopologyChangeNotAllowed = -128,


        /// <summary>
        ///     Prompt the user to close all non-migratable applications.
        /// </summary>
        RequestUserToCloseNonMigratableApps = -129,

        /// <summary>
        ///     Could not allocate sufficient memory to complete the call
        /// </summary>
        OutOfMemory = -130,

        /// <summary>
        ///     The previous operation that is transferring information to or from this surface is incomplete
        /// </summary>
        WasStillDrawing = -131,

        /// <summary>
        ///     The file was not found
        /// </summary>
        FileNotFound = -132,

        /// <summary>
        ///     There are too many unique instances of a particular type of state object
        /// </summary>
        TooManyUniqueStateObjects = -133,


        /// <summary>
        ///     The method call is invalid. For example, a method's parameter may not be a valid pointer
        /// </summary>
        InvalidCall = -134,

        /// <summary>
        ///     d3d10_1.dll can not be loaded
        /// </summary>
        D3D101LibraryNotFound = -135,

        /// <summary>
        ///     Couldn't find the function in loaded DLL library
        /// </summary>
        FunctionNotFound = -136,

        /// <summary>
        ///     Current User is not Administrator
        /// </summary>
        InvalidUserPrivilege = -137,

        /// <summary>
        ///     The handle corresponds to GDIPrimary
        /// </summary>
        ExpectedNonPrimaryDisplayHandle = -138,

        /// <summary>
        ///     Setting PhysX GPU requires that the GPU is compute capable
        /// </summary>
        ExpectedComputeGPUHandle = -139,

        /// <summary>
        ///     Stereo part of NvAPI failed to initialize completely. Check if stereo driver is installed.
        /// </summary>
        StereoNotInitialized = -140,

        /// <summary>
        ///     Access to stereo related registry keys or values failed.
        /// </summary>
        StereoRegistryAccessFailed = -141,

        /// <summary>
        ///     Given registry profile type is not supported.
        /// </summary>
        StereoRegistryProfileTypeNotSupported = -142,

        /// <summary>
        ///     Given registry value is not supported.
        /// </summary>
        StereoRegistryValueNotSupported = -143,

        /// <summary>
        ///     Stereo is not enabled and function needed it to execute completely.
        /// </summary>
        StereoNotEnabled = -144,

        /// <summary>
        ///     Stereo is not turned on and function needed it to execute completely.
        /// </summary>
        StereoNotTurnedOn = -145,

        /// <summary>
        ///     Invalid device interface.
        /// </summary>
        StereoInvalidDeviceInterface = -146,


        /// <summary>
        ///     Separation percentage or JPEG image capture quality out of [0-100] range.
        /// </summary>
        StereoParameterOutOfRange = -147,

        /// <summary>
        ///     Given frustum adjust mode is not supported.
        /// </summary>
        StereoFrustumAdjustModeNotSupported = -148,

        /// <summary>
        ///     The mosaic topology is not possible given the current state of HW
        /// </summary>
        TopologyNotPossible = -149,

        /// <summary>
        ///     An attempt to do a display resolution mode change has failed
        /// </summary>
        ModeChangeFailed = -150,

        /// <summary>
        ///     d3d11.dll/d3d11_beta.dll cannot be loaded.
        /// </summary>
        D3D11LibraryNotFound = -151,

        /// <summary>
        ///     Address outside of valid range.
        /// </summary>
        InvalidAddress = -152,

        /// <summary>
        ///     The pre-allocated string is too small to hold the result.
        /// </summary>
        StringTooSmall = -153,

        /// <summary>
        ///     The input does not match any of the available devices.
        /// </summary>
        MatchingDeviceNotFound = -154,

        /// <summary>
        ///     Driver is running.
        /// </summary>
        DriverRunning = -155,

        /// <summary>
        ///     Driver is not running.
        /// </summary>
        DriverNotRunning = -156,

        /// <summary>
        ///     A driver reload is required to apply these settings.
        /// </summary>
        ErrorDriverReloadRequired = -157,

        /// <summary>
        ///     Intended setting is not allowed.
        /// </summary>
        SetNotAllowed = -158,

        /// <summary>
        ///     Information can't be returned due to "advanced display topology".
        /// </summary>
        AdvancedDisplayTopologyRequired = -159,

        /// <summary>
        ///     Setting is not found.
        /// </summary>
        SettingNotFound = -160,

        /// <summary>
        ///     Setting size is too large.
        /// </summary>
        SettingSizeTooLarge = -161,

        /// <summary>
        ///     There are too many settings for a profile.
        /// </summary>
        TooManySettingsInProfile = -162,

        /// <summary>
        ///     Profile is not found.
        /// </summary>
        ProfileNotFound = -163,

        /// <summary>
        ///     Profile name is duplicated.
        /// </summary>
        ProfileNameInUse = -164,

        /// <summary>
        ///     Profile name is empty.
        /// </summary>
        ProfileNameEmpty = -165,

        /// <summary>
        ///     Application not found in the Profile.
        /// </summary>
        ExecutableNotFound = -166,

        /// <summary>
        ///     Application already exists in the other profile.
        /// </summary>
        ExecutableAlreadyInUse = -167,

        /// <summary>
        ///     Data Type mismatch
        /// </summary>
        DataTypeMismatch = -168,

        /// <summary>
        ///     The profile passed as parameter has been removed and is no longer valid.
        /// </summary>
        ProfileRemoved = -169,

        /// <summary>
        ///     An unregistered resource was passed as a parameter.
        /// </summary>
        UnregisteredResource = -170,

        /// <summary>
        ///     The DisplayId corresponds to a display which is not within the normal outputId range.
        /// </summary>
        IdOutOfRange = -171,

        /// <summary>
        ///     Display topology is not valid so the driver cannot do a mode set on this configuration.
        /// </summary>
        DisplayConfigValidationFailed = -172,

        /// <summary>
        ///     Display Port Multi-Stream topology has been changed.
        /// </summary>
        DPMSTChanged = -173,

        /// <summary>
        ///     Input buffer is insufficient to hold the contents.
        /// </summary>
        InsufficientBuffer = -174,

        /// <summary>
        ///     No access to the caller.
        /// </summary>
        AccessDenied = -175,

        /// <summary>
        ///     The requested action cannot be performed without Mosaic being enabled.
        /// </summary>
        MosaicNotActive = -176,

        /// <summary>
        ///     The surface is relocated away from video memory.
        /// </summary>
        ShareResourceRelocated = -177,

        /// <summary>
        ///     The user should disable DWM before calling NvAPI.
        /// </summary>
        RequestUserToDisableDWM = -178,

        /// <summary>
        ///     D3D device status is "D3DERR_DEVICELOST" or "D3DERR_DEVICENOTRESET" - the user has to reset the device.
        /// </summary>
        D3DDeviceLost = -179,

        /// <summary>
        ///     The requested action cannot be performed in the current state.
        /// </summary>
        InvalidConfiguration = -180,

        /// <summary>
        ///     Call failed as stereo handshake not completed.
        /// </summary>
        StereoHandshakeNotDone = -181,

        /// <summary>
        ///     The path provided was too short to determine the correct NVDRS_APPLICATION
        /// </summary>
        ExecutablePathIsAmbiguous = -182,

        /// <summary>
        ///     Default stereo profile is not currently defined
        /// </summary>
        DefaultStereoProfileIsNotDefined = -183,

        /// <summary>
        ///     Default stereo profile does not exist
        /// </summary>
        DefaultStereoProfileDoesNotExist = -184,

        /// <summary>
        ///     A cluster is already defined with the given configuration.
        /// </summary>
        ClusterAlreadyExists = -185,

        /// <summary>
        ///     The input display id is not that of a multi stream enabled connector or a display device in a multi stream topology
        /// </summary>
        DPMSTDisplayIdExpected = -186,

        /// <summary>
        ///     The input display id is not valid or the monitor associated to it does not support the current operation
        /// </summary>
        InvalidDisplayId = -187,

        /// <summary>
        ///     While playing secure audio stream, stream goes out of sync
        /// </summary>
        StreamIsOutOfSync = -188,

        /// <summary>
        ///     Older audio driver version than required
        /// </summary>
        IncompatibleAudioDriver = -189,

        /// <summary>
        ///     Value already set, setting again not allowed.
        /// </summary>
        ValueAlreadySet = -190,

        /// <summary>
        ///     Requested operation timed out
        /// </summary>
        Timeout = -191,

        /// <summary>
        ///     The requested workstation feature set has incomplete driver internal allocation resources
        /// </summary>
        GPUWorkstationFeatureIncomplete = -192,

        /// <summary>
        ///     Call failed because InitActivation was not called.
        /// </summary>
        StereoInitActivationNotDone = -193,

        /// <summary>
        ///     The requested action cannot be performed without Sync being enabled.
        /// </summary>
        SyncNotActive = -194,

        /// <summary>
        ///     The requested action cannot be performed without Sync Master being enabled.
        /// </summary>
        SyncMasterNotFound = -195,

        /// <summary>
        ///     Invalid displays passed in the NV_GSYNC_DISPLAY pointer.
        /// </summary>
        InvalidSyncTopology = -196,

        /// <summary>
        ///     The specified signing algorithm is not supported. Either an incorrect value was entered or the current installed
        ///     driver/hardware does not support the input value.
        /// </summary>
        ECIDSignAlgoUnsupported = -197,

        /// <summary>
        ///     The encrypted public key verification has failed.
        /// </summary>
        ECIDKeyVerificationFailed = -198,

        /// <summary>
        ///     The device's firmware is out of date.
        /// </summary>
        FirmwareOutOfDate = -199,

        /// <summary>
        ///     The device's firmware is not supported.
        /// </summary>
        FirmwareRevisionNotSupported = -200,

        /// <summary>
        ///     The caller is not authorized to modify the License.
        /// </summary>
        LicenseCallerAuthenticationFailed = -201,

        /// <summary>
        ///     The user tried to use a deferred context without registering the device first
        /// </summary>
        D3DDeviceNotRegistered = -202,

        /// <summary>
        ///     Head or SourceId was not reserved for the VR Display before doing the Mode-Set.
        /// </summary>
        ResourceNotAcquired = -203,

        /// <summary>
        ///     Provided timing is not supported.
        /// </summary>
        TimingNotSupported = -204,

        /// <summary>
        ///     HDCP Encryption Failed for the device. Would be applicable when the device is HDCP Capable.
        /// </summary>
        HDCPEncryptionFailed = -205,

        /// <summary>
        ///     Provided mode is over sink device pclk limitation.
        /// </summary>
        PCLKLimitationFailed = -206,

        /// <summary>
        ///     No connector on GPU found.
        /// </summary>
        NoConnectorFound = -207,

        /// <summary>
        ///     When a non-HDCP capable HMD is connected, we would inform user by this code.
        /// </summary>
        HDCPDisabled = -208,

        /// <summary>
        ///     At least an API is still being called
        /// </summary>
        ApiInUse = -209,

        /// <summary>
        ///     No display found on Nvidia GPU(s).
        /// </summary>
        NVIDIADisplayNotFound = -210
    }

    /// <summary>
    ///     Chipset information flags - obsolete
    /// </summary>
    [Flags]
    public enum ChipsetInfoFlag
    {
        /// <summary>
        ///     No flags
        /// </summary>
        None = 0,

        /// <summary>
        ///     Hybrid chipset configuration
        /// </summary>
        Hybrid = 1
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct StructureVersion : IEquatable<StructureVersion>
    {
        [JsonProperty]
        private uint _version;

        [JsonIgnore]
        public int VersionNumber
        {
            get => (int)(_version >> 16);
            set => _version = (uint) (value << 16);
        }

        [JsonIgnore]
        public int StructureSize
        {
            get => (int)(_version & ~(0xFFFF << 16));
            set => _version = (uint)((value & ~(0xFFFF >> 16)));
        }

        public StructureVersion(int version, Type structureType)
        {
            _version = (uint)(Marshal.SizeOf(structureType) | (version << 16));
        }

        public override string ToString()
        {
            return $"Structure Size: {StructureSize} Bytes, Version: {VersionNumber}";
        }

        public override bool Equals(object obj) => obj is StructureVersion other && this.Equals(other);
        public bool Equals(StructureVersion other)
        => _version == other._version;

        public override int GetHashCode()
        {
            return (_version).GetHashCode();
        }
        public static bool operator ==(StructureVersion lhs, StructureVersion rhs) => lhs.Equals(rhs);

        public static bool operator !=(StructureVersion lhs, StructureVersion rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct GenericString : IInitializable, IEquatable<GenericString>
    {
        public const int GenericStringLength = 4096;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = GenericStringLength)]
        private string _Value;

        public string Value
        {
            get => _Value;
            set => _Value = value ?? string.Empty;
        }

        public GenericString(string value)
        {
            _Value = value ?? string.Empty;
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj) => obj is GenericString other && this.Equals(other);
        public bool Equals(GenericString other)
        => _Value == other._Value;

        public override int GetHashCode()
        {
            return (_Value).GetHashCode();
        }
        public static bool operator ==(GenericString lhs, GenericString rhs) => lhs.Equals(rhs);

        public static bool operator !=(GenericString lhs, GenericString rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ShortString : IInitializable, IEquatable<ShortString> 
    {
        public const int ShortStringLength = 64;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ShortStringLength)]
        private string _Value;

        public string Value
        {
            get => _Value;
            set => _Value = value ?? string.Empty;
        }

        public ShortString(string value)
        {
            _Value = value ?? string.Empty;
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj) => obj is ShortString other && this.Equals(other);
        public bool Equals(ShortString other)
        => _Value == other._Value;

        public override int GetHashCode()
        {
            return (_Value).GetHashCode();
        }
        public static bool operator ==(ShortString lhs, ShortString rhs) => lhs.Equals(rhs);

        public static bool operator !=(ShortString lhs, ShortString rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct LongString : IInitializable, IEquatable<LongString>
    {
        public const int LongStringLength = 256;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = LongStringLength)]
        private string _Value;

        public string Value
        {
            get => _Value;
            set => _Value = value ?? string.Empty;
        }

        public LongString(string value)
        {
            _Value = value ?? string.Empty;
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj) => obj is LongString other && this.Equals(other);
        public bool Equals(LongString other)
        => _Value == other._Value;

        public override int GetHashCode()
        {
            return (_Value).GetHashCode();
        }
        public static bool operator ==(LongString lhs, LongString rhs) => lhs.Equals(rhs);

        public static bool operator !=(LongString lhs, LongString rhs) => !(lhs == rhs);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct UnicodeString : IInitializable, IEquatable<UnicodeString>
    {
        public const int UnicodeStringLength = 2048;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = UnicodeStringLength)]
        private string _Value;

        public string Value
        {
            get => _Value;
            set => _Value = value ?? string.Empty;
        }

        public UnicodeString(string value)
        {
            _Value = value ?? string.Empty;
        }

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(object obj) => obj is UnicodeString other && this.Equals(other);
        public bool Equals(UnicodeString other)
        => _Value == other._Value;

        public override int GetHashCode()
        {
            return (_Value).GetHashCode();
        }
        public static bool operator ==(UnicodeString lhs, UnicodeString rhs) => lhs.Equals(rhs);

        public static bool operator !=(UnicodeString lhs, UnicodeString rhs) => !(lhs == rhs);
    }

    /// <summary>
    ///     Holds information about the system's chipset.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ChipsetInfoV1 : IInitializable, IChipsetInfo, IEquatable<ChipsetInfoV1>
    {
        internal StructureVersion _Version;
        internal uint _VendorId;
        internal uint _DeviceId;
        internal ShortString _VendorName;
        internal ShortString _ChipsetName;

        /// <inheritdoc />
        public bool Equals(ChipsetInfoV1 other)
        {
            return _VendorId == other._VendorId &&
                   _DeviceId == other._DeviceId &&
                   _VendorName.Equals(other._VendorName) &&
                   _ChipsetName.Equals(other._ChipsetName);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ChipsetInfoV1 v1 && Equals(v1);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_VendorId;
                hashCode = (hashCode * 397) ^ (int)_DeviceId;
                hashCode = (hashCode * 397) ^ _VendorName.GetHashCode();
                hashCode = (hashCode * 397) ^ _ChipsetName.GetHashCode();

                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{VendorName} {ChipsetName}";
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public int VendorId
        {
            get => (int)_VendorId;
        }

        /// <inheritdoc />
        public int DeviceId
        {
            get => (int)_DeviceId;
        }

        /// <inheritdoc />
        public string VendorName
        {
            get => _VendorName.Value;
        }

        /// <inheritdoc />
        public string ChipsetName
        {
            get => _ChipsetName.Value;
        }

        /// <inheritdoc />
        public ChipsetInfoFlag Flags
        {
            get => ChipsetInfoFlag.None;
        }
    }

    /// <summary>
    ///     Holds information about the system's chipset.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct ChipsetInfoV2 : IInitializable, IChipsetInfo, IEquatable<ChipsetInfoV2>
    {
        internal StructureVersion _Version;
        internal uint _VendorId;
        internal readonly uint _DeviceId;
        internal ShortString _VendorName;
        internal ShortString _ChipsetName;
        internal ChipsetInfoFlag _Flags;

        /// <inheritdoc />
        public bool Equals(ChipsetInfoV2 other)
        {
            return _VendorId == other._VendorId &&
                   _DeviceId == other._DeviceId &&
                   _VendorName.Equals(other._VendorName) &&
                   _ChipsetName.Equals(other._ChipsetName) &&
                   _Flags == other._Flags;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ChipsetInfoV2 v2 && Equals(v2);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_VendorId;
                hashCode = (hashCode * 397) ^ (int)_DeviceId;
                hashCode = (hashCode * 397) ^ _VendorName.GetHashCode();
                hashCode = (hashCode * 397) ^ _ChipsetName.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_Flags;

                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{VendorName} {ChipsetName}";
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public int VendorId
        {
            get => (int)_VendorId;
        }

        /// <inheritdoc />
        public int DeviceId
        {
            get => (int)_DeviceId;
        }

        /// <inheritdoc />
        public string VendorName
        {
            get => _VendorName.Value;
        }

        /// <inheritdoc />
        public string ChipsetName
        {
            get => _ChipsetName.Value;
        }

        /// <inheritdoc />
        public ChipsetInfoFlag Flags
        {
            get => _Flags;
        }
    }

    /// <summary>
    ///     Holds information about the system's chipset.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct ChipsetInfoV3 : IInitializable, IChipsetInfo, IEquatable<ChipsetInfoV3>
    {
        internal StructureVersion _Version;
        internal uint _VendorId;
        internal uint _DeviceId;
        internal ShortString _VendorName;
        internal ShortString _ChipsetName;
        internal ChipsetInfoFlag _Flags;
        internal uint _SubSystemVendorId;
        internal uint _SubSystemDeviceId;
        internal ShortString _SubSystemVendorName;

        /// <inheritdoc />
        public bool Equals(ChipsetInfoV3 other)
        {
            return _VendorId == other._VendorId &&
                   _DeviceId == other._DeviceId &&
                   _VendorName.Equals(other._VendorName) &&
                   _ChipsetName.Equals(other._ChipsetName) &&
                   _Flags == other._Flags &&
                   _SubSystemVendorId == other._SubSystemVendorId &&
                   _SubSystemDeviceId == other._SubSystemDeviceId &&
                   _SubSystemVendorName.Equals(other._SubSystemVendorName);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ChipsetInfoV3 v3 && Equals(v3);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_VendorId;
                hashCode = (hashCode * 397) ^ (int)_DeviceId;
                hashCode = (hashCode * 397) ^ _VendorName.GetHashCode();
                hashCode = (hashCode * 397) ^ _ChipsetName.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_Flags;
                hashCode = (hashCode * 397) ^ (int)_SubSystemVendorId;
                hashCode = (hashCode * 397) ^ (int)_SubSystemDeviceId;
                hashCode = (hashCode * 397) ^ _SubSystemVendorName.GetHashCode();

                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{SubSystemVendorName} {VendorName} {ChipsetName}";
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public int VendorId
        {
            get => (int)_VendorId;
        }

        /// <inheritdoc />
        public int DeviceId
        {
            get => (int)_DeviceId;
        }

        /// <inheritdoc />
        public string VendorName
        {
            get => _VendorName.Value;
        }

        /// <inheritdoc />
        public string ChipsetName
        {
            get => _ChipsetName.Value;
        }

        /// <inheritdoc />
        public ChipsetInfoFlag Flags
        {
            get => _Flags;
        }

        /// <summary>
        ///     Chipset subsystem vendor identification
        /// </summary>
        public int SubSystemVendorId
        {
            get => (int)_SubSystemVendorId;
        }

        /// <summary>
        ///     Chipset subsystem device identification
        /// </summary>
        public int SubSystemDeviceId
        {
            get => (int)_SubSystemDeviceId;
        }

        /// <summary>
        ///     Chipset subsystem vendor name
        /// </summary>
        public string SubSystemVendorName
        {
            get => _SubSystemVendorName.Value;
        }
    }

    /// <summary>
    ///     Holds information about the system's chipset.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(4)]
    public struct ChipsetInfoV4 : IInitializable, IChipsetInfo, IEquatable<ChipsetInfoV4>
    {
        internal StructureVersion _Version;
        internal uint _VendorId;
        internal uint _DeviceId;
        internal ShortString _VendorName;
        internal ShortString _ChipsetName;
        internal ChipsetInfoFlag _Flags;
        internal uint _SubSystemVendorId;
        internal uint _SubSystemDeviceId;
        internal ShortString _SubSystemVendorName;
        internal uint _HostBridgeVendorId;
        internal uint _HostBridgeDeviceId;
        internal uint _HostBridgeSubSystemVendorId;
        internal uint _HostBridgeSubSystemDeviceId;

        /// <inheritdoc />
        public bool Equals(ChipsetInfoV4 other)
        {
            return _VendorId == other._VendorId &&
                   _DeviceId == other._DeviceId &&
                   _VendorName.Equals(other._VendorName) &&
                   _ChipsetName.Equals(other._ChipsetName) &&
                   _Flags == other._Flags &&
                   _SubSystemVendorId == other._SubSystemVendorId &&
                   _SubSystemDeviceId == other._SubSystemDeviceId &&
                   _SubSystemVendorName.Equals(other._SubSystemVendorName) &&
                   _HostBridgeVendorId == other._HostBridgeVendorId &&
                   _HostBridgeDeviceId == other._HostBridgeDeviceId &&
                   _HostBridgeSubSystemVendorId == other._HostBridgeSubSystemVendorId &&
                   _HostBridgeSubSystemDeviceId == other._HostBridgeSubSystemDeviceId;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is ChipsetInfoV4 v4 && Equals(v4);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_VendorId;
                hashCode = (hashCode * 397) ^ (int)_DeviceId;
                hashCode = (hashCode * 397) ^ _VendorName.GetHashCode();
                hashCode = (hashCode * 397) ^ _ChipsetName.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_Flags;
                hashCode = (hashCode * 397) ^ (int)_SubSystemVendorId;
                hashCode = (hashCode * 397) ^ (int)_SubSystemDeviceId;
                hashCode = (hashCode * 397) ^ _SubSystemVendorName.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_HostBridgeVendorId;
                hashCode = (hashCode * 397) ^ (int)_HostBridgeDeviceId;
                hashCode = (hashCode * 397) ^ (int)_HostBridgeSubSystemVendorId;
                hashCode = (hashCode * 397) ^ (int)_HostBridgeSubSystemDeviceId;

                return hashCode;
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{SubSystemVendorName} {VendorName} {ChipsetName}";
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public int VendorId
        {
            get => (int)_VendorId;
        }

        /// <inheritdoc />
        public int DeviceId
        {
            get => (int)_DeviceId;
        }

        /// <inheritdoc />
        public string VendorName
        {
            get => _VendorName.Value;
        }

        /// <inheritdoc />
        public string ChipsetName
        {
            get => _ChipsetName.Value;
        }

        /// <inheritdoc />
        public ChipsetInfoFlag Flags
        {
            get => _Flags;
        }

        /// <summary>
        ///     Chipset subsystem vendor identification
        /// </summary>
        public int SubSystemVendorId
        {
            get => (int)_SubSystemVendorId;
        }

        /// <summary>
        ///     Chipset subsystem device identification
        /// </summary>
        public int SubSystemDeviceId
        {
            get => (int)_SubSystemDeviceId;
        }

        /// <summary>
        ///     Chipset subsystem vendor name
        /// </summary>
        public string SubSystemVendorName
        {
            get => _SubSystemVendorName.Value;
        }

        /// <summary>
        ///     Host bridge vendor identification
        /// </summary>
        public int HostBridgeVendorId
        {
            get => (int)_HostBridgeVendorId;
        }

        /// <summary>
        ///     Host bridge device identification
        /// </summary>
        public int HostBridgeDeviceId
        {
            get => (int)_HostBridgeDeviceId;
        }

        /// <summary>
        ///     Host bridge subsystem vendor identification
        /// </summary>
        public int HostBridgeSubSystemVendorId
        {
            get => (int)_HostBridgeSubSystemVendorId;
        }

        /// <summary>
        ///     Host bridge subsystem device identification
        /// </summary>
        public int HostBridgeSubSystemDeviceId
        {
            get => (int)_HostBridgeSubSystemDeviceId;
        }
    }

    /// <summary>
    ///     Represents a rectangle coordinates
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct Rectangle
    {
        internal int _X;
        internal int _Y;
        internal int _Width;
        internal int _Height;

        /// <summary>
        ///     Creates a new instance of <see cref="Rectangle" />
        /// </summary>
        /// <param name="x">The horizontal location value.</param>
        /// <param name="y">The vertical location value.</param>
        /// <param name="width">The width of the rectangle.</param>
        /// <param name="height">The height of the rectangle.</param>
        // ReSharper disable once TooManyDependencies
        public Rectangle(int x, int y, int width, int height)
        {
            _X = x;
            _Y = y;
            _Width = width;
            _Height = height;
        }

        /// <summary>
        ///     Gets the horizontal location value
        /// </summary>
        public int X
        {
            get => _X;
        }

        /// <summary>
        ///     Gets the vertical location value
        /// </summary>
        public int Y
        {
            get => _Y;
        }

        /// <summary>
        ///     Gets the rectangle width value
        /// </summary>
        public int Width
        {
            get => _Width;
        }

        /// <summary>
        ///     Gets the rectangle height value
        /// </summary>
        public int Height
        {
            get => _Height;
        }

        /// <summary>
        ///     Gets the horizontal left edge value
        /// </summary>
        public int X2
        {
            get => X + Width;
        }

        /// <summary>
        ///     Gets the vertical bottom edge value
        /// </summary>
        public int Y2
        {
            get => Y + Height;
        }
    }

    /// <summary>
    ///     Holds information about the lid and dock
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct LidDockParameters : IInitializable, IEquatable<LidDockParameters>
    {
        internal StructureVersion _Version;
        internal uint _CurrentLIDState;
        internal uint _CurrentDockState;
        internal uint _CurrentLIDPolicy;
        internal uint _CurrentDockPolicy;
        internal uint _ForcedLIDMechanismPresent;
        internal uint _ForcedDockMechanismPresent;

        /// <inheritdoc />
        public bool Equals(LidDockParameters other)
        {
            return _CurrentLIDState == other._CurrentLIDState &&
                   _CurrentDockState == other._CurrentDockState &&
                   _CurrentLIDPolicy == other._CurrentLIDPolicy &&
                   _CurrentDockPolicy == other._CurrentDockPolicy &&
                   _ForcedLIDMechanismPresent == other._ForcedLIDMechanismPresent &&
                   _ForcedDockMechanismPresent == other._ForcedDockMechanismPresent;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is LidDockParameters parameters && Equals(parameters);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_CurrentLIDState;
                hashCode = (hashCode * 397) ^ (int)_CurrentDockState;
                hashCode = (hashCode * 397) ^ (int)_CurrentLIDPolicy;
                hashCode = (hashCode * 397) ^ (int)_CurrentDockPolicy;
                hashCode = (hashCode * 397) ^ (int)_ForcedLIDMechanismPresent;
                hashCode = (hashCode * 397) ^ (int)_ForcedDockMechanismPresent;

                return hashCode;
            }
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets current lid state
        /// </summary>
        public uint CurrentLidState
        {
            get => _CurrentLIDState;
        }

        /// <summary>
        ///     Gets current dock state
        /// </summary>
        public uint CurrentDockState
        {
            get => _CurrentDockState;
        }

        /// <summary>
        ///     Gets current lid policy
        /// </summary>
        public uint CurrentLidPolicy
        {
            get => _CurrentLIDPolicy;
        }

        /// <summary>
        ///     Gets current dock policy
        /// </summary>
        public uint CurrentDockPolicy
        {
            get => _CurrentDockPolicy;
        }

        /// <summary>
        ///     Gets forced lid mechanism present
        /// </summary>
        public uint ForcedLidMechanismPresent
        {
            get => _ForcedLIDMechanismPresent;
        }

        /// <summary>
        ///     Gets forced dock mechanism present
        /// </summary>
        public uint ForcedDockMechanismPresent
        {
            get => _ForcedDockMechanismPresent;
        }
    }
}
