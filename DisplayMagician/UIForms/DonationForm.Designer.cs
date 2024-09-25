
namespace DisplayMagician.UIForms
{
    partial class DonationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DonationForm));
            lbl_title = new System.Windows.Forms.Label();
            lbl_description = new System.Windows.Forms.Label();
            btn_donate = new System.Windows.Forms.Button();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            btn_exit = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lbl_title
            // 
            lbl_title.BackColor = System.Drawing.Color.Black;
            lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_title.ForeColor = System.Drawing.Color.White;
            lbl_title.Location = new System.Drawing.Point(119, 36);
            lbl_title.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_title.Name = "lbl_title";
            lbl_title.Size = new System.Drawing.Size(547, 35);
            lbl_title.TabIndex = 2;
            lbl_title.Text = "You've used DisplayMagician X times!";
            lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_description
            // 
            lbl_description.BackColor = System.Drawing.Color.WhiteSmoke;
            lbl_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_description.Location = new System.Drawing.Point(187, 126);
            lbl_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_description.Name = "lbl_description";
            lbl_description.Size = new System.Drawing.Size(411, 31);
            lbl_description.TabIndex = 3;
            lbl_description.Text = "Has DisplayMagician made your life better?";
            lbl_description.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_donate
            // 
            btn_donate.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            btn_donate.BackColor = System.Drawing.Color.Black;
            btn_donate.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_donate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_donate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_donate.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            btn_donate.ForeColor = System.Drawing.Color.White;
            btn_donate.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_donate.Location = new System.Drawing.Point(312, 299);
            btn_donate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_donate.Name = "btn_donate";
            btn_donate.Size = new System.Drawing.Size(161, 53);
            btn_donate.TabIndex = 12;
            btn_donate.Text = "D&onate now";
            btn_donate.UseVisualStyleBackColor = false;
            btn_donate.Click += btn_donate_Click;
            // 
            // label1
            // 
            label1.BackColor = System.Drawing.Color.WhiteSmoke;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.Location = new System.Drawing.Point(187, 157);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(411, 37);
            label1.TabIndex = 13;
            label1.Text = "Has it simplified your gaming routine?";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label2
            // 
            label2.BackColor = System.Drawing.Color.Brown;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.ForeColor = System.Drawing.Color.White;
            label2.Location = new System.Drawing.Point(187, 194);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(411, 37);
            label2.TabIndex = 14;
            label2.Text = "Donating to the DisplayMagician team is the best way to keep this project going.";
            label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_exit
            // 
            btn_exit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_exit.BackColor = System.Drawing.Color.Black;
            btn_exit.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_exit.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_exit.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_exit.ForeColor = System.Drawing.Color.White;
            btn_exit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            btn_exit.Location = new System.Drawing.Point(683, 422);
            btn_exit.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_exit.Name = "btn_exit";
            btn_exit.Size = new System.Drawing.Size(88, 27);
            btn_exit.TabIndex = 15;
            btn_exit.Text = "C&lose";
            btn_exit.UseVisualStyleBackColor = false;
            btn_exit.Click += btn_exit_Click;
            // 
            // DonationForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Control;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            ClientSize = new System.Drawing.Size(784, 461);
            ControlBox = false;
            Controls.Add(btn_exit);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btn_donate);
            Controls.Add(lbl_description);
            Controls.Add(lbl_title);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(800, 500);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(800, 500);
            Name = "DonationForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Annual Donation Appeal";
            Click += DonationForm_Click;
            ResumeLayout(false);
        }

        #endregion
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_description;
        private System.Windows.Forms.Button btn_donate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_exit;
    }
}