namespace DisplayMagician.UIForms
{
    partial class MigrationSummaryForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MigrationSummaryForm));
            lbl_heading = new System.Windows.Forms.Label();
            rtb_summary = new System.Windows.Forms.RichTextBox();
            btn_close = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lbl_heading
            // 
            lbl_heading.AutoSize = true;
            lbl_heading.BackColor = System.Drawing.Color.Black;
            lbl_heading.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_heading.ForeColor = System.Drawing.Color.White;
            lbl_heading.Location = new System.Drawing.Point(153, 18);
            lbl_heading.Name = "lbl_heading";
            lbl_heading.Size = new System.Drawing.Size(264, 20);
            lbl_heading.TabIndex = 0;
            lbl_heading.Text = "Your configuration was updated";
            // 
            // rtb_summary
            // 
            rtb_summary.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rtb_summary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            rtb_summary.Location = new System.Drawing.Point(22, 52);
            rtb_summary.Name = "rtb_summary";
            rtb_summary.ReadOnly = true;
            rtb_summary.Size = new System.Drawing.Size(526, 329);
            rtb_summary.TabIndex = 1;
            rtb_summary.Text = "";
            // 
            // btn_close
            // 
            btn_close.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_close.BackColor = System.Drawing.Color.Black;
            btn_close.DialogResult = System.Windows.Forms.DialogResult.OK;
            btn_close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_close.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_close.ForeColor = System.Drawing.Color.White;
            btn_close.Location = new System.Drawing.Point(460, 396);
            btn_close.Name = "btn_close";
            btn_close.Size = new System.Drawing.Size(88, 27);
            btn_close.TabIndex = 2;
            btn_close.Text = "&Close";
            btn_close.UseVisualStyleBackColor = false;
            // 
            // MigrationSummaryForm
            // 
            AcceptButton = btn_close;
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackgroundImage = (System.Drawing.Image)resources.GetObject("$this.BackgroundImage");
            BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            ClientSize = new System.Drawing.Size(570, 435);
            Controls.Add(btn_close);
            Controls.Add(rtb_summary);
            Controls.Add(lbl_heading);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(430, 240);
            Name = "MigrationSummaryForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Updated configuration";
            Load += MigrationSummaryForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Label lbl_heading;
        private System.Windows.Forms.RichTextBox rtb_summary;
        private System.Windows.Forms.Button btn_close;
    }
}
