
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartMessageForm));
            this.lbl_heading_text = new System.Windows.Forms.Label();
            this.rtb_message = new System.Windows.Forms.RichTextBox();
            this.btn_back = new System.Windows.Forms.Button();
            this.pnl_richtextbox = new System.Windows.Forms.Panel();
            this.pnl_richtextbox.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbl_heading_text
            // 
            this.lbl_heading_text.AutoSize = true;
            this.lbl_heading_text.BackColor = System.Drawing.Color.Black;
            this.lbl_heading_text.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_heading_text.ForeColor = System.Drawing.Color.White;
            this.lbl_heading_text.Location = new System.Drawing.Point(437, 19);
            this.lbl_heading_text.Name = "lbl_heading_text";
            this.lbl_heading_text.Size = new System.Drawing.Size(400, 29);
            this.lbl_heading_text.TabIndex = 20;
            this.lbl_heading_text.Text = "Important DisplayMagician Message";
            this.lbl_heading_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rtb_message
            // 
            this.rtb_message.BackColor = System.Drawing.Color.White;
            this.rtb_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtb_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb_message.Location = new System.Drawing.Point(20, 20);
            this.rtb_message.Name = "rtb_message";
            this.rtb_message.ReadOnly = true;
            this.rtb_message.Size = new System.Drawing.Size(1205, 683);
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
            this.btn_back.Text = "&Close";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // pnl_richtextbox
            // 
            this.pnl_richtextbox.BackColor = System.Drawing.Color.White;
            this.pnl_richtextbox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pnl_richtextbox.Controls.Add(this.rtb_message);
            this.pnl_richtextbox.Location = new System.Drawing.Point(13, 63);
            this.pnl_richtextbox.Name = "pnl_richtextbox";
            this.pnl_richtextbox.Padding = new System.Windows.Forms.Padding(20);
            this.pnl_richtextbox.Size = new System.Drawing.Size(1249, 727);
            this.pnl_richtextbox.TabIndex = 23;
            // 
            // StartMessageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(1274, 844);
            this.Controls.Add(this.pnl_richtextbox);
            this.Controls.Add(this.btn_back);
            this.Controls.Add(this.lbl_heading_text);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StartMessageForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DisplayMagician - Message";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.StartMessageForm_Load);
            this.pnl_richtextbox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_heading_text;
        private System.Windows.Forms.RichTextBox rtb_message;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Panel pnl_richtextbox;
    }
}