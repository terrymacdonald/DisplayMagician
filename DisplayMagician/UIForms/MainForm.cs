using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.GameLibraries;
using System.Threading;
using System.Reflection;
using DisplayMagician.Shared;
using System.Runtime.InteropServices;
using System.IO;
using AutoUpdaterDotNET;
using Newtonsoft.Json;
using System.Net;

namespace DisplayMagician.UIForms
{
    public partial class MainForm : Form
    {

        private bool allowVisible;     // ContextMenu's Show command used
        private bool allowClose;       // ContextMenu's Exit command used

        public MainForm(Form formToOpen = null)
        {
            InitializeComponent();
            btn_setup_display_profiles.Parent = splitContainer1.Panel1;
            btn_setup_game_shortcuts.Parent = splitContainer1.Panel2;
            lbl_version.Text = string.Format(lbl_version.Text, Assembly.GetExecutingAssembly().GetName().Version);
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = mainContextMenuStrip;
            RefreshNotifyIconMenus();

            /*WaitingForm testform = new WaitingForm();
            testform.Owner = this;
            testform.Show();*/

            if (Program.AppProgramSettings.MinimiseOnStart) 
            {
                // Make the form minimised on start 
                allowVisible = false;
                // Hide the application to notification area when the form is closed
                allowClose = false;
                cb_minimise_notification_area.Checked = true;
            }
            else
            {
                // Make the form show to the user on startup
                allowVisible = true;
                // Really close the application when the form is closed
                allowClose = true;
                cb_minimise_notification_area.Checked = false;
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
            Application.Exit();
        }

        private void pb_display_profile_Click(object sender, EventArgs e)
        {
            var displayProfileForm = new DisplayProfileForm();
            displayProfileForm.ShowDialog(this);
        }

        private void btn_setup_display_profiles_Click(object sender, EventArgs e)
        {
            var displayProfileForm = new DisplayProfileForm();
            displayProfileForm.ShowDialog(this);
        }

        private void pb_game_shortcut_Click(object sender, EventArgs e)
        {
            var shortcutLibraryForm = new ShortcutLibraryForm();
            shortcutLibraryForm.ShowDialog(this);
        }

        private void btn_setup_game_shortcuts_Click(object sender, EventArgs e)
        {
            var shortcutLibraryForm = new ShortcutLibraryForm();
            shortcutLibraryForm.ShowDialog(this);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Start loading the Steam Games just after the Main form opens
            //SteamGame.GetAllInstalledGames();
            EnableShortcutButtonIfProfiles();

            //Run the AutoUpdater to see if there are any updates available.
            AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;
            AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;
            AutoUpdater.Start("http://displaymagician.littlebitbig.com/update/");
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
            }

        }


        private void RefreshNotifyIconMenus()
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

            // Add the current slist of profiles into the NotifyIcon context menu
            foreach (ProfileItem profile in ProfileRepository.AllProfiles)
            {
                profileToolStripMenuItem.DropDownItems.Add(profile.Name,profile.ProfileBitmap, runProfileToolStripMenuItem_Click);
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
            // Add the current list of profiles into the NotifyIcon context menu
            foreach (ShortcutItem shortcut in ShortcutRepository.AllShortcuts)
            {
                shortcutToolStripMenuItem.DropDownItems.Add(shortcut.Name,shortcut.ShortcutBitmap, runShortcutToolStripMenuItem_Click);
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

                // Run the shortcut if it's still there
                if (profileToRun != null)
                    Program.ApplyProfile(profileToRun);
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
            }
        }

        private void openApplicationWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowVisible = true;
            Restore();
            Show();
            BringToFront();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            allowClose = true;
            Application.Exit();
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
            }
            else
            {
                // Make the form show to the user on startup
                allowVisible = true;
                // Really close the application when the form is closed
                allowClose = true;
                // Disable the MinimiseOnStart setting
                Program.AppProgramSettings.MinimiseOnStart = false;
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

        private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
        {
            dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
            args.UpdateInfo = new UpdateInfoEventArgs
            {
                CurrentVersion = json.version,
                ChangelogURL = json.changelog,
                DownloadURL = json.url,
                Mandatory = new Mandatory
                {
                    Value = json.mandatory.value,
                    UpdateMode = json.mandatory.mode,
                    MinimumVersion = json.mandatory.minVersion
                },
                CheckSum = new CheckSum
                {
                    Value = json.checksum.value,
                    HashingAlgorithm = json.checksum.hashingAlgorithm
                }
            };
        }        

        private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
        {
            if (args.Error == null)
            {
                if (args.IsUpdateAvailable)
                {
                    DialogResult dialogResult;
                    if (args.Mandatory.Value)
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. This is required update. Press Ok to begin updating the application.", @"Update Available",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                    }
                    else
                    {
                        dialogResult =
                            MessageBox.Show(
                                $@"There is new version {args.CurrentVersion} available. You are using version {
                                        args.InstalledVersion
                                    }. Do you want to update the application now?", @"Update Available",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Information);
                    }

                    // Uncomment the following line if you want to show standard update dialog instead.
                    // AutoUpdater.ShowUpdateForm(args);

                    if (dialogResult.Equals(DialogResult.Yes) || dialogResult.Equals(DialogResult.OK))
                    {
                        try
                        {
                            if (AutoUpdater.DownloadUpdate(args))
                            {
                                Application.Exit();
                            }
                        }
                        catch (Exception exception)
                        {
                            MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
                        }
                    }
                }
                /*else
                {
                    MessageBox.Show(@"There is no update available please try again later.", @"No update available",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }*/
            }
            else
            {
                if (args.Error is WebException)
                {
                    MessageBox.Show(
                        @"There is a problem reaching update server. Please check your internet connection and try again later.",
                        @"Update Check Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(args.Error.Message,
                        args.Error.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
    }
}
