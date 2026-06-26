using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Haui.PCB.ViewModels;

/// <summary>One row in the allowed region names editor (1-based index + name).</summary>
public sealed class AllowedRegionNameItem : INotifyPropertyChanged
{
    private int _index;
    private string _name = string.Empty;

    public int Index
    {
        get => _index;
        set { _index = value; OnPropertyChanged(); }
    }

    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>ViewModel for Setting tab — edit and save <c>Config/setting.json</c>.</summary>
public sealed class SettingViewModel : INotifyPropertyChanged
{
    private readonly IAppSettingService _appSettingService;

    private bool _developerMode;
    private bool _virtualSerialPort;
    private bool _showInspectionResultAfterRecognition;
    private string _com = "COM3";
    private string _warehouseCom = string.Empty;
    private int _baudRate = 115200;
    private int _stepsPerDeg = 100;
    private double _jogStepDegrees = 10;
    private int _speedPercent = 50;
    private string _databaseConnection = string.Empty;
    private string _templateCustomFolder = string.Empty;
    private bool _trainWhiteCircuit;
    private string _whiteCircuitCustomFolder = string.Empty;
    private double _minMatchSimilarityPercent = ComponentTemplateSettings.DefaultMinMatchSimilarityPercent;
    private string _orientationComponentName = string.Empty;
    private double _minOrientationMatchScore = 0.55;
    private double _holderQuadRectTolerancePercent = PcbBoardSettings.DefaultQuadRectTolerancePercent;
    private double _holderAspectRatioTolerancePercent = PcbBoardSettings.DefaultAspectRatioTolerancePercent;
    private double _holderQuadAngleToleranceDegrees = PcbBoardSettings.DefaultQuadAngleToleranceDegrees;
    private double _pcbBoardWidthMm = PcbBoardSettings.DefaultWidthMm;
    private double _pcbBoardHeightMm = PcbBoardSettings.DefaultHeightMm;
    private string _cameraCaptureSaveFolder = string.Empty;
    private string _saveStatusText = string.Empty;

    public SettingViewModel(IAppSettingService? appSettingService = null)
    {
        _appSettingService = appSettingService ?? new AppSettingService();
        Load();
    }

    public ObservableCollection<AllowedRegionNameItem> AllowedRegionNames { get; } = [];

    public bool DeveloperMode
    {
        get => _developerMode;
        set
        {
            if (_developerMode == value) return;
            _developerMode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsVirtualSerialVisible));
            if (!value)
                VirtualSerialPort = false;
        }
    }

    public bool VirtualSerialPort
    {
        get => _virtualSerialPort;
        set { _virtualSerialPort = value; OnPropertyChanged(); }
    }

    public bool IsVirtualSerialVisible => DeveloperMode;

    public bool ShowInspectionResultAfterRecognition
    {
        get => _showInspectionResultAfterRecognition;
        set
        {
            if (_showInspectionResultAfterRecognition == value) return;
            _showInspectionResultAfterRecognition = value;
            OnPropertyChanged();
        }
    }

    public string Com
    {
        get => _com;
        set { _com = value; OnPropertyChanged(); }
    }

    public string WarehouseCom
    {
        get => _warehouseCom;
        set { _warehouseCom = value; OnPropertyChanged(); }
    }

    public int BaudRate
    {
        get => _baudRate;
        set { _baudRate = value; OnPropertyChanged(); }
    }

    public int StepsPerDeg
    {
        get => _stepsPerDeg;
        set { _stepsPerDeg = value; OnPropertyChanged(); }
    }

    public double JogStepDegrees
    {
        get => _jogStepDegrees;
        set { _jogStepDegrees = value; OnPropertyChanged(); }
    }

    public int SpeedPercent
    {
        get => _speedPercent;
        set { _speedPercent = value; OnPropertyChanged(); }
    }

    public string DatabaseConnection
    {
        get => _databaseConnection;
        set { _databaseConnection = value; OnPropertyChanged(); }
    }

    public string TemplateCustomFolder
    {
        get => _templateCustomFolder;
        set { _templateCustomFolder = value; OnPropertyChanged(); }
    }

    public bool TrainWhiteCircuit
    {
        get => _trainWhiteCircuit;
        set
        {
            if (_trainWhiteCircuit == value) return;
            _trainWhiteCircuit = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsComponentRegionNamesVisible));
        }
    }

    public bool IsComponentRegionNamesVisible => !TrainWhiteCircuit;

    public string WhiteCircuitCustomFolder
    {
        get => _whiteCircuitCustomFolder;
        set { _whiteCircuitCustomFolder = value; OnPropertyChanged(); }
    }

    public double MinMatchSimilarityPercent
    {
        get => _minMatchSimilarityPercent;
        set { _minMatchSimilarityPercent = value; OnPropertyChanged(); }
    }

    public string OrientationComponentName
    {
        get => _orientationComponentName;
        set { _orientationComponentName = value; OnPropertyChanged(); }
    }

    public double MinOrientationMatchScore
    {
        get => _minOrientationMatchScore;
        set { _minOrientationMatchScore = value; OnPropertyChanged(); }
    }

    public double HolderQuadRectTolerancePercent
    {
        get => _holderQuadRectTolerancePercent;
        set { _holderQuadRectTolerancePercent = value; OnPropertyChanged(); }
    }

    public double HolderAspectRatioTolerancePercent
    {
        get => _holderAspectRatioTolerancePercent;
        set { _holderAspectRatioTolerancePercent = value; OnPropertyChanged(); }
    }

    public double HolderQuadAngleToleranceDegrees
    {
        get => _holderQuadAngleToleranceDegrees;
        set { _holderQuadAngleToleranceDegrees = value; OnPropertyChanged(); }
    }

    public double PcbBoardWidthMm
    {
        get => _pcbBoardWidthMm;
        set { _pcbBoardWidthMm = value; OnPropertyChanged(); }
    }

    public double PcbBoardHeightMm
    {
        get => _pcbBoardHeightMm;
        set { _pcbBoardHeightMm = value; OnPropertyChanged(); }
    }

    public string CameraCaptureSaveFolder
    {
        get => _cameraCaptureSaveFolder;
        set { _cameraCaptureSaveFolder = value; OnPropertyChanged(); }
    }

    public string SaveStatusText
    {
        get => _saveStatusText;
        private set { _saveStatusText = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public void Load()
    {
        var setting = _appSettingService.Load();

        DeveloperMode = setting.DeveloperMode;
        VirtualSerialPort = setting.VirtualSerialPort;
        ShowInspectionResultAfterRecognition = setting.ShowInspectionResultAfterRecognition;
        Com = setting.Com;
        WarehouseCom = setting.WarehouseCom;
        BaudRate = setting.BaudRate;
        StepsPerDeg = setting.StepsPerDeg;
        JogStepDegrees = setting.JogStepDegrees;
        SpeedPercent = setting.SpeedPercent;
        DatabaseConnection = setting.DatabaseConnection;

        TemplateCustomFolder = setting.ComponentTemplates.CustomFolder;
        TrainWhiteCircuit = setting.ComponentTemplates.TrainWhiteCircuit;
        WhiteCircuitCustomFolder = setting.ComponentTemplates.WhiteCircuitCustomFolder;
        MinMatchSimilarityPercent = setting.ComponentTemplates.MinMatchSimilarityPercent;
        OrientationComponentName = setting.ComponentTemplates.OrientationComponentName;
        MinOrientationMatchScore = setting.ComponentTemplates.MinOrientationMatchScore;

        AllowedRegionNames.Clear();
        var names = setting.ComponentTemplates.AllowedRegionNames;
        for (var i = 0; i < names.Count; i++)
        {
            AllowedRegionNames.Add(new AllowedRegionNameItem
            {
                Index = i + 1,
                Name = names[i]
            });
        }

        PcbBoardWidthMm = setting.PcbBoard.WidthMm;
        PcbBoardHeightMm = setting.PcbBoard.HeightMm;
        HolderQuadRectTolerancePercent = setting.PcbBoard.QuadRectTolerancePercent;
        HolderAspectRatioTolerancePercent = setting.PcbBoard.AspectRatioTolerancePercent;
        HolderQuadAngleToleranceDegrees = setting.PcbBoard.QuadAngleToleranceDegrees;

        CameraCaptureSaveFolder = setting.CameraCapture.SaveFolder;

        SaveStatusText = string.Empty;
    }

    /// <summary>Reload last saved values from disk, discarding unsaved edits.</summary>
    public void Reset()
    {
        Load();
        SaveStatusText = "Đã đặt lại cài đặt đã lưu gần nhất từ Config/setting.json.";
    }

    public void AddRegionName()
    {
        AllowedRegionNames.Add(new AllowedRegionNameItem
        {
            Index = AllowedRegionNames.Count + 1,
            Name = string.Empty
        });
    }

    public void RemoveRegionName(AllowedRegionNameItem item)
    {
        if (!AllowedRegionNames.Remove(item)) return;
        ReindexRegionNames();
    }

    public void Save()
    {
        ReindexRegionNames();

        var trimmedNames = AllowedRegionNames
            .Select(r => r.Name.Trim())
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        var duplicateNames = trimmedNames
            .GroupBy(n => n, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateNames.Count > 0)
        {
            SaveStatusText = $"Không lưu được: tên vùng trùng lặp ({string.Join(", ", duplicateNames)}).";
            return;
        }

        var setting = _appSettingService.Load();
        setting.DeveloperMode = DeveloperMode;
        setting.VirtualSerialPort = VirtualSerialPort;
        setting.ShowInspectionResultAfterRecognition = ShowInspectionResultAfterRecognition;
        setting.Com = Com.Trim();
        setting.WarehouseCom = WarehouseCom.Trim();
        setting.BaudRate = BaudRate;
        setting.StepsPerDeg = StepsPerDeg;
        setting.JogStepDegrees = JogStepDegrees;
        setting.SpeedPercent = Math.Clamp(SpeedPercent, 0, 100);
        setting.DatabaseConnection = DatabaseConnection.Trim();

        setting.ComponentTemplates.CustomFolder = TemplateCustomFolder.Trim();
        setting.ComponentTemplates.TrainWhiteCircuit = TrainWhiteCircuit;
        setting.ComponentTemplates.WhiteCircuitCustomFolder = WhiteCircuitCustomFolder.Trim();
        setting.ComponentTemplates.MinMatchSimilarityPercent = MinMatchSimilarityPercent;
        setting.ComponentTemplates.OrientationComponentName = OrientationComponentName.Trim();
        setting.ComponentTemplates.MinOrientationMatchScore = MinOrientationMatchScore;
        setting.ComponentTemplates.AllowedRegionNames = trimmedNames;

        setting.PcbBoard.WidthMm = Math.Max(0, PcbBoardWidthMm);
        setting.PcbBoard.HeightMm = Math.Max(0, PcbBoardHeightMm);
        setting.PcbBoard.QuadRectTolerancePercent = HolderQuadRectTolerancePercent;
        setting.PcbBoard.AspectRatioTolerancePercent = HolderAspectRatioTolerancePercent;
        setting.PcbBoard.QuadAngleToleranceDegrees = HolderQuadAngleToleranceDegrees;

        setting.CameraCapture.SaveFolder = CameraCaptureSaveFolder.Trim();

        _appSettingService.Save(setting);

        MinMatchSimilarityPercent = setting.ComponentTemplates.MinMatchSimilarityPercent;
        MinOrientationMatchScore = setting.ComponentTemplates.MinOrientationMatchScore;
        OrientationComponentName = setting.ComponentTemplates.OrientationComponentName;
        SpeedPercent = setting.SpeedPercent;
        PcbBoardWidthMm = setting.PcbBoard.WidthMm;
        PcbBoardHeightMm = setting.PcbBoard.HeightMm;
        HolderQuadRectTolerancePercent = setting.PcbBoard.QuadRectTolerancePercent;
        HolderAspectRatioTolerancePercent = setting.PcbBoard.AspectRatioTolerancePercent;
        HolderQuadAngleToleranceDegrees = setting.PcbBoard.QuadAngleToleranceDegrees;

        AllowedRegionNames.Clear();
        for (var i = 0; i < trimmedNames.Count; i++)
        {
            AllowedRegionNames.Add(new AllowedRegionNameItem
            {
                Index = i + 1,
                Name = trimmedNames[i]
            });
        }

        SaveStatusText = DeveloperMode && VirtualSerialPort
            ? "Đã lưu vào Config/setting.json. Serial ảo áp dụng ở lần kết nối tiếp theo."
            : DeveloperMode
                ? "Đã lưu vào Config/setting.json. Làm mới Dashboard để thấy nút tạo mẫu."
                : TrainWhiteCircuit
                    ? "Đã lưu vào Config/setting.json. Chế độ train mạch trắng — làm mới Dashboard để cập nhật nút và kiểm tra."
                    : "Đã lưu vào Config/setting.json.";
    }

    private void ReindexRegionNames()
    {
        for (var i = 0; i < AllowedRegionNames.Count; i++)
            AllowedRegionNames[i].Index = i + 1;
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
