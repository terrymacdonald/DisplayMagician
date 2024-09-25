using DisplayMagician.Processes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class DonationForm : Form
    {
        private string _title = "DisplayMagician is loading...";
        private string _description = "If you have installed a lot of games over time or have a lot of games installed now, this may take a while!";
        private int _numberOfStarts = 1;

        public DonationForm()
        {
            InitializeComponent();
            this.TopMost = false;
        }


        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                lbl_title.Text = _title;
                this.Text = _title;
            }
        }

        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                lbl_description.Text = _description;
            }
        }

        public int NumberofStarts
        {
            get
            {
                return _numberOfStarts;
            }
            set
            {
                _numberOfStarts = value;
                lbl_title.Text = $"You've used DisplayMagician {_numberOfStarts} times!";
            }
        }

        private void btn_donate_Click(object sender, EventArgs e)
        {
            string targetURL = "https://github.com/sponsors/terrymacdonald?frequency=one-time";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
            // Update the settings to say that user has donated.
            Utils.UserHasDonated();
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DonationForm_Click(object sender, EventArgs e)
        {
            btn_donate.PerformClick();
        }
    }
}
