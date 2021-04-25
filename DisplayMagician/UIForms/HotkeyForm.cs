using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WK.Libraries.HotkeyListenerNS;

namespace DisplayMagician.UIForms
{
    public partial class HotkeyForm : Form
    {
        HotkeySelector hks;
        Hotkey myHotkey = null;

#pragma warning disable CS3003 // Type is not CLS-compliant
        public Hotkey Hotkey
#pragma warning restore CS3003 // Type is not CLS-compliant
        {
            get 
            { 
                return myHotkey; 
            }
            set
            {
                if (value is Hotkey)
                    myHotkey = value;
            }

        }
               

        public HotkeyForm()
        {
            InitializeComponent();

            myHotkey = new Hotkey();

            hks = new HotkeySelector();
            hks.EmptyHotkeyText = "";
            hks.Enable(txt_hotkey);
            this.ActiveControl = txt_hotkey;
        }

#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public HotkeyForm(Hotkey hotkeyToEdit = null, string hotkeyHeading = "", string hotkeyDescription = "")
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            InitializeComponent();

            if (hotkeyToEdit == null)
                myHotkey = new Hotkey();
            else
                myHotkey = hotkeyToEdit;

            hks = new HotkeySelector();
            hks.EmptyHotkeyText = "";
            hks.Enable(txt_hotkey, hotkeyToEdit);
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
            hks.Clear(txt_hotkey);
            this.ActiveControl = txt_hotkey;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {           
            Program.HotkeyListener.Update
            (
                // Reference the current clipping hotkey for directly updating 
                // the hotkey without a need for restarting your application.
                ref myHotkey,

                // Convert the selected hotkey's text representation 
                // to a Hotkey object and update it.
                HotkeyListener.Convert(txt_hotkey.Text)
            );
            this.Hotkey = myHotkey;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
