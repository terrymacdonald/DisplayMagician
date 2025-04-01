using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static DisplayMagicianShared.NVIDIA.DisplayTopologyStatus;

namespace DisplayMagicianShared.NVIDIA
{
    /// <summary>
    ///     Possible display problems in a topology validation process
    /// </summary>
    [Flags]
    public enum DisplayCapacityProblem : uint
    {
        /// <summary>
        ///     No problem
        /// </summary>
        NoProblem = 0,

        /// <summary>
        ///     Display is connected to the wrong GPU
        /// </summary>
        DisplayOnInvalidGPU = 1,

        /// <summary>
        ///     Display is connected to the wrong connector
        /// </summary>
        DisplayOnWrongConnector = 2,

        /// <summary>
        ///     Timing configuration is missing
        /// </summary>
        NoCommonTimings = 4,

        /// <summary>
        ///     EDID information is missing
        /// </summary>
        NoEDIDAvailable = 8,

        /// <summary>
        ///     Output type combination is not valid
        /// </summary>
        MismatchedOutputType = 16,

        /// <summary>
        ///     There is no display connected
        /// </summary>
        NoDisplayConnected = 32,

        /// <summary>
        ///     GPU is missing
        /// </summary>
        NoGPUTopology = 64,

        /// <summary>
        ///     Not supported
        /// </summary>
        NotSupported = 128,

        /// <summary>
        ///     SLI Bridge is missing
        /// </summary>
        NoSLIBridge = 256,

        /// <summary>
        ///     ECC is enable
        /// </summary>
        ECCEnabled = 512,

        /// <summary>
        ///     Topology is not supported by GPU
        /// </summary>
        GPUTopologyNotSupported = 1024
    }

    /// <summary>
    ///     Possible display problems in a topology validation process
    /// </summary>
    [Flags]
    public enum DisplayTopologyWarning : uint
    {
        /// <summary>
        ///     No warning
        /// </summary>
        NoWarning = 0,

        /// <summary>
        ///     Display position is problematic
        /// </summary>
        DisplayPosition = 1,

        /// <summary>
        ///     Driver reload is required for this changes
        /// </summary>
        DriverReloadRequired = 2
    }

    /// <summary>
    ///     Possible pixel shift types for a display
    /// </summary>
    public enum PixelShiftType
    {
        /// <summary>
        ///     No pixel shift will be applied to this display.
        /// </summary>
        NoPixelShift = 0,

        /// <summary>
        ///     This display will be used to scan-out top left pixels in 2x2 PixelShift configuration
        /// </summary>
        TopLeft2X2Pixels = 1,

        /// <summary>
        ///     This display will be used to scan-out bottom right pixels in 2x2 PixelShift configuration
        /// </summary>
        BottomRight2X2Pixels = 2
    }

    /// <summary>
    ///     Possible flags for setting a display topology
    /// </summary>
    [Flags]
    public enum SetDisplayTopologyFlag : uint
    {
        /// <summary>
        ///     No special flag
        /// </summary>
        NoFlag = 0,

        /// <summary>
        ///     Do not change the current GPU topology. If the NO_DRIVER_RELOAD bit is not specified, then it may still require a
        ///     driver reload.
        /// </summary>
        CurrentGPUTopology = 1,

        /// <summary>
        ///     Do not allow a driver reload. That is, stick with the same master GPU as well as the same SLI configuration.
        /// </summary>
        NoDriverReload = 2,

        /// <summary>
        ///     When choosing a GPU topology, choose the topology with the best performance.
        ///     Without this flag, it will choose the topology that uses the smallest number of GPUs.
        /// </summary>
        MaximizePerformance = 4,

        /// <summary>
        ///     Do not return an error if no configuration will work with all of the grids.
        /// </summary>
        AllowInvalid = 8
    }

    /// <summary>
    ///     Complete list of supported Mosaic topologies.
    ///     Using a "Basic" topology combines multiple monitors to create a single desktop.
    ///     Using a "Passive" topology combines multiples monitors to create a passive stereo desktop.
    ///     In passive stereo, two identical topologies combine - one topology is used for the right eye and the other
    ///     identical topology (targeting different displays) is used for the left eye.
    /// </summary>
    public enum Topology
    {
        /// <summary>
        ///     Not a Mosaic Topology
        /// </summary>
        None = 0,

        // Basic_Begin = 1,

        /// <summary>
        ///     1x2 Basic Topology Configuration
        /// </summary>
        Basic_1X2 = 1,

        /// <summary>
        ///     2x1 Basic Topology Configuration
        /// </summary>
        Basic_2X1 = 2,

        /// <summary>
        ///     1x3 Basic Topology Configuration
        /// </summary>
        Basic_1X3 = 3,

        /// <summary>
        ///     3x1 Basic Topology Configuration
        /// </summary>
        Basic_3X1 = 4,

        /// <summary>
        ///     4x1 Basic Topology Configuration
        /// </summary>
        Basic_1X4 = 5,

        /// <summary>
        ///     4x1 Basic Topology Configuration
        /// </summary>
        Basic_4X1 = 6,

        /// <summary>
        ///     2x2 Basic Topology Configuration
        /// </summary>
        Basic_2X2 = 7,

        /// <summary>
        ///     2x3 Basic Topology Configuration
        /// </summary>
        Basic_2X3 = 8,

        /// <summary>
        ///     2x4 Basic Topology Configuration
        /// </summary>
        Basic_2X4 = 9,

        /// <summary>
        ///     3x2 Basic Topology Configuration
        /// </summary>
        Basic_3X2 = 10,

        /// <summary>
        ///     4x2 Basic Topology Configuration
        /// </summary>
        Basic_4X2 = 11,

        /// <summary>
        ///     1x5 Basic Topology Configuration
        /// </summary>
        Basic_1X5 = 12,

        /// <summary>
        ///     1x6 Basic Topology Configuration
        /// </summary>
        Basic_1X6 = 13,

        /// <summary>
        ///     7x1 Basic Topology Configuration
        /// </summary>
        Basic_7X1 = 14,

        // Basic_End = 23,
        // PassiveStereo_Begin = 24,

        /// <summary>
        ///     1x2 Passive Stereo Configuration
        /// </summary>
        PassiveStereo_1X2 = 24,

        /// <summary>
        ///     2x1 Passive Stereo Configuration
        /// </summary>
        PassiveStereo_2X1 = 25,

        /// <summary>
        ///     1x3 Passive Stereo Configuration
        /// </summary>
        PassiveStereo_1X3 = 26,

        /// <summary>
        ///     3x1 Passive Stereo Configuration
        /// </summary>
        PassiveStereo_3X1 = 27,

        /// <summary>
        ///     1x4 Passive Stereo Configuration
        /// </summary>
        PassiveStereo_1X4 = 28,

        /// <summary>
        ///     4x1 Passive Stereo Configuration
        /// </summary>
        PassiveStereo_4X1 = 29,

        /// <summary>
        ///     2x2 Passive Stereo Configuration
        /// </summary>
        PassiveStereo_2X2 = 30,

        // PassiveStereo_End = 34,
        /// <summary>
        ///     Indicator for the max number of possible configuration, DO NOT USE
        /// </summary>
        Max = 34
    }

    /// <summary>
    ///     These values refer to the different types of Mosaic topologies that are possible. When getting the supported Mosaic
    ///     topologies, you can specify one of these types to narrow down the returned list to only those that match the given
    ///     type.
    /// </summary>
    public enum TopologyType
    {
        /// <summary>
        ///     All mosaic topologies
        /// </summary>
        All = 0,

        /// <summary>
        ///     Basic Mosaic topologies
        /// </summary>
        Basic = 1,

        /// <summary>
        ///     Passive Stereo topologies
        /// </summary>
        PassiveStereo = 2,

        /// <summary>
        ///     Not supported at this time
        /// </summary>
        ScaledClone = 3,

        /// <summary>
        ///     Not supported at this time
        /// </summary>
        PassiveStereoScaledClone = 4
    }

    /// <summary>
    ///     These bits are used to describe the validity of a topo
    /// </summary>
    [Flags]
    public enum TopologyValidity : uint
    {
        /// <summary>
        ///     The topology is valid
        /// </summary>
        Valid = 0,

        /// <summary>
        ///     Not enough SLI GPUs were found to fill the entire topology. PhysicalGPUHandle will be null for these.
        /// </summary>
        MissingGPU = 1,

        /// <summary>
        ///     Not enough displays were found to fill the entire topology. Output identification will be 0 for these.
        /// </summary>
        MissingDisplay = 2,

        /// <summary>
        ///     The topology is only possible with displays of the same output type. Check output identifications to make sure they
        ///     are all CRTs, or all DFPs.
        /// </summary>
        MixedDisplayTypes = 4
    }







    /// <summary>
    ///     Holds a display setting
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct DisplaySettingsV1 : IDisplaySettings,
        IInitializable,
        IEquatable<DisplaySettingsV1>,
        IEquatable<DisplaySettingsV2>,
        ICloneable
    {
        internal StructureVersion _Version;
        internal uint _Width;
        internal uint _Height;
        internal uint _BitsPerPixel;
        internal uint _Frequency;

        /// <summary>
        ///     Creates a new DisplaySettingsV1
        /// </summary>
        /// <param name="width">Per-display width</param>
        /// <param name="height">Per-display height</param>
        /// <param name="bitsPerPixel">Bits per pixel</param>
        /// <param name="frequency">Display frequency</param>
        // ReSharper disable once TooManyDependencies
        public DisplaySettingsV1(int width, int height, int bitsPerPixel, int frequency)
        {
            this = typeof(DisplaySettingsV1).Instantiate<DisplaySettingsV1>();
            _Width = (uint)width;
            _Height = (uint)height;
            _BitsPerPixel = (uint)bitsPerPixel;
            _Frequency = (uint)frequency;
        }


        /// <inheritdoc />
        public bool Equals(DisplaySettingsV1 other)
        {
            return _Width == other._Width &&
                   _Height == other._Height &&
                   _BitsPerPixel == other._BitsPerPixel &&
                   _Frequency == other._Frequency;
        }

        /// <inheritdoc />
        public bool Equals(DisplaySettingsV2 other)
        {
            return _Width == other._Width &&
                   _Height == other._Height &&
                   _BitsPerPixel == other._BitsPerPixel &&
                   _Frequency == other._Frequency;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is DisplaySettingsV1 v1 && Equals(v1);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_Width;
                hashCode = (hashCode * 397) ^ (int)_Height;
                hashCode = (hashCode * 397) ^ (int)_BitsPerPixel;
                hashCode = (hashCode * 397) ^ (int)_Frequency;

                return hashCode;
            }
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(DisplaySettingsV1 left, DisplaySettingsV1 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(DisplaySettingsV1 left, DisplaySettingsV1 right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            DisplaySettingsV1 other = (DisplaySettingsV1)MemberwiseClone();
            return other;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public int Width
        {
            get => (int)_Width; 
            set => _Width = (uint)value;
        }

        /// <inheritdoc />
        public int Height
        {
            get => (int)_Height; 
            set => _Height = (uint)value;
        }

        /// <inheritdoc />
        public int BitsPerPixel
        {
            get => (int)_BitsPerPixel; 
            set => _BitsPerPixel = (uint)value;
        }

        /// <inheritdoc />
        public int Frequency
        {
            get => (int)_Frequency; 
            set => _Frequency = (uint)value;
        }

        /// <inheritdoc />
        public uint FrequencyInMillihertz
        {
            get => _Frequency * 1000;
            set => _Frequency = (uint)value/1000;
        }
    }

    /// <summary>
    ///     Holds a display setting
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct DisplaySettingsV2 : IDisplaySettings,
        IInitializable,
        IEquatable<DisplaySettingsV2>,
        IEquatable<DisplaySettingsV1>,
        ICloneable
    {
        internal StructureVersion _Version;
        internal uint _Width;
        internal uint _Height;
        internal uint _BitsPerPixel;
        internal uint _Frequency;
        internal uint _FrequencyInMillihertz;

        /// <summary>
        ///     Creates a new DisplaySettingsV2
        /// </summary>
        /// <param name="width">Per-display width</param>
        /// <param name="height">Per-display height</param>
        /// <param name="bitsPerPixel">Bits per pixel</param>
        /// <param name="frequency">Display frequency</param>
        /// <param name="frequencyInMillihertz">Display frequency in x1k</param>
        // ReSharper disable once TooManyDependencies
        public DisplaySettingsV2(int width, int height, int bitsPerPixel, int frequency, uint frequencyInMillihertz)
        {
            this = typeof(DisplaySettingsV2).Instantiate<DisplaySettingsV2>();
            _Width = (uint)width;
            _Height = (uint)height;
            _BitsPerPixel = (uint)bitsPerPixel;
            _Frequency = (uint)frequency;
            _FrequencyInMillihertz = frequencyInMillihertz;
        }

        /// <inheritdoc />
        public bool Equals(DisplaySettingsV2 other)
        {
            return _Width == other._Width &&
                   _Height == other._Height &&
                   _BitsPerPixel == other._BitsPerPixel &&
                   _Frequency == other._Frequency &&
                   _FrequencyInMillihertz == other._FrequencyInMillihertz;
        }

        /// <inheritdoc />
        public bool Equals(DisplaySettingsV1 other)
        {
            return _Width == other._Width &&
                   _Height == other._Height &&
                   _BitsPerPixel == other._BitsPerPixel &&
                   _Frequency == other._Frequency;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is DisplaySettingsV2 v2 && Equals(v2);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_Width;
                hashCode = (hashCode * 397) ^ (int)_Height;
                hashCode = (hashCode * 397) ^ (int)_BitsPerPixel;
                hashCode = (hashCode * 397) ^ (int)_Frequency;
                hashCode = (hashCode * 397) ^ (int)_FrequencyInMillihertz;

                return hashCode;
            }
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(DisplaySettingsV2 left, DisplaySettingsV2 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(DisplaySettingsV2 left, DisplaySettingsV2 right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            DisplaySettingsV2 other = (DisplaySettingsV2)MemberwiseClone();
            return other;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public int Width
        {
            get => (int)_Width;
            set => _Width = (uint)value;
        }

        /// <inheritdoc />
        public int Height
        {
            get => (int)_Height;
            set => _Height = (uint)value;
        }

        /// <inheritdoc />
        public int BitsPerPixel
        {
            get => (int)_BitsPerPixel; 
            set => _BitsPerPixel = (uint)value;

        }

        /// <inheritdoc />
        public int Frequency
        {
            get => (int)_Frequency; 
            set => _Frequency = (uint)value;
        }

        /// <inheritdoc />
        public uint FrequencyInMillihertz
        {
            get => _FrequencyInMillihertz;
            set => _FrequencyInMillihertz = (uint)value;
        }
    }


    /// <summary>
    ///     Holds information about a topology validity status
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct DisplayTopologyStatus : IInitializable
    {
        /// <summary>
        ///     Maximum number of displays for this structure
        /// </summary>
        public const int MaxDisplays =
            PhysicalGPUHandle.PhysicalGPUs * NvConstants.AdvancedDisplayHeads;

        internal StructureVersion _Version;
        internal DisplayCapacityProblem _Errors;
        internal DisplayTopologyWarning _Warnings;
        internal uint _DisplayCounts;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDisplays)]
        internal Display[] _Displays;

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        // <inheritdoc />
        public uint DisplayCounts
        {
            get => _DisplayCounts;
            set => _DisplayCounts = value;
        }

        /// <summary>
        ///     Gets all error flags for this topology
        /// </summary>
        public DisplayCapacityProblem Errors
        {
            get => _Errors;
            set => _Errors = value;
        }

        /// <summary>
        ///     Gets all warning flags for this topology
        /// </summary>
        public DisplayTopologyWarning Warnings
        {
            get => _Warnings;
            set => _Warnings = value;
        }

        /// <summary>
        ///     Gets per display statuses
        /// </summary>
        public Display[] Displays
        {
            get => _Displays.Take((int)_DisplayCounts).ToArray();
            set
            {
                _Displays = value;
                _DisplayCounts = (uint)value.Length;
            }
        }

        /// <summary>
        ///     Holds information about a display validity status in a topology
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct Display
        {
            internal uint _DisplayId;
            internal DisplayCapacityProblem _Errors;
            internal DisplayTopologyWarning _Warnings;
            internal uint _RawReserved;

            /// <summary>
            ///     Gets the Display identification of this display.
            /// </summary>
            public uint DisplayId
            {
                get => _DisplayId; 
                set => _DisplayId = value;
            }

            /// <summary>
            ///     Gets all error flags for this display
            /// </summary>
            public DisplayCapacityProblem Errors
            {
                get => _Errors; 
                set => _Errors = value;
            }

            /// <summary>
            ///     Gets all warning flags for this display
            /// </summary>
            public DisplayTopologyWarning Warnings
            {
                get => _Warnings; 
                set => _Warnings = value;
            }

            /// <summary>
            ///     Indicates if this display can be rotated
            /// </summary>
            public bool SupportsRotation
            {
                get => _RawReserved.GetBit(0);
                set => _RawReserved = _RawReserved.SetBit(0, value);
            }
        }
    }

    /// <summary>
    ///     Holds information about a display in a grid topology
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct GridTopologyDisplayV1 : IEquatable<GridTopologyDisplayV1>, ICloneable
    {
        internal uint _DisplayId;
        internal int _OverlapX;
        internal int _OverlapY;
        internal Rotate _Rotation;
        internal uint _CloneGroup;

        /// <summary>
        ///     Creates a new GridTopologyDisplayV1
        /// </summary>
        /// <param name="displayId">Display identification</param>
        /// <param name="overlapX">Horizontal overlap (+overlap, -gap)</param>
        /// <param name="overlapY">Vertical overlap (+overlap, -gap)</param>
        /// <param name="rotation">Rotation of display</param>
        /// <param name="cloneGroup">Clone group identification; Reserved, must be 0</param>
        // ReSharper disable once TooManyDependencies
        public GridTopologyDisplayV1(uint displayId, int overlapX, int overlapY, Rotate rotation, uint cloneGroup = 0)
            : this()
        {
            _DisplayId = displayId;
            _OverlapX = overlapX;
            _OverlapY = overlapY;
            _Rotation = rotation;
            _CloneGroup = cloneGroup;
        }

        /// <inheritdoc />
        public bool Equals(GridTopologyDisplayV1 other)
        {
            return _DisplayId == other._DisplayId &&
                   _OverlapX == other._OverlapX &&
                   _OverlapY == other._OverlapY &&
                   _Rotation == other._Rotation &&
                   _CloneGroup == other._CloneGroup;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is GridTopologyDisplayV1 v1 && Equals(v1);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_DisplayId;
                hashCode = (hashCode * 397) ^ _OverlapX;
                hashCode = (hashCode * 397) ^ _OverlapY;
                hashCode = (hashCode * 397) ^ (int)_Rotation;
                hashCode = (hashCode * 397) ^ (int)_CloneGroup;

                return hashCode;
            }
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(GridTopologyDisplayV1 left, GridTopologyDisplayV1 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(GridTopologyDisplayV1 left, GridTopologyDisplayV1 right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            GridTopologyDisplayV1 other = (GridTopologyDisplayV1)MemberwiseClone();
            return other;
        }

        /// <inheritdoc />
        public uint DisplayId
        {
            get => _DisplayId; 
            set => _DisplayId = value;
        }

        /// <inheritdoc />
        public int OverlapX
        {
            get => _OverlapX; 
            set => _OverlapX = value;
        }

        /// <inheritdoc />
        public int OverlapY
        {
            get => _OverlapY; 
            set => _OverlapY = value;
        }

        /// <inheritdoc />
        public Rotate Rotation
        {
            get => _Rotation; 
            set => _Rotation = value;
        }

        /// <inheritdoc />
        public uint CloneGroup
        {
            get => _CloneGroup; 
            set => _CloneGroup = value;
        }

        /// <inheritdoc />
        public PixelShiftType PixelShiftType
        {
            get => PixelShiftType.NoPixelShift;
        }
    }


    /// <summary>
    ///     Holds information about a display in a grid topology
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct GridTopologyDisplayV2 : IEquatable<GridTopologyDisplayV2>
    {
        internal StructureVersion _Version;
        internal uint _DisplayId;
        internal int _OverlapX;
        internal int _OverlapY;
        internal Rotate _Rotation;
        internal uint _CloneGroup;
        internal PixelShiftType _PixelShiftType;

        /// <summary>
        ///     Creates a new GridTopologyDisplayV2
        /// </summary>
        /// <param name="displayId">Display identification</param>
        /// <param name="overlapX">Horizontal overlap (+overlap, -gap)</param>
        /// <param name="overlapY">Vertical overlap (+overlap, -gap)</param>
        /// <param name="rotation">Rotation of display</param>
        /// <param name="cloneGroup">Clone group identification; Reserved, must be 0</param>
        /// <param name="pixelShiftType">Type of the pixel shift enabled display</param>
        // ReSharper disable once TooManyDependencies
        public GridTopologyDisplayV2(
            uint displayId,
            int overlapX,
            int overlapY,
            Rotate rotation,
            uint cloneGroup = 0,
            PixelShiftType pixelShiftType = PixelShiftType.NoPixelShift) : this()
        {
            this = typeof(GridTopologyDisplayV2).Instantiate<GridTopologyDisplayV2>();
            _DisplayId = displayId;
            _OverlapX = overlapX;
            _OverlapY = overlapY;
            _Rotation = rotation;
            _CloneGroup = cloneGroup;
            _PixelShiftType = pixelShiftType;
        }

        /// <inheritdoc />
        public bool Equals(GridTopologyDisplayV2 other)
        {
            return _DisplayId == other._DisplayId &&
                   _OverlapX == other._OverlapX &&
                   _OverlapY == other._OverlapY &&
                   _Rotation == other._Rotation &&
                   _CloneGroup == other._CloneGroup &&
                   _PixelShiftType == other._PixelShiftType;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is GridTopologyDisplayV2 v2 && Equals(v2);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_DisplayId;
                hashCode = (hashCode * 397) ^ _OverlapX;
                hashCode = (hashCode * 397) ^ _OverlapY;
                hashCode = (hashCode * 397) ^ (int)_Rotation;
                hashCode = (hashCode * 397) ^ (int)_CloneGroup;
                hashCode = (hashCode * 397) ^ (int)_PixelShiftType;

                return hashCode;
            }
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(GridTopologyDisplayV2 left, GridTopologyDisplayV2 right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(GridTopologyDisplayV2 left, GridTopologyDisplayV2 right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            GridTopologyDisplayV2 other = (GridTopologyDisplayV2)MemberwiseClone();
            return other;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public uint DisplayId
        {
            get => _DisplayId;
            set => _DisplayId = value;
        }

        /// <inheritdoc />
        public int OverlapX
        {
            get => _OverlapX;
            set => _OverlapX = value;
        }

        /// <inheritdoc />
        public int OverlapY
        {
            get => _OverlapY; 
            set => _OverlapY = value;
        }

        /// <inheritdoc />
        public Rotate Rotation
        {
            get => _Rotation; 
            set => _Rotation = value;
        }

        /// <inheritdoc />
        public uint CloneGroup
        {
            get => _CloneGroup; 
            set => _CloneGroup = value;
        }

        /// <inheritdoc />
        public PixelShiftType PixelShiftType
        {
            get => _PixelShiftType; 
            set => _PixelShiftType = value;
        }
    }


    /// <summary>
    ///     Holds information about a grid topology
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct GridTopologyV1 : IInitializable, IEquatable<GridTopologyV1>, ICloneable
    {
        /// <summary>
        ///     Maximum number of displays in a topology
        /// </summary>
        public const int MaxDisplays = 64;

        internal StructureVersion _Version;
        internal uint _Rows;
        internal uint _Columns;
        internal uint _DisplayCount;
        internal uint _RawReserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDisplays)]
        internal GridTopologyDisplayV1[] _Displays;

        internal DisplaySettingsV1 _DisplaySettings;

        /// <summary>
        ///     Creates a new GridTopologyV1
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="displays">Topology displays; Displays are done as [(row * columns) + column]</param>
        /// <param name="displaySettings">Display settings</param>
        /// <param name="applyWithBezelCorrectedResolution">
        ///     When enabling and doing the modeset, do we switch to the
        ///     bezel-corrected resolution
        /// </param>
        /// <param name="immersiveGaming">Enable as immersive gaming instead of Mosaic SLI (for Quadro-boards only)</param>
        /// <param name="baseMosaicPanoramic">
        ///     Enable as Base Mosaic (Panoramic) instead of Mosaic SLI (for NVS and Quadro-boards
        ///     only)
        /// </param>
        /// <param name="driverReloadAllowed">
        ///     If necessary, reloading the driver is permitted (for Vista and above only). Will not
        ///     be persisted.
        /// </param>
        /// <param name="acceleratePrimaryDisplay">
        ///     Enable SLI acceleration on the primary display while in single-wide mode (For
        ///     Immersive Gaming only). Will not be persisted.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">Total number of topology displays is below or equal to zero</exception>
        /// <exception cref="ArgumentException">Number of displays doesn't match the arrangement</exception>
        // ReSharper disable once TooManyDependencies
        public GridTopologyV1(
            int rows,
            int columns,
            GridTopologyDisplayV1[] displays,
            DisplaySettingsV1 displaySettings,
            bool applyWithBezelCorrectedResolution,
            bool immersiveGaming,
            bool baseMosaicPanoramic,
            bool driverReloadAllowed,
            bool acceleratePrimaryDisplay)
        {
            if (rows * columns <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(rows)}, {nameof(columns)}",
                    "Invalid display arrangement.");
            }

            if (displays.Length > MaxDisplays)
            {
                throw new ArgumentException("Too many displays.");
            }

            if (displays.Length != rows * columns)
            {
                throw new ArgumentException("Number of displays should match the arrangement.", nameof(displays));
            }

            this = typeof(GridTopologyV1).Instantiate<GridTopologyV1>();
            _Rows = (uint)rows;
            _Columns = (uint)columns;
            _DisplayCount = (uint)displays.Length;
            _Displays = displays;
            _DisplaySettings = displaySettings;
            ApplyWithBezelCorrectedResolution = applyWithBezelCorrectedResolution;
            ImmersiveGaming = immersiveGaming;
            BaseMosaicPanoramic = baseMosaicPanoramic;
            DriverReloadAllowed = driverReloadAllowed;
            AcceleratePrimaryDisplay = acceleratePrimaryDisplay;
            Array.Resize(ref _Displays, MaxDisplays);
        }

        /// <inheritdoc />
        public bool Equals(GridTopologyV1 other)
        {
            return _Rows == other._Rows &&
                   _Columns == other._Columns &&
                   _DisplayCount == other._DisplayCount &&
                   _RawReserved == other._RawReserved &&
                   _Displays.SequenceEqual(other._Displays) &&
                   _DisplaySettings.Equals(other._DisplaySettings);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is GridTopologyV1 v1 && Equals(v1);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_Rows;
                hashCode = (hashCode * 397) ^ (int)_Columns;
                hashCode = (hashCode * 397) ^ (int)_DisplayCount;
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                hashCode = (hashCode * 397) ^ (int)_RawReserved;
                hashCode = (hashCode * 397) ^ (_Displays?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _DisplaySettings.GetHashCode();

                return hashCode;
            }
        }

        public object Clone()
        {
            GridTopologyV1 other = (GridTopologyV1)MemberwiseClone();
            return other;
        }

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        // <inheritdoc />
        public uint DisplayCount
        {
            get => _DisplayCount;
            set => _DisplayCount = value;
        }

        /// <inheritdoc />
        public int Rows
        {
            get => (int)_Rows;
            set => _Rows = (uint)value;
        }

        /// <inheritdoc />
        public int Columns
        {
            get => (int)_Columns;
            set => _Columns = (uint)value;
        }

        /// <inheritdoc />
        public List<GridTopologyDisplayV1> Displays
        {
            get => _Displays.Take((int)_DisplayCount).ToList();
            set => _Displays = value.ToArray();
        }

        /// <inheritdoc />
        public DisplaySettingsV1 DisplaySettings
        {
            get => _DisplaySettings;
            set => _DisplaySettings = value;
        }

        /// <inheritdoc />
        public bool ApplyWithBezelCorrectedResolution
        {
            get => _RawReserved.GetBit(0);
            private set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <inheritdoc />
        public bool ImmersiveGaming
        {
            get => _RawReserved.GetBit(1);
            private set => _RawReserved = _RawReserved.SetBit(1, value);
        }

        /// <inheritdoc />
        public bool BaseMosaicPanoramic
        {
            get => _RawReserved.GetBit(2);
            private set => _RawReserved = _RawReserved.SetBit(2, value);
        }

        /// <inheritdoc />
        public bool DriverReloadAllowed
        {
            get => _RawReserved.GetBit(3);
            private set => _RawReserved = _RawReserved.SetBit(3, value);
        }

        /// <inheritdoc />
        public bool AcceleratePrimaryDisplay
        {
            get => _RawReserved.GetBit(4);
            private set => _RawReserved = _RawReserved.SetBit(4, value);
        }
    }

    /// <summary>
    ///     Holds information about a grid topology
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct GridTopologyV2 : IInitializable, IEquatable<GridTopologyV2>, ICloneable
    {
        /// <summary>
        ///     Maximum number of displays in a topology
        /// </summary>
        public const int MaxDisplays = GridTopologyV1.MaxDisplays;

        internal StructureVersion _Version;
        internal uint _Rows;
        internal uint _Columns;
        internal uint _DisplayCount;
        internal uint _RawReserved;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxDisplays)]
        internal GridTopologyDisplayV2[] _Displays;

        internal DisplaySettingsV1 _DisplaySettings;

        /// <summary>
        ///     Creates a new GridTopologyV2
        /// </summary>
        /// <param name="rows">Number of rows</param>
        /// <param name="columns">Number of columns</param>
        /// <param name="displays">Topology displays; Displays are done as [(row * columns) + column]</param>
        /// <param name="displaySettings">Display settings</param>
        /// <param name="applyWithBezelCorrectedResolution">
        ///     When enabling and doing the mode-set, do we switch to the
        ///     bezel-corrected resolution
        /// </param>
        /// <param name="immersiveGaming">Enable as immersive gaming instead of Mosaic SLI (for Quadro-boards only)</param>
        /// <param name="baseMosaicPanoramic">
        ///     Enable as Base Mosaic (Panoramic) instead of Mosaic SLI (for NVS and Quadro-boards
        ///     only)
        /// </param>
        /// <param name="driverReloadAllowed">
        ///     If necessary, reloading the driver is permitted (for Vista and above only). Will not
        ///     be persisted.
        /// </param>
        /// <param name="acceleratePrimaryDisplay">
        ///     Enable SLI acceleration on the primary display while in single-wide mode (For
        ///     Immersive Gaming only). Will not be persisted.
        /// </param>
        /// <param name="pixelShift">Enable Pixel shift</param>
        /// <exception cref="ArgumentOutOfRangeException">Total number of topology displays is below or equal to zero</exception>
        /// <exception cref="ArgumentException">Number of displays doesn't match the arrangement</exception>
        // ReSharper disable once TooManyDependencies
        public GridTopologyV2(
            int rows,
            int columns,
            GridTopologyDisplayV2[] displays,
            DisplaySettingsV1 displaySettings,
            bool applyWithBezelCorrectedResolution,
            bool immersiveGaming,
            bool baseMosaicPanoramic,
            bool driverReloadAllowed,
            bool acceleratePrimaryDisplay,
            bool pixelShift)
        {
            if (rows * columns <= 0)
            {
                throw new ArgumentOutOfRangeException($"{nameof(rows)}, {nameof(columns)}",
                    "Invalid display arrangement.");
            }

            if (displays.Length > MaxDisplays)
            {
                throw new ArgumentException("Too many displays.");
            }

            if (displays.Length != rows * columns)
            {
                throw new ArgumentException("Number of displays should match the arrangement.", nameof(displays));
            }

            this = typeof(GridTopologyV2).Instantiate<GridTopologyV2>();
            _Rows = (uint)rows;
            _Columns = (uint)columns;
            _DisplayCount = (uint)displays.Length;
            _Displays = displays;
            _DisplaySettings = displaySettings;
            ApplyWithBezelCorrectedResolution = applyWithBezelCorrectedResolution;
            ImmersiveGaming = immersiveGaming;
            BaseMosaicPanoramic = baseMosaicPanoramic;
            DriverReloadAllowed = driverReloadAllowed;
            AcceleratePrimaryDisplay = acceleratePrimaryDisplay;
            PixelShift = pixelShift;
            Array.Resize(ref _Displays, MaxDisplays);
        }

        /// <inheritdoc />
        public bool Equals(GridTopologyV2 other)
        {
            return _Rows == other._Rows &&
                   _Columns == other._Columns &&
                   _DisplayCount == other._DisplayCount &&
                   _RawReserved == other._RawReserved &&
                   _Displays.SequenceEqual(other._Displays) &&
                   _DisplaySettings.Equals(other._DisplaySettings);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is GridTopologyV2 v2 && Equals(v2);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_Rows;
                hashCode = (hashCode * 397) ^ (int)_Columns;
                hashCode = (hashCode * 397) ^ (int)_DisplayCount;
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                hashCode = (hashCode * 397) ^ (int)_RawReserved;
                hashCode = (hashCode * 397) ^ (_Displays?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ _DisplaySettings.GetHashCode();

                return hashCode;
            }
        }

        public object Clone()
        {
            GridTopologyV2 other = (GridTopologyV2)MemberwiseClone();
            return other;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        // <inheritdoc />
        public uint DisplayCount
        {
            get => _DisplayCount;
            set => _DisplayCount = value;
        }

        /// <inheritdoc />
        public int Rows
        {
            get => (int)_Rows; 
            set => _Rows = (uint)value;
        }

        /// <inheritdoc />
        public int Columns
        {
            get => (int)_Columns; 
            set => _Columns = (uint)value;
        }

        /// <inheritdoc />
        public List<GridTopologyDisplayV2> Displays
        {
            get => _Displays.Take((int)_DisplayCount).ToList();
            set
            {
                _DisplayCount = (uint)value.Count;
                _Displays = new GridTopologyDisplayV2[MaxDisplays];
                GridTopologyDisplayV2[] passedDisplays = value.ToArray();
                for (var i = 0; i < passedDisplays.Length; i++)
                {
                    _Displays[i] = passedDisplays[i];
                }
            }
        }

        /// <inheritdoc />
        public DisplaySettingsV1 DisplaySettings
        {
            get => _DisplaySettings;
            set => _DisplaySettings = value;
        }

        /// <inheritdoc />
        public bool ApplyWithBezelCorrectedResolution
        {
            get => _RawReserved.GetBit(0);
            set => _RawReserved = _RawReserved.SetBit(0, value);
        }

        /// <inheritdoc />
        public bool ImmersiveGaming
        {
            get => _RawReserved.GetBit(1);
            set => _RawReserved = _RawReserved.SetBit(1, value);
        }

        /// <inheritdoc />
        public bool BaseMosaicPanoramic
        {
            get => _RawReserved.GetBit(2);
            set => _RawReserved = _RawReserved.SetBit(2, value);
        }

        /// <inheritdoc />
        public bool DriverReloadAllowed
        {
            get => _RawReserved.GetBit(3);
            set => _RawReserved = _RawReserved.SetBit(3, value);
        }

        /// <inheritdoc />
        public bool AcceleratePrimaryDisplay
        {
            get => _RawReserved.GetBit(4);
            set => _RawReserved = _RawReserved.SetBit(4, value);
        }

        /// <summary>
        ///     Enable Pixel shift
        /// </summary>
        public bool PixelShift
        {
            get => _RawReserved.GetBit(5);
            set => _RawReserved = _RawReserved.SetBit(5, value);
        }
    }

    /// <summary>
    ///     Holds information about supported topologies
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct SupportedTopologiesInfoV1 : ISupportedTopologiesInfo,
        IInitializable,
        IEquatable<SupportedTopologiesInfoV1>
    {
        /// <summary>
        ///     Maximum number of display settings possible to retrieve
        /// </summary>
        public const int MaxSettings = 40;

        internal StructureVersion _Version;
        internal uint _TopologyBriefsCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Topology.Max)]
        internal TopologyBrief[]
            _TopologyBriefs;

        internal uint _DisplaySettingsCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSettings)]
        internal DisplaySettingsV1[]
            _DisplaySettings;

        /// <inheritdoc />
        public bool Equals(SupportedTopologiesInfoV1 other)
        {
            return _TopologyBriefsCount == other._TopologyBriefsCount &&
                   _TopologyBriefs.SequenceEqual(other._TopologyBriefs) &&
                   _DisplaySettingsCount == other._DisplaySettingsCount &&
                   _DisplaySettings.SequenceEqual(other._DisplaySettings);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is SupportedTopologiesInfoV1 v1 && Equals(v1);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_TopologyBriefsCount;
                hashCode = (hashCode * 397) ^ (_TopologyBriefs?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)_DisplaySettingsCount;
                hashCode = (hashCode * 397) ^ (_DisplaySettings?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        public object Clone()
        {
            SupportedTopologiesInfoV1 other = (SupportedTopologiesInfoV1)MemberwiseClone();
            return other;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        // <inheritdoc />
        public uint TopologyBriefsCount
        {
            get => _TopologyBriefsCount;
            set => _TopologyBriefsCount = value;
        }

        /// <inheritdoc />
        public List<TopologyBrief> TopologyBriefs
        {
            get => _TopologyBriefs.Take((int)_TopologyBriefsCount).ToList();
            set => _TopologyBriefs = value.ToArray();
        }

        /// <inheritdoc />
        public List<IDisplaySettings> DisplaySettings
        {
            get => _DisplaySettings.Take((int)_DisplaySettingsCount).Cast<IDisplaySettings>().ToList();
            set
            {
                _DisplaySettings = value.Cast<DisplaySettingsV1>().ToArray();
                _DisplaySettingsCount = (uint)_DisplaySettings.Length;
            }
        }
    }

    /// <summary>
    ///     Holds information about supported topologies
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct SupportedTopologiesInfoV2 : ISupportedTopologiesInfo,
        IInitializable,
        IEquatable<SupportedTopologiesInfoV2>
    {
        /// <summary>
        ///     Maximum number of display settings possible to retrieve
        /// </summary>
        public const int MaxSettings = SupportedTopologiesInfoV1.MaxSettings;

        internal StructureVersion _Version;
        internal uint _TopologyBriefsCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)Topology.Max)]
        internal TopologyBrief[]
            _TopologyBriefs;

        internal uint _DisplaySettingsCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxSettings)]
        internal DisplaySettingsV2[]
            _DisplaySettings;

        /// <inheritdoc />
        public bool Equals(SupportedTopologiesInfoV2 other)
        {
            return _TopologyBriefsCount == other._TopologyBriefsCount &&
                   _TopologyBriefs.SequenceEqual(other._TopologyBriefs) &&
                   _DisplaySettingsCount == other._DisplaySettingsCount &&
                   _DisplaySettings.SequenceEqual(other._DisplaySettings);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is SupportedTopologiesInfoV2 v2 && Equals(v2);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)_TopologyBriefsCount;
                hashCode = (hashCode * 397) ^ (_TopologyBriefs?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (int)_DisplaySettingsCount;
                hashCode = (hashCode * 397) ^ (_DisplaySettings?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        public object Clone()
        {
            SupportedTopologiesInfoV2 other = (SupportedTopologiesInfoV2)MemberwiseClone();
            return other;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        // <inheritdoc />
        public uint TopologyBriefsCount
        {
            get => _TopologyBriefsCount;
            set => _TopologyBriefsCount = value;
        }

        /// <inheritdoc />
        public List<TopologyBrief> TopologyBriefs
        {
            get => _TopologyBriefs.Take((int)_TopologyBriefsCount).ToList();
            set => _TopologyBriefs = value.ToArray();
        }

        /// <inheritdoc />
        public List<IDisplaySettings> DisplaySettings
        {
            get => _DisplaySettings.Take((int)_DisplaySettingsCount).Cast<IDisplaySettings>().ToList();
            set
            {
                _DisplaySettings = value.Cast<DisplaySettingsV2>().ToArray();
                _DisplaySettingsCount = (uint)_DisplaySettings.Length;
            }
        }
    }

    /// <summary>
    ///     Holds brief information about a topology
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct TopologyBrief : IInitializable, IEquatable<TopologyBrief>, ICloneable
    {
        internal StructureVersion _Version;
        internal Topology _Topology;
        internal uint _IsEnable;
        internal uint _IsPossible;

        /// <summary>
        ///     Creates a new TopologyBrief
        /// </summary>
        /// <param name="topology">The topology</param>
        public TopologyBrief(Topology topology)
        {
            this = typeof(TopologyBrief).Instantiate<TopologyBrief>();
            _Topology = topology;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public bool Equals(TopologyBrief other)
        {
            return _Topology == other._Topology;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TopologyBrief brief && Equals(brief);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return (int)_Topology;
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(TopologyBrief left, TopologyBrief right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(TopologyBrief left, TopologyBrief right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            TopologyBrief other = (TopologyBrief)MemberwiseClone();
            return other;
        }

        /// <summary>
        ///     The topology
        /// </summary>
        public Topology Topology
        {
            get => _Topology;
            set => _Topology = value;
        }

        /// <summary>
        ///     Indicates if the topology is enable
        /// </summary>
        public bool IsEnable
        {
            get => _IsEnable > 0;
            set => _IsEnable = (uint)(value == true ? 1 : 0); 
        }

        /// <summary>
        ///     Indicates if the topology is possible
        /// </summary>
        public bool IsPossible
        {
            get => _IsPossible > 0;
            set => _IsPossible = (uint)(value == true ? 1 : 0);

        }
    }

    /// <summary>
    ///     Holds extra details about a topology
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct TopologyDetails : IInitializable, IEquatable<TopologyDetails>, ICloneable
    {
        /// <summary>
        ///     Maximum number of rows in a topology detail
        /// </summary>
        public const int MaxLayoutRows = 8;

        /// <summary>
        ///     Maximum number of columns in a topology detail
        /// </summary>
        public const int MaxLayoutColumns = 8;

        internal StructureVersion _Version;
        internal LogicalGPUHandle _LogicalGPUHandle;
        internal TopologyValidity _ValidityFlags;
        internal uint _Rows;
        internal uint _Columns;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLayoutRows)]
        internal LayoutRow[] _LayoutRows;

        /// <inheritdoc />
        public bool Equals(TopologyDetails other)
        {
            return _LogicalGPUHandle.Equals(other._LogicalGPUHandle) &&
                   _ValidityFlags == other._ValidityFlags &&
                   _Rows == other._Rows &&
                   _Columns == other._Columns &&
                   _LayoutRows.SequenceEqual(other._LayoutRows);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TopologyDetails details && Equals(details);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _LogicalGPUHandle.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_ValidityFlags;
                hashCode = (hashCode * 397) ^ (int)_Rows;
                hashCode = (hashCode * 397) ^ (int)_Columns;
                hashCode = (hashCode * 397) ^ (_LayoutRows?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(TopologyDetails left, TopologyDetails right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(TopologyDetails left, TopologyDetails right)
        {
            return !left.Equals(right);
        }

        public object Clone()
        {
            TopologyDetails other = (TopologyDetails)MemberwiseClone();
            return other;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Logical GPU for this topology
        /// </summary>
        public LogicalGPUHandle LogicalGPUHandle
        {
            get => _LogicalGPUHandle;
            set => _LogicalGPUHandle = value;
        }

        /// <summary>
        ///     Indicates topology validity. TopologyValidity.Valid means topology is valid with the current hardware.
        /// </summary>
        public TopologyValidity ValidityFlags
        {
            get => _ValidityFlags;
            set => _ValidityFlags = value;
        }

        /// <summary>
        ///     Number of displays in a row
        /// </summary>
        public int Rows
        {
            get => (int)_Rows;
            set => _Rows = (uint)value;
        }

        /// <summary>
        ///     Number of displays in a column
        /// </summary>
        public int Columns
        {
            get => (int)_Columns;
            set => _Columns = (uint)value;

        }

        /// <summary>
        ///     Gets a 2D array of layout cells containing information about the display layout of the topology
        /// </summary>
        public LayoutCell[][] Layout
        {
            get
            {
                var columns = (int)_Columns;

                return _LayoutRows.Take((int)_Rows).Select(row => row.LayoutCells.Take(columns).ToArray()).ToArray();
            }
            set
            {
                List<LayoutRow> newLayoutRows = new List<LayoutRow>();
                foreach (var row in value)
                {
                    LayoutRow newLayoutRow = new LayoutRow();
                    List<LayoutCell> newLayoutColumns = new List<LayoutCell>();
                    foreach (var column in row)
                    {

                        newLayoutColumns.Add(column);
                    }
                    newLayoutRow.LayoutCells = newLayoutColumns.ToArray();
                    newLayoutRows.Add(newLayoutRow);

                }
                _LayoutRows = newLayoutRows.ToArray();
                _Rows = (uint)newLayoutRows.Count;
                _Columns = (uint)newLayoutRows[0].LayoutCells.Length;
            }
        }

        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        internal struct LayoutRow : IInitializable, IEquatable<LayoutRow>
        {
            [JsonProperty]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxLayoutColumns)]
            internal LayoutCell[]
                LayoutCells;

            public bool Equals(LayoutRow other)
            {
                return LayoutCells.SequenceEqual(other.LayoutCells);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is LayoutRow row && Equals(row);
            }

            public override int GetHashCode()
            {
                return LayoutCells?.GetHashCode() ?? 0;
            }

            public object Clone()
            {
                LayoutRow other = (LayoutRow)MemberwiseClone();
                return other;
            }
        }

        /// <summary>
        ///     Holds information about a topology display
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 8)]
        public struct LayoutCell : IEquatable<LayoutCell>
        {
            internal PhysicalGPUHandle _PhysicalGPUHandle;
            internal OutputId _DisplayOutputId;
            internal int _OverlapX;
            internal int _OverlapY;

            /// <inheritdoc />
            public bool Equals(LayoutCell other)
            {
                return _PhysicalGPUHandle.Equals(other._PhysicalGPUHandle) &&
                       _DisplayOutputId == other._DisplayOutputId &&
                       _OverlapX == other._OverlapX &&
                       _OverlapY == other._OverlapY;
            }

            /// <inheritdoc />
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                {
                    return false;
                }

                return obj is LayoutCell cell && Equals(cell);
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = _PhysicalGPUHandle.GetHashCode();
                    hashCode = (hashCode * 397) ^ (int)_DisplayOutputId;
                    hashCode = (hashCode * 397) ^ _OverlapX;
                    hashCode = (hashCode * 397) ^ _OverlapY;

                    return hashCode;
                }
            }

            /// <summary>
            ///     Checks for equality between two objects of same type
            /// </summary>
            /// <param name="left">The first object</param>
            /// <param name="right">The second object</param>
            /// <returns>true, if both objects are equal, otherwise false</returns>
            public static bool operator ==(LayoutCell left, LayoutCell right)
            {
                return left.Equals(right);
            }

            /// <summary>
            ///     Checks for inequality between two objects of same type
            /// </summary>
            /// <param name="left">The first object</param>
            /// <param name="right">The second object</param>
            /// <returns>true, if both objects are not equal, otherwise false</returns>
            public static bool operator !=(LayoutCell left, LayoutCell right)
            {
                return !left.Equals(right);
            }

            public object Clone()
            {
                LayoutCell other = (LayoutCell)MemberwiseClone();
                return other;
            }

            /// <summary>
            ///     Physical GPU to be used in the topology (0 if GPU missing)
            /// </summary>
            public PhysicalGPUHandle PhysicalGPUHandle
            {
                get => _PhysicalGPUHandle; 
                set => _PhysicalGPUHandle = value;
            }

            /// <summary>
            ///     Connected display target (0 if no display connected)
            /// </summary>
            public OutputId DisplayOutputId
            {
                get => _DisplayOutputId;
                set => _DisplayOutputId = value;
            }

            /// <summary>
            ///     Pixels of overlap on left of target: (+overlap, -gap)
            /// </summary>
            public int OverlapX
            {
                get => _OverlapX; 
                set => _OverlapX = value;
            }

            /// <summary>
            ///     Pixels of overlap on top of target: (+overlap, -gap)
            /// </summary>
            public int OverlapY
            {
                get => _OverlapY; 
                set => _OverlapY = value;
            }
        }
    }

    /// <summary>
    ///     This structure defines a group of topologies that work together to create one overall layout.  All of the supported
    ///     topologies are represented with this structure.
    ///     For example, a 'Passive Stereo' topology would be represented with this structure, and would have separate topology
    ///     details for the left and right eyes. The count would be 2. A 'Basic' topology is also represented by this
    ///     structure, with a count of 1.
    ///     The structure is primarily used internally, but is exposed to applications in a read-only fashion because there are
    ///     some details in it that might be useful (like the number of rows/cols, or connected display information).  A user
    ///     can get the filled-in structure by calling NvAPI_Mosaic_GetTopoGroup().
    ///     You can then look at the detailed values within the structure.  There are no entry points which take this structure
    ///     as input (effectively making it read-only).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct TopologyGroup : IInitializable, IEquatable<TopologyGroup>, ICloneable
    {
        /// <summary>
        ///     Maximum number of topologies per each group
        /// </summary>
        public const int MaxTopologyPerGroup = 2;

        internal StructureVersion _Version;
        internal TopologyBrief _Brief;
        internal uint _TopologiesCount;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaxTopologyPerGroup)]
        internal TopologyDetails[] _TopologyDetails;

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     The brief details of this topology
        /// </summary>
        public TopologyBrief Brief
        {
            get => _Brief; 
            set => _Brief = value;
        }

        /// <summary>
        ///     Number of topologies within this group
        /// </summary>
        public uint TopologiesCount
        {
            get => _TopologiesCount;
            set => _TopologiesCount = value;
        }

        /// <summary>
        ///     Information about the topologies within this group
        /// </summary>
        public TopologyDetails[] TopologyDetails
        {
            get => _TopologyDetails.Take((int)_TopologiesCount).ToArray();
            set
            {
                _TopologyDetails = value;
                _TopologiesCount = (uint)value.Length;
            }
        }

        /// <inheritdoc />
        public bool Equals(TopologyGroup other)
        {
            return _Brief.Equals(other._Brief) &&
                   _TopologiesCount == other._TopologiesCount &&
                   _TopologyDetails.SequenceEqual(other._TopologyDetails);
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is TopologyGroup group && Equals(group);
        }

        /// <summary>
        ///     Checks for equality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are equal, otherwise false</returns>
        public static bool operator ==(TopologyGroup left, TopologyGroup right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(TopologyGroup left, TopologyGroup right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _Brief.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)_TopologiesCount;
                hashCode = (hashCode * 397) ^ (_TopologyDetails?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

        public object Clone()
        {
            TopologyGroup other = (TopologyGroup)MemberwiseClone();
            return other;
        }
    }

}
