
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
            txt_hotkey = new System.Windows.Forms.TextBox();
            lbl_hotkey_selector = new System.Windows.Forms.Label();
            btn_save = new System.Windows.Forms.Button();
            btn_clear = new System.Windows.Forms.Button();
            lbl_hotkey_heading = new System.Windows.Forms.Label();
            lbl_hotkey_description = new System.Windows.Forms.Label();
            btn_back = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // txt_hotkey
            // 
            txt_hotkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_hotkey.HideSelection = false;
            txt_hotkey.Location = new System.Drawing.Point(100, 177);
            txt_hotkey.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_hotkey.Name = "txt_hotkey";
            txt_hotkey.ReadOnly = true;
            txt_hotkey.Size = new System.Drawing.Size(314, 26);
            txt_hotkey.TabIndex = 1;
            txt_hotkey.KeyDown += OnKeyDown;
            txt_hotkey.KeyPress += OnKeyPress;
            txt_hotkey.KeyUp += OnKeyUp;
            // 
            // lbl_hotkey_selector
            // 
            lbl_hotkey_selector.AutoSize = true;
            lbl_hotkey_selector.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            lbl_hotkey_selector.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_selector.Location = new System.Drawing.Point(99, 210);
            lbl_hotkey_selector.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_selector.Name = "lbl_hotkey_selector";
            lbl_hotkey_selector.Size = new System.Drawing.Size(270, 13);
            lbl_hotkey_selector.TabIndex = 2;
            lbl_hotkey_selector.Text = "Hold down the keys you'd like to use, then release them";
            // 
            // btn_save
            // 
            btn_save.Anchor = System.Windows.Forms.AnchorStyles.None;
            btn_save.BackColor = System.Drawing.Color.Black;
            btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_save.ForeColor = System.Drawing.Color.White;
            btn_save.Location = new System.Drawing.Point(235, 256);
            btn_save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_save.Name = "btn_save";
            btn_save.Size = new System.Drawing.Size(138, 38);
            btn_save.TabIndex = 3;
            btn_save.Text = "&Save";
            btn_save.UseVisualStyleBackColor = false;
            btn_save.Click += btn_save_Click;
            // 
            // btn_clear
            // 
            btn_clear.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_clear.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_clear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_clear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_clear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_clear.ForeColor = System.Drawing.Color.White;
            btn_clear.Location = new System.Drawing.Point(424, 177);
            btn_clear.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_clear.Name = "btn_clear";
            btn_clear.Size = new System.Drawing.Size(88, 26);
            btn_clear.TabIndex = 6;
            btn_clear.Text = "&Clear";
            btn_clear.UseVisualStyleBackColor = true;
            btn_clear.Click += btn_clear_Click;
            // 
            // lbl_hotkey_heading
            // 
            lbl_hotkey_heading.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_hotkey_heading.AutoSize = true;
            lbl_hotkey_heading.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_hotkey_heading.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_heading.Location = new System.Drawing.Point(113, 27);
            lbl_hotkey_heading.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_heading.Name = "lbl_hotkey_heading";
            lbl_hotkey_heading.Size = new System.Drawing.Size(326, 20);
            lbl_hotkey_heading.TabIndex = 7;
            lbl_hotkey_heading.Text = "Choose a Hotkey for this Display Profile";
            lbl_hotkey_heading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_hotkey_description
            // 
            lbl_hotkey_description.AutoSize = true;
            lbl_hotkey_description.BackColor = System.Drawing.Color.Black;
            lbl_hotkey_description.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_description.Location = new System.Drawing.Point(122, 77);
            lbl_hotkey_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_description.Name = "lbl_hotkey_description";
            lbl_hotkey_description.Size = new System.Drawing.Size(354, 60);
            lbl_hotkey_description.TabIndex = 0;
            lbl_hotkey_description.Text = resources.GetString("lbl_hotkey_description.Text");
            lbl_hotkey_description.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(506, 276);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 8;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // HotkeyForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(608, 316);
            Controls.Add(btn_back);
            Controls.Add(lbl_hotkey_heading);
            Controls.Add(btn_clear);
            Controls.Add(btn_save);
            Controls.Add(lbl_hotkey_selector);
            Controls.Add(txt_hotkey);
            Controls.Add(lbl_hotkey_description);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "HotkeyForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Choose a Hotkey";
            TopMost = true;
            Activated += HotkeyForm_Activated;
            ResumeLayout(false);
            PerformLayout();
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