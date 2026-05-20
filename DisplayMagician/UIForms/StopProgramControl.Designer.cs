
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
            lbl_priority = new System.Windows.Forms.Label();
            pb_up_arrow = new System.Windows.Forms.PictureBox();
            pb_down_arrow = new System.Windows.Forms.PictureBox();
            lbl_stop_program = new System.Windows.Forms.Label();
            txt_stop_program = new System.Windows.Forms.TextBox();
            btn_stop_program = new System.Windows.Forms.Button();
            chk_disabled = new System.Windows.Forms.CheckBox();
            chk_restart_afterwards = new System.Windows.Forms.CheckBox();
            lbl_restart_priority = new System.Windows.Forms.Label();
            cbx_restart_priority = new System.Windows.Forms.ComboBox();
            chk_run_as_admin = new System.Windows.Forms.CheckBox();
            btn_delete = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pb_up_arrow).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pb_down_arrow).BeginInit();
            SuspendLayout();
            // 
            // lbl_priority
            // 
            lbl_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_priority.Location = new System.Drawing.Point(35, 46);
            lbl_priority.Name = "lbl_priority";
            lbl_priority.Size = new System.Drawing.Size(55, 23);
            lbl_priority.TabIndex = 0;
            lbl_priority.Text = "1";
            lbl_priority.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pb_up_arrow
            // 
            pb_up_arrow.Image = Properties.Resources.whitearrowsup;
            pb_up_arrow.Location = new System.Drawing.Point(40, 25);
            pb_up_arrow.Name = "pb_up_arrow";
            pb_up_arrow.Size = new System.Drawing.Size(45, 24);
            pb_up_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pb_up_arrow.TabIndex = 1;
            pb_up_arrow.TabStop = false;
            pb_up_arrow.Click += pb_up_arrow_Click;
            pb_up_arrow.MouseEnter += pb_up_arrow_MouseEnter;
            pb_up_arrow.MouseLeave += pb_up_arrow_MouseLeave;
            // 
            // pb_down_arrow
            // 
            pb_down_arrow.Image = Properties.Resources.whitearrows;
            pb_down_arrow.Location = new System.Drawing.Point(40, 73);
            pb_down_arrow.Name = "pb_down_arrow";
            pb_down_arrow.Size = new System.Drawing.Size(45, 24);
            pb_down_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pb_down_arrow.TabIndex = 2;
            pb_down_arrow.TabStop = false;
            pb_down_arrow.Click += pb_down_arrow_Click;
            pb_down_arrow.MouseEnter += pb_down_arrow_MouseEnter;
            pb_down_arrow.MouseLeave += pb_down_arrow_MouseLeave;
            // 
            // lbl_stop_program
            // 
            lbl_stop_program.AutoSize = true;
            lbl_stop_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_stop_program.Location = new System.Drawing.Point(129, 18);
            lbl_stop_program.Name = "lbl_stop_program";
            lbl_stop_program.Size = new System.Drawing.Size(139, 20);
            lbl_stop_program.TabIndex = 3;
            lbl_stop_program.Text = "Stop this program:";
            // 
            // txt_stop_program
            // 
            txt_stop_program.BackColor = System.Drawing.Color.White;
            txt_stop_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_stop_program.ForeColor = System.Drawing.Color.Black;
            txt_stop_program.Location = new System.Drawing.Point(275, 16);
            txt_stop_program.Name = "txt_stop_program";
            txt_stop_program.Size = new System.Drawing.Size(538, 26);
            txt_stop_program.TabIndex = 4;
            txt_stop_program.TextChanged += txt_stop_program_TextChanged;
            // 
            // btn_stop_program
            // 
            btn_stop_program.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_stop_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_stop_program.ForeColor = System.Drawing.Color.White;
            btn_stop_program.Location = new System.Drawing.Point(819, 15);
            btn_stop_program.Name = "btn_stop_program";
            btn_stop_program.Size = new System.Drawing.Size(85, 27);
            btn_stop_program.TabIndex = 5;
            btn_stop_program.Text = "Choose";
            btn_stop_program.UseVisualStyleBackColor = true;
            btn_stop_program.Click += btn_stop_program_Click;
            // 
            // chk_disabled
            // 
            chk_disabled.AutoSize = true;
            chk_disabled.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            chk_disabled.ForeColor = System.Drawing.Color.White;
            chk_disabled.Location = new System.Drawing.Point(159, 115);
            chk_disabled.Name = "chk_disabled";
            chk_disabled.Size = new System.Drawing.Size(320, 24);
            chk_disabled.TabIndex = 6;
            chk_disabled.Text = "Temporarily disable stopping this program";
            chk_disabled.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            chk_disabled.UseVisualStyleBackColor = true;
            chk_disabled.CheckedChanged += chk_disabled_CheckedChanged;
            // 
            // chk_restart_afterwards
            // 
            chk_restart_afterwards.AutoSize = true;
            chk_restart_afterwards.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            chk_restart_afterwards.ForeColor = System.Drawing.Color.White;
            chk_restart_afterwards.Location = new System.Drawing.Point(159, 50);
            chk_restart_afterwards.Name = "chk_restart_afterwards";
            chk_restart_afterwards.Size = new System.Drawing.Size(288, 24);
            chk_restart_afterwards.TabIndex = 7;
            chk_restart_afterwards.Text = "Restart program when game finishes";
            chk_restart_afterwards.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            chk_restart_afterwards.UseVisualStyleBackColor = true;
            chk_restart_afterwards.CheckedChanged += chk_restart_afterwards_CheckedChanged;
            // 
            // lbl_restart_priority
            // 
            lbl_restart_priority.AutoSize = true;
            lbl_restart_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_restart_priority.ForeColor = System.Drawing.Color.White;
            lbl_restart_priority.Location = new System.Drawing.Point(563, 53);
            lbl_restart_priority.Name = "lbl_restart_priority";
            lbl_restart_priority.Size = new System.Drawing.Size(117, 20);
            lbl_restart_priority.TabIndex = 8;
            lbl_restart_priority.Text = "Restart Priority:";
            // 
            // cbx_restart_priority
            // 
            cbx_restart_priority.AllowDrop = true;
            cbx_restart_priority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbx_restart_priority.Enabled = false;
            cbx_restart_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cbx_restart_priority.FormattingEnabled = true;
            cbx_restart_priority.Location = new System.Drawing.Point(725, 50);
            cbx_restart_priority.Name = "cbx_restart_priority";
            cbx_restart_priority.Size = new System.Drawing.Size(179, 28);
            cbx_restart_priority.TabIndex = 9;
            cbx_restart_priority.SelectedIndexChanged += cbx_restart_priority_SelectedIndexChanged;
            // 
            // chk_run_as_admin
            // 
            chk_run_as_admin.AutoSize = true;
            chk_run_as_admin.Enabled = false;
            chk_run_as_admin.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            chk_run_as_admin.ForeColor = System.Drawing.Color.White;
            chk_run_as_admin.Location = new System.Drawing.Point(159, 83);
            chk_run_as_admin.Name = "chk_run_as_admin";
            chk_run_as_admin.Size = new System.Drawing.Size(288, 24);
            chk_run_as_admin.TabIndex = 10;
            chk_run_as_admin.Text = "Run as administrator when restarting";
            chk_run_as_admin.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            chk_run_as_admin.UseVisualStyleBackColor = true;
            chk_run_as_admin.CheckedChanged += chk_run_as_admin_CheckedChanged;
            chk_run_as_admin.Paint += checkbox_Paint;
            // 
            // btn_delete
            // 
            btn_delete.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_delete.ForeColor = System.Drawing.Color.White;
            btn_delete.Location = new System.Drawing.Point(996, 7);
            btn_delete.Name = "btn_delete";
            btn_delete.Size = new System.Drawing.Size(29, 27);
            btn_delete.TabIndex = 11;
            btn_delete.Text = "X";
            btn_delete.UseVisualStyleBackColor = true;
            btn_delete.Click += btn_delete_Click;
            // 
            // StopProgramControl
            // 
            AllowDrop = true;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            AutoSize = true;
            BackColor = System.Drawing.Color.Black;
            Controls.Add(btn_delete);
            Controls.Add(chk_run_as_admin);
            Controls.Add(cbx_restart_priority);
            Controls.Add(lbl_restart_priority);
            Controls.Add(chk_restart_afterwards);
            Controls.Add(chk_disabled);
            Controls.Add(btn_stop_program);
            Controls.Add(txt_stop_program);
            Controls.Add(lbl_stop_program);
            Controls.Add(pb_down_arrow);
            Controls.Add(pb_up_arrow);
            Controls.Add(lbl_priority);
            ForeColor = System.Drawing.Color.White;
            Margin = new System.Windows.Forms.Padding(10);
            MinimumSize = new System.Drawing.Size(800, 155);
            Name = "StopProgramControl";
            Size = new System.Drawing.Size(1036, 155);
            ((System.ComponentModel.ISupportInitialize)pb_up_arrow).EndInit();
            ((System.ComponentModel.ISupportInitialize)pb_down_arrow).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
