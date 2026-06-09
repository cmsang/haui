using System.IO.Ports;
using System.Text;
using UTT.ShapesDetector.Common;
using UTT.ShapesDetector.Models;
using UTT.ShapesDetector.Services;
using ZedGraph;

namespace UTT.ShapesDetector
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

        RollingPointPairList lst = new RollingPointPairList(12000);
        RollingPointPairList lst1 = new RollingPointPairList(12000);
        bool bStopTest = false;
        int val2 = 2;
        public frmMain()
        {
            InitializeComponent();

            _pipeline = new DetectionPipeline(new YoloV11DetectionService(), new CameraService());
            _robotService = new RobotService();

            SetupEventHandlers();

            InitChart();
            CheckForIllegalCrossThreadCalls = false;
        }

        private void SetupEventHandlers()
        {
            _pipeline.ErrorOccurred += (_, msg) =>
            {
                if (IsHandleCreated) BeginInvoke(() => UpdateStatus(msg, Color.Red));
            };
        }


        private void InitChart()
        {
            GraphPane mypane = zg.GraphPane;
            mypane.Title.Text = "Đồ thị giám sát độ rung theo thời gian";
            mypane.XAxis.Title.Text = "Thời gian, s";
            mypane.YAxis.Title.Text = "Độ rung";
            mypane.XAxis.MajorGrid.IsVisible = true;
            mypane.YAxis.MajorGrid.IsVisible = true;

            LineItem myCurve = mypane.AddCurve("Độ rung", lst, Color.Red, SymbolType.Default);
            myCurve.Line.Width = 5;
            myCurve.Line.IsVisible = false;
            myCurve.Symbol.Border.IsVisible = false;
            myCurve.Symbol.Fill = new Fill(Color.Red);
            myCurve.Symbol.Size = 2;

        }

        private void UpdateChart()
        {
            zg.AxisChange();
            zg.Invalidate();
            zg.Update();
            zg.Refresh();
        }

        private void CaculateData()
        {
            try
            {
                double time = 0;
                float fStartDegree = 0;
                while (true)
                {
                    float fscale = Convert.ToSingle(2);
                    lst.Add(time, fscale * Math.Sin(Math.PI * fStartDegree / 180));
                    lst1.Add(time, val2);
                    fStartDegree++;
                    time++;
                    UpdateChart();

                    if (bStopTest)
                        break;
                }

            }
            catch (Exception)
            {

            }

        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            bStopTest = false;
            //CaculateData();
            lst.Clear();
            Thread drawChartThred = new Thread(new ThreadStart(CaculateData));
            drawChartThred.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            bStopTest = true;
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

                UpdateStatus("Ready", Color.LimeGreen);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                UpdateStatus("Initialization failed", Color.Red);
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

        private void UpdateStatus(string message, Color color)
        {
            lblStatus.Text = $"● {message}";
            lblStatus.ForeColor = color;
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

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
