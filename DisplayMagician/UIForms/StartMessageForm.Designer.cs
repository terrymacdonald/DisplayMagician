
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
            lbl_heading_text = new System.Windows.Forms.Label();
            rtb_message = new System.Windows.Forms.RichTextBox();
            btn_back = new System.Windows.Forms.Button();
            pnl_richtextbox = new System.Windows.Forms.Panel();
            pnl_richtextbox.SuspendLayout();
            SuspendLayout();
            // 
            // lbl_heading_text
            // 
            lbl_heading_text.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_heading_text.AutoSize = true;
            lbl_heading_text.BackColor = System.Drawing.Color.Black;
            lbl_heading_text.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_heading_text.ForeColor = System.Drawing.Color.White;
            lbl_heading_text.Location = new System.Drawing.Point(193, 22);
            lbl_heading_text.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_heading_text.Name = "lbl_heading_text";
            lbl_heading_text.Size = new System.Drawing.Size(400, 29);
            lbl_heading_text.TabIndex = 20;
            lbl_heading_text.Text = "Important DisplayMagician Message";
            lbl_heading_text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // rtb_message
            // 
            rtb_message.BackColor = System.Drawing.Color.White;
            rtb_message.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtb_message.Dock = System.Windows.Forms.DockStyle.Fill;
            rtb_message.Location = new System.Drawing.Point(23, 23);
            rtb_message.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            rtb_message.Name = "rtb_message";
            rtb_message.ReadOnly = true;
            rtb_message.Size = new System.Drawing.Size(1406, 788);
            rtb_message.TabIndex = 21;
            rtb_message.Text = "";
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(697, 559);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 22;
            btn_back.Text = "&Close";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // pnl_richtextbox
            // 
            pnl_richtextbox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            pnl_richtextbox.BackColor = System.Drawing.Color.White;
            pnl_richtextbox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            pnl_richtextbox.Controls.Add(rtb_message);
            pnl_richtextbox.Location = new System.Drawing.Point(15, 73);
            pnl_richtextbox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pnl_richtextbox.Name = "pnl_richtextbox";
            pnl_richtextbox.Padding = new System.Windows.Forms.Padding(23, 23, 23, 23);
            pnl_richtextbox.Size = new System.Drawing.Size(770, 464);
            pnl_richtextbox.TabIndex = 23;
            // 
            // StartMessageForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(800, 600);
            Controls.Add(pnl_richtextbox);
            Controls.Add(btn_back);
            Controls.Add(lbl_heading_text);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MinimumSize = new System.Drawing.Size(600, 400);
            Name = "StartMessageForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "DisplayMagician - Message";
            TopMost = true;
            Load += StartMessageForm_Load;
            pnl_richtextbox.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_heading_text;
        private System.Windows.Forms.RichTextBox rtb_message;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Panel pnl_richtextbox;
    }
}
