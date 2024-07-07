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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortcutLibraryForm));
            ilv_saved_shortcuts = new Manina.Windows.Forms.ImageListView();
            btn_delete = new System.Windows.Forms.Button();
            btn_back = new System.Windows.Forms.Button();
            btn_run = new System.Windows.Forms.Button();
            btn_edit = new System.Windows.Forms.Button();
            btn_new = new System.Windows.Forms.Button();
            btn_save = new System.Windows.Forms.Button();
            dialog_save = new System.Windows.Forms.SaveFileDialog();
            label1 = new System.Windows.Forms.Label();
            tt_selected = new System.Windows.Forms.ToolTip(components);
            lbl_create_shortcut = new System.Windows.Forms.Label();
            cms_shortcuts = new System.Windows.Forms.ContextMenuStrip(components);
            tsmi_edit = new System.Windows.Forms.ToolStripMenuItem();
            tsmi_run = new System.Windows.Forms.ToolStripMenuItem();
            tsmi_save_to_desktop = new System.Windows.Forms.ToolStripMenuItem();
            sendToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            tsmi_copy = new System.Windows.Forms.ToolStripMenuItem();
            tsmi_delete = new System.Windows.Forms.ToolStripMenuItem();
            lbl_mask = new System.Windows.Forms.Label();
            btn_help = new System.Windows.Forms.Button();
            btn_donate = new System.Windows.Forms.Button();
            btn_copy = new System.Windows.Forms.Button();
            btn_cancel = new System.Windows.Forms.Button();
            cms_shortcuts.SuspendLayout();
            SuspendLayout();
            // 
            // ilv_saved_shortcuts
            // 
            ilv_saved_shortcuts.AllowCheckBoxClick = false;
            ilv_saved_shortcuts.AllowColumnClick = false;
            ilv_saved_shortcuts.AllowColumnResize = false;
            ilv_saved_shortcuts.AllowItemReorder = false;
            ilv_saved_shortcuts.AllowPaneResize = false;
            ilv_saved_shortcuts.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ilv_saved_shortcuts.Location = new System.Drawing.Point(0, 113);
            ilv_saved_shortcuts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ilv_saved_shortcuts.MultiSelect = false;
            ilv_saved_shortcuts.Name = "ilv_saved_shortcuts";
            ilv_saved_shortcuts.PersistentCacheDirectory = "";
            ilv_saved_shortcuts.PersistentCacheSize = 100L;
            ilv_saved_shortcuts.Size = new System.Drawing.Size(1323, 591);
            ilv_saved_shortcuts.TabIndex = 22;
            ilv_saved_shortcuts.UseWIC = true;
            ilv_saved_shortcuts.ItemClick += ilv_saved_shortcuts_ItemClick;
            ilv_saved_shortcuts.ItemHover += ilv_saved_shortcuts_ItemHover;
            ilv_saved_shortcuts.ItemDoubleClick += ilv_saved_shortcuts_ItemDoubleClick;
            // 
            // btn_delete
            // 
            btn_delete.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_delete.ForeColor = System.Drawing.Color.White;
            btn_delete.Location = new System.Drawing.Point(440, 742);
            btn_delete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_delete.Name = "btn_delete";
            btn_delete.Size = new System.Drawing.Size(140, 36);
            btn_delete.TabIndex = 26;
            btn_delete.Text = "&Delete";
            btn_delete.UseVisualStyleBackColor = true;
            btn_delete.Click += btn_delete_Click;
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(1222, 786);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 27;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // btn_run
            // 
            btn_run.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_run.BackColor = System.Drawing.Color.Black;
            btn_run.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_run.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_run.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_run.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_run.ForeColor = System.Drawing.Color.White;
            btn_run.Location = new System.Drawing.Point(734, 742);
            btn_run.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_run.Name = "btn_run";
            btn_run.Size = new System.Drawing.Size(140, 36);
            btn_run.TabIndex = 25;
            btn_run.Text = "&Run";
            btn_run.UseVisualStyleBackColor = false;
            btn_run.Click += btn_run_Click;
            // 
            // btn_edit
            // 
            btn_edit.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_edit.BackColor = System.Drawing.Color.Black;
            btn_edit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_edit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_edit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_edit.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_edit.ForeColor = System.Drawing.Color.White;
            btn_edit.Location = new System.Drawing.Point(293, 742);
            btn_edit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_edit.Name = "btn_edit";
            btn_edit.Size = new System.Drawing.Size(140, 36);
            btn_edit.TabIndex = 28;
            btn_edit.Text = "&Edit";
            btn_edit.UseVisualStyleBackColor = false;
            btn_edit.Click += btn_edit_Click;
            // 
            // btn_new
            // 
            btn_new.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_new.BackColor = System.Drawing.Color.Black;
            btn_new.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_new.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_new.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_new.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_new.ForeColor = System.Drawing.Color.White;
            btn_new.Location = new System.Drawing.Point(146, 742);
            btn_new.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_new.Name = "btn_new";
            btn_new.Size = new System.Drawing.Size(140, 36);
            btn_new.TabIndex = 29;
            btn_new.Text = "&New";
            btn_new.UseVisualStyleBackColor = false;
            btn_new.Click += btn_new_Click;
            // 
            // btn_save
            // 
            btn_save.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_save.BackColor = System.Drawing.Color.Black;
            btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_save.ForeColor = System.Drawing.Color.White;
            btn_save.Location = new System.Drawing.Point(881, 742);
            btn_save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_save.Name = "btn_save";
            btn_save.Size = new System.Drawing.Size(246, 36);
            btn_save.TabIndex = 30;
            btn_save.Text = "&Save to Desktop";
            btn_save.UseVisualStyleBackColor = false;
            btn_save.Click += btn_save_Click;
            // 
            // dialog_save
            // 
            dialog_save.DefaultExt = "lnk";
            dialog_save.DereferenceLinks = false;
            dialog_save.Filter = Resources.Language.Shortcuts_Filter;
            dialog_save.RestoreDirectory = true;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(475, 38);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(309, 33);
            label1.TabIndex = 31;
            label1.Text = "Game Shortcut Library";
            // 
            // lbl_create_shortcut
            // 
            lbl_create_shortcut.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            lbl_create_shortcut.AutoSize = true;
            lbl_create_shortcut.BackColor = System.Drawing.Color.Brown;
            lbl_create_shortcut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl_create_shortcut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            lbl_create_shortcut.ForeColor = System.Drawing.Color.White;
            lbl_create_shortcut.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            lbl_create_shortcut.Location = new System.Drawing.Point(482, 648);
            lbl_create_shortcut.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_create_shortcut.Name = "lbl_create_shortcut";
            lbl_create_shortcut.Size = new System.Drawing.Size(304, 22);
            lbl_create_shortcut.TabIndex = 32;
            lbl_create_shortcut.Text = "Click the 'New' button to create a shortcut";
            // 
            // cms_shortcuts
            // 
            cms_shortcuts.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { tsmi_edit, tsmi_run, tsmi_save_to_desktop, sendToClipboardToolStripMenuItem, tsmi_copy, tsmi_delete });
            cms_shortcuts.Name = "cms_shortcuts";
            cms_shortcuts.Size = new System.Drawing.Size(216, 136);
            // 
            // tsmi_edit
            // 
            tsmi_edit.Font = new System.Drawing.Font("Segoe UI", 9F);
            tsmi_edit.Name = "tsmi_edit";
            tsmi_edit.Size = new System.Drawing.Size(215, 22);
            tsmi_edit.Text = "Edit Shortcut...";
            tsmi_edit.Click += tsmi_edit_Click;
            // 
            // tsmi_run
            // 
            tsmi_run.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            tsmi_run.Name = "tsmi_run";
            tsmi_run.Size = new System.Drawing.Size(215, 22);
            tsmi_run.Text = "Run Shortcut...";
            tsmi_run.Click += tsmi_run_Click;
            // 
            // tsmi_save_to_desktop
            // 
            tsmi_save_to_desktop.Name = "tsmi_save_to_desktop";
            tsmi_save_to_desktop.Size = new System.Drawing.Size(215, 22);
            tsmi_save_to_desktop.Text = "Save Shortcut to Desktop...";
            tsmi_save_to_desktop.Click += tsmi_save_to_desktop_Click;
            // 
            // sendToClipboardToolStripMenuItem
            // 
            sendToClipboardToolStripMenuItem.Name = "sendToClipboardToolStripMenuItem";
            sendToClipboardToolStripMenuItem.Size = new System.Drawing.Size(215, 22);
            sendToClipboardToolStripMenuItem.Text = "Send to Clipboard...";
            sendToClipboardToolStripMenuItem.Click += sendToClipboardToolStripMenuItem_Click;
            // 
            // tsmi_copy
            // 
            tsmi_copy.Name = "tsmi_copy";
            tsmi_copy.Size = new System.Drawing.Size(215, 22);
            tsmi_copy.Text = "Duplicate Shortcut...";
            tsmi_copy.Click += tsmi_copy_Click;
            // 
            // tsmi_delete
            // 
            tsmi_delete.Name = "tsmi_delete";
            tsmi_delete.Size = new System.Drawing.Size(215, 22);
            tsmi_delete.Text = "Delete Shortcut...";
            tsmi_delete.Click += tsmi_delete_Click;
            // 
            // lbl_mask
            // 
            lbl_mask.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lbl_mask.BackColor = System.Drawing.Color.Gray;
            lbl_mask.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_mask.ForeColor = System.Drawing.Color.White;
            lbl_mask.Location = new System.Drawing.Point(420, 353);
            lbl_mask.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_mask.Name = "lbl_mask";
            lbl_mask.Size = new System.Drawing.Size(484, 120);
            lbl_mask.TabIndex = 33;
            lbl_mask.Text = "lbl_masked_form";
            lbl_mask.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_mask.Visible = false;
            // 
            // btn_help
            // 
            btn_help.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_help.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_help.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_help.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_help.ForeColor = System.Drawing.Color.White;
            btn_help.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_help.Location = new System.Drawing.Point(1127, 14);
            btn_help.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_help.Name = "btn_help";
            btn_help.Size = new System.Drawing.Size(88, 27);
            btn_help.TabIndex = 34;
            btn_help.Text = "&Help";
            btn_help.UseVisualStyleBackColor = true;
            btn_help.Click += btn_help_Click;
            // 
            // btn_donate
            // 
            btn_donate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_donate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_donate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_donate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_donate.ForeColor = System.Drawing.Color.White;
            btn_donate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_donate.Location = new System.Drawing.Point(1222, 14);
            btn_donate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_donate.Name = "btn_donate";
            btn_donate.Size = new System.Drawing.Size(88, 27);
            btn_donate.TabIndex = 35;
            btn_donate.Text = "D&onate";
            btn_donate.UseVisualStyleBackColor = true;
            btn_donate.Click += btn_donate_Click;
            // 
            // btn_copy
            // 
            btn_copy.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_copy.BackColor = System.Drawing.Color.Black;
            btn_copy.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_copy.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_copy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_copy.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F);
            btn_copy.ForeColor = System.Drawing.Color.White;
            btn_copy.Location = new System.Drawing.Point(587, 742);
            btn_copy.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_copy.Name = "btn_copy";
            btn_copy.Size = new System.Drawing.Size(140, 36);
            btn_copy.TabIndex = 36;
            btn_copy.Text = "&Copy";
            btn_copy.UseVisualStyleBackColor = false;
            btn_copy.Click += btn_copy_Click;
            // 
            // btn_cancel
            // 
            btn_cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_cancel.BackColor = System.Drawing.Color.Black;
            btn_cancel.Enabled = false;
            btn_cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_cancel.ForeColor = System.Drawing.Color.White;
            btn_cancel.Location = new System.Drawing.Point(587, 563);
            btn_cancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Size = new System.Drawing.Size(140, 42);
            btn_cancel.TabIndex = 37;
            btn_cancel.Text = "&Cancel";
            btn_cancel.UseVisualStyleBackColor = false;
            btn_cancel.Visible = false;
            btn_cancel.Click += btn_cancel_Click;
            // 
            // ShortcutLibraryForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(1323, 826);
            Controls.Add(btn_cancel);
            Controls.Add(btn_copy);
            Controls.Add(btn_donate);
            Controls.Add(btn_help);
            Controls.Add(lbl_mask);
            Controls.Add(lbl_create_shortcut);
            Controls.Add(label1);
            Controls.Add(btn_save);
            Controls.Add(btn_new);
            Controls.Add(btn_edit);
            Controls.Add(btn_delete);
            Controls.Add(btn_back);
            Controls.Add(btn_run);
            Controls.Add(ilv_saved_shortcuts);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(1339, 865);
            MinimumSize = new System.Drawing.Size(987, 444);
            Name = "ShortcutLibraryForm";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician - Game Shortcuts";
            Load += ShortcutLibraryForm_Load;
            KeyPress += ShortcutLibraryForm_KeyPress;
            cms_shortcuts.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
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
        private System.Windows.Forms.Label lbl_mask;
        private System.Windows.Forms.Button btn_help;
        private System.Windows.Forms.Button btn_donate;
        private System.Windows.Forms.Button btn_copy;
        private System.Windows.Forms.ToolStripMenuItem tsmi_copy;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.ToolStripMenuItem sendToClipboardToolStripMenuItem;
    }
}