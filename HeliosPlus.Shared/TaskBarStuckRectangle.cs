using System;
using System.Collections.Generic;
using System.Drawing;
using WindowsDisplayAPI.DisplayConfig;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace HeliosPlus.Shared
{
    public class TaskBarStuckRectangle
    {
        public enum TaskBarEdge : uint
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        [Flags]
        public enum TaskBarOptions : uint
        {
            None = 0,
            AutoHide = 1 << 0,
            KeepOnTop = 1 << 1,
            UseSmallIcons = 1 << 2,
            HideClock = 1 << 3,
            HideVolume = 1 << 4,
            HideNetwork = 1 << 5,
            HidePower = 1 << 6,
            WindowPreview = 1 << 7,
            Unknown1 = 1 << 8,
            Unknown2 = 1 << 9,
            HideActionCenter = 1 << 10,
            Unknown3 = 1 << 11,
            HideLocation = 1 << 12,
            HideLanguageBar = 1 << 13
        }

        private const string MainDisplayAddress =
            "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\StuckRects{0:D}";

        private const string MultiDisplayAddress =
            "Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\MMStuckRects{0:D}";

        private static readonly Dictionary<int, byte[]> Headers = new Dictionary<int, byte[]>
        {
            {2, new byte[] {0x28, 0x00, 0x00, 0x00, 0xFF, 0xFF, 0xFF, 0xFF}},
            {3, new byte[] {0x30, 0x00, 0x00, 0x00, 0xFE, 0xFF, 0xFF, 0xFF}}
        };

        public TaskBarStuckRectangle(int version, string devicePath) : this(version)
        {
            DevicePath = devicePath;
        }

        public TaskBarStuckRectangle(int version)
        {
            if (!Headers.ContainsKey(version))
            {
                throw new ArgumentException(@"Invalid version number specified.", nameof(version));
            }

            Version = version;
            DevicePath = null;

            Binary = new byte[Headers[Version][0]];
            Array.Copy(Headers[Version], 0, Binary, 0, Headers[Version].Length);

            DPI = 96;
            Rows = 1;
            Location = Rectangle.Empty;
            MinSize = Size.Empty;
            Edge = TaskBarEdge.Bottom;
            Options = TaskBarOptions.KeepOnTop;
        }

        public TaskBarStuckRectangle()
        {
        }

        public byte[] Binary { get; set; }

        public string DevicePath { get; set; }

        [JsonIgnore]
        public uint DPI
        {
            get
            {
                if (Binary.Length < 44)
                {
                    return 0;
                }

                return BitConverter.ToUInt32(Binary, 40);
            }
            set
            {
                if (Binary.Length < 44)
                {
                    return;
                }

                var bytes = BitConverter.GetBytes(value);
                Array.Copy(bytes, 0, Binary, 40, 4);
            }
        }

        [JsonIgnore]
        public TaskBarEdge Edge
        {
            get
            {
                if (Binary.Length < 16)
                {
                    return TaskBarEdge.Bottom;
                }

                return (TaskBarEdge) BitConverter.ToUInt32(Binary, 12);
            }
            set
            {
                if (Binary.Length < 16)
                {
                    return;
                }

                var bytes = BitConverter.GetBytes((uint) value);
                Array.Copy(bytes, 0, Binary, 12, 4);
            }
        }

        [JsonIgnore]
        public Rectangle Location
        {
            get
            {
                if (Binary.Length < 40)
                {
                    return Rectangle.Empty;
                }

                var left = BitConverter.ToInt32(Binary, 24);
                var top = BitConverter.ToInt32(Binary, 28);
                var right = BitConverter.ToInt32(Binary, 32);
                var bottom = BitConverter.ToInt32(Binary, 36);

                return Rectangle.FromLTRB(left, top, right, bottom);
            }
            set
            {
                if (Binary.Length < 40)
                {
                    return;
                }

                var bytes = BitConverter.GetBytes(value.Left);
                Array.Copy(bytes, 0, Binary, 24, 4);

                bytes = BitConverter.GetBytes(value.Top);
                Array.Copy(bytes, 0, Binary, 28, 4);

                bytes = BitConverter.GetBytes(value.Right);
                Array.Copy(bytes, 0, Binary, 32, 4);

                bytes = BitConverter.GetBytes(value.Bottom);
                Array.Copy(bytes, 0, Binary, 36, 4);
            }
        }

        [JsonIgnore]
        public Size MinSize
        {
            get
            {
                if (Binary.Length < 24)
                {
                    return Size.Empty;
                }

                var width = BitConverter.ToInt32(Binary, 16);
                var height = BitConverter.ToInt32(Binary, 20);

                return new Size(width, height);
            }
            set
            {
                if (Binary.Length < 24)
                {
                    return;
                }

                var bytes = BitConverter.GetBytes(value.Width);
                Array.Copy(bytes, 0, Binary, 16, 4);

                bytes = BitConverter.GetBytes(value.Height);
                Array.Copy(bytes, 0, Binary, 20, 4);
            }
        }

        [JsonIgnore]
        public TaskBarOptions Options
        {
            get
            {
                if (Binary.Length < 12)
                {
                    return 0;
                }

                return (TaskBarOptions) BitConverter.ToUInt32(Binary, 8);
            }
            set
            {
                if (Binary.Length < 12)
                {
                    return;
                }

                var bytes = BitConverter.GetBytes((uint) value);
                Array.Copy(bytes, 0, Binary, 8, 4);
            }
        }

        [JsonIgnore]
        public uint Rows
        {
            get
            {
                if (Binary.Length < 48)
                {
                    return 1;
                }

                return BitConverter.ToUInt32(Binary, 44);
            }
            set
            {
                if (Binary.Length < 48)
                {
                    return;
                }

                var bytes = BitConverter.GetBytes(value);
                Array.Copy(bytes, 0, Binary, 44, 4);
            }
        }

        public int Version { get; set; }

        public static TaskBarStuckRectangle GetCurrent()
        {
            return GetCurrent((string) null);
        }

        public static TaskBarStuckRectangle GetCurrent(PathDisplayTarget pathTargetInfo)
        {
            var devicePath = pathTargetInfo?.DevicePath;
            var index = devicePath?.IndexOf("{", StringComparison.InvariantCultureIgnoreCase);

            if (index > 0)
            {
                devicePath = devicePath.Substring(0, index.Value).TrimEnd('#');
            }

            index = devicePath?.IndexOf("#", StringComparison.InvariantCultureIgnoreCase);

            if (index > 0)
            {
                devicePath = devicePath.Substring(index.Value).TrimStart('#');
            }

            return GetCurrent(devicePath);
        }

        public static TaskBarStuckRectangle GetCurrent(string devicePath)
        {
            var stuckRectanglesVersion = 0;
            byte[] stuckRectanglesBinary = null;

            // Try to extract the latest version of StuckRectangles available on the User Registry
            foreach (var version in Headers.Keys)
            {
                try
                {
                    var address = devicePath != null
                        ? string.Format(MultiDisplayAddress, version)
                        : string.Format(MainDisplayAddress, version);

                    using (var key = Registry.CurrentUser.OpenSubKey(
                        address,
                        RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        var settings = key?.GetValue(devicePath ?? "Settings") as byte[];

                        if (settings?.Length > 0)
                        {
                            stuckRectanglesBinary = settings;
                            stuckRectanglesVersion = version;
                        }
                    }
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            if (stuckRectanglesVersion == 0 || stuckRectanglesBinary == null)
            {
                return null;
            }

            return new TaskBarStuckRectangle
            {
                DevicePath = devicePath,
                Binary = stuckRectanglesBinary,
                Version = stuckRectanglesVersion
            };
        }

        public bool Apply()
        {
            if (Binary == null ||
                Binary.Length == 0 ||
                Version <= 0)
            {
                throw new InvalidOperationException();
            }

            var address = DevicePath != null
                ? string.Format(MultiDisplayAddress, Version)
                : string.Format(MainDisplayAddress, Version);

            using (var stuckRectanglesKey = Registry.CurrentUser.OpenSubKey(
                address,
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (stuckRectanglesKey == null)
                {
                    return false;
                }

                try
                {
                    stuckRectanglesKey.SetValue(DevicePath ?? "Settings", Binary);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            return true;
        }
    }
}