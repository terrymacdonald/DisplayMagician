using DisplayMagicianShared;
using NHotkey;
using NHotkey.WindowsForms;
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

            // start upgrade settings 
            if (Program.AppProgramSettings.UpgradeToPreReleases)
            {
                cb_upgrade_prerelease.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings UpgradeToPreReleases set to true");
            }
            else
            {
                cb_upgrade_prerelease.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings UpgradeToPreReleases set to false");
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

            // Set the Hotkey values in the form
            UpdateHotkeyLabel(Program.AppProgramSettings.HotkeyMainWindow, lbl_hotkey_main_window);
            logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings HotkeyMainWindow set to {Program.AppProgramSettings.HotkeyMainWindow}");
            UpdateHotkeyLabel(Program.AppProgramSettings.HotkeyDisplayProfileWindow, lbl_hotkey_display_profile);
            logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings HotkeyMainWindow set to {Program.AppProgramSettings.HotkeyDisplayProfileWindow}");
            UpdateHotkeyLabel(Program.AppProgramSettings.HotkeyShortcutLibraryWindow, lbl_hotkey_shortcut_library);
            logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings HotkeyMainWindow set to {Program.AppProgramSettings.HotkeyShortcutLibraryWindow}");

            // Setup the ListView
            lv_dynamic_hotkeys.View = View.Details;
            lv_dynamic_hotkeys.GridLines = true;
            lv_dynamic_hotkeys.FullRowSelect = true;

            //Add column header
            lv_dynamic_hotkeys.Columns.Add("Name", 300);
            lv_dynamic_hotkeys.Columns.Add("Hotkey", 150);

            // Add the ListView Group
            ListViewGroup displayProfile = new ListViewGroup("Display Profile");
            ListViewGroup gameShortcut = new ListViewGroup("Game Shortcut");

            lv_dynamic_hotkeys.Groups.Add(displayProfile);
            lv_dynamic_hotkeys.Groups.Add(gameShortcut);

            // Populate the dynamic hotkey list
            foreach (ProfileItem myProfile in ProfileRepository.AllProfiles)
            {
                if (myProfile.Hotkey != Keys.None)
                {
                    string[] itemText = { myProfile.Name, ConvertHotkeyToText(myProfile.Hotkey) };
                    ListViewItem dynamicHotkey = new ListViewItem(itemText);
                    dynamicHotkey.Group = displayProfile;
                    lv_dynamic_hotkeys.Items.Add(dynamicHotkey);

                }
            }

            foreach (ShortcutItem myShortcut in ShortcutRepository.AllShortcuts)
            {
                if (myShortcut.Hotkey != Keys.None)
                {
                    string[] itemText = { myShortcut.Name, ConvertHotkeyToText(myShortcut.Hotkey) };
                    ListViewItem dynamicHotkey = new ListViewItem(itemText);
                    dynamicHotkey.Group = gameShortcut;
                    lv_dynamic_hotkeys.Items.Add(dynamicHotkey);
                }
                
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

            // save upgrade in prereleases setting
            if (cb_upgrade_prerelease.Checked)
            {
                Program.AppProgramSettings.UpgradeToPreReleases = true;
                logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully set DisplayMagician to upgrade to pre-release versions of software");
            }
            else
            {
                Program.AppProgramSettings.UpgradeToPreReleases = false;
                logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully stopped DisplayMagician from upgrading to pre-release versions of software");
            }
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_hotkey_main_window_Click(object sender, EventArgs e)
        {
            Keys testHotkey;
            if (Program.AppProgramSettings.HotkeyMainWindow != Keys.None)
                testHotkey = Program.AppProgramSettings.HotkeyMainWindow;
            else
                testHotkey = Keys.None;
            string hotkeyHeading = $"Choose a Hotkey for the main DisplayMagician window";
            string hotkeyDescription = $"Choose a Hotkey (a keyboard shortcut) so that you can apply use to" + Environment.NewLine +
                "open the main DisplayMgician window. This must be a Hotkey that" + Environment.NewLine +
                "is unique across all your applications otherwise DisplayMagician" + Environment.NewLine +
                "might not see it.";
            HotkeyForm mainHotkeyForm = new HotkeyForm(testHotkey, hotkeyHeading, hotkeyDescription);
            mainHotkeyForm.ShowDialog(this);
            if (mainHotkeyForm.DialogResult == DialogResult.OK)
            {
                // now we save the Hotkey
                Program.AppProgramSettings.HotkeyMainWindow = mainHotkeyForm.Hotkey;
                // And if we get back and this is a Hotkey with a value, we need to show that in the UI
                UpdateHotkeyLabel(Program.AppProgramSettings.HotkeyMainWindow,lbl_hotkey_main_window);
                // Get the MainForm instance
                var mainForm = Application.OpenForms.OfType<MainForm>().Single();
                if (mainHotkeyForm.Hotkey == Keys.None)
                    // Remove the Hotkey if it needs to be removed
                    HotkeyManager.Current.Remove("HotkeyMainWindow");
                else
                    // And then apply the Hotkey now
                    HotkeyManager.Current.AddOrReplace("HotkeyMainWindow", Program.AppProgramSettings.HotkeyMainWindow, mainForm.OnWindowHotkeyPressed);
            }
        }

        private void lbl_hotkey_main_window_Click(object sender, EventArgs e)
        {
            btn_hotkey_main_window.PerformClick();
        }

        private void UpdateHotkeyLabel(Keys myHotkey, Control myControl)
        {
            // And if we get back and this is a Hotkey with a value, we need to show that in the UI
            if (myHotkey != Keys.None)
            {
                KeysConverter kc = new KeysConverter();

                myControl.Text = "Hotkey: " + kc.ConvertToString(myHotkey);
            }
            else
            {
                myControl.Text = "None Set";
            }

        }

        private void btn_hotkey_display_profile_Click(object sender, EventArgs e)
        {
            Keys testHotkey;
            if (Program.AppProgramSettings.HotkeyDisplayProfileWindow != Keys.None)
                testHotkey = Program.AppProgramSettings.HotkeyDisplayProfileWindow;
            else
                testHotkey = Keys.None;
            string hotkeyHeading = $"Choose a Hotkey for the Display Profile window";
            string hotkeyDescription = $"Choose a Hotkey (a keyboard shortcut) so that you can apply use to" + Environment.NewLine +
                "open the Display Profile window. This must be a Hotkey that" + Environment.NewLine +
                "is unique across all your applications otherwise DisplayMagician" + Environment.NewLine +
                "might not see it.";
            HotkeyForm dpHotkeyForm = new HotkeyForm(testHotkey, hotkeyHeading, hotkeyDescription);
            dpHotkeyForm.ShowDialog(this);
            if (dpHotkeyForm.DialogResult == DialogResult.OK)
            {
                // now we save the Hotkey
                Program.AppProgramSettings.HotkeyDisplayProfileWindow = dpHotkeyForm.Hotkey;
                // And if we get back and this is a Hotkey with a value, we need to show that in the UI
                UpdateHotkeyLabel(Program.AppProgramSettings.HotkeyDisplayProfileWindow, lbl_hotkey_display_profile);
                // Get the MainForm instance
                var mainForm = Application.OpenForms.OfType<MainForm>().Single();
                if (dpHotkeyForm.Hotkey == Keys.None)
                    // Remove the Hotkey if it needs to be removed
                    HotkeyManager.Current.Remove("HotkeyDisplayProfileWindow");
                else
                    // And then apply the Hotkey now
                    HotkeyManager.Current.AddOrReplace("HotkeyDisplayProfileWindow", Program.AppProgramSettings.HotkeyDisplayProfileWindow, mainForm.OnWindowHotkeyPressed);
            }
        }

        private void lbl_hotkey_display_profile_Click(object sender, EventArgs e)
        {
            btn_hotkey_display_profile.PerformClick();
        }

        private void btn_hotkey_shortcuts_Click(object sender, EventArgs e)
        {
            Keys testHotkey;
            if (Program.AppProgramSettings.HotkeyShortcutLibraryWindow != Keys.None)
                testHotkey = Program.AppProgramSettings.HotkeyShortcutLibraryWindow;
            else
                testHotkey = Keys.None;
            string hotkeyHeading = $"Choose a Hotkey for the Shortcut Library window";
            string hotkeyDescription = $"Choose a Hotkey (a keyboard shortcut) so that you can apply use to" + Environment.NewLine +
                "open the Shortcut Library window. This must be a Hotkey that" + Environment.NewLine +
                "is unique across all your applications otherwise DisplayMagician" + Environment.NewLine +
                "might not see it.";
            HotkeyForm scHotkeyForm = new HotkeyForm(testHotkey, hotkeyHeading, hotkeyDescription);
            scHotkeyForm.ShowDialog(this);
            if (scHotkeyForm.DialogResult == DialogResult.OK)
            {
                // now we save the Hotkey
                Program.AppProgramSettings.HotkeyShortcutLibraryWindow = scHotkeyForm.Hotkey;
                // And if we get back and this is a Hotkey with a value, we need to show that in the UI
                UpdateHotkeyLabel(Program.AppProgramSettings.HotkeyShortcutLibraryWindow, lbl_hotkey_display_profile);
                // Get the MainForm instance
                var mainForm = Application.OpenForms.OfType<MainForm>().Single();
                if (scHotkeyForm.Hotkey == Keys.None)
                    // Remove the Hotkey if it needs to be removed
                    HotkeyManager.Current.Remove("HotkeyShortcutLibraryWindow");
                else 
                    // And then apply the Hotkey now
                    HotkeyManager.Current.AddOrReplace("HotkeyShortcutLibraryWindow", Program.AppProgramSettings.HotkeyShortcutLibraryWindow, mainForm.OnWindowHotkeyPressed);
            }
        }

        private void lbl_hotkey_shortcut_library_Click(object sender, EventArgs e)
        {
            btn_hotkey_shortcuts.PerformClick();
        }

        private void btn_clear_all_hotkeys_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to clear all the Hotkeys, including the one that open the Main Window, Display Profile Window and Shortcut Library?", "Clear All Hotkeys?", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // Remove the Main Window
                Program.AppProgramSettings.HotkeyMainWindow = Keys.None;
                HotkeyManager.Current.Remove("HotkeyMainWindow");
                UpdateHotkeyLabel(Program.AppProgramSettings.HotkeyMainWindow, lbl_hotkey_main_window);
                // Remove the Display Profile window
                Program.AppProgramSettings.HotkeyDisplayProfileWindow = Keys.None;
                HotkeyManager.Current.Remove("HotkeyDisplayProfileWindow");
                UpdateHotkeyLabel(Program.AppProgramSettings.HotkeyDisplayProfileWindow, lbl_hotkey_display_profile);
                // Remove the Display Profile window
                Program.AppProgramSettings.HotkeyShortcutLibraryWindow = Keys.None;
                HotkeyManager.Current.Remove("HotkeyShortcutLibraryWindow");
                UpdateHotkeyLabel(Program.AppProgramSettings.HotkeyShortcutLibraryWindow, lbl_hotkey_shortcut_library);

                // Clear the dynamic hotkey list
                foreach (ProfileItem myProfile in ProfileRepository.AllProfiles)
                {
                    if (myProfile.Hotkey != Keys.None)
                    {
                        myProfile.Hotkey = Keys.None;
                        HotkeyManager.Current.Remove(myProfile.UUID);
                    }
                }

                foreach (ShortcutItem myShortcut in ShortcutRepository.AllShortcuts)
                {
                    if (myShortcut.Hotkey != Keys.None)
                    {
                        myShortcut.Hotkey = Keys.None;
                        HotkeyManager.Current.Remove(myShortcut.UUID);
                    }
                }
                // Then clear the ListView here too!
                lv_dynamic_hotkeys.Items.Clear();
            }            
        }

        private string ConvertHotkeyToText(Keys hotkey)
        {
            try
            {
                string parsedHotkey = string.Empty;

                // No hotkey set.
                if (hotkey == Keys.None)
                {
                    // There is nothing selected so just return
                    return parsedHotkey;
                }                
                else
                {
                    // This key combination is ok so lets update the textbox
                    // and save the Hotkey for later
                    KeysConverter kc = new KeysConverter() { };
                    parsedHotkey = kc.ConvertToString(hotkey);

                    // Control also shows as Ctrl+ControlKey, so we trim the +ControlKeu
                    if (parsedHotkey.Contains("+ControlKey"))
                        parsedHotkey = parsedHotkey.Replace("+ControlKey", "");

                    // Shift also shows as Shift+ShiftKey, so we trim the +ShiftKeu
                    if (parsedHotkey.Contains("+ShiftKey"))
                        parsedHotkey = parsedHotkey.Replace("+ShiftKey", "");

                    // Alt also shows as Alt+Menu, so we trim the +Menu
                    if (parsedHotkey.Contains("+Menu"))
                        parsedHotkey = parsedHotkey.Replace("+Menu", "");

                    return parsedHotkey;
                }
            }
            catch (Exception) {
                return String.Empty;
            }
        }

    }
}
