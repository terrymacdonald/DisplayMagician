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
            //CalculateCurrentDisplayLayout();
            FovCalculator.CalculateFOV(
                ScreenLayout.TripleScreen,
                ScreenRatio.SixteenByNine,
                27,
                ScreenMeasurementUnit.Inch,
                56,
                ScreenMeasurementUnit.CM,
                7,
                ScreenMeasurementUnit.MM
            );
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /*private void CalculateCurrentDisplayLayout()
        {
            foreach (var display in ProfileRepository.CurrentProfile.WindowsDisplayConfig.DisplayAdapters)
            {
                display.Value
            }
        }*/

        private void btn_single_screen_Click(object sender, EventArgs e)
        {
            cmb_bezel_thickness.Visible = false;
            lbl_bezel_thickness.Visible = false;
            txt_bezel_thickness.Visible = false;
        }

        private void btn_triple_screens_Click(object sender, EventArgs e)
        {
            cmb_bezel_thickness.Visible = true;
            lbl_bezel_thickness.Visible = true;
            txt_bezel_thickness.Visible = true;
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            FovCalculator.CalculateFOV();

        }
    }
}
