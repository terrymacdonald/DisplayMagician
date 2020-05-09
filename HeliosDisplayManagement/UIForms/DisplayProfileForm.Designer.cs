using HeliosPlus.Resources;
using HeliosPlus.Shared.UserControls;

namespace HeliosPlus.UIForms
{
    partial class DisplayProfileForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DisplayProfileForm));
            this.btn_apply = new System.Windows.Forms.Button();
            this.btn_edit = new System.Windows.Forms.Button();
            this.btn_back = new System.Windows.Forms.Button();
            this.btn_delete = new System.Windows.Forms.Button();
            this.dv_profile = new HeliosPlus.Shared.UserControls.DisplayView();
            this.menu_profiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.applyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.cloneToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createShortcutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.il_profiles = new System.Windows.Forms.ImageList(this.components);
            this.olv_profiles = new BrightIdeasSoftware.ObjectListView();
            this.olvImageColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.olvNameColumn = ((BrightIdeasSoftware.OLVColumn)(new BrightIdeasSoftware.OLVColumn()));
            this.btn_view_current = new System.Windows.Forms.Button();
            this.btn_save_or_rename = new System.Windows.Forms.Button();
            this.pb_down_arrow = new System.Windows.Forms.PictureBox();
            this.lbl_profile_shown = new System.Windows.Forms.Label();
            this.txt_profile_save_name = new System.Windows.Forms.TextBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.menu_profiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.olv_profiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.SuspendLayout();
            // 
            // btn_apply
            // 
            this.btn_apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_apply.BackColor = System.Drawing.Color.Black;
            this.btn_apply.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_apply.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_apply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_apply.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_apply.ForeColor = System.Drawing.Color.White;
            this.btn_apply.Location = new System.Drawing.Point(359, 734);
            this.btn_apply.Name = "btn_apply";
            this.btn_apply.Size = new System.Drawing.Size(120, 40);
            this.btn_apply.TabIndex = 2;
            this.btn_apply.Text = "&Apply";
            this.btn_apply.UseVisualStyleBackColor = false;
            this.btn_apply.Click += new System.EventHandler(this.Apply_Click);
            // 
            // btn_edit
            // 
            this.btn_edit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_edit.BackColor = System.Drawing.Color.Black;
            this.btn_edit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_edit.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_edit.ForeColor = System.Drawing.Color.White;
            this.btn_edit.Location = new System.Drawing.Point(12, 734);
            this.btn_edit.Name = "btn_edit";
            this.btn_edit.Size = new System.Drawing.Size(120, 40);
            this.btn_edit.TabIndex = 8;
            this.btn_edit.Text = "&Edit";
            this.btn_edit.UseVisualStyleBackColor = false;
            this.btn_edit.Visible = false;
            this.btn_edit.Click += new System.EventHandler(this.Edit_Click);
            // 
            // btn_back
            // 
            this.btn_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_back.ForeColor = System.Drawing.Color.White;
            this.btn_back.Location = new System.Drawing.Point(889, 777);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 5;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.Exit_Click);
            // 
            // btn_delete
            // 
            this.btn_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_delete.ForeColor = System.Drawing.Color.White;
            this.btn_delete.Location = new System.Drawing.Point(498, 734);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(120, 40);
            this.btn_delete.TabIndex = 4;
            this.btn_delete.Text = "&Delete";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // dv_profile
            // 
            this.dv_profile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dv_profile.BackColor = System.Drawing.Color.DimGray;
            this.dv_profile.Font = new System.Drawing.Font("Consolas", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            this.dv_profile.Location = new System.Drawing.Point(0, 1);
            this.dv_profile.Margin = new System.Windows.Forms.Padding(18);
            this.dv_profile.Name = "dv_profile";
            this.dv_profile.PaddingX = 100;
            this.dv_profile.PaddingY = 100;
            this.dv_profile.Profile = null;
            this.dv_profile.Size = new System.Drawing.Size(974, 602);
            this.dv_profile.TabIndex = 4;
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
            this.menu_profiles.Size = new System.Drawing.Size(68, 120);
            // 
            // applyToolStripMenuItem
            // 
            this.applyToolStripMenuItem.Name = "applyToolStripMenuItem";
            this.applyToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            this.applyToolStripMenuItem.Click += new System.EventHandler(this.Apply_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(64, 6);
            // 
            // cloneToolStripMenuItem
            // 
            this.cloneToolStripMenuItem.Name = "cloneToolStripMenuItem";
            this.cloneToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            this.cloneToolStripMenuItem.Click += new System.EventHandler(this.Copy_Click);
            // 
            // createShortcutToolStripMenuItem
            // 
            this.createShortcutToolStripMenuItem.Name = "createShortcutToolStripMenuItem";
            this.createShortcutToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            this.createShortcutToolStripMenuItem.Click += new System.EventHandler(this.CreateShortcut_Click);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            this.editToolStripMenuItem.Click += new System.EventHandler(this.Edit_Click);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.Delete_Click);
            // 
            // il_profiles
            // 
            this.il_profiles.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.il_profiles.ImageSize = new System.Drawing.Size(64, 64);
            this.il_profiles.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // olv_profiles
            // 
            this.olv_profiles.AllColumns.Add(this.olvImageColumn);
            this.olv_profiles.AllColumns.Add(this.olvNameColumn);
            this.olv_profiles.CellEditActivation = BrightIdeasSoftware.ObjectListView.CellEditActivateMode.SingleClick;
            this.olv_profiles.CellEditUseWholeCell = false;
            this.olv_profiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.olvImageColumn,
            this.olvNameColumn});
            this.olv_profiles.ContextMenuStrip = this.menu_profiles;
            this.olv_profiles.Cursor = System.Windows.Forms.Cursors.Default;
            this.olv_profiles.GroupImageList = this.il_profiles;
            this.olv_profiles.HasCollapsibleGroups = false;
            this.olv_profiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.olv_profiles.HideSelection = false;
            this.olv_profiles.LargeImageList = this.il_profiles;
            this.olv_profiles.Location = new System.Drawing.Point(1, 601);
            this.olv_profiles.Name = "olv_profiles";
            this.olv_profiles.OwnerDraw = false;
            this.olv_profiles.ShowGroups = false;
            this.olv_profiles.Size = new System.Drawing.Size(974, 125);
            this.olv_profiles.SmallImageList = this.il_profiles;
            this.olv_profiles.TabIndex = 13;
            this.olv_profiles.TileSize = new System.Drawing.Size(128, 128);
            this.olv_profiles.UseCompatibleStateImageBehavior = false;
            this.olv_profiles.View = System.Windows.Forms.View.Tile;
            this.olv_profiles.SelectedIndexChanged += new System.EventHandler(this.olv_profiles_SelectedIndexChanged);
            // 
            // olvImageColumn
            // 
            this.olvImageColumn.AspectName = "";
            this.olvImageColumn.ImageAspectName = "ProfileBitmap";
            this.olvImageColumn.Width = 256;
            // 
            // olvNameColumn
            // 
            this.olvNameColumn.AspectName = "Name";
            this.olvNameColumn.Groupable = false;
            this.olvNameColumn.ShowTextInHeader = false;
            this.olvNameColumn.Width = 256;
            // 
            // btn_view_current
            // 
            this.btn_view_current.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_view_current.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_view_current.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_view_current.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_view_current.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_view_current.ForeColor = System.Drawing.Color.White;
            this.btn_view_current.Location = new System.Drawing.Point(796, 12);
            this.btn_view_current.Name = "btn_view_current";
            this.btn_view_current.Size = new System.Drawing.Size(168, 39);
            this.btn_view_current.TabIndex = 6;
            this.btn_view_current.Text = "View &Current";
            this.btn_view_current.UseVisualStyleBackColor = true;
            // 
            // btn_save_or_rename
            // 
            this.btn_save_or_rename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_save_or_rename.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_save_or_rename.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_save_or_rename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_save_or_rename.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save_or_rename.ForeColor = System.Drawing.Color.White;
            this.btn_save_or_rename.Location = new System.Drawing.Point(221, 545);
            this.btn_save_or_rename.Name = "btn_save_or_rename";
            this.btn_save_or_rename.Size = new System.Drawing.Size(151, 33);
            this.btn_save_or_rename.TabIndex = 1;
            this.btn_save_or_rename.Text = "&Save As";
            this.btn_save_or_rename.UseVisualStyleBackColor = true;
            this.btn_save_or_rename.Click += new System.EventHandler(this.btn_save_as_Click);
            // 
            // pb_down_arrow
            // 
            this.pb_down_arrow.BackColor = System.Drawing.Color.DimGray;
            this.pb_down_arrow.Image = ((System.Drawing.Image)(resources.GetObject("pb_down_arrow.Image")));
            this.pb_down_arrow.Location = new System.Drawing.Point(461, 576);
            this.pb_down_arrow.Name = "pb_down_arrow";
            this.pb_down_arrow.Size = new System.Drawing.Size(54, 27);
            this.pb_down_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_down_arrow.TabIndex = 18;
            this.pb_down_arrow.TabStop = false;
            // 
            // lbl_profile_shown
            // 
            this.lbl_profile_shown.AutoSize = true;
            this.lbl_profile_shown.BackColor = System.Drawing.Color.DimGray;
            this.lbl_profile_shown.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_profile_shown.ForeColor = System.Drawing.Color.White;
            this.lbl_profile_shown.Location = new System.Drawing.Point(21, 16);
            this.lbl_profile_shown.Name = "lbl_profile_shown";
            this.lbl_profile_shown.Size = new System.Drawing.Size(205, 29);
            this.lbl_profile_shown.TabIndex = 19;
            this.lbl_profile_shown.Text = "My Display Profile";
            // 
            // txt_profile_save_name
            // 
            this.txt_profile_save_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_profile_save_name.Location = new System.Drawing.Point(372, 544);
            this.txt_profile_save_name.MaxLength = 200;
            this.txt_profile_save_name.Name = "txt_profile_save_name";
            this.txt_profile_save_name.Size = new System.Drawing.Size(384, 35);
            this.txt_profile_save_name.TabIndex = 20;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // DisplayProfileForm
            // 
            this.AcceptButton = this.btn_apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.CancelButton = this.btn_back;
            this.ClientSize = new System.Drawing.Size(976, 812);
            this.Controls.Add(this.txt_profile_save_name);
            this.Controls.Add(this.lbl_profile_shown);
            this.Controls.Add(this.btn_save_or_rename);
            this.Controls.Add(this.btn_view_current);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.btn_back);
            this.Controls.Add(this.btn_edit);
            this.Controls.Add(this.btn_apply);
            this.Controls.Add(this.olv_profiles);
            this.Controls.Add(this.pb_down_arrow);
            this.Controls.Add(this.dv_profile);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "DisplayProfileForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HeliosPlus - Setup Display Profiles";
            this.TopMost = true;
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menu_profiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.olv_profiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DisplayView dv_profile;
        private System.Windows.Forms.Button btn_apply;
        private System.Windows.Forms.Button btn_edit;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.ContextMenuStrip menu_profiles;
        private System.Windows.Forms.ToolStripMenuItem cloneToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem createShortcutToolStripMenuItem;
        private System.Windows.Forms.ImageList il_profiles;
        private BrightIdeasSoftware.ObjectListView olv_profiles;
        private BrightIdeasSoftware.OLVColumn olvNameColumn;
        private BrightIdeasSoftware.OLVColumn olvImageColumn;
        private System.Windows.Forms.Button btn_view_current;
        private System.Windows.Forms.Button btn_save_or_rename;
        private System.Windows.Forms.PictureBox pb_down_arrow;
        private System.Windows.Forms.Label lbl_profile_shown;
        private System.Windows.Forms.TextBox txt_profile_save_name;
        private System.Windows.Forms.ErrorProvider errorProvider1;
    }
}

