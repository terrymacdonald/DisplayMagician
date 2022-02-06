using DisplayMagicianShared;
using DisplayMagicianShared.Windows;
//using Microsoft.Win32;
using NHotkey;
using NHotkey.WindowsForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using DisplayMagicianShared.Windows;

namespace DisplayMagician.UIForms
{

    public partial class ProfileToolsForm : Form
    {

        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private Dictionary<TaskBarStuckRectangle.TaskBarEdge, string> forcedTaskBarEdgeText = new Dictionary<TaskBarStuckRectangle.TaskBarEdge, string>();

        public ProfileToolsForm()
        {
            logger.Info($"ProfileToolsForm/ProfileToolsForm: Creating a ProfileToolsForm UI Form");

            InitializeComponent();

            // Set up the default return value of Cancel
            DialogResult = DialogResult.Cancel;

            // Populate the Forced Taskbar Location dictionary
            if (Utils.IsWindows11())
            {
                // Is Windows 11
                // In Windows 11, the taskbars will only work properly up top or bottom. Left or right will show the taskbar, but there will be nothing on the taskbar.
                forcedTaskBarEdgeText.Add(TaskBarStuckRectangle.TaskBarEdge.Top, "Top");
                forcedTaskBarEdgeText.Add(TaskBarStuckRectangle.TaskBarEdge.Bottom, "Bottom");
            }
            else
            {
                // Is Windows 10
                // We can put the taskbar anywhere!
                forcedTaskBarEdgeText.Add(TaskBarStuckRectangle.TaskBarEdge.Left, "Left");
                forcedTaskBarEdgeText.Add(TaskBarStuckRectangle.TaskBarEdge.Top, "Top");
                forcedTaskBarEdgeText.Add(TaskBarStuckRectangle.TaskBarEdge.Right, "Right");
                forcedTaskBarEdgeText.Add(TaskBarStuckRectangle.TaskBarEdge.Bottom, "Bottom");
            }

            cmb_taskbar_edge.DisplayMember = "Value";
            cmb_taskbar_edge.ValueMember = "Text";
            cmb_taskbar_edge.DataSource = new BindingSource(forcedTaskBarEdgeText, null);

        }

        public ProfileItem CurrentProfile
        {
            get;
            set;
        }

        private void ProfileToolsForm_Load(object sender, EventArgs e)
        {            
            cmb_taskbar_edge.SelectedIndex = 0;        
        }

        private void btn_back_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_apply_Click(object sender, EventArgs e)
        {
            ProfileRepository.UpdateActiveProfile();

            if (CurrentProfile.NVIDIADisplayConfig.MosaicConfig.IsMosaicEnabled)
            {
                MessageBox.Show(this, "You cannot change the taskbar position while in a NVIDIA Surround/Mosaic display profile",
                    "Taskbar move failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }
            if (CurrentProfile.AMDDisplayConfig.SlsConfig.IsSlsEnabled)
            {
                MessageBox.Show(this, "You cannot change the taskbar position while in an AMD Eyefinity display profile",
                    "Taskbar move failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }
            // Now set the taskbar position for each screen
            if (CurrentProfile.WindowsDisplayConfig.TaskBarLayout.Count > 0)
            {
                TaskBarStuckRectangle.TaskBarEdge taskbarForcedEdge = ((KeyValuePair<TaskBarStuckRectangle.TaskBarEdge, string>)cmb_taskbar_edge.SelectedItem).Key;
                SharedLogger.logger.Trace($"ProfileToolsForm/btn_apply_Click: Setting the taskbar layout.");

                // Tell Windows to refresh the Other Windows Taskbars if needed
                if (CurrentProfile.WindowsDisplayConfig.TaskBarLayout.Count > 1)
                {
                    SharedLogger.logger.Trace($"ProfileToolsForm/btn_apply_Click: Setting the taskbar layout.");
                    foreach (TaskBarStuckRectangle tbsr in CurrentProfile.WindowsDisplayConfig.TaskBarLayout)
                    {
                        if (tbsr.Version >= 2 && tbsr.Version <= 3)
                        {
                            tbsr.Edge = taskbarForcedEdge;
                            if (tbsr.MainScreen)
                            {
                                tbsr.Location = new Rectangle(0, 1392, 2560, 48);
                                tbsr.Options = (TaskBarStuckRectangle.TaskBarOptions)62586;
                            }


                            // Write the settings to registry
                            tbsr.WriteToRegistry();

                            if (tbsr.MainScreen)
                            {
                                TaskBarStuckRectangle.RepositionMainTaskBar(taskbarForcedEdge);
                            }

                        }
                        else
                        {
                            SharedLogger.logger.Error($"ProfileToolsForm/btn_apply_Click: Unable to set the {tbsr.DevicePath} TaskBarStuckRectangle registry settings as the version isn't v2 or v3!");
                        }
                    }

                }
                else
                {
                    SharedLogger.logger.Trace($"ProfileToolsForm/btn_apply_Click: No taskbar layout in display profile so skipping setting it!");
                }
                TaskBarStuckRectangle.RepositionSecondaryTaskBars();

                // Now set the option to completed.
                DialogResult = DialogResult.OK;
            }
        }
    }
}
