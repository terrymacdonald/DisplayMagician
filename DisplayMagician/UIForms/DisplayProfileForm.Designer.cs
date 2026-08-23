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
            tt_selected = new System.Windows.Forms.ToolTip(components);
            lbl_save_profile = new System.Windows.Forms.Label();
            btn_save = new System.Windows.Forms.Button();
            dialog_save = new System.Windows.Forms.SaveFileDialog();
            btn_hotkey = new System.Windows.Forms.Button();
            lbl_hotkey_assigned = new System.Windows.Forms.Label();
            p_upper = new System.Windows.Forms.Panel();
            btn_donate = new System.Windows.Forms.Button();
            btn_help = new System.Windows.Forms.Button();
            dv_profile = new DisplayView();
            btn_profile_settings = new System.Windows.Forms.Button();
            p_lower = new System.Windows.Forms.Panel();
            btn_update = new System.Windows.Forms.Button();
            p_listview = new System.Windows.Forms.Panel();
            p_middle = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            cms_profiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pb_down_arrow).BeginInit();
            p_upper.SuspendLayout();
            p_lower.SuspendLayout();
            p_listview.SuspendLayout();
            p_middle.SuspendLayout();
            SuspendLayout();
            // 
            // btn_apply
            // 
            btn_apply.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_apply.BackColor = System.Drawing.Color.Black;
            btn_apply.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_apply.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_apply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_apply.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            btn_apply.ForeColor = System.Drawing.Color.White;
            btn_apply.Location = new System.Drawing.Point(267, 26);
            btn_apply.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_apply.Name = "btn_apply";
            btn_apply.Size = new System.Drawing.Size(128, 40);
            btn_apply.TabIndex = 9;
            btn_apply.Text = "&Apply";
            btn_apply.UseVisualStyleBackColor = false;
            btn_apply.Click += Apply_Click;
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.BackColor = System.Drawing.Color.Black;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(918, 64);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(82, 29);
            btn_back.TabIndex = 13;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = false;
            btn_back.Click += Exit_Click;
            // 
            // btn_delete
            // 
            btn_delete.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_delete.BackColor = System.Drawing.Color.Black;
            btn_delete.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_delete.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            btn_delete.ForeColor = System.Drawing.Color.White;
            btn_delete.Location = new System.Drawing.Point(529, 26);
            btn_delete.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_delete.Name = "btn_delete";
            btn_delete.Size = new System.Drawing.Size(130, 40);
            btn_delete.TabIndex = 11;
            btn_delete.Text = "&Delete";
            btn_delete.UseVisualStyleBackColor = false;
            btn_delete.Click += Delete_Click;
            // 
            // cms_profiles
            // 
            cms_profiles.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cms_profiles.ImageScalingSize = new System.Drawing.Size(32, 32);
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
            btn_view_current.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_view_current.BackColor = System.Drawing.Color.Black;
            btn_view_current.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_view_current.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_view_current.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_view_current.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_view_current.ForeColor = System.Drawing.Color.White;
            btn_view_current.Location = new System.Drawing.Point(705, 22);
            btn_view_current.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_view_current.Name = "btn_view_current";
            btn_view_current.Size = new System.Drawing.Size(290, 35);
            btn_view_current.TabIndex = 1;
            btn_view_current.Text = "View &Current Display";
            btn_view_current.UseVisualStyleBackColor = false;
            btn_view_current.Click += btn_view_current_Click;
            // 
            // btn_save_or_rename
            // 
            btn_save_or_rename.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_save_or_rename.BackColor = System.Drawing.Color.Black;
            btn_save_or_rename.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save_or_rename.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save_or_rename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save_or_rename.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            btn_save_or_rename.ForeColor = System.Drawing.Color.White;
            btn_save_or_rename.Location = new System.Drawing.Point(124, 11);
            btn_save_or_rename.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_save_or_rename.Name = "btn_save_or_rename";
            btn_save_or_rename.Size = new System.Drawing.Size(164, 36);
            btn_save_or_rename.TabIndex = 4;
            btn_save_or_rename.Text = "&Save";
            btn_save_or_rename.UseVisualStyleBackColor = false;
            btn_save_or_rename.Click += btn_save_as_Click;
            // 
            // pb_down_arrow
            // 
            pb_down_arrow.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            pb_down_arrow.BackColor = System.Drawing.Color.Transparent;
            pb_down_arrow.Image = Properties.Resources.redarrowsdown;
            pb_down_arrow.Location = new System.Drawing.Point(475, 44);
            pb_down_arrow.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pb_down_arrow.Name = "pb_down_arrow";
            pb_down_arrow.Size = new System.Drawing.Size(58, 33);
            pb_down_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pb_down_arrow.TabIndex = 18;
            pb_down_arrow.TabStop = false;
            // 
            // lbl_profile_shown
            // 
            lbl_profile_shown.AutoSize = true;
            lbl_profile_shown.BackColor = System.Drawing.Color.Black;
            lbl_profile_shown.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_profile_shown.ForeColor = System.Drawing.Color.White;
            lbl_profile_shown.Location = new System.Drawing.Point(20, 26);
            lbl_profile_shown.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_profile_shown.Name = "lbl_profile_shown";
            lbl_profile_shown.Size = new System.Drawing.Size(205, 29);
            lbl_profile_shown.TabIndex = 0;
            lbl_profile_shown.Text = "My Display Profile";
            // 
            // txt_profile_save_name
            // 
            txt_profile_save_name.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            txt_profile_save_name.BackColor = System.Drawing.Color.White;
            txt_profile_save_name.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            txt_profile_save_name.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_profile_save_name.Location = new System.Drawing.Point(296, 12);
            txt_profile_save_name.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_profile_save_name.MaxLength = 200;
            txt_profile_save_name.Name = "txt_profile_save_name";
            txt_profile_save_name.Size = new System.Drawing.Size(416, 35);
            txt_profile_save_name.TabIndex = 5;
            txt_profile_save_name.KeyDown += txt_profile_save_name_KeyDown;
            // 
            // ilv_saved_profiles
            // 
            ilv_saved_profiles.AllowCheckBoxClick = false;
            ilv_saved_profiles.AllowColumnClick = false;
            ilv_saved_profiles.AllowColumnResize = false;
            ilv_saved_profiles.AllowItemReorder = false;
            ilv_saved_profiles.AllowPaneResize = false;
            ilv_saved_profiles.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            ilv_saved_profiles.BackColor = System.Drawing.Color.White;
            ilv_saved_profiles.Colors = new Manina.Windows.Forms.ImageListViewColor(resources.GetString("ilv_saved_profiles.Colors"));
            ilv_saved_profiles.Location = new System.Drawing.Point(0, 0);
            ilv_saved_profiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            ilv_saved_profiles.MultiSelect = false;
            ilv_saved_profiles.Name = "ilv_saved_profiles";
            ilv_saved_profiles.PersistentCacheDirectory = "";
            ilv_saved_profiles.PersistentCacheSize = 100L;
            ilv_saved_profiles.Size = new System.Drawing.Size(1008, 165);
            ilv_saved_profiles.TabIndex = 7;
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
            lbl_profile_shown_subtitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_profile_shown_subtitle.ForeColor = System.Drawing.Color.White;
            lbl_profile_shown_subtitle.Location = new System.Drawing.Point(22, 63);
            lbl_profile_shown_subtitle.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_profile_shown_subtitle.Name = "lbl_profile_shown_subtitle";
            lbl_profile_shown_subtitle.Size = new System.Drawing.Size(132, 20);
            lbl_profile_shown_subtitle.TabIndex = 0;
            lbl_profile_shown_subtitle.Text = "My Display Profile";
            // 
            // lbl_save_profile
            // 
            lbl_save_profile.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_save_profile.BackColor = System.Drawing.Color.Firebrick;
            lbl_save_profile.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            lbl_save_profile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            lbl_save_profile.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            lbl_save_profile.ForeColor = System.Drawing.Color.White;
            lbl_save_profile.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            lbl_save_profile.Location = new System.Drawing.Point(2, 116);
            lbl_save_profile.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_save_profile.Name = "lbl_save_profile";
            lbl_save_profile.Size = new System.Drawing.Size(1004, 66);
            lbl_save_profile.TabIndex = 0;
            lbl_save_profile.Text = resources.GetString("lbl_save_profile.Text");
            lbl_save_profile.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_save
            // 
            btn_save.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_save.BackColor = System.Drawing.Color.Black;
            btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            btn_save.ForeColor = System.Drawing.Color.White;
            btn_save.Location = new System.Drawing.Point(662, 26);
            btn_save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_save.Name = "btn_save";
            btn_save.Size = new System.Drawing.Size(228, 40);
            btn_save.TabIndex = 12;
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
            btn_hotkey.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_hotkey.BackColor = System.Drawing.Color.Black;
            btn_hotkey.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_hotkey.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_hotkey.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_hotkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            btn_hotkey.ForeColor = System.Drawing.Color.White;
            btn_hotkey.Location = new System.Drawing.Point(132, 26);
            btn_hotkey.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_hotkey.Name = "btn_hotkey";
            btn_hotkey.Size = new System.Drawing.Size(130, 40);
            btn_hotkey.TabIndex = 8;
            btn_hotkey.Text = "&Hotkey";
            btn_hotkey.UseVisualStyleBackColor = false;
            btn_hotkey.Click += btn_hotkey_Click;
            // 
            // lbl_hotkey_assigned
            // 
            lbl_hotkey_assigned.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            lbl_hotkey_assigned.AutoSize = true;
            lbl_hotkey_assigned.BackColor = System.Drawing.Color.Brown;
            lbl_hotkey_assigned.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_hotkey_assigned.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_assigned.Location = new System.Drawing.Point(4, 2);
            lbl_hotkey_assigned.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_assigned.Name = "lbl_hotkey_assigned";
            lbl_hotkey_assigned.Size = new System.Drawing.Size(113, 20);
            lbl_hotkey_assigned.TabIndex = 0;
            lbl_hotkey_assigned.Text = "Hotkeys: None";
            lbl_hotkey_assigned.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lbl_hotkey_assigned.Visible = false;
            lbl_hotkey_assigned.Click += lbl_hotkey_assigned_Click;
            // 
            // p_upper
            // 
            p_upper.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            p_upper.BackColor = System.Drawing.Color.Transparent;
            p_upper.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            p_upper.Controls.Add(lbl_save_profile);
            p_upper.Controls.Add(btn_donate);
            p_upper.Controls.Add(btn_help);
            p_upper.Controls.Add(btn_view_current);
            p_upper.Controls.Add(lbl_profile_shown);
            p_upper.Controls.Add(lbl_profile_shown_subtitle);
            p_upper.Controls.Add(dv_profile);
            p_upper.Location = new System.Drawing.Point(0, 0);
            p_upper.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_upper.Name = "p_upper";
            p_upper.Size = new System.Drawing.Size(1008, 582);
            p_upper.TabIndex = 37;
            // 
            // btn_donate
            // 
            btn_donate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_donate.BackColor = System.Drawing.Color.Black;
            btn_donate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_donate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_donate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_donate.ForeColor = System.Drawing.Color.White;
            btn_donate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_donate.Location = new System.Drawing.Point(830, 78);
            btn_donate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_donate.Name = "btn_donate";
            btn_donate.Size = new System.Drawing.Size(78, 29);
            btn_donate.TabIndex = 2;
            btn_donate.Text = "D&onate";
            btn_donate.UseVisualStyleBackColor = false;
            btn_donate.Click += btn_donate_Click;
            // 
            // btn_help
            // 
            btn_help.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_help.BackColor = System.Drawing.Color.Black;
            btn_help.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_help.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_help.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_help.ForeColor = System.Drawing.Color.White;
            btn_help.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_help.Location = new System.Drawing.Point(916, 78);
            btn_help.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_help.Name = "btn_help";
            btn_help.Size = new System.Drawing.Size(78, 29);
            btn_help.TabIndex = 3;
            btn_help.Text = "Hel&p";
            btn_help.UseVisualStyleBackColor = false;
            btn_help.Click += btn_help_Click;
            // 
            // dv_profile
            // 
            dv_profile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            dv_profile.AutoSize = true;
            dv_profile.BackColor = System.Drawing.Color.DimGray;
            dv_profile.Font = new System.Drawing.Font("Consolas", 50F);
            dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            dv_profile.Location = new System.Drawing.Point(0, 115);
            dv_profile.Margin = new System.Windows.Forms.Padding(247, 115, 247, 115);
            dv_profile.Name = "dv_profile";
            dv_profile.Size = new System.Drawing.Size(1008, 467);
            dv_profile.TabIndex = 0;
            // 
            // btn_profile_settings
            // 
            btn_profile_settings.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_profile_settings.BackColor = System.Drawing.Color.Black;
            btn_profile_settings.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_profile_settings.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_profile_settings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_profile_settings.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            btn_profile_settings.ForeColor = System.Drawing.Color.White;
            btn_profile_settings.Location = new System.Drawing.Point(720, 12);
            btn_profile_settings.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_profile_settings.Name = "btn_profile_settings";
            btn_profile_settings.Size = new System.Drawing.Size(184, 36);
            btn_profile_settings.TabIndex = 6;
            btn_profile_settings.Text = "&Profile Settings";
            btn_profile_settings.UseVisualStyleBackColor = false;
            btn_profile_settings.Click += btn_profile_settings_Click;
            // 
            // p_lower
            // 
            p_lower.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            p_lower.BackColor = System.Drawing.Color.Transparent;
            p_lower.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            p_lower.Controls.Add(btn_update);
            p_lower.Controls.Add(lbl_hotkey_assigned);
            p_lower.Controls.Add(btn_hotkey);
            p_lower.Controls.Add(btn_delete);
            p_lower.Controls.Add(btn_apply);
            p_lower.Controls.Add(btn_save);
            p_lower.Controls.Add(btn_back);
            p_lower.Location = new System.Drawing.Point(0, 841);
            p_lower.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_lower.MaximumSize = new System.Drawing.Size(0, 118);
            p_lower.Name = "p_lower";
            p_lower.Size = new System.Drawing.Size(1008, 102);
            p_lower.TabIndex = 0;
            // 
            // btn_update
            // 
            btn_update.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_update.BackColor = System.Drawing.Color.Black;
            btn_update.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_update.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_update.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_update.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F);
            btn_update.ForeColor = System.Drawing.Color.White;
            btn_update.Location = new System.Drawing.Point(397, 26);
            btn_update.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_update.Name = "btn_update";
            btn_update.Size = new System.Drawing.Size(128, 40);
            btn_update.TabIndex = 10;
            btn_update.Text = "&Update";
            btn_update.UseVisualStyleBackColor = false;
            btn_update.Click += btn_update_Click;
            // 
            // p_listview
            // 
            p_listview.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            p_listview.BackColor = System.Drawing.Color.White;
            p_listview.Controls.Add(ilv_saved_profiles);
            p_listview.Location = new System.Drawing.Point(0, 675);
            p_listview.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            p_listview.Name = "p_listview";
            p_listview.Size = new System.Drawing.Size(1008, 165);
            p_listview.TabIndex = 39;
            // 
            // p_middle
            // 
            p_middle.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            p_middle.BackColor = System.Drawing.Color.Transparent;
            p_middle.Controls.Add(label1);
            p_middle.Controls.Add(txt_profile_save_name);
            p_middle.Controls.Add(btn_save_or_rename);
            p_middle.Controls.Add(btn_profile_settings);
            p_middle.Controls.Add(pb_down_arrow);
            p_middle.Location = new System.Drawing.Point(0, 581);
            p_middle.Name = "p_middle";
            p_middle.Size = new System.Drawing.Size(1008, 94);
            p_middle.TabIndex = 0;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.Color.Black;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(279, 74);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(450, 20);
            label1.TabIndex = 0;
            label1.Text = "Saved Profiles (These can be used to create shortcuts)";
            label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // DisplayProfileForm
            // 
            AcceptButton = btn_apply;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.DimGray;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            CancelButton = btn_back;
            ClientSize = new System.Drawing.Size(1008, 943);
            Controls.Add(p_middle);
            Controls.Add(p_listview);
            Controls.Add(p_lower);
            Controls.Add(p_upper);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(1024, 970);
            Name = "DisplayProfileForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician - Display Profiles";
            Load += DisplayProfileForm_Load;
            cms_profiles.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pb_down_arrow).EndInit();
            p_upper.ResumeLayout(false);
            p_upper.PerformLayout();
            p_lower.ResumeLayout(false);
            p_lower.PerformLayout();
            p_listview.ResumeLayout(false);
            p_middle.ResumeLayout(false);
            p_middle.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
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
        private System.Windows.Forms.ToolTip tt_selected;
        private System.Windows.Forms.Label lbl_save_profile;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.SaveFileDialog dialog_save;
        private System.Windows.Forms.Button btn_hotkey;
        private System.Windows.Forms.Label lbl_hotkey_assigned;
        private System.Windows.Forms.Panel p_upper;
        private System.Windows.Forms.Button btn_profile_settings;
        private System.Windows.Forms.Panel p_lower;
        private System.Windows.Forms.Panel p_listview;
        private System.Windows.Forms.Button btn_help;
        private System.Windows.Forms.ToolStripMenuItem deleteProfileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveProfileToDesktopToolStripMenuItem;
        private System.Windows.Forms.Button btn_donate;
        private System.Windows.Forms.Button btn_update;
        private DisplayView dv_profile;
        private System.Windows.Forms.Panel p_middle;
        private System.Windows.Forms.Label label1;
    }
}

