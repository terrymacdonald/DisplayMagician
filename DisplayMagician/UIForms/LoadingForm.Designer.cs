
namespace DisplayMagician.UIForms
{
    partial class LoadingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadingForm));
            lbl_title = new System.Windows.Forms.Label();
            lbl_description = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lbl_title
            // 
            lbl_title.Anchor = System.Windows.Forms.AnchorStyles.None;
            lbl_title.BackColor = System.Drawing.Color.Black;
            lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_title.ForeColor = System.Drawing.Color.White;
            lbl_title.Location = new System.Drawing.Point(80, 76);
            lbl_title.Margin = new System.Windows.Forms.Padding(110, 0, 110, 0);
            lbl_title.Name = "lbl_title";
            lbl_title.Size = new System.Drawing.Size(1109, 119);
            lbl_title.TabIndex = 2;
            lbl_title.Text = "DisplayMagician is loading...";
            lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_title.UseWaitCursor = true;
            // 
            // lbl_description
            // 
            lbl_description.Anchor = System.Windows.Forms.AnchorStyles.None;
            lbl_description.BackColor = System.Drawing.Color.WhiteSmoke;
            lbl_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_description.Location = new System.Drawing.Point(80, 195);
            lbl_description.Margin = new System.Windows.Forms.Padding(110, 0, 110, 0);
            lbl_description.Name = "lbl_description";
            lbl_description.Size = new System.Drawing.Size(1109, 168);
            lbl_description.TabIndex = 3;
            lbl_description.Text = "Please wait. We should have everything configured and ready for you to use in just a few moments.";
            lbl_description.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_description.UseWaitCursor = true;
            // 
            // LoadingForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(192F, 192F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(1268, 433);
            ControlBox = false;
            Controls.Add(lbl_description);
            Controls.Add(lbl_title);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(110, 38, 110, 38);
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(1592, 768);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(1180, 400);
            Name = "LoadingForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "DisplayMagician is loading....";
            UseWaitCursor = true;
            Load += LoadingForm_Load;
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_description;
    }
}
