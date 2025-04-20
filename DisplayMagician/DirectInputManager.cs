using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;
using System.Windows.Input;
using Vortice.DirectInput;
using Microsoft.WindowsAPICodePack.Sensors;


namespace DisplayMagician
{
    // Enumeration constants from dinput.h
    public enum DeviceClass : uint
    {
        All = 0,
        Device = 1,
        Gamepad = 2,
        Keyboard = 3,  // DI8DEVCLASS_KEYBOARD
        Pointer = 4,
        Joystick = 5
    }

    public enum HotkeyTask
    {
        None,
        ChangeDisplayProfile,
        RunGameShortcut,
        OpenShortcutLibraryWindow,
        OpenMainWindow,
        OpenDisplayProfileWindow,
        ExitApplication,
        // MinimizeApplication,
    }



    public struct HotkeyBinding
    {
        public DeviceClass DeviceClass;
        public int Code; // DI scan code or button index
        public HotkeyTask Action;
        public Guid TargetId; // profile or shortcut UUID

        public HotkeyBinding()
        {
            DeviceClass = DeviceClass.Keyboard;
            Code = 0;
            Action = HotkeyTask.None;
            TargetId = Guid.Empty;
        }

        public HotkeyBinding(DeviceClass deviceClass, int code, HotkeyTask action, Guid targetId)
        {
            DeviceClass = deviceClass;
            Code = code;
            Action = action;
            TargetId = targetId;
        }
    }

    /// <summary>
    /// GUIDs for the standard DirectInput object types, taken from dinput.h
    /// </summary>
    internal static class GUIDs
    {
        public static readonly Guid GUID_XAxis = new Guid(0xA36D02E0, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
        public static readonly Guid GUID_YAxis = new Guid(0xA36D02E1, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
        public static readonly Guid GUID_ZAxis = new Guid(0xA36D02E2, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
        public static readonly Guid GUID_RxAxis = new Guid(0xA36D02F4, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
        public static readonly Guid GUID_RyAxis = new Guid(0xA36D02F5, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
        public static readonly Guid GUID_RzAxis = new Guid(0xA36D02E3, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
        public static readonly Guid GUID_Slider = new Guid(0xA36D02E4, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
        public static readonly Guid GUID_Button = new Guid(0xA36D02F0, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
        public static readonly Guid GUID_Key = new Guid(0x55728220, 0xD33C, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
        public static readonly Guid GUID_POV = new Guid(0xA36D02F2, 0xC9F3, 0x11CF, 0xBF, 0xC7, 0x44, 0x45, 0x53, 0x54, 0x00, 0x00);
    }


    public class DirectInputManager : IDisposable
    {
        private readonly IDirectInput8 _dinput;
        private readonly List<IDirectInputDevice8> _devices = new();
        private readonly Dictionary<IDirectInputDevice8, DeviceClass> _deviceClasses = new();
        private readonly Thread _pollThread;
        private bool _running;

        public event Action<byte[]> KeyboardStateChanged;
        public event Action<byte[]> JoystickStateChanged;

        // Pinned pointers to our custom DIDATAFORMAT blocks
        private static readonly IntPtr _pKeyboardFormat;
        private static readonly IntPtr _pJoystickFormat;

        // Build both formats once when the type is first used
        static DirectInputManager()
        {
            // --- Build keyboard format (256 keys) ---
            int keyObjSize = Marshal.SizeOf<DIOBJECTDATAFORMAT>();
            int keyFmtSize = Marshal.SizeOf<DIDATAFORMAT>();
            var keyObjs = new DIOBJECTDATAFORMAT[256];
            var guidBtn = PinGuid(GUIDs.GUID_Button);

            for (int i = 0; i < 256; i++)
            {
                keyObjs[i] = new DIOBJECTDATAFORMAT
                {
                    pguid = guidBtn,
                    dwOfs = (uint)(DIMOFS_BUTTON0 + i),
                    dwType = DIDFT_MAKEINSTANCE((uint)i) | DIDFT_PSHBUTTON,
                    dwFlags = 0
                };
            }

            IntPtr kbBlock = Marshal.AllocHGlobal(keyFmtSize + keyObjSize * 256);
            var kbFmt = new DIDATAFORMAT
            {
                dwSize = (uint)keyFmtSize,
                dwObjSize = (uint)keyObjSize,
                dwFlags = DIDF_RELAXIS,
                dwDataSize = 256,
                dwNumObjs = 256,
                rgodf = IntPtr.Add(kbBlock, keyFmtSize)
            };
            Marshal.StructureToPtr(kbFmt, kbBlock, false);
            IntPtr ptr = kbFmt.rgodf;
            foreach (var odf in keyObjs)
            {
                Marshal.StructureToPtr(odf, ptr, false);
                ptr = IntPtr.Add(ptr, keyObjSize);
            }
            _pKeyboardFormat = kbBlock;

            // --- Build joystick format (DIJOYSTATE2) ---
            var joyList = new List<DIOBJECTDATAFORMAT>();
            void Add(Guid g, uint ofs, uint typeFlags)
                => joyList.Add(new DIOBJECTDATAFORMAT
                {
                    pguid = PinGuid(g),
                    dwOfs = ofs,
                    dwType = typeFlags,
                    dwFlags = 0
                });

            Add(GUIDs.GUID_XAxis, DIMOFS_X, DIDFT_AXIS);
            Add(GUIDs.GUID_YAxis, DIMOFS_Y, DIDFT_AXIS);
            Add(GUIDs.GUID_ZAxis, DIMOFS_Z, DIDFT_OPTIONAL | DIDFT_AXIS);
            Add(GUIDs.GUID_RxAxis, DIMOFS_RX, DIDFT_AXIS);
            Add(GUIDs.GUID_RyAxis, DIMOFS_RY, DIDFT_AXIS);
            Add(GUIDs.GUID_RzAxis, DIMOFS_RZ, DIDFT_OPTIONAL | DIDFT_AXIS);
            Add(GUIDs.GUID_Slider, DIMOFS_SLIDER0, DIDFT_AXIS);
            Add(GUIDs.GUID_Slider, DIMOFS_SLIDER1, DIDFT_AXIS);
            Add(GUIDs.GUID_POV, DIMOFS_POV0, DIDFT_POV);
            // Buttons 0–15
            for (uint b = 0; b < 16; b++)
                Add(GUIDs.GUID_Button, DIMOFS_BUTTON0 + b,
                    (b < 2 ? 0 : DIDFT_OPTIONAL) | DIDFT_BUTTON);

            int joyObjSize = Marshal.SizeOf<DIOBJECTDATAFORMAT>();
            int joyFmtSize = Marshal.SizeOf<DIDATAFORMAT>();
            IntPtr joyBlock = Marshal.AllocHGlobal(joyFmtSize + joyObjSize * joyList.Count);

            var joyFmt = new DIDATAFORMAT
            {
                dwSize = (uint)joyFmtSize,
                dwObjSize = (uint)joyObjSize,
                dwFlags = DIDF_ABSAXIS,
                dwDataSize = (uint)Marshal.SizeOf<DIJOYSTATE2>(),
                dwNumObjs = (uint)joyList.Count,
                rgodf = IntPtr.Add(joyBlock, joyFmtSize)
            };
            Marshal.StructureToPtr(joyFmt, joyBlock, false);
            ptr = joyFmt.rgodf;
            foreach (var odf in joyList)
            {
                Marshal.StructureToPtr(odf, ptr, false);
                ptr = IntPtr.Add(ptr, joyObjSize);
            }
            _pJoystickFormat = joyBlock;
        }

        public DirectInputManager(IntPtr hwnd, bool startPolling = true)
        {
            // Create IDirectInput8
            IntPtr diPtr;
            int hr = NativeMethods.DirectInput8Create(
                Process.GetCurrentProcess().MainModule.BaseAddress,
                0x0800,
                NativeMethods.IID_IDirectInput8,
                out diPtr,
                IntPtr.Zero
            );
            if (hr < 0) throw new ExternalException("DirectInput8Create failed", hr);
            _dinput = (IDirectInput8)Marshal.GetObjectForIUnknown(diPtr);

            // Acquire keyboard / gamepad / joystick
            AcquireDevices(DeviceClass.Keyboard, hwnd);
            AcquireDevices(DeviceClass.Gamepad, hwnd);
            AcquireDevices(DeviceClass.Joystick, hwnd);

            // Start poll thread
            _pollThread = new Thread(PollLoop) { IsBackground = true };
            if (startPolling)
            {
                _running = true;
                _pollThread.Start();
            }
        }

        public void StartPolling()
        {
            if (!_running)
            {
                _running = true;
                _pollThread.Start();
            }
        }

        public void StopPolling()
        {
            if (_running)
            {
                _running = false;
                _pollThread.Join();
            }
        }

        private void AcquireDevices(DeviceClass cls, IntPtr hwnd)
        {
            var enumCb = new DIEnumDevicesCallback((ref DIDEVICEINSTANCE ddi, IntPtr ctx) =>
            {
                ddi.dwSize = (uint)Marshal.SizeOf<DIDEVICEINSTANCE>();
                Guid instanceGuid = ddi.guidInstance;

                // Create the device
                _dinput.CreateDevice(ref instanceGuid, out IntPtr devPtr, IntPtr.Zero);
                var dev = (IDirectInputDevice8)Marshal.GetObjectForIUnknown(devPtr);

                // Enumerate real objects
                var objects = new List<DIDEVICEOBJECTINSTANCE>();
                var objCb = new DIEnumDeviceObjectsCallback((ref DIDEVICEOBJECTINSTANCE doi, IntPtr c) =>
                {
                    doi.dwSize = (uint)Marshal.SizeOf<DIDEVICEOBJECTINSTANCE>();
                    objects.Add(doi);
                    return 1;
                });
                const uint DIDFT_ALL = 0x00000000;
                const uint DIDFT_AXIS = 0x00000002;    // Absolute & relative axes
                const uint DIDFT_BUTTON = 0x00001000;    // Buttons
                const uint DIDFT_POV = 0x00000040;    // POV hats
                uint flags = DIDFT_AXIS | DIDFT_BUTTON | DIDFT_POV;
                dev.EnumObjects(
                    Marshal.GetFunctionPointerForDelegate(objCb),
                    IntPtr.Zero,
                    flags
                );

                // 4) Build a matching DIOBJECTDATAFORMAT array
                int objSize = Marshal.SizeOf<DIOBJECTDATAFORMAT>();
                var odf = objects
                    .Select(doi => new DIOBJECTDATAFORMAT
                    {
                        pguid = PinGuid(doi.guidType),
                        dwOfs = doi.dwOfs,
                        dwType = doi.dwType,
                        dwFlags = doi.dwFlags
                    })
                    .ToArray();

                // 5) Allocate & populate a DIDATAFORMAT block
                int fmtSize = Marshal.SizeOf<DIDATAFORMAT>();
                IntPtr block = Marshal.AllocHGlobal(fmtSize + objSize * odf.Length);

                var fmt = new DIDATAFORMAT
                {
                    dwSize = (uint)fmtSize,
                    dwObjSize = (uint)objSize,
                    dwFlags = (cls == DeviceClass.Keyboard ? 0x00000002u : 0x00000001u),
                    dwDataSize = (uint)(cls == DeviceClass.Keyboard
                                        ? 256
                                        : Marshal.SizeOf<DIJOYSTATE2>()),
                    dwNumObjs = (uint)odf.Length,
                    rgodf = IntPtr.Add(block, fmtSize)
                };
                Marshal.StructureToPtr(fmt, block, false);

                IntPtr cur = fmt.rgodf;
                foreach (var o in odf)
                {
                    Marshal.StructureToPtr(o, cur, false);
                    cur = IntPtr.Add(cur, objSize);
                }

                dev.Unacquire();

                // Set the tailored format
                int hr = dev.SetDataFormat(block);
                if (hr < 0) throw new ExternalException("SetDataFormat failed", hr);

                // Co-op level & acquire
                const uint DISCL_FOREGROUND = 0x00000001;
                const uint DISCL_NONEXCLUSIVE = 0x00000020;
                dev.SetCooperativeLevel(hwnd, DISCL_FOREGROUND | DISCL_NONEXCLUSIVE);
                dev.Acquire();

                _devices.Add(dev);
                _deviceClasses[dev] = cls;
                return 1;
            });

            IntPtr enumCbPtr = Marshal.GetFunctionPointerForDelegate(enumCb);
            const uint DIEDFL_ATTACHEDONLY = 0x00000001;
            _dinput.EnumDevices((uint)cls, enumCbPtr, IntPtr.Zero, DIEDFL_ATTACHEDONLY);
        }

        private void PollLoop()
        {
            var prev = new Dictionary<IDirectInputDevice8, byte[]>();
            while (_running)
            {
                foreach (var d in _devices)
                {
                    byte[] buf = new byte[256];
                    var h = GCHandle.Alloc(buf, GCHandleType.Pinned);
                    try
                    {
                        int hr = d.GetDeviceState(buf.Length, h.AddrOfPinnedObject());
                        if (hr >= 0)
                        {
                            if (!_deviceClasses.TryGetValue(d, out var cls)) cls = DeviceClass.Keyboard;
                            if (!prev.TryGetValue(d, out var p) || !buf.SequenceEqual(p))
                            {
                                if (cls == DeviceClass.Keyboard)
                                    KeyboardStateChanged?.Invoke(buf);
                                else
                                    JoystickStateChanged?.Invoke(buf);
                                prev[d] = buf.ToArray();
                            }
                        }
                        else
                        {
                            d.Acquire();
                        }
                    }
                    finally { h.Free(); }
                }
                Thread.Sleep(50);
            }
        }

        public void Dispose()
        {
            _running = false;
            _pollThread.Join();
            foreach (var d in _devices)
            {
                try { d.Unacquire(); } catch { }
                Marshal.ReleaseComObject(d);
            }
            Marshal.ReleaseComObject(_dinput);
        }

        // Helpers for creating pinned GUID memory
        private static IntPtr PinGuid(Guid g)
        {
            IntPtr p = Marshal.AllocHGlobal(Marshal.SizeOf<Guid>());
            Marshal.StructureToPtr(g, p, false);
            return p;
        }

        // Many raw constants and GUIDs pulled from dinput.h:
        const uint DIMOFS_BUTTON0 = 0x00000000;
        const uint DIMOFS_X = 0;
        const uint DIMOFS_Y = 4;
        const uint DIMOFS_Z = 8;
        const uint DIMOFS_RX = 12;
        const uint DIMOFS_RY = 16;
        const uint DIMOFS_RZ = 20;
        const uint DIMOFS_SLIDER0 = 24;
        const uint DIMOFS_SLIDER1 = 28;
        const uint DIMOFS_POV0 = 32;
        const uint DIMOFS_BUTTON1 = DIMOFS_BUTTON0 + 1;
        // … up through DIMOFS_BUTTON31 …

        static uint DIDFT_MAKEINSTANCE(uint i) => unchecked(0x0000_0000u | (i & 0xFFFF));
        const uint DIDFT_PSHBUTTON = 0x0000_0010;
        const uint DIDFT_BUTTON = 0x0000_1000;
        const uint DIDFT_AXIS = 0x0000_0020;
        const uint DIDFT_POV = 0x0000_0040;
        const uint DIDFT_OPTIONAL = 0x8000_0000;
        const uint DIDF_RELAXIS = 0x0000_0002;
        const uint DIDF_ABSAXIS = 0x0000_0001;
    }

}

