using Haui.GarlicDetector.Common;
using Haui.GarlicDetector.ML;
using Haui.GarlicDetector.Models;
using Haui.GarlicDetector.Services;
using Haui.GarlicDetector.Vision;
using System.IO.Ports;
using System.Text;

namespace Haui.GarlicDetector;

public partial class frmMain : Form
{
    private GarlicPipeline _pipeline;
    private HsvSegmenter _segmenter;
    private IGarlicClassifier? _svmClassifier;
    private readonly frmSettings _frmSettings = new();
    private SerialPort Robot = new SerialPort();
    private readonly StringBuilder _serialBuffer = new StringBuilder();
    private bool CameraWait = false;
    private System.Windows.Forms.Timer? _annotationTimer;

    public frmMain()
    {
        InitializeComponent();
    }

    // ─── Khởi tạo form ────────────────────────────────────────────────────────

    private void frmMain_Load(object sender, EventArgs e)
    {
        // Quét và điền danh sách camera vào combobox
        LoadCameraList();

        // Điền các tùy chọn độ phân giải vào combobox
        LoadResolutionList();

        // Đăng ký sự kiện HSV từ form cài đặt
        _frmSettings.HsvChanged += frmSettings_HsvChanged;
        _frmSettings.AutoDetectChanged += frmSettings_AutoDetectChanged;

        // Đăng ký Paint overlay cho vùng nhận diện
        picCamera.Paint += PicCamera_Paint;

        // Nạp model SVM nếu đã có đường dẫn trong settings
        LoadSvmModel();

        // Khôi phục trạng thái AutoDetect từ settings
        ApplyAutoDetectSetting(AppSettings.Instance.AutoDetect);

        // Khởi tạo kết nối serial với robot
        if (!Robot.IsOpen)
        {
            try
            {
                Robot.PortName = AppSettings.Instance.RobotPortName;
                Robot.BaudRate = AppSettings.Instance.RobotBaudRate;
                Robot.Open();
                Robot.DataReceived += Robot_DataReceived;
                lblStatus.Text = $"Kết nối robot: {Robot.PortName} @ {Robot.BaudRate} baud ✓";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối robot: {ex.Message}");
            }
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
    /// Phân tích dữ liệu nhận từ robot.
    /// </summary>
    /// <param name="data">Dữ liệu từ robot</param>
    private void RobotDataAnalys(string data)
    {
        data = data.Trim();

        // Hàng ở vị trí chụp ảnh
        if (data.Contains("S1"))
        {
            ImageDetect(retryCount: 0);
        }
    }

    /// <summary>
    /// Thực hiện nhận dạng và phân loại tỏi trong hình ảnh hiện tại.
    /// Được gọi khi robot gửi tín hiệu "S1" (hàng ở vị trí chụp ảnh).
    /// Chỉ xử lý 1 củ tỏi duy nhất (lấy vùng lớn nhất).
    /// Tận dụng luồng xử lý có sẵn trong GarlicPipeline.SegmentFrame().
    /// </summary>
    /// <param name="retryCount">Số lần đã thử (0-2), tối đa 3 lần</param>
    private void ImageDetect(int retryCount = 0)
    {
        const int MAX_RETRY = 3;

        if (_pipeline == null)
        {
            BeginInvoke(() =>
            {
                lblStatus.Text = "Lỗi: Camera chưa khởi động.";
                MessageBox.Show("Lỗi: Camera chưa khởi động.", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            });
            ConveyerRun();
            return;
        }

        try
        {
            // Chụp snapshot từ camera
            var snapshot = _pipeline.CaptureSnapshot();
            if (snapshot == null)
            {
                // Retry nếu chưa vượt quá giới hạn
                if (retryCount < MAX_RETRY - 1)
                {
                    BeginInvoke(() => lblStatus.Text = $"Không thể chụp ảnh. Thử lại... ({retryCount + 1}/{MAX_RETRY})");
                    Thread.Sleep(500); // Đợi 500ms trước khi thử lại
                    ImageDetect(retryCount + 1);
                    return;
                }

                // Đã thử 3 lần vẫn lỗi
                BeginInvoke(() =>
                {
                    lblStatus.Text = "Lỗi: Không thể chụp ảnh sau 3 lần thử.";
                    MessageBox.Show(
                        "Không thể chụp ảnh từ camera sau 3 lần thử.\nVui lòng kiểm tra lại camera.",
                        "Lỗi chụp ảnh",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                });
                SendResultToRobot(null);
                ConveyerRun();
                return;
            }

            // Tận dụng luồng xử lý có sẵn trong GarlicPipeline
            // (preprocess → segment → classify với SVM → phân kích thước)
            var regions = _pipeline.SegmentFrame(snapshot, _pipeline.DetectionRegion);

            // Kiểm tra có phát hiện củ tỏi không
            if (regions.Count == 0)
            {
                // Retry nếu chưa vượt quá giới hạn
                if (retryCount < MAX_RETRY - 1)
                {
                    BeginInvoke(() => lblStatus.Text = $"Không phát hiện tỏi. Thử lại... ({retryCount + 1}/{MAX_RETRY})");
                    snapshot.Dispose();
                    Thread.Sleep(500); // Đợi 500ms trước khi thử lại
                    ImageDetect(retryCount + 1);
                    return;
                }

                // Đã thử 3 lần vẫn không phát hiện
                BeginInvoke(() =>
                {
                    lblStatus.Text = "Cảnh báo: Không phát hiện tỏi sau 3 lần thử.";
                    MessageBox.Show(
                        "Không phát hiện củ tỏi nào trong khung hình sau 3 lần thử.\n" +
                        "Có thể không có tỏi hoặc ngưỡng HSV chưa phù hợp.",
                        "Cảnh báo",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                });
                SendResultToRobot(null);
                ConveyerRun();
                snapshot.Dispose();
                return;
            }

            // Lấy vùng tỏi lớn nhất (bỏ qua nhiễu nhỏ)
            var largestRegion = regions.OrderByDescending(r => r.Area).First();
            var garlicLabel = largestRegion.FinalLabel ?? GarlicLabel.ToNho;

            // Vẽ kết quả lên ảnh
            var resultBitmap = (Bitmap)snapshot.Clone();
            GarlicPipeline.DrawRegions(resultBitmap, new List<GarlicRegion> { largestRegion });

            // Tên loại tỏi để hiển thị
            string labelText = garlicLabel switch
            {
                GarlicLabel.ToTo => "Tỏi to",
                GarlicLabel.ToNho => "Tỏi nhỏ",
                GarlicLabel.ToHong => "Tỏi hỏng",
                _ => "Không xác định"
            };

            // Hiển thị kết quả lên UI
            BeginInvoke(() =>
            {
                lblStatus.Text = $"✓ Phát hiện: {labelText} (Area: {largestRegion.Area:F0} px², Circ: {largestRegion.Circularity:F2})";
                SetFrame(resultBitmap);


                AddResultToGrid(largestRegion);
            });

            // Gửi kết quả về robot
            SendResultToRobot(garlicLabel);

            // Tiếp tục chạy băng tải
            ConveyerRun();

            // Dọn dẹp
            snapshot.Dispose();
        }
        catch (Exception ex)
        {
            // Retry nếu chưa vượt quá giới hạn
            if (retryCount < MAX_RETRY - 1)
            {
                BeginInvoke(() => lblStatus.Text = $"Lỗi xử lý. Thử lại... ({retryCount + 1}/{MAX_RETRY})");
                Thread.Sleep(500); // Đợi 500ms trước khi thử lại
                ImageDetect(retryCount + 1);
                return;
            }

            // Đã thử 3 lần vẫn lỗi
            BeginInvoke(() =>
            {
                lblStatus.Text = $"Lỗi nghiêm trọng: {ex.Message}";
                MessageBox.Show(
                    $"Lỗi nhận dạng sau 3 lần thử:\n{ex.Message}\n\nVui lòng kiểm tra lại hệ thống.",
                    "Lỗi nghiêm trọng",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            });
            SendResultToRobot(null);
            ConveyerRun();
        }
    }

    /// <summary>
    /// Gửi tín hiệu cho robot để tiếp tục chạy băng tải.
    /// </summary>
    private void ConveyerRun()
    {
        if (!Robot.IsOpen) return;

        try
        {
            // Gửi tín hiệu tiếp tục băng tải
            Robot.Write("C:1x"); // C:1 = Continue conveyer
            BeginInvoke(() => lblStatus.Text = "Băng tải tiếp tục...");
        }
        catch (Exception ex)
        {
            BeginInvoke(() => lblStatus.Text = $"Lỗi điều khiển băng tải: {ex.Message}");
        }
    }

    /// <summary>
    /// Gửi kết quả phân loại về robot qua cổng serial.
    /// </summary>
    /// <param name="garlicLabel">Loại tỏi được phát hiện (null nếu không có tỏi)</param>
    private void SendResultToRobot(GarlicLabel? garlicLabel)
    {
        if (!Robot.IsOpen) return;

        try
        {
            // Format: "R:<loại tỏi>x" 
            // 0 = Tỏi to, 1 = Tỏi nhỏ, 2 = Tỏi hỏng, -1 = Không có tỏi
            int labelValue = garlicLabel.HasValue ? (int)garlicLabel.Value : -1;
            string message = $"R:{labelValue}x";
            Robot.Write(message);
        }
        catch (Exception ex)
        {
            BeginInvoke(() => lblStatus.Text = $"Lỗi gửi dữ liệu: {ex.Message}");
        }
    }

    /// <summary>Quét và điền danh sách camera khả dụng vào combobox.</summary>
    private void LoadCameraList()
    {
        cmbCameras.Items.Clear();
        foreach (var cam in CameraService.GetAvailableCameras())
            cmbCameras.Items.Add(cam);

        if (cmbCameras.Items.Count > 0)
            cmbCameras.SelectedIndex = 0;
    }

    /// <summary>Điền danh sách độ phân giải cài sẵn vào combobox, khôi phục lựa chọn gần nhất.</summary>
    private void LoadResolutionList()
    {
        cmbResolution.Items.Clear();
        foreach (var res in CameraResolution.GetPresets())
            cmbResolution.Items.Add(res);

        // Khôi phục lựa chọn gần nhất từ settings
        var lastLabel = AppSettings.Instance.LastResolutionLabel;
        if (!string.IsNullOrWhiteSpace(lastLabel))
        {
            for (int i = 0; i < cmbResolution.Items.Count; i++)
            {
                if (cmbResolution.Items[i] is CameraResolution r && r.Label == lastLabel)
                {
                    cmbResolution.SelectedIndex = i;
                    return;
                }
            }
        }

        // Mặc định chọn 640 × 480
        cmbResolution.SelectedIndex = Math.Min(1, cmbResolution.Items.Count - 1);
    }

    // ─── Điều khiển camera ────────────────────────────────────────────────────

    /// <summary>Bắt đầu pipeline khi nhấn nút Bắt đầu.</summary>
    private void btnStart_Click(object sender, EventArgs e)
    {
        if (_pipeline != null) return;

        var camService = new CameraService();
        _segmenter = new HsvSegmenter();
        var preprocessor = new HsvGarlicPreprocessor();

        // Khởi tạo feature extractor với kích thước ảnh đã lưu trong settings
        var featureExtractor = new GarlicFeatureExtractor(AppSettings.Instance.SvmTrainImageSize);

        _pipeline = new GarlicPipeline(
            camService,
            preprocessor,
            _segmenter,
            _svmClassifier,
            featureExtractor);

        // Đăng ký sự kiện từ pipeline
        _pipeline.FrameReady += OnFrameReady;
        _pipeline.SegmentationCompleted += OnSegmentationCompleted;
        _pipeline.ErrorOccurred += OnErrorOccurred;

        // Áp dụng vùng nhận diện đã lưu trong AppSettings
        _pipeline.DetectionRegion = AppSettings.Instance.DetectionRectangle;

        // Đồng bộ chế độ AutoDetect vào pipeline
        _pipeline.AutoSegment = AppSettings.Instance.AutoDetect;

        // Đồng bộ giá trị HSV hiện tại từ frmSettings vào segmenter
        SyncHsvToSegmenter();

        // Lấy camera và độ phân giải đã chọn
        int camIndex = cmbCameras.SelectedItem is CameraInfo cam ? cam.Index : 0;
        var resolution = cmbResolution.SelectedItem as CameraResolution;

        _pipeline.Start(camIndex, resolution);

        btnStart.Enabled = false;
        btnStop.Enabled = true;
        btnDetect.Enabled = !AppSettings.Instance.AutoDetect;
        lblStatus.Text = "Đang chạy...";
    }

    /// <summary>Dừng pipeline khi nhấn nút Dừng.</summary>
    private void btnStop_Click(object sender, EventArgs e) => StopPipeline();

    /// <summary>Dừng và giải phóng toàn bộ pipeline.</summary>
    private void StopPipeline()
    {
        if (_pipeline == null) return;

        _pipeline.FrameReady -= OnFrameReady;
        _pipeline.SegmentationCompleted -= OnSegmentationCompleted;
        _pipeline.ErrorOccurred -= OnErrorOccurred;

        _pipeline.Stop();
        _pipeline.Dispose();
        _pipeline = null;
        _segmenter = null;

        // Xóa ảnh đang hiển thị
        picCamera.Image?.Dispose();
        picCamera.Image = null;

        btnStart.Enabled = true;
        btnStop.Enabled = false;
        btnDetect.Enabled = false;
        lblStatus.Text = "Đã dừng.";
    }

    /// <summary>
    /// Người dùng chọn camera khác.
    /// Nếu pipeline đang chạy thì chuyển sang camera mới ngay lập tức.
    /// </summary>
    private void cmbCameras_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_pipeline == null || cmbCameras.SelectedItem is not CameraInfo cam) return;

        var resolution = cmbResolution.SelectedItem as CameraResolution;
        _pipeline.SwitchCamera(cam.Index, resolution);
        lblStatus.Text = $"Đã chuyển sang {cam.Name}.";
    }

    /// <summary>
    /// Người dùng chọn độ phân giải khác.
    /// Nếu pipeline đang chạy thì áp dụng ngay lập tức.
    /// </summary>
    private void cmbResolution_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cmbResolution.SelectedItem is not CameraResolution res) return;

        // Lưu lựa chọn vào settings
        AppSettings.Instance.LastResolutionLabel = res.Label;
        AppSettings.Instance.Save();

        if (_pipeline == null) return;
        _pipeline.SwitchResolution(res);
        lblStatus.Text = $"Độ phân giải: {res.Label}.";
    }

    // ─── Xử lý sự kiện pipeline ──────────────────────────────────────────────

    /// <summary>
    /// Nhận frame thô từ camera (~30 fps).
    /// Overlay các vùng tỏi đã cache lên frame để video mượt mà không chờ phân vùng.
    /// </summary>
    private void OnFrameReady(object? sender, FrameReadyEventArgs e)
    {
        var frame = e.Frame;
        if (e.CachedRegions.Count > 0)
            GarlicPipeline.DrawRegions(frame, e.CachedRegions);

        SetFrame(frame);
    }

    /// <summary>
    /// Nhận kết quả phân vùng mới từ thread-pool.
    /// Frame đã được overlay bên trong pipeline — cập nhật PictureBox và trạng thái.
    /// </summary>
    private void OnSegmentationCompleted(object? sender, SegmentationCompletedEventArgs e)
    {
        // Chế độ thủ công: chỉ hiển thị frame live, không ghi grid
        if (!AppSettings.Instance.AutoDetect)
        {
            SetFrame(e.Frame);
            return;
        }

        SetFrame(e.Frame);

        var text = $"Phát hiện {e.Regions.Count} vùng tỏi.";
        if (InvokeRequired)
            BeginInvoke(() =>
            {
                lblStatus.Text = text;
                foreach (var r in e.Regions)
                    AddResultToGrid(r);
            });
        else
        {
            lblStatus.Text = text;
            foreach (var r in e.Regions)
                AddResultToGrid(r);
        }
    }

    /// <summary>Hiển thị thông báo lỗi từ pipeline lên thanh trạng thái.</summary>
    private void OnErrorOccurred(object? sender, string message)
    {
        var text = $"Lỗi: {message}";
        if (InvokeRequired) BeginInvoke(() => lblStatus.Text = text);
        else lblStatus.Text = text;
    }

    /// <summary>Cập nhật PictureBox trên UI thread, giải phóng ảnh cũ.</summary>
    private void SetFrame(Bitmap frame)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => SetFrame(frame));
            return;
        }

        var old = picCamera.Image;
        picCamera.Image = frame;
        old?.Dispose();

        // Trigger Paint để vẽ overlay viền vùng nhận diện lên trên ảnh mới
        if (_pipeline?.DetectionRegion.HasValue == true)
            picCamera.Invalidate();
    }

    // ─── Overlay vùng nhận diện (Paint event) ────────────────────────────────

    /// <summary>
    /// Vẽ viền xanh lá nét đứt biểu thị vùng nhận diện lên PictureBox
    /// trong tọa độ màn hình (không sửa bitmap) — chạy trên UI thread, tần suất thấp.
    /// </summary>
    private void PicCamera_Paint(object? sender, PaintEventArgs e)
    {
        var region = _pipeline?.DetectionRegion;
        if (!region.HasValue || picCamera.Image == null) return;

        var ir = GetPicBoxImageRect(picCamera);
        if (ir.IsEmpty) return;

        float scaleX = ir.Width / picCamera.Image.Width;
        float scaleY = ir.Height / picCamera.Image.Height;

        var dr = region.Value;
        var displayRect = new RectangleF(
            ir.X + dr.X * scaleX,
            ir.Y + dr.Y * scaleY,
            dr.Width * scaleX,
            dr.Height * scaleY);

        using var regionPen = new Pen(Color.LimeGreen, 2)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
        };
        e.Graphics.DrawRectangle(regionPen, Rectangle.Round(displayRect));

        var font = SystemFonts.SmallCaptionFont ?? SystemFonts.DefaultFont;
        var labelPt = new PointF(displayRect.X + 4, displayRect.Y + 4);
        e.Graphics.DrawString("Vùng nhận diện", font, Brushes.LimeGreen, labelPt);
    }

    /// <summary>
    /// Tính hình chữ nhật (display coords) của ảnh thực tế bên trong PictureBox Zoom mode.
    /// </summary>
    private static RectangleF GetPicBoxImageRect(PictureBox pb)
    {
        if (pb.Image == null) return RectangleF.Empty;

        float imgW = pb.Image.Width;
        float imgH = pb.Image.Height;
        float pbW = pb.ClientSize.Width;
        float pbH = pb.ClientSize.Height;
        float scale = Math.Min(pbW / imgW, pbH / imgH);
        float dw = imgW * scale;
        float dh = imgH * scale;

        return new RectangleF((pbW - dw) / 2f, (pbH - dh) / 2f, dw, dh);
    }

    // ─── Cài đặt HSV ─────────────────────────────────────────────────────────

    /// <summary>Mở/ẩn form cài đặt ngưỡng HSV bên cạnh cửa sổ chính.</summary>
    private void btnSettings_Click(object sender, EventArgs e)
    {
        if (_frmSettings.Visible)
        {
            _frmSettings.Hide();
            return;
        }

        // Right/Top của Form đã là tọa độ màn hình — đặt form Settings sát cạnh phải
        _frmSettings.Location = new Point(Right + 4, Top);
        _frmSettings.Show(this);
    }

    /// <summary>Nhận thông báo từ frmSettings mỗi khi giá trị HSV thay đổi.</summary>
    private void frmSettings_HsvChanged(object? sender, EventArgs e) => SyncHsvToSegmenter();

    /// <summary>Đồng bộ giá trị HSV (cả 2 ngưỡng) từ frmSettings vào HsvSegmenter đang chạy.</summary>
    private void SyncHsvToSegmenter()
    {
        if (_segmenter == null) return;

        // Ngưỡng chính — tỏi trắng / bình thường
        _segmenter.HMin = _frmSettings.HMin;
        _segmenter.HMax = _frmSettings.HMax;
        _segmenter.SMin = _frmSettings.SMin;
        _segmenter.SMax = _frmSettings.SMax;
        _segmenter.VMin = _frmSettings.VMin;
        _segmenter.VMax = _frmSettings.VMax;

        // Ngưỡng phụ — tỏi hỏng / nâu / tối
        _segmenter.H2Min = _frmSettings.H2Min;
        _segmenter.H2Max = _frmSettings.H2Max;
        _segmenter.S2Min = _frmSettings.S2Min;
        _segmenter.S2Max = _frmSettings.S2Max;
        _segmenter.V2Min = _frmSettings.V2Min;
        _segmenter.V2Max = _frmSettings.V2Max;
    }

    // ─── Gán nhãn tỏi ────────────────────────────────────────────────────────

    /// <summary>
    /// Mở form gán nhãn tỏi.
    /// Nếu camera đang chạy, chụp snapshot làm ảnh ban đầu; nếu không thì mở form rỗng.
    /// </summary>
    private void btnLabeling_Click(object sender, EventArgs e)
    {
        Bitmap? snapshot = _pipeline?.CaptureSnapshot()
                        ?? (picCamera.Image is Bitmap bmp ? (Bitmap)bmp.Clone() : null);

        var frm = new frmLabeling(snapshot);
        frm.Show(this);
    }

    // ─── Huấn luyện SVM ──────────────────────────────────────────────────────

    /// <summary>Mở form test nhận diện ảnh tĩnh.</summary>
    private void btnTest_Click(object sender, EventArgs e)
    {
        var frm = new frmTestDetection();
        frm.Show(this);
    }

    /// <summary>Mở form huấn luyện SVM.</summary>
    private void btnTrainSvm_Click(object sender, EventArgs e)
    {
        var frm = new frmTrainSvm();
        frm.ShowDialog(this);

        // Sau khi train xong, tự động thử nạp lại model (path có thể vừa được cập nhật)
        LoadSvmModel();
    }

    /// <summary>
    /// Nạp model SVM từ đường dẫn đã lưu trong <see cref="AppSettings.SvmModelPath"/>.
    /// Hiển thị trạng thái lên <c>lblStatus</c>.
    /// </summary>
    private void LoadSvmModel()
    {
        var path = AppSettings.Instance.SvmModelPath;

        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            _svmClassifier = null;
            lblStatus.Text = "Chưa có model SVM — phân loại theo kích thước (Tỏi to / Tỏi nhỏ).";
            return;
        }

        try
        {
            _svmClassifier ??= new SvmClassifier(new GarlicFeatureExtractor(AppSettings.Instance.SvmTrainImageSize));
            _svmClassifier.Load(path);
            lblStatus.Text = $"Model SVM đã nạp ✓  ({Path.GetFileName(path)})";
        }
        catch (Exception ex)
        {
            _svmClassifier = null;
            lblStatus.Text = $"Lỗi nạp model SVM: {ex.Message}";
        }
    }

    // ─── Chọn vùng nhận diện ─────────────────────────────────────────────────

    /// <summary>
    /// Mở form chọn vùng nhận diện.
    /// Nếu camera đang chạy, chụp snapshot làm nền; nếu không thì dùng frame đang hiển thị.
    /// Kết quả được lưu vào <c>settings.json</c> và áp dụng ngay vào pipeline.
    /// </summary>
    private void btnSelectRegion_Click(object sender, EventArgs e)
    {
        // Lấy ảnh nền: snapshot từ camera hoặc frame hiện tại trên PictureBox
        Bitmap? snapshot = _pipeline?.CaptureSnapshot()
                        ?? (picCamera.Image is Bitmap bmp ? (Bitmap)bmp.Clone() : null);

        if (snapshot == null)
        {
            MessageBox.Show(
                "Vui lòng bắt đầu camera trước khi chọn vùng nhận diện.",
                "Chưa có ảnh",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var selector = new frmRegionSelector(snapshot, AppSettings.Instance.DetectionRectangle);
        snapshot.Dispose();

        if (selector.ShowDialog(this) != DialogResult.OK) return;

        // Lưu vào AppSettings và ghi ra settings.json
        var settings = AppSettings.Instance;
        settings.DetectionRegion = selector.SelectedRegion.HasValue
            ? RegionDto.From(selector.SelectedRegion.Value)
            : null;
        settings.Save();

        // Áp dụng ngay vào pipeline đang chạy (nếu có)
        if (_pipeline != null)
            _pipeline.DetectionRegion = selector.SelectedRegion;

        UpdateRegionStatus();
    }

    /// <summary>Cập nhật <see cref="lblStatus"/> với thông tin vùng nhận diện hiện tại.</summary>
    private void UpdateRegionStatus()
    {
        var region = AppSettings.Instance.DetectionRectangle;
        lblStatus.Text = region.HasValue
            ? $"Vùng: ({region.Value.X},{region.Value.Y}) {region.Value.Width}×{region.Value.Height}px"
            : "Nhận diện toàn bộ khung hình.";
    }

    /// <summary>Dọn dẹp tài nguyên khi đóng form.</summary>
    private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
    {
        _frmSettings.HsvChanged -= frmSettings_HsvChanged;
        _frmSettings.AutoDetectChanged -= frmSettings_AutoDetectChanged;
        _frmSettings.Dispose();
        StopPipeline();
        _svmClassifier?.Dispose();
    }

    // ─── AutoDetect / Nhận diện thủ công ─────────────────────────────────────

    /// <summary>
    /// Áp dụng cài đặt AutoDetect: toggle segmentation pipeline và hiển thị/ẩn btnDetect.
    /// </summary>
    private void ApplyAutoDetectSetting(bool autoDetect)
    {
        btnDetect.Visible = !autoDetect;
        btnDetect.Enabled = !autoDetect && _pipeline != null;

        // Bật/tắt vòng lặp phân vùng tự động trong pipeline
        if (_pipeline != null)
            _pipeline.AutoSegment = autoDetect;

        // Khi chuyển về auto, reset cached regions để tránh overlay cũ
        if (autoDetect)
            _pipeline?.CachedRegions?.Clear();
    }

    /// <summary>Nhận thông báo từ frmSettings khi AutoDetect thay đổi.</summary>
    private void frmSettings_AutoDetectChanged(object? sender, EventArgs e)
        => ApplyAutoDetectSetting(AppSettings.Instance.AutoDetect);

    /// <summary>
    /// Nút Nhận diện (chế độ thủ công): chụp ảnh, nhận diện, hiển thị annotation 2 giây.
    /// </summary>
    private async void btnDetect_Click(object sender, EventArgs e)
    {
        if (_pipeline == null) return;

        btnDetect.Enabled = false;
        lblStatus.Text = "Đang nhận diện...";

        try
        {
            await Task.Run(() =>
            {
                var snapshot = _pipeline.CaptureSnapshot();
                if (snapshot == null)
                {
                    BeginInvoke(() =>
                    {
                        lblStatus.Text = "Không thể chụp ảnh từ camera.";
                        btnDetect.Enabled = true;
                    });
                    return;
                }

                var regions = _pipeline.SegmentFrame(snapshot, _pipeline.DetectionRegion);

                // Vẽ annotation lên ảnh
                var annotated = (Bitmap)snapshot.Clone();
                if (regions.Count > 0)
                    GarlicPipeline.DrawRegions(annotated, regions);
                snapshot.Dispose();

                BeginInvoke(() =>
                {
                    // Hiển thị ảnh annotated
                    SetFrame(annotated);

                    // Ghi kết quả vào grid
                    foreach (var r in regions)
                        AddResultToGrid(r);

                    lblStatus.Text = regions.Count > 0
                        ? $"Phát hiện {regions.Count} củ tỏi."
                        : "Không phát hiện tỏi.";

                    // Sau 2 giây, trả về frame live
                    _annotationTimer?.Stop();
                    _annotationTimer?.Dispose();
                    _annotationTimer = new System.Windows.Forms.Timer { Interval = 250 };
                    _annotationTimer.Tick += (_, _) =>
                    {
                        _annotationTimer.Stop();
                        _annotationTimer.Dispose();
                        _annotationTimer = null;
                        btnDetect.Enabled = _pipeline != null;
                        lblStatus.Text = "";
                    };
                    _annotationTimer.Start();
                });
            });
        }
        catch (Exception ex)
        {
            lblStatus.Text = $"Lỗi: {ex.Message}";
            btnDetect.Enabled = true;
        }
    }

    // ─── Grid kết quả nhận diện ──────────────────────────────────────────────

    /// <summary>
    /// Thêm một kết quả nhận diện mới vào đầu grid, giới hạn tối đa 200 dòng.
    /// </summary>
    private void AddResultToGrid(GarlicRegion region)
    {
        string labelText = region.FinalLabel switch
        {
            GarlicLabel.ToTo   => "🟢 Tỏi to",
            GarlicLabel.ToNho  => "🟡 Tỏi nhỏ",
            GarlicLabel.ToHong => "🔴 Tỏi hỏng",
            _                  => "—"
        };

        // Tâm bounding box làm vị trí đại diện
        int cx = region.BoundingBox.X + region.BoundingBox.Width / 2;
        int cy = region.BoundingBox.Y + region.BoundingBox.Height / 2;

        dgvResults.Rows.Insert(0,
            region.DetectedAt.ToString("HH:mm:ss"),
            labelText,
            $"{region.Area:F0}",
            $"({cx}, {cy})");

        // Giới hạn 200 dòng
        while (dgvResults.Rows.Count > 200)
            dgvResults.Rows.RemoveAt(dgvResults.Rows.Count - 1);
    }

    /// <summary>Xóa toàn bộ danh sách kết quả nhận diện.</summary>
    private void btnClearResults_Click(object sender, EventArgs e)
    {
        dgvResults.Rows.Clear();
    }
}
