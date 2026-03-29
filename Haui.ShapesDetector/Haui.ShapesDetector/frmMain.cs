using Haui.ShapesDetector.Models;
using Haui.ShapesDetector.Services;
using System.Drawing.Imaging;

namespace Haui.ShapesDetector
{
    public partial class frmMain : Form
    {
        private readonly IDetectionService _detectionService;
        private readonly CameraService _cameraService;
        private bool _isProcessing = false;
        private readonly List<DetectionResult> _allDetections = new();

        public frmMain()
        {
            InitializeComponent();

            _detectionService = new YoloV11DetectionService();
            _cameraService = new CameraService();

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _cameraService.FrameCaptured += OnFrameCaptured;
            _cameraService.ErrorOccurred += OnCameraError;

            // Setup DataGridView
            dgvResults.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dgvResults.DefaultCellStyle.ForeColor = Color.White;
            dgvResults.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 122, 204);
            dgvResults.DefaultCellStyle.SelectionForeColor = Color.White;
            dgvResults.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            dgvResults.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvResults.EnableHeadersVisualStyles = false;
        }

        private async void frmMain_Load(object sender, EventArgs e)
        {
            UpdateStatus("Initializing YOLO model...", Color.Orange);

            try
            {
                var modelPath = Path.Combine(Application.StartupPath, "shapes_best.onnx");
                var classesPath = Path.Combine(Application.StartupPath, "best-classes.txt");

                if (!File.Exists(modelPath))
                {
                    MessageBox.Show($"Model file not found: {modelPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Model not found", Color.Red);
                    return;
                }

                if (!File.Exists(classesPath))
                {
                    MessageBox.Show($"Classes file not found: {classesPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    UpdateStatus("Classes file not found", Color.Red);
                    return;
                }

                await _detectionService.InitializeAsync(modelPath, classesPath);
                UpdateStatus("Ready", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Initialization failed", Color.Red);
            }
        }

        private async void OnFrameCaptured(object? sender, Bitmap bitmap)
        {
            if (_isProcessing || !_detectionService.IsInitialized)
                return;

            _isProcessing = true;

            try
            {
                var detections = await DetectObjectsAsync(bitmap);

                if (InvokeRequired)
                {
                    BeginInvoke(() =>
                    {
                        detectionPanel.UpdateFrame((Bitmap)bitmap.Clone(), detections);
                        UpdateResultsGrid(detections);
                    });
                }
            }
            catch (Exception ex)
            {
                BeginInvoke(() => UpdateStatus($"Detection error: {ex.Message}", Color.Red));
            }
            finally
            {
                _isProcessing = false;
            }
        }

        private async Task<List<DetectionResult>> DetectObjectsAsync(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Jpeg);
            ms.Position = 0;

            return await _detectionService.DetectAsync(ms.ToArray(), bitmap.Width, bitmap.Height);
        }

        private void UpdateResultsGrid(List<DetectionResult> detections)
        {
            // Keep only last 50 detections
            _allDetections.AddRange(detections);
            if (_allDetections.Count > 50)
            {
                _allDetections.RemoveRange(0, _allDetections.Count - 50);
            }

            dgvResults.Rows.Clear();

            foreach (var detection in _allDetections.OrderByDescending(d => d.DetectedAt))
            {
                dgvResults.Rows.Add(
                    detection.ClassName,
                    $"{detection.Confidence:P0}",
                    detection.DetectedAt.ToString("HH:mm:ss")
                );
            }
        }

        private void OnCameraError(object? sender, string error)
        {
            BeginInvoke(() => UpdateStatus(error, Color.Red));
        }

        private void UpdateStatus(string message, Color color)
        {
            lblStatus.Text = $"● {message}";
            lblStatus.ForeColor = color;
        }

        private void btnStartCamera_Click(object sender, EventArgs e)
        {
            if (!_detectionService.IsInitialized)
            {
                MessageBox.Show("YOLO model not initialized", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _cameraService.Start();
            btnStartCamera.Enabled = false;
            btnStop.Enabled = true;
            UpdateStatus("Camera running", Color.LimeGreen);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _cameraService.Stop();
            btnStartCamera.Enabled = true;
            btnStop.Enabled = false;
            UpdateStatus("Camera stopped", Color.Orange);
        }

        private async void btnCapture_Click(object sender, EventArgs e)
        {
            if (!_detectionService.IsInitialized)
            {
                MessageBox.Show("YOLO model not initialized", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var snapshot = _cameraService.CaptureSnapshot();
            if (snapshot != null)
            {
                var detections = await DetectObjectsAsync(snapshot);
                detectionPanel.UpdateFrame(snapshot, detections);
                UpdateResultsGrid(detections);
                UpdateStatus($"Captured - {detections.Count} objects detected", Color.LimeGreen);
            }
        }

        private void btnSaveResults_Click(object sender, EventArgs e)
        {
            if (_allDetections.Count == 0)
            {
                MessageBox.Show("No detection results to save", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv|Text files (*.txt)|*.txt",
                DefaultExt = "csv",
                FileName = $"detection_results_{DateTime.Now:yyyyMMdd_HHmmss}"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var lines = new List<string> { "ClassName,Confidence,DetectedAt" };
                    lines.AddRange(_allDetections.Select(d =>
                        $"{d.ClassName},{d.Confidence:F2},{d.DetectedAt:yyyy-MM-dd HH:mm:ss}"));

                    File.WriteAllLines(sfd.FileName, lines);
                    MessageBox.Show("Results saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to save: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cameraService.Dispose();
            (_detectionService as IDisposable)?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
