using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenCvSharp;

namespace Haui.PCB.ViewModels;

/// <summary>
/// ViewModel cho PipelineStepsWindow â€” cháº¡y pipeline debug vÃ  cung cáº¥p
/// danh sÃ¡ch cÃ¡c bÆ°á»›c xá»­ lÃ½ Ä‘á»ƒ hiá»ƒn thá»‹ má»—i bÆ°á»›c má»™t khung riÃªng biá»‡t.
/// </summary>
public class PipelineStepsViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly IPipelineDebugService _debugService;
    private Mat? _sourceMat;
    private string _statusText = "Äang chá»...";
    private bool _isBusy;
    private bool _disposed;

    public event PropertyChangedEventHandler? PropertyChanged;

    // â”€â”€â”€â”€ Properties â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

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

    // â”€â”€â”€â”€ Khá»Ÿi táº¡o â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public PipelineStepsViewModel(IPipelineDebugService debugService)
    {
        _debugService = debugService;
    }

    // â”€â”€â”€â”€ Actions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    /// <summary>Náº¡p áº£nh tá»« camera vÃ  tá»± Ä‘á»™ng cháº¡y pipeline.</summary>
    public void LoadImage(Mat mat)
    {
        _sourceMat?.Dispose();
        _sourceMat = mat.Clone();
        _ = RunAsync();
    }

    /// <summary>Cháº¡y pipeline debug vÃ  Ä‘iá»n káº¿t quáº£ vÃ o <see cref="Steps"/>.</summary>
    public async Task RunAsync()
    {
        if (_sourceMat is null || _sourceMat.Empty())
        {
            StatusText = "KhÃ´ng cÃ³ áº£nh Ä‘áº§u vÃ o.";
            return;
        }

        IsBusy = true;
        StatusText = "Äang cháº¡y pipeline...";
        Steps.Clear();

        try
        {
            using var clone = _sourceMat.Clone();
            var results = await Task.Run(() => _debugService.RunSteps(clone));

            foreach (var step in results)
                Steps.Add(step);

            StatusText = $"HoÃ n thÃ nh â€” {Steps.Count} bÆ°á»›c xá»­ lÃ½.";
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

    // â”€â”€â”€â”€ INotifyPropertyChanged â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // â”€â”€â”€â”€ IDisposable â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    public void Dispose()
    {
        if (_disposed) return;
        _sourceMat?.Dispose();
        _disposed = true;
    }
}
