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
using DisplayMagician.Processes;
using Microsoft.Win32;

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

            // Patch any null start programs (old error that needs repairing)
            // This code will save the file as it should be saved!
            if (myStartProgram.Arguments == null)
            {
                myStartProgram.Arguments = "";
            }

            // Prepare the start program process priority combo box
            cbx_start_program_priority.DataSource = new ComboItem[] {
                    new ComboItem{ Value = ProcessPriority.High, Text = "High" },
                    new ComboItem{ Value = ProcessPriority.AboveNormal, Text = "Above Normal" },
                    new ComboItem{ Value = ProcessPriority.Normal, Text = "Normal" },
                    new ComboItem{ Value = ProcessPriority.BelowNormal, Text = "Below Normal" },
                    new ComboItem{ Value = ProcessPriority.Idle, Text = "Idle" },
                };
            cbx_start_program_priority.ValueMember = "Value";
            cbx_start_program_priority.DisplayMember = "Text";
            cbx_start_program_priority.SelectedItem = "Normal";
            cbx_start_program_priority.Enabled = true;

        }

        public StartProgramControl(StartProgram startProgram)
        {
            InitializeComponent();

            // Update the text with the start program info
            myStartProgram = startProgram;

            // Patch any null start programs (old error that needs repairing)
            // This code will save the file as it should be saved!
            if (myStartProgram.Arguments == null)
            {
                myStartProgram.Arguments = "";
            }

            // Prepare the start program process priority combo box
            cbx_start_program_priority.DataSource = new ComboItem[] {
                    new ComboItem{ Value = ProcessPriority.High, Text = "High" },
                    new ComboItem{ Value = ProcessPriority.AboveNormal, Text = "Above Normal" },
                    new ComboItem{ Value = ProcessPriority.Normal, Text = "Normal" },
                    new ComboItem{ Value = ProcessPriority.BelowNormal, Text = "Below Normal" },
                    new ComboItem{ Value = ProcessPriority.Idle, Text = "Idle" },
                };
            cbx_start_program_priority.ValueMember = "Value";
            cbx_start_program_priority.DisplayMember = "Text";
            cbx_start_program_priority.SelectedItem = "Normal";
            cbx_start_program_priority.Enabled = true;


            UpdateUI();
        }

        public StartProgramControl(StartProgram startProgram, int startProgramOrder)
        {
            InitializeComponent();

            // Update the text with the start program info
            myStartProgram = startProgram;
            myStartProgram.Priority = startProgramOrder;

            // Patch any null start programs (old error that needs repairing)
            // This code will save the file as it should be saved!
            if (myStartProgram.Arguments == null)
            {
                myStartProgram.Arguments = "";
            }

            // Prepare the start program process priority combo box
            cbx_start_program_priority.DataSource = new ComboItem[] {
                    new ComboItem{ Value = ProcessPriority.High, Text = "High" },
                    new ComboItem{ Value = ProcessPriority.AboveNormal, Text = "Above Normal" },
                    new ComboItem{ Value = ProcessPriority.Normal, Text = "Normal" },
                    new ComboItem{ Value = ProcessPriority.BelowNormal, Text = "Below Normal" },
                    new ComboItem{ Value = ProcessPriority.Idle, Text = "Idle" },
                };
            cbx_start_program_priority.ValueMember = "Value";
            cbx_start_program_priority.DisplayMember = "Text";
            cbx_start_program_priority.SelectedItem = "Normal";
            cbx_start_program_priority.Enabled = true;

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
            cbx_start_program_priority.SelectedValue = myStartProgram.ProcessPriority;
            cb_run_as_administrator.Checked = myStartProgram.RunAsAdministrator;
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
                cbx_start_program_priority.Enabled = false;
                cb_run_as_administrator.Enabled = false;
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
                cbx_start_program_priority.Enabled = true;
                cb_run_as_administrator.Enabled = true;
            }            
        }

        private void cb_start_program_pass_args_CheckedChanged(object sender, EventArgs e)
        {
            // Disable the start program 1 fields
            if (cb_start_program_pass_args.Checked)
            {
                myStartProgram.ExecutableArgumentsRequired = true;
                // Enable the Executable Arguments Text field
                txt_start_program_args.Enabled = true;
            }
            else
            {
                myStartProgram.ExecutableArgumentsRequired = false;
                // Disable the Executable Arguments Text field
                txt_start_program_args.Enabled = false;
            }
        }

        private string getExeFile()
        {
            string textToReturn = "";
            System.Windows.Forms.OpenFileDialog dialog_open = new System.Windows.Forms.OpenFileDialog();
            dialog_open.InitialDirectory = Environment.ExpandEnvironmentVariables("%ProgramW6432%"); ;
            dialog_open.Filter = "Executables (*.exe; *.com; *.ps1; *.bat; *.cmd)|*.exe; *.com; *.ps1; *.bat; *.cmd|All files (*.*)|*.*";
            dialog_open.FilterIndex = 1;
            dialog_open.RestoreDirectory = true;
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

        private void cbx_start_program_priority_SelectedIndexChanged(object sender, EventArgs e)
        {
                myStartProgram.ProcessPriority = ProcessUtils.TranslateNameToPriority(cbx_start_program_priority.SelectedValue.ToString());
        }

        private void cb_run_as_administrator_CheckedChanged(object sender, EventArgs e)
        {
            if (cb_run_as_administrator.Checked)
            {
                myStartProgram.RunAsAdministrator = true;
            }
            else
            {
                myStartProgram.RunAsAdministrator = false;
            }
        }
    }    
}
