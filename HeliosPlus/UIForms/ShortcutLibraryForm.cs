using Manina.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeliosPlus.UIForms
{
    public partial class ShortcutLibraryForm : Form
    {

        List<Shortcut> _savedShortcuts = new List<Shortcut>();
        private ShortcutAdaptor _shortcutAdaptor;

        public ShortcutLibraryForm()
        {
            InitializeComponent();
            _shortcutAdaptor = new ShortcutAdaptor();
        }

        private void btn_new_Click(object sender, EventArgs e)
        {
            var shortcutForm = new ShortcutForm();
            shortcutForm.ShowDialog(this);
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShortcutLibraryForm_Load(object sender, EventArgs e)
        {
            // Load all the shortcuts we have saved earlier
            List<Shortcut> _savedShortcuts = Shortcut.LoadAllShortcuts();
            // Refresh the Profile UI
            RefreshDisplayProfileUI();
        }

        private void RefreshDisplayProfileUI()
        {

            if (_savedShortcuts.Count > 0)
            {
                // Temporarily stop updating the saved_profiles listview
                ilv_saved_profiles.SuspendLayout();

                ImageListViewItem newItem = null;
                foreach (Shortcut loadedShortcut in _savedShortcuts)
                {
                    bool thisLoadedProfileIsAlreadyHere = (from item in ilv_saved_profiles.Items where item.Text == loadedShortcut.Name select item.Text).Any();
                    if (!thisLoadedProfileIsAlreadyHere)
                    {
                        newItem = new ImageListViewItem(loadedShortcut, loadedShortcut.Name);
                        //ilv_saved_profiles.Items.Add(newItem);
                        ilv_saved_profiles.Items.Add(newItem, _shortcutAdaptor);
                    }
                }

                // Restart updating the saved_profiles listview
                ilv_saved_profiles.ResumeLayout();

            }

        }
    }
}
