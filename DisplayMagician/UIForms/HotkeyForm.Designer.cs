
namespace DisplayMagician.UIForms
{
    partial class HotkeyForm
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
            this.lbl_hotkey = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbl_hotkey
            // 
            this.lbl_hotkey.AutoSize = true;
            this.lbl_hotkey.BackColor = System.Drawing.Color.Black;
            this.lbl_hotkey.ForeColor = System.Drawing.Color.White;
            this.lbl_hotkey.Location = new System.Drawing.Point(42, 32);
            this.lbl_hotkey.Name = "lbl_hotkey";
            this.lbl_hotkey.Size = new System.Drawing.Size(525, 13);
            this.lbl_hotkey.TabIndex = 0;
            this.lbl_hotkey.Text = "You can set a Hotkey (a keyboard shortcut) so that you can apply this Display Pro" +
    "file just using your keyboard.";
            // 
            // HotkeyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(442, 221);
            this.Controls.Add(this.lbl_hotkey);
            this.Name = "HotkeyForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Select a Hotkey";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lbl_hotkey;
    }
}