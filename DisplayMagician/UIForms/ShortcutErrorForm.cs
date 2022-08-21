using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class ShortcutErrorForm : Form
    {
        public ShortcutErrorForm()
        {
            InitializeComponent();
        }

        public List<string> Errors
        {
            get;
            set;
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ShortcutErrorForm_Load(object sender, EventArgs e)
        {
            
            if (Errors != null)
            {
                for (int i = 0; i < Errors.Count; i++) 
                {
                    Errors[i] = $"- {Errors[i]}";
                }

                if (Errors.Count <= 0) 
                {
                    return;
                }
                else if (Errors.Count == 1)
                {
                    txt_errors.Text = $"Unfortunately we have found an error that you need to fix:{Environment.NewLine}{Environment.NewLine}";
                }
                else if (Errors.Count > 8)
                {
                    txt_errors.Text = $"Unfortunately we have found lots of errors that you need to fix:{Environment.NewLine}{Environment.NewLine}";
                }
                else if (Errors.Count > 3)
                {
                    txt_errors.Text = $"Unfortunately we have found a few errors that you need to fix:{Environment.NewLine}{Environment.NewLine}";
                }
                else if (Errors.Count > 1)
                {
                    txt_errors.Text = $"Unfortunately we have found a couple of errors that you need to fix:{Environment.NewLine}{Environment.NewLine}";
                }

                txt_errors.Text += String.Join($"{Environment.NewLine}{Environment.NewLine}", Errors);
            }
        }
    }
}
