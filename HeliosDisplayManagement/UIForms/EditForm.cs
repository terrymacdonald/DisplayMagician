using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsDisplayAPI.Native.DisplayConfig;
using HeliosDisplayManagement.Resources;
using HeliosDisplayManagement.Shared;
using HeliosDisplayManagement.Shared.Topology;

namespace HeliosDisplayManagement.UIForms
{
    public partial class EditForm : Form
    {
        public EditForm()
        {
            InitializeComponent();
        }

        public EditForm(Profile profile) : this()
        {
            Profile = profile;
            Text = string.Format(Text, profile.Name);
            txt_name.Text = profile.Name;
            dv_profile.Profile = profile;
            RefreshMonitors();
        }

        public Profile Profile { get; set; }

        private DisplayRepresentation SelectedDisplay { get; set; }

        public sealed override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        private void btn_apply_Click(object sender, EventArgs e)
        {
            if (SaveProfile())
            {
                if (!dv_profile.Profile.IsPossible)
                {
                    MessageBox.Show(this, Language.This_profile_is_currently_impossible_to_apply,
                        Language.Apply_Profile,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);

                    return;
                }

                var currentProfile = Profile.GetCurrent(string.Empty);
                Enabled = false;
                var failed = false;

                if (new SplashForm(() =>
                    {
                        Task.Factory.StartNew(() =>
                        {
                            if (!dv_profile.Profile.Apply())
                            {
                                failed = true;
                            }
                        }, TaskCreationOptions.LongRunning);
                    }, 3, 30).ShowDialog(this) !=
                    DialogResult.Cancel)
                {
                    if (failed)
                    {
                        MessageBox.Show(this, Language.Profile_is_invalid_or_not_possible_to_apply, Language.Error,
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        new SplashForm(
                            () =>
                            {
                                Task.Factory.StartNew(() => currentProfile.Apply(), TaskCreationOptions.LongRunning);
                            }, 60, 30) {CancellationMessage = Language.Reverting_in}.ShowDialog(this);
                    }
                }

                Enabled = true;
                Activate();
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            if (SaveProfile())
            {
                DialogResult = DialogResult.OK;
            }
        }

        private void cb_clone_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cb_clone.SelectedItem != null && SelectedDisplay != null)
            {
                Path newSource;

                if (cb_clone.SelectedItem is string)
                {
                    newSource = new Path();
                    Profile.Paths = Profile.Paths.Concat(new[] {newSource}).ToArray();
                }
                else
                {
                    newSource = cb_clone.SelectedItem as Path;
                }

                if (newSource != null)
                {
                    var target = SelectedDisplay.GetPathTarget(Profile);
                    var source = SelectedDisplay.GetPathSource(Profile);
                    source.Targets = source.Targets.Where(pathTarget => pathTarget != target).ToArray();
                    newSource.Targets = newSource.Targets.Concat(new[] {target}).ToArray();

                    if (source.Targets.Length == 0)
                    {
                        Profile.Paths = Profile.Paths.Where(path => path != source).ToArray();
                    }
                }

                cb_resolution_SelectionChangeCommitted(null, null);
                cb_colordepth_SelectionChangeCommitted(null, null);
                cb_frequency_SelectionChangeCommitted(null, null);
                cb_rotation_SelectionChangeCommitted(null, null);
            }

            RefreshArrangementSettings();
        }

        private void cb_colordepth_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cb_colordepth.SelectedItem != null && SelectedDisplay != null)
            {
                SelectedDisplay.GetPathSource(Profile).PixelFormat =
                    ColorDepthToPixelFormat((ColorDepth) cb_colordepth.SelectedItem);
            }

            RefreshArrangementSettings();
        }

        private void cb_frequency_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cb_frequency.SelectedItem != null && SelectedDisplay != null)
            {
                SelectedDisplay.GetPathTarget(Profile).FrequencyInMillihertz = (uint) cb_frequency.SelectedItem * 1000;
            }

            RefreshArrangementSettings();
        }

        private void cb_resolution_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cb_resolution.SelectedItem != null && SelectedDisplay != null)
            {
                SelectedDisplay.GetPathSource(Profile).Resolution = (Size) cb_resolution.SelectedItem;
            }

            RefreshArrangementSettings();
        }

        private void cb_rotation_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cb_rotation.SelectedItem != null && SelectedDisplay != null)
            {
                switch (cb_rotation.SelectedIndex)
                {
                    case 1:
                        SelectedDisplay.GetPathTarget(Profile).Rotation = Rotation.Rotate90;

                        break;
                    case 2:
                        SelectedDisplay.GetPathTarget(Profile).Rotation = Rotation.Rotate180;

                        break;
                    case 3:
                        SelectedDisplay.GetPathTarget(Profile).Rotation = Rotation.Rotate270;

                        break;
                    default:
                        SelectedDisplay.GetPathTarget(Profile).Rotation = Rotation.Identity;

                        break;
                }
            }

            RefreshArrangementSettings();
        }

        private void cb_surround_applybezel_CheckedChanged(object sender, EventArgs e)
        {
        }

        private DisplayConfigPixelFormat ColorDepthToPixelFormat(ColorDepth depth)
        {
            switch (depth)
            {
                case ColorDepth.Depth8Bit:

                    return DisplayConfigPixelFormat.PixelFormat8Bpp;
                case ColorDepth.Depth16Bit:

                    return DisplayConfigPixelFormat.PixelFormat16Bpp;
                case ColorDepth.Depth24Bit:

                    return DisplayConfigPixelFormat.PixelFormat24Bpp;
                case ColorDepth.Depth32Bit:

                    return DisplayConfigPixelFormat.PixelFormat32Bpp;
                default:

                    return DisplayConfigPixelFormat.NotSpecified;
            }
        }

        private void EditForm_Load(object sender, EventArgs e)
        {
        }

        private void lv_monitors_SelectionChangeCommitted(object sender, EventArgs e)
        {
            SelectedDisplay = null;

            if (lv_monitors.SelectedItems.Count > 0)
            {
                SelectedDisplay = lv_monitors.SelectedItems[0].Tag as DisplayRepresentation;
            }

            RefreshArrangementSettings();
        }

        private void nud_x_ValueChanged(object sender, EventArgs e)
        {
        }

        private void nud_y_ValueChanged(object sender, EventArgs e)
        {
        }


        private WindowsDisplayAPI.ColorDepth PixelFormatToColorDepth(DisplayConfigPixelFormat format)
        {
            switch (format)
            {
                case DisplayConfigPixelFormat.PixelFormat8Bpp:

                    return WindowsDisplayAPI.ColorDepth.Depth8Bit;
                case DisplayConfigPixelFormat.PixelFormat16Bpp:

                    return WindowsDisplayAPI.ColorDepth.Depth16Bit;
                case DisplayConfigPixelFormat.PixelFormat24Bpp:

                    return WindowsDisplayAPI.ColorDepth.Depth24Bit;
                case DisplayConfigPixelFormat.PixelFormat32Bpp:

                    return WindowsDisplayAPI.ColorDepth.Depth32Bit;
                default:

                    return WindowsDisplayAPI.ColorDepth.Depth4Bit;
            }
        }

        // ReSharper disable once CyclomaticComplexity
        // ReSharper disable once FunctionComplexityOverflow
        private void RefreshArrangementSettings()
        {
            gb_arrangement.Enabled = false;
            gb_surround.Enabled = false;
            cb_resolution.Items.Clear();
            cb_colordepth.Items.Clear();
            cb_frequency.Items.Clear();
            cb_rotation.Items.Clear();
            cb_clone.Items.Clear();
            dv_profile.Invalidate();
            nud_x.Value = 0;
            nud_y.Value = 0;

            try
            {
                if (SelectedDisplay != null)
                {
                    var pathSource = SelectedDisplay.GetPathSource(Profile);
                    var pathTarget = SelectedDisplay.GetPathTarget(Profile);

                    if (SelectedDisplay.IsAvailable)
                    {
                        //gb_arrangement.Enabled = true;
                        var possibleSettings = SelectedDisplay.PossibleSettings;

                        foreach (var resolution in possibleSettings.Select(setting => setting.Resolution).Distinct())
                        {
                            cb_resolution.Items.Add(resolution);

                            if ((Size) cb_resolution.Items[cb_resolution.Items.Count - 1] == pathSource.Resolution)
                            {
                                cb_resolution.SelectedIndex = cb_resolution.Items.Count - 1;
                            }
                        }

                        foreach (
                            var colorDepth in
                            possibleSettings.Where(setting => setting.Resolution == pathSource.Resolution)
                                .Select(setting => setting.ColorDepth)
                                .Distinct())
                        {
                            cb_colordepth.Items.Add(colorDepth);

                            if ((WindowsDisplayAPI.ColorDepth) cb_colordepth.Items[cb_colordepth.Items.Count - 1] ==
                                PixelFormatToColorDepth(pathSource.PixelFormat))
                            {
                                cb_colordepth.SelectedIndex = cb_colordepth.Items.Count - 1;
                            }
                        }

                        foreach (
                            var frequency in
                            possibleSettings.Where(
                                    setting =>
                                        setting.Resolution == pathSource.Resolution &&
                                        setting.ColorDepth == PixelFormatToColorDepth(pathSource.PixelFormat))
                                .Select(setting => setting.Frequency)
                                .Distinct())
                        {
                            cb_frequency.Items.Add(frequency);

                            if ((int) cb_frequency.Items[cb_frequency.Items.Count - 1] ==
                                (int) (pathTarget.FrequencyInMillihertz / 1000))
                            {
                                cb_frequency.SelectedIndex = cb_frequency.Items.Count - 1;
                            }
                        }
                    }
                    else
                    {
                        cb_resolution.Items.Add(pathSource.Resolution);
                        cb_resolution.SelectedIndex = 0;
                        cb_colordepth.Items.Add(PixelFormatToColorDepth(pathSource.PixelFormat));
                        cb_colordepth.SelectedIndex = 0;
                        cb_frequency.Items.Add((int) (pathTarget.FrequencyInMillihertz / 1000));
                        cb_frequency.SelectedIndex = 0;
                    }

                    nud_x.Value = pathSource.Position.X;
                    nud_y.Value = pathSource.Position.Y;
                    cb_clone.Items.Clear();
                    cb_clone.Items.Add("None");
                    cb_clone.SelectedIndex = 0;

                    foreach (
                        var potentialClone in
                        Profile.Paths.Where(
                            path =>
                                path.Resolution == pathSource.Resolution &&
                                path.Targets.First().DevicePath != SelectedDisplay.Path))
                    {
                        cb_clone.Items.Add(potentialClone);

                        if (potentialClone.Targets.Contains(pathTarget))
                        {
                            cb_clone.SelectedIndex = cb_clone.Items.Count - 1;
                        }
                    }

                    cb_rotation.Items.Clear();
                    cb_rotation.Items.Add("Identity");
                    cb_rotation.Items.Add("90 degree");
                    cb_rotation.Items.Add("180 degree");
                    cb_rotation.Items.Add("270 degree");

                    switch (pathTarget.Rotation)
                    {
                        case Rotation.Identity:
                            cb_rotation.SelectedIndex = 0;

                            break;
                        case Rotation.Rotate90:
                            cb_rotation.SelectedIndex = 1;

                            break;
                        case Rotation.Rotate180:
                            cb_rotation.SelectedIndex = 2;

                            break;
                        case Rotation.Rotate270:
                            cb_rotation.SelectedIndex = 3;

                            break;
                        default:
                            cb_rotation.SelectedIndex = 0;

                            break;
                    }

                    if (pathTarget.SurroundTopology != null)
                    {
                        cb_surround_applybezel.Checked = pathTarget.SurroundTopology.ApplyWithBezelCorrectedResolution;
                    }
                    else
                    {
                        cb_surround_applybezel.Checked = false;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void RefreshMonitors()
        {
            imageList1.Images.Clear();
            lv_monitors.Items.Clear();

            foreach (var display in DisplayRepresentation.GetDisplays(Profile))
            {
                imageList1.Images.Add(display.ToBitmap(imageList1.ImageSize, Profile));
                lv_monitors.Items.Add(new ListViewItem
                {
                    Text = display.Name,
                    Tag = display,
                    ImageIndex = imageList1.Images.Count - 1
                });
            }
        }

        private bool SaveProfile()
        {
            if (string.IsNullOrWhiteSpace(txt_name.Text) ||
                Profile.GetAllProfiles()
                    .Any(
                        profile =>
                            !profile.Equals(Profile) &&
                            profile.Name.Equals(txt_name.Text.Trim(), StringComparison.InvariantCultureIgnoreCase)))
            {
                MessageBox.Show(this, Language.Invalid_or_duplicate_profile_name, Language.Validation,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txt_name.Focus();

                return false;
            }

            Profile.Name = txt_name.Text.Trim();

            return true;
        }
    }
}