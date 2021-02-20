using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WK.Libraries.BootMeUpNS;

namespace DisplayMagician.UIForms
{

    public partial class SettingsForm : Form
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<string, string> logLevelText = new Dictionary<string, string>();

        public SettingsForm()
        {
            logger.Info($"SettingsForm/SettingsForm: Creating a SettingsForm UI Form");

            InitializeComponent();

            // Populate the LogLevel dictionary
            logLevelText.Add("Trace", "Full Application Trace (very large)");
            logLevelText.Add("Debug", "Detailed Debug messages (large)");
            logLevelText.Add("Info", "Information, Warning and Error messages (Default)");
            logLevelText.Add("Warn", "Warning and Error messages only");
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
            {
                cb_start_on_boot.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings StartOnBootUp set to true");
            }
            else
            {
                cb_start_on_boot.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings StartOnBootUp set to false");
            }

            // setup minimise on start
            if (Program.AppProgramSettings.MinimiseOnStart)
            {
                cb_minimise_notification_area.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings MinimiseOnStart set to true");
            }
            else
            {
                cb_minimise_notification_area.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings MinimiseOnStart set to false");
            }


            // setup loglevel on start
            switch (Program.AppProgramSettings.LogLevel)
            {
                case "Trace":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Trace"]);
                    logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings LogLevel set to Trace");
                    break;
                case "Debug":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Debug"]);
                    logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings LogLevel set to Debug");
                    break;
                case "Info":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Info"]);
                    logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings LogLevel set to Info");
                    break;
                case "Warn":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Warn"]);
                    logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings LogLevel set to Warn");
                    break;
                case "Error":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Error"]);
                    logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings LogLevel set to Error");
                    break;
                case "Fatal":
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Fatal"]);
                    logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings LogLevel set to Fatal");
                    break;
                default:
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Info"]);
                    logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings LogLevel set to Info");
                    break;
            }

        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings LogLevel set to Trace");
            var bootMeUp = new BootMeUp
            {
                UseAlternativeOnFail = true,
                BootArea = BootMeUp.BootAreas.Registry,
                TargetUser = BootMeUp.TargetUsers.CurrentUser
            };

            // save start on Boot up
            if (cb_start_on_boot.Checked)
            {
                Program.AppProgramSettings.StartOnBootUp = true;
                bootMeUp.Enabled = true;
                if (!bootMeUp.Successful)
                {
                    logger.Error($"SettingsForm/SettingsForm_FormClosing: Failed to set up DisplayMagician to start when Windows starts");
                    MessageBox.Show("There was an issue setting DisplayMagician to run when the computer starts. Please try launching DisplayMagician again as Admin to see if that helps.");
                }
                else
                    logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully set DisplayMagician to start when Windows starts");
            }
                
            else
            {
                Program.AppProgramSettings.StartOnBootUp = false;
                bootMeUp.Enabled = false;
                if (!bootMeUp.Successful)
                {
                    logger.Error($"SettingsForm/SettingsForm_FormClosing: Failed to stop DisplayMagician from starting when Windows starts");
                    MessageBox.Show("There was an issue stopping DisplayMagician from running when the computer starts. Please try launching DisplayMagician again as Admin to see if that helps.");
                }
                else
                    logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully stopped DisplayMagician from starting when Windows starts");
            }

            // save minimise on close
            if (cb_minimise_notification_area.Checked)
                Program.AppProgramSettings.MinimiseOnStart = true;
            else
                Program.AppProgramSettings.MinimiseOnStart = false;
            logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully saved MinimiseOnStart as {Program.AppProgramSettings.MinimiseOnStart}");

            // save loglevel on close
            // and make that log level live in NLog straight away
            var config = NLog.LogManager.Configuration;
            if (cmb_loglevel.SelectedItem.Equals(logLevelText["Trace"]))
            {
                Program.AppProgramSettings.LogLevel = "Trace";
                config.FindRuleByName("LogToFile").SetLoggingLevels(NLog.LogLevel.Trace, NLog.LogLevel.Fatal);
            }
                
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Debug"]))
            {
                Program.AppProgramSettings.LogLevel = "Debug";
                config.FindRuleByName("LogToFile").SetLoggingLevels(NLog.LogLevel.Debug, NLog.LogLevel.Fatal);
            }
                
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Info"]))
            {
                Program.AppProgramSettings.LogLevel = "Info";
                config.FindRuleByName("LogToFile").SetLoggingLevels(NLog.LogLevel.Info, NLog.LogLevel.Fatal);
            }
                
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Warn"]))
            {
                Program.AppProgramSettings.LogLevel = "Warn";
                config.FindRuleByName("LogToFile").SetLoggingLevels(NLog.LogLevel.Warn, NLog.LogLevel.Fatal);
            }
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Error"]))
            {
                Program.AppProgramSettings.LogLevel = "Error";
                config.FindRuleByName("LogToFile").SetLoggingLevels(NLog.LogLevel.Error, NLog.LogLevel.Fatal);
            }
            else if (cmb_loglevel.SelectedItem.Equals(logLevelText["Fatal"]))
            {
                Program.AppProgramSettings.LogLevel = "Fatal";
                config.FindRuleByName("LogToFile").SetLoggingLevels(NLog.LogLevel.Fatal, NLog.LogLevel.Fatal);
            }
            else
            {
                Program.AppProgramSettings.LogLevel = "Info";
                config.FindRuleByName("LogToFile").SetLoggingLevels(NLog.LogLevel.Info, NLog.LogLevel.Fatal);
            }
            // Use the NLog configuration with the LogLevel we just changed.
            NLog.LogManager.Configuration = config;

            logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully saved LogLevel as {Program.AppProgramSettings.LogLevel}");
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
