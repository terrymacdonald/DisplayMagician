using DisplayMagician.Contracts;
//using DisplayMagician.Resources;
using Manina.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public enum ChooseExecutableFormMode : UInt32
    {
        AppMode = 0,
        ExeMode = 1,
    }

    public partial class ChooseExecutableForm : DisplayMagicianForm
    {
        private AppAdaptor _appAdaptor = new AppAdaptor();
        private readonly List<AppView> _installedApps = new List<AppView>();
        private AppView _selectedApp = null;
        private AppView _appToUse = null;
        private string _exeToUse = null;
        private string _previousExe = null;
        private string _previousAppId = null;
        private readonly ControlServicePipeClient _client;
        private readonly Action<string> _showError;
        private readonly Func<string> _chooseExecutable;
        private readonly CancellationTokenSource _lifetimeSource = new CancellationTokenSource();
        private readonly CancellationToken _lifetimeToken;
        private bool _lifetimeDisposed;
        private ChooseExecutableFormMode _chooseExecutableFormMode = ChooseExecutableFormMode.AppMode;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ChooseExecutableForm() : this(new ControlServicePipeClient()) { }

        internal ChooseExecutableForm(ControlServicePipeClient client, Action<string> showError = null, Func<string> chooseExecutable = null)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _showError = showError ?? (message => MessageBox.Show(this, message, "Applications Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Exclamation));
            _chooseExecutable = chooseExecutable ?? getExeFile;
            _lifetimeToken = _lifetimeSource.Token;
            InitializeComponent();
            btn_select_app.Enabled = false;
            Disposed += (_, _) =>
            {
                if (_lifetimeDisposed) return;
                _lifetimeDisposed = true;
                _lifetimeSource.Cancel();
                _lifetimeSource.Dispose();
            };
            ilv_installed_apps.MultiSelect = false;
            ilv_installed_apps.ThumbnailSize = new Size(100, 100);
            ilv_installed_apps.AllowDrag = false;
            ilv_installed_apps.AllowDrop = false;
            ilv_installed_apps.SetRenderer(new AppILVRenderer());
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (!e.Cancel && !_lifetimeDisposed) _lifetimeSource.Cancel();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int? PreviousAppLibrary { get; set; }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AppView AppToUse
        {
            get
            {
                return _appToUse;
            }
            set
            {
                if (value is AppView)
                {
                    _appToUse = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string ExeToUse
        {
            get
            {
                return _exeToUse;
            }
            set
            {
                if (value is String)
                {
                    _exeToUse = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PreviousExe
        {
            get
            {
                return _previousExe;
            }
            set
            {
                if (value is String)
                {
                    _previousExe = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PreviousAppId
        {
            get
            {
                return _previousAppId;
            }
            set
            {
                if (value is String)
                {
                    _previousAppId = value;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ChooseExecutableFormMode Mode
        {
            get
            {
                return _chooseExecutableFormMode;
            }
            set
            {
                _chooseExecutableFormMode = value;
            }
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }


        private string getExeFile()
        {
            dialog_open.InitialDirectory = Environment.SpecialFolder.ProgramFiles.ToString();
            dialog_open.FileName = "";
            dialog_open.DefaultExt = "*.exe";
            dialog_open.Filter = "Executable files (*.exe;*.com;*.msi;*.bat;*.cmd;*.ps1;*.lnk;*.url) | *.exe;*.com;*.msi;*.bat;*.cmd;*.ps1;*.lnk;*.url | All files(*.*) | *.*";
            string textToReturn = "";
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(dialog_open.FileName))
                {
                    textToReturn = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        "Selected file is not a valid file.",
                        "Executable",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
            return textToReturn;
        }

        private async void ChooseExecutableForm_Load(object sender, EventArgs e)
        {
            await LoadAppsAsync();
        }

        private async Task LoadAppsAsync()
        {
            try
            {
                AppListResult result = await _client.ListAppsAsync(_lifetimeToken);
                if (IsDisposed || Disposing || _lifetimeToken.IsCancellationRequested) return;
                _installedApps.Clear();
                _installedApps.AddRange(result.Apps);
            }
            catch (Exception ex)
            {
                if (IsDisposed || Disposing || _lifetimeToken.IsCancellationRequested) return;
                logger.Error(ex, "ChooseExecutableForm/ChooseExecutableForm_Load: Failed to retrieve applications from the User Agent.");
                _showError("DisplayMagician could not retrieve installed applications. You can still choose an executable file.");
            }

            string previousId = _appToUse?.Id ?? _previousAppId;
            int? previousLibrary = _appToUse?.Library ?? PreviousAppLibrary;
            AppView[] matches = _installedApps.Where(app => string.Equals(app.Id, previousId, StringComparison.OrdinalIgnoreCase) &&
                (!previousLibrary.HasValue || app.Library == previousLibrary.Value)).ToArray();
            _selectedApp = matches.Length == 1 ? matches[0] : null;

            // Refresh the Installed Apps Library UI
            RefreshExecutableFormUI();

            // Gah! I cannot get the EnsureVisible code to work as EnsureVisible doesn't seem to work until the ilistview is drawn
            if (ilv_installed_apps.SelectedItems.Count > 0)
            {
                // Make sure that if the item is selected that it's visible
                int selectedIndex = ilv_installed_apps.SelectedItems[0].Index;
                if (ilv_installed_apps.IsItemVisible(ilv_installed_apps.SelectedItems[0]) == ItemVisibility.NotVisible)
                {
                    ilv_installed_apps.EnsureVisible(selectedIndex);
                }
            }
        }

        private void RefreshExecutableFormUI()
        {

            // Temporarily stop updating the saved_profiles listview
            logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Suspending the imagelistview layout");
            ilv_installed_apps.SuspendLayout();

            ImageListViewItem newItem = null;
            logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Emptying shortcut list");
            ilv_installed_apps.Items.Clear();
            btn_select_app.Enabled = false;


            foreach (AppView installedApp in _installedApps.OrderBy(app => app.Name))
            {
                logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Adding app {installedApp.Name} into the list of applications shown to the user ");

                newItem = new ImageListViewItem(installedApp, installedApp.Name);

                //ilv_saved_profiles.Items.Add(newItem);
                logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Adding this shortcut {installedApp.Name} to the imagelistview");
                ilv_installed_apps.Items.Add(newItem, _appAdaptor);


                // Select it if its the selectedProfile
                if (_selectedApp != null && _selectedApp.Library == installedApp.Library && string.Equals(_selectedApp.Id, installedApp.Id, StringComparison.OrdinalIgnoreCase))
                {
                    logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: This shortcut {installedApp.Name} is the selected one so selecting it in the UI");
                    newItem.Selected = true;
                    newItem.Focused = true;
                    btn_select_app.Enabled = true;
                    //ilv_installed_apps.EnsureVisible(newItem.Index);
                }

            }

            logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Resuming the imagelistview layout");

            // Restart updating the saved_profiles listview
            ilv_installed_apps.ResumeLayout();
            ilv_installed_apps.Refresh();

        }

        private void ilv_installed_apps_ItemClick(object sender, ItemClickEventArgs e)
        {
            _selectedApp = null;
            foreach (AppView app in _installedApps)
            {
                if (e.Item.VirtualItemKey is AppView selected && app.Library == selected.Library &&
                    string.Equals(app.Id, selected.Id, StringComparison.OrdinalIgnoreCase))
                {
                    _selectedApp = app;
                    break;
                }
            }

            try
            {
                btn_select_app.Enabled = _selectedApp != null;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, "ChooseExecutableForm/ilv_installed_apps_ItemClick: Could not update application selection controls.");
            }
        }

        private void btn_select_app_Click(object sender, EventArgs e)
        {
            if (_selectedApp != null && ilv_installed_apps.SelectedItems.Count > 0)
            {
                _chooseExecutableFormMode = ChooseExecutableFormMode.AppMode;
                _appToUse = _selectedApp;
                _exeToUse = null;
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void btn_select_exe_Click(object sender, EventArgs e)
        {
            string executable = _chooseExecutable();
            if (string.IsNullOrWhiteSpace(executable)) return;
            _chooseExecutableFormMode = ChooseExecutableFormMode.ExeMode;
            _appToUse = null;
            _exeToUse = executable;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ChooseExecutableForm_Activated(object sender, EventArgs e)
        {
            /*if (ilv_installed_apps.SelectedItems.Count > 0)
            {
                // Make sure that if the item is selected that it's visible
                int selectedIndex = ilv_installed_apps.SelectedItems[0].Index;
                ilv_installed_apps.EnsureVisible(selectedIndex);
            }*/
        }

        private void ChooseExecutableForm_Shown(object sender, EventArgs e)
        {
        }

        private void ilv_installed_apps_VisibleChanged(object sender, EventArgs e)
        {
        }
    }
}
