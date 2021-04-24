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

        public Hotkey Hotkey {
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

        public HotkeyForm(Hotkey hotkeyToEdit = null, string hotkeyName = "")
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
        }


        private void btn_clear_Click(object sender, EventArgs e)
        {
            hks.Reset(txt_hotkey);
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
    }
}
