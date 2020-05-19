using HeliosPlus.GameLibraries;
using HeliosPlus.Resources;
using HeliosPlus.Shared;
using Manina.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.IconLib;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeliosPlus.UIForms
{
    public partial class ShortcutLibraryForm : Form
    {

        private ShortcutAdaptor _shortcutAdaptor;
        private ImageListViewItem _selectedShortcutILVItem = null;
        private Shortcut _selectedShortcut = null;

        public ShortcutLibraryForm()
        {
            InitializeComponent();
            _shortcutAdaptor = new ShortcutAdaptor();
        }

        private void btn_new_Click(object sender, EventArgs e)
        {
            var shortcutForm = new ShortcutForm();
            shortcutForm.ShowDialog(this);
            if (shortcutForm.DialogResult == DialogResult.OK)
            {
                _selectedShortcut = shortcutForm.Shortcut;
                RefreshShortcutLibraryUI();
            }
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShortcutLibraryForm_Load(object sender, EventArgs e)
        {
            // Load all the shortcuts we have saved earlier
            List<Shortcut> _savedShortcuts = Shortcut.LoadAllShortcuts();
            // Refresh the Shortcut Library UI
            RefreshShortcutLibraryUI();
        }

        private void RefreshShortcutLibraryUI()
        {

            if (Shortcut.AllSavedShortcuts.Count > 0)
            {
                // Temporarily stop updating the saved_profiles listview
                ilv_saved_shortcuts.SuspendLayout();

                ImageListViewItem newItem = null;
                foreach (Shortcut loadedShortcut in Shortcut.AllSavedShortcuts)
                {
                    bool thisLoadedProfileIsAlreadyHere = (from item in ilv_saved_shortcuts.Items where item.Text == loadedShortcut.Name select item.Text).Any();
                    if (!thisLoadedProfileIsAlreadyHere)
                    {
                        newItem = new ImageListViewItem(loadedShortcut, loadedShortcut.Name);
                        //ilv_saved_profiles.Items.Add(newItem);
                        ilv_saved_shortcuts.Items.Add(newItem, _shortcutAdaptor);
                    }

                    if (_selectedShortcut != null && _selectedShortcut is Shortcut)
                        RefreshImageListView(_selectedShortcut);
                }

                // Restart updating the saved_profiles listview
                ilv_saved_shortcuts.ResumeLayout();

            }
            // Refresh the image list view
            //RefreshImageListView(profile);
        }



    
        private void RefreshImageListView(Shortcut shortcut)
        {
            ilv_saved_shortcuts.ClearSelection();
            IEnumerable<ImageListViewItem> matchingImageListViewItems = (from item in ilv_saved_shortcuts.Items where item.Text == shortcut.Name select item);
            if (matchingImageListViewItems.Any())
            {
                matchingImageListViewItems.First().Selected = true;
                matchingImageListViewItems.First().Focused = true;
            }

        }


        private Shortcut GetShortcutFromName(string shortcutName)
        {
            return (from item in Shortcut.AllSavedShortcuts where item.Name == shortcutName select item).First();
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.None;

            // Only do something if there is a shortcut selected
            if (_selectedShortcut != null)
            {

                try
                {
                    // Set the Shortcut save folder to the Desktop as that's where people will want it most likely
                    dialog_save.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    // Try to set up some sensible suggestions for the Shortcut name
                    if (_selectedShortcut.Permanence == ShortcutPermanence.Permanent)
                    {

                        dialog_save.FileName = _selectedShortcut.Name;
                    }
                    else
                    {
                        if (_selectedShortcut.Category == ShortcutCategory.Application)
                        {
                            dialog_save.FileName = String.Concat(Path.GetFileNameWithoutExtension(_selectedShortcut.ExecutableNameAndPath), @" (", _selectedShortcut.Name.ToLower(CultureInfo.InvariantCulture), @")");
                        }
                        else
                        {
                            dialog_save.FileName = String.Concat(_selectedShortcut, @" (", _selectedShortcut.Name, @")");
                        }
                    }

                    // Show the Save Shortcut window
                    if (dialog_save.ShowDialog(this) == DialogResult.OK)
                    {
                        if (_selectedShortcut.CreateShortcut(dialog_save.FileName))
                        {
                            MessageBox.Show(
                                Language.Shortcut_placed_successfully,
                                Language.Shortcut,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show(
                                Language.Failed_to_create_the_shortcut_Unexpected_exception_occurred,
                                Language.Shortcut,
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);
                        }

                        dialog_save.FileName = string.Empty;
                        DialogResult = DialogResult.OK;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Language.Shortcut, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void ilv_saved_shortcuts_ItemClick(object sender, ItemClickEventArgs e)
        {
            _selectedShortcut = GetShortcutFromName(e.Item.Text);
        }

        private void ilv_saved_shortcuts_ItemDoubleClick(object sender, ItemClickEventArgs e)
        {
            _selectedShortcut = GetShortcutFromName(e.Item.Text);

            if (_selectedShortcut == null)
                return;

            var shortcutForm = new ShortcutForm(_selectedShortcut);
            shortcutForm.ShowDialog(this);
            if (shortcutForm.DialogResult == DialogResult.OK)
            {
                _selectedShortcut = shortcutForm.Shortcut;
                RefreshShortcutLibraryUI();
            }

        }

        private void btn_edit_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut == null)
                return;

            var shortcutForm = new ShortcutForm(_selectedShortcut);
            shortcutForm.ShowDialog(this);
            if (shortcutForm.DialogResult == DialogResult.OK)
            {
                _selectedShortcut = shortcutForm.Shortcut;
                RefreshShortcutLibraryUI();
            }

        }
    }
}
