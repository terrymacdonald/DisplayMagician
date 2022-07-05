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
    public partial class ChooseExecutableForm : Form
    {
        private AppAdaptor _appAdaptor = new AppAdaptor();
        private App _selectedApp = null;
        private App _appToUse = null;

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

        private void btn_back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /*private void ChooseExecutableForm(object sender, EventArgs e)
        {
            UpdateImageListBox();
        }

        private void UpdateImageListBox()
        {
            lv_icons.Items.Clear();
            if (AvailableImages.Count > 0)
            {
                foreach (ShortcutBitmap sc in AvailableImages)
                {
                    string[] stringsToAdd = new string[] {
                        $"Image {sc.Order} from {sc.Name}",
                        $"{sc.Size.Width} x {sc.Size.Height}"
                    };
                    ListViewItem lvi = new ListViewItem(stringsToAdd);
                    lvi.Name = sc.UUID;
                    if (ImageUtils.ImagesAreEqual(sc.Image,SelectedImage.Image))
                    {
                        lvi.Selected = true;
                        lvi.Focused = true;
                        lvi.EnsureVisible();
                        pb_selected_icon.Image = SelectedImage.Image;
                        _selectedImage = sc;
                    }
                    lv_icons.Items.Add(lvi);
                }

                // Select the first largest image listed if there isn't one already
                if (lv_icons.SelectedItems.Count == 0)
                {
                    lv_icons.Items[0].Selected = true;
                    pb_selected_icon.Image = AvailableImages[0].Image;
                    _selectedImage = AvailableImages[0];
                }
            }
        }


        private void btn_select_Click(object sender, EventArgs e)
        {
            SelectedImage = _selectedImage;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ilv_installed_apps_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lv_icons.SelectedItems.Count > 0)
            {
                string uuidToFind = lv_icons.SelectedItems[0].Name;
                foreach (ShortcutBitmap sc in AvailableImages)
                {
                    if (sc.UUID.Equals(uuidToFind))
                    {
                        pb_selected_icon.Image = sc.Image;
                        _selectedImage = sc;
                        break;
                    }
                }
            }
        }

        private void btn_add_Click(object sender, EventArgs e)
        {
            dialog_open.InitialDirectory = Program.AppDownloadsPath;
            dialog_open.DefaultExt = "*.exe; *.com; *.ico; *.bmp; *.jpg; *.png; *.tif; *.gif";
            dialog_open.Filter = "All exe and image files (*.exe; *.com; *.ico; *.bmp; *.jpg; *.png; *.tif; *.gif) | *.exe; *.com; *.ico; *.bmp; *.jpg; *.png; *.tif; *.gif | All files(*.*) | *.*";
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(dialog_open.FileName))
                {
                    try
                    {
                        List<ShortcutBitmap> newImages = ImageUtils.GetMeAllBitmapsFromFile(dialog_open.FileName);
                        if (newImages.Count == 0)
                        {
                            logger.Trace($"No new images found when parsing {dialog_open.FileName} for images. Are you sure it's a valid image format?");
                            MessageBox.Show(
                            $"No new images found when parsing {dialog_open.FileName} for images. Are you sure it's a valid image format?",
                            "Add images to icon",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);
                        }
                        else
                        {
                            AvailableImages.AddRange(ImageUtils.GetMeAllBitmapsFromFile(dialog_open.FileName));
                            UpdateImageListBox();
                            logger.Trace($"ChooseImageForm/btn_add_Click: Added {newImages.Count} image(s) from {dialog_open.FileName} to the end of this image list.");
                            MessageBox.Show(
                            $"Added {newImages.Count} image(s) from {dialog_open.FileName} to the end of this image list.",
                            "Add images to icon",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);

                        }
                        dialog_open.FileName = string.Empty;
                    }
                    catch(Exception ex)
                    {
                        logger.Warn(ex, $"ChooseImageForm/btn_add_Click: Exception - unable to parse {dialog_open.FileName} for images. Are you sure it's a valid image format?");
                        MessageBox.Show(
                            $"Unable to parse {dialog_open.FileName} for images. Are you sure it's a valid image format?",
                            "Add images to icon",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);

                    }
                }
                else
                {
                    logger.Warn($"ChooseImageForm/btn_add_Click: Unable to open {dialog_open.FileName} to parse it for images. Are you sure you have the right file permissions?");
                    MessageBox.Show(
                        $"Unable to open {dialog_open.FileName} to parse it for images. Are you sure you have the right file permissions?",
                        "Add images to icon",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
        }

        private void btn_remove_Click(object sender, EventArgs e)
        {
            if (lv_icons.SelectedItems.Count > 0)
            {
                string uuidToFind = lv_icons.SelectedItems[0].Name;
                foreach (ShortcutBitmap sc in AvailableImages)
                {
                    if (sc.UUID.Equals(uuidToFind))
                    {
                        AvailableImages.Remove(sc);
                        pb_selected_icon.Image = null;
                        _selectedImage = default(ShortcutBitmap);
                        UpdateImageListBox();
                        return;
                    }
                }
                // If we get here we didn't find anything to remove!
                MessageBox.Show(
                    $"Unable to remove selected item from the list of images!",
                    "Remove image from icon",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }*/

        private string getExeFile()
        {
            dialog_open.InitialDirectory = Environment.SpecialFolder.ProgramFiles.ToString();
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
                if (_selectedApp is ShortcutItem && _selectedApp.Equals(installedApp))
                {
                    logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: This shortcut {installedApp.Name} is the selected one so selecting it in the UI");
                    newItem.Selected = true;
                }

                //ilv_saved_profiles.Items.Add(newItem);
                logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Adding this shortcut {installedApp.Name} to the imagelistview");
                ilv_installed_apps.Items.Add(newItem, _appAdaptor);
            }

            logger.Trace($"ChooseExecutableForm/RefreshExecutableFormUI: Resuming the imagelistview layout");

            // Restart updating the saved_profiles listview
            ilv_installed_apps.ResumeLayout();

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
                _appToUse = _selectedApp;
                DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }
}
