using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class ChooseIconForm : Form
    {
        private ShortcutBitmap _selectedImage = new ShortcutBitmap();

        public ChooseIconForm()
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
            if (AvailableImages.Count > 0)
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

        }
    }
}
