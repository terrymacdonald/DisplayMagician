using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;

namespace DisplayMagicianShared.NVIDIA
{
    // ==================================
    // ENUMS
    // ==================================

    public enum NVAPI_STATUS : Int32
    {
        // Result Codes
        NVAPI_OK = 0,      //!< Success. Request is completed.
        NVAPI_ERROR = -1,      //!< Generic error
        NVAPI_LIBRARY_NOT_FOUND = -2,      //!< NVAPI support library cannot be loaded.
        NVAPI_NO_IMPLEMENTATION = -3,      //!< not implemented in current driver installation
        NVAPI_API_NOT_INITIALIZED = -4,      //!< NvAPI_Initialize has not been called (successfully)
        NVAPI_INVALID_ARGUMENT = -5,      //!< The argument/parameter value is not valid or NULL.
        NVAPI_NVIDIA_DEVICE_NOT_FOUND = -6,      //!< No NVIDIA display driver, or NVIDIA GPU driving a display, was found.
        NVAPI_END_ENUMERATION = -7,      //!< No more items to enumerate
        NVAPI_INVALID_HANDLE = -8,      //!< Invalid handle
        NVAPI_INCOMPATIBLE_STRUCT_VERSION = -9,      //!< An argument's structure version is not supported
        NVAPI_HANDLE_INVALIDATED = -10,     //!< The handle is no longer valid (likely due to GPU or display re-configuration)
        NVAPI_OPENGL_CONTEXT_NOT_CURRENT = -11,     //!< No NVIDIA OpenGL context is current (but needs to be)
        NVAPI_INVALID_POINTER = -14,     //!< An invalid poInt32er, usually NULL, was passed as a parameter
        NVAPI_NO_GL_EXPERT = -12,     //!< OpenGL Expert is not supported by the current drivers
        NVAPI_INSTRUMENTATION_DISABLED = -13,     //!< OpenGL Expert is supported, but driver instrumentation is currently disabled
        NVAPI_NO_GL_NSIGHT = -15,     //!< OpenGL does not support Nsight

        NVAPI_EXPECTED_LOGICAL_GPU_HANDLE = -100,    //!< Expected a logical GPU handle for one or more parameters
        NVAPI_EXPECTED_PHYSICAL_GPU_HANDLE = -101,    //!< Expected a physical GPU handle for one or more parameters
        NVAPI_EXPECTED_DISPLAY_HANDLE = -102,    //!< Expected an NV display handle for one or more parameters
        NVAPI_INVALID_COMBINATION = -103,    //!< The combination of parameters is not valid. 
        NVAPI_NOT_SUPPORTED = -104,    //!< Requested feature is not supported in the selected GPU
        NVAPI_PORTID_NOT_FOUND = -105,    //!< No port ID was found for the I2C transaction
        NVAPI_EXPECTED_UNATTACHED_DISPLAY_HANDLE = -106,    //!< Expected an unattached display handle as one of the input parameters.
        NVAPI_INVALID_PERF_LEVEL = -107,    //!< Invalid perf level 
        NVAPI_DEVICE_BUSY = -108,    //!< Device is busy; request not fulfilled
        NVAPI_NV_PERSIST_FILE_NOT_FOUND = -109,    //!< NV persist file is not found
        NVAPI_PERSIST_DATA_NOT_FOUND = -110,    //!< NV persist data is not found
        NVAPI_EXPECTED_TV_DISPLAY = -111,    //!< Expected a TV output display
        NVAPI_EXPECTED_TV_DISPLAY_ON_DCONNECTOR = -112,    //!< Expected a TV output on the D Connector - HDTV_EIAJ4120.
        NVAPI_NO_ACTIVE_SLI_TOPOLOGY = -113,    //!< SLI is not active on this device.
        NVAPI_SLI_RENDERING_MODE_NOTALLOWED = -114,    //!< Setup of SLI rendering mode is not possible right now.
        NVAPI_EXPECTED_DIGITAL_FLAT_PANEL = -115,    //!< Expected a digital flat panel.
        NVAPI_ARGUMENT_EXCEED_MAX_SIZE = -116,    //!< Argument exceeds the expected size.
        NVAPI_DEVICE_SWITCHING_NOT_ALLOWED = -117,    //!< Inhibit is ON due to one of the flags in NV_GPU_DISPLAY_CHANGE_INHIBIT or SLI active.
        NVAPI_TESTING_CLOCKS_NOT_SUPPORTED = -118,    //!< Testing of clocks is not supported.
        NVAPI_UNKNOWN_UNDERSCAN_CONFIG = -119,    //!< The specified underscan config is from an unknown source (e.g. INF)
        NVAPI_TIMEOUT_RECONFIGURING_GPU_TOPO = -120,    //!< Timeout while reconfiguring GPUs
        NVAPI_DATA_NOT_FOUND = -121,    //!< Requested data was not found
        NVAPI_EXPECTED_ANALOG_DISPLAY = -122,    //!< Expected an analog display
        NVAPI_NO_VIDLINK = -123,    //!< No SLI video bridge is present
        NVAPI_REQUIRES_REBOOT = -124,    //!< NVAPI requires a reboot for the settings to take effect
        NVAPI_INVALID_HYBRID_MODE = -125,    //!< The function is not supported with the current Hybrid mode.
        NVAPI_MIXED_TARGET_TYPES = -126,    //!< The target types are not all the same
        NVAPI_SYSWOW64_NOT_SUPPORTED = -127,    //!< The function is not supported from 32-bit on a 64-bit system.
        NVAPI_IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED = -128,    //!< There is no implicit GPU topology active. Use NVAPI_SetHybridMode to change topology.
        NVAPI_REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS = -129,      //!< Prompt the user to close all non-migratable applications.    
        NVAPI_OUT_OF_MEMORY = -130,    //!< Could not allocate sufficient memory to complete the call.
        NVAPI_WAS_STILL_DRAWING = -131,    //!< The previous operation that is transferring information to or from this surface is incomplete.
        NVAPI_FILE_NOT_FOUND = -132,    //!< The file was not found.
        NVAPI_TOO_MANY_UNIQUE_STATE_OBJECTS = -133,    //!< There are too many unique instances of a particular type of state object.
        NVAPI_INVALID_CALL = -134,    //!< The method call is invalid. For example, a method's parameter may not be a valid poInt32er.
        NVAPI_D3D10_1_LIBRARY_NOT_FOUND = -135,    //!< d3d10_1.dll cannot be loaded.
        NVAPI_FUNCTION_NOT_FOUND = -136,    //!< Couldn't find the function in the loaded DLL.
        NVAPI_INVALID_USER_PRIVILEGE = -137,    //!< The application will require Administrator privileges to access this API.
                                                //!< The application can be elevated to a higher permission level by selecting "Run as Administrator".
        NVAPI_EXPECTED_NON_PRIMARY_DISPLAY_HANDLE = -138,    //!< The handle corresponds to GDIPrimary.
        NVAPI_EXPECTED_COMPUTE_GPU_HANDLE = -139,    //!< Setting Physx GPU requires that the GPU is compute-capable.
        NVAPI_STEREO_NOT_INITIALIZED = -140,    //!< The Stereo part of NVAPI failed to initialize completely. Check if the stereo driver is installed.
        NVAPI_STEREO_REGISTRY_ACCESS_FAILED = -141,    //!< Access to stereo-related registry keys or values has failed.
        NVAPI_STEREO_REGISTRY_PROFILE_TYPE_NOT_SUPPORTED = -142, //!< The given registry profile type is not supported.
        NVAPI_STEREO_REGISTRY_VALUE_NOT_SUPPORTED = -143,    //!< The given registry value is not supported.
        NVAPI_STEREO_NOT_ENABLED = -144,    //!< Stereo is not enabled and the function needed it to execute completely.
        NVAPI_STEREO_NOT_TURNED_ON = -145,    //!< Stereo is not turned on and the function needed it to execute completely.
        NVAPI_STEREO_INVALID_DEVICE_INTERFACE = -146,    //!< Invalid device Int32erface.
        NVAPI_STEREO_PARAMETER_OUT_OF_RANGE = -147,    //!< Separation percentage or JPEG image capture quality is out of [0-100] range.
        NVAPI_STEREO_FRUSTUM_ADJUST_MODE_NOT_SUPPORTED = -148, //!< The given frustum adjust mode is not supported.
        NVAPI_TOPO_NOT_POSSIBLE = -149,    //!< The mosaic topology is not possible given the current state of the hardware.
        NVAPI_MODE_CHANGE_FAILED = -150,    //!< An attempt to do a display resolution mode change has failed.        
        NVAPI_D3D11_LIBRARY_NOT_FOUND = -151,    //!< d3d11.dll/d3d11_beta.dll cannot be loaded.
        NVAPI_INVALID_ADDRESS = -152,    //!< Address is outside of valid range.
        NVAPI_STRING_TOO_SMALL = -153,    //!< The pre-allocated string is too small to hold the result.
        NVAPI_MATCHING_DEVICE_NOT_FOUND = -154,    //!< The input does not match any of the available devices.
        NVAPI_DRIVER_RUNNING = -155,    //!< Driver is running.
        NVAPI_DRIVER_NOTRUNNING = -156,    //!< Driver is not running.
        NVAPI_ERROR_DRIVER_RELOAD_REQUIRED = -157,    //!< A driver reload is required to apply these settings.
        NVAPI_SET_NOT_ALLOWED = -158,    //!< Intended setting is not allowed.
        NVAPI_ADVANCED_DISPLAY_TOPOLOGY_REQUIRED = -159,    //!< Information can't be returned due to "advanced display topology".
        NVAPI_SETTING_NOT_FOUND = -160,    //!< Setting is not found.
        NVAPI_SETTING_SIZE_TOO_LARGE = -161,    //!< Setting size is too large.
        NVAPI_TOO_MANY_SETTINGS_IN_PROFILE = -162,    //!< There are too many settings for a profile. 
        NVAPI_PROFILE_NOT_FOUND = -163,    //!< Profile is not found.
        NVAPI_PROFILE_NAME_IN_USE = -164,    //!< Profile name is duplicated.
        NVAPI_PROFILE_NAME_EMPTY = -165,    //!< Profile name is empty.
        NVAPI_EXECUTABLE_NOT_FOUND = -166,    //!< Application not found in the Profile.
        NVAPI_EXECUTABLE_ALREADY_IN_USE = -167,    //!< Application already exists in the other profile.
        NVAPI_DATATYPE_MISMATCH = -168,    //!< Data Type mismatch 
        NVAPI_PROFILE_REMOVED = -169,    //!< The profile passed as parameter has been removed and is no longer valid.
        NVAPI_UNREGISTERED_RESOURCE = -170,    //!< An unregistered resource was passed as a parameter. 
        NVAPI_ID_OUT_OF_RANGE = -171,    //!< The DisplayId corresponds to a display which is not within the normal outputId range.
        NVAPI_DISPLAYCONFIG_VALIDATION_FAILED = -172,    //!< Display topology is not valid so the driver cannot do a mode set on this configuration.
        NVAPI_DPMST_CHANGED = -173,    //!< Display Port Multi-Stream topology has been changed.
        NVAPI_INSUFFICIENT_BUFFER = -174,    //!< Input buffer is insufficient to hold the contents.    
        NVAPI_ACCESS_DENIED = -175,    //!< No access to the caller.
        NVAPI_MOSAIC_NOT_ACTIVE = -176,    //!< The requested action cannot be performed without Mosaic being enabled.
        NVAPI_SHARE_RESOURCE_RELOCATED = -177,    //!< The surface is relocated away from video memory.
        NVAPI_REQUEST_USER_TO_DISABLE_DWM = -178,    //!< The user should disable DWM before calling NvAPI.
        NVAPI_D3D_DEVICE_LOST = -179,    //!< D3D device status is D3DERR_DEVICELOST or D3DERR_DEVICENOTRESET - the user has to reset the device.
        NVAPI_INVALID_CONFIGURATION = -180,    //!< The requested action cannot be performed in the current state.
        NVAPI_STEREO_HANDSHAKE_NOT_DONE = -181,    //!< Call failed as stereo handshake not completed.
        NVAPI_EXECUTABLE_PATH_IS_AMBIGUOUS = -182,    //!< The path provided was too short to determine the correct NVDRS_APPLICATION
        NVAPI_DEFAULT_STEREO_PROFILE_IS_NOT_DEFINED = -183,    //!< Default stereo profile is not currently defined
        NVAPI_DEFAULT_STEREO_PROFILE_DOES_NOT_EXIST = -184,    //!< Default stereo profile does not exist
        NVAPI_CLUSTER_ALREADY_EXISTS = -185,    //!< A cluster is already defined with the given configuration.
        NVAPI_DPMST_DISPLAY_ID_EXPECTED = -186,    //!< The input display id is not that of a multi stream enabled connector or a display device in a multi stream topology 
        NVAPI_INVALID_DISPLAY_ID = -187,    //!< The input display id is not valid or the monitor associated to it does not support the current operation
        NVAPI_STREAM_IS_OUT_OF_SYNC = -188,    //!< While playing secure audio stream, stream goes out of sync
        NVAPI_INCOMPATIBLE_AUDIO_DRIVER = -189,    //!< Older audio driver version than required
        NVAPI_VALUE_ALREADY_SET = -190,    //!< Value already set, setting again not allowed.
        NVAPI_TIMEOUT = -191,    //!< Requested operation timed out 
        NVAPI_GPU_WORKSTATION_FEATURE_INCOMPLETE = -192,    //!< The requested workstation feature set has incomplete driver Int32ernal allocation resources
        NVAPI_STEREO_INIT_ACTIVATION_NOT_DONE = -193,    //!< Call failed because InitActivation was not called.
        NVAPI_SYNC_NOT_ACTIVE = -194,    //!< The requested action cannot be performed without Sync being enabled.    
        NVAPI_SYNC_MASTER_NOT_FOUND = -195,    //!< The requested action cannot be performed without Sync Master being enabled.
        NVAPI_INVALID_SYNC_TOPOLOGY = -196,    //!< Invalid displays passed in the NV_GSYNC_DISPLAY poInt32er.
        NVAPI_ECID_SIGN_ALGO_UNSUPPORTED = -197,    //!< The specified signing algorithm is not supported. Either an incorrect value was entered or the current installed driver/hardware does not support the input value.
        NVAPI_ECID_KEY_VERIFICATION_FAILED = -198,    //!< The encrypted public key verification has failed.
        NVAPI_FIRMWARE_OUT_OF_DATE = -199,    //!< The device's firmware is out of date.
        NVAPI_FIRMWARE_REVISION_NOT_SUPPORTED = -200,    //!< The device's firmware is not supported.
        NVAPI_LICENSE_CALLER_AUTHENTICATION_FAILED = -201,    //!< The caller is not authorized to modify the License.
        NVAPI_D3D_DEVICE_NOT_REGISTERED = -202,    //!< The user tried to use a deferred context without registering the device first  
        NVAPI_RESOURCE_NOT_ACQUIRED = -203,    //!< Head or SourceId was not reserved for the VR Display before doing the Modeset.
        NVAPI_TIMING_NOT_SUPPORTED = -204,    //!< Provided timing is not supported.
        NVAPI_HDCP_ENCRYPTION_FAILED = -205,    //!< HDCP Encryption Failed for the device. Would be applicable when the device is HDCP Capable.
        NVAPI_PCLK_LIMITATION_FAILED = -206,    //!< Provided mode is over sink device pclk limitation.
        NVAPI_NO_CONNECTOR_FOUND = -207,    //!< No connector on GPU found. 
        NVAPI_HDCP_DISABLED = -208,    //!< When a non-HDCP capable HMD is connected, we would inform user by this code.
        NVAPI_API_IN_USE = -209,    //!< Atleast an API is still being called
        NVAPI_NVIDIA_DISPLAY_NOT_FOUND = -210,    //!< No display found on Nvidia GPU(s).
        NVAPI_PRIV_SEC_VIOLATION = -211,    //!< Priv security violation, improper access to a secured register.
        NVAPI_INCORRECT_VENDOR = -212,    //!< NVAPI cannot be called by this vendor
        NVAPI_DISPLAY_IN_USE = -213,    //!< DirectMode Display is already in use
        NVAPI_UNSUPPORTED_CONFIG_NON_HDCP_HMD = -214,    //!< The Config is having Non-NVidia GPU with Non-HDCP HMD connected
        NVAPI_MAX_DISPLAY_LIMIT_REACHED = -215,    //!< GPU's Max Display Limit has Reached
        NVAPI_INVALID_DIRECT_MODE_DISPLAY = -216,    //!< DirectMode not Enabled on the Display
        NVAPI_GPU_IN_DEBUG_MODE = -217,    //!< GPU is in debug mode, OC is NOT allowed.
        NVAPI_D3D_CONTEXT_NOT_FOUND = -218,    //!< No NvAPI context was found for this D3D object
        NVAPI_STEREO_VERSION_MISMATCH = -219,    //!< there is version mismatch between stereo driver and dx driver
        NVAPI_GPU_NOT_POWERED = -220,    //!< GPU is not powered and so the request cannot be completed.
        NVAPI_ERROR_DRIVER_RELOAD_IN_PROGRESS = -221,    //!< The display driver update in progress.
        NVAPI_WAIT_FOR_HW_RESOURCE = -222,    //!< Wait for HW resources allocation
        NVAPI_REQUIRE_FURTHER_HDCP_ACTION = -223,    //!< operation requires further HDCP action
        NVAPI_DISPLAY_MUX_TRANSITION_FAILED = -224,    //!< Dynamic Mux transition failure
        NVAPI_INVALID_DSC_VERSION = -225,    //!< Invalid DSC version
        NVAPI_INVALID_DSC_SLICECOUNT = -226,    //!< Invalid DSC slice count
        NVAPI_INVALID_DSC_OUTPUT_BPP = -227,    //!< Invalid DSC output BPP
        NVAPI_FAILED_TO_LOAD_FROM_DRIVER_STORE = -228,    //!< There was an error while loading nvapi.dll from the driver store.
        NVAPI_NO_VULKAN = -229,    //!< OpenGL does not export Vulkan fake extensions
        NVAPI_REQUEST_PENDING = -230,    //!< A request for NvTOPPs telemetry CData has already been made and is pending a response.
        NVAPI_RESOURCE_IN_USE = -231,    //!< Operation cannot be performed because the resource is in use.
    }

    [Flags]
    public enum NV_DISPLAYCONFIG_FLAGS : UInt32
    {
        VALIDATE_ONLY = 0x00000001,
        SAVE_TO_PERSISTENCE = 0x00000002,
        DRIVER_RELOAD_ALLOWED = 0x00000004,               //!< Driver reload is permitted if necessary
        FORCE_MODE_ENUMERATION = 0x00000008,               //!< Refresh OS mode list.
        FORCE_COMMIT_VIDPN = 0x00000010,               //!< Tell OS to avoid optimizing CommitVidPn call during a modeset
    }

    [Flags]
    public enum NV_GPU_CONNECTED_IDS_FLAG : UInt32
    {
        NONE = 0, //!< Get uncached connected devices
        UNCACHED = 0x1, //!< Get uncached connected devices
        SLI = 0x2, //!< Get devices such that those can be selected in an SLI configuration
        LIDSTATE = 0x4, //!< Get devices such that to reflect the Lid State
        FAKE = 0x8, //!< Get devices that includes the fake connected monitors
        EXCLUDE_MST = 0x10, //!< Excludes devices that are part of the multi stream topology.
    }

    public enum NVDRS_SETTING_TYPE : UInt32
    {
        NVDRS_DWORD_TYPE = 0,
        NVDRS_BINARY_TYPE = 1,
        NVDRS_STRING_TYPE = 2,
        NVDRS_WSTRING_TYPE = 3,
    }

    public enum NV_STATIC_METADATA_DESCRIPTOR_ID : UInt32
    {
        NV_STATIC_METADATA_TYPE_1 = 0                   //!< Tells the type of structure used to define the Static Metadata Descriptor block.
    }


    public enum NV_ROTATE : UInt32
    {
        ROTATE_0 = 0,
        ROTATE_90 = 1,
        ROTATE_180 = 2,
        ROTATE_270 = 3,
        ROTATE_IGNORED = 4,
    }

    public enum NV_FORMAT : UInt32
    {
        UNKNOWN = 0,       //!< unknown. Driver will choose one as following value.
        P8 = 41,       //!< for 8bpp mode
        R5G6B5 = 23,       //!< for 16bpp mode
        A8R8G8B8 = 21,       //!< for 32bpp mode
        A16B16G16R16F = 113,      //!< for 64bpp(floating poInt32) mode.

    }

    public enum NV_SCALING : UInt32
    {
        DEFAULT = 0,        //!< No change

        // New Scaling Declarations
        GPU_SCALING_TO_CLOSEST = 1,  //!< Balanced  - Full Screen
        GPU_SCALING_TO_NATIVE = 2,  //!< Force GPU - Full Screen
        GPU_SCANOUT_TO_NATIVE = 3,  //!< Force GPU - Centered\No Scaling
        GPU_SCALING_TO_ASPECT_SCANOUT_TO_NATIVE = 5,  //!< Force GPU - Aspect Ratio
        GPU_SCALING_TO_ASPECT_SCANOUT_TO_CLOSEST = 6,  //!< Balanced  - Aspect Ratio
        GPU_SCANOUT_TO_CLOSEST = 7,  //!< Balanced  - Centered\No Scaling
        GPU_INTEGER_ASPECT_SCALING = 8,  //!< Force GPU - Integer Scaling

        // Legacy Declarations
        MONITOR_SCALING = GPU_SCALING_TO_CLOSEST,
        ADAPTER_SCALING = GPU_SCALING_TO_NATIVE,
        CENTERED = GPU_SCANOUT_TO_NATIVE,
        ASPECT_SCALING = GPU_SCALING_TO_ASPECT_SCANOUT_TO_NATIVE,

        CUSTOMIZED = 255       //!< For future use
    }

    public enum NV_TARGET_VIEW_MODE : UInt32
    {
        STANDARD = 0,
        CLONE = 1,
        HSPAN = 2,
        VSPAN = 3,
        DUALVIEW = 4,
        MULTIVIEW = 5,
    }

    public enum NV_MONITOR_CONN_TYPE : Int32
    {
        UNINITIALIZED = 0,
        VGA = 1,
        COMPONENT = 2,
        SVIDEO = 3,
        HDMI = 4,
        DVI = 5,
        LVDS = 6,
        DP = 7,
        COMPOSITE = 8,
        UNKNOWN = -1
    }

    public enum NV_DISPLAY_TV_FORMAT : UInt32
    {
        NONE = 0,
        SD_NTSCM = 0x00000001,
        SD_NTSCJ = 0x00000002,
        SD_PALM = 0x00000004,
        SD_PALBDGH = 0x00000008,
        SD_PALN = 0x00000010,
        SD_PALNC = 0x00000020,
        SD_576i = 0x00000100,
        SD_480i = 0x00000200,
        ED_480p = 0x00000400,
        ED_576p = 0x00000800,
        HD_720p = 0x00001000,
        HD_1080i = 0x00002000,
        HD_1080p = 0x00004000,
        HD_720p50 = 0x00008000,
        HD_1080p24 = 0x00010000,
        HD_1080i50 = 0x00020000,
        HD_1080p50 = 0x00040000,
        UHD_4Kp30 = 0x00080000,
        UHD_4Kp30_3840 = UHD_4Kp30,
        UHD_4Kp25 = 0x00100000,
        UHD_4Kp25_3840 = UHD_4Kp25,
        UHD_4Kp24 = 0x00200000,
        UHD_4Kp24_3840 = UHD_4Kp24,
        UHD_4Kp24_SMPTE = 0x00400000,
        UHD_4Kp50_3840 = 0x00800000,
        UHD_4Kp60_3840 = 0x00900000,
        UHD_4Kp30_4096 = 0x00A00000,
        UHD_4Kp25_4096 = 0x00B00000,
        UHD_4Kp24_4096 = 0x00C00000,
        UHD_4Kp50_4096 = 0x00D00000,
        UHD_4Kp60_4096 = 0x00E00000,
        UHD_8Kp24_7680 = 0x01000000,
        UHD_8Kp25_7680 = 0x02000000,
        UHD_8Kp30_7680 = 0x04000000,
        UHD_8Kp48_7680 = 0x08000000,
        UHD_8Kp50_7680 = 0x09000000,
        UHD_8Kp60_7680 = 0x0A000000,
        UHD_8Kp100_7680 = 0x0B000000,
        UHD_8Kp120_7680 = 0x0C000000,
        UHD_4Kp48_3840 = 0x0D000000,
        UHD_4Kp48_4096 = 0x0E000000,
        UHD_4Kp100_4096 = 0x0F000000,
        UHD_4Kp100_3840 = 0x10000000,
        UHD_4Kp120_4096 = 0x11000000,
        UHD_4Kp120_3840 = 0x12000000,
        UHD_4Kp100_5120 = 0x13000000,
        UHD_4Kp120_5120 = 0x14000000,
        UHD_4Kp24_5120 = 0x15000000,
        UHD_4Kp25_5120 = 0x16000000,
        UHD_4Kp30_5120 = 0x17000000,
        UHD_4Kp48_5120 = 0x18000000,
        UHD_4Kp50_5120 = 0x19000000,
        UHD_4Kp60_5120 = 0x20000000,
        UHD_10Kp24_10240 = 0x21000000,
        UHD_10Kp25_10240 = 0x22000000,
        UHD_10Kp30_10240 = 0x23000000,
        UHD_10Kp48_10240 = 0x24000000,
        UHD_10Kp50_10240 = 0x25000000,
        UHD_10Kp60_10240 = 0x26000000,
        UHD_10Kp100_10240 = 0x27000000,
        UHD_10Kp120_10240 = 0x28000000,


        SD_OTHER = 0x30000000,
        ED_OTHER = 0x40000000,
        HD_OTHER = 0x50000000,

        ANY = 0x80000000,

    }

    public enum NV_GPU_CONNECTOR_TYPE : UInt32
    {
        VGA_15_PIN = 0x00000000,
        TV_COMPOSITE = 0x00000010,
        TV_SVIDEO = 0x00000011,
        TV_HDTV_COMPONENT = 0x00000013,
        TV_SCART = 0x00000014,
        TV_COMPOSITE_SCART_ON_EIAJ4120 = 0x00000016,
        TV_HDTV_EIAJ4120 = 0x00000017,
        PC_POD_HDTV_YPRPB = 0x00000018,
        PC_POD_SVIDEO = 0x00000019,
        PC_POD_COMPOSITE = 0x0000001A,
        DVI_I_TV_SVIDEO = 0x00000020,
        DVI_I_TV_COMPOSITE = 0x00000021,
        DVI_I = 0x00000030,
        DVI_D = 0x00000031,
        ADC = 0x00000032,
        LFH_DVI_I_1 = 0x00000038,
        LFH_DVI_I_2 = 0x00000039,
        SPWG = 0x00000040,
        OEM = 0x00000041,
        DISPLAYPORT_EXTERNAL = 0x00000046,
        DISPLAYPORT_INTERNAL = 0x00000047,
        DISPLAYPORT_MINI_EXT = 0x00000048,
        HDMI_A = 0x00000061,
        HDMI_C_MINI = 0x00000063,
        LFH_DISPLAYPORT_1 = 0x00000064,
        LFH_DISPLAYPORT_2 = 0x00000065,
        VIRTUAL_WFD = 0x00000070,
        USB_C = 0x00000071,
        UNKNOWN = 0xFFFFFFFF,
    }

    public enum NV_DISPLAYCONFIG_SPANNING_ORIENTATION : UInt32
    {
        NV_DISPLAYCONFIG_SPAN_NONE = 0,
        NV_DISPLAYCONFIG_SPAN_HORIZONTAL = 1,
        NV_DISPLAYCONFIG_SPAN_VERTICAL = 2,
    }

    public enum TIMING_SCAN_MODE : ushort
    {
        /// <summary>
        ///     Progressive scan mode
        /// </summary>
        Progressive = 0,

        /// <summary>
        ///     Interlaced scan mode
        /// </summary>
        Interlaced = 1,

        /// <summary>
        ///     Interlaced scan mode with extra vertical blank
        /// </summary>
        InterlacedWithExtraVerticalBlank = 1,

        /// <summary>
        ///     Interlaced scan mode without extra vertical blank
        /// </summary>
        InterlacedWithNoExtraVerticalBlank = 2
    }

    public enum TIMING_VERTICAL_SYNC_POLARITY : byte
    {
        /// <summary>
        ///     Positive vertical synchronized polarity
        /// </summary>
        Positive = 0,

        /// <summary>
        ///     Negative vertical synchronized polarity
        /// </summary>
        Negative = 1,

        /// <summary>
        ///     Default vertical synchronized polarity
        /// </summary>
        Default = Positive
    }

    public enum TIMING_HORIZONTAL_SYNC_POLARITY : byte
    {
        /// <summary>
        ///     Positive horizontal synchronized polarity
        /// </summary>
        Positive = 0,

        /// <summary>
        ///     Negative horizontal synchronized polarity
        /// </summary>
        Negative = 1,

        /// <summary>
        ///     Default horizontal synchronized polarity
        /// </summary>
        Default = Negative
    }

    public enum NV_TIMING_OVERRIDE : UInt32
    {
        CURRENT = 0,          //!< get the current timing
        AUTO,                 //!< the timing the driver will use based the current policy
        EDID,                 //!< EDID timing
        DMT,                  //!< VESA DMT timing
        DMT_RB,               //!< VESA DMT timing with reduced blanking
        CVT,                  //!< VESA CVT timing
        CVT_RB,               //!< VESA CVT timing with reduced blanking
        GTF,                  //!< VESA GTF timing
        EIA861,               //!< EIA 861x pre-defined timing
        ANALOG_TV,            //!< analog SD/HDTV timing
        CUST,                 //!< NV custom timings
        NV_PREDEFINED,        //!< NV pre-defined timing (basically the PsF timings)
        NV_PSF = NV_PREDEFINED,
        NV_ASPR,
        SDI,                  //!< Override for SDI timing

        NV_TIMING_OVRRIDE_MAX,
    }

    //
    //! These values refer to the different types of Mosaic topologies that are possible.  When
    //! getting the supported Mosaic topologies, you can specify one of these types to narrow down
    //! the returned list to only those that match the given type.
    public enum NV_MOSAIC_TOPO_TYPE : UInt32
    {
        ALL = 0,                          //!< All mosaic topologies
        BASIC = 1,                        //!< Basic Mosaic topologies
        PASSIVE_STEREO = 2,               //!< Passive Stereo topologies
        SCALED_CLONE = 3,                 //!< Not supported at this time
        PASSIVE_STEREO_SCALED_CLONE = 4,  //!< Not supported at this time
        MAX,                          //!< Always leave this at end of the enum
    }


    //
    //! This is a complete list of supported Mosaic topologies.
    //!
    //! Using a "Basic" topology combines multiple monitors to create a single desktop.
    //!
    //! Using a "Passive" topology combines multiples monitors to create a passive stereo desktop.
    //! In passive stereo, two identical topologies combine - one topology is used for the right eye and the other identical //! topology (targeting different displays) is used for the left eye.  \n  
    //! NOTE: common\inc\nvEscDef.h shadows a couple PASSIVE_STEREO enums.  If this
    //!       enum list changes and effects the value of NV_MOSAIC_TOPO_BEGIN_PASSIVE_STEREO
    //!       please update the corresponding value in nvEscDef.h
    public enum NV_MOSAIC_TOPO : UInt32
    {
        TOPO_NONE,

        // 'BASIC' topos start here
        //
        // The result of using one of these Mosaic topos is that multiple monitors
        // will combine to create a single desktop.
        //
        TOPO_BEGIN_BASIC,
        TOPO_1x2_BASIC = TOPO_BEGIN_BASIC,
        TOPO_2x1_BASIC,
        TOPO_1x3_BASIC,
        TOPO_3x1_BASIC,
        TOPO_1x4_BASIC,
        TOPO_4x1_BASIC,
        TOPO_2x2_BASIC,
        TOPO_2x3_BASIC,
        TOPO_2x4_BASIC,
        TOPO_3x2_BASIC,
        TOPO_4x2_BASIC,
        TOPO_1x5_BASIC,
        TOPO_1x6_BASIC,
        TOPO_7x1_BASIC,

        // Add padding for 10 more entries. 6 will be enough room to specify every
        // possible topology with 8 or fewer displays, so this gives us a little
        // extra should we need it.
        TOPO_END_BASIC = TOPO_7x1_BASIC + 9,

        // 'PASSIVE_STEREO' topos start here
        //
        // The result of using one of these Mosaic topos is that multiple monitors
        // will combine to create a single PASSIVE STEREO desktop.  What this means is
        // that there will be two topos that combine to create the overall desktop.
        // One topo will be used for the left eye, and the other topo (of the
        // same rows x cols), will be used for the right eye.  The difference between
        // the two topos is that different GPUs and displays will be used.
        //
        TOPO_BEGIN_PASSIVE_STEREO,    // value shadowed in nvEscDef.h
        TOPO_1x2_PASSIVE_STEREO = TOPO_BEGIN_PASSIVE_STEREO,
        TOPO_2x1_PASSIVE_STEREO,
        TOPO_1x3_PASSIVE_STEREO,
        TOPO_3x1_PASSIVE_STEREO,
        TOPO_1x4_PASSIVE_STEREO,
        TOPO_4x1_PASSIVE_STEREO,
        TOPO_2x2_PASSIVE_STEREO,
        TOPO_END_PASSIVE_STEREO = TOPO_2x2_PASSIVE_STEREO + 4,


        //
        // Total number of topos.  Always leave this at the end of the enumeration.
        //
        TOPO_MAX  //! Total number of topologies.

    }

    [Flags]
    public enum NV_MOSAIC_TOPO_VALIDITY : UInt32
    {
        VALID = 0x00000000,  //!< The topology is valid
        MISSING_GPU = 0x00000001,   //!< Not enough SLI GPUs were found to fill the entire
                                    //! topology. hPhysicalGPU will be 0 for these.
        MISSING_DISPLAY = 0x00000002,   //!< Not enough displays were found to fill the entire
                                        //! topology. displayOutputId will be 0 for these.
        MIXED_DISPLAY_TYPES = 0x00000004,   //!< The topoogy is only possible with displays of the same
                                            //! NV_GPU_OUTPUT_TYPE. Check displayOutputIds to make
                                            //! sure they are all CRTs, or all DFPs.
    }

    public enum NV_GPU_BUS_TYPE : UInt32
    {
        UNDEFINED = 0,
        PCI = 1,
        AGP = 2,
        PCI_EXPRESS = 3,
        FPCI = 4,
        AXI = 5,
    }


    public enum NV_PIXEL_SHIFT_TYPE
    {
        TYPE_NO_PIXEL_SHIFT = 0,          //!< No pixel shift will be applied to this display.
        TYPE_2x2_TOP_LEFT_PIXELS = 1,          //!< This display will be used to scanout top left pixels in 2x2 PixelShift configuration
        TYPE_2x2_BOTTOM_RIGHT_PIXELS = 2,          //!< This display will be used to scanout bottom right pixels in 2x2 PixelShift configuration
        TYPE_2x2_TOP_RIGHT_PIXELS = 4,          //!< This display will be used to scanout top right pixels in 2x2 PixelShift configuration
        TYPE_2x2_BOTTOM_LEFT_PIXELS = 8,          //!< This display will be used to scanout bottom left pixels in 2x2 PixelShift configuration
    }

    public enum NV_HDR_CMD : UInt32
    {
        CMD_GET = 0,                             //!< Get current HDR output configuration
        CMD_SET = 1                              //!< Set HDR output configuration
    }


    public enum NV_HDR_MODE : UInt32
    {
        // Official production-ready HDR modes
        OFF = 0,            //!< Turn off HDR
        UHDA = 2,            //!< Source: CCCS [a.k.a FP16 scRGB, linear, sRGB primaries, [-65504,0, 65504] range, RGB(1,1,1) = 80nits]  Output : UHDA HDR [a.k.a HDR10, RGB/YCC 10/12bpc ST2084(PQ) EOTF RGB(1,1,1) = 10000 nits, Rec2020 color primaries, ST2086 static HDR metadata]. This is the only supported production HDR mode.

        // Experimental
        UHDA_PASSTHROUGH = 5,            //!< Experimental mode only, not for production! Source: HDR10 RGB 10bpc Output: HDR10 RGB 10 bpc - signal UHDA HDR mode (PQ + Rec2020) to the sink but send source pixel values unmodified (no PQ or Rec2020 conversions) - assumes source is already in HDR10 format.
        DOLBY_VISION = 7,            //!< Experimental mode only, not for production! Source: RGB8 Dolby Vision encoded (12 bpc YCbCr422 packed into RGB8) Output: Dolby Vision encoded : Application is to encoded frames in DV format and embed DV dynamic metadata as described in Dolby Vision specification.

        // Unsupported/obsolete HDR modes
        //NV_HDR_MODE_EDR = 3,            //!< Do not use! Internal test mode only, to be removed. Source: CCCS (a.k.a FP16 scRGB) Output : EDR (Extended Dynamic Range) - HDR content is tonemapped and gamut mapped to output on regular SDR display set to max luminance ( ~300 nits ).
        //NV_HDR_MODE_SDR = 4,            //!< Do not use! Internal test mode only, to be removed. Source: any Output: SDR (Standard Dynamic Range), we continuously send SDR EOTF InfoFrame signaling, HDMI compliance testing.
        //NV_HDR_MODE_UHDA_NB = 6,            //!< Do not use! Internal test mode only, to be removed. Source: CCCS (a.k.a FP16 scRGB) Output : notebook HDR
        //NV_HDR_MODE_UHDBD = 2             //!< Do not use! Obsolete, to be removed. NV_HDR_MODE_UHDBD == NV_HDR_MODE_UHDA, reflects obsolete pre-UHDA naming convention.

    }

    public enum NV_COLOR_FORMAT : byte
    {
        RGB = 0,
        YUV422,
        YUV444,
        YUV420,

        DEFAULT = 0xFE,
        AUTO = 0xFF
    }

    public enum NV_DYNAMIC_RANGE : byte
    {
        VESA = 0x0,
        CEA = 0x1,

        AUTO = 0xFF
    }

    public enum NV_BPC : byte
    {
        BPC_DEFAULT = 0,
        BPC_6 = 1,
        BPC_8 = 2,
        BPC_10 = 3,
        BPC_12 = 4,
        BPC_16 = 5,
    }

    public enum NV_HDR_COLOR_FORMAT : UInt32
    {
        RGB = 0,
        YUV422,
        YUV444,
        YUV420,

        DEFAULT = 0xFE,
        AUTO = 0xFF
    }

    public enum NV_HDR_DYNAMIC_RANGE : UInt32
    {
        VESA = 0x0,
        CEA = 0x1,

        AUTO = 0xFF
    }


    public enum NV_COLOR_CMD : byte
    {
        NV_COLOR_CMD_GET = 1,
        NV_COLOR_CMD_SET,
        NV_COLOR_CMD_IS_SUPPORTED_COLOR,
        NV_COLOR_CMD_GET_DEFAULT
    }

    public enum NV_COLOR_COLORIMETRY : UInt32
    {
        NV_COLOR_COLORIMETRY_RGB = 0,
        NV_COLOR_COLORIMETRY_YCC601,
        NV_COLOR_COLORIMETRY_YCC709,
        NV_COLOR_COLORIMETRY_XVYCC601,
        NV_COLOR_COLORIMETRY_XVYCC709,
        NV_COLOR_COLORIMETRY_SYCC601,
        NV_COLOR_COLORIMETRY_ADOBEYCC601,
        NV_COLOR_COLORIMETRY_ADOBERGB,
        NV_COLOR_COLORIMETRY_BT2020RGB,
        NV_COLOR_COLORIMETRY_BT2020YCC,
        NV_COLOR_COLORIMETRY_BT2020cYCC,

        NV_COLOR_COLORIMETRY_DEFAULT = 0xFE,
        NV_COLOR_COLORIMETRY_AUTO = 0xFF
    }

    public enum NV_COLOR_SELECTION_POLICY : UInt32
    {
        NV_COLOR_SELECTION_POLICY_USER = 0,     //!< app/nvcpl make decision to select the desire color format
        NV_COLOR_SELECTION_POLICY_BEST_QUALITY = 1, //!< driver/ OS make decision to select the best color format
        NV_COLOR_SELECTION_POLICY_DEFAULT = NV_COLOR_SELECTION_POLICY_BEST_QUALITY,
        NV_COLOR_SELECTION_POLICY_UNKNOWN = 0xFF,
    }

    public enum NV_DESKTOP_COLOR_DEPTH
    {
        NV_DESKTOP_COLOR_DEPTH_DEFAULT = 0x0,                                    // set if the current setting should be kept
        NV_DESKTOP_COLOR_DEPTH_8BPC = 0x1,                                    //8 bit int per color component (8 bit int alpha)
        NV_DESKTOP_COLOR_DEPTH_10BPC = 0x2,                                    //10 bit int per color component (2 bit int alpha)
        NV_DESKTOP_COLOR_DEPTH_16BPC_FLOAT = 0x3,                                    //16 bit float per color component (16 bit float alpha)
        NV_DESKTOP_COLOR_DEPTH_16BPC_FLOAT_WCG = 0x4,                                    //16 bit float per color component (16 bit float alpha) wide color gamut
        NV_DESKTOP_COLOR_DEPTH_16BPC_FLOAT_HDR = 0x5,                                    //16 bit float per color component (16 bit float alpha) HDR
        NV_DESKTOP_COLOR_DEPTH_MAX_VALUE = NV_DESKTOP_COLOR_DEPTH_16BPC_FLOAT_HDR, // must be set to highest enum value
    }


    public enum NVDRS_SETTING_LOCATION : UInt32
    {
        NVDRS_CURRENT_PROFILE_LOCATION = 0x0,
        NVDRS_GLOBAL_PROFILE_LOCATION = 0x1,
        NVDRS_BASE_PROFILE_LOCATION = 0x2,
        NVDRS_DEFAULT_PROFILE_LOCATION = 0x3,
    }


    [Flags]
    public enum NV_HDR_CAPABILITIES_V2_FLAGS : UInt32
    {
        IsST2084EotfSupported = 0x1,                 //!< HDMI2.0a UHDA HDR with ST2084 EOTF (CEA861.3). Boolean: 0 = not supported, 1 = supported;
        IsTraditionalHdrGammaSupported = 0x2,                 //!< HDMI2.0a traditional HDR gamma (CEA861.3). Boolean: 0 = not supported, 1 = supported;
        IsEdrSupported = 0x4,                 //!< Extended Dynamic Range on SDR displays. Boolean: 0 = not supported, 1 = supported;
        DriverExpandDefaultHdrParameters = 0x8,                 //!< If set, driver will expand default (=zero) HDR capabilities parameters contained in display's EDID.
                                                                //!< Boolean: 0 = report actual HDR parameters, 1 = expand default HDR parameters;
        IsTraditionalSdrGammaSupported = 0x10,                 //!< HDMI2.0a traditional SDR gamma (CEA861.3). Boolean: 0 = not supported, 1 = supported;
        IsDolbyVisionSupported = 0x20,                 //!< Dolby Vision Support. Boolean: 0 = not supported, 1 = supported;
    }

    [Flags]
    public enum NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS : UInt32
    {
        OK = 0x0,
        DISPLAY_ON_INVALID_GPU = 0x1,
        DISPLAY_ON_WRONG_CONNECTOR = 0x2,
        NO_COMMON_TIMINGS = 0x4,
        NO_EDID_AVAILABLE = 0x8,
        MISMATCHED_OUTPUT_TYPE = 0x10,
        NO_DISPLAY_CONNECTED = 0x20,
        NO_GPU_TOPOLOGY = 0x40,
        NOT_SUPPORTED = 0x80,
        NO_SLI_BRIDGE = 0x100,
        ECC_ENABLED = 0x200,
        GPU_TOPOLOGY_NOT_SUPPORTED = 0x400,
    }

    [Flags]
    public enum NV_MOSAIC_DISPLAYTOPO_WARNING_FLAGS : UInt32
    {
        NONE = 0x0,

        //! Indicates that a display's position in the grid is sub-optimal.
        DISPLAY_POSITION = 0x1,

        //! \ingroup mosaicapi
        //! Indicates that SetDisplaySettings would need to perform a driver reload.
        DRIVER_RELOAD_REQUIRED = 0x2,
    }

    [Flags]
    public enum NV_MOSAIC_SETDISPLAYTOPO_FLAGS : UInt32
    {
        // Empty flag which does nothing
        NONE = 0,

        //! Do not change the current GPU topology. If the NO_DRIVER_RELOAD bit is not
        //! specified, then it may still require a driver reload.
        CURRENT_GPU_TOPOLOGY = 0x1,

        //! Do not allow a driver reload. That is, stick with the same master GPU as well as the
        //! same SLI configuration.
        NO_DRIVER_RELOAD = 0x2,

        //! When choosing a GPU topology, choose the topology with the best performance.
        //! Without this flag, it will choose the topology that uses the smallest number
        //! of GPU's.
        MAXIMIZE_PERFORMANCE = 0x4,

        //! Do not return an error if no configuration will work with all of the grids.
        ALLOW_INVALID = 0x8,
    }

    [Flags]
    public enum NVDRS_GPU_SUPPORT : UInt32
    {
        NONE = 0x0,
        GEFORCE = 0x1,
        QUADRO = 0x2,
        NVS = 0x4,
    }

    // ==================================
    // STRUCTS
    // ==================================


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NvDRSSessionHandle : IEquatable<NvDRSSessionHandle>, ICloneable
    {
        public IntPtr Ptr;

        public override bool Equals(object obj) => obj is NvDRSSessionHandle other && this.Equals(other);

        public bool Equals(NvDRSSessionHandle other)
        => Ptr == other.Ptr;

        public override Int32 GetHashCode()
        {
            return (Ptr).GetHashCode();
        }

        public static bool operator ==(NvDRSSessionHandle lhs, NvDRSSessionHandle rhs) => lhs.Equals(rhs);

        public static bool operator !=(NvDRSSessionHandle lhs, NvDRSSessionHandle rhs) => !(lhs == rhs);

        public object Clone()
        {
            NvDRSSessionHandle other = (NvDRSSessionHandle)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NvDRSProfileHandle : IEquatable<NvDRSProfileHandle>, ICloneable
    {
        public IntPtr Ptr;

        public override bool Equals(object obj) => obj is NvDRSProfileHandle other && this.Equals(other);

        public bool Equals(NvDRSProfileHandle other)
        => Ptr == other.Ptr;

        public override Int32 GetHashCode()
        {
            return (Ptr).GetHashCode();
        }

        public static bool operator ==(NvDRSProfileHandle lhs, NvDRSProfileHandle rhs) => lhs.Equals(rhs);

        public static bool operator !=(NvDRSProfileHandle lhs, NvDRSProfileHandle rhs) => !(lhs == rhs);

        public object Clone()
        {
            NvDRSProfileHandle other = (NvDRSProfileHandle)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DisplayHandle : IEquatable<DisplayHandle>, ICloneable
    {
        public IntPtr Ptr;

        public override bool Equals(object obj) => obj is DisplayHandle other && this.Equals(other);

        public bool Equals(DisplayHandle other)
        => Ptr == other.Ptr;

        public override Int32 GetHashCode()
        {
            return (Ptr).GetHashCode();
        }

        public static bool operator ==(DisplayHandle lhs, DisplayHandle rhs) => lhs.Equals(rhs);

        public static bool operator !=(DisplayHandle lhs, DisplayHandle rhs) => !(lhs == rhs);

        public object Clone()
        {
            DisplayHandle other = (DisplayHandle)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct UnAttachedDisplayHandle : IEquatable<UnAttachedDisplayHandle>, ICloneable
    {
        public IntPtr Ptr;

        public override bool Equals(object obj) => obj is UnAttachedDisplayHandle other && this.Equals(other);

        public bool Equals(UnAttachedDisplayHandle other)
        => Ptr == other.Ptr;

        public override Int32 GetHashCode()
        {
            return (Ptr).GetHashCode();
        }

        public static bool operator ==(UnAttachedDisplayHandle lhs, UnAttachedDisplayHandle rhs) => lhs.Equals(rhs);

        public static bool operator !=(UnAttachedDisplayHandle lhs, UnAttachedDisplayHandle rhs) => !(lhs == rhs);

        public object Clone()
        {
            UnAttachedDisplayHandle other = (UnAttachedDisplayHandle)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PhysicalGpuHandle : IEquatable<PhysicalGpuHandle>, ICloneable
    {
        public IntPtr Ptr;

        public override bool Equals(object obj) => obj is PhysicalGpuHandle other && this.Equals(other);

        public bool Equals(PhysicalGpuHandle other)
        => Ptr == other.Ptr;

        public override Int32 GetHashCode()
        {
            return (Ptr).GetHashCode();
        }
        public static bool operator ==(PhysicalGpuHandle lhs, PhysicalGpuHandle rhs) => lhs.Equals(rhs);

        public static bool operator !=(PhysicalGpuHandle lhs, PhysicalGpuHandle rhs) => !(lhs == rhs);

        public object Clone()
        {
            PhysicalGpuHandle other = (PhysicalGpuHandle)MemberwiseClone();
            return other;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct LogicalGpuHandle : IEquatable<LogicalGpuHandle>, ICloneable
    {
        public IntPtr Ptr;

        public override bool Equals(object obj) => obj is LogicalGpuHandle other && this.Equals(other);

        public bool Equals(LogicalGpuHandle other)
        => Ptr == other.Ptr;

        public override Int32 GetHashCode()
        {
            return (Ptr).GetHashCode();
        }
        public static bool operator ==(LogicalGpuHandle lhs, LogicalGpuHandle rhs) => lhs.Equals(rhs);

        public static bool operator !=(LogicalGpuHandle lhs, LogicalGpuHandle rhs) => !(lhs == rhs);

        public object Clone()
        {
            LogicalGpuHandle other = (LogicalGpuHandle)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct NVDRS_PROFILE_V1 : IEquatable<NVDRS_PROFILE_V1>, ICloneable // Note: Version 3 of NV_EDID_V3 structure
    {
        public UInt32 Version;        //!< Structure version
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (Int32)NVImport.NVAPI_UNICODE_STRING_MAX)]
        public string ProfileName;    // EDID_Data[NV_EDID_DATA_SIZE];
        public NVDRS_GPU_SUPPORT GpuSupport;
        public UInt32 IsPredefined;
        public UInt32 NumofApps;
        public UInt32 NumofSettings;

        public override bool Equals(object obj) => obj is NVDRS_PROFILE_V1 other && this.Equals(other);

        public bool Equals(NVDRS_PROFILE_V1 other)
        => Version == other.Version &&
           ProfileName == other.ProfileName &&
           GpuSupport == other.GpuSupport &&
           IsPredefined == other.IsPredefined &&
           NumofApps == other.NumofApps &&
           NumofSettings == other.NumofSettings;

        public override Int32 GetHashCode()
        {
            return (Version, ProfileName, GpuSupport, IsPredefined, NumofApps, NumofSettings).GetHashCode();
        }
        public static bool operator ==(NVDRS_PROFILE_V1 lhs, NVDRS_PROFILE_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVDRS_PROFILE_V1 lhs, NVDRS_PROFILE_V1 rhs) => !(lhs == rhs);

        public object Clone()
        {
            NVDRS_PROFILE_V1 other = (NVDRS_PROFILE_V1)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct NVDRS_SETTING_V1 : IEquatable<NVDRS_SETTING_V1>, ICloneable // Note: Version 1 of NVDRS_SETTING_V1 structure
    {
        public UInt32 InternalVersion;
        public UnicodeString InternalSettingName;
        public UInt32 InternalSettingId;
        public NVDRS_SETTING_TYPE InternalSettingType;
        public NVDRS_SETTING_LOCATION InternalSettingLocation;
        public UInt32 InternalIsCurrentPredefined;
        public UInt32 InternalIsPredefinedValid;
        public NVDRS_SETTING_VALUE InternalPredefinedValue;
        public NVDRS_SETTING_VALUE InternalCurrentValue;

        /// <summary>
        ///     Creates a new instance of <see cref="DRSSettingV1" /> containing the passed value.
        /// </summary>
        /// <param name="id">The setting identification number.</param>
        /// <param name="settingType">The type of the setting's value</param>
        /// <param name="value">The setting's value</param>
        public NVDRS_SETTING_V1(uint id, NVDRS_SETTING_TYPE settingType, object value)
        {
            InternalVersion = NVImport.NVDRS_SETTING_V1_VER;
            InternalSettingId = id;
            InternalIsPredefinedValid = (UInt32)0;
            InternalSettingName = new UnicodeString("");
            InternalSettingId = 0;
            InternalSettingType = settingType;
            InternalSettingLocation = NVDRS_SETTING_LOCATION.NVDRS_BASE_PROFILE_LOCATION;
            InternalIsCurrentPredefined = 0;
            InternalIsPredefinedValid = 0;
            InternalPredefinedValue = new NVDRS_SETTING_VALUE();
            InternalCurrentValue = new NVDRS_SETTING_VALUE(0);

            CurrentValue = value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NVDRS_SETTING_V1" /> containing the passed value.
        /// </summary>
        /// <param name="id">The setting identification number.</param>
        /// <param name="value">The setting's value</param>
        public NVDRS_SETTING_V1(uint id, string value) : this(id, NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE, value)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NVDRS_SETTING_V1" /> containing the passed value.
        /// </summary>
        /// <param name="id">The setting identification number.</param>
        /// <param name="value">The setting's value</param>
        public NVDRS_SETTING_V1(uint id, uint value) : this(id, NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE, value)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NVDRS_SETTING_V1" /> containing the passed value.
        /// </summary>
        /// <param name="id">The setting identification number.</param>
        /// <param name="value">The setting's value</param>
        public NVDRS_SETTING_V1(uint id, byte[] value) : this(id, NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE, value)
        {
        }

        /// <summary>
        ///     Gets the name of the setting
        /// </summary>
        public string Name
        {
            get => InternalSettingName.Value;
        }

        /// <summary>
        ///     Gets the identification number of the setting
        /// </summary>
        public UInt32 SettingId
        {
            get => InternalSettingId;
            private set => InternalSettingId = value;
        }

        /// <summary>
        ///     Gets the setting's value type
        /// </summary>
        public NVDRS_SETTING_TYPE SettingType
        {
            get => InternalSettingType;
            private set => InternalSettingType = value;
        }

        /// <summary>
        ///     Gets the setting location
        /// </summary>
        public NVDRS_SETTING_LOCATION SettingLocation
        {
            get => InternalSettingLocation;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the current value is the predefined value
        /// </summary>
        public bool IsCurrentPredefined
        {
            get => InternalIsCurrentPredefined > 0;
            internal set => InternalIsCurrentPredefined = value ? 1u : 0u;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the predefined value is available and valid
        /// </summary>
        public bool IsPredefinedValid
        {
            get => InternalIsPredefinedValid > 0;
            internal set => InternalIsPredefinedValid = value ? 1u : 0u;
        }

        /// <summary>
        ///     Returns the predefined value as an integer
        /// </summary>
        /// <returns>An integer representing the predefined value</returns>
        public uint GetPredefinedValueAsInteger()
        {
            return InternalPredefinedValue.AsInteger();
        }

        /// <summary>
        ///     Returns the predefined value as an array of bytes
        /// </summary>
        /// <returns>An byte array representing the predefined value</returns>
        public byte[] GetPredefinedValueAsBinary()
        {
            return InternalPredefinedValue.AsBinary();
        }

        /// <summary>
        ///     Returns the predefined value as an unicode string
        /// </summary>
        /// <returns>An unicode string representing the predefined value</returns>
        public string GetPredefinedValueAsUnicodeString()
        {
            return InternalPredefinedValue.AsUnicodeString();
        }

        /// <summary>
        ///     Gets the setting's predefined value
        /// </summary>
        public object PredefinedValue
        {
            get
            {
                if (!IsPredefinedValid)
                {
                    return 0;
                }

                switch (InternalSettingType)
                {
                    case NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE:

                        return GetPredefinedValueAsInteger();
                    case NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE:

                        return GetPredefinedValueAsBinary();
                    case NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE:
                    case NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE:

                        return GetPredefinedValueAsUnicodeString();
                    default:

                        throw new ArgumentOutOfRangeException(nameof(SettingType));
                }
            }
        }

        /// <summary>
        ///     Returns the current value as an integer
        /// </summary>
        /// <returns>An integer representing the current value</returns>
        public uint GetCurrentValueAsInteger()
        {
            return InternalCurrentValue.AsInteger();
        }

        /// <summary>
        ///     Returns the current value as an array of bytes
        /// </summary>
        /// <returns>An byte array representing the current value</returns>
        public byte[] GetCurrentValueAsBinary()
        {
            return InternalCurrentValue.AsBinary();
        }

        /// <summary>
        ///     Returns the current value as an unicode string
        /// </summary>
        /// <returns>An unicode string representing the current value</returns>
        public string GetCurrentValueAsUnicodeString()
        {
            return InternalCurrentValue.AsUnicodeString();
        }

        /// <summary>
        ///     Sets the passed value as the current value
        /// </summary>
        /// <param name="value">The new value for the setting</param>
        public void SetCurrentValueAsInteger(uint value)
        {
            if (SettingType != NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Passed argument is invalid for this setting.");
            }

            InternalCurrentValue = new NVDRS_SETTING_VALUE(value);
            IsCurrentPredefined = IsPredefinedValid && (uint)CurrentValue == (uint)PredefinedValue;
        }

        /// <summary>
        ///     Sets the passed value as the current value
        /// </summary>
        /// <param name="value">The new value for the setting</param>
        public void SetCurrentValueAsBinary(byte[] value)
        {
            if (SettingType != NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Passed argument is invalid for this setting.");
            }

            InternalCurrentValue = new NVDRS_SETTING_VALUE(value);
            IsCurrentPredefined =
                IsPredefinedValid &&
                ((byte[])CurrentValue)?.SequenceEqual((byte[])PredefinedValue ?? new byte[0]) == true;
        }

        /// <summary>
        ///     Sets the passed value as the current value
        /// </summary>
        /// <param name="value">The new value for the setting</param>
        public void SetCurrentValueAsUnicodeString(string value)
        {
            if (SettingType != NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Passed argument is invalid for this setting.");
            }

            InternalCurrentValue = new NVDRS_SETTING_VALUE(value);
            IsCurrentPredefined =
                IsPredefinedValid &&
                string.Equals(
                    (string)CurrentValue,
                    (string)PredefinedValue,
                    StringComparison.InvariantCulture
                );
        }

        /// <summary>
        ///     Gets or sets the setting's current value
        /// </summary>
        public object CurrentValue
        {
            get
            {
                switch (InternalSettingType)
                {
                    case NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE:

                        return GetCurrentValueAsInteger();
                    case NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE:

                        return GetCurrentValueAsBinary();
                    case NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE:
                    case NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE:

                        return GetCurrentValueAsUnicodeString();
                    default:

                        throw new ArgumentOutOfRangeException(nameof(SettingType));
                }
            }
            internal set
            {
                if (value is int intValue)
                {
                    SetCurrentValueAsInteger((uint)intValue);
                }
                else if (value is uint unsignedIntValue)
                {
                    SetCurrentValueAsInteger(unsignedIntValue);
                }
                else if (value is short shortValue)
                {
                    SetCurrentValueAsInteger((uint)shortValue);
                }
                else if (value is ushort unsignedShortValue)
                {
                    SetCurrentValueAsInteger(unsignedShortValue);
                }
                else if (value is long longValue)
                {
                    SetCurrentValueAsInteger((uint)longValue);
                }
                else if (value is ulong unsignedLongValue)
                {
                    SetCurrentValueAsInteger((uint)unsignedLongValue);
                }
                else if (value is byte byteValue)
                {
                    SetCurrentValueAsInteger(byteValue);
                }
                else if (value is string stringValue)
                {
                    SetCurrentValueAsUnicodeString(stringValue);
                }
                else if (value is byte[] binaryValue)
                {
                    SetCurrentValueAsBinary(binaryValue);
                }
                else
                {
                    throw new ArgumentException("Unacceptable argument type.", nameof(value));
                }
            }
        }

        public override bool Equals(object obj) => obj is NVDRS_SETTING_V1 other && this.Equals(other);

        public bool Equals(NVDRS_SETTING_V1 other)
        {
            if (!(InternalVersion == other.InternalVersion &&
            InternalSettingName.Equals(other.InternalSettingName) &&
            InternalSettingId == other.InternalSettingId &&
            InternalSettingType == other.InternalSettingType &&
            InternalSettingLocation == other.InternalSettingLocation &&
            InternalIsCurrentPredefined == other.InternalIsCurrentPredefined &&
            InternalIsPredefinedValid == other.InternalIsPredefinedValid))
            {
                return false;
            }
            if (InternalSettingType == NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE &&
                InternalCurrentValue.AsInteger() == other.InternalCurrentValue.AsInteger())
            {
                return true;
            }
            else if (InternalSettingType == NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE &&
                InternalCurrentValue.AsBinary() == other.InternalCurrentValue.AsBinary())
            {
                return true;
            }
            else if ((InternalSettingType == NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE || InternalSettingType == NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE) &&
                InternalCurrentValue.AsUnicodeString() == other.InternalCurrentValue.AsUnicodeString())
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public override Int32 GetHashCode()
        {
            return (InternalVersion, InternalSettingName, InternalSettingId, InternalSettingType, InternalSettingLocation, InternalIsCurrentPredefined, InternalIsPredefinedValid, InternalCurrentValue).GetHashCode();
        }
        public static bool operator ==(NVDRS_SETTING_V1 lhs, NVDRS_SETTING_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVDRS_SETTING_V1 lhs, NVDRS_SETTING_V1 rhs) => !(lhs == rhs);

        public object Clone()
        {
            NVDRS_SETTING_V1 other = (NVDRS_SETTING_V1)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Unicode)]
    public struct NVDRS_BINARY_SETTING : IEquatable<NVDRS_BINARY_SETTING>, ICloneable
    {
        public UInt32 ValueLength;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (Int32)NVImport.NVAPI_UNICODE_STRING_MAX)]
        public string ValueData;

        public override bool Equals(object obj) => obj is NVDRS_BINARY_SETTING other && this.Equals(other);

        public bool Equals(NVDRS_BINARY_SETTING other)
        => ValueLength == other.ValueLength &&
           ValueData.SequenceEqual(other.ValueData);

        public override Int32 GetHashCode()
        {
            return (ValueLength, ValueData).GetHashCode();
        }
        public static bool operator ==(NVDRS_BINARY_SETTING lhs, NVDRS_BINARY_SETTING rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVDRS_BINARY_SETTING lhs, NVDRS_BINARY_SETTING rhs) => !(lhs == rhs);

        public object Clone()
        {
            NVDRS_BINARY_SETTING other = (NVDRS_BINARY_SETTING)MemberwiseClone();
            return other;
        }
    }


    //NVDRS_SETTING_VALUE_UNION
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NVDRS_SETTING_VALUE : IEquatable<NVDRS_SETTING_VALUE>, ICloneable // Note: Version 1 of NVDRS_SETTINGS_VALUE structure
    {
        private const int UnicodeStringLength = UnicodeString.UnicodeStringLength;
        private const int BinaryDataMax = 4096;

        // Math.Max(BinaryDataMax + sizeof(uint), UnicodeStringLength * sizeof(ushort))
        private const int FullStructureSize = 4100;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FullStructureSize, ArraySubType = UnmanagedType.U1)]
        public byte[] InternalBinaryValue;

        /// <summary>
        ///     Creates a new instance of <see cref="NVDRS_SETTING_VALUE" /> containing the passed unicode string as the value
        /// </summary>
        /// <param name="value">The unicode string value</param>
        public NVDRS_SETTING_VALUE(string value)
        {
            if (value?.Length > UnicodeStringLength)
            {
                value = value.Substring(0, UnicodeStringLength);
            }

            InternalBinaryValue = new byte[FullStructureSize];

            var stringBytes = Encoding.Unicode.GetBytes(value ?? string.Empty);
            Array.Copy(stringBytes, 0, InternalBinaryValue, 0, Math.Min(stringBytes.Length, InternalBinaryValue.Length));
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NVDRS_SETTING_VALUE" /> containing the passed byte array as the value
        /// </summary>
        /// <param name="value">The byte array value</param>
        public NVDRS_SETTING_VALUE(byte[] value)
        {
            InternalBinaryValue = new byte[FullStructureSize];

            if (value?.Length > 0)
            {
                var arrayLength = Math.Min(value.Length, BinaryDataMax);
                var arrayLengthBytes = BitConverter.GetBytes((uint)arrayLength);
                Array.Copy(arrayLengthBytes, 0, InternalBinaryValue, 0, arrayLengthBytes.Length);
                Array.Copy(value, 0, InternalBinaryValue, arrayLengthBytes.Length, arrayLength);
            }
        }

        /// <summary>
        ///     Creates a new instance of <see cref="NVDRS_SETTING_VALUE" /> containing the passed integer as the value
        /// </summary>
        /// <param name="value">The integer value</param>
        public NVDRS_SETTING_VALUE(uint value)
        {
            InternalBinaryValue = new byte[FullStructureSize];
            var arrayLengthBytes = BitConverter.GetBytes(value);
            Array.Copy(arrayLengthBytes, 0, InternalBinaryValue, 0, arrayLengthBytes.Length);
        }

        /// <summary>
        ///     Returns the value as an integer
        /// </summary>
        /// <returns>An integer representing the value</returns>
        public uint AsInteger()
        {
            return BitConverter.ToUInt32(InternalBinaryValue, 0);
        }

        /// <summary>
        ///     Returns the value as an array of bytes
        /// </summary>
        /// <returns>An array of bytes representing the value</returns>
        public byte[] AsBinary()
        {
            return InternalBinaryValue.Skip(sizeof(uint)).Take((int)AsInteger()).ToArray();
        }

        /// <summary>
        ///     Returns the value as an unicode string
        /// </summary>
        /// <returns>An unicode string representing the value</returns>
        public string AsUnicodeString()
        {
            return Encoding.Unicode.GetString(InternalBinaryValue).TrimEnd('\0');
        }

        public override bool Equals(object obj) => obj is NVDRS_SETTING_VALUE other && this.Equals(other);

        public bool Equals(NVDRS_SETTING_VALUE other)
        => InternalBinaryValue == other.InternalBinaryValue;

        public override Int32 GetHashCode()
        {
            return (InternalBinaryValue).GetHashCode();
        }
        public static bool operator ==(NVDRS_SETTING_VALUE lhs, NVDRS_SETTING_VALUE rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVDRS_SETTING_VALUE lhs, NVDRS_SETTING_VALUE rhs) => !(lhs == rhs);

        public object Clone()
        {
            NVDRS_SETTING_VALUE other = (NVDRS_SETTING_VALUE)MemberwiseClone();
            return other;
        }
    }

    //NVDRS_SETTING_VALUES
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NVDRS_SETTING_VALUES_V1 : IEquatable<NVDRS_SETTING_VALUES_V1>, ICloneable // Note: Version 1 of NVDRS_SETTING_VALUES_V1 structure
    {
        internal const int MaximumNumberOfValues = (int)NVImport.NVAPI_SETTING_MAX_VALUES;

        public UInt32 Version;
        public UInt32 NumberOfValues;
        public NVDRS_SETTING_TYPE _SettingType;
        public NVDRS_SETTING_VALUE InternalDefaultValue;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfValues)]
        public NVDRS_SETTING_VALUE[] InternalValues;

        /// <summary>
        ///     Gets the setting's value type
        /// </summary>
        public NVDRS_SETTING_TYPE SettingType
        {
            get => _SettingType;
        }

        /// <summary>
        ///     Gets a list of possible values for the setting
        /// </summary>
        public object[] Values
        {
            get
            {
                switch (_SettingType)
                {
                    case NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE:
                        return ValuesAsInteger().Cast<object>().ToArray();

                    case NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE:
                        return ValuesAsBinary().Cast<object>().ToArray();

                    case NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE:
                    case NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE:
                        return ValuesAsUnicodeString().Cast<object>().ToArray();

                    default:
                        throw new ArgumentOutOfRangeException(nameof(SettingType));
                }
            }
        }

        /// <summary>
        ///     Gets the default value of the setting
        /// </summary>
        public object DefaultValue
        {
            get
            {
                switch (_SettingType)
                {
                    case NVDRS_SETTING_TYPE.NVDRS_DWORD_TYPE:
                        return DefaultValueAsInteger();

                    case NVDRS_SETTING_TYPE.NVDRS_BINARY_TYPE:
                        return DefaultValueAsBinary();

                    case NVDRS_SETTING_TYPE.NVDRS_STRING_TYPE:
                    case NVDRS_SETTING_TYPE.NVDRS_WSTRING_TYPE:
                        return DefaultValueAsUnicodeString();

                    default:
                        throw new ArgumentOutOfRangeException(nameof(SettingType));
                }
            }
        }

        /// <summary>
        ///     Returns the default value as an integer
        /// </summary>
        /// <returns>An integer representing the default value</returns>
        public uint DefaultValueAsInteger()
        {
            return InternalDefaultValue.AsInteger();
        }

        /// <summary>
        ///     Returns the default value as a byte array
        /// </summary>
        /// <returns>An array of bytes representing the default value</returns>
        public byte[] DefaultValueAsBinary()
        {
            return InternalDefaultValue.AsBinary();
        }

        /// <summary>
        ///     Returns the default value as an unicode string
        /// </summary>
        /// <returns>A string representing the default value</returns>
        public string DefaultValueAsUnicodeString()
        {
            return InternalDefaultValue.AsUnicodeString();
        }

        /// <summary>
        ///     Returns the setting's possible values as an array of integers
        /// </summary>
        /// <returns>An array of integers representing the possible values</returns>
        public uint[] ValuesAsInteger()
        {
            return InternalValues.Take((int)NumberOfValues).Select(value => value.AsInteger()).ToArray();
        }

        /// <summary>
        ///     Returns the setting's possible values as an array of byte arrays
        /// </summary>
        /// <returns>An array of byte arrays representing the possible values</returns>
        public byte[][] ValuesAsBinary()
        {
            return InternalValues.Take((int)NumberOfValues).Select(value => value.AsBinary()).ToArray();
        }

        /// <summary>
        ///     Returns the setting's possible values as an array of unicode strings
        /// </summary>
        /// <returns>An array of unicode strings representing the possible values</returns>
        public string[] ValuesAsUnicodeString()
        {
            return InternalValues.Take((int)NumberOfValues).Select(value => value.AsUnicodeString()).ToArray();
        }

        public override bool Equals(object obj) => obj is NVDRS_SETTING_VALUES_V1 other && this.Equals(other);

        public bool Equals(NVDRS_SETTING_VALUES_V1 other)
        => Version == other.Version &&
            NumberOfValues == other.NumberOfValues &&
            _SettingType == other._SettingType &&
            InternalDefaultValue == other.InternalDefaultValue &&
            InternalValues == other.InternalValues;


        public override Int32 GetHashCode()
        {
            return (Version, NumberOfValues, _SettingType, InternalDefaultValue, InternalValues).GetHashCode();
        }
        public static bool operator ==(NVDRS_SETTING_VALUES_V1 lhs, NVDRS_SETTING_VALUES_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NVDRS_SETTING_VALUES_V1 lhs, NVDRS_SETTING_VALUES_V1 rhs) => !(lhs == rhs);

        public object Clone()
        {
            NVDRS_SETTING_VALUES_V1 other = (NVDRS_SETTING_VALUES_V1)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct UnicodeString
    {
        public const int UnicodeStringLength = 2048;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = UnicodeStringLength)]
        public readonly string InternalValue;

        public string Value
        {
            get => InternalValue;
        }

        public UnicodeString(string value)
        {
            InternalValue = value ?? string.Empty;
        }

        public override string ToString()
        {
            return Value;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_LOGICAL_GPU_DATA_V1 : IEquatable<NV_LOGICAL_GPU_DATA_V1>, ICloneable // Note: Version 1 of NV_BOARD_INFO_V1 structure
    {
        public UInt32 Version;                   //!< structure version
        public IntPtr OSAdapterId;
        public UInt32 PhysicalGPUCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)NVImport.NVAPI_MAX_PHYSICAL_GPUS)]
        public PhysicalGpuHandle[] PhysicalGPUHandles;
        public UInt32 Reserved;

        public override bool Equals(object obj) => obj is NV_LOGICAL_GPU_DATA_V1 other && this.Equals(other);

        public bool Equals(NV_LOGICAL_GPU_DATA_V1 other)
        => Version == other.Version &&
           PhysicalGPUCount == other.PhysicalGPUCount &&
           PhysicalGPUHandles.SequenceEqual(other.PhysicalGPUHandles);

        public override Int32 GetHashCode()
        {
            return (Version, PhysicalGPUCount, PhysicalGPUHandles).GetHashCode();
        }
        public static bool operator ==(NV_LOGICAL_GPU_DATA_V1 lhs, NV_LOGICAL_GPU_DATA_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_LOGICAL_GPU_DATA_V1 lhs, NV_LOGICAL_GPU_DATA_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_LOGICAL_GPU_DATA_V1 other = (NV_LOGICAL_GPU_DATA_V1)MemberwiseClone();
            return other;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_BOARD_INFO_V1 : IEquatable<NV_BOARD_INFO_V1>, ICloneable // Note: Version 1 of NV_BOARD_INFO_V1 structure
    {
        public UInt32 Version;                   //!< structure version
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] BoardNum;               //!< Board Serial Number [16]

        public override bool Equals(object obj) => obj is NV_BOARD_INFO_V1 other && this.Equals(other);

        public bool Equals(NV_BOARD_INFO_V1 other)
        => Version == other.Version &&
           BoardNum.SequenceEqual(other.BoardNum);

        public override Int32 GetHashCode()
        {
            return (Version, BoardNum).GetHashCode();
        }
        public static bool operator ==(NV_BOARD_INFO_V1 lhs, NV_BOARD_INFO_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_BOARD_INFO_V1 lhs, NV_BOARD_INFO_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_BOARD_INFO_V1 other = (NV_BOARD_INFO_V1)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_EDID_V3 : IEquatable<NV_EDID_V3>, ICloneable // Note: Version 3 of NV_EDID_V3 structure
    {
        public UInt32 Version;        //!< Structure version
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)NVImport.NV_EDID_DATA_SIZE)]
        public Byte[] EDID_Data;    // EDID_Data[NV_EDID_DATA_SIZE];
        public UInt32 SizeofEDID;
        public UInt32 EdidId;     //!< ID which always returned in a monotonically increasing counter.
                                  //!< Across a split-EDID read we need to verify that all calls returned the same edidId.
                                  //!< This counter is incremented if we get the updated EDID.
        public UInt32 Offset;    //!< Which 256-byte page of the EDID we want to read. Start at 0.
                                 //!< If the read succeeds with edidSize > NV_EDID_DATA_SIZE,
                                 //!< call back again with offset+256 until we have read the entire buffer

        public override bool Equals(object obj) => obj is NV_EDID_V3 other && this.Equals(other);

        public bool Equals(NV_EDID_V3 other)
        => Version == other.Version &&
           EDID_Data.SequenceEqual(other.EDID_Data) &&
           SizeofEDID == other.SizeofEDID &&
           EdidId == other.EdidId &&
           Offset.Equals(other.Offset);

        public override Int32 GetHashCode()
        {
            return (Version, EDID_Data, SizeofEDID, EdidId, Offset).GetHashCode();
        }
        public static bool operator ==(NV_EDID_V3 lhs, NV_EDID_V3 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_EDID_V3 lhs, NV_EDID_V3 rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_EDID_V3 other = (NV_EDID_V3)MemberwiseClone();
            return other;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Ansi)]
    public struct NV_TIMING_EXTRA : IEquatable<NV_TIMING_EXTRA>, ICloneable
    {
        public UInt32 Flags;          //!< Reserved for NVIDIA hardware-based enhancement, such as double-scan.
        public ushort RefreshRate;            //!< Logical refresh rate to present
        public UInt32 FrequencyInMillihertz;         //!< Physical vertical refresh rate in 0.001Hz
        public ushort VerticalAspect;        //!< Display aspect ratio Hi(aspect):horizontal-aspect, Low(aspect):vertical-aspect
        public ushort HorizontalAspect;        //!< Display aspect ratio Hi(aspect):horizontal-aspect, Low(aspect):vertical-aspect
        public ushort HorizontalPixelRepetition;           //!< Bit-wise pixel repetition factor: 0x1:no pixel repetition; 0x2:each pixel repeats twice horizontally,..
        public UInt32 TimingStandard;        //!< Timing standard
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Name;      //!< Timing name

        public override bool Equals(object obj) => obj is NV_TIMING_EXTRA other && this.Equals(other);

        public bool Equals(NV_TIMING_EXTRA other)
        => Flags == other.Flags &&
           RefreshRate == other.RefreshRate &&
           FrequencyInMillihertz == other.FrequencyInMillihertz &&
           VerticalAspect == other.VerticalAspect &&
           HorizontalAspect == other.HorizontalAspect &&
           HorizontalPixelRepetition == other.HorizontalPixelRepetition &&
           TimingStandard == other.TimingStandard &&
           Name == other.Name;

        public override Int32 GetHashCode()
        {
            return (Flags, RefreshRate, FrequencyInMillihertz, HorizontalAspect, HorizontalPixelRepetition, TimingStandard, Name).GetHashCode();
        }
        public static bool operator ==(NV_TIMING_EXTRA lhs, NV_TIMING_EXTRA rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_TIMING_EXTRA lhs, NV_TIMING_EXTRA rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_TIMING_EXTRA other = (NV_TIMING_EXTRA)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8, CharSet = CharSet.Ansi)]
    public struct NV_TIMING_EXTRA_INTERNAL
    {
        public UInt32 Flags;          //!< Reserved for NVIDIA hardware-based enhancement, such as double-scan.
        public ushort RefreshRate;            //!< Logical refresh rate to present
        public UInt32 FrequencyInMillihertz;         //!< Physical vertical refresh rate in 0.001Hz
        public ushort VerticalAspect;        //!< Display aspect ratio Hi(aspect):horizontal-aspect, Low(aspect):vertical-aspect
        public ushort HorizontalAspect;        //!< Display aspect ratio Hi(aspect):horizontal-aspect, Low(aspect):vertical-aspect
        public ushort HorizontalPixelRepetition;           //!< Bit-wise pixel repetition factor: 0x1:no pixel repetition; 0x2:each pixel repeats twice horizontally,..
        public UInt32 TimingStandard;        //!< Timing standard
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string Name;      //!< Timing name

    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_TIMING : IEquatable<NV_TIMING>, ICloneable
    {
        // VESA scan out timing parameters:
        public ushort HVisible;         //!< horizontal visible
        public ushort HBorder;          //!< horizontal border
        public ushort HFrontPorch;      //!< horizontal front porch
        public ushort HSyncWidth;       //!< horizontal sync width
        public ushort HTotal;           //!< horizontal total
        public TIMING_HORIZONTAL_SYNC_POLARITY HSyncPol;         //!< horizontal sync polarity: 1-negative, 0-positive

        public ushort VVisible;         //!< vertical visible
        public ushort VBorder;          //!< vertical border
        public ushort VFrontPorch;      //!< vertical front porch
        public ushort VSyncWidth;       //!< vertical sync width
        public ushort VTotal;           //!< vertical total
        public TIMING_VERTICAL_SYNC_POLARITY VSyncPol;         //!< vertical sync polarity: 1-negative, 0-positive

        public TIMING_SCAN_MODE ScanMode;       //!< 1-Int32erlaced, 0-progressive
        public UInt32 Pclk;             //!< pixel clock in 10 kHz

        //other timing related extras
        public NV_TIMING_EXTRA Extra;

        public override bool Equals(object obj) => obj is NV_TIMING other && this.Equals(other);

        public bool Equals(NV_TIMING other)
        => HVisible == other.HVisible &&
           HBorder == other.HBorder &&
           HFrontPorch == other.HFrontPorch &&
           HSyncWidth == other.HSyncWidth &&
           HTotal == other.HTotal &&
           HSyncPol == other.HSyncPol &&
           VVisible == other.VVisible &&
           VBorder == other.VBorder &&
           VFrontPorch == other.VFrontPorch &&
           VSyncWidth == other.VSyncWidth &&
           VTotal == other.VTotal &&
           VSyncPol == other.VSyncPol &&
           ScanMode == other.ScanMode &&
           Pclk == other.Pclk &&
           Extra.Equals(other.Extra);

        public override Int32 GetHashCode()
        {
            return (HVisible, HBorder, HFrontPorch, HSyncWidth, HTotal, HSyncPol, VVisible, VBorder, VFrontPorch, VSyncWidth, VTotal, VSyncPol, ScanMode, Pclk, Extra).GetHashCode();
        }
        public static bool operator ==(NV_TIMING lhs, NV_TIMING rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_TIMING lhs, NV_TIMING rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_TIMING other = (NV_TIMING)MemberwiseClone();
            other.Extra = (NV_TIMING_EXTRA)Extra.Clone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_TIMING_INTERNAL
    {
        // VESA scan out timing parameters:
        public ushort HVisible;         //!< horizontal visible
        public ushort HBorder;          //!< horizontal border
        public ushort HFrontPorch;      //!< horizontal front porch
        public ushort HSyncWidth;       //!< horizontal sync width
        public ushort HTotal;           //!< horizontal total
        public TIMING_HORIZONTAL_SYNC_POLARITY HSyncPol;         //!< horizontal sync polarity: 1-negative, 0-positive

        public ushort VVisible;         //!< vertical visible
        public ushort VBorder;          //!< vertical border
        public ushort VFrontPorch;      //!< vertical front porch
        public ushort VSyncWidth;       //!< vertical sync width
        public ushort VTotal;           //!< vertical total
        public TIMING_VERTICAL_SYNC_POLARITY VSyncPol;         //!< vertical sync polarity: 1-negative, 0-positive

        public TIMING_SCAN_MODE ScanMode;       //!< 1-Int32erlaced, 0-progressive
        public UInt32 Pclk;             //!< pixel clock in 10 kHz

        //other timing related extras - points to a NV_TIMING_EXTRA_INTERNAL
        public NV_TIMING_EXTRA_INTERNAL Extra;

    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_RECT : IEquatable<NV_RECT>, ICloneable
    {
        public UInt32 Left;
        public UInt32 Top;
        public UInt32 Right;
        public UInt32 Bottom;

        public override bool Equals(object obj) => obj is NV_RECT other && this.Equals(other);

        public bool Equals(NV_RECT other)
        => Left == other.Left &&
           Top == other.Top &&
           Right == other.Right &&
           Bottom == other.Bottom;

        public override Int32 GetHashCode()
        {
            return (Left, Top, Right, Bottom).GetHashCode();
        }
        public static bool operator ==(NV_RECT lhs, NV_RECT rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_RECT lhs, NV_RECT rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_RECT other = (NV_RECT)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_LUID : IEquatable<NV_LUID>, ICloneable
    {
        public UInt32 LowPart;
        public UInt32 HighPart;

        public override bool Equals(object obj) => obj is NV_LUID other && this.Equals(other);

        public bool Equals(NV_LUID other)
        => LowPart == other.LowPart &&
           HighPart == other.HighPart;

        public override Int32 GetHashCode()
        {
            return (LowPart, HighPart).GetHashCode();
        }
        public static bool operator ==(NV_LUID lhs, NV_LUID rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_LUID lhs, NV_LUID rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_LUID other = (NV_LUID)MemberwiseClone();
            return other;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_POSITION : IEquatable<NV_POSITION>, ICloneable
    {
        public Int32 X;
        public Int32 Y;

        public override bool Equals(object obj) => obj is NV_POSITION other && this.Equals(other);

        public bool Equals(NV_POSITION other)
        => X == other.X &&
           Y == other.Y;

        public override Int32 GetHashCode()
        {
            return (X, Y).GetHashCode();
        }
        public static bool operator ==(NV_POSITION lhs, NV_POSITION rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_POSITION lhs, NV_POSITION rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_POSITION other = (NV_POSITION)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_POSITION_INTERNAL
    {
        public Int32 X;
        public Int32 Y;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_RESOLUTION : IEquatable<NV_RESOLUTION>, ICloneable
    {
        public UInt32 Width;
        public UInt32 Height;
        public UInt32 ColorDepth;

        public override bool Equals(object obj) => obj is NV_RESOLUTION other && this.Equals(other);

        public bool Equals(NV_RESOLUTION other)
        => Width == other.Width &&
           Height == other.Height &&
           ColorDepth == other.ColorDepth;

        public override Int32 GetHashCode()
        {
            return (Width, Height, ColorDepth).GetHashCode();
        }
        public static bool operator ==(NV_RESOLUTION lhs, NV_RESOLUTION rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_RESOLUTION lhs, NV_RESOLUTION rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_RESOLUTION other = (NV_RESOLUTION)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_RESOLUTION_INTERNAL
    {
        public UInt32 Width;
        public UInt32 Height;
        public UInt32 ColorDepth;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_VIEWPORTF : IEquatable<NV_VIEWPORTF>, ICloneable
    {
        public float X;    //!<  x-coordinate of the viewport top-left point
        public float Y;    //!<  y-coordinate of the viewport top-left point
        public float W;    //!<  Width of the viewport
        public float H;    //!<  Height of the viewport

        public NV_VIEWPORTF(float myX, float myY, float myW, float myH) : this()
        {
            X = myX;
            Y = myY;
            W = myW;
            H = myH;
        }

        public override bool Equals(object obj) => obj is NV_VIEWPORTF other && this.Equals(other);

        // NOTE: Using Math.Round for equality testing between floats.
        /*public bool Equals(NV_VIEWPORTF other)
        => Math.Round(X, 5) == Math.Round(other.X, 5) &&
           Math.Round(Y, 5) == Math.Round(other.Y, 5) &&
           Math.Round(W, 5) == Math.Round(other.W, 5) &&
           Math.Round(H, 5) == Math.Round(other.H, 5);*/
        public bool Equals(NV_VIEWPORTF other)
        => X == other.X &&
           Y == other.Y &&
           W == other.W &&
           H == other.H;

        public override Int32 GetHashCode()
        {
            return (X, Y, W, H).GetHashCode();
        }
        public static bool operator ==(NV_VIEWPORTF lhs, NV_VIEWPORTF rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_VIEWPORTF lhs, NV_VIEWPORTF rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_VIEWPORTF other = (NV_VIEWPORTF)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 : IEquatable<NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1>, ICloneable // Requires Version 1
    {
        public UInt32 Version;

        // Rotation and Scaling
        public NV_ROTATE Rotation;       //!< (IN) rotation setting.
        public NV_SCALING Scaling;        //!< (IN) scaling setting.

        // Refresh Rate
        public UInt32 RefreshRateInMillihertz;  //!< (IN) Non-Int32erlaced Refresh Rate of the mode, multiplied by 1000, 0 = ignored
                                                //!< This is the value which driver reports to the OS.
                                                // Flags
                                                //public UInt32 Int32erlaced:1;   //!< (IN) Interlaced mode flag, ignored if refreshRate == 0
                                                //public UInt32 primary:1;      //!< (IN) Declares primary display in clone configuration. This is *NOT* GDI Primary.
                                                //!< Only one target can be primary per source. If no primary is specified, the first
                                                //!< target will automatically be primary.
                                                //public UInt32 isPanAndScanTarget:1; //!< Whether on this target Pan and Scan is enabled or has to be enabled. Valid only
                                                //!< when the target is part of clone topology.
                                                //public UInt32 disableVirtualModeSupport:1;
                                                //public UInt32 isPreferredUnscaledTarget:1;
                                                //public UInt32 reserved:27;
        public UInt32 Flags;
        // TV format information
        public NV_GPU_CONNECTOR_TYPE ConnectorType;      //!< Specify connector type. For TV only, ignored if tvFormat == NV_DISPLAY_TV_FORMAT_NONE
        public NV_DISPLAY_TV_FORMAT TvFormat;       //!< (IN) to choose the last TV format set this value to NV_DISPLAY_TV_FORMAT_NONE
                                                    //!< In case of NvAPI_DISP_GetDisplayConfig(), this field will indicate the currently applied TV format;
                                                    //!< if no TV format is applied, this field will have NV_DISPLAY_TV_FORMAT_NONE value.
                                                    //!< In case of NvAPI_DISP_SetDisplayConfig(), this field should only be set in case of TVs;
                                                    //!< for other displays this field will be ignored and resolution & refresh rate specified in input will be used to apply the TV format.

        // Backend (raster) timing standard
        public NV_TIMING_OVERRIDE TimingOverride;     //!< Ignored if timingOverride == NV_TIMING_OVERRIDE_CURRENT
        public NV_TIMING Timing;             //!< Scan out timing, valid only if timingOverride == NV_TIMING_OVERRIDE_CUST
                                             //!< The value NV_TIMING::NV_TIMINGEXT::rrx1k is obtained from the EDID. The driver may
                                             //!< tweak this value for HDTV, stereo, etc., before reporting it to the OS.

        public override bool Equals(object obj) => obj is NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 other && this.Equals(other);

        public bool Equals(NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 other)
        => Version == other.Version &&
           Rotation == other.Rotation &&
           Scaling == other.Scaling &&
           RefreshRateInMillihertz == other.RefreshRateInMillihertz &&
           Flags == other.Flags &&
           ConnectorType == other.ConnectorType &&
           TvFormat == other.TvFormat &&
           TimingOverride == other.TimingOverride &&
           Timing.Equals(other.Timing);

        public override Int32 GetHashCode()
        {
            return (Version, Rotation, Scaling, RefreshRateInMillihertz, Flags, ConnectorType, TvFormat, TimingOverride, Timing).GetHashCode();
        }
        public static bool operator ==(NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 lhs, NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 lhs, NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 other = (NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1)MemberwiseClone();
            other.Timing = (NV_TIMING)Timing.Clone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL
    {
        public UInt32 Version;

        // Rotation and Scaling
        public NV_ROTATE Rotation;       //!< (IN) rotation setting.
        public NV_SCALING Scaling;        //!< (IN) scaling setting.

        // Refresh Rate
        public UInt32 RefreshRateInMillihertz;  //!< (IN) Non-Int32erlaced Refresh Rate of the mode, multiplied by 1000, 0 = ignored
                                                //!< This is the value which driver reports to the OS.
                                                // Flags
                                                //public UInt32 Int32erlaced:1;   //!< (IN) Interlaced mode flag, ignored if refreshRate == 0
                                                //public UInt32 primary:1;      //!< (IN) Declares primary display in clone configuration. This is *NOT* GDI Primary.
                                                //!< Only one target can be primary per source. If no primary is specified, the first
                                                //!< target will automatically be primary.
                                                //public UInt32 isPanAndScanTarget:1; //!< Whether on this target Pan and Scan is enabled or has to be enabled. Valid only
                                                //!< when the target is part of clone topology.
                                                //public UInt32 disableVirtualModeSupport:1;
                                                //public UInt32 isPreferredUnscaledTarget:1;
                                                //public UInt32 reserved:27;
        public UInt32 Flags;
        // TV format information
        public NV_GPU_CONNECTOR_TYPE ConnectorType;      //!< Specify connector type. For TV only, ignored if tvFormat == NV_DISPLAY_TV_FORMAT_NONE
        public NV_DISPLAY_TV_FORMAT TvFormat;       //!< (IN) to choose the last TV format set this value to NV_DISPLAY_TV_FORMAT_NONE
                                                    //!< In case of NvAPI_DISP_GetDisplayConfig(), this field will indicate the currently applied TV format;
                                                    //!< if no TV format is applied, this field will have NV_DISPLAY_TV_FORMAT_NONE value.
                                                    //!< In case of NvAPI_DISP_SetDisplayConfig(), this field should only be set in case of TVs;
                                                    //!< for other displays this field will be ignored and resolution & refresh rate specified in input will be used to apply the TV format.

        // Backend (raster) timing standard
        public NV_TIMING_OVERRIDE TimingOverride;     //!< Ignored if timingOverride == NV_TIMING_OVERRIDE_CURRENT
        public NV_TIMING_INTERNAL Timing;             // Points to a NV_TIMING_INTERNAL object
                                                      //!< Scan out timing, valid only if timingOverride == NV_TIMING_OVERRIDE_CUST
                                                      //!< The value NV_TIMING::NV_TIMINGEXT::rrx1k is obtained from the EDID. The driver may
                                                      //!< tweak this value for HDTV, stereo, etc., before reporting it to the OS.

    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 : IEquatable<NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2>, ICloneable
    {
        public UInt32 DisplayId;  //!< Display ID
        public NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 Details;    //!< NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO - May be NULL if no advanced settings are required
        public UInt32 WindowsCCDTargetId;   //!< Windows CCD target ID. Must be present only for non-NVIDIA adapter, for NVIDIA adapter this parameter is ignored.

        public override bool Equals(object obj) => obj is NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 other && this.Equals(other);

        public bool Equals(NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 other)
        => DisplayId == other.DisplayId &&
           Details.Equals(other.Details) &&
           WindowsCCDTargetId == other.WindowsCCDTargetId;

        public override Int32 GetHashCode()
        {
            return (DisplayId, Details, WindowsCCDTargetId).GetHashCode();
        }
        public static bool operator ==(NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 lhs, NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 lhs, NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 otherTargetInfo = (NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2)MemberwiseClone();
            otherTargetInfo.Details = (NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1)Details.Clone();
            return otherTargetInfo;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1 : IEquatable<NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1>, ICloneable
    {
        public UInt32 DisplayId;  //!< Display ID
        public NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 Details;    //!< May be NULL if no advanced settings are required

        public override bool Equals(object obj) => obj is NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1 other && this.Equals(other);

        public bool Equals(NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1 other)
        => DisplayId == other.DisplayId &&
           Details.Equals(other.Details);

        public override Int32 GetHashCode()
        {
            return (DisplayId, Details).GetHashCode();
        }
        public static bool operator ==(NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1 lhs, NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1 lhs, NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1 rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1 other = (NV_DISPLAYCONFIG_PATH_TARGET_INFO_V1)MemberwiseClone();
            other.Details = (NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1)Details.Clone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_DISPLAYCONFIG_PATH_INFO_V2 : IEquatable<NV_DISPLAYCONFIG_PATH_INFO_V2>, ICloneable // Version is 2
    {
        public UInt32 Version;
        public UInt32 SourceId;               //!< Identifies sourceId used by Windows CCD. This can be optionally set.

        public UInt32 TargetInfoCount;            //!< Number of elements in targetInfo array
        //[MarshalAs(UnmanagedType.ByValArray)]
        public NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2[] TargetInfo;
        public NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 SourceModeInfo;             //!< May be NULL if mode info is not important
        //public IntPtr SourceModeInfo;             //!< May be NULL if mode info is not important
        //public UInt32 IsNonNVIDIAAdapter : 1;     //!< True for non-NVIDIA adapter.
        //public UInt32 reserved : 31;              //!< Must be 0
        public UInt32 Flags;
        //!< Used by Non-NVIDIA adapter for pointer to OS Adapter of LUID
        //!< type, type casted to void *.
        public IntPtr OSAdapterID;

        public bool IsNonNVIDIAAdapter => Flags.GetBit(0); //!< if bit is set then this path uses a non-nvidia adapter

        public override bool Equals(object obj) => obj is NV_DISPLAYCONFIG_PATH_INFO_V2 other && this.Equals(other);

        public bool Equals(NV_DISPLAYCONFIG_PATH_INFO_V2 other)
        {
            if (!(Version == other.Version &&
            SourceId == other.SourceId &&
            TargetInfoCount == other.TargetInfoCount &&
            SourceModeInfo.Equals(other.SourceModeInfo) &&
            Flags == other.Flags))
            {
                return false;
            }

            // Now we need to go through the HDR states comparing vaues, as the order changes if there is a cloned display
            if (!NVImport.EqualButDifferentOrder<NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2>(TargetInfo, other.TargetInfo))
            {
                return false;
            }

            return true;

        }

        public override Int32 GetHashCode()
        {
            return (Version, SourceId, TargetInfoCount, TargetInfo, SourceModeInfo, Flags).GetHashCode();
        }
        public static bool operator ==(NV_DISPLAYCONFIG_PATH_INFO_V2 lhs, NV_DISPLAYCONFIG_PATH_INFO_V2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_DISPLAYCONFIG_PATH_INFO_V2 lhs, NV_DISPLAYCONFIG_PATH_INFO_V2 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_DISPLAYCONFIG_PATH_INFO_V2 other = (NV_DISPLAYCONFIG_PATH_INFO_V2)MemberwiseClone();
            other.TargetInfo = new NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2[TargetInfoCount];
            for (int x = 0; x < (int)TargetInfoCount; x++)
            {
                other.TargetInfo[x] = (NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2)TargetInfo[x].Clone();
            }
            other.SourceModeInfo = (NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1)SourceModeInfo.Clone(); ;
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL // Version is 2 - This is for processing pass 2 only!
    {
        public UInt32 Version;
        public UInt32 SourceId;               //!< Identifies sourceId used by Windows CCD. This can be optionally set.
        public UInt32 TargetInfoCount;            //!< Number of elements in targetInfo array
        public IntPtr TargetInfo;
        public IntPtr SourceModeInfo;             //!< May be NULL if mode info is not important
        //public UInt32 IsNonNVIDIAAdapter : 1;     //!< True for non-NVIDIA adapter.
        //public UInt32 reserved : 31;              //!< Must be 0
        public UInt32 Flags;
        //!< Used by Non-NVIDIA adapter for pointer to OS Adapter of LUID
        //!< type, type casted to void *.
        public IntPtr OSAdapterID;

        public bool IsNonNVIDIAAdapter => Flags.GetBit(0); //!< if bit is set then this path uses a non-nvidia adapter
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL
    {
        public UInt32 DisplayId;  //!< Display ID
        public IntPtr Details;  // Points to an NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL object  
                                //!< NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO - May be NULL if no advanced settings are required
        public UInt32 WindowsCCDTargetId;   //!< Windows CCD target ID. Must be present only for non-NVIDIA adapter, for NVIDIA adapter this parameter is ignored.

    }


    [StructLayout(LayoutKind.Sequential)]
    public struct NV_DISPLAYCONFIG_PATH_INFO_V1 : IEquatable<NV_DISPLAYCONFIG_PATH_INFO_V1>, ICloneable // Version is 1
    {
        public UInt32 Version;
        public UInt32 SourceId;               //!< Identifies sourceId used by Windows CCD. This can be optionally set.

        public UInt32 TargetInfoCount;            //!< Number of elements in targetInfo array
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2[] TargetInfo;
        public NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 SourceModeInfo;             //!< May be NULL if mode info is not important
                                                                                //public UInt32 IsNonNVIDIAAdapter : 1;     //!< True for non-NVIDIA adapter.
                                                                                //public UInt32 reserved : 31;              //!< Must be 0
                                                                                //public LUID pOSAdapterID;              //!< Used by Non-NVIDIA adapter for poInt32er to OS Adapter of LUID
                                                                                //!< type, type casted to void *.

        public override bool Equals(object obj) => obj is NV_DISPLAYCONFIG_PATH_INFO_V1 other && this.Equals(other);

        public bool Equals(NV_DISPLAYCONFIG_PATH_INFO_V1 other)
        => Version == other.Version &&
           SourceId == other.SourceId &&
           TargetInfoCount == other.TargetInfoCount &&
           TargetInfo.SequenceEqual(other.TargetInfo) &&
           SourceModeInfo.Equals(other.SourceModeInfo);

        public override Int32 GetHashCode()
        {
            return (Version, SourceId, TargetInfoCount, TargetInfo, SourceModeInfo).GetHashCode();
        }
        public static bool operator ==(NV_DISPLAYCONFIG_PATH_INFO_V1 lhs, NV_DISPLAYCONFIG_PATH_INFO_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_DISPLAYCONFIG_PATH_INFO_V1 lhs, NV_DISPLAYCONFIG_PATH_INFO_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_DISPLAYCONFIG_PATH_INFO_V2 other = (NV_DISPLAYCONFIG_PATH_INFO_V2)MemberwiseClone();
            other.TargetInfo = new NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2[TargetInfoCount];
            for (int x = 0; x < (int)TargetInfoCount; x++)
            {
                other.TargetInfo[x] = (NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2)TargetInfo[x].Clone();
            }
            other.SourceModeInfo = (NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1)SourceModeInfo.Clone(); ;
            return other;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 : IEquatable<NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1>, ICloneable
    {
        public NV_RESOLUTION Resolution;
        public NV_FORMAT ColorFormat;                //!< Ignored at present, must be NV_FORMAT_UNKNOWN (0)
        public NV_POSITION Position;                   //!< Is all positions are 0 or invalid, displays will be automatically
                                                       //!< positioned from left to right with GDI Primary at 0,0, and all
                                                       //!< other displays in the order of the path array.
        public NV_DISPLAYCONFIG_SPANNING_ORIENTATION SpanningOrientation;        //!< Spanning is only supported on XP
        public UInt32 Flags;

        public bool IsGDIPrimary => (Flags & 0x1) == 0x1; //!< if bit is set then this source is the primary GDI source
        public bool IsSLIFocus => (Flags & 0x2) == 0x2; //!< if bit is set then this source has SLI focus

        public override bool Equals(object obj) => obj is NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 other && this.Equals(other);

        public bool Equals(NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 other)
        => Resolution.Equals(other.Resolution) &&
           ColorFormat == other.ColorFormat &&
           Position.Equals(other.Position) &&
           Flags == other.Flags;

        public override Int32 GetHashCode()
        {
            return (Resolution, ColorFormat, Position, Flags).GetHashCode();
        }
        public static bool operator ==(NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 lhs, NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 lhs, NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 other = (NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1)MemberwiseClone();
            other.Resolution = (NV_RESOLUTION)Resolution.Clone();
            other.Position = (NV_POSITION)Position.Clone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1_INTERNAL
    {
        public IntPtr Resolution;       // Points to a NV_RESOLUTION_INTERNAL object
        public NV_FORMAT ColorFormat;                //!< Ignored at present, must be NV_FORMAT_UNKNOWN (0)
        public IntPtr Position;                   // Points to a NV_POSITION_INTERNAL object
                                                  //!< Is all positions are 0 or invalid, displays will be automatically
                                                  //!< positioned from left to right with GDI Primary at 0,0, and all
                                                  //!< other displays in the order of the path array.
        public NV_DISPLAYCONFIG_SPANNING_ORIENTATION SpanningOrientation;        //!< Spanning is only supported on XP
        public UInt32 Flags;
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_TOPO_BRIEF : IEquatable<NV_MOSAIC_TOPO_BRIEF>, ICloneable // Note: Version 1 of NV_MOSAIC_TOPO_BRIEF structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 1
        public NV_MOSAIC_TOPO Topo;     //!< The topology
        public UInt32 Enabled;            //!< 1 if topo is enabled, else 0
        public UInt32 IsPossible;         //!< 1 if topo *can* be enabled, else 0

        public override bool Equals(object obj) => obj is NV_MOSAIC_TOPO_BRIEF other && this.Equals(other);

        public bool Equals(NV_MOSAIC_TOPO_BRIEF other)
        => Version == other.Version;
        // Note: This is ignored as it changes depending on what the last topology applied was, which of course changes with use. 
        // This doesn't affect the whether the config can be applied, but really all it does is mess up the Equality comparison!
        // so I've ignored it.
        // Topo.Equals(other.Topo); 
        // Note: Removed Enabled and IsPossible from matches so that comparisons work even if they aren't enabled or possible now
        // Enabled == other.Enabled &&
        // IsPossible == other.IsPossible  

        public override Int32 GetHashCode()
        {
            return (Version, Topo, Enabled, IsPossible).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_TOPO_BRIEF lhs, NV_MOSAIC_TOPO_BRIEF rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_TOPO_BRIEF lhs, NV_MOSAIC_TOPO_BRIEF rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_TOPO_BRIEF other = (NV_MOSAIC_TOPO_BRIEF)MemberwiseClone();
            return other;
        }
    }

    //
    //! This structure defines a group of topologies that work together to create one
    //! overall layout.  All of the supported topologies are represented with this
    //! structure.
    //!
    //! For example, a 'Passive Stereo' topology would be represented with this
    //! structure, and would have separate topology details for the left and right eyes.
    //! The count would be 2.  A 'Basic' topology is also represented by this structure,
    //! with a count of 1.
    //!
    //! The structure is primarily used Int32ernally, but is exposed to applications in a
    //! read-only fashion because there are some details in it that might be useful
    //! (like the number of rows/cols, or connected display information).  A user can
    //! get the filled-in structure by calling NvAPI_Mosaic_GetTopoGroup().
    //!
    //! You can then look at the detailed values within the structure.  There are no
    //! entrypoInt32s which take this structure as input (effectively making it read-only).
    [StructLayout(LayoutKind.Sequential)]
    public struct NV_MOSAIC_TOPO_GROUP : IEquatable<NV_MOSAIC_TOPO_GROUP>, ICloneable // Note: Version 1 of NV_MOSAIC_TOPO_GROUP structure
    {
        public UInt32 Version;                        // Version of this structure - MUST BE SET TO 1
        public NV_MOSAIC_TOPO_BRIEF Brief;          //!< The brief details of this topo
        public UInt32 Count;                          //!< Number of topos in array below
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public NV_MOSAIC_TOPO_DETAILS[] Topos;      //!< Topo Array with 1 or 2 entries in it

        public override bool Equals(object obj) => obj is NV_MOSAIC_TOPO_GROUP other && this.Equals(other);

        public bool Equals(NV_MOSAIC_TOPO_GROUP other)
        => Version == other.Version &&
           Brief.Equals(other.Brief) &&
           Count == other.Count &&
           Topos.SequenceEqual(other.Topos);

        public override Int32 GetHashCode()
        {
            return (Version, Brief, Count, Topos).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_TOPO_GROUP lhs, NV_MOSAIC_TOPO_GROUP rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_TOPO_GROUP lhs, NV_MOSAIC_TOPO_GROUP rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_TOPO_GROUP other = (NV_MOSAIC_TOPO_GROUP)MemberwiseClone();
            other.Brief = (NV_MOSAIC_TOPO_BRIEF)Brief.Clone();
            other.Topos = new NV_MOSAIC_TOPO_DETAILS[Topos.Length];
            for (int x = 0; x < (int)Topos.Length; x++)
            {
                other.Topos[x] = (NV_MOSAIC_TOPO_DETAILS)Topos[x].Clone();
            }
            return other;
        }
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct NV_MOSAIC_TOPO_DETAILS : IEquatable<NV_MOSAIC_TOPO_DETAILS>, ICloneable // Note: Version 1 of NV_MOSAIC_TOPO_DETAILS structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 1 
        public LogicalGpuHandle LogicalGPUHandle;     //!< Logical GPU for this topology  
        public NV_MOSAIC_TOPO_VALIDITY ValidityMask;            //!< 0 means topology is valid with the current hardware. 
                                                                //! If not 0, inspect bits against NV_MOSAIC_TOPO_VALIDITY_*.
        public UInt32 RowCount;         //!< Number of displays in a row. size is 4
        public UInt32 ColCount;         //!< Number of displays in a column. size is 4
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)(NVImport.NVAPI_MAX_MOSAIC_DISPLAY_ROWS * NVImport.NVAPI_MAX_MOSAIC_DISPLAY_COLUMNS))]
        public NV_MOSAIC_TOPO_GPU_LAYOUT_CELL[] GPULayout1D;

        public NV_MOSAIC_TOPO_GPU_LAYOUT_CELL[,] GPULayout
        {
            get
            {
                var GpuLayout2D = new NV_MOSAIC_TOPO_GPU_LAYOUT_CELL[NVImport.NVAPI_MAX_MOSAIC_DISPLAY_ROWS, NVImport.NVAPI_MAX_MOSAIC_DISPLAY_COLUMNS];
                var position = 0;
                for (int first = 0; first < 8; first++)
                {
                    for (int second = 0; second < 8; second++)
                    {
                        GpuLayout2D[first, second] = GPULayout1D[position++];
                    }
                }
                return GpuLayout2D;
            }
            set
            {
                GPULayout1D = new NV_MOSAIC_TOPO_GPU_LAYOUT_CELL[NVImport.NVAPI_MAX_MOSAIC_DISPLAY_ROWS * NVImport.NVAPI_MAX_MOSAIC_DISPLAY_COLUMNS];
                var position = 0;
                for (int first = 0; first < 8; first++)
                {
                    for (int second = 0; second < 8; second++)
                    {
                        GPULayout1D[position++] = GPULayout[first, second];
                    }
                }
                GPULayout = value;
            }
        }

        public override bool Equals(object obj) => obj is NV_MOSAIC_TOPO_DETAILS other && this.Equals(other);
        public bool Equals(NV_MOSAIC_TOPO_DETAILS other)
        => Version == other.Version &&
           LogicalGPUHandle.Equals(other.LogicalGPUHandle) &&
           ValidityMask == other.ValidityMask &&
           RowCount == other.RowCount &&
           ColCount == other.ColCount &&
           ValidityMask == other.ValidityMask &&
           GPULayout1D.SequenceEqual(other.GPULayout1D);

        public bool TopologyValid => ValidityMask == 0; //!< The topology is valid
        public bool TopologyMissingGPU => ValidityMask.HasFlag(NV_MOSAIC_TOPO_VALIDITY.MISSING_GPU); //!< Not enough SLI GPUs were found to fill the entire
                                                                                                     //! topology. hPhysicalGPU will be 0 for these.
        public bool TopologyMissingDisplay => ValidityMask.HasFlag(NV_MOSAIC_TOPO_VALIDITY.MISSING_DISPLAY);//!< Not enough displays were found to fill the entire
                                                                                                            //! topology. displayOutputId will be 0 for these.
        public bool TopologyMixedDisplayTypes => ValidityMask.HasFlag(NV_MOSAIC_TOPO_VALIDITY.MIXED_DISPLAY_TYPES);//!< The topoogy is only possible with displays of the same
                                                                                                                   //! NV_GPU_OUTPUT_TYPE. Check displayOutputIds to make
                                                                                                                   //! sure they are all CRTs, or all DFPs.

        public override Int32 GetHashCode()
        {
            return (Version, LogicalGPUHandle, ValidityMask, RowCount, ColCount, ValidityMask).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_TOPO_DETAILS lhs, NV_MOSAIC_TOPO_DETAILS rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_TOPO_DETAILS lhs, NV_MOSAIC_TOPO_DETAILS rhs) => !(lhs == rhs);

        public object Clone()
        {
            NV_MOSAIC_TOPO_DETAILS other = (NV_MOSAIC_TOPO_DETAILS)MemberwiseClone();
            other.LogicalGPUHandle = (LogicalGpuHandle)LogicalGPUHandle.Clone();
            other.GPULayout1D = new NV_MOSAIC_TOPO_GPU_LAYOUT_CELL[GPULayout1D.Length];
            for (int x = 0; x < (int)GPULayout1D.Length; x++)
            {
                other.GPULayout1D[x] = (NV_MOSAIC_TOPO_GPU_LAYOUT_CELL)GPULayout1D[x].Clone();
            }

            other.GPULayout = new NV_MOSAIC_TOPO_GPU_LAYOUT_CELL[GPULayout.GetLength(0), GPULayout.GetLength(1)];
            for (int x = 0; x < (int)GPULayout.GetLength(0); x++)
            {
                for (int y = 0; y < (int)GPULayout.GetLength(1); y++)
                {
                    other.GPULayout[x, y] = (NV_MOSAIC_TOPO_GPU_LAYOUT_CELL)GPULayout[x, y].Clone();
                }
            }
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_TOPO_GPU_LAYOUT_CELL : IEquatable<NV_MOSAIC_TOPO_GPU_LAYOUT_CELL>, ICloneable
    {
        public PhysicalGpuHandle PhysicalGPUHandle;     //!< Physical GPU to be used in the topology (0 if GPU missing) size is 8
        public UInt32 DisplayOutputId;            //!< Connected display target(0 if no display connected) size is 8
        public Int32 OverlapX;         //!< Pixels of overlap on left of target: (+overlap, -gap) size is 8
        public Int32 OverlapY;         //!< Pixels of overlap on top of target: (+overlap, -gap) size is 8

        public override bool Equals(object obj) => obj is NV_MOSAIC_TOPO_GPU_LAYOUT_CELL other && this.Equals(other);

        public bool Equals(NV_MOSAIC_TOPO_GPU_LAYOUT_CELL other)
        => PhysicalGPUHandle.Equals(other.PhysicalGPUHandle) &&
           DisplayOutputId == other.DisplayOutputId &&
            OverlapX == other.OverlapX &&
            OverlapY == other.OverlapY;

        public override Int32 GetHashCode()
        {
            return (PhysicalGPUHandle, DisplayOutputId, OverlapX, OverlapY).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_TOPO_GPU_LAYOUT_CELL lhs, NV_MOSAIC_TOPO_GPU_LAYOUT_CELL rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_TOPO_GPU_LAYOUT_CELL lhs, NV_MOSAIC_TOPO_GPU_LAYOUT_CELL rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_TOPO_GPU_LAYOUT_CELL other = (NV_MOSAIC_TOPO_GPU_LAYOUT_CELL)MemberwiseClone();
            other.PhysicalGPUHandle = (PhysicalGpuHandle)PhysicalGPUHandle.Clone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_DISPLAY_SETTING_V1 : IEquatable<NV_MOSAIC_DISPLAY_SETTING_V1>, ICloneable // Note: Version 1 of NV_MOSAIC_DISPLAY_SETTING structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 1
        public UInt32 Width;              //!< Per-display width
        public UInt32 Height;             //!< Per-display height
        public UInt32 Bpp;                //!< Bits per pixel
        public UInt32 Freq;               //!< Display frequency

        public override bool Equals(object obj) => obj is NV_MOSAIC_DISPLAY_SETTING_V1 other && this.Equals(other);

        public bool Equals(NV_MOSAIC_DISPLAY_SETTING_V1 other)
        => Version == other.Version &&
           Width == other.Width &&
           Height == other.Height &&
           Bpp == other.Bpp &&
           Freq == other.Freq;

        public override Int32 GetHashCode()
        {
            return (Version, Width, Height, Bpp, Freq).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_DISPLAY_SETTING_V1 lhs, NV_MOSAIC_DISPLAY_SETTING_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_DISPLAY_SETTING_V1 lhs, NV_MOSAIC_DISPLAY_SETTING_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_DISPLAY_SETTING_V1 other = (NV_MOSAIC_DISPLAY_SETTING_V1)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_DISPLAY_SETTING_V2 : IEquatable<NV_MOSAIC_DISPLAY_SETTING_V2>, ICloneable // Note: Version 2 of NV_MOSAIC_DISPLAY_SETTING structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 2
        public UInt32 Width;              //!< Per-display width
        public UInt32 Height;             //!< Per-display height
        public UInt32 Bpp;                //!< Bits per pixel
        public UInt32 Freq;               //!< Display frequency
        public UInt32 Rrx1k;              //!< Display frequency in x1k

        public override bool Equals(object obj) => obj is NV_MOSAIC_DISPLAY_SETTING_V2 other && this.Equals(other);

        public bool Equals(NV_MOSAIC_DISPLAY_SETTING_V2 other)
        => Version == other.Version &&
           Width == other.Width &&
           Height == other.Height &&
           Bpp == other.Bpp &&
           Freq == other.Freq &&
           Rrx1k == other.Rrx1k;

        public override Int32 GetHashCode()
        {
            return (Version, Width, Height, Bpp, Freq, Rrx1k).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_DISPLAY_SETTING_V2 lhs, NV_MOSAIC_DISPLAY_SETTING_V2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_DISPLAY_SETTING_V2 lhs, NV_MOSAIC_DISPLAY_SETTING_V2 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_DISPLAY_SETTING_V2 other = (NV_MOSAIC_DISPLAY_SETTING_V2)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_GRID_TOPO_V1 : IEquatable<NV_MOSAIC_GRID_TOPO_V1>, ICloneable // Note: Version 1 of NV_MOSAIC_GRID_TOPO structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 1
        public UInt32 Rows;              //!< Per-display width
        public UInt32 Columns;             //!< Per-display height
        public UInt32 DisplayCount;                //!< Bits per pixel
        public UInt32 Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)NVImport.NV_MOSAIC_MAX_DISPLAYS)]
        public NV_MOSAIC_GRID_TOPO_DISPLAY_V1[] Displays;
        public NV_MOSAIC_DISPLAY_SETTING_V1 DisplaySettings;

        public bool ApplyWithBezelCorrect => (Flags & 0x1) == 0x1;
        public bool ImmersiveGaming => (Flags & 0x2) == 0x2;
        public bool BaseMosaic => (Flags & 0x4) == 0x4;
        public bool DriverReloadAllowed => (Flags & 0x8) == 0x8;
        public bool AcceleratePrimaryDisplay => (Flags & 0x10) == 0x10;

        public override bool Equals(object obj) => obj is NV_MOSAIC_GRID_TOPO_DISPLAY_V1 other && this.Equals(other);

        public bool Equals(NV_MOSAIC_GRID_TOPO_V1 other)
        => Version == other.Version &&
           Rows == other.Rows &&
           Columns == other.Columns &&
           DisplayCount == other.DisplayCount &&
           Flags == other.Flags &&
           Displays.SequenceEqual(other.Displays) &&
           DisplaySettings.Equals(other.DisplaySettings);

        public override Int32 GetHashCode()
        {
            return (Version, Rows, Columns, DisplayCount, Flags, Displays, DisplaySettings).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_GRID_TOPO_V1 lhs, NV_MOSAIC_GRID_TOPO_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_GRID_TOPO_V1 lhs, NV_MOSAIC_GRID_TOPO_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_GRID_TOPO_V1 other = (NV_MOSAIC_GRID_TOPO_V1)MemberwiseClone();
            other.DisplaySettings = (NV_MOSAIC_DISPLAY_SETTING_V1)DisplaySettings.Clone();
            other.Displays = new NV_MOSAIC_GRID_TOPO_DISPLAY_V1[Displays.Length];
            for (int x = 0; x < (int)Displays.Length; x++)
            {
                other.Displays[x] = (NV_MOSAIC_GRID_TOPO_DISPLAY_V1)Displays[x].Clone();
            }
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_GRID_TOPO_V2 : IEquatable<NV_MOSAIC_GRID_TOPO_V2>, ICloneable // Note: Version 2 of NV_MOSAIC_GRID_TOPO structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 2
        public UInt32 Rows;              //!< Per-display width
        public UInt32 Columns;             //!< Per-display height
        public UInt32 DisplayCount;                //!< Bits per pixel
        public UInt32 Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)NVImport.NV_MOSAIC_MAX_DISPLAYS)]
        public NV_MOSAIC_GRID_TOPO_DISPLAY_V2[] Displays;
        public NV_MOSAIC_DISPLAY_SETTING_V1 DisplaySettings;

        public bool ApplyWithBezelCorrect => (Flags & 0x1) == 0x1;
        public bool ImmersiveGaming => (Flags & 0x2) == 0x2;
        public bool BaseMosaic => (Flags & 0x4) == 0x4;
        public bool DriverReloadAllowed => (Flags & 0x8) == 0x8;
        public bool AcceleratePrimaryDisplay => (Flags & 0x10) == 0x10;
        public bool PixelShift => (Flags & 0x20) == 0x20;

        public override bool Equals(object obj) => obj is NV_MOSAIC_GRID_TOPO_V2 other && this.Equals(other);

        public bool Equals(NV_MOSAIC_GRID_TOPO_V2 other)
        => Version == other.Version &&
           Rows == other.Rows &&
           Columns == other.Columns &&
           DisplayCount == other.DisplayCount &&
           Flags == other.Flags &&
           Displays.SequenceEqual(other.Displays) &&
           DisplaySettings.Equals(other.DisplaySettings);

        public override Int32 GetHashCode()
        {
            return (Version, Rows, Columns, DisplayCount, Flags, Displays, DisplaySettings).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_GRID_TOPO_V2 lhs, NV_MOSAIC_GRID_TOPO_V2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_GRID_TOPO_V2 lhs, NV_MOSAIC_GRID_TOPO_V2 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_GRID_TOPO_V2 other = (NV_MOSAIC_GRID_TOPO_V2)MemberwiseClone();
            other.DisplaySettings = (NV_MOSAIC_DISPLAY_SETTING_V1)DisplaySettings.Clone();
            other.Displays = new NV_MOSAIC_GRID_TOPO_DISPLAY_V2[Displays.Length];
            for (int x = 0; x < (int)Displays.Length; x++)
            {
                other.Displays[x] = (NV_MOSAIC_GRID_TOPO_DISPLAY_V2)Displays[x].Clone();
            }
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_GRID_TOPO_DISPLAY_V1 : IEquatable<NV_MOSAIC_GRID_TOPO_DISPLAY_V1>, ICloneable // Note: Version 1 of NV_MOSAIC_GRID_TOPO_DISPLAY structure
    {
        public UInt32 DisplayId;              //!< DisplayID of the display
        public Int32 OverlapX;             //!< (+overlap, -gap)
        public Int32 OverlapY;                //!< (+overlap, -gap)
        public NV_ROTATE Rotation;                //!< Rotation of display
        public UInt32 CloneGroup;                //!< Reserved, must be 0

        public override bool Equals(object obj) => obj is NV_MOSAIC_GRID_TOPO_DISPLAY_V1 other && this.Equals(other);
        public bool Equals(NV_MOSAIC_GRID_TOPO_DISPLAY_V1 other)
        => DisplayId == other.DisplayId &&
           OverlapX == other.OverlapX &&
           OverlapY == other.OverlapY &&
           Rotation == other.Rotation &&
           CloneGroup == other.CloneGroup;

        public override Int32 GetHashCode()
        {
            return (DisplayId, OverlapX, OverlapY, Rotation, CloneGroup).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_GRID_TOPO_DISPLAY_V1 lhs, NV_MOSAIC_GRID_TOPO_DISPLAY_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_GRID_TOPO_DISPLAY_V1 lhs, NV_MOSAIC_GRID_TOPO_DISPLAY_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_GRID_TOPO_DISPLAY_V1 other = (NV_MOSAIC_GRID_TOPO_DISPLAY_V1)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_GRID_TOPO_DISPLAY_V2 : IEquatable<NV_MOSAIC_GRID_TOPO_DISPLAY_V2>, ICloneable // Note: Version 2 of NV_MOSAIC_GRID_TOPO_DISPLAY structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 2
        public UInt32 DisplayId;              //!< DisplayID of the display
        public Int32 OverlapX;             //!< (+overlap, -gap)
        public Int32 OverlapY;                //!< (+overlap, -gap)
        public NV_ROTATE Rotation;                //!< Rotation of display
        public UInt32 CloneGroup;                //!< Reserved, must be 0
        public NV_PIXEL_SHIFT_TYPE PixelShiftType;  //!< Type of the pixel shift enabled display

        public override bool Equals(object obj) => obj is NV_MOSAIC_GRID_TOPO_DISPLAY_V2 other && this.Equals(other);
        public bool Equals(NV_MOSAIC_GRID_TOPO_DISPLAY_V2 other)
        => Version == other.Version &&
           DisplayId == other.DisplayId &&
           OverlapX == other.OverlapX &&
           OverlapY == other.OverlapY &&
           Rotation == other.Rotation &&
           CloneGroup == other.CloneGroup &&
           PixelShiftType == other.PixelShiftType;

        public override Int32 GetHashCode()
        {
            return (Version, DisplayId, OverlapX, OverlapY, Rotation, CloneGroup, PixelShiftType).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_GRID_TOPO_DISPLAY_V2 lhs, NV_MOSAIC_GRID_TOPO_DISPLAY_V2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_GRID_TOPO_DISPLAY_V2 lhs, NV_MOSAIC_GRID_TOPO_DISPLAY_V2 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_GRID_TOPO_DISPLAY_V2 other = (NV_MOSAIC_GRID_TOPO_DISPLAY_V2)MemberwiseClone();
            return other;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_SUPPORTED_TOPO_INFO_V1 : IEquatable<NV_MOSAIC_SUPPORTED_TOPO_INFO_V1>, ICloneable // Note: Version 1 of NV_MOSAIC_SUPPORTED_TOPO_INFO structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 1
        public UInt32 TopoBriefsCount;              //!< Number of topologies in below array
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)NVImport.NV_MOSAIC_TOPO_MAX)]
        public NV_MOSAIC_TOPO_BRIEF[] TopoBriefs;             //!< List of supported topologies with only brief details
        public Int32 DisplaySettingsCount;                //!< Number of display settings in below array
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)NVImport.NV_MOSAIC_DISPLAY_SETTINGS_MAX)]
        public NV_MOSAIC_DISPLAY_SETTING_V1[] DisplaySettings;                //!< List of per display settings possible

        public override bool Equals(object obj) => obj is NV_MOSAIC_SUPPORTED_TOPO_INFO_V1 other && this.Equals(other);

        public bool Equals(NV_MOSAIC_SUPPORTED_TOPO_INFO_V1 other)
        => Version == other.Version &&
           TopoBriefsCount == other.TopoBriefsCount &&
           TopoBriefs.SequenceEqual(other.TopoBriefs) &&
           DisplaySettingsCount == other.DisplaySettingsCount &&
           DisplaySettings.SequenceEqual(other.DisplaySettings);

        public override Int32 GetHashCode()
        {
            return (Version, TopoBriefsCount, TopoBriefs, DisplaySettingsCount, DisplaySettings).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_SUPPORTED_TOPO_INFO_V1 lhs, NV_MOSAIC_SUPPORTED_TOPO_INFO_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_SUPPORTED_TOPO_INFO_V1 lhs, NV_MOSAIC_SUPPORTED_TOPO_INFO_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_SUPPORTED_TOPO_INFO_V1 other = (NV_MOSAIC_SUPPORTED_TOPO_INFO_V1)MemberwiseClone();
            other.TopoBriefs = new NV_MOSAIC_TOPO_BRIEF[TopoBriefs.Length];
            for (int x = 0; x < (int)TopoBriefs.Length; x++)
            {
                other.TopoBriefs[x] = (NV_MOSAIC_TOPO_BRIEF)TopoBriefs[x].Clone();
            }
            other.DisplaySettings = new NV_MOSAIC_DISPLAY_SETTING_V1[DisplaySettings.Length];
            for (int x = 0; x < (int)DisplaySettings.Length; x++)
            {
                other.DisplaySettings[x] = (NV_MOSAIC_DISPLAY_SETTING_V1)DisplaySettings[x].Clone();
            }
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 : IEquatable<NV_MOSAIC_SUPPORTED_TOPO_INFO_V2>, ICloneable // Note: Version 2 of NV_MOSAIC_SUPPORTED_TOPO_INFO structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 2
        public UInt32 TopoBriefsCount;              //!< Number of topologies in below array
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)NVImport.NV_MOSAIC_TOPO_MAX)]
        public NV_MOSAIC_TOPO_BRIEF[] TopoBriefs;             //!< List of supported topologies with only brief details
        public Int32 DisplaySettingsCount;                //!< Number of display settings in below array
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)NVImport.NV_MOSAIC_DISPLAY_SETTINGS_MAX)]
        public NV_MOSAIC_DISPLAY_SETTING_V2[] DisplaySettings;                //!< List of per display settings possible

        public override bool Equals(object obj) => obj is NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 other && this.Equals(other);
        public bool Equals(NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 other)
        => Version == other.Version &&
           TopoBriefsCount == other.TopoBriefsCount &&
           TopoBriefs.SequenceEqual(other.TopoBriefs) &&
           DisplaySettingsCount == other.DisplaySettingsCount &&
           DisplaySettings.SequenceEqual(other.DisplaySettings);

        public override Int32 GetHashCode()
        {
            return (Version, TopoBriefsCount, TopoBriefs, DisplaySettingsCount, DisplaySettings).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 lhs, NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 lhs, NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 other = (NV_MOSAIC_SUPPORTED_TOPO_INFO_V2)MemberwiseClone();
            other.TopoBriefs = new NV_MOSAIC_TOPO_BRIEF[TopoBriefs.Length];
            for (int x = 0; x < (int)TopoBriefs.Length; x++)
            {
                other.TopoBriefs[x] = (NV_MOSAIC_TOPO_BRIEF)TopoBriefs[x].Clone();
            }
            other.DisplaySettings = new NV_MOSAIC_DISPLAY_SETTING_V2[DisplaySettings.Length];
            for (int x = 0; x < (int)DisplaySettings.Length; x++)
            {
                other.DisplaySettings[x] = (NV_MOSAIC_DISPLAY_SETTING_V2)DisplaySettings[x].Clone();
            }
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_GPU_DISPLAYIDS_V2 : IEquatable<NV_GPU_DISPLAYIDS_V2>, ICloneable // Note: Version 2 of NV_GPU_DISPLAYIDS_V2 structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 2 (NOTE R470 contains a bug, and sets this to 3!)
        public NV_MONITOR_CONN_TYPE ConnectorType;              //!< out: vga, tv, dvi, hdmi and dp.This is reserved for future use and clients should not rely on this information.Instead get the
                                                                //!< GPU connector type from NvAPI_GPU_GetConnectorInfo/NvAPI_GPU_GetConnectorInfoEx
        public UInt32 DisplayId;             //!< this is a unique identifier for each device
        public UInt32 Flags;

        public override bool Equals(object obj) => obj is NV_GPU_DISPLAYIDS_V2 other && this.Equals(other);

        public bool Equals(NV_GPU_DISPLAYIDS_V2 other)
        => Version == other.Version &&
           ConnectorType == other.ConnectorType &&
           DisplayId == other.DisplayId &&
           Flags == other.Flags;

        public bool IsDynamic => (Flags & 0x1) == 0x1; //!< if bit is set then this display is part of MST topology and it's a dynamic
        public bool IsMultiStreamRootNode => (Flags & 0x2) == 0x2; //!< if bit is set then this displayID belongs to a multi stream enabled connector(root node). Note that when multi stream is enabled and
                                                                   //!< a single multi stream capable monitor is connected to it, the monitor will share the display id with the RootNode.
                                                                   //!< When there is more than one monitor connected in a multi stream topology, then the root node will have a separate displayId.
        public bool IsActive => (Flags & 0x4) == 0x4; //!< if bit is set then this display is being actively driven
        public bool IsCluster => (Flags & 0x8) == 0x8; //!< if bit is set then this display is the representative display
        public bool isOSVisible => (Flags & 0x10) == 0x10; //!< if bit is set, then this display is reported to the OS
        public bool isWFD => (Flags & 0x20) == 0x20; //!< if bit is set, then this display is wireless
        public bool isConnected => (Flags & 0x40) == 0x40; //!< if bit is set, then this display is connected
        public bool isPhysicallyConnected => (Flags & 0x20000) == 0x20000; //!< if bit is set, then this display is a physically connected display; Valid only when isConnected bit is set 

        public override Int32 GetHashCode()
        {
            return (Version, ConnectorType, DisplayId, Flags).GetHashCode();
        }
        public static bool operator ==(NV_GPU_DISPLAYIDS_V2 lhs, NV_GPU_DISPLAYIDS_V2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_GPU_DISPLAYIDS_V2 lhs, NV_GPU_DISPLAYIDS_V2 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_GPU_DISPLAYIDS_V2 other = (NV_GPU_DISPLAYIDS_V2)MemberwiseClone();
            return other;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_DISPLAY_TOPO_STATUS_V1 : IEquatable<NV_MOSAIC_DISPLAY_TOPO_STATUS_V1>, ICloneable // Note: Version 1 of NV_MOSAIC_DISPLAY_TOPO_STATUS_V1 structure
    {
        public UInt32 Version;
        public NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS ErrorFlags;            //!< (OUT) Any of the NV_MOSAIC_DISPLAYTOPO_ERROR_* flags.
        public NV_MOSAIC_DISPLAYTOPO_WARNING_FLAGS WarningFlags;          //!< (OUT) Any of the NV_MOSAIC_DISPLAYTOPO_WARNING_* flags.
        public UInt32 DisplayCount;          //!< (OUT) The number of valid entries in the displays array.
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (Int32)NVImport.NV_MAX_DISPLAYS)]
        public NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY[] Displays;    // displays[NV_MAX_DISPLAYS] array

        public override bool Equals(object obj) => obj is NV_MOSAIC_DISPLAY_TOPO_STATUS_V1 other && this.Equals(other);

        public bool Equals(NV_MOSAIC_DISPLAY_TOPO_STATUS_V1 other)
        => Version == other.Version &&
           ErrorFlags == other.ErrorFlags &&
           WarningFlags == other.WarningFlags &&
           DisplayCount == other.DisplayCount &&
           Displays.SequenceEqual(other.Displays);

        public override Int32 GetHashCode()
        {
            return (Version, ErrorFlags, WarningFlags, DisplayCount, Displays).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_DISPLAY_TOPO_STATUS_V1 lhs, NV_MOSAIC_DISPLAY_TOPO_STATUS_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_DISPLAY_TOPO_STATUS_V1 lhs, NV_MOSAIC_DISPLAY_TOPO_STATUS_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_DISPLAY_TOPO_STATUS_V1 other = (NV_MOSAIC_DISPLAY_TOPO_STATUS_V1)MemberwiseClone();
            other.Displays = new NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY[Displays.Length];
            for (int x = 0; x < (int)Displays.Length; x++)
            {
                other.Displays[x] = (NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY)Displays[x].Clone();
            }
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY : IEquatable<NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY>, ICloneable
    {
        public UInt32 DisplayId;             //!< (OUT) The DisplayID of this display.
        public NV_MOSAIC_DISPLAYCAPS_PROBLEM_FLAGS ErrorFlags;            //!< (OUT) Any of the NV_MOSAIC_DISPLAYCAPS_PROBLEM_* flags.
        public NV_MOSAIC_DISPLAYTOPO_WARNING_FLAGS WarningFlags;          //!< (OUT) Any of the NV_MOSAIC_DISPLAYTOPO_WARNING_* flags.
        public UInt32 GeneralFlags;
        public bool SupportsRotation => (GeneralFlags & 0x1) == 0x1; //!< (OUT) This display can be rotated

        public override bool Equals(object obj) => obj is NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY other && this.Equals(other);

        public bool Equals(NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY other)
        => DisplayId == other.DisplayId &&
           ErrorFlags == other.ErrorFlags &&
           WarningFlags == other.WarningFlags &&
           GeneralFlags == other.GeneralFlags;

        public override Int32 GetHashCode()
        {
            return (DisplayId, ErrorFlags, WarningFlags, GeneralFlags).GetHashCode();
        }
        public static bool operator ==(NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY lhs, NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY lhs, NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY other = (NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY)MemberwiseClone();
            return other;
        }
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_HDR_CAPABILITIES_V2 : IEquatable<NV_HDR_CAPABILITIES_V2>, ICloneable // Note: Version 2 of NV_HDR_CAPABILITIES structure
    {
        public UInt32 Version;            // Version of this structure - MUST BE SET TO 2
        public NV_HDR_CAPABILITIES_V2_FLAGS SupportFlags;              //!< Various flags indicating HDR support 
        public NV_STATIC_METADATA_DESCRIPTOR_ID StaticMetadataDescriptorId; ////!< Static Metadata Descriptor Id (0 for static metadata type 1)
        public NV_HDR_CAPABILITIES_DISPLAY_DATA DisplayData;
        public NV_HDR_DV_STATIC_METADATA DvStaticMetadata;

        public bool DriverExpandDefaultHdrParameters => SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.DriverExpandDefaultHdrParameters);
        public bool IsDolbyVisionSupported => SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsDolbyVisionSupported);
        public bool IsEdrSupported => SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsEdrSupported);
        public bool IsST2084EotfSupported => SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsST2084EotfSupported);
        public bool IsTraditionalHdrGammaSupported => SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsTraditionalHdrGammaSupported);
        public bool IsTraditionalSdrGammaSupported => SupportFlags.HasFlag(NV_HDR_CAPABILITIES_V2_FLAGS.IsTraditionalSdrGammaSupported);

        public override bool Equals(object obj) => obj is NV_HDR_CAPABILITIES_V2 other && this.Equals(other);

        public bool Equals(NV_HDR_CAPABILITIES_V2 other)
        => Version == other.Version &&
           SupportFlags == other.SupportFlags &&
           StaticMetadataDescriptorId == other.StaticMetadataDescriptorId &&
           DisplayData.Equals(other.DisplayData) &&
           DvStaticMetadata.Equals(other.DvStaticMetadata);

        public override Int32 GetHashCode()
        {
            return (Version, SupportFlags, StaticMetadataDescriptorId, DisplayData, DvStaticMetadata).GetHashCode();
        }
        public static bool operator ==(NV_HDR_CAPABILITIES_V2 lhs, NV_HDR_CAPABILITIES_V2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_HDR_CAPABILITIES_V2 lhs, NV_HDR_CAPABILITIES_V2 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_POSITION other = (NV_POSITION)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_HDR_DV_STATIC_METADATA : IEquatable<NV_HDR_DV_STATIC_METADATA>, ICloneable
    {
        public UInt32 Flags;
        public UInt16 TargetMinLuminance;
        public UInt16 TargetMaxLuminance;
        public UInt16 CCRedX;
        public UInt16 CCRedY;
        public UInt16 CCGreenX;
        public UInt16 CCGreenY;
        public UInt16 CCBlueX;
        public UInt16 CCBlueY;
        public UInt16 CCWhiteX;
        public UInt16 CCWhiteY;

        public override bool Equals(object obj) => obj is NV_HDR_DV_STATIC_METADATA other && this.Equals(other);
        public bool Equals(NV_HDR_DV_STATIC_METADATA other)
        => Flags == other.Flags &&
           TargetMinLuminance == other.TargetMinLuminance &&
           TargetMaxLuminance == other.TargetMaxLuminance &&
           CCRedX == other.CCRedX &&
            CCRedY == other.CCRedY &&
           CCGreenX == other.CCGreenX &&
           CCGreenY == other.CCGreenY &&
            CCBlueX == other.CCBlueX &&
            CCBlueY == other.CCBlueY &&
            CCWhiteX == other.CCWhiteX &&
            CCWhiteY == other.CCWhiteY;

        public override Int32 GetHashCode()
        {
            return (Flags, TargetMinLuminance, TargetMaxLuminance, CCRedX, CCRedY, CCGreenX, CCGreenY, CCBlueX,
                    CCBlueY, CCWhiteX, CCWhiteY).GetHashCode();
        }
        public static bool operator ==(NV_HDR_DV_STATIC_METADATA lhs, NV_HDR_DV_STATIC_METADATA rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_HDR_DV_STATIC_METADATA lhs, NV_HDR_DV_STATIC_METADATA rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_POSITION other = (NV_POSITION)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_HDR_CAPABILITIES_DISPLAY_DATA : IEquatable<NV_HDR_CAPABILITIES_DISPLAY_DATA>, ICloneable
    {
        public UInt16 DisplayPrimaryX0;
        public UInt16 DisplayPrimaryY0;
        public UInt16 DisplayPrimaryX1;
        public UInt16 DisplayPrimaryY1;
        public UInt16 DisplayPrimaryX2;
        public UInt16 DisplayPrimaryY2;
        public UInt16 DisplayWhitePointX;
        public UInt16 DisplayWhitePointY;
        public UInt16 DesiredContentMaxLuminance;
        public UInt16 DesiredContentMinLuminance;
        public UInt16 DesiredContentMaxFrameAverageLuminance;

        public override bool Equals(object obj) => obj is NV_HDR_CAPABILITIES_DISPLAY_DATA other && this.Equals(other);
        public bool Equals(NV_HDR_CAPABILITIES_DISPLAY_DATA other)
        => DisplayPrimaryX0 == other.DisplayPrimaryX0 &&
           DisplayPrimaryY0 == other.DisplayPrimaryY0 &&
           DisplayPrimaryX1 == other.DisplayPrimaryX1 &&
           DisplayPrimaryY1 == other.DisplayPrimaryY1 &&
           DisplayPrimaryX2 == other.DisplayPrimaryX2 &&
           DisplayPrimaryY2 == other.DisplayPrimaryY2 &&
            DisplayWhitePointX == other.DisplayWhitePointX &&
            DisplayWhitePointY == other.DisplayWhitePointY &&
            DesiredContentMaxLuminance == other.DesiredContentMaxLuminance &&
            DesiredContentMinLuminance == other.DesiredContentMinLuminance &&
            DesiredContentMaxFrameAverageLuminance == other.DesiredContentMaxFrameAverageLuminance;

        public override Int32 GetHashCode()
        {
            return (DisplayPrimaryX0, DisplayPrimaryY0, DisplayPrimaryX1, DisplayPrimaryY1, DisplayPrimaryX2, DisplayPrimaryY2, DisplayWhitePointX, DisplayWhitePointY,
                    DisplayWhitePointX, DisplayWhitePointY, DesiredContentMaxLuminance, DesiredContentMinLuminance, DesiredContentMaxFrameAverageLuminance).GetHashCode();
        }
        public static bool operator ==(NV_HDR_CAPABILITIES_DISPLAY_DATA lhs, NV_HDR_CAPABILITIES_DISPLAY_DATA rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_HDR_CAPABILITIES_DISPLAY_DATA lhs, NV_HDR_CAPABILITIES_DISPLAY_DATA rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_POSITION other = (NV_POSITION)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_HDR_COLOR_DATA_V2 : IEquatable<NV_HDR_COLOR_DATA_V2>, ICloneable
    {
        public UInt32 Version;                                 //!< Version of this structure
        public NV_HDR_CMD Cmd;                                     //!< Command get/set
        public NV_HDR_MODE HdrMode;                                 //!< HDR mode
        public NV_STATIC_METADATA_DESCRIPTOR_ID StaticMetadataDescriptorId;           //!< Static Metadata Descriptor Id (0 for static metadata type 1)
        public NV_HDR_COLOR_DISPLAY_DATA MasteringDisplayData; //!< Static Metadata Descriptor Type 1, CEA-861.3, SMPTE ST2086
        public NV_HDR_COLOR_FORMAT HdrColorFormat;                                     //!< Optional, One of NV_COLOR_FORMAT enum values, if set it will apply requested color format for HDR session
        public NV_HDR_DYNAMIC_RANGE HdrDynamicRange;                                    //!< Optional, One of NV_DYNAMIC_RANGE enum values, if set it will apply requested dynamic range for HDR session
        public NV_BPC HdrBpc;                                             //!< Optional, One of NV_BPC enum values, if set it will apply requested color depth
                                                                          //!< Dolby Vision mode: DV supports specific combinations of colorformat, dynamic range and bpc. Please refer Dolby Vision specification.
                                                                          //!<                    If invalid or no combination is passed driver will force default combination of RGB format + full range + 8bpc.
                                                                          //!< HDR mode: These fields are ignored in hdr mode

        public override bool Equals(object obj) => obj is NV_HDR_COLOR_DATA_V2 other && this.Equals(other);
        public bool Equals(NV_HDR_COLOR_DATA_V2 other)
            => Version == other.Version &&
               Cmd == other.Cmd &&
               HdrMode == other.HdrMode &&
               StaticMetadataDescriptorId == other.StaticMetadataDescriptorId &&
               MasteringDisplayData.Equals(other.MasteringDisplayData) &&
               HdrColorFormat == other.HdrColorFormat &&
                HdrDynamicRange == other.HdrDynamicRange &&
                HdrBpc == other.HdrBpc;

        public override Int32 GetHashCode()
        {
            return (Version, Cmd, HdrMode, StaticMetadataDescriptorId, MasteringDisplayData, HdrColorFormat, HdrDynamicRange, HdrBpc).GetHashCode();
        }
        public static bool operator ==(NV_HDR_COLOR_DATA_V2 lhs, NV_HDR_COLOR_DATA_V2 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_HDR_COLOR_DATA_V2 lhs, NV_HDR_COLOR_DATA_V2 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_POSITION other = (NV_POSITION)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_HDR_COLOR_DISPLAY_DATA : IEquatable<NV_HDR_COLOR_DISPLAY_DATA>, ICloneable
    {
        public UInt16 DisplayPrimaryX0;
        public UInt16 DisplayPrimaryY0;
        public UInt16 DisplayPrimaryX1;
        public UInt16 DisplayPrimaryY1;
        public UInt16 DisplayPrimaryX2;
        public UInt16 DisplayPrimaryY2;
        public UInt16 DisplayWhitePointX;
        public UInt16 DisplayWhitePointY;
        public UInt16 MaxDisplayMasteringLuminance;
        public UInt16 MinDisplayMasteringLuminance;
        public UInt16 MaxContentLightLevel;
        public UInt16 MaxFrameAverageLightLevel;

        public override bool Equals(object obj) => obj is NV_HDR_COLOR_DISPLAY_DATA other && this.Equals(other);
        public bool Equals(NV_HDR_COLOR_DISPLAY_DATA other)
        => DisplayPrimaryX0 == other.DisplayPrimaryX0 &&
           DisplayPrimaryY0 == other.DisplayPrimaryY0 &&
           DisplayPrimaryX1 == other.DisplayPrimaryX1 &&
           DisplayPrimaryY1 == other.DisplayPrimaryY1 &&
           DisplayPrimaryX2 == other.DisplayPrimaryX2 &&
           DisplayPrimaryY2 == other.DisplayPrimaryY2 &&
            DisplayWhitePointX == other.DisplayWhitePointX &&
            DisplayWhitePointY == other.DisplayWhitePointY &&
            MaxDisplayMasteringLuminance == other.MaxDisplayMasteringLuminance &&
            MinDisplayMasteringLuminance == other.MinDisplayMasteringLuminance &&
            MaxContentLightLevel == other.MaxContentLightLevel &&
            MaxFrameAverageLightLevel == other.MaxFrameAverageLightLevel;

        public override Int32 GetHashCode()
        {
            return (DisplayPrimaryX0, DisplayPrimaryY0, DisplayPrimaryX1, DisplayPrimaryY1, DisplayPrimaryX2, DisplayPrimaryY2, DisplayWhitePointX, DisplayWhitePointY,
                    DisplayWhitePointX, DisplayWhitePointY, MaxDisplayMasteringLuminance, MinDisplayMasteringLuminance, MaxContentLightLevel, MaxFrameAverageLightLevel).GetHashCode();
        }
        public static bool operator ==(NV_HDR_COLOR_DISPLAY_DATA lhs, NV_HDR_COLOR_DISPLAY_DATA rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_HDR_COLOR_DISPLAY_DATA lhs, NV_HDR_COLOR_DISPLAY_DATA rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_POSITION other = (NV_POSITION)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_COLOR_DATA_V5 : IEquatable<NV_COLOR_DATA_V5>, ICloneable
    {
        public UInt32 Version; //!< Version of this structure
        public UInt16 Size;    //!< Size of this structure
        public NV_COLOR_CMD Cmd;
        public NV_COLOR_FORMAT ColorFormat;          //!< One of NV_COLOR_FORMAT enum values.
        public NV_COLOR_COLORIMETRY Colorimetry;          //!< One of NV_COLOR_COLORIMETRY enum values.
        public NV_DYNAMIC_RANGE DynamicRange;         //!< One of NV_DYNAMIC_RANGE enum values.
        public NV_BPC Bpc;                  //!< One of NV_BPC enum values.
        public NV_COLOR_SELECTION_POLICY ColorSelectionPolicy; //!< One of the color selection policy
        public NV_DESKTOP_COLOR_DEPTH Depth;                //!< One of NV_DESKTOP_COLOR_DEPTH enum values.    

        public override bool Equals(object obj) => obj is NV_COLOR_DATA_V5 other && this.Equals(other);
        public bool Equals(NV_COLOR_DATA_V5 other)
        => Version == other.Version &&
           Size == other.Size &&
           Cmd == other.Cmd &&
           ColorFormat == other.ColorFormat &&
           Colorimetry == other.Colorimetry &&
           DynamicRange == other.DynamicRange &&
            Bpc == other.Bpc &&
            ColorSelectionPolicy == other.ColorSelectionPolicy &&
            Depth == other.Depth;

        public override Int32 GetHashCode()
        {
            return (Version, Size, Cmd, ColorFormat, Colorimetry, DynamicRange, Bpc, ColorSelectionPolicy, Depth).GetHashCode();
        }
        public static bool operator ==(NV_COLOR_DATA_V5 lhs, NV_COLOR_DATA_V5 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_COLOR_DATA_V5 lhs, NV_COLOR_DATA_V5 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_POSITION other = (NV_POSITION)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_CUSTOM_DISPLAY_V1 : IEquatable<NV_CUSTOM_DISPLAY_V1>, ICloneable
    {
        public UInt32 Version; //!< Version of this structure
        public UInt32 Width; //!< Source surface(source mode) width
        public UInt32 Height;            //!< Source surface(source mode) height
        public UInt32 Depth;             //!< Source surface color depth."0" means all 8/16/32bpp
        public NV_FORMAT ColorFormat;       //!< Color format (optional)
        public NV_VIEWPORTF SourcePartition;      //!< For multimon support, should be set to (0,0,1.0,1.0) for now.
        public float XRatio;            //!< Horizontal scaling ratio
        public float YRatio;            //!< Vertical scaling ratio
        public NV_TIMING Timing;            //!< Timing used to program TMDS/DAC/LVDS/HDMI/TVEncoder, etc.
        public UInt32 Flags; //!< If set to 1, it means a hardware modeset without OS update

        //     Gets a boolean value indicating that a hardware mode-set without OS update should be performed.
        public bool IsHardwareModeSetOnly => Flags.GetBit(0);

        public override bool Equals(object obj) => obj is NV_CUSTOM_DISPLAY_V1 other && this.Equals(other);
        public bool Equals(NV_CUSTOM_DISPLAY_V1 other)
        => Version == other.Version &&
           Width == other.Width &&
           Height == other.Height &&
           Depth == other.Depth &&
           ColorFormat == other.ColorFormat &&
           SourcePartition == other.SourcePartition &&
            XRatio == other.XRatio &&
            YRatio == other.YRatio &&
            Timing == other.Timing &&
            Flags == other.Flags;

        public override Int32 GetHashCode()
        {
            return (Version, Width, Height, Depth, ColorFormat, SourcePartition, XRatio, YRatio, Timing, Flags).GetHashCode();
        }
        public static bool operator ==(NV_CUSTOM_DISPLAY_V1 lhs, NV_CUSTOM_DISPLAY_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_CUSTOM_DISPLAY_V1 lhs, NV_CUSTOM_DISPLAY_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_POSITION other = (NV_POSITION)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_GET_ADAPTIVE_SYNC_DATA_V1 : IEquatable<NV_GET_ADAPTIVE_SYNC_DATA_V1>, ICloneable
    {
        public UInt32 Version; // Must be V1
        public UInt32 MaxFrameInterval;             //!< maximum frame interval in micro seconds as set previously using NvAPI_DISP_SetAdaptiveSyncData function. If default values from EDID are used, this parameter returns 0.
        public UInt32 Flags;
        public UInt32 LastFlipRefreshCount;             //!< Number of times the last flip was shown on the screen
        public UInt64 LastFlipTimeStamp;             //!< Timestamp for the lastest flip on the screen
        public UInt32 ReservedEx1;
        public UInt32 ReservedEx2;
        public UInt32 ReservedEx3;
        public UInt32 ReservedEx4;

        public bool DisableAdaptiveSync => (Flags & 0x1) == 0x1; //!< Indicates if adaptive sync is disabled on the display.
        public bool DisableFrameSplitting => (Flags & 0x1) == 0x1; //!< Indicates if frame splitting is disabled on the display.

        public override bool Equals(object obj) => obj is NV_GET_ADAPTIVE_SYNC_DATA_V1 other && this.Equals(other);

        public bool Equals(NV_GET_ADAPTIVE_SYNC_DATA_V1 other)
        => MaxFrameInterval == other.MaxFrameInterval &&
           Flags == other.Flags &&
           LastFlipRefreshCount == other.LastFlipRefreshCount &&
           LastFlipTimeStamp == other.LastFlipTimeStamp;

        public override Int32 GetHashCode()
        {
            return (MaxFrameInterval, Flags, LastFlipRefreshCount, LastFlipTimeStamp).GetHashCode();
        }
        public static bool operator ==(NV_GET_ADAPTIVE_SYNC_DATA_V1 lhs, NV_GET_ADAPTIVE_SYNC_DATA_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_GET_ADAPTIVE_SYNC_DATA_V1 lhs, NV_GET_ADAPTIVE_SYNC_DATA_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_GET_ADAPTIVE_SYNC_DATA_V1 other = (NV_GET_ADAPTIVE_SYNC_DATA_V1)MemberwiseClone();
            return other;
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NV_SET_ADAPTIVE_SYNC_DATA_V1 : IEquatable<NV_SET_ADAPTIVE_SYNC_DATA_V1>, ICloneable
    {
        public UInt32 Version; // Must be V1
        public UInt32 MaxFrameInterval;             //!< maximum frame interval in micro seconds as set previously using NvAPI_DISP_SetAdaptiveSyncData function. If default values from EDID are used, this parameter returns 0.
        public UInt32 Flags;
        public UInt32 ReservedEx1;             //!< Number of times the last flip was shown on the screen
        public UInt64 ReservedEx2;             //!< Timestamp for the lastest flip on the screen
        public UInt32 ReservedEx3;
        public UInt32 ReservedEx4;
        public UInt32 ReservedEx5;
        public UInt32 ReservedEx6;
        public UInt32 ReservedEx7;

        public bool DisableAdaptiveSync => (Flags & 0x1) == 0x1; //!< Indicates if adaptive sync is disabled on the display.
        public bool DisableFrameSplitting => (Flags & 0x1) == 0x1; //!< Indicates if frame splitting is disabled on the display.

        public override bool Equals(object obj) => obj is NV_SET_ADAPTIVE_SYNC_DATA_V1 other && this.Equals(other);

        public bool Equals(NV_SET_ADAPTIVE_SYNC_DATA_V1 other)
        => MaxFrameInterval == other.MaxFrameInterval &&
            Flags == other.Flags;

        public override Int32 GetHashCode()
        {
            return (MaxFrameInterval, Flags).GetHashCode();
        }
        public static bool operator ==(NV_SET_ADAPTIVE_SYNC_DATA_V1 lhs, NV_SET_ADAPTIVE_SYNC_DATA_V1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(NV_SET_ADAPTIVE_SYNC_DATA_V1 lhs, NV_SET_ADAPTIVE_SYNC_DATA_V1 rhs) => !(lhs == rhs);
        public object Clone()
        {
            NV_SET_ADAPTIVE_SYNC_DATA_V1 other = (NV_SET_ADAPTIVE_SYNC_DATA_V1)MemberwiseClone();
            return other;
        }
    }

    // ==================================
    // NVImport Class
    // ==================================

    static class NVImport
    {

        public const UInt32 NV_MAX_HEADS = 4;
        public const UInt32 NV_MAX_VID_PROFILES = 4;
        public const UInt32 NV_MAX_VID_STREAMS = 4;
        public const UInt32 NV_ADVANCED_DISPLAY_HEADS = 4;
        public const UInt32 NV_GENERIC_STRING_MAX = 4096;
        public const UInt32 NV_LONG_STRING_MAX = 256;
        public const UInt32 NV_MAX_ACPI_IDS = 16;
        public const UInt32 NV_MAX_AUDIO_DEVICES = 16;
        public const UInt32 NV_MAX_AVAILABLE_CPU_TOPOLOGIES = 256;
        public const UInt32 NV_MAX_AVAILABLE_SLI_GROUPS = 256;
        public const UInt32 NV_MAX_AVAILABLE_DISPLAY_HEADS = 2;
        public const UInt32 NV_MAX_DISPLAYS = NV_MAX_PHYSICAL_GPUS * NV_ADVANCED_DISPLAY_HEADS;
        public const UInt32 NV_MAX_GPU_PER_TOPOLOGY = 8;
        public const UInt32 NV_MAX_GPU_TOPOLOGIES = NV_MAX_PHYSICAL_GPUS;
        public const UInt32 NV_MAX_HEADS_PER_GPU = 32;
        public const UInt32 NV_MAX_LOGICAL_GPUS = 64;
        public const UInt32 NV_MAX_PHYSICAL_BRIDGES = 100;
        public const UInt32 NV_MAX_PHYSICAL_GPUS = 64;
        public const UInt32 NV_MAX_VIEW_MODES = 8;
        public const UInt32 NV_PHYSICAL_GPUS = 32;
        public const UInt32 NV_SHORT_STRING_MAX = 64;
        public const UInt32 NV_SYSTEM_HWBC_INVALID_ID = 0xffffffff;
        public const UInt32 NV_SYSTEM_MAX_DISPLAYS = NV_MAX_PHYSICAL_GPUS * NV_MAX_HEADS;
        public const UInt32 NV_SYSTEM_MAX_HWBCS = 128;
        public const UInt32 NV_MOSAIC_MAX_DISPLAYS = 64;
        public const UInt32 NV_MOSAIC_DISPLAY_SETTINGS_MAX = 40;
        public const UInt32 NV_MOSAIC_TOPO_IDX_DEFAULT = 0;
        public const UInt32 NV_MOSAIC_TOPO_IDX_LEFT_EYE = 0;
        public const UInt32 NV_MOSAIC_TOPO_IDX_RIGHT_EYE = 1;
        public const UInt32 NV_MOSAIC_TOPO_NUM_EYES = 2;
        public const UInt32 NV_MOSAIC_TOPO_MAX = (UInt32)NV_MOSAIC_TOPO.TOPO_MAX;
        public const UInt32 NVAPI_MAX_MOSAIC_DISPLAY_ROWS = 8;
        public const UInt32 NVAPI_MAX_MOSAIC_DISPLAY_COLUMNS = 8;
        public const UInt32 NVAPI_MAX_MOSAIC_TOPOS = 16;
        public const UInt32 NVAPI_GENERIC_STRING_MAX = 4096;
        public const UInt32 NVAPI_LONG_STRING_MAX = 256;
        public const UInt32 NVAPI_SHORT_STRING_MAX = 64;
        public const UInt32 NVAPI_MAX_PHYSICAL_GPUS = 64;
        public const UInt32 NVAPI_MAX_PHYSICAL_GPUS_QUERIED = 32;
        public const UInt32 NVAPI_UNICODE_STRING_MAX = 2048;
        public const UInt32 NVAPI_BINARY_DATA_MAX = 4096;
        public const UInt32 NVAPI_SETTING_MAX_VALUES = 100;
        public const UInt32 NV_EDID_DATA_SIZE = 256;

        //
        //! This defines the maximum number of topos that can be in a topo group.
        //! At this time, it is set to 2 because our largest topo group (passive
        //! stereo) only needs 2 topos (left eye and right eye).
        //!
        //! If a new topo group with more than 2 topos is added above, then this
        //! number will also have to be incremented.
        public const UInt32 NV_MOSAIC_MAX_TOPO_PER_TOPO_GROUP = 2;

        // Version Constants
        public static UInt32 NV_MOSAIC_TOPO_BRIEF_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_TOPO_BRIEF>(1);
        public static UInt32 NV_MOSAIC_DISPLAY_SETTING_V1_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_DISPLAY_SETTING_V1>(1);
        public static UInt32 NV_MOSAIC_DISPLAY_SETTING_V2_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_DISPLAY_SETTING_V2>(2);
        public static UInt32 NV_MOSAIC_TOPO_GROUP_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_TOPO_GROUP>(1);
        public static UInt32 NV_MOSAIC_TOPO_DETAILS_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_TOPO_DETAILS>(1);
        public static UInt32 NV_MOSAIC_GRID_TOPO_V1_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_GRID_TOPO_V1>(1);
        public static UInt32 NV_MOSAIC_GRID_TOPO_V2_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_GRID_TOPO_V2>(2);
        public static UInt32 NV_MOSAIC_GRID_TOPO_DISPLAY_V1_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_GRID_TOPO_DISPLAY_V1>(1);
        public static UInt32 NV_MOSAIC_GRID_TOPO_DISPLAY_V2_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_GRID_TOPO_DISPLAY_V2>(2);
        public static UInt32 NV_MOSAIC_SUPPORTED_TOPO_INFO_V1_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_SUPPORTED_TOPO_INFO_V1>(1);
        public static UInt32 NV_MOSAIC_SUPPORTED_TOPO_INFO_V2_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_SUPPORTED_TOPO_INFO_V2>(2);
        public static UInt32 NV_HDR_COLOR_DATA_V2_VER = MAKE_NVAPI_VERSION<NV_HDR_COLOR_DATA_V2>(2);
        public static UInt32 NV_COLOR_DATA_V5_VER = MAKE_NVAPI_VERSION<NV_COLOR_DATA_V5>(5);
        public static UInt32 NV_HDR_CAPABILITIES_V2_VER = MAKE_NVAPI_VERSION<NV_HDR_CAPABILITIES_V2>(2);
        public static UInt32 NV_MOSAIC_DISPLAY_TOPO_STATUS_V1_VER = MAKE_NVAPI_VERSION<NV_MOSAIC_DISPLAY_TOPO_STATUS_V1>(1);
        public static UInt32 NV_GPU_DISPLAYIDS_V2_VER = MAKE_NVAPI_VERSION<NV_GPU_DISPLAYIDS_V2>(3); // NOTE: There is a bug in R470 that sets the NV_GPU_DISPLAYIDS_V2 version to 3!
        public static UInt32 NV_BOARD_INFO_V1_VER = MAKE_NVAPI_VERSION<NV_BOARD_INFO_V1>(1);
        public static UInt32 NV_EDID_V3_VER = MAKE_NVAPI_VERSION<NV_EDID_V3>(3);
        public static UInt32 NV_DISPLAYCONFIG_PATH_INFO_V1_VER = MAKE_NVAPI_VERSION<NV_DISPLAYCONFIG_PATH_INFO_V1>(1);
        public static UInt32 NV_DISPLAYCONFIG_PATH_INFO_V2_VER = MAKE_NVAPI_VERSION<NV_DISPLAYCONFIG_PATH_INFO_V2>(2);
        public static UInt32 NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_VER = MAKE_NVAPI_VERSION<NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1>(1);
        public static UInt32 NV_CUSTOM_DISPLAY_V1_VER = MAKE_NVAPI_VERSION<NV_CUSTOM_DISPLAY_V1>(1);
        public static UInt32 NV_LOGICAL_GPU_DATA_V1_VER = MAKE_NVAPI_VERSION<NV_LOGICAL_GPU_DATA_V1>(1);
        public static UInt32 NV_GET_ADAPTIVE_SYNC_DATA_V1_VER = MAKE_NVAPI_VERSION<NV_GET_ADAPTIVE_SYNC_DATA_V1>(1);
        public static UInt32 NV_SET_ADAPTIVE_SYNC_DATA_V1_VER = MAKE_NVAPI_VERSION<NV_SET_ADAPTIVE_SYNC_DATA_V1>(1);

        public static UInt32 NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL_VER = MAKE_NVAPI_VERSION<NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL>(2);
        public static UInt32 NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL_VER = MAKE_NVAPI_VERSION<NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL>(1);

        public static UInt32 NVDRS_PROFILE_V1_VER = MAKE_NVAPI_VERSION<NVDRS_PROFILE_V1>(1);
        public static UInt32 NVDRS_SETTING_V1_VER = MAKE_NVAPI_VERSION<NVDRS_SETTING_V1>(1);
        //public static UInt32 NVDRS_SETTING_VALUES_V1_VER = MAKE_NVAPI_VERSION<NVDRS_SETTING_VALUES_V1>(1);




        #region Internal Constant
        [DllImport("nvapi64.dll", EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr NvApiQueryInterface64(UInt32 apiId);

        [DllImport("nvapi.dll", EntryPoint = "nvapi_QueryInterface", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr NvApiQueryInterface32(UInt32 apiId);


        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(String dllname);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, String procname);
        #endregion Internal Constant

        private static UInt32 MAKE_NVAPI_VERSION<T>(Int32 version)
        {
            return (UInt32)((Marshal.SizeOf(typeof(T))) | (Int32)(version << 16));
        }
        private static UInt32 MAKE_NVAPI_VERSION(Int32 size, Int32 version)
        {
            return (UInt32)((Int32)size | (Int32)(version << 16));
        }

        private static void GetDelegate<T>(UInt32 apiId, out T newDelegate) where T : class
        {
            newDelegate = null;
            try
            {
                IntPtr intPtrApiQuery = IntPtr.Zero;

                if (IntPtr.Size > 4)
                {
                    intPtrApiQuery = NvApiQueryInterface64(apiId);
                }
                else
                {
                    intPtrApiQuery = NvApiQueryInterface32(apiId);
                }

                if (intPtrApiQuery != IntPtr.Zero)
                {
                    newDelegate = Marshal.GetDelegateForFunctionPointer(intPtrApiQuery, typeof(T)) as T;
                }
            }
            catch { }
        }

        #region DLLImport

        /*[DllImport(Kernel32_FileName)]
        public static extern HMODULE GetModuleHandle(string moduleName);*/


        /*// Delegate
        // This function initializes the NvAPI library (if not already initialized) but always increments the ref-counter.
        // This must be called before calling other NvAPI_ functions. Note: It is now mandatory to call NvAPI_Initialize before calling any other NvAPI. NvAPI_Unload should be called to unload the NVAPI Library.
        [UnmanagedFunctionPoInt32er(CallingConvention.Cdecl)]
        private delegate NVAPI_STATUS NvAPI_Initialize();

        //DESCRIPTION: Decrements the ref-counter and when it reaches ZERO, unloads NVAPI library.This must be called in pairs with NvAPI_Initialize.
        // If the client wants unload functionality, it is recommended to always call NvAPI_Initialize and NvAPI_Unload in pairs.
        // Unloading NvAPI library is not supported when the library is in a resource locked state.
        // Some functions in the NvAPI library initiates an operation or allocates certain resources and there are corresponding functions available, to complete the operation or free the allocated resources.
        // All such function pairs are designed to prevent unloading NvAPI library. For example, if NvAPI_Unload is called after NvAPI_XXX which locks a resource, it fails with NVAPI_ERROR.
        // Developers need to call the corresponding NvAPI_YYY to unlock the resources, before calling NvAPI_Unload again.
        [DllImport(NVAPI_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern NVAPI_STATUS NvAPI_Unload();

        // This is used to get a string containing the NVAPI version 
        [DllImport(NVAPI_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern NVAPI_STATUS NvAPI_GetInterfaceVersionStringEx(out string description);

        // NVAPI SESSION HANDLING FUNCTIONS
        // This is used to get a session handle to use to maInt32ain state across multiple NVAPI calls
        [DllImport(NVAPI_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern NVAPI_STATUS NvAPI_DRS_CreateSession(out IntPtr session);

        // This is used to destroy a session handle to used to maInt32ain state across multiple NVAPI calls
        [DllImport(NVAPI_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern NVAPI_STATUS NvAPI_DRS_DestorySession(IntPtr session);

        // This API lets caller retrieve the current global display configuration.
        // USAGE: The caller might have to call this three times to fetch all the required configuration details as follows:
        // First Pass: Caller should Call NvAPI_DISP_GetDisplayConfig() with pathInfo set to NULL to fetch pathInfoCount.
        // Second Pass: Allocate memory for pathInfo with respect to the number of pathInfoCount(from First Pass) to fetch targetInfoCount. If sourceModeInfo is needed allocate memory or it can be initialized to NULL.
        // Third Pass(Optional, only required if target information is required): Allocate memory for targetInfo with respect to number of targetInfoCount(from Second Pass).
        [DllImport(NVAPI_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern NVAPI_STATUS NvAPI_DISP_GetDisplayConfig(ref ulong pathInfoCount, out NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 pathInfo);
*/

        #endregion DLLImport



        #region Initialization code
        private static bool available = false;

        public static bool IsAvailable() { return NVImport.available; }

        static NVImport()
        {
            DllImportAttribute attribute = new DllImportAttribute(GetDllName());
            attribute.CallingConvention = CallingConvention.Cdecl;
            attribute.PreserveSig = true;
            attribute.EntryPoint = "nvapi_QueryInterface";
            PInvokeDelegateFactory.CreateDelegate(attribute, out QueryInterface);

            try
            {
                GetDelegate(NvId_Initialize, out InitializeInternal);
            }
            catch (DllNotFoundException) { return; }
            catch (EntryPointNotFoundException) { return; }
            catch (ArgumentNullException) { return; }
            catch (NullReferenceException) { return; }

            if (InitializeInternal() == NVAPI_STATUS.NVAPI_OK)
            {
                GetDelegate(NvId_Unload, out UnloadInternal);
                GetDelegate(NvId_GetInterfaceVersionString, out GetInterfaceVersionStringInternal);
                GetDelegate(NvId_GetErrorMessage, out GetErrorMessageInternal);

                // Display
                GetDelegate(NvId_EnumNvidiaDisplayHandle, out EnumNvidiaDisplayHandleInternal);
                GetDelegate(NvId_EnumNvidiaUnAttachedDisplayHandle, out EnumNvidiaUnAttachedDisplayHandleInternal);
                GetDelegate(NvId_GetAssociatedNvidiaDisplayHandle, out GetAssociatedNvidiaDisplayHandleInternal);
                GetDelegate(NvId_DISP_GetAssociatedUnAttachedNvidiaDisplayHandle, out GetAssociatedUnAttachedNvidiaDisplayHandleInternal);
                GetDelegate(NvId_DISP_GetGDIPrimaryDisplayId, out DISP_GetGDIPrimaryDisplayIdInternal);
                GetDelegate(NvId_Disp_GetHdrCapabilities, out Disp_GetHdrCapabilitiesInternal);
                GetDelegate(NvId_Disp_HdrColorControl, out Disp_HdrColorControlInternal);
                GetDelegate(NvId_Disp_ColorControl, out Disp_ColorControlInternal);
                GetDelegate(NvId_DISP_GetDisplayConfig, out DISP_GetDisplayConfigInternal);
                GetDelegate(NvId_DISP_GetDisplayConfig, out DISP_GetDisplayConfigInternalNull); // null version of the submission
                GetDelegate(NvId_DISP_SetDisplayConfig, out DISP_SetDisplayConfigInternal);
                GetDelegate(NvId_DISP_GetDisplayIdByDisplayName, out DISP_GetDisplayIdByDisplayNameInternal);
                GetDelegate(NvId_DISP_EnumCustomDisplay, out Disp_EnumCustomDisplayInternal);
                GetDelegate(NvId_DISP_GetAdaptiveSyncData, out DISP_GetAdaptiveSyncDataInternal);
                GetDelegate(NvId_DISP_SetAdaptiveSyncData, out DISP_SetAdaptiveSyncDataInternal);

                // GPUs
                GetDelegate(NvId_EnumPhysicalGPUs, out EnumPhysicalGPUsInternal);
                GetDelegate(NvId_GPU_GetQuadroStatus, out GetQuadroStatusInternal);
                GetDelegate(NvId_GPU_GetConnectedDisplayIds, out GPU_GetConnectedDisplayIdsInternal);
                GetDelegate(NvId_GPU_GetConnectedDisplayIds, out GPU_GetConnectedDisplayIdsInternalNull); // The null version of the submission
                GetDelegate(NvId_GPU_GetFullName, out GPU_GetFullNameInternal);
                GetDelegate(NvId_GPU_GetBoardInfo, out GPU_GetBoardInfoInternal);
                GetDelegate(NvId_GPU_GetBusType, out GPU_GetBusTypeInternal);
                GetDelegate(NvId_GPU_GetBusId, out GPU_GetBusIdInternal);
                GetDelegate(NvId_GPU_GetEDID, out GPU_GetEDIDInternal);
                GetDelegate(NvId_GPU_GetEDID, out GPU_GetEDIDInternal);
                GetDelegate(NvId_GetLogicalGPUFromPhysicalGPU, out GetLogicalGPUFromPhysicalGPUInternal);
                GetDelegate(NvId_GPU_GetLogicalGpuInfo, out GPU_GetLogicalGpuInfoInternal);

                // Mosaic                
                GetDelegate(NvId_Mosaic_EnableCurrentTopo, out Mosaic_EnableCurrentTopoInternal);
                GetDelegate(NvId_Mosaic_SetCurrentTopo, out Mosaic_SetCurrentTopoInternal);
                GetDelegate(NvId_Mosaic_GetCurrentTopo, out Mosaic_GetCurrentTopoInternal);
                GetDelegate(NvId_Mosaic_GetTopoGroup, out Mosaic_GetTopoGroupInternal);
                GetDelegate(NvId_Mosaic_GetSupportedTopoInfo, out Mosaic_GetSupportedTopoInfoInternal);
                GetDelegate(NvId_Mosaic_EnumDisplayModes, out Mosaic_EnumDisplayModesInternal);
                GetDelegate(NvId_Mosaic_EnumDisplayModes, out Mosaic_EnumDisplayModesInternalNull); // The null version of the submission
                GetDelegate(NvId_Mosaic_EnumDisplayGrids, out Mosaic_EnumDisplayGridsInternal);
                GetDelegate(NvId_Mosaic_EnumDisplayGrids, out Mosaic_EnumDisplayGridsInternalNull); // The null version of the submission
                GetDelegate(NvId_Mosaic_SetDisplayGrids, out Mosaic_SetDisplayGridsInternal);
                GetDelegate(NvId_Mosaic_ValidateDisplayGrids, out Mosaic_ValidateDisplayGridsInternal);
                GetDelegate(NvId_Mosaic_GetDisplayViewportsByResolution, out Mosaic_GetDisplayViewportsByResolutionInternal);
                GetDelegate(NvId_Mosaic_GetOverlapLimits, out Mosaic_GetOverlapLimitsInternal);

                // System
                GetDelegate(NvId_SYS_GetGpuAndOutputIdFromDisplayId, out SYS_GetGpuAndOutputIdFromDisplayIdInternal);

                // DRS
                GetDelegate(NvId_DRS_SetProfileInfo, out DRS_SetProfileInfoInternal);
                GetDelegate(NvId_DRS_SetSetting, out DRS_SetSettingInternal);
                GetDelegate(NvId_DRS_GetCurrentGlobalProfile, out DRS_GetCurrentGlobalProfileInternal);
                GetDelegate(NvId_DRS_EnumSettings, out DRS_EnumSettingsInternal);
                GetDelegate(NvId_DRS_GetSetting, out DRS_GetSettingInternal);
                GetDelegate(NvId_DRS_GetProfileInfo, out DRS_GetProfileInfoInternal);
                GetDelegate(NvId_DRS_GetSettingIdFromName, out DRS_GetSettingIdFromNameInternal);
                GetDelegate(NvId_DRS_EnumAvailableSettingIds, out DRS_EnumAvailableSettingIdsInternal);
                GetDelegate(NvId_DRS_CreateSession, out DRS_CreateSessionInternal);
                GetDelegate(NvId_DRS_GetSettingNameFromId, out DRS_GetSettingNameFromIdInternal);
                GetDelegate(NvId_DRS_EnumAvailableSettingValues, out DRS_EnumAvailableSettingValuesInternal);
                GetDelegate(NvId_DRS_EnumProfiles, out DRS_EnumProfilesInternal);
                GetDelegate(NvId_DRS_SetCurrentGlobalProfile, out DRS_SetCurrentGlobalProfileInternal);
                GetDelegate(NvId_DRS_DestroySession, out DRS_DestroySessionInternal);
                GetDelegate(NvId_DRS_LoadSettings, out DRS_LoadSettingsInternal);
                GetDelegate(NvId_DRS_SaveSettings, out DRS_SaveSettingsInternal);
                GetDelegate(NvId_DRS_GetBaseProfile, out DRS_GetBaseProfileInternal);
                GetDelegate(NvId_DRS_GetNumProfiles, out DRS_GetNumProfilesInternal);
                GetDelegate(NvId_DRS_RestoreProfileDefaultSetting, out DRS_RestoreProfileDefaultSettingInternal);

                // Set the availability
                available = true;
            }

            AppDomain.CurrentDomain.ProcessExit += NVImport.OnExit;
        }

        private static string GetDllName()
        {
            if (IntPtr.Size > 4)
            {
                return "nvapi64.dll";
            }
            else
            {
                return "nvapi.dll";
            }
        }

        private static void OnExit(object sender, EventArgs e)
        {
            available = false;

            if (NVImport.UnloadInternal != null) { NVImport.UnloadInternal(); }
        }

        public static TResult BitWiseConvert<TResult, T>(T source)
            where TResult : struct, IConvertible
            where T : struct, IConvertible
        {
            if (typeof(T) == typeof(TResult))
            {
                return (TResult)(object)source;
            }

            var sourceSize = Marshal.SizeOf(typeof(T));
            var destinationSize = Marshal.SizeOf(typeof(TResult));
            var minSize = Math.Min(sourceSize, destinationSize);
            var sourcePoInt32er = Marshal.AllocHGlobal(sourceSize);
            Marshal.StructureToPtr(source, sourcePoInt32er, false);
            var bytes = new byte[destinationSize];

            if (BitConverter.IsLittleEndian)
            {
                Marshal.Copy(sourcePoInt32er, bytes, 0, minSize);
            }
            else
            {
                Marshal.Copy(sourcePoInt32er + (sourceSize - minSize), bytes, destinationSize - minSize, minSize);
            }

            Marshal.FreeHGlobal(sourcePoInt32er);
            var destinationPoInt32er = Marshal.AllocHGlobal(destinationSize);
            Marshal.Copy(bytes, 0, destinationPoInt32er, destinationSize);
            var destination = (TResult)Marshal.PtrToStructure(destinationPoInt32er, typeof(TResult));
            Marshal.FreeHGlobal(destinationPoInt32er);

            return destination;
        }

        public static bool GetBit<T>(this T Int32eger, Int32 index) where T : struct, IConvertible
        {
            var bigInteger = BitWiseConvert<ulong, T>(Int32eger);
            var mask = 1ul << index;

            return (bigInteger & mask) > 0;
        }

        public static ulong GetBits<T>(this T Int32eger, Int32 index, Int32 count) where T : struct, IConvertible
        {
            var bigInteger = BitWiseConvert<ulong, T>(Int32eger);

            if (index > 0)
            {
                bigInteger >>= index;
            }

            count = 64 - count;
            bigInteger <<= count;
            bigInteger >>= count;

            return bigInteger;
        }

        public static class Utils
        {
            public static Int32 SizeOf<T>(T obj)
            {
                return SizeOfCache<T>.SizeOf;
            }

            private static class SizeOfCache<T>
            {
                public static readonly Int32 SizeOf;

                static SizeOfCache()
                {
                    var dm = new DynamicMethod("func", typeof(Int32),
                                               Type.EmptyTypes, typeof(Utils));

                    ILGenerator il = dm.GetILGenerator();
                    il.Emit(OpCodes.Sizeof, typeof(T));
                    il.Emit(OpCodes.Ret);

                    var func = (Func<Int32>)dm.CreateDelegate(typeof(Func<Int32>));
                    SizeOf = func();
                }
            }

        }
        #endregion


        // NvAPI Functions extracted from R470 Developer nvapi64.lib using dumpbin
        // e.g. dumpbin.exe /DISASM R470-developer\amd64\nvapi64.lib
        // Can also use this script: https://raw.githubusercontent.com/terrymacdonald/NVIDIAInfo/main/NVIDIAInfo/NVAPI_Function_Location_Extractor.ps1
        // Note: This only extracts the public NvAPI Functions! The private NvAPI calls were found by Soroush Falahati.

        #region NvAPI Public Functions 

        private const uint NvId_GetErrorMessage = 0x6C2D048C;
        private const uint NvId_GetInterfaceVersionString = 0x1053FA5;
        private const uint NvId_GPU_GetEDID = 0x37D32E69;
        private const uint NvId_SetView = 0x957D7B6;
        private const uint NvId_SetViewEx = 0x6B89E68;
        private const uint NvId_GetDisplayDriverVersion = 0xF951A4D1;
        private const uint NvId_SYS_GetDriverAndBranchVersion = 0x2926AAAD;
        private const uint NvId_GPU_GetMemoryInfo = 0x7F9B368;
        private const uint NvId_OGL_ExpertModeSet = 0x3805EF7A;
        private const uint NvId_OGL_ExpertModeGet = 0x22ED9516;
        private const uint NvId_OGL_ExpertModeDefaultsSet = 0xB47A657E;
        private const uint NvId_OGL_ExpertModeDefaultsGet = 0xAE921F12;
        private const uint NvId_EnumPhysicalGPUs = 0xE5AC921F;
        private const uint NvId_EnumTCCPhysicalGPUs = 0xD9930B07;
        private const uint NvId_EnumLogicalGPUs = 0x48B3EA59;
        private const uint NvId_GetPhysicalGPUsFromDisplay = 0x34EF9506;
        private const uint NvId_GetPhysicalGPUFromUnAttachedDisplay = 0x5018ED61;
        private const uint NvId_GetLogicalGPUFromDisplay = 0xEE1370CF;
        private const uint NvId_GetLogicalGPUFromPhysicalGPU = 0xADD604D1;
        private const uint NvId_GetPhysicalGPUsFromLogicalGPU = 0xAEA3FA32;
        private const uint NvId_GetPhysicalGPUFromGPUID = 0x5380AD1A;
        private const uint NvId_GetGPUIDfromPhysicalGPU = 0x6533EA3E;
        private const uint NvId_GPU_GetShaderSubPipeCount = 0xBE17923;
        private const uint NvId_GPU_GetGpuCoreCount = 0xC7026A87;
        private const uint NvId_GPU_GetAllOutputs = 0x7D554F8E;
        private const uint NvId_GPU_GetConnectedOutputs = 0x1730BFC9;
        private const uint NvId_GPU_GetConnectedSLIOutputs = 0x680DE09;
        private const uint NvId_GPU_GetConnectedDisplayIds = 0x78DBA2;
        private const uint NvId_GPU_GetAllDisplayIds = 0x785210A2;
        private const uint NvId_GPU_GetConnectedOutputsWithLidState = 0xCF8CAF39;
        private const uint NvId_GPU_GetConnectedSLIOutputsWithLidState = 0x96043CC7;
        private const uint NvId_GPU_GetSystemType = 0xBAAABFCC;
        private const uint NvId_GPU_GetActiveOutputs = 0xE3E89B6F;
        private const uint NvId_GPU_SetEDID = 0xE83D6456;
        private const uint NvId_GPU_GetOutputType = 0x40A505E4;
        private const uint NvId_GPU_ValidateOutputCombination = 0x34C9C2D4;
        private const uint NvId_GPU_GetFullName = 0xCEEE8E9F;
        private const uint NvId_GPU_GetPCIIdentifiers = 0x2DDFB66E;
        private const uint NvId_GPU_GetGPUType = 0xC33BAEB1;
        private const uint NvId_GPU_GetBusType = 0x1BB18724;
        private const uint NvId_GPU_GetBusId = 0x1BE0B8E5;
        private const uint NvId_GPU_GetBusSlotId = 0x2A0A350F;
        private const uint NvId_GPU_GetIRQ = 0xE4715417;
        private const uint NvId_GPU_GetVbiosRevision = 0xACC3DA0A;
        private const uint NvId_GPU_GetVbiosOEMRevision = 0x2D43FB31;
        private const uint NvId_GPU_GetVbiosVersionString = 0xA561FD7D;
        private const uint NvId_GPU_GetAGPAperture = 0x6E042794;
        private const uint NvId_GPU_GetCurrentAGPRate = 0xC74925A0;
        private const uint NvId_GPU_GetCurrentPCIEDownstreamWidth = 0xD048C3B1;
        private const uint NvId_GPU_GetPhysicalFrameBufferSize = 0x46FBEB03;
        private const uint NvId_GPU_GetVirtualFrameBufferSize = 0x5A04B644;
        private const uint NvId_GPU_GetQuadroStatus = 0xE332FA47;
        private const uint NvId_GPU_GetBoardInfo = 0x22D54523;
        private const uint NvId_GPU_GetArchInfo = 0xD8265D24;
        private const uint NvId_I2CRead = 0x2FDE12C5;
        private const uint NvId_I2CWrite = 0xE812EB07;
        private const uint NvId_GPU_WorkstationFeatureSetup = 0x6C1F3FE4;
        private const uint NvId_GPU_WorkstationFeatureQuery = 0x4537DF;
        private const uint NvId_GPU_GetHDCPSupportStatus = 0xF089EEF5;
        private const uint NvId_GPU_CudaEnumComputeCapableGpus = 0x5786CC6E;
        private const uint NvId_GPU_GetTachReading = 0x5F608315;
        private const uint NvId_GPU_GetECCStatusInfo = 0xCA1DDAF3;
        private const uint NvId_GPU_GetECCErrorInfo = 0xC71F85A6;
        private const uint NvId_GPU_ResetECCErrorInfo = 0xC02EEC20;
        private const uint NvId_GPU_GetECCConfigurationInfo = 0x77A796F3;
        private const uint NvId_GPU_SetECCConfiguration = 0x1CF639D9;
        private const uint NvId_GPU_QueryWorkstationFeatureSupport = 0x80B1ABB9;
        private const uint NvId_GPU_SetScanoutIntensity = 0xA57457A4;
        private const uint NvId_GPU_GetScanoutIntensityState = 0xE81CE836;
        private const uint NvId_GPU_SetScanoutWarping = 0xB34BAB4F;
        private const uint NvId_GPU_GetScanoutWarpingState = 0x6F5435AF;
        private const uint NvId_GPU_SetScanoutCompositionParameter = 0xF898247D;
        private const uint NvId_GPU_GetScanoutCompositionParameter = 0x58FE51E6;
        private const uint NvId_GPU_GetScanoutConfiguration = 0x6A9F5B63;
        private const uint NvId_GPU_GetScanoutConfigurationEx = 0xE2E1E6F0;
        private const uint NvId_GPU_GetAdapterIdFromPhysicalGpu = 0xFF07FDE;
        private const uint NvId_GPU_GetVirtualizationInfo = 0x44E022A9;
        private const uint NvId_GPU_GetLogicalGpuInfo = 0x842B066E;
        private const uint NvId_GPU_GetLicensableFeatures = 0x3FC596AA;
        private const uint NvId_GPU_GetVRReadyData = 0x81D629C5;
        private const uint NvId_GPU_GetPerfDecreaseInfo = 0x7F7F4600;
        private const uint NvId_GPU_GetPstatesInfoEx = 0x843C0256;
        private const uint NvId_GPU_GetPstates20 = 0x6FF81213;
        private const uint NvId_GPU_GetCurrentPstate = 0x927DA4F6;
        private const uint NvId_GPU_GetDynamicPstatesInfoEx = 0x60DED2ED;
        private const uint NvId_GPU_GetThermalSettings = 0xE3640A56;
        private const uint NvId_GPU_GetAllClockFrequencies = 0xDCB616C3;
        private const uint NvId_GPU_QueryIlluminationSupport = 0xA629DA31;
        private const uint NvId_GPU_GetIllumination = 0x9A1B9365;
        private const uint NvId_GPU_SetIllumination = 0x254A187;
        private const uint NvId_GPU_ClientIllumDevicesGetInfo = 0xD4100E58;
        private const uint NvId_GPU_ClientIllumDevicesGetControl = 0x73C01D58;
        private const uint NvId_GPU_ClientIllumDevicesSetControl = 0x57024C62;
        private const uint NvId_GPU_ClientIllumZonesGetInfo = 0x4B81241B;
        private const uint NvId_GPU_ClientIllumZonesGetControl = 0x3DBF5764;
        private const uint NvId_GPU_ClientIllumZonesSetControl = 0x197D065E;
        private const uint NvId_Event_RegisterCallback = 0xE6DBEA69;
        private const uint NvId_Event_UnregisterCallback = 0xDE1F9B45;
        private const uint NvId_EnumNvidiaDisplayHandle = 0x9ABDD40D;
        private const uint NvId_EnumNvidiaUnAttachedDisplayHandle = 0x20DE9260;
        private const uint NvId_CreateDisplayFromUnAttachedDisplay = 0x63F9799E;
        private const uint NvId_GetAssociatedNvidiaDisplayHandle = 0x35C29134;
        private const uint NvId_DISP_GetAssociatedUnAttachedNvidiaDisplayHandle = 0xA70503B2;
        private const uint NvId_GetAssociatedNvidiaDisplayName = 0x22A78B05;
        private const uint NvId_GetUnAttachedAssociatedDisplayName = 0x4888D790;
        private const uint NvId_EnableHWCursor = 0x2863148D;
        private const uint NvId_DisableHWCursor = 0xAB163097;
        private const uint NvId_GetVBlankCounter = 0x67B5DB55;
        private const uint NvId_SetRefreshRateOverride = 0x3092AC32;
        private const uint NvId_GetAssociatedDisplayOutputId = 0xD995937E;
        private const uint NvId_GetDisplayPortInfo = 0xC64FF367;
        private const uint NvId_SetDisplayPort = 0xFA13E65A;
        private const uint NvId_GetHDMISupportInfo = 0x6AE16EC3;
        private const uint NvId_Disp_InfoFrameControl = 0x6067AF3F;
        private const uint NvId_Disp_ColorControl = 0x92F9D80D;
        private const uint NvId_Disp_GetHdrCapabilities = 0x84F2A8DF;
        private const uint NvId_Disp_HdrColorControl = 0x351DA224;
        private const uint NvId_DISP_GetTiming = 0x175167E9;
        private const uint NvId_DISP_GetMonitorCapabilities = 0x3B05C7E1;
        private const uint NvId_DISP_GetMonitorColorCapabilities = 0x6AE4CFB5;
        private const uint NvId_DISP_EnumCustomDisplay = 0xA2072D59;
        private const uint NvId_DISP_TryCustomDisplay = 0x1F7DB630;
        private const uint NvId_DISP_DeleteCustomDisplay = 0x552E5B9B;
        private const uint NvId_DISP_SaveCustomDisplay = 0x49882876;
        private const uint NvId_DISP_RevertCustomDisplayTrial = 0xCBBD40F0;
        private const uint NvId_GetView = 0xD6B99D89;
        private const uint NvId_GetViewEx = 0xDBBC0AF4;
        private const uint NvId_GetSupportedViews = 0x66FB7FC0;
        private const uint NvId_DISP_GetDisplayIdByDisplayName = 0xAE457190;
        private const uint NvId_DISP_GetGDIPrimaryDisplayId = 0x1E9D8A31;
        private const uint NvId_DISP_GetDisplayConfig = 0x11ABCCF8;
        private const uint NvId_DISP_SetDisplayConfig = 0x5D8CF8DE;
        private const uint NvId_DISP_GetAdaptiveSyncData = 0xB73D1EE9;
        private const uint NvId_DISP_SetAdaptiveSyncData = 0x3EEBBA1D;
        private const uint NvId_DISP_GetVirtualRefreshRateData = 0x8C00429A;
        private const uint NvId_DISP_SetVirtualRefreshRateData = 0x5ABBE6A3;
        private const uint NvId_DISP_SetPreferredStereoDisplay = 0xC9D0E25F;
        private const uint NvId_DISP_GetPreferredStereoDisplay = 0x1F6B4666;
        private const uint NvId_DISP_GetNvManagedDedicatedDisplays = 0xDBDF0CB2;
        private const uint NvId_DISP_AcquireDedicatedDisplay = 0x47C917BA;
        private const uint NvId_DISP_ReleaseDedicatedDisplay = 0x1247825F;
        private const uint NvId_Mosaic_GetSupportedTopoInfo = 0xFDB63C81;
        private const uint NvId_Mosaic_GetTopoGroup = 0xCB89381D;
        private const uint NvId_Mosaic_GetOverlapLimits = 0x989685F0;
        private const uint NvId_Mosaic_SetCurrentTopo = 0x9B542831;
        private const uint NvId_Mosaic_GetCurrentTopo = 0xEC32944E;
        private const uint NvId_Mosaic_EnableCurrentTopo = 0x5F1AA66C;
        private const uint NvId_Mosaic_GetDisplayViewportsByResolution = 0xDC6DC8D3;
        private const uint NvId_Mosaic_SetDisplayGrids = 0x4D959A89;
        private const uint NvId_Mosaic_ValidateDisplayGrids = 0xCF43903D;
        private const uint NvId_Mosaic_EnumDisplayModes = 0x78DB97D7;
        private const uint NvId_Mosaic_EnumDisplayGrids = 0xDF2887AF;
        private const uint NvId_GetSupportedMosaicTopologies = 0x410B5C25;
        private const uint NvId_GetCurrentMosaicTopology = 0xF60852BD;
        private const uint NvId_SetCurrentMosaicTopology = 0xD54B8989;
        private const uint NvId_EnableCurrentMosaicTopology = 0x74073CC9;
        private const uint NvId_GSync_EnumSyncDevices = 0xD9639601;
        private const uint NvId_GSync_QueryCapabilities = 0x44A3F1D1;
        private const uint NvId_GSync_GetTopology = 0x4562BC38;
        private const uint NvId_GSync_SetSyncStateSettings = 0x60ACDFDD;
        private const uint NvId_GSync_GetControlParameters = 0x16DE1C6A;
        private const uint NvId_GSync_SetControlParameters = 0x8BBFF88B;
        private const uint NvId_GSync_AdjustSyncDelay = 0x2D11FF51;
        private const uint NvId_GSync_GetSyncStatus = 0xF1F5B434;
        private const uint NvId_GSync_GetStatusParameters = 0x70D404EC;
        private const uint NvId_D3D_GetCurrentSLIState = 0x4B708B54;
        private const uint NvId_D3D9_RegisterResource = 0xA064BDFC;
        private const uint NvId_D3D9_UnregisterResource = 0xBB2B17AA;
        private const uint NvId_D3D9_AliasSurfaceAsTexture = 0xE5CEAE41;
        private const uint NvId_D3D9_StretchRectEx = 0x22DE03AA;
        private const uint NvId_D3D9_ClearRT = 0x332D3942;
        private const uint NvId_D3D_GetObjectHandleForResource = 0xFCEAC864;
        private const uint NvId_D3D_SetResourceHint = 0x6C0ED98C;
        private const uint NvId_D3D_BeginResourceRendering = 0x91123D6A;
        private const uint NvId_D3D_EndResourceRendering = 0x37E7191C;
        private const uint NvId_D3D9_GetSurfaceHandle = 0xF2DD3F2;
        private const uint NvId_D3D9_VideoSetStereoInfo = 0xB852F4DB;
        private const uint NvId_D3D10_SetDepthBoundsTest = 0x4EADF5D2;
        private const uint NvId_D3D11_CreateDevice = 0x6A16D3A0;
        private const uint NvId_D3D11_CreateDeviceAndSwapChain = 0xBB939EE5;
        private const uint NvId_D3D11_SetDepthBoundsTest = 0x7AAF7A04;
        private const uint NvId_D3D11_IsNvShaderExtnOpCodeSupported = 0x5F68DA40;
        private const uint NvId_D3D11_SetNvShaderExtnSlot = 0x8E90BB9F;
        private const uint NvId_D3D12_SetNvShaderExtnSlotSpace = 0xAC2DFEB5;
        private const uint NvId_D3D12_SetNvShaderExtnSlotSpaceLocalThread = 0x43D867C0;
        private const uint NvId_D3D11_SetNvShaderExtnSlotLocalThread = 0xE6482A0;
        private const uint NvId_D3D11_BeginUAVOverlapEx = 0xBA08208A;
        private const uint NvId_D3D11_BeginUAVOverlap = 0x65B93CA8;
        private const uint NvId_D3D11_EndUAVOverlap = 0x2216A357;
        private const uint NvId_D3D11_GetResourceHandle = 0x9D52986;
        private const uint NvId_D3D_SetFPSIndicatorState = 0xA776E8DB;
        private const uint NvId_D3D9_Present = 0x5650BEB;
        private const uint NvId_D3D9_QueryFrameCount = 0x9083E53A;
        private const uint NvId_D3D9_ResetFrameCount = 0xFA6A0675;
        private const uint NvId_D3D9_QueryMaxSwapGroup = 0x5995410D;
        private const uint NvId_D3D9_QuerySwapGroup = 0xEBA4D232;
        private const uint NvId_D3D9_JoinSwapGroup = 0x7D44BB54;
        private const uint NvId_D3D9_BindSwapBarrier = 0x9C39C246;
        private const uint NvId_D3D1x_Present = 0x3B845A1;
        private const uint NvId_D3D1x_QueryFrameCount = 0x9152E055;
        private const uint NvId_D3D1x_ResetFrameCount = 0xFBBB031A;
        private const uint NvId_D3D1x_QueryMaxSwapGroup = 0x9BB9D68F;
        private const uint NvId_D3D1x_QuerySwapGroup = 0x407F67AA;
        private const uint NvId_D3D1x_JoinSwapGroup = 0x14610CD7;
        private const uint NvId_D3D1x_BindSwapBarrier = 0x9DE8C729;
        private const uint NvId_D3D12_QueryPresentBarrierSupport = 0xA15FAEF7;
        private const uint NvId_D3D12_CreatePresentBarrierClient = 0x4D815DE9;
        private const uint NvId_D3D12_RegisterPresentBarrierResources = 0xD53C9EF0;
        private const uint NvId_DestroyPresentBarrierClient = 0x3C5C351B;
        private const uint NvId_JoinPresentBarrier = 0x17F6BF82;
        private const uint NvId_LeavePresentBarrier = 0xC3EC5A7F;
        private const uint NvId_QueryPresentBarrierFrameStatistics = 0x61B844A1;
        private const uint NvId_D3D12_CreateDDisplayPresentBarrierClient = 0xB5A21987;
        private const uint NvId_D3D11_CreateRasterizerState = 0xDB8D28AF;
        private const uint NvId_D3D_ConfigureAnsel = 0x341C6C7F;
        private const uint NvId_D3D11_CreateTiledTexture2DArray = 0x7886981A;
        private const uint NvId_D3D11_CheckFeatureSupport = 0x106A487E;
        private const uint NvId_D3D11_CreateImplicitMSAATexture2D = 0xB8F79632;
        private const uint NvId_D3D12_CreateCommittedImplicitMSAATexture2D = 0x24C6A07B;
        private const uint NvId_D3D11_ResolveSubresourceRegion = 0xE6BFEDD6;
        private const uint NvId_D3D12_ResolveSubresourceRegion = 0xC24A15BF;
        private const uint NvId_D3D11_TiledTexture2DArrayGetDesc = 0xF1A2B9D5;
        private const uint NvId_D3D11_UpdateTileMappings = 0x9A06EA07;
        private const uint NvId_D3D11_CopyTileMappings = 0xC09EE6BC;
        private const uint NvId_D3D11_TiledResourceBarrier = 0xD6839099;
        private const uint NvId_D3D11_AliasMSAATexture2DAsNonMSAA = 0xF1C54FC9;
        private const uint NvId_D3D11_CreateGeometryShaderEx_2 = 0x99ED5C1C;
        private const uint NvId_D3D11_CreateVertexShaderEx = 0xBEAA0B2;
        private const uint NvId_D3D11_CreateHullShaderEx = 0xB53CAB00;
        private const uint NvId_D3D11_CreateDomainShaderEx = 0xA0D7180D;
        private const uint NvId_D3D11_CreatePixelShaderEx_2 = 0x4162822B;
        private const uint NvId_D3D11_CreateFastGeometryShaderExplicit = 0x71AB7C9C;
        private const uint NvId_D3D11_CreateFastGeometryShader = 0x525D43BE;
        private const uint NvId_D3D11_DecompressView = 0x3A94E822;
        private const uint NvId_D3D12_CreateGraphicsPipelineState = 0x2FC28856;
        private const uint NvId_D3D12_CreateComputePipelineState = 0x2762DEAC;
        private const uint NvId_D3D12_SetDepthBoundsTestValues = 0xB9333FE9;
        private const uint NvId_D3D12_CreateReservedResource = 0x2C85F101;
        private const uint NvId_D3D12_CreateHeap = 0x5CB397CF;
        private const uint NvId_D3D12_CreateHeap2 = 0x924BE9D6;
        private const uint NvId_D3D12_QueryCpuVisibleVidmem = 0x26322BC3;
        private const uint NvId_D3D12_ReservedResourceGetDesc = 0x9AA2AABB;
        private const uint NvId_D3D12_UpdateTileMappings = 0xC6017A7D;
        private const uint NvId_D3D12_CopyTileMappings = 0x47F78194;
        private const uint NvId_D3D12_ResourceAliasingBarrier = 0xB942BAB7;
        private const uint NvId_D3D12_CaptureUAVInfo = 0x6E5EA9DB;
        private const uint NvId_D3D11_GetResourceGPUVirtualAddressEx = 0xAF6D14DA;
        private const uint NvId_D3D11_EnumerateMetaCommands = 0xC7453BA8;
        private const uint NvId_D3D11_CreateMetaCommand = 0xF505FBA0;
        private const uint NvId_D3D11_InitializeMetaCommand = 0xAEC629E9;
        private const uint NvId_D3D11_ExecuteMetaCommand = 0x82236C47;
        private const uint NvId_D3D12_EnumerateMetaCommands = 0xCD9141D8;
        private const uint NvId_D3D12_CreateMetaCommand = 0xEB29634B;
        private const uint NvId_D3D12_InitializeMetaCommand = 0xA4125399;
        private const uint NvId_D3D12_ExecuteMetaCommand = 0xDE24FC3D;
        private const uint NvId_D3D12_CreateCommittedResource = 0x27E98AE;
        private const uint NvId_D3D12_GetCopyableFootprints = 0xF6305EB5;
        private const uint NvId_D3D12_CopyTextureRegion = 0x82B91B25;
        private const uint NvId_D3D12_IsNvShaderExtnOpCodeSupported = 0x3DFACEC8;
        private const uint NvId_D3D12_GetOptimalThreadCountForMesh = 0xB43995CB;
        private const uint NvId_D3D_IsGSyncCapable = 0x9C1EED78;
        private const uint NvId_D3D_IsGSyncActive = 0xE942B0FF;
        private const uint NvId_D3D1x_DisableShaderDiskCache = 0xD0CBCA7D;
        private const uint NvId_D3D11_MultiGPU_GetCaps = 0xD2D25687;
        private const uint NvId_D3D11_MultiGPU_Init = 0x17BE49E;
        private const uint NvId_D3D11_CreateMultiGPUDevice = 0xBDB20007;
        private const uint NvId_D3D_QuerySinglePassStereoSupport = 0x6F5F0A6D;
        private const uint NvId_D3D_SetSinglePassStereoMode = 0xA39E6E6E;
        private const uint NvId_D3D12_QuerySinglePassStereoSupport = 0x3B03791B;
        private const uint NvId_D3D12_SetSinglePassStereoMode = 0x83556D87;
        private const uint NvId_D3D_QueryMultiViewSupport = 0xB6E0A41C;
        private const uint NvId_D3D_SetMultiViewMode = 0x8285C8DA;
        private const uint NvId_D3D_QueryModifiedWSupport = 0xCBF9F4F5;
        private const uint NvId_D3D_SetModifiedWMode = 0x6EA4BF4;
        private const uint NvId_D3D12_QueryModifiedWSupport = 0x51235248;
        private const uint NvId_D3D12_SetModifiedWMode = 0xE1FDABA7;
        private const uint NvId_D3D_CreateLateLatchObject = 0x2DB27D09;
        private const uint NvId_D3D_QueryLateLatchSupport = 0x8CECA0EC;
        private const uint NvId_D3D_RegisterDevice = 0x8C02C4D0;
        private const uint NvId_D3D11_MultiDrawInstancedIndirect = 0xD4E26BBF;
        private const uint NvId_D3D11_MultiDrawIndexedInstancedIndirect = 0x59E890F9;
        private const uint NvId_D3D_ImplicitSLIControl = 0x2AEDE111;
        private const uint NvId_D3D12_UseDriverHeapPriorities = 0xF0D978A8;
        private const uint NvId_D3D12_Mosaic_GetCompanionAllocations = 0xA46022C7;
        private const uint NvId_D3D12_Mosaic_GetViewportAndGpuPartitions = 0xB092B818;
        private const uint NvId_D3D1x_GetGraphicsCapabilities = 0x52B1499A;
        private const uint NvId_D3D12_GetGraphicsCapabilities = 0x1E87354;
        private const uint NvId_D3D11_RSSetExclusiveScissorRects = 0xAE4D73EF;
        private const uint NvId_D3D11_RSSetViewportsPixelShadingRates = 0x34F7938F;
        private const uint NvId_D3D11_CreateShadingRateResourceView = 0x99CA2DFF;
        private const uint NvId_D3D11_RSSetShadingRateResourceView = 0x1B0C2F83;
        private const uint NvId_D3D11_RSGetPixelShadingRateSampleOrder = 0x92442A1;
        private const uint NvId_D3D11_RSSetPixelShadingRateSampleOrder = 0xA942373A;
        private const uint NvId_D3D_InitializeVRSHelper = 0x4780D70B;
        private const uint NvId_D3D_InitializeNvGazeHandler = 0x5B3B7479;
        private const uint NvId_D3D_InitializeSMPAssist = 0x42763D0C;
        private const uint NvId_D3D_QuerySMPAssistSupport = 0xC57921DE;
        private const uint NvId_D3D_GetSleepStatus = 0xAEF96CA1;
        private const uint NvId_D3D_SetSleepMode = 0xAC1CA9E0;
        private const uint NvId_D3D_Sleep = 0x852CD1D2;
        private const uint NvId_D3D_GetLatency = 0x1A587F9C;
        private const uint NvId_D3D_SetLatencyMarker = 0xD9984C05;
        private const uint NvId_D3D12_CreateCubinComputeShader = 0x2A2C79E8;
        private const uint NvId_D3D12_CreateCubinComputeShaderEx = 0x3151211B;
        private const uint NvId_D3D12_CreateCubinComputeShaderWithName = 0x1DC7261F;
        private const uint NvId_D3D12_LaunchCubinShader = 0x5C52BB86;
        private const uint NvId_D3D12_DestroyCubinComputeShader = 0x7FB785BA;
        private const uint NvId_D3D12_GetCudaTextureObject = 0x80403FC9;
        private const uint NvId_D3D12_GetCudaSurfaceObject = 0x48F5B2EE;
        private const uint NvId_D3D12_IsFatbinPTXSupported = 0x70C07832;
        private const uint NvId_D3D11_CreateCubinComputeShader = 0xED98181;
        private const uint NvId_D3D11_CreateCubinComputeShaderEx = 0x32C2A0F6;
        private const uint NvId_D3D11_CreateCubinComputeShaderWithName = 0xB672BE19;
        private const uint NvId_D3D11_LaunchCubinShader = 0x427E236D;
        private const uint NvId_D3D11_DestroyCubinComputeShader = 0x1682C86;
        private const uint NvId_D3D11_IsFatbinPTXSupported = 0x6086BD93;
        private const uint NvId_D3D11_CreateUnorderedAccessView = 0x74A497A1;
        private const uint NvId_D3D11_CreateShaderResourceView = 0x65CB431E;
        private const uint NvId_D3D11_CreateSamplerState = 0x89ECA416;
        private const uint NvId_D3D11_GetCudaTextureObject = 0x9006FA68;
        private const uint NvId_D3D11_GetResourceGPUVirtualAddress = 0x1819B423;
        private const uint NvId_VIO_GetCapabilities = 0x1DC91303;
        private const uint NvId_VIO_Open = 0x44EE4841;
        private const uint NvId_VIO_Close = 0xD01BD237;
        private const uint NvId_VIO_Status = 0xE6CE4F1;
        private const uint NvId_VIO_SyncFormatDetect = 0x118D48A3;
        private const uint NvId_VIO_GetConfig = 0xD34A789B;
        private const uint NvId_VIO_SetConfig = 0xE4EEC07;
        private const uint NvId_VIO_SetCSC = 0xA1EC8D74;
        private const uint NvId_VIO_GetCSC = 0x7B0D72A3;
        private const uint NvId_VIO_SetGamma = 0x964BF452;
        private const uint NvId_VIO_GetGamma = 0x51D53D06;
        private const uint NvId_VIO_SetSyncDelay = 0x2697A8D1;
        private const uint NvId_VIO_GetSyncDelay = 0x462214A9;
        private const uint NvId_VIO_GetPCIInfo = 0xB981D935;
        private const uint NvId_VIO_IsRunning = 0x96BD040E;
        private const uint NvId_VIO_Start = 0xCDE8E1A3;
        private const uint NvId_VIO_Stop = 0x6BA2A5D6;
        private const uint NvId_VIO_IsFrameLockModeCompatible = 0x7BF0A94D;
        private const uint NvId_VIO_EnumDevices = 0xFD7C5557;
        private const uint NvId_VIO_QueryTopology = 0x869534E2;
        private const uint NvId_VIO_EnumSignalFormats = 0xEAD72FE4;
        private const uint NvId_VIO_EnumDataFormats = 0x221FA8E8;
        private const uint NvId_Stereo_CreateConfigurationProfileRegistryKey = 0xBE7692EC;
        private const uint NvId_Stereo_DeleteConfigurationProfileRegistryKey = 0xF117B834;
        private const uint NvId_Stereo_SetConfigurationProfileValue = 0x24409F48;
        private const uint NvId_Stereo_DeleteConfigurationProfileValue = 0x49BCEECF;
        private const uint NvId_Stereo_Enable = 0x239C4545;
        private const uint NvId_Stereo_Disable = 0x2EC50C2B;
        private const uint NvId_Stereo_IsEnabled = 0x348FF8E1;
        private const uint NvId_Stereo_GetStereoSupport = 0x296C434D;
        private const uint NvId_Stereo_CreateHandleFromIUnknown = 0xAC7E37F4;
        private const uint NvId_Stereo_DestroyHandle = 0x3A153134;
        private const uint NvId_Stereo_Activate = 0xF6A1AD68;
        private const uint NvId_Stereo_Deactivate = 0x2D68DE96;
        private const uint NvId_Stereo_IsActivated = 0x1FB0BC30;
        private const uint NvId_Stereo_GetSeparation = 0x451F2134;
        private const uint NvId_Stereo_SetSeparation = 0x5C069FA3;
        private const uint NvId_Stereo_DecreaseSeparation = 0xDA044458;
        private const uint NvId_Stereo_IncreaseSeparation = 0xC9A8ECEC;
        private const uint NvId_Stereo_GetConvergence = 0x4AB00934;
        private const uint NvId_Stereo_SetConvergence = 0x3DD6B54B;
        private const uint NvId_Stereo_DecreaseConvergence = 0x4C87E317;
        private const uint NvId_Stereo_IncreaseConvergence = 0xA17DAABE;
        private const uint NvId_Stereo_GetFrustumAdjustMode = 0xE6839B43;
        private const uint NvId_Stereo_SetFrustumAdjustMode = 0x7BE27FA2;
        private const uint NvId_Stereo_CaptureJpegImage = 0x932CB140;
        private const uint NvId_Stereo_InitActivation = 0xC7177702;
        private const uint NvId_Stereo_Trigger_Activation = 0xD6C6CD2;
        private const uint NvId_Stereo_CapturePngImage = 0x8B7E99B5;
        private const uint NvId_Stereo_ReverseStereoBlitControl = 0x3CD58F89;
        private const uint NvId_Stereo_SetNotificationMessage = 0x6B9B409E;
        private const uint NvId_Stereo_SetActiveEye = 0x96EEA9F8;
        private const uint NvId_Stereo_SetDriverMode = 0x5E8F0BEC;
        private const uint NvId_Stereo_GetEyeSeparation = 0xCE653127;
        private const uint NvId_Stereo_IsWindowedModeSupported = 0x40C8ED5E;
        private const uint NvId_Stereo_SetSurfaceCreationMode = 0xF5DCFCBA;
        private const uint NvId_Stereo_GetSurfaceCreationMode = 0x36F1C736;
        private const uint NvId_Stereo_Debug_WasLastDrawStereoized = 0xED4416C5;
        private const uint NvId_Stereo_SetDefaultProfile = 0x44F0ECD1;
        private const uint NvId_Stereo_GetDefaultProfile = 0x624E21C2;
        private const uint NvId_D3D1x_CreateSwapChain = 0x1BC21B66;
        private const uint NvId_D3D9_CreateSwapChain = 0x1A131E09;
        private const uint NvId_DRS_CreateSession = 0x694D52E;
        private const uint NvId_DRS_DestroySession = 0xDAD9CFF8;
        private const uint NvId_DRS_LoadSettings = 0x375DBD6B;
        private const uint NvId_DRS_SaveSettings = 0xFCBC7E14;
        private const uint NvId_DRS_LoadSettingsFromFile = 0xD3EDE889;
        private const uint NvId_DRS_SaveSettingsToFile = 0x2BE25DF8;
        private const uint NvId_DRS_CreateProfile = 0xCC176068;
        private const uint NvId_DRS_DeleteProfile = 0x17093206;
        private const uint NvId_DRS_SetCurrentGlobalProfile = 0x1C89C5DF;
        private const uint NvId_DRS_GetCurrentGlobalProfile = 0x617BFF9F;
        private const uint NvId_DRS_GetProfileInfo = 0x61CD6FD6;
        private const uint NvId_DRS_SetProfileInfo = 0x16ABD3A9;
        private const uint NvId_DRS_FindProfileByName = 0x7E4A9A0B;
        private const uint NvId_DRS_EnumProfiles = 0xBC371EE0;
        private const uint NvId_DRS_GetNumProfiles = 0x1DAE4FBC;
        private const uint NvId_DRS_CreateApplication = 0x4347A9DE;
        private const uint NvId_DRS_DeleteApplicationEx = 0xC5EA85A1;
        private const uint NvId_DRS_DeleteApplication = 0x2C694BC6;
        private const uint NvId_DRS_GetApplicationInfo = 0xED1F8C69;
        private const uint NvId_DRS_EnumApplications = 0x7FA2173A;
        private const uint NvId_DRS_FindApplicationByName = 0xEEE566B2;
        private const uint NvId_DRS_SetSetting = 0x577DD202;
        private const uint NvId_DRS_GetSetting = 0x73BF8338;
        private const uint NvId_DRS_EnumSettings = 0xAE3039DA;
        private const uint NvId_DRS_EnumAvailableSettingIds = 0xF020614A;
        private const uint NvId_DRS_EnumAvailableSettingValues = 0x2EC39F90;
        private const uint NvId_DRS_GetSettingIdFromName = 0xCB7309CD;
        private const uint NvId_DRS_GetSettingNameFromId = 0xD61CBE6E;
        private const uint NvId_DRS_DeleteProfileSetting = 0xE4A26362;
        private const uint NvId_DRS_RestoreAllDefaults = 0x5927B094;
        private const uint NvId_DRS_RestoreProfileDefault = 0xFA5F6134;
        private const uint NvId_DRS_RestoreProfileDefaultSetting = 0x53F0381E;
        private const uint NvId_DRS_GetBaseProfile = 0xDA8466A0;
        private const uint NvId_SYS_GetChipSetInfo = 0x53DABBCA;
        private const uint NvId_SYS_GetLidAndDockInfo = 0xCDA14D8A;
        private const uint NvId_SYS_GetDisplayIdFromGpuAndOutputId = 0x8F2BAB4;
        private const uint NvId_SYS_GetGpuAndOutputIdFromDisplayId = 0x112BA1A5;
        private const uint NvId_SYS_GetPhysicalGpuFromDisplayId = 0x9EA74659;
        private const uint NvId_SYS_GetDisplayDriverInfo = 0x721FACEB;
        private const uint NvId_GPU_ClientRegisterForUtilizationSampleUpdates = 0xADEEAF67;
        private const uint NvId_Unload = 0xD7C61344;

        #endregion

        #region Private Internal NvAPI Functions

        private const UInt32 NvId_3D_GetProperty = 0x8061A4B1;
        private const UInt32 NvId_3D_GetPropertyRange = 0x0B85DE27C;
        private const UInt32 NvId_3D_SetProperty = 0x0C9175E8D;
        private const UInt32 NvId_AccessDisplayDriverRegistry = 0xF5579360;
        private const UInt32 NvId_Coproc_GetApplicationCoprocInfo = 0x79232685;
        private const UInt32 NvId_Coproc_GetCoprocInfoFlagsEx = 0x69A9874D;
        private const UInt32 NvId_Coproc_GetCoprocStatus = 0x1EFC3957;
        private const UInt32 NvId_Coproc_NotifyCoprocPowerState = 0x0CADCB956;
        private const UInt32 NvId_Coproc_SetCoprocInfoFlagsEx = 0x0F4C863AC;
        private const UInt32 NvId_CreateUnAttachedDisplayFromDisplay = 0xA0C72EE4;
        private const UInt32 NvId_D3D_CreateQuery = 0x5D19BCA4;
        private const UInt32 NvId_D3D_DestroyQuery = 0x0C8FF7258;
        private const UInt32 NvId_D3D_Query_Begin = 0x0E5A9AAE0;
        private const UInt32 NvId_D3D_Query_End = 0x2AC084FA;
        private const UInt32 NvId_D3D_Query_GetData = 0x0F8B53C69;
        private const UInt32 NvId_D3D_Query_GetDataSize = 0x0F2A54796;
        private const UInt32 NvId_D3D_Query_GetType = 0x4ACEEAF7;
        private const UInt32 NvId_D3D_RegisterApp = 0x0D44D3C4E;
        private const UInt32 NvId_D3D10_AliasPrimaryAsTexture = 0x8AAC133D;
        private const UInt32 NvId_D3D10_BeginShareResource = 0x35233210;
        private const UInt32 NvId_D3D10_BeginShareResourceEx = 0x0EF303A9D;
        private const UInt32 NvId_D3D10_CreateDevice = 0x2DE11D61;
        private const UInt32 NvId_D3D10_CreateDeviceAndSwapChain = 0x5B803DAF;
        private const UInt32 NvId_D3D10_EndShareResource = 0x0E9C5853;
        private const UInt32 NvId_D3D10_GetRenderedCursorAsBitmap = 0x0CAC3CE5D;
        private const UInt32 NvId_D3D10_ProcessCallbacks = 0x0AE9C2019;
        private const UInt32 NvId_D3D10_SetPrimaryFlipChainCallbacks = 0x73EB9329;
        private const UInt32 NvId_D3D11_BeginShareResource = 0x121BDC6;
        private const UInt32 NvId_D3D11_EndShareResource = 0x8FFB8E26;
        private const UInt32 NvId_D3D1x_IFR_SetUpTargetBufferToSys = 0x473F7828;
        private const UInt32 NvId_D3D1x_IFR_TransferRenderTarget = 0x9FBAE4EB;
        private const UInt32 NvId_D3D9_AliasPrimaryAsTexture = 0x13C7112E;
        private const UInt32 NvId_D3D9_AliasPrimaryFromDevice = 0x7C20C5BE;
        private const UInt32 NvId_D3D9_CreatePathContextNV = 0x0A342F682;
        private const UInt32 NvId_D3D9_CreatePathNV = 0x71329DF3;
        private const UInt32 NvId_D3D9_CreateRenderTarget = 0x0B3827C8;
        private const UInt32 NvId_D3D9_CreateTexture = 0x0D5E13573;
        private const UInt32 NvId_D3D9_CreateVideo = 0x89FFD9A3;
        private const UInt32 NvId_D3D9_CreateVideoBegin = 0x84C9D553;
        private const UInt32 NvId_D3D9_CreateVideoEnd = 0x0B476BF61;
        private const UInt32 NvId_D3D9_DeletePathNV = 0x73E0019A;
        private const UInt32 NvId_D3D9_DestroyPathContextNV = 0x667C2929;
        private const UInt32 NvId_D3D9_DMA = 0x962B8AF6;
        private const UInt32 NvId_D3D9_DrawPathNV = 0x13199B3D;
        private const UInt32 NvId_D3D9_EnableStereo = 0x492A6954;
        private const UInt32 NvId_D3D9_EnumVideoFeatures = 0x1DB7C52C;
        private const UInt32 NvId_D3D9_FreeVideo = 0x3111BED1;
        private const UInt32 NvId_D3D9_GetCurrentRenderTargetHandle = 0x22CAD61;
        private const UInt32 NvId_D3D9_GetCurrentZBufferHandle = 0x0B380F218;
        private const UInt32 NvId_D3D9_GetIndexBufferHandle = 0x0FC5A155B;
        private const UInt32 NvId_D3D9_GetOverlaySurfaceHandles = 0x6800F5FC;
        private const UInt32 NvId_D3D9_GetSLIInfo = 0x694BFF4D;
        private const UInt32 NvId_D3D9_GetTextureHandle = 0x0C7985ED5;
        private const UInt32 NvId_D3D9_GetVertexBufferHandle = 0x72B19155;
        private const UInt32 NvId_D3D9_GetVideoCapabilities = 0x3D596B93;
        private const UInt32 NvId_D3D9_GetVideoState = 0x0A4527BF8;
        private const UInt32 NvId_D3D9_GPUBasedCPUSleep = 0x0D504DDA7;
        private const UInt32 NvId_D3D9_GpuSyncAcquire = 0x0D00B8317;
        private const UInt32 NvId_D3D9_GpuSyncEnd = 0x754033F0;
        private const UInt32 NvId_D3D9_GpuSyncGetHandleSize = 0x80C9FD3B;
        private const UInt32 NvId_D3D9_GpuSyncInit = 0x6D6FDAD4;
        private const UInt32 NvId_D3D9_GpuSyncMapIndexBuffer = 0x12EE68F2;
        private const UInt32 NvId_D3D9_GpuSyncMapSurfaceBuffer = 0x2AB714AB;
        private const UInt32 NvId_D3D9_GpuSyncMapTexBuffer = 0x0CDE4A28A;
        private const UInt32 NvId_D3D9_GpuSyncMapVertexBuffer = 0x0DBC803EC;
        private const UInt32 NvId_D3D9_GpuSyncRelease = 0x3D7A86BB;
        private const UInt32 NvId_D3D9_IFR_SetUpTargetBufferToNV12BLVideoSurface = 0x0CFC92C15;
        private const UInt32 NvId_D3D9_IFR_SetUpTargetBufferToSys = 0x55255D05;
        private const UInt32 NvId_D3D9_IFR_TransferRenderTarget = 0x0AB7C2DC;
        private const UInt32 NvId_D3D9_IFR_TransferRenderTargetToNV12BLVideoSurface = 0x5FE72F64;
        private const UInt32 NvId_D3D9_Lock = 0x6317345C;
        private const UInt32 NvId_D3D9_NVFBC_GetStatus = 0x0bd3eb475;
        private const UInt32 NvId_D3D9_PathClearDepthNV = 0x157E45C4;
        private const UInt32 NvId_D3D9_PathDepthNV = 0x0FCB16330;
        private const UInt32 NvId_D3D9_PathEnableColorWriteNV = 0x3E2804A2;
        private const UInt32 NvId_D3D9_PathEnableDepthTestNV = 0x0E99BA7F3;
        private const UInt32 NvId_D3D9_PathMatrixNV = 0x0D2F6C499;
        private const UInt32 NvId_D3D9_PathParameterfNV = 0x0F7FF00C1;
        private const UInt32 NvId_D3D9_PathParameteriNV = 0x0FC31236C;
        private const UInt32 NvId_D3D9_PathVerticesNV = 0x0C23DF926;
        private const UInt32 NvId_D3D9_PresentSurfaceToDesktop = 0x0F7029C5;
        private const UInt32 NvId_D3D9_PresentVideo = 0x5CF7F862;
        private const UInt32 NvId_D3D9_QueryAAOverrideMode = 0x0DDF5643C;
        private const UInt32 NvId_D3D9_QueryVideoInfo = 0x1E6634B3;
        private const UInt32 NvId_D3D9_SetGamutData = 0x2BBDA32E;
        private const UInt32 NvId_D3D9_SetPitchSurfaceCreation = 0x18CDF365;
        private const UInt32 NvId_D3D9_SetResourceHInt32 = 0x905F5C27;
        private const UInt32 NvId_D3D9_SetSLIMode = 0x0BFDC062C;
        private const UInt32 NvId_D3D9_SetSurfaceCreationLayout = 0x5609B86A;
        private const UInt32 NvId_D3D9_SetVideoState = 0x0BD4BC56F;
        private const UInt32 NvId_D3D9_StretchRect = 0x0AEAECD41;
        private const UInt32 NvId_D3D9_Unlock = 0x0C182027E;
        private const UInt32 NvId_D3D9_VideoSurfaceEncryptionControl = 0x9D2509EF;
        private const UInt32 NvId_DeleteCustomDisplay = 0x0E7CB998D;
        private const UInt32 NvId_DeleteUnderscanConfig = 0x0F98854C8;
        private const UInt32 NvId_Disp_DpAuxChannelControl = 0x8EB56969;
        private const UInt32 NvId_DISP_EnumHDMIStereoModes = 0x0D2CCF5D6;
        private const UInt32 NvId_DISP_GetDisplayBlankingState = 0x63E5D8DB;
        private const UInt32 NvId_DISP_GetHCloneTopology = 0x47BAD137;
        private const UInt32 NvId_DISP_GetVirtualModeData = 0x3230D69A;
        private const UInt32 NvId_DISP_OverrideDisplayModeList = 0x291BFF2;
        private const UInt32 NvId_DISP_SetDisplayBlankingState = 0x1E17E29B;
        private const UInt32 NvId_DISP_SetHCloneTopology = 0x61041C24;
        private const UInt32 NvId_DISP_ValidateHCloneTopology = 0x5F4C2664;
        private const UInt32 NvId_EnumCustomDisplay = 0x42892957;
        private const UInt32 NvId_EnumUnderscanConfig = 0x4144111A;
        private const UInt32 NvId_GetDisplayDriverBuildTitle = 0x7562E947;
        private const UInt32 NvId_GetDisplayDriverCompileType = 0x988AEA78;
        private const UInt32 NvId_GetDisplayDriverMemoryInfo = 0x774AA982;
        private const UInt32 NvId_GetDisplayDriverRegistryPath = 0x0E24CEEE;
        private const UInt32 NvId_GetDisplayDriverSecurityLevel = 0x9D772BBA;
        private const UInt32 NvId_GetDisplayFeatureConfig = 0x8E985CCD;
        private const UInt32 NvId_GetDisplayFeatureConfigDefaults = 0x0F5F4D01;
        private const UInt32 NvId_GetDisplayPosition = 0x6BB1EE5D;
        private const UInt32 NvId_GetDisplaySettings = 0x0DC27D5D4;
        private const UInt32 NvId_GetDriverMemoryInfo = 0x2DC95125;
        private const UInt32 NvId_GetDriverModel = 0x25EEB2C4;
        private const UInt32 NvId_GetDVCInfo = 0x4085DE45;
        private const UInt32 NvId_GetDVCInfoEx = 0x0E45002D;
        private const UInt32 NvId_GetHDCPLinkParameters = 0x0B3BB0772;
        private const UInt32 NvId_GetHUEInfo = 0x95B64341;
        private const UInt32 NvId_GetHybridMode = 0x0E23B68C1;
        private const UInt32 NvId_GetImageSharpeningInfo = 0x9FB063DF;
        private const UInt32 NvId_GetInfoFrame = 0x9734F1D;
        private const UInt32 NvId_GetInfoFrameState = 0x41511594;
        private const UInt32 NvId_GetInfoFrameStatePvt = 0x7FC17574;
        private const UInt32 NvId_GetInvalidGpuTopologies = 0x15658BE6;
        private const UInt32 NvId_GetLoadedMicrocodePrograms = 0x919B3136;
        private const UInt32 NvId_GetPhysicalGPUFromDisplay = 0x1890E8DA;
        private const UInt32 NvId_GetPVExtName = 0x2F5B08E0;
        private const UInt32 NvId_GetPVExtProfile = 0x1B1B9A16;
        private const UInt32 NvId_GetScalingCaps = 0x8E875CF9;
        private const UInt32 NvId_GetTiming = 0x0AFC4833E;
        private const UInt32 NvId_GetTopologyDisplayGPU = 0x813D89A8;
        private const UInt32 NvId_GetTVEncoderControls = 0x5757474A;
        private const UInt32 NvId_GetTVOutputBorderColor = 0x6DFD1C8C;
        private const UInt32 NvId_GetTVOutputInfo = 0x30C805D5;
        private const UInt32 NvId_GetUnAttachedDisplayDriverRegistryPath = 0x633252D8;
        private const UInt32 NvId_GetValidGpuTopologies = 0x5DFAB48A;
        private const UInt32 NvId_GetVideoState = 0x1C5659CD;
        private const UInt32 NvId_GPS_GetPerfSensors = 0x271C1109;
        private const UInt32 NvId_GPS_GetPowerSteeringStatus = 0x540EE82E;
        private const UInt32 NvId_GPS_GetThermalLimit = 0x583113ED;
        private const UInt32 NvId_GPS_GetVPStateCap = 0x71913023;
        private const UInt32 NvId_GPS_SetPowerSteeringStatus = 0x9723D3A2;
        private const UInt32 NvId_GPS_SetThermalLimit = 0x0C07E210F;
        private const UInt32 NvId_GPS_SetVPStateCap = 0x68888EB4;
        private const UInt32 NvId_GPU_ClearPCIELinkAERInfo = 0x521566BB;
        private const UInt32 NvId_GPU_ClearPCIELinkErrorInfo = 0x8456FF3D;
        private const UInt32 NvId_GPU_ClientPowerPoliciesGetInfo = 0x34206D86;
        private const UInt32 NvId_GPU_ClientPowerPoliciesGetStatus = 0x70916171;
        private const UInt32 NvId_GPU_ClientPowerPoliciesSetStatus = 0x0AD95F5ED;
        private const UInt32 NvId_GPU_ClientPowerTopologyGetInfo = 0x0A4DFD3F2;
        private const UInt32 NvId_GPU_ClientPowerTopologyGetStatus = 0x0EDCF624E;
        private const UInt32 NvId_GPU_EnableDynamicPstates = 0x0FA579A0F;
        private const UInt32 NvId_GPU_EnableOverclockedPstates = 0x0B23B70EE;
        private const UInt32 NvId_GPU_Get_DisplayPort_DongleInfo = 0x76A70E8D;
        private const UInt32 NvId_GPU_GetAllClocks = 0x1BD69F49;
        private const UInt32 NvId_GPU_GetAllGpusOnSameBoard = 0x4DB019E6;
        private const UInt32 NvId_GPU_GetBarInfo = 0xE4B701E3;
        private const UInt32 NvId_GPU_GetClockBoostLock = 0xe440b867; // unknown name; NVAPI_ID_CURVE_GET
        private const UInt32 NvId_GPU_GetClockBoostMask = 0x507b4b59;
        private const UInt32 NvId_GPU_GetClockBoostRanges = 0x64b43a6a;
        private const UInt32 NvId_GPU_GetClockBoostTable = 0x23f1b133;
        private const UInt32 NvId_GPU_GetColorSpaceConversion = 0x8159E87A;
        private const UInt32 NvId_GPU_GetConnectorInfo = 0x4ECA2C10;
        private const UInt32 NvId_GPU_GetCoolerPolicyTable = 0x518A32C;
        private const UInt32 NvId_GPU_GetCoolerSettings = 0x0DA141340;
        private const UInt32 NvId_GPU_GetCoreVoltageBoostPercent = 0x9df23ca1;
        private const UInt32 NvId_GPU_GetCurrentFanSpeedLevel = 0x0BD71F0C9;
        private const UInt32 NvId_GPU_GetCurrentThermalLevel = 0x0D2488B79;
        private const UInt32 NvId_GPU_GetCurrentVoltage = 0x465f9bcf;
        private const UInt32 NvId_GPU_GetDeepIdleState = 0x1AAD16B4;
        private const UInt32 NvId_GPU_GetDeviceDisplayMode = 0x0D2277E3A;
        private const UInt32 NvId_GPU_GetDisplayUnderflowStatus = 0xED9E8057;
        private const UInt32 NvId_GPU_GetDitherControl = 0x932AC8FB;
        private const UInt32 NvId_GPU_GetExtendedMinorRevision = 0x25F17421;
        private const UInt32 NvId_GPU_GetFBWidthAndLocation = 0x11104158;
        private const UInt32 NvId_GPU_GetFlatPanelInfo = 0x36CFF969;
        private const UInt32 NvId_GPU_GetFoundry = 0x5D857A00;
        private const UInt32 NvId_GPU_GetFrameBufferCalibrationLockFailures = 0x524B9773;
        private const UInt32 NvId_GPU_GetHardwareQualType = 0xF91E777B;
        private const UInt32 NvId_GPU_GetHybridControllerInfo = 0xD26B8A58;
        private const UInt32 NvId_GPU_GetLogicalFBWidthAndLocation = 0x8efc0978;
        private const UInt32 NvId_GPU_GetManufacturingInfo = 0xA4218928;
        private const UInt32 NvId_GPU_GetMemPartitionMask = 0x329D77CD;
        private const UInt32 NvId_GPU_GetMXMBlock = 0xB7AB19B9;
        private const UInt32 NvId_GPU_GetPartitionCount = 0x86F05D7A;
        private const UInt32 NvId_GPU_GetPCIEInfo = 0xE3795199;
        private const UInt32 NvId_GPU_GetPerfClocks = 0x1EA54A3B;
        private const UInt32 NvId_GPU_GetPerfHybridMode = 0x5D7CCAEB;
        private const UInt32 NvId_GPU_GetPerGpuTopologyStatus = 0x0A81F8992;
        private const UInt32 NvId_GPU_GetPixelClockRange = 0x66AF10B7;
        private const UInt32 NvId_GPU_GetPowerMizerInfo = 0x76BFA16B;
        private const UInt32 NvId_GPU_GetPSFloorSweepStatus = 0xDEE047AB;
        private const UInt32 NvId_GPU_GetPstateClientLimits = 0x88C82104;
        private const UInt32 NvId_GPU_GetPstatesInfo = 0x0BA94C56E;
        private const UInt32 NvId_GPU_GetRamBankCount = 0x17073A3C;
        private const UInt32 NvId_GPU_GetRamBusWidth = 0x7975C581;
        private const UInt32 NvId_GPU_GetRamConfigStrap = 0x51CCDB2A;
        private const UInt32 NvId_GPU_GetRamMaker = 0x42aea16a;
        private const UInt32 NvId_GPU_GetRamType = 0x57F7CAAC;
        private const UInt32 NvId_GPU_GetRawFuseData = 0xE0B1DCE9;
        private const UInt32 NvId_GPU_GetROPCount = 0xfdc129fa;
        private const UInt32 NvId_GPU_GetSampleType = 0x32E1D697;
        private const UInt32 NvId_GPU_GetSerialNumber = 0x14B83A5F;
        private const UInt32 NvId_GPU_GetShaderPipeCount = 0x63E2F56F;
        private const UInt32 NvId_GPU_GetShortName = 0xD988F0F3;
        private const UInt32 NvId_GPU_GetSMMask = 0x0EB7AF173;
        private const UInt32 NvId_GPU_GetTargetID = 0x35B5FD2F;
        private const UInt32 NvId_GPU_GetThermalPoliciesInfo = 0x00D258BB5; // private const UInt32 NvId_GPU_ClientThermalPoliciesGetInfo
        private const UInt32 NvId_GPU_GetThermalPoliciesStatus = 0x0E9C425A1;
        private const UInt32 NvId_GPU_GetThermalTable = 0xC729203C;
        private const UInt32 NvId_GPU_GetTotalSMCount = 0x0AE5FBCFE;
        private const UInt32 NvId_GPU_GetTotalSPCount = 0x0B6D62591;
        private const UInt32 NvId_GPU_GetTotalTPCCount = 0x4E2F76A8;
        private const UInt32 NvId_GPU_GetTPCMask = 0x4A35DF54;
        private const UInt32 NvId_GPU_GetUsages = 0x189a1fdf;
        private const UInt32 NvId_GPU_GetVbiosImage = 0xFC13EE11;
        private const UInt32 NvId_GPU_GetVbiosMxmVersion = 0xE1D5DABA;
        private const UInt32 NvId_GPU_GetVFPCurve = 0x21537ad4;
        private const UInt32 NvId_GPU_GetVoltageDomainsStatus = 0x0C16C7E2C;
        private const UInt32 NvId_GPU_GetVoltages = 0x7D656244;
        private const UInt32 NvId_GPU_GetVoltageStep = 0x28766157; // unsure of the name
        private const UInt32 NvId_GPU_GetVPECount = 0xD8CBF37B;
        private const UInt32 NvId_GPU_GetVSFloorSweepStatus = 0xD4F3944C;
        private const UInt32 NvId_GPU_GPIOQueryLegalPins = 0x0FAB69565;
        private const UInt32 NvId_GPU_GPIOReadFromPin = 0x0F5E10439;
        private const UInt32 NvId_GPU_GPIOWriteToPin = 0x0F3B11E68;
        private const UInt32 NvId_GPU_PerfPoliciesGetInfo = 0x409d9841;
        private const UInt32 NvId_GPU_PerfPoliciesGetStatus = 0x3d358a0c;
        private const UInt32 NvId_GPU_PhysxQueryRecommendedState = 0x7A4174F4;
        private const UInt32 NvId_GPU_PhysxSetState = 0x4071B85E;
        private const UInt32 NvId_GPU_QueryActiveApps = 0x65B1C5F5;
        private const UInt32 NvId_GPU_RestoreCoolerPolicyTable = 0x0D8C4FE63;
        private const UInt32 NvId_GPU_RestoreCoolerSettings = 0x8F6ED0FB;
        private const UInt32 NvId_GPU_SetClockBoostLock = 0x39442cfb; // unknown name; NVAPI_ID_CURVE_SET
        private const UInt32 NvId_GPU_SetClockBoostTable = 0x0733e009;
        private const UInt32 NvId_GPU_SetClocks = 0x6F151055;
        private const UInt32 NvId_GPU_SetColorSpaceConversion = 0x0FCABD23A;
        private const UInt32 NvId_GPU_SetCoolerLevels = 0x891FA0AE;
        private const UInt32 NvId_GPU_SetCoolerPolicyTable = 0x987947CD;
        private const UInt32 NvId_GPU_SetCoreVoltageBoostPercent = 0xb9306d9b;
        private const UInt32 NvId_GPU_SetCurrentPCIESpeed = 0x3BD32008;
        private const UInt32 NvId_GPU_SetCurrentPCIEWidth = 0x3F28E1B9;
        private const UInt32 NvId_GPU_SetDeepIdleState = 0x568A2292;
        private const UInt32 NvId_GPU_SetDisplayUnderflowMode = 0x387B2E41;
        private const UInt32 NvId_GPU_SetDitherControl = 0x0DF0DFCDD;
        private const UInt32 NvId_GPU_SetPerfClocks = 0x7BCF4AC;
        private const UInt32 NvId_GPU_SetPerfHybridMode = 0x7BC207F8;
        private const UInt32 NvId_GPU_SetPixelClockRange = 0x5AC7F8E5;
        private const UInt32 NvId_GPU_SetPowerMizerInfo = 0x50016C78;
        private const UInt32 NvId_GPU_SetPstateClientLimits = 0x0FDFC7D49;
        private const UInt32 NvId_GPU_SetPstates20 = 0x0F4DAE6B;
        private const UInt32 NvId_GPU_SetPstatesInfo = 0x0CDF27911;
        private const UInt32 NvId_GPU_SetThermalPoliciesStatus = 0x034C0B13D;
        private const UInt32 NvId_Hybrid_IsAppMigrationStateChangeable = 0x584CB0B6;
        private const UInt32 NvId_Hybrid_QueryBlockedMigratableApps = 0x0F4C2F8CC;
        private const UInt32 NvId_Hybrid_QueryUnblockedNonMigratableApps = 0x5F35BCB5;
        private const UInt32 NvId_Hybrid_SetAppMigrationState = 0x0FA0B9A59;
        private const UInt32 NvId_I2CReadEx = 0x4D7B0709;
        private const UInt32 NvId_I2CWriteEx = 0x283AC65A;
        private const UInt32 NvId_LoadMicrocode = 0x3119F36E;
        private const UInt32 NvId_Mosaic_ChooseGpuTopologies = 0x0B033B140;
        private const UInt32 NvId_Mosaic_EnumGridTopologies = 0x0A3C55220;
        private const UInt32 NvId_Mosaic_GetDisplayCapabilities = 0x0D58026B9;
        private const UInt32 NvId_Mosaic_GetMosaicCapabilities = 0x0DA97071E;
        private const UInt32 NvId_Mosaic_GetMosaicViewports = 0x7EBA036;
        private const UInt32 NvId_Mosaic_SetGridTopology = 0x3F113C77;
        private const UInt32 NvId_Mosaic_ValidateDisplayGridsWithSLI = 0x1ECFD263;
        private const UInt32 NvId_QueryNonMigratableApps = 0x0BB9EF1C3;
        private const UInt32 NvId_QueryUnderscanCap = 0x61D7B624;
        private const UInt32 NvId_RestartDisplayDriver = 0xB4B26B65;
        private const UInt32 NvId_RevertCustomDisplayTrial = 0x854BA405;
        private const UInt32 NvId_SaveCustomDisplay = 0x0A9062C78;
        private const UInt32 NvId_SetDisplayFeatureConfig = 0x0F36A668D;
        private const UInt32 NvId_SetDisplayPosition = 0x57D9060F;
        private const UInt32 NvId_SetDisplaySettings = 0x0E04F3D86;
        private const UInt32 NvId_SetDVCLevel = 0x172409B4;
        private const UInt32 NvId_SetDVCLevelEx = 0x4A82C2B1;
        private const UInt32 NvId_SetFrameRateNotify = 0x18919887;
        private const UInt32 NvId_SetGpuTopologies = 0x25201F3D;
        private const UInt32 NvId_SetHUEAngle = 0x0F5A0F22C;
        private const UInt32 NvId_SetHybridMode = 0x0FB22D656;
        private const UInt32 NvId_SetImageSharpeningLevel = 0x3FC9A59C;
        private const UInt32 NvId_SetInfoFrame = 0x69C6F365;
        private const UInt32 NvId_SetInfoFrameState = 0x67EFD887;
        private const UInt32 NvId_SetPVExtName = 0x4FEEB498;
        private const UInt32 NvId_SetPVExtProfile = 0x8354A8F4;
        private const UInt32 NvId_SetTopologyDisplayGPU = 0xF409D5E5;
        private const UInt32 NvId_SetTopologyFocusDisplayAndView = 0x0A8064F9;
        private const UInt32 NvId_SetTVEncoderControls = 0x0CA36A3AB;
        private const UInt32 NvId_SetTVOutputBorderColor = 0x0AED02700;
        private const UInt32 NvId_SetUnderscanConfig = 0x3EFADA1D;
        private const UInt32 NvId_SetVideoState = 0x54FE75A;
        private const UInt32 NvId_Stereo_AppHandShake = 0x8C610BDA;
        private const UInt32 NvId_Stereo_ForceToScreenDepth = 0x2D495758;
        private const UInt32 NvId_Stereo_GetCursorSeparation = 0x72162B35;
        private const UInt32 NvId_Stereo_GetPixelShaderConstantB = 0x0C79333AE;
        private const UInt32 NvId_Stereo_GetPixelShaderConstantF = 0x0D4974572;
        private const UInt32 NvId_Stereo_GetPixelShaderConstantI = 0x0ECD8F8CF;
        private const UInt32 NvId_Stereo_GetStereoCaps = 0x0DFC063B7;
        private const UInt32 NvId_Stereo_GetVertexShaderConstantB = 0x712BAA5B;
        private const UInt32 NvId_Stereo_GetVertexShaderConstantF = 0x622FDC87;
        private const UInt32 NvId_Stereo_GetVertexShaderConstantI = 0x5A60613A;
        private const UInt32 NvId_Stereo_HandShake_Message_Control = 0x315E0EF0;
        private const UInt32 NvId_Stereo_HandShake_Trigger_Activation = 0x0B30CD1A7;
        private const UInt32 NvId_Stereo_Is3DCursorSupported = 0x0D7C9EC09;
        private const UInt32 NvId_Stereo_SetCursorSeparation = 0x0FBC08FC1;
        private const UInt32 NvId_Stereo_SetPixelShaderConstantB = 0x0BA6109EE;
        private const UInt32 NvId_Stereo_SetPixelShaderConstantF = 0x0A9657F32;
        private const UInt32 NvId_Stereo_SetPixelShaderConstantI = 0x912AC28F;
        private const UInt32 NvId_Stereo_SetVertexShaderConstantB = 0x5268716F;
        private const UInt32 NvId_Stereo_SetVertexShaderConstantF = 0x416C07B3;
        private const UInt32 NvId_Stereo_SetVertexShaderConstantI = 0x7923BA0E;
        private const UInt32 NvId_SYS_GetChipSetTopologyStatus = 0x8A50F126;
        private const UInt32 NvId_SYS_GetSliApprovalCookie = 0xB539A26E;
        private const UInt32 NvId_SYS_SetPostOutput = 0xD3A092B1;
        private const UInt32 NvId_SYS_VenturaGetCoolingBudget = 0x0C9D86E33;
        private const UInt32 NvId_SYS_VenturaGetPowerReading = 0x63685979;
        private const UInt32 NvId_SYS_VenturaGetState = 0x0CB7C208D;
        private const UInt32 NvId_SYS_VenturaSetCoolingBudget = 0x85FF5A15;
        private const UInt32 NvId_SYS_VenturaSetState = 0x0CE2E9D9;
        private const UInt32 NvId_TryCustomDisplay = 0x0BF6C1762;
        private const UInt32 NvId_VideoGetStereoInfo = 0x8E1F8CFE;
        private const UInt32 NvId_VideoSetStereoInfo = 0x97063269;
        private const UInt32 NvId_GPU_ClientFanCoolersGetInfo = 0xfb85b01e;
        private const UInt32 NvId_GPU_ClientFanCoolersGetStatus = 0x35aed5e8;
        private const UInt32 NvId_GPU_ClientFanCoolersGetControl = 0x814b209f;
        private const UInt32 NvId_GPU_ClientFanCoolersSetControl = 0xa58971a5;

        #endregion
        private const UInt32 NvId_Initialize = 0x150E828;

        #region General NVAPI Functions
        // QueryInterface

        private delegate IntPtr QueryInterfaceDelegate(UInt32 id);
        private static readonly QueryInterfaceDelegate QueryInterface;

        // Initialize
        private delegate NVAPI_STATUS InitializeDelegate();
        private static readonly InitializeDelegate InitializeInternal;

        // Unload
        private delegate NVAPI_STATUS UnloadDelegate();
        private static readonly UnloadDelegate UnloadInternal;

        // GetErrorMessage
        private delegate NVAPI_STATUS GetErrorMessageDelegate(NVAPI_STATUS nr, StringBuilder szDesc);
        private static readonly GetErrorMessageDelegate GetErrorMessageInternal;

        /// <summary>
        /// This function converts an NvAPI error code Int32o a null terminated string.
        /// </summary>
        /// <param name="nr">The error code to convert</param>
        /// <param name="szDesc">The string corresponding to the error code</param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GetErrorMessage(NVAPI_STATUS nr, out string szDesc)
        {
            StringBuilder builder = new StringBuilder((Int32)NV_SHORT_STRING_MAX);

            NVAPI_STATUS status;
            if (GetErrorMessageInternal != null) { status = GetErrorMessageInternal(nr, builder); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }
            szDesc = builder.ToString();

            return status;
        }

        // GetInterfaceVersionString
        private delegate NVAPI_STATUS GetInterfaceVersionStringDelegate(StringBuilder szDesc);
        private static readonly GetInterfaceVersionStringDelegate GetInterfaceVersionStringInternal;

        /// <summary>
        /// This function returns a string describing the version of the NvAPI library. The contents of the string are human readable. Do not assume a fixed format.
        /// </summary>
        /// <param name="szDesc">User readable string giving NvAPI version information</param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GetInterfaceVersionString(out string szDesc)
        {
            StringBuilder builder = new StringBuilder((Int32)NV_SHORT_STRING_MAX);

            NVAPI_STATUS status;
            if (GetErrorMessageInternal != null) { status = GetInterfaceVersionStringInternal(builder); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }
            szDesc = builder.ToString();

            return status;
        }
        #endregion


        #region Display NVAPI Functions
        // EnumNvidiaDisplayHandle
        private delegate NVAPI_STATUS EnumNvidiaDisplayHandleDelegate(Int32 thisEnum, ref DisplayHandle displayHandle);
        private static readonly EnumNvidiaDisplayHandleDelegate EnumNvidiaDisplayHandleInternal;

        /// <summary>
        /// This function returns the handle of the NVIDIA display specified by the enum index (thisEnum). The client should keep enumerating until it returns NVAPI_END_ENUMERATION.
        /// Note: Display handles can get invalidated on a modeset, so the calling applications need to renum the handles after every modeset.
        /// </summary>
        /// <param name="thisEnum">The index of the NVIDIA display.</param>
        /// <param name="displayHandle">PoInt32er to the NVIDIA display handle.</param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_EnumNvidiaDisplayHandle(Int32 thisEnum, ref DisplayHandle displayHandle)
        {
            NVAPI_STATUS status;
            if (EnumNvidiaDisplayHandleInternal != null) { status = EnumNvidiaDisplayHandleInternal(thisEnum, ref displayHandle); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }
            return status;
        }

        // EnumNvidiaUnAttachedDisplayHandle
        private delegate NVAPI_STATUS EnumNvidiaUnAttachedDisplayHandleDelegate(Int32 thisEnum, ref UnAttachedDisplayHandle pNvDispHandle);
        private static readonly EnumNvidiaUnAttachedDisplayHandleDelegate EnumNvidiaUnAttachedDisplayHandleInternal;

        /// <summary>
        /// This function returns the handle of the NVIDIA unattached display specified by the enum index (thisEnum). The client should keep enumerating until it returns error. Note: Display handles can get invalidated on a modeset, so the calling applications need to renum the handles after every modeset.
        /// </summary>
        /// <param name="thisEnum">The index of the NVIDIA display.</param>
        /// <param name="pNvDispHandle">PoInt32er to the NVIDIA display handle of the unattached display.</param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_EnumNvidiaUnAttachedDisplayHandle(Int32 thisEnum, ref UnAttachedDisplayHandle pNvDispHandle)
        {
            NVAPI_STATUS status;
            if (EnumNvidiaUnAttachedDisplayHandleInternal != null) { status = EnumNvidiaUnAttachedDisplayHandleInternal(thisEnum, ref pNvDispHandle); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }
            return status;
        }

        // GetAssociatedUnAttachedNvidiaDisplayHandle
        private delegate NVAPI_STATUS GetAssociatedNvidiaDisplayHandleDelegate(StringBuilder szDisplayName, ref DisplayHandle pNvDispHandle);
        private static readonly GetAssociatedNvidiaDisplayHandleDelegate GetAssociatedNvidiaDisplayHandleInternal;

        /// <summary>
        /// This function returns the handle of the NVIDIA display that is associated with the given display "name" (such as "\\.\DISPLAY1").
        /// </summary>
        /// <param name="szDisplayName"></param>
        /// <param name="pNvDispHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GetAssociatedNvidiaDisplayHandle(string szDisplayName, ref DisplayHandle pNvDispHandle)
        {
            StringBuilder builder = new StringBuilder((Int32)NV_SHORT_STRING_MAX);
            builder.Append(szDisplayName);

            NVAPI_STATUS status;
            if (GetAssociatedNvidiaDisplayHandleInternal != null) { status = GetAssociatedNvidiaDisplayHandleInternal(builder, ref pNvDispHandle); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }
            szDisplayName = builder.ToString();

            return status;
        }

        // GetAssociatedUnAttachedNvidiaDisplayHandle
        private delegate NVAPI_STATUS GetAssociatedUnAttachedNvidiaDisplayHandleDelegate(StringBuilder szDisplayName, ref UnAttachedDisplayHandle pNvUnAttachedDispHandle);
        private static readonly GetAssociatedUnAttachedNvidiaDisplayHandleDelegate GetAssociatedUnAttachedNvidiaDisplayHandleInternal;

        /// <summary>
        /// This function returns the handle of an unattached NVIDIA display that is associated with the given display name (such as "\\DISPLAY1").
        /// </summary>
        /// <param name="szDisplayName"></param>
        /// <param name="pNvUnAttachedDispHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GetAssociatedUnAttachedNvidiaDisplayHandle(string szDisplayName, ref UnAttachedDisplayHandle pNvUnAttachedDispHandle)
        {
            StringBuilder builder = new StringBuilder((Int32)NV_SHORT_STRING_MAX);
            builder.Append(szDisplayName);

            NVAPI_STATUS status;
            if (GetAssociatedUnAttachedNvidiaDisplayHandleInternal != null) { status = GetAssociatedUnAttachedNvidiaDisplayHandleInternal(builder, ref pNvUnAttachedDispHandle); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }
            szDisplayName = builder.ToString();

            return status;
        }
        #endregion


        // ******** IMPORTANT! This code has an error when attempting to perform the third pass as required by NVAPI documentation *********
        // ******** FOr this reason I have disabled the code as I don't actually need to get it going. ******** 
        // NVAPI_INTERFACE NvAPI_DISP_GetDisplayConfig(__inout NvU32 *pathInfoCount, __out_ecount_full_opt(*pathInfoCount) NV_DISPLAYCONFIG_PATH_INFO *pathInfo);
        private delegate NVAPI_STATUS DISP_GetDisplayConfigDelegate(
            [In][Out] ref UInt32 pathInfoCount,
            [In][Out] IntPtr pathInfoBuffer);
        private static readonly DISP_GetDisplayConfigDelegate DISP_GetDisplayConfigInternal;

        /// <summary>
        /// DESCRIPTION:     This API lets caller retrieve the current global display configuration.
        ///       USAGE:     The caller might have to call this three times to fetch all the required configuration details as follows:
        ///                  First  Pass: Caller should Call NvAPI_DISP_GetDisplayConfig() with pathInfo set to NULL to fetch pathInfoCount.
        ///                  Second Pass: Allocate memory for pathInfo with respect to the number of pathInfoCount(from First Pass) to fetch
        ///                               targetInfoCount. If sourceModeInfo is needed allocate memory or it can be initialized to NULL.
        ///             Third  Pass(Optional, only required if target information is required): Allocate memory for targetInfo with respect
        //                               to number of targetInfoCount(from Second Pass).
        /// SUPPORTED OS:  Windows 7 and higher
        /// </summary>
        /// <param name="PathInfoCount"></param>
        /// <param name="PathInfo"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DISP_GetDisplayConfig(ref UInt32 PathInfoCount, ref NV_DISPLAYCONFIG_PATH_INFO_V2[] PathInfos, bool thirdPass = false)
        {
            NVAPI_STATUS status = NVAPI_STATUS.NVAPI_OK;
            if (thirdPass)
            {
                NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[] pass2PathInfos = new NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[PathInfoCount];
                int totalTargetInfoCount = 0;
                for (Int32 x = 0; x < (Int32)PathInfoCount; x++)
                {
                    totalTargetInfoCount += (int)PathInfos[x].TargetInfoCount;
                }

                // Get the size of the each object
                int onePathInfoMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL));
                int oneSourceModeMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1));
                int onePathTargetMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL));
                int oneAdvTargetMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL));
                int oneTimingMemSize = Marshal.SizeOf(typeof(NV_TIMING_INTERNAL));
                int oneTimingExtraMemSize = Marshal.SizeOf(typeof(NV_TIMING_EXTRA_INTERNAL));

                // Figure out the size of the memory we need to allocate
                int allPathInfoMemSize = onePathInfoMemSize * (int)PathInfoCount;
                int allSourceModeMemSize = oneSourceModeMemSize * (int)PathInfoCount;
                int allPathTargetMemSize = onePathTargetMemSize * totalTargetInfoCount;
                int allAdvTargetMemSize = (oneAdvTargetMemSize + oneTimingMemSize + oneTimingExtraMemSize) * totalTargetInfoCount;
                //int allTimingMemSize = oneTimingMemSize * totalTargetInfoCount;
                //int allTimingExtraMemSize = oneTimingExtraMemSize * totalTargetInfoCount;
                //int allObjectsMemSize = allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize + allAdvTargetMemSize + allTimingMemSize + allTimingExtraMemSize;
                int allObjectsMemSize = allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize + allAdvTargetMemSize;

                IntPtr memPointer = IntPtr.Zero;

                try
                {
                    // Allocate the memory we need
                    memPointer = Marshal.AllocHGlobal(allObjectsMemSize * 2);

                    // Figure out the address of the arrays we will use
                    IntPtr pathInfoPointer = memPointer;
                    IntPtr sourceModeInfoPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize);
                    IntPtr targetInfoPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize + allSourceModeMemSize);
                    IntPtr advTargetPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize);
                    //IntPtr timingPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize + allAdvTargetMemSize);
                    //IntPtr timingExtraPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize + allAdvTargetMemSize + allTimingMemSize);

                    // Figure out each memory pointer so that we can do the memory copying item by item
                    // as we have to do it ourselves (there isn't an easy to use Marshal equivalent)
                    IntPtr currentPathInfoPointer = new IntPtr(pathInfoPointer.ToInt64());
                    IntPtr currentSourceModeInfoPointer = new IntPtr(sourceModeInfoPointer.ToInt64());
                    IntPtr currentTargetInfoPointer = new IntPtr(targetInfoPointer.ToInt64());
                    IntPtr currentAdvTargetPointer = new IntPtr(advTargetPointer.ToInt64());
                    //IntPtr currentTimingPointer = new IntPtr(timingPointer.ToInt64());
                    //IntPtr currentTimingExtraPointer = new IntPtr(timingExtraPointer.ToInt64());

                    // Go through the array and copy things from managed code to unmanaged code
                    for (Int32 x = 0; x < (Int32)PathInfoCount; x++)
                    {
                        // Set up the fields in the path info
                        pass2PathInfos[x].Version = NVImport.NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL_VER;
                        pass2PathInfos[x].TargetInfoCount = PathInfos[x].TargetInfoCount;
                        pass2PathInfos[x].Flags = PathInfos[x].Flags;
                        pass2PathInfos[x].OSAdapterID = PathInfos[x].OSAdapterID;
                        pass2PathInfos[x].SourceId = PathInfos[x].SourceId;
                        // Create a target info array and copy it over
                        NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL[] targetInforArray = new NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL[PathInfos[x].TargetInfoCount];
                        pass2PathInfos[x].TargetInfo = currentTargetInfoPointer;
                        //for (Int32 y = 0; y < (Int32)PathInfos[x].TargetInfoCount; y++)
                        for (Int32 y = 0; y < (Int32)PathInfos[x].TargetInfoCount; y++)
                        {
                            // Create the timingExtra object ready to use shortly
                            //NV_TIMING_EXTRA_INTERNAL timingExtra = new NV_TIMING_EXTRA_INTERNAL();
                            //Marshal.StructureToPtr(timingExtra, currentTimingExtraPointer, true);

                            // Create the timing object, and connect it to the timingExtra object we created earlier
                            //NV_TIMING_INTERNAL timing = new NV_TIMING_INTERNAL();
                            //timing.Extra = new IntPtr(currentTimingExtraPointer.ToInt64());
                            //Marshal.StructureToPtr(timing, currentTimingPointer, true);

                            // Create the Advanced Details object, and connect it to the timing object we ust created
                            NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL advInfo = new NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL();
                            advInfo.Version = NVImport.NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_VER;
                            //advInfo.Timing = new IntPtr(currentTimingPointer.ToInt64());
                            Marshal.StructureToPtr(advInfo, currentAdvTargetPointer, true);

                            // Now connect the Advanced details we created to the details in the TargetInfo array item
                            targetInforArray[y].Details = new IntPtr(currentAdvTargetPointer.ToInt64());
                            Marshal.StructureToPtr(targetInforArray[y], currentTargetInfoPointer, true);
                            currentTargetInfoPointer = new IntPtr(currentTargetInfoPointer.ToInt64() + onePathTargetMemSize);
                            currentAdvTargetPointer = new IntPtr(currentAdvTargetPointer.ToInt64() + oneAdvTargetMemSize + oneTimingMemSize + oneTimingExtraMemSize);
                            //currentTimingPointer = new IntPtr(currentTimingPointer.ToInt64() + oneTimingMemSize);
                            //currentTimingExtraPointer = new IntPtr(currentTimingExtraPointer.ToInt64() + oneTimingExtraMemSize);
                        }

                        // Create a source mode info object and copy it over
                        NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 sourceModeInfo = (NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1)PathInfos[x].SourceModeInfo.Clone();
                        Marshal.StructureToPtr(sourceModeInfo, currentSourceModeInfoPointer, true);
                        pass2PathInfos[x].SourceModeInfo = new IntPtr(currentSourceModeInfoPointer.ToInt64());

                        // Marshal a single gridtopology into unmanaged code ready for sending to the unmanaged NVAPI function
                        Marshal.StructureToPtr(pass2PathInfos[x], currentPathInfoPointer, true);

                        // advance the buffer forwards to the next object for each object
                        currentPathInfoPointer = new IntPtr(currentPathInfoPointer.ToInt64() + onePathInfoMemSize);
                        currentSourceModeInfoPointer = new IntPtr(currentSourceModeInfoPointer.ToInt64() + oneSourceModeMemSize);
                    }

                    if (DISP_GetDisplayConfigInternal != null)
                    {
                        // Use the unmanaged buffer in the unmanaged C call
                        status = DISP_GetDisplayConfigInternal(ref PathInfoCount, pathInfoPointer);

                        if (status == NVAPI_STATUS.NVAPI_OK)
                        {
                            // If everything worked, then copy the data back from the unmanaged array into the managed array
                            // So that we can use it in C# land
                            // Reset the memory pointer we're using for tracking where we are back to the start of the unmanaged memory buffer
                            currentPathInfoPointer = new IntPtr(pathInfoPointer.ToInt64());

                            // Create a managed array to store the received information within
                            PathInfos = new NV_DISPLAYCONFIG_PATH_INFO_V2[PathInfoCount];
                            NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[] returnedPass2PathInfos = new NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[PathInfoCount];
                            // Go through the memory buffer item by item and copy the items into the managed array
                            for (int i = 0; i < PathInfoCount; i++)
                            {
                                // fill the returned pass2 array slot structure with the data from the buffer
                                // This lets us get the information and then copy it across to the one we want to return!
                                returnedPass2PathInfos[i] = (NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL)Marshal.PtrToStructure(currentPathInfoPointer, typeof(NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL));

                                // Next copy the information across to the PathInfo we actually want to return
                                PathInfos[i].SourceId = returnedPass2PathInfos[i].SourceId;
                                PathInfos[i].Flags = returnedPass2PathInfos[i].Flags;
                                PathInfos[i].OSAdapterID = returnedPass2PathInfos[i].OSAdapterID;
                                PathInfos[i].TargetInfoCount = returnedPass2PathInfos[i].TargetInfoCount;
                                PathInfos[i].TargetInfo = new NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2[PathInfos[i].TargetInfoCount];
                                PathInfos[i].Version = returnedPass2PathInfos[i].Version;

                                // And turn the memory pointer to NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 into an actual object and populate the object.
                                PathInfos[i].SourceModeInfo = (NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1)Marshal.PtrToStructure(returnedPass2PathInfos[i].SourceModeInfo, typeof(NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1));

                                currentTargetInfoPointer = returnedPass2PathInfos[i].TargetInfo;
                                for (Int32 y = 0; y < (Int32)PathInfos[i].TargetInfoCount; y++)
                                {

                                    // And turn the memory pointer to NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL into an actual object and populate the our wanted object
                                    NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL returnedTargetInfo = (NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL)Marshal.PtrToStructure(currentTargetInfoPointer, typeof(NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL));
                                    // Next we need to get access to the details object.
                                    NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL returnedAdvTarget = (NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL)Marshal.PtrToStructure(returnedTargetInfo.Details, typeof(NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL));
                                    // Next we need to get access to the timing object.
                                    //NV_TIMING_INTERNAL returnedTiming = (NV_TIMING_INTERNAL)Marshal.PtrToStructure(returnedAdvTarget.Timing, typeof(NV_TIMING_INTERNAL));
                                    // Next we need to get access to the timing extra object.
                                    //NV_TIMING_EXTRA_INTERNAL returnedTimingExtra = (NV_TIMING_EXTRA_INTERNAL)Marshal.PtrToStructure(returnedTiming.Extra, typeof(NV_TIMING_EXTRA_INTERNAL));

                                    // Now we start copying the info into the object we want to return
                                    // We'll start with filling in the Timing Extra info we want to return
                                    /*PathInfos[i].TargetInfo[y].Details.Timing.Extra.Flags = returnedTimingExtra.Flags;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.FrequencyInMillihertz = returnedTimingExtra.FrequencyInMillihertz;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.HorizontalAspect = returnedTimingExtra.HorizontalAspect;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.HorizontalPixelRepetition = returnedTimingExtra.HorizontalPixelRepetition;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.Name = returnedTimingExtra.Name;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.RefreshRate = returnedTimingExtra.RefreshRate;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.TimingStandard = returnedTimingExtra.TimingStandard;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.VerticalAspect = returnedTimingExtra.VerticalAspect;*/

                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.Flags = returnedAdvTarget.Timing.Extra.Flags;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.FrequencyInMillihertz = returnedAdvTarget.Timing.Extra.FrequencyInMillihertz;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.HorizontalAspect = returnedAdvTarget.Timing.Extra.HorizontalAspect;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.HorizontalPixelRepetition = returnedAdvTarget.Timing.Extra.HorizontalPixelRepetition;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.Name = returnedAdvTarget.Timing.Extra.Name;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.RefreshRate = returnedAdvTarget.Timing.Extra.RefreshRate;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.TimingStandard = returnedAdvTarget.Timing.Extra.TimingStandard;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Extra.VerticalAspect = returnedAdvTarget.Timing.Extra.VerticalAspect;


                                    // Next, we'll fill in the Timing info we want to return
                                    /*PathInfos[i].TargetInfo[y].Details.Timing.HBorder = returnedTiming.HBorder;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HFrontPorch = returnedTiming.HFrontPorch;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HSyncPol = returnedTiming.HSyncPol;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HSyncWidth = returnedTiming.HSyncWidth;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HTotal = returnedTiming.HTotal;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HVisible = returnedTiming.HVisible;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Pclk = returnedTiming.Pclk;
                                    PathInfos[i].TargetInfo[y].Details.Timing.ScanMode = returnedTiming.ScanMode;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VBorder = returnedTiming.VBorder;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VFrontPorch = returnedTiming.VFrontPorch;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VSyncPol = returnedTiming.VSyncPol;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VSyncWidth = returnedTiming.VSyncWidth;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VTotal = returnedTiming.VTotal;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VVisible = returnedTiming.VVisible;*/

                                    PathInfos[i].TargetInfo[y].Details.Timing.HBorder = returnedAdvTarget.Timing.HBorder;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HFrontPorch = returnedAdvTarget.Timing.HFrontPorch;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HSyncPol = returnedAdvTarget.Timing.HSyncPol;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HSyncWidth = returnedAdvTarget.Timing.HSyncWidth;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HTotal = returnedAdvTarget.Timing.HTotal;
                                    PathInfos[i].TargetInfo[y].Details.Timing.HVisible = returnedAdvTarget.Timing.HVisible;
                                    PathInfos[i].TargetInfo[y].Details.Timing.Pclk = returnedAdvTarget.Timing.Pclk;
                                    PathInfos[i].TargetInfo[y].Details.Timing.ScanMode = returnedAdvTarget.Timing.ScanMode;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VBorder = returnedAdvTarget.Timing.VBorder;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VFrontPorch = returnedAdvTarget.Timing.VFrontPorch;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VSyncPol = returnedAdvTarget.Timing.VSyncPol;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VSyncWidth = returnedAdvTarget.Timing.VSyncWidth;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VTotal = returnedAdvTarget.Timing.VTotal;
                                    PathInfos[i].TargetInfo[y].Details.Timing.VVisible = returnedAdvTarget.Timing.VVisible;

                                    // Next, we'll deal with the advanced details
                                    PathInfos[i].TargetInfo[y].Details.ConnectorType = returnedAdvTarget.ConnectorType;
                                    PathInfos[i].TargetInfo[y].Details.Flags = returnedAdvTarget.Flags;
                                    PathInfos[i].TargetInfo[y].Details.RefreshRateInMillihertz = returnedAdvTarget.RefreshRateInMillihertz;
                                    PathInfos[i].TargetInfo[y].Details.Rotation = returnedAdvTarget.Rotation;
                                    PathInfos[i].TargetInfo[y].Details.Scaling = returnedAdvTarget.Scaling;
                                    PathInfos[i].TargetInfo[y].Details.TimingOverride = returnedAdvTarget.TimingOverride;
                                    PathInfos[i].TargetInfo[y].Details.TvFormat = returnedAdvTarget.TvFormat;
                                    PathInfos[i].TargetInfo[y].Details.Version = returnedAdvTarget.Version;

                                    // We'll finish with the TargetInfo
                                    PathInfos[i].TargetInfo[y].DisplayId = returnedTargetInfo.DisplayId;
                                    PathInfos[i].TargetInfo[y].WindowsCCDTargetId = returnedTargetInfo.WindowsCCDTargetId;


                                    currentTargetInfoPointer = new IntPtr(currentTargetInfoPointer.ToInt64() + onePathTargetMemSize);
                                }

                                // advance the buffer forwards to the next object
                                currentPathInfoPointer = (IntPtr)((long)currentPathInfoPointer + Marshal.SizeOf(returnedPass2PathInfos[i]));
                            }
                        }
                    }
                    else
                    {
                        status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                    }
                    // Try and remove the memory used
                    pass2PathInfos = null;
                }
                finally
                {
                    if (memPointer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(memPointer);
                    }

                }

            }
            else
            {
                // This is the second pass
                // Second Pass: Allocate memory for pathInfo with respect to the number of pathInfoCount(from First Pass) to fetch
                //                               targetInfoCount. If sourceModeInfo is needed allocate memory or it can be initialized to NULL.
                // Build a new blank object for the second pass (when we have the pathInfoCount, but want the targetInfoCount  for each pathInfo)
                // Build a managed structure for us to use as a data source for another object that the unmanaged NVAPI C library can use
                NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[] pass2PathInfos = new NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[PathInfoCount];

                int onePathInfoMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL));
                int oneSourceModeMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1));

                IntPtr pathInfoPointer = IntPtr.Zero;
                IntPtr sourceModeInfoPointer = IntPtr.Zero;

                try
                {
                    pathInfoPointer = Marshal.AllocHGlobal(onePathInfoMemSize * (int)PathInfoCount);
                    sourceModeInfoPointer = Marshal.AllocHGlobal(oneSourceModeMemSize * (int)PathInfoCount);
                    // Also set another memory pointer to the same place so that we can do the memory copying item by item
                    // as we have to do it ourselves (there isn't an easy to use Marshal equivalent)
                    IntPtr currentPathInfoPointer = pathInfoPointer;
                    IntPtr currentSourceModeInfoPointer = sourceModeInfoPointer;

                    // Go through the array and copy things from managed code to unmanaged code
                    for (Int32 x = 0; x < (Int32)PathInfoCount; x++)
                    {
                        // Set up the fields in the path info
                        pass2PathInfos[x].Version = NVImport.NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL_VER;
                        pass2PathInfos[x].TargetInfoCount = 0;
                        pass2PathInfos[x].TargetInfo = IntPtr.Zero;

                        // Create a source mode info object and copy it over
                        NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 sourceModeInfo = new NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1();
                        Marshal.StructureToPtr(sourceModeInfo, currentSourceModeInfoPointer, true);
                        pass2PathInfos[x].SourceModeInfo = currentSourceModeInfoPointer;

                        // Marshal a single gridtopology into unmanaged code ready for sending to the unmanaged NVAPI function
                        Marshal.StructureToPtr(pass2PathInfos[x], currentPathInfoPointer, true);

                        // advance the buffer forwards to the next object for each object
                        //currentPathInfoPointer = new IntPtr(currentPathInfoPointer.ToInt64() + onePathInfoMemSize + oneSourceModeMemSize);
                        currentPathInfoPointer = new IntPtr(currentPathInfoPointer.ToInt64() + onePathInfoMemSize);
                        currentSourceModeInfoPointer = new IntPtr(currentSourceModeInfoPointer.ToInt64() + oneSourceModeMemSize);
                    }

                    if (DISP_GetDisplayConfigInternal != null)
                    {
                        // Use the unmanaged buffer in the unmanaged C call
                        status = DISP_GetDisplayConfigInternal(ref PathInfoCount, pathInfoPointer);

                        if (status == NVAPI_STATUS.NVAPI_OK)
                        {
                            // If everything worked, then copy the data back from the unmanaged array into the managed array
                            // So that we can use it in C# land
                            // Reset the memory pointer we're using for tracking where we are back to the start of the unmanaged memory buffer
                            currentPathInfoPointer = pathInfoPointer;
                            // Create a managed array to store the received information within
                            PathInfos = new NV_DISPLAYCONFIG_PATH_INFO_V2[PathInfoCount];
                            NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[] returnedPass2PathInfos = new NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[PathInfoCount];
                            // Go through the memory buffer item by item and copy the items into the managed array
                            for (int i = 0; i < PathInfoCount; i++)
                            {
                                // fill the returned pass2 array slot structure with the data from the buffer
                                // This lets us get the information and then copy it across to the one we want to return!
                                returnedPass2PathInfos[i] = (NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL)Marshal.PtrToStructure(currentPathInfoPointer, typeof(NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL));

                                // Next copy the information across to the PathInfo we actually want to return
                                PathInfos[i].SourceId = returnedPass2PathInfos[i].SourceId;
                                PathInfos[i].Flags = returnedPass2PathInfos[i].Flags;
                                PathInfos[i].OSAdapterID = returnedPass2PathInfos[i].OSAdapterID;
                                PathInfos[i].TargetInfo = new NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2[0];
                                PathInfos[i].TargetInfoCount = returnedPass2PathInfos[i].TargetInfoCount;
                                PathInfos[i].Version = returnedPass2PathInfos[i].Version;

                                // And turn the memory pointer to NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 into an actual object and populate the object.
                                PathInfos[i].SourceModeInfo = (NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1)Marshal.PtrToStructure(returnedPass2PathInfos[i].SourceModeInfo, typeof(NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1));

                                // destroy the bit of memory we no longer need
                                //Marshal.DestroyStructure(currentPathInfoPointer, typeof(NV_DISPLAYCONFIG_PATH_INFO_V2));
                                // advance the buffer forwards to the next object
                                currentPathInfoPointer = (IntPtr)((long)currentPathInfoPointer + Marshal.SizeOf(returnedPass2PathInfos[i]));
                            }
                        }
                    }
                    else
                    {
                        status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                    }
                    // Try and remove the memory used
                    pass2PathInfos = null;
                }
                finally
                {
                    if (pathInfoPointer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(pathInfoPointer);
                    }

                    if (sourceModeInfoPointer != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(sourceModeInfoPointer);
                    }


                }

            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DISP_GetDisplayConfig(__inout NvU32 *pathInfoCount, __out_ecount_full_opt(*pathInfoCount) NV_DISPLAYCONFIG_PATH_INFO *pathInfo);
        // NvAPIMosaic_EnumDisplayGrids
        private delegate NVAPI_STATUS DISP_GetDisplayConfigDelegateNull(
            [In][Out] ref UInt32 pathInfoCount,
            [In][Out] IntPtr pathInfoBuffer);
        private static readonly DISP_GetDisplayConfigDelegateNull DISP_GetDisplayConfigInternalNull;

        /// <summary>
        /// DESCRIPTION:     This API lets caller retrieve the current global display configuration.
        ///       USAGE:     The caller might have to call this three times to fetch all the required configuration details as follows:
        ///                  First  Pass: Caller should Call NvAPI_DISP_GetDisplayConfig() with pathInfo set to NULL to fetch pathInfoCount.
        ///                  Second Pass: Allocate memory for pathInfo with respect to the number of pathInfoCount(from First Pass) to fetch
        ///                               targetInfoCount. If sourceModeInfo is needed allocate memory or it can be initialized to NULL.
        ///             Third  Pass(Optional, only required if target information is required): Allocate memory for targetInfo with respect
        //                               to number of targetInfoCount(from Second Pass).
        /// SUPPORTED OS:  Windows 7 and higher
        /// </summary>
        /// <param name="PathInfoCount"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DISP_GetDisplayConfig(ref UInt32 PathInfoCount)
        {
            NVAPI_STATUS status;
            IntPtr pathInfos = IntPtr.Zero;

            if (DISP_GetDisplayConfigInternalNull != null) { status = DISP_GetDisplayConfigInternalNull(ref PathInfoCount, pathInfos); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        // ******** IMPORTANT! This code has an error when attempting to perform the third pass as required by NVAPI documentation *********
        // ******** FOr this reason I have disabled the code as I don't actually need to get it going. ******** 
        // NVAPI_INTERFACE NvAPI_DISP_SetDisplayConfig	(	__in NvU32 	pathInfoCount, __in_ecount(pathInfoCount) NV_DISPLAYCONFIG_PATH_INFO* pathInfo,__in NvU32 flags )	
        private delegate NVAPI_STATUS DISP_SetDisplayConfigDelegate(
            [In] UInt32 pathInfoCount,
            [In] IntPtr pathInfoBuffer,
            [In] NV_DISPLAYCONFIG_FLAGS flags);
        private static readonly DISP_SetDisplayConfigDelegate DISP_SetDisplayConfigInternal;

        /// <summary>
        /// DESCRIPTION: This API lets caller apply a global display configuration across multiple GPUs.
        ///     If all sourceIds are zero, then NvAPI will pick up sourceId's based on the following criteria :
        ///     If user provides sourceModeInfo then we are trying to assign 0th sourceId always to GDIPrimary.This is needed since active windows always moves along with 0th sourceId.
        ///     For rest of the paths, we are incrementally assigning the sourceId per adapter basis.
        ///     If user doesn't provide sourceModeInfo then NVAPI just picks up some default sourceId's in incremental order. Note : NVAPI will not intelligently choose the sourceIDs for any configs that does not need a modeset.
        /// SUPPORTED OS: Windows 7 and higher
        /// </summary>
        /// <param name="pathInfoCount"></param>
        /// <param name="pathInfos"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DISP_SetDisplayConfig(UInt32 pathInfoCount, NV_DISPLAYCONFIG_PATH_INFO_V2[] pathInfos, NV_DISPLAYCONFIG_FLAGS flags)
        {
            NVAPI_STATUS status = NVAPI_STATUS.NVAPI_OK;
            NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[] pass2PathInfos = new NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL[pathInfoCount];

            int totalTargetInfoCount = 0;
            for (Int32 x = 0; x < (Int32)pathInfoCount; x++)
            {
                totalTargetInfoCount += (int)pathInfos[x].TargetInfoCount;
            }

            // Get the size of the each object
            int onePathInfoMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL));
            int oneSourceModeMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1));
            int onePathTargetMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL));
            int oneAdvTargetMemSize = Marshal.SizeOf(typeof(NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL));
            int oneTimingMemSize = Marshal.SizeOf(typeof(NV_TIMING_INTERNAL));
            int oneTimingExtraMemSize = Marshal.SizeOf(typeof(NV_TIMING_EXTRA_INTERNAL));

            // Figure out the size of the memory we need to allocate
            int allPathInfoMemSize = onePathInfoMemSize * (int)pathInfoCount;
            int allSourceModeMemSize = oneSourceModeMemSize * (int)pathInfoCount;
            int allPathTargetMemSize = onePathTargetMemSize * totalTargetInfoCount;
            int allAdvTargetMemSize = (oneAdvTargetMemSize + oneTimingMemSize + oneTimingExtraMemSize) * totalTargetInfoCount;
            //int allTimingMemSize = oneTimingMemSize * totalTargetInfoCount;
            //int allTimingExtraMemSize = oneTimingExtraMemSize * totalTargetInfoCount;
            //int allObjectsMemSize = allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize + allAdvTargetMemSize + allTimingMemSize + allTimingExtraMemSize;
            int allObjectsMemSize = allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize + allAdvTargetMemSize;

            IntPtr memPointer = IntPtr.Zero;

            try
            {

                // Allocate the memory we need
                memPointer = Marshal.AllocHGlobal(allObjectsMemSize * 2);

                // Figure out the address of the arrays we will use
                IntPtr pathInfoPointer = memPointer;
                IntPtr sourceModeInfoPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize);
                IntPtr targetInfoPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize + allSourceModeMemSize);
                IntPtr advTargetPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize);
                //IntPtr timingPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize + allAdvTargetMemSize);
                //IntPtr timingExtraPointer = new IntPtr(memPointer.ToInt64() + allPathInfoMemSize + allSourceModeMemSize + allPathTargetMemSize + allAdvTargetMemSize + allTimingMemSize);

                // Figure out each memory pointer so that we can do the memory copying item by item
                // as we have to do it ourselves (there isn't an easy to use Marshal equivalent)
                IntPtr currentPathInfoPointer = new IntPtr(pathInfoPointer.ToInt64());
                IntPtr currentSourceModeInfoPointer = new IntPtr(sourceModeInfoPointer.ToInt64());
                IntPtr currentTargetInfoPointer = new IntPtr(targetInfoPointer.ToInt64());
                IntPtr currentAdvTargetPointer = new IntPtr(advTargetPointer.ToInt64());
                //IntPtr currentTimingPointer = new IntPtr(timingPointer.ToInt64());
                //IntPtr currentTimingExtraPointer = new IntPtr(timingExtraPointer.ToInt64());

                // Go through the array and copy things from managed code to unmanaged code
                for (Int32 x = 0; x < (Int32)pathInfoCount; x++)
                {
                    // Create a target info array and copy it over
                    NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL[] targetInfoArray = new NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2_INTERNAL[pathInfos[x].TargetInfoCount];
                    pass2PathInfos[x].TargetInfo = currentTargetInfoPointer;
                    for (Int32 y = 0; y < (Int32)pathInfos[x].TargetInfoCount; y++)
                    {

                        NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1 advInfo = new NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1();

                        advInfo.Timing.Extra.Flags = pathInfos[x].TargetInfo[y].Details.Timing.Extra.Flags;
                        advInfo.Timing.Extra.FrequencyInMillihertz = pathInfos[x].TargetInfo[y].Details.Timing.Extra.FrequencyInMillihertz;
                        advInfo.Timing.Extra.HorizontalAspect = pathInfos[x].TargetInfo[y].Details.Timing.Extra.HorizontalAspect;
                        advInfo.Timing.Extra.HorizontalPixelRepetition = pathInfos[x].TargetInfo[y].Details.Timing.Extra.HorizontalPixelRepetition;
                        advInfo.Timing.Extra.Name = pathInfos[x].TargetInfo[y].Details.Timing.Extra.Name;
                        advInfo.Timing.Extra.RefreshRate = pathInfos[x].TargetInfo[y].Details.Timing.Extra.RefreshRate;
                        advInfo.Timing.Extra.TimingStandard = pathInfos[x].TargetInfo[y].Details.Timing.Extra.TimingStandard;
                        advInfo.Timing.Extra.VerticalAspect = pathInfos[x].TargetInfo[y].Details.Timing.Extra.VerticalAspect;

                        advInfo.Timing.HBorder = pathInfos[x].TargetInfo[y].Details.Timing.HBorder;
                        advInfo.Timing.HFrontPorch = pathInfos[x].TargetInfo[y].Details.Timing.HFrontPorch;
                        advInfo.Timing.HSyncPol = pathInfos[x].TargetInfo[y].Details.Timing.HSyncPol;
                        advInfo.Timing.HSyncWidth = pathInfos[x].TargetInfo[y].Details.Timing.HSyncWidth;
                        advInfo.Timing.HTotal = pathInfos[x].TargetInfo[y].Details.Timing.HTotal;
                        advInfo.Timing.HVisible = pathInfos[x].TargetInfo[y].Details.Timing.HVisible;
                        advInfo.Timing.Pclk = pathInfos[x].TargetInfo[y].Details.Timing.Pclk;
                        advInfo.Timing.ScanMode = pathInfos[x].TargetInfo[y].Details.Timing.ScanMode;
                        advInfo.Timing.VBorder = pathInfos[x].TargetInfo[y].Details.Timing.VBorder;
                        advInfo.Timing.VFrontPorch = pathInfos[x].TargetInfo[y].Details.Timing.VFrontPorch;
                        advInfo.Timing.VSyncPol = pathInfos[x].TargetInfo[y].Details.Timing.VSyncPol;
                        advInfo.Timing.VSyncWidth = pathInfos[x].TargetInfo[y].Details.Timing.VSyncWidth;
                        advInfo.Timing.VTotal = pathInfos[x].TargetInfo[y].Details.Timing.VTotal;
                        advInfo.Timing.VVisible = pathInfos[x].TargetInfo[y].Details.Timing.VVisible;

                        advInfo.ConnectorType = pathInfos[x].TargetInfo[y].Details.ConnectorType;
                        advInfo.Flags = pathInfos[x].TargetInfo[y].Details.Flags;
                        advInfo.RefreshRateInMillihertz = pathInfos[x].TargetInfo[y].Details.RefreshRateInMillihertz;
                        advInfo.Rotation = pathInfos[x].TargetInfo[y].Details.Rotation;
                        advInfo.Scaling = pathInfos[x].TargetInfo[y].Details.Scaling;
                        advInfo.TimingOverride = pathInfos[x].TargetInfo[y].Details.TimingOverride;
                        advInfo.TvFormat = pathInfos[x].TargetInfo[y].Details.TvFormat;
                        //advInfo.Version = NVImport.NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_INTERNAL_VER;
                        advInfo.Version = NVImport.NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO_V1_VER;
                        Marshal.StructureToPtr(advInfo, currentAdvTargetPointer, true);

                        // Fill in this target info array item
                        targetInfoArray[y].Details = currentAdvTargetPointer;
                        targetInfoArray[y].DisplayId = pathInfos[x].TargetInfo[y].DisplayId;
                        targetInfoArray[y].WindowsCCDTargetId = pathInfos[x].TargetInfo[y].WindowsCCDTargetId;
                        Marshal.StructureToPtr(targetInfoArray[y], currentTargetInfoPointer, true);

                        // Prepare the pointers for the next objects
                        currentTargetInfoPointer = new IntPtr(currentTargetInfoPointer.ToInt64() + onePathTargetMemSize);
                        currentAdvTargetPointer = new IntPtr(currentAdvTargetPointer.ToInt64() + oneAdvTargetMemSize);
                        currentAdvTargetPointer = new IntPtr(currentAdvTargetPointer.ToInt64() + oneAdvTargetMemSize + oneTimingMemSize + oneTimingExtraMemSize);
                    }


                    // Create a source mode info object and copy it over
                    NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1 sourceModeInfo = new NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1();
                    sourceModeInfo.ColorFormat = pathInfos[x].SourceModeInfo.ColorFormat;
                    sourceModeInfo.Flags = pathInfos[x].SourceModeInfo.Flags;
                    sourceModeInfo.Position = (NV_POSITION)pathInfos[x].SourceModeInfo.Position.Clone();
                    sourceModeInfo.Resolution = (NV_RESOLUTION)pathInfos[x].SourceModeInfo.Resolution.Clone();
                    sourceModeInfo.SpanningOrientation = pathInfos[x].SourceModeInfo.SpanningOrientation;
                    Marshal.StructureToPtr(sourceModeInfo, currentSourceModeInfoPointer, true);

                    // Set up the fields in the path info
                    pass2PathInfos[x].Version = NVImport.NV_DISPLAYCONFIG_PATH_INFO_V2_INTERNAL_VER;
                    pass2PathInfos[x].TargetInfoCount = pathInfos[x].TargetInfoCount;
                    pass2PathInfos[x].Flags = pathInfos[x].Flags;
                    pass2PathInfos[x].OSAdapterID = pathInfos[x].OSAdapterID;
                    pass2PathInfos[x].SourceId = pathInfos[x].SourceId;
                    pass2PathInfos[x].SourceModeInfo = currentSourceModeInfoPointer;

                    // Marshal a single gridtopology into unmanaged code ready for sending to the unmanaged NVAPI function
                    Marshal.StructureToPtr(pass2PathInfos[x], currentPathInfoPointer, true);

                    // advance the buffer forwards to the next object for each object
                    currentPathInfoPointer = new IntPtr(currentPathInfoPointer.ToInt64() + onePathInfoMemSize);
                    currentSourceModeInfoPointer = new IntPtr(currentSourceModeInfoPointer.ToInt64() + oneSourceModeMemSize);
                }

                if (DISP_SetDisplayConfigInternal != null)
                {
                    // Use the unmanaged buffer in the unmanaged C call
                    status = DISP_SetDisplayConfigInternal(pathInfoCount, pathInfoPointer, 0);
                }
                else
                {
                    status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                }
            }
            finally
            {
                if (memPointer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(memPointer);
                }

            }

            return status;
        }



        // NVAPI_INTERFACE NvAPI_DISP_GetDisplayIdByDisplayName(const char *displayName, NvU32* displayId);
        private delegate NVAPI_STATUS DISP_GetDisplayIdByDisplayNameDelegate(
            [In] string displayName,
            [Out] out UInt32 displayId);
        private static readonly DISP_GetDisplayIdByDisplayNameDelegate DISP_GetDisplayIdByDisplayNameInternal;

        /// <summary>
        ///
        /// DESCRIPTION:     This API retrieves the Display Id of a given display by
        ///                  display name. The display must be active to retrieve the
        ///                  displayId. In the case of clone mode or Surround gaming,
        ///                  the primary or top-left display will be returned.
        ///
        /// \param [in]     displayName  Name of display (Eg: "\\DISPLAY1" to
        ///                              retrieve the displayId for.
        /// \param [out]    displayId    Display ID of the requested display.
        /// </summary>
        /// <param name="displayName"></param>
        /// <param name="displayId"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DISP_GetDisplayIdByDisplayName(string displayName, out UInt32 displayId)
        {
            NVAPI_STATUS status;
            displayId = 0;

            if (DISP_GetDisplayIdByDisplayNameInternal != null) { status = DISP_GetDisplayIdByDisplayNameInternal(displayName, out displayId); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }


        // EnumPhysicalGPUs
        private delegate NVAPI_STATUS EnumPhysicalGPUsDelegate(
            [In][Out][MarshalAs(UnmanagedType.LPArray, SizeConst = (int)NV_MAX_PHYSICAL_GPUS)] PhysicalGpuHandle[] gpuHandles,
            [Out] out UInt32 gpuCount);
        private static readonly EnumPhysicalGPUsDelegate EnumPhysicalGPUsInternal;

        /// <summary>
        /// This function returns an array of physical GPU handles. Each handle represents a physical GPU present in the system. That GPU may be part of an SLI configuration, or may not be visible to the OS directly.
        /// At least one GPU must be present in the system and running an NVIDIA display driver. The array nvGPUHandle will be filled with physical GPU handle values. The returned gpuCount determines how many entries in the array are valid..
        /// </summary>
        /// <param name="NvGPUHandle"></param>
        /// <param name="pGPUCount"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_EnumPhysicalGPUs(ref PhysicalGpuHandle[] NvGPUHandle, out UInt32 pGPUCount)
        {
            NVAPI_STATUS status;
            UInt32 retGPUCount = 0;
            if (EnumPhysicalGPUsInternal != null) { status = EnumPhysicalGPUsInternal(NvGPUHandle, out retGPUCount); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }
            pGPUCount = retGPUCount;
            return status;
        }

        // GetQuadroStatus
        private delegate NVAPI_STATUS GetQuadroStatusDelegate(
            [In] PhysicalGpuHandle gpuHandle,
            [Out] out UInt32 status);
        private static readonly GetQuadroStatusDelegate GetQuadroStatusInternal;

        /// <summary>
        /// This function retrieves the Quadro status for the GPU (1 if Quadro, 0 if GeForce)
        /// </summary>
        /// <param name="NvGPUHandle"></param>
        /// <param name="pStatus"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GPU_GetQuadroStatus(PhysicalGpuHandle NvGPUHandle, out UInt32 pStatus)
        {
            NVAPI_STATUS status;
            UInt32 retStatus = 0;
            if (GetQuadroStatusInternal != null) { status = GetQuadroStatusInternal(NvGPUHandle, out retStatus); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }
            pStatus = retStatus;
            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_EnumDisplayModes	(	__in NV_MOSAIC_GRID_TOPO * 	pGridTopology, __inout_ecount_part_opt*,*pDisplayCount NV_MOSAIC_DISPLAY_SETTING * 	pDisplaySettings,
        //                                                        __inout NvU32 * 	pDisplayCount )	
        // NvAPIMosaic_EnumDisplayModes
        private delegate NVAPI_STATUS Mosaic_EnumDisplayModesDelegate(
            [In] NV_MOSAIC_GRID_TOPO_V2 gridTopology,
            [In][Out] IntPtr displaySettingsBuffer,
            [In][Out] ref UInt32 displayCount);
        private static readonly Mosaic_EnumDisplayModesDelegate Mosaic_EnumDisplayModesInternal;

        /// <summary>
        /// Determines the set of available display modes for a given grid topology.
        /// If displaySettings is NULL, then displayCount will receive the total number of modes that are available.
        /// If displaySettings is not NULL, then displayCount should point to the number of elements in the pDisplaySettings array. On return, it will contain the number of modes that were actually returned
        /// </summary>
        /// <param name="gridTopology"></param>
        /// <param name="displaySettingsBuffer"></param>
        /// <param name="displayCount"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_EnumDisplayModes(NV_MOSAIC_GRID_TOPO_V2 gridTopology, ref NV_MOSAIC_DISPLAY_SETTING_V2[] displaySettings, ref UInt32 displayCount)
        {
            NVAPI_STATUS status;

            // Build a managed structure for us to use as a data source for another object that the unmanaged NVAPI C library can use
            displaySettings = new NV_MOSAIC_DISPLAY_SETTING_V2[displayCount];
            // Initialize unmanged memory to hold the unmanaged array of structs
            IntPtr displaySettingsBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NV_MOSAIC_DISPLAY_SETTING_V2)) * (int)displayCount);
            // Also set another memory pointer to the same place so that we can do the memory copying item by item
            // as we have to do it ourselves (there isn't an easy to use Marshal equivalent)
            IntPtr currentDisplaySettingsBuffer = displaySettingsBuffer;
            // Go through the array and copy things from managed code to unmanaged code
            for (Int32 x = 0; x < (Int32)displayCount; x++)
            {
                // Set up the basic structure
                displaySettings[x].Version = NVImport.NV_MOSAIC_DISPLAY_SETTING_V2_VER;

                // Marshal a single gridtopology into unmanaged code ready for sending to the unmanaged NVAPI function
                Marshal.StructureToPtr(displaySettings[x], currentDisplaySettingsBuffer, false);
                // advance the buffer forwards to the next object
                currentDisplaySettingsBuffer = (IntPtr)((long)currentDisplaySettingsBuffer + Marshal.SizeOf(displaySettings[x]));
            }


            if (Mosaic_EnumDisplayModesInternal != null)
            {
                // Use the unmanaged buffer in the unmanaged C call
                status = Mosaic_EnumDisplayModesInternal(gridTopology, displaySettingsBuffer, ref displayCount);

                if (status == NVAPI_STATUS.NVAPI_OK)
                {
                    // If everything worked, then copy the data back from the unmanaged array into the managed array
                    // So that we can use it in C# land
                    // Reset the memory pointer we're using for tracking where we are back to the start of the unmanaged memory buffer
                    currentDisplaySettingsBuffer = displaySettingsBuffer;
                    // Create a managed array to store the received information within
                    displaySettings = new NV_MOSAIC_DISPLAY_SETTING_V2[displayCount];
                    // Go through the memory buffer item by item and copy the items into the managed array
                    for (int i = 0; i < displayCount; i++)
                    {
                        // build a structure in the array slot
                        displaySettings[i] = new NV_MOSAIC_DISPLAY_SETTING_V2();
                        // fill the array slot structure NV_MOSAIC_DISPLAY_SETTING_V2 the data from the buffer
                        displaySettings[i] = (NV_MOSAIC_DISPLAY_SETTING_V2)Marshal.PtrToStructure(currentDisplaySettingsBuffer, typeof(NV_MOSAIC_DISPLAY_SETTING_V2));
                        // destroy the bit of memory we no longer need
                        Marshal.DestroyStructure(currentDisplaySettingsBuffer, typeof(NV_MOSAIC_DISPLAY_SETTING_V2));
                        // advance the buffer forwards to the next object
                        currentDisplaySettingsBuffer = (IntPtr)((long)currentDisplaySettingsBuffer + Marshal.SizeOf(displaySettings[i]));
                    }
                }
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            Marshal.FreeHGlobal(displaySettingsBuffer);

            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_EnumDisplayModes(__inout_ecount_part_opt*,* pGridCount NV_MOSAIC_GRID_TOPO* pGridTopologies,__inout NvU32 * 	pGridCount)
        // NvAPIMosaic_EnumDisplayModes
        private delegate NVAPI_STATUS Mosaic_EnumDisplayModesDelegateNull(
            [In] NV_MOSAIC_GRID_TOPO_V2 gridTopology,
            [In][Out] IntPtr displaySettingsBuffer,
            [In][Out] ref UInt32 displayCount);
        private static readonly Mosaic_EnumDisplayModesDelegateNull Mosaic_EnumDisplayModesInternalNull;

        /// <summary>
        /// Determines the set of available display modes for a given grid topology.
        /// If displaySettings is NULL, then displayCount will receive the total number of modes that are available.
        /// If displaySettings is not NULL, then displayCount should point to the number of elements in the pDisplaySettings array. On return, it will contain the number of modes that were actually returned
        /// </summary>
        /// <param name="gridTopologiesBuffer"></param>
        /// <param name="displaySettingsBuffer"></param>
        /// <param name="displayCount"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_EnumDisplayModes(NV_MOSAIC_GRID_TOPO_V2 gridTopology, ref UInt32 displayCount)
        {
            NVAPI_STATUS status;
            IntPtr displaySettings = IntPtr.Zero;

            if (Mosaic_EnumDisplayModesInternalNull != null) { status = Mosaic_EnumDisplayModesInternalNull(gridTopology, displaySettings, ref displayCount); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_EnumDisplayGrids(__inout_ecount_part_opt*,* pGridCount NV_MOSAIC_GRID_TOPO* pGridTopologies,__inout NvU32 * 	pGridCount)
        // NvAPIMosaic_EnumDisplayGrids
        private delegate NVAPI_STATUS Mosaic_EnumDisplayGridsDelegate(
            [In][Out] IntPtr GridTopologiesBuffer,
            [In][Out] ref UInt32 pGridCount);
        private static readonly Mosaic_EnumDisplayGridsDelegate Mosaic_EnumDisplayGridsInternal;

        /// <summary>
        /// Enumerates the current active grid topologies. This includes Mosaic, IG, and Panoramic topologies, as well as single displays.
        /// If pGridTopologies is NULL, then pGridCount will be set to the number of active grid topologies.
        /// If pGridTopologies is not NULL, then pGridCount contains the maximum number of grid topologies to return. On return, pGridCount will be set to the number of grid topologies that were returned.
        /// </summary>
        /// <param name="GridTopologies"></param>
        /// <param name="GridCount"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_EnumDisplayGrids(ref NV_MOSAIC_GRID_TOPO_V2[] GridTopologies, ref UInt32 GridCount)
        {
            NVAPI_STATUS status;

            // Build a managed structure for us to use as a data source for another object that the unmanaged NVAPI C library can use
            GridTopologies = new NV_MOSAIC_GRID_TOPO_V2[GridCount];
            // Initialize unmanged memory to hold the unmanaged array of structs
            IntPtr gridTopologiesBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NV_MOSAIC_GRID_TOPO_V2)) * (int)GridCount);
            // Also set another memory pointer to the same place so that we can do the memory copying item by item
            // as we have to do it ourselves (there isn't an easy to use Marshal equivalent)
            IntPtr currentGridTopologiesBuffer = gridTopologiesBuffer;
            // Go through the array and copy things from managed code to unmanaged code
            for (Int32 x = 0; x < (Int32)GridCount; x++)
            {
                // Set up the basic structure
                GridTopologies[x].Displays = new NV_MOSAIC_GRID_TOPO_DISPLAY_V2[(Int32)NVImport.NV_MOSAIC_MAX_DISPLAYS];
                GridTopologies[x].DisplayCount = GridCount;
                GridTopologies[x].Version = NVImport.NV_MOSAIC_GRID_TOPO_V2_VER;

                // Marshal a single gridtopology into unmanaged code ready for sending to the unmanaged NVAPI function
                Marshal.StructureToPtr(GridTopologies[x], currentGridTopologiesBuffer, false);
                // advance the buffer forwards to the next object
                currentGridTopologiesBuffer = (IntPtr)((long)currentGridTopologiesBuffer + Marshal.SizeOf(GridTopologies[x]));
            }

            if (Mosaic_EnumDisplayGridsInternal != null)
            {
                // Use the unmanaged buffer in the unmanaged C call
                status = Mosaic_EnumDisplayGridsInternal(gridTopologiesBuffer, ref GridCount);

                if (status == NVAPI_STATUS.NVAPI_OK)
                {
                    // If everything worked, then copy the data back from the unmanaged array into the managed array
                    // So that we can use it in C# land
                    // Reset the memory pointer we're using for tracking where we are back to the start of the unmanaged memory buffer
                    currentGridTopologiesBuffer = gridTopologiesBuffer;
                    // Create a managed array to store the received information within
                    GridTopologies = new NV_MOSAIC_GRID_TOPO_V2[GridCount];
                    // Go through the memory buffer item by item and copy the items into the managed array
                    for (int i = 0; i < GridCount; i++)
                    {
                        // build a structure in the array slot
                        GridTopologies[i] = new NV_MOSAIC_GRID_TOPO_V2();
                        // fill the array slot structure with the data from the buffer
                        GridTopologies[i] = (NV_MOSAIC_GRID_TOPO_V2)Marshal.PtrToStructure(currentGridTopologiesBuffer, typeof(NV_MOSAIC_GRID_TOPO_V2));
                        // destroy the bit of memory we no longer need
                        Marshal.DestroyStructure(currentGridTopologiesBuffer, typeof(NV_MOSAIC_GRID_TOPO_V2));
                        // advance the buffer forwards to the next object
                        currentGridTopologiesBuffer = (IntPtr)((long)currentGridTopologiesBuffer + Marshal.SizeOf(GridTopologies[i]));
                    }
                }
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            Marshal.FreeHGlobal(gridTopologiesBuffer);

            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_EnumDisplayGrids(__inout_ecount_part_opt*,* pGridCount NV_MOSAIC_GRID_TOPO* pGridTopologies,__inout NvU32 * 	pGridCount)
        // NvAPIMosaic_EnumDisplayGrids
        private delegate NVAPI_STATUS Mosaic_EnumDisplayGridsDelegateNull(
            [In][Out] IntPtr GridTopologiesBuffer,
            [In][Out] ref UInt32 GridCount);
        private static readonly Mosaic_EnumDisplayGridsDelegateNull Mosaic_EnumDisplayGridsInternalNull;

        /// <summary>
        ///  Enumerates the current active grid topologies. This includes Mosaic, IG, and Panoramic topologies, as well as single displays.
        ///  If pGridTopologies is NULL, then pGridCount will be set to the number of active grid topologies.
        ///  If pGridTopologies is not NULL, then pGridCount contains the maximum number of grid topologies to return. On return, pGridCount will be set to the number of grid topologies that were returned.
        /// </summary>
        /// <param name="pGridCount"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_EnumDisplayGrids(ref UInt32 pGridCount)
        {
            NVAPI_STATUS status;
            IntPtr pGridTopologies = IntPtr.Zero;

            if (Mosaic_EnumDisplayGridsInternalNull != null) { status = Mosaic_EnumDisplayGridsInternalNull(pGridTopologies, ref pGridCount); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_ValidateDisplayGrids(__in NvU32  setTopoFlags, __in_ecount(gridCount) NV_MOSAIC_GRID_TOPO * 	pGridTopologies, __inout_ecount_full(gridCount) NV_MOSAIC_DISPLAY_TOPO_STATUS * 	pTopoStatus, __in NvU32  gridCount )
        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate NVAPI_STATUS Mosaic_ValidateDisplayGridsDelegate(
            [In] NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags,
            [In] IntPtr GridTopologiesBuffer,
            [In][Out] IntPtr TopoStatusesBuffer,
            [In] UInt32 GridCount
            );
        private static readonly Mosaic_ValidateDisplayGridsDelegate Mosaic_ValidateDisplayGridsInternal;

        /// <summary>
        /// Determines if a list of grid topologies is valid. It will choose an SLI configuration in the same way that NvAPI_Mosaic_SetDisplayGrids() does.
        /// On return, each element in the pTopoStatus array will contain any errors or warnings about each grid topology.If any error flags are set, then the topology is not valid.If any warning flags are set, then the topology is valid, but sub-optimal.
        /// If the ALLOW_INVALID flag is set, then it will continue to validate the grids even if no SLI configuration will allow all of the grids.In this case, a grid grid with no matching GPU topology will have the error flags NO_GPU_TOPOLOGY or NOT_SUPPORTED set.
        /// If the ALLOW_INVALID flag is not set and no matching SLI configuration is found, then it will skip the rest of the validation and return NVAPI_NO_ACTIVE_SLI_TOPOLOGY.
        /// </summary>
        /// <param name="setTopoFlags"></param>
        /// <param name="gridTopologies"></param>
        /// <param name="topoStatuses"></param>
        /// <param name="gridCount"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_ValidateDisplayGrids(NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags, NV_MOSAIC_GRID_TOPO_V2[] gridTopologies, ref NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[] topoStatuses, UInt32 gridCount)
        {
            // Warning! - This function still has some errors with it. It errors with an NVAPI_INCOMPATIBLE_STRUCT_VERSION error. Still needs troubleshooting.
            NVAPI_STATUS status;
            // Initialize unmanged memory to hold the unmanaged array of structs
            IntPtr gridTopologiesBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NV_MOSAIC_GRID_TOPO_V2)) * (int)gridCount);
            // Also set another memory pointer to the same place so that we can do the memory copying item by item
            // as we have to do it ourselves (there isn't an easy to use Marshal equivalent)
            IntPtr currentGridTopologiesBuffer = gridTopologiesBuffer;
            // Go through the array and copy things from managed code to unmanaged code
            for (Int32 x = 0; x < (Int32)gridCount; x++)
            {
                // Marshal a single gridtopology into unmanaged code ready for sending to the unmanaged NVAPI function
                Marshal.StructureToPtr(gridTopologies[x], currentGridTopologiesBuffer, false);
                // advance the buffer forwards to the next object
                currentGridTopologiesBuffer = (IntPtr)((long)currentGridTopologiesBuffer + Marshal.SizeOf(gridTopologies[x]));
            }
            // Build a managed structure for us to use as a data source for another object that the unmanaged NVAPI C library can use
            topoStatuses = new NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[gridCount];
            // Initialize unmanged memory to hold the unmanaged array of structs
            IntPtr topoStatusesBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NV_MOSAIC_DISPLAY_TOPO_STATUS_V1)) * (int)gridCount);
            // Also set another memory pointer to the same place so that we can do the memory copying item by item
            // as we have to do it ourselves (there isn't an easy to use Marshal equivalent)
            IntPtr currentTopoStatusesBuffer = topoStatusesBuffer;
            // Go through the array and copy things from managed code to unmanaged code
            for (Int32 x = 0; x < (Int32)gridCount; x++)
            {
                // Set up the basic structure
                topoStatuses[x].Displays = new NV_MOSAIC_DISPLAY_TOPO_STATUS_DISPLAY[(Int32)NVImport.NV_MAX_DISPLAYS];
                //topoStatuses[x].DisplayCount = gridCount;
                topoStatuses[x].Version = NVImport.NV_MOSAIC_DISPLAY_TOPO_STATUS_V1_VER;

                // Marshal a single gridtopology into unmanaged code ready for sending to the unmanaged NVAPI function
                Marshal.StructureToPtr(topoStatuses[x], currentTopoStatusesBuffer, false);
                // advance the buffer forwards to the next object
                currentTopoStatusesBuffer = (IntPtr)((long)currentTopoStatusesBuffer + Marshal.SizeOf(topoStatuses[x]));
            }
            if (Mosaic_ValidateDisplayGridsInternal != null)
            {
                // Use the unmanaged buffer in the unmanaged C call
                status = Mosaic_ValidateDisplayGridsInternal(setTopoFlags, gridTopologiesBuffer, topoStatusesBuffer, gridCount);
                if (status == NVAPI_STATUS.NVAPI_OK)
                {
                    // If everything worked, then copy the data back from the unmanaged array into the managed array
                    // So that we can use it in C# land
                    // Reset the memory pointer we're using for tracking where we are back to the start of the unmanaged memory buffer
                    currentTopoStatusesBuffer = topoStatusesBuffer;
                    // Create a managed array to store the received information within
                    topoStatuses = new NV_MOSAIC_DISPLAY_TOPO_STATUS_V1[gridCount];
                    // Go through the memory buffer item by item and copy the items into the managed array
                    for (int i = 0; i < gridCount; i++)
                    {
                        // build a structure in the array slot
                        topoStatuses[i] = new NV_MOSAIC_DISPLAY_TOPO_STATUS_V1();
                        // fill the array slot structure with the data from the buffer
                        topoStatuses[i] = (NV_MOSAIC_DISPLAY_TOPO_STATUS_V1)Marshal.PtrToStructure(currentTopoStatusesBuffer, typeof(NV_MOSAIC_DISPLAY_TOPO_STATUS_V1));
                        // destroy the bit of memory we no longer need
                        Marshal.DestroyStructure(currentTopoStatusesBuffer, typeof(NV_MOSAIC_DISPLAY_TOPO_STATUS_V1));
                        // advance the buffer forwards to the next object
                        currentTopoStatusesBuffer = (IntPtr)((long)currentTopoStatusesBuffer + Marshal.SizeOf(topoStatuses[i]));
                    }
                }
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            Marshal.FreeHGlobal(gridTopologiesBuffer);
            Marshal.FreeHGlobal(topoStatusesBuffer);

            return status;
        }


        // NVAPI_INTERFACE NvAPI_Mosaic_SetDisplayGrids	(	__in_ecount(gridCount) NV_MOSAIC_GRID_TOPO * 	pGridTopologies, __in NvU32  gridCount, __in NvU32  setTopoFlags )	
        private delegate NVAPI_STATUS Mosaic_SetDisplayGridsDelegate(
            [In] IntPtr GridTopologies,
            [In] UInt32 GridCount,
            [In] NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags);
        private static readonly Mosaic_SetDisplayGridsDelegate Mosaic_SetDisplayGridsInternal;

        /// <summary>
        ///  Sets a new display topology, replacing any existing topologies that use the same displays.
        ///  This function will look for an SLI configuration that will allow the display topology to work.
        ///  To revert to a single display, specify that display as a 1x1 grid.
        /// </summary>
        /// <param name="pGridTopologies"></param>
        /// <param name="pGridCount"></param>
        /// <param name="setTopoFlags"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_SetDisplayGrids(NV_MOSAIC_GRID_TOPO_V2[] GridTopologies, UInt32 GridCount, NV_MOSAIC_SETDISPLAYTOPO_FLAGS setTopoFlags)
        {
            NVAPI_STATUS status;

            // Initialize unmanged memory to hold the unmanaged array of structs
            IntPtr gridTopologiesBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NV_MOSAIC_GRID_TOPO_V2)) * (int)GridCount);
            // Also set another memory pointer to the same place so that we can do the memory copying item by item
            // as we have to do it ourselves (there isn't an easy to use Marshal equivalent)
            IntPtr currentGridTopologiesBuffer = gridTopologiesBuffer;
            // Go through the array and copy things from managed code to unmanaged code
            for (Int32 x = 0; x < (Int32)GridCount; x++)
            {
                GridTopologies[x].Version = NVImport.NV_MOSAIC_GRID_TOPO_V2_VER;

                // Marshal a single gridtopology into unmanaged code ready for sending to the unmanaged NVAPI function
                Marshal.StructureToPtr(GridTopologies[x], currentGridTopologiesBuffer, false);
                // advance the buffer forwards to the next object
                currentGridTopologiesBuffer = (IntPtr)((long)currentGridTopologiesBuffer + Marshal.SizeOf(GridTopologies[x]));
            }

            if (Mosaic_SetDisplayGridsInternal != null)
            {

                // Use the unmanaged buffer in the unmanaged C call
                status = Mosaic_SetDisplayGridsInternal(gridTopologiesBuffer, GridCount, setTopoFlags);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            Marshal.FreeHGlobal(gridTopologiesBuffer);

            return status;
        }


        // NVAPI_INTERFACE NvAPI_Mosaic_GetDisplayViewportsByResolution(NvU32 displayId, NvU32 srcWidth, NvU32 srcHeight, NV_RECT viewports[NV_MOSAIC_MAX_DISPLAYS], NvU8* bezelCorrected )
        private delegate NVAPI_STATUS Mosaic_GetDisplayViewportsByResolutionDelegate(
            [In] UInt32 displayId,
            [In] UInt32 srcWidth,
            [In] UInt32 srcHeight,
            [In][Out][MarshalAs(UnmanagedType.LPArray, SizeConst = (int)NV_MOSAIC_MAX_DISPLAYS)] NV_RECT[] viewports,
            [In][Out] ref byte bezelCorrected);
        private static readonly Mosaic_GetDisplayViewportsByResolutionDelegate Mosaic_GetDisplayViewportsByResolutionInternal;

        /// <summary>
        ///  This API returns information for the current Mosaic topology. This includes topology, display settings, and overlap values.
        ///  You can call NvAPI_Mosaic_GetTopoGroup() with the topology if you require more information. If there isn't a current topology, then pTopoBrief->topo will be NV_MOSAIC_TOPO_NONE.
        /// </summary>
        /// <param name="pTopoBrief"></param>
        /// <param name="pDisplaySetting"></param>
        /// <param name="pOverlapX"></param>
        /// <param name="pOverlapY"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_GetDisplayViewportsByResolution(UInt32 displayId, UInt32 srcWidth, UInt32 srcHeight, ref NV_RECT[] viewports, ref byte bezelCorrected)
        {
            NVAPI_STATUS status;
            viewports = new NV_RECT[NV_MOSAIC_MAX_DISPLAYS];
            bezelCorrected = 0;

            if (Mosaic_GetDisplayViewportsByResolutionInternal != null) { status = Mosaic_GetDisplayViewportsByResolutionInternal(displayId, srcWidth, srcHeight, viewports, ref bezelCorrected); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_GetSupportedTopoInfo(NV_MOSAIC_SUPPORTED_TOPO_INFO* pSupportedTopoInfo, NV_MOSAIC_TOPO_TYPE type )
        // NvAPI_Mosaic_GetSupportedTopoInfo
        private delegate NVAPI_STATUS Mosaic_GetSupportedTopoInfoDelegate(
            [In][Out] ref NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 pSupportedTopoInfo,
            [In] NV_MOSAIC_TOPO_TYPE TopoType);
        private static readonly Mosaic_GetSupportedTopoInfoDelegate Mosaic_GetSupportedTopoInfoInternal;

        /// <summary>
        ///  This API returns information on the topologies and display resolutions supported by Mosaic mode.
        ///  NOTE: Not all topologies returned can be set immediately.See 'OUT' Notes below.
        ///  Once you get the list of supported topologies, you can call NvAPI_Mosaic_GetTopoGroup() with one of the Mosaic topologies if you need more information about it.
        ///  'IN' Notes: pSupportedTopoInfo->version must be set before calling this function.If the specified version is not supported by this implementation, an error will be returned (NVAPI_INCOMPATIBLE_STRUCT_VERSION).
        ///  'OUT' Notes: Some of the topologies returned might not be valid for one reason or another. It could be due to mismatched or missing displays.
        ///  It could also be because the required number of GPUs is not found. At a high level, you can see if the topology is valid and can be enabled by looking at the pSupportedTopoInfo->topoBriefs[xxx].isPossible flag. 
        ///  If this is true, the topology can be enabled.If it is false, you can find out why it cannot be enabled by getting the details of the topology via NvAPI_Mosaic_GetTopoGroup(). 
        ///  From there, look at the validityMask of the individual topologies. The bits can be tested against the NV_MOSAIC_TOPO_VALIDITY_* bits.
        ///  It is possible for this function to return NVAPI_OK with no topologies listed in the return structure.If this is the case, it means that the current hardware DOES support Mosaic, 
        ///  but with the given configuration no valid topologies were found. This most likely means that SLI was not enabled for the hardware. Once enabled, you should see valid topologies returned from this function.
        /// </summary>
        /// <param name="pSupportedTopoInfo"></param>
        /// <param name="TopoType"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_GetSupportedTopoInfo(ref NV_MOSAIC_SUPPORTED_TOPO_INFO_V2 pSupportedTopoInfo, NV_MOSAIC_TOPO_TYPE TopoType)
        {
            NVAPI_STATUS status;
            //pSupportedTopoInfo = new NV_MOSAIC_SUPPORTED_TOPO_INFO_V2();
            pSupportedTopoInfo.Version = NVImport.NV_MOSAIC_SUPPORTED_TOPO_INFO_V2_VER;

            if (Mosaic_GetSupportedTopoInfoInternal != null) { status = Mosaic_GetSupportedTopoInfoInternal(ref pSupportedTopoInfo, TopoType); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }



            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_GetOverlapLimits(NV_MOSAIC_TOPO_BRIEF *pTopoBrief, NV_MOSAIC_DISPLAY_SETTING *pDisplaySetting, NvS32 *pMinOverlapX, NvS32 *pMaxOverlapX, NvS32 *pMinOverlapY, NvS32 *pMaxOverlapY);
        private delegate NVAPI_STATUS Mosaic_GetOverlapLimitsDelegate(
            [In] ref NV_MOSAIC_TOPO_BRIEF topoBrief,
            [In] ref NV_MOSAIC_DISPLAY_SETTING_V2 displaySetting,
            [Out] out Int32 minOverlapX,
            [Out] out Int32 maxOverlapX,
            [Out] out Int32 minOverlapY,
            [Out] out Int32 maxOverlapY);
        private static readonly Mosaic_GetOverlapLimitsDelegate Mosaic_GetOverlapLimitsInternal;
        /// <summary>
        ///   This API returns the X and Y overlap limits required if the given Mosaic topology and display settings are to be used.
        /// </summary>
        /// <param name="topoBrief"></param>
        /// <param name="displaySetting"></param>
        /// <param name="minOverlapX"></param>
        /// <param name="maxOverlapX"></param>
        /// <param name="minOverlapY"></param>
        /// <param name="maxOverlapY"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_GetOverlapLimits(NV_MOSAIC_TOPO_BRIEF topoBrief, NV_MOSAIC_DISPLAY_SETTING_V2 displaySetting, out Int32 minOverlapX, out Int32 maxOverlapX, out Int32 minOverlapY, out Int32 maxOverlapY)
        {
            NVAPI_STATUS status;
            minOverlapX = 0;
            maxOverlapX = 0;
            minOverlapY = 0;
            maxOverlapY = 0;

            if (Mosaic_GetOverlapLimitsInternal != null) { status = Mosaic_GetOverlapLimitsInternal(ref topoBrief, ref displaySetting, out minOverlapX, out maxOverlapX, out minOverlapY, out maxOverlapY); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_EnableCurrentTopo(NvU32 enable)	
        private delegate NVAPI_STATUS Mosaic_EnableCurrentTopoDelegate(
            [In] UInt32 enable);
        private static readonly Mosaic_EnableCurrentTopoDelegate Mosaic_EnableCurrentTopoInternal;

        /// <summary>
        ///  This API enables or disables the current Mosaic topology based on the setting of the incoming 'enable' parameter.
        ///  An "enable" setting enables the current(previously set) Mosaic topology.Note that when the current Mosaic topology is retrieved, it must have an isPossible value of 1 or an error will occur.
        ///  A "disable" setting disables the current Mosaic topology. The topology information will persist, even across reboots.To re-enable the Mosaic topology, call this function again with the enable parameter set to 1.
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_EnableCurrentTopo(UInt32 enable)
        {
            NVAPI_STATUS status;

            if (Mosaic_EnableCurrentTopoInternal != null) { status = Mosaic_EnableCurrentTopoInternal(enable); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_SetCurrentTopo ( NV_MOSAIC_TOPO_BRIEF * pTopoBrief, NV_MOSAIC_DISPLAY_SETTING* pDisplaySetting, NvS32 overlapX, NvS32 overlapY, NvU32 enable)	
        // NvAPI_Mosaic_SetCurrentTopo
        private delegate NVAPI_STATUS Mosaic_SetCurrentTopoDelegate(
            [In] NV_MOSAIC_TOPO_BRIEF topoBrief,
            [In] NV_MOSAIC_DISPLAY_SETTING_V2 displaySetting,
            [In] Int32 overlapX,
            [In] Int32 overlapY,
            [In] UInt32 enable);
        private static readonly Mosaic_SetCurrentTopoDelegate Mosaic_SetCurrentTopoInternal;

        /// <summary>
        ///  This API sets the Mosaic topology and performs a mode switch using the given display settings.
        ///  If NVAPI_OK is returned, the current Mosaic topology was set correctly. Any other status returned means the topology was not set, and remains what it was before this function was called.
        /// </summary>
        /// <param name="topoBrief"></param>
        /// <param name="displaySetting"></param>
        /// <param name="overlapX"></param>
        /// <param name="overlapY"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_SetCurrentTopo(NV_MOSAIC_TOPO_BRIEF topoBrief, NV_MOSAIC_DISPLAY_SETTING_V2 displaySetting, Int32 overlapX, Int32 overlapY, UInt32 enable)
        {
            NVAPI_STATUS status;
            //topoBrief.Version = NVImport.NV_MOSAIC_TOPO_BRIEF_VER;
            //displaySetting.Version = NVImport.NV_MOSAIC_DISPLAY_SETTING_V2_VER;
            // Set enable to false within the version as we want to enable it now
            // This is needed as the saved display topology object was made when the topology was enabled :)
            //topoBrief.Enabled = 0;
            if (Mosaic_SetCurrentTopoInternal != null) { status = Mosaic_SetCurrentTopoInternal(topoBrief, displaySetting, overlapX, overlapY, enable); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }


        // NVAPI_INTERFACE NvAPI_Mosaic_GetCurrentTopo(NV_MOSAIC_TOPO_BRIEF* pTopoBrief, NV_MOSAIC_DISPLAY_SETTING* pDisplaySetting, NvS32* pOverlapX, NvS32* pOverlapY);
        private delegate NVAPI_STATUS Mosaic_GetCurrentTopoDelegate(
            [In][Out] ref NV_MOSAIC_TOPO_BRIEF pTopoBrief,
            [In][Out] ref NV_MOSAIC_DISPLAY_SETTING_V2 pDisplaySetting,
            [Out] out Int32 pOverlapX,
            [Out] out Int32 pOverlapY);
        private static readonly Mosaic_GetCurrentTopoDelegate Mosaic_GetCurrentTopoInternal;
        /// <summary>
        ///  This API returns information for the current Mosaic topology. This includes topology, display settings, and overlap values.
        ///  You can call NvAPI_Mosaic_GetTopoGroup() with the topology if you require more information. If there isn't a current topology, then pTopoBrief->topo will be NV_MOSAIC_TOPO_NONE.
        /// </summary>
        /// <param name="pTopoBrief"></param>
        /// <param name="pDisplaySetting"></param>
        /// <param name="pOverlapX"></param>
        /// <param name="pOverlapY"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_GetCurrentTopo(ref NV_MOSAIC_TOPO_BRIEF pTopoBrief, ref NV_MOSAIC_DISPLAY_SETTING_V2 pDisplaySetting, out Int32 pOverlapX, out Int32 pOverlapY)
        {
            NVAPI_STATUS status;
            pOverlapX = 0;
            pOverlapY = 0;
            pTopoBrief = new NV_MOSAIC_TOPO_BRIEF();
            pTopoBrief.Version = NVImport.NV_MOSAIC_TOPO_BRIEF_VER;
            pDisplaySetting = new NV_MOSAIC_DISPLAY_SETTING_V2();
            pDisplaySetting.Version = NVImport.NV_MOSAIC_DISPLAY_SETTING_V2_VER;

            if (Mosaic_GetCurrentTopoInternal != null) { status = Mosaic_GetCurrentTopoInternal(ref pTopoBrief, ref pDisplaySetting, out pOverlapX, out pOverlapY); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_Mosaic_GetTopoGroup(NV_MOSAIC_TOPO_BRIEF* pTopoBrief, NV_MOSAIC_TOPO_GROUP* pTopoGroup)
        private delegate NVAPI_STATUS Mosaic_GetTopoGroupDelegate(
            [In] ref NV_MOSAIC_TOPO_BRIEF pTopoBrief,
            [In][Out] ref NV_MOSAIC_TOPO_GROUP pTopoGroup);
        private static readonly Mosaic_GetTopoGroupDelegate Mosaic_GetTopoGroupInternal;

        /// <summary>
        ///  This API returns information for the current Mosaic topology. This includes topology, display settings, and overlap values.
        ///  You can call NvAPI_Mosaic_GetTopoGroup() with the topology if you require more information. If there isn't a current topology, then pTopoBrief->topo will be NV_MOSAIC_TOPO_NONE.
        /// </summary>
        /// <param name="pTopoBrief"></param>
        /// <param name="pTopoGroup"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Mosaic_GetTopoGroup(ref NV_MOSAIC_TOPO_BRIEF pTopoBrief, ref NV_MOSAIC_TOPO_GROUP pTopoGroup)
        {
            NVAPI_STATUS status;
            pTopoGroup = new NV_MOSAIC_TOPO_GROUP();
            pTopoGroup.Brief = pTopoBrief;
            pTopoGroup.Topos = new NV_MOSAIC_TOPO_DETAILS[NV_MOSAIC_MAX_TOPO_PER_TOPO_GROUP];
            for (Int32 i = 0; i < NV_MOSAIC_MAX_TOPO_PER_TOPO_GROUP; i++)
            {
                pTopoGroup.Topos[i].GPULayout1D = new NV_MOSAIC_TOPO_GPU_LAYOUT_CELL[NVAPI_MAX_MOSAIC_DISPLAY_ROWS * NVAPI_MAX_MOSAIC_DISPLAY_COLUMNS];
                pTopoGroup.Topos[i].Version = NVImport.NV_MOSAIC_TOPO_DETAILS_VER;
            }
            pTopoGroup.Version = NVImport.NV_MOSAIC_TOPO_GROUP_VER;


            if (Mosaic_GetTopoGroupInternal != null)
            {
                status = Mosaic_GetTopoGroupInternal(ref pTopoBrief, ref pTopoGroup);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            return status;
        }

        //NVAPI_INTERFACE 	NvAPI_DISP_GetAdaptiveSyncData (__in NvU32 displayId, __inout NV_GET_ADAPTIVE_SYNC_DATA *pAdaptiveSyncData)
        private delegate NVAPI_STATUS DISP_GetAdaptiveSyncDataDelegate(
            [In] UInt32 displayId,
            [In, Out] ref NV_GET_ADAPTIVE_SYNC_DATA_V1 adaptiveSyncData);
        private static readonly DISP_GetAdaptiveSyncDataDelegate DISP_GetAdaptiveSyncDataInternal;
        /// <summary>
        ///  This function is used to get data for the Adaptive Sync Display.
        /// </summary>
        /// <param name="displayId"></param>
        /// <param name="adaptiveSyncData"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DISP_GetAdaptiveSyncData(UInt32 displayId, ref NV_GET_ADAPTIVE_SYNC_DATA_V1 adaptiveSyncData)
        {
            NVAPI_STATUS status = NVAPI_STATUS.NVAPI_ERROR;
            if (DISP_GetAdaptiveSyncDataInternal != null) { status = DISP_GetAdaptiveSyncDataInternal(displayId, ref adaptiveSyncData); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE 	NvAPI_DISP_SetAdaptiveSyncData (__in NvU32 displayId, __in NV_SET_ADAPTIVE_SYNC_DATA *pAdaptiveSyncData)
        private delegate NVAPI_STATUS DISP_SetAdaptiveSyncDataDelegate(
            [In] UInt32 displayId,
            [In] ref NV_SET_ADAPTIVE_SYNC_DATA_V1 adaptiveSyncData);
        private static readonly DISP_SetAdaptiveSyncDataDelegate DISP_SetAdaptiveSyncDataInternal;
        /// <summary>
        ///  This function is used to set data for Adaptive Sync Display.
        /// </summary>
        /// <param name="displayId"></param>
        /// <param name="adaptiveSyncData"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DISP_SetAdaptiveSyncData(UInt32 displayId, ref NV_SET_ADAPTIVE_SYNC_DATA_V1 adaptiveSyncData)
        {
            NVAPI_STATUS status = NVAPI_STATUS.NVAPI_ERROR;
            if (DISP_SetAdaptiveSyncDataInternal != null) { status = DISP_SetAdaptiveSyncDataInternal(displayId, ref adaptiveSyncData); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }


        //NVAPI_INTERFACE NvAPI_DISP_GetGDIPrimaryDisplayId(NvU32* displayId);
        private delegate NVAPI_STATUS DISP_GetGDIPrimaryDisplayIdDelegate(
            [Out] out UInt32 displayId);
        private static readonly DISP_GetGDIPrimaryDisplayIdDelegate DISP_GetGDIPrimaryDisplayIdInternal;
        /// <summary>
        ///  This API returns the Display ID of the GDI Primary display.
        /// </summary>
        /// <param name="displayId"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DISP_GetGDIPrimaryDisplayId(out UInt32 displayId)
        {
            NVAPI_STATUS status = NVAPI_STATUS.NVAPI_ERROR;
            displayId = 0;
            if (DISP_GetGDIPrimaryDisplayIdInternal != null) { status = DISP_GetGDIPrimaryDisplayIdInternal(out displayId); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_GPU_GetConnectedDisplayIds(__in NvPhysicalGpuHandle hPhysicalGpu, __inout_ecount_part_opt(* pDisplayIdCount, * pDisplayIdCount) NV_GPU_DISPLAYIDS* pDisplayIds, __inout NvU32* pDisplayIdCount, __in NvU32 flags);
        private delegate NVAPI_STATUS GPU_GetConnectedDisplayIdsDelegate(
            [In] PhysicalGpuHandle hPhysicalGpu,
            [In][Out] IntPtr pDisplayIds,
            [In][Out] ref UInt32 pDisplayCount,
            [In] NV_GPU_CONNECTED_IDS_FLAG flags);
        private static readonly GPU_GetConnectedDisplayIdsDelegate GPU_GetConnectedDisplayIdsInternal;
        /// <summary>
        //!   DESCRIPTION: Due to space limitation NvAPI_GPU_GetConnectedOutputs can return maximum 32 devices, but
        //!                this is no longer true for DPMST. NvAPI_GPU_GetConnectedDisplayIds will return all
        //!                the connected display devices in the form of displayIds for the associated hPhysicalGpu.
        //!                This function can accept set of flags to request cached, uncached, sli and lid to get the connected devices.
        //!                Default value for flags will be cached .
        //! HOW TO USE: 1) for each PhysicalGpu, make a call to get the number of connected displayId's
        //!                using NvAPI_GPU_GetConnectedDisplayIds by passing the pDisplayIds as NULL
        //!                On call success:
        //!             2) If pDisplayIdCount is greater than 0, allocate memory based on pDisplayIdCount. Then make a call NvAPI_GPU_GetConnectedDisplayIds to populate DisplayIds.
        //!                However, if pDisplayIdCount is 0, do not make this call.
        /// <param name="hPhysicalGpu">GPU selection</param>
        /// <param name="pDisplaySetting"></param>
        /// <param name="pOverlapX"></param>
        /// <param name="pOverlapY"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GPU_GetConnectedDisplayIds(PhysicalGpuHandle physicalGpu, ref NV_GPU_DISPLAYIDS_V2[] displayIds, ref UInt32 displayCount, NV_GPU_CONNECTED_IDS_FLAG flags)
        {
            NVAPI_STATUS status;

            // Build a managed structure for us to use as a data source for another object that the unmanaged NVAPI C library can use
            displayIds = new NV_GPU_DISPLAYIDS_V2[displayCount];
            // Initialize unmanged memory to hold the unmanaged array of structs
            IntPtr displayIdBuffer = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(NV_GPU_DISPLAYIDS_V2)) * (int)displayCount);
            // Also set another memory pointer to the same place so that we can do the memory copying item by item
            // as we have to do it ourselves (there isn't an easy to use Marshal equivalent)
            IntPtr currentDisplayIdBuffer = displayIdBuffer;
            // Go through the array and copy things from managed code to unmanaged code
            for (Int32 x = 0; x < (Int32)displayCount; x++)
            {
                // Set up the basic structure
                displayIds[x].Version = NVImport.NV_GPU_DISPLAYIDS_V2_VER;

                // Marshal a single gridtopology into unmanaged code ready for sending to the unmanaged NVAPI function
                Marshal.StructureToPtr(displayIds[x], currentDisplayIdBuffer, false);
                // advance the buffer forwards to the next object
                currentDisplayIdBuffer = (IntPtr)((long)currentDisplayIdBuffer + Marshal.SizeOf(displayIds[x]));
            }

            if (GPU_GetConnectedDisplayIdsInternal != null)
            {
                status = GPU_GetConnectedDisplayIdsInternal(physicalGpu, displayIdBuffer, ref displayCount, flags);
                if (status == NVAPI_STATUS.NVAPI_OK)
                {
                    // If everything worked, then copy the data back from the unmanaged array into the managed array
                    // So that we can use it in C# land
                    // Reset the memory pointer we're using for tracking where we are back to the start of the unmanaged memory buffer
                    currentDisplayIdBuffer = displayIdBuffer;
                    // Create a managed array to store the received information within
                    displayIds = new NV_GPU_DISPLAYIDS_V2[displayCount];
                    // Go through the memory buffer item by item and copy the items into the managed array
                    for (int i = 0; i < displayCount; i++)
                    {
                        // build a structure in the array slot
                        displayIds[i] = new NV_GPU_DISPLAYIDS_V2();
                        // fill the array slot structure with the data from the buffer
                        displayIds[i] = (NV_GPU_DISPLAYIDS_V2)Marshal.PtrToStructure(currentDisplayIdBuffer, typeof(NV_GPU_DISPLAYIDS_V2));
                        // destroy the bit of memory we no longer need
                        Marshal.DestroyStructure(currentDisplayIdBuffer, typeof(NV_GPU_DISPLAYIDS_V2));
                        // advance the buffer forwards to the next object
                        currentDisplayIdBuffer = (IntPtr)((long)currentDisplayIdBuffer + Marshal.SizeOf(displayIds[i]));
                    }
                }
                // Destroy the unmanaged array so we don't have a memory leak
                Marshal.FreeHGlobal(displayIdBuffer);

            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_GPU_GetConnectedDisplayIds(__in NvPhysicalGpuHandle hPhysicalGpu, __inout_ecount_part_opt(* pDisplayIdCount, * pDisplayIdCount) NV_GPU_DISPLAYIDS* pDisplayIds, __inout NvU32* pDisplayIdCount, __in NvU32 flags);
        private delegate NVAPI_STATUS GPU_GetConnectedDisplayIdsDelegateNull(
            [In] PhysicalGpuHandle hPhysicalGpu,
            [In] IntPtr pDisplayIds,
            [In][Out] ref UInt32 pDisplayCount,
            [In] NV_GPU_CONNECTED_IDS_FLAG flags);
        private static readonly GPU_GetConnectedDisplayIdsDelegateNull GPU_GetConnectedDisplayIdsInternalNull;
        /// <summary>
        //!   DESCRIPTION: Due to space limitation NvAPI_GPU_GetConnectedOutputs can return maximum 32 devices, but
        //!                this is no longer true for DPMST. NvAPI_GPU_GetConnectedDisplayIds will return all
        //!                the connected display devices in the form of displayIds for the associated hPhysicalGpu.
        //!                This function can accept set of flags to request cached, uncached, sli and lid to get the connected devices.
        //!                Default value for flags will be cached .
        //! HOW TO USE: 1) for each PhysicalGpu, make a call to get the number of connected displayId's
        //!                using NvAPI_GPU_GetConnectedDisplayIds by passing the pDisplayIds as NULL
        //!                On call success:
        //!             2) If pDisplayIdCount is greater than 0, allocate memory based on pDisplayIdCount. Then make a call NvAPI_GPU_GetConnectedDisplayIds to populate DisplayIds.
        //!                However, if pDisplayIdCount is 0, do not make this call.
        /// <param name="hPhysicalGpu">GPU selection</param>
        /// <param name="pDisplaySetting"></param>
        /// <param name="pOverlapX"></param>
        /// <param name="pOverlapY"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GPU_GetConnectedDisplayIds(PhysicalGpuHandle hPhysicalGpu, ref UInt32 pDisplayCount, NV_GPU_CONNECTED_IDS_FLAG flags)
        {
            NVAPI_STATUS status;

            if (GPU_GetConnectedDisplayIdsInternalNull != null) { status = GPU_GetConnectedDisplayIdsInternalNull(hPhysicalGpu, IntPtr.Zero, ref pDisplayCount, flags); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_Disp_GetHdrCapabilities(__in NvU32 displayId, __inout NV_HDR_CAPABILITIES *pHdrCapabilities);
        private delegate NVAPI_STATUS Disp_GetHdrCapabilitiesDelegate(
            [In] UInt32 displayId,
            [In][Out] ref NV_HDR_CAPABILITIES_V2 pHdrCapabilities);
        private static readonly Disp_GetHdrCapabilitiesDelegate Disp_GetHdrCapabilitiesInternal;
        /// <summary>
        //!  This API gets High Dynamic Range (HDR) capabilities of the display.
        /// <param name="displayId"></param>
        /// <param name="pHdrCapabilities"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Disp_GetHdrCapabilities(UInt32 displayId, ref NV_HDR_CAPABILITIES_V2 pHdrCapabilities)
        {
            NVAPI_STATUS status;
            pHdrCapabilities.Version = NVImport.NV_HDR_CAPABILITIES_V2_VER;
            if (Disp_GetHdrCapabilitiesInternal != null) { status = Disp_GetHdrCapabilitiesInternal(displayId, ref pHdrCapabilities); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }


        //NVAPI_INTERFACE NvAPI_Disp_HdrColorControl(__in NvU32 displayId, __inout NV_HDR_COLOR_DATA *pHdrColorData);
        private delegate NVAPI_STATUS Disp_HdrColorControlDelegate(
            [In] UInt32 displayId,
            [In][Out] ref NV_HDR_COLOR_DATA_V2 pHdrColorData);
        private static readonly Disp_HdrColorControlDelegate Disp_HdrColorControlInternal;
        /// <summary>
        //!  This API gets High Dynamic Range (HDR) capabilities of the display.
        /// <param name="displayId"></param>
        /// <param name="pHdrCapabilities"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Disp_HdrColorControl(UInt32 displayId, ref NV_HDR_COLOR_DATA_V2 pHdrColorData)
        {
            NVAPI_STATUS status;
            pHdrColorData.Version = NVImport.NV_HDR_COLOR_DATA_V2_VER;
            if (Disp_HdrColorControlInternal != null) { status = Disp_HdrColorControlInternal(displayId, ref pHdrColorData); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_Disp_ColorControl(__in NvU32 displayId, __inout NV_HDR_COLOR_DATA *pHdrColorData);
        private delegate NVAPI_STATUS Disp_ColorControlDelegate(
            [In] UInt32 displayId,
            [In][Out] ref NV_COLOR_DATA_V5 colorData);
        private static readonly Disp_ColorControlDelegate Disp_ColorControlInternal;
        /// <summary>
        //!  This API gets and sets the color capabilities of the display.
        /// <param name="displayId"></param>
        /// <param name="colorData"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_Disp_ColorControl(UInt32 displayId, ref NV_COLOR_DATA_V5 colorData)
        {
            NVAPI_STATUS status;
            colorData.Version = NVImport.NV_COLOR_DATA_V5_VER;
            if (Disp_ColorControlInternal != null) { status = Disp_ColorControlInternal(displayId, ref colorData); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_DISP_EnumCustomDisplay(__in NvU32 displayId, __inout NV_HDR_CAPABILITIES *pHdrCapabilities);
        private delegate NVAPI_STATUS Disp_EnumCustomDisplayDelegate(
            [In] UInt32 displayId,
            [In] UInt32 index,
            [In][Out] ref NV_CUSTOM_DISPLAY_V1 pCustDisp);
        private static readonly Disp_EnumCustomDisplayDelegate Disp_EnumCustomDisplayInternal;
        /// <summary>
        //!  This API gets High Dynamic Range (HDR) capabilities of the display.
        /// <param name="displayId"></param>
        /// <param name="pHdrCapabilities"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DISP_EnumCustomDisplay(UInt32 displayId, UInt32 index, ref NV_CUSTOM_DISPLAY_V1 pCustDisp)
        {
            NVAPI_STATUS status;
            pCustDisp.Version = NVImport.NV_CUSTOM_DISPLAY_V1_VER;
            pCustDisp.SourcePartition = new NV_VIEWPORTF(0, 0, 1, 1);
            if (Disp_EnumCustomDisplayInternal != null) { status = Disp_EnumCustomDisplayInternal(displayId, index, ref pCustDisp); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_GPU_GetFullName(NvPhysicalGpuHandle hPhysicalGpu, NvAPI_ShortString szName);
        private delegate NVAPI_STATUS GPU_GetFullNameDelegate(
            [In] PhysicalGpuHandle gpuHandle,
            [In][Out] StringBuilder gpuNameBuffer);
        private static readonly GPU_GetFullNameDelegate GPU_GetFullNameInternal;
        /// <summary>
        //!  This API gets High Dynamic Range (HDR) capabilities of the display.
        /// <param name="displayId"></param>
        /// <param name="pHdrCapabilities"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GPU_GetFullName(PhysicalGpuHandle gpuHandle, ref String gpuName)
        {
            NVAPI_STATUS status;

            StringBuilder gpuNameBuffer = new StringBuilder((int)NVImport.NV_SHORT_STRING_MAX);
            //IntPtr gpuNameBuffer = (IntPtr)Marshal.StringToHGlobalAnsi(gpuName);

            if (GPU_GetFullNameInternal != null) { status = GPU_GetFullNameInternal(gpuHandle, gpuNameBuffer); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            // Convert the char array to a string
            //gpuName = Marshal.PtrToStringAnsi(gpuNameBuffer);

            //Marshal.ZeroFreeGlobalAllocAnsi(gpuNameBuffer);
            //Marshal.FreeHGlobal(gpuNameBuffer);

            //string gpuName2 = Marshal.PtrToStringAnsi(gpuNameBuffer);
            gpuName = gpuNameBuffer.ToString();

            return status;
        }

        //NVAPI_INTERFACE NvAPI_GPU_GetBoardInfo(NvPhysicalGpuHandle hPhysicalGpu, NV_BOARD_INFO *pBoardInfo);
        private delegate NVAPI_STATUS GPU_GetBoardInfoDelegate(
            [In] PhysicalGpuHandle gpuHandle,
            [In][Out] ref NV_BOARD_INFO_V1 boardInfo);
        private static readonly GPU_GetBoardInfoDelegate GPU_GetBoardInfoInternal;
        /// <summary>
        //!  This API Retrieves the Board information (a unique GPU Board Serial Number) stored in the InfoROM.
        /// <param name="gpuHandle"></param>
        /// <param name="boardInfo"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GPU_GetBoardInfo(PhysicalGpuHandle gpuHandle, ref NV_BOARD_INFO_V1 boardInfo)
        {
            NVAPI_STATUS status;

            boardInfo = new NV_BOARD_INFO_V1();
            boardInfo.BoardNum = new byte[16];
            boardInfo.Version = NV_BOARD_INFO_V1_VER;

            if (GPU_GetBoardInfoInternal != null) { status = GPU_GetBoardInfoInternal(gpuHandle, ref boardInfo); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }


        //NVAPI_INTERFACE NvAPI_GPU_GetBusId(NvPhysicalGpuHandle hPhysicalGpu, NvU32 *pBusId);
        private delegate NVAPI_STATUS GPU_GetBusIdDelegate(
            [In] PhysicalGpuHandle gpuHandle,
            [In][Out] ref UInt32 busId);
        private static readonly GPU_GetBusIdDelegate GPU_GetBusIdInternal;
        /// <summary>
        //!  Returns the ID of the bus associated with this GPU.
        /// <param name="gpuHandle"></param>
        /// <param name="busId"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GPU_GetBusId(PhysicalGpuHandle gpuHandle, ref UInt32 busId)
        {
            NVAPI_STATUS status;

            if (GPU_GetBusIdInternal != null) { status = GPU_GetBusIdInternal(gpuHandle, ref busId); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_GPU_GetBusType(NvPhysicalGpuHandle hPhysicalGpu, NV_GPU_BUS_TYPE* pBusType);
        private delegate NVAPI_STATUS GPU_GetBusTypeDelegate(
            [In] PhysicalGpuHandle gpuHandle,
            [In][Out] ref NV_GPU_BUS_TYPE busType);
        private static readonly GPU_GetBusTypeDelegate GPU_GetBusTypeInternal;
        /// <summary>
        //!  This function returns the type of bus associated with this GPU.
        /// <param name="gpuHandle"></param>
        /// <param name="busId"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GPU_GetBusType(PhysicalGpuHandle gpuHandle, ref NV_GPU_BUS_TYPE busType)
        {
            NVAPI_STATUS status;

            if (GPU_GetBusTypeInternal != null) { status = GPU_GetBusTypeInternal(gpuHandle, ref busType); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_SYS_GetGpuAndOutputIdFromDisplayId(NvU32 displayId, NvPhysicalGpuHandle *hPhysicalGpu, NvU32 *outputId);
        private delegate NVAPI_STATUS SYS_GetGpuAndOutputIdFromDisplayIdDelegate(
            [In] UInt32 displayId,
            [Out] out PhysicalGpuHandle gpuHandle,
            [Out] out UInt32 gpuOutputId);
        private static readonly SYS_GetGpuAndOutputIdFromDisplayIdDelegate SYS_GetGpuAndOutputIdFromDisplayIdInternal;
        /// <summary>
        //!  This API converts a display ID to a Physical GPU handle and output ID.
        /// <param name="gpuHandle"></param>
        /// <param name="busId"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_SYS_GetGpuAndOutputIdFromDisplayId(UInt32 displayId, out PhysicalGpuHandle gpuHandle, out UInt32 gpuOutputId)
        {
            NVAPI_STATUS status;

            PhysicalGpuHandle myGpuHandle = new PhysicalGpuHandle();
            UInt32 myGpuOutputId = 0;

            if (SYS_GetGpuAndOutputIdFromDisplayIdInternal != null) { status = SYS_GetGpuAndOutputIdFromDisplayIdInternal(displayId, out myGpuHandle, out myGpuOutputId); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            gpuHandle = myGpuHandle;
            gpuOutputId = myGpuOutputId;

            return status;
        }

        //NVAPI_INTERFACE NvAPI_SYS_GetGpuAndOutputIdFromDisplayId(NvU32 displayId, NvPhysicalGpuHandle *hPhysicalGpu, NvU32 *outputId);
        private delegate NVAPI_STATUS GPU_GetEDIDDelegate(
            [In] PhysicalGpuHandle gpuHandle,
            [In] UInt32 gpuOutputId,
            [In][Out] ref NV_EDID_V3 edidInfo);
        private static readonly GPU_GetEDIDDelegate GPU_GetEDIDInternal;
        /// <summary>
        //!  This function returns the EDID data for the specified GPU handle and connection bit mask.
        //!  displayOutputId should have exactly 1 bit set to indicate a single display. See \ref handles.
        /// <param name="gpuHandle"></param>
        /// <param name="gpuOutputId"></param>
        /// <param name="edidInfo"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GPU_GetEDID(PhysicalGpuHandle gpuHandle, UInt32 gpuOutputId, ref NV_EDID_V3 edidInfo)
        {
            NVAPI_STATUS status;

            edidInfo = new NV_EDID_V3();
            edidInfo.Version = NVImport.NV_EDID_V3_VER;
            edidInfo.EDID_Data = new Byte[(int)NVImport.NV_EDID_DATA_SIZE];

            if (GPU_GetEDIDInternal != null) { status = GPU_GetEDIDInternal(gpuHandle, gpuOutputId, ref edidInfo); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_GPU_GetLogicalGpuInfo(__in NvLogicalGpuHandle hLogicalGpu, __inout NV_LOGICAL_GPU_DATA * pLogicalGpuData)
        private delegate NVAPI_STATUS GPU_GetLogicalGpuInfoDelegate(
            [In] LogicalGpuHandle gpuHandle,
            [In][Out] ref NV_LOGICAL_GPU_DATA_V1 logicalGPUData);
        private static readonly GPU_GetLogicalGpuInfoDelegate GPU_GetLogicalGpuInfoInternal;
        /// <summary>
        /// This function is used to query Logical GPU information.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="gpuHandle"></param>
        /// <param name="logicalGPUData"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GPU_GetLogicalGpuInfo(LogicalGpuHandle gpuHandle, ref NV_LOGICAL_GPU_DATA_V1 logicalGPUData)
        {
            NVAPI_STATUS status;

            logicalGPUData = new NV_LOGICAL_GPU_DATA_V1();
            logicalGPUData.Version = NVImport.NV_LOGICAL_GPU_DATA_V1_VER;
            logicalGPUData.OSAdapterId = IntPtr.Zero;
            logicalGPUData.PhysicalGPUHandles = new PhysicalGpuHandle[(int)NVImport.NVAPI_MAX_PHYSICAL_GPUS];

            if (GPU_GetEDIDInternal != null) { status = GPU_GetLogicalGpuInfoInternal(gpuHandle, ref logicalGPUData); }
            else { status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND; }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_GetLogicalGPUFromPhysicalGPU(NvPhysicalGpuHandle hPhysicalGPU, NvLogicalGpuHandle* pLogicalGPU)
        private delegate NVAPI_STATUS GetLogicalGPUFromPhysicalGPUDelegate(
            [In] PhysicalGpuHandle physicalGPUHandle,
            [Out] out LogicalGpuHandle logicalGPUHandle);
        private static readonly GetLogicalGPUFromPhysicalGPUDelegate GetLogicalGPUFromPhysicalGPUInternal;
        /// <summary>
        /// This function is used to query Logical GPU information.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="physicalGPUHandle"></param>
        /// <param name="logicalGPUHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_GetLogicalGPUFromPhysicalGPU(PhysicalGpuHandle physicalGPUHandle, out LogicalGpuHandle logicalGPUHandle)
        {
            NVAPI_STATUS status;

            if (GetLogicalGPUFromPhysicalGPUInternal != null)
            {
                status = GetLogicalGPUFromPhysicalGPUInternal(physicalGPUHandle, out LogicalGpuHandle lgpu);
                logicalGPUHandle = lgpu;
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                logicalGPUHandle = new LogicalGpuHandle();
            }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_DRS_CreateSession (NvDRSSessionHandle* phSession)	
        private delegate NVAPI_STATUS DRS_CreateSessionDelegate(
            [Out] out NvDRSSessionHandle drsSessionHandle);
        private static readonly DRS_CreateSessionDelegate DRS_CreateSessionInternal;
        /// <summary>
        /// This API allocates memory and initializes the session.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_CreateSession(out NvDRSSessionHandle drsSessionHandle)
        {
            NVAPI_STATUS status;

            if (DRS_CreateSessionInternal != null)
            {
                status = DRS_CreateSessionInternal(out NvDRSSessionHandle drsSession);
                drsSessionHandle = drsSession;
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsSessionHandle = new NvDRSSessionHandle();
            }

            return status;
        }


        //NVAPI_INTERFACE NvAPI_DRS_DestroySession (NvDRSSessionHandle* phSession)	
        private delegate NVAPI_STATUS DRS_DestroySessionDelegate(
            [In] NvDRSSessionHandle drsSessionHandle);
        private static readonly DRS_DestroySessionDelegate DRS_DestroySessionInternal;
        /// <summary>
        /// This API frees the allocation: cleanup of NvDrsSession.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_DestroySession(NvDRSSessionHandle drsSessionHandle)
        {
            NVAPI_STATUS status;

            if (DRS_DestroySessionInternal != null)
            {
                status = DRS_DestroySessionInternal(drsSessionHandle);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_DRS_LoadSettings(NvDRSSessionHandle hSession);
        private delegate NVAPI_STATUS DRS_LoadSettingsDelegate(
            [In] NvDRSSessionHandle drsSessionHandle);
        private static readonly DRS_LoadSettingsDelegate DRS_LoadSettingsInternal;
        /// <summary>
        /// This API loads and parses the settings data.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_LoadSettings(NvDRSSessionHandle drsSessionHandle)
        {
            NVAPI_STATUS status;

            if (DRS_LoadSettingsInternal != null)
            {
                status = DRS_LoadSettingsInternal(drsSessionHandle);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            return status;
        }

        //NVAPI_INTERFACE NvAPI_DRS_SaveSettings(NvDRSSessionHandle hSession);
        private delegate NVAPI_STATUS DRS_SaveSettingsDelegate(
            [In] NvDRSSessionHandle drsSessionHandle);
        private static readonly DRS_SaveSettingsDelegate DRS_SaveSettingsInternal;
        /// <summary>
        /// This API saves the settings data to the system.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_SaveSettings(NvDRSSessionHandle drsSessionHandle)
        {
            NVAPI_STATUS status;

            if (DRS_SaveSettingsInternal != null)
            {
                status = DRS_SaveSettingsInternal(drsSessionHandle);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            return status;
        }


        // NVAPI_INTERFACE NvAPI_DRS_GetCurrentGlobalProfile(NvDRSSessionHandle hSession, NvDRSProfileHandle* phProfile )
        private delegate NVAPI_STATUS DRS_GetBaseProfileDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [Out] out NvDRSProfileHandle drsProfileHandle);
        private static readonly DRS_GetBaseProfileDelegate DRS_GetBaseProfileInternal;
        /// <summary>
        /// This API returns the handle to the current global profile.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsProfileHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_GetBaseProfile(NvDRSSessionHandle drsSessionHandle, out NvDRSProfileHandle drsProfileHandle)
        {
            NVAPI_STATUS status;

            if (DRS_GetBaseProfileInternal != null)
            {
                status = DRS_GetBaseProfileInternal(drsSessionHandle, out drsProfileHandle);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsProfileHandle = new NvDRSProfileHandle();
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_GetCurrentGlobalProfile(NvDRSSessionHandle hSession, NvDRSProfileHandle* phProfile )
        private delegate NVAPI_STATUS DRS_GetCurrentGlobalProfileDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [Out] out NvDRSProfileHandle drsProfileHandle);
        private static readonly DRS_GetCurrentGlobalProfileDelegate DRS_GetCurrentGlobalProfileInternal;
        /// <summary>
        /// This API returns the handle to the current global profile.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsProfileHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_GetCurrentGlobalProfile(NvDRSSessionHandle drsSessionHandle, out NvDRSProfileHandle drsProfileHandle)
        {
            NVAPI_STATUS status;

            if (DRS_GetCurrentGlobalProfileInternal != null)
            {
                status = DRS_GetCurrentGlobalProfileInternal(drsSessionHandle, out drsProfileHandle);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsProfileHandle = new NvDRSProfileHandle();
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_SetCurrentGlobalProfile(NvDRSSessionHandle hSession, NvAPI_UnicodeString wszGlobalProfileName)		
        private delegate NVAPI_STATUS DRS_SetCurrentGlobalProfileDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [In] string drsProfileName);
        private static readonly DRS_SetCurrentGlobalProfileDelegate DRS_SetCurrentGlobalProfileInternal;
        /// <summary>
        /// This API returns the handle to the current global profile.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsProfileName"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_SetCurrentGlobalProfile(NvDRSSessionHandle drsSessionHandle, string drsProfileName)
        {
            NVAPI_STATUS status;

            if (DRS_SetCurrentGlobalProfileInternal != null)
            {
                status = DRS_SetCurrentGlobalProfileInternal(drsSessionHandle, drsProfileName);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_GetProfileInfo(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile, NVDRS_PROFILE* pProfileInfo )
        private delegate NVAPI_STATUS DRS_GetProfileInfoDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [In] NvDRSProfileHandle drsProfileHandle,
            [In][Out] ref NVDRS_PROFILE_V1 drsProfileInfo);
        private static readonly DRS_GetProfileInfoDelegate DRS_GetProfileInfoInternal;
        /// <summary>
        /// This API gets information about the given profile. User needs to specify the name of the Profile.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsProfileHandle"></param>
        /// <param name="drsProfileInfo"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_GetProfileInfo(NvDRSSessionHandle drsSessionHandle, NvDRSProfileHandle drsProfileHandle, ref NVDRS_PROFILE_V1 drsProfileInfo)
        {
            NVAPI_STATUS status;

            if (DRS_GetProfileInfoInternal != null)
            {
                drsProfileInfo.Version = NVImport.NVDRS_PROFILE_V1_VER;
                status = DRS_GetProfileInfoInternal(drsSessionHandle, drsProfileHandle, ref drsProfileInfo);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsProfileInfo = new NVDRS_PROFILE_V1();
            }

            return status;
        }


        // NVAPI_INTERFACE NvAPI_DRS_SetProfileInfo(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile, NVDRS_PROFILE* pProfileInfo )
        private delegate NVAPI_STATUS DRS_SetProfileInfoDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [In] NvDRSProfileHandle drsProfileHandle,
            [In] NVDRS_PROFILE_V1 drsProfileInfo);
        private static readonly DRS_SetProfileInfoDelegate DRS_SetProfileInfoInternal;
        /// <summary>
        /// This API gets information about the given profile. User needs to specify the name of the Profile.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsProfileHandle"></param>
        /// <param name="drsProfileInfo"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_SetProfileInfo(NvDRSSessionHandle drsSessionHandle, NvDRSProfileHandle drsProfileHandle, NVDRS_PROFILE_V1 drsProfileInfo)
        {
            NVAPI_STATUS status;

            if (DRS_SetProfileInfoInternal != null)
            {
                status = DRS_SetProfileInfoInternal(drsSessionHandle, drsProfileHandle, drsProfileInfo);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_EnumProfiles(NvDRSSessionHandle hSession, NvU32 index, NvDRSProfileHandle* phProfile)
        private delegate NVAPI_STATUS DRS_EnumProfilesDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [In] UInt32 drsIndex,
            [Out] out NvDRSProfileHandle drsProfileHandle);
        private static readonly DRS_EnumProfilesDelegate DRS_EnumProfilesInternal;
        /// <summary>
        /// This API enumerates through all the profiles in the session.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsIndex"></param>
        /// <param name="drsProfileInfo"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_EnumProfiles(NvDRSSessionHandle drsSessionHandle, UInt32 drsIndex, out NvDRSProfileHandle drsProfileHandle)
        {
            NVAPI_STATUS status;

            if (DRS_EnumProfilesInternal != null)
            {
                status = DRS_EnumProfilesInternal(drsSessionHandle, drsIndex, out drsProfileHandle);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsProfileHandle = new NvDRSProfileHandle();
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_GetNumProfiles(NvDRSSessionHandle hSession, NvU32 *numProfiles);
        private delegate NVAPI_STATUS DRS_GetNumProfilesDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [Out] out UInt32 drsNumProfiles);
        private static readonly DRS_GetNumProfilesDelegate DRS_GetNumProfilesInternal;
        /// <summary>
        /// This API obtains the number of profiles in the current session object.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsNumProfiles"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_GetNumProfiles(NvDRSSessionHandle drsSessionHandle, out UInt32 drsNumProfiles)
        {
            NVAPI_STATUS status;

            if (DRS_GetNumProfilesInternal != null)
            {
                status = DRS_GetNumProfilesInternal(drsSessionHandle, out drsNumProfiles);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsNumProfiles = 0;
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_SetSetting(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile, NVDRS_SETTING* pSetting)
        private delegate NVAPI_STATUS DRS_SetSettingDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [In] NvDRSProfileHandle drsProfileHandle,
            [In] NVDRS_SETTING_V1 drsSetting);
        private static readonly DRS_SetSettingDelegate DRS_SetSettingInternal;
        /// <summary>
        /// This API adds/modifies a setting to a profile.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsProfileHandle"></param>
        /// <param name="drsSetting"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_SetSetting(NvDRSSessionHandle drsSessionHandle, NvDRSProfileHandle drsProfileHandle, NVDRS_SETTING_V1 drsSetting)
        {
            NVAPI_STATUS status;

            if (DRS_SetSettingInternal != null)
            {
                status = DRS_SetSettingInternal(drsSessionHandle, drsProfileHandle, drsSetting);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsSetting = new NVDRS_SETTING_V1();
            }

            return status;
        }


        // NVAPI_INTERFACE NvAPI_DRS_GetSetting(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile, NvU32 settingId, NVDRS_SETTING* pSetting)
        private delegate NVAPI_STATUS DRS_GetSettingDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [In] NvDRSProfileHandle drsProfileHandle,
            [In] UInt32 drsSettingId,
            [Out] out NVDRS_SETTING_V1 drsSetting);
        private static readonly DRS_GetSettingDelegate DRS_GetSettingInternal;
        /// <summary>
        /// This API gets information about the given setting.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsProfileHandle"></param>
        /// <param name="drsSetting"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_GetSetting(NvDRSSessionHandle drsSessionHandle, NvDRSProfileHandle drsProfileHandle, UInt32 drsSettingId, out NVDRS_SETTING_V1 drsSetting)
        {
            NVAPI_STATUS status;

            if (DRS_GetSettingInternal != null)
            {
                status = DRS_GetSettingInternal(drsSessionHandle, drsProfileHandle, drsSettingId, out drsSetting);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsSetting = new NVDRS_SETTING_V1();
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_EnumSettings(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile, NvU32 startIndex, NvU32* settingsCount, NVDRS_SETTING* pSetting)
        private delegate NVAPI_STATUS DRS_EnumSettingsDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [In] NvDRSProfileHandle drsProfileHandle,
            [In] UInt32 drsStartIndex,
            [In][Out] ref UInt32 drsSettingCount,
            [In][Out] NVDRS_SETTING_V1[] drsSettings);
        private static readonly DRS_EnumSettingsDelegate DRS_EnumSettingsInternal;
        /// <summary>
        /// This API enumerates all the settings of a given profile from startIndex to the maximum length.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsProfileHandle"></param>
        /// <param name="drsStartIndex"></param>
        /// <param name="drsSettingCount"></param>
        /// <param name="drsSettings"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_EnumSettings(NvDRSSessionHandle drsSessionHandle, NvDRSProfileHandle drsProfileHandle, UInt32 drsStartIndex, ref UInt32 drsSettingCount, ref NVDRS_SETTING_V1[] drsSettings)
        {
            NVAPI_STATUS status;

            if (DRS_EnumSettingsInternal != null)
            {
                if (drsSettingCount == 0)
                {
                    drsSettingCount = NVImport.NVAPI_SETTING_MAX_VALUES;
                }
                NVDRS_SETTING_V1[] drsSettingsInternal = new NVDRS_SETTING_V1[drsSettingCount];
                for (int i = 0; i < drsSettingCount; i++)
                {
                    drsSettingsInternal[i].InternalVersion = NVImport.NVDRS_SETTING_V1_VER;

                }
                status = DRS_EnumSettingsInternal(drsSessionHandle, drsProfileHandle, drsStartIndex, ref drsSettingCount, drsSettingsInternal);
                drsSettings = new NVDRS_SETTING_V1[drsSettingCount];
                Array.Copy(drsSettingsInternal, drsSettings, drsSettingCount);
                /*for (int i = 0; i < drsSettingCount; i++)
                {
                    drsSettings[i] = drsSettingsInternal[i].Clone();
                }*/

            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsSettings = new NVDRS_SETTING_V1[0];
                drsSettingCount = 0;
            }

            return status;
        }


        // NVAPI_INTERFACE NvAPI_DRS_EnumAvailableSettingIds(NvU32* pSettingIds, NvU32* pMaxCount)
        private delegate NVAPI_STATUS DRS_EnumAvailableSettingIdsDelegate(
            [In][Out][MarshalAs(UnmanagedType.SysUInt, SizeConst = (int)Int32.MaxValue)] UInt32[] drsSettingsIds,
            [In][Out] ref UInt32 drsSettingCount);
        private static readonly DRS_EnumAvailableSettingIdsDelegate DRS_EnumAvailableSettingIdsInternal;
        /// <summary>
        /// This API enumerates all the Ids of all the settings recognized by NVAPI.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSettingsIds"></param>
        /// <param name="drsSettingCount"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_EnumAvailableSettingIds(ref UInt32[] drsSettingsIds, ref UInt32 drsSettingCount)
        {
            NVAPI_STATUS status;

            if (DRS_EnumAvailableSettingIdsInternal != null)
            {
                UInt32 drsSettingCountInternal = Int32.MaxValue;
                UInt32[] drsSettingIdsInternal = new UInt32[drsSettingCountInternal];
                status = DRS_EnumAvailableSettingIdsInternal(drsSettingIdsInternal, ref drsSettingCountInternal);
                drsSettingCount = drsSettingCountInternal;
                drsSettingsIds = new UInt32[drsSettingCount];
                Array.Copy(drsSettingIdsInternal, drsSettingsIds, drsSettingCount);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsSettingsIds = new UInt32[0];
                drsSettingCount = 0;
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_EnumAvailableSettingValues(NvU32 settingId, NvU32* pMaxNumValues, NVDRS_SETTING_VALUES* pSettingValues)
        private delegate NVAPI_STATUS DRS_EnumAvailableSettingValuesDelegate(
            [In] UInt32 drsSettingId,
            [In, Out] ref UInt32 drsMaxNumValues,
            [In][Out] NVDRS_SETTING_VALUES_V1[] drsSettingsValues);
        private static readonly DRS_EnumAvailableSettingValuesDelegate DRS_EnumAvailableSettingValuesInternal;
        /// <summary>
        /// This API enumerates all available setting values for a given setting.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSettingId"></param>
        /// <param name="drsMaxNumValues"></param>
        /// <param name="drsSettingsValues"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_EnumAvailableSettingValues(UInt32 drsSettingId, ref UInt32 drsMaxNumValues, ref NVDRS_SETTING_VALUES_V1[] drsSettingsValues)
        {
            NVAPI_STATUS status;

            if (DRS_EnumAvailableSettingValuesInternal != null)
            {
                status = DRS_EnumAvailableSettingValuesInternal(drsSettingId, ref drsMaxNumValues, drsSettingsValues);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsSettingsValues = new NVDRS_SETTING_VALUES_V1[0];
                drsMaxNumValues = 0;
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_GetSettingIdFromName(NvAPI_UnicodeString settingName, NvU32* pSettingId)
        private delegate NVAPI_STATUS DRS_GetSettingIdFromNameDelegate(
            [In] string drsSettingName,
            [Out] out UInt32 drsSettingId);
        private static readonly DRS_GetSettingIdFromNameDelegate DRS_GetSettingIdFromNameInternal;
        /// <summary>
        /// This API gets the binary ID of a setting given the setting name.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSettingName"></param>
        /// <param name="drsSettingId"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_GetSettingIdFromName(string drsSettingName, out UInt32 drsSettingId)
        {
            NVAPI_STATUS status;

            if (DRS_GetSettingIdFromNameInternal != null)
            {
                status = DRS_GetSettingIdFromNameInternal(drsSettingName, out drsSettingId);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsSettingId = 0;
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_GetSettingNameFromId(NvU32 settingId, NvAPI_UnicodeString* pSettingName)
        private delegate NVAPI_STATUS DRS_GetSettingNameFromIdDelegate(
            [In] UInt32 drsSettingId,
            [Out] out string drsSettingName);
        private static readonly DRS_GetSettingNameFromIdDelegate DRS_GetSettingNameFromIdInternal;
        /// <summary>
        /// This API gets the setting name given the binary ID.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSettingId"></param>
        /// <param name="drsSettingName"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_GetSettingNameFromId(UInt32 drsSettingId, out string drsSettingName)
        {
            NVAPI_STATUS status;

            if (DRS_GetSettingNameFromIdInternal != null)
            {
                status = DRS_GetSettingNameFromIdInternal(drsSettingId, out drsSettingName);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
                drsSettingName = "";
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_RestoreProfileDefaultSetting(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile, NvU32 settingId);
        private delegate NVAPI_STATUS DRS_RestoreProfileDefaultSettingDelegate(
            [In] NvDRSSessionHandle drsSessionHandle,
            [In] NvDRSProfileHandle drsProfileHandle,
            [In] UInt32 drsSettingId);
        private static readonly DRS_RestoreProfileDefaultSettingDelegate DRS_RestoreProfileDefaultSettingInternal;
        /// <summary>
        /// This API gets information about the given setting.
        /// SUPPORTED OS: Windows 7 and higher
        /// <param name="drsSessionHandle"></param>
        /// <param name="drsProfileHandle"></param>
        /// <returns></returns>
        public static NVAPI_STATUS NvAPI_DRS_RestoreProfileDefaultSetting(NvDRSSessionHandle drsSessionHandle, NvDRSProfileHandle drsProfileHandle, UInt32 drsSettingId)
        {
            NVAPI_STATUS status;

            if (DRS_RestoreProfileDefaultSettingInternal != null)
            {
                status = DRS_RestoreProfileDefaultSettingInternal(drsSessionHandle, drsProfileHandle, drsSettingId);
            }
            else
            {
                status = NVAPI_STATUS.NVAPI_FUNCTION_NOT_FOUND;
            }

            return status;
        }

        // NVAPI_INTERFACE NvAPI_DRS_CreateProfile(NvDRSSessionHandle hSession, NVDRS_PROFILE* pProfileInfo, NvDRSProfileHandle* phProfile)

        // NVAPI_INTERFACE NvAPI_DRS_DeleteProfile(NvDRSSessionHandle hSession, NvDRSProfileHandle hProfile)

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
}