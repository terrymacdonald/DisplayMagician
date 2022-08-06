using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician;
using DisplayMagicianShared;

namespace DisplayMagician.UIForms
{
    public partial class FovForm : Form
    {         

        public FovForm()
        {
            InitializeComponent();
            CalculateCurrentDisplayLayout();
            FovCalculator.CalculateFOV();
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CalculateCurrentDisplayLayout()
        {
            foreach (var display in ProfileRepository.CurrentProfile.WindowsDisplayConfig.DisplayAdapters)
            {
                display.Value
            }
        }
    }
}
