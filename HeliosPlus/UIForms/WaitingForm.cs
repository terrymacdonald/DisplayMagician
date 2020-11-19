using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeliosPlus.UIForms
{
    public partial class WaitingForm : Form
    {
        public WaitingForm()
        {
            InitializeComponent();
        }

        private void btn_stop_waiting_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

    }
}
