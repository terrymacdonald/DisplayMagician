using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HeliosPlus.UIForms
{
    public partial class MaskedForm : Form
    {
        private Label lbl_message;

        // from https://stackoverflow.com/questions/21530699/how-to-draw-overlay-window-winform-apps-c-sharp

        public static MaskedForm mask;



        private MaskedForm(Form parent, string message)
        {
            this.SuspendLayout();
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            //this.BackColor = System.Drawing.Color.Black;
            this.Opacity = 0.50;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.Size = parent.ClientSize;
            this.Location = parent.PointToScreen(System.Drawing.Point.Empty);
            //this.lbl_message.BackColor = System.Drawing.Color.Black;
            this.lbl_message.Text = message;
            this.MdiParent = parent.MdiParent;
            this.TopLevel = true;
            parent.Move += AdjustPosition;
            parent.SizeChanged += AdjustPosition;
            this.ResumeLayout();
        }

        private void AdjustPosition(object sender, EventArgs e)
        {
            Form parent = sender as Form;
            this.Location = parent.PointToScreen(System.Drawing.Point.Empty);
            this.ClientSize = parent.ClientSize;
        }

        //
        public static MaskedForm Show(Form parent, string message)
        {
            mask = new MaskedForm(parent, message);
            mask.Show();
            mask.Activate();
            Application.DoEvents();
            return mask;
        }

        public static MaskedForm ShowDialog(Form parent, string message)
        {
            mask = new MaskedForm(parent, message);
            mask.ShowDialog();
            mask.Activate();
            Application.DoEvents();
            return mask;
        }

        /*public static void Close()
        {
            MaskedForm.mask.Close();
        }*/

        private void InitializeComponent()
        {
            this.lbl_message = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbl_message
            // 
            this.lbl_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lbl_message.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbl_message.Location = new System.Drawing.Point(161, 93);
            this.lbl_message.Name = "lbl_message";
            this.lbl_message.Size = new System.Drawing.Size(415, 104);
            this.lbl_message.TabIndex = 0;
            this.lbl_message.Text = "label1";
            this.lbl_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MaskedForm
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(714, 310);
            this.Controls.Add(this.lbl_message);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "MaskedForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MaskedForm_FormClosing);
            this.Load += new System.EventHandler(this.MaskedForm_Load);
            this.ResumeLayout(false);

        }

        private void MaskedForm_Load(object sender, EventArgs e)
        {
        }

        private void MaskedForm_FormClosing(object sender, FormClosingEventArgs e)
        {
        }
    }
}
