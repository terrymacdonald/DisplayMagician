using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace DisplayMagician.UIForms
{
    public partial class StartMessageForm : Form
    {

        public string Filename;

        public StartMessageForm()
        {
            InitializeComponent();
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void StartMessageForm_Load(object sender, EventArgs e)
        {
            if (File.Exists(Filename))
            {
                rtb_message.LoadFile(Filename, RichTextBoxStreamType.RichText);
            }            
        }
    }
}
