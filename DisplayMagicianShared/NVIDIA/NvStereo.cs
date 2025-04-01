using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.NVIDIA
{
    /// <summary>
    ///     Holds a list of valid values for the stereo activation process
    /// </summary>
    public enum StereoActivationFlag
    {
        /// <summary>
        ///     Immediate activation
        /// </summary>
        Immediate = 0,

        /// <summary>
        ///     Delayed activation
        /// </summary>
        Delayed
    }

    /// <summary>
    ///     Holds a list of valid values for back buffer mode
    /// </summary>
    public enum StereoActiveEye
    {
        /// <summary>
        ///     No back buffer
        /// </summary>
        None = 0,

        /// <summary>
        ///     Right eye back buffer mode
        /// </summary>
        RightEye = 1,

        /// <summary>
        ///     Left eye back buffer mode
        /// </summary>
        LeftEye = 2,

        /// <summary>
        ///     Mono back buffer mode
        /// </summary>
        Mono = 3
    }

    /// <summary>
    ///     Holds a list of possible values for the driver stereo mode
    /// </summary>
    public enum StereoDriverMode
    {
        /// <summary>
        ///     Automatic stereo mode
        /// </summary>
        Automatic = 0,

        /// <summary>
        ///     Direct stereo mode
        /// </summary>
        Direct = 2
    }

    /// <summary>
    ///     Holds a list of valid frustum adjust modes
    /// </summary>
    public enum StereoFrustumAdjustMode
    {
        /// <summary>
        ///     No frustum adjustment
        /// </summary>
        NoFrustumAdjust,

        /// <summary>
        ///     Stretch frustum adjustment
        /// </summary>
        Stretch,

        /// <summary>
        ///     Clear edges frustum adjustment
        /// </summary>
        ClearEdges
    }

    /// <summary>
    ///     Holds a list of valid identification for registry values
    /// </summary>
    public enum StereoRegistryIdentification
    {
        /// <summary>
        ///     Convergence value identification
        /// </summary>
        Convergence,

        /// <summary>
        ///     Frustum adjust mode value identification
        /// </summary>
        FrustumAdjustMode
    }

    /// <summary>
    ///     Holds a list of valid application configuration registry profiles
    /// </summary>
    public enum StereoRegistryProfileType
    {
        /// <summary>
        ///     The default profile
        /// </summary>
        DefaultProfile,

        /// <summary>
        ///     The DirectX 9 specific profile
        /// </summary>
        DirectX9Profile,

        /// <summary>
        ///     The DirectX 10 specific profile
        /// </summary>
        DirectX10Profile
    }

    /// <summary>
    ///     Holds a list of valid values for the stereo surface creation mode
    /// </summary>
    public enum StereoSurfaceCreateMode
    {
        /// <summary>
        ///     Automatic surface creation
        /// </summary>
        Auto = 0,

        /// <summary>
        ///     Force stereo surface creation
        /// </summary>
        ForceStereo,

        /// <summary>
        ///     Force mono surface creation
        /// </summary>
        ForceMono
    }

    /// <summary>
    ///     Holds a list of valid flags for the swap chain mode
    /// </summary>
    public enum StereoSwapChainMode
    {
        /// <summary>
        ///     Automatic
        /// </summary>
        Default = 0,

        /// <summary>
        ///     Stereo
        /// </summary>
        Stereo,

        /// <summary>
        ///     Mono
        /// </summary>
        Mono
    }




    /// <summary>
    ///     Holds information regarding the stereo capabilities of a monitor
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct StereoCapabilitiesV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal uint _Flags;
        internal uint _Reserved1;
        internal uint _Reserved2;
        internal uint _Reserved3;

        /// <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if no windowed mode is supported
        /// </summary>
        public bool IsNoWindowedModeSupported
        {
            get => _Flags.GetBit(0);
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if automatic windowed mode is supported
        /// </summary>
        public bool IsAutomaticWindowedModeSupported
        {
            get => _Flags.GetBit(1);
            set => _Flags = _Flags.SetBit(1, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if the persistent windowed mode is supported
        /// </summary>
        public bool IsPersistentWindowedModeSupported
        {
            get => _Flags.GetBit(2); 
            set => _Flags = _Flags.SetBit(2, value);
        }
    }

    /// <summary>
    /// Holds a handle representing a Device Stereo Session
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StereoHandle : IHandle, IEquatable<StereoHandle>
    {
        internal readonly IntPtr _MemoryAddress;

        /// <inheritdoc />
        public bool Equals(StereoHandle other)
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

            return obj is StereoHandle handle && Equals(handle);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _MemoryAddress.GetHashCode();
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"StereoHandle #{MemoryAddress.ToInt64()}";
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
        public static bool operator ==(StereoHandle left, StereoHandle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(StereoHandle left, StereoHandle right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        ///     Gets default StereoHandle with a null pointer
        /// </summary>
        public static StereoHandle DefaultHandle
        {
            get => default(StereoHandle);
        }
    }
}
