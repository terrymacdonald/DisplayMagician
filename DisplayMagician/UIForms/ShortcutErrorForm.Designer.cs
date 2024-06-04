namespace DisplayMagician.UIForms
{
    partial class ShortcutErrorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortcutErrorForm));
            lbl_title = new System.Windows.Forms.Label();
            btn_save = new System.Windows.Forms.Button();
            txt_errors = new System.Windows.Forms.TextBox();
            SuspendLayout();
            // 
            // lbl_title
            // 
            lbl_title.AutoSize = true;
            lbl_title.BackColor = System.Drawing.Color.Black;
            lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_title.ForeColor = System.Drawing.Color.White;
            lbl_title.Location = new System.Drawing.Point(306, 17);
            lbl_title.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_title.Name = "lbl_title";
            lbl_title.Size = new System.Drawing.Size(277, 29);
            lbl_title.TabIndex = 21;
            lbl_title.Text = "Shortcut errors detected!";
            lbl_title.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btn_save
            // 
            btn_save.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_save.ForeColor = System.Drawing.Color.White;
            btn_save.Location = new System.Drawing.Point(397, 459);
            btn_save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_save.Name = "btn_save";
            btn_save.Size = new System.Drawing.Size(140, 38);
            btn_save.TabIndex = 22;
            btn_save.Text = "&OK";
            btn_save.UseVisualStyleBackColor = true;
            btn_save.Click += btn_save_Click;
            // 
            // txt_errors
            // 
            txt_errors.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_errors.Location = new System.Drawing.Point(14, 67);
            txt_errors.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_errors.Multiline = true;
            txt_errors.Name = "txt_errors";
            txt_errors.Size = new System.Drawing.Size(906, 380);
            txt_errors.TabIndex = 23;
            // 
            // ShortcutErrorForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(933, 519);
            Controls.Add(txt_errors);
            Controls.Add(btn_save);
            Controls.Add(lbl_title);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ShortcutErrorForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Shortcut errors detected";
            TopMost = true;
            Load += ShortcutErrorForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.TextBox txt_errors;
    }
}