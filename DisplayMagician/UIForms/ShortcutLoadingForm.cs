using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class ShortcutLoadingForm : DisplayMagicianForm
    {
        private string _title = "DisplayMagician is loading...";
        private int _closeRequested;
        private string _description = "If you have installed a lot of games over time or have a lot of games installed now, this may take a while!";

        public ShortcutLoadingForm()
        {
            InitializeComponent();
            this.TopMost = false;
        }


        internal void RequestClose()
        {
            Interlocked.Exchange(ref _closeRequested, 1);
            if (IsDisposed || !IsHandleCreated) return;
            try { BeginInvoke((Action)(() => { if (!IsDisposed) Close(); })); }
            catch (InvalidOperationException) { /* The splash window closed before the callback was queued. */ }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (Volatile.Read(ref _closeRequested) != 0) Close();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
                lbl_title.Text = _title;
                this.Text = _title;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
                lbl_description.Text = _description;
            }
        }


        private void LoadingForm_Load(object sender, EventArgs e)
        {

        }

    }
}
