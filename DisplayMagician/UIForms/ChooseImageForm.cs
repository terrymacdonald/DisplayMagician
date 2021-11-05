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
    public partial class ChooseImageForm : Form
    {
        private ShortcutBitmap _selectedImage = new ShortcutBitmap();

        public ChooseImageForm()
        {
            InitializeComponent();
        }

        public List<ShortcutBitmap> AvailableImages
        {
            get;
            set;
        }

        public ShortcutBitmap SelectedImage
        {
            get;
            set;
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ChooseIconForm_Load(object sender, EventArgs e)
        {
            UpdateImageListBox();
            /*if (AvailableImages.Count > 0)
            {
                bool alreadySelected = false;
                // Load all the images into the list
                int imageCount = 1;
                foreach (ShortcutBitmap sc in AvailableImages)
                {
                    string[] stringsToAdd = new string[] { 
                        $"Image {sc.Order} from {sc.Name}",
                        $"{sc.Size.Width} x {sc.Size.Height}"
                    };
                    ListViewItem lvi = new ListViewItem(stringsToAdd);
                    lvi.Name = sc.UUID;
                    if (sc.Equals(SelectedImage))
                    {
                        lvi.Selected = true;
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
            }*/
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

        private void lv_icons_SelectedIndexChanged(object sender, EventArgs e)
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
                        MessageBox.Show(
                            $"Unable to parse {dialog_open.FileName} for images. Are you sure it's a valid image format?",
                            "Add images to icon",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Exclamation);

                    }
                }
                else
                {
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
        }
    }
}
