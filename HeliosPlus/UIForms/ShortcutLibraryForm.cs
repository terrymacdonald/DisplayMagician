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

        private ShortcutAdaptor _shortcutAdaptor = new ShortcutAdaptor();
        //private ImageListViewItem _selectedShortcutILVItem = null;
        private ShortcutItem _selectedShortcut = null;

        public ShortcutLibraryForm()
        {
            InitializeComponent();
            //_shortcutAdaptor = new ShortcutAdaptor();
            //_shortcutRepository = new ShortcutRepository();
            //_profileRepository = new ProfileRepository();
            ilv_saved_shortcuts.MultiSelect = false;
            ilv_saved_shortcuts.ThumbnailSize = new Size(100,100);
            ilv_saved_shortcuts.AllowDrag = false;
            ilv_saved_shortcuts.AllowDrop = false;
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShortcutLibraryForm_Load(object sender, EventArgs e)
        {
            // Refresh the Shortcut Library UI
            RefreshShortcutLibraryUI();
        }

        private void RefreshShortcutLibraryUI()
        {

            if (ShortcutRepository.ShortcutCount == 0)
                return;

            // Temporarily stop updating the saved_profiles listview
            ilv_saved_shortcuts.SuspendLayout();

            ImageListViewItem newItem = null;
            ilv_saved_shortcuts.Items.Clear();

            
            foreach (ShortcutItem loadedShortcut in ShortcutRepository.AllShortcuts.OrderBy(s => s.Name))
            {
                //loadedProfile.SaveProfileImageToCache();
                //newItem = new ImageListViewItem(loadedProfile.SavedProfileCacheFilename, loadedProfile.Name);
                //newItem = new ImageListViewItem(loadedProfile, loadedProfile.Name);
                newItem = new ImageListViewItem(loadedShortcut, loadedShortcut.Name);

                // Select it if its the selectedProfile
                if (_selectedShortcut is ShortcutItem && _selectedShortcut.Equals(loadedShortcut))
                    newItem.Selected = true;

                //ilv_saved_profiles.Items.Add(newItem);
                ilv_saved_shortcuts.Items.Add(newItem, _shortcutAdaptor);
            }

    
            // Restart updating the saved_profiles listview
            ilv_saved_shortcuts.ResumeLayout();

        }
    
        private ShortcutItem GetShortcutFromName(string shortcutName)
        {
            return (from item in ShortcutRepository.AllShortcuts where item.Name == shortcutName select item).First();
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
                            dialog_save.FileName = _selectedShortcut.Name;
                        }
                    }

                    // Show the Save Shortcut window
                    if (dialog_save.ShowDialog(this) == DialogResult.OK)
                    {
                        if (_selectedShortcut.CreateShortcut(dialog_save.FileName))
                        {
                            MessageBox.Show(
                                String.Format(Language.Shortcut_placed_successfully, dialog_save.FileName),
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

            ShortcutItem oldShortcut = _selectedShortcut;
            var shortcutForm = new ShortcutForm(_selectedShortcut);
            shortcutForm.ShowDialog(this);
            if (shortcutForm.DialogResult == DialogResult.OK)
            {
                ShortcutRepository.RemoveShortcut(oldShortcut);
                _selectedShortcut = shortcutForm.Shortcut;
                ShortcutRepository.AddShortcut(_selectedShortcut);
                RefreshShortcutLibraryUI();
                // As this is an edit, we need to manually force saving the shortcut library
                //ShortcutRepository.SaveShortcuts();
            }

        }

        private void btn_new_Click(object sender, EventArgs e)
        {
            var shortcutForm = new ShortcutForm();
            shortcutForm.ShowDialog(this);
            if (shortcutForm.DialogResult == DialogResult.OK)
            {
                ShortcutRepository.AddShortcut(shortcutForm.Shortcut);
                _selectedShortcut = shortcutForm.Shortcut;
                RefreshShortcutLibraryUI();
            }
        }

        private void btn_edit_Click(object sender, EventArgs e)
        {
            int currentIlvIndex = ilv_saved_shortcuts.SelectedItems[0].Index;
            string shortcutName = ilv_saved_shortcuts.Items[currentIlvIndex].Text;
            _selectedShortcut = GetShortcutFromName(shortcutName);

            if (_selectedShortcut == null)
                return;

            ShortcutItem oldShortcut = _selectedShortcut;
            var shortcutForm = new ShortcutForm(_selectedShortcut);
            shortcutForm.ShowDialog(this);
            if (shortcutForm.DialogResult == DialogResult.OK)
            {
                ShortcutRepository.RemoveShortcut(oldShortcut);
                _selectedShortcut = shortcutForm.Shortcut;
                ShortcutRepository.AddShortcut(_selectedShortcut);
                RefreshShortcutLibraryUI();
                // As this is an edit, we need to manually force saving the shortcut library
                //ShortcutRepository.SaveShortcuts();
            }

        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut == null)
                return;

            if (MessageBox.Show($"Are you sure you want to delete the '{_selectedShortcut.Name}' Shortcut?", $"Delete '{_selectedShortcut.Name}' Shortcut?", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.No)
                return;

            // remove the profile from the imagelistview
            int currentIlvIndex = ilv_saved_shortcuts.SelectedItems[0].Index;
            ilv_saved_shortcuts.Items.RemoveAt(currentIlvIndex);

            // Remove the shortcut
            ShortcutRepository.RemoveShortcut(_selectedShortcut);
            _selectedShortcut = null;

            RefreshShortcutLibraryUI();
        }

        private void btn_run_Click(object sender, EventArgs e)
        {
            if (_selectedShortcut == null)
                return;

            // Run the shortcut
            ShortcutRepository.RunShortcut(_selectedShortcut);
        }
    }
}
