
namespace DisplayMagician.UIForms
{
    partial class StopProgramControl
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        private void InitializeComponent()
        {
            this.lbl_priority = new System.Windows.Forms.Label();
            this.pb_up_arrow = new System.Windows.Forms.PictureBox();
            this.pb_down_arrow = new System.Windows.Forms.PictureBox();
            this.lbl_stop_program = new System.Windows.Forms.Label();
            this.txt_stop_program = new System.Windows.Forms.TextBox();
            this.btn_stop_program = new System.Windows.Forms.Button();
            this.chk_disabled = new System.Windows.Forms.CheckBox();
            this.chk_restart_afterwards = new System.Windows.Forms.CheckBox();
            this.lbl_restart_priority = new System.Windows.Forms.Label();
            this.cbx_restart_priority = new System.Windows.Forms.ComboBox();
            this.chk_run_as_admin = new System.Windows.Forms.CheckBox();
            this.btn_delete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pb_up_arrow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).BeginInit();
            this.SuspendLayout();
            // 
            // lbl_priority
            // 
            this.lbl_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_priority.Location = new System.Drawing.Point(35, 46);
            this.lbl_priority.Name = "lbl_priority";
            this.lbl_priority.Size = new System.Drawing.Size(55, 23);
            this.lbl_priority.TabIndex = 0;
            this.lbl_priority.Text = "1";
            this.lbl_priority.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pb_up_arrow
            // 
            this.pb_up_arrow.Image = global::DisplayMagician.Properties.Resources.whitearrowsup;
            this.pb_up_arrow.Location = new System.Drawing.Point(40, 25);
            this.pb_up_arrow.Name = "pb_up_arrow";
            this.pb_up_arrow.Size = new System.Drawing.Size(45, 24);
            this.pb_up_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_up_arrow.TabIndex = 1;
            this.pb_up_arrow.TabStop = false;
            this.pb_up_arrow.Click += new System.EventHandler(this.pb_up_arrow_Click);
            this.pb_up_arrow.MouseEnter += new System.EventHandler(this.pb_up_arrow_MouseEnter);
            this.pb_up_arrow.MouseLeave += new System.EventHandler(this.pb_up_arrow_MouseLeave);
            // 
            // pb_down_arrow
            // 
            this.pb_down_arrow.Image = global::DisplayMagician.Properties.Resources.whitearrows;
            this.pb_down_arrow.Location = new System.Drawing.Point(40, 73);
            this.pb_down_arrow.Name = "pb_down_arrow";
            this.pb_down_arrow.Size = new System.Drawing.Size(45, 24);
            this.pb_down_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_down_arrow.TabIndex = 2;
            this.pb_down_arrow.TabStop = false;
            this.pb_down_arrow.Click += new System.EventHandler(this.pb_down_arrow_Click);
            this.pb_down_arrow.MouseEnter += new System.EventHandler(this.pb_down_arrow_MouseEnter);
            this.pb_down_arrow.MouseLeave += new System.EventHandler(this.pb_down_arrow_MouseLeave);
            // 
            // lbl_stop_program
            // 
            this.lbl_stop_program.AutoSize = true;
            this.lbl_stop_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_stop_program.Location = new System.Drawing.Point(129, 18);
            this.lbl_stop_program.Name = "lbl_stop_program";
            this.lbl_stop_program.Size = new System.Drawing.Size(140, 20);
            this.lbl_stop_program.TabIndex = 3;
            this.lbl_stop_program.Text = "Stop this program:";
            // 
            // txt_stop_program
            // 
            this.txt_stop_program.BackColor = System.Drawing.Color.White;
            this.txt_stop_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_stop_program.ForeColor = System.Drawing.Color.Black;
            this.txt_stop_program.Location = new System.Drawing.Point(275, 16);
            this.txt_stop_program.Name = "txt_stop_program";
            this.txt_stop_program.Size = new System.Drawing.Size(538, 26);
            this.txt_stop_program.TabIndex = 4;
            this.txt_stop_program.TextChanged += new System.EventHandler(this.txt_stop_program_TextChanged);
            // 
            // btn_stop_program
            // 
            this.btn_stop_program.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_stop_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_stop_program.ForeColor = System.Drawing.Color.White;
            this.btn_stop_program.Location = new System.Drawing.Point(819, 15);
            this.btn_stop_program.Name = "btn_stop_program";
            this.btn_stop_program.Size = new System.Drawing.Size(85, 27);
            this.btn_stop_program.TabIndex = 5;
            this.btn_stop_program.Text = "Choose";
            this.btn_stop_program.UseVisualStyleBackColor = true;
            this.btn_stop_program.Click += new System.EventHandler(this.btn_stop_program_Click);
            // 
            // chk_disabled
            // 
            this.chk_disabled.AutoSize = true;
            this.chk_disabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk_disabled.ForeColor = System.Drawing.Color.White;
            this.chk_disabled.Location = new System.Drawing.Point(159, 115);
            this.chk_disabled.Name = "chk_disabled";
            this.chk_disabled.Size = new System.Drawing.Size(312, 24);
            this.chk_disabled.TabIndex = 6;
            this.chk_disabled.Text = "Temporarily disable stopping this program";
            this.chk_disabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chk_disabled.UseVisualStyleBackColor = true;
            this.chk_disabled.CheckedChanged += new System.EventHandler(this.chk_disabled_CheckedChanged);
            // 
            // chk_restart_afterwards
            // 
            this.chk_restart_afterwards.AutoSize = true;
            this.chk_restart_afterwards.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk_restart_afterwards.ForeColor = System.Drawing.Color.White;
            this.chk_restart_afterwards.Location = new System.Drawing.Point(159, 50);
            this.chk_restart_afterwards.Name = "chk_restart_afterwards";
            this.chk_restart_afterwards.Size = new System.Drawing.Size(267, 24);
            this.chk_restart_afterwards.TabIndex = 7;
            this.chk_restart_afterwards.Text = "Restart program when game finishes";
            this.chk_restart_afterwards.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chk_restart_afterwards.UseVisualStyleBackColor = true;
            this.chk_restart_afterwards.CheckedChanged += new System.EventHandler(this.chk_restart_afterwards_CheckedChanged);
            // 
            // lbl_restart_priority
            // 
            this.lbl_restart_priority.AutoSize = true;
            this.lbl_restart_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_restart_priority.ForeColor = System.Drawing.Color.White;
            this.lbl_restart_priority.Location = new System.Drawing.Point(563, 53);
            this.lbl_restart_priority.Name = "lbl_restart_priority";
            this.lbl_restart_priority.Size = new System.Drawing.Size(163, 20);
            this.lbl_restart_priority.TabIndex = 8;
            this.lbl_restart_priority.Text = "Restart Priority:";
            // 
            // cbx_restart_priority
            // 
            this.cbx_restart_priority.AllowDrop = true;
            this.cbx_restart_priority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbx_restart_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbx_restart_priority.FormattingEnabled = true;
            this.cbx_restart_priority.Location = new System.Drawing.Point(725, 50);
            this.cbx_restart_priority.Name = "cbx_restart_priority";
            this.cbx_restart_priority.Size = new System.Drawing.Size(179, 28);
            this.cbx_restart_priority.TabIndex = 9;
            this.cbx_restart_priority.Enabled = false;
            this.cbx_restart_priority.SelectedIndexChanged += new System.EventHandler(this.cbx_restart_priority_SelectedIndexChanged);
            // 
            // chk_run_as_admin
            // 
            this.chk_run_as_admin.AutoSize = true;
            this.chk_run_as_admin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chk_run_as_admin.ForeColor = System.Drawing.Color.White;
            this.chk_run_as_admin.Location = new System.Drawing.Point(159, 83);
            this.chk_run_as_admin.Name = "chk_run_as_admin";
            this.chk_run_as_admin.Size = new System.Drawing.Size(267, 24);
            this.chk_run_as_admin.TabIndex = 10;
            this.chk_run_as_admin.Text = "Run as administrator when restarting";
            this.chk_run_as_admin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chk_run_as_admin.Enabled = false;
            this.chk_run_as_admin.UseVisualStyleBackColor = true;
            this.chk_run_as_admin.CheckedChanged += new System.EventHandler(this.chk_run_as_admin_CheckedChanged);
            // 
            // btn_delete
            // 
            this.btn_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_delete.ForeColor = System.Drawing.Color.White;
            this.btn_delete.Location = new System.Drawing.Point(996, 7);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(29, 27);
            this.btn_delete.TabIndex = 11;
            this.btn_delete.Text = "X";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // StopProgramControl
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.chk_run_as_admin);
            this.Controls.Add(this.cbx_restart_priority);
            this.Controls.Add(this.lbl_restart_priority);
            this.Controls.Add(this.chk_restart_afterwards);
            this.Controls.Add(this.chk_disabled);
            this.Controls.Add(this.btn_stop_program);
            this.Controls.Add(this.txt_stop_program);
            this.Controls.Add(this.lbl_stop_program);
            this.Controls.Add(this.pb_down_arrow);
            this.Controls.Add(this.pb_up_arrow);
            this.Controls.Add(this.lbl_priority);
            this.ForeColor = System.Drawing.Color.White;
            this.Margin = new System.Windows.Forms.Padding(10);
            this.Name = "StopProgramControl";
            this.Size = new System.Drawing.Size(1036, 150);
            ((System.ComponentModel.ISupportInitialize)(this.pb_up_arrow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lbl_priority;
        private System.Windows.Forms.PictureBox pb_up_arrow;
        private System.Windows.Forms.PictureBox pb_down_arrow;
        private System.Windows.Forms.Label lbl_stop_program;
        private System.Windows.Forms.TextBox txt_stop_program;
        private System.Windows.Forms.Button btn_stop_program;
        private System.Windows.Forms.CheckBox chk_disabled;
        private System.Windows.Forms.CheckBox chk_restart_afterwards;
        private System.Windows.Forms.Label lbl_restart_priority;
        private System.Windows.Forms.ComboBox cbx_restart_priority;
        private System.Windows.Forms.CheckBox chk_run_as_admin;
        private System.Windows.Forms.Button btn_delete;
    }
}
