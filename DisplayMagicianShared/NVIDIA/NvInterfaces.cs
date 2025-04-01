using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.NVIDIA
{
    /// <summary>
    ///     Marker interface for all types that should be allocated before passing to the managed code
    /// </summary>
    public interface IAllocatable : IInitializable, IDisposable
    {
        void Allocate();
    }

    /// <summary>
    ///     Interface for all pointer based handles
    /// </summary>
    public interface IHandle
    {
        /// <summary>
        ///     Returns true if the handle is null and not pointing to a valid location in the memory
        /// </summary>
        bool IsNull { get; }

        /// <summary>
        ///     Gets the address of the handle in the memory
        /// </summary>
        IntPtr MemoryAddress { get; }
    }

    /// <summary>
    ///     Marker interface for all types that should be filled with information before passing to un-managed code
    /// </summary>
    public interface IInitializable
    {
    }

    /// <summary>
    ///     Contains data corresponding to color information
    /// </summary>
    public interface IColorData
    {
        /// <summary>
        ///     Gets the color data color depth
        /// </summary>
        ColorDataDepth? ColorDepth { get; }

        /// <summary>
        ///     Gets the color data dynamic range
        /// </summary>
        ColorDataDynamicRange? DynamicRange { get; }

        /// <summary>
        ///     Gets the color data color format
        /// </summary>
        ColorDataFormat ColorFormat { get; }

        /// <summary>
        ///     Gets the color data color space
        /// </summary>
        ColorDataColorimetry Colorimetry { get; }

        /// <summary>
        ///     Gets the color data selection policy
        /// </summary>
        ColorDataSelectionPolicy? SelectionPolicy { get; }

        /// <summary>
        ///     Gets the color data desktop color depth
        /// </summary>
        ColorDataDesktopDepth? DesktopColorDepth { get; }
    }

    /// <summary>
    ///     Holds information regarding a display color space configurations
    /// </summary>
    public interface IDisplayColorData
    {
        /// <summary>
        ///     Gets the first primary color space coordinate (e.g. Red for RGB) [(0.0, 0.0)-(1.0, 1.0)]
        /// </summary>
        ColorDataColorCoordinate FirstColorCoordinate { get; }

        /// <summary>
        ///     Gets the second primary color space coordinate (e.g. Green for RGB) [(0.0, 0.0)-(1.0, 1.0)]
        /// </summary>
        ColorDataColorCoordinate SecondColorCoordinate { get; }

        /// <summary>
        ///     Gets the third primary color space coordinate (e.g. Blue for RGB) [(0.0, 0.0)-(1.0, 1.0)]
        /// </summary>
        ColorDataColorCoordinate ThirdColorCoordinate { get; }

        /// <summary>
        ///     Gets the white color space coordinate [(0.0, 0.0)-(1.0, 1.0)]
        /// </summary>
        ColorDataColorCoordinate WhiteColorCoordinate { get; }
    }

    /// <summary>
    ///     Holds the Digital Vibrance Control information regarding the saturation level.
    /// </summary>
    public interface IDisplayDVCInfo
    {
        /// <summary>
        ///     Gets the current saturation level
        /// </summary>
        int CurrentLevel { get; }

        /// <summary>
        ///     Gets the default saturation level
        /// </summary>
        int DefaultLevel { get; }

        /// <summary>
        ///     Gets the maximum valid saturation level
        /// </summary>
        int MaximumLevel { get; }

        /// <summary>
        ///     Gets the minimum valid saturation level
        /// </summary>
        int MinimumLevel { get; }
    }

    /// <summary>
    ///     Contains information about the HDMI capabilities of the GPU, output and the display device attached
    /// </summary>
    public interface IHDMISupportInfo
    {
        /// <summary>
        ///     Gets the display's EDID 861 Extension Revision
        /// </summary>
        uint EDID861ExtensionRevision { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the GPU is capable of HDMI output
        /// </summary>
        bool IsGPUCapableOfHDMIOutput { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the display is connected via HDMI
        /// </summary>
        bool IsHDMIMonitor { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the connected display is capable of Adobe RGB if such data is available;
        ///     otherwise null
        /// </summary>
        bool? IsMonitorCapableOfAdobeRGB { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the connected display is capable of Adobe YCC601 if such data is available;
        ///     otherwise null
        /// </summary>
        bool? IsMonitorCapableOfAdobeYCC601 { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the connected display is capable of basic audio
        /// </summary>
        bool IsMonitorCapableOfBasicAudio { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the connected display is capable of sYCC601 if such data is available;
        ///     otherwise null
        /// </summary>
        bool? IsMonitorCapableOfsYCC601 { get; }


        /// <summary>
        ///     Gets a boolean value indicating that the connected display is capable of underscan
        /// </summary>
        bool IsMonitorCapableOfUnderscan { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the connected display is capable of xvYCC601
        /// </summary>
        // ReSharper disable once IdentifierTypo
        bool IsMonitorCapableOfxvYCC601 { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the connected display is capable of xvYCC709
        /// </summary>
        // ReSharper disable once IdentifierTypo
        bool IsMonitorCapableOfxvYCC709 { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the connected display is capable of YCbCr422
        /// </summary>
        bool IsMonitorCapableOfYCbCr422 { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the connected display is capable of YCbCr444
        /// </summary>
        bool IsMonitorCapableOfYCbCr444 { get; }
    }

    /// <summary>
    ///     Contains information regarding HDR color data
    /// </summary>
    public interface IHDRColorData
    {
        /// <summary>
        ///     Gets the HDR color depth if available; otherwise null
        ///     For Dolby Vision only, should and will be ignored if HDR is on
        /// </summary>
        ColorDataDepth? ColorDepth { get; }

        /// <summary>
        ///     Gets the HDR color format if available; otherwise null
        /// </summary>
        ColorDataFormat? ColorFormat { get; }

        /// <summary>
        ///     Gets the HDR dynamic range if available; otherwise null
        /// </summary>
        ColorDataDynamicRange? DynamicRange { get; }

        /// <summary>
        ///     Gets the HDR mode
        /// </summary>
        ColorDataHDRMode HDRMode { get; }

        /// <summary>
        ///     Gets the color space coordinates
        /// </summary>
        MasteringDisplayColorData MasteringDisplayData { get; }
    }

    /// <summary>
    ///     Contains information regarding HDR capabilities
    /// </summary>
    public interface IHDRCapabilities
    {
        /// <summary>
        ///     Gets the display color space configurations
        /// </summary>
        // ReSharper disable once ConvertToAutoProperty
        DisplayColorData DisplayData { get; }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a UHDA HDR with ST2084 EOTF (CEA861.3) is supported.
        /// </summary>
        bool IsST2084EOTFSupported { get; }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a traditional HDR gamma (CEA861.3) is supported.
        /// </summary>
        bool IsTraditionalHDRGammaSupported { get; }

        /// <summary>
        ///     Gets a boolean value indicating if the Extended Dynamic Range on SDR displays is supported.
        /// </summary>
        bool IsEDRSupported { get; }

        /// <summary>
        ///     Gets a boolean value indicating if the default EDID HDR parameters is expanded;
        ///     otherwise false if this instance contains actual HDR parameters.
        /// </summary>
        bool IsDriverDefaultHDRParametersExpanded { get; }

        /// <summary>
        ///     Gets a boolean value indicating if the HDMI2.0a traditional SDR gamma is supported.
        /// </summary>
        bool IsTraditionalSDRGammaSupported { get; }

        /// <summary>
        ///     Gets a boolean value indicating if Dolby Vision is supported.
        /// </summary>
        bool IsDolbyVisionSupported { get; }

    }

    /// <summary>
    ///     Interface for all PathInfo structures
    /// </summary>
    public interface IPathInfo : IDisposable
    {
        /// <summary>
        ///     Identifies sourceId used by Windows CCD. This can be optionally set.
        /// </summary>
        uint SourceId { get; set; }

        /// <summary>
        ///     Contains information about the source mode
        /// </summary>
        SourceModeInfo SourceModeInfo { get; set; }

        /// <summary>
        ///     Contains information about path targets
        /// </summary>
        List<IPathTargetInfo> TargetsInfo { get; set; }
    }

    /// <summary>
    ///     Interface for all PathTargetInfo structures
    /// </summary>
    public interface IPathTargetInfo
    {
        /// <summary>
        ///     Contains extra information. NULL for Non-NVIDIA Display.
        /// </summary>
        PathAdvancedTargetInfo? Details { get; set; }

        /// <summary>
        ///     Display identification
        /// </summary>
        uint DisplayId { get; set; }
    }

    /// <summary>
    /// Contains information regarding the scan-out intensity data
    /// </summary>
    public interface IScanOutIntensity
    {

        /// <summary>
        ///     Gets the array of floating values building an intensity RGB texture
        /// </summary>
        float[] BlendingTexture { get; }

        /// <summary>
        ///     Gets the height of the input texture
        /// </summary>
        uint Height { get; }


        /// <summary>
        ///     Gets the width of the input texture
        /// </summary>
        uint Width { get; }
    }

    /// <summary>
    ///     Interface for all ChipsetInfo structures
    /// </summary>
    public interface IChipsetInfo
    {
        /// <summary>
        ///     Chipset device name
        /// </summary>
        string ChipsetName { get; }

        /// <summary>
        ///     Chipset device identification
        /// </summary>
        int DeviceId { get; }

        /// <summary>
        ///     Chipset information flags - obsolete
        /// </summary>
        ChipsetInfoFlag Flags { get; }

        /// <summary>
        ///     Chipset vendor identification
        /// </summary>
        int VendorId { get; }

        /// <summary>
        ///     Chipset vendor name
        /// </summary>
        string VendorName { get; }
    }

    /// <summary>
    ///     Interface for all DisplaySettings structures
    /// </summary>
    public interface IDisplaySettings
    {
        /// <summary>
        ///     Bits per pixel
        /// </summary>
        int BitsPerPixel { get; }

        /// <summary>
        ///     Display frequency
        /// </summary>
        int Frequency { get; }

        /// <summary>
        ///     Display frequency in x1k
        /// </summary>
        uint FrequencyInMillihertz { get; }

        /// <summary>
        ///     Per-display height
        /// </summary>
        int Height { get; }

        /// <summary>
        ///     Per-display width
        /// </summary>
        int Width { get; }
    }

    /// <summary>
    ///     Interface for all GridTopology structures
    /// </summary>
    public interface IGridTopology
    {
        /// <summary>
        ///     Enable SLI acceleration on the primary display while in single-wide mode (For Immersive Gaming only). Will not be
        ///     persisted. Value undefined on get.
        /// </summary>
        bool AcceleratePrimaryDisplay { get; }

        /// <summary>
        ///     When enabling and doing the mode-set, do we switch to the bezel-corrected resolution
        /// </summary>
        bool ApplyWithBezelCorrectedResolution { get; }

        /// <summary>
        ///     Enable as Base Mosaic (Panoramic) instead of Mosaic SLI (for NVS and Quadro-boards only)
        /// </summary>
        bool BaseMosaicPanoramic { get; }

        /// <summary>
        ///     Number of columns
        /// </summary>
        int Columns { get; }

        /// <summary>
        ///     Topology displays; Displays are done as [(row * columns) + column]
        /// </summary>
        List<IGridTopologyDisplay> Displays { get; }

        /// <summary>
        ///     Display settings
        /// </summary>
        DisplaySettingsV1 DisplaySettings { get; }

        /// <summary>
        ///     If necessary, reloading the driver is permitted (for Vista and above only). Will not be persisted. Value undefined
        ///     on get.
        /// </summary>
        bool DriverReloadAllowed { get; }

        /// <summary>
        ///     Enable as immersive gaming instead of Mosaic SLI (for Quadro-boards only)
        /// </summary>
        bool ImmersiveGaming { get; }

        /// <summary>
        ///     Number of rows
        /// </summary>
        int Rows { get; }
    }

    /// <summary>
    ///     Interface for all GridTopologyDisplay structures
    /// </summary>
    public interface IGridTopologyDisplay
    {
        /// <summary>
        ///     Gets the clone group identification; Reserved, must be 0
        /// </summary>
        uint CloneGroup { get; }

        /// <summary>
        ///     Gets the display identification
        /// </summary>
        uint DisplayId { get; }

        /// <summary>
        ///     Gets the horizontal overlap (+overlap, -gap)
        /// </summary>
        int OverlapX { get; }

        /// <summary>
        ///     Gets the vertical overlap (+overlap, -gap)
        /// </summary>
        int OverlapY { get; }


        /// <summary>
        ///     Gets the type of display pixel shift
        /// </summary>
        PixelShiftType PixelShiftType { get; }

        /// <summary>
        ///     Gets the rotation of display
        /// </summary>
        Rotate Rotation { get; }
    }

    /// <summary>
    ///     Interface for all SupportedTopologiesInfo structures
    /// </summary>
    public interface ISupportedTopologiesInfo
    {
        /// <summary>
        ///     List of per display settings possible
        /// </summary>
        List<IDisplaySettings> DisplaySettings { get; }

        /// <summary>
        ///     List of supported topologies with only brief details
        /// </summary>
        List<TopologyBrief> TopologyBriefs { get; }
    }

    /// <summary>
    ///     Interface for all ClockFrequencies structures
    /// </summary>
    public interface IClockFrequencies
    {
        /// <summary>
        ///     Gets all valid clocks
        /// </summary>
        IReadOnlyDictionary<PublicClockDomain, ClockDomainInfo> Clocks { get; }

        /// <summary>
        ///     Gets the type of clock frequencies provided with this object
        /// </summary>
        ClockType ClockType { get; }

        /// <summary>
        ///     Gets graphics engine clock
        /// </summary>
        ClockDomainInfo GraphicsClock { get; }

        /// <summary>
        ///     Gets memory decoding clock
        /// </summary>
        ClockDomainInfo MemoryClock { get; }

        /// <summary>
        ///     Gets processor clock
        /// </summary>
        ClockDomainInfo ProcessorClock { get; }

        /// <summary>
        ///     Gets video decoding clock
        /// </summary>
        ClockDomainInfo VideoDecodingClock { get; }
    }

    /// <summary>
    ///     Interface for all DisplayDriverMemoryInfo structures
    /// </summary>
    public interface IDisplayDriverMemoryInfo
    {
        /// <summary>
        ///     Size(in kb) of the available physical frame buffer for allocating video memory surfaces.
        /// </summary>
        uint AvailableDedicatedVideoMemoryInkB { get; }

        /// <summary>
        ///     Size(in kb) of the current available physical frame buffer for allocating video memory surfaces.
        /// </summary>
        uint CurrentAvailableDedicatedVideoMemoryInkB { get; }

        /// <summary>
        ///     Size(in kb) of the physical frame buffer.
        /// </summary>
        uint DedicatedVideoMemoryInkB { get; }

        /// <summary>
        ///     Size(in kb) of shared system memory that driver is allowed to commit for surfaces across all allocations.
        /// </summary>
        uint SharedSystemMemoryInkB { get; }

        /// <summary>
        ///     Size(in kb) of system memory the driver allocates at load time.
        /// </summary>
        uint SystemVideoMemoryInkB { get; }
    }

    /// <summary>
    ///     Interface for all DisplayIds structures
    /// </summary>
    public interface IDisplayIds
    {
        /// <summary>
        ///     Gets connection type. This is reserved for future use and clients should not rely on this information. Instead get
        ///     the GPU connector type from NvAPI_GPU_GetConnectorInfo/NvAPI_GPU_GetConnectorInfoEx
        /// </summary>
        MonitorConnectionType ConnectionType { get; }

        /// <summary>
        ///     Gets a unique identifier for each device
        /// </summary>
        uint DisplayId { get; }

        /// <summary>
        ///     Indicates if the display is being actively driven
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        ///     Indicates if the display is the representative display
        /// </summary>
        bool IsCluster { get; }

        /// <summary>
        ///     Indicates if the display is connected
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        ///     Indicates if the display is part of MST topology and it's a dynamic
        /// </summary>
        bool IsDynamic { get; }

        /// <summary>
        ///     Indicates if the display identification belongs to a multi stream enabled connector (root node). Note that when
        ///     multi stream is enabled and a single multi stream capable monitor is connected to it, the monitor will share the
        ///     display id with the RootNode.
        ///     When there is more than one monitor connected in a multi stream topology, then the root node will have a separate
        ///     displayId.
        /// </summary>
        bool IsMultiStreamRootNode { get; }

        /// <summary>
        ///     Indicates if the display is reported to the OS
        /// </summary>
        bool IsOSVisible { get; }

        /// <summary>
        ///     Indicates if the display is a physically connected display; Valid only when IsConnected is true
        /// </summary>
        bool IsPhysicallyConnected { get; }

        /// <summary>
        ///     Indicates if the display is wireless
        /// </summary>
        bool IsWFD { get; }
    }

    /// <summary>
    ///     Interface for all EDID structures
    /// </summary>
    public interface IEDID
    {
        /// <summary>
        ///     Gets whole or a part of the EDID data
        /// </summary>
        byte[] Data { get; }
    }

    /// <summary>
    ///     Contains an I2C packet transmitted or to be transmitted
    /// </summary>
    public interface II2CInfo
    {
        /// <summary>
        ///     Gets the payload data
        /// </summary>
        byte[] Data { get; }

        /// <summary>
        ///     Gets the device I2C slave address
        /// </summary>
        byte DeviceAddress { get; }

        /// <summary>
        ///     Gets a boolean value indicating that this instance contents information about a read operation
        /// </summary>
        bool IsReadOperation { get; }

        /// <summary>
        ///     Gets the target display output mask
        /// </summary>
        OutputId OutputMask { get; }

        /// <summary>
        ///     Gets the port id on which device is connected
        /// </summary>
        byte? PortId { get; }

        /// <summary>
        ///     Gets the target I2C register address
        /// </summary>
        byte[] RegisterAddress { get; }

        /// <summary>
        ///     Gets the target speed of the transaction in kHz
        /// </summary>
        I2CSpeed Speed { get; }

        /// <summary>
        ///     Gets a boolean value indicating that the DDC port should be used instead of the communication port
        /// </summary>
        bool UseDDCPort { get; }
    }

    /// <summary>
    ///     Holds information regarding a performance state
    /// </summary>
    public interface IPerformanceState
    {
        /// <summary>
        ///     Gets a boolean value indicating if this performance state is overclockable
        /// </summary>
        bool IsOverclockable { get; }

        /// <summary>
        ///     Gets a boolean value indicating if this performance state is currently overclocked
        /// </summary>
        bool IsOverclocked { get; }

        /// <summary>
        ///     Gets a boolean value indicating if this performance state is limited to use PCIE generation 1 or PCIE generation 2
        /// </summary>
        bool IsPCIELimited { get; }

        /// <summary>
        ///     Gets the performance state identification
        /// </summary>
        PerformanceStateId StateId { get; }
    }

    /// <summary>
    ///     Holds information regarding a performance state v2
    /// </summary>
    public interface IPerformanceState20
    {
        /// <summary>
        ///     Gets a boolean value indicating if this performance state is editable
        /// </summary>
        bool IsEditable { get; }

        /// <summary>
        ///     Gets the performance state identification
        /// </summary>
        PerformanceStateId StateId { get; }
    }

    /// <summary>
    ///     Holds information regarding the frequency range of a clock domain as well as the dependent voltage domain and the
    ///     range of the voltage
    /// </summary>
    public interface IPerformanceStates20ClockDependentFrequencyRange
    {
        /// <summary>
        ///     Gets the maximum clock frequency in kHz
        /// </summary>
        uint MaximumFrequencyInkHz { get; }

        /// <summary>
        ///     Gets the dependent voltage domain's maximum voltage in uV
        /// </summary>
        uint MaximumVoltageInMicroVolt { get; }

        /// <summary>
        ///     Gets the minimum clock frequency in kHz
        /// </summary>
        uint MinimumFrequencyInkHz { get; }

        /// <summary>
        ///     Gets the dependent voltage domain's minimum voltage in uV
        /// </summary>
        uint MinimumVoltageInMicroVolt { get; }

        /// <summary>
        ///     Gets the dependent voltage domain identification
        /// </summary>
        PerformanceVoltageDomain VoltageDomainId { get; }
    }

    /// <summary>
    ///     Holds information regarding the clock frequency of a fixed frequency clock domain
    /// </summary>
    public interface IPerformanceStates20ClockDependentSingleFrequency
    {
        /// <summary>
        ///     Gets the clock frequency of a clock domain in kHz
        /// </summary>
        uint FrequencyInkHz { get; }
    }

    /// <summary>
    ///     Holds information regarding a clock domain of a performance states
    /// </summary>
    public interface IPerformanceStates20ClockEntry
    {
        /// <summary>
        ///     Gets the type of clock frequency
        /// </summary>
        PerformanceStates20ClockType ClockType { get; }

        /// <summary>
        ///     Gets the domain identification
        /// </summary>
        PublicClockDomain DomainId { get; }

        /// <summary>
        ///     Gets the current base frequency delta value and the range for a valid delta value
        /// </summary>
        PerformanceStates20ParameterDelta FrequencyDeltaInkHz { get; }

        /// <summary>
        ///     Gets the fixed frequency of the clock
        /// </summary>
        IPerformanceStates20ClockDependentFrequencyRange FrequencyRange { get; }


        /// <summary>
        ///     Gets a boolean value indicating if this clock is editable
        /// </summary>
        bool IsEditable { get; }

        /// <summary>
        ///     Gets the range of clock frequency and related voltage information if present
        /// </summary>
        IPerformanceStates20ClockDependentSingleFrequency SingleFrequency { get; }
    }

    /// <summary>
    ///     Holds information regarding the valid power states and their clock and voltage settings as well as general
    ///     over-volting settings
    /// </summary>
    public interface IPerformanceStates20Info
    {
        /// <summary>
        ///     Gets a dictionary for valid power states and their clock frequencies
        /// </summary>
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStates20ClockEntry[]> Clocks { get; }

        /// <summary>
        ///     Gets the list of general over-volting settings
        /// </summary>
        IPerformanceStates20VoltageEntry[] GeneralVoltages { get; }

        /// <summary>
        ///     Gets a boolean value indicating if performance states are editable
        /// </summary>
        bool IsEditable { get; }

        /// <summary>
        ///     Gets an array of valid power states for the GPU
        /// </summary>
        IPerformanceState20[] PerformanceStates { get; }

        /// <summary>
        ///     Gets a dictionary for valid power states and their voltage settings
        /// </summary>
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStates20VoltageEntry[]> Voltages { get; }
    }

    /// <summary>
    ///     Holds information regarding the voltage of a voltage domain
    /// </summary>
    public interface IPerformanceStates20VoltageEntry
    {
        /// <summary>
        ///     Gets the voltage domain identification
        /// </summary>
        PerformanceVoltageDomain DomainId { get; }

        /// <summary>
        ///     Gets a boolean value indicating this voltage domain is editable
        /// </summary>
        bool IsEditable { get; }

        /// <summary>
        ///     Gets the base voltage delta and the range of valid values for the delta value
        /// </summary>
        PerformanceStates20ParameterDelta ValueDeltaInMicroVolt { get; }

        /// <summary>
        ///     Gets the current value of this voltage domain in uV
        /// </summary>
        uint ValueInMicroVolt { get; }
    }

    /// <summary>
    ///     Holds information regarding a clock domain of a performance state
    /// </summary>
    public interface IPerformanceStatesClock
    {
        /// <summary>
        ///     Gets the clock domain identification
        /// </summary>
        PublicClockDomain DomainId { get; }

        /// <summary>
        ///     Gets the clock frequency in kHz
        /// </summary>
        uint Frequency { get; }
    }

    /// <summary>
    ///     Holds information regarding performance states status of a GPU
    /// </summary>
    public interface IPerformanceStatesInfo
    {
        /// <summary>
        ///     Gets a boolean value indicating if the device is capable of dynamic performance state switching
        /// </summary>
        bool IsCapableOfDynamicPerformance { get; }

        /// <summary>
        ///     Gets a boolean value indicating if the dynamic performance state switching is enable
        /// </summary>
        bool IsDynamicPerformanceEnable { get; }

        /// <summary>
        ///     Gets a boolean value indicating if the performance monitoring is enable
        /// </summary>
        bool IsPerformanceMonitorEnable { get; }

        /// <summary>
        ///     Gets an array of valid and available performance states information
        /// </summary>
        IPerformanceState[] PerformanceStates { get; }

        /// <summary>
        ///     Gets a dictionary of valid and available performance states and their clock information as an array
        /// </summary>
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStatesClock[]> PerformanceStatesClocks { get; }

        /// <summary>
        ///     Gets a dictionary of valid and available performance states and their voltage information as an array
        /// </summary>
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStatesVoltage[]> PerformanceStatesVoltages { get; }
    }

    /// <summary>
    ///     Holds information regarding a voltage domain of a performance state
    /// </summary>
    public interface IPerformanceStatesVoltage
    {
        /// <summary>
        ///     Gets the voltage domain identification
        /// </summary>
        PerformanceVoltageDomain DomainId { get; }

        /// <summary>
        ///     Gets the voltage in mV
        /// </summary>
        uint Value { get; }
    }

    /// <summary>
    ///     Provides information about a single thermal sensor
    /// </summary>
    public interface IThermalSensor
    {
        /// <summary>
        ///     Internal, ADM1032, MAX6649...
        /// </summary>
        ThermalController Controller { get; }

        /// <summary>
        ///     Current temperature value of the thermal sensor in degree Celsius
        /// </summary>
        int CurrentTemperature { get; }

        /// <summary>
        ///     Maximum default temperature value of the thermal sensor in degree Celsius
        /// </summary>
        int DefaultMaximumTemperature { get; }

        /// <summary>
        ///     Minimum default temperature value of the thermal sensor in degree Celsius
        /// </summary>
        int DefaultMinimumTemperature { get; }

        /// <summary>
        ///     Thermal sensor targeted - GPU, memory, chipset, power supply, Visual Computing Device, etc
        /// </summary>
        ThermalSettingsTarget Target { get; }
    }

    /// <summary>
    ///     Holds a list of thermal sensors
    /// </summary>
    public interface IThermalSettings
    {
        /// <summary>
        ///     Gets a list of requested thermal sensor information
        /// </summary>
        IThermalSensor[] Sensors { get; }
    }

    /// <summary>
    ///     Holds information about a utilization domain
    /// </summary>
    public interface IUtilizationDomainInfo
    {
        /// <summary>
        ///     Gets a boolean value that indicates if this utilization domain is present on this GPU.
        /// </summary>
        bool IsPresent { get; }

        /// <summary>
        ///     Gets the percentage of time where the domain is considered busy in the last 1 second interval.
        /// </summary>
        uint Percentage { get; }
    }

    /// <summary>
    ///     Holds information about the GPU utilization domains
    /// </summary>
    public interface IUtilizationStatus
    {

        /// <summary>
        ///     Gets the Bus interface (BUS) utilization
        /// </summary>
        IUtilizationDomainInfo BusInterface { get; }
        /// <summary>
        ///     Gets all valid utilization domains and information
        /// </summary>
        Dictionary<UtilizationDomain, IUtilizationDomainInfo> Domains { get; }

        /// <summary>
        ///     Gets the frame buffer (FB) utilization
        /// </summary>
        IUtilizationDomainInfo FrameBuffer { get; }

        /// <summary>
        ///     Gets the graphic engine (GPU) utilization
        /// </summary>
        IUtilizationDomainInfo GPU { get; }


        /// <summary>
        ///     Gets the Video engine (VID) utilization
        /// </summary>
        IUtilizationDomainInfo VideoEngine { get; }
    }


    /// <summary>
    ///     Represents an application rule registered in a profile
    /// </summary>
    public interface IDRSApplication
    {
        /// <summary>
        ///     Gets the application name
        /// </summary>
        string ApplicationName { get; }

        /// <summary>
        ///     Gets the application friendly name
        /// </summary>
        string FriendlyName { get; }

        /// <summary>
        ///     Gets a boolean value indicating if this application is predefined as part of NVIDIA driver
        /// </summary>
        bool IsPredefined { get; }

        /// <summary>
        ///     Gets the application launcher name.
        /// </summary>
        string LauncherName { get; }
    }

}
