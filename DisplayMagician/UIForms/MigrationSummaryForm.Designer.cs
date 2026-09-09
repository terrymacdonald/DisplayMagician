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
            lbl_heading = new System.Windows.Forms.Label();
            rtb_summary = new System.Windows.Forms.RichTextBox();
            btn_close = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lbl_heading
            // 
            lbl_heading.AutoSize = true;
            lbl_heading.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_heading.Location = new System.Drawing.Point(18, 16);
            lbl_heading.Name = "lbl_heading";
            lbl_heading.Size = new System.Drawing.Size(276, 20);
            lbl_heading.TabIndex = 0;
            lbl_heading.Text = "DisplayMagician was updated";
            // 
            // rtb_summary
            // 
            rtb_summary.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            rtb_summary.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            rtb_summary.Location = new System.Drawing.Point(22, 52);
            rtb_summary.Name = "rtb_summary";
            rtb_summary.ReadOnly = true;
            rtb_summary.Size = new System.Drawing.Size(526, 158);
            rtb_summary.TabIndex = 1;
            rtb_summary.Text = "";
            // 
            // btn_close
            // 
            btn_close.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_close.DialogResult = System.Windows.Forms.DialogResult.OK;
            btn_close.Location = new System.Drawing.Point(473, 225);
            btn_close.Name = "btn_close";
            btn_close.Size = new System.Drawing.Size(75, 27);
            btn_close.TabIndex = 2;
            btn_close.Text = "&Close";
            btn_close.UseVisualStyleBackColor = true;
            // 
            // MigrationSummaryForm
            // 
            AcceptButton = btn_close;
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(570, 264);
            Controls.Add(btn_close);
            Controls.Add(rtb_summary);
            Controls.Add(lbl_heading);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(430, 240);
            Name = "MigrationSummaryForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "DisplayMagician migration";
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
