using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DisplayMagicianShared.NVIDIA
{
    /// <summary>
    ///     Holds possible values for the setting location
    /// </summary>
    public enum DRSSettingLocation : uint
    {
        /// <summary>
        ///     Setting is part of the current profile
        /// </summary>
        CurrentProfile = 0,

        /// <summary>
        ///     Setting is part of the global profile
        /// </summary>
        GlobalProfile,

        /// <summary>
        ///     Setting is part of the base profile
        /// </summary>
        BaseProfile,

        /// <summary>
        ///     Setting is part of the default profile
        /// </summary>
        DefaultProfile
    }
    
    /// <summary>
     ///     Holds a list of possible setting value types
     /// </summary>
    public enum DRSSettingType : uint
    {
        /// <summary>
        ///     Integer value type
        /// </summary>
        Integer = 0,

        /// <summary>
        ///     Binary value type
        /// </summary>
        Binary,

        /// <summary>
        ///     ASCII string value type
        /// </summary>
        String,

        /// <summary>
        ///     Unicode string value type
        /// </summary>
        UnicodeString
    }




    /// <inheritdoc cref="IDRSApplication" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct DRSApplicationV1 : IInitializable, IDRSApplication
    {
        internal StructureVersion _Version;
        internal uint _IsPredefined;
        internal UnicodeString _ApplicationName;
        internal UnicodeString _FriendlyName;
        internal UnicodeString _LauncherName;

        /// <summary>
        ///     Creates a new instance of <see cref="DRSApplicationV1" />
        /// </summary>
        /// <param name="applicationName">The application file name.</param>
        /// <param name="friendlyName">The application friendly name.</param>
        /// <param name="launcherName">The application launcher name.</param>
        public DRSApplicationV1(
            string applicationName,
            string friendlyName = null,
            string launcherName = null
        )
        {
            this = typeof(DRSApplicationV1).Instantiate<DRSApplicationV1>();
            IsPredefined = false;
            ApplicationName = applicationName;
            FriendlyName = friendlyName ?? string.Empty;
            LauncherName = launcherName ?? string.Empty;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public bool IsPredefined
        {
            get => _IsPredefined > 0;
            private set => _IsPredefined = value ? 1u : 0u;
        }

        /// <inheritdoc />
        public string ApplicationName
        {
            get => _ApplicationName.Value;
            private set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Name can not be empty or null.");
                }

                _ApplicationName = new UnicodeString(value);
            }
        }

        /// <inheritdoc />
        public string FriendlyName
        {
            get => _FriendlyName.Value;
            private set => _FriendlyName = new UnicodeString(value);
        }

        /// <inheritdoc />
        public string LauncherName
        {
            get => _LauncherName.Value;
            private set => _LauncherName = new UnicodeString(value);
        }
    }

    /// <inheritdoc cref="IDRSApplication" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(2)]
    public struct DRSApplicationV2 : IInitializable, IDRSApplication
    {
        internal const char FileInFolderSeparator = ':';

        internal StructureVersion _Version;
        internal uint _IsPredefined;
        internal UnicodeString _ApplicationName;
        internal UnicodeString _FriendlyName;
        internal UnicodeString _LauncherName;
        internal UnicodeString _FileInFolder;

        /// <summary>
        ///     Creates a new instance of <see cref="DRSApplicationV2" />
        /// </summary>
        /// <param name="applicationName">The application file name.</param>
        /// <param name="friendlyName">The application friendly name.</param>
        /// <param name="launcherName">The application launcher name.</param>
        /// <param name="fileInFolders">The list of files that are necessary to be present in the application parent directory.</param>
        // ReSharper disable once TooManyDependencies
        public DRSApplicationV2(
            string applicationName,
            string friendlyName = null,
            string launcherName = null,
            string[] fileInFolders = null
        )
        {
            this = typeof(DRSApplicationV2).Instantiate<DRSApplicationV2>();
            IsPredefined = false;
            ApplicationName = applicationName;
            FriendlyName = friendlyName ?? string.Empty;
            LauncherName = launcherName ?? string.Empty;
            FilesInFolder = fileInFolders ?? new string[0];
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public bool IsPredefined
        {
            get => _IsPredefined > 0;
            private set => _IsPredefined = value ? 1u : 0u;
        }

        /// <inheritdoc />
        public string ApplicationName
        {
            get => _ApplicationName.Value;
            private set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Name can not be empty or null.");
                }

                _ApplicationName = new UnicodeString(value);
            }
        }

        /// <inheritdoc />
        public string FriendlyName
        {
            get => _FriendlyName.Value;
            private set => _FriendlyName = new UnicodeString(value);
        }

        /// <inheritdoc />
        public string LauncherName
        {
            get => _LauncherName.Value;
            private set => _LauncherName = new UnicodeString(value);
        }

        /// <summary>
        ///     Gets the list of files that are necessary to be present in the application parent directory.
        /// </summary>
        public string[] FilesInFolder
        {
            get => _FileInFolder.Value?.Split(new[] { FileInFolderSeparator }, StringSplitOptions.RemoveEmptyEntries) ??
                   new string[0];
            private set => _FileInFolder = new UnicodeString(string.Join(FileInFolderSeparator.ToString(), value));
        }
    }

    /// <inheritdoc cref="IDRSApplication" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(3)]
    public struct DRSApplicationV3 : IInitializable, IDRSApplication
    {
        internal const char FileInFolderSeparator = DRSApplicationV2.FileInFolderSeparator;

        internal StructureVersion _Version;
        internal uint _IsPredefined;
        internal UnicodeString _ApplicationName;
        internal UnicodeString _FriendlyName;
        internal UnicodeString _LauncherName;
        internal UnicodeString _FileInFolder;
        internal uint _Flags;

        /// <summary>
        ///     Creates a new instance of <see cref="DRSApplicationV3" />
        /// </summary>
        /// <param name="applicationName">The application file name.</param>
        /// <param name="friendlyName">The application friendly name.</param>
        /// <param name="launcherName">The application launcher name.</param>
        /// <param name="fileInFolders">The list of files that are necessary to be present in the application parent directory.</param>
        /// <param name="isMetro">A boolean value indicating if this application is a metro application.</param>
        // ReSharper disable once TooManyDependencies
        public DRSApplicationV3(
            string applicationName,
            string friendlyName = null,
            string launcherName = null,
            string[] fileInFolders = null,
            bool isMetro = false
        )
        {
            this = typeof(DRSApplicationV3).Instantiate<DRSApplicationV3>();
            IsPredefined = false;
            ApplicationName = applicationName;
            FriendlyName = friendlyName ?? string.Empty;
            LauncherName = launcherName ?? string.Empty;
            FilesInFolder = fileInFolders ?? new string[0];
            IsMetroApplication = isMetro;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public bool IsPredefined
        {
            get => _IsPredefined > 0;
            private set => _IsPredefined = value ? 1u : 0u;
        }

        /// <summary>
        ///     Gets a boolean value indicating if this application is a metro application
        /// </summary>
        public bool IsMetroApplication
        {
            get => _Flags.GetBit(0);
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if this application has command line arguments
        /// </summary>
        public bool HasCommandLine
        {
            get => _Flags.GetBit(1); 
            set => _Flags = _Flags.SetBit(1, value);
        }

        /// <inheritdoc />
        public string ApplicationName
        {
            get => _ApplicationName.Value;
            private set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Name can not be empty or null.");
                }

                _ApplicationName = new UnicodeString(value);
            }
        }

        /// <inheritdoc />
        public string FriendlyName
        {
            get => _FriendlyName.Value;
            private set => _FriendlyName = new UnicodeString(value);
        }

        /// <inheritdoc />
        public string LauncherName
        {
            get => _LauncherName.Value;
            private set => _LauncherName = new UnicodeString(value);
        }

        /// <summary>
        ///     Gets the list of files that are necessary to be present in the application parent directory.
        /// </summary>
        public string[] FilesInFolder
        {
            get => _FileInFolder.Value?.Split(new[] { FileInFolderSeparator }, StringSplitOptions.RemoveEmptyEntries) ??
                   new string[0];
            private set => _FileInFolder = new UnicodeString(string.Join(FileInFolderSeparator.ToString(), value));
        }
    }

    /// <inheritdoc cref="IDRSApplication" />
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(4)]
    public struct DRSApplicationV4 : IInitializable, IDRSApplication
    {
        internal const char FileInFolderSeparator = DRSApplicationV3.FileInFolderSeparator;

        internal StructureVersion _Version;
        internal uint _IsPredefined;
        internal UnicodeString _ApplicationName;
        internal UnicodeString _FriendlyName;
        internal UnicodeString _LauncherName;
        internal UnicodeString _FileInFolder;
        internal uint _Flags;
        internal UnicodeString _CommandLine;

        /// <summary>
        ///     Creates a new instance of <see cref="DRSApplicationV4" />
        /// </summary>
        /// <param name="applicationName">The application file name.</param>
        /// <param name="friendlyName">The application friendly name.</param>
        /// <param name="launcherName">The application launcher name.</param>
        /// <param name="fileInFolders">The list of files that are necessary to be present in the application parent directory.</param>
        /// <param name="isMetro">A boolean value indicating if this application is a metro application.</param>
        /// <param name="commandLine">The application's command line arguments.</param>
        // ReSharper disable once TooManyDependencies
        public DRSApplicationV4(
            string applicationName,
            string friendlyName = null,
            string launcherName = null,
            string[] fileInFolders = null,
            bool isMetro = false,
            string commandLine = null
        )
        {
            this = typeof(DRSApplicationV4).Instantiate<DRSApplicationV4>();
            IsPredefined = false;
            ApplicationName = applicationName;
            FriendlyName = friendlyName ?? string.Empty;
            LauncherName = launcherName ?? string.Empty;
            FilesInFolder = fileInFolders ?? new string[0];
            IsMetroApplication = isMetro;
            ApplicationCommandLine = commandLine ?? string.Empty;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }

        /// <inheritdoc />
        public bool IsPredefined
        {
            get => _IsPredefined > 0;
            private set => _IsPredefined = value ? 1u : 0u;
        }

        /// <summary>
        ///     Gets a boolean value indicating if this application is a metro application
        /// </summary>
        public bool IsMetroApplication
        {
            get => _Flags.GetBit(0);
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <summary>
        ///     Gets a boolean value indicating if this application has command line arguments
        /// </summary>
        public bool HasCommandLine
        {
            get => _Flags.GetBit(1);
            set => _Flags = _Flags.SetBit(1, value);
        }

        /// <inheritdoc />
        public string ApplicationName
        {
            get => _ApplicationName.Value;
            private set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("Name can not be empty or null.");
                }

                _ApplicationName = new UnicodeString(value);
            }
        }

        /// <summary>
        ///     Gets the application command line arguments
        /// </summary>
        public string ApplicationCommandLine
        {
            get => (HasCommandLine ? _CommandLine.Value : null) ?? string.Empty;
            private set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _CommandLine = new UnicodeString(null);

                    if (HasCommandLine)
                    {
                        HasCommandLine = false;
                    }
                }
                else
                {
                    _CommandLine = new UnicodeString(value);

                    if (!HasCommandLine)
                    {
                        HasCommandLine = true;
                    }
                }
            }
        }

        /// <inheritdoc />
        public string FriendlyName
        {
            get => _FriendlyName.Value;
            private set => _FriendlyName = new UnicodeString(value);
        }

        /// <inheritdoc />
        public string LauncherName
        {
            get => _LauncherName.Value;
            private set => _LauncherName = new UnicodeString(value);
        }

        /// <summary>
        ///     Gets the list of files that are necessary to be present in the application parent directory.
        /// </summary>
        public string[] FilesInFolder
        {
            get => _FileInFolder.Value?.Split(new[] { FileInFolderSeparator }, StringSplitOptions.RemoveEmptyEntries) ??
                   new string[0];
            private set => _FileInFolder = new UnicodeString(string.Join(FileInFolderSeparator.ToString(), value));
        }
    }

    /// <summary>
    ///     Contains a list of supported GPU series by a NVIDIA driver setting profile
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DRSGPUSupport : IEquatable<DRSGPUSupport>
    {
        internal uint _Flags;

        /// <summary>
        ///     Gets or sets a value indicating if the GeForce line of products are supported
        /// </summary>
        public bool IsGeForceSupported
        {
            get => _Flags.GetBit(0);
            set => _Flags = _Flags.SetBit(0, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating if the Quadro line of products are supported
        /// </summary>
        public bool IsQuadroSupported
        {
            get => _Flags.GetBit(1);
            set => _Flags = _Flags.SetBit(1, value);
        }

        /// <summary>
        ///     Gets or sets a value indicating if the NVS line of products are supported
        /// </summary>
        public bool IsNVSSupported
        {
            get => _Flags.GetBit(2);
            set => _Flags = _Flags.SetBit(2, value);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var supportedGPUs = new List<string>();

            if (IsGeForceSupported)
            {
                supportedGPUs.Add("GeForce");
            }

            if (IsQuadroSupported)
            {
                supportedGPUs.Add("Quadro");
            }

            if (IsNVSSupported)
            {
                supportedGPUs.Add("NVS");
            }

            if (supportedGPUs.Any())
            {
                return $"[{_Flags}] = {string.Join(", ", supportedGPUs)}";
            }

            return $"[{_Flags}]";
        }

        public override bool Equals(object obj) => obj is DRSGPUSupport other && this.Equals(other);
        public bool Equals(DRSGPUSupport other)
        => _Flags == other._Flags;

        public override int GetHashCode()
        {
            return (_Flags).GetHashCode();
        }
        public static bool operator ==(DRSGPUSupport lhs, DRSGPUSupport rhs) => lhs.Equals(rhs);

        public static bool operator !=(DRSGPUSupport lhs, DRSGPUSupport rhs) => !(lhs == rhs);
    }

    /// <summary>
    ///     DRSProfileHandle is a reference to a DRS profile.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DRSProfileHandle : IHandle, IEquatable<DRSProfileHandle>
    {
        internal readonly IntPtr _MemoryAddress;

        private DRSProfileHandle(IntPtr memoryAddress)
        {
            _MemoryAddress = memoryAddress;
        }

        /// <inheritdoc />
        public bool Equals(DRSProfileHandle other)
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

            return obj is DRSProfileHandle handle && Equals(handle);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _MemoryAddress.GetHashCode();
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"DRSProfileHandle #{MemoryAddress.ToInt64()}";
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
        public static bool operator ==(DRSProfileHandle left, DRSProfileHandle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(DRSProfileHandle left, DRSProfileHandle right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        ///     Gets default DRSProfileHandle with a null pointer
        /// </summary>
        public static DRSProfileHandle DefaultHandle
        {
            get => default(DRSProfileHandle);
        }

        /// <summary>
        ///     Gets the default global profile handle
        /// </summary>
        public static DRSProfileHandle DefaultGlobalProfileHandle
        {
            get => new DRSProfileHandle(new IntPtr(-1));
        }
    }

    /// <summary>
    ///     Represents a NVIDIA driver settings profile
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct DRSProfileV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal UnicodeString _ProfileName;
        internal DRSGPUSupport _GPUSupport;
        internal uint _IsPredefined;
        internal uint _NumberOfApplications;
        internal uint _NumberOfSettings;

        /// <summary>
        ///     Creates a new instance of <see cref="DRSProfileV1" /> with the passed name and GPU series support list.
        /// </summary>
        /// <param name="name">The name of the profile.</param>
        /// <param name="gpuSupport">An instance of <see cref="DRSGPUSupport" /> containing the list of supported GPU series.</param>
        public DRSProfileV1(string name, DRSGPUSupport gpuSupport)
        {
            this = typeof(DRSProfileV1).Instantiate<DRSProfileV1>();
            _ProfileName = new UnicodeString(name);
            _GPUSupport = gpuSupport;
        }

        // <inheritdoc />
        public StructureVersion Version
        {
            get => _Version;
            set => _Version = value;
        }


        /// <summary>
        ///     Gets the name of the profile
        /// </summary>
        public string Name
        {
            get => _ProfileName.Value;
            set => _ProfileName.Value = value;
        }

        /// <summary>
        ///     Gets or sets the GPU series support list
        /// </summary>
        public DRSGPUSupport GPUSupport
        {
            get => _GPUSupport;
            set => _GPUSupport = value;
        }

        /// <summary>
        ///     Gets a boolean value indicating if this profile is predefined
        /// </summary>
        public bool IsPredefined
        {
            get => _IsPredefined > 0;
            set
            {
                if (value)
                {
                    _IsPredefined = 0x1;
                }   
                else
                {
                    _IsPredefined = 0;
                }
            }
        }

        /// <summary>
        ///     Gets the number of applications registered under this profile
        /// </summary>
        public int NumberOfApplications
        {
            get => (int)_NumberOfApplications;
            set => _NumberOfApplications = (uint)value;
        }

        /// <summary>
        ///     Gets the number of setting registered under this profile
        /// </summary>
        public int NumberOfSettings
        {
            get => (int)_NumberOfSettings;
            set => _NumberOfSettings = (uint)value;
        }

        public override bool Equals(object obj) => obj is DRSProfileV1 other && this.Equals(other);
        public bool Equals(DRSProfileV1 other)
        => _Version == other._Version &&
            _ProfileName == other._ProfileName &&
            _GPUSupport == other._GPUSupport &&
            _IsPredefined == other._IsPredefined &&
            _NumberOfApplications == other._NumberOfApplications &&
            _NumberOfSettings == other._NumberOfSettings;

        public override int GetHashCode()
        {
            return (_Version, _ProfileName, _GPUSupport, _IsPredefined, _NumberOfApplications, _NumberOfSettings).GetHashCode();
            //return (HasDRSSettings, ProfileInfo, DriverSettings).GetHashCode();
        }
        public static bool operator ==(DRSProfileV1 lhs, DRSProfileV1 rhs) => lhs.Equals(rhs);

        public static bool operator !=(DRSProfileV1 lhs, DRSProfileV1 rhs) => !(lhs == rhs);
    }

    /// <summary>
    ///     DRSSessionHandle is a reference to a DRS session.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DRSSessionHandle : IHandle, IEquatable<DRSSessionHandle>
    {
        internal readonly IntPtr _MemoryAddress;

        /// <inheritdoc />
        public bool Equals(DRSSessionHandle other)
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

            return obj is DRSSessionHandle handle && Equals(handle);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return _MemoryAddress.GetHashCode();
        }


        /// <inheritdoc />
        public override string ToString()
        {
            return $"DRSSessionHandle #{MemoryAddress.ToInt64()}";
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
        public static bool operator ==(DRSSessionHandle left, DRSSessionHandle right)
        {
            return left.Equals(right);
        }

        /// <summary>
        ///     Checks for inequality between two objects of same type
        /// </summary>
        /// <param name="left">The first object</param>
        /// <param name="right">The second object</param>
        /// <returns>true, if both objects are not equal, otherwise false</returns>
        public static bool operator !=(DRSSessionHandle left, DRSSessionHandle right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        ///     Gets default DRSSessionHandle with a null pointer
        /// </summary>
        public static DRSSessionHandle DefaultHandle
        {
            get => default(DRSSessionHandle);
        }
    }

    /// <summary>
    ///     Represents a NVIDIA driver setting
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct DRSSettingV1 : IInitializable
    {
        internal StructureVersion _Version;
        internal UnicodeString _SettingName;
        internal uint _SettingId;
        internal DRSSettingType _SettingType;
        internal DRSSettingLocation _SettingLocation;
        internal uint _IsCurrentPredefined;
        internal uint _IsPredefinedValid;
        internal DRSSettingValue _PredefinedValue;
        internal DRSSettingValue _CurrentValue;

        /// <summary>
        ///     Creates a new instance of <see cref="DRSSettingV1" /> containing the passed value.
        /// </summary>
        /// <param name="id">The setting identification number.</param>
        /// <param name="settingType">The type of the setting's value</param>
        /// <param name="value">The setting's value</param>
        public DRSSettingV1(uint id, DRSSettingType settingType, object value)
        {
            this = typeof(DRSSettingV1).Instantiate<DRSSettingV1>();
            Id = id;
            IsPredefinedValueValid = false;
            _SettingType = settingType;
            CurrentValue = value;
        }

        /// <summary>
        ///     Creates a new instance of <see cref="DRSSettingV1" /> containing the passed value.
        /// </summary>
        /// <param name="id">The setting identification number.</param>
        /// <param name="value">The setting's value</param>
        public DRSSettingV1(uint id, string value) : this(id, DRSSettingType.String, value)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="DRSSettingV1" /> containing the passed value.
        /// </summary>
        /// <param name="id">The setting identification number.</param>
        /// <param name="value">The setting's value</param>
        public DRSSettingV1(uint id, uint value) : this(id, DRSSettingType.Integer, value)
        {
        }

        /// <summary>
        ///     Creates a new instance of <see cref="DRSSettingV1" /> containing the passed value.
        /// </summary>
        /// <param name="id">The setting identification number.</param>
        /// <param name="value">The setting's value</param>
        public DRSSettingV1(uint id, byte[] value) : this(id, DRSSettingType.Binary, value)
        {
        }

        /// <summary>
        ///     Gets the name of the setting
        /// </summary>
        public string Name
        {
            get => _SettingName.Value;
        }

        /// <summary>
        ///     Gets the identification number of the setting
        /// </summary>
        public uint Id
        {
            get => _SettingId;
            private set => _SettingId = value;
        }

        /// <summary>
        ///     Gets the setting's value type
        /// </summary>
        public DRSSettingType SettingType
        {
            get => _SettingType;
            private set => _SettingType = value;
        }

        /// <summary>
        ///     Gets the setting location
        /// </summary>
        public DRSSettingLocation SettingLocation
        {
            get => _SettingLocation;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the current value is the predefined value
        /// </summary>
        public bool IsCurrentValuePredefined
        {
            get => _IsCurrentPredefined > 0;
            private set => _IsCurrentPredefined = value ? 1u : 0u;
        }

        /// <summary>
        ///     Gets a boolean value indicating if the predefined value is available and valid
        /// </summary>
        public bool IsPredefinedValueValid
        {
            get => _IsPredefinedValid > 0;
            private set => _IsPredefinedValid = value ? 1u : 0u;
        }

        /// <summary>
        ///     Returns the predefined value as an integer
        /// </summary>
        /// <returns>An integer representing the predefined value</returns>
        public uint GetPredefinedValueAsInteger()
        {
            return _PredefinedValue.AsInteger();
        }

        /// <summary>
        ///     Returns the predefined value as an array of bytes
        /// </summary>
        /// <returns>An byte array representing the predefined value</returns>
        public byte[] GetPredefinedValueAsBinary()
        {
            return _PredefinedValue.AsBinary();
        }

        /// <summary>
        ///     Returns the predefined value as an unicode string
        /// </summary>
        /// <returns>An unicode string representing the predefined value</returns>
        public string GetPredefinedValueAsUnicodeString()
        {
            return _PredefinedValue.AsUnicodeString();
        }

        /// <summary>
        ///     Gets the setting's predefined value
        /// </summary>
        public object PredefinedValue
        {
            get
            {
                if (!IsPredefinedValueValid)
                {
                    return null;
                }

                switch (_SettingType)
                {
                    case DRSSettingType.Integer:

                        return GetPredefinedValueAsInteger();
                    case DRSSettingType.Binary:

                        return GetPredefinedValueAsBinary();
                    case DRSSettingType.String:
                    case DRSSettingType.UnicodeString:

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
            return _CurrentValue.AsInteger();
        }

        /// <summary>
        ///     Returns the current value as an array of bytes
        /// </summary>
        /// <returns>An byte array representing the current value</returns>
        public byte[] GetCurrentValueAsBinary()
        {
            return _CurrentValue.AsBinary();
        }

        /// <summary>
        ///     Returns the current value as an unicode string
        /// </summary>
        /// <returns>An unicode string representing the current value</returns>
        public string GetCurrentValueAsUnicodeString()
        {
            return _CurrentValue.AsUnicodeString();
        }

        /// <summary>
        ///     Sets the passed value as the current value
        /// </summary>
        /// <param name="value">The new value for the setting</param>
        public void SetCurrentValueAsInteger(uint value)
        {
            if (SettingType != DRSSettingType.Integer)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Passed argument is invalid for this setting.");
            }

            _CurrentValue = new DRSSettingValue(value);
            IsCurrentValuePredefined = IsPredefinedValueValid && (uint)CurrentValue == (uint)PredefinedValue;
        }

        /// <summary>
        ///     Sets the passed value as the current value
        /// </summary>
        /// <param name="value">The new value for the setting</param>
        public void SetCurrentValueAsBinary(byte[] value)
        {
            if (SettingType != DRSSettingType.Binary)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Passed argument is invalid for this setting.");
            }

            _CurrentValue = new DRSSettingValue(value);
            IsCurrentValuePredefined =
                IsPredefinedValueValid &&
                ((byte[])CurrentValue)?.SequenceEqual((byte[])PredefinedValue ?? new byte[0]) == true;
        }

        /// <summary>
        ///     Sets the passed value as the current value
        /// </summary>
        /// <param name="value">The new value for the setting</param>
        public void SetCurrentValueAsUnicodeString(string value)
        {
            if (SettingType != DRSSettingType.UnicodeString)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Passed argument is invalid for this setting.");
            }

            _CurrentValue = new DRSSettingValue(value);
            IsCurrentValuePredefined =
                IsPredefinedValueValid &&
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
                switch (_SettingType)
                {
                    case DRSSettingType.Integer:

                        return GetCurrentValueAsInteger();
                    case DRSSettingType.Binary:

                        return GetCurrentValueAsBinary();
                    case DRSSettingType.String:
                    case DRSSettingType.UnicodeString:

                        return GetCurrentValueAsUnicodeString();
                    default:

                        throw new ArgumentOutOfRangeException(nameof(SettingType));
                }
            }
            private set
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
    }

    /// <summary>
    ///     Represents a setting value
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct DRSSettingValue : IInitializable
    {
        private const int UnicodeStringLength = UnicodeString.UnicodeStringLength;
        private const int BinaryDataMax = 4096;

        // Math.Max(BinaryDataMax + sizeof(uint), UnicodeStringLength * sizeof(ushort))
        private const int FullStructureSize = 4100;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = FullStructureSize, ArraySubType = UnmanagedType.U1)]
        internal byte[] _BinaryValue;

        /// <summary>
        ///     Creates a new instance of <see cref="DRSSettingValue" /> containing the passed unicode string as the value
        /// </summary>
        /// <param name="value">The unicode string value</param>
        public DRSSettingValue(string value)
        {
            if (value?.Length > UnicodeStringLength)
            {
                value = value.Substring(0, UnicodeStringLength);
            }

            _BinaryValue = new byte[FullStructureSize];

            var stringBytes = Encoding.Unicode.GetBytes(value ?? string.Empty);
            Array.Copy(stringBytes, 0, _BinaryValue, 0, Math.Min(stringBytes.Length, _BinaryValue.Length));
        }

        /// <summary>
        ///     Creates a new instance of <see cref="DRSSettingValue" /> containing the passed byte array as the value
        /// </summary>
        /// <param name="value">The byte array value</param>
        public DRSSettingValue(byte[] value)
        {
            _BinaryValue = new byte[FullStructureSize];

            if (value?.Length > 0)
            {
                var arrayLength = Math.Min(value.Length, BinaryDataMax);
                var arrayLengthBytes = BitConverter.GetBytes((uint)arrayLength);
                Array.Copy(arrayLengthBytes, 0, _BinaryValue, 0, arrayLengthBytes.Length);
                Array.Copy(value, 0, _BinaryValue, arrayLengthBytes.Length, arrayLength);
            }
        }

        /// <summary>
        ///     Creates a new instance of <see cref="DRSSettingValue" /> containing the passed integer as the value
        /// </summary>
        /// <param name="value">The integer value</param>
        public DRSSettingValue(uint value)
        {
            _BinaryValue = new byte[FullStructureSize];
            var arrayLengthBytes = BitConverter.GetBytes(value);
            Array.Copy(arrayLengthBytes, 0, _BinaryValue, 0, arrayLengthBytes.Length);
        }

        /// <summary>
        ///     Returns the value as an integer
        /// </summary>
        /// <returns>An integer representing the value</returns>
        public uint AsInteger()
        {
            return BitConverter.ToUInt32(_BinaryValue, 0);
        }

        /// <summary>
        ///     Returns the value as an array of bytes
        /// </summary>
        /// <returns>An array of bytes representing the value</returns>
        public byte[] AsBinary()
        {
            return _BinaryValue.Skip(sizeof(uint)).Take((int)AsInteger()).ToArray();
        }

        /// <summary>
        ///     Returns the value as an unicode string
        /// </summary>
        /// <returns>An unicode string representing the value</returns>
        public string AsUnicodeString()
        {
            return Encoding.Unicode.GetString(_BinaryValue).TrimEnd('\0');
        }
    }

    /// <summary>
    ///     Contains a list of all possible values for a setting as well as its default value
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    [StructureVersion(1)]
    public struct DRSSettingValues : IInitializable
    {
        internal const int MaximumNumberOfValues = 100;
        internal StructureVersion _Version;
        internal uint _NumberOfValues;
        internal DRSSettingType _SettingType;
        internal DRSSettingValue _DefaultValue;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MaximumNumberOfValues)]
        internal DRSSettingValue[] _Values;

        /// <summary>
        ///     Gets the setting's value type
        /// </summary>
        public DRSSettingType SettingType
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
                    case DRSSettingType.Integer:

                        return ValuesAsInteger().Cast<object>().ToArray();
                    case DRSSettingType.Binary:

                        return ValuesAsBinary().Cast<object>().ToArray();
                    case DRSSettingType.String:
                    case DRSSettingType.UnicodeString:

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
                    case DRSSettingType.Integer:

                        return DefaultValueAsInteger();
                    case DRSSettingType.Binary:

                        return DefaultValueAsBinary();
                    case DRSSettingType.String:
                    case DRSSettingType.UnicodeString:

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
            return _DefaultValue.AsInteger();
        }

        /// <summary>
        ///     Returns the default value as a byte array
        /// </summary>
        /// <returns>An array of bytes representing the default value</returns>
        public byte[] DefaultValueAsBinary()
        {
            return _DefaultValue.AsBinary();
        }

        /// <summary>
        ///     Returns the default value as an unicode string
        /// </summary>
        /// <returns>A string representing the default value</returns>
        public string DefaultValueAsUnicodeString()
        {
            return _DefaultValue.AsUnicodeString();
        }

        /// <summary>
        ///     Returns the setting's possible values as an array of integers
        /// </summary>
        /// <returns>An array of integers representing the possible values</returns>
        public uint[] ValuesAsInteger()
        {
            return _Values.Take((int)_NumberOfValues).Select(value => value.AsInteger()).ToArray();
        }

        /// <summary>
        ///     Returns the setting's possible values as an array of byte arrays
        /// </summary>
        /// <returns>An array of byte arrays representing the possible values</returns>
        public byte[][] ValuesAsBinary()
        {
            return _Values.Take((int)_NumberOfValues).Select(value => value.AsBinary()).ToArray();
        }

        /// <summary>
        ///     Returns the setting's possible values as an array of unicode strings
        /// </summary>
        /// <returns>An array of unicode strings representing the possible values</returns>
        public string[] ValuesAsUnicodeString()
        {
            return _Values.Take((int)_NumberOfValues).Select(value => value.AsUnicodeString()).ToArray();
        }
    }

}
