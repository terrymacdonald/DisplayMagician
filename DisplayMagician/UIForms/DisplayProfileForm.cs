using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DisplayMagician.Contracts;
using IconLib;
using Manina.Windows.Forms;

namespace DisplayMagician.UIForms
{
    internal partial class DisplayProfileForm : DisplayMagicianForm
    {
        protected override bool FitToWorkingAreaAfterDpiChange => true;

        private readonly ControlServicePipeClient _controlServiceClient;
        private readonly Action<string, string> _showError;
        private readonly CancellationToken _lifetimeToken;
        private int _profileRefreshVersion;
        private bool _commandInProgress;
        private bool _lifetimeDisposed;
        private readonly ProfileAdaptor _profileAdaptor = new ProfileAdaptor();
        private readonly CancellationTokenSource _initialLoadCancellationTokenSource = new CancellationTokenSource();
        private DisplayProfileView _selectedProfile;
        private DisplayProfileView _currentLayout;
        private bool _initialLoadStarted;
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public DisplayProfileForm() : this(new ControlServicePipeClient()) { }

        internal DisplayProfileForm(ControlServicePipeClient client, Action<string, string> showError = null)
        {
            _controlServiceClient = client ?? throw new ArgumentNullException(nameof(client));
            _showError = showError ?? ((message, title) => MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Error));
            _lifetimeToken = _initialLoadCancellationTokenSource.Token;
            InitializeComponent();
            Disposed += (_, _) =>
            {
                if (_lifetimeDisposed) return;
                _lifetimeDisposed = true;
                _initialLoadCancellationTokenSource.Cancel();
                _initialLoadCancellationTokenSource.Dispose();
                pb_profile_layout.Image?.Dispose();
            };
            DoubleBuffered = true;
            AcceptButton = btn_save_or_rename;
            ilv_saved_profiles.MultiSelect = false;
            ilv_saved_profiles.AllowDrag = false;
            ilv_saved_profiles.AllowDrop = false;
            ilv_saved_profiles.SetRenderer(new ProfileILVRenderer());
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!e.Cancel && !_lifetimeDisposed) _initialLoadCancellationTokenSource.Cancel();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (!_initialLoadStarted)
            {
                _initialLoadStarted = true;
                _ = InitialiseDisplayProfileAsync(_lifetimeToken);
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
                await RefreshProfilesAsync(null, cancellationToken);
                if (!IsDisposed && !_lifetimeToken.IsCancellationRequested && Utils.TimeToRunDonationAnimation()) Utils.AddAnimation(btn_donate);
            }
            catch (OperationCanceledException)
            {
                logger.Trace("DisplayProfileForm/InitialiseDisplayProfileAsync: Initial form loading was cancelled because the form closed.");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "DisplayProfileForm/InitialiseDisplayProfileAsync: Failed to load the current display configuration.");
                if (!IsDisposed && !_lifetimeToken.IsCancellationRequested) _showError("DisplayMagician could not read the current display configuration. You can close this window and try again.", "Display Profile Window Load Error");
            }
            finally { if (!IsDisposed && !_lifetimeToken.IsCancellationRequested && IsHandleCreated) SetProfileActionsEnabled(true); }
        }

        private async Task RefreshProfilesAsync(string selectedProfileId, CancellationToken cancellationToken, bool showCurrentLayout = false)
        {
            if (IsDisposed || _lifetimeToken.IsCancellationRequested) return;
            int refreshVersion = ++_profileRefreshVersion;
            DisplayMagician.Contracts.ProfileListResult profiles = await _controlServiceClient.ListProfilesAsync(cancellationToken,
                detailedProfileId: selectedProfileId, includeActiveProfileDetail: !showCurrentLayout && string.IsNullOrWhiteSpace(selectedProfileId),
                includeCurrentLayoutDetail: showCurrentLayout);
            if (IsDisposed || Disposing || _lifetimeToken.IsCancellationRequested || refreshVersion != _profileRefreshVersion) return;
            _currentLayout = profiles.CurrentLayout;
            DisplayProfileView selected = showCurrentLayout ? _currentLayout : !string.IsNullOrWhiteSpace(selectedProfileId)
                ? profiles.SavedProfiles.FirstOrDefault(profile => string.Equals(profile.Id, selectedProfileId, StringComparison.OrdinalIgnoreCase))
                : profiles.SavedProfiles.FirstOrDefault(profile => profile.IsActive);
            ChangeSelectedProfile(selected ?? _currentLayout ?? profiles.SavedProfiles.FirstOrDefault());
            ilv_saved_profiles.SuspendLayout();
            ilv_saved_profiles.Items.Clear();
            foreach (DisplayProfileView profile in profiles.SavedProfiles.OrderBy(profile => profile.Name))
            {
                ImageListViewItem item = new ImageListViewItem(profile, profile.Name) { Selected = profile.Id == _selectedProfile?.Id };
                ilv_saved_profiles.Items.Add(item, _profileAdaptor);
            }
            ilv_saved_profiles.ResumeLayout();
        }

        private void ChangeSelectedProfile(DisplayProfileView profile)
        {
            _selectedProfile = profile;
            if (profile == null)
            {
                lbl_profile_shown.Text = "No display profile is available.";
                txt_profile_save_name.Clear();
                RenderProfileThumbnail(null);
                p_profile_advisory.Visible = false;
                lbl_profile_shown_subtitle.Visible = false;
                lbl_hotkey_assigned.Visible = false;
                btn_apply.Visible = false;
                btn_update.Visible = false;
                SetProfileActionsEnabled(true);
                return;
            }
            lbl_profile_shown.Text = profile.Name;
            txt_profile_save_name.Text = profile.Name;
            RenderProfileThumbnail(profile.DetailedLayoutPngBase64 ?? profile.ThumbnailPngBase64);
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
            SetProfileActionsEnabled(true);
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
            enabled = enabled && !_commandInProgress;
            bool saved = _selectedProfile?.IsSaved == true;
            ilv_saved_profiles.Enabled = enabled;
            btn_view_current.Enabled = enabled;
            btn_save_or_rename.Enabled = enabled && _selectedProfile != null;
            btn_update.Enabled = enabled && saved;
            btn_delete.Enabled = enabled && saved;
            btn_apply.Enabled = enabled && saved && _selectedProfile.IsValid && !_selectedProfile.IsActive;
            btn_save.Enabled = enabled && saved;
            btn_hotkey.Enabled = enabled && saved;
            btn_profile_settings.Enabled = enabled && saved;
            applyToolStripMenuItem.Enabled = btn_apply.Enabled;
        }

        private async Task RunProfileActionAsync(Func<Task> action)
        {
            if (_commandInProgress || IsDisposed || _lifetimeToken.IsCancellationRequested) return;
            _commandInProgress = true;
            SetProfileActionsEnabled(false);
            try { await action(); }
            catch (OperationCanceledException) when (_lifetimeToken.IsCancellationRequested) { /* The form closed while waiting. */ }
            catch (Exception ex)
            {
                logger.Error(ex, "DisplayProfileForm/RunProfileActionAsync: Could not complete the requested profile action.");
                if (!IsDisposed && !_lifetimeToken.IsCancellationRequested) _showError("DisplayMagician could not complete the profile action through the Control Service. Please try again.", "Display Profile");
            }
            finally
            {
                _commandInProgress = false;
                if (!IsDisposed && !_lifetimeToken.IsCancellationRequested) SetProfileActionsEnabled(true);
            }
        }

        private async void Apply_Click(object sender, EventArgs e)
        {
            await RunProfileActionAsync(async () =>
            {
                DisplayProfileView selected = _selectedProfile;
                if (selected?.IsSaved != true || !selected.IsValid) return;
                ControlResponse response = await _controlServiceClient.ApplyProfileWhenAgentAvailableAsync(selected.Id, _lifetimeToken);
                if (IsDisposed || _lifetimeToken.IsCancellationRequested) return;
                if (!response.IsSuccessful) _showError(response.Message, "Apply Profile"); else await RefreshProfilesAsync(selected.Id, _lifetimeToken);
            });
        }

        private async void btn_save_as_Click(object sender, EventArgs e)
        {
            await RunProfileActionAsync(async () =>
            {
                if (string.IsNullOrWhiteSpace(txt_profile_save_name.Text) || !Program.IsValidFilename(txt_profile_save_name.Text)) { MessageBox.Show(this, "Please provide a valid display profile name.", "Invalid Display Profile Name", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                DisplayProfileView selected = _selectedProfile;
                string profileName = txt_profile_save_name.Text;
                ControlResponse response = selected?.IsSaved == true
                    ? await _controlServiceClient.RenameProfileAsync(selected.Id, profileName, _lifetimeToken)
                    : await _controlServiceClient.CreateProfileFromCurrentAsync(profileName, _lifetimeToken);
                if (IsDisposed || _lifetimeToken.IsCancellationRequested) return;
                if (!response.IsSuccessful) { _showError(response.Message, "Save Display Profile"); return; }
                await RefreshProfilesAsync(selected?.IsSaved == true ? selected.Id : null, _lifetimeToken);
                if (IsDisposed || _lifetimeToken.IsCancellationRequested) return;
                RefreshInstalledProfileContextMenu();
            });
        }

        private async void Delete_Click(object sender, EventArgs e)
        {
            await RunProfileActionAsync(async () =>
            {
                if (_selectedProfile?.IsSaved != true || MessageBox.Show(this, $"Are you sure you want to delete the '{_selectedProfile.Name}' Display Profile? This cannot be undone.", "Delete Display Profile", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                string deletedProfileId = _selectedProfile.Id;
                ControlResponse response = await _controlServiceClient.DeleteProfileAsync(deletedProfileId, _lifetimeToken);
                if (IsDisposed || _lifetimeToken.IsCancellationRequested) return;
                if (!response.IsSuccessful) { _showError(response.Message, "Delete Display Profile"); return; }
                bool iconCleanupFailed = false;
                if (Guid.TryParse(deletedProfileId, out _))
                {
                    string iconPath = Path.Combine(Program.AppIconPath, "ProfileCache", $"{deletedProfileId}.ico");
                    try
                    {
                        // The desktop shortcut icon is created by this client, not the User Agent.
                        // Missing files are expected when the profile was never saved to the desktop.
                        File.Delete(iconPath);
                    }
                    catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is NotSupportedException)
                    {
                        iconCleanupFailed = true;
                        logger.Warn(ex, "DisplayProfileForm/Delete_Click: Deleted profile {0}, but could not remove its desktop shortcut icon at {1}.", deletedProfileId, iconPath);
                    }
                }
                else
                {
                    iconCleanupFailed = true;
                    logger.Warn("DisplayProfileForm/Delete_Click: Deleted profile {0}, but its ID was not valid for desktop shortcut icon cleanup.", deletedProfileId);
                }
                Program.AppDirectInputManager.RemoveHotkeysByUUID(deletedProfileId);
                await RefreshProfilesAsync(null, _lifetimeToken);
                if (IsDisposed || _lifetimeToken.IsCancellationRequested) return;
                RefreshInstalledProfileContextMenu();
                if (iconCleanupFailed)
                    MessageBox.Show(this, "The display profile was deleted, but its cached desktop shortcut icon could not be removed.", "Display Profile Deleted", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            });
        }

        private async void btn_view_current_Click(object sender, EventArgs e) { await RunProfileActionAsync(() => RefreshProfilesAsync(null, _lifetimeToken, showCurrentLayout: true)); }
        public void RefreshCurrentView()
        {
            if (IsDisposed || _lifetimeToken.IsCancellationRequested || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                try { BeginInvoke((MethodInvoker)RefreshCurrentView); }
                catch (InvalidOperationException) { /* The window closed before the refresh could be queued. */ }
                return;
            }
            _ = RunProfileActionAsync(() => RefreshProfilesAsync(null, _lifetimeToken, showCurrentLayout: true));
        }

        private void RefreshInstalledProfileContextMenu()
        {
            if (Program.AppProgramSettings.InstallDesktopContextMenu && !DisplayMagician.ContextMenu.UpdateProfileContextMenu())
            {
                logger.Warn("DisplayProfileForm/RefreshInstalledProfileContextMenu: Failed to refresh the desktop profile context menu.");
            }
        }

        private async void ilv_saved_profiles_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (_commandInProgress || IsDisposed || _lifetimeToken.IsCancellationRequested) return;
            int refreshVersion = _profileRefreshVersion + 1;
            if (e.Item.VirtualItemKey is DisplayProfileView profile)
            {
                try
                {
                    await RefreshProfilesAsync(profile.Id, _lifetimeToken);
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "DisplayProfileForm/ilv_saved_profiles_ItemClick: Could not load the detailed preview for profile '{0}'.", profile.Name);
                    if (!IsDisposed && !_lifetimeToken.IsCancellationRequested && refreshVersion == _profileRefreshVersion)
                        _showError("DisplayMagician could not load the selected display profile. Please try again.", "Display Profile");
                }
            }
            if (IsDisposed || _lifetimeToken.IsCancellationRequested || refreshVersion != _profileRefreshVersion) return;
            if (e.Buttons == MouseButtons.Right) cms_profiles.Show(ilv_saved_profiles, e.Location);
        }

        private void ilv_saved_profiles_ItemDoubleClick(object sender, ItemClickEventArgs e) { if (!_commandInProgress && e.Item.VirtualItemKey is DisplayProfileView profile) { ++_profileRefreshVersion; ChangeSelectedProfile(profile); btn_apply.PerformClick(); } }
        private void ilv_saved_profiles_ItemHover(object sender, ItemHoverEventArgs e) { if (e.Item != null) tt_selected.SetToolTip(ilv_saved_profiles, e.Item.Text); else tt_selected.RemoveAll(); }
        private void txt_profile_save_name_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) btn_save_or_rename.PerformClick(); }

        private async void btn_update_Click(object sender, EventArgs e)
        {
            await RunProfileActionAsync(async () =>
            {
                if (_selectedProfile?.IsSaved != true || MessageBox.Show(this, $"Do you really want to overwrite the display settings in the '{_selectedProfile.Name}' Display Profile with the display settings currently in use? This cannot be undone.", "Update Display Profile settings?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
                string selectedId = _selectedProfile.Id;
                ControlResponse response = await _controlServiceClient.UpdateProfileFromCurrentAsync(selectedId, _lifetimeToken);
                if (IsDisposed || _lifetimeToken.IsCancellationRequested) return;
                if (!response.IsSuccessful) { _showError(response.Message, "Update Display Profile"); return; }
                await RefreshProfilesAsync(selectedId, _lifetimeToken);
            });
        }

        private async void btn_profile_settings_Click(object sender, EventArgs e)
        {
            await RunProfileActionAsync(async () =>
            {
                if (_selectedProfile?.IsSaved != true) return;
                string selectedId = _selectedProfile.Id;
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
                    ControlResponse response = await _controlServiceClient.UpdateDisplayProfileSettingsAsync(selectedId, form.Settings, _lifetimeToken);
                    if (IsDisposed || _lifetimeToken.IsCancellationRequested) return;
                    if (!response.IsSuccessful) { _showError(response.Message, "Profile Settings"); return; }
                }
                await RefreshProfilesAsync(selectedId, _lifetimeToken);
            });
        }

        private void UpdateHotkeyText()
        {
            if (_selectedProfile?.IsSaved != true || Program.AppProgramSettings == null || Program.AppDirectInputManager == null) { lbl_hotkey_assigned.Visible = false; return; }
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
        private void Save_Click(object sender, EventArgs e)
        {
            if (_selectedProfile?.IsSaved != true)
            {
                return;
            }

            dialog_save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            dialog_save.FileName = _selectedProfile.Name;
            if (dialog_save.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            string shortcutPath = Path.ChangeExtension(dialog_save.FileName, ".lnk");
            try
            {
                if (string.IsNullOrWhiteSpace(_selectedProfile.DesktopIconIcoBase64) && string.IsNullOrWhiteSpace(_selectedProfile.ThumbnailPngBase64))
                    throw new InvalidOperationException("The profile layout icon is unavailable.");

                string iconDirectory = Path.Combine(Program.AppIconPath, "ProfileCache");
                Directory.CreateDirectory(iconDirectory);
                string iconPath = Path.Combine(iconDirectory, $"{_selectedProfile.Id}.ico");
                if (!string.IsNullOrWhiteSpace(_selectedProfile.DesktopIconIcoBase64))
                {
                    File.WriteAllBytes(iconPath, Convert.FromBase64String(_selectedProfile.DesktopIconIcoBase64));
                }
                else
                {
                    using MemoryStream imageStream = new MemoryStream(Convert.FromBase64String(_selectedProfile.ThumbnailPngBase64));
                    using Image image = Image.FromStream(imageStream);
                    MultiIcon multiIcon = new MultiIcon();
                    SingleIcon icon = multiIcon.Add("ProfileLayout");
                    foreach (int size in new[] { 256, 64, 48, 32, 24, 16 })
                    {
                        using Bitmap bitmap = new Bitmap(size, size);
                        using (Graphics graphics = Graphics.FromImage(bitmap))
                        {
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.DrawImage(image, 0, 0, size, size);
                        }
                        icon.Add(bitmap);
                        if (size == 256) icon[icon.Count - 1].IconImageFormat = IconImageFormat.PNG;
                    }
                    multiIcon.SelectedIndex = 0;
                    multiIcon.Save(iconPath);
                }
                using (FileStream iconCheck = File.OpenRead(iconPath))
                {
                    if (iconCheck.Length == 0) throw new IOException("The profile icon file is empty.");
                }

                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null)
                {
                    throw new InvalidOperationException("WScript.Shell COM is unavailable.");
                }

                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = Environment.ProcessPath;
                shortcut.Arguments = $"{DisplayMagicianStartupAction.ChangeProfile} \"{_selectedProfile.Id}\"";
                shortcut.Description = $"Switching display profile to '{_selectedProfile.Name}'.";
                shortcut.WorkingDirectory = Path.GetDirectoryName(Environment.ProcessPath) ?? string.Empty;
                shortcut.IconLocation = iconPath;
                shortcut.Save();

                MessageBox.Show(this, $"Shortcut successfully saved to '{shortcutPath}'.", "Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "DisplayProfileForm/Save_Click: Could not save desktop shortcut for profile {0} to {1}.", _selectedProfile.Id, shortcutPath);
                MessageBox.Show(this, "DisplayMagician could not create the desktop shortcut.", "Shortcut", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                dialog_save.FileName = string.Empty;
            }
        }
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
