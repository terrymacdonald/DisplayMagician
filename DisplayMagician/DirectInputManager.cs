using DisplayMagician.UIForms;
using NLog;
using NLog.Targets;
using SharpGen.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using Vortice.DirectInput;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DisplayMagician
{
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

    public struct HotkeyKeyboard
    {
        public List<Key> KeyCodes; // List of keys in the combination
        public HotkeyTask Task;
        public Guid UUID; // profile or shortcut UUID

        public HotkeyKeyboard()
        {
            KeyCodes = new List<Key>();
            Task = HotkeyTask.None;
            UUID = Guid.Empty; // profile or shortcut UUID
        }

        public HotkeyKeyboard(List<Key> keyCodes, HotkeyTask task, Guid uuid)
        {
            KeyCodes = keyCodes;
            Task = task;
            UUID = uuid; // profile or shortcut UUID
        }
    }

    public struct HotkeyJoystick
    {
        public DeviceType DeviceType;
        public Guid TargetId; // profile or shortcut UUID
        public int ButtonIndex; // DI scan code or button index
        public HotkeyTask Task;
        public Guid UUID; // profile or shortcut UUID

        public HotkeyJoystick()
        {
            DeviceType = DeviceType.Joystick;
            TargetId = Guid.Empty; // profile or shortcut UUID
            ButtonIndex = 0;
            Task = HotkeyTask.None;
            UUID = Guid.Empty;
        }

        public HotkeyJoystick(DeviceType deviceClass, Guid targetId, int code, HotkeyTask action, Guid uuid)
        {
            DeviceType = deviceClass;
            TargetId = targetId;
            ButtonIndex = code;
            Task = action;
            UUID = uuid;
        }
    }

    /// <summary>
    /// Wraps Vortice.DirectInput for keyboard and joystick hotkeys—no P/Invoke.
    /// </summary>
    public class DirectInputManager : IDisposable
    {
        private readonly IDirectInput8 _directInput;
        private readonly Dictionary<Guid, IDirectInputDevice8> _keyboardDevices;
        private readonly Dictionary<Guid, IDirectInputDevice8> _joystickDevices;
        private readonly Dictionary<List<Key>, Action> _keyBindings = new Dictionary<List<Key>, Action>(new KeyCombinationComparer());
        private readonly Dictionary<(Guid, int), Action> _buttonBindings = new();
        private Thread _pollThread;
        private CancellationTokenSource _cts;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

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

                if (_directInput.IsDeviceAttached(di.InstanceGuid))
                {
                    ConfigureDevice(dev, windowHandle, true);
                    _keyboardDevices[di.InstanceGuid] = dev;
                }
            }

            // Gamepads & Joysticks
            foreach (var di in _directInput
                .GetDevices(DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly)
                .Concat(_directInput.GetDevices(DeviceType.Joystick, DeviceEnumerationFlags.AttachedOnly))
                .DistinctBy(d => d.InstanceGuid))
            {
                var dev = _directInput.CreateDevice(di.InstanceGuid);
                if (_directInput.IsDeviceAttached(di.InstanceGuid))
                {
                    ConfigureDevice(dev, windowHandle, true);
                    _joystickDevices[di.InstanceGuid] = dev;
                }                
            }
        }

        /// <summary>
        /// Sets cooperative level, buffer size, and data format on a device.
        /// </summary>
        private void ConfigureDevice(IDirectInputDevice8 device, IntPtr hwnd, bool isKeyboard)
        {
            device.SetCooperativeLevel(hwnd,
                CooperativeLevel.Background | CooperativeLevel.NonExclusive);      
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
                Name = "DisplayMagician Input Poller"
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

        public IEnumerable<IDirectInputDevice8> GetKeyboards()
        {
            return _keyboardDevices.Values;
        }

        public IEnumerable<IDirectInputDevice8> GetJoysticks()
        {
            return _joystickDevices.Values;
        }

        /// <summary>
        /// Register key combinations for an action.
        /// </summary>
        public void RegisterKeyCombination(List<Key> keyCombination, Action action)
        {
            _keyBindings[keyCombination] = action;
        }

        /// <summary>
        /// Remove a previously registered key combination.
        /// </summary>
        public bool RemoveKeyCombination(List<Key> keyCombination)
            => _keyBindings.Remove(keyCombination);

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
                    var keyboard = kv.Value;
                    Result result = keyboard.Poll();  // Update state

                    if (result.Failure)
                    {
                        result = keyboard.Acquire();

                        if (result.Failure)
                            break;
                    }

                    try
                    {
                        var state = keyboard.GetCurrentKeyboardState();

                        foreach (var binding in _keyBindings)
                        {
                            if (binding.Key.All(k => state.IsPressed(k)))
                            {
                                // Invoke the associated action
                                if (Program.AppMainForm.InvokeRequired)
                                {
                                    Program.AppMainForm.BeginInvoke((MethodInvoker)delegate {
                                        binding.Value();
                                    });
                                }
                                else
                                {
                                    binding.Value();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }

                }

                // Joysticks
                foreach (var joystick in _joystickDevices)
                {
                    Result result_ = joystick.Value.Poll();

                    if (result_.Failure)
                    {
                        result_ = joystick.Value.Acquire();

                        if (result_.Failure)
                            break;
                    }

                    try
                    {
                        JoystickUpdate[] bufferedData = joystick.Value.GetBufferedJoystickData();

                        if (bufferedData.Length > 0)
                        {

                            foreach (var upd in bufferedData)
                            {
                                if (upd.Value > 0 && _buttonBindings.TryGetValue((joystick.Key, (int)upd.Offset), out var act))
                                {
                                    if (Program.AppMainForm.InvokeRequired)
                                    {
                                        Program.AppMainForm.BeginInvoke((System.Windows.Forms.MethodInvoker) delegate
                                        {
                                            act();
                                        });
                                    }
                                    else
                                    {
                                        act();
                                    }
                                }
                                    
                            }
                            Trace.WriteLine(bufferedData[0].ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Trace.Write(ex.Message);
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
            // Stop the poll thread if it's still running
            if (_pollThread?.IsAlive ?? false)
            {
                Stop();
            }

            // dispose all devices and the interface
            foreach (var dev in _keyboardDevices.Values) { dev.Unacquire(); dev.Dispose(); }
            foreach (var dev in _joystickDevices.Values) { dev.Unacquire(); dev.Dispose(); }
            _directInput.Dispose();
        }


        /// <summary>
        /// Gets the product name of the joystick device associated with the hotkey.
        /// </summary>
        public string GetNameOfJoystickHotkey(HotkeyJoystick joystickHotkey)
        {
            foreach (var device in _joystickDevices)
            {
                if (device.Key == joystickHotkey.TargetId)
                {
                    return device.Value.DeviceInfo.ProductName;
                }
            }
            return "";
        }

        // <summary>
        /// Gets the name of the key pressed on the keyboard associated with the hotkey.
        /// </summary>
        public string GetNameOfKeyboardHotkey(HotkeyKeyboard keyboardHotkey)
        {
            if (keyboardHotkey.KeyCodes == null || !keyboardHotkey.KeyCodes.Any())
                return string.Empty;

            return string.Join(" + ", keyboardHotkey.KeyCodes.Select(k => k.ToString()));
        }

        public bool RegisterStoredHotkeys(ProgramSettings programSettings)
        {
            try
            {
                if (programSettings.KeyboardHotkeys != null && programSettings.KeyboardHotkeys is List<HotkeyKeyboard> && programSettings.KeyboardHotkeys.Count > 0)
                {
                    logger.Trace($"DirectInputManager/RegisterStoredHotkeys: We have {Program.AppProgramSettings.KeyboardHotkeys.Count} keyboard hotkeys to set up.");
                    foreach (var hotkey in Program.AppProgramSettings.KeyboardHotkeys)
                    {
                        if (hotkey.Task == HotkeyTask.OpenMainWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering key combination '{hotkey.KeyCodes}' to open the main window.");
                            Action openMainWindow = delegate { Program.AppMainForm.openApplicationWindow(); };
                            RegisterKeyCombination(hotkey.KeyCodes, openMainWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenDisplayProfileWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering key combination '{hotkey.KeyCodes}' to open the display profile window.");
                            Action openDisplayProfileWindow = delegate { Program.AppMainForm.openDisplayProfileWindow(); };
                            RegisterKeyCombination(hotkey.KeyCodes, openDisplayProfileWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenShortcutLibraryWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering key combination '{hotkey.KeyCodes}' to open the shortcut library window.");
                            Action openShortcutLibraryWindow = delegate { Program.AppMainForm.openShortcutLibraryWindow(); };
                            RegisterKeyCombination(hotkey.KeyCodes, openShortcutLibraryWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.RunGameShortcut)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering key combination '{hotkey.KeyCodes}' to open the main window.");
                            Action runGameShortcut = delegate { Program.RunShortcut(hotkey.UUID.ToString()); ; };
                            RegisterKeyCombination(hotkey.KeyCodes, runGameShortcut);
                        }
                        else if (hotkey.Task == HotkeyTask.ChangeDisplayProfile)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering key combination '{hotkey.KeyCodes}' to run the game shortcut {hotkey.UUID.ToString()}.");
                            Action changeDisplayProfile = delegate { Program.RunProfile(hotkey.UUID.ToString()); };
                            RegisterKeyCombination(hotkey.KeyCodes, changeDisplayProfile);
                        }
                        else if (hotkey.Task == HotkeyTask.ExitApplication)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering key combination '{hotkey.KeyCodes}' to change to display profile {hotkey.UUID.ToString()}.");
                            Action exitApplication = delegate { Program.AppMainForm.exitApplication(); };
                            RegisterKeyCombination(hotkey.KeyCodes, exitApplication);
                        }
                        else
                        {
                            logger.Warn($"DirectInputManager/RegisterStoredHotkeys: WARNING - The hotkey combination '{hotkey.KeyCodes}' is not a valid hotkey. Please check the hotkey and try again.");
                        }

                    }
                }
                else
                {
                    logger.Trace($"DirectInputManager/RegisterStoredHotkeys: We have no keyboard hotkeys to set up so skipping them.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/RegisterStoredHotkeys: WARNING - Exception while trying to register the Keyboard Hotkey. It may already be registered to something else. Please choose another Hotkey, or stop the other application from using it.");
                MessageBox.Show(
                                $"Exception while trying to register the Keyboard Hotkey. It may already be registered to something else. Please choose another Hotkey, or stop the other application from using it.", @"DisplayMagician Hotkey Registration Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }

            try
            {
                if (programSettings.JoystickHotkeys != null && programSettings.JoystickHotkeys is List<HotkeyJoystick> && programSettings.JoystickHotkeys.Count > 0)
                {
                    logger.Trace($"DirectInputManager/RegisterStoredHotkeys: We have {Program.AppProgramSettings.JoystickHotkeys.Count} joystick and gamepad hotkeys to set up.");
                    foreach (var hotkey in Program.AppProgramSettings.JoystickHotkeys)
                    {
                        if (hotkey.Task == HotkeyTask.OpenMainWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.ButtonIndex}' on device '{GetNameOfJoystickHotkey(hotkey)}' to open the main window.");
                            Action openMainWindow = delegate { Program.AppMainForm.openApplicationWindow(); };
                            RegisterJoystickButton(hotkey.TargetId, hotkey.ButtonIndex, openMainWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenDisplayProfileWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.ButtonIndex}' on device '{GetNameOfJoystickHotkey(hotkey)}' to open the display profile window.");
                            Action openDisplayProfileWindow = delegate { Program.AppMainForm.openDisplayProfileWindow(); };
                            RegisterJoystickButton(hotkey.TargetId, hotkey.ButtonIndex, openDisplayProfileWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenShortcutLibraryWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.ButtonIndex}' on device '{GetNameOfJoystickHotkey(hotkey)}' to open the shortcut library  window.");
                            Action openShortcutLibraryWindow = delegate { Program.AppMainForm.openShortcutLibraryWindow(); };
                            RegisterJoystickButton(hotkey.TargetId, hotkey.ButtonIndex, openShortcutLibraryWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.RunGameShortcut)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.ButtonIndex}' on device '{GetNameOfJoystickHotkey(hotkey)}' to run the game shortcut {hotkey.UUID.ToString()}.");
                            Action runGameShortcut = delegate { Program.RunShortcut(hotkey.UUID.ToString()); ; };
                            RegisterJoystickButton(hotkey.TargetId, hotkey.ButtonIndex, runGameShortcut);
                        }
                        else if (hotkey.Task == HotkeyTask.ChangeDisplayProfile)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.ButtonIndex}' on device '{GetNameOfJoystickHotkey(hotkey)}' to change to display profile {hotkey.UUID.ToString()}.");
                            Action changeDisplayProfile = delegate { Program.RunProfile(hotkey.UUID.ToString()); };
                            RegisterJoystickButton(hotkey.TargetId, hotkey.ButtonIndex, changeDisplayProfile);
                        }
                        else if (hotkey.Task == HotkeyTask.ExitApplication)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.ButtonIndex}' on device '{GetNameOfJoystickHotkey(hotkey)}' to exit the application.");
                            Action exitApplication = delegate { Program.AppMainForm.exitApplication(); };
                            RegisterJoystickButton(hotkey.TargetId, hotkey.ButtonIndex, exitApplication);
                        }
                        else
                        {
                            logger.Warn($"DirectInputManager/RegisterStoredHotkeys: WARNING - The joystick button '{hotkey.ButtonIndex}' on device '{GetNameOfJoystickHotkey(hotkey)}' is not a valid hotkey. Please check the hotkey and try again.");
                        }

                    }
                }
                else
                {
                    logger.Trace($"DirectInputManager/RegisterStoredHotkeys: We have no joystick hotkeys to set up so skipping them.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/RegisterStoredHotkeys: WARNING - Exception while trying to register the Keyboard Hotkey. It may already be registered to something else. Please choose another Hotkey, or stop the other application from using it.");
                MessageBox.Show(
                                $"Exception while trying to register the Keyboard Hotkey. It may already be registered to something else. Please choose another Hotkey, or stop the other application from using it.", @"DisplayMagician Hotkey Registration Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }
            return true;
        }

        public string HotkeyToString(Keys hotkey)
        {
            string parsedHotkey = string.Empty;
            KeysConverter kc = new KeysConverter();

            // Lets parse the hotkey to create the text we need
            parsedHotkey = kc.ConvertToString(hotkey);

            // Control also shows as Ctrl+ControlKey, so we trim the +ControlKeu
            if (parsedHotkey.Contains("+ControlKey"))
                parsedHotkey = parsedHotkey.Replace("+ControlKey", "");

            // Shift also shows as Shift+ShiftKey, so we trim the +ShiftKeu
            if (parsedHotkey.Contains("+ShiftKey"))
                parsedHotkey = parsedHotkey.Replace("+ShiftKey", "");

            // Alt also shows as Alt+Menu, so we trim the +Menu
            if (parsedHotkey.Contains("+Menu"))
                parsedHotkey = parsedHotkey.Replace("+Menu", "");

            return parsedHotkey;
        }

        public bool RemoveHotkeysByUUID(string uuid)
        {
            try
            {
                if (Program.AppProgramSettings.KeyboardHotkeys != null && Program.AppProgramSettings.KeyboardHotkeys is List<HotkeyKeyboard> && Program.AppProgramSettings.KeyboardHotkeys.Count > 0)
                {
                    foreach (var hotkey in Program.AppProgramSettings.KeyboardHotkeys)
                    {
                        if (hotkey.UUID.ToString() == uuid)
                        {
                            // Remove it from the stored list
                            Program.AppProgramSettings.KeyboardHotkeys.Remove(hotkey);

                            // If it is currently registered, then deregister it
                            RemoveKeyCombination(hotkey.KeyCodes);
                        }
                    }
                }
                else
                {
                    logger.Trace($"DirectInputManager/RemoveHotkeysByUUID: We have no  keyboard hotkeys to set up so skipping them.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/RemoveHotkeysByUUID: WARNING - Exception while trying to register the Keyboard Hotkey. It may already be registered to something else. Please choose another Hotkey, or stop the other application from using it.");
            }

            if (Program.AppProgramSettings.JoystickHotkeys != null && Program.AppProgramSettings.JoystickHotkeys is List<HotkeyJoystick> && Program.AppProgramSettings.JoystickHotkeys.Count > 0)
            {
                foreach (var hotkey in Program.AppProgramSettings.JoystickHotkeys)
                {
                    if (hotkey.UUID.ToString() == uuid)
                    {
                        // Remove it from the stored list
                        Program.AppProgramSettings.JoystickHotkeys.Remove(hotkey);

                        // If it is currently registered, then deregister it
                        RemoveJoystickButton(hotkey.TargetId, hotkey.ButtonIndex);
                    }
                }
            }
            else
            {
                logger.Trace($"DirectInputManager/RemoveHotkeysByUUID: We have no joystick hotkeys to set up so skipping them.");
            }

            return true;
        }

        public List<HotkeyKeyboard> GetKeyboardHotkeysByUUID(string uuid)
        {
            List<HotkeyKeyboard> hotkeysToReturn = new List<HotkeyKeyboard>();

            try
            {
                if (Program.AppProgramSettings.KeyboardHotkeys != null && Program.AppProgramSettings.KeyboardHotkeys is List<HotkeyKeyboard> && Program.AppProgramSettings.KeyboardHotkeys.Count > 0)
                {
                    foreach (var hotkey in Program.AppProgramSettings.KeyboardHotkeys)
                    {
                        if (hotkey.UUID.ToString() == uuid)
                        {
                            hotkeysToReturn.Add(hotkey);
                        }
                    }
                }
                else
                {
                    logger.Trace($"DirectInputManager/GetKeyboardHotkeysByUUID: We have no  keyboard hotkeys to set up so skipping them.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/GetKeyboardHotkeysByUUID: WARNING - Exception while trying to register the Keyboard Hotkey. It may already be registered to something else. Please choose another Hotkey, or stop the other application from using it.");                
            }            

            return hotkeysToReturn;
        }

        public List<HotkeyJoystick> GeJoystickHotkeysByUUID(string uuid)
        {
            List<HotkeyJoystick> hotkeysToReturn = new List<HotkeyJoystick>();

            try
            {
                if (Program.AppProgramSettings.JoystickHotkeys != null && Program.AppProgramSettings.JoystickHotkeys is List<HotkeyJoystick> && Program.AppProgramSettings.JoystickHotkeys.Count > 0)
                {
                    foreach (var hotkey in Program.AppProgramSettings.JoystickHotkeys)
                    {
                        if (hotkey.UUID.ToString() == uuid)
                        {
                            hotkeysToReturn.Add(hotkey);
                        }
                    }
                }
                else
                {
                    logger.Trace($"DirectInputManager/GetKeyboardHotkeysByUUID: We have no  keyboard hotkeys to set up so skipping them.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/GetKeyboardHotkeysByUUID: WARNING - Exception while trying to register the Keyboard Hotkey. It may already be registered to something else. Please choose another Hotkey, or stop the other application from using it.");
            }


            return hotkeysToReturn;
        }

    }

    public class KeyCombinationComparer : IEqualityComparer<List<Key>>
    {
        public bool Equals(List<Key> x, List<Key> y)
        {
            if (x == null || y == null) return false;
            return x.Count == y.Count && !x.Except(y).Any();
        }

        public int GetHashCode(List<Key> obj)
        {
            unchecked
            {
                int hash = 17;
                foreach (var key in obj.OrderBy(k => k))
                {
                    hash = hash * 31 + key.GetHashCode();
                }
                return hash;
            }
        }
    }
}
