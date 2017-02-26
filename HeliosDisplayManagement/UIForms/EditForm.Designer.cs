using HeliosDisplayManagement.Resources;
using HeliosDisplayManagement.Shared.UserControls;

namespace HeliosDisplayManagement.UIForms
{
    partial class EditForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditForm));
            this.btn_save = new System.Windows.Forms.Button();
            this.btn_apply = new System.Windows.Forms.Button();
            this.btn_cancel = new System.Windows.Forms.Button();
            this.dv_profile = new HeliosDisplayManagement.Shared.UserControls.DisplayView();
            this.txt_name = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.lv_monitors = new System.Windows.Forms.ListView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.gb_arrangement = new System.Windows.Forms.GroupBox();
            this.nud_y = new System.Windows.Forms.NumericUpDown();
            this.nud_x = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.cb_clone = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.cb_rotation = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.cb_frequency = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.cb_colordepth = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.cb_resolution = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.gb_surround = new System.Windows.Forms.GroupBox();
            this.cb_surround_applybezel = new System.Windows.Forms.CheckBox();
            this.btn_topo = new System.Windows.Forms.Button();
            this.gb_eyefinity = new System.Windows.Forms.GroupBox();
            this.label10 = new System.Windows.Forms.Label();
            this.gb_arrangement.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_y)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_x)).BeginInit();
            this.gb_surround.SuspendLayout();
            this.gb_eyefinity.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_save
            // 
            this.btn_save.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_save.Location = new System.Drawing.Point(569, 572);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(75, 23);
            this.btn_save.TabIndex = 5;
            this.btn_save.Text = global::HeliosDisplayManagement.Resources.Language.Save;
            this.btn_save.UseVisualStyleBackColor = true;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // btn_apply
            // 
            this.btn_apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_apply.Location = new System.Drawing.Point(472, 572);
            this.btn_apply.Name = "btn_apply";
            this.btn_apply.Size = new System.Drawing.Size(91, 23);
            this.btn_apply.TabIndex = 6;
            this.btn_apply.Text = global::HeliosDisplayManagement.Resources.Language.Save_and_Apply;
            this.btn_apply.UseVisualStyleBackColor = true;
            this.btn_apply.Click += new System.EventHandler(this.btn_apply_Click);
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Location = new System.Drawing.Point(12, 572);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 7;
            this.btn_cancel.Text = global::HeliosDisplayManagement.Resources.Language.Cancel;
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // dv_profile
            // 
            this.dv_profile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dv_profile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(184)))), ((int)(((byte)(196)))));
            this.dv_profile.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dv_profile.Font = new System.Drawing.Font("Consolas", 50F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dv_profile.ForeColor = System.Drawing.Color.MidnightBlue;
            this.dv_profile.Location = new System.Drawing.Point(-1, -1);
            this.dv_profile.Margin = new System.Windows.Forms.Padding(18);
            this.dv_profile.Name = "dv_profile";
            this.dv_profile.PaddingX = 100;
            this.dv_profile.PaddingY = 100;
            this.dv_profile.Profile = null;
            this.dv_profile.Size = new System.Drawing.Size(658, 236);
            this.dv_profile.TabIndex = 2;
            // 
            // txt_name
            // 
            this.txt_name.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txt_name.Location = new System.Drawing.Point(12, 263);
            this.txt_name.Name = "txt_name";
            this.txt_name.Size = new System.Drawing.Size(632, 20);
            this.txt_name.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 247);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Profile Name:";
            // 
            // lv_monitors
            // 
            this.lv_monitors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lv_monitors.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lv_monitors.HideSelection = false;
            this.lv_monitors.LargeImageList = this.imageList1;
            this.lv_monitors.Location = new System.Drawing.Point(-1, 294);
            this.lv_monitors.Name = "lv_monitors";
            this.lv_monitors.ShowGroups = false;
            this.lv_monitors.Size = new System.Drawing.Size(658, 75);
            this.lv_monitors.SmallImageList = this.imageList1;
            this.lv_monitors.TabIndex = 10;
            this.lv_monitors.UseCompatibleStateImageBehavior = false;
            this.lv_monitors.View = System.Windows.Forms.View.List;
            this.lv_monitors.SelectedIndexChanged += new System.EventHandler(this.lv_monitors_SelectionChangeCommitted);
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(48, 48);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // gb_arrangement
            // 
            this.gb_arrangement.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.gb_arrangement.Controls.Add(this.nud_y);
            this.gb_arrangement.Controls.Add(this.nud_x);
            this.gb_arrangement.Controls.Add(this.label11);
            this.gb_arrangement.Controls.Add(this.cb_clone);
            this.gb_arrangement.Controls.Add(this.label9);
            this.gb_arrangement.Controls.Add(this.cb_rotation);
            this.gb_arrangement.Controls.Add(this.label8);
            this.gb_arrangement.Controls.Add(this.cb_frequency);
            this.gb_arrangement.Controls.Add(this.label4);
            this.gb_arrangement.Controls.Add(this.cb_colordepth);
            this.gb_arrangement.Controls.Add(this.label3);
            this.gb_arrangement.Controls.Add(this.cb_resolution);
            this.gb_arrangement.Controls.Add(this.label2);
            this.gb_arrangement.Enabled = false;
            this.gb_arrangement.Location = new System.Drawing.Point(11, 375);
            this.gb_arrangement.Name = "gb_arrangement";
            this.gb_arrangement.Size = new System.Drawing.Size(207, 184);
            this.gb_arrangement.TabIndex = 11;
            this.gb_arrangement.TabStop = false;
            this.gb_arrangement.Text = "Arrangement";
            // 
            // nud_y
            // 
            this.nud_y.Location = new System.Drawing.Point(141, 155);
            this.nud_y.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nud_y.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.nud_y.Name = "nud_y";
            this.nud_y.Size = new System.Drawing.Size(60, 20);
            this.nud_y.TabIndex = 24;
            this.nud_y.ValueChanged += new System.EventHandler(this.nud_y_ValueChanged);
            // 
            // nud_x
            // 
            this.nud_x.Location = new System.Drawing.Point(78, 155);
            this.nud_x.Maximum = new decimal(new int[] {
            999999,
            0,
            0,
            0});
            this.nud_x.Minimum = new decimal(new int[] {
            999999,
            0,
            0,
            -2147483648});
            this.nud_x.Name = "nud_x";
            this.nud_x.Size = new System.Drawing.Size(60, 20);
            this.nud_x.TabIndex = 23;
            this.nud_x.ValueChanged += new System.EventHandler(this.nud_x_ValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 157);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(51, 13);
            this.label11.TabIndex = 22;
            this.label11.Text = "Location:";
            // 
            // cb_clone
            // 
            this.cb_clone.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_clone.FormattingEnabled = true;
            this.cb_clone.Location = new System.Drawing.Point(78, 127);
            this.cb_clone.Name = "cb_clone";
            this.cb_clone.Size = new System.Drawing.Size(123, 21);
            this.cb_clone.TabIndex = 19;
            this.cb_clone.SelectionChangeCommitted += new System.EventHandler(this.cb_clone_SelectionChangeCommitted);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(6, 130);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(37, 13);
            this.label9.TabIndex = 20;
            this.label9.Text = "Clone:";
            // 
            // cb_rotation
            // 
            this.cb_rotation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_rotation.FormattingEnabled = true;
            this.cb_rotation.Location = new System.Drawing.Point(78, 100);
            this.cb_rotation.Name = "cb_rotation";
            this.cb_rotation.Size = new System.Drawing.Size(123, 21);
            this.cb_rotation.TabIndex = 17;
            this.cb_rotation.SelectionChangeCommitted += new System.EventHandler(this.cb_rotation_SelectionChangeCommitted);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 103);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(50, 13);
            this.label8.TabIndex = 18;
            this.label8.Text = "Rotation:";
            // 
            // cb_frequency
            // 
            this.cb_frequency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_frequency.FormattingEnabled = true;
            this.cb_frequency.Location = new System.Drawing.Point(78, 73);
            this.cb_frequency.Name = "cb_frequency";
            this.cb_frequency.Size = new System.Drawing.Size(123, 21);
            this.cb_frequency.TabIndex = 15;
            this.cb_frequency.SelectionChangeCommitted += new System.EventHandler(this.cb_frequency_SelectionChangeCommitted);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(60, 13);
            this.label4.TabIndex = 16;
            this.label4.Text = "Frequency:";
            // 
            // cb_colordepth
            // 
            this.cb_colordepth.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_colordepth.FormattingEnabled = true;
            this.cb_colordepth.Location = new System.Drawing.Point(78, 46);
            this.cb_colordepth.Name = "cb_colordepth";
            this.cb_colordepth.Size = new System.Drawing.Size(123, 21);
            this.cb_colordepth.TabIndex = 13;
            this.cb_colordepth.SelectionChangeCommitted += new System.EventHandler(this.cb_colordepth_SelectionChangeCommitted);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 49);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Color Depth:";
            // 
            // cb_resolution
            // 
            this.cb_resolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cb_resolution.FormattingEnabled = true;
            this.cb_resolution.Location = new System.Drawing.Point(78, 19);
            this.cb_resolution.Name = "cb_resolution";
            this.cb_resolution.Size = new System.Drawing.Size(123, 21);
            this.cb_resolution.TabIndex = 12;
            this.cb_resolution.SelectionChangeCommitted += new System.EventHandler(this.cb_resolution_SelectionChangeCommitted);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 13);
            this.label2.TabIndex = 12;
            this.label2.Text = "Resolution:";
            // 
            // gb_surround
            // 
            this.gb_surround.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.gb_surround.Controls.Add(this.cb_surround_applybezel);
            this.gb_surround.Controls.Add(this.btn_topo);
            this.gb_surround.Enabled = false;
            this.gb_surround.Location = new System.Drawing.Point(224, 375);
            this.gb_surround.Name = "gb_surround";
            this.gb_surround.Size = new System.Drawing.Size(207, 184);
            this.gb_surround.TabIndex = 17;
            this.gb_surround.TabStop = false;
            this.gb_surround.Text = "NVidia Surround";
            // 
            // cb_surround_applybezel
            // 
            this.cb_surround_applybezel.AutoSize = true;
            this.cb_surround_applybezel.Location = new System.Drawing.Point(6, 21);
            this.cb_surround_applybezel.Name = "cb_surround_applybezel";
            this.cb_surround_applybezel.Size = new System.Drawing.Size(183, 17);
            this.cb_surround_applybezel.TabIndex = 18;
            this.cb_surround_applybezel.Text = "Apply Bezel Corrected Resolution";
            this.cb_surround_applybezel.UseVisualStyleBackColor = true;
            this.cb_surround_applybezel.CheckedChanged += new System.EventHandler(this.cb_surround_applybezel_CheckedChanged);
            // 
            // btn_topo
            // 
            this.btn_topo.Enabled = false;
            this.btn_topo.Location = new System.Drawing.Point(116, 155);
            this.btn_topo.Name = "btn_topo";
            this.btn_topo.Size = new System.Drawing.Size(85, 23);
            this.btn_topo.TabIndex = 17;
            this.btn_topo.Text = "Edit Topology";
            this.btn_topo.UseVisualStyleBackColor = true;
            // 
            // gb_eyefinity
            // 
            this.gb_eyefinity.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_eyefinity.Controls.Add(this.label10);
            this.gb_eyefinity.Enabled = false;
            this.gb_eyefinity.Location = new System.Drawing.Point(437, 375);
            this.gb_eyefinity.Name = "gb_eyefinity";
            this.gb_eyefinity.Size = new System.Drawing.Size(207, 184);
            this.gb_eyefinity.TabIndex = 17;
            this.gb_eyefinity.TabStop = false;
            this.gb_eyefinity.Text = "AMD Eyefinity";
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(6, 20);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(195, 145);
            this.label10.TabIndex = 12;
            this.label10.Text = "Not Available";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // EditForm
            // 
            this.AcceptButton = this.btn_save;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(656, 607);
            this.Controls.Add(this.gb_eyefinity);
            this.Controls.Add(this.gb_surround);
            this.Controls.Add(this.gb_arrangement);
            this.Controls.Add(this.lv_monitors);
            this.Controls.Add(this.txt_name);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_cancel);
            this.Controls.Add(this.btn_apply);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.dv_profile);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Profile \'{0}\'";
            this.Load += new System.EventHandler(this.EditForm_Load);
            this.gb_arrangement.ResumeLayout(false);
            this.gb_arrangement.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nud_y)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nud_x)).EndInit();
            this.gb_surround.ResumeLayout(false);
            this.gb_surround.PerformLayout();
            this.gb_eyefinity.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private DisplayView dv_profile;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Button btn_apply;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.TextBox txt_name;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListView lv_monitors;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.GroupBox gb_arrangement;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox cb_resolution;
        private System.Windows.Forms.GroupBox gb_surround;
        private System.Windows.Forms.GroupBox gb_eyefinity;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button btn_topo;
        private System.Windows.Forms.ComboBox cb_clone;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox cb_rotation;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cb_frequency;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox cb_colordepth;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown nud_y;
        private System.Windows.Forms.NumericUpDown nud_x;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox cb_surround_applybezel;
    }
}