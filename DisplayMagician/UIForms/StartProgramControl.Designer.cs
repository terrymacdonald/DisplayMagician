
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
            this.cb_dont_start_if_running = new System.Windows.Forms.CheckBox();
            this.txt_start_program = new System.Windows.Forms.TextBox();
            this.cb_start_program_close = new System.Windows.Forms.CheckBox();
            this.btn_start_program = new System.Windows.Forms.Button();
            this.txt_start_program_args = new System.Windows.Forms.TextBox();
            this.cb_start_program_pass_args = new System.Windows.Forms.CheckBox();
            this.btn_delete = new System.Windows.Forms.Button();
            this.cb_disable_start_program = new System.Windows.Forms.CheckBox();
            this.lbl_start_program = new System.Windows.Forms.Label();
            this.lbl_priority = new System.Windows.Forms.Label();
            this.pb_up_arrow = new System.Windows.Forms.PictureBox();
            this.pb_down_arrow = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pb_up_arrow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).BeginInit();
            this.SuspendLayout();
            // 
            // cb_dont_start_if_running
            // 
            this.cb_dont_start_if_running.AutoSize = true;
            this.cb_dont_start_if_running.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_dont_start_if_running.ForeColor = System.Drawing.Color.White;
            this.cb_dont_start_if_running.Location = new System.Drawing.Point(160, 86);
            this.cb_dont_start_if_running.Name = "cb_dont_start_if_running";
            this.cb_dont_start_if_running.Size = new System.Drawing.Size(289, 24);
            this.cb_dont_start_if_running.TabIndex = 26;
            this.cb_dont_start_if_running.Text = "Don\'t start if program already running";
            this.cb_dont_start_if_running.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_dont_start_if_running.UseVisualStyleBackColor = true;
            this.cb_dont_start_if_running.CheckedChanged += new System.EventHandler(this.cb_dont_start_if_running_CheckedChanged);
            // 
            // txt_start_program
            // 
            this.txt_start_program.BackColor = System.Drawing.Color.White;
            this.txt_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_start_program.ForeColor = System.Drawing.Color.Black;
            this.txt_start_program.Location = new System.Drawing.Point(275, 16);
            this.txt_start_program.Name = "txt_start_program";
            this.txt_start_program.Size = new System.Drawing.Size(538, 26);
            this.txt_start_program.TabIndex = 25;
            this.txt_start_program.TextChanged += new System.EventHandler(this.txt_start_program_TextChanged);
            // 
            // cb_start_program_close
            // 
            this.cb_start_program_close.AutoSize = true;
            this.cb_start_program_close.Checked = true;
            this.cb_start_program_close.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cb_start_program_close.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_start_program_close.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_close.Location = new System.Drawing.Point(506, 86);
            this.cb_start_program_close.Name = "cb_start_program_close";
            this.cb_start_program_close.Size = new System.Drawing.Size(398, 24);
            this.cb_start_program_close.TabIndex = 24;
            this.cb_start_program_close.Text = "Close started program when you finish playing Game";
            this.cb_start_program_close.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_close.UseVisualStyleBackColor = true;
            this.cb_start_program_close.CheckedChanged += new System.EventHandler(this.cb_start_program_close_CheckedChanged);
            // 
            // btn_start_program
            // 
            this.btn_start_program.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_start_program.ForeColor = System.Drawing.Color.White;
            this.btn_start_program.Location = new System.Drawing.Point(819, 15);
            this.btn_start_program.Name = "btn_start_program";
            this.btn_start_program.Size = new System.Drawing.Size(85, 27);
            this.btn_start_program.TabIndex = 23;
            this.btn_start_program.Text = "Choose";
            this.btn_start_program.UseVisualStyleBackColor = true;
            this.btn_start_program.Click += new System.EventHandler(this.btn_start_program_Click);
            // 
            // txt_start_program_args
            // 
            this.txt_start_program_args.BackColor = System.Drawing.Color.White;
            this.txt_start_program_args.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_start_program_args.ForeColor = System.Drawing.Color.Black;
            this.txt_start_program_args.Location = new System.Drawing.Point(398, 56);
            this.txt_start_program_args.Name = "txt_start_program_args";
            this.txt_start_program_args.Size = new System.Drawing.Size(506, 26);
            this.txt_start_program_args.TabIndex = 22;
            this.txt_start_program_args.TextChanged += new System.EventHandler(this.txt_start_program_args_TextChanged);
            // 
            // cb_start_program_pass_args
            // 
            this.cb_start_program_pass_args.AutoSize = true;
            this.cb_start_program_pass_args.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_start_program_pass_args.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_pass_args.Location = new System.Drawing.Point(160, 56);
            this.cb_start_program_pass_args.Name = "cb_start_program_pass_args";
            this.cb_start_program_pass_args.Size = new System.Drawing.Size(228, 24);
            this.cb_start_program_pass_args.TabIndex = 21;
            this.cb_start_program_pass_args.Text = "Pass arguments to program:";
            this.cb_start_program_pass_args.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_pass_args.UseVisualStyleBackColor = true;
            this.cb_start_program_pass_args.CheckedChanged += new System.EventHandler(this.cb_start_program_pass_args_CheckedChanged);
            // 
            // btn_delete
            // 
            this.btn_delete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_delete.ForeColor = System.Drawing.Color.White;
            this.btn_delete.Location = new System.Drawing.Point(1004, 3);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(29, 27);
            this.btn_delete.TabIndex = 27;
            this.btn_delete.Text = "X";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // cb_disable_start_program
            // 
            this.cb_disable_start_program.AutoSize = true;
            this.cb_disable_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_disable_start_program.ForeColor = System.Drawing.Color.White;
            this.cb_disable_start_program.Location = new System.Drawing.Point(160, 114);
            this.cb_disable_start_program.Name = "cb_disable_start_program";
            this.cb_disable_start_program.Size = new System.Drawing.Size(312, 24);
            this.cb_disable_start_program.TabIndex = 28;
            this.cb_disable_start_program.Text = "Temporarily disable starting this program";
            this.cb_disable_start_program.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_disable_start_program.UseVisualStyleBackColor = true;
            this.cb_disable_start_program.CheckedChanged += new System.EventHandler(this.cb_start_program_CheckedChanged);
            // 
            // lbl_start_program
            // 
            this.lbl_start_program.AutoSize = true;
            this.lbl_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_start_program.Location = new System.Drawing.Point(129, 18);
            this.lbl_start_program.Name = "lbl_start_program";
            this.lbl_start_program.Size = new System.Drawing.Size(140, 20);
            this.lbl_start_program.TabIndex = 29;
            this.lbl_start_program.Text = "Start this program:";
            // 
            // lbl_priority
            // 
            this.lbl_priority.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_priority.Location = new System.Drawing.Point(35, 59);
            this.lbl_priority.Name = "lbl_priority";
            this.lbl_priority.Size = new System.Drawing.Size(55, 23);
            this.lbl_priority.TabIndex = 30;
            this.lbl_priority.Text = "1";
            this.lbl_priority.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pb_up_arrow
            // 
            this.pb_up_arrow.Image = global::DisplayMagician.Properties.Resources.whitearrowsup;
            this.pb_up_arrow.Location = new System.Drawing.Point(40, 38);
            this.pb_up_arrow.Name = "pb_up_arrow";
            this.pb_up_arrow.Size = new System.Drawing.Size(45, 24);
            this.pb_up_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_up_arrow.TabIndex = 32;
            this.pb_up_arrow.TabStop = false;
            this.pb_up_arrow.Click += new System.EventHandler(this.pb_up_arrow_Click);
            this.pb_up_arrow.MouseEnter += new System.EventHandler(this.pb_up_arrow_MouseEnter);
            this.pb_up_arrow.MouseLeave += new System.EventHandler(this.pb_up_arrow_MouseLeave);
            // 
            // pb_down_arrow
            // 
            this.pb_down_arrow.Image = global::DisplayMagician.Properties.Resources.whitearrows;
            this.pb_down_arrow.Location = new System.Drawing.Point(40, 86);
            this.pb_down_arrow.Name = "pb_down_arrow";
            this.pb_down_arrow.Size = new System.Drawing.Size(45, 24);
            this.pb_down_arrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pb_down_arrow.TabIndex = 31;
            this.pb_down_arrow.TabStop = false;
            this.pb_down_arrow.Click += new System.EventHandler(this.pb_down_arrow_Click);
            this.pb_down_arrow.MouseEnter += new System.EventHandler(this.pb_down_arrow_MouseEnter);
            this.pb_down_arrow.MouseLeave += new System.EventHandler(this.pb_down_arrow_MouseLeave);
            // 
            // StartProgramControl
            // 
            this.AllowDrop = true;
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
            this.AutoSize = true;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.pb_up_arrow);
            this.Controls.Add(this.pb_down_arrow);
            this.Controls.Add(this.lbl_priority);
            this.Controls.Add(this.lbl_start_program);
            this.Controls.Add(this.cb_disable_start_program);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.cb_dont_start_if_running);
            this.Controls.Add(this.txt_start_program);
            this.Controls.Add(this.cb_start_program_close);
            this.Controls.Add(this.btn_start_program);
            this.Controls.Add(this.txt_start_program_args);
            this.Controls.Add(this.cb_start_program_pass_args);
            this.ForeColor = System.Drawing.Color.White;
            this.Margin = new System.Windows.Forms.Padding(10);
            this.Name = "StartProgramControl";
            this.Size = new System.Drawing.Size(1036, 148);
            ((System.ComponentModel.ISupportInitialize)(this.pb_up_arrow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pb_down_arrow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
