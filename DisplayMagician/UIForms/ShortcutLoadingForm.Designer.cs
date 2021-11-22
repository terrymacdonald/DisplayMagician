
namespace DisplayMagician.UIForms
{
    partial class ShortcutLoadingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ShortcutLoadingForm));
            this.lbl_title = new System.Windows.Forms.Label();
            this.lbl_description = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbl_title
            // 
            this.lbl_title.BackColor = System.Drawing.Color.Black;
            this.lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_title.ForeColor = System.Drawing.Color.White;
            this.lbl_title.Location = new System.Drawing.Point(36, 26);
            this.lbl_title.Name = "lbl_title";
            this.lbl_title.Size = new System.Drawing.Size(347, 30);
            this.lbl_title.TabIndex = 2;
            this.lbl_title.Text = "DisplayMagician is loading...";
            this.lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_title.UseWaitCursor = true;
            // 
            // lbl_description
            // 
            this.lbl_description.BackColor = System.Drawing.Color.WhiteSmoke;
            this.lbl_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_description.Location = new System.Drawing.Point(36, 56);
            this.lbl_description.Name = "lbl_description";
            this.lbl_description.Size = new System.Drawing.Size(347, 48);
            this.lbl_description.TabIndex = 3;
            this.lbl_description.Text = "If you have installed a lot of games over time or have a lot of games installed n" +
    "ow, this may take a while!";
            this.lbl_description.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_description.UseWaitCursor = true;
            // 
            // ShortcutLoadingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.ClientSize = new System.Drawing.Size(418, 129);
            this.ControlBox = false;
            this.Controls.Add(this.lbl_description);
            this.Controls.Add(this.lbl_title);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(434, 168);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(434, 168);
            this.Name = "ShortcutLoadingForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "DisplayMagician is loading....";
            this.UseWaitCursor = true;
            this.Load += new System.EventHandler(this.LoadingForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_description;
    }
}