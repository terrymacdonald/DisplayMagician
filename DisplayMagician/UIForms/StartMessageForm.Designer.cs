
namespace DisplayMagician.UIForms
{
    partial class StartMessageForm
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
            this.lbl_profile_shown = new System.Windows.Forms.Label();
            this.rtb_message = new System.Windows.Forms.RichTextBox();
            this.btn_back = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lbl_profile_shown
            // 
            this.lbl_profile_shown.AutoSize = true;
            this.lbl_profile_shown.BackColor = System.Drawing.Color.Black;
            this.lbl_profile_shown.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_profile_shown.ForeColor = System.Drawing.Color.White;
            this.lbl_profile_shown.Location = new System.Drawing.Point(478, 18);
            this.lbl_profile_shown.Name = "lbl_profile_shown";
            this.lbl_profile_shown.Size = new System.Drawing.Size(318, 29);
            this.lbl_profile_shown.TabIndex = 20;
            this.lbl_profile_shown.Text = "Important Upgrade Message";
            // 
            // rtb_message
            // 
            this.rtb_message.Location = new System.Drawing.Point(0, 62);
            this.rtb_message.Name = "rtb_message";
            this.rtb_message.ReadOnly = true;
            this.rtb_message.Size = new System.Drawing.Size(1275, 732);
            this.rtb_message.TabIndex = 21;
            this.rtb_message.Text = "";
            // 
            // btn_back
            // 
            this.btn_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_back.ForeColor = System.Drawing.Color.White;
            this.btn_back.Location = new System.Drawing.Point(1187, 809);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 22;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // StartMessageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1274, 844);
            this.Controls.Add(this.btn_back);
            this.Controls.Add(this.rtb_message);
            this.Controls.Add(this.lbl_profile_shown);
            this.Name = "StartMessageForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "DisplayMagician - Startup Message";
            this.Load += new System.EventHandler(this.StartMessageForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_profile_shown;
        private System.Windows.Forms.RichTextBox rtb_message;
        private System.Windows.Forms.Button btn_back;
    }
}