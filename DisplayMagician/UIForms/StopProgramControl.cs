using DisplayMagician.Processes;
using System;
using System.ComponentModel;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace DisplayMagician.UIForms
{
    public partial class StopProgramControl : UserControl, IProgramControl
    {
        private StopProgram myStopProgram = new StopProgram();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int ProgramNumber
        {
            get => myStopProgram.Priority;
            set => ChangePriority(value);
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StopProgram StopProgram
        {
            get { return myStopProgram; }
            set
            {
                myStopProgram = value;
                UpdateUI();
            }
        }

        public StopProgramControl()
        {
            InitializeComponent();
            SetupPriorityCombo();
        }

        public StopProgramControl(StopProgram stopProgram)
        {
            InitializeComponent();
            myStopProgram = stopProgram;
            SetupPriorityCombo();
            UpdateUI();
        }

        public StopProgramControl(StopProgram stopProgram, int order)
        {
            InitializeComponent();
            myStopProgram = stopProgram;
            myStopProgram.Priority = order;
            SetupPriorityCombo();
            UpdateUI();
        }

        private void SetupPriorityCombo()
        {
            cbx_restart_priority.DataSource = new ComboItem[]
            {
                new ComboItem { Value = ProcessPriority.High,        Text = "High" },
                new ComboItem { Value = ProcessPriority.AboveNormal, Text = "Above Normal" },
                new ComboItem { Value = ProcessPriority.Normal,      Text = "Normal" },
                new ComboItem { Value = ProcessPriority.BelowNormal, Text = "Below Normal" },
                new ComboItem { Value = ProcessPriority.Idle,        Text = "Idle" },
            };
            cbx_restart_priority.ValueMember = "Value";
            cbx_restart_priority.DisplayMember = "Text";
            cbx_restart_priority.SelectedValue = ProcessPriority.Normal;
        }

        public void UpdateUI()
        {
            lbl_priority.Text = myStopProgram.Priority.ToString();
            txt_stop_program.Text = myStopProgram.Executable;
            chk_disabled.Checked = myStopProgram.Disabled;
            chk_restart_afterwards.Checked = myStopProgram.RestartAfterwards;
            cbx_restart_priority.SelectedValue = myStopProgram.RestartProcessPriority;
            chk_run_as_admin.Checked = myStopProgram.RunAsAdministrator;
            UpdateRestartDependentControls();
        }

        public void ChangePriority(int priority)
        {
            myStopProgram.Priority = priority;
            lbl_priority.Text = priority.ToString();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (var pen = new System.Drawing.Pen(System.Drawing.Color.Red, 4))
            {
                e.Graphics.DrawRectangle(pen, 2, 2, Width - 5, Height - 5);
            }
        }

        private void UpdateRestartDependentControls()
        {
            bool restartEnabled = chk_restart_afterwards.Checked;
            cbx_restart_priority.Enabled = restartEnabled;
            chk_run_as_admin.Enabled = restartEnabled;
        }

        private void btn_stop_program_Click(object sender, EventArgs e)
        {
            string file = GetExeFile();
            if (!string.IsNullOrEmpty(file))
                txt_stop_program.Text = file;
        }

        private void txt_stop_program_TextChanged(object sender, EventArgs e)
        {
            myStopProgram.Executable = txt_stop_program.Text;
        }

        private void chk_disabled_CheckedChanged(object sender, EventArgs e)
        {
            myStopProgram.Disabled = chk_disabled.Checked;
            bool enabled = !chk_disabled.Checked;
            txt_stop_program.Enabled = enabled;
            btn_stop_program.Enabled = enabled;
            chk_restart_afterwards.Enabled = enabled;
            if (enabled)
            {
                UpdateRestartDependentControls();
            }
            else
            {
                cbx_restart_priority.Enabled = false;
                chk_run_as_admin.Enabled = false;
            }
        }

        private void chk_restart_afterwards_CheckedChanged(object sender, EventArgs e)
        {
            myStopProgram.RestartAfterwards = chk_restart_afterwards.Checked;
            UpdateRestartDependentControls();
        }

        private void cbx_restart_priority_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cbx_restart_priority.SelectedItem is ComboItem item)
                myStopProgram.RestartProcessPriority = (ProcessPriority)item.Value;
        }

        private void chk_run_as_admin_CheckedChanged(object sender, EventArgs e)
        {
            myStopProgram.RunAsAdministrator = chk_run_as_admin.Checked;
        }

        private void btn_delete_Click(object sender, EventArgs e)
        {
            ((ShortcutForm)this.Parent.Parent.Parent.Parent).RemoveStopProgram(this);
        }

        private void pb_up_arrow_Click(object sender, EventArgs e)
        {
            ((ShortcutForm)this.Parent.Parent.Parent.Parent).ProgramEarlier(this);
        }

        private void pb_down_arrow_Click(object sender, EventArgs e)
        {
            ((ShortcutForm)this.Parent.Parent.Parent.Parent).ProgramLater(this);
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
            pb_down_arrow.Image = DisplayMagician.Properties.Resources.redarrowsdown;
        }

        private void pb_down_arrow_MouseLeave(object sender, EventArgs e)
        {
            pb_down_arrow.Image = DisplayMagician.Properties.Resources.whitearrows;
        }

        private string GetExeFile()
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.InitialDirectory = Environment.ExpandEnvironmentVariables("%ProgramW6432%");
                dialog.Filter = "Executables (*.exe; *.com; *.ps1; *.bat; *.cmd)|*.exe; *.com; *.ps1; *.bat; *.cmd|All files (*.*)|*.*";
                dialog.FilterIndex = 1;
                dialog.RestoreDirectory = true;
                if (dialog.ShowDialog(this) == DialogResult.OK && File.Exists(dialog.FileName))
                    return dialog.FileName;
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return string.Empty;
                MessageBox.Show("Selected file is not a valid file.", "Executable", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return string.Empty;
            }
        }

        private void checkbox_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);

            CheckBox checkbox = sender as CheckBox;

            if (!checkbox.Enabled)
            {
                int x = ClientRectangle.X + CheckBoxRenderer.GetGlyphSize(
                    e.Graphics, CheckBoxState.UncheckedNormal).Width;
                int y = ClientRectangle.Y + 1;

                TextRenderer.DrawText(e.Graphics, checkbox.Text,
                    checkbox.Font, new Point(x, y), Color.Gray,
                    TextFormatFlags.LeftAndRightPadding);
            }
        }
    }
}
