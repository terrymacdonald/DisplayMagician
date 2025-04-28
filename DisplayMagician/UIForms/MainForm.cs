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
using System.Linq;
using System.Diagnostics;
using DisplayMagician.Processes;

namespace DisplayMagician.UIForms
{
    public partial class MainForm : Form
    {

        private bool _allowVisible = false;     // Default to not showing form
        private bool _allowClose;       // ContextMenu's Exit command used
        private List<string> hotkeyDisplayProfiles = new List<string>() { };
        private List<string> hotkeyShortcuts = new List<string>() { };

        private DisplayProfileForm DisplayProfileWindow = new DisplayProfileForm();
        private ShortcutLibraryForm ShortcutLibraryWindow = new ShortcutLibraryForm();
        private bool _screenHasChanged = false; // Used to stop the screen changing when the user is changing profiles

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// When true, allows the form to become visible.
        /// </summary>
        public bool AllowVisible
        {
            get => _allowVisible;
            set
            {
                _allowVisible = value;
                if (value)
                    this.Visible = true;  // triggers SetVisibleCore(true)
                else
                    this.Visible = false;
            }
        }

        /// <summary>
        /// When true, allows the form to become visible.
        /// </summary>
        public bool AllowClose
        {
            get => _allowClose;
            set =>  _allowClose = value;            
        }


        public MainForm(Form formToOpen = null)
        {
            InitializeComponent();
            btn_setup_display_profiles.Parent = splitContainer1.Panel1;
            btn_setup_game_shortcuts.Parent = splitContainer1.Panel2;
            lbl_version.Text = string.Format(lbl_version.Text, Program.AppVersion);

            // Refresh all possible profiles and shortcuts
            ProfileRepository.IsPossibleRefresh();
            ShortcutRepository.IsValidRefresh();

            // Update the system tray menus
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = mainContextMenuStrip;
            RefreshNotifyIconMenus();

            // Shut down the splash screen
            if (Program.AppProgramSettings.ShowSplashScreen && Program.AppSplashScreen != null && !Program.AppSplashScreen.Disposing && !Program.AppSplashScreen.IsDisposed)
                Program.AppSplashScreen.Invoke(new Action(() => Program.AppSplashScreen.Close()));

            if (Program.AppProgramSettings.MinimiseOnStart)
            {
                // Make the form minimised on start 
                //_allowVisible = false;
                // Hide the application to notification area when the form is closed
                _allowClose = false;
                cb_minimise_notification_area.Checked = true;
                // Change the exit_button text to say 'Close'
                btn_exit.Text = "&Close";

                if (Program.AppProgramSettings.ShowMinimiseMessageInActionCenter)
                {
                    // Remind the user that DisplayMagician is running the in background
                    // Construct the toast content
                    ToastContentBuilder tcBuilder = new ToastContentBuilder()
                    .AddText("DisplayMagician is minimised...", hintMaxLines: 1)
                    .AddText("DisplayMagician will wait in the background until you need it.")
                    .AddButton(new ToastButton()
                        .SetContent("Open")
                        .AddArgument("action", "open")
                        .SetBackgroundActivation())
                    .AddButton(new ToastButton()
                        .SetContent("Exit")
                        .AddArgument("action", "exit")
                        .SetBackgroundActivation())
                    .AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true)
                    .SetToastDuration(ToastDuration.Short);
                    ToastContent toastContent = tcBuilder.Content;
                    // Make sure to use Windows.Data.Xml.Dom
                    var doc = new Windows.Data.Xml.Dom.XmlDocument();
                    doc.LoadXml(toastContent.GetContent());

                    // And create the toast notification
                    var toast = new ToastNotification(doc);

                    // Remove any other Notifications from us
                    ToastNotificationManagerCompat.History.Clear();

                    // And then show it
                    ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
                }

            }
            else
            {
                // Make the form show to the user on startup
                //_allowVisible = true;
                // Really close the application when the form is closed
                _allowClose = true;
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
                notifyIcon.Text = $"DisplayMagician ({shortProfileName})";
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
                /*if (!Program.AppProgramSettings.MinimiseOnStart)
                {
                    if (Program.AppMainForm is Form)                     
                    {
                        // Center the MainAppForm
                        Utils.CenterOnPrimaryScreen(Program.AppMainForm);
                    }
                    // Bring the window back to the front            
                    Utils.ActivateCenteredOnPrimaryScreen(Program.AppMainForm);

                }*/
            }

            // Start the donation animation if it's time to do so
            if (Utils.TimeToRunDonationAnimation())
            {
                Utils.AddAnimation(btn_donate);
            }

            // Start the donation animation if it's time to do so
            if (Utils.TimeToShowDonationForm())
            {
                DonationForm donationForm = new DonationForm();
                donationForm.NumberofStarts = Program.AppProgramSettings.NumberOfTimesRun;
                donationForm.ShowDialog(Program.AppMainForm);
                // Update the settings to record the donation has been shown
                Program.AppProgramSettings.LastDonationFormDate = DateTime.UtcNow;
                Program.AppProgramSettings.SaveSettings();
            }


        }

        protected override void SetVisibleCore(bool value)
        {
            if (!_allowVisible)
            {
                value = false;
                if (!this.IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!_allowClose)
            {
                this.Hide();
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            if (cb_minimise_notification_area.Checked && Program.AppProgramSettings.ShowMinimiseMessageInActionCenter)
            {
                // Tell the user that 
                // Construct the toast content
                ToastContentBuilder tcBuilder = new ToastContentBuilder()
                    .AddText("DisplayMagician is minimised...", hintMaxLines: 1)
                    .AddText("DisplayMagician will wait in the background until you need it.")
                    .AddButton(new ToastButton()
                        .SetContent("Open")
                        .AddArgument("action", "open")
                        .SetBackgroundActivation())
                    .AddButton(new ToastButton()
                        .SetContent("Exit")
                        .AddArgument("action", "exit")
                        .SetBackgroundActivation())
                    .AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true)
                    .SetToastDuration(ToastDuration.Short);
                ToastContent toastContent = tcBuilder.Content;
                // Make sure to use Windows.Data.Xml.Dom
                var doc = new Windows.Data.Xml.Dom.XmlDocument();
                doc.LoadXml(toastContent.GetContent());

                // And create the toast notification
                var toast = new ToastNotification(doc);

                // Remove any other Notifications from us
                ToastNotificationManagerCompat.History.Clear();

                // And then show it
                ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
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

            // Check if *any* other modal window is already open
            foreach (Form f in Application.OpenForms)
            {
                if (f.Modal && f.Visible && f != this)
                {
                    // Another modal window is already open!
                    MessageBox.Show($"Please close the {f.Text} window before opening the Display Profiles window.",
                        "DisplayMagician",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            DisplayProfileWindow = new DisplayProfileForm();
            DisplayProfileWindow.StartPosition = FormStartPosition.CenterParent;
            DisplayProfileWindow.ShowDialog(this);

            /*if (DisplayProfileWindow == null || DisplayProfileWindow.IsDisposed)
            {
                DisplayProfileWindow = new DisplayProfileForm();
                DisplayProfileWindow.StartPosition = FormStartPosition.CenterParent;
                DisplayProfileWindow.ShowDialog(this);
            }
            else
            {
                DisplayProfileWindow.Activate();
            }                */
        }

        private void pb_game_shortcut_Click(object sender, EventArgs e)
        {
            btn_setup_game_shortcuts.PerformClick();
        }

        private void btn_setup_game_shortcuts_Click(object sender, EventArgs e)
        {
            logger.Trace($"MainForm/btn_setup_game_shortcuts_Click: User pressed the Game Shortcuts button (or selected the menu item)");

            // Check if *any* other modal window is already open
            foreach (Form f in Application.OpenForms)
            {
                if (f.Modal && f.Visible && f != this)
                {
                    logger.Trace($"MainForm/btn_setup_game_shortcuts_Click: User pressed the Game Shortcuts button (or selected the menu item) but another dialog window was open");
                    // Another modal window is already open!
                    MessageBox.Show($"Please close the {f.Text} window before opening the Shortcut Library window.",
                        "DisplayMagician",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            ShortcutLibraryWindow = new ShortcutLibraryForm();
            ShortcutLibraryWindow.StartPosition = FormStartPosition.CenterParent;
            ShortcutLibraryWindow.ShowDialog(this);

            /*if (ShortcutLibraryWindow == null || ShortcutLibraryWindow.IsDisposed)
            {
                ShortcutLibraryWindow = new ShortcutLibraryForm();
                ShortcutLibraryWindow.StartPosition = FormStartPosition.CenterParent;
                ShortcutLibraryWindow.ShowDialog(this);
            }
            else 
            {
                ShortcutLibraryWindow.Activate();
            }*/
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            EnableShortcutButtonIfProfiles();

            logger.Trace($"MainForm/MainForm_Load: User has run DisplayMagician {Program.AppProgramSettings.NumberOfTimesRun} times.");
            if (Program.AppProgramSettings.NumberOfTimesRun == 1)
            {
                lbl_donate.Text = $"You've used DisplayMagician 1 time.";
            }
            else
            {
                lbl_donate.Text = $"You've used DisplayMagician {Program.AppProgramSettings.NumberOfTimesRun} times.";
            }

            if (Program.AppProgramSettings.NumberOfDonations > 0 && Program.AppProgramSettings.LastDonationDate > DateTime.Parse("2024-01-01"))
            {
                logger.Trace($"MainForm/MainForm_Load: User has donated {Program.AppProgramSettings.NumberOfDonations} times.");
                lbl_donate.Text = $"You've used DisplayMagician {Program.AppProgramSettings.NumberOfTimesRun} times and donated - thank you!";
            }
            else
            {
                if (Program.AppProgramSettings.NumberOfTimesRun > 100)
                {
                    lbl_donate.BackColor = Color.Brown;
                    lbl_donate.Text = $"You've used DisplayMagician {Program.AppProgramSettings.NumberOfTimesRun} times without donating.";
                }
            }
            

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
                    shortcut.RefreshValidity();
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
                    //ProfileRepository.ApplyProfile(profileToRun);
                    Program.ApplyProfileTask(profileToRun);

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
                    //ShortcutRepository.RunShortcut(shortcutToRun, notifyIcon);
                    Program.RunShortcutTask(shortcutToRun);

                // Also refresh the right-click menu (if we have a main form loaded)
                if (Program.AppMainForm is Form)
                {
                    Program.AppMainForm.RefreshNotifyIconMenus();
                }
            }
        }

        public void openApplicationWindow()
        {
            _allowVisible = true;
            // Center the form on the primary screen
            //Utils.ActivateCenteredOnPrimaryScreen(this);
        }

        public void openShortcutLibraryWindow()
        {

            foreach(var form in this.MdiChildren)
            {
                if (form is ShortcutLibraryForm && form.Modal)
                {
                    logger.Trace($"MainForm/openShortcutLibraryWindow: ShortcutLibraryForm is open already so ignoring this");
                    return;
                }
                if (form is DisplayProfileForm && form.Modal)
                {
                    logger.Trace($"MainForm/openShortcutLibraryWindow: DisplayProfileForm is open already so we need to close it.");
                    form.Close();
                    return;
                }
                if (form is SettingsForm && form.Modal)
                {
                    logger.Trace($"MainForm/openShortcutLibraryWindow: SettingsForm is open already so we need to close it");
                    form.Close();
                    return;
                }
            }

            _allowVisible = true;           

            // Center this form on the primary screen
            //Utils.ActivateCenteredOnPrimaryScreen(this);
           
            btn_setup_game_shortcuts.PerformClick();
        }

        public void openDisplayProfileWindow()
        {
            foreach (var form in this.MdiChildren)
            {
                if (form is ShortcutLibraryForm && form.Modal)
                {
                    logger.Trace($"MainForm/openDisplayProfileWindow: ShortcutLibraryForm is open already so we need to close it");
                    form.Close();
                    return;
                }
                if (form is DisplayProfileForm && form.Modal)
                {
                    logger.Trace($"MainForm/openDisplayProfileWindow: DisplayProfileForm is open already so ignoring this");
                    return;
                }
                if (form is SettingsForm && form.Modal)
                {
                    logger.Trace($"MainForm/openDisplayProfileWindow: SettingsForm is open already so we need to close it");
                    form.Close();
                    return;
                }
            }

            _allowVisible = true;

            // Center this form on the primary screen
            //Utils.ActivateCenteredOnPrimaryScreen(this);

            // Now open the ShortcutLibraryWindow
            btn_setup_display_profiles.PerformClick();

        }


        public void exitApplication()
        {
            _allowClose = true;
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
                _allowVisible = false;
                // Hide the application to notification area when the form is closed
                _allowClose = false;
                // Enable the MinimiseOnStart setting
                Program.AppProgramSettings.MinimiseOnStart = true;
                SettingsForm.SetBootMeUp(true);
                // Change the exit_button text to say 'Close'
                btn_exit.Text = "&Close";
            }
            else
            {
                // Make the form show to the user on startup
                _allowVisible = true;
                // Really close the application when the form is closed
                _allowClose = true;
                // Disable the MinimiseOnStart setting
                Program.AppProgramSettings.MinimiseOnStart = false;
                SettingsForm.SetBootMeUp(false);
                // Change the exit_button text to say 'Exit'
                btn_exit.Text = "&Exit";
            }
            // Force a settings save after the settings changed.
            Program.AppProgramSettings.SaveSettings();

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
            string targetURL = "https://github.com/sponsors/terrymacdonald?frequency=one-time";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
            // Update the settings to say that user has donated.
            Utils.UserHasDonated();
            // revert the button back to a nice donated message
            lbl_donate.BackColor = Color.Black;
            lbl_donate.Text = $"You've used DisplayMagician {Program.AppProgramSettings.NumberOfTimesRun} times and donated - Thank you!";
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            openApplicationWindow();
        }

        private void btn_help_Click(object sender, EventArgs e)
        {
            string targetURL = @"https://github.com/terrymacdonald/DisplayMagician/wiki";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
        }

        public void UpdateNotifyIconText(string text)
        {
            if (notifyIcon != null)
            {
                string shortText = text;
                if (shortText.Length >= 64)
                {
                    shortText = text.Substring(0, 45);

                }
                notifyIcon.Text = shortText;
                Application.DoEvents();
            }
        }

        private void btn_fov_calc_Click(object sender, EventArgs e)
        {
            var fovCalcForm = new FovCalcForm();
            fovCalcForm.ShowDialog(this);
        }

        private void lbl_donate_Click(object sender, EventArgs e)
        {
            string targetURL = "https://github.com/sponsors/terrymacdonald?frequency=one-time";
            ProcessUtils.StartProcess(targetURL, "", ProcessPriority.Normal);
            // Update the settings to say that user has donated.
            Utils.UserHasDonated();
        }

        public void RepositionDisplayMagician()
        {
            // Step 1: Check if MainForm is visible on any screen
            bool isMainFormVisible = Screen.AllScreens.Any(screen => screen.WorkingArea.IntersectsWith(this.Bounds));

            if (!isMainFormVisible)
            {
                this.ShowCenteredOnPrimaryScreen();

                /*// Step 2: Reposition MainForm to the center of the primary screen
                Screen primaryScreen = Screen.PrimaryScreen;
                Rectangle workingArea = primaryScreen.WorkingArea;

                int newX = workingArea.Left + (workingArea.Width - this.Width) / 2;
                int newY = workingArea.Top + (workingArea.Height - this.Height) / 2;

                this.StartPosition = FormStartPosition.Manual;
                this.Location = new Point(newX, newY);*/
            }

            //this.Activate();

            // Step 3: Reposition child forms to center on MainForm
            foreach (Form childForm in Application.OpenForms)
            {
                if (childForm != this && childForm.Owner == this)
                {
                    // Calculate the center position relative to MainForm
                    int childX = this.Left + (this.Width - childForm.Width) / 2;
                    int childY = this.Top + (this.Height - childForm.Height) / 2;

                    /*childForm.StartPosition = FormStartPosition.Manual;
                    childForm.Location = new Point(childX, childY);*/
                    childForm.SetDesktopLocation(childX,childY);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DISPLAYCHANGE = 0x007E;
            const int WM_SETTINGCHANGE = 0x001A;
            const int WM_DEVICECHANGE = 0x0219;

            const int DBT_DEVICEARRIVAL = 0x8000;
            const int DBT_DEVICEREMOVECOMPLETE = 0x8004;

            // if the user is changing profiles, then we want to record if anything changed! We change the status of the _screenHasChanged variable if we detect a change
            if (ProfileRepository.UserChangingProfiles)
            {
                switch (m.Msg)
                {

                    case WM_DEVICECHANGE:
                        switch ((int)m.WParam)
                        {
                            case DBT_DEVICEARRIVAL:
                                logger.Trace($"DisplayProfileForm/WndProc: Windows just sent a msg telling us a device has been added. We need to check if this was a USB display. Updating the current view by running btn_view_current.");
                                _screenHasChanged = true;
                                break;

                            case DBT_DEVICEREMOVECOMPLETE:
                                logger.Trace($"DisplayProfileForm/WndProc: Windows just sent a msg telling us a device has been removed. We need to check if this was a USB display. Updating the current view by running btn_view_current.");
                                _screenHasChanged = true;
                                break;
                        }
                        break;

                    case WM_DISPLAYCHANGE:
                        logger.Trace($"DisplayProfileForm/WndProc: Windows just sent a msg telling us the display has changed. Updating the current view by running btn_view_current.");
                        _screenHasChanged = true;
                        break;
                }
            }
            else if (_screenHasChanged)
            {
                // If the user is not changing profiles, then the first time we hit this we need to reposition DM and reset the _screenHasChanged variable
                RepositionDisplayMagician();
                // if the DisplayProfileForm is open, Update the current view by running btn_view_current.
                if (DisplayProfileWindow != null && !DisplayProfileWindow.IsDisposed)
                {
                    DisplayProfileWindow.RefreshCurrentView();
                }

                _screenHasChanged = false;
            }

            base.WndProc(ref m);
        }
    }
}
