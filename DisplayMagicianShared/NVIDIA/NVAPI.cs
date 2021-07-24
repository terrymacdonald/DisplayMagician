using System;
using System.Runtime.InteropServices;
using FARPROC = System.IntPtr;
using HMODULE = System.IntPtr;

namespace DisplayMagicianShared.NVIDIA
{
    //public delegate IntPtr ADL_Main_Memory_Alloc_Delegate(int size);

    public enum NVAPI_STATUS : int
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
        NVAPI_INVALID_POINTER = -14,     //!< An invalid pointer, usually NULL, was passed as a parameter
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
        NVAPI_INVALID_CALL = -134,    //!< The method call is invalid. For example, a method's parameter may not be a valid pointer.
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
        NVAPI_STEREO_INVALID_DEVICE_INTERFACE = -146,    //!< Invalid device interface.
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
        NVAPI_GPU_WORKSTATION_FEATURE_INCOMPLETE = -192,    //!< The requested workstation feature set has incomplete driver internal allocation resources
        NVAPI_STEREO_INIT_ACTIVATION_NOT_DONE = -193,    //!< Call failed because InitActivation was not called.
        NVAPI_SYNC_NOT_ACTIVE = -194,    //!< The requested action cannot be performed without Sync being enabled.    
        NVAPI_SYNC_MASTER_NOT_FOUND = -195,    //!< The requested action cannot be performed without Sync Master being enabled.
        NVAPI_INVALID_SYNC_TOPOLOGY = -196,    //!< Invalid displays passed in the NV_GSYNC_DISPLAY pointer.
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

    public enum NV_DISPLAYCONFIG_FLAGS : uint
    {
        NV_DISPLAYCONFIG_VALIDATE_ONLY = 0x00000001,
        NV_DISPLAYCONFIG_SAVE_TO_PERSISTENCE = 0x00000002,
        NV_DISPLAYCONFIG_DRIVER_RELOAD_ALLOWED = 0x00000004,               //!< Driver reload is permitted if necessary
        NV_DISPLAYCONFIG_FORCE_MODE_ENUMERATION = 0x00000008,               //!< Refresh OS mode list.
        NV_FORCE_COMMIT_VIDPN = 0x00000010,               //!< Tell OS to avoid optimizing CommitVidPn call during a modeset
    }

    public enum NV_ROTATE : uint
    {
        NV_ROTATE_0 = 0,
        NV_ROTATE_90 = 1,
        NV_ROTATE_180 = 2,
        NV_ROTATE_270 = 3,
        NV_ROTATE_IGNORED = 4,
    }

    public enum NV_FORMAT : uint
    {
        NV_FORMAT_UNKNOWN = 0,       //!< unknown. Driver will choose one as following value.
        NV_FORMAT_P8 = 41,       //!< for 8bpp mode
        NV_FORMAT_R5G6B5 = 23,       //!< for 16bpp mode
        NV_FORMAT_A8R8G8B8 = 21,       //!< for 32bpp mode
        NV_FORMAT_A16B16G16R16F = 113,      //!< for 64bpp(floating point) mode.

    }

    public enum NV_SCALING : uint
    {
        NV_SCALING_DEFAULT = 0,        //!< No change

        // New Scaling Declarations
        NV_SCALING_GPU_SCALING_TO_CLOSEST = 1,  //!< Balanced  - Full Screen
        NV_SCALING_GPU_SCALING_TO_NATIVE = 2,  //!< Force GPU - Full Screen
        NV_SCALING_GPU_SCANOUT_TO_NATIVE = 3,  //!< Force GPU - Centered\No Scaling
        NV_SCALING_GPU_SCALING_TO_ASPECT_SCANOUT_TO_NATIVE = 5,  //!< Force GPU - Aspect Ratio
        NV_SCALING_GPU_SCALING_TO_ASPECT_SCANOUT_TO_CLOSEST = 6,  //!< Balanced  - Aspect Ratio
        NV_SCALING_GPU_SCANOUT_TO_CLOSEST = 7,  //!< Balanced  - Centered\No Scaling
        NV_SCALING_GPU_INTEGER_ASPECT_SCALING = 8,  //!< Force GPU - Integer Scaling

        // Legacy Declarations
        NV_SCALING_MONITOR_SCALING = NV_SCALING_GPU_SCALING_TO_CLOSEST,
        NV_SCALING_ADAPTER_SCALING = NV_SCALING_GPU_SCALING_TO_NATIVE,
        NV_SCALING_CENTERED = NV_SCALING_GPU_SCANOUT_TO_NATIVE,
        NV_SCALING_ASPECT_SCALING = NV_SCALING_GPU_SCALING_TO_ASPECT_SCANOUT_TO_NATIVE,

        NV_SCALING_CUSTOMIZED = 255       //!< For future use
    }

    public enum NV_TARGET_VIEW_MODE : uint
    {
        NV_VIEW_MODE_STANDARD = 0,
        NV_VIEW_MODE_CLONE = 1,
        NV_VIEW_MODE_HSPAN = 2,
        NV_VIEW_MODE_VSPAN = 3,
        NV_VIEW_MODE_DUALVIEW = 4,
        NV_VIEW_MODE_MULTIVIEW = 5,
    }

    public enum NV_DISPLAY_TV_FORMAT : uint
    {
        NV_DISPLAY_TV_FORMAT_NONE = 0,
        NV_DISPLAY_TV_FORMAT_SD_NTSCM = 0x00000001,
        NV_DISPLAY_TV_FORMAT_SD_NTSCJ = 0x00000002,
        NV_DISPLAY_TV_FORMAT_SD_PALM = 0x00000004,
        NV_DISPLAY_TV_FORMAT_SD_PALBDGH = 0x00000008,
        NV_DISPLAY_TV_FORMAT_SD_PALN = 0x00000010,
        NV_DISPLAY_TV_FORMAT_SD_PALNC = 0x00000020,
        NV_DISPLAY_TV_FORMAT_SD_576i = 0x00000100,
        NV_DISPLAY_TV_FORMAT_SD_480i = 0x00000200,
        NV_DISPLAY_TV_FORMAT_ED_480p = 0x00000400,
        NV_DISPLAY_TV_FORMAT_ED_576p = 0x00000800,
        NV_DISPLAY_TV_FORMAT_HD_720p = 0x00001000,
        NV_DISPLAY_TV_FORMAT_HD_1080i = 0x00002000,
        NV_DISPLAY_TV_FORMAT_HD_1080p = 0x00004000,
        NV_DISPLAY_TV_FORMAT_HD_720p50 = 0x00008000,
        NV_DISPLAY_TV_FORMAT_HD_1080p24 = 0x00010000,
        NV_DISPLAY_TV_FORMAT_HD_1080i50 = 0x00020000,
        NV_DISPLAY_TV_FORMAT_HD_1080p50 = 0x00040000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp30 = 0x00080000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp30_3840 = NV_DISPLAY_TV_FORMAT_UHD_4Kp30,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp25 = 0x00100000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp25_3840 = NV_DISPLAY_TV_FORMAT_UHD_4Kp25,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp24 = 0x00200000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp24_3840 = NV_DISPLAY_TV_FORMAT_UHD_4Kp24,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp24_SMPTE = 0x00400000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp50_3840 = 0x00800000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp60_3840 = 0x00900000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp30_4096 = 0x00A00000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp25_4096 = 0x00B00000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp24_4096 = 0x00C00000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp50_4096 = 0x00D00000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp60_4096 = 0x00E00000,
        NV_DISPLAY_TV_FORMAT_UHD_8Kp24_7680 = 0x01000000,
        NV_DISPLAY_TV_FORMAT_UHD_8Kp25_7680 = 0x02000000,
        NV_DISPLAY_TV_FORMAT_UHD_8Kp30_7680 = 0x04000000,
        NV_DISPLAY_TV_FORMAT_UHD_8Kp48_7680 = 0x08000000,
        NV_DISPLAY_TV_FORMAT_UHD_8Kp50_7680 = 0x09000000,
        NV_DISPLAY_TV_FORMAT_UHD_8Kp60_7680 = 0x0A000000,
        NV_DISPLAY_TV_FORMAT_UHD_8Kp100_7680 = 0x0B000000,
        NV_DISPLAY_TV_FORMAT_UHD_8Kp120_7680 = 0x0C000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp48_3840 = 0x0D000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp48_4096 = 0x0E000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp100_4096 = 0x0F000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp100_3840 = 0x10000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp120_4096 = 0x11000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp120_3840 = 0x12000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp100_5120 = 0x13000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp120_5120 = 0x14000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp24_5120 = 0x15000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp25_5120 = 0x16000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp30_5120 = 0x17000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp48_5120 = 0x18000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp50_5120 = 0x19000000,
        NV_DISPLAY_TV_FORMAT_UHD_4Kp60_5120 = 0x20000000,
        NV_DISPLAY_TV_FORMAT_UHD_10Kp24_10240 = 0x21000000,
        NV_DISPLAY_TV_FORMAT_UHD_10Kp25_10240 = 0x22000000,
        NV_DISPLAY_TV_FORMAT_UHD_10Kp30_10240 = 0x23000000,
        NV_DISPLAY_TV_FORMAT_UHD_10Kp48_10240 = 0x24000000,
        NV_DISPLAY_TV_FORMAT_UHD_10Kp50_10240 = 0x25000000,
        NV_DISPLAY_TV_FORMAT_UHD_10Kp60_10240 = 0x26000000,
        NV_DISPLAY_TV_FORMAT_UHD_10Kp100_10240 = 0x27000000,
        NV_DISPLAY_TV_FORMAT_UHD_10Kp120_10240 = 0x28000000,


        NV_DISPLAY_TV_FORMAT_SD_OTHER = 0x30000000,
        NV_DISPLAY_TV_FORMAT_ED_OTHER = 0x40000000,
        NV_DISPLAY_TV_FORMAT_HD_OTHER = 0x50000000,

        NV_DISPLAY_TV_FORMAT_ANY = 0x80000000,

    }

    public enum NV_GPU_CONNECTOR_TYPE : uint
    {
        NVAPI_GPU_CONNECTOR_VGA_15_PIN = 0x00000000,
        NVAPI_GPU_CONNECTOR_TV_COMPOSITE = 0x00000010,
        NVAPI_GPU_CONNECTOR_TV_SVIDEO = 0x00000011,
        NVAPI_GPU_CONNECTOR_TV_HDTV_COMPONENT = 0x00000013,
        NVAPI_GPU_CONNECTOR_TV_SCART = 0x00000014,
        NVAPI_GPU_CONNECTOR_TV_COMPOSITE_SCART_ON_EIAJ4120 = 0x00000016,
        NVAPI_GPU_CONNECTOR_TV_HDTV_EIAJ4120 = 0x00000017,
        NVAPI_GPU_CONNECTOR_PC_POD_HDTV_YPRPB = 0x00000018,
        NVAPI_GPU_CONNECTOR_PC_POD_SVIDEO = 0x00000019,
        NVAPI_GPU_CONNECTOR_PC_POD_COMPOSITE = 0x0000001A,
        NVAPI_GPU_CONNECTOR_DVI_I_TV_SVIDEO = 0x00000020,
        NVAPI_GPU_CONNECTOR_DVI_I_TV_COMPOSITE = 0x00000021,
        NVAPI_GPU_CONNECTOR_DVI_I = 0x00000030,
        NVAPI_GPU_CONNECTOR_DVI_D = 0x00000031,
        NVAPI_GPU_CONNECTOR_ADC = 0x00000032,
        NVAPI_GPU_CONNECTOR_LFH_DVI_I_1 = 0x00000038,
        NVAPI_GPU_CONNECTOR_LFH_DVI_I_2 = 0x00000039,
        NVAPI_GPU_CONNECTOR_SPWG = 0x00000040,
        NVAPI_GPU_CONNECTOR_OEM = 0x00000041,
        NVAPI_GPU_CONNECTOR_DISPLAYPORT_EXTERNAL = 0x00000046,
        NVAPI_GPU_CONNECTOR_DISPLAYPORT_INTERNAL = 0x00000047,
        NVAPI_GPU_CONNECTOR_DISPLAYPORT_MINI_EXT = 0x00000048,
        NVAPI_GPU_CONNECTOR_HDMI_A = 0x00000061,
        NVAPI_GPU_CONNECTOR_HDMI_C_MINI = 0x00000063,
        NVAPI_GPU_CONNECTOR_LFH_DISPLAYPORT_1 = 0x00000064,
        NVAPI_GPU_CONNECTOR_LFH_DISPLAYPORT_2 = 0x00000065,
        NVAPI_GPU_CONNECTOR_VIRTUAL_WFD = 0x00000070,
        NVAPI_GPU_CONNECTOR_USB_C = 0x00000071,
        NVAPI_GPU_CONNECTOR_UNKNOWN = 0xFFFFFFFF,
    }

    public enum NV_TIMING_OVERRIDE : uint
    {
        NV_TIMING_OVERRIDE_CURRENT = 0,          //!< get the current timing
        NV_TIMING_OVERRIDE_AUTO,                 //!< the timing the driver will use based the current policy
        NV_TIMING_OVERRIDE_EDID,                 //!< EDID timing
        NV_TIMING_OVERRIDE_DMT,                  //!< VESA DMT timing
        NV_TIMING_OVERRIDE_DMT_RB,               //!< VESA DMT timing with reduced blanking
        NV_TIMING_OVERRIDE_CVT,                  //!< VESA CVT timing
        NV_TIMING_OVERRIDE_CVT_RB,               //!< VESA CVT timing with reduced blanking
        NV_TIMING_OVERRIDE_GTF,                  //!< VESA GTF timing
        NV_TIMING_OVERRIDE_EIA861,               //!< EIA 861x pre-defined timing
        NV_TIMING_OVERRIDE_ANALOG_TV,            //!< analog SD/HDTV timing
        NV_TIMING_OVERRIDE_CUST,                 //!< NV custom timings
        NV_TIMING_OVERRIDE_NV_PREDEFINED,        //!< NV pre-defined timing (basically the PsF timings)
        NV_TIMING_OVERRIDE_NV_PSF = NV_TIMING_OVERRIDE_NV_PREDEFINED,
        NV_TIMING_OVERRIDE_NV_ASPR,
        NV_TIMING_OVERRIDE_SDI,                  //!< Override for SDI timing

        NV_TIMING_OVRRIDE_MAX,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DisplayHandle
    {
        private readonly IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UnAttachedDisplayHandle
    {
        public readonly IntPtr ptr;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PhysicalGpuHandle
    {
        private readonly IntPtr ptr;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct NV_TIMINGEXT
    {
        public uint flag;          //!< Reserved for NVIDIA hardware-based enhancement, such as double-scan.
        public ushort rr;            //!< Logical refresh rate to present
        public uint rrx1k;         //!< Physical vertical refresh rate in 0.001Hz
        public uint aspect;        //!< Display aspect ratio Hi(aspect):horizontal-aspect, Low(aspect):vertical-aspect
        public ushort rep;           //!< Bit-wise pixel repetition factor: 0x1:no pixel repetition; 0x2:each pixel repeats twice horizontally,..
        public uint status;        //!< Timing standard
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 40)]
        public string name;      //!< Timing name
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_TIMING
    {
        // VESA scan out timing parameters:
        public ushort HVisible;         //!< horizontal visible
        public ushort HBorder;          //!< horizontal border
        public ushort HFrontPorch;      //!< horizontal front porch
        public ushort HSyncWidth;       //!< horizontal sync width
        public ushort HTotal;           //!< horizontal total
        public byte HSyncPol;         //!< horizontal sync polarity: 1-negative, 0-positive

        public ushort VVisible;         //!< vertical visible
        public ushort VBorder;          //!< vertical border
        public ushort VFrontPorch;      //!< vertical front porch
        public ushort VSyncWidth;       //!< vertical sync width
        public ushort VTotal;           //!< vertical total
        public byte VSyncPol;         //!< vertical sync polarity: 1-negative, 0-positive

        public ushort interlaced;       //!< 1-interlaced, 0-progressive
        public uint pclk;             //!< pixel clock in 10 kHz

        //other timing related extras
        NV_TIMINGEXT etc;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct NV_POSITION
    {
        public int x;
        public int y;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct NV_RESOLUTION
    {
        public uint width;
        public uint height;
        public uint colorDepth;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_VIEWPORTF
    {
        public float x;    //!<  x-coordinate of the viewport top-left point
        public float y;    //!<  y-coordinate of the viewport top-left point
        public float w;    //!<  Width of the viewport
        public float h;    //!<  Height of the viewport
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO
    {
        public uint version;

        // Rotation and Scaling
        public NV_ROTATE rotation;       //!< (IN) rotation setting.
        public NV_SCALING scaling;        //!< (IN) scaling setting.

        // Refresh Rate
        public uint refreshRate1K;  //!< (IN) Non-interlaced Refresh Rate of the mode, multiplied by 1000, 0 = ignored
                                    //!< This is the value which driver reports to the OS.
                                    // Flags
                                    //public uint interlaced:1;   //!< (IN) Interlaced mode flag, ignored if refreshRate == 0
                                    //public uint primary:1;      //!< (IN) Declares primary display in clone configuration. This is *NOT* GDI Primary.
                                    //!< Only one target can be primary per source. If no primary is specified, the first
                                    //!< target will automatically be primary.
                                    //public uint isPanAndScanTarget:1; //!< Whether on this target Pan and Scan is enabled or has to be enabled. Valid only
                                    //!< when the target is part of clone topology.
                                    //public uint disableVirtualModeSupport:1;
                                    //public uint isPreferredUnscaledTarget:1;
                                    //public uint reserved:27;
                                    // TV format information
        public NV_GPU_CONNECTOR_TYPE connector;      //!< Specify connector type. For TV only, ignored if tvFormat == NV_DISPLAY_TV_FORMAT_NONE
        public NV_DISPLAY_TV_FORMAT tvFormat;       //!< (IN) to choose the last TV format set this value to NV_DISPLAY_TV_FORMAT_NONE
                                                    //!< In case of NvAPI_DISP_GetDisplayConfig(), this field will indicate the currently applied TV format;
                                                    //!< if no TV format is applied, this field will have NV_DISPLAY_TV_FORMAT_NONE value.
                                                    //!< In case of NvAPI_DISP_SetDisplayConfig(), this field should only be set in case of TVs;
                                                    //!< for other displays this field will be ignored and resolution & refresh rate specified in input will be used to apply the TV format.

        // Backend (raster) timing standard
        public NV_TIMING_OVERRIDE timingOverride;     //!< Ignored if timingOverride == NV_TIMING_OVERRIDE_CURRENT
        public NV_TIMING timing;             //!< Scan out timing, valid only if timingOverride == NV_TIMING_OVERRIDE_CUST
                                             //!< The value NV_TIMING::NV_TIMINGEXT::rrx1k is obtained from the EDID. The driver may
                                             //!< tweak this value for HDTV, stereo, etc., before reporting it to the OS.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2
    {
        public uint displayId;  //!< Display ID
        NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO[] details;    //!< May be NULL if no advanced settings are required
        public uint targetId;   //!< Windows CCD target ID. Must be present only for non-NVIDIA adapter, for NVIDIA adapter this parameter is ignored.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_DISPLAYCONFIG_PATH_INFO_V2
    {
        public uint Version;
        public uint SourceId;               //!< Identifies sourceId used by Windows CCD. This can be optionally set.

        public uint TargetInfoCount;            //!< Number of elements in targetInfo array
        public NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2[] TargetInfo;
        public NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1[] sourceModeInfo;             //!< May be NULL if mode info is not important
                                                                                  //public uint IsNonNVIDIAAdapter : 1;     //!< True for non-NVIDIA adapter.
                                                                                  //public uint reserved : 31;              //!< Must be 0
                                                                                  //public LUID pOSAdapterID;              //!< Used by Non-NVIDIA adapter for pointer to OS Adapter of LUID
                                                                                  //!< type, type casted to void *.
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NV_DISPLAYCONFIG_SOURCE_MODE_INFO_V1
    {
        public NV_RESOLUTION resolution;
        public NV_FORMAT colorFormat;                //!< Ignored at present, must be NV_FORMAT_UNKNOWN (0)
        public NV_POSITION position;                   //!< Is all positions are 0 or invalid, displays will be automatically
                                                       //!< positioned from left to right with GDI Primary at 0,0, and all
                                                       //!< other displays in the order of the path array.
                                                       //public NV_DISPLAYCONFIG_SPANNING_ORIENTATION spanningOrientation;        //!< Spanning is only supported on XP
                                                       //public uint bGDIPrimary : 1;
                                                       //public uint bSLIFocus : 1;
                                                       //public uint reserved : 30;              //!< Must be 0
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct NV_DISPLAYCONFIG_PATH_TARGET_INFO
    {
        public uint displayId;  //!< Display ID
        public NV_DISPLAYCONFIG_PATH_ADVANCED_TARGET_INFO[] details;    //!< May be NULL if no advanced settings are required
        public uint targetId;   //!< Windows CCD target ID. Must be present only for non-NVIDIA adapter, for NVIDIA adapter this parameter is ignored.
    }


    class NVImport
    {

        public const uint NV_MAX_HEADS = 4;
        public const uint NV_MAX_VID_PROFILES = 4;
        public const uint NV_MAX_VID_STREAMS = 4;
        public const uint NV_ADVANCED_DISPLAY_HEADS = 4;
        public const uint NV_GENERIC_STRING_MAX = 4096;
        public const uint NV_LONG_STRING_MAX = 256;
        public const uint NV_MAX_ACPI_IDS = 16;
        public const uint NV_MAX_AUDIO_DEVICES = 16;
        public const uint NV_MAX_AVAILABLE_CPU_TOPOLOGIES = 256;
        public const uint NV_MAX_AVAILABLE_SLI_GROUPS = 256;
        public const uint NV_MAX_AVAILABLE_DISPLAY_HEADS = 2;
        public const uint NV_MAX_DISPLAYS = NV_PHYSICAL_GPUS * NV_ADVANCED_DISPLAY_HEADS;
        public const uint NV_MAX_GPU_PER_TOPOLOGY = 8;
        public const uint NV_MAX_GPU_TOPOLOGIES = NV_MAX_PHYSICAL_GPUS;
        public const uint NV_MAX_HEADS_PER_GPU = 32;
        public const uint NV_MAX_LOGICAL_GPUS = 64;
        public const uint NV_MAX_PHYSICAL_BRIDGES = 100;
        public const uint NV_MAX_PHYSICAL_GPUS = 64;
        public const uint NV_MAX_VIEW_MODES = 8;
        public const uint NV_PHYSICAL_GPUS = 32;
        public const uint NV_SHORT_STRING_MAX = 64;
        public const uint NV_SYSTEM_HWBC_INVALID_ID = 0xffffffff;
        public const uint NV_SYSTEM_MAX_DISPLAYS = NV_MAX_PHYSICAL_GPUS * NV_MAX_HEADS;
        public const uint NV_SYSTEM_MAX_HWBCS = 128;


        #region Internal Constant
        /// <summary> Nvapi64_FileName </summary>
        public const string NVAPI_DLL = "nvapi64.dll";
        /// <summary> Kernel32_FileName </summary>
        public const string Kernel32_FileName = "kernel32.dll";
        #endregion Internal Constant

        #region DLLImport
        [DllImport(Kernel32_FileName)]
        public static extern HMODULE GetModuleHandle(string moduleName);

        // This function initializes the NvAPI library (if not already initialized) but always increments the ref-counter.
        // This must be called before calling other NvAPI_ functions. Note: It is now mandatory to call NvAPI_Initialize before calling any other NvAPI. NvAPI_Unload should be called to unload the NVAPI Library.
        [DllImport(NVAPI_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern NVAPI_STATUS NvAPI_Initialize();

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
        // This is used to get a session handle to use to maintain state across multiple NVAPI calls
        [DllImport(NVAPI_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern NVAPI_STATUS NvAPI_DRS_CreateSession(out IntPtr session);

        // This is used to destroy a session handle to used to maintain state across multiple NVAPI calls
        [DllImport(NVAPI_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern NVAPI_STATUS NvAPI_DRS_DestorySession(IntPtr session);

        // This API lets caller retrieve the current global display configuration.
        // USAGE: The caller might have to call this three times to fetch all the required configuration details as follows:
        // First Pass: Caller should Call NvAPI_DISP_GetDisplayConfig() with pathInfo set to NULL to fetch pathInfoCount.
        // Second Pass: Allocate memory for pathInfo with respect to the number of pathInfoCount(from First Pass) to fetch targetInfoCount. If sourceModeInfo is needed allocate memory or it can be initialized to NULL.
        // Third Pass(Optional, only required if target information is required): Allocate memory for targetInfo with respect to number of targetInfoCount(from Second Pass).
        [DllImport(NVAPI_DLL, CallingConvention = CallingConvention.Cdecl)]
        public static extern NVAPI_STATUS NvAPI_DISP_GetDisplayConfig(ref ulong pathInfoCount, out NV_DISPLAYCONFIG_PATH_TARGET_INFO_V2 pathInfo);




        #endregion DLLImport

        /*public static ADL_Main_Memory_Alloc_Delegate ADL_Main_Memory_Alloc = ADL_Main_Memory_Alloc_Function;
        /// <summary> Build in memory allocation function</summary>
        /// <param name="size">input size</param>
        /// <returns>return the memory buffer</returns>
        public static IntPtr ADL_Main_Memory_Alloc_Function(int size)
        {
            //Console.WriteLine($"\nCallback called with param: {size}");
            IntPtr result = Marshal.AllocCoTaskMem(size);           
            return result;
        }*/

    }
}