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
            this.btn_view_current = new System.Windows.Forms.Button();
            this.btn_save_or_rename = new System.Windows.Forms.Button();
            this.pb_down_arrow = new System.Windows.Forms.PictureBox();
            this.lbl_profile_shown = new System.Windows.Forms.Label();
            this.txt_profile_save_name = new System.Windows.Forms.TextBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.ilv_saved_profiles = new Manina.Windows.Forms.ImageListView();
            this.lbl_profile_shown_subtitle = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.menu_profiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).BeginInit();
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
            this.btn_apply.Location = new System.Drawing.Point(359, 750);
            this.btn_apply.Name = "btn_apply";
            this.btn_apply.Size = new System.Drawing.Size(120, 40);
            this.btn_apply.TabIndex = 2;
            this.btn_apply.Text = "&Apply";
            this.btn_apply.UseVisualStyleBackColor = false;
            this.btn_apply.Click += new System.EventHandler(this.Apply_Click);
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
            this.btn_delete.Location = new System.Drawing.Point(498, 750);
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
            this.dv_profile.Location = new System.Drawing.Point(0, 63);
            this.dv_profile.Margin = new System.Windows.Forms.Padding(18);
            this.dv_profile.Name = "dv_profile";
            this.dv_profile.PaddingX = 100;
            this.dv_profile.PaddingY = 100;
            this.dv_profile.Profile = null;
            this.dv_profile.Size = new System.Drawing.Size(976, 517);
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
            // 
            // createShortcutToolStripMenuItem
            // 
            this.createShortcutToolStripMenuItem.Name = "createShortcutToolStripMenuItem";
            this.createShortcutToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(67, 22);
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
            this.btn_view_current.Click += new System.EventHandler(this.btn_view_current_Click);
            // 
            // btn_save_or_rename
            // 
            this.btn_save_or_rename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_save_or_rename.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_save_or_rename.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_save_or_rename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_save_or_rename.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save_or_rename.ForeColor = System.Drawing.Color.White;
            this.btn_save_or_rename.Location = new System.Drawing.Point(221, 520);
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
            this.pb_down_arrow.Location = new System.Drawing.Point(461, 551);
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
            this.lbl_profile_shown.Location = new System.Drawing.Point(18, 73);
            this.lbl_profile_shown.Name = "lbl_profile_shown";
            this.lbl_profile_shown.Size = new System.Drawing.Size(205, 29);
            this.lbl_profile_shown.TabIndex = 19;
            this.lbl_profile_shown.Text = "My Display Profile";
            // 
            // txt_profile_save_name
            // 
            this.txt_profile_save_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_profile_save_name.Location = new System.Drawing.Point(372, 519);
            this.txt_profile_save_name.MaxLength = 200;
            this.txt_profile_save_name.Name = "txt_profile_save_name";
            this.txt_profile_save_name.Size = new System.Drawing.Size(384, 35);
            this.txt_profile_save_name.TabIndex = 20;
            this.txt_profile_save_name.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_profile_save_name_KeyDown);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // ilv_saved_profiles
            // 
            this.ilv_saved_profiles.AllowCheckBoxClick = false;
            this.ilv_saved_profiles.AllowColumnClick = false;
            this.ilv_saved_profiles.AllowColumnResize = false;
            this.ilv_saved_profiles.AllowItemReorder = false;
            this.ilv_saved_profiles.AllowPaneResize = false;
            this.ilv_saved_profiles.Colors = new Manina.Windows.Forms.ImageListViewColor(resources.GetString("ilv_saved_profiles.Colors"));
            this.ilv_saved_profiles.Location = new System.Drawing.Point(0, 601);
            this.ilv_saved_profiles.MultiSelect = false;
            this.ilv_saved_profiles.Name = "ilv_saved_profiles";
            this.ilv_saved_profiles.PersistentCacheDirectory = "";
            this.ilv_saved_profiles.PersistentCacheSize = ((long)(100));
            this.ilv_saved_profiles.Size = new System.Drawing.Size(976, 128);
            this.ilv_saved_profiles.TabIndex = 21;
            this.ilv_saved_profiles.UseWIC = true;
            this.ilv_saved_profiles.View = Manina.Windows.Forms.View.HorizontalStrip;
            this.ilv_saved_profiles.ItemClick += new Manina.Windows.Forms.ItemClickEventHandler(this.ilv_saved_profiles_ItemClick);
            // 
            // lbl_profile_shown_subtitle
            // 
            this.lbl_profile_shown_subtitle.AutoSize = true;
            this.lbl_profile_shown_subtitle.BackColor = System.Drawing.Color.DimGray;
            this.lbl_profile_shown_subtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_profile_shown_subtitle.ForeColor = System.Drawing.Color.White;
            this.lbl_profile_shown_subtitle.Location = new System.Drawing.Point(20, 102);
            this.lbl_profile_shown_subtitle.Name = "lbl_profile_shown_subtitle";
            this.lbl_profile_shown_subtitle.Size = new System.Drawing.Size(132, 20);
            this.lbl_profile_shown_subtitle.TabIndex = 22;
            this.lbl_profile_shown_subtitle.Text = "My Display Profile";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(263, 581);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(450, 20);
            this.label1.TabIndex = 23;
            this.label1.Text = "Saved Profiles (These can be used to create shortcuts)";
            // 
            // DisplayProfileForm
            // 
            this.AcceptButton = this.btn_apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.CancelButton = this.btn_back;
            this.ClientSize = new System.Drawing.Size(976, 812);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbl_profile_shown_subtitle);
            this.Controls.Add(this.ilv_saved_profiles);
            this.Controls.Add(this.txt_profile_save_name);
            this.Controls.Add(this.lbl_profile_shown);
            this.Controls.Add(this.btn_save_or_rename);
            this.Controls.Add(this.btn_view_current);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.btn_back);
            this.Controls.Add(this.btn_apply);
            this.Controls.Add(this.pb_down_arrow);
            this.Controls.Add(this.dv_profile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "DisplayProfileForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "HeliosPlus - Setup Display Profiles";
            this.Activated += new System.EventHandler(this.DisplayProfileForm_Activated);
            this.Load += new System.EventHandler(this.DisplayProfileForm_Load);
            this.menu_profiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DisplayView dv_profile;
        private System.Windows.Forms.Button btn_apply;
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
        private System.Windows.Forms.Button btn_view_current;
        private System.Windows.Forms.Button btn_save_or_rename;
        private System.Windows.Forms.PictureBox pb_down_arrow;
        private System.Windows.Forms.Label lbl_profile_shown;
        private System.Windows.Forms.TextBox txt_profile_save_name;
        private System.Windows.Forms.ImageList imageList1;
        private Manina.Windows.Forms.ImageListView ilv_saved_profiles;
        private System.Windows.Forms.Label lbl_profile_shown_subtitle;
        private System.Windows.Forms.Label label1;
    }
}

