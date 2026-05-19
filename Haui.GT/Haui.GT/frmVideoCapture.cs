using OpenCvSharp;
using System.Diagnostics;

namespace Haui.GT
{
    public partial class frmVideoCapture : Form
    {
        private readonly List<string> _videoPaths = new();
        private string _outputFolder = string.Empty;
        private CancellationTokenSource? _cts;
        private bool _isRunning = false;

        public frmVideoCapture()
        {
            InitializeComponent();
            _outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "VideoCapture");
            txtOutputFolder.Text = _outputFolder;
        }

        private void btnAddVideo_Click(object sender, EventArgs e)
        {
            using var dlg = new OpenFileDialog
            {
                Title = "Chọn video",
                Filter = "Video files|*.mp4;*.avi;*.mkv;*.mov;*.wmv;*.flv;*.webm|All files|*.*",
                Multiselect = true
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                foreach (var f in dlg.FileNames)
                {
                    if (!_videoPaths.Contains(f))
                    {
                        _videoPaths.Add(f);
                        lsvVideos.Items.Add(Path.GetFileName(f));
                    }
                }
                UpdateVideoCount();
            }
        }

        private void btnRemoveVideo_Click(object sender, EventArgs e)
        {
            for (int i = lsvVideos.SelectedIndices.Count - 1; i >= 0; i--)
            {
                int idx = lsvVideos.SelectedIndices[i];
                _videoPaths.RemoveAt(idx);
                lsvVideos.Items.RemoveAt(idx);
            }
            UpdateVideoCount();
        }

        private void UpdateVideoCount()
        {
            grpVideo.Text = _videoPaths.Count == 0
                ? "Danh sách Video"
                : $"Danh sách Video ({_videoPaths.Count} file)";
        }

        private void btnBrowseFolder_Click(object sender, EventArgs e)
        {
            using var dlg = new FolderBrowserDialog
            {
                Description = "Chọn thư mục lưu ảnh",
                SelectedPath = _outputFolder
            };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _outputFolder = dlg.SelectedPath;
                txtOutputFolder.Text = _outputFolder;
            }
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            if (_isRunning)
            {
                _cts?.Cancel();
                return;
            }

            if (_videoPaths.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất một file video.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!double.TryParse(txtInterval.Text, out double intervalSeconds) || intervalSeconds <= 0)
            {
                MessageBox.Show("Khoảng thời gian t phải là số dương.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _outputFolder = txtOutputFolder.Text.Trim();
            if (string.IsNullOrWhiteSpace(_outputFolder))
            {
                MessageBox.Show("Vui lòng chọn thư mục lưu ảnh.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Directory.CreateDirectory(_outputFolder);

            _isRunning = true;
            _cts = new CancellationTokenSource();
            btnStart.Text = "⏹ Dừng";
            btnStart.BackColor = Color.FromArgb(231, 72, 86);
            progressBar.Visible = true;
            lblLog.Text = "Đang xử lý...";

            var videos = _videoPaths.ToList();
            bool singleFolder = chkSingleFolder.Checked;
            try
            {
                for (int vi = 0; vi < videos.Count && !_cts.IsCancellationRequested; vi++)
                {
                    string videoPath = videos[vi];
                    string videoName = Path.GetFileNameWithoutExtension(videoPath);
                    string videoFolder = singleFolder
                        ? _outputFolder
                        : Path.Combine(_outputFolder, videoName);
                    Directory.CreateDirectory(videoFolder);

                    int videoIndex = vi;
                    BeginInvoke(() =>
                    {
                        lsvVideos.SelectedIndex = videoIndex;
                        lblLog.Text = $"[{videoIndex + 1}/{videos.Count}] Đang xử lý: {Path.GetFileName(videoPath)}";
                        progressBar.Value = 0;
                    });

                    await Task.Run(() => ProcessVideo(videoPath, intervalSeconds, videoFolder, vi + 1, videos.Count, _cts.Token));
                }

                if (!_cts.IsCancellationRequested)
                    UpdateLog($"✅ Hoàn thành {videos.Count} video!");
                else
                    UpdateLog("⛔ Đã dừng.");
            }
            catch (Exception ex)
            {
                UpdateLog($"❌ Lỗi: {ex.Message}");
            }
            finally
            {
                _isRunning = false;
                BeginInvoke(() =>
                {
                    btnStart.Text = "▶ Bắt đầu";
                    btnStart.BackColor = Color.FromArgb(16, 185, 129);
                    progressBar.Visible = false;
                });
            }
        }

        private void ProcessVideo(string videoPath, double intervalSeconds, string outputFolder, int videoIdx, int videoTotal, CancellationToken token)
        {
            using var cap = new VideoCapture(videoPath);
            if (!cap.IsOpened())
                throw new Exception($"Không thể mở file video: {Path.GetFileName(videoPath)}");

            double fps = cap.Get(VideoCaptureProperties.Fps);
            double totalFrames = cap.Get(VideoCaptureProperties.FrameCount);
            double totalSeconds = totalFrames / fps;

            if (fps <= 0) fps = 25;

            int savedCount = 0;
            double currentSec = 0;
            string shortName = Path.GetFileName(videoPath);

            while (currentSec < totalSeconds && !token.IsCancellationRequested)
            {
                double segStart = currentSec;
                double segEnd = Math.Min(currentSec + intervalSeconds, totalSeconds);

                Mat? bestFrame = null;
                double bestSharpness = -1;

                int startFrame = (int)(segStart * fps);
                int endFrame = (int)(segEnd * fps);

                cap.Set(VideoCaptureProperties.PosFrames, startFrame);

                for (int f = startFrame; f < endFrame && !token.IsCancellationRequested; f++)
                {
                    using var frame = new Mat();
                    if (!cap.Read(frame) || frame.Empty()) break;

                    double sharpness = ComputeSharpness(frame);
                    if (sharpness > bestSharpness)
                    {
                        bestSharpness = sharpness;
                        bestFrame?.Dispose();
                        bestFrame = frame.Clone();
                    }
                }

                if (bestFrame != null && !token.IsCancellationRequested)
                {
                    savedCount++;
                    string fileName = Path.Combine(outputFolder, $"frame_{savedCount:D5}_t{segStart:F1}s.jpg");
                    Cv2.ImWrite(fileName, bestFrame);
                    bestFrame.Dispose();

                    int pct = totalSeconds > 0 ? (int)(segEnd / totalSeconds * 100) : 0;
                    UpdateLog($"[{videoIdx}/{videoTotal}] {shortName} — {pct}% — ảnh {savedCount}: t={segStart:F1}s");
                    BeginInvoke(() => progressBar.Value = Math.Min(pct, 100));
                }
                else
                {
                    bestFrame?.Dispose();
                }

                currentSec += intervalSeconds;
            }
        }

        private static double ComputeSharpness(Mat frame)
        {
            using var gray = new Mat();
            Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);
            using var lap = new Mat();
            Cv2.Laplacian(gray, lap, MatType.CV_64F);
            Cv2.MeanStdDev(lap, out _, out Scalar stddev);
            return stddev.Val0 * stddev.Val0;
        }

        private void UpdateLog(string message)
        {
            if (IsHandleCreated)
                BeginInvoke(() => lblLog.Text = message);
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            string folder = txtOutputFolder.Text.Trim();
            if (Directory.Exists(folder))
                Process.Start("explorer.exe", folder);
            else
                MessageBox.Show("Thư mục chưa tồn tại.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _cts?.Cancel();
            base.OnFormClosing(e);
        }
    }
}
