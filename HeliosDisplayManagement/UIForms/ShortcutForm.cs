using System;
using System.Collections.Generic;
using System.Drawing.IconLib;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using HeliosDisplayManagement.Resources;
using HeliosDisplayManagement.Shared;
using HeliosDisplayManagement.Steam;

namespace HeliosDisplayManagement.UIForms
{
    public partial class ShortcutForm : Form
    {
        public ShortcutForm()
        {
            InitializeComponent();
        }

        public ShortcutForm(Profile profile) : this()
        {
            Profile = profile;
        }

        public string Arguments
        {
            get { return cb_args.Checked ? txt_args.Text : string.Empty; }
            set
            {
                txt_args.Text = value;
                cb_args.Checked = !string.IsNullOrWhiteSpace(txt_args.Text);
            }
        }

        public string FileName
        {
            get { return cb_temp.Checked && rb_standalone.Checked ? txt_executable.Text : string.Empty; }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    cb_temp.Checked = true;
                    rb_standalone.Checked = true;
                    txt_executable.Text = value;
                }
            }
        }

        public static string IconCache
            =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Assembly.GetExecutingAssembly().GetName().Name, @"IconCache");

        public string ProcessName
        {
            get
            {
                return cb_temp.Checked && rb_standalone.Checked && cb_process.Checked ? txt_process.Text : string.Empty;
            }
            set
            {
                txt_process.Text = value;
                cb_process.Checked = !string.IsNullOrWhiteSpace(txt_process.Text);
            }
        }

        public Profile Profile
        {
            get { return dv_profile.Profile; }
            set { dv_profile.Profile = value; }
        }

        public uint SteamAppId
        {
            get { return cb_temp.Checked && rb_steam.Checked ? (uint) nud_steamappid.Value : 0; }
            set
            {
                if (value > 0)
                {
                    cb_temp.Checked = true;
                    rb_steam.Checked = true;
                    nud_steamappid.Value = value;
                }
            }
        }

        public uint Timeout
        {
            get
            {
                if (!cb_temp.Checked)
                    return 0;
                if (!rb_standalone.Checked)
                    return (uint) nud_steamtimeout.Value;
                if (cb_process.Checked)
                    return (uint) nud_timeout.Value;
                return 0;
            }
            set
            {
                if (value > 0)
                {
                    nud_timeout.Value = value;
                    nud_steamtimeout.Value = value;
                }
            }
        }

        private void btn_app_executable_Click(object sender, EventArgs e)
        {
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
                if (File.Exists(dialog_open.FileName) && (Path.GetExtension(dialog_open.FileName) == @".exe"))
                {
                    txt_executable.Text = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        Language.Selected_file_is_not_a_valid_executable_file,
                        Language.Executable,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.None;
            try
            {
                if (dialog_save.ShowDialog(this) == DialogResult.OK)
                {
                    if (CreateShortcut(dialog_save.FileName))
                        MessageBox.Show(
                            Language.Shortcut_place_successfully,
                            Language.Shortcut,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    else
                        MessageBox.Show(
                            Language.Failed_to_create_the_shortcut_Unexpected_exception_occurred,
                            Language.Shortcut,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                    dialog_save.FileName = string.Empty;
                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Language.Shortcut, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Controls_CheckedChanged(object sender, EventArgs e)
        {
            g_temp.Enabled = cb_temp.Checked;

            p_standalone.Enabled = rb_standalone.Checked;
            txt_process.Enabled = cb_process.Checked;
            nud_timeout.Enabled = cb_process.Checked;

            p_steam.Enabled = rb_steam.Checked;

            txt_args.Enabled = cb_args.Checked;

            if (rb_steam.Checked)
                nud_steamappid_ValueChanged(rb_steam, e);
        }

        // ReSharper disable once FunctionComplexityOverflow
        // ReSharper disable once CyclomaticComplexity
        private bool CreateShortcut(string fileName)
        {
            var programName = Path.GetFileNameWithoutExtension(txt_executable.Text);
            var description = string.Empty;
            var icon = string.Empty;
            var args = new List<string>
            {
                $"-a {HeliosStartupAction.SwitchProfile}",
                $"-p \"{dv_profile.Profile.Name}\""
            };

            if (cb_temp.Checked)
            {
                if (rb_standalone.Checked)
                {
                    if (string.IsNullOrWhiteSpace(txt_executable.Text))
                        throw new Exception(Language.Executable_address_can_not_be_empty);
                    if (!File.Exists(txt_executable.Text))
                        throw new Exception(Language.Executable_file_not_found);
                    args.Add($"-e \"{txt_executable.Text.Trim()}\"");
                    if (!string.IsNullOrWhiteSpace(txt_process.Text))
                    {
                        args.Add($"-w \"{txt_process.Text.Trim()}\"");
                        args.Add($"-t {(int) nud_timeout.Value}");
                    }
                    description = string.Format(Language.Executing_application_with_profile, programName, Profile.Name);
                    try
                    {
                        icon = Path.Combine(IconCache, Guid.NewGuid() + ".ico");
                        new ProfileIcon(Profile).ToIconOverly(txt_executable.Text)
                            .Save(icon, MultiIconFormat.ICO);
                    }
                    catch (Exception)
                    {
                        icon = $"{txt_executable.Text.Trim()},0";
                    }
                }
                else if (rb_steam.Checked)
                {
                    if (!SteamGame.SteamInstalled)
                        throw new Exception(Language.Steam_is_not_installed);
                    var steamGame = new SteamGame((uint) nud_steamappid.Value);
                    args.Add($"-s {(int) nud_steamappid.Value}");
                    args.Add($"-t {(int) nud_steamtimeout.Value}");
                    description = string.Format(Language.Executing_application_with_profile, steamGame.Name,
                        Profile.Name);
                    var steamIcon = steamGame.GetIcon().Result;
                    if (!string.IsNullOrWhiteSpace(steamIcon))
                        try
                        {
                            icon = Path.Combine(IconCache, Guid.NewGuid() + ".ico");
                            new ProfileIcon(Profile).ToIconOverly(steamIcon)
                                .Save(icon, MultiIconFormat.ICO);
                        }
                        catch (Exception)
                        {
                            icon = steamIcon;
                        }
                    else
                        icon = $"{SteamGame.SteamAddress},0";
                }
                if (cb_args.Checked && !string.IsNullOrWhiteSpace(txt_args.Text))
                    args.Add($"--arguments \"{txt_args.Text.Trim()}\"");
            }
            else
            {
                description = string.Format(Language.Switching_display_profile_to_profile, Profile.Name);
                try
                {
                    icon = Path.Combine(IconCache, Guid.NewGuid() + ".ico");
                    new ProfileIcon(Profile).ToIcon().Save(icon, MultiIconFormat.ICO);
                }
                catch
                {
                    icon = string.Empty;
                }
            }

            fileName = Path.ChangeExtension(fileName, @"lnk");
            if (fileName != null)
                try
                {
                    // Remove the old file to replace it
                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    var wshShellType = Type.GetTypeFromCLSID(new Guid("72C24DD5-D70A-438B-8A42-98424B88AFB8"));
                    dynamic wshShell = Activator.CreateInstance(wshShellType);
                    try
                    {
                        var shortcut = wshShell.CreateShortcut(fileName);
                        try
                        {
                            shortcut.TargetPath = Application.ExecutablePath;
                            shortcut.Arguments = string.Join(" ", args);
                            shortcut.Description = description;
                            shortcut.WorkingDirectory = Path.GetDirectoryName(Application.ExecutablePath) ??
                                                        string.Empty;
                            if (!string.IsNullOrWhiteSpace(icon))
                                shortcut.IconLocation = icon;
                            shortcut.Save();
                        }
                        finally
                        {
                            Marshal.FinalReleaseComObject(shortcut);
                        }
                    }
                    finally
                    {
                        Marshal.FinalReleaseComObject(wshShell);
                    }
                }
                catch
                {
                    // Clean up a failed attempt
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                }
            return (fileName != null) && File.Exists(fileName);
        }

        private void nud_steamappid_ValueChanged(object sender, EventArgs e)
        {
            lbl_steamname.Text = new SteamGame((uint) nud_steamappid.Value).ToString();
        }

        private void nud_steamapps_Click(object sender, EventArgs e)
        {
            var steamGamesForm = new SteamGamesForm();
            if ((steamGamesForm.ShowDialog(this) == DialogResult.OK) && (steamGamesForm.SteamGame != null))
                nud_steamappid.Value = steamGamesForm.SteamGame.AppId;
        }

        private void txt_executable_TextChanged(object sender, EventArgs e)
        {
            try
            {
                txt_process.Text = Path.GetFileNameWithoutExtension(txt_executable.Text)?.ToLower() ?? txt_process.Text;
            }
            catch
            {
                // ignored
            }
        }
    }
}