using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using Manina.Windows.Forms;

namespace DisplayMagician.UIForms
{
    internal partial class DisplayProfileForm : DisplayMagicianForm
    {
        protected override bool FitToWorkingAreaAfterDpiChange => true;

        private readonly ControlServicePipeClient _controlServiceClient = new ControlServicePipeClient();
        private readonly ProfileAdaptor _profileAdaptor = new ProfileAdaptor();
        private readonly CancellationTokenSource _initialLoadCancellationTokenSource = new CancellationTokenSource();
        private DisplayProfileView _selectedProfile;
        private DisplayProfileView _currentLayout;
        private bool _initialLoadStarted;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public DisplayProfileForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            AcceptButton = btn_save_or_rename;
            ilv_saved_profiles.MultiSelect = false;
            ilv_saved_profiles.AllowDrag = false;
            ilv_saved_profiles.AllowDrop = false;
            ilv_saved_profiles.SetRenderer(new ProfileILVRenderer());
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _initialLoadCancellationTokenSource.Cancel();
            base.OnFormClosing(e);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!_initialLoadStarted)
            {
                _initialLoadStarted = true;
                _ = InitialiseDisplayProfileAsync(_initialLoadCancellationTokenSource.Token);
            }
        }

        private void DisplayProfileForm_Load(object sender, EventArgs e)
        {
            SetProfileActionsEnabled(false);
            lbl_profile_shown.Text = "Loading display profile...";
            lbl_profile_shown_subtitle.Text = "Checking current display configuration...";
            lbl_profile_shown_subtitle.Visible = true;
        }

        private async Task InitialiseDisplayProfileAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!Program.EnsureUserAgentStarted()) throw new InvalidOperationException("DisplayMagician could not start the User Agent required to load display profiles.");
                await RefreshProfilesAsync(null, cancellationToken);
                if (Utils.TimeToRunDonationAnimation()) Utils.AddAnimation(btn_donate);
            }
            catch (OperationCanceledException)
            {
                logger.Trace("DisplayProfileForm/InitialiseDisplayProfileAsync: Initial form loading was cancelled because the form closed.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DisplayProfileForm/InitialiseDisplayProfileAsync: Failed to load the current display configuration.");
                MessageBox.Show(this, "DisplayMagician could not read the current display configuration. You can close this window and try again.", "Display Profile Window Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { if (!IsDisposed && IsHandleCreated) SetProfileActionsEnabled(true); }
        }

        private async Task RefreshProfilesAsync(string selectedProfileId, CancellationToken cancellationToken)
        {
            DisplayMagician.Contracts.ProfileListResult profiles = await _controlServiceClient.ListProfilesAsync(cancellationToken);
            _currentLayout = profiles.CurrentLayout;
            DisplayProfileView selected = !string.IsNullOrWhiteSpace(selectedProfileId)
                ? profiles.Views.FirstOrDefault(profile => string.Equals(profile.Id, selectedProfileId, StringComparison.OrdinalIgnoreCase))
                : profiles.Views.FirstOrDefault(profile => profile.IsActive);
            ChangeSelectedProfile(selected ?? _currentLayout ?? profiles.Views.FirstOrDefault());
            ilv_saved_profiles.SuspendLayout();
            ilv_saved_profiles.Items.Clear();
            foreach (DisplayProfileView profile in profiles.Views.OrderBy(profile => profile.Name))
            {
                ImageListViewItem item = new ImageListViewItem(profile, profile.Name) { Selected = profile.Id == _selectedProfile?.Id };
                ilv_saved_profiles.Items.Add(item, _profileAdaptor);
            }
            ilv_saved_profiles.ResumeLayout();
        }

        private void ChangeSelectedProfile(DisplayProfileView profile)
        {
            if (profile == null) return;
            _selectedProfile = profile;
            lbl_profile_shown.Text = profile.Name;
            txt_profile_save_name.Text = profile.Name;
            RenderProfileThumbnail(profile.ThumbnailPngBase64);
            bool hasDiagnostic = !string.IsNullOrWhiteSpace(profile.DiagnosticMessage);
            p_profile_advisory.Visible = profile.IsSaved && (!profile.IsValid || hasDiagnostic);
            if (p_profile_advisory.Visible)
            {
                p_profile_advisory.BackColor = profile.IsValid ? Color.FromArgb(255, 193, 7) : Color.Firebrick;
                lbl_profile_advisory.ForeColor = profile.IsValid ? Color.Black : Color.White;
                lbl_profile_advisory_title.ForeColor = lbl_profile_advisory.ForeColor;
                lbl_profile_advisory_title.Text = profile.IsValid ? "Your display profile may not apply as expected." : "This display profile contains errors and cannot be applied.";
                lbl_profile_advisory.Text = profile.DiagnosticMessage;
            }
            bool saved = profile.IsSaved;
            btn_save_or_rename.Text = saved ? "Rename To" : "Save";
            lbl_save_profile.Visible = !saved;
            btn_update.Visible = saved;
            btn_delete.Enabled = saved;
            btn_profile_settings.Enabled = saved;
            btn_apply.Visible = saved && profile.IsValid && !profile.IsActive;
            applyToolStripMenuItem.Enabled = btn_apply.Visible;
            lbl_profile_shown_subtitle.Visible = !saved || profile.IsActive || !profile.IsValid;
            lbl_profile_shown_subtitle.Text = !saved ? "The current Display configuration has not been saved as a Display Profile yet." : !profile.IsValid ? "This Display Profile contains errors." : profile.IsActive ? "This is the Display Profile currently in use." : string.Empty;
            UpdateHotkeyText();
        }

        private void RenderProfileThumbnail(string thumbnailPngBase64)
        {
            Image previous = pb_profile_layout.Image;
            pb_profile_layout.Image = null;
            previous?.Dispose();
            if (string.IsNullOrWhiteSpace(thumbnailPngBase64)) return;
            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(thumbnailPngBase64)))
            using (Image image = Image.FromStream(stream)) pb_profile_layout.Image = new Bitmap(image);
        }

        private void SetProfileActionsEnabled(bool enabled)
        {
            ilv_saved_profiles.Enabled = enabled;
            btn_view_current.Enabled = enabled;
            btn_save_or_rename.Enabled = enabled;
            btn_update.Enabled = enabled;
            btn_delete.Enabled = enabled;
            btn_apply.Enabled = enabled && btn_apply.Visible;
            btn_save.Enabled = enabled;
            btn_hotkey.Enabled = enabled;
            btn_profile_settings.Enabled = enabled;
            applyToolStripMenuItem.Enabled = enabled && btn_apply.Visible;
        }

        private async void Apply_Click(object sender, EventArgs e)
        {
            if (_selectedProfile?.IsSaved != true || !_selectedProfile.IsValid) return;
            ControlResponse response = await _controlServiceClient.ApplyProfileWhenAgentAvailableAsync(_selectedProfile.Id, CancellationToken.None);
            if (!response.IsSuccessful) MessageBox.Show(this, response.Message, "Apply Profile", MessageBoxButtons.OK, MessageBoxIcon.Error); else await RefreshProfilesAsync(_selectedProfile.Id, CancellationToken.None);
        }

        private async void btn_save_as_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txt_profile_save_name.Text) || !Program.IsValidFilename(txt_profile_save_name.Text)) { MessageBox.Show(this, "Please provide a valid display profile name.", "Invalid Display Profile Name", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            ControlResponse response = _selectedProfile?.IsSaved == true
                ? await _controlServiceClient.RenameProfileAsync(_selectedProfile.Id, txt_profile_save_name.Text, CancellationToken.None)
                : await _controlServiceClient.CreateProfileFromCurrentAsync(txt_profile_save_name.Text, CancellationToken.None);
            if (!response.IsSuccessful) { MessageBox.Show(this, response.Message, "Save Display Profile", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            await RefreshProfilesAsync(_selectedProfile?.IsSaved == true ? _selectedProfile.Id : null, CancellationToken.None);
        }

        private async void Delete_Click(object sender, EventArgs e)
        {
            if (_selectedProfile?.IsSaved != true || MessageBox.Show(this, $"Are you sure you want to delete the '{_selectedProfile.Name}' Display Profile? This cannot be undone.", "Delete Display Profile", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            string deletedProfileId = _selectedProfile.Id;
            ControlResponse response = await _controlServiceClient.DeleteProfileAsync(deletedProfileId, CancellationToken.None);
            if (!response.IsSuccessful) { MessageBox.Show(this, response.Message, "Delete Display Profile", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            Program.AppDirectInputManager.RemoveHotkeysByUUID(deletedProfileId);
            await RefreshProfilesAsync(null, CancellationToken.None);
        }

        private async void btn_view_current_Click(object sender, EventArgs e) { await RefreshProfilesAsync(null, CancellationToken.None); ChangeSelectedProfile(_currentLayout); }
        public void RefreshCurrentView() => btn_view_current.PerformClick();

        private void ilv_saved_profiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.Item.VirtualItemKey is DisplayProfileView profile) ChangeSelectedProfile(profile);
            if (e.Buttons == MouseButtons.Right) cms_profiles.Show(ilv_saved_profiles, e.Location);
        }

        private void ilv_saved_profiles_ItemDoubleClick(object sender, ItemClickEventArgs e) { if (e.Item.VirtualItemKey is DisplayProfileView profile) { ChangeSelectedProfile(profile); btn_apply.PerformClick(); } }
        private void ilv_saved_profiles_ItemHover(object sender, ItemHoverEventArgs e) { if (e.Item != null) tt_selected.SetToolTip(ilv_saved_profiles, e.Item.Text); else tt_selected.RemoveAll(); }
        private void txt_profile_save_name_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) btn_save_or_rename.PerformClick(); }

        private async void btn_update_Click(object sender, EventArgs e)
        {
            if (_selectedProfile?.IsSaved != true || MessageBox.Show(this, $"Do you really want to overwrite the display settings in the '{_selectedProfile.Name}' Display Profile with the display settings currently in use? This cannot be undone.", "Update Display Profile settings?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
            ControlResponse response = await _controlServiceClient.UpdateProfileFromCurrentAsync(_selectedProfile.Id, CancellationToken.None);
            if (!response.IsSuccessful) { MessageBox.Show(this, response.Message, "Update Display Profile", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            await RefreshProfilesAsync(_selectedProfile.Id, CancellationToken.None);
        }

        private async void btn_profile_settings_Click(object sender, EventArgs e)
        {
            if (_selectedProfile?.IsSaved != true) return;
            DisplayProfileSettings settings = new DisplayProfileSettings
            {
                ApplyWallpaper = _selectedProfile.Settings.ApplyWallpaper,
                BackgroundDescription = _selectedProfile.Settings.BackgroundDescription,
                ApplyProfileCount = _selectedProfile.Settings.ApplyProfileCount,
                ApplyProfileDelay = _selectedProfile.Settings.ApplyProfileDelay,
                ForceExplorerRestart = _selectedProfile.Settings.ForceExplorerRestart
            };
            using (ProfileSettingsForm form = new ProfileSettingsForm { Settings = settings })
            {
                form.ShowDialog(this);
                if (!form.ProfileSettingChanged) return;
                ControlResponse response = await _controlServiceClient.UpdateDisplayProfileSettingsAsync(_selectedProfile.Id, form.Settings, CancellationToken.None);
                if (!response.IsSuccessful) { MessageBox.Show(this, response.Message, "Profile Settings", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
            }
            await RefreshProfilesAsync(_selectedProfile.Id, CancellationToken.None);
        }

        private void UpdateHotkeyText()
        {
            if (_selectedProfile?.IsSaved != true) { lbl_hotkey_assigned.Visible = false; return; }
            List<string> keyboard = Program.AppProgramSettings.KeyboardHotkeys.Where(hotkey => hotkey.Task == HotkeyTask.ChangeDisplayProfile && hotkey.UUID == _selectedProfile.Id).Select(Program.AppDirectInputManager.GetNameOfKeyboardHotkey).ToList();
            List<string> joystick = Program.AppProgramSettings.JoystickHotkeys.Where(hotkey => hotkey.Task == HotkeyTask.ChangeDisplayProfile && hotkey.UUID == _selectedProfile.Id).Select(Program.AppDirectInputManager.GetNameOfJoystickHotkey).ToList();
            List<string> hotkeys = keyboard.Count > 0 ? keyboard : joystick;
            lbl_hotkey_assigned.Text = $"Hotkeys: {string.Join(", ", hotkeys)}";
            lbl_hotkey_assigned.Visible = hotkeys.Count > 0;
        }

        private void btn_hotkey_Click(object sender, EventArgs e)
        {
            if (_selectedProfile?.IsSaved != true) return;
            using (HotkeyForm form = new HotkeyForm(HotkeyTask.ChangeDisplayProfile, _selectedProfile.Id, $"Manage your '{_selectedProfile.Name}' Display Profile Hotkeys", "Choose one or more Hotkeys to apply this Display Profile.")) { form.ShowDialog(this); if (form.Changed) UpdateHotkeyText(); }
        }

        private void lbl_hotkey_assigned_Click(object sender, EventArgs e) => btn_hotkey.PerformClick();
        private void Exit_Click(object sender, EventArgs e) => Close();
        private void btn_help_Click(object sender, EventArgs e) => DesktopShellUtilities.OpenUrl("https://github.com/terrymacdonald/DisplayMagician/wiki/Initial-DisplayMagician-Setup");
        private void btn_donate_Click(object sender, EventArgs e) { DesktopShellUtilities.OpenUrl("https://github.com/sponsors/terrymacdonald?frequency=one-time"); Utils.UserHasDonated(); }
        private void applyToolStripMenuItem_Click(object sender, EventArgs e) => btn_apply.PerformClick();
        private void deleteProfileToolStripMenuItem_Click(object sender, EventArgs e) => btn_delete.PerformClick();
        private void saveProfileToDesktopToolStripMenuItem_Click(object sender, EventArgs e) => Save_Click(sender, e);
        private void Save_Click(object sender, EventArgs e) => MessageBox.Show(this, "Creating desktop shortcuts for display profiles is not available while profile execution is owned by the User Agent.", "Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Information);
        private void sendToClipboardToolStripMenuItem_Click(object sender, EventArgs e) { if (_selectedProfile?.IsSaved == true) Clipboard.SetText(_selectedProfile.Id); }

        private void ResizeProfileAdvisoryPanel()
        {
            if (p_profile_advisory == null || !p_profile_advisory.Visible || string.IsNullOrWhiteSpace(lbl_profile_advisory.Text)) return;
            int availableWidth = Math.Max(1, p_profile_advisory.ClientSize.Width - lbl_profile_advisory.Padding.Horizontal);
            Size titleSize = TextRenderer.MeasureText(lbl_profile_advisory_title.Text, lbl_profile_advisory_title.Font, new Size(availableWidth, int.MaxValue), TextFormatFlags.WordBreak);
            Size messageSize = TextRenderer.MeasureText(lbl_profile_advisory.Text, lbl_profile_advisory.Font, new Size(availableWidth, int.MaxValue), TextFormatFlags.WordBreak);
            lbl_profile_advisory_title.Height = Math.Max(28, titleSize.Height + lbl_profile_advisory_title.Padding.Vertical);
            p_profile_advisory.Height = Math.Max(80, lbl_profile_advisory_title.Height + messageSize.Height + lbl_profile_advisory.Padding.Vertical);
        }
    }
}
