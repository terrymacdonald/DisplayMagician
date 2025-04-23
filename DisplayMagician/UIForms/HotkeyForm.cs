using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician;
using Microsoft.VisualBasic.Devices;
using SharpGen.Runtime;
using Vortice.DirectInput;
using WinCopies.Util;

namespace DisplayMagician.UIForms
{
    public partial class HotkeyForm : Form
    {
        //HotkeyListener myHotkeyListener = null;
        //HotkeySelector hks;
        Keys myHotkey = Keys.None;
        string emptyHotkeyText = "";
        string invalidHotkeyText = " (invalid - try again!)";
        List<Keys> _invalidKeyCombination = new List<Keys>() { };
        List<HotkeyKeyboard> _keyboardHotkeys = new List<HotkeyKeyboard>() { };
        List<HotkeyJoystick> _joystickHotkeys = new List<HotkeyJoystick>() { };
        //KeysConverter kc = new KeysConverter() { };

        private CancellationTokenSource _captureCts;
        private Thread _captureThread;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private List<Key> _lastKeys = new List<Key>();
        private List<(Guid deviceId, int buttonIndex)> _lastButtons = new();

        private DateTime _lastUpdateTime = DateTime.MinValue;


        public List<HotkeyKeyboard> KeyboardHotkeys
        {
            get
            {
                if (_keyboardHotkeys == null)
                    _keyboardHotkeys = new List<HotkeyKeyboard>() { };
                return _keyboardHotkeys;
            }
            set
            {
                _keyboardHotkeys = value;
            }
        }

        public List<HotkeyJoystick> JoystickHotkeys
        {
            get
            {
                if (_joystickHotkeys == null)
                    _joystickHotkeys = new List<HotkeyJoystick>() { };
                return _joystickHotkeys;
            }
            set
            {
                _joystickHotkeys = value;
            }
        }


        public HotkeyForm()
        {
            InitializeComponent();

            //hks = new HotkeySelector();
            //hks.EmptyHotkeyText = "";
            //hks.Enable(txt_hotkey);
            //this.ActiveControl = txt_hotkey;
            //txt_hotkey.DeselectAll();
        }

        public HotkeyForm(Keys hotkeyToEdit = Keys.None, string hotkeyHeading = "", string hotkeyDescription = "")
        {
            InitializeComponent();

            GenerateInvalidModifiers();
            myHotkey = hotkeyToEdit;
            //Refresh(txt_hotkey);

            if (!String.IsNullOrEmpty(hotkeyHeading))
            {
                if (hotkeyHeading.Length > 60)
                    lbl_hotkey_heading.Text = hotkeyHeading.Substring(0, 50);
                else
                    lbl_hotkey_heading.Text = hotkeyHeading;
                lbl_hotkey_description.Text = hotkeyDescription;
            }
            else
            {
                lbl_hotkey_heading.Text = $"Choose a Hotkey";
                lbl_hotkey_description.Text = $"Choose a Hotkey (a keyboard shortcut) so that you can apply to this" + Environment.NewLine +
                    "screen using your keyboard. This must be a Hotkey that" + Environment.NewLine +
                    "is unique across all your applications otherwise DisplayMagician" + Environment.NewLine +
                    "might not see it.";
            }
            Point newHeadingPoint = new Point((this.Width - lbl_hotkey_heading.Width) / 2, lbl_hotkey_heading.Location.Y);
            lbl_hotkey_heading.Location = newHeadingPoint;

            // Disable the hotkey monitoring while we're on this form 
            Program.AppDirectInputManager.Stop();
            // Now start the capture thread to listen for hotkeys
            StartCapture();
        }

        private void HotkeyForm_Load(object sender, EventArgs e)
        {
            // Setup the Keyboard ListView
            lv_hotkeys.View = View.Details;
            lv_hotkeys.GridLines = true;
            lv_hotkeys.FullRowSelect = true;

            //Add column header
            lv_hotkeys.Columns.Add("Hotkey Combination", 200);
            lv_hotkeys.Columns.Add("Action", 200);

        }

        /// <summary>
        /// Start the background polling thread (idempotent).
        /// </summary>
        public void StartCapture()
        {
            if (_captureThread?.IsAlive == true) return;

            _captureCts = new CancellationTokenSource();
            _captureThread = new Thread(() => CaptureLoop(_captureCts.Token))
            {
                IsBackground = true,
                Name = "DisplayMagician Input Poller"
            };
            _captureThread.Start();
        }

        /// <summary>
        /// Stops polling and waits for thread to exit.
        /// </summary>
        public void StopCapture()
        {
            if (_captureCts == null) return;
            _captureCts.Cancel();
            _captureThread.Join();
            _captureCts.Dispose();
            _captureCts = null;
            _captureThread = null;
        }

        private void btn_clear_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.None;
            txt_hotkey.Text = "";
            myHotkey = Keys.None;
            //this.ActiveControl = txt_hotkey;
            //txt_hotkey.DeselectAll();
        }

        private void btn_save_Click(object sender, EventArgs e)
        {

            //this.Hotkey = myHotkey;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private void CaptureLoop(CancellationToken token)
        {          

            while (!token.IsCancellationRequested)
            {

                var pressedKeys = new List<Key>();
                var pressedButtons = new List<(Guid deviceId, int buttonIndex)>();

                // Poll keyboard devices
                foreach (var keyboard in Program.AppDirectInputManager.GetKeyboards())
                {

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
                        pressedKeys.AddRange(state.PressedKeys);
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                }

                // Poll joystick devices
                foreach (var joystick in Program.AppDirectInputManager.GetJoysticks())
                {
                    Result result = joystick.Poll();  // Update state

                    if (result.Failure)
                    {
                        result = joystick.Acquire();

                        if (result.Failure)
                            break;
                    }

                    try
                    {
                        var state = joystick.GetCurrentJoystickState();
                        var buttons = state.Buttons;
                        for (int i = 0; i < buttons.Length; i++)
                        {
                            if (buttons[i])
                            {
                                pressedButtons.Add((joystick.DeviceInfo.InstanceGuid, i));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.Write(ex.Message);
                    }
                }

                // Check if 500ms have passed since the last update
                
                if (pressedKeys.Any() || pressedButtons.Any())
                {
                    if (!pressedKeys.SequenceEqual(_lastKeys) || !pressedButtons.SequenceEqual(_lastButtons))
                    {
                        if ((DateTime.Now - _lastUpdateTime).TotalMilliseconds >= 250)
                        {
                            // No change in pressed keys or buttons
                            _lastKeys = new List<Key>(pressedKeys);
                            _lastButtons = new List<(Guid, int)>(pressedButtons);
                            UpdateHotkeyText(_lastKeys, _lastButtons);
                            _lastUpdateTime = DateTime.Now;
                        }
                    }
                }

                Thread.Sleep(50);
            }
        }

        private void UpdateHotkeyText(List<Key> keys, List<(Guid deviceId, int buttonIndex)> buttons)
        {

            // We want the keyboard hotkeys to win if both are provided. Joystick and keyboard hotkeys do not mix and cannot be used together.
            IEnumerable<string> hotkeyNames = keys.Select(k => k.ToString());
            if (!hotkeyNames.Any())
            {
                hotkeyNames = buttons.Select(b => $"Joystick {b.deviceId} Button {b.buttonIndex}");
            }
            string hotkeyText = string.Join(" + ", hotkeyNames);

            if (txt_hotkey.InvokeRequired)
            {
                txt_hotkey.Invoke(new Action(() => txt_hotkey.Text = hotkeyText));
            }
            else
            {
                txt_hotkey.Text = hotkeyText;
            }
        }

        private void GenerateInvalidModifiers()
        {
            // Create a List of all Invalid key combinations
            _invalidKeyCombination = new List<Keys>() { };

            // Shift by itself
            _invalidKeyCombination.Add(Keys.Shift);
            _invalidKeyCombination.Add(Keys.ShiftKey);
            _invalidKeyCombination.Add(Keys.Shift | Keys.ShiftKey);
            // Control by itself
            _invalidKeyCombination.Add(Keys.Control);
            _invalidKeyCombination.Add(Keys.ControlKey);
            _invalidKeyCombination.Add(Keys.Control | Keys.ControlKey);
            // Alt by itself
            _invalidKeyCombination.Add(Keys.Alt);
            _invalidKeyCombination.Add(Keys.Menu);
            _invalidKeyCombination.Add(Keys.Alt | Keys.Menu);
            // Control + Alt
            _invalidKeyCombination.Add(Keys.Control | Keys.Alt);
            _invalidKeyCombination.Add(Keys.Control | Keys.Alt | Keys.Menu);
            _invalidKeyCombination.Add(Keys.Control | Keys.ControlKey | Keys.Alt | Keys.Menu);
            // Control + Shift
            _invalidKeyCombination.Add(Keys.Control | Keys.Shift);
            _invalidKeyCombination.Add(Keys.Control | Keys.Shift | Keys.ShiftKey);
            _invalidKeyCombination.Add(Keys.Control | Keys.ControlKey | Keys.Shift | Keys.ShiftKey);
            // Shift + Alt
            _invalidKeyCombination.Add(Keys.Alt | Keys.Shift);
            _invalidKeyCombination.Add(Keys.Alt | Keys.Shift | Keys.ShiftKey);
            _invalidKeyCombination.Add(Keys.Alt | Keys.Menu | Keys.Shift | Keys.ShiftKey);
            // Ctrl + Shift + Alt
            _invalidKeyCombination.Add(Keys.Alt | Keys.Shift | Keys.Control);
            _invalidKeyCombination.Add(Keys.Alt | Keys.Menu | Keys.Shift | Keys.Control);

            // LWin by itself
            _invalidKeyCombination.Add(Keys.LWin);
            // RWin by itself
            _invalidKeyCombination.Add(Keys.RWin);

            // Delete by itself
            _invalidKeyCombination.Add(Keys.Delete);

            // Shift + 0 - 9, A - Z.
            for (Keys k = Keys.D0; k <= Keys.Z; k++)
                _invalidKeyCombination.Add(Keys.Shift | k);

            // Shift + Numpad keys.
            for (Keys k = Keys.NumPad0; k <= Keys.NumPad9; k++)
                _invalidKeyCombination.Add(Keys.Shift | k);

            // Shift + Misc (,;<./ etc).
            for (Keys k = Keys.Oem1; k <= Keys.OemBackslash; k++)
                _invalidKeyCombination.Add(Keys.Shift | k);

            // Shift + Space, PgUp, PgDn, End, Home.
            for (Keys k = Keys.Space; k <= Keys.Home; k++)
                _invalidKeyCombination.Add(Keys.Shift | k);

            // Misc keys that we can't loop through.
            _invalidKeyCombination.Add(Keys.Shift | Keys.Insert);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Help);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Multiply);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Add);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Subtract);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Divide);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Decimal);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Return);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Escape);
            _invalidKeyCombination.Add(Keys.Shift | Keys.NumLock);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Scroll);
            _invalidKeyCombination.Add(Keys.Shift | Keys.Pause);

            // Ctrl+Alt + 0 - 9.
            for (Keys k = Keys.D0; k <= Keys.D9; k++)
                _invalidKeyCombination.Add(Keys.Control | Keys.Alt & k);

            // Ctrl + Del
            _invalidKeyCombination.Add(Keys.Control | Keys.Delete);
        }

        private void HotkeyForm_Activated(object sender, EventArgs e)
        {
            //this.ActiveControl = txt_hotkey;
            txt_hotkey.Focus();
            txt_hotkey.DeselectAll();
        }

        private void HotkeyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Now stop the capture thread to listen for hotkeys
            StopCapture();

            // restart the hotkey monitoring as we're leaving this form
            Program.AppDirectInputManager.Start();
            
        }
    }
}
