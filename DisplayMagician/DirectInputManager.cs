using DisplayMagician.UIForms;
using DisplayMagicianShared;
using NLog;
using NLog.Targets;
using SharpGen.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using Vortice.DirectInput;
//using WinCopies.Util;
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
        public string Description;
        public List<Key> KeyCodes; // List of keys in the combination
        public HotkeyTask Task;
        public string UUID; // profile or shortcut UUID

        public HotkeyKeyboard()
        {
            Description = string.Empty;
            KeyCodes = new List<Key>();
            Task = HotkeyTask.None;
            UUID = string.Empty; // profile or shortcut UUID
        }

        public HotkeyKeyboard(List<Key> keyCodes, HotkeyTask task, string uuid)
        {
            KeyCodes = keyCodes;
            Task = task;
            UUID = uuid; // profile or shortcut UUID

            string myDescription = string.Empty;
            switch (task)
            {
                case HotkeyTask.ChangeDisplayProfile:
                    string profileName = ProfileRepository.GetProfileName(uuid);
                    if (string.IsNullOrEmpty(profileName))
                    {
                        myDescription = "Change Display Profile to 'Unknown'";
                    }
                    else
                    {
                        myDescription = "Change Display Profile to '" + profileName + "'";
                    }
                    break;
                case HotkeyTask.RunGameShortcut:
                    string shortcutName = ShortcutRepository.GetShortcutName(uuid);
                    if (string.IsNullOrEmpty(shortcutName))
                    {
                        myDescription = "Run Game Shortcut 'Unknown'";
                    }
                    else
                    {
                        myDescription = "Run Game Shortcut '" + shortcutName + "'";
                    }
                    break;
                case HotkeyTask.OpenShortcutLibraryWindow:
                    myDescription = "Open Shortcut Library Window";
                    break;
                case HotkeyTask.OpenMainWindow:
                    myDescription = "Open Main Window";
                    break;
                case HotkeyTask.OpenDisplayProfileWindow:
                    myDescription = "Open Display Profile Window";
                    break;
                case HotkeyTask.ExitApplication:
                    myDescription = "Exit Application";
                    break;
                default:
                    myDescription = "Unknown Task";
                    break;
            }
            Description = myDescription;

        }


    }

    public struct JoystickDevice
    {
        public DeviceType DeviceType;
        public Guid DeviceTargetId; // device identifier
        public string DeviceName;
        public int DeviceButtonIndex; // DI scan code or button index

        public JoystickDevice()
        {
            DeviceType = DeviceType.Joystick;
            DeviceTargetId = Guid.Empty;
            DeviceName = "";
            DeviceButtonIndex = 0;
        }

        public JoystickDevice(DeviceType deviceClass, string name, Guid targetId, int code)
        {
            DeviceType = deviceClass;
            DeviceTargetId = targetId;
            DeviceName = name;
            DeviceButtonIndex = code;
        }

    }

    public struct HotkeyJoystick
    {
        public string Description;
        public JoystickDevice Device;
        public HotkeyTask Task;
        public string UUID; // profile or shortcut UUID

        public HotkeyJoystick()
        {
            Description = string.Empty;
            Device = new JoystickDevice();
            Task = HotkeyTask.None;
            UUID = string.Empty;
        }

        public HotkeyJoystick(JoystickDevice device, HotkeyTask task, string uuid)
        {
            Device = device;
            Task = task;
            UUID = uuid;

            string myDescription = string.Empty;
            switch (task)
            {
                case HotkeyTask.ChangeDisplayProfile:
                    string profileName = ProfileRepository.GetProfileName(uuid);
                    if (string.IsNullOrEmpty(profileName))
                    {
                        myDescription = "Change Display Profile to 'Unknown'";
                    }
                    else
                    {
                        myDescription = "Change Display Profile to '" + profileName + "'";
                    }
                    break;
                case HotkeyTask.RunGameShortcut:
                    string shortcutName = ShortcutRepository.GetShortcutName(uuid);
                    if (string.IsNullOrEmpty(shortcutName))
                    {
                        myDescription = "Run Game Shortcut 'Unknown'";
                    }
                    else
                    {
                        myDescription = "Run Game Shortcut '" + shortcutName + "'";
                    }
                    break;
                case HotkeyTask.OpenShortcutLibraryWindow:
                    myDescription = "Open Shortcut Library Window";
                    break;
                case HotkeyTask.OpenMainWindow:
                    myDescription = "Open Main Window";
                    break;
                case HotkeyTask.OpenDisplayProfileWindow:
                    myDescription = "Open Display Profile Window";
                    break;
                case HotkeyTask.ExitApplication:
                    myDescription = "Exit Application";
                    break;
                default:
                    myDescription = "Unknown Task";
                    break;
            }
            Description = myDescription;
        }

        public HotkeyJoystick(string description, DeviceType deviceType, string name, Guid targetId, int code, HotkeyTask task, string uuid)
        {
            Description = description;
            Device = new JoystickDevice(deviceType, name, targetId, code);
            Task = task;
            UUID = uuid;

            string myDescription = string.Empty;
            switch (task)
            {
                case HotkeyTask.ChangeDisplayProfile:
                    string profileName = ProfileRepository.GetProfileName(uuid);
                    if (string.IsNullOrEmpty(profileName))
                    {
                        myDescription = "Change Display Profile to 'Unknown'";
                    }
                    else
                    {
                        myDescription = "Change Display Profile to '" + profileName + "'";
                    }
                    break;
                case HotkeyTask.RunGameShortcut:
                    string shortcutName = ShortcutRepository.GetShortcutName(uuid);
                    if (string.IsNullOrEmpty(shortcutName))
                    {
                        myDescription = "Run Game Shortcut 'Unknown'";
                    }
                    else
                    {
                        myDescription = "Run Game Shortcut '" + shortcutName + "'";
                    }
                    break;
                case HotkeyTask.OpenShortcutLibraryWindow:
                    myDescription = "Open Shortcut Library Window";
                    break;
                case HotkeyTask.OpenMainWindow:
                    myDescription = "Open Main Window";
                    break;
                case HotkeyTask.OpenDisplayProfileWindow:
                    myDescription = "Open Display Profile Window";
                    break;
                case HotkeyTask.ExitApplication:
                    myDescription = "Exit Application";
                    break;
                default:
                    myDescription = "Unknown Task";
                    break;
            }
            Description = myDescription;
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
        private readonly Dictionary<JoystickDevice, Action> _buttonBindings = new();
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
        public void RegisterJoystickButton(JoystickDevice joystick, Action action)
            => _buttonBindings[joystick] = action;

        /// <summary>
        /// Remove a previously registered joystick button.
        /// </summary>
        public bool RemoveJoystickButton(JoystickDevice joystick)
            => _buttonBindings.Remove(joystick);

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
                                if (upd.Value > 0 && _buttonBindings.TryGetValue(new JoystickDevice(joystick.Value.DeviceInfo.Type, joystick.Value.DeviceInfo.InstanceName, joystick.Key, (int)upd.Offset), out var act))
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
                if (device.Key == joystickHotkey.Device.DeviceTargetId)
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
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.Device.DeviceButtonIndex}' on device '{hotkey.Device.DeviceName}' to open the main window.");
                            Action openMainWindow = delegate { Program.AppMainForm.openApplicationWindow(); };
                            RegisterJoystickButton(hotkey.Device, openMainWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenDisplayProfileWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.Device.DeviceButtonIndex}' on device '{hotkey.Device.DeviceName}' to open the display profile window.");
                            Action openDisplayProfileWindow = delegate { Program.AppMainForm.openDisplayProfileWindow(); };
                            RegisterJoystickButton(hotkey.Device, openDisplayProfileWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenShortcutLibraryWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.Device.DeviceButtonIndex}' on device '{hotkey.Device.DeviceName}' to open the shortcut library  window.");
                            Action openShortcutLibraryWindow = delegate { Program.AppMainForm.openShortcutLibraryWindow(); };
                            RegisterJoystickButton(hotkey.Device, openShortcutLibraryWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.RunGameShortcut)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.Device.DeviceButtonIndex}' on device '{hotkey.Device.DeviceName}' to run the game shortcut {hotkey.UUID.ToString()}.");
                            Action runGameShortcut = delegate { Program.RunShortcut(hotkey.UUID.ToString()); ; };
                            RegisterJoystickButton(hotkey.Device, runGameShortcut);
                        }
                        else if (hotkey.Task == HotkeyTask.ChangeDisplayProfile)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.Device.DeviceButtonIndex}' on device '{hotkey.Device.DeviceName}' to change to display profile {hotkey.UUID.ToString()}.");
                            Action changeDisplayProfile = delegate { Program.RunProfile(hotkey.UUID.ToString()); };
                            RegisterJoystickButton(hotkey.Device, changeDisplayProfile);
                        }
                        else if (hotkey.Task == HotkeyTask.ExitApplication)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering button '{hotkey.Device.DeviceButtonIndex}' on device '{hotkey.Device.DeviceName}' to exit the application.");
                            Action exitApplication = delegate { Program.AppMainForm.exitApplication(); };
                            RegisterJoystickButton(hotkey.Device, exitApplication);
                        }
                        else
                        {
                            logger.Warn($"DirectInputManager/RegisterStoredHotkeys: WARNING - The joystick button '{hotkey.Device.DeviceButtonIndex}' on device '{hotkey.Device.DeviceName}' is not a valid hotkey. Please check the hotkey and try again.");
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

        public bool UpdateOrAddHotkeys(List<HotkeyKeyboard> updatedKeyboardHotkeys, List<HotkeyJoystick> updatedJoystickHotkeys)
        {
            try
            {
                if (updatedKeyboardHotkeys != null && updatedKeyboardHotkeys is List<HotkeyKeyboard> && updatedKeyboardHotkeys.Count > 0)
                {
                    foreach (var hotkey in updatedKeyboardHotkeys)
                    {
                        // check if the hotkey is already in the list, and if so, remove it
                        
                        foreach (var existingHotkey in Program.AppProgramSettings.KeyboardHotkeys)
                        {
                            if (existingHotkey.UUID.ToString() == hotkey.UUID.ToString() && existingHotkey.KeyCodes.Equals(hotkey.KeyCodes))
                            {
                                // Remove it from the stored list
                                Program.AppProgramSettings.KeyboardHotkeys.Remove(existingHotkey);
                                // If it is currently registered, then deregister it
                                RemoveKeyCombination(existingHotkey.KeyCodes);
                            }
                        }

                        // Add the key combination to the store of keyboard hotkeys
                        Program.AppProgramSettings.KeyboardHotkeys.Add(hotkey);


                        if (hotkey.Task == HotkeyTask.OpenMainWindow)
                        {
                            Action openMainWindow = delegate { Program.AppMainForm.openApplicationWindow(); };
                            RegisterKeyCombination(hotkey.KeyCodes, openMainWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenDisplayProfileWindow)
                        {
                            Action openDisplayProfileWindow = delegate { Program.AppMainForm.openDisplayProfileWindow(); };
                            RegisterKeyCombination(hotkey.KeyCodes, openDisplayProfileWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenShortcutLibraryWindow)
                        {
                            Action openShortcutLibraryWindow = delegate { Program.AppMainForm.openShortcutLibraryWindow(); };
                            RegisterKeyCombination(hotkey.KeyCodes, openShortcutLibraryWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.RunGameShortcut)
                        {
                            Action runGameShortcut = delegate { Program.RunShortcut(hotkey.UUID.ToString()); ; };
                            RegisterKeyCombination(hotkey.KeyCodes, runGameShortcut);
                        }
                        else if (hotkey.Task == HotkeyTask.ChangeDisplayProfile)
                        {
                            Action changeDisplayProfile = delegate { Program.RunProfile(hotkey.UUID.ToString()); };
                            RegisterKeyCombination(hotkey.KeyCodes, changeDisplayProfile);
                        }
                        else if (hotkey.Task == HotkeyTask.ExitApplication)
                        {
                            Action exitApplication = delegate { Program.AppMainForm.exitApplication(); };
                            RegisterKeyCombination(hotkey.KeyCodes, exitApplication);
                        }
                    }
                }
                if (updatedJoystickHotkeys != null && updatedJoystickHotkeys is List<HotkeyJoystick> && updatedJoystickHotkeys.Count > 0)
                {
                    foreach (var hotkey in updatedJoystickHotkeys)
                    {
                        // check if the hotkey is already in the list, and if so, remove it

                        foreach (var existingHotkey in Program.AppProgramSettings.JoystickHotkeys)
                        {
                            if (existingHotkey.UUID.ToString() == updatedJoystickHotkeys[0].UUID.ToString() && existingHotkey.Device.DeviceButtonIndex == updatedJoystickHotkeys[0].Device.DeviceButtonIndex)
                            {
                                // Remove it from the stored list
                                Program.AppProgramSettings.JoystickHotkeys.Remove(existingHotkey);
                                // If it is currently registered, then deregister it
                                RemoveJoystickButton(existingHotkey.Device);
                            }
                        }

                        // Add the button combination to the store of joystick hotkeys
                        Program.AppProgramSettings.JoystickHotkeys.Add(hotkey);


                        if (hotkey.Task == HotkeyTask.OpenMainWindow)
                        {
                            Action openMainWindow = delegate { Program.AppMainForm.openApplicationWindow(); };
                            RegisterJoystickButton(hotkey.Device, openMainWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenDisplayProfileWindow)
                        {
                            Action openDisplayProfileWindow = delegate { Program.AppMainForm.openDisplayProfileWindow(); };
                            RegisterJoystickButton(hotkey.Device, openDisplayProfileWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenShortcutLibraryWindow)
                        {
                            Action openShortcutLibraryWindow = delegate { Program.AppMainForm.openShortcutLibraryWindow(); };
                            RegisterJoystickButton(hotkey.Device, openShortcutLibraryWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.RunGameShortcut)
                        {
                            Action runGameShortcut = delegate { Program.RunShortcut(hotkey.UUID.ToString()); ; };
                            RegisterJoystickButton(hotkey.Device, runGameShortcut);
                        }
                        else if (hotkey.Task == HotkeyTask.ChangeDisplayProfile)
                        {
                            Action changeDisplayProfile = delegate { Program.RunProfile(hotkey.UUID.ToString()); };
                            RegisterJoystickButton(hotkey.Device, changeDisplayProfile);
                        }
                        else if (hotkey.Task == HotkeyTask.ExitApplication)
                        {
                            Action exitApplication = delegate { Program.AppMainForm.exitApplication(); };
                            RegisterJoystickButton(hotkey.Device, exitApplication);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/UpdateOrAddHotkeys: WARNING - Exception while trying to updated or add the Hotkeys.");
            }
            return true;
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
                        RemoveJoystickButton(hotkey.Device);
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
                    logger.Trace($"DirectInputManager/GetKeyboardHotkeysByUUID: We have no  keyboard hotkeys to find so returning an empty list.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/GetKeyboardHotkeysByUUID: WARNING - Exception while trying to find a keyboard hotkey by task.");                
            }            

            return hotkeysToReturn;
        }

        public List<HotkeyKeyboard> GetKeyboardHotkeysByTask(HotkeyTask task)
        {
            List<HotkeyKeyboard> hotkeysToReturn = new List<HotkeyKeyboard>();

            try
            {
                if (Program.AppProgramSettings.KeyboardHotkeys != null && Program.AppProgramSettings.KeyboardHotkeys is List<HotkeyKeyboard> && Program.AppProgramSettings.KeyboardHotkeys.Count > 0)
                {
                    foreach (var hotkey in Program.AppProgramSettings.KeyboardHotkeys)
                    {
                        if (hotkey.Task == task)
                        {
                            hotkeysToReturn.Add(hotkey);
                        }
                    }
                }
                else
                {
                    logger.Trace($"DirectInputManager/GetKeyboardHotkeysByTask: We have no keyboard hotkeys to find so returning an empty list.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/GetKeyboardHotkeysByTask: WARNING - Exception while trying to find a keyboard hotkey by task.");
            }

            return hotkeysToReturn;
        }

        public List<HotkeyJoystick> GetJoystickHotkeysByUUID(string uuid)
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
                    logger.Trace($"DirectInputManager/GetKeyboardHotkeysByUUID: We have no joystick hotkeys to find so returning an empty list.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/GetKeyboardHotkeysByUUID: WARNING - Exception while trying to find a joystick hotkey by task.");
            }


            return hotkeysToReturn;
        }

        public List<HotkeyJoystick> GetJoystickHotkeysByTask(HotkeyTask task)
        {
            List<HotkeyJoystick> hotkeysToReturn = new List<HotkeyJoystick>();

            try
            {
                if (Program.AppProgramSettings.JoystickHotkeys != null && Program.AppProgramSettings.JoystickHotkeys is List<HotkeyJoystick> && Program.AppProgramSettings.JoystickHotkeys.Count > 0)
                {
                    foreach (var hotkey in Program.AppProgramSettings.JoystickHotkeys)
                    {
                        if (hotkey.Task == task)
                        {
                            hotkeysToReturn.Add(hotkey);
                        }
                    }
                }
                else
                {
                    logger.Trace($"DirectInputManager/GetJoystickHotkeysByTask: We have no joystick hotkeys to find so returning an empty list.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/GetJoystickHotkeysByTask: WARNING - Exception while trying to find a joystick hotkey by task.");
            }


            return hotkeysToReturn;
        }

        public string GenerateKeyboardHotkeyText(HotkeyKeyboard keyboardHotkey)
        {
            // We want the keyboard hotkeys to win if both are provided. Joystick and keyboard hotkeys do not mix and cannot be used together.
            List<string> keyNames = new List<string>();

            foreach (var key in keyboardHotkey.KeyCodes)
            {
                keyNames.Add(key.ToString("G"));
            }
            
            return string.Join(" + ", keyNames);
        }

        public string GenerateKeyboardHotkeyText(List<HotkeyKeyboard> keyboardHotkeys)
        {
            // We want the keyboard hotkeys to win if both are provided. Joystick and keyboard hotkeys do not mix and cannot be used together.
            
            string hotkeyListText = string.Empty;

            foreach (var singleHotkey in keyboardHotkeys)
            {
                if (singleHotkey.KeyCodes == null || !singleHotkey.KeyCodes.Any())
                    continue;

                hotkeyListText += string.Join(", ", GenerateKeyboardHotkeyText(singleHotkey));

            }
            return hotkeyListText;
        }

        public string GenerateJoystickHotkeyText(HotkeyJoystick joystickHotkey)
        {
            // We want the keyboard hotkeys to win if both are provided. Joystick and keyboard hotkeys do not mix and cannot be used together.
            return joystickHotkey.Device.DeviceName.ToString() + " Button #" + joystickHotkey.Device.DeviceButtonIndex.ToString();
        }

        public string GenerateJoystickHotkeyText(List<HotkeyJoystick> joystickHotkeys)
        {
            // We want the keyboard hotkeys to win if both are provided. Joystick and keyboard hotkeys do not mix and cannot be used together.

            string hotkeyListText = string.Empty;

            foreach (var singleHotkey in joystickHotkeys)
            {
                hotkeyListText += string.Join(", ", GenerateJoystickHotkeyText(singleHotkey));
            }
            return hotkeyListText;
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
