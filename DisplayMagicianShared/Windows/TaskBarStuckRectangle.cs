using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DisplayMagicianShared;
using Microsoft.Win32;
using Newtonsoft.Json;

// This file is taken from Soroush Falahati's amazing HeliosDisplayManagement software
// available at https://github.com/falahati/HeliosDisplayManagement

// Modifications made by Terry MacDonald

namespace DisplayMagicianShared.Windows
{
    public class TaskBarStuckRectangle
    {
        public enum TaskBarForcedEdge : UInt32
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3,
            None = 9999
        }

        public enum TaskBarEdge : UInt32
        {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        [Flags]
        public enum TaskBarOptions : UInt32
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

        public byte[] BinaryBackup { get; set; }

        public string DevicePath { get; set; }

        public bool MainScreen { get; set; }

        [JsonIgnore]
        public UInt32 DPI
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

                return (TaskBarEdge)BitConverter.ToUInt32(Binary, 12);
            }
            set
            {
                if (Binary.Length < 16)
                {
                    return;
                }

                var bytes = BitConverter.GetBytes((uint)value);
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

                return (TaskBarOptions)BitConverter.ToUInt32(Binary, 8);
            }
            set
            {
                if (Binary.Length < 12)
                {
                    return;
                }

                var bytes = BitConverter.GetBytes((uint)value);
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

        public override bool Equals(object obj) => obj is TaskBarStuckRectangle other && this.Equals(other);
        public bool Equals(TaskBarStuckRectangle other)
        {
            // We return all the fields
            return Version == other.Version &&
                MainScreen == other.MainScreen &&
                DevicePath == other.DevicePath &&
                Edge == other.Edge &&
                Location == other.Location;
                // &&
                //Xor(Binary, other.Binary);
        }
        


        public override int GetHashCode()
        {
            //return (Version, MainScreen, DevicePath, Binary).GetHashCode();
            return (Version, MainScreen, DevicePath, Edge, Location).GetHashCode();
        }
        public static bool operator ==(TaskBarStuckRectangle lhs, TaskBarStuckRectangle rhs) => lhs.Equals(rhs);

        public static bool operator !=(TaskBarStuckRectangle lhs, TaskBarStuckRectangle rhs) => !(lhs == rhs);

        static bool Xor(byte[] a, byte[] b)

        {

            int x = a.Length ^ b.Length;

            for (int i = 0; i < a.Length && i < b.Length; ++i)

            {

                x |= a[i] ^ b[i];

            }

            return x == 0;

        }


        public static List<TaskBarStuckRectangle> GetCurrent(List<string> displayIdentifiers)
        {
            List<TaskBarStuckRectangle> taskBarStuckRectangles = new List<TaskBarStuckRectangle>();

            int version = 2;
            string address = "";
            address = string.Format(MultiDisplayAddress, version);
            if (Registry.CurrentUser.OpenSubKey(address) == null)
            {
                // If it's not version 2, then try version 3
                version = 3;
                address = string.Format(MultiDisplayAddress, version);
                if (Registry.CurrentUser.OpenSubKey(address) == null)
                {
                    // It's not v2 or v3, so error
                    version = -1;
                }
            }

            if (version >= 2)
            {
                foreach (string displayId in displayIdentifiers)
                {
                    // e.g. "WINAPI|\\\\?\\PCI#VEN_10DE&DEV_2482&SUBSYS_408E1458&REV_A1#4&2283f625&0&0019#{5b45201d-f2f2-4f3b-85bb-30ff1f953599}|DISPLAYCONFIG_OUTPUT_TECHNOLOGY_DVI|54074|4318|\\\\?\\DISPLAY#NVS10DE#5&2b46c695&0&UID185344#{e6f07b5f-ee97-4a90-b076-33f57bf4eaa7}|NV Surround"
                    string[] winapiLine = displayId.Split('|');
                    string pattern = @"DISPLAY\#(.*)\#\{";
                    Match match = Regex.Match(winapiLine[5], pattern);
                    if (match.Success)
                    {
                        TaskBarStuckRectangle taskBarStuckRectangle = new TaskBarStuckRectangle();
                        string tbStuckRectKey = match.Groups[1].Value;
                        using (var key = Registry.CurrentUser.OpenSubKey(
                            address,
                            RegistryKeyPermissionCheck.ReadSubTree))
                        {
                            var settings = key?.GetValue(tbStuckRectKey) as byte[];
                            if (settings?.Length > 0)
                            {
                                taskBarStuckRectangle = new TaskBarStuckRectangle
                                {
                                    MainScreen = false,
                                    DevicePath = tbStuckRectKey,
                                    Binary = settings,
                                    BinaryBackup = settings,
                                    Version = version
                                };
                                taskBarStuckRectangles.Add(taskBarStuckRectangle);
                                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: The taskbar for {taskBarStuckRectangle.DevicePath} is against the {taskBarStuckRectangle.Edge} edge, is positioned at ({taskBarStuckRectangle.Location.X},{taskBarStuckRectangle.Location.Y}) and is {taskBarStuckRectangle.Location.Width}x{taskBarStuckRectangle.Location.Height} in size.");
                            }
                            else
                            {
                                SharedLogger.logger.Trace($"WinLibrary/GetWindowsDisplayConfig: Unable to get the TaskBarStuckRectangle for {displayId}.");
                            }
                        }
                    }
                    else
                    {
                        SharedLogger.logger.Warn($"WinLibrary/GetWindowsDisplayConfig: We were unable to figure out the DevicePath for the '{displayId}' display identifier.");
                    }
                }
            }

            version = 2;
            address = string.Format(MainDisplayAddress, version);
            if (Registry.CurrentUser.OpenSubKey(address) == null)
            {
                // If it's not version 2, then try version 3
                version = 3;
                address = string.Format(MainDisplayAddress, version);
                if (Registry.CurrentUser.OpenSubKey(address) == null)
                {
                    // It's not v2 or v3, so error
                    version = -1;
                }
            }

            if (version >= 2)
            {
                // Grab the main screen taskbar placement
                try
                {
                    using (var key = Registry.CurrentUser.OpenSubKey(
                        address,
                        RegistryKeyPermissionCheck.ReadSubTree))
                    {
                        var settings = key?.GetValue("Settings") as byte[];

                        if (settings?.Length > 0)
                        {
                            TaskBarStuckRectangle taskBarStuckRectangle = new TaskBarStuckRectangle
                            {
                                MainScreen = true,
                                DevicePath = "Settings",
                                Binary = settings,
                                BinaryBackup = settings,
                                Version = version
                            };
                            taskBarStuckRectangles.Add(taskBarStuckRectangle);
                        }
                    }
                }
                catch (Exception ex)
                {
                    SharedLogger.logger.Error(ex, $"TaskBarStuckRectangle/GetCurrent: Unable to read the Main Screen TaskBarStuckRectangle registry settings due to an exception!");
                }
            }


            return taskBarStuckRectangles;

        }

        public static bool ForceTaskBarIfNeeded(ref List<TaskBarStuckRectangle> taskBarStuckRectangles, TaskBarForcedEdge forcedEdge = TaskBarForcedEdge.None)
        {
            try
            {
                if (forcedEdge != TaskBarForcedEdge.None)
                {
                    for (int i = 0; i < taskBarStuckRectangles.Count; i++)
                    {
                        // Force the taskbar change
                        taskBarStuckRectangles[i].Edge = (TaskBarEdge)forcedEdge;
                        taskBarStuckRectangles[i].Location = Rectangle.Empty;
                    }
                }
                else if (forcedEdge == TaskBarForcedEdge.None)
                {
                    // Revert the forced taskbar change from the backup
                    for (int i = 0; i < taskBarStuckRectangles.Count; i++)
                    {
                        taskBarStuckRectangles[i].Binary = taskBarStuckRectangles[i].BinaryBackup;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public static bool Apply(List<TaskBarStuckRectangle> taskBarStuckRectangles, TaskBarForcedEdge forcedEdge = TaskBarForcedEdge.None)
        {
            string address;
            if (taskBarStuckRectangles.Count < 1)
            {
                SharedLogger.logger.Trace($"TaskBarStuckRectangle/Apply: There are no TaskBarStuckRectangle registry settings to apply! This taskbar configuration is invalid.");
                return false;
            }

            foreach (TaskBarStuckRectangle tbsr in taskBarStuckRectangles)
            {
                if (tbsr.Version >= 2 && tbsr.Version <= 3)
                {
                    if (!tbsr.MainScreen)
                    {
                        address = string.Format(MultiDisplayAddress, tbsr.Version);
                        // Grab the main screen taskbar placement
                        try
                        {
                            using (var key = Registry.CurrentUser.OpenSubKey(
                                address,
                                RegistryKeyPermissionCheck.ReadWriteSubTree))
                            {
                                key.SetValue(tbsr.DevicePath, tbsr.Binary);
                                SharedLogger.logger.Trace($"TaskBarStuckRectangle/Apply: Successfully applied TaskBarStuckRectangle registry settings for the {tbsr.DevicePath} Screen!");
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"TaskBarStuckRectangle/GetCurrent: Unable to set the {tbsr.DevicePath} TaskBarStuckRectangle registry settings due to an exception!");
                        }
                    }
                }
                else
                {
                    SharedLogger.logger.Error($"TaskBarStuckRectangle/GetCurrent: Unable to set the {tbsr.DevicePath} TaskBarStuckRectangle registry settings as the version isn't v2 or v3!");
                }
            }

            // Tell Windows to refresh the Windows Taskbar (which will only refresh the non-main screen)
            Utils.SendNotifyMessage((IntPtr)Utils.HWND_BROADCAST, Utils.WM_SETTINGCHANGE, (UIntPtr)Utils.NULL, "TraySettings");

            foreach (TaskBarStuckRectangle tbsr in taskBarStuckRectangles)
            {
                if (tbsr.Version >= 2 && tbsr.Version <= 3)
                {
                    if (tbsr.MainScreen)
                    {

                        address = string.Format(MainDisplayAddress, tbsr.Version);
                        // Grab the main screen taskbar placement
                        try
                        {
                            using (var key2 = Registry.CurrentUser.OpenSubKey(
                                address,
                                RegistryKeyPermissionCheck.ReadWriteSubTree))
                            {
                                key2.SetValue("Settings", tbsr.Binary);
                                SharedLogger.logger.Trace($"TaskBarStuckRectangle/Apply: Successfully applied TaskBarStuckRectangle registry settings for the Main Screen!");
                            }
                        }
                        catch (Exception ex)
                        {
                            SharedLogger.logger.Error(ex, $"TaskBarStuckRectangle/GetCurrent: Unable to set the Main Screen TaskBarStuckRectangle registry settings due to an exception!");
                        }
                    }
                }
                else
                {
                    SharedLogger.logger.Error($"TaskBarStuckRectangle/GetCurrent: Unable to set the Main Screen TaskBarStuckRectangle registry settings as the version isn't v2 or v3!");
                }
            }

            // Tell Windows to refresh the Windows Taskbar (which will only refresh the non-main screen)
            Utils.SendNotifyMessage((IntPtr)Utils.HWND_BROADCAST, Utils.WM_SETTINGCHANGE, (UIntPtr)Utils.NULL, "TraySettings");

            Task.Delay(2000);

            // This will refresh the main screen as well. No idea why the above notification doesn't update the main screen too :/)
            //RestartManagerSession.RestartExplorer();

            return true;
        }
    }
}