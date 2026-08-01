namespace DisplayMagician.UIForms
{
    partial class AudioProfileNameForm
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
            gb_main = new System.Windows.Forms.GroupBox();
            lbl_validation = new System.Windows.Forms.Label();
            txt_profile_name = new System.Windows.Forms.TextBox();
            lbl_instruction = new System.Windows.Forms.Label();
            btn_cancel = new System.Windows.Forms.Button();
            btn_ok = new System.Windows.Forms.Button();
            gb_main.SuspendLayout();
            SuspendLayout();
            // 
            // gb_main
            // 
            gb_main.Controls.Add(lbl_validation);
            gb_main.Controls.Add(txt_profile_name);
            gb_main.Controls.Add(lbl_instruction);
            gb_main.Controls.Add(btn_cancel);
            gb_main.Controls.Add(btn_ok);
            gb_main.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            gb_main.ForeColor = System.Drawing.Color.White;
            gb_main.Location = new System.Drawing.Point(12, 12);
            gb_main.Name = "gb_main";
            gb_main.Size = new System.Drawing.Size(560, 220);
            gb_main.TabIndex = 0;
            gb_main.TabStop = false;
            gb_main.Text = "Audio Profile Name";
            gb_main.Paint += groupbox_Paint;
            // 
            // lbl_validation
            // 
            lbl_validation.AutoSize = true;
            lbl_validation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_validation.ForeColor = System.Drawing.Color.IndianRed;
            lbl_validation.Location = new System.Drawing.Point(24, 130);
            lbl_validation.Name = "lbl_validation";
            lbl_validation.Size = new System.Drawing.Size(44, 16);
            lbl_validation.TabIndex = 4;
            lbl_validation.Text = "Error";
            lbl_validation.Visible = false;
            // 
            // txt_profile_name
            // 
            txt_profile_name.BackColor = System.Drawing.Color.White;
            txt_profile_name.ForeColor = System.Drawing.Color.Black;
            txt_profile_name.Location = new System.Drawing.Point(24, 72);
            txt_profile_name.Name = "txt_profile_name";
            txt_profile_name.Size = new System.Drawing.Size(512, 26);
            txt_profile_name.TabIndex = 1;
            txt_profile_name.TextChanged += txt_profile_name_TextChanged;
            // 
            // lbl_instruction
            // 
            lbl_instruction.AutoSize = true;
            lbl_instruction.ForeColor = System.Drawing.Color.White;
            lbl_instruction.Location = new System.Drawing.Point(24, 40);
            lbl_instruction.Name = "lbl_instruction";
            lbl_instruction.Size = new System.Drawing.Size(213, 20);
            lbl_instruction.TabIndex = 0;
            lbl_instruction.Text = "Enter a name for the profile:";
            lbl_instruction.Paint += label_Paint;
            // 
            // btn_cancel
            // 
            btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_cancel.ForeColor = System.Drawing.Color.White;
            btn_cancel.Location = new System.Drawing.Point(440, 168);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Size = new System.Drawing.Size(96, 32);
            btn_cancel.TabIndex = 3;
            btn_cancel.Text = "&Cancel";
            btn_cancel.UseVisualStyleBackColor = true;
            btn_cancel.Click += btn_cancel_Click;
            // 
            // btn_ok
            // 
            btn_ok.Enabled = false;
            btn_ok.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_ok.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_ok.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_ok.ForeColor = System.Drawing.Color.White;
            btn_ok.Location = new System.Drawing.Point(328, 168);
            btn_ok.Name = "btn_ok";
            btn_ok.Size = new System.Drawing.Size(96, 32);
            btn_ok.TabIndex = 2;
            btn_ok.Text = "&OK";
            btn_ok.UseVisualStyleBackColor = true;
            btn_ok.Click += btn_ok_Click;
            // 
            // AudioProfileNameForm
            // 
            AcceptButton = btn_ok;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            CancelButton = btn_cancel;
            ClientSize = new System.Drawing.Size(584, 244);
            Controls.Add(gb_main);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AudioProfileNameForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Audio Profile Name";
            Load += AudioProfileNameForm_Load;
            gb_main.ResumeLayout(false);
            gb_main.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox gb_main;
        private System.Windows.Forms.Label lbl_validation;
        private System.Windows.Forms.TextBox txt_profile_name;
        private System.Windows.Forms.Label lbl_instruction;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_ok;
    }
}
