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
    public partial class ShortcutLoadingForm : Form
    {
        private string _title = "DisplayMagician is loading...";
        private string _description = "If you have installed a lot of games over time or have a lot of games installed now, this may take a while!";

        public ShortcutLoadingForm()
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


        private void LoadingForm_Load(object sender, EventArgs e)
        {

        }
    }
}
