﻿namespace DisplayMagician.UIForms
{
    partial class ApplyingProfileForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplyingProfileForm));
            this.progressPanel = new System.Windows.Forms.Panel();
            this.lbl_sub_message = new System.Windows.Forms.Label();
            this.progressBar = new CircularProgressBar.CircularProgressBar();
            this.lbl_message = new System.Windows.Forms.Label();
            this.t_countdown = new System.Windows.Forms.Timer(this.components);
            this.btn_close = new System.Windows.Forms.Button();
            this.progressPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressPanel
            // 
            this.progressPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.progressPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.progressPanel.Controls.Add(this.lbl_sub_message);
            this.progressPanel.Controls.Add(this.progressBar);
            this.progressPanel.Controls.Add(this.lbl_message);
            this.progressPanel.Location = new System.Drawing.Point(40, 154);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Size = new System.Drawing.Size(720, 270);
            this.progressPanel.TabIndex = 1;
            // 
            // lbl_sub_message
            // 
            this.lbl_sub_message.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_sub_message.ForeColor = System.Drawing.Color.White;
            this.lbl_sub_message.Location = new System.Drawing.Point(110, 67);
            this.lbl_sub_message.Name = "lbl_sub_message";
            this.lbl_sub_message.Size = new System.Drawing.Size(501, 30);
            this.lbl_sub_message.TabIndex = 2;
            this.lbl_sub_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar
            // 
            this.progressBar.AnimationFunction = WinFormAnimation.KnownAnimationFunctions.Liner;
            this.progressBar.AnimationSpeed = 1000;
            this.progressBar.BackColor = System.Drawing.Color.Transparent;
            this.progressBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 39.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.progressBar.InnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.progressBar.InnerMargin = 0;
            this.progressBar.InnerWidth = 1;
            this.progressBar.Location = new System.Drawing.Point(293, 115);
            this.progressBar.MarqueeAnimationSpeed = 2000;
            this.progressBar.Name = "progressBar";
            this.progressBar.OuterColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.progressBar.OuterMargin = 0;
            this.progressBar.OuterWidth = 0;
            this.progressBar.ProgressColor = System.Drawing.Color.DodgerBlue;
            this.progressBar.ProgressWidth = 23;
            this.progressBar.SecondaryFont = new System.Drawing.Font("Microsoft Sans Serif", 26.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressBar.Size = new System.Drawing.Size(135, 135);
            this.progressBar.StartAngle = 270;
            this.progressBar.SubscriptColor = System.Drawing.Color.FromArgb(((int)(((byte)(166)))), ((int)(((byte)(166)))), ((int)(((byte)(166)))));
            this.progressBar.SubscriptMargin = new System.Windows.Forms.Padding(10, -35, 0, 0);
            this.progressBar.SubscriptText = "";
            this.progressBar.SuperscriptColor = System.Drawing.Color.Gray;
            this.progressBar.SuperscriptMargin = new System.Windows.Forms.Padding(0, 30, 0, 0);
            this.progressBar.SuperscriptText = "";
            this.progressBar.TabIndex = 0;
            this.progressBar.TextMargin = new System.Windows.Forms.Padding(0, 4, 1, 0);
            this.progressBar.Value = 68;
            // 
            // lbl_message
            // 
            this.lbl_message.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.lbl_message.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lbl_message.Location = new System.Drawing.Point(3, 0);
            this.lbl_message.Name = "lbl_message";
            this.lbl_message.Size = new System.Drawing.Size(714, 61);
            this.lbl_message.TabIndex = 1;
            this.lbl_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // t_countdown
            // 
            this.t_countdown.Interval = 1000;
            this.t_countdown.Tick += new System.EventHandler(this.t_countdown_Tick);
            // 
            // btn_close
            // 
            this.btn_close.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_close.FlatAppearance.BorderSize = 0;
            this.btn_close.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_close.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_close.ForeColor = System.Drawing.Color.White;
            this.btn_close.Location = new System.Drawing.Point(759, 12);
            this.btn_close.Name = "btn_close";
            this.btn_close.Size = new System.Drawing.Size(29, 32);
            this.btn_close.TabIndex = 2;
            this.btn_close.Text = "X";
            this.btn_close.UseVisualStyleBackColor = true;
            this.btn_close.Click += new System.EventHandler(this.btn_close_Click);
            // 
            // ApplyingProfileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(50)))), ((int)(((byte)(50)))), ((int)(((byte)(50)))));
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btn_close);
            this.Controls.Add(this.progressPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ApplyingProfileForm";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DisplayMagician - Please Wait";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ApplyingProfileForm_FormClosing);
            this.Load += new System.EventHandler(this.ApplyingProfileForm_Reposition);
            this.Shown += new System.EventHandler(this.ApplyingProfileForm_Shown);
            this.LocationChanged += new System.EventHandler(this.ApplyingProfileForm_Reposition);
            this.Leave += new System.EventHandler(this.ApplyingProfileForm_Reposition);
            this.Move += new System.EventHandler(this.ApplyingProfileForm_Reposition);
            this.progressPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel progressPanel;
        private CircularProgressBar.CircularProgressBar progressBar;
        private System.Windows.Forms.Label lbl_sub_message;
        private System.Windows.Forms.Label lbl_message;
        private System.Windows.Forms.Timer t_countdown;
        private System.Windows.Forms.Button btn_close;
    }
}