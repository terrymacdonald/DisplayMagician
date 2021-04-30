
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HotkeyForm));
            this.txt_hotkey = new System.Windows.Forms.TextBox();
            this.lbl_hotkey_selector = new System.Windows.Forms.Label();
            this.btn_save = new System.Windows.Forms.Button();
            this.btn_clear = new System.Windows.Forms.Button();
            this.lbl_hotkey_heading = new System.Windows.Forms.Label();
            this.lbl_hotkey_description = new System.Windows.Forms.Label();
            this.btn_back = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txt_hotkey
            // 
            this.txt_hotkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txt_hotkey.HideSelection = false;
            this.txt_hotkey.Location = new System.Drawing.Point(86, 153);
            this.txt_hotkey.Name = "txt_hotkey";
            this.txt_hotkey.ReadOnly = true;
            this.txt_hotkey.Size = new System.Drawing.Size(270, 26);
            this.txt_hotkey.TabIndex = 1;
            this.txt_hotkey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.OnKeyDown);
            this.txt_hotkey.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.OnKeyPress);
            this.txt_hotkey.KeyUp += new System.Windows.Forms.KeyEventHandler(this.OnKeyUp);
            // 
            // lbl_hotkey_selector
            // 
            this.lbl_hotkey_selector.AutoSize = true;
            this.lbl_hotkey_selector.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lbl_hotkey_selector.ForeColor = System.Drawing.Color.White;
            this.lbl_hotkey_selector.Location = new System.Drawing.Point(85, 182);
            this.lbl_hotkey_selector.Name = "lbl_hotkey_selector";
            this.lbl_hotkey_selector.Size = new System.Drawing.Size(270, 13);
            this.lbl_hotkey_selector.TabIndex = 2;
            this.lbl_hotkey_selector.Text = "Hold down the keys you\'d like to use, then release them";
            // 
            // btn_save
            // 
            this.btn_save.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btn_save.BackColor = System.Drawing.Color.Black;
            this.btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_save.ForeColor = System.Drawing.Color.White;
            this.btn_save.Location = new System.Drawing.Point(201, 222);
            this.btn_save.Name = "btn_save";
            this.btn_save.Size = new System.Drawing.Size(118, 40);
            this.btn_save.TabIndex = 3;
            this.btn_save.Text = "&Save";
            this.btn_save.UseVisualStyleBackColor = false;
            this.btn_save.Click += new System.EventHandler(this.btn_save_Click);
            // 
            // btn_clear
            // 
            this.btn_clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_clear.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_clear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_clear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_clear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_clear.ForeColor = System.Drawing.Color.White;
            this.btn_clear.Location = new System.Drawing.Point(363, 153);
            this.btn_clear.Name = "btn_clear";
            this.btn_clear.Size = new System.Drawing.Size(75, 26);
            this.btn_clear.TabIndex = 6;
            this.btn_clear.Text = "&Clear";
            this.btn_clear.UseVisualStyleBackColor = true;
            this.btn_clear.Click += new System.EventHandler(this.btn_clear_Click);
            // 
            // lbl_hotkey_heading
            // 
            this.lbl_hotkey_heading.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbl_hotkey_heading.AutoSize = true;
            this.lbl_hotkey_heading.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_hotkey_heading.ForeColor = System.Drawing.Color.White;
            this.lbl_hotkey_heading.Location = new System.Drawing.Point(97, 23);
            this.lbl_hotkey_heading.Name = "lbl_hotkey_heading";
            this.lbl_hotkey_heading.Size = new System.Drawing.Size(326, 20);
            this.lbl_hotkey_heading.TabIndex = 7;
            this.lbl_hotkey_heading.Text = "Choose a Hotkey for this Display Profile";
            this.lbl_hotkey_heading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_hotkey_description
            // 
            this.lbl_hotkey_description.AutoSize = true;
            this.lbl_hotkey_description.BackColor = System.Drawing.Color.Black;
            this.lbl_hotkey_description.ForeColor = System.Drawing.Color.White;
            this.lbl_hotkey_description.Location = new System.Drawing.Point(105, 67);
            this.lbl_hotkey_description.Name = "lbl_hotkey_description";
            this.lbl_hotkey_description.Size = new System.Drawing.Size(318, 52);
            this.lbl_hotkey_description.TabIndex = 0;
            this.lbl_hotkey_description.Text = resources.GetString("lbl_hotkey_description.Text");
            this.lbl_hotkey_description.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_back
            // 
            this.btn_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_back.ForeColor = System.Drawing.Color.White;
            this.btn_back.Location = new System.Drawing.Point(434, 239);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 8;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            this.btn_back.Click += new System.EventHandler(this.btn_back_Click);
            // 
            // HotkeyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(521, 274);
            this.Controls.Add(this.btn_back);
            this.Controls.Add(this.lbl_hotkey_heading);
            this.Controls.Add(this.btn_clear);
            this.Controls.Add(this.btn_save);
            this.Controls.Add(this.lbl_hotkey_selector);
            this.Controls.Add(this.txt_hotkey);
            this.Controls.Add(this.lbl_hotkey_description);
            this.Name = "HotkeyForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose a Hotkey";
            this.Activated += new System.EventHandler(this.HotkeyForm_Activated);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txt_hotkey;
        private System.Windows.Forms.Label lbl_hotkey_selector;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Button btn_clear;
        private System.Windows.Forms.Label lbl_hotkey_heading;
        private System.Windows.Forms.Label lbl_hotkey_description;
        private System.Windows.Forms.Button btn_back;
    }
}