using Haui.ShapesDetector.Services;

namespace Haui.ShapesDetector
{
    public partial class frmSettings : Form
    {
        private readonly DetectionPipeline _pipeline;

        public frmSettings(DetectionPipeline pipeline)
        {
            _pipeline = pipeline;
            InitializeComponent();
        }

        private void frmSettings_Load(object sender, EventArgs e)
        {
            int current = (int)Math.Round(_pipeline.ConfidenceThreshold * 100);
            trConfidence.Value = Math.Clamp(current, trConfidence.Minimum, trConfidence.Maximum);
            UpdateConfidenceLabel();
        }

        private void trConfidence_ValueChanged(object sender, EventArgs e)
        {
            UpdateConfidenceLabel();
        }

        private void UpdateConfidenceLabel()
        {
            lblConfidenceValue.Text = $"{trConfidence.Value}%";

            lblConfidenceValue.ForeColor = trConfidence.Value switch
            {
                >= 70 => Color.LimeGreen,
                >= 50 => Color.Orange,
                _     => Color.OrangeRed
            };
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            _pipeline.SetConfidenceThreshold(trConfidence.Value / 100f);
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
