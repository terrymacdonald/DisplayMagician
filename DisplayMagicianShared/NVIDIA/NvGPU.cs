using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.NVIDIA
{
    /// <summary>
    ///     Holds the possible clock lock modes
    /// </summary>
    public enum ClockLockMode : uint
    {
        /// <summary>
        ///     No clock lock
        /// </summary>
        None = 0,

        /// <summary>
        ///     Manual clock lock
        /// </summary>
        Manual = 3
    }

    /// <summary>
    ///     Clock types to request
    /// </summary>
    public enum ClockType : uint
    {
        /// <summary>
        ///     Current clock frequencies
        /// </summary>
        CurrentClock = 0,

        /// <summary>
        ///     Base clock frequencies
        /// </summary>
        BaseClock = 1,

        /// <summary>
        ///     Boost clock frequencies
        /// </summary>
        BoostClock = 2
    }

    /// <summary>
    ///     Flags used for retrieving a list of display identifications
    /// </summary>
    [Flags]
    public enum ConnectedIdsFlag : uint
    {
        /// <summary>
        ///     No specific flag
        /// </summary>
        None = 0,

        /// <summary>
        ///     Get un-cached connected devices
        /// </summary>
        UnCached = 1,

        /// <summary>
        ///     Get devices such that those can be selected in an SLI configuration
        /// </summary>
        SLI = 2,

        /// <summary>
        ///     Get devices such that to reflect the Lid State
        /// </summary>
        LidState = 4,

        /// <summary>
        ///     Get devices that includes the fake connected monitors
        /// </summary>
        Fake = 8,

        /// <summary>
        ///     Excludes devices that are part of the multi stream topology
        /// </summary>
        ExcludeList = 16
    }

    /// <summary>
    ///     Possible display connectors
    /// </summary>
    public enum ConnectorType : uint
    {
        /// <summary>
        ///     VGA 15 Pin connector
        /// </summary>
        VGA15Pin = 0x00000000,

        /// <summary>
        ///     TV Composite
        /// </summary>
        // ReSharper disable once InconsistentNaming
        TV_Composite = 0x00000010,

        /// <summary>
        ///     TV SVideo
        /// </summary>
        // ReSharper disable once InconsistentNaming
        TV_SVideo = 0x00000011,

        /// <summary>
        ///     TV HDTV Component
        /// </summary>
        // ReSharper disable once InconsistentNaming
        TV_HDTVComponent = 0x00000013,

        /// <summary>
        ///     TV SCART
        /// </summary>
        // ReSharper disable once InconsistentNaming
        TV_SCART = 0x00000014,

        /// <summary>
        ///     TV Composite through SCART on EIAJ4120
        /// </summary>
        // ReSharper disable once InconsistentNaming
        TV_CompositeSCARTOnEIAJ4120 = 0x00000016,

        /// <summary>
        ///     TV HDTV EIAJ4120
        /// </summary>
        // ReSharper disable once InconsistentNaming
        TV_HDTV_EIAJ4120 = 0x00000017,

        /// <summary>
        ///     HDTV YPbPr through VESA Plug On Display
        /// </summary>
        // ReSharper disable once InconsistentNaming
        PC_POD_HDTV_YPbPr = 0x00000018,

        /// <summary>
        ///     SVideo through VESA Plug On Display
        /// </summary>
        // ReSharper disable once InconsistentNaming
        PC_POD_SVideo = 0x00000019,

        /// <summary>
        ///     Composite through VESA Plug On Display
        /// </summary>
        // ReSharper disable once InconsistentNaming
        PC_POD_Composite = 0x0000001A,

        /// <summary>
        ///     TV SVideo through DVI Integrated
        /// </summary>
        // ReSharper disable once InconsistentNaming
        DVI_I_TV_SVideo = 0x00000020,

        /// <summary>
        ///     TV Composite through DVI Integrated
        /// </summary>
        // ReSharper disable once InconsistentNaming
        DVI_I_TV_COMPOSITE = 0x00000021,

        /// <summary>
        ///     DVI Integrated
        /// </summary>
        // ReSharper disable once InconsistentNaming
        DVI_I = 0x00000030,

        /// <summary>
        ///     DVI Digital
        /// </summary>
        // ReSharper disable once InconsistentNaming
        DVI_D = 0x00000031,

        /// <summary>
        ///     Apple Display Connector
        /// </summary>
        ADC = 0x00000032,

        /// <summary>
        ///     DVI 1 through LFH
        /// </summary>
        // ReSharper disable once InconsistentNaming
        LFH_DVI_I1 = 0x00000038,

        /// <summary>
        ///     DVI 2 through LFH
        /// </summary>
        // ReSharper disable once InconsistentNaming
        LFH_DVI_I2 = 0x00000039,

        /// <summary>
        ///     SPWG pin-out connector
        /// </summary>
        SPWG = 0x00000040,

        /// <summary>
        ///     OEM connector
        /// </summary>
        OEM = 0x00000041,

        /// <summary>
        ///     External DisplayPort
        /// </summary>
        DisplayPortExternal = 0x00000046,

        /// <summary>
        ///     Internal DisplayPort
        /// </summary>
        DisplayPortInternal = 0x00000047,

        /// <summary>
        ///     External Mini DisplayPort
        /// </summary>
        DisplayPortMiniExternal = 0x00000048,

        /// <summary>
        ///     HDMI Analog
        /// </summary>
        // ReSharper disable once InconsistentNaming
        HDMI_Analog = 0x00000061,

        /// <summary>
        ///     Mini HDMI
        /// </summary>
        // ReSharper disable once InconsistentNaming
        HDMI_CMini = 0x00000063,

        /// <summary>
        ///     DisplayPort 1 through LFH
        /// </summary>
        LFHDisplayPort1 = 0x00000064,

        /// <summary>
        ///     DisplayPort 2 through LFH
        /// </summary>
        LFHDisplayPort2 = 0x00000065,

        /// <summary>
        ///     Virtual Wireless
        /// </summary>
        VirtualWFD = 0x00000070,

        /// <summary>
        ///     Unknown connector
        /// </summary>
        Unknown = 0xFFFFFFFF
    }

    /// <summary>
    ///     Holds possible cooler control modes
    /// </summary>
    public enum CoolerControlMode : uint
    {
        /// <summary>
        ///     No cooler control
        /// </summary>
        None = 0,

        /// <summary>
        ///     Toggle based cooler control mode
        /// </summary>
        Toggle,

        /// <summary>
        ///     Variable cooler control mode
        /// </summary>
        Variable
    }

    /// <summary>
    ///     Holds the list of possible cooler controllers
    /// </summary>
    public enum CoolerController : uint
    {
        /// <summary>
        ///     No cooler controller
        /// </summary>
        None = 0,

        /// <summary>
        ///     ADI cooler controller
        /// </summary>
        ADI,

        /// <summary>
        ///     Internal cooler controller
        /// </summary>
        Internal
    }

    /// <summary>
    ///     Holds possible cooler policies
    /// </summary>
    [Flags]
    public enum CoolerPolicy : uint
    {
        /// <summary>
        ///     No cooler policy
        /// </summary>
        None = 0,

        /// <summary>
        ///     Manual cooler control
        /// </summary>
        Manual = 0b1,

        /// <summary>
        ///     Performance optimized cooler policy
        /// </summary>
        Performance = 0b10,

        /// <summary>
        ///     Discrete temperature based cooler policy
        /// </summary>
        TemperatureDiscrete = 0b100,

        /// <summary>
        ///     Continues temperature based cooler policy
        /// </summary>
        TemperatureContinuous = 0b1000,

        /// <summary>
        ///     Silent cooler policy
        /// </summary>
        Silent = 0b10000
    }

    /// <summary>
    ///     Holds a list of possible cooler targets
    /// </summary>
    [Flags]
    public enum CoolerTarget : uint
    {
        /// <summary>
        ///     No cooler target
        /// </summary>
        None = 0,

        /// <summary>
        ///     Cooler targets GPU
        /// </summary>
        GPU = 0b1,

        /// <summary>
        ///     Cooler targets memory
        /// </summary>
        Memory = 0b10,

        /// <summary>
        ///     Cooler targets power supply
        /// </summary>
        PowerSupply = 0b100,

        /// <summary>
        ///     Cooler targets GPU, memory and power supply
        /// </summary>
        All = GPU | Memory | PowerSupply
    }

    /// <summary>
    ///     Holds a list of possible cooler types
    /// </summary>
    public enum CoolerType : uint
    {
        /// <summary>
        ///     No cooler type
        /// </summary>
        None,

        /// <summary>
        ///     Air cooling
        /// </summary>
        Fan,

        /// <summary>
        ///     Water cooling
        /// </summary>
        Water,

        /// <summary>
        ///     Liquid nitrogen cooling
        /// </summary>
        LiquidNitrogen
    }

    /// <summary>
    ///     Holds a list of possible ECC memory configurations
    /// </summary>
    public enum ECCConfiguration : uint
    {
        /// <summary>
        ///     ECC memory configurations are not supported
        /// </summary>
        NotSupported = 0,

        /// <summary>
        ///     Changes require a POST to take effect
        /// </summary>
        Deferred,

        /// <summary>
        ///     Changes can optionally be made to take effect immediately
        /// </summary>
        Immediate
    }

    /// <summary>
    ///     Holds possible fan cooler control modes
    /// </summary>
    [Flags]
    public enum FanCoolersControlMode : uint
    {
        /// <summary>
        ///     Automatic fan cooler control
        /// </summary>
        Auto = 0,

        /// <summary>
        ///     Manual fan cooler control
        /// </summary>
        Manual = 0b1,
    }

    /// <summary>
    ///     Associated GPU bus types
    /// </summary>
    public enum GPUBusType
    {
        /// <summary>
        ///     Bus type is undefined
        /// </summary>
        Undefined = 0,

        /// <summary>
        ///     PCI Bus
        /// </summary>
        PCI = 1,

        /// <summary>
        ///     AGP Bus
        /// </summary>
        AGP = 2,

        /// <summary>
        ///     PCIExpress Bus
        /// </summary>
        PCIExpress = 3,

        /// <summary>
        ///     FPCI Bus
        /// </summary>
        FPCI = 4,

        /// <summary>
        ///     AXI Bus
        /// </summary>
        AXI = 5
    }

    /// <summary>
    ///     Holds a list of known GPU foundries
    /// </summary>
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public enum GPUFoundry : uint
    {
        /// <summary>
        ///     Unknown foundry
        /// </summary>
        Unknown,

        /// <summary>
        ///     Taiwan Semiconductor Manufacturing Company Limited
        /// </summary>
        TSMC,

        /// <summary>
        ///     United Microelectronics
        /// </summary>
        UMC,

        /// <summary>
        ///     International Business Machines Corporation
        /// </summary>
        IBM,

        /// <summary>
        ///     Semiconductor Manufacturing International Corporation
        /// </summary>
        SMIC,

        /// <summary>
        ///     Chartered Semiconductor Manufacturing
        /// </summary>
        CSM,

        /// <summary>
        ///     Toshiba Corporation
        /// </summary>
        Toshiba
    }

    /// <summary>
    ///     Holds a list of known memory makers
    /// </summary>
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    // ReSharper disable CommentTypo
    public enum GPUMemoryMaker : uint
    {
        /// <summary>
        ///     Unknown memory maker
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Samsung Group
        /// </summary>
        Samsung,

        /// <summary>
        ///     Qimonda AG
        /// </summary>
        Qimonda,

        /// <summary>
        ///     Elpida Memory, Inc.
        /// </summary>
        Elpida,

        /// <summary>
        ///     Etron Technology, Inc.
        /// </summary>
        Etron,

        /// <summary>
        ///     Nanya Technology Corporation
        /// </summary>
        Nanya,

        /// <summary>
        ///     SK Hynix
        /// </summary>
        Hynix,

        /// <summary>
        ///     Mosel Vitelic Corporation
        /// </summary>
        Mosel,

        /// <summary>
        ///     Winbond Electronics Corporation
        /// </summary>
        Winbond,

        /// <summary>
        ///     Elite Semiconductor Memory Technology Inc.
        /// </summary>
        Elite,

        /// <summary>
        ///     Micron Technology, Inc.
        /// </summary>
        Micron
    }

    /// <summary>
    ///     Holds a list of known memory types
    /// </summary>
    public enum GPUMemoryType : uint
    {
        /// <summary>
        ///     Unknown memory type
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Synchronous dynamic random-access memory
        /// </summary>
        SDRAM,

        /// <summary>
        ///     Double Data Rate Synchronous Dynamic Random-Access Memory
        /// </summary>
        DDR1,

        /// <summary>
        ///     Double Data Rate 2 Synchronous Dynamic Random-Access Memory
        /// </summary>
        DDR2,

        /// <summary>
        ///     Graphics Double Data Rate 2 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR2,

        /// <summary>
        ///     Graphics Double Data Rate 3 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR3,

        /// <summary>
        ///     Graphics Double Data Rate 4 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR4,

        /// <summary>
        ///     Double Data Rate 3 Synchronous Dynamic Random-Access Memory
        /// </summary>
        DDR3,

        /// <summary>
        ///     Graphics Double Data Rate 5 Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR5,

        /// <summary>
        ///     Lowe Power Double Data Rate 2 Synchronous Dynamic Random-Access Memory
        /// </summary>
        LPDDR2,

        /// <summary>
        ///     Graphics Double Data Rate 5X Synchronous Dynamic Random-Access Memory
        /// </summary>
        GDDR5X
    }

    /// <summary>
    ///     Possible GPU types
    /// </summary>
    public enum GPUType
    {
        /// <summary>
        ///     Unknown GPU type
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Integrated GPU
        /// </summary>
        Integrated = 1,

        /// <summary>
        ///     Discrete GPU
        /// </summary>
        Discrete = 2
    }

    /// <summary>
    ///     Contains the flags used by the GPUApi.GetPerformanceStatesInfo() function
    /// </summary>
    [Flags]
    public enum GetPerformanceStatesInfoFlags
    {
        /// <summary>
        ///     Current performance states settings
        /// </summary>
        Current = 0,

        /// <summary>
        ///     Default performance states settings
        /// </summary>
        Default = 1,

        /// <summary>
        ///     Maximum range of performance states values
        /// </summary>
        Maximum = 2,

        /// <summary>
        ///     Minimum range of performance states values
        /// </summary>
        Minimum = 4
    }

    /// <summary>
    ///     Contains possible I2C bus speed values in kHz
    /// </summary>
    public enum I2CSpeed : uint
    {
        /// <summary>
        ///     Current / Default frequency setting
        /// </summary>
        Default,

        /// <summary>
        ///     3kHz
        /// </summary>
        I2C3KHz,

        /// <summary>
        ///     10kHz
        /// </summary>
        I2C10KHz,

        /// <summary>
        ///     33kHz
        /// </summary>
        I2C33KHz,

        /// <summary>
        ///     100kHz
        /// </summary>
        I2C100KHz,

        /// <summary>
        ///     200kHz
        /// </summary>
        I2C200KHz,

        /// <summary>
        ///     400kHz
        /// </summary>
        I2C400KHz
    }

    /// <summary>
    ///     Contains a list of valid illumination attributes
    /// </summary>
    public enum IlluminationAttribute : uint
    {
        /// <summary>
        ///     Logo brightness control
        /// </summary>
        LogoBrightness = 0,

        /// <summary>
        ///     SLI bridge brightness control
        /// </summary>
        SLIBrightness
    }

    /// <summary>
    ///     Contains a list of valid illumination zone device types
    /// </summary>
    public enum IlluminationDeviceType : uint
    {
        /// <summary>
        ///     Invalid device type
        /// </summary>
        Invalid = 0,

        /// <summary>
        ///     MCUV10 device
        /// </summary>
        MCUV10
    }

    /// <summary>
    ///     Contains a list of valid cycle types for the piecewise linear control mode
    /// </summary>
    public enum IlluminationPiecewiseLinearCycleType : uint
    {
        /// <summary>
        ///     Half half cycle mode
        /// </summary>
        HalfHalf = 0,

        /// <summary>
        ///     Full half cycle mode
        /// </summary>
        FullHalf,

        /// <summary>
        ///     Full repeat cycle mode
        /// </summary>
        FullRepeat,

        /// <summary>
        ///     Invalid cycle mode
        /// </summary>
        Invalid = 0xFF
    }

    /// <summary>
    ///     Contains a list of available illumination zone control modes
    /// </summary>
    [Flags]
    public enum IlluminationZoneControlMode : uint
    {
        /// <summary>
        ///     manual RGB control
        /// </summary>
        ManualRGB = 0,

        /// <summary>
        ///     Piecewise linear RGB control
        /// </summary>
        PiecewiseLinearRGB,

        /// <summary>
        ///     Invalid control mode
        /// </summary>
        Invalid = 0xFF
    }

    /// <summary>
    ///     Contains a list of valid zone control value types to set or to retrieve
    /// </summary>
    public enum IlluminationZoneControlValuesType
    {
        /// <summary>
        ///     Currently active values
        /// </summary>
        CurrentlyActive = 0,

        /// <summary>
        ///     Default values
        /// </summary>
        Default = 1
    }

    /// <summary>
    ///     Contains a list of possible illumination zone locations
    /// </summary>
    public enum IlluminationZoneLocation : uint
    {
        /// <summary>
        ///     Located on the top of GPU
        /// </summary>
        GPUTop = 0x00,

        /// <summary>
        ///     Located on the top of SLI bridge
        /// </summary>
        SLITop = 0x20,

        /// <summary>
        ///     Invalid zone location
        /// </summary>
        Invalid = 0xFFFFFFFF
    }

    /// <summary>
    ///     Contains a list of valid illumination zone types
    /// </summary>
    public enum IlluminationZoneType : uint
    {
        /// <summary>
        ///     Invalid zone type
        /// </summary>
        Invalid = 0,

        /// <summary>
        ///     RGB zone
        /// </summary>
        RGB,

        /// <summary>
        ///     Fixed color zone
        /// </summary>
        FixedColor
    }

    /// <summary>
    ///     Monitor connection types. This is reserved for future use and clients should not rely on this information.
    /// </summary>
    public enum MonitorConnectionType
    {
        /// <summary>
        ///     Monitor not yet initialized
        /// </summary>
        Uninitialized = 0,

        /// <summary>
        ///     Connected through a VGA compatible connector
        /// </summary>
        VGA,

        /// <summary>
        ///     Connected through a Component compatible connector
        /// </summary>
        Component,

        /// <summary>
        ///     Connected through a SVideo compatible connector
        /// </summary>
        SVideo,

        /// <summary>
        ///     Connected through a HDMI compatible connector
        /// </summary>
        HDMI,

        /// <summary>
        ///     Connected through a LVDS compatible connector
        /// </summary>
        DVI,

        /// <summary>
        ///     Connected through a DisplayPort compatible connector
        /// </summary>
        LVDS,

        /// <summary>
        ///     Connected through a DisplayPort compatible connector
        /// </summary>
        DisplayPort,

        /// <summary>
        ///     Connected through a Composite compatible connector
        /// </summary>
        Composite,

        /// <summary>
        ///     Connection type unknown
        /// </summary>
        Unknown = -1
    }

    /// <summary>
    ///     GPU output IDs are identifiers for the GPU outputs that drive display devices. The GPU output might or might not be
    ///     connected to a display, or be active. Each output is identified by a bit setting within a 32-bit unsigned integer.
    ///     A GPU output mask consists of a 32-bit integer with several bits set, identifying more than one output from the
    ///     same physical GPU.
    /// </summary>
    [Flags]
    public enum OutputId : uint
    {
        /// <summary>
        ///     Invalid output if
        /// </summary>
        Invalid = 0,

        /// <summary>
        ///     Represents Output 1
        /// </summary>
        Output1 = 1U,

        /// <summary>
        ///     Represents Output 2
        /// </summary>
        Output2 = 1u << 1,

        /// <summary>
        ///     Represents Output 3
        /// </summary>
        Output3 = 1u << 2,

        /// <summary>
        ///     Represents Output 4
        /// </summary>
        Output4 = 1u << 3,

        /// <summary>
        ///     Represents Output 5
        /// </summary>
        Output5 = 1u << 4,

        /// <summary>
        ///     Represents Output 6
        /// </summary>
        Output6 = 1u << 5,

        /// <summary>
        ///     Represents Output 7
        /// </summary>
        Output7 = 1u << 6,

        /// <summary>
        ///     Represents Output 8
        /// </summary>
        Output8 = 1u << 7,

        /// <summary>
        ///     Represents Output 9
        /// </summary>
        Output9 = 1u << 8,

        /// <summary>
        ///     Represents Output 10
        /// </summary>
        Output10 = 1u << 9,

        /// <summary>
        ///     Represents Output 11
        /// </summary>
        Output11 = 1u << 10,

        /// <summary>
        ///     Represents Output 12
        /// </summary>
        Output12 = 1u << 11,

        /// <summary>
        ///     Represents Output 13
        /// </summary>
        Output13 = 1u << 12,

        /// <summary>
        ///     Represents Output 14
        /// </summary>
        Output14 = 1u << 13,

        /// <summary>
        ///     Represents Output 15
        /// </summary>
        Output15 = 1u << 14,

        /// <summary>
        ///     Represents Output 16
        /// </summary>
        Output16 = 1u << 15,

        /// <summary>
        ///     Represents Output 17
        /// </summary>
        Output17 = 1u << 16,

        /// <summary>
        ///     Represents Output 18
        /// </summary>
        Output18 = 1u << 17,

        /// <summary>
        ///     Represents Output 19
        /// </summary>
        Output19 = 1u << 18,

        /// <summary>
        ///     Represents Output 20
        /// </summary>
        Output20 = 1u << 19,

        /// <summary>
        ///     Represents Output 21
        /// </summary>
        Output21 = 1u << 20,

        /// <summary>
        ///     Represents Output 22
        /// </summary>
        Output22 = 1u << 21,

        /// <summary>
        ///     Represents Output 23
        /// </summary>
        Output23 = 1u << 22,

        /// <summary>
        ///     Represents Output 24
        /// </summary>
        Output24 = 1u << 23,

        /// <summary>
        ///     Represents Output 25
        /// </summary>
        Output25 = 1u << 24,

        /// <summary>
        ///     Represents Output 26
        /// </summary>
        Output26 = 1u << 25,

        /// <summary>
        ///     Represents Output 27
        /// </summary>
        Output27 = 1u << 26,

        /// <summary>
        ///     Represents Output 28
        /// </summary>
        Output28 = 1u << 27,

        /// <summary>
        ///     Represents Output 29
        /// </summary>
        Output29 = 1u << 28,

        /// <summary>
        ///     Represents Output 30
        /// </summary>
        Output30 = 1u << 29,

        /// <summary>
        ///     Represents Output 31
        /// </summary>
        Output31 = 1u << 30,

        /// <summary>
        ///     Represents Output 32
        /// </summary>
        Output32 = 1u << 31
    }

    /// <summary>
    ///     Connected output device types
    /// </summary>
    public enum OutputType : uint
    {
        /// <summary>
        ///     Unknown display device
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     CRT display device
        /// </summary>
        CRT = 1,

        /// <summary>
        ///     Digital Flat Panel display device
        /// </summary>
        DFP = 2,

        /// <summary>
        ///     TV display device
        /// </summary>
        TV = 3
    }

    /// <summary>
    ///     Holds a list of known PCI-e generations and versions
    /// </summary>
    public enum PCIeGeneration : uint
    {
        /// <summary>
        ///     PCI-e 1.0
        /// </summary>
        PCIe1 = 0,

        /// <summary>
        ///     PCI-e 1.1
        /// </summary>
        PCIe1Minor1,

        /// <summary>
        ///     PCI-e 2.0
        /// </summary>
        PCIe2,

        /// <summary>
        ///     PCI-e 3.0
        /// </summary>
        PCIe3
    }

    /// <summary>
    ///     Holds a list possible reasons for performance decrease
    /// </summary>
    public enum PerformanceDecreaseReason : uint
    {
        /// <summary>
        ///     No performance decrease
        /// </summary>
        None = 0,

        /// <summary>
        ///     Thermal protection performance decrease
        /// </summary>
        ThermalProtection = 0x00000001,

        /// <summary>
        ///     Power control performance decrease
        /// </summary>
        PowerControl = 0x00000002,

        /// <summary>
        ///     AC-BATT event performance decrease
        /// </summary>
        // ReSharper disable once InconsistentNaming
        AC_BATT = 0x00000004,

        /// <summary>
        ///     API triggered performance decrease
        /// </summary>
        ApiTriggered = 0x00000008,

        /// <summary>
        ///     Insufficient performance decrease (Power Connector Missing)
        /// </summary>
        InsufficientPower = 0x00000010,

        /// <summary>
        ///     Unknown
        /// </summary>
        Unknown = 0x80000000
    }

    /// <summary>
    ///     Holds a list of known performance limitations
    /// </summary>
    [Flags]
    public enum PerformanceLimit : uint
    {
        /// <summary>
        ///     No performance limitation
        /// </summary>
        None = 0,

        /// <summary>
        ///     Limited by power usage
        /// </summary>
        PowerLimit = 0b1,

        /// <summary>
        ///     Limited by temperature
        /// </summary>
        TemperatureLimit = 0b10,

        /// <summary>
        ///     Limited by voltage
        /// </summary>
        VoltageLimit = 0b100,

        /// <summary>
        ///     Unknown limitation
        /// </summary>
        Unknown8 = 0b1000,

        /// <summary>
        ///     Limited due to no load
        /// </summary>
        NoLoadLimit = 0b10000
    }

    /// <summary>
    ///     Contains the list of valid performance state identifications
    /// </summary>
    public enum PerformanceStateId : uint
    {
        /// <summary>
        ///     Performance state 0 (Maximum 3D Quality)
        /// </summary>
        P0_3DPerformance = 0,

        /// <summary>
        ///     Performance state 1 (Maximum 3D Quality)
        /// </summary>
        P1_3DPerformance,

        /// <summary>
        ///     Performance state 2 (Balanced Performance)
        /// </summary>
        // ReSharper disable once InconsistentNaming
        P2_Balanced,

        /// <summary>
        ///     Performance state 3 (Balanced Performance)
        /// </summary>
        // ReSharper disable once InconsistentNaming
        P3_Balanced,

        /// <summary>
        ///     Performance state 4
        /// </summary>
        P4,

        /// <summary>
        ///     Performance state 5
        /// </summary>
        P5,

        /// <summary>
        ///     Performance state 6
        /// </summary>
        P6,

        /// <summary>
        ///     Performance state 7
        /// </summary>
        P7,

        /// <summary>
        ///     Performance state 8 (HD Video Playback)
        /// </summary>
        // ReSharper disable once InconsistentNaming
        P8_HDVideoPlayback,

        /// <summary>
        ///     Performance state 9
        /// </summary>
        P9,

        /// <summary>
        ///     Performance state 10 (DVD Video Playback)
        /// </summary>
        // ReSharper disable once InconsistentNaming
        P10_DVDPlayback,

        /// <summary>
        ///     Performance state 11
        /// </summary>
        P11,

        /// <summary>
        ///     Performance state 12 (Idle - PowerSaving mode)
        /// </summary>
        // ReSharper disable once InconsistentNaming
        P12_Idle,

        /// <summary>
        ///     Performance state 13
        /// </summary>
        P13,

        /// <summary>
        ///     Performance state 14
        /// </summary>
        P14,

        /// <summary>
        ///     Performance state 15
        /// </summary>
        P15,

        /// <summary>
        ///     Undefined performance state
        /// </summary>
        Undefined = PerformanceStatesInfoV1.MaxPerformanceStates,

        /// <summary>
        ///     All performance states
        /// </summary>
        All
    }

    /// <summary>
    ///     Contains valid clock frequency types
    /// </summary>
    public enum PerformanceStates20ClockType
    {
        /// <summary>
        ///     Single frequency clock
        /// </summary>
        Single = 0,

        /// <summary>
        ///     Variable frequency clock
        /// </summary>
        Range
    }

    /// <summary>
    ///     Contains the list of possible voltage domains
    /// </summary>
    public enum PerformanceVoltageDomain : uint
    {
        /// <summary>
        ///     GPU Core
        /// </summary>
        Core = 0,

        /// <summary>
        ///     Undefined voltage domain
        /// </summary>
        Undefined = PerformanceStatesInfoV2.MaxPerformanceStateVoltages
    }

    /// <summary>
    ///     Holds a list of known power topology domain
    /// </summary>
    public enum PowerTopologyDomain : uint
    {
        /// <summary>
        ///     The GPU
        /// </summary>
        GPU = 0,

        /// <summary>
        ///     The GPU board
        /// </summary>
        Board
    }

    /// <summary>
    ///     Contains the list of clocks available to public
    /// </summary>
    public enum PublicClockDomain
    {
        /// <summary>
        ///     Undefined
        /// </summary>
        Undefined = ClockFrequenciesV1.MaxClocksPerGPU,

        /// <summary>
        ///     3D graphics clock
        /// </summary>
        Graphics = 0,

        /// <summary>
        ///     Memory clock
        /// </summary>
        Memory = 4,

        /// <summary>
        ///     Processor clock
        /// </summary>
        Processor = 7,

        /// <summary>
        ///     Video decoding clock
        /// </summary>
        Video = 8
    }

    /// <summary>
    ///     GPU systems
    /// </summary>
    public enum SystemType
    {
        /// <summary>
        ///     Unknown type
        /// </summary>
        Unknown = 0,

        /// <summary>
        ///     Laptop GPU
        /// </summary>
        Laptop = 1,

        /// <summary>
        ///     Desktop GPU
        /// </summary>
        Desktop = 2
    }

    /// <summary>
    ///     List of possible thermal sensor controllers
    /// </summary>
    public enum ThermalController
    {
        /// <summary>
        ///     No Thermal Controller
        /// </summary>
        None = 0,

        /// <summary>
        ///     GPU acting as thermal controller
        /// </summary>
        GPU,

        /// <summary>
        ///     ADM1032 Thermal Controller
        /// </summary>
        ADM1032,

        /// <summary>
        ///     MAX6649 Thermal Controller
        /// </summary>
        MAX6649,

        /// <summary>
        ///     MAX1617 Thermal Controller
        /// </summary>
        MAX1617,

        /// <summary>
        ///     LM99 Thermal Controller
        /// </summary>
        LM99,

        /// <summary>
        ///     LM89 Thermal Controller
        /// </summary>
        LM89,

        /// <summary>
        ///     LM64 Thermal Controller
        /// </summary>
        LM64,

        /// <summary>
        ///     ADT7473 Thermal Controller
        /// </summary>
        ADT7473,

        /// <summary>
        ///     SBMAX6649 Thermal Controller
        /// </summary>
        SBMAX6649,

        /// <summary>
        ///     VideoBios acting as thermal controller
        /// </summary>
        VideoBiosEvent,

        /// <summary>
        ///     Operating System acting as thermal controller
        /// </summary>
        OperatingSystem,

        /// <summary>
        ///     Unknown Thermal Controller
        /// </summary>
        Unknown = -1
    }

    /// <summary>
    ///     List of possible thermal targets
    /// </summary>
    [Flags]
    public enum ThermalSettingsTarget
    {
        /// <summary>
        ///     None
        /// </summary>
        None = 0,

        /// <summary>
        ///     GPU core temperature
        /// </summary>
        GPU = 1,

        /// <summary>
        ///     GPU memory temperature
        /// </summary>
        Memory = 2,

        /// <summary>
        ///     GPU power supply temperature
        /// </summary>
        PowerSupply = 4,

        /// <summary>
        ///     GPU board ambient temperature
        /// </summary>
        Board = 8,

        /// <summary>
        ///     Visual Computing Device Board temperature requires NvVisualComputingDeviceHandle
        /// </summary>
        VisualComputingBoard = 9,

        /// <summary>
        ///     Visual Computing Device Inlet temperature requires NvVisualComputingDeviceHandle
        /// </summary>
        VisualComputingInlet = 10,

        /// <summary>
        ///     Visual Computing Device Outlet temperature requires NvVisualComputingDeviceHandle
        /// </summary>
        VisualComputingOutlet = 11,

        /// <summary>
        ///     Used for retrieving all thermal settings
        /// </summary>
        All = 15,

        /// <summary>
        ///     Unknown thermal target
        /// </summary>
        Unknown = -1
    }

    /// <summary>
    ///     Valid utilization domain
    /// </summary>
    public enum UtilizationDomain
    {
        /// <summary>
        ///     GPU utilization domain
        /// </summary>
        GPU,

        /// <summary>
        ///     Frame buffer utilization domain
        /// </summary>
        FrameBuffer,

        /// <summary>
        ///     Video engine utilization domain
        /// </summary>
        VideoEngine,

        /// <summary>
        ///     Bus interface utilization domain
        /// </summary>
        BusInterface
    }

    public enum WorkstationFeatureType : UInt32
    {
        NvidiaRTXVRReady = 1,  //!< NVIDIA RTX VR Ready
        QyadroVRReady = NvidiaRTXVRReady,  //!< DEPRECATED name - do not use
        Proviz = 2,
    }



    /// <summary>
    ///     Holds the board information (a unique GPU Board Serial Number) stored in the InfoROM
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct BoardInfo : IInitializable, IEquatable<BoardInfo>
    {
        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        internal byte[] _SerialNumber;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }
        
        /// <summary>
         ///     Board Serial Number
         /// </summary>
        public byte[] SerialNumber
        {
            get => _SerialNumber;
            set => _SerialNumber = value;    
        }

        /// <inheritdoc />
        public bool Equals(BoardInfo other)
        {
            return _SerialNumber.SequenceEqual(other._SerialNumber);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is BoardInfo info && Equals(info);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return _SerialNumber?.GetHashCode() ?? 0;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return SerialNumber == null ? "Unknown" : "Serial " + BitConverter.ToString(SerialNumber);
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(BoardInfo left, BoardInfo right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(BoardInfo left, BoardInfo right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>
    ///     Holds information about the clock frequency of an specific clock domain
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct ClockDomainInfo
    {
        internal uint _IsPresent;
        internal uint _Frequency;

        /// <summary>
        ///     Gets a boolean value that indicates if this clock domain is present on this GPU and with the requested clock type.
        /// </summary>
        public bool IsPresent
        {
            get => _IsPresent.GetBit(0);
            set => _IsPresent.SetBit(0,value);
        }

        /// <summary>
        ///     Gets the clock frequency in kHz
        /// </summary>
        public uint Frequency
        {
            get => _Frequency;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return IsPresent ? $"{_Frequency:N0} kHz" : "N/A";
        }
    }

    /// <summary>
    ///     Holds clock frequencies currently associated with a physical GPU
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ClockFrequenciesV1 : IInitializable, IClockFrequencies
    {
        internal const int MaxClocksPerGPU = 32;

        internal StructureVersion _Version;
        internal uint _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxClocksPerGPU)]
        internal ClockDomainInfo[] _Clocks;

        /// <inheritdoc />
        public IReadOnlyDictionary<PublicClockDomain, ClockDomainInfo> Clocks
        {
            get => _Clocks
                .Select((value, index) => new { index, value })
                .Where(arg => Enum.IsDefined(typeof(PublicClockDomain), arg.index))
                .ToDictionary(arg => (PublicClockDomain)arg.index, arg => arg.value);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public ClockType ClockType
        {
            get => ClockType.CurrentClock;
        }

        /// <inheritdoc />
        public ClockDomainInfo GraphicsClock
        {
            get => _Clocks[(int)PublicClockDomain.Graphics];
        }

        /// <inheritdoc />
        public ClockDomainInfo MemoryClock
        {
            get => _Clocks[(int)PublicClockDomain.Memory];
        }

        /// <inheritdoc />
        public ClockDomainInfo VideoDecodingClock
        {
            get => _Clocks[(int)PublicClockDomain.Video];
        }

        /// <inheritdoc />
        public ClockDomainInfo ProcessorClock
        {
            get => _Clocks[(int)PublicClockDomain.Processor];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"[{ClockType}] 3D Graphics = {GraphicsClock} - Memory = {MemoryClock} - Video Decoding = {VideoDecodingClock} - Processor = {ProcessorClock}";
        }
    }

    /// <summary>
    ///     Holds clock frequencies associated with a physical GPU and an specified clock type
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct ClockFrequenciesV2 : IInitializable, IClockFrequencies
    {
        internal const int MaxClocksPerGpu = 32;

        internal StructureVersion _Version;
        internal uint _ClockTypeAndReserve;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxClocksPerGpu)]
        internal ClockDomainInfo[] _Clocks;

        /// <summary>
        ///     Creates a new ClockFrequenciesV2
        /// </summary>
        /// <param name="clockType">The type of the clock frequency being requested</param>
        public ClockFrequenciesV2(ClockType clockType = ClockType.CurrentClock)
        {
            this = typeof(ClockFrequenciesV2).Instantiate<ClockFrequenciesV2>();
            _ClockTypeAndReserve = 0u.SetBits(0, 2, (uint)clockType);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<PublicClockDomain, ClockDomainInfo> Clocks
        {
            get => _Clocks
                .Select((value, index) => new { index, value })
                .Where(arg => Enum.IsDefined(typeof(PublicClockDomain), arg.index))
                .ToDictionary(arg => (PublicClockDomain)arg.index, arg => arg.value);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public ClockType ClockType
        {
            get => (ClockType)_ClockTypeAndReserve.GetBits(0, 2);
        }

        /// <inheritdoc />
        public ClockDomainInfo GraphicsClock
        {
            get => _Clocks[(int)PublicClockDomain.Graphics];
        }

        /// <inheritdoc />
        public ClockDomainInfo MemoryClock
        {
            get => _Clocks[(int)PublicClockDomain.Memory];
        }

        /// <inheritdoc />
        public ClockDomainInfo VideoDecodingClock
        {
            get => _Clocks[(int)PublicClockDomain.Video];
        }

        /// <inheritdoc />
        public ClockDomainInfo ProcessorClock
        {
            get => _Clocks[(int)PublicClockDomain.Processor];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"[{ClockType}] 3D Graphics = {GraphicsClock} - Memory = {MemoryClock} - Video Decoding = {VideoDecodingClock} - Processor = {ProcessorClock}";
        }
    }


    /// <summary>
    ///     Holds clock frequencies associated with a physical GPU and an specified clock type
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct ClockFrequenciesV3 : IInitializable, IClockFrequencies
    {
        internal const int MaxClocksPerGpu = 32;

        internal StructureVersion _Version;
        internal uint _ClockTypeAndReserve;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxClocksPerGpu)]
        internal ClockDomainInfo[] _Clocks;

        /// <summary>
        ///     Creates a new ClockFrequenciesV3
        /// </summary>
        /// <param name="clockType">The type of the clock frequency being requested</param>
        public ClockFrequenciesV3(ClockType clockType = ClockType.CurrentClock)
        {
            this = typeof(ClockFrequenciesV3).Instantiate<ClockFrequenciesV3>();
            _ClockTypeAndReserve = 0u.SetBits(0, 2, (uint)clockType);
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<PublicClockDomain, ClockDomainInfo> Clocks
        {
            get => _Clocks
                .Select((value, index) => new { index, value })
                .Where(arg => Enum.IsDefined(typeof(PublicClockDomain), arg.index))
                .ToDictionary(arg => (PublicClockDomain)arg.index, arg => arg.value);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }
        
        /// <summary>
         ///     Gets the type of clock frequencies provided with this object
         /// </summary>
        public ClockType ClockType
        {
            get => (ClockType)_ClockTypeAndReserve.GetBits(0, 2);
        }

        /// <inheritdoc />
        public ClockDomainInfo GraphicsClock
        {
            get => _Clocks[(int)PublicClockDomain.Graphics];
        }

        /// <inheritdoc />
        public ClockDomainInfo MemoryClock
        {
            get => _Clocks[(int)PublicClockDomain.Memory];
        }

        /// <inheritdoc />
        public ClockDomainInfo VideoDecodingClock
        {
            get => _Clocks[(int)PublicClockDomain.Video];
        }

        /// <inheritdoc />
        public ClockDomainInfo ProcessorClock
        {
            get => _Clocks[(int)PublicClockDomain.Processor];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"[{ClockType}] 3D Graphics = {GraphicsClock} - Memory = {MemoryClock} - Video Decoding = {VideoDecodingClock} - Processor = {ProcessorClock}";
        }
    }

    /// <summary>
    ///     Holds information about the system's display driver memory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct DisplayDriverMemoryInfoV1 : IInitializable, IDisplayDriverMemoryInfo
    {
        internal StructureVersion _Version;
        internal uint _DedicatedVideoMemory;
        internal uint _AvailableDedicatedVideoMemory;
        internal uint _SystemVideoMemory;
        internal uint _SharedSystemMemory;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint DedicatedVideoMemoryInkB
        {
            get => _DedicatedVideoMemory;
        }

        /// <inheritdoc />
        public uint AvailableDedicatedVideoMemoryInkB
        {
            get => _AvailableDedicatedVideoMemory;
        }

        /// <inheritdoc />
        public uint SystemVideoMemoryInkB
        {
            get => _SystemVideoMemory;
        }

        /// <inheritdoc />
        public uint SharedSystemMemoryInkB
        {
            get => _SharedSystemMemory;
        }

        /// <inheritdoc />
        public uint CurrentAvailableDedicatedVideoMemoryInkB
        {
            get => _AvailableDedicatedVideoMemory;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{AvailableDedicatedVideoMemoryInkB / 1024} MB / {DedicatedVideoMemoryInkB / 1024} MB";
        }
    }

    /// <summary>
    ///     Holds information about the system's display driver memory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct DisplayDriverMemoryInfoV2 : IInitializable, IDisplayDriverMemoryInfo
    {
        internal StructureVersion _Version;
        internal uint _DedicatedVideoMemory;
        internal uint _AvailableDedicatedVideoMemory;
        internal uint _SystemVideoMemory;
        internal uint _SharedSystemMemory;
        internal uint _CurrentAvailableDedicatedVideoMemory;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint DedicatedVideoMemoryInkB
        {
            get => _DedicatedVideoMemory;
        }

        /// <inheritdoc />
        public uint AvailableDedicatedVideoMemoryInkB
        {
            get => _AvailableDedicatedVideoMemory;
        }

        /// <inheritdoc />
        public uint SystemVideoMemoryInkB
        {
            get => _SystemVideoMemory;
        }

        /// <inheritdoc />
        public uint SharedSystemMemoryInkB
        {
            get => _SharedSystemMemory;
        }

        /// <inheritdoc />
        public uint CurrentAvailableDedicatedVideoMemoryInkB
        {
            get => _CurrentAvailableDedicatedVideoMemory;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{AvailableDedicatedVideoMemoryInkB / 1024} MB ({CurrentAvailableDedicatedVideoMemoryInkB / 1024} MB) / {DedicatedVideoMemoryInkB / 1024} MB";
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct NvGUID : IEquatable<NvGUID>, ICloneable
    {
        public UInt32 data1;
        public UInt16 data2;
        public UInt16 data3;
        //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
        public UInt32 data4;

        /// <summary>
        ///     Creates a new instance of <see cref="GetIlluminationParameterV1" />.
        /// </summary>
        /// <param name="gpuHandle">The physical gpu handle.</param>
        /// <param name="attribute">The attribute.</param>
        internal NvGUID(UInt32 data1, UInt16 data2, UInt16 data3, UInt32 data4)
        {
            this.data1 = data1;
            this.data2 = data2;
            this.data3 = data3; 
            this.data4 = data4;
        }

        public override bool Equals(object obj) => obj is NvGUID other && this.Equals(other);

        public bool Equals(NvGUID other)
        => data1 == other.data1 &&
           data2 == other.data2 &&
           data3 == other.data3 &&
           data4 == other.data4;

        public override Int32 GetHashCode()
        {
            return (data1, data2, data3, data4).GetHashCode();
        }
        public static bool operator ==(NvGUID lhs, NvGUID rhs) => lhs.Equals(rhs);

        public static bool operator !=(NvGUID lhs, NvGUID rhs) => !(lhs == rhs);
        public object Clone()
        {
            NvGUID other = (NvGUID)MemberwiseClone();
            return other;
        }
    }


    /*[StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct LogicalGPUData : IEquatable<LogicalGPUData>, ICloneable // Note: Version 1 of NV_BOARD_INFO_V1 structure
    {
        public UInt32 Version;                   //!< structure version
        public IntPtr OSAdapterId;
        //public NvGUID OSAdapterId;
        public UInt32 PhysicalGPUCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)PhysicalGPUHandle.MaxPhysicalGPUs)]
        public PhysicalGPUHandle[] PhysicalGPUHandles;
        public UInt32 Reserved;

        public override bool Equals(object obj) => obj is LogicalGPUData other && this.Equals(other);

        public bool Equals(LogicalGPUData other)
        => Version == other.Version &&
           PhysicalGPUCount == other.PhysicalGPUCount &&
           PhysicalGPUHandles.SequenceEqual(other.PhysicalGPUHandles);

        public override Int32 GetHashCode()
        {
            return (Version, PhysicalGPUCount, PhysicalGPUHandles).GetHashCode();
        }
        public static bool operator ==(LogicalGPUData lhs, LogicalGPUData rhs) => lhs.Equals(rhs);

        public static bool operator !=(LogicalGPUData lhs, LogicalGPUData rhs) => !(lhs == rhs);
        public object Clone()
        {
            LogicalGPUData other = (LogicalGPUData)MemberwiseClone();
            return other;
        }
    }*/


    /// <summary>
    ///     Holds information about the system's display driver memory.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct DisplayDriverMemoryInfoV3 : IInitializable, IDisplayDriverMemoryInfo
    {
        internal StructureVersion _Version;
        internal uint _DedicatedVideoMemory;
        internal uint _AvailableDedicatedVideoMemory;
        internal uint _SystemVideoMemory;
        internal uint _SharedSystemMemory;
        internal uint _CurrentAvailableDedicatedVideoMemory;
        internal uint _DedicatedVideoMemoryEvictionsSize;
        internal uint _DedicatedVideoMemoryEvictionCount;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint DedicatedVideoMemoryInkB
        {
            get => _DedicatedVideoMemory;
        }

        /// <inheritdoc />
        public uint AvailableDedicatedVideoMemoryInkB
        {
            get => _AvailableDedicatedVideoMemory;
        }

        /// <inheritdoc />
        public uint SystemVideoMemoryInkB
        {
            get => _SystemVideoMemory;
        }

        /// <inheritdoc />
        public uint SharedSystemMemoryInkB
        {
            get => _SharedSystemMemory;
        }

        /// <inheritdoc />
        public uint CurrentAvailableDedicatedVideoMemoryInkB
        {
            get => _CurrentAvailableDedicatedVideoMemory;
        }

        /// <summary>
        ///     Size(in kb) of the total size of memory released as a result of the evictions.
        /// </summary>
        public uint DedicatedVideoMemoryEvictionsSize
        {
            get => _DedicatedVideoMemoryEvictionsSize;
        }

        /// <summary>
        ///     Indicates the number of eviction events that caused an allocation to be removed from dedicated video memory to free
        ///     GPU video memory to make room for other allocations.
        /// </summary>
        public uint DedicatedVideoMemoryEvictionCount
        {
            get => _DedicatedVideoMemoryEvictionCount;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return
                $"{AvailableDedicatedVideoMemoryInkB / 1024} MB ({CurrentAvailableDedicatedVideoMemoryInkB / 1024} MB) / {DedicatedVideoMemoryInkB / 1024} MB";
        }
    }

    /// <summary>
    ///     Represents a display identification and its attributes
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct DisplayIdsV2 : IInitializable, IDisplayIds, IEquatable<DisplayIdsV2>
    {
        internal StructureVersion _Version;
        internal MonitorConnectionType _ConnectionType;
        internal uint _DisplayId;
        internal uint _RawReserved;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint DisplayId
        {
            get => _DisplayId;
        }

        /// <inheritdoc />
        public bool Equals(DisplayIdsV2 other)
        {
            return _DisplayId == other._DisplayId;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is DisplayIdsV2 v2 && Equals(v2);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int)_DisplayId;
        }

        /// <inheritdoc />
        public MonitorConnectionType ConnectionType
        {
            get => _ConnectionType;
        }

        /// <inheritdoc />
        public bool IsDynamic
        {
            get => _RawReserved.GetBit(0);
            set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <inheritdoc />
        public bool IsMultiStreamRootNode
        {
            get => _RawReserved.GetBit(1);
            set => _RawReserved = _RawReserved.SetBit(1, value);
        }

        /// <inheritdoc />
        public bool IsActive
        {
            get => _RawReserved.GetBit(2);
            set => _RawReserved = _RawReserved.SetBit(2, value);
        }

        /// <inheritdoc />
        public bool IsCluster
        {
            get => _RawReserved.GetBit(3);
            set => _RawReserved = _RawReserved.SetBit(3, value);
        }

        /// <inheritdoc />
        public bool IsOSVisible
        {
            get => _RawReserved.GetBit(4);
            set => _RawReserved = _RawReserved.SetBit(4, value);
        }

        /// <inheritdoc />
        public bool IsWFD
        {
            get => _RawReserved.GetBit(5);
            set => _RawReserved = _RawReserved.SetBit(5, value);
        }

        /// <inheritdoc />
        public bool IsConnected
        {
            get => _RawReserved.GetBit(6);
            set => _RawReserved = _RawReserved.SetBit(6, value);
        }

        /// <inheritdoc />
        public bool IsPhysicallyConnected
        {
            get => _RawReserved.GetBit(17);
            set => _RawReserved = _RawReserved.SetBit(17, value);
        }
    }

    /// <summary>
    ///     Holds information about the dynamic performance states (such as GPU utilization domain)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct DynamicPerformanceStatesInfoV1 : IInitializable, IUtilizationStatus
    {
        internal const int MaxGpuUtilizations = 8;

        internal StructureVersion _Version;
        internal uint _Flags;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxGpuUtilizations)]
        internal UtilizationDomainInfo[] _UtilizationDomain;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint Flags
        {
            get => _Flags;
            set => _Flags = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the dynamic performance state is enabled
        /// </summary>
        public bool IsDynamicPerformanceStatesEnabled
        {
            get => _Flags.GetBit(0);
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <inheritdoc />
        public Dictionary<UtilizationDomain, IUtilizationDomainInfo> Domains
        {
            get => _UtilizationDomain
                .Select((value, index) => new { index, value })
                .Where(arg => Enum.IsDefined(typeof(UtilizationDomain), arg.index) && arg.value.IsPresent)
                .ToDictionary(arg => (UtilizationDomain)arg.index, arg => arg.value as IUtilizationDomainInfo);
        }

        /// <inheritdoc />
        public IUtilizationDomainInfo GPU
        {
            get => _UtilizationDomain[(int)UtilizationDomain.GPU];
        }

        /// <inheritdoc />
        public IUtilizationDomainInfo FrameBuffer
        {
            get => _UtilizationDomain[(int)UtilizationDomain.FrameBuffer];
        }

        /// <inheritdoc />
        public IUtilizationDomainInfo VideoEngine
        {
            get => _UtilizationDomain[(int)UtilizationDomain.VideoEngine];
        }

        /// <inheritdoc />
        public IUtilizationDomainInfo BusInterface
        {
            get => _UtilizationDomain[(int)UtilizationDomain.BusInterface];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"GPU = {GPU} - " +
                   $"FrameBuffer = {FrameBuffer} - " +
                   $"VideoEngine = {VideoEngine} - " +
                   $"BusInterface = {BusInterface}";
        }

        /// <summary>
        ///     Holds information about a dynamic performance state utilization domain
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct UtilizationDomainInfo : IUtilizationDomainInfo
        {
            internal uint _IsPresent;
            internal uint _Percentage;

            /// <inheritdoc />
            public bool IsPresent
            {
                get => _IsPresent.GetBit(0);
                set => _IsPresent.SetBit(0, value);
            }

            /// <inheritdoc />
            public uint Percentage
            {
                get => _Percentage;
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return IsPresent ? $"{Percentage}%" : "N/A";
            }
        }
    }

    /// <summary>
    ///     Contains information about the ECC memory configurations
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ECCConfigurationInfoV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _Flags;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the ECC memory is enabled
        /// </summary>
        public bool IsEnabled
        {
            get => _Flags.GetBit(0); 
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the ECC memory is enabled by default
        /// </summary>
        public bool IsEnabledByDefault
        {
            get => _Flags.GetBit(1); 
            set => _Flags = _Flags.SetBit(1, value);
        }
    }



    /// <summary>
    ///     Contains information regarding the ECC Memory errors
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ECCErrorInfoV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal ECCErrorInfo _CurrentErrors;
        internal ECCErrorInfo _AggregatedErrors;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the number of current errors
        /// </summary>
        public ECCErrorInfo CurrentErrors
        {
            get => _CurrentErrors;
        }

        /// <summary>
        ///     Gets the number of aggregated errors
        /// </summary>
        public ECCErrorInfo AggregatedErrors
        {
            get => _AggregatedErrors;
        }

        /// <summary>
        ///     Contains ECC memory error counters information
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ECCErrorInfo
        {
            internal ulong _SingleBitErrors;
            internal ulong _DoubleBitErrors;

            /// <summary>
            ///     Gets the number of single bit errors
            /// </summary>
            public ulong SingleBitErrors
            {
                get => _SingleBitErrors;
            }

            /// <summary>
            ///     Gets the number of double bit errors
            /// </summary>
            public ulong DoubleBitErrors
            {
                get => _DoubleBitErrors;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding the ECC Memory status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ECCStatusInfoV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _IsSupported;
        internal ECCConfiguration _ConfigurationOptions;
        internal uint _IsEnabled;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the ECC memory is available and supported
        /// </summary>
        public bool IsSupported
        {
            get => _IsSupported.GetBit(0); 
            set => _IsSupported.SetBit(0, value);
        }

        /// <summary>
        ///     Gets the ECC memory configurations
        /// </summary>
        public ECCConfiguration ConfigurationOptions
        {
            get => _ConfigurationOptions;
        }

        /// <summary>
        ///     Gets boolean value indicating if the ECC memory is currently enabled
        /// </summary>
        public bool IsEnabled
        {
            get => _IsEnabled.GetBit(0); 
            set => _IsEnabled.SetBit(0, value);
        }
    }

    /// <summary>
    ///     Holds whole or a part of the EDID information
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct EDIDV1 : IEDID, IInitializable
    {
        /// <summary>
        ///     The maximum number of data bytes that this structure can hold
        /// </summary>
        public const int MaxDataSize = 256;

        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDataSize)]
        internal byte[] _Data;

        internal static EDIDV1 CreateWithData(byte[] data)
        {
            if (data.Length > MaxDataSize)
            {
                throw new ArgumentException("Data is too big.", nameof(data));
            }

            var edid = typeof(EDIDV1).Instantiate<EDIDV1>();
            Array.Copy(data, edid._Data, data.Length);

            return edid;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets whole or a part of the EDID data
        /// </summary>
        public byte[] Data
        {
            get => _Data;
        }
    }

    /// <summary>
    ///     Holds whole or a part of the EDID information
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct EDIDV2 : IEDID, IInitializable
    {
        /// <summary>
        ///     The maximum number of data bytes that this structure can hold
        /// </summary>
        public const int MaxDataSize = EDIDV1.MaxDataSize;

        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDataSize)]
        internal byte[] _Data;

        internal uint _TotalSize;

        internal static EDIDV2 CreateWithData(byte[] data, int totalSize)
        {
            if (data.Length > MaxDataSize)
            {
                throw new ArgumentException("Data is too big.", nameof(data));
            }

            var edid = typeof(EDIDV2).Instantiate<EDIDV2>();
            edid._TotalSize = (uint)totalSize;
            Array.Copy(data, 0, edid._Data, 0, totalSize);

            return edid;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets whole size of the EDID data
        /// </summary>
        public int TotalSize
        {
            get => (int)_TotalSize;
        }

        /// <summary>
        ///     Gets whole or a part of the EDID data
        /// </summary>
        public byte[] Data
        {
            get => _Data.Take((int)Math.Min(_TotalSize, MaxDataSize)).ToArray();
        }
    }

    /// <summary>
    ///     Holds whole or a part of the EDID information
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct EDIDV3 : IEDID, IInitializable
    {
        /// <summary>
        ///     The maximum number of data bytes that this structure can hold
        /// </summary>
        public const int MaxDataSize = EDIDV1.MaxDataSize;

        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDataSize)]
        internal byte[] _Data;

        internal uint _TotalSize;
        internal uint _Identification;
        internal uint _DataOffset;

        internal static EDIDV3 CreateWithOffset(uint id, uint offset)
        {
            var edid = typeof(EDIDV3).Instantiate<EDIDV3>();
            edid._Identification = id;
            edid._DataOffset = offset;

            return edid;
        }

        internal static EDIDV3 CreateWithData(uint id, uint offset, byte[] data, int totalSize)
        {
            if (data.Length > MaxDataSize)
            {
                throw new ArgumentException("Data is too big.", nameof(data));
            }

            var edid = typeof(EDIDV3).Instantiate<EDIDV3>();
            edid._Identification = id;
            edid._DataOffset = offset;
            edid._TotalSize = (uint)totalSize;
            Array.Copy(data, 0, edid._Data, offset, totalSize);

            return edid;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Identification which always returned in a monotonically increasing counter. Across a split-EDID read we need to
        ///     verify that all calls returned the same value. This counter is incremented if we get the updated EDID.
        /// </summary>
        public int Identification
        {
            get => (int)_DataOffset;
        }

        /// <summary>
        ///     Gets data offset of this part of EDID data. Which 256-byte page of the EDID we want to read. Start at 0. If the
        ///     read succeeds with TotalSize > MaxDataSize, call back again with offset+256 until we have read the entire buffer
        /// </summary>
        public int DataOffset
        {
            get => (int)_DataOffset;
        }

        /// <summary>
        ///     Gets whole size of the EDID data
        /// </summary>
        public int TotalSize
        {
            get => (int)_TotalSize;
        }

        /// <inheritdoc />
        public byte[] Data
        {
            get => _Data.Take((int)Math.Min(_TotalSize - DataOffset, MaxDataSize)).ToArray();
        }
    }

    /// <summary>
    ///     Holds necessary information to get an illumination attribute value
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct GetIlluminationParameterV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal PhysicalGPUHandle _GPUHandle;
        internal IlluminationAttribute _Attribute;
        internal uint _ValueInPercentage;

        /// <summary>
        ///     Creates a new instance of <see cref="GetIlluminationParameterV1" />.
        /// </summary>
        /// <param name="gpuHandle">The physical gpu handle.</param>
        /// <param name="attribute">The attribute.</param>
        public GetIlluminationParameterV1(PhysicalGPUHandle gpuHandle, IlluminationAttribute attribute)
        {
            this = typeof(GetIlluminationParameterV1).Instantiate<GetIlluminationParameterV1>();
            _GPUHandle = gpuHandle;
            _Attribute = attribute;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the parameter physical gpu handle
        /// </summary>
        public PhysicalGPUHandle PhysicalGPUHandle
        {
            get => _GPUHandle;
        }

        /// <summary>
        ///     Gets the parameter attribute
        /// </summary>
        public IlluminationAttribute Attribute
        {
            get => _Attribute;
        }

        /// <summary>
        ///     Gets the parameter value in percentage
        /// </summary>
        public uint ValueInPercentage
        {
            get => _ValueInPercentage;
        }
    }

    /// <inheritdoc cref="II2CInfo" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct I2CInfoV2 : IInitializable, IDisposable, II2CInfo
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private StructureVersion _Version;
        private OutputId _OutputMask;
        private byte _UseDDCPort;
        private byte _I2CDeviceAddress;
        private ValueTypeArray _I2CRegisterAddress;
        private uint _I2CRegisterAddressLength;
        private ValueTypeArray _Data;
        private uint _DataLength;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private uint _I2CSpeed;
        private I2CSpeed _I2CSpeedInKHz;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public OutputId OutputMask
        {
            get => _OutputMask;
        }

        /// <inheritdoc />
        public bool UseDDCPort
        {
            get => _UseDDCPort > 0;
        }


        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public I2CSpeed Speed
        {
            get => _I2CSpeedInKHz;
        }

        /// <inheritdoc />
        public bool IsReadOperation
        {
            get => (_I2CDeviceAddress & 1) == 1;
        }

        /// <inheritdoc />
        public byte DeviceAddress
        {
            get => (byte)(_I2CDeviceAddress >> 1);
        }

        /// <inheritdoc />
        public byte[] Data
        {
            get
            {
                if (_Data.IsNull || _DataLength == 0)
                {
                    return new byte[0];
                }

                return _Data.ToArray<byte>((int)_DataLength);
            }
        }

        /// <inheritdoc />
        public byte[] RegisterAddress
        {
            get
            {
                if (_I2CRegisterAddress.IsNull || _I2CRegisterAddressLength == 0)
                {
                    return new byte[0];
                }

                return _I2CRegisterAddress.ToArray<byte>((int)_I2CRegisterAddressLength);
            }
        }

        /// <inheritdoc />
        public byte? PortId
        {
            get => null;
        }

        /// <summary>
        ///     Creates an instance of <see cref="I2CInfoV2" /> for write operations.
        /// </summary>
        /// <param name="outputMask">The target display output mask</param>
        /// <param name="useDDCPort">A boolean value indicating that the DDC port should be used instead of the communication port</param>
        /// <param name="deviceAddress">The device I2C slave address</param>
        /// <param name="registerAddress">The target I2C register address</param>
        /// <param name="data">The payload data</param>
        /// <param name="speed">The target speed of the transaction in kHz</param>
        public I2CInfoV2(
            OutputId outputMask,
            bool useDDCPort,
            byte deviceAddress,
            byte[] registerAddress,
            byte[] data,
            I2CSpeed speed = I2CSpeed.Default
        ) : this(outputMask, useDDCPort, deviceAddress, false, registerAddress, data, speed)
        {
        }

        /// <summary>
        ///     Creates an instance of <see cref="I2CInfoV2" /> for read operations.
        /// </summary>
        /// <param name="outputMask">The target display output mask</param>
        /// <param name="useDDCPort">A boolean value indicating that the DDC port should be used instead of the communication port</param>
        /// <param name="deviceAddress">The device I2C slave address</param>
        /// <param name="registerAddress">The target I2C register address</param>
        /// <param name="readDataLength">The length of the buffer to allocate for the read operation.</param>
        /// <param name="speed">The target speed of the transaction in kHz</param>
        public I2CInfoV2(
            OutputId outputMask,
            bool useDDCPort,
            byte deviceAddress,
            byte[] registerAddress,
            uint readDataLength,
            I2CSpeed speed = I2CSpeed.Default
        ) : this(outputMask, useDDCPort, deviceAddress, true, registerAddress, new byte[readDataLength], speed)
        {
        }

        private I2CInfoV2(
            OutputId outputMask,
            bool useDDCPort,
            byte deviceAddress,
            bool isRead,
            byte[] registerAddress,
            byte[] data,
            I2CSpeed speed = I2CSpeed.Default
        )
        {
            this = typeof(I2CInfoV2).Instantiate<I2CInfoV2>();

            _UseDDCPort = useDDCPort ? (byte)1 : (byte)0;
            _OutputMask = outputMask;
            _I2CDeviceAddress = (byte)(deviceAddress << 1);
            _I2CSpeed = 0xFFFF; // Deprecated
            _I2CSpeedInKHz = speed;

            if (isRead)
            {
                _I2CDeviceAddress |= 1;
            }

            if (registerAddress?.Length > 0)
            {
                _I2CRegisterAddress = ValueTypeArray.FromArray(registerAddress);
                _I2CRegisterAddressLength = (uint)registerAddress.Length;
            }
            else
            {
                _I2CRegisterAddress = ValueTypeArray.Null;
                _I2CRegisterAddressLength = 0;
            }

            if (data?.Length > 0)
            {
                _Data = ValueTypeArray.FromArray(data);
                _DataLength = (uint)data.Length;
            }
            else
            {
                _Data = ValueTypeArray.Null;
                _DataLength = 0;
            }
        }

        /// <summary>
        ///     Calculates and fills the last byte of data to the checksum value required by the DDCCI protocol
        /// </summary>
        /// <param name="deviceAddress">The target device address.</param>
        /// <param name="registerAddress">The target register address.</param>
        /// <param name="data">The data to be sent and store the checksum.</param>
        public static void FillDDCCIChecksum(byte deviceAddress, byte[] registerAddress, byte[] data)
        {
            var checksum = deviceAddress;

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("Checksum needs at least one free byte.", nameof(data));
            }

            if (registerAddress == null)
            {
                throw new ArgumentNullException(nameof(registerAddress));
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < registerAddress.Length; i++)
            {
                checksum ^= registerAddress[i];
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < data.Length - 1; i++)
            {
                checksum ^= data[i];
            }

            data[data.Length - 1] = checksum;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_I2CRegisterAddress.IsNull)
            {
                _I2CRegisterAddress.Dispose();
            }

            if (!_Data.IsNull)
            {
                _Data.Dispose();
            }
        }
    }

    /// <inheritdoc cref="II2CInfo" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct I2CInfoV3 : IInitializable, IDisposable, II2CInfo
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private StructureVersion _Version;
        private OutputId _OutputMask;
        private byte _UseDDCPort;
        private byte _I2CDeviceAddress;
        private ValueTypeArray _I2CRegisterAddress;
        private uint _I2CRegisterAddressLength;
        private ValueTypeArray _Data;
        private uint _DataLength;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private uint _I2CSpeed;
        private I2CSpeed _I2CSpeedInKHz;
        private byte _PortId;
        private uint _IsPortIdPresent;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public OutputId OutputMask
        {
            get => _OutputMask;
        }

        /// <inheritdoc />
        public bool UseDDCPort
        {
            get => _UseDDCPort > 0;
        }

        /// <inheritdoc />
        // ReSharper disable once ConvertToAutoProperty
        public I2CSpeed Speed
        {
            get => _I2CSpeedInKHz;
        }

        /// <inheritdoc />
        public bool IsReadOperation
        {
            get => (_I2CDeviceAddress & 1) == 1;
        }

        /// <inheritdoc />
        public byte DeviceAddress
        {
            get => (byte)(_I2CDeviceAddress >> 1);
        }

        /// <inheritdoc />
        public byte? PortId
        {
            get
            {
                if (_IsPortIdPresent > 0)
                {
                    return _PortId;
                }

                return null;
            }
        }

        /// <inheritdoc />
        public byte[] Data
        {
            get
            {
                if (_Data.IsNull || _DataLength == 0)
                {
                    return new byte[0];
                }

                return _Data.ToArray<byte>((int)_DataLength);
            }
        }

        /// <inheritdoc />
        public byte[] RegisterAddress
        {
            get
            {
                if (_I2CRegisterAddress.IsNull || _I2CRegisterAddressLength == 0)
                {
                    return new byte[0];
                }

                return _I2CRegisterAddress.ToArray<byte>((int)_I2CRegisterAddressLength);
            }
        }

        /// <summary>
        ///     Creates an instance of <see cref="I2CInfoV3" /> for write operations.
        /// </summary>
        /// <param name="outputMask">The target display output mask</param>
        /// <param name="portId">The port id on which device is connected</param>
        /// <param name="useDDCPort">A boolean value indicating that the DDC port should be used instead of the communication port</param>
        /// <param name="deviceAddress">The device I2C slave address</param>
        /// <param name="registerAddress">The target I2C register address</param>
        /// <param name="data">The payload data</param>
        /// <param name="speed">The target speed of the transaction in kHz</param>
        public I2CInfoV3(
            OutputId outputMask,
            byte? portId,
            bool useDDCPort,
            byte deviceAddress,
            byte[] registerAddress,
            byte[] data,
            I2CSpeed speed = I2CSpeed.Default
        ) : this(outputMask, portId, useDDCPort, deviceAddress, false, registerAddress, data, speed)
        {
        }

        /// <summary>
        ///     Creates an instance of <see cref="I2CInfoV3" /> for read operations.
        /// </summary>
        /// <param name="outputMask">The target display output mask</param>
        /// <param name="portId">The port id on which device is connected</param>
        /// <param name="useDDCPort">A boolean value indicating that the DDC port should be used instead of the communication port</param>
        /// <param name="deviceAddress">The device I2C slave address</param>
        /// <param name="registerAddress">The target I2C register address</param>
        /// <param name="readDataLength">The length of the buffer to allocate for the read operation.</param>
        /// <param name="speed">The target speed of the transaction in kHz</param>
        public I2CInfoV3(
            OutputId outputMask,
            byte? portId,
            bool useDDCPort,
            byte deviceAddress,
            byte[] registerAddress,
            uint readDataLength,
            I2CSpeed speed = I2CSpeed.Default
        ) : this(outputMask, portId, useDDCPort, deviceAddress, true, registerAddress, new byte[readDataLength], speed)
        {
        }

        private I2CInfoV3(
            OutputId outputMask,
            byte? portId,
            bool useDDCPort,
            byte deviceAddress,
            bool isRead,
            byte[] registerAddress,
            byte[] data,
            I2CSpeed speed = I2CSpeed.Default
        )
        {
            this = typeof(I2CInfoV3).Instantiate<I2CInfoV3>();

            _UseDDCPort = useDDCPort ? (byte)1 : (byte)0;
            _OutputMask = outputMask;
            _I2CDeviceAddress = (byte)(deviceAddress << 1);
            _I2CSpeed = 0xFFFF; // Deprecated
            _I2CSpeedInKHz = speed;

            if (isRead)
            {
                _I2CDeviceAddress |= 1;
            }

            if (portId != null)
            {
                _PortId = portId.Value;
                _IsPortIdPresent = 1;
            }
            else
            {
                _IsPortIdPresent = 0;
            }

            if (registerAddress?.Length > 0)
            {
                _I2CRegisterAddress = ValueTypeArray.FromArray(registerAddress);
                _I2CRegisterAddressLength = (uint)registerAddress.Length;
            }
            else
            {
                _I2CRegisterAddress = ValueTypeArray.Null;
                _I2CRegisterAddressLength = 0;
            }

            if (data?.Length > 0)
            {
                _Data = ValueTypeArray.FromArray(data);
                _DataLength = (uint)data.Length;
            }
            else
            {
                _Data = ValueTypeArray.Null;
                _DataLength = 0;
            }
        }

        /// <summary>
        ///     Calculates and fills the last byte of data to the checksum value required by the DDCCI protocol
        /// </summary>
        /// <param name="deviceAddress">The target device address.</param>
        /// <param name="registerAddress">The target register address.</param>
        /// <param name="data">The data to be sent and store the checksum.</param>
        public static void FillDDCCIChecksum(byte deviceAddress, byte[] registerAddress, byte[] data)
        {
            I2CInfoV2.FillDDCCIChecksum(deviceAddress, registerAddress, data);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (!_I2CRegisterAddress.IsNull)
            {
                _I2CRegisterAddress.Dispose();
            }

            if (!_Data.IsNull)
            {
                _Data.Dispose();
            }
        }
    }

    /// <summary>
    ///     Holds information regarding available devices illumination settings
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct IlluminationDeviceControlParametersV1 : IInitializable
    {
        private const int MaximumNumberOfReserved = 64;
        private const int MaximumNumberOfDevices = 32;
        internal StructureVersion _Version;
        internal uint _NumberOfDevices;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReserved)]
        internal byte[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfDevices)]
        internal IlluminationDeviceControlV1[] _Devices;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationDeviceControlParametersV1" />.
        /// </summary>
        /// <param name="devices">The list of illumination settings of devices.</param>
        public IlluminationDeviceControlParametersV1(IlluminationDeviceControlV1[] devices)
        {
            if (!(devices?.Length > 0) || devices.Length > MaximumNumberOfDevices)
            {
                throw new ArgumentOutOfRangeException(nameof(devices));
            }

            this = typeof(IlluminationDeviceControlParametersV1).Instantiate<IlluminationDeviceControlParametersV1>();
            _NumberOfDevices = (uint)devices.Length;
            Array.Copy(devices, 0, _Devices, 0, devices.Length);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint NumberOfDevices
        {
            get => _NumberOfDevices;
            set => _NumberOfDevices = value;
        }

        /// <summary>
        ///     Gets a list of available illumination settings of devices.
        /// </summary>
        public IlluminationDeviceControlV1[] Devices
        {
            get => _Devices.Take((int)_NumberOfDevices).ToArray();
        }
    }

    /// <summary>
    ///     Holds information regarding a device illumination settings
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct IlluminationDeviceControlV1 : IInitializable
    {
        private const int MaximumNumberOfReserved = 64;
        internal IlluminationDeviceType _DeviceType;
        internal IlluminationDeviceSyncV1 _SyncInformation;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReserved)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationDeviceControlV1" />.
        /// </summary>
        /// <param name="deviceType">The device type.</param>
        /// <param name="syncInformation">The device sync information.</param>
        public IlluminationDeviceControlV1(IlluminationDeviceType deviceType, IlluminationDeviceSyncV1 syncInformation)
        {
            this = typeof(IlluminationDeviceControlV1).Instantiate<IlluminationDeviceControlV1>();
            _DeviceType = deviceType;
            _SyncInformation = syncInformation;
        }

        /// <summary>
        ///     Gets the illumination device type
        /// </summary>
        public IlluminationDeviceType DeviceType
        {
            get => _DeviceType;
        }

        /// <summary>
        ///     Gets the illumination synchronization information
        /// </summary>
        public IlluminationDeviceSyncV1 SyncInformation
        {
            get => _SyncInformation;
        }
    }

    /// <summary>
    ///     Holds information regarding available illumination devices
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct IlluminationDeviceInfoParametersV1 : IInitializable
    {
        private const int MaximumNumberOfReserved = 64;
        private const int MaximumNumberOfDevices = 32;
        internal StructureVersion _Version;
        internal uint _NumberOfDevices;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReserved)]
        internal byte[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfDevices)]
        internal IlluminationDeviceInfoV1[] _Devices;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint NumberOfDevices
        {
            get => _NumberOfDevices;
            set => _NumberOfDevices = value;
        }


        /// <summary>
        ///     Gets an array containing all available illumination devices
        /// </summary>
        public IlluminationDeviceInfoV1[] Devices
        {
            get => _Devices.Take((int)_NumberOfDevices).ToArray();
        }
    }

    /// <summary>
    ///     Holds information regarding a illumination device
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationDeviceInfoV1 : IInitializable
    {
        private const int MaximumNumberOfReserved = 64;
        private const int MaximumNumberOfDeviceData = 64;
        internal IlluminationDeviceType _DeviceType;
        internal IlluminationZoneControlMode _ControlModes;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfDeviceData)]
        internal byte[] _DeviceData;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReserved)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Gets the illumination device type
        /// </summary>
        public IlluminationDeviceType DeviceType
        {
            get => _DeviceType;
        }

        /// <summary>
        ///     Gets the illumination device control mode
        /// </summary>
        public IlluminationZoneControlMode ControlMode
        {
            get => _ControlModes;
        }

        /// <summary>
        ///     Gets the I2C index for a MCUV10 device
        /// </summary>
        /// <exception cref="InvalidOperationException" accessor="get">Device type is not MCUV10.</exception>
        public byte MCUV10DeviceI2CIndex
        {
            get
            {
                if (DeviceType != IlluminationDeviceType.MCUV10)
                {
                    throw new InvalidOperationException("Device type is not MCUV10.");
                }

                return _DeviceData[0];
            }
        }
    }

    /// <summary>
    ///     Holds information regarding the data necessary for synchronization.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationDeviceSyncV1 : IInitializable
    {
        private const int MaximumNumberOfReserved = 64;
        internal byte _IsSync;
        internal ulong _TimeStampInMS;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReserved)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationDeviceSyncV1" />
        /// </summary>
        /// <param name="isSync">A boolean value indicating if synchronization is enabled.</param>
        /// <param name="timeStampInMS">The synchronization timestamp in ms</param>
        public IlluminationDeviceSyncV1(bool isSync, ulong timeStampInMS)
        {
            this = typeof(IlluminationDeviceSyncV1).Instantiate<IlluminationDeviceSyncV1>();
            _IsSync = isSync ? (byte)1 : (byte)0;
            _TimeStampInMS = timeStampInMS;
        }

        /// <summary>
        ///     Gets a boolean value indicating the need for synchronization.
        /// </summary>
        public bool IsSync
        {
            get => _IsSync > 0;
        }

        /// <summary>
        ///     Gets the timestamp in milliseconds required for synchronization.
        /// </summary>
        public ulong TimeStampInMS
        {
            get => _TimeStampInMS;
        }
    }

    /// <summary>
    ///     Holds information regarding a fixed color control data
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlDataFixedColor : IInitializable
    {
        private const int MaximumNumberOfDataBytes = 64;
        private const int MaximumNumberOfReservedBytes = 64;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfDataBytes)]
        internal byte[] _Data;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReservedBytes)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataFixedColor" />.
        /// </summary>
        /// <param name="manualFixedColor">The zone manual control data.</param>
        public IlluminationZoneControlDataFixedColor(IlluminationZoneControlDataManualFixedColor manualFixedColor)
            : this(manualFixedColor.ToByteArray())
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataFixedColor" />.
        /// </summary>
        /// <param name="piecewiseLinearFixedColor">The zone piecewise linear control data.</param>
        public IlluminationZoneControlDataFixedColor(
            IlluminationZoneControlDataPiecewiseLinearFixedColor piecewiseLinearFixedColor)
            : this(piecewiseLinearFixedColor.ToByteArray())
        {
        }

        private IlluminationZoneControlDataFixedColor(byte[] data)
        {
            if (!(data?.Length > 0) || data.Length > MaximumNumberOfDataBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            this = typeof(IlluminationZoneControlDataFixedColor).Instantiate<IlluminationZoneControlDataFixedColor>();
            Array.Copy(data, 0, _Data, 0, data.Length);
        }

        /// <summary>
        ///     Gets the control data as a manual control structure.
        /// </summary>
        /// <returns>An instance of <see cref="IlluminationZoneControlDataManualFixedColor" /> containing manual settings.</returns>
        public IlluminationZoneControlDataManualFixedColor AsManual()
        {
            return _Data.ToStructure<IlluminationZoneControlDataManualFixedColor>();
        }

        /// <summary>
        ///     Gets the control data as a piecewise linear control structure.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="IlluminationZoneControlDataPiecewiseLinearFixedColor" /> containing piecewise
        ///     settings.
        /// </returns>
        public IlluminationZoneControlDataPiecewiseLinearFixedColor AsPiecewise()
        {
            return _Data.ToStructure<IlluminationZoneControlDataPiecewiseLinearFixedColor>();
        }
    }

    /// <summary>
    ///     Holds information regarding a fixed color
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlDataFixedColorParameters
    {
        internal byte _BrightnessInPercentage;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataFixedColorParameters" />.
        /// </summary>
        /// <param name="brightnessInPercentage">The brightness percentage value of the zone.</param>
        public IlluminationZoneControlDataFixedColorParameters(byte brightnessInPercentage)
        {
            _BrightnessInPercentage = brightnessInPercentage;
        }

        /// <summary>
        ///     Gets the brightness percentage value of the zone.
        /// </summary>
        public byte BrightnessInPercentage
        {
            get => _BrightnessInPercentage;
        }
    }

    /// <summary>
    ///     Holds information regarding a manual fixed color control method
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlDataManualFixedColor : IInitializable
    {
        internal IlluminationZoneControlDataFixedColorParameters _Parameters;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataManualFixedColor" />.
        /// </summary>
        /// <param name="parameters">The fixed color parameters.</param>
        public IlluminationZoneControlDataManualFixedColor(IlluminationZoneControlDataFixedColorParameters parameters)
        {
            _Parameters = parameters;
        }

        /// <summary>
        ///     Gets the fixed color parameters
        /// </summary>
        internal IlluminationZoneControlDataFixedColorParameters Parameters
        {
            get => _Parameters;
        }
    }

    /// <summary>
    ///     Holds information regarding a RGB control method
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlDataManualRGB : IInitializable
    {
        internal IlluminationZoneControlDataManualRGBParameters _Parameters;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataManualRGB" />.
        /// </summary>
        /// <param name="parameters">The RGB parameters.</param>
        public IlluminationZoneControlDataManualRGB(IlluminationZoneControlDataManualRGBParameters parameters)
        {
            _Parameters = parameters;
        }

        /// <summary>
        ///     Gets the RGB parameters
        /// </summary>
        public IlluminationZoneControlDataManualRGBParameters Parameters
        {
            get => _Parameters;
        }
    }

    /// <summary>
    ///     Holds information regarding a RGB color
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlDataManualRGBParameters
    {
        internal byte _Red;
        internal byte _Green;
        internal byte _Blue;
        internal byte _BrightnessInPercentage;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataManualRGBParameters" />.
        /// </summary>
        /// <param name="red">The red component of color applied to the zone.</param>
        /// <param name="green">The green component of color applied to the zone.</param>
        /// <param name="blue">The blue component of color applied to the zone.</param>
        /// <param name="brightnessInPercentage">The brightness percentage value of the zone.</param>
        // ReSharper disable once TooManyDependencies
        public IlluminationZoneControlDataManualRGBParameters(
            byte red,
            byte green,
            byte blue,
            byte brightnessInPercentage)
        {
            _Red = red;
            _Green = green;
            _Blue = blue;
            _BrightnessInPercentage = brightnessInPercentage;
        }

        /// <summary>
        ///     Gets the red component of color applied to the zone.
        /// </summary>
        public byte Red
        {
            get => _Red;
        }

        /// <summary>
        ///     Gets the green component of color applied to the zone.
        /// </summary>
        public byte Green
        {
            get => _Green;
        }

        /// <summary>
        ///     Gets the blue component of color applied to the zone.
        /// </summary>
        public byte Blue
        {
            get => _Blue;
        }

        /// <summary>
        ///     Gets the brightness percentage value of the zone.
        /// </summary>
        public byte BrightnessInPercentage
        {
            get => _BrightnessInPercentage;
        }
    }

    /// <summary>
    ///     Holds information regarding a piecewise linear function settings
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlDataPiecewiseLinear
    {
        internal IlluminationPiecewiseLinearCycleType _CycleType;
        internal byte _GroupPeriodRepeatCount;
        internal ushort _RiseDurationInMS;
        internal ushort _FallDurationInMS;
        internal ushort _ADurationInMS;
        internal ushort _BDurationInMS;
        internal ushort _NextGroupIdleDurationInMS;
        internal ushort _PhaseOffsetInMS;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataPiecewiseLinear" />.
        /// </summary>
        /// <param name="cycleType">The type of cycle effect to apply.</param>
        /// <param name="groupPeriodRepeatCount">The number of times to repeat function within group period.</param>
        /// <param name="riseDurationInMS">The time in millisecond to transition from color A to color B.</param>
        /// <param name="fallDurationInMS">The time in millisecond to transition from color B to color A.</param>
        /// <param name="aDurationInMS">The time in millisecond to remain at color A before color A to color B transition.</param>
        /// <param name="bDurationInMS">The time in millisecond to remain at color B before color B to color A transition.</param>
        /// <param name="nextGroupIdleDurationInMS">
        ///     The time in millisecond to remain idle before next group of repeated function
        ///     cycles.
        /// </param>
        /// <param name="phaseOffsetInMS">The time in millisecond to offset the cycle relative to other zones.</param>
        // ReSharper disable once TooManyDependencies
        public IlluminationZoneControlDataPiecewiseLinear(
            IlluminationPiecewiseLinearCycleType cycleType,
            byte groupPeriodRepeatCount,
            ushort riseDurationInMS,
            ushort fallDurationInMS,
            ushort aDurationInMS,
            ushort bDurationInMS,
            ushort nextGroupIdleDurationInMS,
            ushort phaseOffsetInMS)
        {
            _CycleType = cycleType;
            _GroupPeriodRepeatCount = groupPeriodRepeatCount;
            _RiseDurationInMS = riseDurationInMS;
            _FallDurationInMS = fallDurationInMS;
            _ADurationInMS = aDurationInMS;
            _BDurationInMS = bDurationInMS;
            _NextGroupIdleDurationInMS = nextGroupIdleDurationInMS;
            _PhaseOffsetInMS = phaseOffsetInMS;
        }

        /// <summary>
        ///     Gets the time in millisecond to offset the cycle relative to other zones.
        /// </summary>
        public ushort PhaseOffsetInMS
        {
            get => _PhaseOffsetInMS;
        }

        /// <summary>
        ///     Gets the time in millisecond to remain idle before next group of repeated function cycles.
        /// </summary>
        public ushort NextGroupIdleDurationInMS
        {
            get => _NextGroupIdleDurationInMS;
        }

        /// <summary>
        ///     Gets the time in millisecond to remain at color B before color B to color A transition.
        /// </summary>
        public ushort BDurationInMS
        {
            get => _BDurationInMS;
        }

        /// <summary>
        ///     Gets the time in millisecond to remain at color A before color A to color B transition.
        /// </summary>
        public ushort ADurationInMS
        {
            get => _ADurationInMS;
        }

        /// <summary>
        ///     Gets the time in millisecond to transition from color B to color A.
        /// </summary>
        public ushort FallDurationInMS
        {
            get => _FallDurationInMS;
        }

        /// <summary>
        ///     Gets the time in millisecond to transition from color A to color B.
        /// </summary>
        public ushort RiseDurationInMS
        {
            get => _RiseDurationInMS;
        }

        /// <summary>
        ///     Gets the number of times to repeat function within group period.
        /// </summary>
        public byte GroupPeriodRepeatCount
        {
            get => _GroupPeriodRepeatCount;
        }

        /// <summary>
        ///     Gets the type of cycle effect to apply.
        /// </summary>
        public IlluminationPiecewiseLinearCycleType CycleType
        {
            get => _CycleType;
        }
    }

    /// <summary>
    ///     Holds information regarding a piecewise linear fixed color control method
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlDataPiecewiseLinearFixedColor : IInitializable
    {
        private const int NumberColorEndPoints = 2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NumberColorEndPoints)]
        internal IlluminationZoneControlDataFixedColorParameters[] _EndPoints;

        internal IlluminationZoneControlDataPiecewiseLinear _PiecewiseLinearData;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataPiecewiseLinearFixedColor" />.
        /// </summary>
        /// <param name="endPoints">The list of fixed color piecewise function endpoints.</param>
        /// <param name="piecewiseLinearData">The piecewise function settings.</param>
        public IlluminationZoneControlDataPiecewiseLinearFixedColor(
            IlluminationZoneControlDataFixedColorParameters[] endPoints,
            IlluminationZoneControlDataPiecewiseLinear piecewiseLinearData)
        {
            if (endPoints?.Length != NumberColorEndPoints)
            {
                throw new ArgumentOutOfRangeException(nameof(endPoints));
            }

            this = typeof(IlluminationZoneControlDataPiecewiseLinearFixedColor)
                .Instantiate<IlluminationZoneControlDataPiecewiseLinearFixedColor>();
            _PiecewiseLinearData = piecewiseLinearData;
            Array.Copy(endPoints, 0, _EndPoints, 0, endPoints.Length);
        }

        /// <summary>
        ///     Gets the piecewise function settings
        /// </summary>
        public IlluminationZoneControlDataPiecewiseLinear PiecewiseLinearData
        {
            get => _PiecewiseLinearData;
        }

        /// <summary>
        ///     Gets the list of fixed color piecewise function endpoints
        /// </summary>
        public IlluminationZoneControlDataFixedColorParameters[] EndPoints
        {
            get => _EndPoints;
        }
    }

    /// <summary>
    ///     Holds information regarding a piecewise linear RGB control method
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlDataPiecewiseLinearRGB : IInitializable
    {
        private const int NumberColorEndPoints = 2;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NumberColorEndPoints)]
        internal IlluminationZoneControlDataManualRGBParameters[] _EndPoints;

        internal IlluminationZoneControlDataPiecewiseLinear _PiecewiseLinearData;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataPiecewiseLinearRGB" />.
        /// </summary>
        /// <param name="endPoints">The list of RGB piecewise function endpoints.</param>
        /// <param name="piecewiseLinearData">The piecewise function settings.</param>
        public IlluminationZoneControlDataPiecewiseLinearRGB(
            IlluminationZoneControlDataManualRGBParameters[] endPoints,
            IlluminationZoneControlDataPiecewiseLinear piecewiseLinearData)
        {
            if (endPoints?.Length != NumberColorEndPoints)
            {
                throw new ArgumentOutOfRangeException(nameof(endPoints));
            }

            this = typeof(IlluminationZoneControlDataPiecewiseLinearRGB)
                .Instantiate<IlluminationZoneControlDataPiecewiseLinearRGB>();
            _PiecewiseLinearData = piecewiseLinearData;
            Array.Copy(endPoints, 0, _EndPoints, 0, endPoints.Length);
        }

        /// <summary>
        ///     Gets the piecewise function settings
        /// </summary>
        public IlluminationZoneControlDataPiecewiseLinear PiecewiseLinearData
        {
            get => _PiecewiseLinearData;
        }

        /// <summary>
        ///     Gets the list of RGB function endpoints
        /// </summary>
        public IlluminationZoneControlDataManualRGBParameters[] EndPoints
        {
            get => _EndPoints;
        }
    }

    /// <summary>
    ///     Holds information regarding a RGB control data
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlDataRGB : IInitializable
    {
        private const int MaximumNumberOfDataBytes = 64;
        private const int MaximumNumberOfReservedBytes = 64;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfDataBytes)]
        internal byte[] _Data;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReservedBytes)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataRGB" />.
        /// </summary>
        /// <param name="manualRGB">The zone manual control data.</param>
        public IlluminationZoneControlDataRGB(IlluminationZoneControlDataManualRGB manualRGB)
            : this(manualRGB.ToByteArray())
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlDataRGB" />.
        /// </summary>
        /// <param name="piecewiseLinearRGB">The zone piecewise linear control data.</param>
        public IlluminationZoneControlDataRGB(IlluminationZoneControlDataPiecewiseLinearRGB piecewiseLinearRGB)
            : this(piecewiseLinearRGB.ToByteArray())
        {
        }

        private IlluminationZoneControlDataRGB(byte[] data)
        {
            if (!(data?.Length > 0) || data.Length > MaximumNumberOfDataBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            this = typeof(IlluminationZoneControlDataRGB).Instantiate<IlluminationZoneControlDataRGB>();
            Array.Copy(data, 0, _Data, 0, data.Length);
        }

        /// <summary>
        ///     Gets the control data as a manual control structure.
        /// </summary>
        /// <returns>An instance of <see cref="IlluminationZoneControlDataManualRGB" /> containing manual settings.</returns>
        public IlluminationZoneControlDataManualRGB AsManual()
        {
            return _Data.ToStructure<IlluminationZoneControlDataManualRGB>();
        }

        /// <summary>
        ///     Gets the control data as a piecewise linear control structure.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="IlluminationZoneControlDataPiecewiseLinearRGB" /> containing piecewise linear
        ///     settings.
        /// </returns>
        public IlluminationZoneControlDataPiecewiseLinearRGB AsPiecewise()
        {
            return _Data.ToStructure<IlluminationZoneControlDataPiecewiseLinearRGB>();
        }
    }

    /// <summary>
    ///     Holds information regarding available zone control settings
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct IlluminationZoneControlParametersV1 : IInitializable
    {
        private const int MaximumNumberOfZoneControls = 32;
        private const int MaximumNumberOfReservedBytes = 64;
        internal StructureVersion _Version;
        internal uint _Flags;
        internal uint _NumberOfZoneControls;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReservedBytes)]
        internal byte[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfZoneControls)]
        internal IlluminationZoneControlV1[] _ZoneControls;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlParametersV1" />.
        /// </summary>
        /// <param name="valuesType">The type of settings to represents.</param>
        public IlluminationZoneControlParametersV1(IlluminationZoneControlValuesType valuesType)
        {
            this = typeof(IlluminationZoneControlParametersV1).Instantiate<IlluminationZoneControlParametersV1>();
            _Flags.SetBit(0, valuesType == IlluminationZoneControlValuesType.Default);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlParametersV1" />.
        /// </summary>
        /// <param name="valuesType">The type of settings to represents.</param>
        /// <param name="zoneControls">An array of zone control settings.</param>
        public IlluminationZoneControlParametersV1(
            IlluminationZoneControlValuesType valuesType,
            IlluminationZoneControlV1[] zoneControls) : this(valuesType)
        {
            if (!(zoneControls?.Length > 0) || zoneControls.Length > MaximumNumberOfZoneControls)
            {
                throw new ArgumentOutOfRangeException(nameof(valuesType));
            }

            _NumberOfZoneControls = (uint)zoneControls.Length;
            Array.Copy(zoneControls, 0, _ZoneControls, 0, zoneControls.Length);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the type of settings to represents.
        /// </summary>
        public IlluminationZoneControlValuesType ValuesType
        {
            get => _Flags.GetBit(0)
                ? IlluminationZoneControlValuesType.Default
                : IlluminationZoneControlValuesType.CurrentlyActive;
        }

        /// <summary>
        ///     Gets an array of zone control settings
        /// </summary>
        public IlluminationZoneControlV1[] ZoneControls
        {
            get => _ZoneControls.Take((int)_NumberOfZoneControls).ToArray();
        }
    }

    /// <summary>
    ///     Holds information regarding a zone control status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneControlV1 : IInitializable
    {
        private const int MaximumNumberOfDataBytes = 128;
        private const int MaximumNumberOfReservedBytes = 64;

        internal IlluminationZoneType _ZoneType;

        internal IlluminationZoneControlMode _ControlMode;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfDataBytes)]
        internal byte[] _Data;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReservedBytes)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlV1" />.
        /// </summary>
        /// <param name="controlMode">The zone control mode.</param>
        /// <param name="rgbData">The zone control RGB data.</param>
        public IlluminationZoneControlV1(
            IlluminationZoneControlMode controlMode,
            IlluminationZoneControlDataRGB rgbData)
            : this(controlMode, IlluminationZoneType.RGB, rgbData.ToByteArray())
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="IlluminationZoneControlV1" />.
        /// </summary>
        /// <param name="controlMode">The zone control mode.</param>
        /// <param name="fixedColorData">The zone control fixed color data.</param>
        public IlluminationZoneControlV1(
            IlluminationZoneControlMode controlMode,
            IlluminationZoneControlDataFixedColor fixedColorData)
            : this(controlMode, IlluminationZoneType.FixedColor, fixedColorData.ToByteArray())
        {
        }

        private IlluminationZoneControlV1(
            IlluminationZoneControlMode controlMode,
            IlluminationZoneType zoneType,
            byte[] data)
        {
            if (!(data?.Length > 0) || data.Length > MaximumNumberOfDataBytes)
            {
                throw new ArgumentOutOfRangeException(nameof(data));
            }

            this = typeof(IlluminationZoneControlV1).Instantiate<IlluminationZoneControlV1>();
            _ControlMode = controlMode;
            _ZoneType = zoneType;
            Array.Copy(data, 0, _Data, 0, data.Length);
        }

        /// <summary>
        ///     Gets the type of zone and the type of data needed to control this zone
        /// </summary>
        internal IlluminationZoneType ZoneType
        {
            get => _ZoneType;
        }

        /// <summary>
        ///     Gets the zone control mode
        /// </summary>
        internal IlluminationZoneControlMode ControlMode
        {
            get => _ControlMode;
        }

        /// <summary>
        ///     Gets the control data as a RGB data structure.
        /// </summary>
        /// <returns>An instance of <see cref="IlluminationZoneControlDataRGB" /> containing RGB settings.</returns>
        public IlluminationZoneControlDataRGB AsRGBData()
        {
            return _Data.ToStructure<IlluminationZoneControlDataRGB>();
        }

        /// <summary>
        ///     Gets the control data as a fixed color data structure.
        /// </summary>
        /// <returns>An instance of <see cref="IlluminationZoneControlDataFixedColor" /> containing fixed color settings.</returns>
        public IlluminationZoneControlDataFixedColor AsFixedColorData()
        {
            return _Data.ToStructure<IlluminationZoneControlDataFixedColor>();
        }
    }

    /// <summary>
    ///     Holds information regarding illumination zones
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct IlluminationZoneInfoParametersV1 : IInitializable
    {
        private const int MaximumNumberOfReserved = 64;
        private const int MaximumNumberOfZones = 32;
        internal StructureVersion _Version;
        internal uint _NumberOfZones;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReserved)]
        internal byte[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfZones)]
        internal IlluminationZoneInfoV1[] _Zones;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint NumberOfZones
        {
            get => _NumberOfZones;
            set => _NumberOfZones = value;
        }

        /// <summary>
        ///     Gets the list of illumination zones.
        /// </summary>
        public IlluminationZoneInfoV1[] Zones
        {
            get => _Zones.Take((int)_NumberOfZones).ToArray();
        }
    }

    /// <summary>
    ///     Holds information regarding a illumination zone
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct IlluminationZoneInfoV1 : IInitializable
    {
        private const int MaximumNumberOfReserved = 64;
        private const int MaximumNumberOfDataBytes = 64;

        internal IlluminationZoneType _ZoneType;
        internal byte _DeviceIndex;
        internal byte _ProviderIndex;
        internal IlluminationZoneLocation _ZoneLocation;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfDataBytes)]
        internal byte[] _Data;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfReserved)]
        internal byte[] _Reserved;

        /// <summary>
        ///     Gets the index of the illumination device that controls this zone.
        /// </summary>
        public int DeviceIndex
        {
            get => _DeviceIndex;
        }

        /// <summary>
        ///     Gets the provider index used for representing logical to physical zone mapping.
        /// </summary>
        public int ProviderIndex
        {
            get => _ProviderIndex;
        }

        /// <summary>
        ///     Gets the location of the zone on the board.
        /// </summary>
        public IlluminationZoneLocation ZoneLocation
        {
            get => _ZoneLocation;
        }

        /// <summary>
        ///     Gets the zone type.
        /// </summary>
        internal IlluminationZoneType ZoneType
        {
            get => _ZoneType;
        }
    }

    /// <summary>
    ///     LogicalGPUHandle is a reference to one or more physical GPUs acting as a single logical device. A single GPU will
    ///     have a single logical GPU handle and a single physical GPU handle. Two GPUs acting in an SLI configuration will
    ///     have a single logical GPU handle and two physical GPU handles.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct LogicalGPUHandle : IHandle, IEquatable<LogicalGPUHandle>
    {
        /// <summary>
        ///     Maximum number of logical GPUs
        /// </summary>
        public const int MaxLogicalGPUs = 64;

        internal readonly IntPtr _MemoryAddress;

        /// <inheritdoc />
        public bool Equals(LogicalGPUHandle other)
        {
            return _MemoryAddress.Equals(other._MemoryAddress);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is LogicalGPUHandle handle && Equals(handle);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _MemoryAddress.GetHashCode();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"LogicalGPUHandle #{MemoryAddress.ToInt64()}";
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(LogicalGPUHandle left, LogicalGPUHandle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(LogicalGPUHandle left, LogicalGPUHandle right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public IntPtr MemoryAddress
        {
            get => _MemoryAddress;
        }

        /// <inheritdoc />
        public bool IsNull
        {
            get => _MemoryAddress == IntPtr.Zero;
        }
    }

    /// <inheritdoc cref="IPerformanceStates20VoltageEntry" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PerformanceStates20BaseVoltageEntryV1 : IPerformanceStates20VoltageEntry
    {
        internal PerformanceVoltageDomain _DomainId;
        internal uint _Flags;
        internal uint _Value;
        internal PerformanceStates20ParameterDelta _ValueDelta;

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20BaseVoltageEntryV1" />.
        /// </summary>
        /// <param name="domain">The voltage domain.</param>
        /// <param name="value">The value in micro volt.</param>
        /// <param name="valueDelta">The base value delta.</param>
        public PerformanceStates20BaseVoltageEntryV1(
            PerformanceVoltageDomain domain,
            uint value,
            PerformanceStates20ParameterDelta valueDelta) : this()
        {
            _DomainId = domain;
            _Value = value;
            _ValueDelta = valueDelta;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20BaseVoltageEntryV1" />.
        /// </summary>
        /// <param name="domain">The voltage domain.</param>
        /// <param name="valueDelta">The base value delta.</param>
        public PerformanceStates20BaseVoltageEntryV1(
            PerformanceVoltageDomain domain,
            PerformanceStates20ParameterDelta valueDelta) : this()
        {
            _DomainId = domain;
            _ValueDelta = valueDelta;
        }

        /// <inheritdoc />
        public PerformanceVoltageDomain DomainId
        {
            get => _DomainId;
        }

        /// <inheritdoc />
        public bool IsEditable
        {
            get => _Flags.GetBit(0); 
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <inheritdoc />
        public uint ValueInMicroVolt
        {
            get => _Value;
        }

        /// <inheritdoc />
        public PerformanceStates20ParameterDelta ValueDeltaInMicroVolt
        {
            get => _ValueDelta;
        }
    }

    /// <inheritdoc cref="IPerformanceStates20ClockEntry" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PerformanceStates20ClockEntryV1 : IPerformanceStates20ClockEntry
    {
        internal PublicClockDomain _DomainId;
        internal PerformanceStates20ClockType _ClockType;
        internal uint _Flags;
        internal PerformanceStates20ParameterDelta _FrequencyDeltaInkHz;
        internal PerformanceStates20ClockDependentInfo _ClockDependentInfo;

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20ClockEntryV1" />
        /// </summary>
        /// <param name="domain">The public clock domain.</param>
        /// <param name="valueDelta">The base value delta.</param>
        public PerformanceStates20ClockEntryV1(
            PublicClockDomain domain,
            PerformanceStates20ParameterDelta valueDelta) : this()
        {
            _DomainId = domain;
            _FrequencyDeltaInkHz = valueDelta;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20ClockEntryV1" />
        /// </summary>
        /// <param name="domain">The public clock domain.</param>
        /// <param name="clockType">The type of the clock frequency.</param>
        /// <param name="valueDelta">The base value delta.</param>
        public PerformanceStates20ClockEntryV1(
            PublicClockDomain domain,
            PerformanceStates20ClockType clockType,
            PerformanceStates20ParameterDelta valueDelta) : this(domain, valueDelta)
        {
            _ClockType = clockType;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20ClockEntryV1" />
        /// </summary>
        /// <param name="domain">The public clock domain.</param>
        /// <param name="valueDelta">The base value delta.</param>
        /// <param name="singleFrequency">The clock frequency value.</param>
        // ReSharper disable once TooManyDependencies
        public PerformanceStates20ClockEntryV1(
            PublicClockDomain domain,
            PerformanceStates20ParameterDelta valueDelta,
            PerformanceStates20ClockDependentSingleFrequency singleFrequency) :
            this(domain, PerformanceStates20ClockType.Single, valueDelta)
        {
            _ClockDependentInfo = new PerformanceStates20ClockDependentInfo(singleFrequency);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20ClockEntryV1" />
        /// </summary>
        /// <param name="domain">The public clock domain.</param>
        /// <param name="valueDelta">The base value delta.</param>
        /// <param name="frequencyRange">The clock frequency range value.</param>
        // ReSharper disable once TooManyDependencies
        public PerformanceStates20ClockEntryV1(
            PublicClockDomain domain,
            PerformanceStates20ParameterDelta valueDelta,
            PerformanceStates20ClockDependentFrequencyRange frequencyRange) :
            this(domain, PerformanceStates20ClockType.Range, valueDelta)
        {
            _ClockDependentInfo = new PerformanceStates20ClockDependentInfo(frequencyRange);
        }

        /// <inheritdoc />
        public PublicClockDomain DomainId
        {
            get => _DomainId;
        }

        /// <inheritdoc />
        public PerformanceStates20ClockType ClockType
        {
            get => _ClockType;
        }

        /// <inheritdoc />
        public bool IsEditable
        {
            get => _Flags.GetBit(0); 
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <inheritdoc />
        public PerformanceStates20ParameterDelta FrequencyDeltaInkHz
        {
            get => _FrequencyDeltaInkHz;
        }

        /// <inheritdoc />
        IPerformanceStates20ClockDependentSingleFrequency IPerformanceStates20ClockEntry.SingleFrequency
        {
            get => _ClockDependentInfo._Single;
        }

        /// <inheritdoc />
        IPerformanceStates20ClockDependentFrequencyRange IPerformanceStates20ClockEntry.FrequencyRange
        {
            get => _ClockDependentInfo._Range;
        }

        /// <summary>
        ///     Gets the range of clock frequency and related voltage information if present
        /// </summary>
        public PerformanceStates20ClockDependentSingleFrequency SingleFrequency
        {
            get => _ClockDependentInfo._Single;
        }

        /// <summary>
        ///     Gets the fixed frequency of the clock
        /// </summary>
        public PerformanceStates20ClockDependentFrequencyRange FrequencyRange
        {
            get => _ClockDependentInfo._Range;
        }

        /// <inheritdoc cref="IPerformanceStates20ClockDependentSingleFrequency" />
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PerformanceStates20ClockDependentSingleFrequency :
            IPerformanceStates20ClockDependentSingleFrequency
        {
            internal uint _FrequencyInkHz;

            /// <inheritdoc />
            public uint FrequencyInkHz
            {
                get => _FrequencyInkHz;
            }

            /// <summary>
            ///     Creates a new instance of <see cref="PerformanceStates20ClockDependentSingleFrequency" />.
            /// </summary>
            /// <param name="frequencyInkHz">The fixed frequency in kHz.</param>
            public PerformanceStates20ClockDependentSingleFrequency(uint frequencyInkHz)
            {
                _FrequencyInkHz = frequencyInkHz;
            }
        }

        /// <inheritdoc cref="IPerformanceStates20ClockDependentFrequencyRange" />
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PerformanceStates20ClockDependentFrequencyRange :
            IPerformanceStates20ClockDependentFrequencyRange
        {
            internal uint _MinimumFrequency;
            internal uint _MaximumFrequency;
            internal PerformanceVoltageDomain _VoltageDomainId;
            internal uint _MinimumVoltage;
            internal uint _MaximumVoltage;

            /// <summary>
            ///     Creates a new instance of <see cref="PerformanceStates20ClockDependentFrequencyRange" />.
            /// </summary>
            /// <param name="minimumFrequency">The minimum frequency in kHz.</param>
            /// <param name="maximumFrequency">The maximum frequency in kHz.</param>
            /// <param name="voltageDomainId">The corresponding voltage domain identification number.</param>
            /// <param name="minimumVoltage">The minimum voltage in uV.</param>
            /// <param name="maximumVoltage">The maximum voltage in uV.</param>
            // ReSharper disable once TooManyDependencies
            public PerformanceStates20ClockDependentFrequencyRange(
                uint minimumFrequency,
                uint maximumFrequency,
                PerformanceVoltageDomain voltageDomainId,
                uint minimumVoltage,
                uint maximumVoltage) : this()
            {
                _MinimumFrequency = minimumFrequency;
                _MaximumFrequency = maximumFrequency;
                _VoltageDomainId = voltageDomainId;
                _MinimumVoltage = minimumVoltage;
                _MaximumVoltage = maximumVoltage;
            }

            /// <inheritdoc />
            public uint MinimumFrequencyInkHz
            {
                get => _MinimumFrequency;
            }

            /// <inheritdoc />
            public uint MaximumFrequencyInkHz
            {
                get => _MaximumFrequency;
            }

            /// <inheritdoc />
            public PerformanceVoltageDomain VoltageDomainId
            {
                get => _VoltageDomainId;
            }

            /// <inheritdoc />
            public uint MinimumVoltageInMicroVolt
            {
                get => _MinimumVoltage;
            }

            /// <inheritdoc />
            public uint MaximumVoltageInMicroVolt
            {
                get => _MaximumVoltage;
            }
        }

        [StructLayout(LayoutKind.Explicit, Pack = 8)]
        internal struct PerformanceStates20ClockDependentInfo
        {
            [FieldOffset(0)] internal PerformanceStates20ClockDependentSingleFrequency _Single;
            [FieldOffset(0)] internal PerformanceStates20ClockDependentFrequencyRange _Range;

            public PerformanceStates20ClockDependentInfo(
                PerformanceStates20ClockDependentSingleFrequency singleFrequency
            ) : this()
            {
                _Single = singleFrequency;
            }

            public PerformanceStates20ClockDependentInfo(
                PerformanceStates20ClockDependentFrequencyRange frequencyRange
            ) : this()
            {
                _Range = frequencyRange;
            }
        }
    }

    /// <inheritdoc cref="IPerformanceStates20Info" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PerformanceStates20InfoV1 : IInitializable, IPerformanceStates20Info
    {
        internal const int MaxPerformanceStates = 16;
        internal const int MaxPerformanceStatesClocks = 8;
        internal const int MaxPerformanceStatesBaseVoltages = 4;

        internal StructureVersion _Version;
        internal uint _Flags;
        internal uint _NumberOfPerformanceStates;
        internal uint _NumberOfClocks;
        internal uint _NumberOfBaseVoltages;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStates)]
        internal PerformanceState20[] _PerformanceStates;

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20InfoV1" />
        /// </summary>
        /// <param name="performanceStates">The list of performance states and their settings.</param>
        /// <param name="clocksCount">Number of clock frequencies per each performance state.</param>
        /// <param name="baseVoltagesCount">Number of base voltage per each performance state.</param>
        public PerformanceStates20InfoV1(
            PerformanceState20[] performanceStates,
            uint clocksCount,
            uint baseVoltagesCount)
        {
            if (performanceStates?.Length > MaxPerformanceStatesClocks)
            {
                throw new ArgumentException(
                    $"Maximum of {MaxPerformanceStates} performance states are configurable.",
                    nameof(performanceStates)
                );
            }

            if (performanceStates == null)
            {
                throw new ArgumentNullException(nameof(performanceStates));
            }

            this = typeof(PerformanceStates20InfoV1).Instantiate<PerformanceStates20InfoV1>();
            _NumberOfClocks = clocksCount;
            _NumberOfBaseVoltages = baseVoltagesCount;
            _NumberOfPerformanceStates = (uint)performanceStates.Length;
            Array.Copy(performanceStates, 0, _PerformanceStates, 0, performanceStates.Length);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint NumberOfPerformanceStates
        {
            get => _NumberOfPerformanceStates;
            set => _NumberOfPerformanceStates = value;
        }

        /// <inheritdoc />
        public uint NumberOfClocks
        {
            get => _NumberOfClocks;
            set => _NumberOfClocks = value;
        }

        /// <inheritdoc />
        public uint NumberOfBaseVoltages
        {
            get => _NumberOfBaseVoltages;
            set => _NumberOfBaseVoltages = value;
        }

        /// <inheritdoc />
        public IPerformanceStates20VoltageEntry[] GeneralVoltages
        {
            get => new IPerformanceStates20VoltageEntry[0];
        }

        /// <inheritdoc />
        public bool IsEditable
        {
            get => _Flags.GetBit(0);
        }

        /// <summary>
        ///     Gets an array of valid power states for the GPU
        /// </summary>
        public PerformanceState20[] PerformanceStates
        {
            get => _PerformanceStates.Take((int)_NumberOfPerformanceStates).ToArray();
        }

        /// <inheritdoc />
        IPerformanceState20[] IPerformanceStates20Info.PerformanceStates
        {
            get => PerformanceStates.Cast<IPerformanceState20>().ToArray();
        }

        /// <summary>
        ///     Gets a dictionary for valid power states and their clock frequencies
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceStates20ClockEntryV1[]> Clocks
        {
            get
            {
                var clocks = (int)_NumberOfClocks;

                return PerformanceStates.ToDictionary(
                    state20 => state20.StateId,
                    state20 => state20._Clocks.Take(clocks).ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStates20ClockEntry[]> IPerformanceStates20Info.Clocks
        {
            get
            {
                return Clocks.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStates20ClockEntry>().ToArray()
                );
            }
        }

        /// <summary>
        ///     Gets a dictionary for valid power states and their voltage settings
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceStates20BaseVoltageEntryV1[]> Voltages
        {
            get
            {
                var baseVoltages = (int)_NumberOfBaseVoltages;

                return PerformanceStates.ToDictionary(
                    state20 => state20.StateId,
                    state20 => state20._BaseVoltages.Take(baseVoltages).ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStates20VoltageEntry[]> IPerformanceStates20Info.Voltages
        {
            get
            {
                return Voltages.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStates20VoltageEntry>().ToArray()
                );
            }
        }

        /// <inheritdoc cref="IPerformanceState20" />
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PerformanceState20 : IInitializable, IPerformanceState20
        {
            internal PerformanceStateId _Id;
            internal uint _Flags;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStatesClocks)]
            internal PerformanceStates20ClockEntryV1[] _Clocks;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStatesBaseVoltages)]
            internal PerformanceStates20BaseVoltageEntryV1[] _BaseVoltages;

            /// <summary>
            ///     Creates a new instance of <see cref="PerformanceState20" />.
            /// </summary>
            /// <param name="stateId">The performance identification number.</param>
            /// <param name="clocks">The list of clock entries.</param>
            /// <param name="baseVoltages">The list of base voltages.</param>
            public PerformanceState20(
                PerformanceStateId stateId,
                PerformanceStates20ClockEntryV1[] clocks,
                PerformanceStates20BaseVoltageEntryV1[] baseVoltages)
            {
                if (clocks?.Length > MaxPerformanceStatesClocks)
                {
                    throw new ArgumentException(
                        $"Maximum of {MaxPerformanceStatesClocks} clocks are configurable.",
                        nameof(clocks)
                    );
                }

                if (clocks == null)
                {
                    throw new ArgumentNullException(nameof(clocks));
                }

                if (baseVoltages?.Length > MaxPerformanceStatesBaseVoltages)
                {
                    throw new ArgumentException(
                        $"Maximum of {MaxPerformanceStatesBaseVoltages} base voltages are configurable.",
                        nameof(baseVoltages)
                    );
                }

                if (baseVoltages == null)
                {
                    throw new ArgumentNullException(nameof(baseVoltages));
                }

                this = typeof(PerformanceState20).Instantiate<PerformanceState20>();
                _Id = stateId;
                Array.Copy(clocks, 0, _Clocks, 0, clocks.Length);
                Array.Copy(baseVoltages, 0, _BaseVoltages, 0, baseVoltages.Length);
            }

            /// <inheritdoc />
            public PerformanceStateId StateId
            {
                get => _Id;
            }

            /// <inheritdoc />
            public bool IsEditable
            {
                get => _Flags.GetBit(0); 
                set => _Flags = _Flags.SetBit(0, value);
            }
        }
    }

    /// <inheritdoc cref="IPerformanceStates20Info" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct PerformanceStates20InfoV2 : IInitializable, IPerformanceStates20Info
    {
        internal const int MaxPerformanceStates = PerformanceStates20InfoV1.MaxPerformanceStates;

        internal const int MaxPerformanceStatesBaseVoltages =
            PerformanceStates20InfoV1.MaxPerformanceStatesBaseVoltages;

        internal StructureVersion _Version;
        internal uint _Flags;
        internal uint _NumberOfPerformanceStates;
        internal uint _NumberOfClocks;
        internal uint _NumberOfBaseVoltages;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStates)]
        internal PerformanceStates20InfoV1.PerformanceState20[] _PerformanceStates;

        internal PerformanceStates20OverVoltingSetting _OverVoltingSettings;

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20InfoV2" />
        /// </summary>
        /// <param name="performanceStates">The list of performance states and their settings.</param>
        /// <param name="clocksCount">Number of clock frequencies per each performance state.</param>
        /// <param name="baseVoltagesCount">Number of base voltage per each performance state.</param>
        public PerformanceStates20InfoV2(
            PerformanceStates20InfoV1.PerformanceState20[] performanceStates,
            uint clocksCount,
            uint baseVoltagesCount)
        {
            if (performanceStates?.Length > PerformanceStates20InfoV1.MaxPerformanceStatesClocks)
            {
                throw new ArgumentException(
                    $"Maximum of {MaxPerformanceStates} performance states are configurable.",
                    nameof(performanceStates)
                );
            }

            if (performanceStates == null)
            {
                throw new ArgumentNullException(nameof(performanceStates));
            }

            this = typeof(PerformanceStates20InfoV2).Instantiate<PerformanceStates20InfoV2>();
            _NumberOfClocks = clocksCount;
            _NumberOfBaseVoltages = baseVoltagesCount;
            _NumberOfPerformanceStates = (uint)performanceStates.Length;
            Array.Copy(performanceStates, 0, _PerformanceStates, 0, performanceStates.Length);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20InfoV2" />
        /// </summary>
        /// <param name="performanceStates">The list of performance states and their settings.</param>
        /// <param name="clocksCount">Number of clock frequencies per each performance state.</param>
        /// <param name="baseVoltagesCount">Number of base voltage per each performance state.</param>
        /// <param name="generalVoltages">The list of general voltages and their settings.</param>
        // ReSharper disable once TooManyDependencies
        public PerformanceStates20InfoV2(
            PerformanceStates20InfoV1.PerformanceState20[] performanceStates,
            uint clocksCount,
            uint baseVoltagesCount,
            PerformanceStates20BaseVoltageEntryV1[] generalVoltages) :
            this(performanceStates, clocksCount, baseVoltagesCount)
        {
            _OverVoltingSettings = new PerformanceStates20OverVoltingSetting(generalVoltages);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint NumberOfPerformanceStates
        {
            get => _NumberOfPerformanceStates;
            set => _NumberOfPerformanceStates = value;
        }

        /// <inheritdoc />
        public uint NumberOfClocks
        {
            get => _NumberOfClocks;
            set => _NumberOfClocks = value;
        }

        /// <inheritdoc />
        public uint NumberOfBaseVoltages
        {
            get => _NumberOfBaseVoltages;
            set => _NumberOfBaseVoltages = value;
        }

        /// <summary>
        ///     Gets the list of general over-volting settings
        /// </summary>
        public PerformanceStates20BaseVoltageEntryV1[] GeneralVoltages
        {
            get => _OverVoltingSettings.Voltages.ToArray();
        }

        /// <inheritdoc />
        IPerformanceStates20VoltageEntry[] IPerformanceStates20Info.GeneralVoltages
        {
            get => GeneralVoltages.Cast<IPerformanceStates20VoltageEntry>().ToArray();
        }

        /// <inheritdoc />
        public bool IsEditable
        {
            get => _Flags.GetBit(0); 
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <summary>
        ///     Gets an array of valid power states for the GPU
        /// </summary>
        public PerformanceStates20InfoV1.PerformanceState20[] PerformanceStates
        {
            get => _PerformanceStates.Take((int)_NumberOfPerformanceStates).ToArray();
        }

        /// <inheritdoc />
        IPerformanceState20[] IPerformanceStates20Info.PerformanceStates
        {
            get => PerformanceStates.Cast<IPerformanceState20>().ToArray();
        }

        /// <summary>
        ///     Gets a dictionary for valid power states and their clock frequencies
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceStates20ClockEntryV1[]> Clocks
        {
            get
            {
                var clocks = (int)_NumberOfClocks;

                return PerformanceStates.ToDictionary(
                    state20 => state20.StateId,
                    state20 => state20._Clocks.Take(clocks).ToArray()
                );
            }
        }

        /// <summary>
        ///     Gets a dictionary for valid power states and their voltage settings
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceStates20BaseVoltageEntryV1[]> Voltages
        {
            get
            {
                var baseVoltages = (int)_NumberOfBaseVoltages;

                return PerformanceStates.ToDictionary(
                    state20 => state20.StateId,
                    state20 => state20._BaseVoltages.Take(baseVoltages)
                        .ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStates20ClockEntry[]> IPerformanceStates20Info.Clocks
        {
            get
            {
                return Clocks.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStates20ClockEntry>().ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStates20VoltageEntry[]> IPerformanceStates20Info.Voltages
        {
            get
            {
                return Voltages.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStates20VoltageEntry>().ToArray()
                );
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct PerformanceStates20OverVoltingSetting : IInitializable
        {
            internal uint _NumberOfVoltages;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStatesBaseVoltages)]
            internal PerformanceStates20BaseVoltageEntryV1[] _Voltages;

            public PerformanceStates20OverVoltingSetting(PerformanceStates20BaseVoltageEntryV1[] voltages)
            {
                if (voltages?.Length > PerformanceStates20InfoV1.MaxPerformanceStatesBaseVoltages)
                {
                    throw new ArgumentException(
                        $"Maximum of {MaxPerformanceStatesBaseVoltages} voltages are configurable.",
                        nameof(voltages)
                    );
                }

                if (voltages == null)
                {
                    throw new ArgumentNullException(nameof(voltages));
                }

                this = typeof(PerformanceStates20OverVoltingSetting)
                    .Instantiate<PerformanceStates20OverVoltingSetting>();
                _NumberOfVoltages = (uint)voltages.Length;
                Array.Copy(voltages, 0, _Voltages, 0, voltages.Length);
            }

            public PerformanceStates20BaseVoltageEntryV1[] Voltages
            {
                get => _Voltages.Take((int)_NumberOfVoltages).ToArray();
            }
        }
    }

    /// <inheritdoc cref="IPerformanceStates20Info" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct PerformanceStates20InfoV3 : IInitializable, IPerformanceStates20Info
    {
        internal const int MaxPerformanceStates = PerformanceStates20InfoV2.MaxPerformanceStates;

        internal const int MaxPerformanceStates20BaseVoltages =
            PerformanceStates20InfoV2.MaxPerformanceStatesBaseVoltages;

        internal StructureVersion _Version;
        internal uint _Flags;
        internal uint _NumberOfPerformanceStates;
        internal uint _NumberOfClocks;
        internal uint _NumberOfBaseVoltages;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStates)]
        internal PerformanceStates20InfoV1.PerformanceState20[] _PerformanceStates;

        internal PerformanceStates20InfoV2.PerformanceStates20OverVoltingSetting _OverVoltingSettings;

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20InfoV3" />
        /// </summary>
        /// <param name="performanceStates">The list of performance states and their settings.</param>
        /// <param name="clocksCount">Number of clock frequencies per each performance state.</param>
        /// <param name="baseVoltagesCount">Number of base voltage per each performance state.</param>
        public PerformanceStates20InfoV3(
            PerformanceStates20InfoV1.PerformanceState20[] performanceStates,
            uint clocksCount,
            uint baseVoltagesCount)
        {
            if (performanceStates?.Length > PerformanceStates20InfoV1.MaxPerformanceStatesClocks)
            {
                throw new ArgumentException(
                    $"Maximum of {MaxPerformanceStates} performance states are configurable.",
                    nameof(performanceStates)
                );
            }

            if (performanceStates == null)
            {
                throw new ArgumentNullException(nameof(performanceStates));
            }

            this = typeof(PerformanceStates20InfoV3).Instantiate<PerformanceStates20InfoV3>();
            _NumberOfClocks = clocksCount;
            _NumberOfBaseVoltages = baseVoltagesCount;
            _NumberOfPerformanceStates = (uint)performanceStates.Length;
            Array.Copy(performanceStates, 0, _PerformanceStates, 0, performanceStates.Length);
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20InfoV3" />
        /// </summary>
        /// <param name="performanceStates">The list of performance states and their settings.</param>
        /// <param name="clocksCount">Number of clock frequencies per each performance state.</param>
        /// <param name="baseVoltagesCount">Number of base voltage per each performance state.</param>
        /// <param name="generalVoltages">The list of general voltages and their settings.</param>
        // ReSharper disable once TooManyDependencies
        public PerformanceStates20InfoV3(
            PerformanceStates20InfoV1.PerformanceState20[] performanceStates,
            uint clocksCount,
            uint baseVoltagesCount,
            PerformanceStates20BaseVoltageEntryV1[] generalVoltages) :
            this(performanceStates, clocksCount, baseVoltagesCount)
        {
            _OverVoltingSettings = new PerformanceStates20InfoV2.PerformanceStates20OverVoltingSetting(generalVoltages);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint NumberOfPerformanceStates
        {
            get => _NumberOfPerformanceStates;
            set => _NumberOfPerformanceStates = value;
        }

        /// <inheritdoc />
        public uint NumberOfClocks
        {
            get => _NumberOfClocks;
            set => _NumberOfClocks = value;
        }

        /// <inheritdoc />
        public uint NumberOfBaseVoltages
        {
            get => _NumberOfBaseVoltages;
            set => _NumberOfBaseVoltages = value;
        }

        /// <summary>
        ///     Gets the list of general over-volting settings
        /// </summary>
        public PerformanceStates20BaseVoltageEntryV1[] GeneralVoltages
        {
            get => _OverVoltingSettings.Voltages.ToArray();
        }

        /// <inheritdoc />
        IPerformanceStates20VoltageEntry[] IPerformanceStates20Info.GeneralVoltages
        {
            get => GeneralVoltages.Cast<IPerformanceStates20VoltageEntry>().ToArray();
        }

        /// <inheritdoc />
        public bool IsEditable
        {
            get => _Flags.GetBit(0);
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <summary>
        ///     Gets an array of valid power states for the GPU
        /// </summary>
        public PerformanceStates20InfoV1.PerformanceState20[] PerformanceStates
        {
            get => _PerformanceStates.Take((int)_NumberOfPerformanceStates).ToArray();
        }

        /// <inheritdoc />
        IPerformanceState20[] IPerformanceStates20Info.PerformanceStates
        {
            get => PerformanceStates.Cast<IPerformanceState20>().ToArray();
        }

        /// <summary>
        ///     Gets a dictionary for valid power states and their clock frequencies
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceStates20ClockEntryV1[]> Clocks
        {
            get
            {
                var clocks = (int)_NumberOfClocks;

                return PerformanceStates.ToDictionary(
                    state20 => state20.StateId,
                    state20 => state20._Clocks.Take(clocks).ToArray()
                );
            }
        }

        /// <summary>
        ///     Gets a dictionary for valid power states and their voltage settings
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceStates20BaseVoltageEntryV1[]> Voltages
        {
            get
            {
                var baseVoltages = (int)_NumberOfBaseVoltages;

                return PerformanceStates.ToDictionary(
                    state20 => state20.StateId,
                    state20 => state20._BaseVoltages.Take(baseVoltages)
                        .ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStates20ClockEntry[]> IPerformanceStates20Info.Clocks
        {
            get
            {
                return Clocks.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStates20ClockEntry>().ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStates20VoltageEntry[]> IPerformanceStates20Info.Voltages
        {
            get
            {
                return Voltages.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStates20VoltageEntry>().ToArray()
                );
            }
        }
    }

    /// <summary>
    ///     Hold information regarding delta values and delta ranges for voltages or clock frequencies in their respective unit
    ///     (uV or kHz)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct PerformanceStates20ParameterDelta
    {
        internal int _DeltaValue;
        internal PerformanceState20ParameterDeltaValueRange _DeltaRange;

        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20ParameterDelta" />
        /// </summary>
        /// <param name="deltaValue">The delta value.</param>
        /// <param name="deltaMinimum">The delta range minimum value.</param>
        /// <param name="deltaMaximum">The delta range maximum value.</param>
        public PerformanceStates20ParameterDelta(int deltaValue, int deltaMinimum, int deltaMaximum)
        {
            _DeltaValue = deltaValue;
            _DeltaRange = new PerformanceState20ParameterDeltaValueRange(deltaMinimum, deltaMaximum);
        }


        /// <summary>
        ///     Creates a new instance of <see cref="PerformanceStates20ParameterDelta" />
        /// </summary>
        /// <param name="deltaValue">The delta value.</param>
        public PerformanceStates20ParameterDelta(int deltaValue)
        {
            _DeltaValue = deltaValue;
            _DeltaRange = new PerformanceState20ParameterDeltaValueRange();
        }

        /// <summary>
        ///     Gets the delta value in the respective unit (uV or kHz)
        /// </summary>
        public int DeltaValue
        {
            get => _DeltaValue;
            set => _DeltaValue = value;
        }

        /// <summary>
        ///     Gets the range of the valid delta values in the respective unit (uV or kHz)
        /// </summary>
        public PerformanceState20ParameterDeltaValueRange DeltaRange
        {
            get => _DeltaRange;
        }

        /// <summary>
        ///     Holds information regarding a range of values
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PerformanceState20ParameterDeltaValueRange
        {
            internal int _Minimum;
            internal int _Maximum;

            /// <summary>
            ///     Creates a new instance of <see cref="PerformanceState20ParameterDeltaValueRange" />.
            /// </summary>
            /// <param name="minimum">The minimum value of delta range.</param>
            /// <param name="maximum">The maximum value of delta range.</param>
            public PerformanceState20ParameterDeltaValueRange(int minimum, int maximum)
            {
                _Minimum = minimum;
                _Maximum = maximum;
            }

            /// <summary>
            ///     Gets the minimum value
            /// </summary>
            public int Minimum
            {
                get => _Minimum;
            }

            /// <summary>
            ///     Gets the maximum value
            /// </summary>
            public int Maximum
            {
                get => _Maximum;
            }
        }
    }

    /// <inheritdoc cref="IPerformanceStatesInfo" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PerformanceStatesInfoV1 : IInitializable, IPerformanceStatesInfo
    {
        internal const int MaxPerformanceStates = 16;
        internal const int MaxPerformanceStateClocks = 32;

        internal StructureVersion _Version;
        internal uint _Flags;
        internal uint _NumberOfPerformanceStates;
        internal uint _NumberOfClocks;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStates)]
        internal PerformanceState[] _PerformanceStates;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint NumberOfPerformanceStates
        {
            get => _NumberOfPerformanceStates;
            set => _NumberOfPerformanceStates = value;
        }

        /// <inheritdoc />
        public uint NumberOfClocks
        {
            get => _NumberOfClocks;
            set => _NumberOfClocks = value;
        }

        /// <inheritdoc />
        public bool IsPerformanceMonitorEnable
        {
            get => _Flags.GetBit(0); 
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <inheritdoc />
        public bool IsCapableOfDynamicPerformance
        {
            get => _Flags.GetBit(1);
            set => _Flags = _Flags.SetBit(1, value);
        }

        /// <inheritdoc />
        public bool IsDynamicPerformanceEnable
        {
            get => _Flags.GetBit(2); 
            set => _Flags = _Flags.SetBit(2, value);
        }

        /// <summary>
        ///     Gets an array of valid and available performance states information
        /// </summary>
        public PerformanceState[] PerformanceStates
        {
            get => _PerformanceStates.Take((int)_NumberOfPerformanceStates).ToArray();
        }

        /// <inheritdoc />
        IPerformanceState[] IPerformanceStatesInfo.PerformanceStates
        {
            get => PerformanceStates.Cast<IPerformanceState>().ToArray();
        }

        /// <inheritdoc />
        public IReadOnlyDictionary<PerformanceStateId, IPerformanceStatesVoltage[]> PerformanceStatesVoltages
        {
            get => new ReadOnlyDictionary<PerformanceStateId, IPerformanceStatesVoltage[]>(
                new Dictionary<PerformanceStateId, IPerformanceStatesVoltage[]>()
            );
        }

        /// <summary>
        ///     Gets a dictionary of valid and available performance states and their clock information as an array
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceState.PerformanceStatesClock[]>
            PerformanceStatesClocks
        {
            get
            {
                var clocks = (int)_NumberOfClocks;

                return PerformanceStates.ToDictionary(
                    state => state.StateId,
                    state => state._Clocks.Take(clocks).ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStatesClock[]> IPerformanceStatesInfo.
            PerformanceStatesClocks
        {
            get
            {
                return PerformanceStatesClocks.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStatesClock>().ToArray()
                );
            }
        }

        /// <inheritdoc cref="IPerformanceState" />
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PerformanceState : IInitializable, IPerformanceState
        {
            internal PerformanceStateId _Id;
            internal uint _Flags;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStateClocks)]
            internal PerformanceStatesClock[] _Clocks;

            /// <inheritdoc />
            public PerformanceStateId StateId
            {
                get => _Id;
            }

            /// <inheritdoc />
            public bool IsPCIELimited
            {
                get => _Flags.GetBit(0); 
                set => _Flags = _Flags.SetBit(0, value);
            }

            /// <inheritdoc />
            public bool IsOverclocked
            {
                get => _Flags.GetBit(1); 
                set => _Flags = _Flags.SetBit(1, value);
            }

            /// <inheritdoc />
            public bool IsOverclockable
            {
                get => _Flags.GetBit(2); 
                set => _Flags = _Flags.SetBit(2, value);
            }


            /// <inheritdoc cref="IPerformanceStatesClock" />
            [StructLayout(LayoutKind.Sequential, Pack = 8)]
            public struct PerformanceStatesClock : IInitializable, IPerformanceStatesClock
            {
                internal PublicClockDomain _Id;
                internal uint _Flags;
                internal uint _Frequency;

                /// <inheritdoc />
                public PublicClockDomain DomainId
                {
                    get => _Id;
                }

                /// <inheritdoc />
                public uint Frequency
                {
                    get => _Frequency;
                }
            }
        }
    }

    /// <inheritdoc cref="IPerformanceStatesInfo" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct PerformanceStatesInfoV2 : IInitializable, IPerformanceStatesInfo
    {
        internal const int MaxPerformanceStates = PerformanceStatesInfoV1.MaxPerformanceStates;
        internal const int MaxPerformanceStateClocks = PerformanceStatesInfoV1.MaxPerformanceStateClocks;
        internal const int MaxPerformanceStateVoltages = 16;

        internal StructureVersion _Version;
        internal uint _Flags;
        internal uint _NumberOfPerformanceStates;
        internal uint _NumberOfClocks;
        internal uint _NumberOfVoltages;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStates)]
        internal PerformanceState[] _PerformanceStates;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint NumberOfPerformanceStates
        {
            get => _NumberOfPerformanceStates;
            set => _NumberOfPerformanceStates = value;
        }

        /// <inheritdoc />
        public uint NumberOfClocks
        {
            get => _NumberOfClocks;
            set => _NumberOfClocks = value;
        }


        /// <inheritdoc />
        public bool IsPerformanceMonitorEnable
        {
            get => _Flags.GetBit(0); 
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <inheritdoc />
        public bool IsCapableOfDynamicPerformance
        {
            get => _Flags.GetBit(1); 
            set => _Flags = _Flags.SetBit(1, value);
        }

        /// <inheritdoc />
        public bool IsDynamicPerformanceEnable
        {
            get => _Flags.GetBit(2); 
            set => _Flags = _Flags.SetBit(2, value);
        }

        /// <summary>
        ///     Gets an array of valid and available performance states information
        /// </summary>
        public PerformanceState[] PerformanceStates
        {
            get => _PerformanceStates.Take((int)_NumberOfPerformanceStates).ToArray();
        }

        /// <inheritdoc />
        IPerformanceState[] IPerformanceStatesInfo.PerformanceStates
        {
            get => PerformanceStates.Cast<IPerformanceState>().ToArray();
        }

        /// <summary>
        ///     Gets a dictionary of valid and available performance states and their voltage information as an array
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceState.PerformanceStatesVoltage[]>
            PerformanceStatesVoltages
        {
            get
            {
                var voltages = (int)_NumberOfVoltages;

                return PerformanceStates.ToDictionary(
                    state => state.StateId,
                    state => state._Voltages.Take(voltages).ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStatesVoltage[]> IPerformanceStatesInfo.
            PerformanceStatesVoltages
        {
            get
            {
                return PerformanceStatesVoltages.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStatesVoltage>().ToArray()
                );
            }
        }

        /// <summary>
        ///     Gets a dictionary of valid and available performance states and their clock information as an array
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceState.PerformanceStatesClock[]>
            PerformanceStatesClocks
        {
            get
            {
                var clocks = (int)_NumberOfClocks;

                return PerformanceStates.ToDictionary(
                    state => state.StateId,
                    state => state._Clocks.Take(clocks).ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStatesClock[]> IPerformanceStatesInfo.
            PerformanceStatesClocks
        {
            get
            {
                return PerformanceStatesClocks.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStatesClock>().ToArray()
                );
            }
        }

        /// <inheritdoc cref="IPerformanceState" />
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PerformanceState : IInitializable, IPerformanceState
        {
            internal PerformanceStateId _Id;
            internal uint _Flags;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStateClocks)]
            internal PerformanceStatesClock[] _Clocks;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStateVoltages)]
            internal PerformanceStatesVoltage[] _Voltages;

            /// <inheritdoc />
            public PerformanceStateId StateId
            {
                get => _Id;
            }

            /// <inheritdoc />
            public bool IsPCIELimited
            {
                get => _Flags.GetBit(0); 
                set => _Flags = _Flags.SetBit(0, value);
            }

            /// <inheritdoc />
            public bool IsOverclocked
            {
                get => _Flags.GetBit(1); 
                set => _Flags = _Flags.SetBit(1, value);
            }

            /// <inheritdoc />
            public bool IsOverclockable
            {
                get => _Flags.GetBit(2); 
                set => _Flags = _Flags.SetBit(2, value);
            }

            /// <inheritdoc cref="IPerformanceStatesVoltage" />
            [StructLayout(LayoutKind.Sequential, Pack = 8)]
            public struct PerformanceStatesVoltage : IInitializable, IPerformanceStatesVoltage
            {
                internal PerformanceVoltageDomain _Id;
                internal uint _Flags;
                internal uint _Value;

                /// <inheritdoc />
                public PerformanceVoltageDomain DomainId
                {
                    get => _Id;
                }

                /// <inheritdoc />
                public uint Value
                {
                    get => _Value;
                }
            }

            /// <inheritdoc cref="IPerformanceStatesClock" />
            [StructLayout(LayoutKind.Sequential, Pack = 8)]
            public struct PerformanceStatesClock : IInitializable, IPerformanceStatesClock
            {
                internal PublicClockDomain _Id;
                internal uint _Flags;
                internal uint _Frequency;

                /// <summary>
                ///     Gets a boolean value indicating if this clock domain is overclockable
                /// </summary>
                public bool IsOverclockable
                {
                    get => _Flags.GetBit(0);
                    set => _Flags = _Flags.SetBit(0, value);
                }

                /// <inheritdoc />
                public PublicClockDomain DomainId
                {
                    get => _Id;
                }

                /// <inheritdoc />
                public uint Frequency
                {
                    get => _Frequency;
                }
            }
        }
    }

    /// <inheritdoc cref="IPerformanceStatesInfo" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct PerformanceStatesInfoV3 : IInitializable, IPerformanceStatesInfo
    {
        internal const int MaxPerformanceStates = PerformanceStatesInfoV2.MaxPerformanceStates;
        internal const int MaxPerformanceStateClocks = PerformanceStatesInfoV2.MaxPerformanceStateClocks;
        internal const int MaxPerformanceStateVoltages = PerformanceStatesInfoV2.MaxPerformanceStateVoltages;

        internal StructureVersion _Version;
        internal uint _Flags;
        internal uint _NumberOfPerformanceStates;
        internal uint _NumberOfClocks;
        internal uint _NumberOfVoltages;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxPerformanceStates)]
        internal PerformanceStatesInfoV2.PerformanceState[] _PerformanceStates;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint NumberOfPerformanceStates
        {
            get => _NumberOfPerformanceStates;
            set => _NumberOfPerformanceStates = value;
        }

        /// <inheritdoc />
        public uint NumberOfClocks
        {
            get => _NumberOfClocks;
            set => _NumberOfClocks = value;
        }

        /// <inheritdoc />
        public uint NumberOfVoltages
        {
            get => _NumberOfVoltages;
            set => _NumberOfVoltages = value;
        }

        /// <inheritdoc />
        public bool IsPerformanceMonitorEnable
        {
            get => _Flags.GetBit(0); 
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <inheritdoc />
        public bool IsCapableOfDynamicPerformance
        {
            get => _Flags.GetBit(1); 
            set => _Flags = _Flags.SetBit(1, value);
        }

        /// <inheritdoc />
        public bool IsDynamicPerformanceEnable
        {
            get => _Flags.GetBit(2); 
            set => _Flags = _Flags.SetBit(2, value);
        }

        /// <summary>
        ///     Gets an array of valid and available performance states information
        /// </summary>
        public PerformanceStatesInfoV2.PerformanceState[] PerformanceStates
        {
            get => _PerformanceStates.Take((int)_NumberOfPerformanceStates).ToArray();
        }

        /// <inheritdoc />
        IPerformanceState[] IPerformanceStatesInfo.PerformanceStates
        {
            get => PerformanceStates.Cast<IPerformanceState>().ToArray();
        }

        /// <summary>
        ///     Gets a dictionary of valid and available performance states and their voltage information as an array
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceStatesInfoV2.PerformanceState.PerformanceStatesVoltage
                []>
            PerformanceStatesVoltages
        {
            get
            {
                var voltages = (int)_NumberOfVoltages;

                return PerformanceStates.ToDictionary(
                    state => state.StateId,
                    state => state._Voltages.Take(voltages).ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStatesVoltage[]> IPerformanceStatesInfo.
            PerformanceStatesVoltages
        {
            get
            {
                return PerformanceStatesVoltages.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStatesVoltage>().ToArray()
                );
            }
        }

        /// <summary>
        ///     Gets a dictionary of valid and available performance states and their clock information as an array
        /// </summary>
        public IReadOnlyDictionary<PerformanceStateId, PerformanceStatesInfoV2.PerformanceState.PerformanceStatesClock[]
            >
            PerformanceStatesClocks
        {
            get
            {
                var clocks = (int)_NumberOfClocks;

                return PerformanceStates.ToDictionary(
                    state => state.StateId,
                    state => state._Clocks.Take(clocks).ToArray()
                );
            }
        }

        /// <inheritdoc />
        IReadOnlyDictionary<PerformanceStateId, IPerformanceStatesClock[]> IPerformanceStatesInfo.
            PerformanceStatesClocks
        {
            get
            {
                return PerformanceStatesClocks.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Cast<IPerformanceStatesClock>().ToArray()
                );
            }
        }
    }

    /// <summary>
    ///     PhysicalGPUHandle is a reference to a physical GPU. Each GPU in a multi-GPU board will have its own handle. GPUs
    ///     are assigned a handle even if they are not in use by the OS.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PhysicalGPUHandle : IHandle, IEquatable<PhysicalGPUHandle>
    {
        /// <summary>
        ///     Queryable number of physical GPUs
        /// </summary>
        public const int PhysicalGPUs = 32;

        /// <summary>
        ///     Maximum number of physical GPUs
        /// </summary>
        public const int MaxPhysicalGPUs = 64;

        internal readonly IntPtr _MemoryAddress;

        /// <inheritdoc />
        public bool Equals(PhysicalGPUHandle other)
        {
            return _MemoryAddress.Equals(other._MemoryAddress);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is PhysicalGPUHandle handle && Equals(handle);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _MemoryAddress.GetHashCode();
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"PhysicalGPUHandle #{MemoryAddress.ToInt64()}";
        }

        /// <inheritdoc />
        public IntPtr MemoryAddress
        {
            get => _MemoryAddress;
        }

        /// <inheritdoc />
        public bool IsNull
        {
            get => _MemoryAddress == IntPtr.Zero;
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(PhysicalGPUHandle left, PhysicalGPUHandle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(PhysicalGPUHandle left, PhysicalGPUHandle right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        ///     Gets default PhysicalGPUHandle with a null pointer
        /// </summary>
        public static PhysicalGPUHandle DefaultHandle
        {
            get => default(PhysicalGPUHandle);
        }
    }

    [StructureVersion(2)]
    [StructLayout(LayoutKind.Sequential)]
    public struct PrivateActiveApplicationV2 : IInitializable
    {
        internal const int MaximumNumberOfApplications = 128;

        internal StructureVersion _Version;
        internal uint _ProcessId;
        internal LongString _ProcessName;

        public int ProcessId
        {
            get => (int)_ProcessId;
        }

        public string ProcessName
        {
            get => _ProcessName.Value;
        }
    }

    /// <summary>
    ///     Contains information regarding a GPU architecture
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [StructureVersion(2)]
    public struct PrivateArchitectInfoV2 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _Unknown1;
        internal uint _Unknown2;
        internal uint _Revision;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the GPU revision
        /// </summary>
        public uint Revision
        {
            get => _Revision;
        }
    }

    /// <summary>
    ///     Contains information regarding the GPU clock boost locks
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct PrivateClockBoostLockV2 : IInitializable
    {
        internal const int MaxNumberOfClocksPerGPU = ClockFrequenciesV1.MaxClocksPerGPU;

        internal StructureVersion _Version;
        internal uint _Unknown;
        internal uint _ClockBoostLocksCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfClocksPerGPU)]
        internal ClockBoostLock[] _ClockBoostLocks;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the list of clock boost locks
        /// </summary>
        public ClockBoostLock[] ClockBoostLocks
        {
            get => _ClockBoostLocks.Take((int)_ClockBoostLocksCount).ToArray();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivateClockBoostLockV2" />
        /// </summary>
        /// <param name="clockBoostLocks">The list of clock boost locks</param>
        public PrivateClockBoostLockV2(ClockBoostLock[] clockBoostLocks)
        {
            if (clockBoostLocks?.Length > MaxNumberOfClocksPerGPU)
            {
                throw new ArgumentException($"Maximum of {MaxNumberOfClocksPerGPU} clocks are configurable.",
                    nameof(clockBoostLocks));
            }

            if (clockBoostLocks == null || clockBoostLocks.Length == 0)
            {
                throw new ArgumentException("Array is null or empty.", nameof(clockBoostLocks));
            }

            this = typeof(PrivateClockBoostLockV2).Instantiate<PrivateClockBoostLockV2>();
            _ClockBoostLocksCount = (uint)clockBoostLocks.Length;
            Array.Copy(clockBoostLocks, 0, _ClockBoostLocks, 0, clockBoostLocks.Length);
        }

        /// <summary>
        ///     Contains information regarding a clock boost lock
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ClockBoostLock
        {
            internal PublicClockDomain _ClockDomain;
            internal uint _Unknown1;
            internal ClockLockMode _LockMode;
            internal uint _Unknown2;
            internal uint _VoltageInMicroV;
            internal uint _Unknown3;

            /// <summary>
            ///     Gets the public clock domain
            /// </summary>
            public PublicClockDomain ClockDomain
            {
                get => _ClockDomain;
            }

            /// <summary>
            ///     Gets the clock lock mode
            /// </summary>
            public ClockLockMode LockMode
            {
                get => _LockMode;
            }

            /// <summary>
            ///     Gets the locked voltage in uV
            /// </summary>
            public uint VoltageInMicroV
            {
                get => _VoltageInMicroV;
            }

            /// <summary>
            ///     Creates a new instance of <see cref="ClockBoostLock" />.
            /// </summary>
            /// <param name="clockDomain">The public clock domain.</param>
            /// <param name="lockMode">The clock lock mode.</param>
            /// <param name="voltageInMicroV">The locked voltage in uV.</param>
            public ClockBoostLock(PublicClockDomain clockDomain, ClockLockMode lockMode, uint voltageInMicroV) : this()
            {
                _ClockDomain = clockDomain;
                _LockMode = lockMode;
                _VoltageInMicroV = voltageInMicroV;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU clock boost masks
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateClockBoostMasksV1 : IInitializable
    {
        internal const int MaxMasks = 4;
        internal const int MaxUnknown1 = 8;
        internal const int MaxClockBoostMasks = 103;
        internal const int MaxUnknown2 = 916;

        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxMasks)]
        internal readonly uint[] _Masks;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxUnknown1)]
        internal readonly uint[] _Unknown1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxClockBoostMasks)]
        internal readonly ClockBoostMask[] _ClocksBoostMasks;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxUnknown2)]
        internal readonly uint[] _Unknown2;

        /// <summary>
        ///     Gets a list of clock boost masks
        /// </summary>
        public ClockBoostMask[] ClockBoostMasks
        {
            get => _ClocksBoostMasks;
        }

        /// <summary>
        ///     Contains information regarding a clock boost mask
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ClockBoostMask
        {
            internal readonly uint _Unknown;
            internal readonly uint _Unknown2;
            internal readonly uint _Unknown3;
            internal readonly uint _Unknown4;
            internal readonly int _MemoryDelta;
            internal readonly int _GPUDelta;

            /// <summary>
            ///     Memory clock frequency delta
            /// </summary>
            public int MemoryDelta
            {
                get => _MemoryDelta;
            }

            /// <summary>
            ///     GPU clock frequency delta
            /// </summary>
            public int GPUDelta
            {
                get => _GPUDelta;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU clock boost ranges
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateClockBoostRangesV1 : IInitializable
    {
        internal const int MaxNumberOfClocksPerGPU = ClockFrequenciesV1.MaxClocksPerGPU;
        internal const int MaxNumberOfUnknown = 8;

        internal StructureVersion _Version;
        internal uint _ClockBoostRangesCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown)]
        internal uint[] _Unknown;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfClocksPerGPU)]
        internal ClockBoostRange[] _ClockBoostRanges;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint ClockBoostRangesCount
        {
            get => _ClockBoostRangesCount;
            set => _ClockBoostRangesCount = value;
        }

        /// <summary>
        ///     Gets a list of clock boost ranges
        /// </summary>
        public ClockBoostRange[] ClockBoostRanges
        {
            get => _ClockBoostRanges.Take((int)_ClockBoostRangesCount).ToArray();
        }

        /// <summary>
        ///     Contains information regarding a clock boost range
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ClockBoostRange
        {
            internal uint _Unknown1;
            internal ClockType _ClockType;
            internal uint _Unknown2;
            internal uint _Unknown3;
            internal uint _Unknown4;
            internal uint _Unknown5;
            internal uint _Unknown6;
            internal uint _Unknown7;
            internal uint _Unknown8;
            internal uint _Unknown9;
            internal int _RangeMaximumInkHz;
            internal int _RangeMinimumInkHz;
            internal int _MaximumTemperature;
            internal uint _Unknown10;
            internal uint _Unknown11;
            internal uint _Unknown12;
            internal uint _Unknown13;
            internal uint _Unknown14;

            /// <summary>
            ///     Gets the clock type
            /// </summary>
            public ClockType ClockType
            {
                get => _ClockType;
            }

            /// <summary>
            ///     Gets the maximum boost frequency in kHz
            /// </summary>
            public int MaximumInkHz
            {
                get => _RangeMaximumInkHz;
            }

            /// <summary>
            ///     Gets the minimum boost frequency in kHz
            /// </summary>
            public int MinimumInkHz
            {
                get => _RangeMinimumInkHz;
            }

            /// <summary>
            ///     Gets the maximum boost temperature
            /// </summary>
            public int MaximumTemperature
            {
                get => _MaximumTemperature;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU clock boost table
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateClockBoostTableV1 : IInitializable
    {
        internal const int MaxNumberOfMasks = 4;
        internal const int MaxNumberOfUnknown1 = 12;
        internal const int MaxNumberOfGPUDeltas = 80;
        internal const int MaxNumberOfMemoryFilled = 23;
        internal const int MaxNumberOfMemoryDeltas = 23;
        internal const int MaxNumberOfUnknown2 = 1529;

        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfMasks)]
        internal uint[] _Masks;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown1)]
        internal uint[] _Unknown1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfGPUDeltas)]
        internal GPUDelta[] _GPUDeltas;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfMemoryFilled)]
        internal uint[] _MemoryFilled;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfMemoryDeltas)]
        internal int[] _MemoryDeltas;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown2)]
        internal uint[] _Unknown2;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets a list of clock delta entries
        /// </summary>
        public GPUDelta[] GPUDeltas
        {
            get => _GPUDeltas;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivateClockBoostTableV1" />
        /// </summary>
        /// <param name="gpuDeltas">The list of GPU clock frequency delta entries.</param>
        // ReSharper disable once TooManyDependencies
        public PrivateClockBoostTableV1(GPUDelta[] gpuDeltas)
        {
            if (gpuDeltas?.Length > MaxNumberOfGPUDeltas)
            {
                throw new ArgumentException($"Maximum of {MaxNumberOfGPUDeltas} GPU delta values are configurable.",
                    nameof(gpuDeltas));
            }

            if (gpuDeltas == null)
            {
                throw new ArgumentNullException(nameof(gpuDeltas));
            }

            this = typeof(PrivateClockBoostTableV1).Instantiate<PrivateClockBoostTableV1>();

            Array.Copy(gpuDeltas, 0, _GPUDeltas, 0, gpuDeltas.Length);
        }


        /// <summary>
        ///     Contains information regarding a GPU delta entry in the clock boost table
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct GPUDelta
        {
            internal uint _Unknown1;
            internal uint _Unknown2;
            internal uint _Unknown3;
            internal uint _Unknown4;
            internal uint _Unknown5;
            internal int _FrequencyDeltaInkHz;
            internal uint _Unknown7;
            internal uint _Unknown8;
            internal uint _Unknown9;

            /// <summary>
            ///     Gets the frequency delta in kHz
            /// </summary>
            public int FrequencyDeltaInkHz
            {
                get => _FrequencyDeltaInkHz;
            }

            /// <summary>
            ///     Creates a new instance of GPUDelta.
            /// </summary>
            /// <param name="frequencyDeltaInkHz">The clock frequency in kHz.</param>
            public GPUDelta(int frequencyDeltaInkHz) : this()
            {
                _FrequencyDeltaInkHz = frequencyDeltaInkHz;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU cooler levels
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateCoolerLevelsV1 : IInitializable
    {
        internal const int MaxNumberOfCoolersPerGPU = PrivateCoolerSettingsV1.MaxNumberOfCoolersPerGPU;

        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfCoolersPerGPU)]
        internal CoolerLevel[] _CoolerLevels;

        /// <summary>
        ///     Gets the list of cooler levels.
        /// </summary>
        /// <param name="count">The number of cooler levels to return.</param>
        /// <returns>An array of <see cref="CoolerLevel" /> instances.</returns>
        public CoolerLevel[] GetCoolerLevels(int count)
        {
            return _CoolerLevels.Take(count).ToArray();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivateCoolerLevelsV1" />.
        /// </summary>
        /// <param name="levels">The list of cooler levels.</param>
        public PrivateCoolerLevelsV1(CoolerLevel[] levels)
        {
            if (levels?.Length > MaxNumberOfCoolersPerGPU)
            {
                throw new ArgumentException($"Maximum of {MaxNumberOfCoolersPerGPU} cooler levels are configurable.",
                    nameof(levels));
            }

            if (levels == null || levels.Length == 0)
            {
                throw new ArgumentException("Array is null or empty.", nameof(levels));
            }

            this = typeof(PrivateCoolerLevelsV1).Instantiate<PrivateCoolerLevelsV1>();
            Array.Copy(levels, 0, _CoolerLevels, 0, levels.Length);
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }


        /// <summary>
        ///     Contains information regarding a cooler level
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct CoolerLevel
        {
            internal uint _CurrentLevel;
            internal CoolerPolicy _CurrentPolicy;

            /// <summary>
            ///     Creates a new instance of <see cref="CoolerLevel" />
            /// </summary>
            /// <param name="coolerPolicy">The cooler policy.</param>
            /// <param name="level">The cooler level in percentage.</param>
            public CoolerLevel(CoolerPolicy coolerPolicy, uint level)
            {
                _CurrentPolicy = coolerPolicy;
                _CurrentLevel = level;
            }

            /// <summary>
            ///     Creates a new instance of <see cref="CoolerLevel" />
            /// </summary>
            /// <param name="coolerPolicy">The cooler policy.</param>
            public CoolerLevel(CoolerPolicy coolerPolicy) : this(coolerPolicy, 0)
            {
                if (coolerPolicy == CoolerPolicy.Manual)
                {
                    throw new ArgumentException(
                        "Manual policy is not valid when no level value is provided.",
                        nameof(coolerPolicy)
                    );
                }
            }

            /// <summary>
            ///     Creates a new instance of <see cref="CoolerLevel" />
            /// </summary>
            /// <param name="level">The cooler level in percentage.</param>
            public CoolerLevel(uint level) : this(CoolerPolicy.Manual, level)
            {
            }

            /// <summary>
            ///     Gets the cooler level in percentage.
            /// </summary>
            public uint CurrentLevel
            {
                get => _CurrentLevel;
            }

            /// <summary>
            ///     Gets the cooler policy
            /// </summary>
            public CoolerPolicy CoolerPolicy
            {
                get => _CurrentPolicy;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU cooler policy table
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateCoolerPolicyTableV1 : IInitializable
    {
        internal const int MaxNumberOfPolicyLevels = 24;

        internal StructureVersion _Version;
        internal CoolerPolicy _Policy;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfPolicyLevels)]
        internal CoolerPolicyTableEntry[] _TableEntries;

        /// <summary>
        ///     Gets an array of policy table entries
        /// </summary>
        /// <param name="count">The number of table entries.</param>
        /// <returns>An array of <see cref="CoolerPolicyTableEntry" /> instances.</returns>
        public CoolerPolicyTableEntry[] TableEntries(int count)
        {
            return _TableEntries.Take(count).ToArray();
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the table cooler policy
        /// </summary>
        public CoolerPolicy Policy
        {
            get => _Policy;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivateCoolerPolicyTableV1" />
        /// </summary>
        /// <param name="policy">The table cooler policy.</param>
        /// <param name="policyTableEntries">An array of table entries.</param>
        public PrivateCoolerPolicyTableV1(CoolerPolicy policy, CoolerPolicyTableEntry[] policyTableEntries)
        {
            if (policyTableEntries?.Length > MaxNumberOfPolicyLevels)
            {
                throw new ArgumentException($"Maximum of {MaxNumberOfPolicyLevels} policy levels are configurable.",
                    nameof(policyTableEntries));
            }

            if (policyTableEntries == null || policyTableEntries.Length == 0)
            {
                throw new ArgumentException("Array is null or empty.", nameof(policyTableEntries));
            }

            this = typeof(PrivateCoolerPolicyTableV1).Instantiate<PrivateCoolerPolicyTableV1>();
            _Policy = policy;
            Array.Copy(policyTableEntries, 0, _TableEntries, 0, policyTableEntries.Length);
        }

        /// <summary>
        ///     Contains information regarding a clock boost mask
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct CoolerPolicyTableEntry
        {
            internal uint _EntryId;
            internal uint _CurrentLevel;
            internal uint _DefaultLevel;

            /// <summary>
            ///     Gets the entry identification number
            /// </summary>
            public uint EntryId
            {
                get => _EntryId;
            }

            /// <summary>
            ///     Gets the current level in percentage
            /// </summary>
            public uint CurrentLevel
            {
                get => _CurrentLevel;
            }

            /// <summary>
            ///     Gets the default level in percentage
            /// </summary>
            public uint DefaultLevel
            {
                get => _DefaultLevel;
            }

            /// <summary>
            ///     Creates a new instance of <see cref="CoolerPolicyTableEntry" />.
            /// </summary>
            /// <param name="entryId">The entry identification number.</param>
            /// <param name="currentLevel">The current level in percentage.</param>
            /// <param name="defaultLevel">The default level in percentage.</param>
            public CoolerPolicyTableEntry(uint entryId, uint currentLevel, uint defaultLevel)
            {
                _EntryId = entryId;
                _CurrentLevel = currentLevel;
                _DefaultLevel = defaultLevel;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU cooler settings
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateCoolerSettingsV1 : IInitializable
    {
        internal const int MaxNumberOfCoolersPerGPU = 3;

        internal StructureVersion _Version;
        internal uint _CoolerSettingsCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfCoolersPerGPU)]
        internal CoolerSetting[] _CoolerSettings;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint CoolerSettingsCount
        {
            get => _CoolerSettingsCount;
            set => _CoolerSettingsCount = value;
        }


        /// <summary>
        ///     Gets the list of cooler settings
        /// </summary>
        public CoolerSetting[] CoolerSettings
        {
            get => _CoolerSettings.Take((int)_CoolerSettingsCount).ToArray();
        }

        /// <summary>
        ///     Contains information regarding a cooler settings
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct CoolerSetting
        {
            internal CoolerType _CoolerType;
            internal CoolerController _CoolerController;
            internal uint _DefaultMinimumLevel;
            internal uint _DefaultMaximumLevel;
            internal uint _CurrentMinimumLevel;
            internal uint _CurrentMaximumLevel;
            internal uint _CurrentLevel;
            internal CoolerPolicy _DefaultPolicy;
            internal CoolerPolicy _CurrentPolicy;
            internal CoolerTarget _Target;
            internal CoolerControlMode _ControlMode;
            internal uint _IsActive;

            /// <summary>
            ///     Gets the current cooler level in percentage.
            /// </summary>
            public uint CurrentLevel
            {
                get => _CurrentLevel;
            }

            /// <summary>
            ///     Gets the default minimum cooler level in percentage.
            /// </summary>
            public uint DefaultMinimumLevel
            {
                get => _DefaultMinimumLevel;
            }

            /// <summary>
            ///     Gets the default maximum cooler level in percentage.
            /// </summary>
            public uint DefaultMaximumLevel
            {
                get => _DefaultMaximumLevel;
            }

            /// <summary>
            ///     Gets the current minimum cooler level in percentage.
            /// </summary>
            public uint CurrentMinimumLevel
            {
                get => _CurrentMinimumLevel;
            }

            /// <summary>
            ///     Gets the current maximum cooler level in percentage.
            /// </summary>
            public uint CurrentMaximumLevel
            {
                get => _CurrentMaximumLevel;
            }

            /// <summary>
            ///     Gets the cooler type.
            /// </summary>
            public CoolerType CoolerType
            {
                get => _CoolerType;
            }

            /// <summary>
            ///     Gets the cooler controller.
            /// </summary>
            public CoolerController CoolerController
            {
                get => _CoolerController;
            }

            /// <summary>
            ///     Gets the cooler default policy.
            /// </summary>
            public CoolerPolicy DefaultPolicy
            {
                get => _DefaultPolicy;
            }

            /// <summary>
            ///     Gets the cooler current policy.
            /// </summary>
            public CoolerPolicy CurrentPolicy
            {
                get => _CurrentPolicy;
            }

            /// <summary>
            ///     Gets the cooler target.
            /// </summary>
            public CoolerTarget Target
            {
                get => _Target;
            }

            /// <summary>
            ///     Gets the cooler control mode.
            /// </summary>
            public CoolerControlMode ControlMode
            {
                get => _ControlMode;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateFanCoolersControlV1 : IInitializable
    {
        internal const int MaxNumberOfFanCoolerControlEntries = 32;
        internal StructureVersion _Version;
        internal uint _UnknownUInt;
        internal uint _FanCoolersControlCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
        internal uint[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfFanCoolerControlEntries)]
        internal FanCoolersControlEntry[] _FanCoolersControlEntries;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint FanCoolersControlCount
        {
            get => _FanCoolersControlCount;
            set => _FanCoolersControlCount = value;
        }

        public FanCoolersControlEntry[] FanCoolersControlEntries
        {
            get => _FanCoolersControlEntries.Take((int)_FanCoolersControlCount).ToArray();
        }

        public uint UnknownUInt
        {
            get => _UnknownUInt;
        }

        public PrivateFanCoolersControlV1(FanCoolersControlEntry[] entries, uint unknownUInt = 0)
        {
            if (entries?.Length > MaxNumberOfFanCoolerControlEntries)
            {
                throw new ArgumentException(
                    $"Maximum of {MaxNumberOfFanCoolerControlEntries} coolers are configurable.",
                    nameof(entries));
            }

            if (entries == null || entries.Length == 0)
            {
                throw new ArgumentException("Array is null or empty.", nameof(entries));
            }

            entries = entries.OrderBy(entry => entry.CoolerId).ToArray();

            this = typeof(PrivateFanCoolersControlV1).Instantiate<PrivateFanCoolersControlV1>();
            _UnknownUInt = unknownUInt;
            _FanCoolersControlCount = (uint)entries.Length;
            Array.Copy(entries, 0, _FanCoolersControlEntries, 0, entries.Length);
        }


        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FanCoolersControlEntry
        {
            internal uint _CoolerId;
            internal uint _Level;
            internal FanCoolersControlMode _ControlMode;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
            internal uint[] _Reserved;

            public FanCoolersControlEntry(uint coolerId, FanCoolersControlMode controlMode, uint level)
            {
                this = typeof(FanCoolersControlEntry).Instantiate<FanCoolersControlEntry>();
                _CoolerId = coolerId;
                _ControlMode = controlMode;
                _Level = level;
            }

            public FanCoolersControlEntry(uint coolerId, FanCoolersControlMode controlMode) : this(coolerId,
                controlMode, 0)
            {
                if (controlMode == FanCoolersControlMode.Manual)
                {
                    throw new ArgumentException(
                        "Manual control mode is not valid when no level value is provided.",
                        nameof(controlMode)
                    );
                }
            }

            public uint CoolerId
            {
                get => _CoolerId;
            }

            public uint Level
            {
                get => _Level;
            }

            public FanCoolersControlMode ControlMode
            {
                get => _ControlMode;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateFanCoolersInfoV1 : IInitializable
    {
        internal const int MaxNumberOfFanCoolerInfoEntries = 32;

        internal StructureVersion _Version;
        internal uint _UnknownUInt1;
        internal uint _FanCoolersInfoCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
        internal uint[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfFanCoolerInfoEntries)]
        internal FanCoolersInfoEntry[] _FanCoolersInfoEntries;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint FanCoolersInfoCount
        {
            get => _FanCoolersInfoCount;
            set => _FanCoolersInfoCount = value;
        }

        public FanCoolersInfoEntry[] FanCoolersInfoEntries
        {
            get => _FanCoolersInfoEntries.Take((int)_FanCoolersInfoCount).ToArray();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FanCoolersInfoEntry
        {
            internal uint _CoolerId;
            internal uint _UnknownUInt3;
            internal uint _UnknownUInt4;
            internal uint _MaximumRPM;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
            internal uint[] _Reserved;

            public uint CoolerId
            {
                get => _CoolerId;
            }

            public uint MaximumRPM
            {
                get => _MaximumRPM;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateFanCoolersStatusV1 : IInitializable
    {
        internal const int MaxNumberOfFanCoolerStatusEntries = 32;

        internal StructureVersion _Version;
        internal uint _FanCoolersStatusCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
        internal uint[] _Reserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfFanCoolerStatusEntries)]
        internal FanCoolersStatusEntry[] _FanCoolersStatusEntries;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint FanCoolersStatusCount
        {
            get => _FanCoolersStatusCount;
            set => _FanCoolersStatusCount = value;
        }

        public FanCoolersStatusEntry[] FanCoolersStatusEntries
        {
            get => _FanCoolersStatusEntries.Take((int)_FanCoolersStatusCount).ToArray();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct FanCoolersStatusEntry
        {
            internal readonly uint _CoolerId;
            internal readonly uint _CurrentRPM;
            internal readonly uint _CurrentMinimumLevel;
            internal readonly uint _CurrentMaximumLevel;
            internal readonly uint _CurrentLevel;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8, ArraySubType = UnmanagedType.U4)]
            internal readonly uint[] _Reserved;

            public uint CoolerId
            {
                get => _CoolerId;
            }

            public uint CurrentRPM
            {
                get => _CurrentRPM;
            }

            public uint CurrentMinimumLevel
            {
                get => _CurrentMinimumLevel;
            }

            public uint CurrentMaximumLevel
            {
                get => _CurrentMaximumLevel;
            }

            public uint CurrentLevel
            {
                get => _CurrentLevel;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU PCI-e connection configurations
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct PrivatePCIeInfoV2 : IInitializable
    {
        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        internal readonly PCIePerformanceStateInfo[] _PCIePerformanceStateInfos;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }       

        /// <summary>
        ///     Gets the list of performance state PCI-e configurations information
        /// </summary>
        public PCIePerformanceStateInfo[] PCIePerformanceStateInfos
        {
            get => _PCIePerformanceStateInfos;
        }

        /// <summary>
        ///     Contains information regarding a performance state PCI-e connection
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PCIePerformanceStateInfo
        {
            internal uint _TransferRate;
            internal PCIeGeneration _Version;
            internal uint _LanesNumber;
            internal PCIeGeneration _Generation;

            /// <summary>
            ///     Gets the PCI-e transfer rate in Mega Transfers per Second
            /// </summary>
            public uint TransferRateInMTps
            {
                get => _TransferRate;
            }

            /// <summary>
            ///     Gets the PCI-e generation
            /// </summary>
            public PCIeGeneration Generation
            {
                get => _Generation;
            }

            /// <summary>
            ///     Gets the PCI-e down stream lanes
            /// </summary>
            public uint Lanes
            {
                get => _LanesNumber;
            }

            /// <summary>
            ///     Gets the PCI-e version
            /// </summary>
            public PCIeGeneration Version
            {
                get => _Version;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU performance limitations
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivatePerformanceInfoV1 : IInitializable
    {
        internal const int MaxNumberOfUnknown2 = 16;

        internal StructureVersion _Version;
        internal uint _Unknown1;
        internal PerformanceLimit _SupportedLimits;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown2)]
        internal uint[] _Unknown2;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if performance limit by power usage is supported.
        /// </summary>
        public bool IsPowerLimitSupported
        {
            get => _SupportedLimits.HasFlag(PerformanceLimit.PowerLimit);
        }


        /// <summary>
        ///     Gets a boolean value indicating if performance limit by temperature is supported.
        /// </summary>
        public bool IsTemperatureLimitSupported
        {
            get => _SupportedLimits.HasFlag(PerformanceLimit.TemperatureLimit);
        }


        /// <summary>
        ///     Gets a boolean value indicating if performance limit by voltage usage is supported.
        /// </summary>
        public bool IsVoltageLimitSupported
        {
            get => _SupportedLimits.HasFlag(PerformanceLimit.VoltageLimit);
        }


        /// <summary>
        ///     Gets a boolean value indicating if performance limit by detecting no load is supported.
        /// </summary>
        public bool IsNoLoadLimitSupported
        {
            get => _SupportedLimits.HasFlag(PerformanceLimit.NoLoadLimit);
        }
    }

    /// <summary>
    ///     Contains information regarding GPU performance limitations status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivatePerformanceStatusV1 : IInitializable
    {
        internal const int MaxNumberOfTimers = 3;
        internal const int MaxNumberOfUnknown5 = 326;

        internal StructureVersion _Version;
        internal uint _Unknown1;
        internal ulong _TimerInNanoSecond;
        internal PerformanceLimit _PerformanceLimit;
        internal uint _Unknown2;
        internal uint _Unknown3;
        internal uint _Unknown4;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfTimers)]
        internal ulong[] _TimersInNanoSecond;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown5)]
        internal uint[] _Unknown5;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the current effective performance limitation
        /// </summary>
        public PerformanceLimit PerformanceLimit
        {
            get => _PerformanceLimit;
        }
    }

    /// <summary>
    ///     Contains information regarding GPU power policies
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivatePowerPoliciesInfoV1 : IInitializable
    {
        internal const int MaxNumberOfPowerPolicyInfoEntries = 4;

        internal StructureVersion _Version;
        internal byte _Valid;
        internal byte _PowerPolicyEntriesCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfPowerPolicyInfoEntries)]
        internal PowerPolicyInfoEntry[] _PowerPolicyInfoEntries;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint PowerPolicyEntriesCount
        {
            get => _PowerPolicyEntriesCount;
            set => _PowerPolicyEntriesCount = (byte) value;
        }

        /// <summary>
        ///     Gets a list of power policy entries
        /// </summary>
        public PowerPolicyInfoEntry[] PowerPolicyInfoEntries
        {
            get => _PowerPolicyInfoEntries.Take(_PowerPolicyEntriesCount).ToArray();
        }

        /// <summary>
        ///     Contains information regarding a GPU power policy entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PowerPolicyInfoEntry
        {
            internal PerformanceStateId _StateId;
            internal uint _Unknown1;
            internal uint _Unknown2;
            internal uint _MinimumPower;
            internal uint _Unknown3;
            internal uint _Unknown4;
            internal uint _DefaultPower;
            internal uint _Unknown5;
            internal uint _Unknown6;
            internal uint _MaximumPower;
            internal uint _Unknown7;

            /// <summary>
            ///     Gets the performance state identification number
            /// </summary>
            public PerformanceStateId PerformanceStateId
            {
                get => _StateId;
            }

            /// <summary>
            ///     Gets the minimum power limit in per cent mille
            /// </summary>
            public uint MinimumPowerInPCM
            {
                get => _MinimumPower;
            }

            /// <summary>
            ///     Gets the default power limit in per cent mille
            /// </summary>
            public uint DefaultPowerInPCM
            {
                get => _DefaultPower;
            }

            /// <summary>
            ///     Gets the maximum power limit in per cent mille
            /// </summary>
            public uint MaximumPowerInPCM
            {
                get => _MaximumPower;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU power policies status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivatePowerPoliciesStatusV1 : IInitializable
    {
        internal const int MaxNumberOfPowerPoliciesStatusEntries = 4;

        internal StructureVersion _Version;
        internal uint _PowerPoliciesStatusEntriesCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfPowerPoliciesStatusEntries,
            ArraySubType = UnmanagedType.Struct)]
        internal PowerPolicyStatusEntry[] _PowerPoliciesStatusEntries;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint PowerPoliciesStatusEntriesCount
        {
            get => _PowerPoliciesStatusEntriesCount;
            set => _PowerPoliciesStatusEntriesCount = value;
        }

        /// <summary>
        ///     Gets a list of power policy status entries
        /// </summary>
        public PowerPolicyStatusEntry[] PowerPolicyStatusEntries
        {
            get => _PowerPoliciesStatusEntries.Take((int)_PowerPoliciesStatusEntriesCount).ToArray();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivatePowerPoliciesStatusV1" />
        /// </summary>
        /// <param name="powerPoliciesStatusEntries">The list of power policy status entries.</param>
        public PrivatePowerPoliciesStatusV1(PowerPolicyStatusEntry[] powerPoliciesStatusEntries)
        {
            if (powerPoliciesStatusEntries?.Length > MaxNumberOfPowerPoliciesStatusEntries)
            {
                throw new ArgumentException(
                    $"Maximum of {MaxNumberOfPowerPoliciesStatusEntries} power policies entries are configurable.",
                    nameof(powerPoliciesStatusEntries)
                );
            }

            if (powerPoliciesStatusEntries == null || powerPoliciesStatusEntries.Length == 0)
            {
                throw new ArgumentException("Array is null or empty.", nameof(powerPoliciesStatusEntries));
            }

            this = typeof(PrivatePowerPoliciesStatusV1).Instantiate<PrivatePowerPoliciesStatusV1>();
            _PowerPoliciesStatusEntriesCount = (uint)powerPoliciesStatusEntries.Length;
            Array.Copy(
                powerPoliciesStatusEntries,
                0,
                _PowerPoliciesStatusEntries,
                0,
                powerPoliciesStatusEntries.Length
            );
        }

        /// <summary>
        ///     Contains information regarding a power policies status entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PowerPolicyStatusEntry
        {
            internal PerformanceStateId _PerformanceStateId;
            internal uint _Unknown1;
            internal uint _PowerTargetInPCM;
            internal uint _Unknown2;

            /// <summary>
            ///     Gets the performance state identification number
            /// </summary>
            public PerformanceStateId PerformanceStateId
            {
                get => _PerformanceStateId;
            }

            /// <summary>
            ///     Creates a new instance of PowerPolicyStatusEntry.
            /// </summary>
            /// <param name="powerTargetInPCM">The power limit target in per cent mille.</param>
            public PowerPolicyStatusEntry(uint powerTargetInPCM) : this()
            {
                _PowerTargetInPCM = powerTargetInPCM;
            }

            /// <summary>
            ///     Gets the power limit target in per cent mille
            /// </summary>
            public uint PowerTargetInPCM
            {
                get => _PowerTargetInPCM;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU power topology status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivatePowerTopologiesStatusV1 : IInitializable
    {
        internal const int MaxNumberOfPowerTopologiesStatusEntries = 4;

        internal StructureVersion _Version;
        internal uint _PowerTopologiesStatusEntriesCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfPowerTopologiesStatusEntries,
            ArraySubType = UnmanagedType.Struct)]
        internal PowerTopologiesStatusEntry[] _PowerTopologiesStatusEntries;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint PowerTopologiesStatusEntriesCount
        {
            get => _PowerTopologiesStatusEntriesCount;
            set => _PowerTopologiesStatusEntriesCount = value;
        }

        /// <summary>
        ///     Gets a list of power topology status entries
        /// </summary>
        public PowerTopologiesStatusEntry[] PowerPolicyStatusEntries
        {
            get => _PowerTopologiesStatusEntries.Take((int)_PowerTopologiesStatusEntriesCount).ToArray();
        }

        /// <summary>
        ///     Contains information regarding a power topology status entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct PowerTopologiesStatusEntry
        {
            internal PowerTopologyDomain _Domain;
            internal uint _Unknown2;
            internal uint _PowerUsageInPCM;
            internal uint _Unknown3;

            /// <summary>
            ///     Gets the power topology domain
            /// </summary>
            public PowerTopologyDomain Domain
            {
                get => _Domain;
            }

            /// <summary>
            ///     Gets the power usage in per cent mille
            /// </summary>
            public uint PowerUsageInPCM
            {
                get => _PowerUsageInPCM;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU thermal policies
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct PrivateThermalPoliciesInfoV2 : IInitializable
    {
        internal const int MaxNumberOfThermalPoliciesInfoEntries = 4;

        internal StructureVersion _Version;
        internal byte _ThermalPoliciesInfoCount;
        internal byte _Unknown;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfThermalPoliciesInfoEntries,
            ArraySubType = UnmanagedType.Struct)]
        internal ThermalPoliciesInfoEntry[] _ThermalPoliciesInfoEntries;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public byte ThermalPoliciesInfoCount
        {
            get => _ThermalPoliciesInfoCount;
            set => _ThermalPoliciesInfoCount = value;
        }

        /// <summary>
        ///     Gets a list of thermal policy entries
        /// </summary>
        public ThermalPoliciesInfoEntry[] ThermalPoliciesInfoEntries
        {
            get => _ThermalPoliciesInfoEntries.Take(_ThermalPoliciesInfoCount).ToArray();
        }

        /// <summary>
        ///     Contains information regarding a thermal policies entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ThermalPoliciesInfoEntry
        {
            internal ThermalController _Controller;
            internal uint _Unknown1;
            internal int _MinimumTemperature;
            internal int _DefaultTemperature;
            internal int _MaximumTemperature;
            internal uint _Unknown2;

            /// <summary>
            ///     Gets the thermal controller
            /// </summary>
            public ThermalController Controller
            {
                get => _Controller;
            }

            /// <summary>
            ///     Gets the minimum temperature limit target
            /// </summary>
            public int MinimumTemperature
            {
                get => _MinimumTemperature >> 8;
            }

            /// <summary>
            ///     Gets the default temperature limit target
            /// </summary>
            public int DefaultTemperature
            {
                get => _DefaultTemperature >> 8;
            }

            /// <summary>
            ///     Gets the maximum temperature limit target
            /// </summary>
            public int MaximumTemperature
            {
                get => _MaximumTemperature >> 8;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU thermal policies status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct PrivateThermalPoliciesStatusV2 : IInitializable
    {
        internal const int MaxNumberOfThermalPoliciesStatusEntries = 4;

        internal StructureVersion _Version;
        internal uint _ThermalPoliciesStatusEntriesCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfThermalPoliciesStatusEntries)]
        internal ThermalPoliciesStatusEntry[] _ThermalPoliciesStatusEntries;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint ThermalPoliciesStatusEntriesCount
        {
            get => _ThermalPoliciesStatusEntriesCount;
            set => _ThermalPoliciesStatusEntriesCount = value;
        }

        /// <summary>
        ///     Gets a list of thermal policy status entries
        /// </summary>
        public ThermalPoliciesStatusEntry[] ThermalPoliciesStatusEntries
        {
            get => _ThermalPoliciesStatusEntries.Take((int)_ThermalPoliciesStatusEntriesCount).ToArray();
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivateThermalPoliciesStatusV2" />
        /// </summary>
        /// <param name="policiesStatusEntries">The list of thermal policy status entries</param>
        public PrivateThermalPoliciesStatusV2(ThermalPoliciesStatusEntry[] policiesStatusEntries)
        {
            if (policiesStatusEntries?.Length > MaxNumberOfThermalPoliciesStatusEntries)
            {
                throw new ArgumentException(
                    $"Maximum of {MaxNumberOfThermalPoliciesStatusEntries} thermal policies entries are configurable.",
                    nameof(policiesStatusEntries)
                );
            }

            if (policiesStatusEntries == null || policiesStatusEntries.Length == 0)
            {
                throw new ArgumentException("Array is null or empty.", nameof(policiesStatusEntries));
            }

            this = typeof(PrivateThermalPoliciesStatusV2).Instantiate<PrivateThermalPoliciesStatusV2>();
            _ThermalPoliciesStatusEntriesCount = (uint)policiesStatusEntries.Length;
            Array.Copy(
                policiesStatusEntries,
                0,
                _ThermalPoliciesStatusEntries,
                0,
                policiesStatusEntries.Length
            );
        }

        /// <summary>
        ///     Contains information regarding a thermal policies status entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct ThermalPoliciesStatusEntry
        {
            internal ThermalController _Controller;
            internal int _TargetTemperature;
            internal PerformanceStateId _PerformanceStateId;

            /// <summary>
            ///     Creates a new instance of <see cref="ThermalPoliciesStatusEntry" />
            /// </summary>
            /// <param name="controller">The thermal controller</param>
            /// <param name="targetTemperature">The target temperature.</param>
            public ThermalPoliciesStatusEntry(ThermalController controller, int targetTemperature) : this()
            {
                _Controller = controller;
                _TargetTemperature = targetTemperature * 256;
            }

            /// <summary>
            ///     Creates a new instance of <see cref="ThermalPoliciesStatusEntry" />
            /// </summary>
            /// <param name="performanceStateId">The performance state identification number</param>
            /// <param name="controller">The thermal controller</param>
            /// <param name="targetTemperature">The target temperature.</param>
            public ThermalPoliciesStatusEntry(
                PerformanceStateId performanceStateId,
                ThermalController controller,
                int targetTemperature) : this(controller, targetTemperature)
            {
                _PerformanceStateId = performanceStateId;
            }

            /// <summary>
            ///     Gets the thermal controller
            /// </summary>
            public ThermalController Controller
            {
                get => _Controller;
            }

            /// <summary>
            ///     Gets the performance state identification number
            /// </summary>
            public PerformanceStateId PerformanceStateId
            {
                get => _PerformanceStateId;
            }

            /// <summary>
            ///     Gets the target temperature
            /// </summary>
            public int TargetTemperature
            {
                get => _TargetTemperature >> 8;
            }
        }
    }

    /// <summary>
    ///     Holds information about the GPU usage statistics
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateUsagesInfoV1 : IInitializable, IUtilizationStatus
    {
        internal const int MaxNumberOfUsageEntries = DynamicPerformanceStatesInfoV1.MaxGpuUtilizations;

        internal StructureVersion _Version;
        internal uint _Unknown;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUsageEntries, ArraySubType = UnmanagedType.Struct)]
        internal UsagesInfoEntry[] _UsagesInfoEntries;

        /// <inheritdoc />
        public Dictionary<UtilizationDomain, IUtilizationDomainInfo> Domains
        {
            get => _UsagesInfoEntries
                .Select((value, index) => new { index, value })
                .Where(arg => Enum.IsDefined(typeof(UtilizationDomain), arg.index) && arg.value.IsPresent)
                .ToDictionary(arg => (UtilizationDomain)arg.index, arg => arg.value as IUtilizationDomainInfo);
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public IUtilizationDomainInfo GPU
        {
            get => _UsagesInfoEntries[(int)UtilizationDomain.GPU];
        }

        /// <inheritdoc />
        public IUtilizationDomainInfo FrameBuffer
        {
            get => _UsagesInfoEntries[(int)UtilizationDomain.FrameBuffer];
        }

        /// <inheritdoc />
        public IUtilizationDomainInfo VideoEngine
        {
            get => _UsagesInfoEntries[(int)UtilizationDomain.VideoEngine];
        }

        /// <inheritdoc />
        public IUtilizationDomainInfo BusInterface
        {
            get => _UsagesInfoEntries[(int)UtilizationDomain.BusInterface];
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"GPU = {GPU} - " +
                   $"FrameBuffer = {FrameBuffer} - " +
                   $"VideoEngine = {VideoEngine} - " +
                   $"BusInterface = {BusInterface}";
        }

        /// <summary>
        ///     Holds information about the usage statistics for a domain
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct UsagesInfoEntry : IUtilizationDomainInfo
        {
            internal uint _IsPresent;
            internal uint _Percentage;
            internal uint _Unknown1;
            internal uint _Unknown2;

            /// <inheritdoc />
            public bool IsPresent
            {
                get => _IsPresent > 0;
            }

            /// <inheritdoc />
            public uint Percentage
            {
                get => _Percentage;
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return IsPresent ? $"{Percentage}%" : "N/A";
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU boost frequency curve
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateVFPCurveV1 : IInitializable
    {
        internal const int MaxNumberOfMasks = 4;
        internal const int MaxNumberOfUnknown1 = 12;
        internal const int MaxNumberOfGPUCurveEntries = 80;
        internal const int MaxNumberOfMemoryCurveEntries = 23;
        internal const int MaxNumberOfUnknown2 = 1064;

        internal StructureVersion _Version;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfMasks)]
        internal uint[] _Masks;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown1)]
        internal uint[] _Unknown1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfGPUCurveEntries)]
        internal VFPCurveEntry[] _GPUCurveEntries;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfMemoryCurveEntries)]
        internal VFPCurveEntry[] _MemoryCurveEntries;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown2)]
        internal uint[] _Unknown2;

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }
      
        /// <summary>
        ///     Gets the list of GPU curve entries
        /// </summary>
        public VFPCurveEntry[] GPUCurveEntries
        {
            get => _GPUCurveEntries;
        }

        /// <summary>
        ///     Gets the list of memory curve entries
        /// </summary>
        public VFPCurveEntry[] MemoryCurveEntries
        {
            get => _MemoryCurveEntries;
        }

        /// <summary>
        ///     Contains information regarding a boost frequency curve entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct VFPCurveEntry
        {
            internal uint _Unknown1;
            internal uint _FrequencyInkHz;
            internal uint _VoltageInMicroV;
            internal uint _Unknown2;
            internal uint _Unknown3;
            internal uint _Unknown4;
            internal uint _Unknown5;

            /// <summary>
            ///     Gets the frequency in kHz
            /// </summary>
            public uint FrequencyInkHz
            {
                get => _FrequencyInkHz;
            }

            /// <summary>
            ///     Gets the voltage in uV
            /// </summary>
            public uint VoltageInMicroV
            {
                get => _VoltageInMicroV;
            }
        }
    }

    /// <summary>
    ///     Contains information regarding GPU voltage boost percentage
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateVoltageBoostPercentV1 : IInitializable
    {
        internal const int MaxNumberOfUnknown = 8;

        internal StructureVersion _Version;

        internal readonly uint _Percent;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown)]
        internal readonly uint[] _Unknown;

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the voltage boost in percentage
        /// </summary>
        public uint Percent
        {
            get => _Percent;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="PrivateVoltageBoostPercentV1" />
        /// </summary>
        /// <param name="percent">The voltage boost in percentage</param>
        public PrivateVoltageBoostPercentV1(uint percent)
        {
            this = typeof(PrivateVoltageBoostPercentV1).Instantiate<PrivateVoltageBoostPercentV1>();
            _Percent = percent;
        }
    }

    /// <summary>
    ///     Contains information regarding GPU voltage boost status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct PrivateVoltageStatusV1 : IInitializable
    {
        internal const int MaxNumberOfUnknown2 = 8;
        internal const int MaxNumberOfUnknown3 = 8;

        internal StructureVersion _Version;

        internal readonly uint _Unknown1;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown2)]
        internal readonly uint[] _Unknown2;

        internal readonly uint _ValueInuV;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxNumberOfUnknown3)]
        internal readonly uint[] _Unknown3;

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the value in uV
        /// </summary>
        public uint ValueInMicroVolt
        {
            get => _ValueInuV;
        }
    }

    /// <summary>
    ///     Holds necessary information to get an illumination attribute support status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct QueryIlluminationSupportParameterV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal PhysicalGPUHandle _GPUHandle;
        internal IlluminationAttribute _Attribute;
        internal uint _IsSupported;

        /// <summary>
        ///     Creates a new instance of <see cref="QueryIlluminationSupportParameterV1" />.
        /// </summary>
        /// <param name="gpuHandle">The physical gpu handle.</param>
        /// <param name="attribute">The attribute.</param>
        public QueryIlluminationSupportParameterV1(PhysicalGPUHandle gpuHandle, IlluminationAttribute attribute)
        {
            this = typeof(QueryIlluminationSupportParameterV1).Instantiate<QueryIlluminationSupportParameterV1>();
            _GPUHandle = gpuHandle;
            _Attribute = attribute;
        }

        /// <summary>
        ///     Gets the parameter physical gpu handle
        /// </summary>
        public PhysicalGPUHandle PhysicalGPUHandle
        {
            get => _GPUHandle;
        }

        /// <summary>
        ///     Gets the parameter attribute
        /// </summary>
        public IlluminationAttribute Attribute
        {
            get => _Attribute;
        }

        /// <summary>
        ///     Gets a boolean value indicating if this attribute is supported and controllable via this GPU
        /// </summary>
        public bool IsSupported
        {
            get => _IsSupported > 0;
        }
    }

    /// <summary>
    ///     Holds necessary information to set an illumination attribute value
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct SetIlluminationParameterV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal PhysicalGPUHandle _GPUHandle;
        internal IlluminationAttribute _Attribute;
        internal uint _ValueInPercentage;

        /// <summary>
        ///     Creates a new instance of <see cref="SetIlluminationParameterV1" />.
        /// </summary>
        /// <param name="gpuHandle">The physical gpu handle.</param>
        /// <param name="attribute">The attribute.</param>
        /// <param name="valueInPercentage">The attribute value in percentage.</param>
        public SetIlluminationParameterV1(
            PhysicalGPUHandle gpuHandle,
            IlluminationAttribute attribute,
            uint valueInPercentage)
        {
            this = typeof(SetIlluminationParameterV1).Instantiate<SetIlluminationParameterV1>();
            _GPUHandle = gpuHandle;
            _Attribute = attribute;
            _ValueInPercentage = valueInPercentage;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets the parameter physical gpu handle
        /// </summary>
        public PhysicalGPUHandle PhysicalGPUHandle
        {
            get => _GPUHandle;
        }

        /// <summary>
        ///     Gets the parameter attribute
        /// </summary>
        public IlluminationAttribute Attribute
        {
            get => _Attribute;
        }

        /// <summary>
        ///     Gets the parameter value in percentage
        /// </summary>
        public uint ValueInPercentage
        {
            get => _ValueInPercentage;
        }
    }

    /// <summary>
    ///     Holds a list of thermal sensor information settings (temperature values)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct ThermalSettingsV1 : IInitializable, IThermalSettings
    {
        internal const int MaxThermalSensorsPerGPU = 3;

        internal StructureVersion _Version;
        internal uint _Count;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxThermalSensorsPerGPU)]
        internal ThermalSensor[] _Sensors;

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint Count
        {
            get => _Count;
            set => _Count = value;
        }

        /// <inheritdoc />
        public IThermalSensor[] Sensors
        {
            get => _Sensors.Take((int)_Count).Cast<IThermalSensor>().ToArray();
        }

        /// <summary>
        ///     Holds information about a single thermal sensor
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ThermalSensor : IThermalSensor
        {
            internal ThermalController _Controller;
            internal uint _DefaultMinTemp;
            internal uint _DefaultMaxTemp;
            internal uint _CurrentTemp;
            internal ThermalSettingsTarget _Target;

            /// <inheritdoc />
            public ThermalController Controller
            {
                get => _Controller;
            }

            /// <inheritdoc />
            public int DefaultMinimumTemperature
            {
                get => (int)_DefaultMinTemp;
            }

            /// <inheritdoc />
            public int DefaultMaximumTemperature
            {
                get => (int)_DefaultMaxTemp;
            }

            /// <inheritdoc />
            public int CurrentTemperature
            {
                get => (int)_CurrentTemp;
            }

            /// <inheritdoc />
            public ThermalSettingsTarget Target
            {
                get => _Target;
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return
                    $"[{Target} @ {Controller}] Current: {CurrentTemperature}°C - Default Range: [({DefaultMinimumTemperature}°C) , ({DefaultMaximumTemperature}°C)]";
            }
        }
    }

    /// <summary>
    ///     Holds a list of thermal sensor information settings (temperature values)
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct ThermalSettingsV2 : IInitializable, IThermalSettings
    {
        internal StructureVersion _Version;
        internal uint _Count;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ThermalSettingsV1.MaxThermalSensorsPerGPU)]
        internal ThermalSensor[] _Sensors;

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint Count
        {
            get => _Count;
            set => _Count = value;
        }

        /// <inheritdoc />
        public IThermalSensor[] Sensors
        {
            get => _Sensors.Take((int)_Count).Cast<IThermalSensor>().ToArray();
        }

        /// <summary>
        ///     Holds information about a single thermal sensor
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct ThermalSensor : IThermalSensor
        {
            internal ThermalController _Controller;
            internal int _DefaultMinTemp;
            internal int _DefaultMaxTemp;
            internal int _CurrentTemp;
            internal ThermalSettingsTarget _Target;

            /// <inheritdoc />
            public ThermalController Controller
            {
                get => _Controller;
            }

            /// <inheritdoc />
            public int DefaultMinimumTemperature
            {
                get => _DefaultMinTemp;
            }

            /// <inheritdoc />
            public int DefaultMaximumTemperature
            {
                get => _DefaultMaxTemp;
            }

            /// <inheritdoc />
            public int CurrentTemperature
            {
                get => _CurrentTemp;
            }

            /// <inheritdoc />
            public ThermalSettingsTarget Target
            {
                get => _Target;
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return
                    $"[{Target} @ {Controller}] Current: {CurrentTemperature}°C - Default Range: [({DefaultMinimumTemperature}°C) , ({DefaultMaximumTemperature}°C)]";
            }
        }
    }

}
