using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician;
using DisplayMagicianShared;
using DisplayMagicianShared.Windows;
using Microsoft.WindowsAPICodePack.Win32Native;
using static DisplayMagician.WindowsThumbnailProvider;

namespace DisplayMagician.UIForms
{
    public partial class FovCalcForm : Form
    {   
        
        private ScreenLayout _screenLayout = ScreenLayout.TripleScreen;
        private bool _formLoaded = false;

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

            // Center the form on the primary screen
            Utils.CenterOnPrimaryScreen(this);
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            // Save the current settings to the program settings so we can load them later
            Program.AppProgramSettings.FovCalcScreenLayout = _screenLayout;
            double result;
            if (Double.TryParse(txt_bezel_thickness.Text, out result))
            {
                Program.AppProgramSettings.FovCalcBezelSize = result;
            }
            else
            {
                Program.AppProgramSettings.FovCalcBezelSize = 0;
            }
            Program.AppProgramSettings.FovCalcBezelSizeUnit = (ScreenMeasurementUnit)cmb_bezel_thickness.SelectedValue;

            // Next, do the Screen size
            if (Double.TryParse(txt_screen_size.Text, out result))
            {
                Program.AppProgramSettings.FovCalcScreenSize = result;
            }
            else
            {
                Program.AppProgramSettings.FovCalcScreenSize = 0;
            }
            Program.AppProgramSettings.FovCalcScreenSizeUnit = (ScreenMeasurementUnit)cmb_screen_size_units.SelectedValue;

            // Next, do the Distance to Screen
            if (Double.TryParse(txt_distance_to_screen.Text, out result))
            {
                Program.AppProgramSettings.FovCalcDistanceToScreen = result;
            }
            else
            {
                Program.AppProgramSettings.FovCalcDistanceToScreen = 0;
            }
            Program.AppProgramSettings.FovCalcDistanceToScreenUnit = (ScreenMeasurementUnit)cmb_distance_to_screen.SelectedValue;

            // Next, do the Screen Aspect Ratio
            Program.AppProgramSettings.FovCalcAspectRatio = (ScreenAspectRatio)cmb_aspect_ratio.SelectedValue;
            result = 0;
            if (Double.TryParse(txt_aspect_ratio_x.Text, out result))
            {
                Program.AppProgramSettings.FovCalcAspectRatioX = result;
            }
            else
            {
                Program.AppProgramSettings.FovCalcAspectRatioX = 16;
            }
            result = 0;
            if (Double.TryParse(txt_aspect_ratio_y.Text, out result))
            {
                Program.AppProgramSettings.FovCalcAspectRatioY = result;
            }
            else
            {
                Program.AppProgramSettings.FovCalcAspectRatioY = 9;
            }
            Program.AppProgramSettings.SaveSettings();

            this.Close();
        }

        private void btn_single_screen_Click(object sender, EventArgs e)
        {
            _screenLayout = ScreenLayout.SingleScreen;

            btn_single_screen.BackColor = Color.Maroon;
            btn_triple_screens.BackColor = Color.Black;

            cmb_bezel_thickness.Visible = false;
            lbl_bezel_thickness.Visible = false;
            lbl_bezel_thickness_description.Visible = false;
            txt_bezel_thickness.Visible = false;

            if (_formLoaded)
            {
                RunFovCalculation();
            }
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

            if (_formLoaded)
            {
                RunFovCalculation();
            }
        }

        
        private void RunFovCalculation()
        {
            double result;
            // Populate the FOV Calc data, ready for the calculation
            // Firstly do the bezels (if needed)
            if (_screenLayout == ScreenLayout.TripleScreen)
            {
                FovCalculator.ScreenLayout = ScreenLayout.TripleScreen;
                if (Double.TryParse(txt_bezel_thickness.Text, out result))
                {
                    FovCalculator.BezelWidth = result;
                }
                else
                {
                    FovCalculator.BezelWidth = 0;
                }
            }
            else
            {
                FovCalculator.ScreenLayout = ScreenLayout.SingleScreen;
                FovCalculator.BezelWidth = 0;
            }
            if (cmb_bezel_thickness.SelectedValue == null)
            {
                return;
            }
            FovCalculator.BezelWidthUnit = (ScreenMeasurementUnit)cmb_bezel_thickness.SelectedValue;

            // Next, do the Screen size
            if (Double.TryParse(txt_screen_size.Text, out result))
            {
                FovCalculator.ScreenSize = result;
            }
            else
            {
                FovCalculator.ScreenSize = 0;
            }
            if (cmb_screen_size_units.SelectedValue == null)
            {
                return;
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
            if (cmb_distance_to_screen.SelectedValue == null)
            {
                return;
            }
            FovCalculator.DistanceToScreenUnit = (ScreenMeasurementUnit)cmb_distance_to_screen.SelectedValue;

            // Next, do the Screen Aspect Ratio
            if (cmb_aspect_ratio.SelectedValue == null)
            {
                return;
            }
            FovCalculator.ScreenAspectRatio = (ScreenAspectRatio)cmb_aspect_ratio.SelectedValue;
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

            //rtb_results.Text = FovCalculator.PrintResultsToString();
            rtb_results.Rtf = FovCalculator.CreateRtfResults();

            lbl_hresult.Text = FovCalculator.ResultHorizontalDegrees.ToString();
            lbl_vresult.Text = FovCalculator.ResultVerticalDegrees.ToString();

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
            if (_formLoaded)
            {
                RunFovCalculation();
            }
        }

        private void FovCalcForm_Load(object sender, EventArgs e)
        {
            // Refresh the profiles to see whats valid
            ProfileRepository.IsPossibleRefresh();

            // Populate the fov calculator based on the last settings we saved:
            if (Program.AppProgramSettings.FovCalcScreenLayout == ScreenLayout.TripleScreen)
            {
                btn_triple_screens.PerformClick();
            }
            else
            {
                btn_single_screen.PerformClick();
            }
            txt_screen_size.Text = Program.AppProgramSettings.FovCalcScreenSize.ToString();
            cmb_screen_size_units.SelectedValue = Program.AppProgramSettings.FovCalcScreenSizeUnit;
            cmb_aspect_ratio.SelectedValue = Program.AppProgramSettings.FovCalcAspectRatio;
            txt_aspect_ratio_x.Text = Program.AppProgramSettings.FovCalcAspectRatioX.ToString();
            txt_aspect_ratio_y.Text = Program.AppProgramSettings.FovCalcAspectRatioY.ToString();
            txt_distance_to_screen.Text = Program.AppProgramSettings.FovCalcDistanceToScreen.ToString();
            cmb_distance_to_screen.SelectedValue = Program.AppProgramSettings.FovCalcDistanceToScreenUnit;
            txt_bezel_thickness.Text = Program.AppProgramSettings.FovCalcBezelSize.ToString();
            cmb_bezel_thickness.SelectedValue = Program.AppProgramSettings.FovCalcBezelSizeUnit;

            // Use the profiles to populate the form based on the current config
            // If there is 3 or more screens connected, then assume its a triple screen

            if (ProfileRepository.CurrentProfile.WindowsDisplayConfig.DisplayIdentifiers.Count >= 3)
            {
                btn_triple_screens.PerformClick();
            }
            else
            {
                btn_single_screen.PerformClick();
            }

            try
            {
                GDI_DISPLAY_SETTING gdiDevice = ProfileRepository.CurrentProfile.WindowsDisplayConfig.GdiDisplaySettings.Where(dp => dp.Value.IsPrimary).First().Value;
                double aspectRatio = gdiDevice.DeviceMode.PixelsWidth / gdiDevice.DeviceMode.PixelsHeight;
                if (aspectRatio == (16 / 9)) {
                    cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.SixteenByNine;
                }
                else if (aspectRatio == (16 / 10)) {
                    cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.SixteenByTen;
                }
                else if (aspectRatio == (21 / 9)) {
                    cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.TwentyOneByNine;
                }
                else if (aspectRatio == (21 / 10)) {
                    cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.TwentyOneByTen;
                }
                else if (aspectRatio == (32 / 9)) {
                    cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.ThirtyTwoByNine;
                }
                else if (aspectRatio == (32 / 10)) {
                    cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.ThirtyTwoByTen;
                }
                else if (aspectRatio == (4 / 3)) {
                    cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.FourByThree;
                }
                else if (aspectRatio == (5 / 4))
                {
                    cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.FiveByFour;
                }
                else
                {
                    cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.Custom;
                    txt_aspect_ratio_x.Text = gdiDevice.DeviceMode.PixelsWidth.ToString(); 
                    txt_aspect_ratio_y.Text = gdiDevice.DeviceMode.PixelsHeight.ToString();
                }

            }
            catch (Exception ex)
            {
                cmb_aspect_ratio.SelectedValue = ScreenAspectRatio.SixteenByNine;
            }

            RunFovCalculation();

            _formLoaded = true;
        }


        private bool CheckValueWithinRange(TextBox value, ComboBox unit, double lowerBound, double upperBound)
        {
            // Convert bezelSize to cm
            double result = 0;
            double resultInCm = 0;
            if (Double.TryParse(value.Text, out result))
            {

                ScreenMeasurementUnit unitValue = (ScreenMeasurementUnit)unit.SelectedValue;
                if (unitValue == ScreenMeasurementUnit.Inch)
                {
                    resultInCm = result * 2.54;
                }
                else if (unitValue == ScreenMeasurementUnit.MM)
                {
                    resultInCm = result / 10;
                }
                else if (unitValue == ScreenMeasurementUnit.CM)
                {
                    resultInCm = result;
                }
                else
                {
                    // Unit supplied is not one we know about!
                    resultInCm = 0;
                }

                if (resultInCm >= lowerBound && resultInCm <= upperBound)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }

        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            // Save to file
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "rtf files (*.rtf)|*.rtf|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 1;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = "DisplayMagician-FOV-Results.rtf";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filename = saveFileDialog.FileName;

                File.WriteAllText(filename,FovCalculator.SaveRtfResultsFile());
            }
        }

        private void cmb_screen_size_units_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_formLoaded && CheckValueWithinRange(txt_screen_size, cmb_screen_size_units, 17, 508))
            {
                RunFovCalculation();
            }            
        }

        private void cmb_distance_to_screen_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check that distance to screen is between 5cm and 10m
            if (_formLoaded && CheckValueWithinRange(txt_distance_to_screen, cmb_distance_to_screen, 5, 10000))
            { 
                RunFovCalculation();
            }
        }

        private void cmb_bezel_thickness_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Check that bezel size is between 0 and 10cm
            if (_formLoaded && CheckValueWithinRange(txt_bezel_thickness, cmb_bezel_thickness, 0, 10))
            {
                RunFovCalculation();
            }
        }

        private void txt_screen_size_TextChanged(object sender, EventArgs e)
        {
            // Check that screen size is between 17cm and 508cm diagonally (7 inch to 200 inch screen sizes)
            if (_formLoaded && CheckValueWithinRange(txt_screen_size, cmb_screen_size_units, 17, 508))
            {
                RunFovCalculation();
            }
        }

        private void txt_aspect_ratio_y_TextChanged(object sender, EventArgs e)
        {
            if (_formLoaded)
            {
                RunFovCalculation();
            }
        }

        private void txt_aspect_ratio_x_TextChanged(object sender, EventArgs e)
        {
            if (_formLoaded)
            {
                RunFovCalculation();
            }
        }

        private void txt_distance_to_screen_TextChanged(object sender, EventArgs e)
        {
            if (_formLoaded && CheckValueWithinRange(txt_distance_to_screen, cmb_distance_to_screen, 5, 10000))
            {
                RunFovCalculation();
            }
        }

        private void txt_bezel_thickness_TextChanged(object sender, EventArgs e)
        {
            if (_formLoaded && CheckValueWithinRange(txt_bezel_thickness, cmb_bezel_thickness, 0, 10))
            {
                RunFovCalculation();
            }
        }

        private void llbl_markus_ewert_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {        
            System.Diagnostics.Process.Start("https://github.com/dinex86/FOV-Calculator");
        }
    }
}
