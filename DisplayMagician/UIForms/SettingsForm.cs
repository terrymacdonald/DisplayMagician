using DisplayMagicianShared;
using NHotkey;
using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using WK.Libraries.BootMeUpNS;

namespace DisplayMagician.UIForms
{

    public partial class SettingsForm : Form
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _installedDesktopContextMenu = true;

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
            if (Program.AppProgramSettings.StartOnBootUp == true)
            {
                cb_start_on_boot.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings StartOnBootUp set to true");
            }
            else
            {
                cb_start_on_boot.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings StartOnBootUp set to false");
            }

            // setup minimise DM to system tray when it runs
            if (Program.AppProgramSettings.MinimiseOnStart == true)
            {
                cb_minimise_notification_area.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings MinimiseOnStart set to true");
            }
            else
            {
                cb_minimise_notification_area.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings MinimiseOnStart set to false");
            }

            // show splashscreen on startup 
            if (Program.AppProgramSettings.ShowSplashScreen == true)
            {
                cb_show_splashscreen.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings ShowSplashScreen set to true");
            }
            else
            {
                cb_show_splashscreen.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings ShowSplashScreen set to false");
            }

            // show the minimise reminder message when starting or closing the window 
            if (Program.AppProgramSettings.ShowMinimiseMessageInActionCenter == true)
            {
                cb_show_minimise_action.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings ShowMinimiseMessageInActionCenter set to true");
            }
            else
            {
                cb_show_minimise_action.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings ShowMinimiseMessageInActionCenter set to false");
            }

            // show the status messages in Action Center. Turning this off turns off all messages in Action Center
            if (Program.AppProgramSettings.ShowStatusMessageInActionCenter == true)
            {
                cb_show_status_action.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings ShowStatusMessageInActionCenter set to true");
            }
            else
            {
                cb_show_status_action.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings ShowStatusMessageInActionCenter set to false");
            }

            // start upgrade settings 
            if (Program.AppProgramSettings.UpgradeToPreReleases == true)
            {
                cb_upgrade_prerelease.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings UpgradeToPreReleases set to true");
            }
            else
            {
                cb_upgrade_prerelease.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings UpgradeToPreReleases set to false");
            }

            if (Program.AppProgramSettings.InstalledDesktopContextMenu == true)
            {
                _installedDesktopContextMenu = true;
                btn_context_menu.Text = "Uninstall Desktop Context Menu";
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings InstalledDesktopContextMenu set to true");
            }
            else
            {
                _installedDesktopContextMenu = false;
                btn_context_menu.Text = "Install Desktop Context Menu";
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings InstalledDesktopContextMenu set to false");
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
                    cmb_loglevel.SelectedIndex = cmb_loglevel.FindStringExact(logLevelText["Trace"]);
                    logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings LogLevel set to Trace");
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

        public static bool SetBootMeUp(bool enabled)
        {
            var bootMeUp = new BootMeUp
            {
                UseAlternativeOnFail = true,
                BootArea = BootMeUp.BootAreas.Registry,
                TargetUser = BootMeUp.TargetUsers.CurrentUser
            };

            // save start on Boot up
            if (enabled)
            {
                Program.AppProgramSettings.StartOnBootUp = true;
                bootMeUp.Enabled = true;
                if (!bootMeUp.Successful)
                {
                    logger.Error($"SettingsForm/SettingsForm_FormClosing: Failed to set up DisplayMagician to start when Windows starts");
                    MessageBox.Show("There was an issue setting DisplayMagician to run when the computer starts. Please try launching DisplayMagician again as Admin to see if that helps.");
                    return false;
                }
                else
                {
                    logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully set DisplayMagician to start when Windows starts");
                    return true;
                }
                    
            }
            else
            {
                Program.AppProgramSettings.StartOnBootUp = false;
                bootMeUp.Enabled = false;
                if (!bootMeUp.Successful)
                {
                    logger.Error($"SettingsForm/SettingsForm_FormClosing: Failed to stop DisplayMagician from starting when Windows starts");
                    MessageBox.Show("There was an issue stopping DisplayMagician from running when the computer starts. Please try launching DisplayMagician again as Admin to see if that helps.");
                    return false;
                }
                else
                {
                    logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully stopped DisplayMagician from starting when Windows starts");
                    return true;
                }
                    
            }
        }

        private void SettingsForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            logger.Info($"SettingsForm/SettingsForm_Load: Setting BootMeUp to {cb_start_on_boot.Checked}");
            SetBootMeUp(cb_start_on_boot.Checked);

            // save minimise on close
            if (cb_minimise_notification_area.Checked)
                Program.AppProgramSettings.MinimiseOnStart = true;
            else
                Program.AppProgramSettings.MinimiseOnStart = false;
            logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully saved MinimiseOnStart as {Program.AppProgramSettings.MinimiseOnStart}");

            // save show splashscreen on startup
            if (cb_show_splashscreen.Checked)
                Program.AppProgramSettings.ShowSplashScreen = true;
            else
                Program.AppProgramSettings.ShowSplashScreen = false;
            logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully saved ShowSplashScreen as {Program.AppProgramSettings.ShowSplashScreen}");

            // save show ShowMinimiseMessageInActionCenter 
            if (cb_show_minimise_action.Checked)
                Program.AppProgramSettings.ShowMinimiseMessageInActionCenter = true;
            else
                Program.AppProgramSettings.ShowMinimiseMessageInActionCenter = false;
            logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully saved ShowMinimiseMessageInActionCenter as {Program.AppProgramSettings.ShowMinimiseMessageInActionCenter}");

            // save show ShowStatusMessageInActionCenter 
            if (cb_show_status_action.Checked)
                Program.AppProgramSettings.ShowStatusMessageInActionCenter = true;
            else
                Program.AppProgramSettings.ShowStatusMessageInActionCenter = false;
            logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully saved ShowStatusMessageInActionCenter as {Program.AppProgramSettings.ShowStatusMessageInActionCenter}");


            // save install desktop context menu setting
            if (_installedDesktopContextMenu)
                Program.AppProgramSettings.InstalledDesktopContextMenu = true;
            else
                Program.AppProgramSettings.InstalledDesktopContextMenu = false;
            logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully saved InstallDesktopContextMenu as {Program.AppProgramSettings.InstalledDesktopContextMenu}");

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

            // Save ProgramSettings
            Program.AppProgramSettings.SaveSettings();
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

        private void btn_create_support_package_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    DateTime now = DateTime.Now;
                    saveFileDialog.InitialDirectory = Environment.SpecialFolder.MyDocuments.ToString();
                    saveFileDialog.Filter = "Zip Files(*.zip)| *.zip | All files(*.*) | *.*";
                    saveFileDialog.FilterIndex = 2;
                    saveFileDialog.RestoreDirectory = true;
                    saveFileDialog.FileName = $"DisplayMagician-Support-{now.ToString("yyyyMMdd-HHmm")}.zip";
                    saveFileDialog.Title = "Save a DisplayMagician Support ZIP file";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        //Get the path of specified file
                        string zipFilePath = saveFileDialog.FileName;
                        SharedLogger.logger.Trace($"SettingsForm/btn_create_support_package_Click: Creating support zip file at {zipFilePath}.");

                        if (File.Exists(zipFilePath))
                        {
                            File.Delete(zipFilePath);
                        }

                        ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);
                                                                                              
                        // Get the list of files we want to look for to zip (they may or may not exist)
                        List<string> listOfFiles = new List<string> {
                            // Try to copy the logs if they exist                           
                            Path.Combine(Program.AppLogPath,"DisplayMagician.log"),
                            Path.Combine(Program.AppLogPath,"DisplayMagician1.log"),
                            Path.Combine(Program.AppLogPath,"DisplayMagician2.log"),
                            Path.Combine(Program.AppLogPath,"DisplayMagician3.log"),
                            Path.Combine(Program.AppLogPath,"DisplayMagician4.log"),
                            // Also try to copy the new configs if they exist
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.3.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.2.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.1.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.0.json"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_1.0.json"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.0.json"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.2.json"),
                            Path.Combine(Program.AppDataPath,"Settings_1.0.json"),
                            Path.Combine(Program.AppDataPath,"Settings_2.0.json"),
                            Path.Combine(Program.AppDataPath,"Settings_2.4.json"),
                            // Also try to copy the old configs if they exist
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.3.json.old"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.2.json.old"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.1.json.old"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.0.json.old"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_1.0.json.old"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.0.json.old"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.2.json.old"),
                            Path.Combine(Program.AppDataPath,"Settings_1.0.json.old"),
                            Path.Combine(Program.AppDataPath,"Settings_2.0.json.old"),
                            Path.Combine(Program.AppDataPath,"Settings_2.4.json.old")
                        };
                        foreach (string filename in listOfFiles)
                        {
                            try
                            {
                                if (File.Exists(filename))
                                {
                                    archive.CreateEntryFromFile(filename, Path.GetFileName(filename), CompressionLevel.Optimal);
                                }
                                else
                                {
                                    SharedLogger.logger.Warn($"SettingsForm/btn_create_support_package_Click: Couldn't add {filename} to the support ZIP file {zipFilePath} as it doesn't exist.");
                                }
                                
                            }
                            catch (ArgumentNullException ex)
                            {
                                SharedLogger.logger.Warn(ex, $"SettingsForm/btn_create_support_package_Click: Argument Null Exception while adding files to the support zip file.");
                            }
                            catch (System.Runtime.InteropServices.ExternalException ex)
                            {
                                SharedLogger.logger.Warn(ex, $"SettingsForm/btn_create_support_package_Click: External InteropServices Exception while adding files to the support zip file.");
                            }
                            catch (Exception ex)
                            {
                                SharedLogger.logger.Warn(ex, $"SettingsForm/btn_create_support_package_Click: Exception while while adding files to the support zip file.");
                            }


                        }

                        archive.Dispose();
                        SharedLogger.logger.Trace($"SettingsForm/btn_create_support_package_Click: Finished creating support zip file at {zipFilePath}.");
                        MessageBox.Show($"Created DisplayMagician Support ZIP file {zipFilePath}. You can now attach this file to your GitHub issue.");
                    }
                }
            }
            catch (ArgumentNullException ex)
            {
                SharedLogger.logger.Warn(ex, $"SettingsForm/btn_create_support_package_Click: Argument Null Exception while creating support zip file.");
            }
            catch (System.Runtime.InteropServices.ExternalException ex)
            {
                SharedLogger.logger.Warn(ex, $"SettingsForm/btn_create_support_package_Click: External InteropServices Exception while creating support zip file.");
            }
            catch (Exception ex)
            {
                SharedLogger.logger.Warn(ex, $"SettingsForm/btn_create_support_package_Click: Exception while while creating support zip file.");
            }

            
        }

        private void btn_context_menu_Click(object sender, EventArgs e)
        {
            if (_installedDesktopContextMenu)
            {
                if (Program.InstallDeskTopContextMenu(false))
                {
                    _installedDesktopContextMenu = false;
                    btn_context_menu.Text = "Install Desktop Context Menu";
                    RestartManagerSession.RestartExplorer();
                }
                else
                {
                    MessageBox.Show("We were unable to uninstall the DisplayMagician Desktop Context Menu! Please check your DisplayMagician.log file for more details.",
                                         "Error uninstalling Desktop Context Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                if (Program.InstallDeskTopContextMenu())
                {
                    _installedDesktopContextMenu = true;
                    btn_context_menu.Text = "Uninstall Desktop Context Menu";
                    RestartManagerSession.RestartExplorer();
                }
                else
                {
                    MessageBox.Show("We were unable to install the DisplayMagician Desktop Context Menu! Please check your DisplayMagician.log file for more details.",
                                         "Error uninstalling Desktop Context Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
        }

    }
}
