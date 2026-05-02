using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenCvSharp;
using Haui.PCB.Processing;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel cho TestPipelineWindow — chứa toàn bộ logic xử lý ảnh và phân vùng PCB.
/// Tách biệt hoàn toàn khỏi UI, tuân theo SOLID: SRP, DIP.
/// </summary>
public class TestPipelineViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPcbSegmentationService _segmentation;

    private Mat? _sourceMat;
    private string _statusText = string.Empty;
    private bool _isBusy;
    private bool _disposed;

    // ──── Sự kiện ────────────────────────────────────────────────────────────

    /// <summary>Phát khi ảnh gốc đã được tải — BitmapSource đã Freeze.</summary>
    public event Action<System.Windows.Media.Imaging.BitmapSource>? SourceImageReady;

    /// <summary>Phát khi ảnh đã xử lý sẵn sàng — BitmapSource đã Freeze.</summary>
    public event Action<System.Windows.Media.Imaging.BitmapSource?>? ProcessedImageReady;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ──── Properties ─────────────────────────────────────────────────────────

    public string StatusText
    {
        get => _statusText;
        private set { _statusText = value; OnPropertyChanged(); }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set { _isBusy = value; OnPropertyChanged(); }
    }

    public bool HasSource => _sourceMat is not null && !_sourceMat.Empty();

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public TestPipelineViewModel(IPcbSegmentationService segmentation)
    {
        _segmentation = segmentation;
    }

    // ──── Actions ─────────────────────────────────────────────────────────────

    /// <summary>Nạp ảnh từ bên ngoài (ví dụ từ camera chụp).</summary>
    public void LoadImage(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();

        var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(_sourceMat);
        bitmap.Freeze();
        SourceImageReady?.Invoke(bitmap);
        ProcessedImageReady?.Invoke(null);

        StatusText = "Ảnh đã tải. Bấm ▶ Test để xử lý.";
        OnPropertyChanged(nameof(HasSource));
    }

    /// <summary>Nạp ảnh từ đường dẫn file.</summary>
    public void LoadImageFromFile(string filePath)
    {
        _sourceMat?.Dispose();
        _sourceMat = Cv2.ImRead(filePath, ImreadModes.Color);

        if (_sourceMat.Empty())
        {
            StatusText = "Không thể đọc ảnh.";
            OnPropertyChanged(nameof(HasSource));
            return;
        }

        var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(_sourceMat);
        bitmap.Freeze();
        SourceImageReady?.Invoke(bitmap);
        ProcessedImageReady?.Invoke(null);

        StatusText = $"Đã chọn: {System.IO.Path.GetFileName(filePath)}";
        OnPropertyChanged(nameof(HasSource));
    }

    /// <summary>Chạy phân vùng PCB trên thread nền và phát kết quả.</summary>
    public async Task RunSegmentationAsync()
    {
        if (!HasSource)
        {
            StatusText = "Vui lòng chọn ảnh trước.";
            return;
        }

        IsBusy = true;
        StatusText = "Đang xử lý...";

        // Clone để tránh race condition khi xử lý trên thread nền
        using var source = _sourceMat!.Clone();
        Mat? result = null;

        try
        {
            result = await Task.Run(() => _segmentation.Segment(source));

            if (result is null)
            {
                StatusText = "Không phát hiện được bo mạch. Thử điều chỉnh ảnh.";
                ProcessedImageReady?.Invoke(null);
            }
            else
            {
                var bitmap = OpenCvSharp.WpfExtensions.BitmapSourceConverter.ToBitmapSource(result);
                bitmap.Freeze();
                ProcessedImageReady?.Invoke(bitmap);
                StatusText = $"Hoàn thành. Kích thước PCB: {result.Width}×{result.Height} px";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Lỗi: {ex.Message}";
            ProcessedImageReady?.Invoke(null);
        }
        finally
        {
            result?.Dispose();
            IsBusy = false;
        }
    }

    // ──── INotifyPropertyChanged ──────────────────────────────────────────────

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // ──── IDisposable ─────────────────────────────────────────────────────────

    public void Dispose()
    {
        if (_disposed) return;
        _sourceMat?.Dispose();
        _disposed = true;
    }
}
