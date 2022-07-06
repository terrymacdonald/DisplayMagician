using DisplayMagician.AppLibraries;
using DisplayMagician.GameLibraries;
using DisplayMagician.Resources;
using Manina.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public enum ChooseExecutableFormMode : UInt32
    {
        AppMode = 0,
        ExeMode = 1,
    }

    public partial class ChooseExecutableForm : Form
    {
        private AppAdaptor _appAdaptor = new AppAdaptor();
        private App _selectedApp = null;
        private App _appToUse = null;
        private string _exeToUse = null;
        private string _previousExe = null;
        private ChooseExecutableFormMode _chooseExecutableFormMode = ChooseExecutableFormMode.AppMode;

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public ChooseExecutableForm()
        {
            InitializeComponent();
            ilv_installed_apps.MultiSelect = false;
            ilv_installed_apps.ThumbnailSize = new Size(100, 100);
            ilv_installed_apps.AllowDrag = false;
            ilv_installed_apps.AllowDrop = false;
            ilv_installed_apps.SetRenderer(new AppILVRenderer());
        }

        public App AppToUse
        {
            get
            {
                return _appToUse;
            }
            set
            {
                if (value is App)
                {
                    _appToUse = value;
                }                    
            }
        }

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

        public ChooseExecutableFormMode Mode
        {
            get
            {
                return _chooseExecutableFormMode;
            }
            set
            {
                if (value is ChooseExecutableFormMode)
                {
                    _chooseExecutableFormMode = value;
                }
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
            dialog_open.Filter = "exe files (*.exe;*.com) | *.exe;*.com | All files(*.*) | *.*";
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
                        Language.Selected_file_is_not_a_valid_file,
                        Language.Executable,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
            return textToReturn;
        }

        private void ChooseExecutableForm_Load(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(_previousExe))
            {
                // First lookup to see if this path is an app or just an exe
                // If it is an app then select it
                foreach (App installedApp in AppLibrary.AllInstalledAppsInAllLibraries)
                {
                    if (installedApp.ExePath.Equals(_previousExe))
                    {
                        _selectedApp = installedApp;
                    }
                }
            }

            // Set up the _selectedApp if we're passed something
            if (_chooseExecutableFormMode == ChooseExecutableFormMode.AppMode && _appToUse is App)
            {
                _selectedApp = _appToUse;
            }

            // Refresh the Installed Apps Library UI
            RefreshExecutableFormUI();

        }

        private void RefreshExecutableFormUI()
        {

            if (AppLibrary.AllInstalledAppsInAllLibraries.Count == 0)
                return;

            // Temporarily stop updating the saved_profiles listview
            logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Suspending the imagelistview layout");
            ilv_installed_apps.SuspendLayout();

            ImageListViewItem newItem = null;
            logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Emptying shortcut list");
            ilv_installed_apps.Items.Clear();


            foreach (App installedApp in AppLibrary.AllInstalledAppsInAllLibraries.OrderBy(s => s.Name))
            {
                logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Adding app {installedApp.Name} into the list of applications shown to the user ");

                newItem = new ImageListViewItem(installedApp, installedApp.Name);

                // Select it if its the selectedProfile
                if (_selectedApp is App && _selectedApp.Equals(installedApp))
                {
                    logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: This shortcut {installedApp.Name} is the selected one so selecting it in the UI");
                    newItem.Selected = true;
                    newItem.Focused = true;
                    btn_select_app.Enabled = true;                    
                }

                //ilv_saved_profiles.Items.Add(newItem);
                logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Adding this shortcut {installedApp.Name} to the imagelistview");
                ilv_installed_apps.Items.Add(newItem, _appAdaptor);
            }

            logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Resuming the imagelistview layout");

            // Restart updating the saved_profiles listview
            ilv_installed_apps.ResumeLayout();

            if (ilv_installed_apps.SelectedItems.Count > 0)
            {
                // Make sure that if the item is selected that it's visible
                int selectedIndex = ilv_installed_apps.SelectedItems[0].Index;                
                ilv_installed_apps.EnsureVisible(selectedIndex);
            }            

        }

        private void ilv_installed_apps_ItemClick(object sender, ItemClickEventArgs e)
        {
            string selectedInstalledApp = e.Item.Text;
            foreach (App app in AppLibrary.AllInstalledAppsInAllLibraries)
            {
                if (app.Name == selectedInstalledApp)
                {
                    _selectedApp = app;
                    break;
                }
            }

            try
            {
                btn_select_app.Enabled = true;
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"ShortcutForm/ilv_games_ItemClick: Exception while figuring out if the select button shoud be enabled.");
            }
        }

        private void btn_select_app_Click(object sender, EventArgs e)
        {
            if (_selectedApp is App && ilv_installed_apps.SelectedItems.Count > 0)
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
            _chooseExecutableFormMode = ChooseExecutableFormMode.ExeMode;
            _appToUse = null;
            _exeToUse = getExeFile();
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
