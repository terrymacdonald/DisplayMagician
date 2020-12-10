namespace DisplayMagician.UIForms
{
    partial class ShortcutLibraryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortcutLibraryForm));
            this.ilv_saved_shortcuts = new Manina.Windows.Forms.ImageListView();
            this.btn_delete = new System.Windows.Forms.Button();
            this.btn_back = new System.Windows.Forms.Button();
            this.btn_run = new System.Windows.Forms.Button();
            this.btn_edit = new System.Windows.Forms.Button();
            this.btn_new = new System.Windows.Forms.Button();
            this.btn_save = new System.Windows.Forms.Button();
            this.dialog_save = new System.Windows.Forms.SaveFileDialog();
            this.label1 = new System.Windows.Forms.Label();
            this.tt_selected = new System.Windows.Forms.ToolTip(this.components);
            this.lbl_create_shortcut = new System.Windows.Forms.Label();
            this.cms_shortcuts = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tsmi_edit = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_run = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_save_to_desktop = new System.Windows.Forms.ToolStripMenuItem();
            this.tsmi_delete = new System.Windows.Forms.ToolStripMenuItem();
            this.cms_shortcuts.SuspendLayout();
            this.SuspendLayout();
            // 
            // ilv_saved_shortcuts
            // 
            this.ilv_saved_shortcuts.AllowCheckBoxClick = false;
            this.ilv_saved_shortcuts.AllowColumnClick = false;
            this.ilv_saved_shortcuts.AllowColumnResize = false;
            this.ilv_saved_shortcuts.AllowItemReorder = false;
            this.ilv_saved_shortcuts.AllowPaneResize = false;
            this.ilv_saved_shortcuts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ilv_saved_shortcuts.Colors = new Manina.Windows.Forms.ImageListViewColor(resources.GetString("ilv_saved_shortcuts.Colors"));
            this.ilv_saved_shortcuts.Location = new System.Drawing.Point(0, 98);
            this.ilv_saved_shortcuts.MultiSelect = false;
            this.ilv_saved_shortcuts.Name = "ilv_saved_shortcuts";
            this.ilv_saved_shortcuts.PersistentCacheDirectory = "";
            this.ilv_saved_shortcuts.PersistentCacheSize = ((long)(100));
            this.ilv_saved_shortcuts.Size = new System.Drawing.Size(1134, 512);
            this.ilv_saved_shortcuts.TabIndex = 22;
            this.ilv_saved_shortcuts.UseWIC = true;
            this.ilv_saved_shortcuts.ItemClick += new Manina.Windows.Forms.ItemClickEventHandler(this.ilv_saved_shortcuts_ItemClick);
            this.ilv_saved_shortcuts.ItemHover += new Manina.Windows.Forms.ItemHoverEventHandler(this.ilv_saved_shortcuts_ItemHover);
            this.ilv_saved_shortcuts.ItemDoubleClick += new Manina.Windows.Forms.ItemDoubleClickEventHandler(this.ilv_saved_shortcuts_ItemDoubleClick);
            // 
            // btn_delete
            // 
            this.btn_delete.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_delete.ForeColor = System.Drawing.Color.White;
            this.btn_delete.Location = new System.Drawing.Point(462, 630);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(120, 40);
            this.btn_delete.TabIndex = 26;
            this.btn_delete.Text = "&Delete";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // btn_back
            // 
            this.btn_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_back.ForeColor = System.Drawing.Color.White;
            this.btn_back.Location = new System.Drawing.Point(1047, 681);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 27;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // btn_run
            // 
            this.btn_run.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_run.BackColor = System.Drawing.Color.Black;
            this.btn_run.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_run.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_run.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_run.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_run.ForeColor = System.Drawing.Color.White;
            this.btn_run.Location = new System.Drawing.Point(588, 630);
            this.btn_run.Name = "btn_run";
            this.btn_run.Size = new System.Drawing.Size(120, 40);
            this.btn_run.TabIndex = 25;
            this.btn_run.Text = "&Run";
            this.btn_run.UseVisualStyleBackColor = false;
            this.btn_run.Click += new System.EventHandler(this.btn_run_Click);
            // 
            // btn_edit
            // 
            this.btn_edit.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_edit.BackColor = System.Drawing.Color.Black;
            this.btn_edit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_edit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_edit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_edit.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_edit.ForeColor = System.Drawing.Color.White;
            this.btn_edit.Location = new System.Drawing.Point(336, 630);
            this.btn_edit.Name = "btn_edit";
            this.btn_edit.Size = new System.Drawing.Size(120, 40);
            this.btn_edit.TabIndex = 28;
            this.btn_edit.Text = "&Edit";
            this.btn_edit.UseVisualStyleBackColor = false;
            this.btn_edit.Click += new System.EventHandler(this.btn_edit_Click);
            // 
            // btn_new
            // 
            this.btn_new.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_new.BackColor = System.Drawing.Color.Black;
            this.btn_new.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_new.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_new.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_new.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_new.ForeColor = System.Drawing.Color.White;
            this.btn_new.Location = new System.Drawing.Point(210, 630);
            this.btn_new.Name = "btn_new";
            this.btn_new.Size = new System.Drawing.Size(120, 40);
            this.btn_new.TabIndex = 29;
            this.btn_new.Text = "&New";
            this.btn_new.UseVisualStyleBackColor = false;
            this.btn_new.Click += new System.EventHandler(this.btn_new_Click);
            // 
            // btn_save
            // 
            this.btn_save.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_save.BackColor = System.Drawing.Color.Black;
            this.btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save.ForeColor = System.Drawing.Color.White;
            this.btn_save.Location = new System.Drawing.Point(714, 630);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(211, 40);
            this.btn_save.TabIndex = 30;
            this.btn_save.Text = "&Save to Desktop";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // dialog_save
            // 
            this.dialog_save.DefaultExt = "lnk";
            this.dialog_save.DereferenceLinks = false;
            this.dialog_save.Filter = global::DisplayMagician.Resources.Language.Shortcuts_Filter;
            this.dialog_save.RestoreDirectory = true;
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(407, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(309, 33);
            this.label1.TabIndex = 31;
            this.label1.Text = "Game Shortcut Library";
            // 
            // lbl_create_shortcut
            // 
            this.lbl_create_shortcut.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lbl_create_shortcut.AutoSize = true;
            this.lbl_create_shortcut.BackColor = System.Drawing.Color.Brown;
            this.lbl_create_shortcut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbl_create_shortcut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lbl_create_shortcut.ForeColor = System.Drawing.Color.White;
            this.lbl_create_shortcut.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_create_shortcut.Location = new System.Drawing.Point(413, 562);
            this.lbl_create_shortcut.Name = "lbl_create_shortcut";
            this.lbl_create_shortcut.Size = new System.Drawing.Size(304, 22);
            this.lbl_create_shortcut.TabIndex = 32;
            this.lbl_create_shortcut.Text = "Click the \'New\' button to create a shortcut";
            // 
            // cms_shortcuts
            // 
            this.cms_shortcuts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsmi_edit,
            this.tsmi_run,
            this.tsmi_save_to_desktop,
            this.tsmi_delete});
            this.cms_shortcuts.Name = "cms_shortcuts";
            this.cms_shortcuts.Size = new System.Drawing.Size(216, 114);
            // 
            // tsmi_edit
            // 
            this.tsmi_edit.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tsmi_edit.Name = "tsmi_edit";
            this.tsmi_edit.Size = new System.Drawing.Size(215, 22);
            this.tsmi_edit.Text = "Edit Shortcut...";
            this.tsmi_edit.Click += new System.EventHandler(this.tsmi_edit_Click);
            // 
            // tsmi_run
            // 
            this.tsmi_run.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.tsmi_run.Name = "tsmi_run";
            this.tsmi_run.Size = new System.Drawing.Size(215, 22);
            this.tsmi_run.Text = "Run Shortcut...";
            this.tsmi_run.Click += new System.EventHandler(this.tsmi_run_Click);
            // 
            // tsmi_save_to_desktop
            // 
            this.tsmi_save_to_desktop.Name = "tsmi_save_to_desktop";
            this.tsmi_save_to_desktop.Size = new System.Drawing.Size(215, 22);
            this.tsmi_save_to_desktop.Text = "Save Shortcut to Desktop...";
            this.tsmi_save_to_desktop.Click += new System.EventHandler(this.tsmi_save_to_desktop_Click);
            // 
            // tsmi_delete
            // 
            this.tsmi_delete.Name = "tsmi_delete";
            this.tsmi_delete.Size = new System.Drawing.Size(215, 22);
            this.tsmi_delete.Text = "Delete Shortcut...";
            this.tsmi_delete.Click += new System.EventHandler(this.tsmi_delete_Click);
            // 
            // ShortcutLibraryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1134, 716);
            this.Controls.Add(this.lbl_create_shortcut);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.btn_new);
            this.Controls.Add(this.btn_edit);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.btn_back);
            this.Controls.Add(this.btn_run);
            this.Controls.Add(this.ilv_saved_shortcuts);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1150, 755);
            this.MinimumSize = new System.Drawing.Size(756, 375);
            this.Name = "ShortcutLibraryForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DisplayMagician - Setup Game Shortcuts";
            this.Activated += new System.EventHandler(this.ShortcutLibraryForm_Activated);
            this.Load += new System.EventHandler(this.ShortcutLibraryForm_Load);
            this.cms_shortcuts.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Manina.Windows.Forms.ImageListView ilv_saved_shortcuts;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Button btn_run;
        private System.Windows.Forms.Button btn_edit;
        private System.Windows.Forms.Button btn_new;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.SaveFileDialog dialog_save;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip tt_selected;
        private System.Windows.Forms.Label lbl_create_shortcut;
        private System.Windows.Forms.ContextMenuStrip cms_shortcuts;
        private System.Windows.Forms.ToolStripMenuItem tsmi_edit;
        private System.Windows.Forms.ToolStripMenuItem tsmi_run;
        private System.Windows.Forms.ToolStripMenuItem tsmi_delete;
        private System.Windows.Forms.ToolStripMenuItem tsmi_save_to_desktop;
    }
}