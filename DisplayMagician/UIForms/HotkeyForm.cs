using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NHotkey.WindowsForms;

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
        KeysConverter kc = new KeysConverter() { };
        //List<int> _needNonShiftModifier = new List<int>() { };
        //List<int> _needNonAltGrModifier = new List<int>() { };


#pragma warning disable CS3003 // Type is not CLS-compliant
        public Keys Hotkey
#pragma warning restore CS3003 // Type is not CLS-compliant
        {
            get;
            /*{ 
                return myHotkey; 
            }*/
            set;
            /*{
                if (value is Keys)
                    myHotkey = value;
            }*/

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

#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public HotkeyForm(Keys hotkeyToEdit = Keys.None, string hotkeyHeading = "", string hotkeyDescription = "")
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            InitializeComponent();

            GenerateInvalidModifiers();
            myHotkey = hotkeyToEdit;
            Refresh(txt_hotkey);

            if (!String.IsNullOrEmpty(hotkeyHeading))
            {
                if (hotkeyHeading.Length > 60)
                    lbl_hotkey_heading.Text = hotkeyHeading.Substring(0,50);
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

            this.Hotkey = myHotkey;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }


        /// <summary>
        /// Fires when a key is pressed down. Here, we'll want to update the Text  
        /// property to notify the user what key combination is currently pressed.
        /// </summary>
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Alt || e.Control || e.Shift)
            {
                myHotkey = e.KeyCode | e.Modifiers;
                Refresh(txt_hotkey);
            }
            
        }

        /// <summary>
        /// Fires when all keys are released. If the current hotkey isn't valid, reset it.
        /// Otherwise, do nothing and keep the Text and hotkey as it was.
        /// </summary>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (myHotkey == Keys.None)
            {
                myHotkey = Keys.None;
                Refresh(txt_hotkey);

                return;
            }
        }

        /// <summary>
        /// Prevents anything entered in Input controls from being displayed.
        /// Without this, a "A" key press would appear as "aControl, Alt + A".
        /// </summary>
        private void OnKeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void Refresh(Control control)
        {
            try
            {
                string parsedHotkey = string.Empty;

                // No hotkey set.
                if (myHotkey == Keys.None)
                {
                    // There is nothing selected so just return
                    txt_hotkey.Text = emptyHotkeyText;
                    return;
                }
                else if (_invalidKeyCombination.Contains(myHotkey))
                {
                    // Tell the user this is an invalid combination
                    parsedHotkey = kc.ConvertToString(myHotkey);

                    // Control also shows as Ctrl+ControlKey, so we trim the +ControlKeu
                    if (parsedHotkey.Contains("+ControlKey"))
                        parsedHotkey = parsedHotkey.Replace("+ControlKey", "");

                    // Shift also shows as Shift+ShiftKey, so we trim the +ShiftKeu
                    if (parsedHotkey.Contains("+ShiftKey"))
                        parsedHotkey = parsedHotkey.Replace("+ShiftKey", "");

                    // Alt also shows as Alt+Menu, so we trim the +Menu
                    if (parsedHotkey.Contains("+Menu"))
                        parsedHotkey = parsedHotkey.Replace("+Menu", "");

                    txt_hotkey.Text = parsedHotkey + $" {invalidHotkeyText}";
                } 
                else
                {
                    // This key combination is ok so lets update the textbox
                    // and save the Hotkey for later
                    parsedHotkey = kc.ConvertToString(myHotkey);

                    // Control also shows as Ctrl+ControlKey, so we trim the +ControlKeu
                    if (parsedHotkey.Contains("+ControlKey"))
                        parsedHotkey = parsedHotkey.Replace("+ControlKey", "");

                    // Shift also shows as Shift+ShiftKey, so we trim the +ShiftKeu
                    if (parsedHotkey.Contains("+ShiftKey"))
                        parsedHotkey = parsedHotkey.Replace("+ShiftKey", "");

                    // Alt also shows as Alt+Menu, so we trim the +Menu
                    if (parsedHotkey.Contains("+Menu"))
                        parsedHotkey = parsedHotkey.Replace("+Menu", "");

                    txt_hotkey.Text = parsedHotkey;
                }
            }
            catch (Exception) { }
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
            _invalidKeyCombination.Add(Keys.Alt | Keys.Menu| Keys.Shift | Keys.ShiftKey);
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

    }
}
