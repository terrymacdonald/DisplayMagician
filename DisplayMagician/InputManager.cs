using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Vortice.DirectInput;

namespace DisplayMagician.Input
{
    /// <summary>
    /// Wraps Vortice.DirectInput for keyboard and joystick hotkeys—no P/Invoke.
    /// </summary>
    public class DirectInputManager : IDisposable
    {
        private readonly IDirectInput8 _directInput;
        private readonly Dictionary<Guid, IDirectInputDevice8> _keyboardDevices;
        private readonly Dictionary<Guid, IDirectInputDevice8> _joystickDevices;
        private readonly Dictionary<Key, Action> _keyBindings = new();
        private readonly Dictionary<(Guid, int), Action> _buttonBindings = new();
        private Thread _pollThread;
        private CancellationTokenSource _cts;

        /// <summary>
        /// Creates the DirectInput factory and sets up empty device dictionaries.
        /// </summary>
        public DirectInputManager()
        {
            // 1) Factory via DInput helper
            _directInput = DInput.DirectInput8Create();      
            _keyboardDevices = new Dictionary<Guid, IDirectInputDevice8>();
            _joystickDevices = new Dictionary<Guid, IDirectInputDevice8>();
        }

        /// <summary>
        /// Enumerates and initializes all attached keyboards and game‑devices.
        /// </summary>
        public void Initialize(IntPtr windowHandle)
        {
            // Keyboards
            foreach (var di in _directInput.GetDevices(DeviceType.Keyboard, DeviceEnumerationFlags.AttachedOnly))
            {
                var dev = _directInput.CreateDevice(di.InstanceGuid);
                ConfigureDevice(dev, windowHandle, true);
                _keyboardDevices[di.InstanceGuid] = dev;
            }

            // Gamepads & Joysticks
            foreach (var di in _directInput
                .GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly)
                .Concat(_directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly))
                .DistinctBy(d => d.InstanceGuid))
            {
                var dev = _directInput.CreateDevice(di.InstanceGuid);
                ConfigureDevice(dev, windowHandle, false);
                _joystickDevices[di.InstanceGuid] = dev;
            }
        }

        /// <summary>
        /// Sets cooperative level, buffer size, and data format on a device.
        /// </summary>
        private void ConfigureDevice(IDirectInputDevice8 device, IntPtr hwnd, bool isKeyboard)
        {
            device.SetCooperativeLevel(hwnd,
                CooperativeLevel.Foreground | CooperativeLevel.NonExclusive);      
            device.Properties.BufferSize = 16;                                      
            // Choose format
            if (isKeyboard)
            {
                var r = device.SetDataFormat<RawKeyboardState>();                 
                if (!r.Success) throw new InvalidOperationException("Keyboard DFMT failed");
            }
            else
            {
                var r = device.SetDataFormat<RawJoystickState>();                  
                if (!r.Success) throw new InvalidOperationException("Joystick DFMT failed");
            }

            device.Acquire();                                                       
        }

        /// <summary>
        /// Start the background polling thread (idempotent).
        /// </summary>
        public void Start(int pollIntervalMs = 50)
        {
            if (_pollThread?.IsAlive == true) return;

            _cts = new CancellationTokenSource();
            _pollThread = new Thread(() => PollLoop(_cts.Token, pollIntervalMs))
            {
                IsBackground = true,
                Name = "DInput Poller"
            };
            _pollThread.Start();
        }

        /// <summary>
        /// Stops polling and waits for thread to exit.
        /// </summary>
        public void Stop()
        {
            if (_cts == null) return;
            _cts.Cancel();
            _pollThread.Join();
            _cts.Dispose();
            _cts = null;
            _pollThread = null;
        }

        /// <summary>
        /// Register a keyboard scan‑code for an action.
        /// </summary>
        public void RegisterKey(Key key, Action action)
            => _keyBindings[key] = action;

        /// <summary>
        /// Remove a previously registered key.
        /// </summary>
        public bool RemoveKey(Key key)
            => _keyBindings.Remove(key);

        /// <summary>
        /// Register a joystick button on a device GUID for an action.
        /// </summary>
        public void RegisterJoystickButton(Guid deviceGuid, int buttonIndex, Action action)
            => _buttonBindings[(deviceGuid, buttonIndex)] = action;

        /// <summary>
        /// Remove a previously registered joystick button.
        /// </summary>
        public bool RemoveJoystickButton(Guid deviceGuid, int buttonIndex)
            => _buttonBindings.Remove((deviceGuid, buttonIndex));

        /// <summary>The background poll loop: reads buffered events and fires your callbacks.</summary>
        private void PollLoop(CancellationToken token, int intervalMs)
        {
            while (!token.IsCancellationRequested)
            {
                // Keyboards
                foreach (var kv in _keyboardDevices)
                {
                    var dev = kv.Value;
                    dev.Poll();  // Update state
                    var events = dev.GetBufferedKeyboardData();  // KeyEvent[]
                    foreach (var e in events)
                    {
                        if (e.IsPressed && _keyBindings.TryGetValue(e.Key, out var act))
                            act();
                    }
                }

                // Joysticks
                foreach (var kv in _joystickDevices)
                {
                    var guid = kv.Key;
                    var dev = kv.Value;
                    dev.Poll();
                    var events = dev.GetBufferedJoystickData();  // JoystickUpdate[]
                    foreach (var upd in events)
                    {
                        if (upd.Value > 0 && _buttonBindings.TryGetValue((guid, (int)upd.Offset), out var act))
                            act();
                    }
                }

                Thread.Sleep(intervalMs);
            }
        }

        /// <summary>
        /// Unacquire and release all devices and the factory.
        /// </summary>
        public void Dispose()
        {
            foreach (var dev in _keyboardDevices.Values) { dev.Unacquire(); dev.Dispose(); }
            foreach (var dev in _joystickDevices.Values) { dev.Unacquire(); dev.Dispose(); }
            _directInput.Dispose();
        }
    }
}
