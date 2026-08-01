
namespace DisplayMagician.UIForms
{
    partial class StartProgramControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            cb_dont_start_if_running = new System.Windows.Forms.CheckBox();
            txt_start_program = new System.Windows.Forms.TextBox();
            cb_start_program_close = new System.Windows.Forms.CheckBox();
            btn_start_program = new System.Windows.Forms.Button();
            txt_start_program_args = new System.Windows.Forms.TextBox();
            cb_start_program_pass_args = new System.Windows.Forms.CheckBox();
            btn_delete = new System.Windows.Forms.Button();
            cb_disable_start_program = new System.Windows.Forms.CheckBox();
            lbl_start_program = new System.Windows.Forms.Label();
            lbl_priority = new System.Windows.Forms.Label();
            pb_up_arrow = new System.Windows.Forms.PictureBox();
            pb_down_arrow = new System.Windows.Forms.PictureBox();
            cbx_start_program_priority = new System.Windows.Forms.ComboBox();
            lbl_start_program_priority = new System.Windows.Forms.Label();
            cb_run_as_administrator = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)pb_up_arrow).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pb_down_arrow).BeginInit();
            SuspendLayout();
            // 
            // cb_dont_start_if_running
            // 
            cb_dont_start_if_running.AutoSize = true;
            cb_dont_start_if_running.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cb_dont_start_if_running.ForeColor = System.Drawing.Color.White;
            cb_dont_start_if_running.Location = new System.Drawing.Point(160, 112);
            cb_dont_start_if_running.Name = "cb_dont_start_if_running";
            cb_dont_start_if_running.Size = new System.Drawing.Size(289, 24);
            cb_dont_start_if_running.TabIndex = 26;
            cb_dont_start_if_running.Text = "Don't start if program already running";
            cb_dont_start_if_running.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_dont_start_if_running.UseVisualStyleBackColor = true;
            cb_dont_start_if_running.CheckedChanged += cb_dont_start_if_running_CheckedChanged;
            // 
            // txt_start_program
            // 
            txt_start_program.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txt_start_program.BackColor = System.Drawing.Color.White;
            txt_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_start_program.ForeColor = System.Drawing.Color.Black;
            txt_start_program.Location = new System.Drawing.Point(275, 16);
            txt_start_program.Name = "txt_start_program";
            txt_start_program.Size = new System.Drawing.Size(710, 26);
            txt_start_program.TabIndex = 25;
            txt_start_program.TextChanged += txt_start_program_TextChanged;
            // 
            // cb_start_program_close
            // 
            cb_start_program_close.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cb_start_program_close.AutoSize = true;
            cb_start_program_close.Checked = true;
            cb_start_program_close.CheckState = System.Windows.Forms.CheckState.Checked;
            cb_start_program_close.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cb_start_program_close.ForeColor = System.Drawing.Color.White;
            cb_start_program_close.Location = new System.Drawing.Point(758, 120);
            cb_start_program_close.Name = "cb_start_program_close";
            cb_start_program_close.Size = new System.Drawing.Size(290, 44);
            cb_start_program_close.TabIndex = 24;
            cb_start_program_close.Text = "Close started program when finished \r\n(unless already running)";
            cb_start_program_close.UseVisualStyleBackColor = true;
            cb_start_program_close.CheckedChanged += cb_start_program_close_CheckedChanged;
            // 
            // btn_start_program
            // 
            btn_start_program.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_start_program.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_start_program.ForeColor = System.Drawing.Color.White;
            btn_start_program.Location = new System.Drawing.Point(995, 15);
            btn_start_program.Name = "btn_start_program";
            btn_start_program.Size = new System.Drawing.Size(85, 27);
            btn_start_program.TabIndex = 23;
            btn_start_program.Text = "Choose";
            btn_start_program.UseVisualStyleBackColor = true;
            btn_start_program.Click += btn_start_program_Click;
            // 
            // txt_start_program_args
            // 
            txt_start_program_args.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txt_start_program_args.BackColor = System.Drawing.Color.White;
            txt_start_program_args.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_start_program_args.ForeColor = System.Drawing.Color.Black;
            txt_start_program_args.Location = new System.Drawing.Point(398, 50);
            txt_start_program_args.Name = "txt_start_program_args";
            txt_start_program_args.Size = new System.Drawing.Size(587, 26);
            txt_start_program_args.TabIndex = 22;
            txt_start_program_args.TextChanged += txt_start_program_args_TextChanged;
            // 
            // cb_start_program_pass_args
            // 
            cb_start_program_pass_args.AutoSize = true;
            cb_start_program_pass_args.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cb_start_program_pass_args.ForeColor = System.Drawing.Color.White;
            cb_start_program_pass_args.Location = new System.Drawing.Point(160, 50);
            cb_start_program_pass_args.Name = "cb_start_program_pass_args";
            cb_start_program_pass_args.Size = new System.Drawing.Size(228, 24);
            cb_start_program_pass_args.TabIndex = 21;
            cb_start_program_pass_args.Text = "Pass arguments to program:";
            cb_start_program_pass_args.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_start_program_pass_args.UseVisualStyleBackColor = true;
            cb_start_program_pass_args.CheckedChanged += cb_start_program_pass_args_CheckedChanged;
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
            btn_delete.TabIndex = 27;
            btn_delete.Text = "X";
            btn_delete.UseVisualStyleBackColor = true;
            btn_delete.Click += btn_delete_Click;
            // 
            // cb_disable_start_program
            // 
            cb_disable_start_program.AutoSize = true;
            cb_disable_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cb_disable_start_program.ForeColor = System.Drawing.Color.White;
            cb_disable_start_program.Location = new System.Drawing.Point(159, 143);
            cb_disable_start_program.Name = "cb_disable_start_program";
            cb_disable_start_program.Size = new System.Drawing.Size(312, 24);
            cb_disable_start_program.TabIndex = 28;
            cb_disable_start_program.Text = "Temporarily disable starting this program";
            cb_disable_start_program.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_disable_start_program.UseVisualStyleBackColor = true;
            cb_disable_start_program.CheckedChanged += cb_start_program_CheckedChanged;
            // 
            // lbl_start_program
            // 
            lbl_start_program.AutoSize = true;
            lbl_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_start_program.Location = new System.Drawing.Point(129, 18);
            lbl_start_program.Name = "lbl_start_program";
            lbl_start_program.Size = new System.Drawing.Size(140, 20);
            lbl_start_program.TabIndex = 29;
            lbl_start_program.Text = "Start this program:";
            // 
            // lbl_priority
            // 
            lbl_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_priority.Location = new System.Drawing.Point(35, 59);
            lbl_priority.Name = "lbl_priority";
            lbl_priority.Size = new System.Drawing.Size(55, 23);
            lbl_priority.TabIndex = 30;
            lbl_priority.Text = "1";
            lbl_priority.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pb_up_arrow
            // 
            pb_up_arrow.Image = Properties.Resources.whitearrowsup;
            pb_up_arrow.Location = new System.Drawing.Point(40, 38);
            pb_up_arrow.Name = "pb_up_arrow";
            pb_up_arrow.Size = new System.Drawing.Size(45, 24);
            pb_up_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pb_up_arrow.TabIndex = 32;
            pb_up_arrow.TabStop = false;
            pb_up_arrow.Click += pb_up_arrow_Click;
            pb_up_arrow.MouseEnter += pb_up_arrow_MouseEnter;
            pb_up_arrow.MouseLeave += pb_up_arrow_MouseLeave;
            // 
            // pb_down_arrow
            // 
            pb_down_arrow.Image = Properties.Resources.whitearrows;
            pb_down_arrow.Location = new System.Drawing.Point(40, 86);
            pb_down_arrow.Name = "pb_down_arrow";
            pb_down_arrow.Size = new System.Drawing.Size(45, 24);
            pb_down_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            pb_down_arrow.TabIndex = 31;
            pb_down_arrow.TabStop = false;
            pb_down_arrow.Click += pb_down_arrow_Click;
            pb_down_arrow.MouseEnter += pb_down_arrow_MouseEnter;
            pb_down_arrow.MouseLeave += pb_down_arrow_MouseLeave;
            // 
            // cbx_start_program_priority
            // 
            cbx_start_program_priority.AllowDrop = true;
            cbx_start_program_priority.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cbx_start_program_priority.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cbx_start_program_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cbx_start_program_priority.FormattingEnabled = true;
            cbx_start_program_priority.Location = new System.Drawing.Point(901, 84);
            cbx_start_program_priority.Name = "cbx_start_program_priority";
            cbx_start_program_priority.Size = new System.Drawing.Size(179, 28);
            cbx_start_program_priority.TabIndex = 34;
            cbx_start_program_priority.SelectedIndexChanged += cbx_start_program_priority_SelectedIndexChanged;
            // 
            // lbl_start_program_priority
            // 
            lbl_start_program_priority.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            lbl_start_program_priority.AutoSize = true;
            lbl_start_program_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_start_program_priority.ForeColor = System.Drawing.Color.White;
            lbl_start_program_priority.Location = new System.Drawing.Point(739, 87);
            lbl_start_program_priority.Name = "lbl_start_program_priority";
            lbl_start_program_priority.Size = new System.Drawing.Size(163, 20);
            lbl_start_program_priority.TabIndex = 33;
            lbl_start_program_priority.Text = "Start Program Priority:";
            // 
            // cb_run_as_administrator
            // 
            cb_run_as_administrator.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            cb_run_as_administrator.AutoSize = true;
            cb_run_as_administrator.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            cb_run_as_administrator.ForeColor = System.Drawing.Color.White;
            cb_run_as_administrator.Location = new System.Drawing.Point(785, 84);
            cb_run_as_administrator.Name = "cb_run_as_administrator";
            cb_run_as_administrator.Size = new System.Drawing.Size(238, 24);
            cb_run_as_administrator.TabIndex = 35;
            cb_run_as_administrator.Text = "Run program as administrator";
            cb_run_as_administrator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            cb_run_as_administrator.UseVisualStyleBackColor = true;
            cb_run_as_administrator.CheckedChanged += cb_run_as_administrator_CheckedChanged;
            // 
            // StartProgramControl
            // 
            AllowDrop = true;
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            AutoSize = true;
            BackColor = System.Drawing.Color.Black;
            Controls.Add(cb_run_as_administrator);
            Controls.Add(cbx_start_program_priority);
            Controls.Add(lbl_start_program_priority);
            Controls.Add(pb_up_arrow);
            Controls.Add(pb_down_arrow);
            Controls.Add(lbl_priority);
            Controls.Add(lbl_start_program);
            Controls.Add(cb_disable_start_program);
            Controls.Add(btn_delete);
            Controls.Add(cb_dont_start_if_running);
            Controls.Add(txt_start_program);
            Controls.Add(cb_start_program_close);
            Controls.Add(btn_start_program);
            Controls.Add(txt_start_program_args);
            Controls.Add(cb_start_program_pass_args);
            ForeColor = System.Drawing.Color.White;
            Margin = new System.Windows.Forms.Padding(10);
            MinimumSize = new System.Drawing.Size(800, 185);
            Name = "StartProgramControl";
            Size = new System.Drawing.Size(1090, 185);
            ((System.ComponentModel.ISupportInitialize)pb_up_arrow).EndInit();
            ((System.ComponentModel.ISupportInitialize)pb_down_arrow).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cb_dont_start_if_running;
        private System.Windows.Forms.TextBox txt_start_program;
        private System.Windows.Forms.CheckBox cb_start_program_close;
        private System.Windows.Forms.Button btn_start_program;
        private System.Windows.Forms.TextBox txt_start_program_args;
        private System.Windows.Forms.CheckBox cb_start_program_pass_args;
        private System.Windows.Forms.Button btn_delete;
        private System.Windows.Forms.CheckBox cb_disable_start_program;
        private System.Windows.Forms.Label lbl_start_program;
        private System.Windows.Forms.Label lbl_priority;
        private System.Windows.Forms.PictureBox pb_down_arrow;
        private System.Windows.Forms.PictureBox pb_up_arrow;
        private System.Windows.Forms.ComboBox cbx_start_program_priority;
        private System.Windows.Forms.Label lbl_start_program_priority;
        private System.Windows.Forms.CheckBox cb_run_as_administrator;
    }
}

