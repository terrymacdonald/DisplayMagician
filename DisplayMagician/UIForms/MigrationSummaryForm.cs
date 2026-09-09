using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace DisplayMagician.UIForms
{
    public partial class MigrationSummaryForm : DisplayMagicianForm
    {
        #region Class Variables

        private string _summaryText = string.Empty;

        #endregion

        #region Properties

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string SummaryText
        {
            get => _summaryText;
            set => _summaryText = value ?? string.Empty;
        }

        #endregion

        #region Constructors

        public MigrationSummaryForm()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private void MigrationSummaryForm_Load(object sender, EventArgs e)
        {
            rtb_summary.Text = SummaryText;
        }

        #endregion
    }
}
