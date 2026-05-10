using AutoUpdaterDotNET;
using DisplayMagicianShared;
//using NHotkey;
//using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Windows.Forms;
using Vortice.DirectInput;

namespace DisplayMagician.UIForms
{

    public partial class SettingsForm : Form
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private bool _installDesktopContextMenu = true;

        private Dictionary<string, string> logLevelText = new Dictionary<string, string>();

        public SettingsForm()
        {
            logger.Info($"SettingsForm/SettingsForm: Creating a SettingsForm UI Form");

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

            // Setup the Keyboard ListView
            lv_hotkeys.View = View.Details;
            lv_hotkeys.GridLines = true;
            lv_hotkeys.FullRowSelect = true;

            //Add column header
            lv_hotkeys.Columns.Add("", 25); // New column for delete icon!
            lv_hotkeys.Columns.Add("Hotkey Combination", 200);
            lv_hotkeys.Columns.Add("Action", 274);

            // Create ImageList
            var imageList = new ImageList();
            imageList.ImageSize = new Size(16, 16);
            // Directly access strongly-typed resources
            imageList.Images.Add("delete", (Bitmap)Properties.Resources.redcross);
            lv_hotkeys.SmallImageList = imageList;
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

            // load the DLLs that keep NVIDIA and AMD from turning off their dGPUs in gaming laptops
            if (Program.AppProgramSettings.WakeUpGpus == true)
            {
                cb_wake_up_gpus.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings WakeUpGPUs set to true");
            }
            else
            {
                cb_wake_up_gpus.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings WakeUpGpus set to false");
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

            if (Program.AppProgramSettings.UpgradeEnabled == true)
            {
                cb_upgrade_enabled.Checked = true;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings UpgradeEnabled set to true");
            }
            else
            {
                cb_upgrade_enabled.Checked = false;
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings UpgradeEnabled set to false");
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


            // Add the hotkeys from the joystick and keyboard hotkeys to the ListViews
            // Update the listview
            RefreshHotkeyListView();

            // setup the notify icon double click action
            var cmb_notify_icon_double_click = this.Controls.Find("cmb_notify_icon_double_click", true).FirstOrDefault() as ComboBox;
            if (cmb_notify_icon_double_click != null)
            {
                cmb_notify_icon_double_click.Items.Clear();
                cmb_notify_icon_double_click.Items.Add("Do Nothing");
                cmb_notify_icon_double_click.Items.Add("Open Main Window");
                cmb_notify_icon_double_click.Items.Add("Open Game Shortcuts");
                cmb_notify_icon_double_click.Items.Add("Open Display Profiles");
                switch (Program.AppProgramSettings.NotifyIconDoubleClickAction)
                {
                    case NotifyIconDoubleClickAction.DoNothing:
                        cmb_notify_icon_double_click.SelectedItem = "Do Nothing";
                        break;
                    case NotifyIconDoubleClickAction.MainForm:
                        cmb_notify_icon_double_click.SelectedItem = "Open Main Window";
                        break;
                    case NotifyIconDoubleClickAction.DisplayProfileForm:
                        cmb_notify_icon_double_click.SelectedItem = "Open Display Profiles";
                        break;
                    default:
                        cmb_notify_icon_double_click.SelectedItem = "Open Game Shortcuts";
                        break;
                }
                logger.Info($"SettingsForm/SettingsForm_Load: AppProgramSettings NotifyIconDoubleClickAction set to {Program.AppProgramSettings.NotifyIconDoubleClickAction}");
            }
        }

        public static bool SetBootMeUp(bool enabled)
        {

            // save start on Boot up
            if (enabled)
            {
                Program.AppProgramSettings.StartOnBootUp = true;
                if (!StartupManager.EnableStartup())
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
                if (!StartupManager.DisableStartup())
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

            // save the wakeupgpus setting that controls loading the DLLs that keep NVIDIA and AMD from turning off their dGPUs in gaming laptops
            if (cb_wake_up_gpus.Checked == true)
            {
                Program.AppProgramSettings.WakeUpGpus = true;
                logger.Info($"SettingsForm/SettingsForm_FormClosing: AppProgramSettings WakeUpGPUs now set to true");
            }
            else
            {
                Program.AppProgramSettings.WakeUpGpus = false;
                logger.Info($"SettingsForm/SettingsForm_FormClosing: AppProgramSettings WakeUpGpus now set to false");
            }

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

            NLog.LogManager.ReconfigExistingLoggers();

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

            // save upgrade in prereleases setting
            if (cb_upgrade_enabled.Checked)
            {
                Program.AppProgramSettings.UpgradeEnabled = true;
                logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully set DisplayMagician to look for upgrades when starting");
            }
            else
            {
                Program.AppProgramSettings.UpgradeEnabled = false;
                // Stop any autoupdate reminder timers as we no longer want to upgrade
                if (Program.AppUpdateRemindLaterTimer is System.Timers.Timer && Program.AppUpdateRemindLaterTimer.Enabled)
                {
                    Program.AppUpdateRemindLaterTimer.Stop();
                    Program.AppUpdateRemindLaterTimer.Dispose();
                }
                AutoUpdater.PersistenceProvider.SetSkippedVersion(null);
                AutoUpdater.PersistenceProvider.SetRemindLater(DateTime.Now);
                logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully stopped DisplayMagician from looking for upgrades when starting");
            }

            // save notify icon double click action
            var cmb_notify_icon_double_click = this.Controls.Find("cmb_notify_icon_double_click", true).FirstOrDefault() as ComboBox;
            if (cmb_notify_icon_double_click != null)
            {
                if (cmb_notify_icon_double_click.SelectedItem?.ToString() == "Do Nothing")
                    Program.AppProgramSettings.NotifyIconDoubleClickAction = NotifyIconDoubleClickAction.DoNothing;
                else if (cmb_notify_icon_double_click.SelectedItem?.ToString() == "Open Main Window")
                    Program.AppProgramSettings.NotifyIconDoubleClickAction = NotifyIconDoubleClickAction.MainForm;
                else if (cmb_notify_icon_double_click.SelectedItem?.ToString() == "Open Display Profiles")
                    Program.AppProgramSettings.NotifyIconDoubleClickAction = NotifyIconDoubleClickAction.DisplayProfileForm;
                else
                    Program.AppProgramSettings.NotifyIconDoubleClickAction = NotifyIconDoubleClickAction.ShortcutLibraryForm;
                logger.Info($"SettingsForm/SettingsForm_FormClosing: Successfully saved NotifyIconDoubleClickAction as {Program.AppProgramSettings.NotifyIconDoubleClickAction}");
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
            // Find the matching hotkeys so that we can load them in
            // and then show the hotkey form
            /*List<HotkeyKeyboard> _keyboardHotkeys = new List<HotkeyKeyboard>();
            _keyboardHotkeys.AddRange(Program.AppDirectInputManager.GetKeyboardHotkeysByTask(HotkeyTask.OpenMainWindow));
            List<HotkeyJoystick> _joystickHotkeys = new List<HotkeyJoystick>();
            _joystickHotkeys.AddRange(Program.AppDirectInputManager.GetJoystickHotkeysByTask(HotkeyTask.OpenMainWindow));*/

            string hotkeyHeading = $"Manage your Hotkeys for the main DisplayMagician window";
            string hotkeyDescription = $"Choose one or more Hotkeys so that you can open the main DisplayMgician window using your keyboard, joystick or button box. " +
                "This must be a Hotkey that is unique across all your applications otherwise DisplayMagician might not see it. " +
                "Click Add to add it to the list or click the trashcan to remove it from the list. To see all your hotkeys " +
                "go to the Main Window and click the Settings button. ";
            HotkeyForm mainHotkeyForm = new HotkeyForm(HotkeyTask.OpenMainWindow, string.Empty, hotkeyHeading, hotkeyDescription);
            mainHotkeyForm.ShowDialog(this);
            if (mainHotkeyForm.Changed)
            {
                /*// now we store the Hotkey to be saved later
                Program.AppDirectInputManager.UpdateOrAddHotkeys(mainHotkeyForm.ExistingKeyboardHotkeys, mainHotkeyForm.ExistingJoystickHotkeys);*/
                // Update the listview
                RefreshHotkeyListView();
            }
        }

        private void lbl_hotkey_main_window_Click(object sender, EventArgs e)
        {
            btn_hotkey_main_window.PerformClick();
        }

        private void btn_hotkey_display_profile_Click(object sender, EventArgs e)
        {
            // Find the matching hotkeys so that we can load them in
            // and then show the hotkey form
            /*List<HotkeyKeyboard> _keyboardHotkeys = new List<HotkeyKeyboard>();
            _keyboardHotkeys.AddRange(Program.AppDirectInputManager.GetKeyboardHotkeysByTask(HotkeyTask.OpenDisplayProfileWindow));
            List<HotkeyJoystick> _joystickHotkeys = new List<HotkeyJoystick>();
            _joystickHotkeys.AddRange(Program.AppDirectInputManager.GetJoystickHotkeysByTask(HotkeyTask.OpenDisplayProfileWindow));*/

            string hotkeyHeading = $"Manage your Hotkeys for the Display Profile window";
            string hotkeyDescription = $"Choose one or more Hotkeys so that you can open the Display Profile window using your keyboard, joystick or button box. " +
                "This must be a Hotkey that is unique across all your applications otherwise DisplayMagician might not see it. " +
                "Click Add to add it to the list or click the trashcan to remove it from the list. To see all your hotkeys " +
                "go to the Main Window and click the Settings button. ";
            HotkeyForm dpHotkeyForm = new HotkeyForm(HotkeyTask.OpenDisplayProfileWindow, string.Empty, hotkeyHeading, hotkeyDescription);
            dpHotkeyForm.ShowDialog(this);
            if (dpHotkeyForm.Changed)
            {
                /*// now we store the Hotkey to be saved later
                Program.AppDirectInputManager.UpdateOrAddHotkeys(dpHotkeyForm.ExistingKeyboardHotkeys, dpHotkeyForm.ExistingJoystickHotkeys);*/
                // Update the listview
                RefreshHotkeyListView();
            }
        }

        private void lbl_hotkey_display_profile_Click(object sender, EventArgs e)
        {
            btn_hotkey_display_profile.PerformClick();
        }

        private void btn_hotkey_shortcuts_Click(object sender, EventArgs e)
        {
            // Find the matching hotkeys so that we can load them in
            // and then show the hotkey form
            /*List<HotkeyKeyboard> _keyboardHotkeys = new List<HotkeyKeyboard>();
            _keyboardHotkeys.AddRange(Program.AppDirectInputManager.GetKeyboardHotkeysByTask(HotkeyTask.OpenShortcutLibraryWindow));
            List<HotkeyJoystick> _joystickHotkeys = new List<HotkeyJoystick>();
            _joystickHotkeys.AddRange(Program.AppDirectInputManager.GetJoystickHotkeysByTask(HotkeyTask.OpenShortcutLibraryWindow));*/

            string hotkeyHeading = $"Manage your Hotkeys for the Shortcut Library window";
            string hotkeyDescription = $"Choose one or more Hotkeys so that you can open the Shortcut Library window using your keyboard, joystick or button box. " +
                "This must be a Hotkey that is unique across all your applications otherwise DisplayMagician might not see it. " +
                "Click Add to add it to the list or click the trashcan to remove it from the list. To see all your hotkeys " +
                "go to the Main Window and click the Settings button. ";
            HotkeyForm scHotkeyForm = new HotkeyForm(HotkeyTask.OpenShortcutLibraryWindow, string.Empty, hotkeyHeading, hotkeyDescription);
            scHotkeyForm.ShowDialog(this);
            if (scHotkeyForm.Changed)
            {
                /*// now we store the Hotkey to be saved later
                Program.AppDirectInputManager.UpdateOrAddHotkeys(scHotkeyForm.ExistingKeyboardHotkeys, scHotkeyForm.ExistingJoystickHotkeys);*/
                // Update the listview
                RefreshHotkeyListView();
            }
        }

        private void lbl_hotkey_shortcut_library_Click(object sender, EventArgs e)
        {
            btn_hotkey_shortcuts.PerformClick();
        }

        private void btn_hotkey_exit_app_Click(object sender, EventArgs e)
        {
            // Find the matching hotkeys so that we can load them in
            // and then show the hotkey form
            /*List<HotkeyKeyboard> _keyboardHotkeys = new List<HotkeyKeyboard>();
            _keyboardHotkeys.AddRange(Program.AppDirectInputManager.GetKeyboardHotkeysByTask(HotkeyTask.ExitApplication));
            List<HotkeyJoystick> _joystickHotkeys = new List<HotkeyJoystick>();
            _joystickHotkeys.AddRange(Program.AppDirectInputManager.GetJoystickHotkeysByTask(HotkeyTask.ExitApplication));*/

            string hotkeyHeading = $"Manage your Hotkeys to quit DisplayMagician";
            string hotkeyDescription = $"Choose one or more Hotkeys so that you can close and quite DisplayMagician using your keyboard, joystick or button box. " +
                "This must be a Hotkey that is unique across all your applications otherwise DisplayMagician might not see it. " +
                "Click Add to add it to the list or click the trashcan to remove it from the list. To see all your hotkeys " +
                "go to the Main Window and click the Settings button. ";
            HotkeyForm exitHotkeyForm = new HotkeyForm(HotkeyTask.ExitApplication, string.Empty, hotkeyHeading, hotkeyDescription);
            exitHotkeyForm.ShowDialog(this);
            if (exitHotkeyForm.Changed)
            {
                /*// now we store the Hotkey to be saved later
                Program.AppDirectInputManager.UpdateOrAddHotkeys(exitHotkeyForm.ExistingKeyboardHotkeys, exitHotkeyForm.ExistingJoystickHotkeys);*/
                // Update the listview
                RefreshHotkeyListView();
            }
        }

        private void RefreshHotkeyListView()
        {
            //lv_hotkeys.BeginUpdate();
            lv_hotkeys.Items.Clear();
            // Add the hotkeys from the joystick and keyboard hotkeys to the ListViews
            foreach (var keyboardHotkey in Program.AppProgramSettings.KeyboardHotkeys)
            {
                if (keyboardHotkey.KeyCodes.Count > 0)
                {
                    ListViewItem lvItem = new ListViewItem("");
                    lvItem.SubItems.Add(Program.AppDirectInputManager.GenerateKeyboardHotkeyText(keyboardHotkey));
                    lvItem.SubItems.Add(keyboardHotkey.Description);
                    lvItem.ImageIndex = 0; // Set the image index for the delete icon
                    lv_hotkeys.Items.Add(lvItem);
                }
            }

            foreach (var joystickHotkey in Program.AppProgramSettings.JoystickHotkeys)
            {
                if (joystickHotkey.Buttons.Count > 0)
                {
                    ListViewItem lvItem = new ListViewItem("");
                    lvItem.SubItems.Add(Program.AppDirectInputManager.GenerateJoystickHotkeyText(joystickHotkey));
                    lvItem.SubItems.Add(joystickHotkey.Description);
                    lvItem.ImageIndex = 0; // Set the image index for the delete icon
                    lv_hotkeys.Items.Add(lvItem);
                }
            }
            //lv_hotkeys.EndUpdate();
        }

        private void lbl_hotkey_exit_Click(object sender, EventArgs e)
        {
            btn_hotkey_exit.PerformClick();
        }

        private void btn_clear_all_hotkeys_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to clear all the Hotkeys, including the one that open the Main Window, Display Profile Window and Shortcut Library?", "Clear All Hotkeys?", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                // Clear the Hotkeys
                Program.AppProgramSettings.KeyboardHotkeys.Clear();
                Program.AppProgramSettings.JoystickHotkeys.Clear();
                // Save the empty Hotkeys to JSON file
                Program.AppProgramSettings.SaveSettings();
                // Then clear the ListView here too!
                lv_hotkeys.Items.Clear();
            }
        }

        private void btn_create_support_package_Click(object sender, EventArgs e)
        {
            try
            {
                using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                {
                    DateTime now = DateTime.UtcNow;
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

                        NLog.LogManager.SuspendLogging();

                        ZipArchive archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);

                        // Look for log files
                        List<string> listOfLogFiles = Directory.GetFiles(Program.AppLogPath, "DisplayMagician*.log").ToList();


                        // Get the list of files we want to look for to zip (they may or may not exist)
                        List<string> listOfFilesToArchive = new List<string> {
                            // Also try to copy the new configs if they exist
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.6.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.5.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.4.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.3.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.2.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.1.json"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.0.json"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_1.0.json"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.0.json"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.2.json"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.5.json"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.6.json"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts.json"),
                            Path.Combine(Program.AppDataPath,"Settings_1.0.json"),
                            Path.Combine(Program.AppDataPath,"Settings_2.0.json"),
                            Path.Combine(Program.AppDataPath,"Settings_2.3.json"),
                            Path.Combine(Program.AppDataPath,"Settings_2.4.json"),
                            Path.Combine(Program.AppDataPath,"Settings_2.5.json"),
                            Path.Combine(Program.AppDataPath,"Settings_2.6.json"),
                            Path.Combine(Program.AppDataPath,"Settings.json"),
                            // Also try to copy the old configs if they exist
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.6.json.old"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.5.json.old"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.4.json.old"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.3.json.old"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.2.json.old"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.1.json.old"),
                            Path.Combine(Program.AppProfilePath,"DisplayProfiles_2.0.json.old"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_1.0.json.old"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.0.json.old"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.2.json.old"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.5.json.old"),
                            Path.Combine(Program.AppShortcutPath,"Shortcuts_2.6.json.old"),
                            Path.Combine(Program.AppDataPath,"Settings_1.0.json.old"),
                            Path.Combine(Program.AppDataPath,"Settings_2.0.json.old"),
                            Path.Combine(Program.AppDataPath,"Settings_2.3.json.old"),
                            Path.Combine(Program.AppDataPath,"Settings_2.4.json.old"),
                            Path.Combine(Program.AppDataPath,"Settings_2.5.json.old"),
                            Path.Combine(Program.AppDataPath,"Settings_2.6.json.old")
                        };
                        // Also add the log files found (including the new date style formatted ones).
                        listOfFilesToArchive.AddRange(listOfLogFiles);


                        foreach (string filename in listOfFilesToArchive)
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

                        NLog.LogManager.ResumeLogging();

                        SharedLogger.logger.Trace($"SettingsForm/btn_create_support_package_Click: Finished creating support zip file at {zipFilePath}.");
                        MessageBox.Show($"Created DisplayMagician Support ZIP file {zipFilePath}. You can now attach this file to your GitHub issue using your Web Browser.");
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

        private void btn_context_menu_remove_Click(object sender, EventArgs e)
        {
            _installDesktopContextMenu = false;
            Program.AppProgramSettings.InstallDesktopContextMenu = false;

            if (DisplayMagician.ContextMenu.UninstallContextMenu())
            {
                MessageBox.Show("Successfully removed the Desktop Background Context Menu.",
                                        "Removed Desktop Background Context Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("We were unable to remove the DisplayMagician Desktop Background Context Menu! Please check your DisplayMagician.log file for more details.",
                                        "Error removing Desktop Context Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_context_menu_add_Click(object sender, EventArgs e)
        {
            _installDesktopContextMenu = true;
            Program.AppProgramSettings.InstallDesktopContextMenu = true;

            if (DisplayMagician.ContextMenu.InstallContextMenu())
            {
                MessageBox.Show("Successfully added the Desktop Background Context Menu.",
                                        "Added Desktop Background Context Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("We were unable to add the DisplayMagician Desktop Background Context Menu! Please check your DisplayMagician.log file for more details.",
                                        "Error adding Desktop Context Menu", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }


        private void lv_hotkeys_MouseClick(object sender, MouseEventArgs e)
        {
            var hit = lv_hotkeys.HitTest(e.Location);
            if (hit.Item != null && hit.SubItem != null)
            {
                int subItemIndex = hit.Item.SubItems.IndexOf(hit.SubItem);

                // Check if the clicked subitem is the "Delete" column
                if (subItemIndex == 0) // assuming 1st column is Delete
                {
                    if (MessageBox.Show("Are you sure you want to delete this hotkey?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        lv_hotkeys.Items.Remove(hit.Item);
                        // remove the joystick hotkey from the list stored in the settings and deregister it
                        Program.AppDirectInputManager.RemoveHotkeysByName(hit.Item.SubItems[1].Text);
                    }
                }
            }
        }

    }
}
