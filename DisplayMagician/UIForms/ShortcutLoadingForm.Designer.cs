
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
            lbl_title = new System.Windows.Forms.Label();
            lbl_description = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lbl_title
            // 
            lbl_title.BackColor = System.Drawing.Color.Black;
            lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_title.ForeColor = System.Drawing.Color.White;
            lbl_title.Location = new System.Drawing.Point(42, 30);
            lbl_title.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_title.Name = "lbl_title";
            lbl_title.Size = new System.Drawing.Size(405, 35);
            lbl_title.TabIndex = 2;
            lbl_title.Text = "Scanning  your Apps and Games...";
            lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_title.UseWaitCursor = true;
            // 
            // lbl_description
            // 
            lbl_description.BackColor = System.Drawing.Color.WhiteSmoke;
            lbl_description.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lbl_description.Location = new System.Drawing.Point(42, 65);
            lbl_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_description.Name = "lbl_description";
            lbl_description.Size = new System.Drawing.Size(405, 55);
            lbl_description.TabIndex = 3;
            lbl_description.Text = "If you have installed a lot of games over time or have a lot of games installed now, this may take a while!";
            lbl_description.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lbl_description.UseWaitCursor = true;
            // 
            // ShortcutLoadingForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            ClientSize = new System.Drawing.Size(488, 149);
            ControlBox = false;
            Controls.Add(lbl_description);
            Controls.Add(lbl_title);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(504, 188);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(504, 188);
            Name = "ShortcutLoadingForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician is scanning your Apps and Games....";
            UseWaitCursor = true;
            Load += LoadingForm_Load;
            Shown += ShortcutLoadingForm_Shown;
            ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_description;
    }
}
