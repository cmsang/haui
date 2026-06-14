using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Haui.PCB.ViewModels;

/// <summary>
/// Shared line-status state for the shell Monitor panel (Load/Unload, AGV gauges, interval counters).
/// </summary>
public sealed class MonitorViewModel : INotifyPropertyChanged
{
    private int _loadCount;
    private int _unloadCount;
    private int _count5Min;
    private int _count10Min;
    private int _count15Min;
    private int _count20Min;

    private double _totalAgv = 100;
    private double _agvConnectAngle;
    private double _agvFullAngle;
    private double _agvFullFull;
    private double _agvFullEmpty;
    private double _agvRunAngle;
    private double _agvRunFull;
    private double _agvRunStop;
    private double _bufferAngle;
    private double _bufferFull;
    private double _bufferEmpty;

    public event PropertyChangedEventHandler? PropertyChanged;

    public int LoadCount
    {
        get => _loadCount;
        set { _loadCount = value; NotifyCount(nameof(LoadCount), nameof(LoadCountDisplay)); }
    }

    public int UnloadCount
    {
        get => _unloadCount;
        set { _unloadCount = value; NotifyCount(nameof(UnloadCount), nameof(UnloadCountDisplay)); }
    }

    public string LoadCountDisplay => LoadCount.ToString("D3");
    public string UnloadCountDisplay => UnloadCount.ToString("D3");

    public int Count5Min
    {
        get => _count5Min;
        set { _count5Min = value; NotifyCount(nameof(Count5Min), nameof(Count5MinDisplay)); }
    }

    public int Count10Min
    {
        get => _count10Min;
        set { _count10Min = value; NotifyCount(nameof(Count10Min), nameof(Count10MinDisplay)); }
    }

    public int Count15Min
    {
        get => _count15Min;
        set { _count15Min = value; NotifyCount(nameof(Count15Min), nameof(Count15MinDisplay)); }
    }

    public int Count20Min
    {
        get => _count20Min;
        set { _count20Min = value; NotifyCount(nameof(Count20Min), nameof(Count20MinDisplay)); }
    }

    public string Count5MinDisplay => Count5Min.ToString("D3");
    public string Count10MinDisplay => Count10Min.ToString("D3");
    public string Count15MinDisplay => Count15Min.ToString("D3");
    public string Count20MinDisplay => Count20Min.ToString("D3");

    public double TotalAgv
    {
        get => _totalAgv;
        set { _totalAgv = value; OnPropertyChanged(); }
    }

    public double AgvConnectAngle
    {
        get => _agvConnectAngle;
        set { _agvConnectAngle = value; OnPropertyChanged(); }
    }

    public double AgvFullAngle
    {
        get => _agvFullAngle;
        set { _agvFullAngle = value; OnPropertyChanged(); }
    }

    public double AgvFullFull
    {
        get => _agvFullFull;
        set { _agvFullFull = value; OnPropertyChanged(); }
    }

    public double AgvFullEmpty
    {
        get => _agvFullEmpty;
        set { _agvFullEmpty = value; OnPropertyChanged(); }
    }

    public double AgvRunAngle
    {
        get => _agvRunAngle;
        set { _agvRunAngle = value; OnPropertyChanged(); }
    }

    public double AgvRunFull
    {
        get => _agvRunFull;
        set { _agvRunFull = value; OnPropertyChanged(); }
    }

    public double AgvRunStop
    {
        get => _agvRunStop;
        set { _agvRunStop = value; OnPropertyChanged(); }
    }

    public double BufferAngle
    {
        get => _bufferAngle;
        set { _bufferAngle = value; OnPropertyChanged(); }
    }

    public double BufferFull
    {
        get => _bufferFull;
        set { _bufferFull = value; OnPropertyChanged(); }
    }

    public double BufferEmpty
    {
        get => _bufferEmpty;
        set { _bufferEmpty = value; OnPropertyChanged(); }
    }

    public void IncrementLoad() => LoadCount++;

    public void IncrementUnload() => UnloadCount++;

    private void NotifyCount(string countProperty, string displayProperty)
    {
        OnPropertyChanged(countProperty);
        OnPropertyChanged(displayProperty);
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
