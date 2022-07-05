
namespace DisplayMagician.UIForms
{
    partial class ChooseExecutableForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseExecutableForm));
            this.btn_select_app = new System.Windows.Forms.Button();
            this.dialog_open = new System.Windows.Forms.OpenFileDialog();
            this.lbl_installed_apps = new System.Windows.Forms.Label();
            this.lbl_title = new System.Windows.Forms.Label();
            this.lbl_select_exe = new System.Windows.Forms.Label();
            this.gb_select_exe = new System.Windows.Forms.GroupBox();
            this.btn_select_exe = new System.Windows.Forms.Button();
            this.lbl_or = new System.Windows.Forms.Label();
            this.ilv_installed_apps = new Manina.Windows.Forms.ImageListView();
            this.btn_back = new System.Windows.Forms.Button();
            this.gb_select_exe.SuspendLayout();
            this.SuspendLayout();
            // 
            // btn_select_app
            // 
            this.btn_select_app.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_select_app.Enabled = false;
            this.btn_select_app.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_select_app.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_select_app.ForeColor = System.Drawing.Color.White;
            this.btn_select_app.Location = new System.Drawing.Point(168, 506);
            this.btn_select_app.Name = "btn_select_app";
            this.btn_select_app.Size = new System.Drawing.Size(352, 40);
            this.btn_select_app.TabIndex = 40;
            this.btn_select_app.Text = "Select Application from List";
            this.btn_select_app.UseVisualStyleBackColor = true;
            this.btn_select_app.Click += new System.EventHandler(this.btn_select_app_Click);
            // 
            // dialog_open
            // 
            this.dialog_open.FileName = "openFileDialog1";
            // 
            // lbl_installed_apps
            // 
            this.lbl_installed_apps.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbl_installed_apps.AutoSize = true;
            this.lbl_installed_apps.BackColor = System.Drawing.Color.Black;
            this.lbl_installed_apps.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_installed_apps.ForeColor = System.Drawing.Color.White;
            this.lbl_installed_apps.Location = new System.Drawing.Point(141, 84);
            this.lbl_installed_apps.Name = "lbl_installed_apps";
            this.lbl_installed_apps.Size = new System.Drawing.Size(406, 20);
            this.lbl_installed_apps.TabIndex = 2;
            this.lbl_installed_apps.Text = "Select the Application you wish to use from the list below";
            this.lbl_installed_apps.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // lbl_title
            // 
            this.lbl_title.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbl_title.AutoSize = true;
            this.lbl_title.Font = new System.Drawing.Font("Microsoft Sans Serif", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_title.ForeColor = System.Drawing.Color.White;
            this.lbl_title.Location = new System.Drawing.Point(455, 19);
            this.lbl_title.Name = "lbl_title";
            this.lbl_title.Size = new System.Drawing.Size(375, 33);
            this.lbl_title.TabIndex = 44;
            this.lbl_title.Text = "Select an Executable to use";
            // 
            // lbl_select_exe
            // 
            this.lbl_select_exe.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbl_select_exe.AutoSize = true;
            this.lbl_select_exe.BackColor = System.Drawing.Color.Black;
            this.lbl_select_exe.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_select_exe.ForeColor = System.Drawing.Color.White;
            this.lbl_select_exe.Location = new System.Drawing.Point(775, 84);
            this.lbl_select_exe.Name = "lbl_select_exe";
            this.lbl_select_exe.Size = new System.Drawing.Size(438, 20);
            this.lbl_select_exe.TabIndex = 45;
            this.lbl_select_exe.Text = "Or, choose an executable from somewhere on your computer";
            this.lbl_select_exe.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // gb_select_exe
            // 
            this.gb_select_exe.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gb_select_exe.Controls.Add(this.btn_select_exe);
            this.gb_select_exe.Location = new System.Drawing.Point(740, 114);
            this.gb_select_exe.Name = "gb_select_exe";
            this.gb_select_exe.Size = new System.Drawing.Size(513, 381);
            this.gb_select_exe.TabIndex = 47;
            this.gb_select_exe.TabStop = false;
            // 
            // btn_select_exe
            // 
            this.btn_select_exe.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btn_select_exe.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_select_exe.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btn_select_exe.ForeColor = System.Drawing.Color.White;
            this.btn_select_exe.Location = new System.Drawing.Point(73, 172);
            this.btn_select_exe.Name = "btn_select_exe";
            this.btn_select_exe.Size = new System.Drawing.Size(377, 40);
            this.btn_select_exe.TabIndex = 41;
            this.btn_select_exe.Text = "Choose Exe from your Computer";
            this.btn_select_exe.UseVisualStyleBackColor = true;
            // 
            // lbl_or
            // 
            this.lbl_or.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lbl_or.AutoSize = true;
            this.lbl_or.BackColor = System.Drawing.Color.Black;
            this.lbl_or.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_or.ForeColor = System.Drawing.Color.White;
            this.lbl_or.Location = new System.Drawing.Point(674, 292);
            this.lbl_or.Name = "lbl_or";
            this.lbl_or.Size = new System.Drawing.Size(51, 29);
            this.lbl_or.TabIndex = 48;
            this.lbl_or.Text = "OR";
            this.lbl_or.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // ilv_installed_apps
            // 
            this.ilv_installed_apps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.ilv_installed_apps.Location = new System.Drawing.Point(27, 120);
            this.ilv_installed_apps.Name = "ilv_installed_apps";
            this.ilv_installed_apps.PersistentCacheDirectory = "";
            this.ilv_installed_apps.PersistentCacheSize = ((long)(100));
            this.ilv_installed_apps.Size = new System.Drawing.Size(634, 375);
            this.ilv_installed_apps.TabIndex = 49;
            this.ilv_installed_apps.UseWIC = true;
            this.ilv_installed_apps.ItemClick += new Manina.Windows.Forms.ItemClickEventHandler(this.ilv_installed_apps_ItemClick);
            // 
            // btn_back
            // 
            this.btn_back.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btn_back.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btn_back.FlatAppearance.MouseDownBackColor = System.Drawing.Color.IndianRed;
            this.btn_back.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Brown;
            this.btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btn_back.ForeColor = System.Drawing.Color.White;
            this.btn_back.Location = new System.Drawing.Point(1197, 534);
            this.btn_back.Name = "btn_back";
            this.btn_back.Size = new System.Drawing.Size(75, 23);
            this.btn_back.TabIndex = 50;
            this.btn_back.Text = "&Back";
            this.btn_back.UseVisualStyleBackColor = true;
            // 
            // ChooseExecutableForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(1284, 569);
            this.Controls.Add(this.btn_back);
            this.Controls.Add(this.ilv_installed_apps);
            this.Controls.Add(this.lbl_or);
            this.Controls.Add(this.gb_select_exe);
            this.Controls.Add(this.lbl_select_exe);
            this.Controls.Add(this.lbl_title);
            this.Controls.Add(this.btn_select_app);
            this.Controls.Add(this.lbl_installed_apps);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ChooseExecutableForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose an application";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.ChooseExecutableForm_Load);
            this.gb_select_exe.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btn_select_app;
        private System.Windows.Forms.OpenFileDialog dialog_open;
        private System.Windows.Forms.Label lbl_installed_apps;
        private System.Windows.Forms.Label lbl_title;
        private System.Windows.Forms.Label lbl_select_exe;
        private System.Windows.Forms.GroupBox gb_select_exe;
        private System.Windows.Forms.Label lbl_or;
        private Manina.Windows.Forms.ImageListView ilv_installed_apps;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.Button btn_select_exe;
    }
}