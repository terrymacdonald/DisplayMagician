
namespace DisplayMagician.UIForms
{
    partial class SettingsForm
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
            this.cb_minimise_notification_area = new System.Windows.Forms.CheckBox();
            this.cmb_loglevel = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cb_minimise_notification_area
            // 
            this.cb_minimise_notification_area.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.cb_minimise_notification_area.AutoSize = true;
            this.cb_minimise_notification_area.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.cb_minimise_notification_area.ForeColor = System.Drawing.Color.White;
            this.cb_minimise_notification_area.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.cb_minimise_notification_area.Location = new System.Drawing.Point(73, 41);
            this.cb_minimise_notification_area.Name = "cb_minimise_notification_area";
            this.cb_minimise_notification_area.Size = new System.Drawing.Size(332, 20);
            this.cb_minimise_notification_area.TabIndex = 6;
            this.cb_minimise_notification_area.Text = "Start DisplayMagician minimised in notification area";
            this.cb_minimise_notification_area.UseVisualStyleBackColor = true;
            // 
            // cmb_loglevel
            // 
            this.cmb_loglevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmb_loglevel.FormattingEnabled = true;
            this.cmb_loglevel.Location = new System.Drawing.Point(213, 86);
            this.cmb_loglevel.Name = "cmb_loglevel";
            this.cmb_loglevel.Size = new System.Drawing.Size(275, 24);
            this.cmb_loglevel.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Transparent;
            this.label1.Location = new System.Drawing.Point(70, 89);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 16);
            this.label1.TabIndex = 8;
            this.label1.Text = "What type of logging?";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(544, 187);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmb_loglevel);
            this.Controls.Add(this.cb_minimise_notification_area);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Settings";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingsForm_FormClosing);
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox cb_minimise_notification_area;
        private System.Windows.Forms.ComboBox cmb_loglevel;
        private System.Windows.Forms.Label label1;
    }
}