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
        private Point _wantedLocation = new Point(0,0);

        public ShortcutLoadingForm()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            /*_owner = owner;            
            int resultX = _owner.DisplayRectangle.X + _owner.Width - this.Width;
            int resultY = _owner.DisplayRectangle.Y + _owner.Height - this.Height;
            _owner.Location = new Point(resultX, resultY);*/
            this.TopMost = true;
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

        public Point WantedLocation
        {
            set
            {
                _wantedLocation = value;
            }
        }

        private void LoadingForm_Load(object sender, EventArgs e)
        {
            DesktopLocation = _wantedLocation;
        }

        private void ShortcutLoadingForm_Shown(object sender, EventArgs e)
        {
            DesktopLocation = _wantedLocation;
        }
    }
}
