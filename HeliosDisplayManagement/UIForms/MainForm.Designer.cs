using HeliosDisplayManagement.Resources;
using HeliosDisplayManagement.Shared.UserControls;

namespace HeliosDisplayManagement.UIForms
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btn_clone = new System.Windows.Forms.Button();
            this.btn_apply = new System.Windows.Forms.Button();
            this.btn_edit = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.lbl_profile = new System.Windows.Forms.Label();
            this.btn_delete = new System.Windows.Forms.Button();
            this.dv_profile = new HeliosDisplayManagement.Shared.UserControls.DisplayView();
            this.btn_shortcut = new System.Windows.Forms.Button();
            this.menu_profiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.applyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createShortcutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lv_profiles = new System.Windows.Forms.ListView();
            this.il_profiles = new System.Windows.Forms.ImageList(this.components);
            this.lbl_version = new System.Windows.Forms.Label();
            this.menu_profiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_clone
            // 
            this.btn_clone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_clone.Location = new System.Drawing.Point(488, 437);
            this.btn_clone.Name = "btn_clone";
            this.btn_clone.Size = new System.Drawing.Size(75, 23);
            this.btn_clone.TabIndex = 6;
            this.btn_clone.Text = global::HeliosDisplayManagement.Resources.Language.Clone;
            this.btn_clone.UseVisualStyleBackColor = true;
            this.btn_clone.Click += new System.EventHandler(this.Clone_Click);
            // 
            // btn_apply
            // 
            this.btn_apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_apply.Location = new System.Drawing.Point(731, 437);
            this.btn_apply.Name = "btn_apply";
            this.btn_apply.Size = new System.Drawing.Size(75, 23);
            this.btn_apply.TabIndex = 9;
            this.btn_apply.Text = global::HeliosDisplayManagement.Resources.Language.Apply;
            this.btn_apply.UseVisualStyleBackColor = true;
            this.btn_apply.Click += new System.EventHandler(this.Apply_Click);
            // 
            // btn_edit
            // 
            this.btn_edit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_edit.Location = new System.Drawing.Point(650, 437);
            this.btn_edit.Name = "btn_edit";
            this.btn_edit.Size = new System.Drawing.Size(75, 23);
            this.btn_edit.TabIndex = 8;
            this.btn_edit.Text = global::HeliosDisplayManagement.Resources.Language.Edit;
            this.btn_edit.UseVisualStyleBackColor = true;
            this.btn_edit.Click += new System.EventHandler(this.Edit_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Location = new System.Drawing.Point(265, 437);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 5;
            this.btn_cancel.Text = global::HeliosDisplayManagement.Resources.Language.Cancel;
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // lbl_profile
            // 
            this.lbl_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.lbl_profile.Location = new System.Drawing.Point(262, 8);
            this.lbl_profile.Name = "lbl_profile";
            this.lbl_profile.Size = new System.Drawing.Size(382, 13);
            this.lbl_profile.TabIndex = 3;
            this.lbl_profile.Text = global::HeliosDisplayManagement.Resources.Language.None;
            this.lbl_profile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btn_delete
            // 
            this.btn_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_delete.Location = new System.Drawing.Point(569, 437);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(75, 23);
            this.btn_delete.TabIndex = 7;
            this.btn_delete.Text = global::HeliosDisplayManagement.Resources.Language.Delete;
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // dv_profile
            // 
            this.dv_profile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dv_profile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(184)))), ((int)(((byte)(196)))));
            this.dv_profile.Font = new System.Drawing.Font("Consolas", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            this.dv_profile.Location = new System.Drawing.Point(256, 18);
            this.dv_profile.Margin = new System.Windows.Forms.Padding(18);
            this.dv_profile.Name = "dv_profile";
            this.dv_profile.PaddingX = 100;
            this.dv_profile.PaddingY = 100;
            this.dv_profile.Profile = null;
            this.dv_profile.Size = new System.Drawing.Size(560, 413);
            this.dv_profile.TabIndex = 4;
            // 
            // btn_shortcut
            // 
            this.btn_shortcut.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_shortcut.Location = new System.Drawing.Point(392, 437);
            this.btn_shortcut.Name = "btn_shortcut";
            this.btn_shortcut.Size = new System.Drawing.Size(90, 23);
            this.btn_shortcut.TabIndex = 10;
            this.btn_shortcut.Text = global::HeliosDisplayManagement.Resources.Language.Create_Shortcut;
            this.btn_shortcut.UseVisualStyleBackColor = true;
            this.btn_shortcut.Click += new System.EventHandler(this.CreateShortcut_Click);
            // 
            // menu_profiles
            // 
            this.menu_profiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applyToolStripMenuItem,
            this.toolStripMenuItem2,
            this.cloneToolStripMenuItem,
            this.createShortcutToolStripMenuItem,
            this.editToolStripMenuItem,
            this.deleteToolStripMenuItem});
            this.menu_profiles.Name = "menu_profiles";
            this.menu_profiles.Size = new System.Drawing.Size(157, 120);
            // 
            // applyToolStripMenuItem
            // 
            this.applyToolStripMenuItem.Name = "applyToolStripMenuItem";
            this.applyToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.applyToolStripMenuItem.Text = global::HeliosDisplayManagement.Resources.Language.Apply;
            this.applyToolStripMenuItem.Click += new System.EventHandler(this.Apply_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(153, 6);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.cloneToolStripMenuItem.Text = global::HeliosDisplayManagement.Resources.Language.Clone;
            this.cloneToolStripMenuItem.Click += new System.EventHandler(this.Clone_Click);
            // 
            // createShortcutToolStripMenuItem
            // 
            this.createShortcutToolStripMenuItem.Name = "createShortcutToolStripMenuItem";
            this.createShortcutToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.createShortcutToolStripMenuItem.Text = global::HeliosDisplayManagement.Resources.Language.Create_Shortcut;
            this.createShortcutToolStripMenuItem.Click += new System.EventHandler(this.CreateShortcut_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.editToolStripMenuItem.Text = global::HeliosDisplayManagement.Resources.Language.Edit;
            this.editToolStripMenuItem.Click += new System.EventHandler(this.Edit_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.deleteToolStripMenuItem.Text = global::HeliosDisplayManagement.Resources.Language.Delete;
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.Delete_Click);
            // 
            // lv_profiles
            // 
            this.lv_profiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lv_profiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lv_profiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lv_profiles.HideSelection = false;
            this.lv_profiles.LabelEdit = true;
            this.lv_profiles.LargeImageList = this.il_profiles;
            this.lv_profiles.Location = new System.Drawing.Point(-1, -1);
            this.lv_profiles.MultiSelect = false;
            this.lv_profiles.Name = "lv_profiles";
            this.lv_profiles.Size = new System.Drawing.Size(254, 475);
            this.lv_profiles.SmallImageList = this.il_profiles;
            this.lv_profiles.TabIndex = 11;
            this.lv_profiles.UseCompatibleStateImageBehavior = false;
            this.lv_profiles.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.lv_profiles_AfterLabelEdit);
            this.lv_profiles.BeforeLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.lv_profiles_BeforeLabelEdit);
            this.lv_profiles.SelectedIndexChanged += new System.EventHandler(this.lv_profiles_SelectedIndexChanged);
            this.lv_profiles.DoubleClick += new System.EventHandler(this.lv_profiles_DoubleClick);
            this.lv_profiles.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lv_profiles_MouseUp);
            // 
            // il_profiles
            // 
            this.il_profiles.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.il_profiles.ImageSize = new System.Drawing.Size(48, 48);
            this.il_profiles.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lbl_version
            // 
            this.lbl_version.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_version.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.lbl_version.Location = new System.Drawing.Point(650, 8);
            this.lbl_version.Name = "lbl_version";
            this.lbl_version.Size = new System.Drawing.Size(157, 13);
            this.lbl_version.TabIndex = 12;
            this.lbl_version.Text = "v{0}";
            this.lbl_version.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // MainForm
            // 
            this.AcceptButton = this.btn_apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(184)))), ((int)(((byte)(196)))));
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(819, 472);
            this.Controls.Add(this.lbl_version);
            this.Controls.Add(this.lv_profiles);
            this.Controls.Add(this.btn_shortcut);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.lbl_profile);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_edit);
            this.Controls.Add(this.btn_apply);
            this.Controls.Add(this.dv_profile);
            this.Controls.Add(this.btn_clone);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = global::HeliosDisplayManagement.Resources.Language.Helios_Display_Management;
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menu_profiles.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btn_clone;
        private DisplayView dv_profile;
        private System.Windows.Forms.Button btn_apply;
        private System.Windows.Forms.Button btn_edit;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Label lbl_profile;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.Button btn_shortcut;
        private System.Windows.Forms.ContextMenuStrip menu_profiles;
        private System.Windows.Forms.ToolStripMenuItem cloneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem createShortcutToolStripMenuItem;
        private System.Windows.Forms.ListView lv_profiles;
        private System.Windows.Forms.ImageList il_profiles;
        private System.Windows.Forms.Label lbl_version;
    }
}

