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

        private StartProgram myStartProgram = new StartProgram() { };

        public StartProgram StartProgram
        {
            get
            {
                return myStartProgram;
            }
            set
            {
                myStartProgram = value;
                UpdateUI();
            }
        }
        public StartProgramControl()
        {
            InitializeComponent();
        }

        public StartProgramControl(StartProgram startProgram)
        {
            InitializeComponent();

            // Update the text with the start program info
            myStartProgram = startProgram;
            UpdateUI();
        }

        public StartProgramControl(StartProgram startProgram, int startProgramOrder)
        {
            InitializeComponent();

            // Update the text with the start program info
            myStartProgram = startProgram;
            myStartProgram.Priority = startProgramOrder;
            UpdateUI();
        }

        public void UpdateUI()
        {
            // Now populate the controls with the start program data
            lbl_priority.Text = myStartProgram.Priority.ToString();
            txt_start_program.Text = myStartProgram.Executable;
            cb_disable_start_program.Checked = myStartProgram.Disabled;
            cb_start_program_pass_args.Checked = myStartProgram.ExecutableArgumentsRequired;
            txt_start_program_args.Text = myStartProgram.Arguments;
            cb_start_program_close.Checked = myStartProgram.CloseOnFinish;
            cb_dont_start_if_running.Checked = myStartProgram.DontStartIfAlreadyRunning;

        }

        public void ChangePriority(int priority)
        {
            // Now update the priority field
            myStartProgram.Priority = priority;
            lbl_priority.Text = priority.ToString();
        }


        private void btn_start_program_Click(object sender, EventArgs e)
        {
            txt_start_program.Text = getExeFile();
        }

        private void cb_start_program_CheckedChanged(object sender, EventArgs e)
        {
            // Disable the start program fields
            if (cb_disable_start_program.Checked)
            {
                myStartProgram.Disabled = true;
                // Disable the Executable Arguments Text field
                txt_start_program.Enabled = false;
                btn_start_program.Enabled = false;
                txt_start_program_args.Enabled = false;
                cb_start_program_pass_args.Enabled = false;
                cb_start_program_close.Enabled = false;
                cb_dont_start_if_running.Enabled = false;
            }
            else
            {
                myStartProgram.Disabled = false;
                // Enable the Executable Arguments Text field
                txt_start_program.Enabled = true;
                btn_start_program.Enabled = true;
                txt_start_program_args.Enabled = true;
                cb_start_program_pass_args.Enabled = true;
                cb_start_program_close.Enabled = true;
                cb_dont_start_if_running.Enabled = true;
            }            
        }

        private void cb_start_program_pass_args_CheckedChanged(object sender, EventArgs e)
        {
            // Disable the start program 1 fields
            if (cb_start_program_pass_args.Checked)
            {
                // Enable the Executable Arguments Text field
                txt_start_program_args.Enabled = true;
            }
            else
            {
                // Disable the Executable Arguments Text field
                txt_start_program_args.Enabled = false;
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

        private void btn_delete_Click(object sender, EventArgs e)
        {
            ((ShortcutForm)this.Parent.Parent.Parent.Parent).RemoveStartProgram(this);
            //this.Parent.Controls.Remove(this);
        }

        private void pb_up_arrow_MouseEnter(object sender, EventArgs e)
        {
            pb_up_arrow.Image = DisplayMagician.Properties.Resources.redarrowsup;
        }

        private void pb_up_arrow_MouseLeave(object sender, EventArgs e)
        {
            pb_up_arrow.Image = DisplayMagician.Properties.Resources.whitearrowsup;
        }

        private void pb_down_arrow_MouseEnter(object sender, EventArgs e)
        {
            pb_down_arrow.Image = DisplayMagician.Properties.Resources.RedArrows;
        }

        private void pb_down_arrow_MouseLeave(object sender, EventArgs e)
        {
            pb_down_arrow.Image = DisplayMagician.Properties.Resources.whitearrows;
        }

        private void pb_down_arrow_Click(object sender, EventArgs e)
        {
            ((ShortcutForm)this.Parent.Parent.Parent.Parent).StartProgramLater(this);
        }

        private void pb_up_arrow_Click(object sender, EventArgs e)
        {
            ((ShortcutForm)this.Parent.Parent.Parent.Parent).StartProgramEarlier(this);
        }

        private void cb_dont_start_if_running_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_dont_start_if_running.Checked)
            {
                myStartProgram.DontStartIfAlreadyRunning = true;
            }
            else
            {
                myStartProgram.DontStartIfAlreadyRunning = false;
            }
        }

        private void cb_start_program_close_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_start_program_close.Checked)
            {
                myStartProgram.CloseOnFinish = true;
            }
            else
            {
                myStartProgram.CloseOnFinish = false;
            }
        }

        private void txt_start_program_TextChanged(object sender, EventArgs e)
        {
            myStartProgram.Executable = txt_start_program.Text;            
        }

        private void txt_start_program_args_TextChanged(object sender, EventArgs e)
        {
            myStartProgram.Arguments = txt_start_program_args.Text;
        }
    }    
}
