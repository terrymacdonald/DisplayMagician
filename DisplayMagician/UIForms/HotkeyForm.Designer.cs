
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
            btn_clear_all = new System.Windows.Forms.Button();
            lbl_hotkey_heading = new System.Windows.Forms.Label();
            lbl_hotkey_description = new System.Windows.Forms.Label();
            btn_back = new System.Windows.Forms.Button();
            lv_hotkeys = new System.Windows.Forms.ListView();
            btn_clear = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // txt_hotkey
            // 
            txt_hotkey.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            txt_hotkey.HideSelection = false;
            txt_hotkey.Location = new System.Drawing.Point(50, 291);
            txt_hotkey.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            txt_hotkey.Name = "txt_hotkey";
            txt_hotkey.ReadOnly = true;
            txt_hotkey.Size = new System.Drawing.Size(420, 26);
            txt_hotkey.TabIndex = 1;
            // 
            // lbl_hotkey_selector
            // 
            lbl_hotkey_selector.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            lbl_hotkey_selector.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_selector.Location = new System.Drawing.Point(50, 320);
            lbl_hotkey_selector.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_selector.Name = "lbl_hotkey_selector";
            lbl_hotkey_selector.Size = new System.Drawing.Size(420, 59);
            lbl_hotkey_selector.TabIndex = 2;
            lbl_hotkey_selector.Text = "Hold down the keys or buttons you'd like to use as a Hotkey, then release them. Selected keys or buttons will clear automatically in 10 seconds or you can clear them yourself with the Clear button.";
            lbl_hotkey_selector.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btn_save
            // 
            btn_save.Anchor = System.Windows.Forms.AnchorStyles.None;
            btn_save.BackColor = System.Drawing.Color.Black;
            btn_save.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_save.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_save.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_save.ForeColor = System.Drawing.Color.White;
            btn_save.Location = new System.Drawing.Point(478, 291);
            btn_save.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_save.Name = "btn_save";
            btn_save.Size = new System.Drawing.Size(91, 26);
            btn_save.TabIndex = 3;
            btn_save.Text = "&Add";
            btn_save.UseVisualStyleBackColor = false;
            btn_save.Click += btn_apply_Click;
            // 
            // btn_clear_all
            // 
            btn_clear_all.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_clear_all.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btn_clear_all.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_clear_all.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_clear_all.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_clear_all.ForeColor = System.Drawing.Color.White;
            btn_clear_all.Location = new System.Drawing.Point(577, 244);
            btn_clear_all.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_clear_all.Name = "btn_clear_all";
            btn_clear_all.Size = new System.Drawing.Size(88, 26);
            btn_clear_all.TabIndex = 6;
            btn_clear_all.Text = "&Remove All";
            btn_clear_all.UseVisualStyleBackColor = true;
            btn_clear_all.Click += btn_clear_Click;
            // 
            // lbl_hotkey_heading
            // 
            lbl_hotkey_heading.Anchor = System.Windows.Forms.AnchorStyles.Top;
            lbl_hotkey_heading.AutoSize = true;
            lbl_hotkey_heading.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
            lbl_hotkey_heading.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_heading.Location = new System.Drawing.Point(198, 24);
            lbl_hotkey_heading.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_heading.Name = "lbl_hotkey_heading";
            lbl_hotkey_heading.Size = new System.Drawing.Size(326, 20);
            lbl_hotkey_heading.TabIndex = 7;
            lbl_hotkey_heading.Text = "Choose a Hotkey for this Display Profile";
            lbl_hotkey_heading.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_hotkey_description
            // 
            lbl_hotkey_description.BackColor = System.Drawing.Color.Black;
            lbl_hotkey_description.ForeColor = System.Drawing.Color.White;
            lbl_hotkey_description.Location = new System.Drawing.Point(50, 54);
            lbl_hotkey_description.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            lbl_hotkey_description.Name = "lbl_hotkey_description";
            lbl_hotkey_description.Size = new System.Drawing.Size(627, 84);
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
            btn_back.Location = new System.Drawing.Point(621, 352);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 8;
            btn_back.Text = "&Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // lv_hotkeys
            // 
            lv_hotkeys.Location = new System.Drawing.Point(50, 151);
            lv_hotkeys.Name = "lv_hotkeys";
            lv_hotkeys.ShowGroups = false;
            lv_hotkeys.Size = new System.Drawing.Size(519, 119);
            lv_hotkeys.TabIndex = 9;
            lv_hotkeys.UseCompatibleStateImageBehavior = false;
            lv_hotkeys.MouseClick += lv_hotkeys_MouseClick;
            // 
            // btn_clear
            // 
            btn_clear.Anchor = System.Windows.Forms.AnchorStyles.None;
            btn_clear.BackColor = System.Drawing.Color.Black;
            btn_clear.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            btn_clear.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            btn_clear.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_clear.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_clear.ForeColor = System.Drawing.Color.White;
            btn_clear.Location = new System.Drawing.Point(577, 291);
            btn_clear.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_clear.Name = "btn_clear";
            btn_clear.Size = new System.Drawing.Size(91, 26);
            btn_clear.TabIndex = 10;
            btn_clear.Text = "&Clear";
            btn_clear.UseVisualStyleBackColor = false;
            btn_clear.Click += btn_clear_Click_1;
            // 
            // HotkeyForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(723, 392);
            Controls.Add(btn_clear);
            Controls.Add(lv_hotkeys);
            Controls.Add(btn_back);
            Controls.Add(lbl_hotkey_heading);
            Controls.Add(btn_clear_all);
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
            FormClosing += HotkeyForm_FormClosing;
            Load += HotkeyForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.TextBox txt_hotkey;
        private System.Windows.Forms.Label lbl_hotkey_selector;
        private System.Windows.Forms.Button btn_save;
        private System.Windows.Forms.Button btn_clear_all;
        private System.Windows.Forms.Label lbl_hotkey_heading;
        private System.Windows.Forms.Label lbl_hotkey_description;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.ListView lv_hotkeys;
        private System.Windows.Forms.Button btn_clear;
    }
}