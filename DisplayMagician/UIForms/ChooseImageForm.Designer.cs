
namespace DisplayMagician.UIForms
{
    partial class ChooseImageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChooseImageForm));
            lv_icons = new System.Windows.Forms.ListView();
            columnHeaderName = new System.Windows.Forms.ColumnHeader();
            columnHeaderSize = new System.Windows.Forms.ColumnHeader();
            pb_selected_icon = new System.Windows.Forms.PictureBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            btn_add = new System.Windows.Forms.Button();
            btn_select = new System.Windows.Forms.Button();
            btn_back = new System.Windows.Forms.Button();
            dialog_open = new System.Windows.Forms.OpenFileDialog();
            btn_remove = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)pb_selected_icon).BeginInit();
            SuspendLayout();
            // 
            // lv_icons
            // 
            lv_icons.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            lv_icons.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeaderName, columnHeaderSize });
            lv_icons.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            lv_icons.Location = new System.Drawing.Point(17, 42);
            lv_icons.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            lv_icons.MultiSelect = false;
            lv_icons.Name = "lv_icons";
            lv_icons.ShowGroups = false;
            lv_icons.Size = new System.Drawing.Size(344, 230);
            lv_icons.TabIndex = 1;
            lv_icons.UseCompatibleStateImageBehavior = false;
            lv_icons.View = System.Windows.Forms.View.Details;
            lv_icons.SelectedIndexChanged += lv_icons_SelectedIndexChanged;
            // 
            // columnHeaderName
            // 
            columnHeaderName.Text = "Name";
            columnHeaderName.Width = 270;
            // 
            // columnHeaderSize
            // 
            columnHeaderSize.Text = "Size";
            columnHeaderSize.Width = 70;
            // 
            // pb_selected_icon
            // 
            pb_selected_icon.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            pb_selected_icon.BackColor = System.Drawing.Color.DimGray;
            pb_selected_icon.Location = new System.Drawing.Point(381, 42);
            pb_selected_icon.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            pb_selected_icon.Name = "pb_selected_icon";
            pb_selected_icon.Size = new System.Drawing.Size(233, 231);
            pb_selected_icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            pb_selected_icon.TabIndex = 1;
            pb_selected_icon.TabStop = false;
            // 
            // label1
            // 
            label1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            label1.AutoSize = true;
            label1.BackColor = System.Drawing.Color.Black;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label1.ForeColor = System.Drawing.Color.White;
            label1.Location = new System.Drawing.Point(108, 23);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(137, 16);
            label1.TabIndex = 0;
            label1.Text = "Choose image to use:";
            label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label2
            // 
            label2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            label2.AutoSize = true;
            label2.BackColor = System.Drawing.Color.Black;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            label2.ForeColor = System.Drawing.Color.White;
            label2.Location = new System.Drawing.Point(445, 23);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(105, 16);
            label2.TabIndex = 0;
            label2.Text = "Selected image:";
            label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // btn_add
            // 
            btn_add.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_add.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_add.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_add.ForeColor = System.Drawing.Color.White;
            btn_add.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            btn_add.Location = new System.Drawing.Point(17, 287);
            btn_add.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_add.Name = "btn_add";
            btn_add.Size = new System.Drawing.Size(100, 35);
            btn_add.TabIndex = 2;
            btn_add.Text = "Add images";
            btn_add.UseVisualStyleBackColor = true;
            btn_add.Click += btn_add_Click;
            // 
            // btn_select
            // 
            btn_select.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_select.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_select.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_select.ForeColor = System.Drawing.Color.White;
            btn_select.Location = new System.Drawing.Point(265, 287);
            btn_select.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_select.Name = "btn_select";
            btn_select.Size = new System.Drawing.Size(208, 35);
            btn_select.TabIndex = 4;
            btn_select.Text = "Save and use selected image";
            btn_select.UseVisualStyleBackColor = true;
            btn_select.Click += btn_select_Click;
            // 
            // btn_back
            // 
            btn_back.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            btn_back.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_back.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_back.ForeColor = System.Drawing.Color.White;
            btn_back.Location = new System.Drawing.Point(526, 320);
            btn_back.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_back.Name = "btn_back";
            btn_back.Size = new System.Drawing.Size(88, 27);
            btn_back.TabIndex = 5;
            btn_back.Text = "Back";
            btn_back.UseVisualStyleBackColor = true;
            btn_back.Click += btn_back_Click;
            // 
            // dialog_open
            // 
            dialog_open.FileName = "openFileDialog1";
            // 
            // btn_remove
            // 
            btn_remove.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btn_remove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn_remove.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            btn_remove.ForeColor = System.Drawing.Color.White;
            btn_remove.Location = new System.Drawing.Point(125, 287);
            btn_remove.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btn_remove.Name = "btn_remove";
            btn_remove.Size = new System.Drawing.Size(132, 35);
            btn_remove.TabIndex = 3;
            btn_remove.Text = "Remove image";
            btn_remove.UseVisualStyleBackColor = true;
            btn_remove.Click += btn_remove_Click;
            // 
            // ChooseImageForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            BackColor = System.Drawing.Color.Black;
            ClientSize = new System.Drawing.Size(631, 360);
            Controls.Add(btn_remove);
            Controls.Add(btn_back);
            Controls.Add(btn_select);
            Controls.Add(btn_add);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pb_selected_icon);
            Controls.Add(lv_icons);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(647, 399);
            Name = "ChooseImageForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Choose shortcut image";
            TopMost = true;
            Load += ChooseIconForm_Load;
            ((System.ComponentModel.ISupportInitialize)pb_selected_icon).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lv_icons;
        private System.Windows.Forms.PictureBox pb_selected_icon;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_add;
        private System.Windows.Forms.Button btn_select;
        private System.Windows.Forms.Button btn_back;
        private System.Windows.Forms.ColumnHeader columnHeaderName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.OpenFileDialog dialog_open;
        private System.Windows.Forms.Button btn_remove;
    }
}