using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using HeliosPlus.Resources;
using WinFormAnimation;

namespace HeliosPlus.UIForms
{
    public sealed partial class ApplyingChangesForm : Form
    {
        private readonly Action _job;
        private readonly Bitmap _progressImage;
        private readonly List<Point> _progressPositions = new List<Point>();
        private int _countdownCounter;
        private readonly int _displayChangeMaxDelta = -1;
        private int _displayChangeDelta;
        private int _lastCount;
        private bool _isClosing;
        private int _startCounter;

        public ApplyingChangesForm()
        {
            InitializeComponent();
            _progressImage = new Bitmap(progressPanel.Width, progressPanel.Height);
            Controls.Remove(progressPanel);
            progressPanel.BackColor = BackColor;
            progressBar.Invalidated += (sender, args) => Invalidate();
            progressPanel.Invalidated += (sender, args) => Invalidate();
            Reposition();
        }

        public ApplyingChangesForm(Action job, int cancellationTimeout = 0, int countdown = 0) : this()
        {
            _job = job;
            _startCounter = cancellationTimeout;
            _countdownCounter = countdown; _lastCount = _countdownCounter;
        }

        public ApplyingChangesForm(Action job, int cancellationTimeout = 0, int countdown = 0, int displayChangeMaxDelta = 5, string state = null) : this(job, cancellationTimeout, countdown)
        {
            _displayChangeMaxDelta = displayChangeMaxDelta;
            if (!string.IsNullOrEmpty(state)) CountdownMessage = state;
        }

        public string CancellationMessage { get; set; } = Language.Starting_in;
        public string CancellationSubMessage { get; set; } = Language.Press_ESC_to_cancel;

        public string CountdownMessage { get; set; } = Language.Please_wait;
        public string CountdownSubMessage { get; set; } = Language.It_wont_be_long_now;


        protected override void OnKeyDown(KeyEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            lock (_progressPositions)
            {
                progressPanel.DrawToBitmap(_progressImage, new Rectangle(Point.Empty, progressPanel.Size));

                foreach (var position in _progressPositions)
                {
                    e.Graphics.DrawImage(_progressImage, new Rectangle(position, progressPanel.Size));
                }
            }

            base.OnPaint(e);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData != Keys.Escape)
            {
                return base.ProcessCmdKey(ref msg, keyData);
            }

            if (t_start.Enabled)
            {
                t_start.Stop();
                t_countdown.Stop();
                DialogResult = DialogResult.Cancel;
                Close();

                return true;
            }

            return true;
        }

        private void HandleDisplayChangeDelta()
        {
            if (_displayChangeMaxDelta <= -1) return;
            _displayChangeDelta = _lastCount - _countdownCounter;
            if (_displayChangeDelta > _displayChangeMaxDelta)
            {
                Debug.Print("_displayChangeDelta > _displayChangeMaxDelta! " + _displayChangeDelta + " > " + _displayChangeMaxDelta);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void DoJob()
        {
            lbl_message.Text = CountdownMessage;
            lbl_sub_message.Text = CountdownSubMessage;
            progressBar.ProgressColor = Color.OrangeRed;

            if (_countdownCounter > 0)
            {
                progressBar.Text = (progressBar.Value = progressBar.Maximum = _countdownCounter).ToString();
                t_countdown.Start();
                _job?.Invoke();
            }
            else
            {
                progressBar.Style = ProgressBarStyle.Marquee;
                progressBar.Text = "";
                progressBar.Maximum = 100;
                progressBar.Value = 50;
                progressBar.Style = ProgressBarStyle.Marquee;
                _job?.Invoke();
                DialogResult = DialogResult.OK;
                Close();
            }

            HandleDisplayChangeDelta();
        }

        private void DoTimeout()
        {
            lbl_message.Text = CancellationMessage;
            lbl_sub_message.Text = CancellationSubMessage;
            progressBar.ProgressColor = Color.DodgerBlue;

            if (_startCounter > 0)
            {
                progressBar.Text = (progressBar.Value = progressBar.Maximum = _startCounter).ToString();
                t_start.Start();
            }
            else
            {
                DoJob();
            }
            HandleDisplayChangeDelta();
        }

        private void Reposition()
        {
            lock (_progressPositions)
            {

                Screen[] screens = Screen.AllScreens;
                Size = screens.Select(screen => screen.Bounds)
                                            .Aggregate(Rectangle.Union)
                                            .Size;
                
                _progressPositions.Clear();
                _progressPositions.AddRange(
                    screens.Select(
                        screen =>
                            new Point(screen.Bounds.X + ((screen.Bounds.Width - progressPanel.Width) / 2),
                                screen.Bounds.Y + ((screen.Bounds.Height - progressPanel.Height) / 2))
                    )
                );
            }
            Invalidate();
        }

        private void SplashForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_isClosing)
            {
                return;
            }

            _isClosing = true;
            e.Cancel = true;
            var dialogResult = DialogResult;
            new Animator(new Path((float) Opacity, 0, 200))
                .Play(new SafeInvoker<float>(f =>
                {
                    try
                    {
                        Opacity = f;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ApplyingChangesForm/SplashForm_FormClosing exception: {ex.Message}: {ex.InnerException}");
                        // ignored
                    }
                }, this), new SafeInvoker(() =>
                {
                    DialogResult = dialogResult;
                    Close();
                }, this));
        }

        private void SplashForm_Reposition(object sender, EventArgs e)
        {
            Reposition();
        }

        private void SplashForm_Shown(object sender, EventArgs e)
        {
            new Animator(new Path((float) Opacity, 0.97f, 200))
                .Play(new SafeInvoker<float>(f =>
                {
                    try
                    {
                        Opacity = f;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"ApplyingChangesForm/SplashForm_Shown exception: {ex.Message}: {ex.InnerException}");
                        // ignored
                    }
                }, this), new SafeInvoker(DoTimeout, this));
        }

        private void t_countdown_Tick(object sender, EventArgs e)
        {
            if (_countdownCounter < 0)
            {
                t_countdown.Stop();
                DialogResult = DialogResult.OK;
                Close();

                return;
            }

            HandleDisplayChangeDelta();

            progressBar.Value = _countdownCounter;
            progressBar.Text = progressBar.Value.ToString();
            _countdownCounter--;
            Reposition();
        }

        private void t_start_Tick(object sender, EventArgs e)
        {
            if (_startCounter < 0)
            {
                t_start.Stop();
                DoJob();

                return;
            }

            progressBar.Value = _startCounter;
            progressBar.Text = progressBar.Value.ToString();
            _startCounter--;
            Reposition();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_SETTINGCHANGE = 0x001A;
            const int SPI_SETWORKAREA = 0x02F;
            const int WM_DISPLAYCHANGE = 0x007E;

            const int x_bitshift = 0;
            const int y_bitshift = 16;
            const int xy_mask = 0xFFFF;

            bool displayChange = false;

            switch (m.Msg)
            {
                case WM_SETTINGCHANGE:
                    Debug.Print("Message: " + m.ToString());
                    Debug.Print("WM_SETTINGCHANGE");
                    switch ((int)m.WParam)
                    {
                        case SPI_SETWORKAREA:
                            Debug.Print("SPI_SETWORKAREA");
                            displayChange = true;
                            break;
                    }
                    break;
                case WM_DISPLAYCHANGE:
                    int cxScreen = (xy_mask & ((int)m.LParam) >> x_bitshift);
                    int cyScreen = (xy_mask & ((int)m.LParam) >> y_bitshift);
                    Debug.Print("Message: " + m.ToString());
                    Debug.Print("WM_DISPLAYCHANGE");
                    Debug.Print("cxScreen: " + cxScreen + " cyScreen: " + cyScreen);
                    displayChange = true;
                    break;
            }
            if (displayChange)
            {
                _displayChangeDelta = _lastCount - _countdownCounter;
                _lastCount = _countdownCounter;
                Debug.Print("Display Change Detected at t " + _lastCount + " difference between changes is " + _displayChangeDelta);
            }

            base.WndProc(ref m);
        }

    }
}