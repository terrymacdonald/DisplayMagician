using DisplayMagician.Resources;
using DisplayMagicianShared.UserControls;

namespace DisplayMagician.UIForms
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
            this.cms_profiles = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.applyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProfileToDesktopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sendToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteProfileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.il_profiles = new System.Windows.Forms.ImageList(this.components);
            this.btn_view_current = new System.Windows.Forms.Button();
            this.btn_save_or_rename = new System.Windows.Forms.Button();
            this.pb_down_arrow = new System.Windows.Forms.PictureBox();
            this.lbl_profile_shown = new System.Windows.Forms.Label();
            this.txt_profile_save_name = new System.Windows.Forms.TextBox();
            this.ilv_saved_profiles = new Manina.Windows.Forms.ImageListView();
            this.lbl_profile_shown_subtitle = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tt_selected = new System.Windows.Forms.ToolTip(this.components);
            this.lbl_save_profile = new System.Windows.Forms.Label();
            this.btn_save = new System.Windows.Forms.Button();
            this.dialog_save = new System.Windows.Forms.SaveFileDialog();
            this.btn_hotkey = new System.Windows.Forms.Button();
            this.lbl_hotkey_assigned = new System.Windows.Forms.Label();
            this.dv_profile = new DisplayMagicianShared.UserControls.DisplayView();
            this.p_upper = new System.Windows.Forms.Panel();
            this.btn_donate = new System.Windows.Forms.Button();
            this.btn_help = new System.Windows.Forms.Button();
            this.btn_profile_settings = new System.Windows.Forms.Button();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.p_lower = new System.Windows.Forms.Panel();
            this.p_fill = new System.Windows.Forms.Panel();
            this.cms_profiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).BeginInit();
            this.p_upper.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.p_lower.SuspendLayout();
            this.p_fill.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_apply
            // 
            this.btn_apply.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_apply.BackColor = System.Drawing.Color.Black;
            this.btn_apply.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_apply.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_apply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_apply.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_apply.ForeColor = System.Drawing.Color.White;
            this.btn_apply.Location = new System.Drawing.Point(300, 23);
            this.btn_apply.Name = "btn_apply";
            this.btn_apply.Size = new System.Drawing.Size(118, 40);
            this.btn_apply.TabIndex = 2;
            this.btn_apply.Text = "&Apply";
            this.btn_apply.UseVisualStyleBackColor = false;
            this.btn_apply.Click += new System.EventHandler(this.Apply_Click);
            // 
            // btn_back
            // 
            this.btn_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_back.ForeColor = System.Drawing.Color.White;
            this.btn_back.Location = new System.Drawing.Point(847, 48);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 5;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.Exit_Click);
            // 
            // btn_delete
            // 
            this.btn_delete.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_delete.ForeColor = System.Drawing.Color.White;
            this.btn_delete.Location = new System.Drawing.Point(424, 23);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(120, 40);
            this.btn_delete.TabIndex = 4;
            this.btn_delete.Text = "&Delete";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // cms_profiles
            // 
            this.cms_profiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applyToolStripMenuItem,
            this.saveProfileToDesktopToolStripMenuItem,
            this.sendToClipboardToolStripMenuItem,
            this.deleteProfileToolStripMenuItem});
            this.cms_profiles.Name = "menu_profiles";
            this.cms_profiles.Size = new System.Drawing.Size(205, 92);
            // 
            // applyToolStripMenuItem
            // 
            this.applyToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.applyToolStripMenuItem.Name = "applyToolStripMenuItem";
            this.applyToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.applyToolStripMenuItem.Text = "Apply Profile...";
            this.applyToolStripMenuItem.Click += new System.EventHandler(this.applyToolStripMenuItem_Click);
            // 
            // saveProfileToDesktopToolStripMenuItem
            // 
            this.saveProfileToDesktopToolStripMenuItem.Name = "saveProfileToDesktopToolStripMenuItem";
            this.saveProfileToDesktopToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.saveProfileToDesktopToolStripMenuItem.Text = "Save Profile to Desktop...";
            this.saveProfileToDesktopToolStripMenuItem.Click += new System.EventHandler(this.saveProfileToDesktopToolStripMenuItem_Click);
            // 
            // sendToClipboardToolStripMenuItem
            // 
            this.sendToClipboardToolStripMenuItem.Name = "sendToClipboardToolStripMenuItem";
            this.sendToClipboardToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.sendToClipboardToolStripMenuItem.Text = "Send to Clipboard...";
            this.sendToClipboardToolStripMenuItem.Click += new System.EventHandler(this.sendToClipboardToolStripMenuItem_Click);
            // 
            // deleteProfileToolStripMenuItem
            // 
            this.deleteProfileToolStripMenuItem.Name = "deleteProfileToolStripMenuItem";
            this.deleteProfileToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            this.deleteProfileToolStripMenuItem.Text = "Delete Profile...";
            this.deleteProfileToolStripMenuItem.Click += new System.EventHandler(this.deleteProfileToolStripMenuItem_Click);
            // 
            // il_profiles
            // 
            this.il_profiles.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.il_profiles.ImageSize = new System.Drawing.Size(64, 64);
            this.il_profiles.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // btn_view_current
            // 
            this.btn_view_current.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_view_current.BackColor = System.Drawing.Color.Black;
            this.btn_view_current.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_view_current.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_view_current.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_view_current.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_view_current.ForeColor = System.Drawing.Color.White;
            this.btn_view_current.Location = new System.Drawing.Point(655, 18);
            this.btn_view_current.Name = "btn_view_current";
            this.btn_view_current.Size = new System.Drawing.Size(267, 39);
            this.btn_view_current.TabIndex = 6;
            this.btn_view_current.Text = "View &Current Display";
            this.btn_view_current.UseVisualStyleBackColor = false;
            this.btn_view_current.Click += new System.EventHandler(this.btn_view_current_Click);
            // 
            // btn_save_or_rename
            // 
            this.btn_save_or_rename.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_save_or_rename.BackColor = System.Drawing.Color.Black;
            this.btn_save_or_rename.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_save_or_rename.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_save_or_rename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_save_or_rename.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save_or_rename.ForeColor = System.Drawing.Color.White;
            this.btn_save_or_rename.Location = new System.Drawing.Point(120, 438);
            this.btn_save_or_rename.Name = "btn_save_or_rename";
            this.btn_save_or_rename.Size = new System.Drawing.Size(151, 33);
            this.btn_save_or_rename.TabIndex = 1;
            this.btn_save_or_rename.Text = "&Save";
            this.btn_save_or_rename.UseVisualStyleBackColor = false;
            this.btn_save_or_rename.Click += new System.EventHandler(this.btn_save_as_Click);
            // 
            // pb_down_arrow
            // 
            this.pb_down_arrow.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.pb_down_arrow.BackColor = System.Drawing.Color.DimGray;
            this.pb_down_arrow.Image = ((System.Drawing.Image)(resources.GetObject("pb_down_arrow.Image")));
            this.pb_down_arrow.Location = new System.Drawing.Point(440, 467);
            this.pb_down_arrow.Name = "pb_down_arrow";
            this.pb_down_arrow.Size = new System.Drawing.Size(54, 27);
            this.pb_down_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_down_arrow.TabIndex = 18;
            this.pb_down_arrow.TabStop = false;
            // 
            // lbl_profile_shown
            // 
            this.lbl_profile_shown.AutoSize = true;
            this.lbl_profile_shown.BackColor = System.Drawing.Color.Black;
            this.lbl_profile_shown.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_profile_shown.ForeColor = System.Drawing.Color.White;
            this.lbl_profile_shown.Location = new System.Drawing.Point(19, 22);
            this.lbl_profile_shown.Name = "lbl_profile_shown";
            this.lbl_profile_shown.Size = new System.Drawing.Size(205, 29);
            this.lbl_profile_shown.TabIndex = 19;
            this.lbl_profile_shown.Text = "My Display Profile";
            // 
            // txt_profile_save_name
            // 
            this.txt_profile_save_name.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txt_profile_save_name.BackColor = System.Drawing.Color.White;
            this.txt_profile_save_name.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txt_profile_save_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_profile_save_name.Location = new System.Drawing.Point(271, 437);
            this.txt_profile_save_name.MaxLength = 200;
            this.txt_profile_save_name.Name = "txt_profile_save_name";
            this.txt_profile_save_name.Size = new System.Drawing.Size(384, 35);
            this.txt_profile_save_name.TabIndex = 20;
            this.txt_profile_save_name.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txt_profile_save_name_KeyDown);
            // 
            // ilv_saved_profiles
            // 
            this.ilv_saved_profiles.AllowCheckBoxClick = false;
            this.ilv_saved_profiles.AllowColumnClick = false;
            this.ilv_saved_profiles.AllowColumnResize = false;
            this.ilv_saved_profiles.AllowItemReorder = false;
            this.ilv_saved_profiles.AllowPaneResize = false;
            this.ilv_saved_profiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ilv_saved_profiles.Location = new System.Drawing.Point(0, 0);
            this.ilv_saved_profiles.MultiSelect = false;
            this.ilv_saved_profiles.Name = "ilv_saved_profiles";
            this.ilv_saved_profiles.PersistentCacheDirectory = "";
            this.ilv_saved_profiles.PersistentCacheSize = ((long)(100));
            this.ilv_saved_profiles.Size = new System.Drawing.Size(934, 161);
            this.ilv_saved_profiles.TabIndex = 21;
            this.ilv_saved_profiles.UseWIC = true;
            this.ilv_saved_profiles.View = Manina.Windows.Forms.View.HorizontalStrip;
            this.ilv_saved_profiles.ItemClick += new Manina.Windows.Forms.ItemClickEventHandler(this.ilv_saved_profiles_ItemClick);
            this.ilv_saved_profiles.ItemHover += new Manina.Windows.Forms.ItemHoverEventHandler(this.ilv_saved_profiles_ItemHover);
            this.ilv_saved_profiles.ItemDoubleClick += new Manina.Windows.Forms.ItemDoubleClickEventHandler(this.ilv_saved_profiles_ItemDoubleClick);
            // 
            // lbl_profile_shown_subtitle
            // 
            this.lbl_profile_shown_subtitle.AutoSize = true;
            this.lbl_profile_shown_subtitle.BackColor = System.Drawing.Color.Black;
            this.lbl_profile_shown_subtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_profile_shown_subtitle.ForeColor = System.Drawing.Color.White;
            this.lbl_profile_shown_subtitle.Location = new System.Drawing.Point(21, 51);
            this.lbl_profile_shown_subtitle.Name = "lbl_profile_shown_subtitle";
            this.lbl_profile_shown_subtitle.Size = new System.Drawing.Size(132, 20);
            this.lbl_profile_shown_subtitle.TabIndex = 22;
            this.lbl_profile_shown_subtitle.Text = "My Display Profile";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Black;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(242, 497);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(450, 20);
            this.label1.TabIndex = 23;
            this.label1.Text = "Saved Profiles (These can be used to create shortcuts)";
            // 
            // lbl_save_profile
            // 
            this.lbl_save_profile.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbl_save_profile.BackColor = System.Drawing.Color.Firebrick;
            this.lbl_save_profile.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbl_save_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lbl_save_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lbl_save_profile.ForeColor = System.Drawing.Color.White;
            this.lbl_save_profile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lbl_save_profile.Location = new System.Drawing.Point(24, 80);
            this.lbl_save_profile.Name = "lbl_save_profile";
            this.lbl_save_profile.Size = new System.Drawing.Size(625, 88);
            this.lbl_save_profile.TabIndex = 33;
            this.lbl_save_profile.Text = resources.GetString("lbl_save_profile.Text");
            this.lbl_save_profile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
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
            this.btn_save.Location = new System.Drawing.Point(550, 23);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(211, 40);
            this.btn_save.TabIndex = 34;
            this.btn_save.Text = "Save to Desk&top";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.Save_Click);
            // 
            // dialog_save
            // 
            this.dialog_save.DefaultExt = "lnk";
            this.dialog_save.DereferenceLinks = false;
            this.dialog_save.Filter = global::DisplayMagician.Resources.Language.Shortcuts_Filter;
            this.dialog_save.RestoreDirectory = true;
            // 
            // btn_hotkey
            // 
            this.btn_hotkey.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_hotkey.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_hotkey.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_hotkey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_hotkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_hotkey.ForeColor = System.Drawing.Color.White;
            this.btn_hotkey.Location = new System.Drawing.Point(174, 23);
            this.btn_hotkey.Name = "btn_hotkey";
            this.btn_hotkey.Size = new System.Drawing.Size(120, 40);
            this.btn_hotkey.TabIndex = 35;
            this.btn_hotkey.Text = "&Hotkey";
            this.btn_hotkey.UseVisualStyleBackColor = true;
            this.btn_hotkey.Click += new System.EventHandler(this.btn_hotkey_Click);
            // 
            // lbl_hotkey_assigned
            // 
            this.lbl_hotkey_assigned.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lbl_hotkey_assigned.AutoSize = true;
            this.lbl_hotkey_assigned.BackColor = System.Drawing.Color.Transparent;
            this.lbl_hotkey_assigned.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_hotkey_assigned.ForeColor = System.Drawing.Color.White;
            this.lbl_hotkey_assigned.Location = new System.Drawing.Point(21, 39);
            this.lbl_hotkey_assigned.Name = "lbl_hotkey_assigned";
            this.lbl_hotkey_assigned.Size = new System.Drawing.Size(57, 16);
            this.lbl_hotkey_assigned.TabIndex = 36;
            this.lbl_hotkey_assigned.Text = "Hotkey: ";
            this.lbl_hotkey_assigned.Visible = false;
            this.lbl_hotkey_assigned.Click += new System.EventHandler(this.lbl_hotkey_assigned_Click);
            // 
            // dv_profile
            // 
            this.dv_profile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dv_profile.BackColor = System.Drawing.Color.DimGray;
            this.dv_profile.Font = new System.Drawing.Font("Consolas", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            this.dv_profile.Location = new System.Drawing.Point(0, 95);
            this.dv_profile.Margin = new System.Windows.Forms.Padding(18);
            this.dv_profile.Name = "dv_profile";
            this.dv_profile.PaddingX = 100;
            this.dv_profile.PaddingY = 100;
            this.dv_profile.Profile = null;
            this.dv_profile.Size = new System.Drawing.Size(934, 399);
            this.dv_profile.TabIndex = 4;
            // 
            // p_upper
            // 
            this.p_upper.BackColor = System.Drawing.Color.DimGray;
            this.p_upper.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("p_upper.BackgroundImage")));
            this.p_upper.Controls.Add(this.btn_donate);
            this.p_upper.Controls.Add(this.btn_help);
            this.p_upper.Controls.Add(this.txt_profile_save_name);
            this.p_upper.Controls.Add(this.lbl_save_profile);
            this.p_upper.Controls.Add(this.btn_profile_settings);
            this.p_upper.Controls.Add(this.pb_down_arrow);
            this.p_upper.Controls.Add(this.pbLogo);
            this.p_upper.Controls.Add(this.btn_view_current);
            this.p_upper.Controls.Add(this.btn_save_or_rename);
            this.p_upper.Controls.Add(this.label1);
            this.p_upper.Controls.Add(this.lbl_profile_shown);
            this.p_upper.Controls.Add(this.lbl_profile_shown_subtitle);
            this.p_upper.Controls.Add(this.dv_profile);
            this.p_upper.Dock = System.Windows.Forms.DockStyle.Top;
            this.p_upper.Location = new System.Drawing.Point(0, 0);
            this.p_upper.Name = "p_upper";
            this.p_upper.Size = new System.Drawing.Size(934, 517);
            this.p_upper.TabIndex = 37;
            // 
            // btn_donate
            // 
            this.btn_donate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_donate.BackColor = System.Drawing.Color.Black;
            this.btn_donate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_donate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_donate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_donate.ForeColor = System.Drawing.Color.White;
            this.btn_donate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_donate.Location = new System.Drawing.Point(850, 63);
            this.btn_donate.Name = "btn_donate";
            this.btn_donate.Size = new System.Drawing.Size(72, 23);
            this.btn_donate.TabIndex = 42;
            this.btn_donate.Text = "D&onate";
            this.btn_donate.UseVisualStyleBackColor = false;
            this.btn_donate.Click += new System.EventHandler(this.btn_donate_Click);
            // 
            // btn_help
            // 
            this.btn_help.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_help.BackColor = System.Drawing.Color.Black;
            this.btn_help.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_help.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_help.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_help.ForeColor = System.Drawing.Color.White;
            this.btn_help.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_help.Location = new System.Drawing.Point(772, 63);
            this.btn_help.Name = "btn_help";
            this.btn_help.Size = new System.Drawing.Size(72, 23);
            this.btn_help.TabIndex = 39;
            this.btn_help.Text = "Hel&p";
            this.btn_help.UseVisualStyleBackColor = false;
            this.btn_help.Click += new System.EventHandler(this.btn_help_Click);
            // 
            // btn_profile_settings
            // 
            this.btn_profile_settings.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_profile_settings.BackColor = System.Drawing.Color.Black;
            this.btn_profile_settings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_profile_settings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_profile_settings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_profile_settings.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_profile_settings.ForeColor = System.Drawing.Color.White;
            this.btn_profile_settings.Location = new System.Drawing.Point(655, 438);
            this.btn_profile_settings.Name = "btn_profile_settings";
            this.btn_profile_settings.Size = new System.Drawing.Size(170, 33);
            this.btn_profile_settings.TabIndex = 38;
            this.btn_profile_settings.Text = "&Profile Settings";
            this.btn_profile_settings.UseVisualStyleBackColor = false;
            this.btn_profile_settings.Click += new System.EventHandler(this.btn_profile_settings_Click);
            // 
            // pbLogo
            // 
            this.pbLogo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbLogo.Location = new System.Drawing.Point(822, 111);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(100, 49);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLogo.TabIndex = 0;
            this.pbLogo.TabStop = false;
            // 
            // p_lower
            // 
            this.p_lower.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("p_lower.BackgroundImage")));
            this.p_lower.Controls.Add(this.lbl_hotkey_assigned);
            this.p_lower.Controls.Add(this.btn_hotkey);
            this.p_lower.Controls.Add(this.btn_delete);
            this.p_lower.Controls.Add(this.btn_apply);
            this.p_lower.Controls.Add(this.btn_save);
            this.p_lower.Controls.Add(this.btn_back);
            this.p_lower.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.p_lower.Location = new System.Drawing.Point(0, 678);
            this.p_lower.Name = "p_lower";
            this.p_lower.Size = new System.Drawing.Size(934, 83);
            this.p_lower.TabIndex = 38;
            // 
            // p_fill
            // 
            this.p_fill.BackColor = System.Drawing.Color.White;
            this.p_fill.Controls.Add(this.ilv_saved_profiles);
            this.p_fill.Dock = System.Windows.Forms.DockStyle.Fill;
            this.p_fill.Location = new System.Drawing.Point(0, 517);
            this.p_fill.Name = "p_fill";
            this.p_fill.Size = new System.Drawing.Size(934, 161);
            this.p_fill.TabIndex = 39;
            // 
            // DisplayProfileForm
            // 
            this.AcceptButton = this.btn_apply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.CancelButton = this.btn_back;
            this.ClientSize = new System.Drawing.Size(934, 761);
            this.Controls.Add(this.p_fill);
            this.Controls.Add(this.p_lower);
            this.Controls.Add(this.p_upper);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(950, 800);
            this.Name = "DisplayProfileForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DisplayMagician - Display Profiles";
            this.Load += new System.EventHandler(this.DisplayProfileForm_Load);
            this.cms_profiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).EndInit();
            this.p_upper.ResumeLayout(false);
            this.p_upper.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.p_lower.ResumeLayout(false);
            this.p_lower.PerformLayout();
            this.p_fill.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private DisplayView dv_profile;
        private System.Windows.Forms.Button btn_apply;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.ContextMenuStrip cms_profiles;
        private System.Windows.Forms.ToolStripMenuItem sendToClipboardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem applyToolStripMenuItem;
        private System.Windows.Forms.ImageList il_profiles;
        private System.Windows.Forms.Button btn_view_current;
        private System.Windows.Forms.Button btn_save_or_rename;
        private System.Windows.Forms.PictureBox pb_down_arrow;
        private System.Windows.Forms.Label lbl_profile_shown;
        private System.Windows.Forms.TextBox txt_profile_save_name;
        private Manina.Windows.Forms.ImageListView ilv_saved_profiles;
        private System.Windows.Forms.Label lbl_profile_shown_subtitle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip tt_selected;
        private System.Windows.Forms.Label lbl_save_profile;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.SaveFileDialog dialog_save;
        private System.Windows.Forms.Button btn_hotkey;
        private System.Windows.Forms.Label lbl_hotkey_assigned;
        private System.Windows.Forms.Panel p_upper;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Button btn_profile_settings;
        private System.Windows.Forms.Panel p_lower;
        private System.Windows.Forms.Panel p_fill;
        private System.Windows.Forms.Button btn_help;
        private System.Windows.Forms.ToolStripMenuItem deleteProfileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveProfileToDesktopToolStripMenuItem;
        private System.Windows.Forms.Button btn_donate;
    }
}

