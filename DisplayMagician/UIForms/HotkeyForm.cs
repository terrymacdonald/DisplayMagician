using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using WK.Libraries.HotkeyListenerNS;
using NHotkey.WindowsForms;

namespace DisplayMagician.UIForms
{
    public partial class HotkeyForm : Form
    {
        //HotkeyListener myHotkeyListener = null;
        //HotkeySelector hks;
        Keys myHotkey = Keys.None;
        string emptyHotkeyText = "";
        string invalidHotkeyText = "";
        List<int> _needNonShiftModifier = new List<int>() { };
        List<int> _needNonAltGrModifier = new List<int>() { };


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
            this.ActiveControl = txt_hotkey;
        }

#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public HotkeyForm(Keys hotkeyToEdit = Keys.None, string hotkeyHeading = "", string hotkeyDescription = "")
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            InitializeComponent();

            myHotkey = hotkeyToEdit;

            this.ActiveControl = txt_hotkey;

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
            this.ActiveControl = txt_hotkey;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {

/*            if (!String.IsNullOrWhiteSpace(txt_hotkey.Text))
            {
                
            }
            else
            {
                myHotkey = Keys.None;
            }
*/            
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
            if (e.KeyData == Keys.Delete || e.KeyData == (Keys.Control | Keys.Delete))
            {
                myHotkey = Keys.None;
                Refresh(txt_hotkey, false);
            }
                

            if (e.KeyData == (Keys.Shift | Keys.Insert))
            {
                myHotkey = Keys.Shift;

                e.Handled = true;
            }

            // Clear the current hotkey.
            if (e.KeyCode == Keys.Back || e.KeyCode == Keys.Delete)
            {
                myHotkey = Keys.None;
                Refresh(txt_hotkey, false);

                return;
            }
            else
            {
                myHotkey = e.KeyCode;

                Refresh(txt_hotkey, false);
            }
        }

        /// <summary>
        /// Fires when all keys are released. If the current hotkey isn't valid, reset it.
        /// Otherwise, do nothing and keep the Text and hotkey as it was.
        /// </summary>
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (myHotkey == Keys.None && Control.ModifierKeys == Keys.None)
            {
                myHotkey = Keys.None;
                Refresh(txt_hotkey, false);

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

        private void Refresh(Control control, bool internalCall)
        {
            try
            {
                string parsedHotkey = string.Empty;

                // No hotkey set.
                if (myHotkey == Keys.None)
                {
                    txt_hotkey.Text = emptyHotkeyText;

                    return;
                }

                // LWin/RWin don't work as hotkeys...
                // (neither do they work as modifier keys in .NET 2.0).
                if (myHotkey == Keys.LWin || myHotkey == Keys.RWin)
                {
                    txt_hotkey.Text = invalidHotkeyText;

                    return;
                }

                // Only validate input if it comes from the user.
                if (internalCall == false)
                {
                    // No modifier or shift only, and a hotkey that needs another modifier.
                    if ((myHotkey == Keys.Shift || myHotkey == Keys.None) &&
                        this._needNonShiftModifier.Contains((int)myHotkey))
                    {
                        if (this.myHotkey == Keys.None)
                        {
                            // Set Ctrl+Alt as the modifier unless Ctrl+Alt+<key> won't work.
                            if (_needNonAltGrModifier.Contains((int)myHotkey) == false)
                            {
                                this.myHotkey |= Keys.Alt | Keys.Control;
                            }
                            else
                            {
                                // ...In that case, use Shift+Alt instead.
                                this.myHotkey |= Keys.Alt | Keys.Shift;
                            }
                        }
                        else
                        {
                            // User pressed Shift and an invalid key (e.g. a letter or a number), 
                            // that needs another set of modifier keys.
                            this.myHotkey = Keys.None;

                            txt_hotkey.Text = this.myHotkey.ToString() + $" + {invalidHotkeyText}";

                            return;
                        }
                    }
                }

                // Without this code, pressing only Ctrl 
                // will show up as "Control + ControlKey", etc.
                if (this.myHotkey == Keys.Menu || /* Alt */
                    this.myHotkey == Keys.ShiftKey ||
                    this.myHotkey == Keys.ControlKey)
                {
                    this.myHotkey = Keys.None;
                }

                // A final compilation of the processed keys in string format.
                parsedHotkey = this.myHotkey.ToString();

                txt_hotkey.Text = parsedHotkey;

                return;
            }
            catch (Exception) { }
        }

        private void GenerateInvalidModifiers()
        {
            // Fill the ArrayLists that contain  
            // all invalid hotkey combinations.
            _needNonShiftModifier = new List<int>() { };
            _needNonAltGrModifier = new List<int>() { };

            // Shift + 0 - 9, A - Z.
            for (Keys k = Keys.D0; k <= Keys.Z; k++)
                _needNonShiftModifier.Add((int)k);

            // Shift + Numpad keys.
            for (Keys k = Keys.NumPad0; k <= Keys.NumPad9; k++)
                _needNonShiftModifier.Add((int)k);

            // Shift + Misc (,;<./ etc).
            for (Keys k = Keys.Oem1; k <= Keys.OemBackslash; k++)
                _needNonShiftModifier.Add((int)k);

            // Shift + Space, PgUp, PgDn, End, Home.
            for (Keys k = Keys.Space; k <= Keys.Home; k++)
                _needNonShiftModifier.Add((int)k);

            // Misc keys that we can't loop through.
            _needNonShiftModifier.Add((int)Keys.Insert);
            _needNonShiftModifier.Add((int)Keys.Help);
            _needNonShiftModifier.Add((int)Keys.Multiply);
            _needNonShiftModifier.Add((int)Keys.Add);
            _needNonShiftModifier.Add((int)Keys.Subtract);
            _needNonShiftModifier.Add((int)Keys.Divide);
            _needNonShiftModifier.Add((int)Keys.Decimal);
            _needNonShiftModifier.Add((int)Keys.Return);
            _needNonShiftModifier.Add((int)Keys.Escape);
            _needNonShiftModifier.Add((int)Keys.NumLock);
            _needNonShiftModifier.Add((int)Keys.Scroll);
            _needNonShiftModifier.Add((int)Keys.Pause);

            // Ctrl+Alt + 0 - 9.
            for (Keys k = Keys.D0; k <= Keys.D9; k++)
                _needNonAltGrModifier.Add((int)k);
        }


    }
}
