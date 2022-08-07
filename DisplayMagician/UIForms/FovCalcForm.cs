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
    public partial class FovCalcForm : Form
    {   
        
        private ScreenLayout _screenLayout = ScreenLayout.TripleScreen;

        public FovCalcForm()
        {
            InitializeComponent();

            Dictionary<ScreenMeasurementUnit, string> measurementComboBoxData = new Dictionary<ScreenMeasurementUnit, string>();
            // Populate the measurementComboBoxData dictionary
            measurementComboBoxData.Add(ScreenMeasurementUnit.CM, "Centimetres");
            measurementComboBoxData.Add(ScreenMeasurementUnit.Inch, "Inches");
            measurementComboBoxData.Add(ScreenMeasurementUnit.MM, "Millimetres");
           
            // Now use it to populate the various comboboxes
            cmb_distance_to_screen.DataSource = new BindingSource(measurementComboBoxData, null);
            cmb_distance_to_screen.DisplayMember = "Value";
            cmb_distance_to_screen.ValueMember = "Key";
            cmb_distance_to_screen.SelectedValue = ScreenMeasurementUnit.CM;

            cmb_bezel_thickness.DataSource = new BindingSource(measurementComboBoxData, null);
            cmb_bezel_thickness.DisplayMember = "Value";
            cmb_bezel_thickness.ValueMember = "Key";
            cmb_bezel_thickness.SelectedValue = ScreenMeasurementUnit.MM;

            cmb_screen_size_units.DataSource = new BindingSource(measurementComboBoxData, null);
            cmb_screen_size_units.DisplayMember = "Value";
            cmb_screen_size_units.ValueMember = "Key";
            cmb_screen_size_units.SelectedValue = ScreenMeasurementUnit.Inch;


            Dictionary<ScreenAspectRatio, string> aspectRatioComboBoxData = new Dictionary<ScreenAspectRatio, string>();
            // Populate the measurementComboBoxData dictionary
            aspectRatioComboBoxData.Add(ScreenAspectRatio.SixteenByNine, "16:9");
            aspectRatioComboBoxData.Add(ScreenAspectRatio.SixteenByTen, "16:10");
            aspectRatioComboBoxData.Add(ScreenAspectRatio.TwentyOneByNine, "21:9");
            aspectRatioComboBoxData.Add(ScreenAspectRatio.TwentyOneByTen, "21:10");
            aspectRatioComboBoxData.Add(ScreenAspectRatio.ThirtyTwoByNine, "32:9");
            aspectRatioComboBoxData.Add(ScreenAspectRatio.ThirtyTwoByTen, "32:10");
            aspectRatioComboBoxData.Add(ScreenAspectRatio.FourByThree, "4:3");
            aspectRatioComboBoxData.Add(ScreenAspectRatio.FiveByFour, "5:4");
            aspectRatioComboBoxData.Add(ScreenAspectRatio.Custom, "Custom");

            // Now use it to populate the various comboboxes
            cmb_aspect_ratio.DataSource = new BindingSource(aspectRatioComboBoxData, null);
            cmb_aspect_ratio.DisplayMember = "Value";
            cmb_aspect_ratio.ValueMember = "Key";

            // Hide the custom aspect ration bits until needed
            lbl_aspect_ratio_arrow.Visible = false;
            lbl_aspect_ratio_separator.Visible = false;
            txt_aspect_ratio_x.Visible = false;
            txt_aspect_ratio_y.Visible = false;


            //CalculateCurrentDisplayLayout();
            FovCalculator.CalculateFOV(
                ScreenLayout.SingleScreen,
                ScreenAspectRatio.SixteenByNine,
                27,
                ScreenMeasurementUnit.Inch,
                56,
                ScreenMeasurementUnit.CM,
                0,
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
            _screenLayout = ScreenLayout.SingleScreen;

            btn_single_screen.BackColor = Color.Maroon;
            btn_triple_screens.BackColor = Color.Black;

            cmb_bezel_thickness.Visible = false;
            lbl_bezel_thickness.Visible = false;
            lbl_bezel_thickness_description.Visible = false;
            txt_bezel_thickness.Visible = false;

        }

        private void btn_triple_screens_Click(object sender, EventArgs e)
        {
            _screenLayout = ScreenLayout.TripleScreen;

            btn_triple_screens.BackColor = Color.Maroon;
            btn_single_screen.BackColor = Color.Black;
            
            cmb_bezel_thickness.Visible = true;
            lbl_bezel_thickness.Visible = true;
            lbl_bezel_thickness_description.Visible = true;
            txt_bezel_thickness.Visible = true;
        }

        private void btn_update_Click(object sender, EventArgs e)
        {
            double result;
            // Populate the FOV Calc data, ready for the calculation
            // Firstly do the bezels (if needed)
            if (_screenLayout == ScreenLayout.TripleScreen)
            {
                if (Double.TryParse(txt_bezel_thickness.Text, out result))
                {
                    FovCalculator.BezelSize = result;
                }
                else
                {
                    FovCalculator.BezelSize = 0;
                }
            }            
            else
            {
                FovCalculator.BezelSize = 0;
            }
            FovCalculator.BezelSizeUnit = (ScreenMeasurementUnit)cmb_bezel_thickness.SelectedValue;

            // Next, do the Screen size
            if (Double.TryParse(txt_screen_size.Text, out result))
            {
                FovCalculator.ScreenSize = result;
            }
            else
            {
                FovCalculator.ScreenSize = 0;
            }
            FovCalculator.ScreenSizeUnit = (ScreenMeasurementUnit)cmb_screen_size_units.SelectedValue;

            // Next, do the Distance to Screen
            if (Double.TryParse(txt_distance_to_screen.Text, out result))
            {
                FovCalculator.DistanceToScreen = result;
            } 
            else
            {
                FovCalculator.DistanceToScreen = 0;
            }
            FovCalculator.DistanceToScreenUnit = (ScreenMeasurementUnit)cmb_distance_to_screen.SelectedValue;

            // Next, do the Screen Aspect Ratio

            if (((ScreenAspectRatio)cmb_aspect_ratio.SelectedValue).Equals(ScreenAspectRatio.Custom))
            {
                result = 0;
                if (Double.TryParse(txt_aspect_ratio_x.Text, out result))
                {
                    FovCalculator.ScreenRatioX = result;
                }
                else
                {
                    FovCalculator.ScreenRatioX = 16;
                }
                result = 0;
                if (Double.TryParse(txt_aspect_ratio_y.Text, out result))
                {
                    FovCalculator.ScreenRatioY = result;
                }
                else
                {
                    FovCalculator.ScreenRatioY = 9;
                }
            }            
            
            // Now actually do the calculation!
            FovCalculator.CalculateFOV();

        }

        private void lbl_bezel_thickness_Click(object sender, EventArgs e)
        {

        }

        private void txt_bezel_thickness_TextChanged(object sender, EventArgs e)
        {

        }

        private void cmb_aspect_ratio_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmb_aspect_ratio.SelectedValue.Equals(ScreenAspectRatio.Custom))
            {
                // Hide the custom aspect ration bits until needed
                lbl_aspect_ratio_arrow.Visible = true;
                lbl_aspect_ratio_separator.Visible = true;
                txt_aspect_ratio_x.Visible = true;
                txt_aspect_ratio_y.Visible = true;
            }
            else
            {
                // Hide the custom aspect ration bits until needed
                lbl_aspect_ratio_arrow.Visible = false;
                lbl_aspect_ratio_separator.Visible = false;
                txt_aspect_ratio_x.Visible = false;
                txt_aspect_ratio_y.Visible = false;
            }
        }
    }
}
