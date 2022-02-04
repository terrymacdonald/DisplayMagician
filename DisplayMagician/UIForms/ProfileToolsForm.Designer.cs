
namespace DisplayMagician.UIForms
{
    partial class ProfileToolsForm
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
            this.btn_cancel = new System.Windows.Forms.Button();
            this.gb_taskbars = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmb_taskbar_edge = new System.Windows.Forms.ComboBox();
            this.btn_apply = new System.Windows.Forms.Button();
            this.gb_taskbars.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_cancel.ForeColor = System.Drawing.Color.White;
            this.btn_cancel.Location = new System.Drawing.Point(480, 145);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(94, 25);
            this.btn_cancel.TabIndex = 6;
            this.btn_cancel.Text = "&Back";
            this.btn_cancel.UseVisualStyleBackColor = true;
            this.btn_cancel.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // gb_taskbars
            // 
            this.gb_taskbars.Controls.Add(this.btn_apply);
            this.gb_taskbars.Controls.Add(this.label2);
            this.gb_taskbars.Controls.Add(this.label1);
            this.gb_taskbars.Controls.Add(this.cmb_taskbar_edge);
            this.gb_taskbars.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gb_taskbars.ForeColor = System.Drawing.SystemColors.Control;
            this.gb_taskbars.Location = new System.Drawing.Point(22, 22);
            this.gb_taskbars.Name = "gb_taskbars";
            this.gb_taskbars.Size = new System.Drawing.Size(552, 100);
            this.gb_taskbars.TabIndex = 7;
            this.gb_taskbars.TabStop = false;
            this.gb_taskbars.Text = "Taskbar Tools";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(316, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 16);
            this.label2.TabIndex = 2;
            this.label2.Text = "of the screen";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(158, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Move all Taskbars to the ";
            // 
            // cmb_taskbar_edge
            // 
            this.cmb_taskbar_edge.FormattingEnabled = true;
            this.cmb_taskbar_edge.Location = new System.Drawing.Point(189, 43);
            this.cmb_taskbar_edge.Name = "cmb_taskbar_edge";
            this.cmb_taskbar_edge.Size = new System.Drawing.Size(121, 24);
            this.cmb_taskbar_edge.TabIndex = 0;
            // 
            // btn_apply
            // 
            this.btn_apply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_apply.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_apply.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_apply.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_apply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_apply.ForeColor = System.Drawing.Color.White;
            this.btn_apply.Location = new System.Drawing.Point(417, 42);
            this.btn_apply.Name = "btn_apply";
            this.btn_apply.Size = new System.Drawing.Size(94, 25);
            this.btn_apply.TabIndex = 7;
            this.btn_apply.Text = "&Apply";
            this.btn_apply.UseVisualStyleBackColor = true;
            this.btn_apply.Click += new System.EventHandler(this.btn_apply_Click);
            // 
            // ProfileToolsForm
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(598, 193);
            this.Controls.Add(this.gb_taskbars);
            this.Controls.Add(this.btn_cancel);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "ProfileToolsForm";
            this.ShowIcon = false;
            this.Text = "Profile Tools";
            this.gb_taskbars.ResumeLayout(false);
            this.gb_taskbars.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.GroupBox gb_general;
        private System.Windows.Forms.Label lbl_style;
        private System.Windows.Forms.ComboBox cmb_wallpaper_display_mode;
        private System.Windows.Forms.Button btn_select;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.PictureBox pb_wallpaper;
        private System.Windows.Forms.Button btn_current;
        private System.Windows.Forms.RadioButton rb_leave_wallpaper;
        private System.Windows.Forms.RadioButton rb_clear_wallpaper;
        private System.Windows.Forms.RadioButton rb_apply_wallpaper;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.GroupBox gb_taskbars;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmb_taskbar_edge;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_apply;
    }
}