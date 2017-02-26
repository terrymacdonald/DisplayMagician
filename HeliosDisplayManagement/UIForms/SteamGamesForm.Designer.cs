using HeliosDisplayManagement.Resources;

namespace HeliosDisplayManagement.UIForms
{
    partial class SteamGamesForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SteamGamesForm));
            this.btn_cancel = new System.Windows.Forms.Button();
            this.btn_ok = new System.Windows.Forms.Button();
            this.il_games = new System.Windows.Forms.ImageList(this.components);
            this.lv_games = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // btn_cancel
            // 
            this.btn_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_cancel.Location = new System.Drawing.Point(171, 476);
            this.btn_cancel.Name = "btn_cancel";
            this.btn_cancel.Size = new System.Drawing.Size(75, 23);
            this.btn_cancel.TabIndex = 1;
            this.btn_cancel.Text = global::HeliosDisplayManagement.Resources.Language.Cancel;
            this.btn_cancel.UseVisualStyleBackColor = true;
            // 
            // btn_ok
            // 
            this.btn_ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_ok.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btn_ok.Enabled = false;
            this.btn_ok.Location = new System.Drawing.Point(252, 476);
            this.btn_ok.Name = "btn_ok";
            this.btn_ok.Size = new System.Drawing.Size(75, 23);
            this.btn_ok.TabIndex = 2;
            this.btn_ok.Text = global::HeliosDisplayManagement.Resources.Language.Ok;
            this.btn_ok.UseVisualStyleBackColor = true;
            // 
            // il_games
            // 
            this.il_games.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.il_games.ImageSize = new System.Drawing.Size(32, 32);
            this.il_games.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // lv_games
            // 
            this.lv_games.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lv_games.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(248)))), ((int)(((byte)(248)))), ((int)(((byte)(248)))));
            this.lv_games.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lv_games.LargeImageList = this.il_games;
            this.lv_games.Location = new System.Drawing.Point(-1, -1);
            this.lv_games.Name = "lv_games";
            this.lv_games.Size = new System.Drawing.Size(341, 466);
            this.lv_games.SmallImageList = this.il_games;
            this.lv_games.TabIndex = 3;
            this.lv_games.UseCompatibleStateImageBehavior = false;
            this.lv_games.SelectedIndexChanged += new System.EventHandler(this.lv_games_SelectedIndexChanged);
            this.lv_games.DoubleClick += new System.EventHandler(this.lv_games_DoubleClick);
            // 
            // SteamGamesForm
            // 
            this.AcceptButton = this.btn_ok;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(184)))), ((int)(((byte)(196)))));
            this.CancelButton = this.btn_cancel;
            this.ClientSize = new System.Drawing.Size(339, 511);
            this.Controls.Add(this.lv_games);
            this.Controls.Add(this.btn_ok);
            this.Controls.Add(this.btn_cancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SteamGamesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = global::HeliosDisplayManagement.Resources.Language.Steam_Games;
            this.Load += new System.EventHandler(this.SteamGamesForm_Load);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button btn_cancel;
        private System.Windows.Forms.Button btn_ok;
        private System.Windows.Forms.ImageList il_games;
        private System.Windows.Forms.ListView lv_games;
    }
}