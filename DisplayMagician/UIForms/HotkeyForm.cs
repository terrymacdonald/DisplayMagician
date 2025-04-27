using System;
using System.Collections.Generic;
using System.Resources;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician;
//using DisplayMagician.Resources;
using Microsoft.VisualBasic.Devices;
using SharpGen.Runtime;
using Vortice.DirectInput;
using WinCopies.Util;

namespace DisplayMagician.UIForms
{
    public partial class HotkeyForm : Form
    {
        //List<Keys> _invalidKeyCombination = new List<Keys>() { };
        private List<HotkeyKeyboard> _shownKeyboardHotkeys = new();
        private List<HotkeyJoystick> _shownJoystickHotkeys = new();

        string _uuid = string.Empty;
        HotkeyTask _taskMode = HotkeyTask.RunGameShortcut;
        bool _changed = false;

        private CancellationTokenSource _captureCts;
        private Thread _captureThread;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private List<Key> _displayedKeys = new List<Key>();
        private List<JoystickButton> _displayedButtons = new();

        private DateTime _lastNonEmptyPressTime = DateTime.MinValue;
        private const double _gracePeriodMilliseconds = 10000; // 10 seconds
        private TimeSpan _gracePeriod = TimeSpan.FromMilliseconds(_gracePeriodMilliseconds);       

        public List<HotkeyKeyboard> ShownKeyboardHotkeys
        {
            get
            {
                if (_shownKeyboardHotkeys == null)
                    _shownKeyboardHotkeys = new List<HotkeyKeyboard>() { };
                return _shownKeyboardHotkeys;
            }
            set
            {
                _shownKeyboardHotkeys = value;
            }
        }

        public List<HotkeyJoystick> ShownJoystickHotkeys
        {
            get
            {
                if (_shownJoystickHotkeys == null)
                    _shownJoystickHotkeys = new List<HotkeyJoystick>() { };
                return _shownJoystickHotkeys;
            }
            set
            {
                _shownJoystickHotkeys = value;
            }
        }

        public string UUID
        {
            get
            {
                if (_uuid == null)
                    _uuid = string.Empty;
                return _uuid;
            }
            set
            {
                _uuid = value;
            }
        }

        public HotkeyTask TaskMode
        {
            get
            {
                return _taskMode;
            }
            set
            {
                _taskMode = value;
            }
        }

        public bool Changed
        {
            get
            {
                return _changed;
            }
            set
            {
                _changed = value;
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

        public HotkeyForm(HotkeyTask taskMode, string uuid, string hotkeyHeading = "", string hotkeyDescription = "")
        {
            InitializeComponent();

            //GenerateInvalidModifiers();

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

            // Set the variables we need
            _taskMode = taskMode;
            UUID = uuid;
            _changed = false;
            _shownKeyboardHotkeys.Clear();
            _shownJoystickHotkeys.Clear();

            // Find the matching hotkeys in the settings file that match what we want to show here and add them to the list of shown hotkeys
            if (String.IsNullOrEmpty(uuid))
            {
                try
                {
                    _shownKeyboardHotkeys = Program.AppProgramSettings.KeyboardHotkeys.Where(k => k.Task == taskMode).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"HotkeyForm/HotkeyForm: Exception attempting to get the keyboard hotkeys from the settings file that match this taskmode {taskMode}.");
                }
                try
                {
                    _shownJoystickHotkeys = Program.AppProgramSettings.JoystickHotkeys.Where(k => k.Task == taskMode).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"HotkeyForm/HotkeyForm: Exception attempting to get the joystick hotkeys from the settings file that match this taskmode {taskMode}.");
                }
            }
            else
            {
                try
                {
                    _shownKeyboardHotkeys = Program.AppProgramSettings.KeyboardHotkeys.Where(k => k.Task == taskMode && k.UUID == uuid).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"HotkeyForm/HotkeyForm: Exception attempting to get the keyboard hotkeys from the settings file that match this taskmode {taskMode} and UUID {UUID}.");
                }
                try
                {
                    _shownJoystickHotkeys = Program.AppProgramSettings.JoystickHotkeys.Where(k => k.Task == taskMode && k.UUID == uuid).ToList();
                }
                catch (Exception ex)
                {
                    logger.Error(ex, $"HotkeyForm/HotkeyForm: Exception attempting to get the joystick hotkeys from the settings file that match this taskmode {taskMode} and UUID {UUID}.");
                }
            }


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
            lv_hotkeys.Columns.Add("", 25); // New column for delete icon!
            lv_hotkeys.Columns.Add("Hotkey Combination", 200);
            lv_hotkeys.Columns.Add("Action", 274);

            // Create ImageList
            var imageList = new ImageList();
            imageList.ImageSize = new Size(16, 16);
            // Directly access strongly-typed resources
            imageList.Images.Add("delete", (Bitmap)Properties.Resources.redcross);
            lv_hotkeys.SmallImageList = imageList;

            // Load the list with any pre-existing hotkeys we have been given
            // Check if the hotkey is a keyboard hotkey or a joystick hotkey, and add it to the list of hotkeys
            if (_shownKeyboardHotkeys.Any())
            {                
                foreach (var existingHotkey in _shownKeyboardHotkeys)
                {
                    ListViewItem lvItem = new ListViewItem("");
                    lvItem.SubItems.Add(Program.AppDirectInputManager.GenerateKeyboardHotkeyText(existingHotkey));
                    lvItem.SubItems.Add(existingHotkey.Description);

                    lvItem.ImageIndex = 0; // Set the image index for the delete icon
                    lv_hotkeys.Items.Add(lvItem);
                }
            }
            if (_shownJoystickHotkeys.Any())
            {               
                foreach (var existingHotkey in _shownJoystickHotkeys)
                {
                    ListViewItem lvItem = new ListViewItem("");
                    lvItem.SubItems.Add(Program.AppDirectInputManager.GenerateJoystickHotkeyText(existingHotkey));
                    lvItem.SubItems.Add(existingHotkey.Description);
                    lvItem.ImageIndex = 0; // Set the image index for the delete icon
                    lv_hotkeys.Items.Add(lvItem);
                }
            }

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
                Name = "DisplayMagician Capture Poller"
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

        }

        private void btn_apply_Click(object sender, EventArgs e)
        {
            if (txt_hotkey.Text == string.Empty)
            {
                logger.Warn($"HotkeyForm/btn_apply_Click: User pressed the Apply button but there was no key combination or button pressed.");
                MessageBox.Show("Please press a key or button combination and then press the Apply button.", "No Hotkey Detected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            
            // Check if the hotkey is a keyboard hotkey or a joystick hotkey, and add it to the list of hotkeys
            if (_displayedKeys.Any())
            {
                // Check that the hotkey wasn't already added
                if (Program.AppProgramSettings.KeyboardHotkeys.Any(h => h.KeyCodes.SequenceEqual(_displayedKeys)))
                {
                    logger.Warn($"HotkeyForm/btn_apply_Click: User pressed the Apply button but the hotkey key combination was already added.");
                    MessageBox.Show("This key combination is already assigned. Please delete the existing hotkey or choose a different combination.", "Duplicate Hotkey Key Combination", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                HotkeyKeyboard newHotkey = new HotkeyKeyboard(_displayedKeys, TaskMode, UUID);
                Program.AppProgramSettings.KeyboardHotkeys.AddIfNotContains(newHotkey);
                ListViewItem lvItem = new ListViewItem("");
                lvItem.SubItems.Add(Program.AppDirectInputManager.GenerateKeyboardHotkeyText(newHotkey));
                lvItem.SubItems.Add(newHotkey.Description);

                lvItem.ImageIndex = 0; // Set the image index for the delete icon
                lv_hotkeys.Items.Add(lvItem);
                _changed = true;
            }
            else if (_displayedButtons.Any())
            {
                // Check that the hotkey wasn't already added
                if (Program.AppProgramSettings.JoystickHotkeys.Any(h => h.Buttons.SequenceEqual(_displayedButtons)))
                {
                    logger.Warn($"HotkeyForm/btn_apply_Click: User pressed the Apply button but the hotkey button combination was already added.");
                    MessageBox.Show("This button combination is already assigned. Please delete the existing hotkey or choose a different combination.", "Duplicate Hotkey Button Combination", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                HotkeyJoystick newHotkey = new HotkeyJoystick(_displayedButtons, TaskMode, UUID);
                Program.AppProgramSettings.JoystickHotkeys.AddIfNotContains(newHotkey);
                ListViewItem lvItem = new ListViewItem("");
                lvItem.SubItems.Add(Program.AppDirectInputManager.GenerateJoystickHotkeyText(newHotkey));
                lvItem.SubItems.Add(newHotkey.Description);
                lvItem.ImageIndex = 0; // Set the image index for the delete icon
                lv_hotkeys.Items.Add(lvItem);
                _changed = true;
            }

            // Also trigger the saving of the hotkey data to the settings file to make the settings permananet
            Program.AppProgramSettings.SaveSettings();

            txt_hotkey.Text = string.Empty;
            _displayedKeys.Clear();
            _displayedButtons.Clear();
            _changed = true;
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }


        private void CaptureLoop(CancellationToken token)
        {

            while (!token.IsCancellationRequested)
            {

                var pressedKeys = new List<Key>();
                var pressedButtons = new List<JoystickButton>();

                // Poll keyboard devices
                foreach (var keyboard in Program.AppDirectInputManager.GetKeyboards())
                {

                    Result result = keyboard.Poll();  // Update state

                    if (result.Failure)
                    {
                        logger.Warn($"HotkeyForm/CaptureLoop: Couldn't poll the keyboard to access the keyboard data.");
                        result = keyboard.Acquire();

                        if (result.Failure)
                        {
                            logger.Warn($"HotkeyForm/CaptureLoop: Couldn't acquire the keyboard to poll it a second time to get the keyboard data.");
                            break;
                        }
                    }

                    try
                    {
                        var state = keyboard.GetCurrentKeyboardState();
                        pressedKeys.AddRange(state.PressedKeys);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"HotkeyForm/CaptureLoop: Exception attempting to read the keyboard data with GetCurrentKeyboardState().");
                    }
                }

                // Poll joystick devices
                foreach (var joystick in Program.AppDirectInputManager.GetJoysticks())
                {
                    Result result = joystick.Poll();  // Update state

                    if (result.Failure)
                    {
                        logger.Warn($"HotkeyForm/CaptureLoop: Couldn't poll the joystick to access the joystick data.");
                        result = joystick.Acquire();

                        if (result.Failure)
                        {
                            logger.Warn($"HotkeyForm/CaptureLoop: Couldn't Acquire the joystick to poll it a second time to get the joystick data.");
                            break;
                        }
                            
                    }

                    try
                    {
                        var state = joystick.GetCurrentJoystickState();
                        var buttons = state.Buttons;
                        for (int i = 0; i < buttons.Length; i++)
                        {
                            if (buttons[i])
                            {
                                pressedButtons.Add(new JoystickButton(joystick.DeviceInfo.Type, joystick.DeviceInfo.InstanceName, joystick.DeviceInfo.InstanceGuid, i));
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, $"HotkeyForm/CaptureLoop: Exception attempting to read the joystick data with GetCurrentJoystickState().");
                    }
                }

                bool isComboBigger = (pressedKeys.Count() > _displayedKeys.Count()) || (pressedButtons.Count() > _displayedButtons.Count());
                bool isPressActive = pressedKeys.Any() || pressedButtons.Any();
                //bool isInvalidKeyCombination = _invalidKeyCombination.Equals(pressedKeys);

                // Check if the new key combination is bigger if it is a keypress
                if (isComboBigger && isPressActive)
                {
                    // New bigger valid combination — display immediately
                    _displayedKeys = new List<Key>(pressedKeys);
                    _displayedButtons = new List<JoystickButton>(pressedButtons);
                    _lastNonEmptyPressTime = DateTime.Now;
                    UpdateHotkeyText(_displayedKeys, _displayedButtons);
                }
                else if (!isPressActive && (DateTime.Now - _lastNonEmptyPressTime) < _gracePeriod)
                {
                    // Still within grace period — keep displaying
                    UpdateHotkeyText(_displayedKeys, _displayedButtons);
                }/* 
                else
                {
                    // We clear the selection if the user has not pressed any keys or buttons for a while
                    _displayedKeys.Clear();
                    _displayedButtons.Clear();
                    UpdateHotkeyText(_displayedKeys, _displayedButtons);
                }*/

                Thread.Sleep(50);
            }
        }

        private void UpdateHotkeyText(List<Key> keys, List<JoystickButton> buttons)
        {

            // We want the keyboard hotkeys to win if both are provided. Joystick and keyboard hotkeys do not mix and cannot be used together.
            if (keys.Count > 0)
            {
                IEnumerable<string> hotkeyNames = keys.Select(k => k.ToString());
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
            else if (buttons.Count > 0)
            {
                IEnumerable<string> hotkeyNames = buttons.Select(b => $"{b.DeviceName} Button #{b.DeviceButtonIndex}");
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
            
        }

        /* private void GenerateInvalidModifiers()
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
         }*/

        private void HotkeyForm_Activated(object sender, EventArgs e)
        {
            //this.ActiveControl = txt_hotkey;
            txt_hotkey.Focus();
            txt_hotkey.DeselectAll();
        }

        private void HotkeyForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_changed)
            {
                DialogResult = DialogResult.OK;
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }

            // Now stop the capture thread to listen for hotkeys
            StopCapture();

            // restart the hotkey monitoring as we're leaving this form
            Program.AppDirectInputManager.Start();

        }

        private void btn_clear_Click_1(object sender, EventArgs e)
        {
            _displayedKeys.Clear();
            _displayedButtons.Clear();
            txt_hotkey.Text = string.Empty;
        }

        private void lv_hotkeys_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = lv_hotkeys.HitTest(e.Location);
            if (hit.Item != null && hit.SubItem != null)
            {
                int subItemIndex = hit.Item.SubItems.IndexOf(hit.SubItem);

                // Check if the clicked subitem is the "Delete" column
                if (subItemIndex == 0) // assuming 1st column is Delete
                {
                    if (MessageBox.Show("Are you sure you want to delete this hotkey?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        lv_hotkeys.Items.Remove(hit.Item);
                        // remove the joystick hotkey from the list stored in the settings and deregister it
                        Program.AppDirectInputManager.RemoveHotkeysByName(hit.Item.SubItems[1].Text);
                        _changed = true;
                    }
                }
            }
        }
    }
}
