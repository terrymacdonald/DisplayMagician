
namespace HeliosPlus.UIForms
{
    partial class WaitingForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WaitingForm));
            this.btn_stop_waiting = new System.Windows.Forms.Button();
            this.lbl_title = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btn_stop_waiting
            // 
            this.btn_stop_waiting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_stop_waiting.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_stop_waiting.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_stop_waiting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_stop_waiting.ForeColor = System.Drawing.Color.White;
            this.btn_stop_waiting.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.btn_stop_waiting.Location = new System.Drawing.Point(534, 183);
            this.btn_stop_waiting.Name = "btn_stop_waiting";
            this.btn_stop_waiting.Size = new System.Drawing.Size(99, 23);
            this.btn_stop_waiting.TabIndex = 4;
            this.btn_stop_waiting.Text = "&Stop Waiting";
            this.btn_stop_waiting.UseVisualStyleBackColor = true;
            this.btn_stop_waiting.Click += new System.EventHandler(this.btn_stop_waiting_Click);
            // 
            // lbl_title
            // 
            this.lbl_title.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_title.AutoSize = true;
            this.lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_title.ForeColor = System.Drawing.Color.White;
            this.lbl_title.Location = new System.Drawing.Point(202, 84);
            this.lbl_title.Name = "lbl_title";
            this.lbl_title.Size = new System.Drawing.Size(259, 26);
            this.lbl_title.TabIndex = 5;
            this.lbl_title.Text = "Waiting for game to exit...";
            this.lbl_title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lbl_title.UseWaitCursor = true;
            // 
            // WaitingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DimGray;
            this.ClientSize = new System.Drawing.Size(645, 218);
            this.ControlBox = false;
            this.Controls.Add(this.lbl_title);
            this.Controls.Add(this.btn_stop_waiting);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WaitingForm";
            this.Opacity = 0.8D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Waiting...";
            this.TopMost = true;
            this.UseWaitCursor = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btn_stop_waiting;
        private System.Windows.Forms.Label lbl_title;
    }
}