using Haui.ShapesDetector.Common;
using Haui.ShapesDetector.Models;
using Haui.ShapesDetector.Services;
using System.IO.Ports;
using System.Text;

namespace Haui.ShapesDetector
{
    public partial class frmMain : Form
    {
        private readonly DetectionPipeline _pipeline;
        private readonly RobotService _robotService;
        private SerialPort Robot = new SerialPort();

        private readonly StringBuilder _serialBuffer = new StringBuilder();

        private readonly List<DetectionResult> _allDetections = new();
        private bool RobotArm_isReady = false;
        private bool RobotArm_doneS1 = false;
        private bool RobotArm_doneS2 = false;
        private bool CameraWait = false;
        private bool RobotArmWait = false;
        private string _material = string.Empty;
        private string _oldMaterial = string.Empty;

        public frmMain()
        {
            InitializeComponent();

            _pipeline = new DetectionPipeline(new YoloV11DetectionService(), new CameraService());
            _robotService = new RobotService();

            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            _pipeline.FrameReady += OnFrameReady;
            _pipeline.DetectionCompleted += OnDetectionCompleted;
            _pipeline.ErrorOccurred += (_, msg) =>
            {
                if (IsHandleCreated) BeginInvoke(() => UpdateStatus(msg, Color.Red));
            };

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
                var classesPath = Path.Combine(Application.StartupPath, "shapes_classes.txt");

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

                await _pipeline.InitializeAsync(modelPath, classesPath);

                if (!Robot.IsOpen)
                {
                    Robot.PortName = clsFileIO.ReadValue("COM_ROBOT");
                    Robot.BaudRate = int.Parse(clsFileIO.ReadValue("BAURATE_ROBOT"));
                    Robot.Open();
                    Robot.DataReceived += Robot_DataReceived;
                }

                // Load danh sách cameras
                LoadAvailableCameras();

                UpdateStatus("Ready", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Initialization failed", Color.Red);
            }
        }

        private void LoadAvailableCameras()
        {
            cmbCameras.Items.Clear();

            var cameras = DetectionPipeline.GetAvailableCameras();

            if (cameras.Count == 0)
            {
                cmbCameras.Items.Add("No cameras found");
                cmbCameras.Enabled = false;
                return;
            }

            foreach (var camera in cameras)
            {
                cmbCameras.Items.Add(camera);
            }

            cmbCameras.SelectedIndex = 0;
            cmbCameras.Enabled = true;
        }

        private void cmbCameras_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbCameras.SelectedItem is CameraInfo camera)
            {
                _pipeline.SwitchCamera(camera.Index);
                UpdateStatus($"Switched to {camera.Name}", Color.LimeGreen);
            }
        }

        private void Robot_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                // Defensive checks
                if (Robot == null || !Robot.IsOpen) return;

                int bytes = Robot.BytesToRead;
                if (bytes == 0) return;

                // Guard against huge bursts
                if (bytes > 500)
                {
                    Robot.DiscardInBuffer();
                    return;
                }

                // Non-blocking read of whatever is available right now
                var chunk = Robot.ReadExisting();
                if (string.IsNullOrEmpty(chunk)) return;

                // Accumulate fragment(s) into buffer
                lock (_serialBuffer)
                {
                    _serialBuffer.Append(chunk);

                    // Messages terminated by 'x' (as used previously with ReadTo("x"))
                    string bufferContent = _serialBuffer.ToString();
                    int delimIndex;
                    while ((delimIndex = bufferContent.IndexOf('x')) >= 0)
                    {
                        string message = bufferContent.Substring(0, delimIndex).Trim();
                        if (!string.IsNullOrEmpty(message))
                        {
                            // Marshal to UI thread for further processing
                            if (IsHandleCreated)
                                BeginInvoke(() => RobotDataAnalys(message));
                            else
                                RobotDataAnalys(message);
                        }
                        bufferContent = bufferContent.Substring(delimIndex + 1);
                    }
                    _serialBuffer.Clear();
                    _serialBuffer.Append(bufferContent);
                }
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.ToString(), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void RobotDataAnalys(string data)
        {
            data = data.Trim();
            //Báo arm đã sẵn sàng ở vị trí home
            if (data.Contains("A1"))
            {
                RobotArm_isReady = true;
                RobotArm_doneS1 = false;
                RobotArm_doneS2 = false;
            }
            //Báo arm lấy xong hàng
            else if (data.Contains("A2"))
            {
                RobotArm_isReady = false;
                RobotArm_doneS1 = true;
                RobotArm_doneS2 = false;
                RobotarmControl(3);
            }
            //Báo arm trả xong hàng
            else if (data.Contains("A3"))
            {
                RobotArm_isReady = false;
                RobotArm_doneS1 = false;
                RobotArm_doneS2 = true;
                _material = string.Empty;
            }
            //Báo arm về home xong
            else if (data.Contains("A4"))
            {
                RobotArm_isReady = true;
                RobotArm_doneS1 = false;
                RobotArm_doneS2 = false;
                _material = string.Empty;
            }
            //Hàng ở vị trí chụp ảnh
            else if (data.Contains("S1"))
            {
                CameraWait = true;
                RobotArmWait = false;
            }
            //Hàng ở vị trí chờ gắp
            else if (data.Contains("S2"))
            {
                CameraWait = false;
                RobotArmWait = true;
            }
        }

        /// <summary>
        /// Điều khiển cánh tay robot gắp chuyển hàng
        /// </summary>
        /// <param name="detections"></param>
        private void RobotarmControl(int step)
        {
            if (!string.IsNullOrEmpty(_material) != null)
            {
                string dest = string.Empty;
                if (step == 1 && CameraWait && _material != _oldMaterial)
                {
                    CallConveyer("d1");
                    RobotarmControl(2);
                    _oldMaterial = _material;
                    CameraWait = false;
                }
                else if (step == 2) //gọi cánh tay đi lấy hàng
                {
                    if (RobotArm_isReady && RobotArmWait) //cánh tay đang wait => gọi luôn
                    {
                        dest = _robotService.GetRobotDest("POS0");
                        CallRobotarm(dest);
                        timerCheckJob.Stop();
                    }
                    else  //Cánh tay đang busy => 0.5s sau quét lại
                    {
                        timerCheckJob.Start();
                    }
                }
                else if (step == 3) //Cánh tay lấy hàng xong => dựa theo loại sản phẩm để lấy điểm trả hàng
                {
                    switch (_material)
                    {
                        case "cylinder": //trụ
                            dest = _robotService.GetRobotDest("POS1");
                            CallRobotarm(dest);
                            break;
                        case "pentagonal_prism": //ngũ giác
                            dest = _robotService.GetRobotDest("POS2");
                            CallRobotarm(dest);
                            break;
                        case "hexagonal_prism": //lục giác
                            dest = _robotService.GetRobotDest("POS3");
                            CallRobotarm(dest);
                            break;
                        case "star6": //sao 6 cánh
                            dest = _robotService.GetRobotDest("POS4");
                            CallRobotarm(dest);
                            break;
                        case "cuboid": //hình hộp chữ nhật
                            dest = _robotService.GetRobotDest("POS5");
                            CallRobotarm(dest);
                            break;
                        default:
                            dest = _robotService.GetRobotDest("POS6");
                            CallRobotarm(dest);
                            break;
                    }
                }
            }
        }

        private void timerCheckJob_Tick(object sender, EventArgs e)
        {
            RobotarmControl(2);
        }

        private void CallConveyer(string mess)
        {
            Robot.Write(mess);
        }

        private void CallRobotarm(string dest)
        {
            Robot.Write("m" + dest);
        }

        // ─── Pipeline event handlers ────────────────────────────────────────────────

        /// <summary>Fired on the camera thread — pushes the raw frame to the UI immediately.</summary>
        private void OnFrameReady(object? sender, FrameReadyEventArgs e)
        {
            if (IsHandleCreated)
                BeginInvoke(() => detectionPanel.UpdateFrame(e.Frame, e.CachedDetections));
        }

        /// <summary>Fired after YOLO finishes — updates the panel with detection boxes.</summary>
        private void OnDetectionCompleted(object? sender, DetectionCompletedEventArgs e)
        {
            if (IsHandleCreated)
                BeginInvoke(() =>
                {
                    detectionPanel.UpdateFrame(e.Frame, e.Detections);
                    UpdateResultsGrid(e.Detections);
                    if (e.Detections.Count > 0)
                    {
                        _material = e.Detections[0].ClassName.ToLower().Trim();
                        RobotarmControl(1);
                    }
                });
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

        private void UpdateStatus(string message, Color color)
        {
            lblStatus.Text = $"● {message}";
            lblStatus.ForeColor = color;
        }

        private void btnStartCamera_Click(object sender, EventArgs e)
        {
            if (!_pipeline.IsInitialized)
            {
                MessageBox.Show("YOLO model not initialized", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            _pipeline.Start();
            btnStartCamera.Enabled = false;
            btnStop.Enabled = true;
            UpdateStatus("Camera running", Color.LimeGreen);
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _pipeline.Stop();
            btnStartCamera.Enabled = true;
            btnStop.Enabled = false;
            UpdateStatus("Camera stopped", Color.Orange);
        }

        private async void btnCapture_Click(object sender, EventArgs e)
        {
            if (!_pipeline.IsInitialized)
            {
                MessageBox.Show("YOLO model not initialized", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var snapshot = _pipeline.CaptureSnapshot();
            if (snapshot != null)
            {
                // Show frame immediately with last cached detections
                detectionPanel.UpdateFrame((Bitmap)snapshot.Clone(), _pipeline.CachedDetections);
                UpdateStatus("Detecting...", Color.Orange);

                var detections = await _pipeline.DetectSnapshotAsync(snapshot);
                detectionPanel.UpdateFrame(snapshot, detections);
                UpdateResultsGrid(detections);
                UpdateStatus($"Captured - {detections.Count} objects detected", Color.LimeGreen);
                if (detections.Count > 0)
                {
                    _material = detections[0].ClassName.ToLower().Trim();
                    RobotarmControl(1);
                }
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
            _pipeline.Dispose();
            base.OnFormClosing(e);
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            frmRobotTurning frm = new frmRobotTurning();
            Robot.Close();
            if (frm.ShowDialog() == DialogResult.OK)
            {
                Robot.Open();
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            if (!_pipeline.IsInitialized)
            {
                MessageBox.Show("YOLO model chưa được khởi tạo. Vui lòng chờ khởi tạo xong.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var frm = new frmTestDetection(_pipeline);
            frm.Show(this);
        }

        private void btnPrepareDataset_Click(object sender, EventArgs e)
        {
            using var frm = new frmPrepareDataset();
            frm.ShowDialog(this);
        }

    }
}
