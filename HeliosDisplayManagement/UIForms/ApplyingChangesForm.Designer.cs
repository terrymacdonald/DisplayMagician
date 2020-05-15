using System.Windows.Forms;
using HeliosPlus.Resources;

namespace HeliosPlus.UIForms
{
    sealed partial class ApplyingChangesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApplyingChangesForm));
            this.progressPanel = new System.Windows.Forms.Panel();
            this.lbl_sub_message = new System.Windows.Forms.Label();
            this.lbl_message = new System.Windows.Forms.Label();
            this.progressBar = new CircularProgressBar.CircularProgressBar();
            this.t_start = new System.Windows.Forms.Timer(this.components);
            this.t_countdown = new System.Windows.Forms.Timer(this.components);
            this.progressPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // progressPanel
            // 
            this.progressPanel.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.progressPanel.Controls.Add(this.lbl_sub_message);
            this.progressPanel.Controls.Add(this.lbl_message);
            this.progressPanel.Controls.Add(this.progressBar);
            this.progressPanel.Location = new System.Drawing.Point(12, 54);
            this.progressPanel.Name = "progressPanel";
            this.progressPanel.Size = new System.Drawing.Size(621, 270);
            this.progressPanel.TabIndex = 0;
            // 
            // lbl_sub_message
            // 
            this.lbl_sub_message.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_sub_message.ForeColor = System.Drawing.Color.White;
            this.lbl_sub_message.Location = new System.Drawing.Point(160, 85);
            this.lbl_sub_message.Name = "lbl_sub_message";
            this.lbl_sub_message.Size = new System.Drawing.Size(300, 16);
            this.lbl_sub_message.TabIndex = 2;
            this.lbl_sub_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbl_message
            // 
            this.lbl_message.Font = new System.Drawing.Font("Microsoft Sans Serif", 27.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(178)));
            this.lbl_message.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.lbl_message.Location = new System.Drawing.Point(3, 27);
            this.lbl_message.Name = "lbl_message";
            this.lbl_message.Size = new System.Drawing.Size(615, 61);
            this.lbl_message.TabIndex = 1;
            this.lbl_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // progressBar
            // 
            this.progressBar.AnimationFunction = WinFormAnimation.KnownAnimationFunctions.Liner;
            this.progressBar.AnimationSpeed = 1000;
            this.progressBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.progressBar.Font = new System.Drawing.Font("Microsoft Sans Serif", 39.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.progressBar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.progressBar.InnerColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.progressBar.InnerMargin = 0;
            this.progressBar.InnerWidth = 0;
            this.progressBar.Location = new System.Drawing.Point(243, 125);
            this.progressBar.MarqueeAnimationSpeed = 2000;
            this.progressBar.Name = "progressBar";
            this.progressBar.OuterColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.progressBar.OuterMargin = -8;
            this.progressBar.OuterWidth = 6;
            this.progressBar.ProgressColor = System.Drawing.Color.DodgerBlue;
            this.progressBar.ProgressWidth = 10;
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
            this.progressBar.TextMargin = new System.Windows.Forms.Padding(2, 5, 0, 0);
            this.progressBar.Value = 68;
            // 
            // t_start
            // 
            this.t_start.Interval = 1000;
            this.t_start.Tick += new System.EventHandler(this.t_start_Tick);
            // 
            // t_countdown
            // 
            this.t_countdown.Interval = 1000;
            this.t_countdown.Tick += new System.EventHandler(this.t_countdown_Tick);
            // 
            // ApplyingChangesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.ClientSize = new System.Drawing.Size(645, 418);
            this.ControlBox = false;
            this.Controls.Add(this.progressPanel);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ApplyingChangesForm";
            this.Opacity = 0D;
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "HeliosPlus - Please Wait";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SplashForm_FormClosing);
            this.Load += new System.EventHandler(this.SplashForm_Reposition);
            this.Shown += new System.EventHandler(this.SplashForm_Shown);
            this.LocationChanged += new System.EventHandler(this.SplashForm_Reposition);
            this.Leave += new System.EventHandler(this.SplashForm_Reposition);
            this.Move += new System.EventHandler(this.SplashForm_Reposition);
            this.progressPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Panel progressPanel;
        private CircularProgressBar.CircularProgressBar progressBar;
        private System.Windows.Forms.Label lbl_message;
        private System.Windows.Forms.Timer t_start;
        private System.Windows.Forms.Timer t_countdown;
        private Label lbl_sub_message;
    }
}