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
            lbl_validation = new System.Windows.Forms.Label();
            txt_profile_name = new System.Windows.Forms.TextBox();
            lbl_instruction = new System.Windows.Forms.Label();
            btn_cancel = new System.Windows.Forms.Button();
            btn_ok = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lbl_validation
            // 
            lbl_validation.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lbl_validation.AutoSize = true;
            lbl_validation.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_validation.ForeColor = System.Drawing.Color.IndianRed;
            lbl_validation.Location = new System.Drawing.Point(214, 57);
            lbl_validation.Margin = new System.Windows.Forms.Padding(41, 0, 41, 0);
            lbl_validation.Name = "lbl_validation";
            lbl_validation.Size = new System.Drawing.Size(36, 16);
            lbl_validation.TabIndex = 0;
            lbl_validation.Text = "Error";
            lbl_validation.Visible = false;
            // 
            // txt_profile_name
            // 
            txt_profile_name.Anchor = System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            txt_profile_name.BackColor = System.Drawing.Color.White;
            txt_profile_name.ForeColor = System.Drawing.Color.Black;
            txt_profile_name.Location = new System.Drawing.Point(217, 28);
            txt_profile_name.Margin = new System.Windows.Forms.Padding(41, 19, 41, 19);
            txt_profile_name.Name = "txt_profile_name";
            txt_profile_name.Size = new System.Drawing.Size(451, 21);
            txt_profile_name.TabIndex = 1;
            txt_profile_name.TextChanged += txt_profile_name_TextChanged;
            // 
            // lbl_instruction
            // 
            lbl_instruction.Anchor = System.Windows.Forms.AnchorStyles.Left;
            lbl_instruction.AutoSize = true;
            lbl_instruction.ForeColor = System.Drawing.Color.White;
            lbl_instruction.Location = new System.Drawing.Point(23, 31);
            lbl_instruction.Margin = new System.Windows.Forms.Padding(41, 0, 41, 0);
            lbl_instruction.Name = "lbl_instruction";
            lbl_instruction.Size = new System.Drawing.Size(192, 15);
            lbl_instruction.TabIndex = 0;
            lbl_instruction.Text = "Enter a name for the audio profile:";
            lbl_instruction.Paint += label_Paint;
            // 
            // btn_cancel
            // 
            btn_cancel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_cancel.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_cancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_cancel.ForeColor = System.Drawing.Color.White;
            btn_cancel.Location = new System.Drawing.Point(785, 25);
            btn_cancel.Margin = new System.Windows.Forms.Padding(41, 19, 41, 19);
            btn_cancel.Name = "btn_cancel";
            btn_cancel.Size = new System.Drawing.Size(88, 27);
            btn_cancel.TabIndex = 3;
            btn_cancel.Text = "&Cancel";
            btn_cancel.UseVisualStyleBackColor = true;
            btn_cancel.Click += btn_cancel_Click;
            // 
            // btn_ok
            // 
            btn_ok.Anchor = System.Windows.Forms.AnchorStyles.Right;
            btn_ok.Enabled = false;
            btn_ok.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_ok.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_ok.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_ok.ForeColor = System.Drawing.Color.White;
            btn_ok.Location = new System.Drawing.Point(687, 25);
            btn_ok.Margin = new System.Windows.Forms.Padding(41, 19, 41, 19);
            btn_ok.Name = "btn_ok";
            btn_ok.Size = new System.Drawing.Size(88, 27);
            btn_ok.TabIndex = 2;
            btn_ok.Text = "&OK";
            btn_ok.UseVisualStyleBackColor = true;
            btn_ok.Click += btn_ok_Click;
            // 
            // AudioProfileNameForm
            // 
            AcceptButton = btn_ok;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            CancelButton = btn_cancel;
            ClientSize = new System.Drawing.Size(910, 87);
            Controls.Add(lbl_validation);
            Controls.Add(txt_profile_name);
            Controls.Add(lbl_instruction);
            Controls.Add(btn_ok);
            Controls.Add(btn_cancel);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Margin = new System.Windows.Forms.Padding(55, 19, 55, 19);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(926, 126);
            Name = "AudioProfileNameForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Audio Profile Name";
            Load += AudioProfileNameForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Label lbl_validation;
        private System.Windows.Forms.TextBox txt_profile_name;
        private System.Windows.Forms.Label lbl_instruction;
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_ok;
    }
}

