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

    public partial class SettingsForm : Form
    {

        ProgramSettings mySettings = null;
        private Dictionary<string, string> logLevelText = new Dictionary<string, string>();

        public SettingsForm()
        {
            InitializeComponent();

            // Populate the LogLevel dictionary
            logLevelText.Add("Trace", "Full Application Trace (very large)");
            logLevelText.Add("Debug", "Detailed Debug messages (large)");
            logLevelText.Add("Info", "Information, Warning and Error messages");
            logLevelText.Add("Warn", "Warning and Error messages only");
            logLevelText.Add("Error", "Error messages only");
            logLevelText.Add("Fatal", "Fatal Error messages only");

            // Now use it to populate the LogLevel Dropdown
            cmb_loglevel.Items.Clear();
            cmb_loglevel.Items.AddRange(logLevelText.Values.ToArray());
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            // Load the program settings
            mySettings = Program.AppProgramSettings;

            // setup minimise on start
            if (mySettings.MinimiseOnStart)
                cb_minimise_notification_area.Checked = true;
            else
                cb_minimise_notification_area.Checked = false;

            // setup loglevel on start
            switch (mySettings.LogLevel)
            {
                case "Trace":
                    cmb_loglevel.SelectedItem = logLevelText["Trace"];
                    break;
                case "Debug":
                    cmb_loglevel.SelectedItem = logLevelText["Debug"];
                    break;
                case "Info":
                    cmb_loglevel.SelectedItem = logLevelText["Info"];
                    break;
                case "Warn":
                    cmb_loglevel.SelectedItem = logLevelText["Warn"];
                    break;
                case "Error":
                    cmb_loglevel.SelectedItem = logLevelText["Error"];
                    break;
                case "Fatal":
                    cmb_loglevel.SelectedItem = logLevelText["Fatal"];
                    break;
                default:
                    cmb_loglevel.SelectedItem = logLevelText["Warn"];
                    break;
            }

        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            // save minimise on close
            if (cb_minimise_notification_area.Checked)
                mySettings.MinimiseOnStart = true;
            else
                mySettings.MinimiseOnStart = false;

            // save loglevel on close
            if (cmb_loglevel.SelectedItem.Equals(logLevelText["Trace"]))
                mySettings.LogLevel = "Trace";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Debug"]))
                mySettings.LogLevel = "Debug";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Info"]))
                mySettings.LogLevel = "Info";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Warn"]))
                mySettings.LogLevel = "Warn";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Error"]))
                mySettings.LogLevel = "Error";
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Fatal"]))
                mySettings.LogLevel = "Fatal";
            else
                mySettings.LogLevel = "Warn";

        }
    }
}
