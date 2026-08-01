//using DisplayMagician.Resources;
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DisplayProfileForm));
            btn_apply = new System.Windows.Forms.Button();
            btn_back = new System.Windows.Forms.Button();
            btn_delete = new System.Windows.Forms.Button();
            cms_profiles = new System.Windows.Forms.ContextMenuStrip(components);
            applyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveProfileToDesktopToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            sendToClipboardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            deleteProfileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            il_profiles = new System.Windows.Forms.ImageList(components);
            btn_view_current = new System.Windows.Forms.Button();
            btn_save_or_rename = new System.Windows.Forms.Button();
            pb_down_arrow = new System.Windows.Forms.PictureBox();
            lbl_profile_shown = new System.Windows.Forms.Label();
            txt_profile_save_name = new System.Windows.Forms.TextBox();
            ilv_saved_profiles = new Manina.Windows.Forms.ImageListView();
            lbl_profile_shown_subtitle = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            tt_selected = new System.Windows.Forms.ToolTip(components);
            lbl_save_profile = new System.Windows.Forms.Label();
            btn_save = new System.Windows.Forms.Button();
            dialog_save = new System.Windows.Forms.SaveFileDialog();
            btn_hotkey = new System.Windows.Forms.Button();
            lbl_hotkey_assigned = new System.Windows.Forms.Label();
            p_upper = new System.Windows.Forms.Panel();
            tlp_upper = new System.Windows.Forms.TableLayoutPanel();
            btn_donate = new System.Windows.Forms.Button();
            btn_help = new System.Windows.Forms.Button();
            btn_profile_settings = new System.Windows.Forms.Button();
            p_lower = new System.Windows.Forms.Panel();
            btn_update = new System.Windows.Forms.Button();
            p_fill = new System.Windows.Forms.Panel();
            dv_profile = new DisplayView();
            flp_header_buttons = new System.Windows.Forms.FlowLayoutPanel();
            flp_save = new System.Windows.Forms.FlowLayoutPanel();
            tlp_lower = new System.Windows.Forms.TableLayoutPanel();
            cms_profiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_down_arrow).BeginInit();
            p_upper.SuspendLayout();
            p_lower.SuspendLayout();
            p_fill.SuspendLayout();
            SuspendLayout();
            // 
            // btn_apply
            // 
            btn_apply.BackColor = System.Drawing.Color.Black;
            btn_apply.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_apply.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_apply.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_apply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_apply.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_apply.ForeColor = System.Drawing.Color.White;
            btn_apply.Location = new System.Drawing.Point(3, 3);
            btn_apply.Name = "btn_apply";
            btn_apply.Size = new System.Drawing.Size(118, 54);
            btn_apply.TabIndex = 0;
            btn_apply.Text = "&Apply";
            btn_apply.UseVisualStyleBackColor = false;
            btn_apply.Click += Apply_Click;
            // 
            // btn_back
            // 
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(743, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(84, 54);
            btn_back.TabIndex = 6;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += Exit_Click;
            // 
            // btn_delete
            // 
            btn_delete.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_delete.ForeColor = System.Drawing.Color.White;
            btn_delete.Location = new System.Drawing.Point(255, 3);
            btn_delete.Name = "btn_delete";
            btn_delete.Size = new System.Drawing.Size(120, 54);
            btn_delete.TabIndex = 2;
            btn_delete.Text = "&Delete";
            btn_delete.UseVisualStyleBackColor = true;
            btn_delete.Click += Delete_Click;
            // 
            // cms_profiles
            // 
            cms_profiles.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { applyToolStripMenuItem, saveProfileToDesktopToolStripMenuItem, sendToClipboardToolStripMenuItem, deleteProfileToolStripMenuItem });
            cms_profiles.Name = "menu_profiles";
            cms_profiles.Size = new System.Drawing.Size(205, 92);
            // 
            // applyToolStripMenuItem
            // 
            applyToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            applyToolStripMenuItem.Name = "applyToolStripMenuItem";
            applyToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            applyToolStripMenuItem.Text = "Apply Profile...";
            applyToolStripMenuItem.Click += applyToolStripMenuItem_Click;
            // 
            // saveProfileToDesktopToolStripMenuItem
            // 
            saveProfileToDesktopToolStripMenuItem.Name = "saveProfileToDesktopToolStripMenuItem";
            saveProfileToDesktopToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            saveProfileToDesktopToolStripMenuItem.Text = "Save Profile to Desktop...";
            saveProfileToDesktopToolStripMenuItem.Click += saveProfileToDesktopToolStripMenuItem_Click;
            // 
            // sendToClipboardToolStripMenuItem
            // 
            sendToClipboardToolStripMenuItem.Name = "sendToClipboardToolStripMenuItem";
            sendToClipboardToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            sendToClipboardToolStripMenuItem.Text = "Send to Clipboard...";
            sendToClipboardToolStripMenuItem.Click += sendToClipboardToolStripMenuItem_Click;
            // 
            // deleteProfileToolStripMenuItem
            // 
            deleteProfileToolStripMenuItem.Name = "deleteProfileToolStripMenuItem";
            deleteProfileToolStripMenuItem.Size = new System.Drawing.Size(204, 22);
            deleteProfileToolStripMenuItem.Text = "Delete Profile...";
            deleteProfileToolStripMenuItem.Click += deleteProfileToolStripMenuItem_Click;
            // 
            // il_profiles
            // 
            il_profiles.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            il_profiles.ImageSize = new System.Drawing.Size(64, 64);
            il_profiles.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // btn_view_current
            // 
            btn_view_current.BackColor = System.Drawing.Color.Black;
            btn_view_current.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_view_current.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_view_current.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_view_current.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_view_current.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_view_current.ForeColor = System.Drawing.Color.White;
            btn_view_current.Location = new System.Drawing.Point(558, 3);
            btn_view_current.Name = "btn_view_current";
            btn_view_current.Size = new System.Drawing.Size(269, 44);
            btn_view_current.TabIndex = 4;
            btn_view_current.Text = "View &Current Display";
            btn_view_current.UseVisualStyleBackColor = false;
            btn_view_current.Click += btn_view_current_Click;
            // 
            // btn_save_or_rename
            // 
            btn_save_or_rename.BackColor = System.Drawing.Color.Black;
            btn_save_or_rename.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_save_or_rename.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save_or_rename.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save_or_rename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save_or_rename.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_save_or_rename.ForeColor = System.Drawing.Color.White;
            btn_save_or_rename.Location = new System.Drawing.Point(3, 3);
            btn_save_or_rename.Name = "btn_save_or_rename";
            btn_save_or_rename.Size = new System.Drawing.Size(154, 44);
            btn_save_or_rename.TabIndex = 0;
            btn_save_or_rename.Text = "&Save";
            btn_save_or_rename.UseVisualStyleBackColor = false;
            btn_save_or_rename.Click += btn_save_as_Click;
            // 
            // pb_down_arrow
            // 
            pb_down_arrow.BackColor = System.Drawing.Color.DimGray;
            pb_down_arrow.Image = Properties.Resources.redarrowsdown;
            pb_down_arrow.Location = new System.Drawing.Point(517, 539);
            pb_down_arrow.Name = "pb_down_arrow";
            pb_down_arrow.Size = new System.Drawing.Size(63, 31);
            pb_down_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pb_down_arrow.TabIndex = 18;
            pb_down_arrow.TabStop = false;
            // 
            // lbl_profile_shown
            // 
            lbl_profile_shown.AutoSize = true;
            lbl_profile_shown.BackColor = System.Drawing.Color.Black;
            lbl_profile_shown.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_profile_shown.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_profile_shown.ForeColor = System.Drawing.Color.White;
            lbl_profile_shown.Location = new System.Drawing.Point(3, 0);
            lbl_profile_shown.Name = "lbl_profile_shown";
            lbl_profile_shown.Size = new System.Drawing.Size(1086, 49);
            lbl_profile_shown.TabIndex = 19;
            lbl_profile_shown.Text = "My Display Profile";
            // 
            // txt_profile_save_name
            // 
            txt_profile_save_name.BackColor = System.Drawing.Color.White;
            txt_profile_save_name.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txt_profile_save_name.Dock = System.Windows.Forms.DockStyle.Fill;
            txt_profile_save_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_profile_save_name.Location = new System.Drawing.Point(163, 3);
            txt_profile_save_name.MaxLength = 200;
            txt_profile_save_name.Name = "txt_profile_save_name";
            txt_profile_save_name.Size = new System.Drawing.Size(314, 35);
            txt_profile_save_name.TabIndex = 1;
            txt_profile_save_name.KeyDown += txt_profile_save_name_KeyDown;
            // 
            // ilv_saved_profiles
            // 
            ilv_saved_profiles.AllowCheckBoxClick = false;
            ilv_saved_profiles.AllowColumnClick = false;
            ilv_saved_profiles.AllowColumnResize = false;
            ilv_saved_profiles.AllowItemReorder = false;
            ilv_saved_profiles.AllowPaneResize = false;
            ilv_saved_profiles.Dock = System.Windows.Forms.DockStyle.Fill;
            ilv_saved_profiles.Location = new System.Drawing.Point(0, 0);
            ilv_saved_profiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ilv_saved_profiles.MultiSelect = false;
            ilv_saved_profiles.Name = "ilv_saved_profiles";
            ilv_saved_profiles.PersistentCacheDirectory = "";
            ilv_saved_profiles.PersistentCacheSize = 100L;
            ilv_saved_profiles.Size = new System.Drawing.Size(1098, 181);
            ilv_saved_profiles.TabIndex = 21;
            ilv_saved_profiles.UseWIC = true;
            ilv_saved_profiles.View = Manina.Windows.Forms.View.HorizontalStrip;
            ilv_saved_profiles.ItemClick += ilv_saved_profiles_ItemClick;
            ilv_saved_profiles.ItemHover += ilv_saved_profiles_ItemHover;
            ilv_saved_profiles.ItemDoubleClick += ilv_saved_profiles_ItemDoubleClick;
            // 
            // lbl_profile_shown_subtitle
            // 
            lbl_profile_shown_subtitle.AutoSize = true;
            lbl_profile_shown_subtitle.BackColor = System.Drawing.Color.Black;
            lbl_profile_shown_subtitle.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_profile_shown_subtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_profile_shown_subtitle.ForeColor = System.Drawing.Color.White;
            lbl_profile_shown_subtitle.Location = new System.Drawing.Point(3, 49);
            lbl_profile_shown_subtitle.Name = "lbl_profile_shown_subtitle";
            lbl_profile_shown_subtitle.Size = new System.Drawing.Size(1086, 32);
            lbl_profile_shown_subtitle.TabIndex = 22;
            lbl_profile_shown_subtitle.Text = "My Display Profile";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.Color.Black;
            label1.Dock = System.Windows.Forms.DockStyle.Fill;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(3, 526);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(1086, 36);
            label1.TabIndex = 23;
            label1.Text = "Saved Profiles (These can be used to create shortcuts)";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_save_profile
            // 
            lbl_save_profile.BackColor = System.Drawing.Color.Firebrick;
            lbl_save_profile.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lbl_save_profile.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_save_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            lbl_save_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            lbl_save_profile.ForeColor = System.Drawing.Color.White;
            lbl_save_profile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            lbl_save_profile.Location = new System.Drawing.Point(3, 81);
            lbl_save_profile.Name = "lbl_save_profile";
            lbl_save_profile.Size = new System.Drawing.Size(1086, 62);
            lbl_save_profile.TabIndex = 33;
            lbl_save_profile.Text = resources.GetString("lbl_save_profile.Text");
            lbl_save_profile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_save
            // 
            btn_save.BackColor = System.Drawing.Color.Black;
            btn_save.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_save.ForeColor = System.Drawing.Color.White;
            btn_save.Location = new System.Drawing.Point(381, 3);
            btn_save.Name = "btn_save";
            btn_save.Size = new System.Drawing.Size(206, 54);
            btn_save.TabIndex = 4;
            btn_save.Text = "Save to Desk&top";
            btn_save.UseVisualStyleBackColor = false;
            btn_save.Click += Save_Click;
            // 
            // dialog_save
            // 
            dialog_save.DefaultExt = "lnk";
            dialog_save.DereferenceLinks = false;
            dialog_save.Filter = "Shortcuts|*.lnk";
            dialog_save.RestoreDirectory = true;
            // 
            // btn_hotkey
            // 
            btn_hotkey.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_hotkey.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_hotkey.ForeColor = System.Drawing.Color.White;
            btn_hotkey.Location = new System.Drawing.Point(127, 3);
            btn_hotkey.Name = "btn_hotkey";
            btn_hotkey.Size = new System.Drawing.Size(122, 54);
            btn_hotkey.TabIndex = 1;
            btn_hotkey.Text = "&Hotkey";
            btn_hotkey.UseVisualStyleBackColor = true;
            btn_hotkey.Click += btn_hotkey_Click;
            // 
            // lbl_hotkey_assigned
            // 
            lbl_hotkey_assigned.AutoSize = true;
            lbl_hotkey_assigned.BackColor = System.Drawing.Color.Brown;
            lbl_hotkey_assigned.Dock = System.Windows.Forms.DockStyle.Fill;
            lbl_hotkey_assigned.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_hotkey_assigned.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_assigned.Location = new System.Drawing.Point(3, 0);
            lbl_hotkey_assigned.Name = "lbl_hotkey_assigned";
            lbl_hotkey_assigned.Size = new System.Drawing.Size(118, 60);
            lbl_hotkey_assigned.TabIndex = 36;
            lbl_hotkey_assigned.Text = "Hotkeys: None";
            lbl_hotkey_assigned.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lbl_hotkey_assigned.Visible = false;
            lbl_hotkey_assigned.Click += lbl_hotkey_assigned_Click;
            // 
            // p_upper
            // 
            p_upper.BackColor = System.Drawing.Color.DimGray;
            p_upper.BackgroundImage = (System.Drawing.Image)resources.GetObject("p_upper.BackgroundImage");
            p_upper.Controls.Add(tlp_upper);
            p_upper.Dock = System.Windows.Forms.DockStyle.Top;
            p_upper.Location = new System.Drawing.Point(0, 0);
            p_upper.Name = "p_upper";
            p_upper.Size = new System.Drawing.Size(1098, 597);
            p_upper.TabIndex = 37;
            // 
            // tlp_upper
            // 
            tlp_upper.ColumnCount = 1;
            tlp_upper.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_upper.Controls.Add(flp_header_buttons, 0, 0);
            tlp_upper.Controls.Add(lbl_profile_shown, 0, 1);
            tlp_upper.Controls.Add(lbl_profile_shown_subtitle, 0, 2);
            tlp_upper.Controls.Add(lbl_save_profile, 0, 3);
            tlp_upper.Controls.Add(dv_profile, 0, 4);
            tlp_upper.Controls.Add(flp_save, 0, 5);
            tlp_upper.Controls.Add(pb_down_arrow, 0, 6);
            tlp_upper.Controls.Add(label1, 0, 7);
            tlp_upper.Dock = System.Windows.Forms.DockStyle.Fill;
            tlp_upper.Location = new System.Drawing.Point(0, 0);
            tlp_upper.Name = "tlp_upper";
            tlp_upper.RowCount = 8;
            tlp_upper.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_upper.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_upper.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_upper.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_upper.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tlp_upper.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_upper.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_upper.RowStyles.Add(new System.Windows.Forms.RowStyle());
            tlp_upper.Size = new System.Drawing.Size(1098, 597);
            tlp_upper.TabIndex = 0;
            // 
            // flp_header_buttons
            // 
            flp_header_buttons.Controls.Add(btn_help);
            flp_header_buttons.Controls.Add(btn_donate);
            flp_header_buttons.Controls.Add(btn_view_current);
            flp_header_buttons.Dock = System.Windows.Forms.DockStyle.Fill;
            flp_header_buttons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            flp_header_buttons.Location = new System.Drawing.Point(3, 3);
            flp_header_buttons.Name = "flp_header_buttons";
            flp_header_buttons.Size = new System.Drawing.Size(1092, 50);
            flp_header_buttons.TabIndex = 0;
            // 
            // btn_help
            // 
            btn_help.BackColor = System.Drawing.Color.Black;
            btn_help.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_help.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_help.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_help.ForeColor = System.Drawing.Color.White;
            btn_help.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_help.Location = new System.Drawing.Point(1005, 3);
            btn_help.Name = "btn_help";
            btn_help.Size = new System.Drawing.Size(84, 44);
            btn_help.TabIndex = 1;
            btn_help.Text = "Hel&p";
            btn_help.UseVisualStyleBackColor = false;
            btn_help.Click += btn_help_Click;
            // 
            // btn_donate
            // 
            btn_donate.BackColor = System.Drawing.Color.Black;
            btn_donate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_donate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_donate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_donate.ForeColor = System.Drawing.Color.White;
            btn_donate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_donate.Location = new System.Drawing.Point(915, 3);
            btn_donate.Name = "btn_donate";
            btn_donate.Size = new System.Drawing.Size(84, 44);
            btn_donate.TabIndex = 0;
            btn_donate.Text = "D&onate";
            btn_donate.UseVisualStyleBackColor = false;
            btn_donate.Click += btn_donate_Click;
            // 
            // flp_save
            // 
            flp_save.Controls.Add(btn_save_or_rename);
            flp_save.Controls.Add(txt_profile_save_name);
            flp_save.Controls.Add(btn_profile_settings);
            flp_save.Dock = System.Windows.Forms.DockStyle.Fill;
            flp_save.Location = new System.Drawing.Point(3, 475);
            flp_save.Name = "flp_save";
            flp_save.Size = new System.Drawing.Size(1092, 48);
            flp_save.TabIndex = 5;
            // 
            // btn_profile_settings
            // 
            btn_profile_settings.BackColor = System.Drawing.Color.Black;
            btn_profile_settings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_profile_settings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_profile_settings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_profile_settings.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_profile_settings.ForeColor = System.Drawing.Color.White;
            btn_profile_settings.Location = new System.Drawing.Point(483, 3);
            btn_profile_settings.Name = "btn_profile_settings";
            btn_profile_settings.Size = new System.Drawing.Size(198, 44);
            btn_profile_settings.TabIndex = 2;
            btn_profile_settings.Text = "&Profile Settings";
            btn_profile_settings.UseVisualStyleBackColor = false;
            btn_profile_settings.Click += btn_profile_settings_Click;
            // 
            // dv_profile
            // 
            dv_profile.BackColor = System.Drawing.Color.DimGray;
            dv_profile.Dock = System.Windows.Forms.DockStyle.Fill;
            dv_profile.Font = new System.Drawing.Font("Consolas", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            dv_profile.Location = new System.Drawing.Point(3, 146);
            dv_profile.Margin = new System.Windows.Forms.Padding(21);
            dv_profile.Name = "dv_profile";
            dv_profile.PaddingX = 100;
            dv_profile.PaddingY = 100;
            dv_profile.Profile = null;
            dv_profile.Size = new System.Drawing.Size(1092, 323);
            dv_profile.TabIndex = 4;
            // 
            // p_lower
            // 
            p_lower.BackgroundImage = (System.Drawing.Image)resources.GetObject("p_lower.BackgroundImage");
            p_lower.Controls.Add(tlp_lower);
            p_lower.Dock = System.Windows.Forms.DockStyle.Bottom;
            p_lower.Location = new System.Drawing.Point(0, 778);
            p_lower.Name = "p_lower";
            p_lower.Size = new System.Drawing.Size(1098, 110);
            p_lower.TabIndex = 38;
            // 
            // tlp_lower
            // 
            tlp_lower.ColumnCount = 7;
            tlp_lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlp_lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlp_lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlp_lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlp_lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlp_lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            tlp_lower.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            tlp_lower.Controls.Add(lbl_hotkey_assigned, 0, 0);
            tlp_lower.Controls.Add(btn_hotkey, 0, 1);
            tlp_lower.Controls.Add(btn_apply, 1, 1);
            tlp_lower.Controls.Add(btn_update, 2, 1);
            tlp_lower.Controls.Add(btn_delete, 3, 1);
            tlp_lower.Controls.Add(btn_save, 4, 1);
            tlp_lower.Controls.Add(btn_back, 6, 1);
            tlp_lower.Dock = System.Windows.Forms.DockStyle.Fill;
            tlp_lower.Location = new System.Drawing.Point(0, 0);
            tlp_lower.Name = "tlp_lower";
            tlp_lower.RowCount = 2;
            tlp_lower.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlp_lower.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            tlp_lower.Size = new System.Drawing.Size(1098, 110);
            tlp_lower.TabIndex = 0;
            // 
            // btn_update
            // 
            btn_update.BackColor = System.Drawing.Color.Black;
            btn_update.Dock = System.Windows.Forms.DockStyle.Fill;
            btn_update.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_update.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_update.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_update.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_update.ForeColor = System.Drawing.Color.White;
            btn_update.Location = new System.Drawing.Point(127, 3);
            btn_update.Name = "btn_update";
            btn_update.Size = new System.Drawing.Size(122, 54);
            btn_update.TabIndex = 3;
            btn_update.Text = "&Update";
            btn_update.UseVisualStyleBackColor = false;
            btn_update.Click += btn_update_Click;
            // 
            // p_fill
            // 
            p_fill.BackColor = System.Drawing.Color.White;
            p_fill.Controls.Add(ilv_saved_profiles);
            p_fill.Dock = System.Windows.Forms.DockStyle.Fill;
            p_fill.Location = new System.Drawing.Point(0, 597);
            p_fill.Name = "p_fill";
            p_fill.Size = new System.Drawing.Size(1098, 181);
            p_fill.TabIndex = 39;
            // 
            // DisplayProfileForm
            // 
            AcceptButton = btn_apply;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            CancelButton = btn_back;
            ClientSize = new System.Drawing.Size(1098, 888);
            Controls.Add(p_fill);
            Controls.Add(p_lower);
            Controls.Add(p_upper);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            MinimumSize = new System.Drawing.Size(1106, 820);
            Name = "DisplayProfileForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician - Display Profiles";
            Load += DisplayProfileForm_Load;
            cms_profiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pb_down_arrow).EndInit();
            p_upper.ResumeLayout(false);
            tlp_upper.ResumeLayout(false);
            tlp_upper.PerformLayout();
            flp_header_buttons.ResumeLayout(false);
            flp_save.ResumeLayout(false);
            flp_save.PerformLayout();
            p_lower.ResumeLayout(false);
            tlp_lower.ResumeLayout(false);
            tlp_lower.PerformLayout();
            p_fill.ResumeLayout(false);
            ResumeLayout(false);
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
        private System.Windows.Forms.TableLayoutPanel tlp_upper;
        private System.Windows.Forms.FlowLayoutPanel flp_header_buttons;
        private System.Windows.Forms.Button btn_help;
        private System.Windows.Forms.Button btn_donate;
        private System.Windows.Forms.FlowLayoutPanel flp_save;
        private System.Windows.Forms.Button btn_profile_settings;
        private System.Windows.Forms.Panel p_lower;
        private System.Windows.Forms.TableLayoutPanel tlp_lower;
        private System.Windows.Forms.Button btn_update;
        private System.Windows.Forms.Panel p_fill;
        private System.Windows.Forms.ToolStripMenuItem deleteProfileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveProfileToDesktopToolStripMenuItem;
    }
}


