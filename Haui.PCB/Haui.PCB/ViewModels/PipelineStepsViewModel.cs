using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel cho PipelineStepsWindow — chạy pipeline debug và cung cấp
/// danh sách các bước xử lý để hiển thị mỗi bước một khung riêng biệt.
/// </summary>
public class PipelineStepsViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPipelineDebugService _debugService;
    private Mat? _sourceMat;
    private string _statusText = "Đang chờ...";
    private bool _isBusy;
    private bool _disposed;

    public event PropertyChangedEventHandler? PropertyChanged;

    // ──── Properties ─────────────────────────────────────────────────────────

    public ObservableCollection<PipelineStep> Steps { get; } = [];

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

    // ──── Khởi tạo ───────────────────────────────────────────────────────────

    public PipelineStepsViewModel(IPipelineDebugService debugService)
    {
        _debugService = debugService;
    }

    // ──── Actions ─────────────────────────────────────────────────────────────

    /// <summary>Nạp ảnh từ camera và tự động chạy pipeline.</summary>
    public void LoadImage(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();
        _ = RunAsync();
    }

    /// <summary>Chạy pipeline debug và điền kết quả vào <see cref="Steps"/>.</summary>
    public async Task RunAsync()
    {
        if (_sourceMat is null || _sourceMat.Empty())
        {
            StatusText = "Không có ảnh đầu vào.";
            return;
        }

        IsBusy = true;
        StatusText = "Đang chạy pipeline...";
        Steps.Clear();

        try
        {
            using var clone = _sourceMat.Clone();
            var results = await Task.Run(() => _debugService.RunSteps(clone));

            foreach (var step in results)
                Steps.Add(step);

            StatusText = $"Hoàn thành — {Steps.Count} bước xử lý.";
        }
        catch (Exception ex)
        {
            StatusText = $"Lá»—i: {ex.Message}";
        }
        finally
        {
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
