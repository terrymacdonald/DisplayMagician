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


namespace DisplayMagician
{
    // Enumeration constants from dinput.h
    enum DeviceClass : uint
    {
        All = 0,
        Device = 1,
        Gamepad = 2,
        Keyboard = 3,  // DI8DEVCLASS_KEYBOARD
        Pointer = 4,
        Joystick = 5
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

        // DataFormat pointers loaded once
        private readonly IntPtr _dfKeyboard;
        private readonly IntPtr _dfJoystick;

        public DirectInputManager(IntPtr ownerWindowHandle)
        {
            // Load data-format exports
            _dfKeyboard = NativeMethods.GetDataFormatPtr("c_dfDIKeyboard");
            _dfJoystick = NativeMethods.GetDataFormatPtr("c_dfDIJoystick2");

            // Create DirectInput8 COM object
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

            // Enumerate & acquire devices
            AcquireDevices(DeviceClass.Keyboard, _dfKeyboard, ownerWindowHandle);
            AcquireDevices(DeviceClass.Gamepad, _dfJoystick, ownerWindowHandle);
            AcquireDevices(DeviceClass.Joystick, _dfJoystick, ownerWindowHandle);

            // Start polling
            _running = true;
            _pollThread = new Thread(PollLoop) { IsBackground = true };
            _pollThread.Start();
        }

        private void AcquireDevices(DeviceClass cls, IntPtr dataFormatPtr, IntPtr hwnd)
        {
            var callback = new DIEnumDevicesCallback((ref DIDEVICEINSTANCE ddi, IntPtr ctx) =>
            {
                ddi.dwSize = (uint)Marshal.SizeOf<DIDEVICEINSTANCE>();
                Guid guid = ddi.guidInstance;
                _dinput.CreateDevice(ref guid, out IntPtr devPtr, IntPtr.Zero);
                var dev = (IDirectInputDevice8)Marshal.GetObjectForIUnknown(devPtr);

                dev.SetDataFormat(dataFormatPtr);
                const uint DISCL_FOREGROUND = 0x00000001;
                const uint DISCL_NONEXCLUSIVE = 0x00000020;
                dev.SetCooperativeLevel(hwnd, DISCL_FOREGROUND | DISCL_NONEXCLUSIVE);
                dev.Acquire();

                _devices.Add(dev);
                _deviceClasses[dev] = cls;
                return 1;
            });

            IntPtr cbPtr = Marshal.GetFunctionPointerForDelegate(callback);
            const uint DIEDFL_ATTACHEDONLY = 0x00000001;
            _dinput.EnumDevices((uint)cls, cbPtr, IntPtr.Zero, DIEDFL_ATTACHEDONLY);
        }

        private void PollLoop()
        {
            var prevStates = new Dictionary<IDirectInputDevice8, byte[]>();
            while (_running)
            {
                foreach (var dev in _devices)
                {
                    var buf = new byte[256];
                    var handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
                    try
                    {
                        int hr = dev.GetDeviceState(buf.Length, handle.AddrOfPinnedObject());
                        if (hr >= 0)
                        {
                            if (!prevStates.TryGetValue(dev, out var prev) ||
                                !buf.SequenceEqual(prev))
                            {
                                if (_deviceClasses[dev] == DeviceClass.Keyboard)
                                    KeyboardStateChanged?.Invoke(buf);
                                else
                                    JoystickStateChanged?.Invoke(buf);

                                prevStates[dev] = (byte[])buf.Clone();
                            }
                        }
                        else
                        {
                            dev.Acquire();
                        }
                    }
                    finally
                    {
                        handle.Free();
                    }
                }
                Thread.Sleep(50);
            }
        }

        public void Dispose()
        {
            _running = false;
            _pollThread.Join();
            foreach (var dev in _devices)
            {
                try { dev.Unacquire(); } catch { }
                Marshal.ReleaseComObject(dev);
            }
            _devices.Clear();
            if (_dinput != null)
                Marshal.ReleaseComObject(_dinput);
        }
    }
}

