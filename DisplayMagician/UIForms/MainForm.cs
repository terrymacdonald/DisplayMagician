using System;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using DisplayMagicianShared;
using System.Runtime.InteropServices;
using Windows.UI.Notifications;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System.Net;
using Windows.Data.Xml.Dom;
using Microsoft.Toolkit.Uwp.Notifications;
//using WK.Libraries.HotkeyListenerNS;
using System.Collections.Generic;
using NHotkey.WindowsForms;
using NHotkey;
using System.Linq;
using System.Diagnostics;

namespace DisplayMagician.UIForms
{
    public partial class MainForm : Form
    {

        private bool allowVisible;     // ContextMenu's Show command used
        private bool allowClose;       // ContextMenu's Exit command used
        private List<string> hotkeyDisplayProfiles = new List<string>() { };
        private List<string> hotkeyShortcuts = new List<string>() { };

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public MainForm(Form formToOpen = null)
        {
            InitializeComponent();
            btn_setup_display_profiles.Parent = splitContainer1.Panel1;
            btn_setup_game_shortcuts.Parent = splitContainer1.Panel2;
            lbl_version.Text = string.Format(lbl_version.Text, Assembly.GetExecutingAssembly().GetName().Version);
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = mainContextMenuStrip;
            RefreshNotifyIconMenus();

            try
            {
                if (Program.AppProgramSettings.HotkeyMainWindow != Keys.None)
                    HotkeyManager.Current.AddOrReplace("HotkeyMainWindow", Program.AppProgramSettings.HotkeyMainWindow, OnWindowHotkeyPressed);
            }
            catch (HotkeyAlreadyRegisteredException ex)
            {
                logger.Warn(ex, $"MainForm/MainForm: The '{Program.HotkeyToString(Program.AppProgramSettings.HotkeyMainWindow)}' Hotkey to open the Main Window is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.");
                MessageBox.Show(
                                $"The '{Program.HotkeyToString(Program.AppProgramSettings.HotkeyMainWindow)}' Hotkey you set to open the Main Window is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.", @"DisplayMagician Hotkey Registration Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }

            try
            {
                if (Program.AppProgramSettings.HotkeyDisplayProfileWindow != Keys.None)
                    HotkeyManager.Current.AddOrReplace("HotkeyDisplayProfileWindow", Program.AppProgramSettings.HotkeyDisplayProfileWindow, OnWindowHotkeyPressed);

            }
            catch (HotkeyAlreadyRegisteredException ex)
            {
                logger.Warn(ex, $"MainForm/MainForm: The '{Program.HotkeyToString(Program.AppProgramSettings.HotkeyDisplayProfileWindow)}' Hotkey to open the Display Profile Window is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.");
                MessageBox.Show(
                                $"The '{Program.HotkeyToString(Program.AppProgramSettings.HotkeyDisplayProfileWindow)}' Hotkey you set to open the Display Profile Window is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.", @"DisplayMagician Hotkey Registration Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }

            try
            {
                if (Program.AppProgramSettings.HotkeyShortcutLibraryWindow != Keys.None)
                    HotkeyManager.Current.AddOrReplace("HotkeyShortcutLibraryWindow", Program.AppProgramSettings.HotkeyShortcutLibraryWindow, OnWindowHotkeyPressed);

            }
            catch (HotkeyAlreadyRegisteredException ex)
            {
                logger.Warn(ex, $"MainForm/MainForm: The '{Program.HotkeyToString(Program.AppProgramSettings.HotkeyShortcutLibraryWindow)}' Hotkey to open the Shortcut Library Window is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.");
                MessageBox.Show(
                                $"The '{Program.HotkeyToString(Program.AppProgramSettings.HotkeyShortcutLibraryWindow)}' Hotkey you set to open the Shortcut Library Window is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.", @"DisplayMagician Hotkey Registration Error",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
            }

            // Add all the Profile Hotkeys that are set
            foreach (ProfileItem myProfile in ProfileRepository.AllProfiles)
            {
                if (myProfile.Hotkey != Keys.None)
                {
                    try
                    {
                        HotkeyManager.Current.AddOrReplace(myProfile.UUID, myProfile.Hotkey, OnWindowHotkeyPressed); 
                        hotkeyDisplayProfiles.Add(myProfile.UUID);                        
                    }
                    catch (HotkeyAlreadyRegisteredException ex)
                    {
                        logger.Warn(ex, $"MainForm/MainForm: The '{Program.HotkeyToString(myProfile.Hotkey)}' Hotkey you set to run the {myProfile.Name} Display Profile is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.");
                        MessageBox.Show(
                                        $"The '{Program.HotkeyToString(myProfile.Hotkey)}' Hotkey you set to run the {myProfile.Name} Display Profile is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.", $"DisplayMagician Hotkey Registration Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }
                }
            }

            // Add all the Shortcut Hotkeys that are set
            foreach (ShortcutItem myShortcut in ShortcutRepository.AllShortcuts)
            {
                if (myShortcut.Hotkey != Keys.None)
                {
                    try
                    {
                        HotkeyManager.Current.AddOrReplace(myShortcut.UUID, myShortcut.Hotkey, OnWindowHotkeyPressed);
                        hotkeyShortcuts.Add(myShortcut.UUID);
                    }
                    catch (HotkeyAlreadyRegisteredException ex)
                    {
                        logger.Warn(ex, $"MainForm/MainForm: The '{Program.HotkeyToString(myShortcut.Hotkey)}' Hotkey you set to run the {myShortcut.Name} Shortcut is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.");
                        MessageBox.Show(
                                        $"The '{Program.HotkeyToString(myShortcut.Hotkey)}' Hotkey you set to run the {myShortcut.Name} Shortcut is already registered by something else! We cannot use that Hotkey. Please choose another Hotkey, or stop the other application from using it.", $"DisplayMagician Hotkey Registration Error",
                                        MessageBoxButtons.OK,
                                        MessageBoxIcon.Error);
                    }
                }
            }

            // Shut down the splash screen
            if (Program.AppProgramSettings.ShowSplashScreen && Program.AppSplashScreen != null && !Program.AppSplashScreen.Disposing && !Program.AppSplashScreen.IsDisposed)
                Program.AppSplashScreen.Invoke(new Action(() => Program.AppSplashScreen.Close()));

            if (Program.AppProgramSettings.MinimiseOnStart) 
            {
                // Make the form minimised on start 
                allowVisible = false;
                // Hide the application to notification area when the form is closed
                allowClose = false;
                cb_minimise_notification_area.Checked = true;
                // Change the exit_button text to say 'Close'
                btn_exit.Text = "&Close";

                // Remind the user that DisplayMagician is running the in background
                // Construct the toast content
                ToastContentBuilder tcBuilder = new ToastContentBuilder()
                    .AddToastActivationInfo("notify=minimiseStart&action=open", ToastActivationType.Foreground)
                    .AddText("DisplayMagician is minimised", hintMaxLines: 1)
                    .AddButton("Open", ToastActivationType.Background, "notify=minimiseStart&action=open")
                    .SetToastDuration(ToastDuration.Short);              
                ToastContent toastContent = tcBuilder.Content;
                // Make sure to use Windows.Data.Xml.Dom
                var doc = new XmlDocument();
                doc.LoadXml(toastContent.GetContent());

                // And create the toast notification
                var toast = new ToastNotification(doc);

                // Remove any other Notifications from us
                DesktopNotifications.DesktopNotificationManagerCompat.History.Clear();

                // And then show it
                DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);

            }
            else
            {
                // Make the form show to the user on startup
                allowVisible = true;
                // Really close the application when the form is closed
                allowClose = true;
            }

            if (Program.AppProgramSettings.MinimiseOnStart && Program.AppProgramSettings.StartOnBootUp)
            {
                cb_minimise_notification_area.Checked = true;
            }
            else
            {
                cb_minimise_notification_area.Checked = false;
            }

            // Set the notifyIcon text with the current profile
            if (notifyIcon != null)
            {
                string shortProfileName = ProfileRepository.CurrentProfile.Name;
                if (shortProfileName.Length >= 64)
                {
                    shortProfileName = ProfileRepository.CurrentProfile.Name.Substring(0, 45);
                    
                }
                notifyIcon.Text = $"DisplayMagician ({shortProfileName })";
                Application.DoEvents();
            }
            
            // If we've been handed a Form of some kind, then open it straight away
            if (formToOpen is DisplayProfileForm)
            {
                var displayProfileForm = new DisplayProfileForm();
                displayProfileForm.ShowDialog(this);
            }
            else if (formToOpen is ShortcutLibraryForm)
            {
                var shortcutLibraryForm = new ShortcutLibraryForm();
            shortcutLibraryForm.ShowDialog(this);
            }            
            else
            {
                // Make this window top most if we're not minimised
                if (!Program.AppProgramSettings.MinimiseOnStart)
                {
                    this.TopMost = true;
                    this.Activate();
                    this.TopMost = false;
                }
            }

        }

        protected override void SetVisibleCore(bool value)
        {
            if (!allowVisible)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!allowClose)
            {
                this.Hide();
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            if (cb_minimise_notification_area.Checked)
            {
                // Tell the user that 
                // Construct the toast content
                ToastContentBuilder tcBuilder = new ToastContentBuilder()
                    .AddToastActivationInfo("notify=stillRunning", ToastActivationType.Foreground)
                    .AddText("DisplayMagician is still running...", hintMaxLines: 1)
                    .AddText("DisplayMagician will wait in the background until you need it.")
                    .AddButton("Open DisplayMagician", ToastActivationType.Background, "notify=stillRunning&action=open")
                    .AddButton("Exit DisplayMagician", ToastActivationType.Background, "notify=stillRunning&action=exit")
                    .AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true);
                ToastContent toastContent = tcBuilder.Content;
                // Make sure to use Windows.Data.Xml.Dom
                var doc = new XmlDocument();
                doc.LoadXml(toastContent.GetContent());

                // And create the toast notification
                var toast = new ToastNotification(doc);

                // Remove any other Notifications from us
                DesktopNotifications.DesktopNotificationManagerCompat.History.Clear();
                
                // And then show it
                DesktopNotifications.DesktopNotificationManagerCompat.CreateToastNotifier().Show(toast);
            }
            Application.Exit();
        }

        private void pb_display_profile_Click(object sender, EventArgs e)
        {
            btn_setup_display_profiles.PerformClick();
        }

        private void btn_setup_display_profiles_Click(object sender, EventArgs e)
        {
            logger.Trace($"MainForm/btn_setup_display_profiles_Click: User pressed the Display Profiles button (or selected the menu item)");
            DisplayProfileForm displayProfileForm = null;
            if (Application.OpenForms.OfType<DisplayProfileForm>().Any())
            {
                displayProfileForm = Application.OpenForms.OfType<DisplayProfileForm>().Single();
                displayProfileForm.Activate(); 
                displayProfileForm.Show();
                displayProfileForm.BringToFront();

            }
            else
            {
                displayProfileForm = new DisplayProfileForm();
                displayProfileForm.ShowDialog(this);
            }            
        }

        private void pb_game_shortcut_Click(object sender, EventArgs e)
        {
            btn_setup_game_shortcuts.PerformClick();
        }

        private void btn_setup_game_shortcuts_Click(object sender, EventArgs e)
        {
            logger.Trace($"MainForm/btn_setup_game_shortcuts_Click: User pressed the Game Shortcuts button (or selected the menu item)");
            ShortcutLibraryForm shortcutLibraryForm = null;
            if (Application.OpenForms.OfType<ShortcutLibraryForm>().Any())
            {
                shortcutLibraryForm = Application.OpenForms.OfType<ShortcutLibraryForm>().Single();
                shortcutLibraryForm.Activate();
                shortcutLibraryForm.Show();
                shortcutLibraryForm.BringToFront();
            }
            else
            {
                shortcutLibraryForm = new ShortcutLibraryForm();
                shortcutLibraryForm.ShowDialog(this);
            }            
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Start loading the Steam Games just after the Main form opens
            //SteamGame.GetAllInstalledGames();
            EnableShortcutButtonIfProfiles();

            logger.Trace($"MainForm/MainForm_Load: Main Window has loaded.");
        }

        private void EnableShortcutButtonIfProfiles()
        {
            if (ProfileRepository.AllProfiles.Count > 0)
            {
                btn_setup_game_shortcuts.Visible = true;
                pb_game_shortcut.Enabled = true;
                lbl_create_profile.Visible = false;

                if (ShortcutRepository.AllShortcuts.Count > 0)
                    lbl_create_shortcut.Visible = false;
                else
                    lbl_create_shortcut.Visible = true;
            }
            else
            {
                btn_setup_game_shortcuts.Visible = false;
                pb_game_shortcut.Enabled = false;
                lbl_create_profile.Visible = true;
                lbl_create_shortcut.Visible = false;
            }

        }


        public void RefreshNotifyIconMenus()
        {
            // Clear all the profiles
            profileToolStripMenuItem.DropDownItems.Clear();
            // Prepare the heading shortcuts
            ToolStripMenuItem heading = new ToolStripMenuItem();
            heading.Text = "Display Profiles";
            Font headingFont = new Font(heading.Font, FontStyle.Italic);
            heading.Font = headingFont;
            heading.Enabled = false;
            profileToolStripMenuItem.DropDownItems.Add(heading);
            ToolStripSeparator separator = new ToolStripSeparator();
            profileToolStripMenuItem.DropDownItems.Add(separator);

            
            if (ProfileRepository.AllProfiles.Count > 0)
            {
                // Add the current slist of profiles into the NotifyIcon context menu
                foreach (ProfileItem profile in ProfileRepository.AllProfiles)
                {
                    ToolStripMenuItem profileMenuItem = new ToolStripMenuItem(profile.Name, profile.ProfileBitmap, runProfileToolStripMenuItem_Click);
                    if (!profile.IsPossible)
                    {
                        profileMenuItem.Enabled = false;
                    }                        
                    else if (profile.IsActive)
                    {
                        profileMenuItem.Enabled = true;
                        profileMenuItem.Font = new Font(profileMenuItem.Font, FontStyle.Bold); 
                    }
                    else
                    {
                        profileMenuItem.Enabled = true;
                    }                        
                    profileToolStripMenuItem.DropDownItems.Add(profileMenuItem);
                }

            }

            // Clear all the shortcuts
            shortcutToolStripMenuItem.DropDownItems.Clear();
            // Prepare the heading shortcuts
            heading = new ToolStripMenuItem();
            heading.Text = "Game Shortcuts";
            heading.Font = headingFont;
            heading.Enabled = false;
            shortcutToolStripMenuItem.DropDownItems.Add(heading);
            separator = new ToolStripSeparator();
            shortcutToolStripMenuItem.DropDownItems.Add(separator);
            if (ShortcutRepository.AllShortcuts.Count > 0)
            {
                // Add the current list of profiles into the NotifyIcon context menu
                foreach (ShortcutItem shortcut in ShortcutRepository.AllShortcuts)
                {
                    ToolStripMenuItem shortcutMenuItem = new ToolStripMenuItem(shortcut.Name, shortcut.ShortcutBitmap, runShortcutToolStripMenuItem_Click);
                    if (shortcut.IsValid == ShortcutValidity.Warning || shortcut.IsValid == ShortcutValidity.Error)
                        shortcutMenuItem.Enabled = false;
                    else
                        shortcutMenuItem.Enabled = true;
                    shortcutToolStripMenuItem.DropDownItems.Add(shortcutMenuItem);
                }
            }

            // Apply it by running the Application.DoEvents();
            Application.DoEvents();

        }

        private void runProfileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            ProfileItem profileToRun = null;
            if (menuItem != null)
            {
                foreach (ProfileItem profile in ProfileRepository.AllProfiles)
                {
                    if (profile.Name.Equals(menuItem.Text))
                    {
                        profileToRun = profile;
                        break;
                    }
                }

                // Run the shortcut if it's still there
                if (profileToRun != null)
                    ProfileRepository.ApplyProfile(profileToRun);

                // Also refresh the right-click menu (if we have a main form loaded)
                if (Program.AppMainForm is Form)
                {
                    Program.AppMainForm.RefreshNotifyIconMenus();
                }
            }
        }

        private void runShortcutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var menuItem = sender as ToolStripMenuItem;
            ShortcutItem shortcutToRun = null;
            if (menuItem != null)
            {
                foreach (ShortcutItem shortcut in ShortcutRepository.AllShortcuts)
                {
                    if (shortcut.Name.Equals(menuItem.Text))
                    {
                        shortcutToRun = shortcut;
                        break;
                    }
                }

                // Run the shortcut if it's still there
                if (shortcutToRun != null)
                    ShortcutRepository.RunShortcut(shortcutToRun, notifyIcon);

                // Also refresh the right-click menu (if we have a main form loaded)
                if (Program.AppMainForm is Form)
                {
                    Program.AppMainForm.RefreshNotifyIconMenus();
                }
            }
        }

        public void openApplicationWindow()
        {
            allowVisible = true;
            Restore();
            Show();
            BringToFront();
            this.TopMost = true;
            this.Activate();
            this.TopMost = false;
        }

        public void exitApplication()
        {
            allowClose = true;
            Application.Exit();
        }

        private void openApplicationWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openApplicationWindow();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exitApplication();
        }

        private void cb_minimise_notification_area_CheckedChanged(object sender, EventArgs e)
        {            
            if (cb_minimise_notification_area.Checked)
            {
                // Make the form minimised on start 
                allowVisible = false;
                // Hide the application to notification area when the form is closed
                allowClose = false;
                // Enable the MinimiseOnStart setting
                Program.AppProgramSettings.MinimiseOnStart = true;
                SettingsForm.SetBootMeUp(true);
                // Change the exit_button text to say 'Close'
                btn_exit.Text = "&Close";
            }
            else
            {
                // Make the form show to the user on startup
                allowVisible = true;
                // Really close the application when the form is closed
                allowClose = true;
                // Disable the MinimiseOnStart setting
                Program.AppProgramSettings.MinimiseOnStart = false;
                SettingsForm.SetBootMeUp(false);
                // Change the exit_button text to say 'Exit'
                btn_exit.Text = "&Exit";

            }
        }

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hWnd, uint Msg);

        private const uint SW_RESTORE = 0x09;

        public void Restore()
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowWindow(Handle, SW_RESTORE);
            }
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            EnableShortcutButtonIfProfiles();
        }

        private void btn_settings_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingsForm();
            settingsForm.ShowDialog(this);
            ProgramSettings mySettings = Program.AppProgramSettings;
            // if the MainForm settings are different to the changes made when
            // tweaking the settings in the settings page, then align them
            if (mySettings.MinimiseOnStart && !cb_minimise_notification_area.Checked)
                cb_minimise_notification_area.Checked = true;
            else if (!mySettings.MinimiseOnStart && cb_minimise_notification_area.Checked)
                cb_minimise_notification_area.Checked = false;
        }

        private void lbl_create_shortcut_Click(object sender, EventArgs e)
        {
            btn_setup_game_shortcuts.PerformClick();
        }

        private void lbl_create_profile_Click(object sender, EventArgs e)
        {
            btn_setup_display_profiles.PerformClick();
        }

        private void btn_donate_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/sponsors/terrymacdonald";
            System.Diagnostics.Process.Start(targetURL);
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            openApplicationWindow();
        }
        
#pragma warning disable CS3001 // Argument type is not CLS-compliant
        public void OnWindowHotkeyPressed(object sender, HotkeyEventArgs e)
#pragma warning restore CS3001 // Argument type is not CLS-compliant
        {
            if (e.Name == "HotkeyMainWindow")
                openApplicationWindow();
            else if (e.Name == "HotkeyDisplayProfileWindow")
                btn_setup_display_profiles.PerformClick();
            else if (e.Name == "HotkeyShortcutLibraryWindow")
                btn_setup_game_shortcuts.PerformClick();
            else if (hotkeyDisplayProfiles.Contains(e.Name))
            {
                string displayProfileUUID = e.Name;
                ProfileItem chosenProfile = ProfileRepository.GetProfile(displayProfileUUID);
                if (chosenProfile is ProfileItem)
                    ProfileRepository.ApplyProfile(chosenProfile);
            }
            else if (hotkeyShortcuts.Contains(e.Name))
            {
                string shortcutUUID = e.Name;
                ShortcutItem chosenShortcut = ShortcutRepository.GetShortcut(shortcutUUID);
                if (chosenShortcut is ShortcutItem)
                    ShortcutRepository.RunShortcut(chosenShortcut);
            }
        }

        private void btn_help_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/wiki";
            System.Diagnostics.Process.Start(targetURL);
        }
    }
}
