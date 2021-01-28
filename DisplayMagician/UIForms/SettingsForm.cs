using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WK.Libraries.BootMeUpNS;

namespace DisplayMagician.UIForms
{

    public partial class SettingsForm : Form
    {

        //ProgramSettings mySettings = null;
        private Dictionary<string, string> logLevelText = new Dictionary<string, string>();

        public SettingsForm()
        {
            InitializeComponent();

            // Populate the LogLevel dictionary
            logLevelText.Add("Trace", "Full Application Trace (very large)");
            logLevelText.Add("Debug", "Detailed Debug messages (large)");
            logLevelText.Add("Info", "Information, Warning and Error messages");
            logLevelText.Add("Warn", "Warning and Error messages only (Default)");
            logLevelText.Add("Error", "Error messages only");
            logLevelText.Add("Fatal", "Fatal Error messages only");

            // Now use it to populate the LogLevel Dropdown
            cmb_loglevel.Items.Clear();
            cmb_loglevel.Items.AddRange(logLevelText.Values.ToArray());
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            // start displaymagician when computer starts
            if (Program.AppProgramSettings.StartOnBootUp)
                cb_start_on_boot.Checked = true;
            else
                cb_start_on_boot.Checked = false;

            // setup minimise on start
            if (Program.AppProgramSettings.MinimiseOnStart)
                cb_minimise_notification_area.Checked = true;
            else
                cb_minimise_notification_area.Checked = false;


            // setup loglevel on start
            switch (Program.AppProgramSettings.LogLevel)
            {
                case "Trace":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Trace"]);
                    break;
                case "Debug":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Debug"]);
                    break;
                case "Info":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Info"]);
                    break;
                case "Warn":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Warn"]);
                    break;
                case "Error":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Error"]);
                    break;
                case "Fatal":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Fatal"]);
                    break;
                default:
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Warn"]);
                    break;
            }

        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            var bootMeUp = new BootMeUp();

            bootMeUp.UseAlternativeOnFail = true;
            bootMeUp.BootArea = BootMeUp.BootAreas.Registry;
            bootMeUp.TargetUser = BootMeUp.TargetUsers.CurrentUser;

            // save start on Boot up
            if (cb_start_on_boot.Checked)
            {
                Program.AppProgramSettings.StartOnBootUp = true;
                bootMeUp.Enabled = true;
                if (!bootMeUp.Successful)
                    MessageBox.Show("There was an issue setting DisplayMagician to run when the computer starts. Please try launching DisplayMagician again as Admin to see if that helps.");
            }
                
            else
            {
                Program.AppProgramSettings.StartOnBootUp = false;
                bootMeUp.Enabled = false;
                if (!bootMeUp.Successful)
                    MessageBox.Show("There was an issue stopping DisplayMagician from running when the computer starts. Please try launching DisplayMagician again as Admin to see if that helps.");

            }

            // save minimise on close
            if (cb_minimise_notification_area.Checked)
                Program.AppProgramSettings.MinimiseOnStart = true;
            else
                Program.AppProgramSettings.MinimiseOnStart = false;

            // save loglevel on close
            if (cmb_loglevel.SelectedItem.Equals(logLevelText["Trace"]))
                Program.AppProgramSettings.LogLevel = "Trace";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Debug"]))
                Program.AppProgramSettings.LogLevel = "Debug";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Info"]))
                Program.AppProgramSettings.LogLevel = "Info";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Warn"]))
                Program.AppProgramSettings.LogLevel = "Warn";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Error"]))
                Program.AppProgramSettings.LogLevel = "Error";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Fatal"]))
                Program.AppProgramSettings.LogLevel = "Fatal";
            else
                Program.AppProgramSettings.LogLevel = "Warn";
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
