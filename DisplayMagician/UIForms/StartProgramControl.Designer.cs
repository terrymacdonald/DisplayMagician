
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
            this.cb_start_program = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cb_dont_start_if_running
            // 
            this.cb_dont_start_if_running.AutoSize = true;
            this.cb_dont_start_if_running.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_dont_start_if_running.ForeColor = System.Drawing.Color.White;
            this.cb_dont_start_if_running.Location = new System.Drawing.Point(135, 97);
            this.cb_dont_start_if_running.Name = "cb_dont_start_if_running";
            this.cb_dont_start_if_running.Size = new System.Drawing.Size(289, 24);
            this.cb_dont_start_if_running.TabIndex = 26;
            this.cb_dont_start_if_running.Text = "Don\'t start if program already running";
            this.cb_dont_start_if_running.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_dont_start_if_running.UseVisualStyleBackColor = true;
            // 
            // txt_start_program
            // 
            this.txt_start_program.BackColor = System.Drawing.Color.White;
            this.txt_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_start_program.ForeColor = System.Drawing.Color.Black;
            this.txt_start_program.Location = new System.Drawing.Point(268, 20);
            this.txt_start_program.Name = "txt_start_program";
            this.txt_start_program.Size = new System.Drawing.Size(535, 26);
            this.txt_start_program.TabIndex = 25;
            // 
            // cb_start_program_close
            // 
            this.cb_start_program_close.AutoSize = true;
            this.cb_start_program_close.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_start_program_close.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_close.Location = new System.Drawing.Point(515, 97);
            this.cb_start_program_close.Name = "cb_start_program_close";
            this.cb_start_program_close.Size = new System.Drawing.Size(398, 24);
            this.cb_start_program_close.TabIndex = 24;
            this.cb_start_program_close.Text = "Close started program when you finish playing Game";
            this.cb_start_program_close.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program_close.UseVisualStyleBackColor = true;
            // 
            // btn_start_program
            // 
            this.btn_start_program.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_start_program.ForeColor = System.Drawing.Color.White;
            this.btn_start_program.Location = new System.Drawing.Point(819, 19);
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
            this.txt_start_program_args.Location = new System.Drawing.Point(365, 60);
            this.txt_start_program_args.Name = "txt_start_program_args";
            this.txt_start_program_args.Size = new System.Drawing.Size(540, 26);
            this.txt_start_program_args.TabIndex = 22;
            // 
            // cb_start_program_pass_args
            // 
            this.cb_start_program_pass_args.AutoSize = true;
            this.cb_start_program_pass_args.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_start_program_pass_args.ForeColor = System.Drawing.Color.White;
            this.cb_start_program_pass_args.Location = new System.Drawing.Point(135, 62);
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
            this.btn_delete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_delete.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_delete.ForeColor = System.Drawing.Color.White;
            this.btn_delete.Location = new System.Drawing.Point(933, 3);
            this.btn_delete.Name = "btn_delete";
            this.btn_delete.Size = new System.Drawing.Size(29, 27);
            this.btn_delete.TabIndex = 27;
            this.btn_delete.Text = "X";
            this.btn_delete.UseVisualStyleBackColor = true;
            this.btn_delete.Click += new System.EventHandler(this.btn_delete_Click);
            // 
            // cb_start_program
            // 
            this.cb_start_program.AutoSize = true;
            this.cb_start_program.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cb_start_program.ForeColor = System.Drawing.Color.White;
            this.cb_start_program.Location = new System.Drawing.Point(34, 21);
            this.cb_start_program.Name = "cb_start_program";
            this.cb_start_program.Size = new System.Drawing.Size(222, 24);
            this.cb_start_program.TabIndex = 28;
            this.cb_start_program.Text = "Start the following program:";
            this.cb_start_program.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cb_start_program.UseVisualStyleBackColor = true;
            this.cb_start_program.CheckedChanged += new System.EventHandler(this.cb_start_program_CheckedChanged);
            // 
            // StartProgramControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.Controls.Add(this.cb_start_program);
            this.Controls.Add(this.btn_delete);
            this.Controls.Add(this.cb_dont_start_if_running);
            this.Controls.Add(this.txt_start_program);
            this.Controls.Add(this.cb_start_program_close);
            this.Controls.Add(this.btn_start_program);
            this.Controls.Add(this.txt_start_program_args);
            this.Controls.Add(this.cb_start_program_pass_args);
            this.ForeColor = System.Drawing.Color.White;
            this.Name = "StartProgramControl";
            this.Size = new System.Drawing.Size(965, 136);
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
        private System.Windows.Forms.CheckBox cb_start_program;
    }
}
