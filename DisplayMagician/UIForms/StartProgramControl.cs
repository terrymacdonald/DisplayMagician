using DisplayMagician.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class StartProgramControl : UserControl
    {
        public StartProgramControl()
        {
            InitializeComponent();
        }

        public StartProgramControl(StartProgram startProgram)
        {
            InitializeComponent();

            // Now initialise the controls 
            txt_start_program.Text = startProgram.Executable;
            txt_start_program.BackColor = Color.White;
            txt_start_program.ForeColor = Color.Black;
            txt_start_program.Visible= true;
            txt_start_program.BringToFront();
            cb_start_program.Checked = startProgram.Enabled;
            txt_start_program.ForeColor = Color.White;
            cb_start_program_pass_args.Checked = startProgram.ExecutableArgumentsRequired;
            txt_start_program_args.Text = startProgram.Arguments;
            cb_start_program_close.Checked = startProgram.CloseOnFinish;
            cb_dont_start_if_running.Checked = startProgram.DontStartIfAlreadyRunning;


        }


        private void btn_start_program_Click(object sender, EventArgs e)
        {
            txt_start_program.Text = getExeFile();
        }

        private void cb_start_program_CheckedChanged(object sender, EventArgs e)
        {
            // Disable the start program 1 fields
            if (cb_start_program.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program.Visible = true;
                btn_start_program.Visible = true;
                cb_start_program_pass_args.Visible = true;
                cb_start_program_close.Visible = true;
                cb_dont_start_if_running.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program.Visible = false;
                btn_start_program.Visible = false;
                cb_start_program_pass_args.Visible = false;
                cb_start_program_close.Visible = false;
                cb_dont_start_if_running.Visible = false;
            }
        }

        private void cb_start_program_pass_args_CheckedChanged(object sender, EventArgs e)
        {
            // Disable the start program 1 fields
            if (cb_start_program_pass_args.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program_args.Visible = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program_args.Visible = false;
            }
        }

        private string getExeFile()
        {
            string textToReturn = "";
            OpenFileDialog dialog_open = new OpenFileDialog();
            if (dialog_open.ShowDialog(this) == DialogResult.OK)
            {
                if (File.Exists(dialog_open.FileName))
                {
                    textToReturn = dialog_open.FileName;
                    dialog_open.FileName = string.Empty;
                }
                else
                {
                    MessageBox.Show(
                        Language.Selected_file_is_not_a_valid_file,
                        Language.Executable,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                }
            }
            return textToReturn;
        }

        private void lbl_start_program_Click(object sender, EventArgs e)
        {
            if (!cb_start_program.Checked)
                cb_start_program.CheckState = CheckState.Checked;
            else
                cb_start_program.CheckState = CheckState.Unchecked;
        }


        private void btn_delete_Click(object sender, EventArgs e)
        {

        }

    }
    
}
