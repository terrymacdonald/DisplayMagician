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
            KeyCodes = new List<Key>(keyCodes);
            Task = task;
            UUID = uuid; // profile or shortcut UUID

            string myDescription = task switch
            {
                HotkeyTask.ChangeDisplayProfile => $"Change Display Profile to '{ProfileRepository.GetProfileName(uuid) ?? "Unknown"}'",
                HotkeyTask.RunGameShortcut => $"Run Game Shortcut '{ShortcutRepository.GetShortcutName(uuid) ?? "Unknown"}'",
                HotkeyTask.OpenShortcutLibraryWindow => "Open Shortcut Library Window",
                HotkeyTask.OpenMainWindow => "Open Main Window",
                HotkeyTask.OpenDisplayProfileWindow => "Open Display Profile Window",
                HotkeyTask.ExitApplication => "Exit Application",
                _ => "Unknown Task"
            };

            Description = myDescription;

        }


    }

    public struct JoystickButton
    {
        public DeviceType DeviceType;
        public Guid DeviceTargetId; // device identifier
        public string DeviceName;
        public int DeviceButtonIndex; // DI scan code or button index

        public JoystickButton()
        {
            DeviceType = DeviceType.Joystick;
            DeviceTargetId = Guid.Empty;
            DeviceName = "";
            DeviceButtonIndex = 0;
        }

        public JoystickButton(DeviceType deviceClass, string name, Guid targetId, int code)
        {
            DeviceType = deviceClass;
            DeviceTargetId = targetId;
            DeviceName = name;
            DeviceButtonIndex = code;
        }

        public override string ToString()
        {
            return DeviceName + " Button #" + DeviceButtonIndex.ToString(CultureInfo.InvariantCulture);
        }

    }

    public struct HotkeyJoystick
    {
        public string Description;
        public List<JoystickButton> Buttons;
        public HotkeyTask Task;
        public string UUID; // profile or shortcut UUID

        public HotkeyJoystick()
        {
            Description = string.Empty;
            Buttons = new List<JoystickButton>();
            Task = HotkeyTask.None;
            UUID = string.Empty;
        }

        public HotkeyJoystick(List<JoystickButton> buttons, HotkeyTask task, string uuid)
        {
            Buttons = new List<JoystickButton>(buttons);
            Task = task;
            UUID = uuid;

            string myDescription = task switch
            {
                HotkeyTask.ChangeDisplayProfile => $"Change Display Profile to '{ProfileRepository.GetProfileName(uuid) ?? "Unknown"}'",
                HotkeyTask.RunGameShortcut => $"Run Game Shortcut '{ShortcutRepository.GetShortcutName(uuid) ?? "Unknown"}'",
                HotkeyTask.OpenShortcutLibraryWindow => "Open Shortcut Library Window",
                HotkeyTask.OpenMainWindow => "Open Main Window",
                HotkeyTask.OpenDisplayProfileWindow => "Open Display Profile Window",
                HotkeyTask.ExitApplication => "Exit Application",
                _ => "Unknown Task"
            };

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
        private readonly Dictionary<List<JoystickButton>, Action> _buttonBindings = new(new JoystickButtonCombinationComparer());
        private readonly HashSet<string> _activeKeyboardBindings = new HashSet<string>();
        private readonly HashSet<string> _activeJoystickBindings = new HashSet<string>();
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
                    ConfigureDevice(dev, windowHandle, false);
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
            if (_pollThread != null && !_pollThread.Join(TimeSpan.FromSeconds(2)))
            {
                logger.Warn($"DirectInputManager/Stop: DirectInput polling thread did not stop within 2 seconds.");
            }
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
        /// Register a joystick button combination on a device GUID for an action.
        /// </summary>
        public void RegisterJoystickButtons(List<JoystickButton> buttonCombination, Action action)
            => _buttonBindings[buttonCombination] = action;

        /// <summary>
        /// Remove a previously registered joystick button combination.
        /// </summary>
        public bool RemoveJoystickButtons(List<JoystickButton> buttonCombination)
            => _buttonBindings.Remove(buttonCombination);

        /// <summary>The background poll loop: reads buffered events and fires your callbacks.</summary>
        private void PollLoop(CancellationToken token, int intervalMs)
        {
            while (!token.IsCancellationRequested)
            {
                HashSet<string> keyboardBindingsDownThisPoll = new HashSet<string>();
                HashSet<string> joystickBindingsDownThisPoll = new HashSet<string>();

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
                            if (state.PressedKeys.Count > 0 && binding.Key.Count > 0 && binding.Key.All(k => state.IsPressed(k)))
                            {
                                string bindingSignature = CreateKeyboardBindingSignature(binding.Key);
                                keyboardBindingsDownThisPoll.Add(bindingSignature);
                                if (_activeKeyboardBindings.Add(bindingSignature))
                                {
                                    DispatchHotkeyAction(binding.Value);
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
                        var state = joystick.Value.GetCurrentJoystickState();

                        // convert the state to a list of joystick buttons to our JoystickButton struct
                        var pressedButtons = CreateJoystickButtonsList(state.Buttons, joystick.Value.DeviceInfo.Type, joystick.Value.DeviceInfo.InstanceName, joystick.Key);

                        foreach (var binding in _buttonBindings)
                        {

                            // If there is a button pressed and it is in the list of buttons for the binding
                            if (pressedButtons.Count > 0 && binding.Key.Count > 0 && binding.Key.All(k => pressedButtons.Any(p => JoystickButtonsMatch(p, k))))
                            {
                                string bindingSignature = CreateJoystickBindingSignature(binding.Key);
                                joystickBindingsDownThisPoll.Add(bindingSignature);
                                if (_activeJoystickBindings.Add(bindingSignature))
                                {
                                    DispatchHotkeyAction(binding.Value);
                                }
                            }

                        }
                        /*JoystickUpdate[] bufferedData = joystick.Value.GetBufferedJoystickData();

                        if (bufferedData.Length > 0)
                        {

                            foreach (var upd in bufferedData)
                            {
                                // Check if the button is pressed
                                if (upd.Value > 0 && _buttonBindings.TryGetValue(new JoystickButton(joystick.Value.DeviceInfo.Type, joystick.Value.DeviceInfo.InstanceName, joystick.Key, (int)upd.Offset), out var act))
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
                        }*/
                    }
                    catch (Exception ex)
                    {
                        Trace.Write(ex.Message);
                    }
                }

                _activeKeyboardBindings.RemoveWhere(binding => !keyboardBindingsDownThisPoll.Contains(binding));
                _activeJoystickBindings.RemoveWhere(binding => !joystickBindingsDownThisPoll.Contains(binding));

                Thread.Sleep(intervalMs);
            }
        }

        private void DispatchHotkeyAction(Action action)
        {
            if (Program.AppMainForm == null || Program.AppMainForm.IsDisposed)
                return;

            if (Program.AppMainForm.InvokeRequired)
            {
                Program.AppMainForm.BeginInvoke((MethodInvoker)delegate
                {
                    action();
                });
            }
            else
            {
                action();
            }
        }

        private static string CreateKeyboardBindingSignature(IEnumerable<Key> keys)
        {
            return string.Join("+", keys.OrderBy(k => k).Select(k => k.ToString("G")));
        }

        private static string CreateJoystickBindingSignature(IEnumerable<JoystickButton> buttons)
        {
            return string.Join("+", buttons
                .OrderBy(b => b.DeviceTargetId)
                .ThenBy(b => b.DeviceButtonIndex)
                .Select(b => $"{b.DeviceTargetId:N}:{b.DeviceButtonIndex}"));
        }

        private static bool JoystickButtonsMatch(JoystickButton pressedButton, JoystickButton requiredButton)
        {
            return pressedButton.DeviceTargetId == requiredButton.DeviceTargetId &&
                   pressedButton.DeviceButtonIndex == requiredButton.DeviceButtonIndex;
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
            foreach (var dev in _keyboardDevices.Values) 
            {
                if (dev != null)
                {
                    dev.Unacquire();
                    dev.Dispose();
                }
            }
            foreach (var dev in _joystickDevices.Values) 
            {
                if (dev != null)
                {
                    dev.Unacquire();
                    dev.Dispose();
                }
            }
            _directInput.Dispose();
        }

        /// <summary>
        /// Gets the product name of the joystick device associated with the hotkey.
        /// </summary>
        public List<JoystickButton> CreateJoystickButtonsList(bool[] joystickButtons, DeviceType deviceType, string deviceName, Guid deviceGuid)
        {
            List<JoystickButton> buttons = new List<JoystickButton>();
            for (int i = 0; i < joystickButtons.Length; i++)
            {
                if (joystickButtons[i])
                {
                    buttons.Add(new JoystickButton(deviceType, deviceName, deviceGuid, i));
                }
            }
            return buttons;
        }

        /// <summary>
        /// Gets the product name of the joystick device associated with the hotkey.
        /// </summary>
        public string GetNameOfJoystickHotkey(HotkeyJoystick joystickHotkey)
        {
            foreach (var device in _joystickDevices)
            {

                if (joystickHotkey.Buttons == null || !joystickHotkey.Buttons.Any())
                    return string.Empty;

                return string.Join(" + ", joystickHotkey.Buttons.Select(k => k.ToString()));
               
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
                    logger.Trace($"DirectInputManager/RegisterStoredHotkeys: We have {programSettings.KeyboardHotkeys.Count} keyboard hotkeys to set up and register.");
                    foreach (var hotkey in programSettings.KeyboardHotkeys)
                    {

                        // Check to make sure that the hotkey has at least one key assigned to it, and skip it as faulty if it doesn't
                        if (hotkey.KeyCodes.Count == 0)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Skipping registering key combination as it has no keys associaited with it!");
                            continue;
                        }

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
                    logger.Trace($"DirectInputManager/RegisterStoredHotkeys: We have {programSettings.JoystickHotkeys.Count} joystick and gamepad hotkeys to set up.");
                    foreach (var hotkey in programSettings.JoystickHotkeys)
                    {
                        // Check to make sure that the hotkey has at least one button assigned to it, and skip it as faulty if it doesn't
                        if (hotkey.Buttons.Count == 0)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Skipping registering joystick combination as it has no buttons selected!");
                            continue;
                        }
                        


                        if (hotkey.Task == HotkeyTask.OpenMainWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering buttons '{string.Join("+",hotkey.Buttons)}' to open the main window.");
                            Action openMainWindow = delegate { Program.AppMainForm.openApplicationWindow(); };
                            RegisterJoystickButtons(hotkey.Buttons, openMainWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenDisplayProfileWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering buttons '{string.Join(", ", hotkey.Buttons)}' to open the display profile window.");
                            Action openDisplayProfileWindow = delegate { Program.AppMainForm.openDisplayProfileWindow(); };
                            RegisterJoystickButtons(hotkey.Buttons, openDisplayProfileWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.OpenShortcutLibraryWindow)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering buttons ' {string.Join(", ", hotkey.Buttons)}' to open the shortcut library  window.");
                            Action openShortcutLibraryWindow = delegate { Program.AppMainForm.openShortcutLibraryWindow(); };
                            RegisterJoystickButtons(hotkey.Buttons, openShortcutLibraryWindow);
                        }
                        else if (hotkey.Task == HotkeyTask.RunGameShortcut)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering buttons ' {string.Join(", ", hotkey.Buttons)} ' to run the game shortcut ");
                            Action runGameShortcut = delegate { Program.RunShortcut(hotkey.UUID.ToString()); ; };
                            RegisterJoystickButtons(hotkey.Buttons, runGameShortcut);
                        }
                        else if (hotkey.Task == HotkeyTask.ChangeDisplayProfile)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering buttons ' {string.Join(", ", hotkey.Buttons)} ' to change to display profile ");
                            Action changeDisplayProfile = delegate { Program.RunProfile(hotkey.UUID.ToString()); };
                            RegisterJoystickButtons(hotkey.Buttons, changeDisplayProfile);
                        }
                        else if (hotkey.Task == HotkeyTask.ExitApplication)
                        {
                            logger.Trace($"DirectInputManager/RegisterStoredHotkeys: Registering buttons ' {string.Join(", ", hotkey.Buttons)}' to exit the application.");
                            Action exitApplication = delegate { Program.AppMainForm.exitApplication(); };
                            RegisterJoystickButtons(hotkey.Buttons, exitApplication);
                        }
                        else
                        {
                            logger.Warn($"DirectInputManager/RegisterStoredHotkeys: WARNING - The joystick button '{string.Join(", ", hotkey.Buttons)}'  is not a valid hotkey. Please check the hotkey and try again.");
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
                    foreach (var updatedHotkey in updatedKeyboardHotkeys)
                    {
                        // check if the hotkey is already in the list, and if so, remove it
                        // Remove it from the stored list
                        int numRemoved = Program.AppProgramSettings.KeyboardHotkeys.RemoveAll(k => k.Equals(updatedHotkey));
                        // If it is currently registered, then deregister it
                        if (numRemoved > 0)
                        {
                            RemoveKeyCombination(updatedHotkey.KeyCodes);
                        }                        

                        // Add the key combination to the store of keyboard hotkeys
                        Program.AppProgramSettings.KeyboardHotkeys.Add(updatedHotkey);

                        if (updatedHotkey.Task == HotkeyTask.OpenMainWindow)
                        {
                            Action openMainWindow = delegate { Program.AppMainForm.openApplicationWindow(); };
                            RegisterKeyCombination(updatedHotkey.KeyCodes, openMainWindow);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.OpenDisplayProfileWindow)
                        {
                            Action openDisplayProfileWindow = delegate { Program.AppMainForm.openDisplayProfileWindow(); };
                            RegisterKeyCombination(updatedHotkey.KeyCodes, openDisplayProfileWindow);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.OpenShortcutLibraryWindow)
                        {
                            Action openShortcutLibraryWindow = delegate { Program.AppMainForm.openShortcutLibraryWindow(); };
                            RegisterKeyCombination(updatedHotkey.KeyCodes, openShortcutLibraryWindow);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.RunGameShortcut)
                        {
                            Action runGameShortcut = delegate { Program.RunShortcut(updatedHotkey.UUID.ToString()); ; };
                            RegisterKeyCombination(updatedHotkey.KeyCodes, runGameShortcut);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.ChangeDisplayProfile)
                        {
                            Action changeDisplayProfile = delegate { Program.RunProfile(updatedHotkey.UUID.ToString()); };
                            RegisterKeyCombination(updatedHotkey.KeyCodes, changeDisplayProfile);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.ExitApplication)
                        {
                            Action exitApplication = delegate { Program.AppMainForm.exitApplication(); };
                            RegisterKeyCombination(updatedHotkey.KeyCodes, exitApplication);
                        }
                    }
                }
                if (updatedJoystickHotkeys != null && updatedJoystickHotkeys is List<HotkeyJoystick> && updatedJoystickHotkeys.Count > 0)
                {
                    foreach (var updatedHotkey in updatedJoystickHotkeys)
                    {
                        // check if the hotkey is already in the list, and if so, remove it
                        // Remove it from the stored list
                        int numRemoved = Program.AppProgramSettings.JoystickHotkeys.RemoveAll(k => k.Equals(updatedHotkey));
                        // If it is currently registered, then deregister it
                        if (numRemoved > 0)
                        {
                            RemoveJoystickButtons(updatedHotkey.Buttons);
                        }

                        // Add the button combination to the store of joystick hotkeys
                        Program.AppProgramSettings.JoystickHotkeys.Add(updatedHotkey);

                        if (updatedHotkey.Task == HotkeyTask.OpenMainWindow)
                        {
                            Action openMainWindow = delegate { Program.AppMainForm.openApplicationWindow(); };
                            RegisterJoystickButtons(updatedHotkey.Buttons, openMainWindow);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.OpenDisplayProfileWindow)
                        {
                            Action openDisplayProfileWindow = delegate { Program.AppMainForm.openDisplayProfileWindow(); };
                            RegisterJoystickButtons(updatedHotkey.Buttons, openDisplayProfileWindow);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.OpenShortcutLibraryWindow)
                        {
                            Action openShortcutLibraryWindow = delegate { Program.AppMainForm.openShortcutLibraryWindow(); };
                            RegisterJoystickButtons(updatedHotkey.Buttons, openShortcutLibraryWindow);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.RunGameShortcut)
                        {
                            Action runGameShortcut = delegate { Program.RunShortcut(updatedHotkey.UUID.ToString()); ; };
                            RegisterJoystickButtons(updatedHotkey.Buttons, runGameShortcut);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.ChangeDisplayProfile)
                        {
                            Action changeDisplayProfile = delegate { Program.RunProfile(updatedHotkey.UUID.ToString()); };
                            RegisterJoystickButtons(updatedHotkey.Buttons, changeDisplayProfile);
                        }
                        else if (updatedHotkey.Task == HotkeyTask.ExitApplication)
                        {
                            Action exitApplication = delegate { Program.AppMainForm.exitApplication(); };
                            RegisterJoystickButtons(updatedHotkey.Buttons, exitApplication);
                        }
                    }
                }

                // Save the settings
                Program.AppProgramSettings.SaveSettings();
                

            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/UpdateOrAddHotkeys: WARNING - Exception while trying to updated or add the Hotkeys.");
            }
            return true;
        }

        public bool RemoveHotkeysByName(string hotkeyName)
        {
            try
            {
                if (Program.AppProgramSettings.KeyboardHotkeys != null && Program.AppProgramSettings.KeyboardHotkeys is List<HotkeyKeyboard> && Program.AppProgramSettings.KeyboardHotkeys.Count > 0)
                {
                    foreach (var hotkey in Program.AppProgramSettings.KeyboardHotkeys)
                    {
                        if (GetNameOfKeyboardHotkey(hotkey) == hotkeyName)
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
                    logger.Trace($"DirectInputManager/RemoveHotkeysByName: We have no keyboard hotkeys to remove so skipping them.");
                }
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"DirectInputManager/RemoveHotkeysByName: WARNING - Exception while trying to register the Keyboard Hotkey. It may already be registered to something else. Please choose another Hotkey, or stop the other application from using it.");
            }

            if (Program.AppProgramSettings.JoystickHotkeys != null && Program.AppProgramSettings.JoystickHotkeys is List<HotkeyJoystick> && Program.AppProgramSettings.JoystickHotkeys.Count > 0)
            {
                foreach (var hotkey in Program.AppProgramSettings.JoystickHotkeys)
                {
                    if (GetNameOfJoystickHotkey(hotkey) == hotkeyName)
                    {
                        // Remove it from the stored list
                        Program.AppProgramSettings.JoystickHotkeys.Remove(hotkey);

                        // If it is currently registered, then deregister it
                        RemoveJoystickButtons(hotkey.Buttons);
                    }
                }
            }
            else
            {
                logger.Trace($"DirectInputManager/RemoveHotkeysByName: We have no joystick hotkeys to remove so skipping them.");
            }

            // Save the settings
            Program.AppProgramSettings.SaveSettings();

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
                    logger.Trace($"DirectInputManager/RemoveHotkeysByUUID: We have no keyboard hotkeys to remove so skipping them.");
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
                        RemoveJoystickButtons(hotkey.Buttons);
                    }
                }
            }
            else
            {
                logger.Trace($"DirectInputManager/RemoveHotkeysByUUID: We have no joystick hotkeys to remove so skipping them.");
            }

            // Save the settings
            Program.AppProgramSettings.SaveSettings();

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
            // We want the keyboard hotkeys to win if both are provided. Joystick and keyboard hotkeys do not mix and cannot be used together.
            List<string> keyNames = new List<string>();

            foreach (var button in joystickHotkey.Buttons)
            {
                keyNames.Add(button.ToString());
            }

            return string.Join(" + ", keyNames);            
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

    public class JoystickButtonCombinationComparer : IEqualityComparer<List<JoystickButton>>
    {
        public bool Equals(List<JoystickButton> x, List<JoystickButton> y)
        {
            if (x == null || y == null) return false;
            return x.Count == y.Count && !x.Except(y).Any();
        }

        public int GetHashCode(List<JoystickButton> obj)
        {
            unchecked
            {
                int hash = 17;
                foreach (var button in obj.OrderBy(b => b.DeviceTargetId).ThenBy(b => b.DeviceButtonIndex))
                {
                    hash = hash * 31 + button.DeviceTargetId.GetHashCode();
                    hash = hash * 31 + button.DeviceButtonIndex.GetHashCode();
                }
                return hash;
            }
        }
    }
}
