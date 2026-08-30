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
using System.ComponentModel;

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
        private MessagesForm MessagesWindow;
        //private Button _btnMessages;
        private bool _screenHasChanged = false; // Used to stop the screen changing when the user is changing profiles

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// When true, allows the form to become visible.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool AllowClose
        {
            get => _allowClose;
            set => _allowClose = value;
        }


        public MainForm(Form formToOpen = null)
        {
            InitializeComponent();
            btn_setup_display_profiles.Parent = splitContainer1.Panel1;
            btn_setup_game_shortcuts.Parent = splitContainer1.Panel2;
            lbl_version.Text = string.Format(lbl_version.Text, Program.AppVersion);

            // Update the message count on the Messages button to reflect any unread messages
            SetUnreadMessageCount(Program.GetUnreadMessageCount());

            // Refresh all possible profiles and shortcuts
            ProfileRepository.IsPossibleRefresh();
            ShortcutRepository.IsValidRefresh();

            // Update the active profile so the UI knows which profile is currently in use
            ProfileRepository.UpdateActiveProfile();

            // Update the system tray menus
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = mainContextMenuStrip;
            RefreshNotifyIconMenus();

            if (Program.AppProgramSettings.MinimiseOnStart)
            {
                // Make the form minimised on start 
                _allowVisible = false;
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
                // Now show the form
                _allowVisible = true;
            }

            if (Program.AppProgramSettings.MinimiseOnStart && Program.AppProgramSettings.StartOnBootUp)
            {
                cb_minimise_notification_area.Checked = true;
            }
            else
            {
                cb_minimise_notification_area.Checked = false;
            }

            //// Set the notifyIcon text with the current profile
            //if (notifyIcon != null)
            //{
            //    string shortProfileName = ProfileRepository.CurrentProfile.Name;
            //    if (shortProfileName.Length >= 64)
            //    {
            //        shortProfileName = ProfileRepository.CurrentProfile.Name.Substring(0, 45);

            //    }
            //    notifyIcon.Text = $"DisplayMagician ({shortProfileName})";
            //    Application.DoEvents();
            //}

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
                donationForm.NumberofStarts = Program.AppDonationSettings.NumberOfTimesRun;
                donationForm.ShowDialog(Program.AppMainForm);
                // Update the settings to record the donation has been shown
                Program.AppDonationSettings.LastDonationFormDate = DateTime.UtcNow;
                Program.AppDonationSettings.SaveSettings();
            }
        }

        private void ResizeDonateLabel()
        {
            if (lbl_donate == null) return;

            // The right edge of the label must stay exactly where it is now.
            int rightEdge = lbl_donate.Right;

            // Measure the text using the label's own device context so DPI (1080p vs 4K) is correct.
            Size textSize;
            using (Graphics g = lbl_donate.CreateGraphics())
            {
                textSize = TextRenderer.MeasureText(
                    g,
                    lbl_donate.Text,
                    lbl_donate.Font,
                    new Size(int.MaxValue, int.MaxValue),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }

            // Move/rescale in one step so the label width matches the text while the right edge stays put.
            int newWidth = textSize.Width + lbl_donate.Margin.Left + lbl_donate.Margin.Right;
            int newLeft = rightEdge - newWidth;
            lbl_donate.SetBounds(newLeft, lbl_donate.Top, newWidth, lbl_donate.Height);
        }

        private void splitContainer1_Panel1_Resize(object sender, EventArgs e)
        {
            ResizeDonateLabel();
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

                if (cb_minimise_notification_area.Checked && Program.AppProgramSettings.ShowMinimiseMessageInActionCenter)
                {
                    // Tell the user that DisplayMagician is still running in the background
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
            base.OnFormClosing(e);
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pb_display_profile_Click(object sender, EventArgs e)
        {
            btn_setup_display_profiles.PerformClick();
        }

        private void btn_audio_Click(object sender, EventArgs e)
        {
            logger.Trace($"MainForm/btn_audio_Click: User pressed the Audio Profiles button (or selected the menu item)");

            // Check if *any* other modal window is already open
            foreach (Form f in Application.OpenForms)
            {
                if (f.Modal && f.Visible && f != this)
                {
                    // Another modal window is already open!
                    MessageBox.Show(this,
                        $"Please close the {f.Text} window before opening the Audio Profiles window.",
                        "DisplayMagician",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            using (AudioProfilesForm audioProfilesForm = new AudioProfilesForm())
            {
                audioProfilesForm.StartPosition = FormStartPosition.CenterParent;
                audioProfilesForm.ShowDialog(this);
            }
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

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            // Shut down the splash screen
            //if (Program.AppProgramSettings.ShowSplashScreen && Program.AppSplashScreen != null && !Program.AppSplashScreen.Disposing && !Program.AppSplashScreen.IsDisposed)
            //    Program.AppSplashScreen.Invoke(new Action(() => Program.AppSplashScreen.Close()));

            EnableShortcutButtonIfProfiles();

            logger.Trace($"MainForm/MainForm_Load: User has run DisplayMagician {Program.AppDonationSettings.NumberOfTimesRun} times.");
            if (Program.AppDonationSettings.NumberOfTimesRun == 1)
            {
                lbl_donate.Text = $"You've used DisplayMagician 1 time.";
                ResizeDonateLabel();
            }
            else
            {
                lbl_donate.Text = $"You've used DisplayMagician {Program.AppDonationSettings.NumberOfTimesRun} times.";
                ResizeDonateLabel();
            }

            if (Program.AppDonationSettings.NumberOfDonations > 0 && Program.AppDonationSettings.LastDonationDate > DateTime.Parse("2024-01-01"))
            {
                logger.Trace($"MainForm/MainForm_Load: User has donated {Program.AppDonationSettings.NumberOfDonations} times.");
                lbl_donate.Text = $"You've used DisplayMagician {Program.AppDonationSettings.NumberOfTimesRun} times and donated - thank you!";
                ResizeDonateLabel();
            }
            else
            {
                if (Program.AppDonationSettings.NumberOfTimesRun > 100)
                {
                    lbl_donate.BackColor = Color.Brown;
                    lbl_donate.Text = $"You've used DisplayMagician {Program.AppDonationSettings.NumberOfTimesRun} times without donating.";
                    ResizeDonateLabel();
                }
            }


            logger.Trace($"MainForm/MainForm_Load: Main Window has loaded.");
            SetUnreadMessageCount(Program.GetUnreadMessageCount());
        }

        public void SetUnreadMessageCount(int unreadCount)
        {
            btn_messages.Text = unreadCount > 0
                ? $"Messages ({unreadCount})"
                : "Messages";
            btn_messages.UseVisualStyleBackColor = false;
            btn_messages.BackColor = unreadCount > 0
                ? btn_messages.FlatAppearance.MouseOverBackColor
                : Color.Black;
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

                // Only apply the profile if it exists and is not already the active profile
                if (profileToRun != null)
                {
                    if (!ProfileRepository.IsActiveProfile(profileToRun))
                    {
                        ApplyProfileResult result = Program.ApplyProfileTask(profileToRun);
                        if (result == ApplyProfileResult.Successful)
                        {
                            logger.Trace($"MainForm/runProfileToolStripMenuItem_Click: Profile {profileToRun.Name} was successfully applied.");
                            UpdateNotifyIconText($"DisplayMagician ({ProfileRepository.CurrentProfile.Name})");
                            ToastContentBuilder tcBuilder = new ToastContentBuilder()
                                .AddText("Display Profile Applied", hintMaxLines: 1)
                                .AddText($"\"{profileToRun.Name}\" has been applied successfully.")
                                .AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true)
                                .SetToastDuration(ToastDuration.Short);
                            ToastContent toastContent = tcBuilder.Content;
                            var doc = new Windows.Data.Xml.Dom.XmlDocument();
                            doc.LoadXml(toastContent.GetContent());
                            var toast = new ToastNotification(doc);
                            ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
                        }
                        else if (result == ApplyProfileResult.Cancelled)
                        {
                            logger.Warn($"MainForm/runProfileToolStripMenuItem_Click: The user cancelled changing to Profile {profileToRun.Name}.");
                        }
                        else
                        {
                            logger.Error($"MainForm/runProfileToolStripMenuItem_Click: Error applying Profile {profileToRun.Name}.");
                        }
                    }
                    else
                    {
                        // Profile is already active - notify the user via toast
                        logger.Trace($"MainForm/runProfileToolStripMenuItem_Click: Profile {profileToRun.Name} is already the active profile.");
                        ToastContentBuilder tcBuilder = new ToastContentBuilder()
                            .AddText("Display Profile Already Active", hintMaxLines: 1)
                            .AddText($"\"{profileToRun.Name}\" is already the current display profile.")
                            .AddAudio(new Uri("ms-winsoundevent:Notification.Default"), false, true)
                            .SetToastDuration(ToastDuration.Short);
                        ToastContent toastContent = tcBuilder.Content;
                        var doc = new Windows.Data.Xml.Dom.XmlDocument();
                        doc.LoadXml(toastContent.GetContent());
                        var toast = new ToastNotification(doc);
                        ToastNotificationManagerCompat.CreateToastNotifier().Show(toast);
                    }
                }

                // Refresh the right-click menu to reflect the new state
                if (Program.AppMainForm is Form)
                {
                    Program.AppMainForm.RefreshNotifyIconMenus();
                }
            }
        }

        private async void runShortcutToolStripMenuItem_Click(object sender, EventArgs e)
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
                    await Program.RunShortcutTaskAsync(shortcutToRun);

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
            Utils.ActivateCenteredOnPrimaryScreen(this);
        }

        public void openShortcutLibraryWindow()
        {

            foreach (var form in this.MdiChildren)
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

        public void openMessagesWindow(bool selectNewestUnread = false)
        {
            foreach (Form f in Application.OpenForms)
            {
                if (f.Modal && f.Visible && f != this)
                {
                    MessageBox.Show($"Please close the {f.Text} window before opening Messages.",
                        "DisplayMagician",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }
            }

            MessagesWindow = new MessagesForm(selectNewestUnread);
            MessagesWindow.StartPosition = FormStartPosition.CenterParent;
            MessagesWindow.ShowDialog(this);
            SetUnreadMessageCount(Program.GetUnreadMessageCount());
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

        private void btn_messages_Click(object sender, EventArgs e)
        {
            openMessagesWindow();
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
            lbl_donate.Text = $"You've used DisplayMagician {Program.AppDonationSettings.NumberOfTimesRun} times and donated - Thank you!";
            ResizeDonateLabel();
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            switch (Program.AppProgramSettings.NotifyIconDoubleClickAction)
            {
                case NotifyIconDoubleClickAction.DoNothing:
                    break;
                case NotifyIconDoubleClickAction.DisplayProfileForm:
                    openApplicationWindow();
                    openDisplayProfileWindow();
                    break;
                case NotifyIconDoubleClickAction.MainForm:
                    openApplicationWindow();
                    break;
                default:
                    // Default is ShortcutLibraryForm
                    openApplicationWindow();
                    openShortcutLibraryWindow();
                    break;
            }
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
                    childForm.SetDesktopLocation(childX, childY);
                }
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_DISPLAYCHANGE = 0x007E;

            // If the user is changing profiles, record if a display change occurred so we can react after they finish
            if (ProfileRepository.UserChangingProfiles)
            {
                if (m.Msg == WM_DISPLAYCHANGE)
                {
                    logger.Trace($"MainForm/WndProc: Display changed while user was changing profiles. Will update view afterwards.");
                    _screenHasChanged = true;
                }
            }
            else if (m.Msg == WM_DISPLAYCHANGE)
            {
                // Display changed while idle - flag it for handling on the next message
                logger.Trace($"MainForm/WndProc: Windows sent WM_DISPLAYCHANGE while idle. Flagging screen as changed.");
                _screenHasChanged = true;
            }
            else if (_screenHasChanged)
            {
                // Display change has settled - update all state and UI
                logger.Trace($"MainForm/WndProc: Handling display change - refreshing profile state and menus.");
                _screenHasChanged = false;

                RepositionDisplayMagician();

                ProfileRepository.IsPossibleRefresh();
                ProfileRepository.UpdateActiveProfile();
                RefreshNotifyIconMenus();

                // If the DisplayProfileForm is open, refresh its view too
                if (DisplayProfileWindow != null && !DisplayProfileWindow.IsDisposed)
                {
                    DisplayProfileWindow.RefreshCurrentView();
                }
            }

            base.WndProc(ref m);
        }

    }
}
