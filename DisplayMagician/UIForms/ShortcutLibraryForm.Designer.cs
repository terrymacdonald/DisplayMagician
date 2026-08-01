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
            btn_copy = new System.Windows.Forms.Button();
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
            btn_cancel = new System.Windows.Forms.Button();
            tlpMain = new System.Windows.Forms.TableLayoutPanel();
            flp_header = new System.Windows.Forms.FlowLayoutPanel();
            flp_actions = new System.Windows.Forms.FlowLayoutPanel();
            tlpMain.SuspendLayout();
            flp_header.SuspendLayout();
            flp_actions.SuspendLayout();
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
            ilv_saved_shortcuts.Dock = System.Windows.Forms.DockStyle.Fill;
            ilv_saved_shortcuts.Location = new System.Drawing.Point(12, 68);
            ilv_saved_shortcuts.MultiSelect = false;
            ilv_saved_shortcuts.Name = "ilv_saved_shortcuts";
            ilv_saved_shortcuts.PersistentCacheDirectory = "";
            ilv_saved_shortcuts.PersistentCacheSize = 100L;
            ilv_saved_shortcuts.Size = new System.Drawing.Size(1286, 615);
            ilv_saved_shortcuts.TabIndex = 22;
            ilv_saved_shortcuts.UseWIC = true;
            ilv_saved_shortcuts.ItemClick += ilv_saved_shortcuts_ItemClick;
            ilv_saved_shortcuts.ItemHover += ilv_saved_shortcuts_ItemHover;
            ilv_saved_shortcuts.ItemDoubleClick += ilv_saved_shortcuts_ItemDoubleClick;
            // 
            // btn_delete
            // 
            btn_delete.AutoSize = true;
            btn_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_delete.ForeColor = System.Drawing.Color.White;
            btn_delete.Location = new System.Drawing.Point(219, 3);
            btn_delete.Name = "btn_delete";
            btn_delete.Size = new System.Drawing.Size(82, 42);
            btn_delete.TabIndex = 26;
            btn_delete.Text = "&Delete";
            btn_delete.UseVisualStyleBackColor = true;
            btn_delete.Click += btn_delete_Click;
            // 
            // btn_back
            // 
            btn_back.AutoSize = true;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(1227, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(56, 42);
            btn_back.TabIndex = 27;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // btn_run
            // 
            btn_run.AutoSize = true;
            btn_run.BackColor = System.Drawing.Color.Black;
            btn_run.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_run.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_run.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_run.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_run.ForeColor = System.Drawing.Color.White;
            btn_run.Location = new System.Drawing.Point(3, 3);
            btn_run.Name = "btn_run";
            btn_run.Size = new System.Drawing.Size(66, 42);
            btn_run.TabIndex = 25;
            btn_run.Text = "&Run";
            btn_run.UseVisualStyleBackColor = false;
            btn_run.Click += btn_run_Click;
            // 
            // btn_edit
            // 
            btn_edit.AutoSize = true;
            btn_edit.BackColor = System.Drawing.Color.Black;
            btn_edit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_edit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_edit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_edit.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_edit.ForeColor = System.Drawing.Color.White;
            btn_edit.Location = new System.Drawing.Point(75, 3);
            btn_edit.Name = "btn_edit";
            btn_edit.Size = new System.Drawing.Size(66, 42);
            btn_edit.TabIndex = 28;
            btn_edit.Text = "&Edit";
            btn_edit.UseVisualStyleBackColor = false;
            btn_edit.Click += btn_edit_Click;
            // 
            // btn_new
            // 
            btn_new.AutoSize = true;
            btn_new.BackColor = System.Drawing.Color.Black;
            btn_new.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_new.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_new.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_new.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_new.ForeColor = System.Drawing.Color.White;
            btn_new.Location = new System.Drawing.Point(147, 3);
            btn_new.Name = "btn_new";
            btn_new.Size = new System.Drawing.Size(66, 42);
            btn_new.TabIndex = 29;
            btn_new.Text = "&New";
            btn_new.UseVisualStyleBackColor = false;
            btn_new.Click += btn_new_Click;
            // 
            // btn_save
            // 
            btn_save.AutoSize = true;
            btn_save.BackColor = System.Drawing.Color.Black;
            btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_save.ForeColor = System.Drawing.Color.White;
            btn_save.Location = new System.Drawing.Point(307, 3);
            btn_save.Name = "btn_save";
            btn_save.Size = new System.Drawing.Size(150, 42);
            btn_save.TabIndex = 30;
            btn_save.Text = "&Save to Desktop";
            btn_save.UseVisualStyleBackColor = false;
            btn_save.Click += btn_save_Click;
            // 
            // btn_copy
            // 
            btn_copy.AutoSize = true;
            btn_copy.BackColor = System.Drawing.Color.Black;
            btn_copy.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_copy.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_copy.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_copy.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            btn_copy.ForeColor = System.Drawing.Color.White;
            btn_copy.Location = new System.Drawing.Point(463, 3);
            btn_copy.Name = "btn_copy";
            btn_copy.Size = new System.Drawing.Size(75, 42);
            btn_copy.TabIndex = 36;
            btn_copy.Text = "&Copy";
            btn_copy.UseVisualStyleBackColor = false;
            btn_copy.Click += btn_copy_Click;
            // 
            // dialog_save
            // 
            dialog_save.DefaultExt = "lnk";
            dialog_save.DereferenceLinks = false;
            dialog_save.Filter = "Shortcuts|*.lnk";
            dialog_save.RestoreDirectory = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(3, 6);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(231, 29);
            label1.TabIndex = 31;
            label1.Text = "Game Shortcut Library";
            // 
            // lbl_create_shortcut
            // 
            lbl_create_shortcut.AutoSize = true;
            lbl_create_shortcut.BackColor = System.Drawing.Color.Brown;
            lbl_create_shortcut.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lbl_create_shortcut.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_create_shortcut.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            lbl_create_shortcut.ForeColor = System.Drawing.Color.White;
            lbl_create_shortcut.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            lbl_create_shortcut.Location = new System.Drawing.Point(12, 689);
            lbl_create_shortcut.Name = "lbl_create_shortcut";
            lbl_create_shortcut.Size = new System.Drawing.Size(1286, 49);
            lbl_create_shortcut.TabIndex = 32;
            lbl_create_shortcut.Text = "Click the 'New' button to create a shortcut";
            lbl_create_shortcut.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            lbl_mask.BackColor = System.Drawing.Color.Gray;
            lbl_mask.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_mask.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_mask.ForeColor = System.Drawing.Color.White;
            lbl_mask.Location = new System.Drawing.Point(12, 68);
            lbl_mask.Name = "lbl_mask";
            lbl_mask.Size = new System.Drawing.Size(1286, 615);
            lbl_mask.TabIndex = 33;
            lbl_mask.Text = "lbl_masked_form";
            lbl_mask.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_mask.Visible = false;
            // 
            // btn_help
            // 
            btn_help.AutoSize = true;
            btn_help.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_help.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_help.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_help.ForeColor = System.Drawing.Color.White;
            btn_help.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_help.Location = new System.Drawing.Point(1125, 3);
            btn_help.Name = "btn_help";
            btn_help.Size = new System.Drawing.Size(55, 42);
            btn_help.TabIndex = 34;
            btn_help.Text = "&Help";
            btn_help.UseVisualStyleBackColor = true;
            btn_help.Click += btn_help_Click;
            // 
            // btn_donate
            // 
            btn_donate.AutoSize = true;
            btn_donate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_donate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_donate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_donate.ForeColor = System.Drawing.Color.White;
            btn_donate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_donate.Location = new System.Drawing.Point(1186, 3);
            btn_donate.Name = "btn_donate";
            btn_donate.Size = new System.Drawing.Size(70, 42);
            btn_donate.TabIndex = 35;
            btn_donate.Text = "D&onate";
            btn_donate.UseVisualStyleBackColor = true;
            btn_donate.Click += btn_donate_Click;
            // 
            // btn_cancel
            // 
            btn_cancel.BackColor = System.Drawing.Color.Black;
            btn_cancel.Enabled = false;
            btn_cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_cancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_cancel.ForeColor = System.Drawing.Color.White;
            btn_cancel.Location = new System.Drawing.Point(587, 563);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Size = new System.Drawing.Size(140, 42);
            btn_cancel.TabIndex = 37;
            btn_cancel.Text = "&Cancel";
            btn_cancel.UseVisualStyleBackColor = false;
            btn_cancel.Visible = false;
            btn_cancel.Click += btn_cancel_Click;
            // 
            // flp_header
            // 
            flp_header.Controls.Add(btn_donate);
            flp_header.Controls.Add(btn_help);
            flp_header.Controls.Add(label1);
            flp_header.Dock = System.Windows.Forms.DockStyle.Fill;
            flp_header.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flp_header.Location = new System.Drawing.Point(12, 12);
            flp_header.Name = "flp_header";
            flp_header.Size = new System.Drawing.Size(1286, 50);
            flp_header.TabIndex = 38;
            // 
            // flp_actions
            // 
            flp_actions.Controls.Add(btn_back);
            flp_actions.Controls.Add(btn_copy);
            flp_actions.Controls.Add(btn_save);
            flp_actions.Controls.Add(btn_new);
            flp_actions.Controls.Add(btn_edit);
            flp_actions.Controls.Add(btn_delete);
            flp_actions.Controls.Add(btn_run);
            flp_actions.Dock = System.Windows.Forms.DockStyle.Fill;
            flp_actions.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flp_actions.Location = new System.Drawing.Point(12, 741);
            flp_actions.Name = "flp_actions";
            flp_actions.Size = new System.Drawing.Size(1286, 48);
            flp_actions.TabIndex = 39;
            // 
            // tlpMain
            // 
            tlpMain.ColumnCount = 1;
            tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpMain.Controls.Add(flp_header, 0, 0);
            tlpMain.Controls.Add(ilv_saved_shortcuts, 0, 1);
            tlpMain.Controls.Add(lbl_create_shortcut, 0, 2);
            tlpMain.Controls.Add(flp_actions, 0, 3);
            tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            tlpMain.Location = new System.Drawing.Point(0, 0);
            tlpMain.Name = "tlpMain";
            tlpMain.Padding = new System.Windows.Forms.Padding(9);
            tlpMain.RowCount = 4;
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlpMain.Size = new System.Drawing.Size(1310, 801);
            tlpMain.TabIndex = 0;
            // 
            // ShortcutLibraryForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(1310, 801);
            Controls.Add(tlpMain);
            Controls.Add(lbl_mask);
            Controls.Add(btn_cancel);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimumSize = new System.Drawing.Size(1000, 700);
            Name = "ShortcutLibraryForm";
            ShowIcon = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician - Game Shortcuts";
            Load += ShortcutLibraryForm_Load;
            KeyPress += ShortcutLibraryForm_KeyPress;
            cms_shortcuts.ResumeLayout(false);
            flp_actions.ResumeLayout(false);
            flp_actions.PerformLayout();
            flp_header.ResumeLayout(false);
            flp_header.PerformLayout();
            tlpMain.ResumeLayout(false);
            tlpMain.PerformLayout();
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
            private System.Windows.Forms.TableLayoutPanel tlpMain;
            private System.Windows.Forms.FlowLayoutPanel flp_header;
            private System.Windows.Forms.FlowLayoutPanel flp_actions;
        }
    }

